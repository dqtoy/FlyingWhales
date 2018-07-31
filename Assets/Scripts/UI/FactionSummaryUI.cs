using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionSummaryUI : UIMenu {

    [SerializeField] private ScrollRect factionsScrollView;
    [SerializeField] private GameObject factionItemPrefab;
    [SerializeField] private Color evenColor;
    [SerializeField] private Color oddColor;

    private Dictionary<Faction, FactionSummaryItem> items;

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Faction>(Signals.FACTION_CREATED, OnFactionCreated);
        Messenger.AddListener<Faction>(Signals.FACTION_DELETED, OnFactionDeleted);
        items = new Dictionary<Faction, FactionSummaryItem>();
    }

    private void OnFactionCreated(Faction createdFaction) {
        GameObject factionItemGO = UIManager.Instance.InstantiateUIObject(factionItemPrefab.name, factionsScrollView.content);
        FactionSummaryItem factionItem = factionItemGO.GetComponent<FactionSummaryItem>();
        factionItem.SetFaction(createdFaction);
        items.Add(createdFaction, factionItem);
        UpdateColors();
    }
    private void OnFactionDeleted(Faction deletedFaction) {
        if (items.ContainsKey(deletedFaction)) {
            ObjectPoolManager.Instance.DestroyObject(items[deletedFaction].gameObject);
            items.Remove(deletedFaction);
            UpdateColors();
        }
    }

    private void UpdateColors() {
        int counter = 0;
        foreach (KeyValuePair<Faction, FactionSummaryItem> kvp in items) {
            if (Utilities.IsEven(counter)) {
                kvp.Value.SetBGColor(evenColor);
            } else {
                kvp.Value.SetBGColor(oddColor);
            }
            counter++;
        }
    }
}
