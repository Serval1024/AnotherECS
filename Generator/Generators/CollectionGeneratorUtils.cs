namespace AnotherECS.Generator
{
    internal static class CollectionGeneratorUtils
    {
        public static TemplateParser.Variables GetVariablesDefault(int[] collectionSizes)
        {
            TemplateParser.Variables variables = null;
            variables = new()
            {
                { "STRUCT_COUNT", p => collectionSizes.Length.ToString() },
                { "ELEMENT_COUNT", p => collectionSizes[p].ToString() },
                { "SEPARATOR1:,", p =>
                    (variables.GetIndex(1) < variables.GetLength(1) - 1)
                    ? ", "
                    : string.Empty
                },
            };

            return variables;
        }
    }
}
