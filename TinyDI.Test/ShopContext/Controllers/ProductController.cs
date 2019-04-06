using TinyDI.Test.ShopContext.Data.Entity;
using TinyDI.Test.ShopContext.Service;

namespace TinyDI.Test
{
    public interface IProductController
    {
        int Add(Product product);
    }

    public class ProductController : IProductController
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public int Add(Product product)
        {
            return _productService.Add(product);
        }
    }
}
