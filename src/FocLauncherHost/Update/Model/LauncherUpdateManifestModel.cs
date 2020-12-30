using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using FocLauncher;

namespace FocLauncherHost.Update.Model
{
    [Serializable]
    [XmlType(TypeName = "Product")]
    public class LauncherUpdateManifestModel
    {
        private List<LauncherComponent> _components = new();

        private string _name;
        private string _author;
        private ApplicationType _application;

        [XmlArrayItem("Dependency", IsNullable = false)]
        [XmlArray("Dependencies")]
        public List<LauncherComponent> Components
        {
            get => _components;
            set => _components = value;
        }

        [XmlAttribute]
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        [XmlAttribute]
        public string Author
        {
            get => _author;
            set => _author = value;
        }

        [XmlAttribute]
        public ApplicationType ApplicationType
        {
            get => _application;
            set => _application = value;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Launcher: ");
            sb.Append($"Name: {Name}, ");
            sb.Append($"ApplicationType: {ApplicationType}");
            sb.Append($"Author: {Author}");

            if (!Components.Any())
                return sb.ToString();

            var dependencySb = new StringBuilder();
            foreach (var dependency in Components) 
                dependencySb.AppendLine("\t" + dependency);

            sb.AppendLine($"Components ({Components.Count}):");
            sb.Append(dependencySb);
            return sb.ToString();
        }
    }
}
