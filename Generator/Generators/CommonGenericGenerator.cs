namespace AnotherECS.Generator
{
    public class CommonGenericGenerator : IGenerator, IArgumentGenerator
    {
        private readonly string _path;
        private readonly string _template;

        private int _count = 1;

        public CommonGenericGenerator(string path, string template)
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
        {
            TemplateParser.Variables variables = JobBagGVariablesConfigGenerator.Get(_count);

            return new ContentGenerator(_path, TemplateParser.Transform(_template, variables));
        }
    }

    internal static class JobBagGVariablesConfigGenerator
    {
        public static TemplateParser.Variables Get(int count)
        {
            TemplateParser.Variables variables = null;
            variables = new()
            {
                { "STRUCT_COUNT", () => count },
                { "GENERIC_COUNT", () => variables.GetIndex(0) + 1 },
                { "SEPARATOR1:,", () =>
                    (variables.GetIndex(1) < variables.GetLength(1) - 1)
                    ? ", "
                    : string.Empty
                },
            };

            return variables;
        }
    }
}
