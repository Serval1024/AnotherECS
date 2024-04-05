using System.Globalization;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class UxmlUlongAttributeDescription : TypedUxmlAttributeDescription<ulong>
    {
        public override string defaultValueAsString => base.defaultValue.ToString(CultureInfo.InvariantCulture.NumberFormat);

        public UxmlUlongAttributeDescription()
        {
            base.type = "ulong";
            base.typeNamespace = "http://www.w3.org/2001/XMLSchema";
            base.defaultValue = default;
        }

        public override ulong GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
            => GetValueFromBag(bag, cc, (string s, ulong l) => ConvertValueToUlong(s, l), base.defaultValue);

        public bool TryGetValueFromBag(IUxmlAttributes bag, CreationContext cc, ref ulong value)
            => TryGetValueFromBag(bag, cc, (string s, ulong l) => ConvertValueToUlong(s, l), base.defaultValue, ref value);

        private static ulong ConvertValueToUlong(string v, ulong defaultValue)
            => (v == null || !ulong.TryParse(v, out var result))
            ? defaultValue
            : result;
    }
}