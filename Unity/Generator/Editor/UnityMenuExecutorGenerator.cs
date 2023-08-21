using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using AnotherECS.Generator;
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
    }
}
