using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Instigator : Job {

    INTERACTION_TYPE[] chaosEvents = new INTERACTION_TYPE[] { //TODO: Put this somwhere else
        INTERACTION_TYPE.INDUCE_WAR,
        INTERACTION_TYPE.INDUCE_GRUDGE,
        INTERACTION_TYPE.INFLICT_ILLNESS,
    };

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
        //weights.AddElement("Fail", fail);
        //weights.AddElement("Crit Fail", critFail);
        string result = weights.PickRandomElementGivenWeights();
        if (result == "Success") {
            List<INTERACTION_TYPE> choices = GetValidChaosEvents();
            if (choices.Count > 0) {
                INTERACTION_TYPE chosenType = choices[Random.Range(0, choices.Count)];
                //Get Random Chaos Event
                Interaction interaction = InteractionManager.Instance.CreateNewInteraction(chosenType, area.coreTile.landmarkOnTile);
                //SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.SPAWN_NEUTRAL_CHARACTER, area.coreTile.landmarkOnTile)); //NOT FINAL!
                interaction.AddEndInteractionAction(() => StartJobAction());
                interaction.ScheduleSecondTimeOut();
                Character targetCharacter = GetTargetCharacter(chosenType);
                targetCharacter.AddInteraction(interaction);
                SetCreatedInteraction(interaction);
            } else {
                StartJobAction();
            }
            return;
        } else if (result == "Fail") {
            Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, area.coreTile.landmarkOnTile);
            interaction.AddEndInteractionAction(() => StartJobAction());
            interaction.ScheduleSecondTimeOut();
            _character.specificLocation.tileLocation.landmarkOnTile.AddInteraction(interaction);
            SetCreatedInteraction(interaction);
        } else if (result == "Crit Fail") {
            Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, area.coreTile.landmarkOnTile);
            interaction.AddEndInteractionAction(() => StartJobAction());
            interaction.ScheduleSecondTimeOut();
            _character.specificLocation.tileLocation.landmarkOnTile.AddInteraction(interaction);
            SetCreatedInteraction(interaction);
        }

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
        //checkWeights.AddElement("Dont Check", 70);
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
        if (result == "Success") {
            SetJobActionPauseState(true);
            area.SetStopDefaultInteractionsState(true);
            SetCreatedInteraction(choices[UnityEngine.Random.Range(0, choices.Count)]);
            _createdInteraction.AddEndInteractionAction(() => SetJobActionPauseState(false));
            _createdInteraction.AddEndInteractionAction(() => ForceDefaultAllExistingInteractions());
            InteractionUI.Instance.OpenInteractionUI(_createdInteraction);
        } else if (result == "Crit Fail") {
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

    private List<INTERACTION_TYPE> GetValidChaosEvents() {
        List<INTERACTION_TYPE> validTypes = new List<INTERACTION_TYPE>();
        for (int i = 0; i < chaosEvents.Length; i++) {
            INTERACTION_TYPE type = chaosEvents[i];
            if (InteractionManager.Instance.CanCreateInteraction(type, _character)) {
                validTypes.Add(type);
            }
        }
        return validTypes;
    }
    private Character GetTargetCharacter(INTERACTION_TYPE type) {
        if(type == INTERACTION_TYPE.INDUCE_GRUDGE) {
            Area targetArea = character.specificLocation.tileLocation.areaOfTile;
            for (int i = 0; i < targetArea.areaResidents.Count; i++) {
                Character resident = targetArea.areaResidents[i];
                if (!resident.alreadyTargetedByGrudge && !resident.isDefender && (resident.race == RACE.HUMANS || resident.race == RACE.ELVES || resident.race == RACE.GOBLIN) && resident.specificLocation.tileLocation.areaOfTile.id == targetArea.id) {
                    resident.SetAlreadyTargetedByGrudge(true);
                    return resident;
                }
            }
        }
        return _character;
    }
}
