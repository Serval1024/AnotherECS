using System.Globalization;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class UxmlSfloatAttributeDescription : TypedUxmlAttributeDescription<sfloat>
    {
        public override string defaultValueAsString => base.defaultValue.ToString(CultureInfo.InvariantCulture.NumberFormat);

        public UxmlSfloatAttributeDescription()
        {
            base.type = "float";
            base.typeNamespace = "http://www.w3.org/2001/XMLSchema";
            base.defaultValue = default;
        }

        public override sfloat GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
            => GetValueFromBag(bag, cc, (string s, sfloat l) => ConvertValueToSfloat(s, l), base.defaultValue);

        public bool TryGetValueFromBag(IUxmlAttributes bag, CreationContext cc, ref sfloat value)
            => TryGetValueFromBag(bag, cc, (string s, sfloat l) => ConvertValueToSfloat(s, l), base.defaultValue, ref value);

        private static sfloat ConvertValueToSfloat(string v, sfloat defaultValue)
            => (v == null || !sfloat.TryParse(v, out var result))
            ? defaultValue
            : result;
    }
}