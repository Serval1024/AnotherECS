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

        public static void SetWithCheck<TTextValueField, TValue>(ObjectProperty value, VisualElement container)
            where TTextValueField : BaseField<TValue>
        {
            var textValueField = (BaseField<TValue>)container;
            var newValue = value.GetValue<TValue>();
            if (!EqualityComparer<TValue>.Default.Equals(textValueField.value, newValue))
            {
                textValueField.SetValueWithoutNotify(newValue);
            }
        }
    }
}

