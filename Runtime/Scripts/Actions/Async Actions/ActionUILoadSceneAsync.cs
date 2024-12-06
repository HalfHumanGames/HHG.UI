using HHG.Common.Runtime;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class ActionUILoadSceneAsync : IActionAsync
    {
        [SerializeField, Dropdown] private SceneNameAsset sceneName;
        [SerializeField, Dropdown] private LoadingScreenAsset loadingScreen;

        private Action onLoaded;
        private LoadingScreen data;
        private float timestamp;

        public ActionUILoadSceneAsync()
        {

        }

        public ActionUILoadSceneAsync(SceneNameAsset sceneName, LoadingScreenAsset loadingScreen = null, Action onLoaded = null)
        {
            this.sceneName = sceneName;
            this.loadingScreen = loadingScreen;
            this.onLoaded = onLoaded;
        }

        public IEnumerator InvokeAsync(MonoBehaviour invoker)
        {
            if (loadingScreen == null)
            {
                loadingScreen = Database.Get<LoadingScreenAsset>(ls => ls.IsDefault);
            }

            ActionLoadSceneAsync loadScene = new ActionLoadSceneAsync(sceneName, onLoaded);

            if (UI.TryGet(out UILoadingScreen ui))
            {
                // Cache data in case it is dynamic and changes later
                data = loadingScreen.Data;
                ui.Refresh(data);
                yield return ui.Open();
                timestamp = Time.time;
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneLoaded += OnSceneLoaded;
                yield return loadScene.InvokeAsync(invoker);
            }
            else
            {
                yield return loadScene.InvokeAsync(invoker);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            // Use CoroutineUtil since original invoker was
            // (likely) destroyed when the new scene loaded
            CoroutineUtil.StartCoroutine(InvokeAsyncPart2());
        }

        private IEnumerator InvokeAsyncPart2()
        {
            float elapsed = timestamp - Time.time;
            float wait = Mathf.Max(loadingScreen.Data.MinDuration - elapsed, 0f);
            
            if (UI.TryGet(out UILoadingScreen ui))
            {
                // Refresh in case not using persistent loading screen
                // Used cache data in case it is dynamic and has changed
                ui.Refresh(data);

                if (wait > 0f)
                {
                    yield return new WaitForSecondsRealtime(wait);
                }

                yield return ui.Close();
            }
        }
    }
}