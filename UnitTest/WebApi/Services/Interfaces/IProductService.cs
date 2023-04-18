using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> Get();

        Task<Product> Get(Guid id);

        Task<Product> Add(Product product);

        Task Remove(Guid id);
    }
}