using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FactionSummaryUI : UIMenu {

    internal bool isShowing;

    [SerializeField] private UIGrid _factionsGrid;
    [SerializeField] private UIEventTrigger orderBySettlements;
    [SerializeField] private UIEventTrigger orderByPopulation;
    [SerializeField] private UIEventTrigger orderByCharacters;
    private FactionSummaryEntry[] factionSummaryEntries;

    internal override void Initialize() {
        factionSummaryEntries = Utilities.GetComponentsInDirectChildren<FactionSummaryEntry>(_factionsGrid.gameObject);
        EventDelegate.Add(orderBySettlements.onClick, () => FactionManager.Instance.SetOrderBy(ORDER_BY.CITIES));
        EventDelegate.Add(orderByPopulation.onClick, () => FactionManager.Instance.SetOrderBy(ORDER_BY.POPULATION));
        EventDelegate.Add(orderByCharacters.onClick, () => FactionManager.Instance.SetOrderBy(ORDER_BY.CHARACTERS));
    }

    public void ShowFactionSummary() {
        this.gameObject.SetActive(true);
    }
    public void HideFactionSummary() {
        this.gameObject.SetActive(false);
    }

    public void UpdateFactionsSummary() {
        List<Faction> factions = new List<Faction>(FactionManager.Instance.orderedFactions); ;
        //factions.Reverse();
        for (int i = 0; i < factionSummaryEntries.Length; i++) {
            FactionSummaryEntry fse = factionSummaryEntries[i];
            Faction faction = factions.ElementAtOrDefault(i);
            if(faction == null) {
                fse.gameObject.SetActive(false);
            } else {
                fse.gameObject.SetActive(true);
                fse.SetFaction(faction);
            }
        }
        _factionsGrid.Reposition();
    }
}
