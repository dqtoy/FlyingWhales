using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCostItem : MonoBehaviour {

    public Resource resource;

    public System.Action<Resource> onDeleteResource;
    public System.Action onEditResource;

    [SerializeField] private Dropdown resourceTypeDropdown;
    [SerializeField] private InputField resourceAmountField;

    public void SetResource(Resource resource) {
        this.resource = resource;
        LoadResourceDropdown();
        resourceTypeDropdown.value = Utilities.GetOptionIndex(resourceTypeDropdown, resource.resource.ToString());
        resourceAmountField.text = resource.amount.ToString();
    }

    private void LoadResourceDropdown() {
        resourceTypeDropdown.ClearOptions();
        resourceTypeDropdown.AddOptions(Utilities.GetEnumChoices<RESOURCE>());
    }

    public void DeleteResource() {
        if (onDeleteResource != null) {
            onDeleteResource(resource);
        }
    }

    public void OnResourceEdited() {
        if (onEditResource != null) {
            onEditResource();
        }
    }

    public void OnResourceChanged(int choice) {
        string resourceString = resourceTypeDropdown.options[resourceTypeDropdown.value].text;
        RESOURCE resourceType = (RESOURCE)System.Enum.Parse(typeof(RESOURCE), resourceString);
        resource.resource = resourceType;
        OnResourceEdited();
    }
    public void OnResourceAmountChanged(string amount) {
        if (string.IsNullOrEmpty(amount)) {
            return;
        }
        resource.amount = System.Int32.Parse(amount);
        OnResourceEdited();
    }
}
