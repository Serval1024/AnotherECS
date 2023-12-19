using System.IO;

namespace AnotherECS.Generator
{
    public class ElementsInstallerGenerator : IFileGenerator
    {
        public string SaveFilePostfixName => "ElementsInstaller.gen.cs";
        public string TemplateFileName => "elementsinstaller.template.txt";

        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => new[] { CompileInternal(context) };

        public DeleteContentGenerator GetUnusedFiles(GeneratorContext context)
            => default;

        public string[] GetSaveFileNames(GeneratorContext context)
            => new[] { GetPath(context) };

        private ContentGenerator CompileInternal(GeneratorContext context)
        {
            var variables = VariablesConfigGenerator.GetElements(context);

            return new ContentGenerator(
                GetPath(context),
                TemplateParser.Transform(context.GetTemplate(TemplateFileName), variables)
                );
        }

        private string GetPath(GeneratorContext context)
           => Path.Combine(context.FindRootGenDirectory(), SaveFilePostfixName);
    }
}