using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotItem : MonoBehaviour {

    public object placedObject { get; private set; }
    public System.Type neededType { get; private set; }

    public SlotItemDropEvent onItemDropped;
    public ItemDroppedCallback itemDroppedCallback;
    public ItemDroppedOutCallback itemDroppedOutCallback;

    [Space(10)]
    [Header("Slot Elements")]
    public CharacterPortrait portrait;
    public AreaEmblem areaEmblem;
    public FactionEmblem factionEmblem;
    public CustomDropZone dropZone;
    public SlotItemDraggableItem draggable;

    [SerializeField] private string neededTypeStr;


    public int slotIndex { get; private set; }

    private string hoverInfo;

    public void SetNeededType(System.Type neededType) {
        this.neededType = neededType;
        neededTypeStr = neededType.ToString();
    }
    public void SetSlotIndex(int index) {
        slotIndex = index;
    }

    public void OnDropItemAtDropZone(GameObject go) { //this is used to filter if the dragged object is valid for this slot
        DragObject dragObj = go.GetComponent<DragObject>();
        if (dragObj == null) {
            return;
        }
        IDragParentItem parentItem = dragObj.parentItem;
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
            hoverInfo = (associatedObj as FactionIntel).faction.name;
        } else if (associatedObj is LocationIntel) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(true);
            portrait.gameObject.SetActive(false);
            hoverInfo = (associatedObj as LocationIntel).location.name;
        } else if (associatedObj is CharacterIntel) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(false);
            portrait.gameObject.SetActive(true);
            portrait.GeneratePortrait((associatedObj as CharacterIntel).character);
            hoverInfo = (associatedObj as CharacterIntel).character.name;
        } else if (associatedObj is Minion) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(false);
            portrait.gameObject.SetActive(true);
            portrait.GeneratePortrait((associatedObj as Minion).icharacter);
            hoverInfo = (associatedObj as Minion).name;
        } else if (associatedObj is ICharacter) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(false);
            portrait.gameObject.SetActive(true);
            portrait.GeneratePortrait((associatedObj as ICharacter));
            hoverInfo = (associatedObj as ICharacter).name;
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
}

[System.Serializable]
public class SlotItemDropEvent : UnityEvent<IDragParentItem> { }

public class ItemDroppedCallback : UnityEvent<object, int> { }
public class ItemDroppedOutCallback : UnityEvent<object, int> { }
