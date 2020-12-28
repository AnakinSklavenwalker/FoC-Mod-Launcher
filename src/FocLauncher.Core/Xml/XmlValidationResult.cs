using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FocLauncher.Xml
{
    public class XmlValidationResult : IEnumerable<XmlValidationError>
    {
        private readonly IEnumerable<XmlValidationError> _errors;

        public bool IsValid => !_errors.Any() || Exception is not null;

        public Exception? Exception { get; }

        public XmlValidationResult(Exception e)
        {
            Exception = e;
        }

        public XmlValidationResult(IEnumerable<XmlValidationError> errors)
        {
            _errors = errors.ToList();
        }

        public IEnumerator<XmlValidationError> GetEnumerator()
        {
            return _errors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}