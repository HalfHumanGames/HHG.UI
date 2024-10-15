using HHG.Common.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace HHG.UISystem.Runtime
{
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour
    {
        public Button Button => button;

        public ActionEvent OnClick => onClick;

        [SerializeField] private ActionEvent onClick = new ActionEvent();

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => onClick.Invoke(this));
        }
    }
}
