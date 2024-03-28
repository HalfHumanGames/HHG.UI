using HHG.UISystem.Runtime;
using TMPro;

namespace HHG.UISystem.Sample
{
    public class Menu : UIView
    {
        public MenuType Type;
        public TextMeshProUGUI TitleLabel;

        public override object Id => Type;

        protected override void Awake()
        {
            base.Awake();
            TitleLabel.text = Type.ToString().Replace("_", " ");
        }
    } 
}