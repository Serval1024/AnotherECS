namespace AnotherECS.Generator
{
    public class FStringGenerator : IGenerator
    {
        private readonly string _path;
        private readonly string _template;

        public readonly int[] COLLECTION_SIZES = new[] { 2, 4, 8, 16, 32, 64 };

        public FStringGenerator(string path, string template)
        {
            _path = path;
            _template = template;
        }

        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => new[] { Compile() };

        public ContentGenerator Compile()
        {
            TemplateParser.Variables variables = CollectionVariablesConfigGenerator.Get(COLLECTION_SIZES);

            return new ContentGenerator(_path, TemplateParser.Transform(_template, variables));
        }
    }
}
