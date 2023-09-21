using AnotherECS.Core;
using System;
using System.IO;
using System.Linq;

namespace AnotherECS.Generator
{
    public class FastAccessGenerator : IFileGenerator
    {
        public string SaveFilePostfixName => "_FastAccess.gen.cs";
        public string TemplateFileName => "fastaccess.template.txt";
        

        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => CompileInternal(context, isForceOverride);

        public DeleteContentGenerator GetUnusedFiles(GeneratorContext context)
           => new(GetSaveFileNames(context), SaveFilePostfixName);

        public string[] GetSaveFileNames(GeneratorContext context)
            => context
                .GetComponents()
                .Select(p => GetPathByOptions(context, new TypeOptions(p)))
                .ExceptDublicates()
                .ToArray();

        private ContentGenerator[] CompileInternal(
            GeneratorContext context,
            Type[] types,
            Func<TypeOptions, bool> skipCompileRule = null
            )
        {

            return types
                .Select(p => new TypeOptions(p))
                .Select(option =>
                {
                    if (skipCompileRule == null || skipCompileRule(option))
                    {
                        var variables = VariablesConfigGenerator.GetStorage(option);

                        return new ContentGenerator(
                            GetPathByOptions(context, option),
                            TemplateParser.Transform(context.GetTemplate(TemplateFileName), variables)
                            );
                    }

                    return default;
                })
                .Where(p => p.path != null)
                .GroupBy(p => p.path)
                .Select(p => p.First())
                .ToArray();
        }

        private ContentGenerator[] CompileInternal(GeneratorContext context, bool isForceOverride)
            => CompileInternal(
                context,
                context.GetComponents(),
                (option) => isForceOverride || !File.Exists(GetPathByOptions(context, option))
                );

        private string GetPathByOptions(GeneratorContext context, TypeOptions typeOptions)
            => Path.Combine(context.FindRootGenCommonDirectory(), TypeOptionsUtils.GetCallerFlags(typeOptions) + SaveFilePostfixName);
    }
}
