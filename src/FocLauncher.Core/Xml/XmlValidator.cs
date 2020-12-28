using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using NLog;
using Validation;

namespace FocLauncher.Xml
{
    public class XmlValidator : IDisposable
    {
        private List<XmlValidationError> _errors = new();

        private readonly Stream _schemeStream;
        private readonly ILogger? _logger;

        public XmlValidator(Stream schemeStream, ILogger? logger = null)
        {
            Requires.NotNull(schemeStream, nameof(schemeStream));
            _schemeStream = schemeStream;
            _logger = logger;
        }

        
        public XmlValidationResult Validate(string filePath, ConformanceLevel conformanceLevel = ConformanceLevel.Auto)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return InternalValidate(stream, conformanceLevel);
        }

        public XmlValidationResult Validate(Stream stream, ConformanceLevel conformanceLevel = ConformanceLevel.Auto)
        {
            return InternalValidate(stream, conformanceLevel);
        }

        private XmlValidationResult InternalValidate(Stream stream, ConformanceLevel conformanceLevel)
        {
            if (!stream.CanRead)
                throw new InvalidOperationException("Cannot read from stream");

            stream.Seek(0, SeekOrigin.Begin);
            _schemeStream.Seek(0, SeekOrigin.Begin);
            try
            {
                var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation |
                                            XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ConformanceLevel = conformanceLevel;
                settings.ValidationEventHandler += Settings_ValidationEventHandler;
                using var schemaReader = XmlReader.Create(_schemeStream);
                settings.Schemas.Add(null, schemaReader);

                var reader = XmlReader.Create(stream, settings);
                while (reader.Read())
                {
                }
                reader.Close();
            }
            catch(Exception e)
            {
                _logger?.Error(e, e.Message);
                return new XmlValidationResult(e);
            }
            return new XmlValidationResult(_errors);
        }

        private void Settings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            var error = new XmlValidationError(e.Message, e.Exception.LineNumber, e.Exception.LinePosition);
            _errors.Add(error);
            _logger?.Warn(error.ToString);
        }

        public void Dispose()
        {
            _schemeStream.Dispose();
            _errors.Clear();
            _errors = null;
        }
    }
}
