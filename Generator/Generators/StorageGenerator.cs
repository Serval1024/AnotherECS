using AnotherECS.Core;
using System;
using System.IO;
using System.Linq;

namespace AnotherECS.Generator
{
    public class StorageGenerator : IFileGenerator
    {
        public string SaveFilePostfixName => "_Storage.gen.cs";
        public string TemplateFileName0 => "storage.template.txt";
        public string TemplateFileName1 => "storageS.template.txt";


        private const string CONSTRUCT_HISTORY = ", <#IF HISTORY:BYCHANGE#>HistoryByChangeArgs<#ELSE#>HistoryByTickArgs<#END#> historyArgs";
        private const string CONSTRUCT_VERSION = ", TickProvider tickProvider";
        private const string CONSTRUCT_INJECT = ", ref InjectContainer injectContainer, IInjectMethodsReference injectMethods";


        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => CompileInternal(
                context,
                context.GetComponentTypesExceptDublicates().Select(p => new TypeOptions(p)).ToArray(),
                (option) => isForceOverride || !File.Exists(GetPathByOptions(context, option))
                );

        public DeleteContentGenerator GetUnusedFiles(GeneratorContext context)
           => new(GetSaveFileNames(context), SaveFilePostfixName);

        public string[] GetSaveFileNames(GeneratorContext context)
            => context
                .GetComponentTypesExceptDublicates()
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
                    var variables = GeneratorHelper.DefaultVariables(option);
                    variables.Add("CONSTRUCT_HISTORY", p => option.isHistory ? CONSTRUCT_HISTORY : string.Empty);
                    variables.Add("CONSTRUCT_VERSION", p => option.isVersion ? CONSTRUCT_VERSION : string.Empty);
                    variables.Add("CONSTRUCT_INJECT", p => option.isInject ? CONSTRUCT_INJECT : string.Empty);
                    

                    return new ContentGenerator(
                        GetPathByOptions(context, option),
                        TemplateParser.Transform(context.GetTemplate(GetTemplate(option)), variables)
                        );
                }
                return default;
            })
            .Where(p => p.path != null)
            .GroupBy(p => p.path)
            .Select(p => p.First())
            .ToArray();

        private string GetTemplate(TypeOptions typeOptions)
            => typeOptions.isShared ? TemplateFileName1 : TemplateFileName0;

        private string GetPathByOptions(GeneratorContext context, TypeOptions typeOptions)
            => Path.Combine(context.FindRootGenCommonDirectory(), TypeOptionsUtils.GetStorageFlags(typeOptions) + SaveFilePostfixName);
    }
}
