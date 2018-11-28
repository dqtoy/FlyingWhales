using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Recruiter : Job {

    public Recruiter(Character character) : base(character, JOB.RECRUITER) {
        _actionDuration = 80;
        _hasCaptureEvent = true;
    }

    #region Overrides
    public override void DoJobAction() {
        base.DoJobAction();
        Area area = _character.specificLocation.tileLocation.areaOfTile;
        List<Character> areaResidents = area.areaResidents;
        Character chosenCharacter = null;
        int success = 0;
        for (int i = 0; i < areaResidents.Count; i++) {
            Character resident = areaResidents[i];
            if(resident != resident.faction.leader) {
                if (resident.isFactionless) {
                    chosenCharacter = resident;
                    success = 30;
                    break;
                } else {
                    FactionRelationship relationship = PlayerManager.Instance.player.playerFaction.GetRelationshipWith(resident.faction);
                    if (relationship != null && relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NON_HOSTILE) {
                        chosenCharacter = resident;
                        success = 10 * PlayerManager.Instance.player.playerFaction.favor[resident.faction];
                        break;
                    }
                }
            }
        }
        if(chosenCharacter != null) {
            int multiplier = _character.level - 5;
            if (multiplier < 0) {
                multiplier = 0;
            }
            success += multiplier;
            int fail = 40;
            int critFail = 12 - (multiplier / 4);
            WeightedDictionary<string> weights = new WeightedDictionary<string>();
            weights.AddElement("Success", success);
            weights.AddElement("Fail", fail);
            weights.AddElement("Crit Fail", critFail);
            string result = weights.PickRandomElementGivenWeights();
            if (result == "Success") {
                //This is only for reference, Change this to Lust - Friendly Character Encountered
                SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.FRIENDLY_CHARACTER_ENCOUNTERED, area.coreTile.landmarkOnTile));
                _createdInteraction.SetEndInteractionAction(() => StartJobAction());
                _createdInteraction.ScheduleSecondTimeOut();
                chosenCharacter.AddInteraction(_createdInteraction);
            } else if (result == "Fail") {
                SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, area.coreTile.landmarkOnTile));
                _createdInteraction.SetEndInteractionAction(() => StartJobAction());
                _createdInteraction.ScheduleSecondTimeOut();
                _character.specificLocation.tileLocation.landmarkOnTile.AddInteraction(_createdInteraction);
            } else if (result == "Crit Fail") {
                SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, area.coreTile.landmarkOnTile));
                _createdInteraction.SetEndInteractionAction(() => StartJobAction());
                _createdInteraction.ScheduleSecondTimeOut();
                _character.specificLocation.tileLocation.landmarkOnTile.AddInteraction(_createdInteraction);
            }
        } else {
            StartJobAction();
        }
    }
    public override void ApplyActionDuration() {
        int multiplier = _character.level - 5;
        if(multiplier < 0) {
            multiplier = 0;
        }
        _actionDuration = 80 - (2 * multiplier);
    }
    public override void CaptureRandomLandmarkEvent() {
        Area area = _character.specificLocation.tileLocation.areaOfTile;
        if(area == null) {
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
        if(critFail < 0) {
            critFail = 0;
        }
        successWeights.AddElement("Success", success);
        successWeights.AddElement("Crit Fail", critFail);
        string result = successWeights.PickRandomElementGivenWeights();
        if (result == "Success") {
            List<Interaction> choices = area.GetInteractionsOfJob(_jobType);
            if (choices.Count <= 0) {
                //No interaction for job type
                return;
            }
            SetJobActionPauseState(true);
            SetCreatedInteraction(choices[UnityEngine.Random.Range(0, choices.Count)]);
        } else if (result == "Crit Fail"){
            SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, area.coreTile.landmarkOnTile));
        }
        _createdInteraction.SetEndInteractionAction(() => SetJobActionPauseState(false));
        _createdInteraction.ScheduleSecondTimeOut();
        _character.specificLocation.tileLocation.landmarkOnTile.AddInteraction(_createdInteraction);
    }
    #endregion
}
