using AnotherECS.Core;
using AnotherECS.Debug.Diagnostic.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Editor
{
    [CustomEditor(typeof(WorldDiagnosticView))]
    public class WorldDiagnosticViewEditor : UnityEditor.Editor
    {
        [SerializeField]
        public StyleSheet uss;

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

            container.styleSheets.Add(uss);

            container.Add(CreateGeneral());
            container.Add(CreateSpace());
            container.Add(CreateSystems());

            return container;
        }

        private VisualElement CreateSpace()
        {
            var space = new VisualElement();
            space.AddToClassList("vertical-space");
            return space;
        }

        private VisualElement CreateSystems()
        {
            var tagField = new TabMenu()
            {
                name = "world-systems",
            };

            tagField.SetTabsAsString(
                new[] {
                    "Create", "Tick", "Destroy",
                    "Create Module", "Destroy Module", "Tick Started", "Tick Finished",
                    "State Tick Start", "State Tick Finished", "Revert Time",
                    }
                );

            var contents = new VisualElement[tagField.TabsCount];
            for (int i = 0; i < tagField.TabsCount; ++i)
            {
                contents[i] = new ClassField(ClassField.Option.SkipFirstLabel)
                {
                    name = "world-systems__tab",
                };
            }
            tagField.Contents = contents;
            tagField.SelectIndex = 1;

            return tagField;
        }

        private VisualElement CreateGeneral()
        {
            var container = new VisualElement
            {
                name = "world-general"
            };
            container.AddToClassList("label-head");

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

            container.Add(new Label("Unsafe memory:"));

            var memoryGroup = new VisualElement();
            memoryGroup.AddToClassList("group-content");

            container.Add(memoryGroup);

            var memory = new Label()
            {
                name = "world-memory"
            };
            memoryGroup.Add(memory);

            var memoryHistory = new Label()
            {
                name = "world-memory-history"
            };
            memoryGroup.Add(memoryHistory);

            var memoryTotal = new Label()
            {
                name = "world-memory-total"
            };
            memoryGroup.Add(memoryTotal);
            
            return container;
        }


        private void UpdateLayout(VisualElement container)
        {
            var visualData = Target.visualData;

            container.Q<Label>("world-name").text = string.IsNullOrEmpty(visualData.data.worldName) ? $"Name: <No name>" : $"Name: '{visualData.data.worldName}'";
            container.Q<Label>("world-entity-count").text = $"Entity total: {visualData.data.entityCount}.";
            container.Q<Label>("world-component-total").text = $"Component total: {visualData.data.componentTotal}.";

            var memoryState = visualData.data.memoryTotal - visualData.data.historyMemoryTotal;

            var factor0 = memoryState * 100.0 / visualData.data.memoryTotal;
            if (!double.IsNormal(factor0))
            {
                factor0 = 0f;
            }
            container.Q<Label>("world-memory").text =
                $"State: {DisplayUtils.ToStringMemory(memoryState)}. ({factor0:0}%)";

            var factor1 = visualData.data.historyMemoryTotal * 100.0 / visualData.data.memoryTotal;
            if (!double.IsNormal(factor1))
            {
                factor1 = 0f;
            }
            container.Q<Label>("world-memory-history").text =
                $"History: {DisplayUtils.ToStringMemory(visualData.data.historyMemoryTotal)}. ({factor1:0}%)";

            container.Q<Label>("world-memory-total").text =
                $"Total: {DisplayUtils.ToStringMemory(visualData.data.memoryTotal)}.";


            var contentsTabMenu = container.Q<TabMenu>("world-systems").Contents;

            contentsTabMenu[0].Q<ClassField>("world-systems__tab").value = visualData.data.createSystems;
            contentsTabMenu[1].Q<ClassField>("world-systems__tab").value = visualData.data.tickSystems;
            contentsTabMenu[2].Q<ClassField>("world-systems__tab").value = visualData.data.destroySystems;

            contentsTabMenu[3].Q<ClassField>("world-systems__tab").value = visualData.data.createModule;
            contentsTabMenu[4].Q<ClassField>("world-systems__tab").value = visualData.data.destroyModule;
            
            contentsTabMenu[5].Q<ClassField>("world-systems__tab").value = visualData.data.tickStartedModule;
            contentsTabMenu[6].Q<ClassField>("world-systems__tab").value = visualData.data.tickFinishedModule;

            contentsTabMenu[7].Q<ClassField>("world-systems__tab").value = visualData.data.stateTickStart;
            contentsTabMenu[8].Q<ClassField>("world-systems__tab").value = visualData.data.stateTickFinished;
            contentsTabMenu[9].Q<ClassField>("world-systems__tab").value = visualData.data.stateRevertTo;
        }
    }
}