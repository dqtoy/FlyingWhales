using BayatGames.SaveGameFree.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FactionSaveData {
    public int factionID;
    public string factionName;
    public RACE factionRace;
    public FACTION_TYPE factionType;
    public ColorSave factionColor;
    public List<int> ownedRegions;

    public FactionSaveData(Faction faction) {
        factionID = faction.id;
        factionName = faction.name;
        factionRace = faction.race;
        factionType = faction.factionType;
        factionColor = new ColorSave(faction.factionColor);
        ConstructOwnedRegions(faction);
    }
    private void ConstructOwnedRegions(Faction faction) {
        ownedRegions = new List<int>();
        for (int i = 0; i < faction.ownedRegions.Count; i++) {
            Region region = faction.ownedRegions[i];
            ownedRegions.Add(region.id);
        }
    }
}
