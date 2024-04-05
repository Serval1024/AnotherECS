using AnotherECS.Core;
using System;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
{
    internal struct SystemStatisticDataPresent : IPresent
    {
        Type IPresent.Type => typeof(SystemStatisticData);

        VisualElement IPresent.Create(ObjectProperty property)
        {
            var value = property.GetValue<SystemStatisticData>();

            var container = new VisualElement();
            container.AddToClassList("statistic-deep");

            var nameLabel = new Label(value.name);
            container.Add(nameLabel);

            var statisticTime = new VisualElement()
            {
                name = "statistic-time"
            };
            container.Add(statisticTime);

            var timeLabel = new Label()
            {
                name = "statistic-time__label"
            };
            statisticTime.Add(timeLabel);  

            return container;
        }
            

        void IPresent.Set(ObjectProperty property, VisualElement container)
        {
            var value = property.GetValue<SystemStatisticData>();

            var statisticTimeLabel = container.Q<Label>("statistic-time__label");

            var median = value.timer.GetAverage();
            statisticTimeLabel.text = $"Average: '{median.ToStringDisplay()} ms'.";
        }

        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange) { }
    }
}

