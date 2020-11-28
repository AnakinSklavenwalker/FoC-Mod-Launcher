using System;
using System.Collections.Generic;
using System.Text;
using TaskBasedUpdater.ProductComponent;

namespace TaskBasedUpdater
{
    [Serializable]
    public class UpdateItemFailedException : UpdaterException
    {
        private readonly IEnumerable<IUpdateItem>? _failedUpdateItems;
        private string? _error;

        public override string Message => Error;

        private string Error
        {
            get
            {
                if (_error != null)
                    return _error;
                var stringBuilder = new StringBuilder();
                if (_failedUpdateItems != null)
                {
                    foreach (var item in _failedUpdateItems)
                        stringBuilder.Append("Update Item '" + item.Name + "' failed to " + item.RequiredAction + ";");
                }
                return stringBuilder.ToString().TrimEnd(';');
            }
        }

        public UpdateItemFailedException(IEnumerable<IUpdateItem> failedUpdateItems) : base()
        {
            _failedUpdateItems = failedUpdateItems;
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