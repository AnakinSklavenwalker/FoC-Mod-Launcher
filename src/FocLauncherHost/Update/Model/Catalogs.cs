using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using FocLauncher.Xml;

namespace FocLauncherHost.Update.Model
{
    [Serializable]
    [XmlRoot("Products", Namespace = "", IsNullable = false)]
    public class Catalogs
    {
        private List<ProductCatalog> _products = new();

        [XmlElement("Product")]
        public List<ProductCatalog> Products
        {
            get => _products;
            set => _products = value;
        }

        public static Catalogs FromStream(Stream stream)
        {
            if (stream == null || stream.Length == 0)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
                throw new NotSupportedException();

            var parser = new XmlObjectParser<Catalogs>(stream);
            return parser.Parse();
        }
    }
}