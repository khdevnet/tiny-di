using TinyDI.Test.ShopContext;
using TinyDI.Test.ShopContext.Data.Entity;
using Xunit;

namespace TinyDI.Test
{
    public class ControllerDefaultFactoryTest
    {
        [Fact]
        public void GetControllerTest()
        {
            IProductController controller = ControllerDefaultFactory.CreateController<IProductController>();

            Assert.IsType<ProductController>(controller);
        }

        [Fact]
        public void GetControllerIsServiceInjectedTest()
        {
            IProductController controller = ControllerDefaultFactory.CreateController<IProductController>();

            var id = controller.Add(new Product { Id = 1, Name = "test product" });

            Assert.Equal(2, id);
        }
    }
}
