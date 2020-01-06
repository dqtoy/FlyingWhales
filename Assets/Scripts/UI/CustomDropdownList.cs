using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CustomDropdownList : MonoBehaviour {
    [Header("Object Picker")]
    [SerializeField] private ScrollRect dropdownScrollView;
    [SerializeField] private GameObject dropdownListItemPrefab;
    private Action<string> onClickDropdownItem;

    public void ShowDropdown(List<string> items, Action<string> onClickDropdownItem) {
        Utilities.DestroyChildren(dropdownScrollView.content);

        this.onClickDropdownItem = onClickDropdownItem;

        for (int i = 0; i < items.Count; i++) {
            string item = items[i];
            GameObject ddListItem = UIManager.Instance.InstantiateUIObject(dropdownListItemPrefab.name, dropdownScrollView.content);
            CustomDDListItem ddItem = ddListItem.GetComponent<CustomDDListItem>();
            ddItem.SetText(item);
            ddItem.SetClickAction(OnClickDropdownItem);
        }
        gameObject.SetActive(true);
    }

    public void HideDropdown() {
        gameObject.SetActive(false);
    }

    public void OnClickDropdownItem(CustomDDListItem item) {
        onClickDropdownItem.Invoke(item.itemText.text);
    }
}