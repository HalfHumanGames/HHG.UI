using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HHG.UI.Runtime
{
    public class UILoadingScreen : UI<LoadingScreen>
    {
        [SerializeField] private List<Image> images = new List<Image>();
        [SerializeField] private List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();

        public override void Refresh(LoadingScreen data)
        {
            base.Refresh(data);

            for (int i = 0; i < images.Count; i++)
            {
                if (i < data.Sprites.Count)
                {
                    images[i].sprite = data.Sprites[i];
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
                if (i < data.Text.Count)
                {
                    labels[i].text = data.Text[i];
                }
                else
                {
                    labels[i].text = string.Empty;
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