using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
            if(resident.role.roleType != CHARACTER_ROLE.LEADER && !resident.isDefender && resident.specificLocation.tileLocation.areaOfTile.id == area.id && !resident.currentParty.icon.isTravelling) {
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
            string result = "Success"; // weights.PickRandomElementGivenWeights();
            Interaction interaction = null;
            if (result == "Success") {
                interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.FRIENDLY_CHARACTER_ENCOUNTERED, area.coreTile.landmarkOnTile);
                interaction.AddEndInteractionAction(() => StartJobAction());
                interaction.ScheduleSecondTimeOut();
                chosenCharacter.AddInteraction(interaction);
            } else if (result == "Fail") {
                interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, area.coreTile.landmarkOnTile);
                interaction.AddEndInteractionAction(() => StartJobAction());
                interaction.ScheduleSecondTimeOut();
                _character.specificLocation.tileLocation.landmarkOnTile.AddInteraction(interaction);
            } else if (result == "Crit Fail") {
                interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, area.coreTile.landmarkOnTile);
                interaction.AddEndInteractionAction(() => StartJobAction());
                interaction.ScheduleSecondTimeOut();
                _character.specificLocation.tileLocation.landmarkOnTile.AddInteraction(interaction);
            }
            SetCreatedInteraction(interaction);
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
        List<Interaction> choices = area.GetInteractionsOfJob(_jobType);
        if (choices.Count <= 0) {
            //No interaction for job type
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
            SetJobActionPauseState(true);
            area.SetStopDefaultInteractionsState(true);
            SetCreatedInteraction(choices[UnityEngine.Random.Range(0, choices.Count)]);
            _createdInteraction.AddEndInteractionAction(() => SetJobActionPauseState(false));
            _createdInteraction.AddEndInteractionAction(() => ForceDefaultAllExistingInteractions());
            InteractionUI.Instance.OpenInteractionUI(_createdInteraction);
        } else if (result == "Crit Fail"){
            SetJobActionPauseState(true);
            Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, area.coreTile.landmarkOnTile);
            interaction.AddEndInteractionAction(() => SetJobActionPauseState(false));
            interaction.ScheduleSecondTimeOut();
            _character.specificLocation.tileLocation.landmarkOnTile.AddInteraction(interaction);
            SetCreatedInteraction(interaction);
        }

    }
    public override int GetSuccessRate() {
        int baseRate = 60;
        int multiplier = _character.level - 5;
        if (multiplier < 0) {
            multiplier = 0;
        }
        return baseRate + multiplier;
    }
    #endregion
}
