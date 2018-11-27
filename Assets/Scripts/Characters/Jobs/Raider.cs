﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Raider : Job {

    private string _action; //Raid or Scavenge

    #region getters/setters
    public string action {
        get { return _action; }
    }
    #endregion

    public Raider(Character character) : base(character, JOB.RAIDER) {
        _actionDuration = 50;
        _hasCaptureEvent = false;
    }

    #region Overrides
    public override void DoJobAction() {
        base.DoJobAction();

        int baseSuccessRate = 50;
        int baseFailRate = 40;
        int criticalFailRate = 12;

        //Success Rate +1 per level starting at Level 6
        baseSuccessRate += (character.level - 5);
        //Critical Fail Rate -1 per mult of 4 level starting at Level 6
        if (character.level > 6) {
            criticalFailRate -= Mathf.FloorToInt(character.level / 4);
        }

        WeightedDictionary<JOB_RESULT> rateWeights = new WeightedDictionary<JOB_RESULT>();
        rateWeights.AddElement(JOB_RESULT.SUCCESS, baseSuccessRate);
        rateWeights.AddElement(JOB_RESULT.FAIL, baseFailRate);
        rateWeights.AddElement(JOB_RESULT.CRITICAL_FAIL, criticalFailRate);

        if (rateWeights.GetTotalOfWeights() > 0) {
            JOB_RESULT chosenResult = rateWeights.PickRandomElementGivenWeights();
            switch (chosenResult) {
                case JOB_RESULT.SUCCESS:
                    RaidSuccess();
                    break;
                case JOB_RESULT.FAIL:
                    RaidFail();
                    break;
                case JOB_RESULT.CRITICAL_FAIL:
                    CriticalRaidFail();
                    break;
                default:
                    break;
            }
        } else {
            //go back home
            GoBackHome();
        }
    }
    #endregion

    private void RaidSuccess() {
        Interaction raidSuccess = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.RAID_SUCCESS, character.specificLocation.tileLocation.landmarkOnTile);
        raidSuccess.SetEndInteractionAction(() => GoBackHome());
        raidSuccess.ScheduleSecondTimeOut();
        SupplyObtained();
        //When a raid succeeds, the target Faction's Favor Count towards the raider is reduced by -2. 
        FavorEffects(-2);
    }
    private void RaidFail() {
        //TODO: Shared Fail c/o Chy
        //When a raid fails, the target Faction's Favor Count towards the raider is reduced by -1. The raider will not get anything.
        FavorEffects(-1);
        GoBackHome();
    }
    private void CriticalRaidFail() {
        //TODO: Shared Critical Fail c/o Chy
        //When a raid critically fails, the target Faction's Favor Count towards the raider is reduced by -1. The raider will also perish.
        FavorEffects(-1);
        GoBackHome();
    }

    private void GoBackHome() {
        character.currentParty.GoHome();
    }

    private void SupplyObtained() {
        //When a raid succeeds, the amount of Supply obtained is based on character level.
        //5% to 15% of location's supply 
        //+1% every other level starting at level 6
        Area characterHomeArea = character.homeLandmark.tileLocation.areaOfTile;
        Area targetArea = character.specificLocation.tileLocation.areaOfTile;
        int supplyObtainedPercent = Random.Range(5, 16);
        supplyObtainedPercent += (character.level - 5);

        int obtainedSupply = Mathf.FloorToInt(targetArea.suppliesInBank * (supplyObtainedPercent / 100f));
        characterHomeArea.AdjustSuppliesInBank(obtainedSupply);
    }

    private void FavorEffects(int amount) {
        Area targetArea = character.specificLocation.tileLocation.areaOfTile;
        if (targetArea.owner != null) {
            targetArea.owner.AdjustFavorFor(character.faction, amount);
        }
    }
}