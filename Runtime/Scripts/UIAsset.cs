using HHG.Common.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace HHG.UISystem.Runtime
{
    [CreateAssetMenu(fileName = "UI", menuName = "HHG/Assets/Variable/UI")]
    public class UIAsset : StringAsset
    {
        [SerializeField, FormerlySerializedAs("view")] private GameObject prefab;

        private void OnValidate()
        {
            if (prefab != null && prefab.TryGetComponent(out UI ui))
            {
                string name = ui.GetType().AssemblyQualifiedName;

                if (value != name)
                {
                    value = name;
                }
            }
        }
    }

    public abstract class UIAssetT : UIAsset
    {
        public abstract object WeakData { get; }
    }

    public class UIAsset<T> : UIAssetT
    {
        public T Data => data;
        public override object WeakData => data;

        [SerializeField] private T data;
    }
}