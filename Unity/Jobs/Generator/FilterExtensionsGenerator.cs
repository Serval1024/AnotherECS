using AnotherECS.Generator;

namespace AnotherECS.Unity.Jobs.Generator
{
    public class FilterExtensionsGenerator : IGenerator, IArgumentGenerator
    {
        private readonly string _path;
        private readonly string _template;

        private int _count = 1;

        public FilterExtensionsGenerator(string path, string template)
        {
            _path = path;
            _template = template;
        }

        public void SetArgs(object ags)
        {
            _count = (int)ags;
        }

        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => new[] { Compile() };

        public ContentGenerator Compile()
             => new(
                 _path,
                 TemplateParser.Transform(_template, BagFilterVariablesConfigGenerator.Get(_count))
                 );


    }
}