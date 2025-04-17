using HHG.Common.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace HHG.UI.Runtime
{
    public class UIMainMenu : UI
    {
        private string saveFile0 = "Save Slot 0";

        [Header("Main")]
        [SerializeField] private SaveFileAssetBase saveFileAsset;

        [Header("Menus")]
        [SerializeField] private UI saveSlotsMenu;
        [SerializeField] private UI optionsMenu;
        [SerializeField] private UI creditsMenu;

        [Header("Buttons")]
        [SerializeField] private UIButton playButton;
        [SerializeField] private UIButton continueButton;
        [SerializeField] private UIButton saveSlotsButton;
        [SerializeField] private UIButton optionsButton;
        [SerializeField] private UIButton creditsButton;
        [SerializeField] private UIButton exitButton;

        protected override void Start()
        {
            base.Start();

            playButton.OnClick.AddListener(OnClickPlay);
            continueButton.OnClick.AddListener(OnClickContinue);
            saveSlotsButton.OnClick.AddListener(OnClickSaveSlots);
            optionsButton.OnClick.AddListener(OnClickOptions);
            creditsButton.OnClick.AddListener(OnClickCredits);
            exitButton.OnClick.AddListener(OnClickExit); 

            Refresh();
        }

        protected override void OnFocus()
        {
            base.OnFocus();

            Refresh();
        }

        private void Refresh()
        {
            bool exists = saveFileAsset.AnyExists();
            playButton.gameObject.SetActive(!exists);
            continueButton.gameObject.SetActive(exists);
            saveSlotsButton.gameObject.SetActive(exists);

            using (ListPool<Button>.Get(out List<Button> buttons))
            {
                GetComponentsInChildren(true, buttons);

                var activeButtons = buttons.Where(b => b.isActiveAndEnabled && b.interactable);
                activeButtons.SetNavigationVertical();

                if (activeButtons.Any())
                {
                    select = activeButtons.FirstOrDefault();
                }
            }
        }

        private void OnClickPlay()
        {
            saveFileAsset.Delete(saveFile0);

            Push("Vertical", new ModalData
            (
                "Select Difficulty",
                string.Empty,
                true,
                ConfigBase.Difficulty.Difficulties.Select(difficulty => new ButtonData
                (
                    difficulty.DisplayName,
                    difficulty.Description,
                    (button, ui) =>
                    {
                        saveFileAsset.Load(saveFile0);
                        saveFileAsset.DisplayName = difficulty.DisplayName;
                        saveFileAsset.DifficultyIndex = (ui as UIModal).Selection;
                        saveFileAsset.Save();
                        this.LoadScene(ConfigBase.Scenes.NewGameScene.SceneName);
                    }
                ))
            ));
        }

        private void OnClickContinue()
        {
            saveFileAsset.LoadLastSaved();
            this.LoadScene(ConfigBase.Scenes.GetScene(saveFileAsset.SaveFileScene).SceneName);
        }

        private void OnClickSaveSlots()
        {
            saveSlotsMenu.Push();
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
            ApplicationUtil.Quit();
        }
    }
}