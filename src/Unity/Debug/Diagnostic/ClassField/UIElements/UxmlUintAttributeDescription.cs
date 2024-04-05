using System.Globalization;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class UxmlUintAttributeDescription : TypedUxmlAttributeDescription<uint>
    {
        public override string defaultValueAsString => base.defaultValue.ToString(CultureInfo.InvariantCulture.NumberFormat);

        public UxmlUintAttributeDescription()
        {
            base.type = "uint";
            base.typeNamespace = "http://www.w3.org/2001/XMLSchema";
            base.defaultValue = default;
        }
        
        public override uint GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
            => GetValueFromBag(bag, cc, (string s, uint l) => ConvertValueToUint(s, l), base.defaultValue);

        public bool TryGetValueFromBag(IUxmlAttributes bag, CreationContext cc, ref uint value)
            => TryGetValueFromBag(bag, cc, (string s, uint l) => ConvertValueToUint(s, l), base.defaultValue, ref value);

        private static uint ConvertValueToUint(string v, uint defaultValue)
            => (v == null || !uint.TryParse(v, out var result))
            ? defaultValue
            : result;
    }
}