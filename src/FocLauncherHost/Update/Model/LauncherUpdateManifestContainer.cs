using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using FocLauncher.Xml;

namespace FocLauncherHost.Update.Model
{
    [Serializable]
    [XmlRoot("Products", Namespace = "", IsNullable = false)]
    public class LauncherUpdateManifestContainer
    {
        private List<LauncherUpdateManifestModel> _manifests = new();

        [XmlElement("Product")]
        public List<LauncherUpdateManifestModel> Manifests
        {
            get => _manifests;
            set => _manifests = value;
        }

        public static LauncherUpdateManifestContainer FromStream(Stream stream)
        {
            if (stream == null || stream.Length == 0)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
                throw new NotSupportedException();

            var parser = new XmlObjectParser<LauncherUpdateManifestContainer>(stream);
            return parser.Parse();
        }
    }
}