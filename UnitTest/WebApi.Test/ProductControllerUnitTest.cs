using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net;
using WebApi.Controllers;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Test
{
    public class ProductControllerUnitTest
    {
        private readonly Faker<Product> _fakeProducts;
        private int[] count = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        public ProductControllerUnitTest()
        {
            //// global seed
            //Randomizer.Seed = new Random(1000);

            // using Fluent Syntax
            // local seed: dữ liệu không thay đổi mỗi khi chạy
            _fakeProducts = new Faker<Product>().UseSeed(1000)
                // f.{Dataset}.{Method}
                .RuleFor(x => x.Id, f => Guid.NewGuid()) //Use a method outside scope
                .RuleFor(x => x.Code, f => f.Lorem.Word())
                .RuleFor(x => x.Name, f => f.Name.FullName())
                .RuleFor(x => x.Description, (f, u) => $"Description {u.Code}")
                .RuleFor(x => x.Price, f => f.Random.Int(0, 100_000))
                .RuleFor(x => x.Count, f => f.PickRandom(count))
                .RuleFor(x => x.CreateDate, f => f.Date.Past());

            // Using the Faker facade
            var faker = new Faker(locale: "vi"); // locale
            var p = new Product()
            {
                Id = new Guid("8025CFB7-AF7B-4313-9306-780FD64F59DD"),
                Code = faker.Lorem.Sentence(),
                Name = faker.Lorem.Letter(),
                Description = faker.Lorem.Text(),
                Price = faker.Random.Number(0, 100_000),
                Count = new Randomizer().Int(0, 30), // Using DataSets directly
                CreateDate = faker.Date.Future()
            };
        }

        [Fact]
        public async Task Get_ReturnOk()
        {
            // Arrange
            var _mockService = new Mock<IProductService>();
            _mockService.Setup(service => service.Get()).ReturnsAsync(_fakeProducts.Generate(10)); // Generate 10 record
            var controller = new ProductController(_mockService.Object);

            // Act
            var result = await controller.Get();

            // Assert
            result.Should().BeOfType<ActionResult<List<Product>>>()
                .Which.Value.Should().BeOfType<List<Product>>()
                .Which.Count.Should().Be(10);
        }

        [Fact]
        public void Get_ThrowException()
        {
            // Arrange
            var _mockService = new Mock<IProductService>();
            _mockService.Setup(service => service.Get()).ThrowsAsync(new ArgumentException("command")); // throw exception
            var controller = new ProductController(_mockService.Object);

            // Act
            var act = async () => await controller.Get();

            // Assert
            act.Should().ThrowAsync<ArgumentException>();
            act.Should().NotThrowAsync<CannotUnloadAppDomainException>();
        }

        [Fact]
        public async Task GetId_ReturnNotFound()
        {
            // Arrange
            var _mockService = new Mock<IProductService>();
            var id = Guid.NewGuid();
            _mockService.Setup(service => service.Get(id)).ReturnsAsync(null as Product);
            var controller = new ProductController(_mockService.Object);

            // Act
            var result = await controller.Get(id);

            // Assert
            result.Should().BeOfType<ActionResult<Product>>()
                .Which.Result.Should().BeOfType(typeof(NotFoundResult));
        }

        [Fact]
        public async Task GetId_ReturnOk()
        {
            // Arrange
            var _mockService = new Mock<IProductService>();
            // Using the Faker facade
            var faker = new Faker(locale: "vi"); // locale
            var id = new Guid("8025CFB7-AF7B-4313-9306-780FD64F59DD");
            var p = new Product()
            {
                Id = id,
                Code = faker.Lorem.Sentence(),
                Name = faker.Lorem.Letter(),
                Description = faker.Lorem.Text(),
                Price = faker.Random.Number(0, 100_000),
                Count = new Randomizer().Int(0, 30), // Using DataSets directly
                CreateDate = faker.Date.Future()
            };
            _mockService.Setup(service => service.Get(It.IsAny<Guid>())).ReturnsAsync(p);
            var controller = new ProductController(_mockService.Object);

            // Act
            var result = await controller.Get(id);

            // Assert
            result.Should().BeOfType<ActionResult<Product>>()
                .Which.Value.Should().BeOfType<Product>()
                .Which.Id.Should().Be(new Guid("8025CFB7-AF7B-4313-9306-780FD64F59DD"));
        }

        [Fact]
        public async Task Remove_ReturnNotFound()
        {
            // Arrange
            var _mockService = new Mock<IProductService>();
            // Using the Faker facade
            var faker = new Faker(locale: "vi"); // locale
            var id = new Guid("8025CFB7-AF7B-4313-9306-780FD64F59DD");
            _mockService.Setup(service => service.Get(It.IsAny<Guid>())).ReturnsAsync(null as Product);
            _mockService.Setup(service => service.Remove(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask)
                .Verifiable("Remove should be executed always!"); // đảm bảo hàm luôn được chạy qua
            var controller = new ProductController(_mockService.Object);

            // Act
            var result = await controller.Delete(id);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockService.Verify(); // hàm chưa dc chạy nên fail
        }

        [Fact]
        public async Task Remove_ReturnNoContent()
        {
            // Arrange
            var _mockService = new Mock<IProductService>();
            // Using the Faker facade
            var faker = new Faker(locale: "vi"); // locale
            var id = new Guid("8025CFB7-AF7B-4313-9306-780FD64F59DD");
            var p = new Product()
            {
                Id = id,
                Code = faker.Lorem.Sentence(),
                Name = faker.Lorem.Letter(),
                Description = faker.Lorem.Text(),
                Price = faker.Random.Number(0, 100_000),
                Count = new Randomizer().Int(0, 30), // Using DataSets directly
                CreateDate = faker.Date.Future()
            };
            _mockService.Setup(service => service.Get(It.IsAny<Guid>())).ReturnsAsync(p);
            _mockService.Setup(service => service.Remove(It.IsAny<Guid>())).Returns(Task.CompletedTask).Verifiable("Remove should be executed always!");
            var controller = new ProductController(_mockService.Object);

            // Act
            var result = await controller.Delete(id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockService.Verify();
        }

        [Fact]
        public async Task Post_InvalidModel()
        {
            // Arrange
            var _mockService = new Mock<IProductService>();
            // Using the Faker facade
            var faker = new Faker(locale: "vi"); // locale
            var id = new Guid("8025CFB7-AF7B-4313-9306-780FD64F59DD");
            var p = new Product()
            {
                Id = id,
                Code = faker.Lorem.Sentence(),
                Name = null,
                Description = faker.Lorem.Text(),
                Price = faker.Random.Number(0, 100_000),
                Count = new Randomizer().Int(0, 30), // Using DataSets directly
                CreateDate = faker.Date.Future()
            };
            _mockService.Setup(service => service.Add(p)).ReturnsAsync(p);
            var controller = new ProductController(_mockService.Object);
            controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await controller.Post(p);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Post_ReturnOk()
        {
            // Arrange
            var _mockService = new Mock<IProductService>();
            // Using the Faker facade
            var faker = new Faker(locale: "vi"); // locale
            var id = new Guid("8025CFB7-AF7B-4313-9306-780FD64F59DD");
            var p = new Product()
            {
                Id = id,
                Code = faker.Lorem.Sentence(),
                Name = faker.Lorem.Word(),
                Description = faker.Lorem.Text(),
                Price = faker.Random.Number(0, 100_000),
                Count = new Randomizer().Int(0, 30), // Using DataSets directly
                CreateDate = faker.Date.Future()
            };
            _mockService.Setup(service => service.Add(p)).ReturnsAsync(p).Verifiable();
            var controller = new ProductController(_mockService.Object);

            // Act
            var result = await controller.Post(p);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>()
                .Which.RouteValues["id"].Should().Be(id);
            _mockService.Verify();
        }

        [Fact]
        public async Task MiddlewareTest_ReturnsNotFoundForRequest()
        {
            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .Configure(app =>
                        {
                            app.UseLastLogin();
                        });
                })
                .StartAsync();

            var response = await host.GetTestClient().GetAsync("/"); // catch response

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task TestMiddleware_ExpectedResponse()
        {
            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .Configure(app =>
                        {
                            app.UseLastLogin();
                        });
                })
                .StartAsync();

            var server = host.GetTestServer(); // catch request

            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Post;
                c.Request.Path = "/and/file.txt";
                c.Request.QueryString = new QueryString("?and=query");
            });

            context.RequestAborted.CanBeCanceled.Should().BeTrue();
            context.Request.Protocol.Should().BeEquivalentTo(HttpProtocol.Http11);
            context.Request.Method.Should().BeEquivalentTo("post");
            context.Request.Path.ToString().Should().EndWith("file.txt");
            context.Request.QueryString.ToString().Should().Contain("query");
        }
    }
}