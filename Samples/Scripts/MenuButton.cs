using UnityEngine;
using UnityEngine.UI;
using HHG.UISystem.Runtime;

namespace HHG.UISystem.Sample
{
    [RequireComponent(typeof(Button))]
    public class MenuButton : MonoBehaviour
    {
        public MenuAction Action;
        public MenuType Type;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        public void OnClick()
        {
            switch (Action)
            {
                case MenuAction.GoTo:
                    Runtime.UI.GoTo<Menu>(Type);
                    break;
                case MenuAction.Push:
                    Runtime.UI.Push<Menu>(Type);
                    break;
                case MenuAction.Pop:
                    Runtime.UI.Pop();
                    break;
                case MenuAction.Clear:
                    Runtime.UI.Clear();
                    break;
                case MenuAction.Swap:
                    Runtime.UI.Swap<Menu>(Type);
                    break;
            }
        }
    } 
}