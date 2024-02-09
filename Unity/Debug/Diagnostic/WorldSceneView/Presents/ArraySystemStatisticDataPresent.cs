using AnotherECS.Core;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
{
    internal struct ArraySystemStatisticDataPresent : IPresent
    {
        Type IPresent.Type => typeof(SystemStatisticData[]);

        public VisualElement Create(ObjectProperty property)
        {
            var container = new VisualElement();
            IPresent present = new SystemStatisticDataPresent();
            var lastGroups = new HashSet<int>();

            int group = -1;
            foreach (var value in property.GetChildren())
            {
                var content = new VisualElement();
                var space = new VisualElement();
                space.AddToClassList("vertical-space-small");
                content.Add(space);

                var valueData = value.GetValue<SystemStatisticData>();
                var cGroup = valueData.group;
                var cDeep = valueData.deep;

                if (group != cGroup)
                {
                    if (!lastGroups.Contains(cGroup))
                    {
                        content.Add(CreateGroupLabel());
                        lastGroups.Add(group);
                    }
                    group = cGroup;
                }

                var view = present.Create(value);
                view.name = "system-statistic-container";
                content.Add(view);

                content.style.paddingLeft = 15 * cDeep;

                container.Add(content);
            }
            

            return container;
        }

        private VisualElement CreateGroupLabel()
        {
            var label = new Label("System Group =>");
            label.AddToClassList("label-head");
            return label;
        }

        public void Set(ObjectProperty property, VisualElement container)
        {
            IPresent present = new SystemStatisticDataPresent();

            int index = 0;
            foreach (var value in property.GetChildren())
            {
                present.Set(value, container.ElementAt(index).Q("system-statistic-container"));
                ++index;
            }
        }

        public void Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange) { }
    }
}

