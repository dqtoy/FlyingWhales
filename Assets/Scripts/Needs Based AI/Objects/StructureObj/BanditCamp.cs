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
    //public override void GenerateInitialDefenders() {
    //    if (_objectLocation.owner == null) {
    //        return;
    //    }
    //    Debug.Log("Generating initial defenders for " + _specificObjectType.ToString());
    //    LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_specificObjectType);
    //    for (int i = 0; i < INITIAL_DEFENDERS; i++) {
    //        WeightedDictionary<LandmarkDefender> defenderWeights;
    //        if (i == 0) {
    //            defenderWeights = data.firstElementDefenderWeights;
    //        } else {
    //            defenderWeights = data.defenderWeights;
    //        }
    //        if (defenderWeights.GetTotalOfWeights() > 0) {
    //            LandmarkDefender chosenDefender = defenderWeights.PickRandomElementGivenWeights();
    //            CharacterArmyUnit defenderUnit = CharacterManager.Instance.CreateCharacterArmyUnit(_objectLocation.owner.race, chosenDefender, _objectLocation.owner, _objectLocation);
    //            _objectLocation.AddDefender(defenderUnit.ownParty);
    //        }
    //    }
    //}
    #endregion

    private void ReplenishDefenderUnits() {
        string replenishSummary = GameManager.Instance.TodayLogString() + "Replenishing defender units for " + _objectLocation.locationName + ": ";
        for (int j = 0; j < _objectLocation.defenders.Length; j++) {
            Party currParty = _objectLocation.defenders[j];
            if (currParty != null) {
                for (int k = 0; k < currParty.icharacters.Count; k++) {
                    ICharacter currCharacter = currParty.icharacters[k];
                    if (currCharacter is CharacterArmyUnit) {
                        CharacterArmyUnit armyUnit = currCharacter as CharacterArmyUnit;
                        if (!armyUnit.isCapped()) {
                            armyUnit.AdjustArmyCount(1);
                            replenishSummary += "\nReplensihed 1 " + armyUnit.characterClass.className;
                        }
                    } else if (currCharacter is MonsterArmyUnit) {
                        MonsterArmyUnit armyUnit = currCharacter as MonsterArmyUnit;
                        //if (!armyUnit.isCapped()) {
                            armyUnit.AdjustArmyCount(1);
                        //}
                    }
                }
            }
        }
        Debug.Log(replenishSummary);
    }
}
