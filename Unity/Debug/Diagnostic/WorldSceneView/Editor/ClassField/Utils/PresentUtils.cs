using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Editor
{
    internal static class PresentUtils
    {
        public static VisualElement CreateGroupBox(string label)
        {
            var container = CreateGroup(label);
            container.Q("group-content").style.paddingLeft = new StyleLength(new Length(15, LengthUnit.Pixel));
            return container;
        }

        public static VisualElement CreateGroup(string label)
        {
            var container = new VisualElement();

            if (!string.IsNullOrEmpty(label))
            {
                var labelElement = new Label(label);
                labelElement.style.paddingLeft = 4;
                container.Add(labelElement);
            }

            var groupContent = new VisualElement
            {
                name = "group-content"
            };

            container.Add(groupContent);

            return container;
        }

        public static VisualElement CreateHorizontal(string label)
        {
            var container = CreateGroup(label);
            container.Q("group-content").style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            return container;
        }

        public static TTextValueField CreateField<TTextValueField, TValue>(ref ObjectProperty property)
            where TTextValueField : TextInputBaseField<TValue>, new()
            => new()
            {
                label = property.GetFieldDisplayName(),
                isDelayed = true,
            };


        public static TTextValueField CreateField<TTextValueField, TValue, TSubTextValueField, TSubValue>(ref ObjectProperty property)
            where TTextValueField : AnotherECS.Debug.Diagnostic.Editor.UIElements.BaseCompositeField<TValue, TSubTextValueField, TSubValue>, new()
            where TSubTextValueField : TextValueField<TSubValue>, new()
            => new()
            {
                label = property.GetFieldDisplayName(),
                isDelayed = true,
            };

        public static Toggle CreateFieldBool(ref ObjectProperty property)
            => new()
            {
                label = property.GetFieldDisplayName(),
            };

        public static void SetWithCheck<TTextValueField, TValue>(ref ObjectProperty property, VisualElement container)
            where TTextValueField : BaseField<TValue>
        {
            var textValueField = (BaseField<TValue>)container;
            var newValue = property.GetValue<TValue>();
            if (!EqualityComparer<TValue>.Default.Equals(textValueField.value, newValue))
            {
                textValueField.SetValueWithoutNotify(newValue);
            }
        }
    }
}

