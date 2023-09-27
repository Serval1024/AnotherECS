using System;
using System.IO;
using System.Linq;
using AnotherECS.Core;

namespace AnotherECS.Generator
{
    public class CallerGenerator : IFileGenerator
    {
        public string SaveFilePostfixName => "_Caller.gen.cs";
        public string TemplateFileName => "caller.template.txt";
        

        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => CompileInternal(
                context,
                context.GetComponents().Select(p => new TypeOptions(p)).ToArray(),
                (option) => isForceOverride || !File.Exists(GetPathByOptions(context, option))
                );

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
            TypeOptions[] options,
            Func<TypeOptions, bool> skipCompileRule = null
            )
            => options.Select(option =>
            {
                if (skipCompileRule(option))
                {
                    var variables = VariablesConfigGenerator.GetCaller(option);

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

        private string GetPathByOptions(GeneratorContext context, TypeOptions typeOptions)
            => Path.Combine(context.FindRootGenCommonDirectory(), TypeOptionsUtils.GetCallerFlags(typeOptions) + SaveFilePostfixName);
    }
}
