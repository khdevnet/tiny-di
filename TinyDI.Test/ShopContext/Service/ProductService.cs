using TinyDI.Test.ShopContext.Data.Entity;

namespace TinyDI.Test.ShopContext.Service
{
    public interface IProductService
    {
        int Add(Product product);
    }

    public class ProductService : IProductService
    {
        public int Add(Product product)
        {
            return product.Id + 1;
        }
    }
}
