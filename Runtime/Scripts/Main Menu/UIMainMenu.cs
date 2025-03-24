using HHG.Common.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HHG.UI.Runtime
{
    public class UIMainMenu : UI
    {
        private const string saveFile1 = "Save Slot 1";
        private const string saveFile2 = "Save Slot 2";
        private const string saveFile3 = "Save Slot 3";

        private static readonly string[] saveFileNames = new string[] { saveFile1, saveFile2, saveFile3 };

        [Header("Main")]
        [SerializeField] private SaveFileAssetBase saveFileAsset;

        [Header("Menus")]
        [SerializeField] private UI saveSlotsMenu;
        [SerializeField] private UI optionsMenu;
        [SerializeField] private UI creditsMenu;

        [Header("Buttons")]
        [SerializeField] private UIButton playButton;
        [SerializeField] private UIButton continueButton;
        [SerializeField] private UIButton loadGameButton;
        [SerializeField] private UIButton newGameButton;
        [SerializeField] private UIButton optionsButton;
        [SerializeField] private UIButton creditsButton;
        [SerializeField] private UIButton exitButton;

        private List<Button> buttons = new List<Button>();
        private IEnumerable<Button> activeButtons = Enumerable.Empty<Button>();

        protected override void Start()
        {
            base.Start();

            playButton.OnClick.AddListener(OnClickPlay);
            continueButton.OnClick.AddListener(OnClickContinue);
            loadGameButton.OnClick.AddListener(OnClickLoadGame);
            newGameButton.OnClick.AddListener(OnClickNewGame);
            optionsButton.OnClick.AddListener(OnClickOptions);
            creditsButton.OnClick.AddListener(OnClickCredits);
            exitButton.OnClick.AddListener(OnClickExit);

            bool exists = saveFileAsset.AnyExists(saveFileNames);
            playButton.gameObject.SetActive(!exists);
            continueButton.gameObject.SetActive(exists);
            loadGameButton.gameObject.SetActive(exists);
            newGameButton.gameObject.SetActive(exists);

            GetComponentsInChildren(true, buttons);
            activeButtons = buttons.Where(b => b.isActiveAndEnabled && b.interactable);
            activeButtons.SetNavigationVertical();

            if (activeButtons.Any())
            {
                select = activeButtons.FirstOrDefault();
            }
        }

        private void OnClickPlay()
        {
            saveFileAsset.Delete(saveFile1);
            saveFileAsset.Load(saveFile1);
            this.LoadScene(ConfigBase.Scenes.NewGameScene.SceneName);
        }

        private void OnClickContinue()
        {
            saveFileAsset.LoadLastSaved(saveFileNames);
            this.LoadScene(ConfigBase.Scenes.GetScene(saveFileAsset.SaveFileScene).SceneName);
        }

        private void OnClickLoadGame()
        {

        }

        private void OnClickNewGame()
        {

        }

        private void OnClickOptions()
        {
            optionsMenu.Push();
        }

        private void OnClickCredits()
        {
            creditsMenu.Push();
        }

        private void OnClickExit()
        {
            Application.Quit();

#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
        }
    }
}