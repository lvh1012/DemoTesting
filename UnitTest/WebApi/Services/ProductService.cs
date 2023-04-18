using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class ProductService : IProductService
    {
        public Task<Product> Add(Product product)
        {
            throw new NotImplementedException();
        }

        public Task<List<Product>> Get()
        {
            throw new NotImplementedException();
        }

        public Task<Product> Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task Remove(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}