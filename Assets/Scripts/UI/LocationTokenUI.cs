using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationTokenUI : UIMenu {

    [SerializeField] private ScrollRect locationsScrollView;
    [SerializeField] private GameObject locationItemPrefab;
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closePosition;
    [SerializeField] private EasyTween tweener;
    [SerializeField] private AnimationCurve curve;

    private Dictionary<Area, LocationTokenItem> items;

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Area>(Signals.AREA_CREATED, OnAreaCreated);
        Messenger.AddListener<Area>(Signals.AREA_DELETED, OnAreanDeleted);
        Messenger.AddListener<Token>(Signals.TOKEN_ADDED, OnIntelAdded);
        items = new Dictionary<Area, LocationTokenItem>();
    }
    public override void CloseMenu() {
        isShowing = false;
    }
    public override void OpenMenu() {
        isShowing = true;
    }

    private void OnAreaCreated(Area createdArea) {
        GameObject locationItemGO = UIManager.Instance.InstantiateUIObject(locationItemPrefab.name, locationsScrollView.content);
        LocationTokenItem locationItem = locationItemGO.GetComponent<LocationTokenItem>();
        locationItem.SetLocation(createdArea.locationToken);
        locationItem.gameObject.SetActive(false);
        items.Add(createdArea, locationItem);
    }
    private void OnAreanDeleted(Area deletedArea) {
        if (items.ContainsKey(deletedArea)) {
            ObjectPoolManager.Instance.DestroyObject(items[deletedArea].gameObject);
            items.Remove(deletedArea);
        }
    }
    private LocationTokenItem GetItem(Area area) {
        if (items.ContainsKey(area)) {
            return items[area];
        }
        return null;
    }
    private void OnIntelAdded(Token intel) {
        if (intel is LocationToken) {
            LocationTokenItem item = GetItem((intel as LocationToken).location);
            if (item != null) {
                item.gameObject.SetActive(true);
            }
        }
    }
}
