using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace worldcreator {
    public class EditFactionsMenu : MonoBehaviour {
        [SerializeField] private GameObject factionItemPrefab;
        [SerializeField] private ScrollRect factionsScrollView;

        [SerializeField] private FactionInfoEditor factionInfoEditor;

        private Dictionary<Faction, FactionEditorItem> items = new Dictionary<Faction, FactionEditorItem>();

        public void OnFactionCreated(Faction newFaction) {
            GameObject newFactionItemGO = GameObject.Instantiate(factionItemPrefab, factionsScrollView.content.transform);
            FactionEditorItem newFactionItem = newFactionItemGO.GetComponent<FactionEditorItem>();
            newFactionItem.SetFaction(newFaction);
            items.Add(newFaction, newFactionItem);
            UpdateItems();
            factionInfoEditor.OnNewFactionCreated(newFaction);
        }
        public void OnFactionDeleted(Faction deletedFaction) {
            FactionEditorItem item = GetFactionItem(deletedFaction);
            GameObject.Destroy(item.gameObject);
            items.Remove(deletedFaction);
            UpdateItems();
            factionInfoEditor.OnFactionDeleted(deletedFaction);
        }

        public void CreateNewFaction() {
            WorldCreatorManager.Instance.CreateNewFaction();
        }

        public void UpdateItems() {
            foreach (KeyValuePair<Faction, FactionEditorItem> kvp in items) {
                FactionEditorItem currItemGO = kvp.Value;
                currItemGO.UpdateInfo();
            }
        }
        public void ShowFactionInfo(Faction faction) {
            factionInfoEditor.ShowFactionInfo(faction);
        }
        public void HideFactionInfo() {
            factionInfoEditor.CloseMenu();
        }

        #region Utilities
        private FactionEditorItem GetFactionItem(Faction faction) {
            return items[faction];
        }
        #endregion
    }
}