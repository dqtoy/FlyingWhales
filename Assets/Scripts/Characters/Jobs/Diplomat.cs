﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Diplomat : Job {

	public Diplomat(Character character) : base(character, JOB.DIPLOMAT) {
        _actionDuration = -1;
        _hasCaptureEvent = true;
    }

    #region Overrides
    public override void CaptureRandomLandmarkEvent() {
        Area area = _character.specificLocation.tileLocation.areaOfTile;
        if (area == null) {
            //Current location has no area
            return;
        }
        WeightedDictionary<string> checkWeights = new WeightedDictionary<string>();
        int checkMultiplier = _character.level - 5;
        if (checkMultiplier < 0) {
            checkMultiplier = 0;
        }
        int check = 30 + (2 * checkMultiplier);
        checkWeights.AddElement("Check", check);
        checkWeights.AddElement("Dont Check", 70);
        string checkResult = checkWeights.PickRandomElementGivenWeights();
        if (checkResult == "Dont Check") {
            return;
        }
        //---------------------------------------- When the result is Check
        WeightedDictionary<string> successWeights = new WeightedDictionary<string>();
        int levelMultiplier = _character.level - 5;
        if (levelMultiplier < 0) {
            levelMultiplier = 0;
        }
        int success = 90 + levelMultiplier;
        int critFail = 12 - (levelMultiplier / 4);
        if (critFail < 0) {
            critFail = 0;
        }
        successWeights.AddElement("Success", success);
        successWeights.AddElement("Crit Fail", critFail);
        string result = successWeights.PickRandomElementGivenWeights();
        Interaction createdInteraction = null;
        if (result == "Success") {
            List<Interaction> choices = area.GetInteractionsOfJob(_jobType);
            if (choices.Count <= 0) {
                //No interaction for job type
                return;
            }
            SetJobActionPauseState(true);
            createdInteraction = choices[UnityEngine.Random.Range(0, choices.Count)];
        } else if (result == "Crit Fail") {
            createdInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, area.coreTile.landmarkOnTile);
        }
        createdInteraction.SetEndInteractionAction(() => SetJobActionPauseState(false));
        createdInteraction.ScheduleSecondTimeOut();
        _character.specificLocation.tileLocation.landmarkOnTile.AddInteraction(createdInteraction);
    }
    #endregion
}
