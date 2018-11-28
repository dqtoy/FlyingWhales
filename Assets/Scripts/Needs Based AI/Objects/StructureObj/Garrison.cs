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
        //ReplenishDefenderUnits();
        base.StartDayAction();
        //ArmyTraining();
        //ArmyMobilize();
        //ArmyAttacks();
    }
    #endregion

    //private void ReplenishDefenderUnits() {
    //    if (_objectLocation.defenders == null) {
    //        return;
    //    }
    //    string replenishSummary = GameManager.Instance.TodayLogString() + "Replenishing defender units for " + _objectLocation.locationName + ": ";
    //    for (int j = 0; j < _objectLocation.defenders.icharacters.Count; j++) {
    //        ICharacter currCharacter = _objectLocation.defenders.icharacters[j];
    //        if (currCharacter is CharacterArmyUnit) {
    //            CharacterArmyUnit armyUnit = currCharacter as CharacterArmyUnit;
    //            int productionCost = armyUnit.GetProductionCost();
    //            if (!armyUnit.isCapped() && _objectLocation.tileLocation.areaOfTile.HasEnoughSupplies(productionCost)) {
    //                armyUnit.AdjustArmyCount(1);
    //                _objectLocation.tileLocation.areaOfTile.AdjustSuppliesInBank(-productionCost);
    //                replenishSummary += "\nReplensihed 1 " + armyUnit.characterClass.className + " for " + productionCost;
    //            }
    //        } else if (currCharacter is MonsterArmyUnit) {
    //            MonsterArmyUnit armyUnit = currCharacter as MonsterArmyUnit;
    //            int productionCost = armyUnit.GetProductionCost();
    //            if (_objectLocation.tileLocation.areaOfTile.HasEnoughSupplies(productionCost)) {
    //                armyUnit.AdjustArmyCount(1);
    //                _objectLocation.tileLocation.areaOfTile.AdjustSuppliesInBank(-productionCost);
    //            }
    //        }
    //    }
    //    Debug.Log(replenishSummary);
    //}
    private void ArmyTraining() {
        if(_objectLocation.tileLocation.areaOfTile.suppliesInBank >= 100 && _objectLocation.GetInteractionOfType(INTERACTION_TYPE.ARMY_UNIT_TRAINING) == null) {
            bool hasArmyPartyWithAtLeast3Members = false;
            for (int i = 0; i < _objectLocation.charactersWithHomeOnLandmark.Count; i++) {
                if (_objectLocation.charactersWithHomeOnLandmark[i].currentParty.icharacters.Count >= 3) {
                    hasArmyPartyWithAtLeast3Members = true;
                    break;
                }
            }

            if (hasArmyPartyWithAtLeast3Members) {
                int chance = UnityEngine.Random.Range(0, 200);
                if(chance < 75) {
                    //Trigger Army Training
                    Interaction armyUnitTrainingInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.ARMY_UNIT_TRAINING, _objectLocation);
                    _objectLocation.AddInteraction(armyUnitTrainingInteraction);
                }
            }
        }
    }
    private void ArmyMobilize() {
        if (_objectLocation.GetInteractionOfType(INTERACTION_TYPE.ARMY_MOBILIZATION) == null) {
            int armyUnitCount = 0;
            for (int i = 0; i < _objectLocation.charactersWithHomeOnLandmark.Count; i++) {
                if (_objectLocation.charactersWithHomeOnLandmark[i] is CharacterArmyUnit || _objectLocation.charactersWithHomeOnLandmark[i] is MonsterArmyUnit) {
                    armyUnitCount++;
                    if (armyUnitCount >= 3) {
                        break;
                    }
                }
            }
            if (armyUnitCount >= 3) {
                int chance = UnityEngine.Random.Range(0, 200);
                if (chance < 75) {
                    //Trigger Army Mobilization
                    Interaction armyMobilizationInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.ARMY_MOBILIZATION, _objectLocation);
                    _objectLocation.AddInteraction(armyMobilizationInteraction);
                }
            }
        }
    }
    /*
     Random event that may show up in Garrison tiles. Sends out army units to attack enemy factions. Only triggers if:
    - there is an Army Party residing in the location with at least 3 occupied slots
    - the faction owner is at war with another faction
    - no other active Army Attacks event in the tile

    Trigger rate at the start of each day that conditions are met:
    75 out of 200
         */
    private void ArmyAttacks() {
        if (_objectLocation.GetInteractionOfType(INTERACTION_TYPE.ARMY_ATTACKS) == null && _objectLocation.owner.HasRelationshipStatus(FACTION_RELATIONSHIP_STATUS.AT_WAR)) {
            bool hasArmyPartyWithAtLeast3Members = false;
            for (int i = 0; i < _objectLocation.charactersWithHomeOnLandmark.Count; i++) {
                if (_objectLocation.charactersWithHomeOnLandmark[i].currentParty.icharacters.Count >= 3) {
                    hasArmyPartyWithAtLeast3Members = true;
                    break;
                }
            }

            if (hasArmyPartyWithAtLeast3Members) {
                //int chance = UnityEngine.Random.Range(0, 200);
                //if (chance < 75) {
                    //Trigger Army Attacks
                    Interaction armyAttacksInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.ARMY_ATTACKS, _objectLocation);
                    _objectLocation.AddInteraction(armyAttacksInteraction);
                //}
            }
        }
    }
}
