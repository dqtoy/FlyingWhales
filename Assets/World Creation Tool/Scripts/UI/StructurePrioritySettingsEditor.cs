using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructurePrioritySettingsEditor : MonoBehaviour {

    private StructurePrioritySetting currSetting;

    [SerializeField] private Dropdown actionTypeDropdown;
    [SerializeField] private Dropdown landmarkTypeDropdown;
    [SerializeField] private ScrollRect resourceCostScrollView;
    [SerializeField] private GameObject resourceItemPrefab;

    public void Initialize() {
        LoadDropdownOptions();
    }
    private void LoadDropdownOptions() {
        actionTypeDropdown.ClearOptions();
        actionTypeDropdown.AddOptions(Utilities.GetEnumChoices<ACTION_TYPE>());

        landmarkTypeDropdown.ClearOptions();
        landmarkTypeDropdown.AddOptions(Utilities.GetEnumChoices<LANDMARK_TYPE>());
    }

    public void ShowSettings(StructurePrioritySetting setting) {
        currSetting = setting;
        actionTypeDropdown.value = Utilities.GetOptionIndex(actionTypeDropdown, currSetting.actionType.ToString());
        landmarkTypeDropdown.value = Utilities.GetOptionIndex(landmarkTypeDropdown, currSetting.landmarkType.ToString());
        UpdateResourceCosts();
        this.gameObject.SetActive(true);
    }
    public void UpdateResourceCosts() {
        Utilities.DestroyChildren(resourceCostScrollView.content);
        for (int i = 0; i < currSetting.resourceCost.Count; i++) {
            Resource cost = currSetting.resourceCost[i];
            GameObject resourceGO = GameObject.Instantiate(resourceItemPrefab, resourceCostScrollView.content);
            ResourceCostItem item = resourceGO.GetComponent<ResourceCostItem>();
            item.SetResource(cost);
            item.onDeleteResource = DeleteResource;
        }
    }
    public void DeleteResource(Resource resource) {
        currSetting.RemoveResourceCost(resource);
        UpdateResourceCosts();
    }
    public void AddResource() {
        currSetting.AddResourceCost(new Resource(RESOURCE.FOOD, 0));
        UpdateResourceCosts();
    }
    public void OnActionChanged(int choice) {
        string actionString = actionTypeDropdown.options[actionTypeDropdown.value].text;
        ACTION_TYPE actionType = (ACTION_TYPE)System.Enum.Parse(typeof(ACTION_TYPE), actionString);
        currSetting.actionType = actionType;
    }
    public void OnLandmarkChanged(int choice) {
        string landmarkString = landmarkTypeDropdown.options[landmarkTypeDropdown.value].text;
        LANDMARK_TYPE landmarkType = (LANDMARK_TYPE)System.Enum.Parse(typeof(LANDMARK_TYPE), landmarkString);
        currSetting.landmarkType = landmarkType;
    }

    public void Hide() {
        this.gameObject.SetActive(false);
    }
}
