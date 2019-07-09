using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace worldcreator {
    public class EditRegionsMenu : MonoBehaviour {
        [SerializeField] private GameObject regionItemPrefab;
        [SerializeField] private ScrollRect regionsScrollView;
        [SerializeField] private Button addRegionButton;

        private Dictionary<Region, RegionEditorItem> items = new Dictionary<Region, RegionEditorItem>();

        public void OnRegionCreated(Region newRegion) {
            GameObject newRegionItemGO = GameObject.Instantiate(regionItemPrefab, regionsScrollView.content.transform);
            RegionEditorItem newRegionItem = newRegionItemGO.GetComponent<RegionEditorItem>();
            newRegionItem.SetRegion(newRegion);
            items.Add(newRegion, newRegionItem);
            UpdateItems();

            if (WorldCreatorManager.Instance.allRegions.Count == 1) {
                items[WorldCreatorManager.Instance.allRegions[0]].SetDeleteButtonState(false);
            } else {
                foreach (KeyValuePair<Region, RegionEditorItem> kvp in items) {
                    RegionEditorItem currItemGO = kvp.Value;
                    currItemGO.SetDeleteButtonState(true);
                }
            }
        }
        public void OnRegionEdited() {
            UpdateItems();
        }
        public void OnRegionDeleted(Region deletedRegion) {
            RegionEditorItem item = GetRegionItem(deletedRegion);
            if (item != null) {
                GameObject.Destroy(item.gameObject);
                items.Remove(deletedRegion);
            }
            UpdateItems();

            if (WorldCreatorManager.Instance.allRegions.Count == 1) {
                Region region = WorldCreatorManager.Instance.allRegions[0];
                if (items.ContainsKey(region)) {
                    items[region].SetDeleteButtonState(false);
                }
            } else {
                foreach (KeyValuePair<Region, RegionEditorItem> kvp in items) {
                    RegionEditorItem currItemGO = kvp.Value;
                    currItemGO.SetDeleteButtonState(true);
                }
            }
        }

        public void CreateNewRegion() {
            List<Region> affectedRegions = new List<Region>();
            WorldCreatorManager.Instance.CreateNewRegion(WorldCreatorManager.Instance.selectionComponent.selection, ref affectedRegions);
            WorldCreatorManager.Instance.ValidateRegions(affectedRegions);
        }

        private void UpdateItems() {
            foreach (KeyValuePair<Region, RegionEditorItem> kvp in items) {
                RegionEditorItem currItemGO = kvp.Value;
                currItemGO.UpdateRegionInfo();
            }
        }

        //public void OnRegionSelected(Region region) {
        //    if (selectedRegion != null && selectedRegion.id != region.id) {
        //        selectedRegion.UnhighlightRegion();
        //    }
        //    selectedRegion = region;
        //}

        #region Utilities
        private RegionEditorItem GetRegionItem(Region region) {
            if (items.ContainsKey(region)) {
                return items[region];
            }
            return null;
        }
        #endregion

        #region Monobehaviours
        private void Update() {
            if (WorldCreatorManager.Instance.selectionComponent.selection.Count > 0) {
                addRegionButton.interactable = true;
            } else {
                addRegionButton.interactable = false;
            }
        }
        #endregion
    }
}

