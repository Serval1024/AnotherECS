using System;
using System.Collections.Generic;
using AnotherECS.Converter;
using AnotherECS.Core;

namespace AnotherECS.Generator
{
    public class SystemGenerator : IFileGenerator
    {
        public string TemplateFileName => "system.template.txt";


        private readonly Dictionary<Type, Type> _typeMap = new();

        public SystemGenerator(Type stateType)
        {
            _typeMap.Add(typeof(IState), stateType);
            _typeMap.Add(typeof(State), stateType);
        }

        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => new[] { CompileInternal(context) };

        public DeleteContentGenerator GetUnusedFiles(GeneratorContext context)
            => default;


        public string[] GetSaveFileNames(GeneratorContext context)
            => Array.Empty<string>();

        private ContentGenerator CompileInternal(GeneratorContext context)
        {
            var systems = context.GetSystems();

            TemplateParser.Variables variables = new()
            {
                { "SYSTEM_NAME", p => GetSystemName(systems, p) },
                { "SYSTEM_COUNT", p => systems.Count().ToString() },
            };

            return new ContentGenerator(string.Empty, TemplateParser.Transform(context.GetTemplate(TemplateFileName), variables));
        }

        private string GetSystemName(ITypeToUshort systems, int index)
        {
            try
            {
                return ReflectionUtils.GetGeneratorFullName(systems.IdToType((ushort)(index + 1)), _typeMap);
            }
            catch(InvalidOperationException e)
            {
                throw new Exception("Unable to sort generic system.", e);
            }
        }
    }
}
