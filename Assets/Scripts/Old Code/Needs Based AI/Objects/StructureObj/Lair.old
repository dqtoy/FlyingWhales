using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Lair : StructureObj {

	public Lair() : base() {
        _specificObjectType = LANDMARK_TYPE.LAIR;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Lair clone = new Lair();
        SetCommonData(clone);
        return clone;
    }
    public override void OnAddToLandmark(BaseLandmark newLocation) {
        base.OnAddToLandmark(newLocation);
        //SpawnDragonAndEgg();
    }
    #endregion

    //private void SpawnDragonAndEgg() {
    //    MonsterPartyComponent monsterPartyComponent = LandmarkManager.Instance.GetLandmarkData(LANDMARK_TYPE.LAIR).startingMonsterSpawn;
    //    if(monsterPartyComponent != null) {
    //        MonsterParty monsterParty = MonsterManager.Instance.SpawnMonsterPartyOnLandmark(_objectLocation, monsterPartyComponent);
    //        if(monsterParty.mainCharacter.name == "Dragon") {
    //            monsterParty.actionData.AssignAction(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.HIBERNATE), monsterParty.icharacterObject);
    //            Item dragonEgg = ItemManager.Instance.CreateNewItemInstance("Dragon Egg");
    //            _objectLocation.AddItem(dragonEgg);
    //            Log log = new Log(GameManager.Instance.Today(), "Events", "DragonAttack", "lay_egg");
    //            log.AddToFillers(monsterParty.mainCharacter, monsterParty.mainCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //            log.AddToFillers(_objectLocation, _objectLocation.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
    //            monsterParty.mainCharacter.AddHistory(log);
    //            _objectLocation.AddHistory(log);
    //        }
    //    }
    //}
}
