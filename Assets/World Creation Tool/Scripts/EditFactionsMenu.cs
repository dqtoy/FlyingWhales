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
        [SerializeField] private Dropdown raceDropdown;

        private Dictionary<Faction, FactionEditorItem> items = new Dictionary<Faction, FactionEditorItem>();

        private void Awake() {
            LoadRacesDropdown();
        }

        public void OnFactionCreated(Faction newFaction) {
            GameObject newFactionItemGO = GameObject.Instantiate(factionItemPrefab, factionsScrollView.content.transform);
            FactionEditorItem newFactionItem = newFactionItemGO.GetComponent<FactionEditorItem>();
            newFactionItem.SetFaction(newFaction);
            items.Add(newFaction, newFactionItem);
            UpdateItems();
        }
        public void OnFactionDeleted(Faction deletedFaction) {
            FactionEditorItem item = GetFactionItem(deletedFaction);
            GameObject.Destroy(item.gameObject);
            items.Remove(deletedFaction);
            UpdateItems();
        }

        public void CreateNewFaction() {
            RACE chosenRace = (RACE)Enum.Parse(typeof(RACE), raceDropdown.options[raceDropdown.value].text);
            WorldCreatorManager.Instance.CreateNewFaction(chosenRace);
        }

        public void UpdateItems() {
            foreach (KeyValuePair<Faction, FactionEditorItem> kvp in items) {
                FactionEditorItem currItemGO = kvp.Value;
                currItemGO.UpdateInfo();
            }
        }

        public void OnAssignRegion() {
            UpdateItems();
        }
        public void OnRegionDeleted(Region deletedRegion) {
            UpdateItems();
        }

        #region Utilities
        private FactionEditorItem GetFactionItem(Faction faction) {
            return items[faction];
        }
        private void LoadRacesDropdown() {
            RACE[] races = Utilities.GetEnumValues<RACE>();
            List<string> options = new List<string>();
            for (int i = 0; i < races.Length; i++) {
                RACE currRace = races[i];
                if (currRace != RACE.NONE) {
                    options.Add(currRace.ToString());
                }
            }
            raceDropdown.AddOptions(options);
        }
        #endregion
    }
}