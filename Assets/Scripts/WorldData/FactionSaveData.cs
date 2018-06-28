using BayatGames.SaveGameFree.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FactionSaveData {
    public int factionID;
    public string factionName;
    public ColorSave factionColor;
    public List<int> ownedAreas;
    public int leaderID;

    public FactionSaveData(Faction faction) {
        factionID = faction.id;
        factionName = faction.name;
        factionColor = new ColorSave(faction.factionColor);
        ConstructOwnedAreas(faction);

        if (faction.leader == null) {
            leaderID = -1;
        } else {
            leaderID = faction.leader.id;
        }
    }
    private void ConstructOwnedAreas(Faction faction) {
        ownedAreas = new List<int>();
        for (int i = 0; i < faction.ownedAreas.Count; i++) {
            Area area = faction.ownedAreas[i];
            ownedAreas.Add(area.id);
        }
    }
}
