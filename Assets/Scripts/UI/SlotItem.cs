using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotItem : MonoBehaviour {

    public object placedObject { get; private set; }
    public System.Type neededType { get; private set; }

    public SlotItemDropEvent onItemDroppedValid;
    public ItemDroppedCallback itemDroppedCallback;
    public ItemDroppedOutCallback itemDroppedOutCallback;

    public Func<bool> otherValidation;

    [Space(10)]
    [Header("Slot Elements")]
    public CharacterPortrait portrait;
    public AreaEmblem areaEmblem;
    public FactionEmblem factionEmblem;
    public CustomDropZone dropZone;
    public SlotItemDraggable draggable;
    [SerializeField] private string neededTypeStr;

    public int slotIndex { get; private set; }
    private string hoverInfo;

    #region Drop Zone
    public void OnDropItemAtDropZone(GameObject go) { //this is used to filter if the dragged object is valid for this slot
        DragObject dragObj = go.GetComponent<DragObject>();
        if (dragObj == null) {
            return;
        }
        IDragParentItem parentItem = dragObj.parentItem;
        if (parentItem != null) {
            if (parentItem.associatedObj.GetType() == neededType || parentItem.associatedObj.GetType().BaseType == neededType) {
                SuccessfulDropZoneDrop(parentItem);
            } else {

                Messenger.Broadcast<string, bool>(Signals.SHOW_POPUP_MESSAGE, "This slot requires a " + GetTypeString(neededType), true);
            }
        }
    }
    private void SuccessfulDropZoneDrop(IDragParentItem parentItem) {
        if (onItemDroppedValid != null) {
            if (otherValidation == null || otherValidation()) {
                onItemDroppedValid.Invoke(parentItem);
            }
        }
    }
    #endregion

    #region Core Functions
    public void SetSlotIndex(int index) {
        slotIndex = index;
    }
    public void SetNeededType(System.Type neededType) {
        this.neededType = neededType;
        neededTypeStr = neededType.ToString();
    }
    public void OnDroppedItemValid(IDragParentItem item) {
        PlaceObject(item.associatedObj);
        if (itemDroppedCallback != null) {
            itemDroppedCallback.Invoke(item.associatedObj, slotIndex);
        }
    }
    public void PlaceObject(object associatedObj) {
        placedObject = associatedObj;
        if (associatedObj is FactionToken) {
            factionEmblem.gameObject.SetActive(true);
            areaEmblem.gameObject.SetActive(false);
            portrait.gameObject.SetActive(false);
            factionEmblem.SetFaction((associatedObj as FactionToken).faction);
            hoverInfo = (associatedObj as FactionToken).faction.name;
        } else if (associatedObj is LocationToken) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(true);
            portrait.gameObject.SetActive(false);
            hoverInfo = (associatedObj as LocationToken).location.name;
        } else if (associatedObj is CharacterToken) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(false);
            portrait.gameObject.SetActive(true);
            portrait.GeneratePortrait((associatedObj as CharacterToken).character);
            hoverInfo = (associatedObj as CharacterToken).character.name;
        } else if (associatedObj is Minion) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(false);
            portrait.gameObject.SetActive(true);
            portrait.GeneratePortrait((associatedObj as Minion).character);
            hoverInfo = (associatedObj as Minion).name;
        } else if (associatedObj is Character) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(false);
            portrait.gameObject.SetActive(true);
            portrait.GeneratePortrait((associatedObj as Character));
            hoverInfo = (associatedObj as Character).name;
        }
    }
    public void ClearSlot(bool keepType = false) {
        if (!keepType) {
            neededType = null;
        }
        placedObject = null;
        factionEmblem.gameObject.SetActive(false);
        areaEmblem.gameObject.SetActive(false);
        portrait.gameObject.SetActive(false);
    }
    #endregion

    #region Draggable Item
    public void OnItemDroppedOut() {
        if (itemDroppedOutCallback != null) {
            itemDroppedOutCallback.Invoke(placedObject, slotIndex);
        }
    }
    #endregion

    #region Utilities
    private string GetTypeString(System.Type type) {
        if (type == null) {
            return "null";
        }
        if (type == typeof(FactionToken)) {
            return "Faction";
        } else if (type == typeof(LocationToken)) {
            return "Location";
        } else if (type == typeof(CharacterToken)) {
            return "Character";
        } else if (type == typeof(Minion)) {
            return "Minion";
        } else if (type == typeof(Character)) {
            return "Character";
        } else {
            return type.ToString();
        }
    }
    public void HideVisuals() {
        portrait.gameObject.SetActive(false);
        factionEmblem.gameObject.SetActive(false);
        areaEmblem.gameObject.SetActive(false);
    }
    public void ShowObjectInfo() {
        if (placedObject != null) {
            UIManager.Instance.ShowSmallInfo(hoverInfo);
        }
    }
    public void HideObjectInfo() {
        if (placedObject != null) {
            UIManager.Instance.HideSmallInfo();
        }
    }
    #endregion
}

[System.Serializable]
public class SlotItemDropEvent : UnityEvent<IDragParentItem> { }

public class ItemDroppedCallback : UnityEvent<object, int> { }
public class ItemDroppedOutCallback : UnityEvent<object, int> { }
