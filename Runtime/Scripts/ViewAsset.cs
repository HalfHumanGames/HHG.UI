using HHG.Common.Runtime;
using UnityEngine;

namespace HHG.UISystem.Runtime
{
    [CreateAssetMenu(fileName = "View", menuName = "HHG/Assets/Variable/View")]
    public class ViewAsset : StringAsset
    {
        [SerializeField] private GameObject view;

        private void OnValidate()
        {
            if (view != null && view.GetComponent<UIView>() is UIView ui)
            {
                string tempValue = ui.GetType().AssemblyQualifiedName;

                if (value != tempValue)
                {
                    value = tempValue;
                }
            }
        }
    }

    public abstract class ViewAssetT : ViewAsset
    {
        public abstract object WeakData { get; }
    }

    public class ViewAsset<T> : ViewAssetT
    {
        public T Data => data;
        public override object WeakData => data;

        [SerializeField] private T data;
    }
}