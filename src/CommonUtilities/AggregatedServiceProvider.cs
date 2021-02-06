using System;
using System.Collections.Generic;
using Validation;

namespace CommonUtilities
{
    public class AggregatedServiceProvider : DisposableObject, IServiceProvider
    {
        private IServiceProvider[] _serviceProviders;

        private Dictionary<Type, int> _serviceLookupCache = new();
        
        public AggregatedServiceProvider(params IServiceProvider[] serviceProviders)
        {
            Requires.NotNullOrEmpty(serviceProviders, nameof(serviceProviders));
            _serviceProviders = serviceProviders;
        }

        public object? GetService(Type serviceType)
        {
            VerifyNotDisposed();
            if (_serviceLookupCache.TryGetValue(serviceType, out var foundIndex))
                return foundIndex == -1 ? null : _serviceProviders[foundIndex].GetService(serviceType);


            for (var index = 0; index < _serviceProviders.Length; index++)
            {
                var service = _serviceProviders[index].GetService(serviceType);
                if (service is null) 
                    continue;

                _serviceLookupCache.Add(serviceType, index);
                return service;
            }
            _serviceLookupCache.Add(serviceType, -1);
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            _serviceLookupCache.Clear();
            _serviceLookupCache = null!;
            _serviceProviders = null!;
            base.Dispose(disposing);
        }
    }
}
