using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationIntelUI : UIMenu {

    [SerializeField] private ScrollRect locationsScrollView;
    [SerializeField] private GameObject locationItemPrefab;
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closePosition;
    [SerializeField] private Vector3 halfPosition;
    [SerializeField] private EasyTween tweener;
    [SerializeField] private AnimationCurve curve;

    private Dictionary<Area, LocationIntelItem> items;

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Area>(Signals.AREA_CREATED, OnAreaCreated);
        Messenger.AddListener<Area>(Signals.AREA_DELETED, OnAreanDeleted);
        Messenger.AddListener<Intel>(Signals.INTEL_ADDED, OnIntelAdded);
        //Messenger.AddListener(Signals.INTERACTION_MENU_OPENED, OnInteractionMenuOpened);
        //Messenger.AddListener(Signals.INTERACTION_MENU_CLOSED, OnInteractionMenuClosed);
        items = new Dictionary<Area, LocationIntelItem>();
    }
    public override void CloseMenu() {
        isShowing = false;
    }
    public override void OpenMenu() {
        isShowing = true;
    }

    private void OnAreaCreated(Area createdArea) {
        GameObject locationItemGO = UIManager.Instance.InstantiateUIObject(locationItemPrefab.name, locationsScrollView.content);
        LocationIntelItem locationItem = locationItemGO.GetComponent<LocationIntelItem>();
        locationItem.SetLocation(createdArea.locationIntel);
        locationItem.gameObject.SetActive(false);
        items.Add(createdArea, locationItem);
    }
    private void OnAreanDeleted(Area deletedArea) {
        if (items.ContainsKey(deletedArea)) {
            ObjectPoolManager.Instance.DestroyObject(items[deletedArea].gameObject);
            items.Remove(deletedArea);
        }
    }
    private LocationIntelItem GetItem(Area area) {
        if (items.ContainsKey(area)) {
            return items[area];
        }
        return null;
    }
    private void OnIntelAdded(Intel intel) {
        if (intel is LocationIntel) {
            LocationIntelItem item = GetItem((intel as LocationIntel).location);
            if (item != null) {
                item.gameObject.SetActive(true);
            }
        }
    }

    private void OnInteractionMenuOpened() {
        if (this.isShowing) {
            //if the menu is showing update it's open position
            //only open halfway
            tweener.SetAnimationPosition(openPosition, halfPosition, curve, curve);
            tweener.ChangeSetState(false);
            tweener.TriggerOpenClose();
            tweener.SetAnimationPosition(closePosition, halfPosition, curve, curve);
        } else {
            //only open halfway
            tweener.SetAnimationPosition(closePosition, halfPosition, curve, curve);
        }
    }
    private void OnInteractionMenuClosed() {
        if (this.isShowing) {
            tweener.SetAnimationPosition(halfPosition, openPosition, curve, curve);
            tweener.ChangeSetState(false);
            tweener.TriggerOpenClose();
            tweener.SetAnimationPosition(closePosition, openPosition, curve, curve);
        } else {
            //reset positions to normal
            tweener.SetAnimationPosition(closePosition, openPosition, curve, curve);
        }
    }
}
