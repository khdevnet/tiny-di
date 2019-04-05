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
        IConfigurableNSubContainer Customize();

        /// <summary>
        /// Create an explicit scope, so all dependencies with the <see cref="NSubLifetime.PerScope"/> lifetime
        /// are preserved for multiple resolve requests.
        /// </summary>
        ITinyDIResolver CreateScope();
    }

    public interface IConfigurableNSubContainer : ITinyDIResolver
    {
        IConfigurableNSubContainer Register<TKey, TImpl>(NSubLifetime lifetime) where TImpl : TKey;

        IConfigurableNSubContainer Register<TKey>(Func<ITinyDIResolver, TKey> factory, NSubLifetime lifetime);
    }

    public static class ConfigurableNSubContainerExtensions
    {
        public static IConfigurableNSubContainer RegisterPerScope<TKey, TImpl>(this IConfigurableNSubContainer container)
            where TImpl : TKey
        {
            return container.Register<TKey, TImpl>(NSubLifetime.PerScope);
        }

        public static IConfigurableNSubContainer RegisterPerScope<TKey>(this IConfigurableNSubContainer container, Func<ITinyDIResolver, TKey> factory)
        {
            return container.Register(factory, NSubLifetime.PerScope);
        }

        public static IConfigurableNSubContainer RegisterSingleton<TKey, TImpl>(this IConfigurableNSubContainer container)
            where TImpl : TKey
        {
            return container.Register<TKey, TImpl>(NSubLifetime.Singleton);
        }
    }
}