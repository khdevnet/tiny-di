using TinyDI.Core;
using TinyDI.Test.ShopContext.Service;

namespace TinyDI.Test.ShopContext
{
    public static class ControllerDefaultFactory
    {
        /// <summary>
        /// The default controllers registrations. Feel free to configure the existing container to customize
        /// and override controllers parts.
        /// </summary>
        public static ITinyDIContainer DefaultContainer { get; } = CreateDefaultContainer();

        public static TController CreateController<TController>() => DefaultContainer.Resolve<TController>();

        private static ITinyDIContainer CreateDefaultContainer()
        {
            return new TinyDIContainer()
                  .RegisterPerScope<IProductController, ProductController>()
                  .RegisterPerScope<IProductService, ProductService>();
        }
    }
}