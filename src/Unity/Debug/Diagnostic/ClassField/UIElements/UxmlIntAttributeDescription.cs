using System.Globalization;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class UxmlIntAttributeDescription : TypedUxmlAttributeDescription<int>
    {
        public override string defaultValueAsString => base.defaultValue.ToString(CultureInfo.InvariantCulture.NumberFormat);

        public UxmlIntAttributeDescription()
        {
            base.type = "int";
            base.typeNamespace = "http://www.w3.org/2001/XMLSchema";
            base.defaultValue = default;
        }

        public override int GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
            => GetValueFromBag(bag, cc, (string s, int l) => ConvertValueToInt(s, l), base.defaultValue);

        public bool TryGetValueFromBag(IUxmlAttributes bag, CreationContext cc, ref int value)
            => TryGetValueFromBag(bag, cc, (string s, int l) => ConvertValueToInt(s, l), base.defaultValue, ref value);

        private static int ConvertValueToInt(string v, int defaultValue)
            => (v == null || !int.TryParse(v, out var result))
            ? defaultValue
            : result;
    }
}