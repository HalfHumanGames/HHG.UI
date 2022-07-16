using UnityEngine;
using UnityEngine.UI;

namespace HHG.UI.Sample
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
                    UI.GoTo<Menu>(Type);
                    break;
                case MenuAction.Push:
                    UI.Push<Menu>(Type);
                    break;
                case MenuAction.Pop:
                    UI.Pop();
                    break;
                case MenuAction.Clear:
                    UI.Clear();
                    break;
                case MenuAction.Swap:
                    UI.Swap<Menu>(Type);
                    break;
            }
        }
    } 
}