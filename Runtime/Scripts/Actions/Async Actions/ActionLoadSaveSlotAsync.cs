//using HHG.Common.Runtime;
//using System.Collections;
//using UnityEngine;

//namespace HHG.UI.Runtime
//{
//    [System.Serializable]
//    public class ActionLoadSaveSlotAsync : IActionAsync
//    {
//        [SerializeField] private Session session;

//        private const string worldMapScene = "World Map";

//        public IEnumerator InvokeAsync(MonoBehaviour invoker)
//        {
//            yield break;

//            //if (session.tempFileExists())
//            //{
//            //    yield return new ActionLoadLevelAsync().InvokeAsync(invoker);
//            //}
//            //else
//            //{
//            //    SceneNameAsset sceneName = Database.Get<SceneNameAsset>(worldMapScene);
//            //    LoadingScreenAsset loadingScreen = Database.Get<LoadingScreenAsset>(ls => !ls.IsDefault);
//            //    yield return new ActionUILoadSceneAsync(sceneName, loadingScreen, OnSceneLoaded).InvokeAsync(invoker);
//            //}
//        }

//        private void OnSceneLoaded()
//        {
//            //Message.Publish(CmdGameLoad.Instance);
//        }
//    }
//}