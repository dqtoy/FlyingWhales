using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;

public class ItemEditorItem : MonoBehaviour {

    private Character character;
    private Item _item;

    [SerializeField] private Text equipmentNameLbl;
    [SerializeField] private Button deleteBtn;

    #region gettters/setters
    public Item item {
        get { return _item; }
    }
    #endregion

    public void SetItem(Item item, Character character) {
        this.character = character;
        _item = item;
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
