using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionTokenUI : UIMenu {

    [SerializeField] private ScrollRect factionsScrollView;
    [SerializeField] private GameObject factionItemPrefab;
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closePosition;
    [SerializeField] private Vector3 halfPosition;
    [SerializeField] private EasyTween tweener;
    [SerializeField] private AnimationCurve curve;

    private Dictionary<Faction, FactionTokenItem> items;

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Faction>(Signals.FACTION_CREATED, OnFactionCreated);
        Messenger.AddListener<Faction>(Signals.FACTION_DELETED, OnFactionDeleted);
        Messenger.AddListener<Token>(Signals.TOKEN_ADDED, OnTokenAdded);
        items = new Dictionary<Faction, FactionTokenItem>();
    }
    public override void CloseMenu() {
        isShowing = false;
    }
    public override void OpenMenu() {
        isShowing = true;
    }

    private void OnFactionCreated(Faction createdFaction) {
        GameObject factionItemGO = UIManager.Instance.InstantiateUIObject(factionItemPrefab.name, factionsScrollView.content);
        FactionTokenItem factionItem = factionItemGO.GetComponent<FactionTokenItem>();
        factionItem.SetFactionToken(createdFaction.factionToken);
        factionItem.gameObject.SetActive(false);
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
    private FactionTokenItem GetItem(Faction faction) {
        if (items.ContainsKey(faction)) {
            return items[faction];
        }
        return null;
    }
    private void OnTokenAdded(Token token) {
        if (token is FactionToken) {
            FactionTokenItem item = GetItem((token as FactionToken).faction);
            if (item != null) {
                item.gameObject.SetActive(true);
            }
        }
    }
}
