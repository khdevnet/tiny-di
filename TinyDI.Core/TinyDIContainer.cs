using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyDI.Core
{
    /// <summary>
    /// Tiny and very limited implementation of the DI services.
    /// Container supports the following features required by DI:
    ///     - Registration by type with automatic constructor injection
    ///     - Registration of factory methods for the complex objects
    ///     - Support of the most required lifetimes:
    ///         - <see cref="ServiceLifetime.Transient" />
    ///         - <see cref="ServiceLifetime.PerScope" />
    ///         - <see cref="ServiceLifetime.Singleton" />
    ///     - Immutability (via interfaces) and customization by creating a nested container
    /// </summary>
    public class TinyDIContainer : IConfigurableTinyDIContainer
    {
        private readonly TinyDIContainer _parentContainer;
        private readonly object _syncRoot;
        private readonly Dictionary<Type, Registration> _registrations = new Dictionary<Type, Registration>();

        public TinyDIContainer()
        {
            _syncRoot = new object();
        }

        private TinyDIContainer(TinyDIContainer parentContainer)
        {
            _parentContainer = parentContainer;

            // Use the same synchronization object in descendant containers.
            // It's required to e.g. ensure that singleton dependencies are resolved only once.
            _syncRoot = parentContainer._syncRoot;
        }

        public T Resolve<T>() => CreateScope().Resolve<T>();

        public IConfigurableTinyDIContainer Register<TKey, TImpl>(ServiceLifetime lifetime) where TImpl : TKey
        {
            var constructors = typeof(TImpl).GetConstructors();
            if (constructors.Length != 1)
            {
                throw new ArgumentException(
                    $"Type '{typeof(TImpl).FullName}' should contain only single public constructor. " +
                    $"Please register type using factory method to avoid ambiguity.");
            }

            var ctor = constructors[0];

            object Factory(Scope scope)
            {
                var args = ctor.GetParameters()
                    .Select(p => scope.Resolve(p.ParameterType))
                    .ToArray();
                return ctor.Invoke(args);
            }

            AddRegistration(typeof(TKey), new Registration(Factory, lifetime));

            return this;
        }

        public IConfigurableTinyDIContainer Register<TKey>(Func<ITinyDIResolver, TKey> factory, ServiceLifetime lifetime)
        {
            object Factory(Scope scope)
            {
                return factory.Invoke(scope);
            }

            AddRegistration(typeof(TKey), new Registration(Factory, lifetime));

            return this;
        }

        public IConfigurableTinyDIContainer Customize()
        {
            return new TinyDIContainer(this);
        }

        public ITinyDIResolver CreateScope()
        {
            return new Scope(this);
        }

        private void AddRegistration(Type type, Registration registration)
        {
            lock (_syncRoot)
            {
                _registrations[type] = registration;
            }
        }

        private class Registration
        {
            private readonly Func<Scope, object> _factory;
            private readonly ServiceLifetime _lifetime;
            private object _singletonValue;

            public Registration(Func<Scope, object> factory, ServiceLifetime lifetime)
            {
                _factory = factory;
                _lifetime = lifetime;
            }

            public object Resolve(Scope scope)
            {
                switch (_lifetime)
                {
                    case ServiceLifetime.Transient:
                        return _factory.Invoke(scope);

                    case ServiceLifetime.Singleton:
                        return _singletonValue ?? (_singletonValue = _factory.Invoke(scope));

                    case ServiceLifetime.PerScope:
                        if (scope.TryGetCached(this, out var result))
                        {
                            return result;
                        }

                        result = _factory.Invoke(scope);
                        scope.Set(this, result);
                        return result;

                    default:
                        throw new InvalidOperationException("Unknown lifetime.");
                }
            }
        }

        private class Scope : ITinyDIResolver
        {
            private readonly Dictionary<Registration, object> _cache = new Dictionary<Registration, object>();
            private readonly TinyDIContainer _mostNestedContainer;

            public Scope(TinyDIContainer mostNestedContainer)
            {
                _mostNestedContainer = mostNestedContainer;
            }

            public T Resolve<T>()
            {
                return (T)Resolve(typeof(T));
            }

            public bool TryGetCached(Registration registration, out object result)
            {
                return _cache.TryGetValue(registration, out result);
            }

            public void Set(Registration registration, object value)
            {
                _cache[registration] = value;
            }

            public object Resolve(Type type)
            {
                // The same lock object is shared among all the nested containers,
                // so no need to synchronize on new object for each time.
                lock (_mostNestedContainer._syncRoot)
                {
                    var currentContainer = _mostNestedContainer;
                    while (currentContainer != null)
                    {
                        if (currentContainer._registrations.TryGetValue(type, out var registration))
                        {
                            return registration.Resolve(this);
                        }

                        currentContainer = currentContainer._parentContainer;
                    }

                    throw new InvalidOperationException($"Type is not registered: {type.FullName}");
                }
            }
        }
    }
}