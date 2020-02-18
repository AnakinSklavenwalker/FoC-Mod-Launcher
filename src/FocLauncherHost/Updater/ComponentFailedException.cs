﻿using System;
using System.Collections.Generic;
using System.Text;
using FocLauncherHost.Updater.Component;

namespace FocLauncherHost.Updater
{
    [Serializable]
    public class ComponentFailedException : UpdaterException
    {
        private readonly IEnumerable<IComponent> _failedComponents;
        private string _error;

        public override string Message => Error;

        private string Error
        {
            get
            {
                if (this._error != null)
                    return this._error;
                var stringBuilder = new StringBuilder();
                if (this._failedComponents != null)
                {
                    foreach (var component in _failedComponents)
                        stringBuilder.Append("Package '" + component.Name + "' failed to " + component.RequiredAction + ";");
                }
                return stringBuilder.ToString().TrimEnd(';');
            }
        }

        public ComponentFailedException(IEnumerable<IComponent> failedComponents) : base()
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