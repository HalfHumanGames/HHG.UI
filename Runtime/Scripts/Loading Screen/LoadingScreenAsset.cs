using UnityEngine;

namespace HHG.UI.Runtime
{
    [CreateAssetMenu(fileName = "Loading Screen", menuName = "HHG/UI/Views/Loading Screen")]
    public class LoadingScreenAsset : UIAsset<LoadingScreenData>
    {
        public bool IsDefault => isDefault;

        [SerializeField] private bool isDefault;
    }
}