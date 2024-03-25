using AnotherECS.Core;
using System.Runtime.CompilerServices;
using UnityEngine;
using EntityId = System.UInt32;

[assembly: InternalsVisibleTo("AnotherECS.Unity.Debug.Diagnostic.Editor")]
namespace AnotherECS.Unity.Debug.Diagnostic
{
    public class EntityDiagnosticView : MonoBehaviour
    {
        internal static readonly string EntityLabel = "Entity";

        internal World World => _parent.World;

        internal VisualData visualData;

        private WorldDiagnosticView _parent;

        private bool _isDestroy;

        internal bool IsValid
        {
            get => visualData.id != 0;
        }

        internal void Construct(WorldDiagnosticView parent, EntityId id)
        {
            _parent = parent;
            gameObject.name = $"{EntityLabel}: {id}";

            visualData.owner = this;
            visualData.id = id;
        }

        internal void UpdateView()
        {
            if (transform.parent == null || transform.parent.GetComponent<WorldDiagnosticView>() == null)
            {
                DestroyView();
            }
            else
            {
                visualData.Update(_parent.World);
            }
        }

        internal void DestroyView()
        {
            if (this != null)
            {
                _isDestroy = true;
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (!_isDestroy)
            {
                if (!World.IsDisposed)
                {
                    World.SendEvent(new CheatEvent()
                    {
                        command = CheatEvent.Command.RemoveEntity,
                        id = visualData.id,
                    });
                }
            }
        }

        internal struct VisualData
        {
            public EntityDiagnosticView owner;
            public int version;

            public uint id;
            public IComponent[] components;

            public void Update(World world)
            {
                if (world.State.IsHas(id))
                {
                    var state = world.State;
                    var componentCount = (int)state.GetCount(id);
                    if (components == null || components.Length != componentCount)
                    {
                        components = new IComponent[componentCount];
                    }

                    for (int i = 0; i < componentCount; ++i)
                    {
                        components[i] = state.Read(id, (uint)i);
                    }
                }
                else
                {
                    id = 0;
                }
                ++version;
            }
        }
    }
}
