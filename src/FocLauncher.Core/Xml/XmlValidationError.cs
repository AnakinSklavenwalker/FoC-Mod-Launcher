namespace FocLauncher.Xml
{
    public class XmlValidationError
    {
        public string Message { get; }
        
        public int LineNumber { get; }

        public int LinePosition { get; }

        public XmlValidationError(string message, int line, int position)
        {
            Message = message;
            LineNumber = line;
            LinePosition = position;
        }

        public override string ToString()
        {
            return $"XML validation error at {LineNumber}:{LinePosition}: {Message}";
        }
    }
}