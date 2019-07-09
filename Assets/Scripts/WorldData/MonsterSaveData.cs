using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSaveData {

    public int id;
    public string monsterName;
    public LOCATION_IDENTIFIER locationType;
    public int locationID;

    public MonsterSaveData(MonsterParty monsterParty) {
        id = monsterParty.id;
        //monsterName = monsterParty.setupName;
        //locationType = monsterParty.specificLocation.locIdentifier;
        locationID = monsterParty.specificLocation.id;
    }
}
