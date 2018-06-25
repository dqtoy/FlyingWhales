using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSaveData {

    public string monsterName;
    public LOCATION_IDENTIFIER locationType;
    public int locationID;

    public MonsterSaveData(Monster monster) {
        monsterName = monster.name;
        locationType = monster.specificLocation.locIdentifier;
        locationID = monster.specificLocation.id;
    }
}
