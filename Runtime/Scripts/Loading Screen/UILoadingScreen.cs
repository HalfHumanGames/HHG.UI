using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HHG.UI.Runtime
{
    public class UILoadingScreen : UI<LoadingScreenData>
    {
        [SerializeField] private List<Image> images = new List<Image>();
        [SerializeField] private List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();

        public override void Refresh(LoadingScreenData data)
        {
            base.Refresh(data);

            for (int i = 0; i < images.Count; i++)
            {
                if (i < data.Sprites.Count)
                {
                    images[i].enabled = true;
                    images[i].sprite = data.Sprites[i];
                    images[i].SetNativeSize();
                }
                else
                {
                    images[i].enabled = false;
                }
            }

            for (int i = 0; i < labels.Count; i++)
            {
                if (i < data.Texts.Count)
                {
                    labels[i].enabled = true;
                    labels[i].text = data.Texts[i];
                }
                else
                {
                    labels[i].enabled = false;
                }
            }
        }
    }

    public static class UILoadingScreenExtensions
    {
        public static void LoadScene(this MonoBehaviour mono, string sceneName)
        {
            mono.StartCoroutine(LoadSceneAsync(sceneName));
        }

        public static IEnumerator LoadSceneAsync(string sceneName)
        {
            yield return UI.Push<UILoadingScreen>();
            yield return SceneManager.LoadSceneAsync(sceneName);
        }
    }
}