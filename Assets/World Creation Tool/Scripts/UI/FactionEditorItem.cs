using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace worldcreator {
    public class FactionEditorItem : MonoBehaviour {

        [SerializeField] private Text factionNameLbl;
        //[SerializeField] private Text factionRegionsLbl;
        //[SerializeField] private Text factionCharactersLbl;
        [SerializeField] private Button deleteBtn;
        //[SerializeField] private Button assignBtn;
        //[SerializeField] private Button unassignBtn;

        private Faction _faction;

        public void SetFaction(Faction faction) {
            _faction = faction;
            UpdateInfo();
        }
        public void UpdateInfo() {
            factionNameLbl.text = _faction.name;
            //factionRegionsLbl.text = _faction.ownedRegions.Count.ToString();
            //factionCharactersLbl.text = _faction.characters.Count.ToString();
        }
        //public void AssignFaction() {
        //    for (int i = 0; i < WorldCreatorManager.Instance.selectionComponent.selectedRegions.Count; i++) {
        //        Region currRegion = WorldCreatorManager.Instance.selectionComponent.selectedRegions[i];
        //        currRegion.SetOwner(_faction);
        //        _faction.OwnRegion(currRegion);
        //        currRegion.ReColorBorderTiles(_faction.factionColor);
        //    }
        //    WorldCreatorUI.Instance.editFactionsMenu.UpdateItems();
        //}
        public void DeleteFaction() {
            WorldCreatorManager.Instance.DeleteFaction(_faction);
        }
        //public void UnassignFaction() {
        //    for (int i = 0; i < WorldCreatorManager.Instance.selectionComponent.selectedRegions.Count; i++) {
        //        Region currRegion = WorldCreatorManager.Instance.selectionComponent.selectedRegions[i];
        //        if (currRegion.owner != null && currRegion.owner.id == _faction.id) {
        //            currRegion.SetOwner(null);
        //        }
        //    }
        //    WorldCreatorUI.Instance.editFactionsMenu.UpdateItems();
        //}

        private void Update() {
            UpdateInfo();
        }

        #region Info Editing
        //public void OnChangeFactionName() {
        //    string newName = factionNameLbl.text;
        //    _faction.SetName(newName);
        //}
        public void ShowFactionInfoEditor() {
            WorldCreatorUI.Instance.editFactionsMenu.ShowFactionInfo(_faction);
        }
        #endregion

    }
}