using HHG.UI.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HHG.Common.Runtime
{
    public class UICarousel : UI<CarouselData>
    {
        [SerializeField] private float fadeDuration = .25f;
        [SerializeField] private bool hideButtons;
        [SerializeField] private InputActionReference continueAction;
        [SerializeField] private List<Image> images = new List<Image>();
        [SerializeField] private List<TMP_Text> labels = new List<TMP_Text>();
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button jumpButton;
        [SerializeField] private ActionEvent onContinue;

        private List<Button> buttons = new List<Button>();
        private Coroutine coroutine;
        private int current;

        private Color color
        {
            get => images[0].color;
            set
            {
                foreach (Graphic graphic in Enumerable.Empty<Graphic>().Concat(images).Concat(labels))
                {
                    graphic.color = value;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            previousButton.onClick.AddListener(PageLeft);
            nextButton.onClick.AddListener(PageRight);
            jumpButton.gameObject.SetActive(false);
        }

        private IEnumerator RefreshAsync()
        {
            yield return TweenUtil.TweenAsync(() => color, value => color = value, Color.white.WithA(0f), fadeDuration, TimeScale.Unscaled, EaseUtil.InOutQuad);
            RefreshNow(); // Do in the middle of the fade animation
            yield return TweenUtil.TweenAsync(() => color, value => color = value, Color.white.WithA(1f), fadeDuration, TimeScale.Unscaled, EaseUtil.InOutQuad);
            CoroutineUtil.StopAndNullifyCoroutine(ref coroutine);
        }

        private void RefreshNow()
        {
            CarouselItemData item = data.Items[current];

            UpdateGraphics(images, item.Sprites, (image, sprite) => image.sprite = sprite);
            UpdateGraphics(labels, item.Texts, (label, text) => label.text = text);

            buttons[current].interactable = true;
            previousButton.interactable = current > 0;

            if (!previousButton.interactable && previousButton.IsSelected())
            {
                nextButton.Select();
            }

            previousButton.ClearNavigation();
            nextButton.ClearNavigation();
            buttons.ClearNavigation();

            previousButton.SetNavigationRight(nextButton);
            previousButton.SetNavigationDown(buttons.FirstOrDefault());
            nextButton.SetNavigationLeft(previousButton);

            IEnumerable<Button> activeButtons = buttons.Where(b => b.isActiveAndEnabled && b.interactable);
            nextButton.SetNavigationDown(activeButtons.LastOrDefault());
            activeButtons.SetNavigationHorizontal();
            activeButtons.SetNavigationUp(nextButton);
        }

        private void UpdateGraphics<TGraphic, TData>(List<TGraphic> graphics, List<TData> data, System.Action<TGraphic, TData> update) where TGraphic : Graphic
        {
            for (int i = 0; i < graphics.Count; i++)
            {
                if (i < data.Count)
                {
                    update(graphics[i], data[i]);
                    graphics[i].enabled = true;
                }
                else
                {
                    graphics[i].enabled = false;
                }
            }
        }

        public override void Refresh(CarouselData data)
        {
            base.Refresh(data);

            foreach (Button button in buttons)
            {
                Destroy(button.gameObject);
            }

            buttons.Clear();

            for (int i = 0; i < data.Items.Count; i++)
            {
                int index = i;
                Button button = Instantiate(jumpButton, jumpButton.transform.parent);
                button.onClick.AddListener(() => PageTo(index));
                button.interactable = i == 0;
                button.gameObject.SetActive(true);
                buttons.Add(button);
            }

            RefreshNow();
        }

        public void Refresh()
        {
            if (coroutine == null)
            {
                coroutine = StartCoroutine(RefreshAsync());
            }
        }

        public void PageLeft()
        {
            if (coroutine == null)
            {
                current = Mathf.Clamp(current - 1, 0, data.Items.Count - 1);
                Refresh();
            }
        }

        public void PageRight()
        {
            if (coroutine == null)
            {
                if (current == data.Items.Count - 1)
                {
                    Continue();
                }
                else
                {
                    current = Mathf.Clamp(current + 1, 0, data.Items.Count - 1);
                    Refresh();
                }
            }
        }

        public void PageTo(int index)
        {
            if (coroutine == null && current != index)
            {
                current = Mathf.Clamp(index, 0, data.Items.Count - 1);
                Refresh();
            }
        }

        public void PageToInstant(int index)
        {
            if (current != index)
            {
                current = Mathf.Clamp(index, 0, data.Items.Count - 1);
                RefreshNow();
            }
        }

        public void Continue()
        {
            onContinue?.Invoke(this);

            if (data != null)
            {
                data.OnContinue?.Invoke(this);
            }
        }
    }
}