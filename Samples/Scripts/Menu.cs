using TMPro;

namespace HHG.UI.Sample
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