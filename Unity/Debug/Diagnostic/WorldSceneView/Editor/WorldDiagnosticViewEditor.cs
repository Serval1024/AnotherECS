using UnityEngine.UIElements;
using UnityEditor;

namespace AnotherECS.Unity.Debug.Diagnostic.Editor
{
    [CustomEditor(typeof(WorldDiagnosticView))]
    public class WorldDiagnosticViewEditor : UnityEditor.Editor
    {
        private WorldDiagnosticView Target 
            => (WorldDiagnosticView)target;

        public override VisualElement CreateInspectorGUI()
        {
            var container = CreateLayout();
            UpdateLayout(container);

            container.schedule.Execute(() => UpdateLayout(container)).Every(100);
            
            return container;
        }

        private VisualElement CreateLayout()
        {
            var container = new VisualElement();
            var worldLabel = new Label()
            {
                name = "world-name"
            };
            container.Add(worldLabel);

            var entityCount = new Label()
            {
                name = "world-entity-count"
            };
            container.Add(entityCount);

            var componentTotal = new Label()
            {
                name = "world-component-total"
            };
            container.Add(componentTotal);

            return container;
        }

        private void UpdateLayout(VisualElement container)
        {
            var visualData = Target.visualData;

            container.Q<Label>("world-name").text = string.IsNullOrEmpty(visualData.worldName) ? $"Name: <No name>" : $"Name: '{visualData.worldName}'";
            container.Q<Label>("world-entity-count").text = $"Entity total: {visualData.entityCount}";
            container.Q<Label>("world-component-total").text = $"Component total: {visualData.componentTotal}";
        }
    }
}