using AnotherECS.Generator;
using System.IO;
using System.Linq;
using UnityEditor;

namespace AnotherECS.Unity.Editor.Generator
{
    public class UnityEnvironmentProvider : IEnvironmentProvider
    {
        public string GetFilePathByStateName(string stateName)
        {
            var guid = AssetDatabase.FindAssets($"t:Script {stateName}").FirstOrDefault();
            if (!string.IsNullOrEmpty(guid))
            {
                var fullPath = AssetDatabase.GUIDToAssetPath(guid);
                var name = UnityGeneratorUtils.GetFileNameWithoutExtension(fullPath);

                if (name == stateName)
                {
                    return Path.GetDirectoryName(fullPath);
                }
            }
            return string.Empty;
        }

        public string FindRootGenDirectory()
            => UnityGeneratorUtils.FindRootGenDirectory();
        public string FindRootGenCommonDirectory()
            => UnityGeneratorUtils.FindRootGenCommonDirectory();
        public string GetTemplate(string fileName)
            => UnityGeneratorUtils.GetTemplate(fileName);
    }
}

