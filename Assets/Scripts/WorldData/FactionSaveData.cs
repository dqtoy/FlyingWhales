using BayatGames.SaveGameFree.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FactionSaveData {
    public int factionID;
    public string factionName;
    public string factionDescription;
    public Race race;
    public ColorSave factionColor;
    public List<int> ownedAreas;
    public Dictionary<int, FACTION_RELATIONSHIP_STATUS> relationships;
    public int leaderID;
    public int emblemIndex;
    public int level;
    public bool isActive;
    public MORALITY morality;
    //public Dictionary<int, int> favor;
    public Dictionary<AreaCharacterClass, int> defenderWeights;

    public FactionSaveData(Faction faction) {
        factionID = faction.id;
        factionName = faction.name;
        factionDescription = faction.description;
        factionColor = new ColorSave(faction.factionColor);
        race = faction.race;
        ConstructOwnedAreas(faction);
        ConstructRelationships(faction);
        //ConstructFavor(faction);

        if (faction.leader == null) {
            leaderID = -1;
        } else {
            leaderID = faction.leader.id;
        }
        emblemIndex = FactionManager.Instance.GetFactionEmblemIndex(faction.emblem);
        morality = faction.morality;
        //defenderWeights = faction.defenderWeights.dictionary;
        level = faction.level;
        isActive = faction.isActive;
    }
    private void ConstructOwnedAreas(Faction faction) {
        ownedAreas = new List<int>();
        for (int i = 0; i < faction.ownedAreas.Count; i++) {
            Area area = faction.ownedAreas[i];
            ownedAreas.Add(area.id);
        }
    }
    private void ConstructRelationships(Faction faction) {
        relationships = new Dictionary<int, FACTION_RELATIONSHIP_STATUS>();
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in faction.relationships) {
            relationships.Add(kvp.Key.id, kvp.Value.relationshipStatus);
        }
    }
    //private void ConstructFavor(Faction faction) {
    //    favor = new Dictionary<int, int>();
    //    foreach (KeyValuePair<Faction, int> kvp in faction.favor) {
    //        favor.Add(kvp.Key.id, kvp.Value);
    //    }
    //}
}
