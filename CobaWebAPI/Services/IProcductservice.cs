using CobaWebAPI.Models;

namespace CobaWebAPI.Services
{
    public interface IProductService
    {
        List<Product> GetAll();
        Product? GetById(int id);
        Product Add(Product product);
        bool Update(int id, Product product);
        bool Delete(int id);
    }
}
