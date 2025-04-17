using HHG.Common.Runtime;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HHG.UI.Runtime
{
    public class UISaveSlot : MonoBehaviour
    {
        private const string saveFileNameFormat = "Save Slot {0}";

        public Button LoadSaveButton => loadSaveButton;
        public Button RenameSaveButton => renameSaveButton;
        public Button DeleteSaveButton => deleteSaveButton;

        [SerializeField] private GameObject saveContainer;
        [SerializeField] private GameObject emptySaveContainer;
        [SerializeField] private TMP_Text saveNameLabel;
        [SerializeField] private TMP_Text saveDateLabel;
        [SerializeField] private TMP_Text saveInfoLabel;
        [SerializeField] private Button loadSaveButton;
        [SerializeField] private Button renameSaveButton;
        [SerializeField] private Button deleteSaveButton;
        [SerializeField] private Button createSaveButton;

        private SaveFileAssetBase saveFileAsset;
        private int saveFileIndex;
        private string saveFileName;
        private bool saveFileExists;

        public void Refresh()
        {
            Refresh(saveFileAsset, saveFileIndex);
        }

        public void Refresh(SaveFileAssetBase newSaveFile, int newSaveFileIndex)
        {
            saveFileAsset = newSaveFile;
            saveFileIndex = newSaveFileIndex;
            saveFileName = string.Format(saveFileNameFormat, saveFileIndex);
            saveFileExists = saveFileAsset.Exists(saveFileName);
            gameObject.name = saveFileName;

            string saveName = string.Empty;
            string saveDate = string.Empty;
            string saveInfo = string.Empty;

            if (saveFileExists)
            {
                saveFileAsset.Load(saveFileName);

                saveName = saveFileAsset.DisplayName;
                saveDate = saveFileAsset.LastSavedFormatted;
                saveInfo = saveFileAsset.ProgressInfo;

                Enumerable.Empty<Selectable>().
                    Append(loadSaveButton).
                    Append(renameSaveButton).
                    Append(deleteSaveButton).
                    SetNavigationHorizontal();
            }

            saveNameLabel.text = saveName;
            saveDateLabel.text = saveDate;
            saveInfoLabel.text = saveInfo;

            loadSaveButton.onClick.RemoveListener(OnClickLoadSave);
            loadSaveButton.onClick.AddListener(OnClickLoadSave);
            renameSaveButton.onClick.RemoveListener(OnClickRenameSave);
            renameSaveButton.onClick.AddListener(OnClickRenameSave);
            deleteSaveButton.onClick.RemoveListener(OnClickDeleteSave);
            deleteSaveButton.onClick.AddListener(OnClickDeleteSave);
            createSaveButton.onClick.RemoveListener(OnClickCreateSave);
            createSaveButton.onClick.AddListener(OnClickCreateSave);

            saveContainer.gameObject.SetActive(saveFileExists);
            emptySaveContainer.gameObject.SetActive(!saveFileExists);
        }

        public void SetupNavigation(UISaveSlot other)
        {
            if (saveFileExists && other.saveFileExists)
            {
                SetupNavigationHelper(loadSaveButton, other.loadSaveButton);
                SetupNavigationHelper(renameSaveButton, other.renameSaveButton);
                SetupNavigationHelper(deleteSaveButton, other.deleteSaveButton);
            }
            else if (saveFileExists && !other.saveFileExists)
            {
                SetupNavigationHelper(renameSaveButton, other.createSaveButton);
                SetupNavigationHelper(deleteSaveButton, other.createSaveButton);
                // Do last so create save button goes to load save button
                SetupNavigationHelper(loadSaveButton, other.createSaveButton);
            }
            else if (!saveFileExists && other.saveFileExists)
            {
                SetupNavigationHelper(createSaveButton, other.renameSaveButton);
                SetupNavigationHelper(createSaveButton, other.deleteSaveButton);
                // Do last so create save button goes to load save button
                SetupNavigationHelper(createSaveButton, other.loadSaveButton);
            }
            else if (!saveFileExists && !other.saveFileExists)
            {
                SetupNavigationHelper(createSaveButton, other.createSaveButton);
            }
        }

        private void SetupNavigationHelper(Selectable top, Selectable bottom)
        {
            top.SetNavigationDown(bottom);
            bottom.SetNavigationUp(top);
        }

        private void OnClickLoadSave()
        {
            saveFileAsset.Load(saveFileName);
            SaveFileScene saveFileScene = saveFileAsset.SaveFileScene;
            SerializedScene scene = ConfigBase.Scenes.GetScene(saveFileScene);
            this.LoadScene(scene.SceneName);
        }

        private void OnClickRenameSave()
        {
            saveFileAsset.Load(saveFileName);

            UI.Push(new InputFieldModalData(

                "Enter New Name",
                string.Empty,
                saveFileAsset.DisplayName,
                true,
                new()
                {
                    new ("Confirm", (button, ui) =>
                    {
                        saveFileAsset.Load(saveFileName);
                        saveFileAsset.DisplayName = (ui as UIInputFieldModal).InputFieldText;
                        saveFileAsset.Save();
                        Refresh();
                        UI.Pop();
                    }),
                    new ("Cancel", () =>
                    {
                        UI.Pop();
                    }),
                }
            ));
        }

        private void OnClickDeleteSave()
        {
            UI.Push(new ModalData(
                "Confirm Delete",
                "Are you sure you want to delete this save? All progress will be lost.",
                true,
                new()
                {
                    new("Yes", () =>
                    {
                        saveFileAsset.Delete(saveFileName);
                        Refresh();
                        UI.Pop();
                    }) ,
                    new("No", () =>
                    {
                        UI.Pop();
                    })
                }
            ));
        }

        private void OnClickCreateSave()
        {

            UI.Push("Vertical", new ModalData
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
                        saveFileAsset.Load(saveFileName);
                        saveFileAsset.DisplayName = difficulty.DisplayName;
                        saveFileAsset.DifficultyIndex = (ui as UIModal).Selection;
                        saveFileAsset.Save();
                        Refresh();
                        UI.Pop();
                    }
                ))
            ));
        }
    }
}