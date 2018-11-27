using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Instigator : Job {

    public Instigator(Character character) : base(character, JOB.INSTIGATOR) {
        _actionDuration = 120;
        _hasCaptureEvent = true;
    }

    #region Overrides
    public override void DoJobAction() {
        base.DoJobAction();
        Area area = _character.specificLocation.tileLocation.areaOfTile;
        int multiplier = _character.level - 5;
        if (multiplier < 0) {
            multiplier = 0;
        }
        int success = 50 + multiplier;
        int fail = 40;
        int critFail = 12 - (multiplier / 4);
        WeightedDictionary<string> weights = new WeightedDictionary<string>();
        weights.AddElement("Success", success);
        weights.AddElement("Fail", fail);
        weights.AddElement("Crit Fail", critFail);
        string result = weights.PickRandomElementGivenWeights();
        Interaction createdInteraction = null;
        if (result == "Success") {
            //This is only for reference, Change this to Lust - Friendly Character Encountered
            //createdInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.KILLER_ON_THE_LOOSE, area.coreTile.landmarkOnTile);
        } else if (result == "Fail") {
            createdInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, area.coreTile.landmarkOnTile);
        } else if (result == "Crit Fail") {
            createdInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, area.coreTile.landmarkOnTile);
        }
        createdInteraction.SetEndInteractionAction(() => StartJobAction());
        createdInteraction.ScheduleSecondTimeOut();
        _character.specificLocation.tileLocation.landmarkOnTile.AddInteraction(createdInteraction);
    }
    public override void ApplyActionDuration() {
        int multiplier = _character.level - 5;
        if (multiplier < 0) {
            multiplier = 0;
        }
        _actionDuration = 80 - (3 * multiplier);
    }
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
