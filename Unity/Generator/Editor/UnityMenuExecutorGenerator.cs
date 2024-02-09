using AnotherECS.Core;
using AnotherECS.Generator;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Logger = AnotherECS.Debug.Logger;

namespace AnotherECS.Unity.Editor.Generator
{
    public class UnityMenuExecutorGenerator
    {
        [MenuItem("Assets/AnotherECS/Compile Template")]
        private static void CompileTemplate()
        {
            if (Selection.activeObject != null && Selection.activeObject is TextAsset textAsset)
            {
                var metaExpression = TemplateParser.GetMetaHeader(textAsset.text);
                if (metaExpression != null)
                {
                    var sourcePath = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(textAsset)), metaExpression.FileName);
                    var generator = (IGenerator)System.Activator.CreateInstance(metaExpression.GeneratorType, new[] { sourcePath, textAsset.text });
                    if (generator is IArgumentGenerator argumentGenerator)
                    {
                        argumentGenerator.SetArgs(metaExpression.N);
                    }
                    var contentGenerator = generator.Compile(null, true).First();
                    
                    var destinationPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), contentGenerator.path);

                    UnityGeneratorUtils.SaveFile(destinationPath, contentGenerator.text);
                    Logger.CompileFinished();
                }
                else
                {
                    Logger.CompileFailed();
                }
            }
        }

        [MenuItem("Assets/AnotherECS/Compile Template", true)]
        private static bool CompileTemplateVaidate()
            => Selection.activeObject != null
            && Selection.activeObject is TextAsset textAsset
            && TemplateParser.MetaExpression.Is(textAsset.text);

        [MenuItem("Assets/AnotherECS/Create State")]
        private static void CreateState()
        {
            if (Selection.activeObject != null)
            {
                var stateName = StateGenerator.GetStateNameGen(nameof(State));
                var folder = UnityGeneratorUtils.GetSelectedFolder();
                var generator = new StateGenerator();
                
                (string path, string subName) = UnityGeneratorUtils.FindNoExistsFile(p => generator.GetPathByState(folder, stateName + p));

                var content = generator.Compile(new GeneratorContext(new UnityEnvironmentProvider()), stateName + subName);
                UnityGeneratorUtils.SaveFile(path, content.text);

                UnityListenerGenerator.Compile(true);
            }
        }

    }
}
