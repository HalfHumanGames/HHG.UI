using HHG.Common.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HHG.UI.Runtime
{
    public class UISaveSlotsMenu : UI
    {
        private const int saveSlotCount = 10;

        [SerializeField] private SaveFileAssetBase saveFileAsset;
        [SerializeField] private UISaveSlot saveSlotTemplate;

        private List<UISaveSlot> saveSlots = new List<UISaveSlot>();

        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < saveSlotCount; i++)
            {
                UISaveSlot saveSlot = Instantiate(saveSlotTemplate, saveSlotTemplate.transform.parent);
                saveSlot.Refresh(saveFileAsset, i);
                saveSlots.Add(saveSlot);
            }

            saveSlotTemplate.transform.SetParent(null);
            Destroy(saveSlotTemplate.gameObject);

            saveSlots.Select(s => s.LoadSaveButton).SetNavigationVertical();
            saveSlots.Select(s => s.RenameSaveButton).SetNavigationVertical();
            saveSlots.Select(s => s.DeleteSaveButton).SetNavigationVertical();

            for (int i = 0; i < saveSlots.Count - 1; i++)
            {
                saveSlots[i].SetupNavigation(saveSlots[i + 1]);
            }
        }

        protected override void OnFocus()
        {
            base.OnFocus();

            saveSlots.FirstOrDefault().LoadSaveButton.Select();
        }
    }
}