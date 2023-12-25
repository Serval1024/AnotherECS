namespace AnotherECS.Generator
{
    public interface IGenerator
    {
        ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride);
    }

    public interface IFileGenerator : IGenerator
    {
        string[] GetSaveFileNames(GeneratorContext context);
        DeleteContentGenerator GetUnusedFiles(GeneratorContext context);
    }

    public interface IArgumentGenerator
    {
        void SetArgs(object ags);
    }

    public struct ContentGenerator
    {
        public string path;
        public string text;

        public ContentGenerator(string path, string text)
        {
            this.path = path;
            this.text = text;
        }
    }

    public struct DeleteContentGenerator
    {
        public string keepFilePostfixName;
        public string[] paths;

        public DeleteContentGenerator(string[] paths, string keepFilePostfixName)
        {
            this.paths = paths;
            this.keepFilePostfixName = keepFilePostfixName;
        }
    }
}
