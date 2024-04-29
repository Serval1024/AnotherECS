using System;
using System.IO;

namespace AnotherECS.Generator
{
    public class ElementInstallerGenerator : IFileGenerator
    {
        public string SaveFilePostfixName => "ElementInstaller.gen.cs";
        public string TemplateFileName => "elementinstaller.template.txt";

        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
        {
            throw new NotSupportedException();
        }

        public ContentGenerator[] Compile(GeneratorContext context, Type stateType)
            => new[] { CompileInternal(context, stateType) };

        public DeleteContentGenerator GetUnusedFiles(GeneratorContext context)
            => default;

        public string[] GetSaveFileNames(GeneratorContext context)
            => new[] { GetPath(context) };

        private ContentGenerator CompileInternal(GeneratorContext context, Type stateType)
        {
            var variables = VariablesConfigGenerator.GetElements(context, stateType);
            
            return new ContentGenerator(
                GetPath(context),
                TemplateParser.Transform(context.GetTemplate(TemplateFileName), variables)
                );
        }

        private string GetPath(GeneratorContext context)
           => Path.Combine(context.FindRootGenDirectory(), SaveFilePostfixName);
    }
}