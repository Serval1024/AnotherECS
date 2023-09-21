using System.IO;

namespace AnotherECS.Generator
{
    public class LayoutInstallerGenerator : IFileGenerator
    {
        public string SaveFilePostfixName => "LayoutInstaller.gen.cs";
        public string TemplateFileName => "layoutInstaller.template.txt";

        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => new[] { CompileInternal(context) };

        public DeleteContentGenerator GetUnusedFiles(GeneratorContext context)
            => default;

        public string[] GetSaveFileNames(GeneratorContext context)
            => new[] { GetPath(context) };

        private ContentGenerator CompileInternal(GeneratorContext context)
        {
            var variables = VariablesConfigGenerator.GetLayoutInstaller(context.GetComponents());
            
            return new ContentGenerator(
                GetPath(context),
                TemplateParser.Transform(context.GetTemplate(TemplateFileName), variables)
                );
        }

        private string GetPath(GeneratorContext context)
            => Path.Combine(context.FindRootGenCommonDirectory(), SaveFilePostfixName);
    }
}
