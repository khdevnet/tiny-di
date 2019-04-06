using System;

namespace TinyDI.Core
{
    public interface ITinyDIResolver
    {
        T Resolve<T>();
    }

    public interface ITinyDIContainer : ITinyDIResolver
    {
        /// <summary>
        /// Creates a new container based on the current one,
        /// which can be configured to override the existing registrations without affecting the existing container.
        /// </summary>
        IConfigurableTinyDIContainer Customize();

        /// <summary>
        /// Create an explicit scope, so all dependencies with the <see cref="ServiceLifetime.PerScope"/> lifetime
        /// are preserved for multiple resolve requests.
        /// </summary>
        ITinyDIResolver CreateScope();
    }

    public interface IConfigurableTinyDIContainer : ITinyDIContainer
    {
        IConfigurableTinyDIContainer Register<TKey, TImpl>(ServiceLifetime lifetime) where TImpl : TKey;

        IConfigurableTinyDIContainer Register<TKey>(Func<ITinyDIResolver, TKey> factory, ServiceLifetime lifetime);
    }

    public static class ConfigurableTinyDIContainerExtensions
    {
        public static IConfigurableTinyDIContainer RegisterPerScope<TKey, TImpl>(this IConfigurableTinyDIContainer container)
            where TImpl : TKey
        {
            return container.Register<TKey, TImpl>(ServiceLifetime.PerScope);
        }

        public static IConfigurableTinyDIContainer RegisterPerScope<TKey>(this IConfigurableTinyDIContainer container, Func<ITinyDIResolver, TKey> factory)
        {
            return container.Register(factory, ServiceLifetime.PerScope);
        }

        public static IConfigurableTinyDIContainer RegisterSingleton<TKey, TImpl>(this IConfigurableTinyDIContainer container)
            where TImpl : TKey
        {
            return container.Register<TKey, TImpl>(ServiceLifetime.Singleton);
        }
    }
}