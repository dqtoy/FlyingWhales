using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace worldcreator {
    public class RegionEditorItem : MonoBehaviour {

        [SerializeField] private Text regionLbl;
        [SerializeField] private Text tilesLbl;
        [SerializeField] private Button deleteBtn;

        #region getters/setters
        public Region region { get; private set; }
        #endregion

        public void SetRegion(Region region) {
            this.region = region;
            UpdateRegionInfo();
        }
        public void UpdateRegionInfo() {
            regionLbl.text = region.name;
            tilesLbl.text = region.tilesInRegion.Count.ToString();
        }
        public void SetDeleteButtonState(bool state) {
            deleteBtn.interactable = state;
        }

        #region Mouse Functions
        public void OnPointerEnter() {
            region.HighlightRegion(Color.gray, 128f/255f);
        }
        public void OnPointerExit() {
            region.UnhighlightRegion();
        }
        //public void OnItemClicked() {
        //    WorldCreatorUI.Instance.editRegionsMenu.OnRegionSelected(region);
        //}
        public void DeleteRegion() {
            WorldCreatorManager.Instance.DeleteRegion(region);
        }
        #endregion
    }
}

