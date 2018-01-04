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
    protected float _population;

    public Faction(RACE race) {
        SetRace(race);
    }

    public void SetRace(RACE race) {
        _race = race;
    }


    //TODO: Add function that creates a character from the population
}
