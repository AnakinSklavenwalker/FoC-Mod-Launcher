namespace ProductMetadata.Component
{
    public class FileCondition : Condition
    {
        public override ConditionType Type => ConditionType.File;

        public FileItem File { get; }

        public FileCondition(FileItem file)
        {
            File = file;
        }
    }
}