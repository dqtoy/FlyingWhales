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
		this.isOneTime = true;
        _ruinLocation = ruinLocation;
        _ruinLocation.PutEventOnTile(this);
        Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "AncientRuin", "event_title");
    }

    #region Overrides
    internal override void OnCollectAvatarAction(Kingdom claimant) {
        base.OnCollectAvatarAction(claimant);
        SetStartedBy(claimant.king);
        _discoveredByKingdom = claimant;
        DiscoverAncientRuin();
    }
    #endregion

    internal void DiscoverAncientRuin() {
        //Debug.Log("Discover Ancient Ruin");
        RUIN_DISCOVERIES newDiscovery = GetRuinDiscovery();
        switch (newDiscovery) {
            case RUIN_DISCOVERIES.ANCIENT_WEAPONS:
                EventCreator.Instance.CreateDevelopWeaponsEvent(_discoveredByKingdom);
                break;
            case RUIN_DISCOVERIES.PLAGUE:
                EventCreator.Instance.CreatePlagueEvent(_discoveredByKingdom);
                break;
            case RUIN_DISCOVERIES.CONSTRUCTION_MATERIAL:
                int totalGrowthGained = 500;
                int growthPerCity = totalGrowthGained / _discoveredByKingdom.cities.Count;
                for (int i = 0; i < _discoveredByKingdom.cities.Count; i++) {
                    _discoveredByKingdom.cities[i].AdjustDailyGrowth(growthPerCity);
                }
                break;
            case RUIN_DISCOVERIES.ANCIENT_TECH:
                _discoveredByKingdom.AdjustTechCounter(1000);
                break;
            case RUIN_DISCOVERIES.MAP:
                List<HexTile> tilesToExpose = _ruinLocation.GetTilesInRange(8);
                for (int i = 0; i < tilesToExpose.Count; i++) {
                    HexTile currTile = tilesToExpose[i];
                    _discoveredByKingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
                }
                break;
            default:
                break;
        }
        Log newDiscoveryLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "AncientRuin", newDiscovery.ToString());
        newDiscoveryLog.AddToFillers(_discoveredByKingdom, _discoveredByKingdom.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        EventManager.Instance.AddEventToDictionary(this);
		EventIsCreated(this._discoveredByKingdom, true);
        DoneEvent();
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
