using System;
using System.Xml.Serialization;

namespace FocLauncherHost.Update.Model
{
    [Serializable]
    public class Dependency
    {
        private string _destination;
        private string _origin;
        private byte[] _sha2;
        private string _name;
        private string _version;
        private long? _size;
        
        public string Destination
        {
            get => _destination;
            set => _destination = value;
        }

        [XmlElement(DataType="long")]
        public long? Size
        {
            get => _size;
            set => _size = value;
        }

        [XmlElement(DataType = "anyURI")]
        public string Origin
        {
            get => _origin;
            set => _origin = value;
        }

        [XmlElement(ElementName = "SHA2", DataType = "hexBinary")]
        public byte[]? Sha2
        {
            get => _sha2;
            set => _sha2 = value;
        }

        [XmlAttribute]
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        [XmlAttribute]
        public string Version
        {
            get => _version;
            set => _version = value;
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Version)}: {Version}, {nameof(Destination)}: {Destination}, {nameof(Origin)}: {Origin}";
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) 
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((Dependency)obj);
        }
        
        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        // Prevents null tags when deserializing 
        public bool ShouldSerializeSize()
        {
            return _size != null;
        }

        // Prevents null tags when deserializing 
        public bool ShouldSerializeSha2()
        {
            return _sha2 != null;
        }

        protected bool Equals(Dependency other)
        {
            return _name == other._name;
        }
    }
}