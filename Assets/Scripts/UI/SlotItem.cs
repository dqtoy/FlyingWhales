using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SlotItem : MonoBehaviour {

    public object placedObject { get; private set; }
    public System.Type neededType { get; private set; }

    public SlotItemDropEvent onItemDropped;
    public ItemDroppedCallback itemDroppedCallback;
    public ItemDroppedOutCallback itemDroppedOutCallback;

    public CharacterPortrait portrait;
    public AreaEmblem areaEmblem;
    public FactionEmblem factionEmblem;
    public CustomDropZone dropZone;

    public int slotIndex { get; private set; }

    public void SetNeededType(System.Type neededType) {
        this.neededType = neededType;
    }
    public void SetSlotIndex(int index) {
        slotIndex = index;
    }

    public void OnDropItemAtDropZone(Transform trans) { //this is used to filter if the dragged object is valid for this slot
        IDragParentItem parentItem = trans.gameObject.GetComponent<IDragParentItem>();
        if (parentItem != null) {
            if (neededType == typeof(IUnit)) {
                if (parentItem.associatedObj is IUnit) { //TODO: Make this more elegant!
                    SuccessfulDropZoneDrop(parentItem);
                } else {
                    //dragged object is not of needed type
                    //Debug.Log("Dragged invalid object");
                    Messenger.Broadcast<string, bool>(Signals.SHOW_POPUP_MESSAGE, "This slot requires a " + GetTypeString(neededType), true);
                }
            } else {
                if (parentItem.associatedObj.GetType() == neededType || parentItem.associatedObj.GetType().BaseType == neededType) {
                    SuccessfulDropZoneDrop(parentItem);
                } else {
                    //dragged object is not of needed type
                    //Debug.Log("Dragged invalid object");
                    Messenger.Broadcast<string, bool>(Signals.SHOW_POPUP_MESSAGE, "This slot requires a " + GetTypeString(neededType), true);
                }
            }
        }
    }
    private void SuccessfulDropZoneDrop(IDragParentItem parentItem) {
        if (onItemDropped != null) {
            onItemDropped.Invoke(parentItem);
        }
    }
    public void OnDropItemSlotItem(IDragParentItem item) {
        PlaceObject(item.associatedObj);
        if (itemDroppedCallback != null) {
            itemDroppedCallback.Invoke(item.associatedObj, slotIndex);
        }
    }
    public void PlaceObject(object associatedObj) {
        placedObject = associatedObj;
        if (associatedObj is FactionIntel) {
            factionEmblem.gameObject.SetActive(true);
            areaEmblem.gameObject.SetActive(false);
            portrait.gameObject.SetActive(false);
            factionEmblem.SetFaction((associatedObj as FactionIntel).faction);
        } else if (associatedObj is LocationIntel) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(true);
            portrait.gameObject.SetActive(false);
        } else if (associatedObj is CharacterIntel) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(false);
            portrait.gameObject.SetActive(true);
            portrait.GeneratePortrait((associatedObj as CharacterIntel).character, 95, true);
        } else if (associatedObj is Minion) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(false);
            portrait.gameObject.SetActive(true);
            portrait.GeneratePortrait((associatedObj as Minion).icharacter, 95, true);
        } else if (associatedObj is ICharacter) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(false);
            portrait.gameObject.SetActive(true);
            portrait.GeneratePortrait((associatedObj as ICharacter), 95, true);
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
    private string GetTypeString(System.Type type) {
        if (type == null) {
            return "null";
        }
        if (type == typeof(FactionIntel)) {
            return "Faction";
        } else if (type == typeof(LocationIntel)) {
            return "Location";
        } else if (type == typeof(CharacterIntel)) {
            return "Character";
        } else if (type == typeof(Minion)) {
            return "Minion";
        } else if (type == typeof(ICharacter)) {
            return "Character";
        } else if (type == typeof(IUnit)) {
            return "Army/Minion";
        } else {
            return type.ToString();
        }
    }
    public void OnItemDroppedOut() {
        if (itemDroppedOutCallback != null) {
            itemDroppedOutCallback.Invoke(placedObject, slotIndex);
        }
    }
    public void HideVisuals() {
        portrait.gameObject.SetActive(false);
        factionEmblem.gameObject.SetActive(false);
        areaEmblem.gameObject.SetActive(false);
    }
}

[System.Serializable]
public class SlotItemDropEvent : UnityEvent<IDragParentItem> { }

public class ItemDroppedCallback : UnityEvent<object, int> { }
public class ItemDroppedOutCallback : UnityEvent<object, int> { }
