using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HHG.UISystem.Runtime
{
    public class UILoadingScreen : UI<LoadingScreen>
    {
        [SerializeField] private List<Image> images = new List<Image>();
        [SerializeField] private List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();

        protected override void Awake()
        {
            // In case using a persistent loading screen
            // and each scene has one, destroy duplicates
            if (TryGet(out UILoadingScreen _))
            {
                Destroy(transform.root.gameObject);
                return;
            }

            base.Awake();
            DontDestroyOnLoad(transform.root.gameObject);
        }

        public override void Refresh(LoadingScreen modal)
        {
            for (int i = 0; i < images.Count; i++)
            {
                if (i < modal.Sprites.Count)
                {
                    images[i].sprite = modal.Sprites[i];
                    images[i].color = Color.white;
                    images[i].SetNativeSize();
                }
                else
                {
                    images[i].color = Color.clear;
                }
            }

            for (int i = 0; i < labels.Count; i++)
            {
                if (i < modal.Text.Count)
                {
                    labels[i].text = modal.Text[i];
                }
                else
                {
                    labels[i].text = string.Empty;
                }
            }
        }
    }
}