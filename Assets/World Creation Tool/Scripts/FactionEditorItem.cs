using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace worldcreator {
    public class FactionEditorItem : MonoBehaviour {

        [SerializeField] private InputField factionNameField;
        [SerializeField] private Text factionRegionsLbl;
        [SerializeField] private Dropdown raceDropdown;
        [SerializeField] private Button deleteBtn;
        [SerializeField] private Button assignBtn;
        [SerializeField] private Button unassignBtn;

        private Faction _faction;

        public void SetFaction(Faction faction) {
            _faction = faction;
            LoadRacesDropdown();
            UpdateInfo();
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

        private int GetRaceIndex(RACE race) {
            for (int i = 0; i < raceDropdown.options.Count; i++) {
                string currOption = raceDropdown.options[i].text;
                if (currOption.Equals(race.ToString())) {
                    return i;
                }
            }
            throw new System.Exception("There is no " + race.ToString() + " choice in dropdown!");
        }
        private RACE GetRaceFromIndex(int raceIndex) {
            string currOption = raceDropdown.options[raceIndex].text;
            return (RACE)System.Enum.Parse(typeof(RACE), currOption);
        }
        public void UpdateInfo() {
            factionNameField.text = _faction.name;
            factionRegionsLbl.text = _faction.ownedRegions.Count.ToString();
            raceDropdown.value = GetRaceIndex(_faction.race);
        }
        public void AssignFaction() {
            for (int i = 0; i < WorldCreatorManager.Instance.selectionComponent.selectedRegions.Count; i++) {
                Region currRegion = WorldCreatorManager.Instance.selectionComponent.selectedRegions[i];
                currRegion.SetOwner(_faction);
                _faction.OwnRegion(currRegion);
                currRegion.ReColorBorderTiles(_faction.factionColor);
            }
            WorldCreatorUI.Instance.editFactionsMenu.UpdateItems();
        }
        public void DeleteFaction() {
            WorldCreatorManager.Instance.DeleteFaction(_faction);
        }
        public void UnassignFaction() {
            for (int i = 0; i < WorldCreatorManager.Instance.selectionComponent.selectedRegions.Count; i++) {
                Region currRegion = WorldCreatorManager.Instance.selectionComponent.selectedRegions[i];
                if (currRegion.owner != null && currRegion.owner.id == _faction.id) {
                    currRegion.SetOwner(null);
                }
            }
            WorldCreatorUI.Instance.editFactionsMenu.UpdateItems();
        }

        private void Update() {
            if (WorldCreatorManager.Instance.selectionComponent.selection.Count > 0) {
                assignBtn.interactable = true;
                unassignBtn.interactable = true;
            } else {
                assignBtn.interactable = false;
                unassignBtn.interactable = false;
            }
        }

        #region Info Editing
        public void OnRaceChange(int raceIndex) {
            RACE newRace = GetRaceFromIndex(raceIndex);
            _faction.SetRace(newRace);
            UpdateInfo();
        }
        public void OnChangeFactionName() {
            string newName = factionNameField.text;
            _faction.SetName(newName);
        }
        #endregion

    }
}