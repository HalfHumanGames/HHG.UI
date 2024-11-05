using UnityEngine;

namespace HHG.UISystem.Runtime
{
    [CreateAssetMenu(fileName = "Loading Screen", menuName = "HHG/UI/Views/Loading Screen")]
    public class LoadingScreenAsset : UIAsset<LoadingScreen>
    {
        public bool IsDefault => isDefault;

        [SerializeField] private bool isDefault;
    }
}