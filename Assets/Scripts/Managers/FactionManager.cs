using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FactionManager : MonoBehaviour {

    public static FactionManager Instance = null;

    [SerializeField] private RACE[] inititalRaces;

    public List<Faction> allFactions;

    private void Awake() {
        Instance = this;
    }

    /*
     Generate the initital factions,
     races are specified in the inspector (inititalRaces)
     */
    public void GenerateInititalFactions() {
        for (int i = 0; i < inititalRaces.Length; i++) {
            RACE inititalRace = inititalRaces[i];
            CreateNewFaction(typeof(Tribe), inititalRace);
        }
    }

    public Faction CreateNewFaction(System.Type factionType, RACE race) {
        if(factionType == typeof(Tribe)) {
            Tribe newTribe = new Tribe(race);
            allFactions.Add(newTribe);
            return newTribe;
        }
        return null;
    }
}
