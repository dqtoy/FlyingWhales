/*
 This is the base class for each faction (major/minor)
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Faction {
    protected string _name;
    protected RACE _race;
    protected FACTION_TYPE _factionType;
    private Sprite _emblem;
    private Sprite _emblemBG;
    [SerializeField] private List<Sprite> usedEmblems = new List<Sprite>();
    protected List<BaseLandmark> _ownedLandmarks;//List of settlements (cities/landmarks) owned by this faction
    protected List<TECHNOLOGY> _inititalTechnologies;
    internal Color factionColor;
    //TODO: Add list for characters that are part of the faction

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public RACE race {
        get { return _race; }
    }
    public List<BaseLandmark> ownedLandmarks {
        get { return _ownedLandmarks; }
    }
    public List<TECHNOLOGY> inititalTechnologies {
        get { return _inititalTechnologies; }
    }
    #endregion

    public Faction(RACE race, FACTION_TYPE factionType) {
        SetRace(race);
        _name = RandomNameGenerator.Instance.GenerateKingdomName(race);
        _factionType = factionType;
        _emblem = FactionManager.Instance.GenerateFactionEmblem(this);
        _emblemBG = FactionManager.Instance.GenerateFactionEmblemBG();
        _ownedLandmarks = new List<BaseLandmark>();
        factionColor = Utilities.GetColorForFaction();
        ConstructInititalTechnologies();
    }

    public void SetRace(RACE race) {
        _race = race;
    }

    #region Settlements
    public void AddLandmarkAsOwned(BaseLandmark landmark) {
        if (!_ownedLandmarks.Contains(landmark)) {
            _ownedLandmarks.Add(landmark);
        }
    }
    public void RemoveLandmarkAsOwned(BaseLandmark landmark) {
        _ownedLandmarks.Remove(landmark);
    }
    #endregion

    #region Technologies
    protected void ConstructInititalTechnologies() {
        _inititalTechnologies = new List<TECHNOLOGY>();
        if (FactionManager.Instance.inititalRaceTechnologies.ContainsKey(this.race)) {
            _inititalTechnologies.AddRange(FactionManager.Instance.inititalRaceTechnologies[this.race]);
        }
    }
    #endregion
}
