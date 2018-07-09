using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour {
    public static MonsterManager Instance = null;

    private Dictionary<string, Monster> _monstersDictionary;
    public GameObject monsterIconPrefab;
    public MonsterComponent monsterComponent;

    [SerializeField] private List<MonsterPartyComponent> monsterPartySetups;
    public List<MonsterParty> allMonsterParties;

    #region getters/setters
    public Dictionary<string, Monster> monstersDictionary {
        get { return _monstersDictionary; }
    }
    #endregion

    private void Awake() {
        Instance = this;
        allMonsterParties = new List<MonsterParty>();
    }

    public void Initialize() {
        ConstructAllMonsters();
    }

    private void ConstructAllMonsters() {
        _monstersDictionary = new Dictionary<string, Monster>();
        string path = Utilities.dataPath + "Monsters/";
        string[] classes = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < classes.Length; i++) {
            JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(classes[i]), monsterComponent);
            Monster monster = new Monster();
            monster.SetData(monsterComponent);
            _monstersDictionary.Add(monster.name, monster);
        }
    }
    public Monster CreateNewMonster(string monsterName) {
        Monster newMonster = _monstersDictionary[monsterName].CreateNewCopy();
        newMonster.Initialize();
        return newMonster;
    }
    //public Monster CreateNewMonster(MonsterSaveData data) {
    //    Monster newMonster = _monstersDictionary[data.monsterName].CreateNewCopy();
    //    newMonster.Initialize(data);
    //    return newMonster;
    //}
    public MonsterPartyComponent GetMonsterPartySetup(string partyName) {
        for (int i = 0; i < monsterPartySetups.Count; i++) {
            MonsterPartyComponent currComponent = monsterPartySetups[i];
            if (currComponent.name.Equals(partyName)) {
                return currComponent;
            }
        }
        throw new System.Exception("No monster party setup with name " + partyName);
    }

    //public Monster SpawnMonsterOnTile(HexTile tile, string monsterName) {
    //    Monster newMonster = CreateNewMonster(monsterName);
    //    MonsterParty monsterParty = newMonster.CreateNewParty();
    //    monsterParty.CreateIcon();
    //    monsterParty.icon.SetPosition(tile.transform.position);
    //    monsterParty.SetSpecificLocation(tile);
    //    return newMonster;
    //}
    //public Monster SpawnMonsterOnTile(HexTile tile, MonsterSaveData data) {
    //    Monster newMonster = CreateNewMonster(data);
    //    MonsterParty monsterParty = newMonster.CreateNewParty();
    //    monsterParty.CreateIcon();
    //    monsterParty.icon.SetPosition(tile.transform.position);
    //    monsterParty.SetSpecificLocation(tile);
    //    return newMonster;
    //}
//    public Monster SpawnMonsterOnLandmark(BaseLandmark landmark, string monsterName) {
//        Monster newMonster = CreateNewMonster(monsterName);
//        MonsterParty monsterParty = newMonster.CreateNewParty();
//#if !WORLD_CREATION_TOOL
//        monsterParty.CreateIcon();
//        monsterParty.icon.SetPosition(landmark.tileLocation.transform.position);
//#endif
//        landmark.AddCharacterToLocation(monsterParty);
//        return newMonster;
//    }
//    public Monster SpawnMonsterOnLandmark(BaseLandmark landmark, MonsterSaveData data) {
//        Monster newMonster = CreateNewMonster(data);
//        MonsterParty monsterParty = newMonster.CreateNewParty();
//#if !WORLD_CREATION_TOOL
//        monsterParty.CreateIcon();
//        monsterParty.icon.SetPosition(landmark.tileLocation.transform.position);
//#endif
//        landmark.AddCharacterToLocation(monsterParty);
//        return newMonster;
//    }
    public MonsterParty SpawnMonsterPartyOnLandmark(BaseLandmark landmark, MonsterPartyComponent monsterPartyComponent) {
        MonsterParty monsterParty = new MonsterParty();
        monsterParty.SetSetupName(monsterPartyComponent.name);
        for (int i = 0; i < monsterPartyComponent.monsters.Length; i++) {
            string monsterName = monsterPartyComponent.monsters[i].name;
            Monster monster = CreateNewMonster(monsterName);
            monsterParty.AddCharacter(monster);
        }
#if !WORLD_CREATION_TOOL
        monsterParty.CreateIcon();
        monsterParty.icon.SetPosition(landmark.tileLocation.transform.position);
#endif
        landmark.AddCharacterToLocation(monsterParty);
        allMonsterParties.Add(monsterParty);
        return monsterParty;
    }
    public void DespawnMonsterPartyOnLandmark(BaseLandmark landmark, MonsterParty monsterParty) {
        landmark.RemoveCharacterFromLocation(monsterParty);
        RemoveMonster(monsterParty);
#if !WORLD_CREATION_TOOL
        GameObject.Destroy(monsterParty.icon.gameObject);
#endif
    }
    //public void DespawnMonsterOnLandmark(BaseLandmark landmark, Monster monster) {
    //    landmark.RemoveCharacterFromLocation(monster.party);
    //    //RemoveMonster(monster);
    //}
    public void RemoveMonster(MonsterParty party) {
        allMonsterParties.Remove(party);
#if !WORLD_CREATION_TOOL
        GameObject.Destroy(party.icon.gameObject);
#endif
    }
    
    public bool HasMonsterOnTile(HexTile tile) {
        for (int i = 0; i < allMonsterParties.Count; i++) {
            MonsterParty currMonsterParty = allMonsterParties[i];
            if (currMonsterParty.specificLocation.locIdentifier == LOCATION_IDENTIFIER.HEXTILE && currMonsterParty.specificLocation.id == tile.id) {
                return true;
            }
        }
        return false;
    }

    public bool HasMonsterOnLandmark(BaseLandmark landmark) {
        for (int i = 0; i < landmark.charactersAtLocation.Count; i++) {
            NewParty currParty = landmark.charactersAtLocation[i];
            if (currParty is MonsterParty) {
                return true;
            }
        }
        return false;
    }
    
    public List<MonsterParty> GetMonstersOnTile(HexTile tile) {
        List<MonsterParty> monsterParties = new List<MonsterParty>();
        for (int i = 0; i < allMonsterParties.Count; i++) {
            MonsterParty currMonsterParty = allMonsterParties[i];
            if (currMonsterParty.specificLocation.locIdentifier == LOCATION_IDENTIFIER.HEXTILE && currMonsterParty.specificLocation.id == tile.id) {
                monsterParties.Add(currMonsterParty);
            }
        }
        return monsterParties;
    }
    public void RemoveMonstersOnTile(HexTile tile) {
        List<MonsterParty> monsterParties = GetMonstersOnTile(tile);
        for (int i = 0; i < monsterParties.Count; i++) {
            RemoveMonster(monsterParties[i]);
        }
    }

    public void LoadMonsters(WorldSaveData data) {
        if (data.monstersData != null) {
            for (int i = 0; i < data.monstersData.Count; i++) {
                MonsterSaveData monsterData = data.monstersData[i];
                MonsterPartyComponent partyComp = GetMonsterPartySetup(monsterData.monsterName);
                if (monsterData.locationType == LOCATION_IDENTIFIER.LANDMARK) {
                    BaseLandmark landmark = LandmarkManager.Instance.GetLandmarkByID(monsterData.locationID);
                    SpawnMonsterPartyOnLandmark(landmark, partyComp);
                }

//                if (monsterData.locationType == LOCATION_IDENTIFIER.HEXTILE) {
//#if WORLD_CREATION_TOOL
//                    HexTile tile = worldcreator.WorldCreatorManager.Instance.GetHexTile(monsterData.locationID);
//#else
//                    HexTile tile = GridMap.Instance.GetHexTile(monsterData.locationID);
//#endif
//                    SpawnMonsterOnTile(tile, monsterData);
//                } else if (monsterData.locationType == LOCATION_IDENTIFIER.LANDMARK) {
//                    BaseLandmark landmark = LandmarkManager.Instance.GetLandmarkByID(monsterData.locationID);
//                    SpawnMonsterOnLandmark(landmark, monsterData);
//                }
            }
        }
    }
}
