using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Diplomat : Job {

	public Diplomat(Character character) : base(character, JOB.DIPLOMAT) {
        _actionDuration = -1;
        _hasCaptureEvent = true;
        _tokenInteractionTypes = new Dictionary<TOKEN_TYPE, INTERACTION_TYPE> {
            {TOKEN_TYPE.CHARACTER, INTERACTION_TYPE.DIPLOMAT_CHARACTER_ENCOUNTER},
            {TOKEN_TYPE.LOCATION, INTERACTION_TYPE.DIPLOMAT_TARGET_LOCATION},
            {TOKEN_TYPE.FACTION, INTERACTION_TYPE.DIPLOMAT_FACTION_MEDIATION},
        };
    }

    #region Overrides
    protected override void PassiveEffect(Area area) {
        float supplies = area.suppliesInBank * 1.5f;
        area.SetSuppliesInBank((int)supplies);
    }
    protected override bool IsTokenCompatibleWithJob(Token token) {
        if (token.tokenType == TOKEN_TYPE.CHARACTER) {
            CharacterToken characterToken = token as CharacterToken;
            return characterToken.character.specificLocation.tileLocation.areaOfTile.id == _character.specificLocation.tileLocation.areaOfTile.id;
        } else if (token.tokenType == TOKEN_TYPE.LOCATION) {
            LocationToken locationToken = token as LocationToken;
            //If current area has faction, and target area's faction is different from current area's faction or target area has no faction, and target area is not the current area - return true
            return _character.specificLocation.tileLocation.areaOfTile.owner != null
                && locationToken.location.owner == null
                && locationToken.location.id != _character.specificLocation.tileLocation.areaOfTile.id;
        } else if (token.tokenType == TOKEN_TYPE.FACTION) {
            FactionToken factionToken = token as FactionToken;
            //If current area has faction, and target faction is different from current area's faction - return true
            return _character.specificLocation.tileLocation.areaOfTile.owner != null
                && factionToken.faction.id != _character.specificLocation.tileLocation.areaOfTile.owner.id;
        }
        return base.IsTokenCompatibleWithJob(token);
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
}
