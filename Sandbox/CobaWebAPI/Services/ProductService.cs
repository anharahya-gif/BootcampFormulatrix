using CobaWebAPI.Models;

namespace CobaWebAPI.Services
{
    public class ProductService : IProductService
    {
        private static List<Product> _products = new();
        private static int _nextId = 1;

        public List<Product> GetAll()
        {
            return _products;
        }

        public Product? GetById(int id)
        {
            return _products.FirstOrDefault(x => x.Id == id);
        }

        public Product Add(Product product)
        {
            product.Id = _nextId++;
            _products.Add(product);
            return product;
        }

        public bool Update(int id, Product product)
        {
            var existing = GetById(id);
            if (existing == null) return false;

            existing.Name = product.Name;
            existing.Price = product.Price;
            return true;
        }

        public bool Delete(int id)
        {
            var product = GetById(id);
            if (product == null) return false;

            _products.Remove(product);
            return true;
        }
    }
}
