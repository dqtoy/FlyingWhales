using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionIntelUI : UIMenu {

    [SerializeField] private ScrollRect factionsScrollView;
    [SerializeField] private GameObject factionItemPrefab;
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closePosition;
    [SerializeField] private Vector3 halfPosition;
    [SerializeField] private EasyTween tweener;
    [SerializeField] private AnimationCurve curve;

    private Dictionary<Faction, FactionIntelItem> items;

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Faction>(Signals.FACTION_CREATED, OnFactionCreated);
        Messenger.AddListener<Faction>(Signals.FACTION_DELETED, OnFactionDeleted);
        Messenger.AddListener<Intel>(Signals.INTEL_ADDED, OnIntelAdded);
        Messenger.AddListener(Signals.INTERACTION_MENU_OPENED, OnInteractionMenuOpened);
        Messenger.AddListener(Signals.INTERACTION_MENU_CLOSED, OnInteractionMenuClosed);
        items = new Dictionary<Faction, FactionIntelItem>();
    }
    public override void CloseMenu() {
        isShowing = false;
    }
    public override void OpenMenu() {
        isShowing = true;
    }

    private void OnFactionCreated(Faction createdFaction) {
        GameObject factionItemGO = UIManager.Instance.InstantiateUIObject(factionItemPrefab.name, factionsScrollView.content);
        FactionIntelItem factionItem = factionItemGO.GetComponent<FactionIntelItem>();
        factionItem.SetFactionIntel(createdFaction.factionIntel);
        //factionItem.gameObject.SetActive(false);
        items.Add(createdFaction, factionItem);
        //UpdateColors();
    }
    private void OnFactionDeleted(Faction deletedFaction) {
        if (items.ContainsKey(deletedFaction)) {
            ObjectPoolManager.Instance.DestroyObject(items[deletedFaction].gameObject);
            items.Remove(deletedFaction);
            //UpdateColors();
        }
    }
    private FactionIntelItem GetItem(Faction faction) {
        if (items.ContainsKey(faction)) {
            return items[faction];
        }
        return null;
    }
    private void OnIntelAdded(Intel intel) {
        if (intel is FactionIntel) {
            FactionIntelItem item = GetItem((intel as FactionIntel).faction);
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
