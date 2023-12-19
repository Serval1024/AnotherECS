using AnotherECS.Converter;
using AnotherECS.Core;
using System;
using System.IO;
using System.Linq;

namespace AnotherECS.Generator
{
    public class StateGenerator : IFileGenerator
    {
        public string SaveFilePostfixName => ".gen.cs";
        public string TemplateFileName => "state.template.txt";

        private const string STATE_NAME_POSTFIX = "Compile";

        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => context.GetStateTypes()
                .Select(state => CompileInternal(context, state, state.Name))
                .Where(p => p.path != null)
                .ToArray();

        public DeleteContentGenerator GetUnusedFiles(GeneratorContext context)
            => default;

        public string[] GetSaveFileNames(GeneratorContext context)
            => context
                .GetStateTypes()
                .Select(p => GetPathByState(context.GetStatePath(p.Name), p.Name))
                .ExceptDublicates()
                .ToArray();

        public ContentGenerator Compile(GeneratorContext context, string stateName)
            => CompileInternal(context, stateName);

        public static string GetStateNameGen(string stateName)
            => GetStateName(stateName, STATE_NAME_POSTFIX);

        public string GetPathByState(string path, string stateName)
            => Path.Combine(path, GetStateName(stateName, SaveFilePostfixName));

        public static string GetStateName(string stateName, string postfix)
            => stateName + postfix;

        private ContentGenerator CompileInternal(GeneratorContext context, string stateName)
            => CompileInternal(context, typeof(MockState), stateName);

        private ContentGenerator CompileInternal(GeneratorContext context, Type state, string stateName)
        {
            var variables = VariablesConfigGenerator.GetState(context, stateName, context.GetComponents(state), context.GetConfigs(state));

            return new ContentGenerator(
                GetPathByState(context.GetStatePath(stateName), stateName),
                TemplateParser.Transform(context.GetTemplate(TemplateFileName), variables)
                );

        }

        [IgnoreCompile]
        private class MockState : IState { }
    }
}
