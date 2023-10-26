using System.IO;

namespace AnotherECS.Generator
{
    public class CommonLayoutInstallerGenerator : IFileGenerator
    {
        public string SaveFilePostfixName => "CommonLayoutInstaller.gen.cs";
        public string TemplateFileName => "commonlayoutinstaller.template.txt";

        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => new[] { CompileInternal(context) };

        public DeleteContentGenerator GetUnusedFiles(GeneratorContext context)
            => default;

        public string[] GetSaveFileNames(GeneratorContext context)
            => new[] { GetPath(context) };

        private ContentGenerator CompileInternal(GeneratorContext context)
        {
            var variables = VariablesConfigGenerator.GetCommonLayoutInstaller(context.GetComponents());
            
            return new ContentGenerator(
                GetPath(context),
                TemplateParser.Transform(context.GetTemplate(TemplateFileName), variables)
                );
        }

        private string GetPath(GeneratorContext context)
            => Path.Combine(context.FindRootGenCommonDirectory(), SaveFilePostfixName);
    }
}
