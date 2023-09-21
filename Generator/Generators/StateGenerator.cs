using AnotherECS.Core;
using System;
using System.IO;
using System.Linq;

namespace AnotherECS.Generator
{
    public class StateGenerator : IFileGenerator
    {
        public string SaveFilePostfixName => "_State.gen.cs";
        public string TemplateFileName => "state.template.txt";

        private const string STATE_NAME_POSTFIX =
            "Data";

        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => context.GetStateTypes()
            .Select(state => CompileInternal(state, context))
            .Where(p => p.path != null)
            .ToArray();

        public DeleteContentGenerator GetUnusedFiles(GeneratorContext context)
            => default;

        public string[] GetSaveFileNames(GeneratorContext context)
            => context
                .GetStateTypes()
                .Select(p => GetPathByState(context.GetStatePath(p), p.Name))
                .ExceptDublicates()
                .ToArray();

        public static string GetStateNameGen(Type stateType)
            => stateType.Name + STATE_NAME_POSTFIX;

        private ContentGenerator CompileInternal(Type stateType, GeneratorContext context)
        {
            var variables = VariablesConfigGenerator.GetState(stateType, GetStateNameGen(stateType), context.GetComponents(stateType));

            return new ContentGenerator(
                GetPathByState(context.GetStatePath(stateType), stateType.Name),
                TemplateParser.Transform(context.GetTemplate(TemplateFileName), variables)
                );

        }

        private string GetPathByState(string path, string stateName)
            => Path.Combine(path, stateName + SaveFilePostfixName);
    }
}
