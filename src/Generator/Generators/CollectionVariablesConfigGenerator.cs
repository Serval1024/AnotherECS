namespace AnotherECS.Generator
{
    internal static class CollectionVariablesConfigGenerator
    {
        public static TemplateParser.Variables Get(int[] collectionSizes)
        {
            TemplateParser.Variables variables = null;
            variables = new()
            {
                { "STRUCT_COUNT", () => collectionSizes.Length.ToString() },
                { "ELEMENT_COUNT", () => collectionSizes[variables.GetIndex(0)].ToString() },
                { "SEPARATOR1:,", () =>
                    (variables.GetIndex(1) < variables.GetLength(1) - 1)
                    ? ", "
                    : string.Empty
                },
            };

            return variables;
        }
    }
}
