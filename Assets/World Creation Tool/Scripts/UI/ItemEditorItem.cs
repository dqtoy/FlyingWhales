using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;

public class ItemEditorItem : MonoBehaviour {

    public Item item { get; private set; }

    [SerializeField] private Text equipmentNameLbl;
    [SerializeField] private Button deleteBtn;

    public void SetItem(Item item, Character character) {
        this.item = item;
        equipmentNameLbl.text = item.itemName;
    }

    public void SetDeleteItemAction(UnityAction deleteAction) {
        deleteBtn.onClick.RemoveAllListeners();
        deleteBtn.onClick.AddListener(deleteAction);
    }

    //public void DeleteItem() {
    //    character.UnequipItem(item);
    //}


}
