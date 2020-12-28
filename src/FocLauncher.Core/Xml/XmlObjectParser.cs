using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace FocLauncher.Xml
{
    public class XmlObjectParser<T> where T : class
    {
        private Stream FileStream { get; }

        public XmlObjectParser(Stream dataStream)
        {
            if (dataStream == null || dataStream.Length == 0)
                throw new ArgumentNullException(nameof(dataStream));
            if (!dataStream.CanRead)
                throw new NotSupportedException();
            FileStream = dataStream;
        }

        public T Parse()
        {
            FileStream.Seek(0, SeekOrigin.Begin);
            var reader = XmlReader.Create(FileStream,
                new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Document });
            return (T) new XmlSerializer(typeof(T)).Deserialize(reader);
        }
    }
}
