using AnotherECS.Core;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;

namespace AnotherECS.Generator
{
    public class AdapterGenerator : IFileGenerator
    {
        public string SaveFilePostfixName => "_Adapter.gen.cs";
        public string TemplateFileName0 => "adapter.template.txt";
        public string TemplateFileName1 => "adapterS.template.txt";
        

        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => CompileInternal(context, isForceOverride);

        public DeleteContentGenerator GetUnusedFiles(GeneratorContext context)
           => new(GetSaveFileNames(context), SaveFilePostfixName);

        public string[] GetSaveFileNames(GeneratorContext context)
            => context
                .GetComponentTypesExceptDublicates()
                .Select(p => GetPathByOptions(context, new TypeOptions(p), context.GetComponentBindWithFilter()))
                .ExceptDublicates()
                .ToArray();

        private ContentGenerator[] CompileInternal(
            GeneratorContext context,
            Type[] types,
            GeneratorContext.ComponentFilterData componentFilterData,
            Func<TypeOptions, GeneratorContext.ComponentFilterData, bool> skipCompileRule = null
            )
        {
            var (includes, excludes) = componentFilterData;

            return types
                .Select(p => new TypeOptions(p))
                .Select(option =>
                {
                    if (skipCompileRule == null || skipCompileRule(option, componentFilterData))
                    {
                        var variables = GeneratorHelper.DefaultVariables(option, componentFilterData);
                        variables.Add("FILTER", p => (includes.Contains(option.type) || excludes.Contains(option.type)).ToString());
                        variables.Add("INCLUDE", p => includes.Contains(option.type).ToString());
                        variables.Add("EXCLUDE", p => excludes.Contains(option.type).ToString());

                        return new ContentGenerator(
                            GetPathByOptions(context, option, componentFilterData),
                            TemplateParser.Transform(context.GetTemplate(GetTemplate(option)), variables)
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
                context.GetComponentTypesExceptDublicates(),
                context.GetComponentBindWithFilter(),
                (option, componentFilterData) => isForceOverride || !File.Exists(GetPathByOptions(context, option, componentFilterData))
                );

        private string GetTemplate(TypeOptions typeOptions)
            => typeOptions.isShared ? TemplateFileName1 : TemplateFileName0;

        private string GetPathByOptions(GeneratorContext context, TypeOptions typeOptions, GeneratorContext.ComponentFilterData componentFilterData)
            => Path.Combine(context.FindRootGenCommonDirectory(), TypeOptionsUtils.GetAdapterFlags(typeOptions, componentFilterData) + SaveFilePostfixName);
    }
}
