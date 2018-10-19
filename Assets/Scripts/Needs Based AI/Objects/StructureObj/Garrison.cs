using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garrison : StructureObj {

    public Garrison() {
        _specificObjectType = LANDMARK_TYPE.GARRISON;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Garrison clone = new Garrison();
        SetCommonData(clone);
        return clone;
    }
    public override void StartDayAction() {
        ReplenishDefenderUnits();
        base.StartDayAction();
    }
    #endregion

    private void ReplenishDefenderUnits() {
        if (_objectLocation.defenders == null) {
            return;
        }
        string replenishSummary = GameManager.Instance.TodayLogString() + "Replenishing defender units for " + _objectLocation.locationName + ": ";
        for (int j = 0; j < _objectLocation.defenders.icharacters.Count; j++) {
            ICharacter currCharacter = _objectLocation.defenders.icharacters[j];
            if (currCharacter is CharacterArmyUnit) {
                CharacterArmyUnit armyUnit = currCharacter as CharacterArmyUnit;
                int productionCost = armyUnit.GetProductionCost();
                if (!armyUnit.isCapped() && _objectLocation.tileLocation.areaOfTile.HasEnoughSupplies(productionCost)) {
                    armyUnit.AdjustArmyCount(1);
                    _objectLocation.tileLocation.areaOfTile.AdjustSuppliesInBank(-productionCost);
                    replenishSummary += "\nReplensihed 1 " + armyUnit.characterClass.className + " for " + productionCost;
                }
            } else if (currCharacter is MonsterArmyUnit) {
                MonsterArmyUnit armyUnit = currCharacter as MonsterArmyUnit;
                int productionCost = armyUnit.GetProductionCost();
                if (_objectLocation.tileLocation.areaOfTile.HasEnoughSupplies(productionCost)) {
                    armyUnit.AdjustArmyCount(1);
                    _objectLocation.tileLocation.areaOfTile.AdjustSuppliesInBank(-productionCost);
                }
            }
        }
        Debug.Log(replenishSummary);
    }

}
