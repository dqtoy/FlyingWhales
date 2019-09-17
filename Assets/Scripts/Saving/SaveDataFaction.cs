using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;

[System.Serializable]
public class SaveDataFaction {
    public int id;
    public string name;
    public string description;
    public string initialLeaderClass;
    public string requirementForJoining;
    public int level;
    public int inventoryTaskWeight;
    public bool isPlayerFaction;
    public GENDER initialLeaderGender;
    public RACE initialLeaderRace;
    public RACE race;
    public int emblemIndex;
    public ColorSave factionColor;
    public List<int> ownedLandmarkIDs;
    //public List<int> characterIDs;
    public List<int> ownedRegionIDs;
    //public List<RACE> recruitableRaces { get; protected set; }
    //public List<RACE> startingFollowers { get; protected set; }
    //public Dictionary<Faction, FactionRelationship> relationships { get; protected set; }

    public MORALITY morality;
    public FACTION_SIZE size;
    public FACTION_TYPE factionType;
    //public WeightedDictionary<AreaCharacterClass> additionalClassWeights { get; private set; }
    public bool isActive;
    //public List<Log> history { get; private set; }

    public void Save(Faction faction) {
        id = faction.id;
        name = faction.name;
        description = faction.description;
        initialLeaderClass = faction.initialLeaderClass;
        level = faction.level;
        inventoryTaskWeight = faction.inventoryTaskWeight;
        isPlayerFaction = faction.isPlayerFaction;
        initialLeaderGender = faction.initialLeaderGender;
        initialLeaderRace = faction.initialLeaderRace;
        race = faction.race;
        emblemIndex = FactionManager.Instance.GetFactionEmblemIndex(faction.emblem);
        factionColor = faction.factionColor;
        morality = faction.morality;
        size = faction.size;
        factionType = faction.factionType;
        isActive = faction.isActive;
        requirementForJoining = faction.requirementForJoining;

        ownedLandmarkIDs = new List<int>();
        for (int i = 0; i < faction.ownedLandmarks.Count; i++) {
            ownedLandmarkIDs.Add(faction.ownedLandmarks[i].id);
        }

        //characterIDs = new List<int>();
        //for (int i = 0; i < faction.characters.Count; i++) {
        //    characterIDs.Add(faction.characters[i].id);
        //}

        ownedRegionIDs = new List<int>();
        for (int i = 0; i < faction.ownedRegions.Count; i++) {
            ownedRegionIDs.Add(faction.ownedRegions[i].id);
        }
    }

    public void Load(List<BaseLandmark> allLandmarks) {
        Faction faction = FactionManager.Instance.CreateNewFaction(this);

        for (int i = 0; i < ownedLandmarkIDs.Count; i++) {
            for (int j = 0; j < allLandmarks.Count; j++) {
                BaseLandmark landmark = allLandmarks[j];
                if (landmark.id == ownedLandmarkIDs[i]) {
                    faction.OwnLandmark(landmark);
                    break;
                }
            }
        }

        for (int i = 0; i < ownedRegionIDs.Count; i++) {
            Region region = GridMap.Instance.GetRegionByID(ownedRegionIDs[i]);
            LandmarkManager.Instance.OwnRegion(faction, faction.race, region);
        }
    }
}
