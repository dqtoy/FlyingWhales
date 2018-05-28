using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace worldcreator {
    public class FactionEditorItem : MonoBehaviour {

        [SerializeField] private Text factionNameLbl;
        [SerializeField] private Text factionRegionsLbl;
        [SerializeField] private Button deleteBtn;
        [SerializeField] private Button assignBtn;

        private Faction _faction;

        public void SetFaction(Faction faction) {
            _faction = faction;
            UpdateInfo();
        }

        public void UpdateInfo() {
            factionNameLbl.text = _faction.name;
            factionRegionsLbl.text = _faction.ownedRegions.Count.ToString();
        }

        public void AssignFaction() {
            for (int i = 0; i < WorldCreatorManager.Instance.selectionComponent.selectedRegions.Count; i++) {
                Region currRegion = WorldCreatorManager.Instance.selectionComponent.selectedRegions[i];
                currRegion.SetOwner(_faction);
                _faction.OwnRegion(currRegion);
                currRegion.ReColorBorderTiles(_faction.factionColor);
            }
            WorldCreatorUI.Instance.editFactionsMenu.OnAssignRegion();
        }

        public void DeleteFaction() {
            WorldCreatorManager.Instance.DeleteFaction(_faction);
        }

        private void Update() {
            if (WorldCreatorManager.Instance.selectionComponent.selection.Count > 0) {
                assignBtn.interactable = true;
            } else {
                assignBtn.interactable = false;
            }
        }
    }
}