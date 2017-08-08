using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AncientRuin : GameEvent {

    private HexTile _ruinLocation;

    private Kingdom _discoveredByKingdom;

    private enum RUIN_DISCOVERIES {
        NONE,
        ANCIENT_WEAPONS,
        PLAGUE,
        CONSTRUCTION_MATERIAL,
        ANCIENT_TECH,
        MAP
    }

    private Dictionary<RUIN_DISCOVERIES, int> discoveryChances = new Dictionary<RUIN_DISCOVERIES, int>() {
        {RUIN_DISCOVERIES.ANCIENT_WEAPONS, 6},
        {RUIN_DISCOVERIES.PLAGUE, 2},
        {RUIN_DISCOVERIES.CONSTRUCTION_MATERIAL, 25},
        {RUIN_DISCOVERIES.ANCIENT_TECH, 25},
        {RUIN_DISCOVERIES.MAP, 15}
    };

    public AncientRuin(int startWeek, int startMonth, int startYear, Citizen startedBy, HexTile ruinLocation) : base(startWeek, startMonth, startYear, startedBy) {
        eventType = EVENT_TYPES.ANCIENT_RUIN;
        name = "Ancient Ruin";
        _ruinLocation = ruinLocation;
        _ruinLocation.PutEventOnTile(this);
        Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "AncientRuin", "event_title");
    }

    #region Overrides
    internal override void OnCollectAvatarAction(Citizen claimant) {
        base.OnCollectAvatarAction(claimant);
        SetStartedBy(claimant.city.kingdom.king);
        _discoveredByKingdom = claimant.city.kingdom;
        DiscoverAncientRuin();
        EventManager.Instance.AddEventToDictionary(this);
        EventIsCreated();
    }
    #endregion

    internal void DiscoverAncientRuin() {
        Debug.Log("Discover Ancient Ruin");

    }

    private RUIN_DISCOVERIES GetRuinDiscovery() {
        int maxChance = discoveryChances.Sum(x => x.Value);
        int chance = Random.Range(0, maxChance);
        int lowerBound = 0;
        int upperBound = 0;
        for (int i = 0; i < discoveryChances.Count; i++) {
            upperBound += discoveryChances.Values.ElementAt(i);
            RUIN_DISCOVERIES currDiscovery = discoveryChances.Keys.ElementAt(i);
            if(chance > lowerBound && chance <= upperBound) {
                return currDiscovery;
            }
            lowerBound = upperBound;
        }
        return RUIN_DISCOVERIES.NONE;
    }
}
