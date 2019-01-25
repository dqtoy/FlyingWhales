using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour {
    public static MonsterManager Instance = null;

    private Dictionary<string, Monster> _monstersDictionary;
    private Dictionary<string, MonsterArmyUnit> _monsterArmyUnitsDictionary;

    public GameObject monsterIconPrefab;
    //public MonsterComponent monsterComponent;

    [SerializeField] private List<MonsterPartyComponent> monsterPartySetups;
    public List<MonsterPartyComponent> monsterAttackParties;

    public List<MonsterParty> allMonsterParties;
    public List<Monster> allMonsters;
    public List<MonsterArmyUnit> allMonsterArmyUnits;

    [Header("Monster Icons")]
    [SerializeField] private Sprite catSprite;
    [SerializeField] private Sprite direBeast1Sprite;
    [SerializeField] private Sprite direBeast2Sprite;
    [SerializeField] private Sprite golem1Sprite;
    [SerializeField] private Sprite golem2Sprite;
    [SerializeField] private Sprite orc1Sprite;
    [SerializeField] private Sprite rat1Sprite;
    [SerializeField] private Sprite rat2Sprite;
    [SerializeField] private Sprite slime1Sprite;
    [SerializeField] private Sprite slime2Sprite;
    [SerializeField] private Sprite wolf1Sprite;

    #region getters/setters
    public Dictionary<string, Monster> monstersDictionary {
        get { return _monstersDictionary; }
    }
    #endregion

    private void Awake() {
        Instance = this;
        allMonsterParties = new List<MonsterParty>();
        allMonsters = new List<Monster>();
        allMonsterArmyUnits = new List<MonsterArmyUnit>();
    }

    public void Initialize() {
        //ConstructAllMonsters();
    }

    private void ConstructAllMonsters() {
        _monstersDictionary = new Dictionary<string, Monster>();
        _monsterArmyUnitsDictionary = new Dictionary<string, MonsterArmyUnit>();
        string path = Utilities.dataPath + "Monsters/";
        string[] monsters = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < monsters.Length; i++) {
            //JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(classes[i]), monsterComponent);
            Monster monster = JsonUtility.FromJson<Monster>(System.IO.File.ReadAllText(monsters[i]));
            MonsterArmyUnit monsterArmyUnit = JsonUtility.FromJson<MonsterArmyUnit>(System.IO.File.ReadAllText(monsters[i]));
            monster.ConstructMonsterData();
            monsterArmyUnit.ConstructMonsterData();
            _monstersDictionary.Add(monster.name, monster);
            _monsterArmyUnitsDictionary.Add(monsterArmyUnit.name, monsterArmyUnit);
        }
    }
    public Monster CreateNewMonster(string monsterName) {
        Monster newMonster = _monstersDictionary[monsterName].CreateNewCopy();
        newMonster.Initialize();
        allMonsters.Add(newMonster);
        return newMonster;
    }
    public MonsterArmyUnit CreateNewMonsterArmyUnit(string monsterName) {
        MonsterArmyUnit newMonster = _monsterArmyUnitsDictionary[monsterName].CreateNewCopy();
        newMonster.Initialize();
        allMonsterArmyUnits.Add(newMonster);
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
    public MonsterParty SpawnMonsterPartyOnLandmark(BaseLandmark landmark, MonsterPartyComponent monsterPartyComponent, bool makeMonsterResident = true) {
        MonsterParty monsterParty = new MonsterParty();
        monsterParty.SetSetupName(monsterPartyComponent.name);
        for (int i = 0; i < monsterPartyComponent.monsters.Length; i++) {
            if (monsterPartyComponent.monsters[i] != null) {
                string monsterName = monsterPartyComponent.monsters[i].name;
                Monster monster = CreateNewMonster(monsterName);
                if (makeMonsterResident) {
                    //landmark.AddCharacterHomeOnLandmark(monster);
                }
                monster.SetOwnedParty(monsterParty);
                monster.SetCurrentParty(monsterParty);
                //monsterParty.AddCharacter(monster);
            }
        }
#if !WORLD_CREATION_TOOL
        monsterParty.CreateIcon();
        monsterParty.icon.SetPosition(landmark.tileLocation.transform.position);
#endif
        //landmark.AddCharacterToLocation(monsterParty);
        allMonsterParties.Add(monsterParty);
        return monsterParty;
    }
    public void RemoveMonster(MonsterParty party) {
        allMonsterParties.Remove(party);
#if !WORLD_CREATION_TOOL
        GameObject.Destroy(party.icon.gameObject);
#endif
    }
    public Monster GetMonsterByID(int id) {
        for (int i = 0; i < allMonsters.Count; i++) {
            Monster currChar = allMonsters[i];
            if (currChar.id == id) {
                return currChar;
            }
        }
        return null;
    }

    #region Icons
    public Sprite GetMonsterSprite(string monster) {
        if (monster.Contains("Cat")) {
            return catSprite;
        } else if (monster.Contains("Direbeast")) {
            if (monster.Contains("1")) {
                return direBeast1Sprite;
            } else {
                return direBeast2Sprite;
            }
        } else if (monster.Contains("Golem")) {
            if (monster.Contains("1")) {
                return golem1Sprite;
            } else {
                return golem2Sprite;
            }
        } else if (monster.Contains("Orc")) {
            return orc1Sprite;
        } else if (monster.Contains("Rat")) {
            return rat1Sprite;
        } else if (monster.Contains("Slime")) {
            if (monster.Contains("1")) {
                return slime1Sprite;
            } else {
                return slime2Sprite;
            }
        } else if (monster.Contains("Wolf")) {
            return wolf1Sprite;
        }
        return null;
    }
    #endregion
}
