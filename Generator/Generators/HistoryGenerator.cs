using AnotherECS.Core;
using System.IO;
using System.Linq;

namespace AnotherECS.Generator
{
    public class HistoryGenerator : IFileGenerator
    {
        public string SaveFilePostfixName => "_History.gen.cs";
        public string TemplateFileName0 => "history.template.txt";
        public string TemplateFileName1 => "historyS.template.txt";


        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => CompileInternal(context, isForceOverride);

        public DeleteContentGenerator GetUnusedFiles(GeneratorContext context)
           => new(GetSaveFileNames(context), SaveFilePostfixName);

        public string[] GetSaveFileNames(GeneratorContext context)
            => GetElements(context)
                .Select(p => GetPathByOptions(context, p))
                .ExceptDublicates()
                .ToArray();

        private ContentGenerator[] CompileInternal(GeneratorContext context, bool isForceOverride)
            => GetElements(context).Select(option =>
                {
                   var path = GetPathByOptions(context, option);
                   if (isForceOverride || !File.Exists(path))
                   {
                       var variables = GeneratorHelper.DefaultVariables(option);

                       return new ContentGenerator(path, TemplateParser.Transform(context.GetTemplate(GetTemplate(option)), variables));
                   }
                   return default;
                })
                .Where(p => p.path != null)
                .ToArray();

        private TypeOptions[] GetElements(GeneratorContext context)
            => context
                .GetComponentTypesExceptDublicates()
                .Select(p => new TypeOptions(p))
                .Where(p => p.isHistory)
                .ToArray();

        private string GetTemplate(TypeOptions typeOptions)
            => typeOptions.isShared ? TemplateFileName1 : TemplateFileName0;

        private string GetPathByOptions(GeneratorContext context, TypeOptions typeOptions)
            => Path.Combine(context.FindRootGenCommonDirectory(), TypeOptionsUtils.GetHistoryFlags(typeOptions) + SaveFilePostfixName);
    }
}
