/*
 This is the base class for each faction (major/minor)
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Faction {

    protected RACE _race;
    protected List<BaseLandmark> _ownedLandmarks;//List of settlements (cities/landmarks) owned by this faction
    internal Color factionColor;
    //TODO: Add list for characters that are part of the faction

    #region getters/setters
    public RACE race {
        get { return _race; }
    }
    public List<BaseLandmark> ownedLandmarks {
        get { return _ownedLandmarks; }
    }
    #endregion

    public Faction(RACE race) {
        SetRace(race);
        _ownedLandmarks = new List<BaseLandmark>();
        factionColor = Utilities.GetColorForFaction();
    }

    public void SetRace(RACE race) {
        _race = race;
    }

    #region Settlements
    public void AddLandmarkAsOwned(BaseLandmark landmark) {
        if (_ownedLandmarks.Contains(landmark)) {
            _ownedLandmarks.Add(landmark);
        }
    }
    public void RemoveLandmarkAsOwned(BaseLandmark landmark) {
        _ownedLandmarks.Remove(landmark);
    }
    #endregion
}
