using AnotherECS.Generator;

namespace AnotherECS.Unity.Jobs.Generator
{
    internal static class BagFilterVariablesConfigGenerator
        {
            public static TemplateParser.Variables Get(int count)
            {
                TemplateParser.Variables variables = null;
                variables = new()
                {
                    { "RW:TAG", () => variables.GetIndex(0) == 0 ? "R" : "W" },
                    { "R:TAG", () => variables.GetIndex(0) == 0 ? "R" : "" },
                    { "RW:READ_ONLY:TAG", () => variables.GetIndex(0) == 0 ? "[ReadOnly]" : "" },
                    { "RW:READ_ONLY", () => variables.GetIndex(0) == 1 },

                    { "R", () => variables.GetIndex(0) == 0 },
                    { "RW", () => variables.GetIndex(0) == 1 },

                    { "STRUCT_COUNT", () => count },
                    { "GENERIC_COUNT", () => variables.GetIndex(1) + 1 },
                    { "SEPARATOR1:,", () =>
                        (variables.GetIndex(2) < variables.GetLength(2) - 1)
                        ? ", "
                        : string.Empty
                    },
                };

                return variables;
            }
        }
}