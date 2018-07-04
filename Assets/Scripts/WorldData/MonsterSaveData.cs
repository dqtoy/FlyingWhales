using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSaveData {

    public int id;
    public string monsterName;
    public LOCATION_IDENTIFIER locationType;
    public int locationID;

    public MonsterSaveData(Monster monster) {
        id = monster.id;
        monsterName = monster.name;
        locationType = monster.party.specificLocation.locIdentifier;
        locationID = monster.party.specificLocation.id;
    }
}
