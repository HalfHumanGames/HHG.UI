using UnityEngine;

namespace HHG.UI
{ 
    public class UIView : UIElement
    {
        protected override void Awake()
        {
            base.Awake();
            RectTransform.anchoredPosition = Vector2.zero;
        }

        protected override void Start()
        {
            base.Start();
            InitializeState();
        }

        internal override void InitializeState()
        {
            children.ForEach(child => child.InitializeState());
            switch (state)
            {
                case OpenState.Closed:
                    Close(true);
                    break;
                case OpenState.Opening:
                    state = OpenState.Open;
                    Close(true);
                    UI.Push(GetType(), Id);
                    break;
                case OpenState.Open:
                    Close(true);
                    UI.Push(GetType(), Id, true);
                    break;
                case OpenState.Closing:
                    state = OpenState.Closed;
                    Open(true);
                    Close(false);
                    break;
            }
        }
    }
}