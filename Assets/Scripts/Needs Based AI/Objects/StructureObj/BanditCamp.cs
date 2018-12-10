using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanditCamp : StructureObj {

    public BanditCamp() : base() {
        _specificObjectType = LANDMARK_TYPE.BANDIT_CAMP;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        BanditCamp clone = new BanditCamp();
        SetCommonData(clone);
        return clone;
    }
    #endregion

    //private void ReplenishDefenderUnits() {
    //    if (_objectLocation.defenders == null) {
    //        return;
    //    }
    //    string replenishSummary = GameManager.Instance.TodayLogString() + "Replenishing defender units for " + _objectLocation.locationName + ": ";
    //    for (int j = 0; j < _objectLocation.defenders.icharacters.Count; j++) {
    //        ICharacter currDefender = _objectLocation.defenders.icharacters[j];
    //        if (currDefender is CharacterArmyUnit) {
    //            CharacterArmyUnit armyUnit = currDefender as CharacterArmyUnit;
    //            if (!armyUnit.isCapped()) {
    //                armyUnit.AdjustArmyCount(1);
    //                replenishSummary += "\nReplensihed 1 " + armyUnit.characterClass.className;
    //            }
    //        } else if (currDefender is MonsterArmyUnit) {
    //            MonsterArmyUnit armyUnit = currDefender as MonsterArmyUnit;
    //            //if (!armyUnit.isCapped()) {
    //            armyUnit.AdjustArmyCount(1);
    //            //}
    //        }
    //    }
    //    Debug.Log(replenishSummary);
    //}
}
