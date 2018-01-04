/*
 This is the base class for each faction (major/minor)
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Faction {

    protected RACE _race;
    protected List<Settlement> _settlements;//List of settlements (cities/landmarks) owned by this faction
    //TODO: Add list for characters that are part of the faction

    public Faction(RACE race) {
        SetRace(race);
    }

    public void SetRace(RACE race) {
        _race = race;
    }

    #region Settlements
    public void AddSettlement(Settlement settlement) {
        if (_settlements.Contains(settlement)) {
            _settlements.Add(settlement);
        }
    }
    public void RemoveSettlement(Settlement settlement) {
        _settlements.Remove(settlement);
    }
    #endregion
}
