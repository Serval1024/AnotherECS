using AnotherECS.Core;
using System;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
{
    internal struct TaskStatisticDataPresent : IPresent
    {
        Type IPresent.Type => typeof(TaskStatisticData);

        VisualElement IPresent.Create(ObjectProperty property)
        {
            var value = property.GetValue<TaskStatisticData>();

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
            var value = property.GetValue<TaskStatisticData>();

            var statisticTimeLabel = container.Q<Label>("statistic-time__label");

            var median = value.timer.GetAverage();
            statisticTimeLabel.text = $"Average: '{median.ToStringDisplay()} ms'.";
        }

        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange) { }
    }
}

