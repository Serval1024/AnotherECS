using AnotherECS.Debug.Diagnostic.UIElements;
using AnotherECS.Unity.Debug.Diagnostic.Present;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using EntityId = System.UInt32;

namespace AnotherECS.Unity.Debug.Diagnostic.Editor
{
    [CustomEditor(typeof(EntityDiagnosticView))]
    internal class EntityDiagnosticViewEditor : UnityEditor.Editor
    {
        [SerializeField]
        public StyleSheet uss;

        private EntityDiagnosticView Target
          => (EntityDiagnosticView)target;

        private int _version;

        public override VisualElement CreateInspectorGUI()
        {
            var visualElement = CreateLayout();
            UpdateLayout(visualElement);

            visualElement.schedule.Execute(() => UpdateLayout(visualElement)).Every(100);

            return visualElement;
        }

        private VisualElement CreateLayout()
        {
            var container = new VisualElement();
            container.styleSheets.Add(uss);

            var classField = new ClassField("Components", ClassField.Option.SkipFirstLabel);
            classField.SetValueWithoutNotify(Target.visualData.components);
            classField.name = "components-field";

            classField.RegisterValueChangeCallback(OnChange);

            container.Add(classField);

            container.RegisterCallback<EntityPresent.EntityLocatedButtonEvent>(p => OnLocateEntity(p.id));

            return container;
        }

        private void UpdateLayout(VisualElement container)
        {
            if (_version != Target.visualData.version)
            {
                _version = Target.visualData.version;
                container.Q<ClassField>("components-field").SetValueWithoutNotify(Target.visualData.components);
            }
        }

        private void OnChange(ObjectProperty property, object previousValue, object value)
        {
            if (Target.visualData.id != 0)
            {
                Target.World.Send(new CheatEvent()
                {
                    command = CheatEvent.Command.ChangeComponent,
                    id = Target.visualData.id,
                    componentIndex = (uint)property.GetPathIterator().GetIndex(),
                    pathInsideComponent = property.GetPathIterator().Next().ToPath(),
                    value = value,
                });
            }
        }

        internal void OnLocateEntity(EntityId id)
        {
            if (Target.visualData.id == id)
            {
                LocateEntity();
            }
        }

        internal void LocateEntity()
        {
            var trSelected = Target.transform.parent.Find($"{EntityDiagnosticView.EntityLabel}: {Target.visualData.id}");
            if (trSelected != null)
            {
                Selection.activeObject = trSelected.gameObject;
                EditorGUIUtility.PingObject(trSelected.gameObject);
            }
        }
    }
}
