using System;
using System.Collections.Generic;
using System.Text;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater
{
    [Serializable]
    public class UpdateItemFailedException : UpdaterException
    {
        private readonly IEnumerable<IUpdateItem>? _failedComponents;
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
                    foreach (var component in _failedComponents)
                        stringBuilder.Append("Package '" + component.Name + "' failed to " + component.RequiredAction + ";");
                }
                return stringBuilder.ToString().TrimEnd(';');
            }
        }

        public UpdateItemFailedException(IEnumerable<IUpdateItem> failedComponents) : base()
        {
            _failedComponents = failedComponents;
            HResult = 1603;
        }

        internal UpdateItemFailedException(string error, int errorCode = 1603)
        {
            if (string.IsNullOrEmpty(error))
                throw new ArgumentNullException(nameof(error));
            _error = error;
            HResult = errorCode;
        }
    }
}