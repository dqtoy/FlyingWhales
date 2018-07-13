using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructurePrioritySettingsEditor : MonoBehaviour {

    private StructurePrioritySetting currSetting;
    private StructurePriority parentPriority;

    [SerializeField] private Dropdown landmarkTypeDropdown;
    [SerializeField] private ScrollRect buildResourceCostScrollView;
    [SerializeField] private ScrollRect repairResourceCostScrollView;
    [SerializeField] private GameObject resourceItemPrefab;

    public void Initialize() {
        LoadDropdownOptions();
    }
    private void LoadDropdownOptions() {
        landmarkTypeDropdown.ClearOptions();
        landmarkTypeDropdown.AddOptions(Utilities.GetEnumChoices<LANDMARK_TYPE>());
    }

    public void ShowSettings(StructurePrioritySetting setting, StructurePriority parent) {
        currSetting = setting;
        parentPriority = parent;
        landmarkTypeDropdown.value = Utilities.GetOptionIndex(landmarkTypeDropdown, currSetting.landmarkType.ToString());
        UpdateBuildResourceCosts();
        UpdateRepairResourceCosts();
        this.gameObject.SetActive(true);
    }

    #region Build Costs
    public void UpdateBuildResourceCosts() {
        Utilities.DestroyChildren(buildResourceCostScrollView.content);
        for (int i = 0; i < currSetting.buildResourceCost.Count; i++) {
            Resource cost = currSetting.buildResourceCost[i];
            GameObject resourceGO = GameObject.Instantiate(resourceItemPrefab, buildResourceCostScrollView.content);
            ResourceCostItem item = resourceGO.GetComponent<ResourceCostItem>();
            item.SetResource(cost);
            item.onDeleteResource = DeleteBuildResource;
            item.onEditResource = () => worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.OnPriorityEdited(parentPriority);
        }
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.OnPriorityEdited(parentPriority);
    }
    public void DeleteBuildResource(Resource resource) {
        currSetting.RemoveBuildResourceCost(resource);
        UpdateBuildResourceCosts();
    }
    public void AddBuildResource() {
        currSetting.AddBuildResourceCost(new Resource(RESOURCE.FOOD, 0));
        UpdateBuildResourceCosts();
    }
    #endregion

    #region Repair Costs
    public void UpdateRepairResourceCosts() {
        Utilities.DestroyChildren(repairResourceCostScrollView.content);
        for (int i = 0; i < currSetting.repairResourceCost.Count; i++) {
            Resource cost = currSetting.repairResourceCost[i];
            GameObject resourceGO = GameObject.Instantiate(resourceItemPrefab, repairResourceCostScrollView.content);
            ResourceCostItem item = resourceGO.GetComponent<ResourceCostItem>();
            item.SetResource(cost);
            item.onDeleteResource = DeleteRepairResource;
            item.onEditResource = () => worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.OnPriorityEdited(parentPriority);
        }
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.OnPriorityEdited(parentPriority);
    }
    public void DeleteRepairResource(Resource resource) {
        currSetting.RemoveRepairResourceCost(resource);
        UpdateRepairResourceCosts();
    }
    public void AddRepairResource() {
        currSetting.AddRepairResourceCost(new Resource(RESOURCE.FOOD, 0));
        UpdateRepairResourceCosts();
    }
    #endregion

    public void OnLandmarkChanged(int choice) {
        string landmarkString = landmarkTypeDropdown.options[landmarkTypeDropdown.value].text;
        LANDMARK_TYPE landmarkType = (LANDMARK_TYPE)System.Enum.Parse(typeof(LANDMARK_TYPE), landmarkString);
        currSetting.landmarkType = landmarkType;
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.OnPriorityEdited(parentPriority);
    }

    public void Hide() {
        this.gameObject.SetActive(false);
    }
}
