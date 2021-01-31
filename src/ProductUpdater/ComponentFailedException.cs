using System;
using System.Collections.Generic;
using System.Text;
using ProductMetadata.Component;

namespace ProductUpdater
{
    [Serializable]
    public class ComponentFailedException : UpdaterException
    {
        private readonly IEnumerable<ProductComponent>? _failedComponents;
        private string? _error;

        public override string Message => Error;

        private string Error
        {
            get
            {
                if (_error != null)
                    return _error;
                var stringBuilder = new StringBuilder();
                if (_failedComponents != null)
                {
                    foreach (var item in _failedComponents)
                        stringBuilder.Append("Product Component '" + item.Name + "' failed to " + item.RequiredAction + ";");
                }
                return stringBuilder.ToString().TrimEnd(';');
            }
        }

        public ComponentFailedException(IEnumerable<ProductComponent> failedComponents)
        {
            _failedComponents = failedComponents;
            HResult = 1603;
        }

        internal ComponentFailedException(string error, int errorCode = 1603)
        {
            if (string.IsNullOrEmpty(error))
                throw new ArgumentNullException(nameof(error));
            _error = error;
            HResult = errorCode;
        }
    }
}