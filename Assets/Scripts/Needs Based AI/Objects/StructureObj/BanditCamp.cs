using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanditCamp : StructureObj {

    private const int INITIAL_DEFENDERS = 3;

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
    public override void GenerateInitialDefenders() {
        if (_objectLocation.owner == null) {
            return;
        }
        Debug.Log("Generating initial defenders for " + _specificObjectType.ToString());
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_specificObjectType);
        for (int i = 0; i < INITIAL_DEFENDERS; i++) {
            WeightedDictionary<LandmarkDefender> defenderWeights;
            if (i == 0) {
                defenderWeights = data.firstElementDefenderWeights;
            } else {
                defenderWeights = data.defenderWeights;
            }
            if (defenderWeights.GetTotalOfWeights() > 0) {
                LandmarkDefender chosenDefender = defenderWeights.PickRandomElementGivenWeights();
                CharacterArmyUnit defenderUnit = CharacterManager.Instance.CreateCharacterArmyUnit(_objectLocation.owner.race, chosenDefender, _objectLocation.owner, _objectLocation);
                _objectLocation.AddDefender(defenderUnit.ownParty);
            }
        }
    }
    #endregion
}
