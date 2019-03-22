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
        _actionDuration = -1;
        _hasCaptureEvent = true;
        _tokenInteractionTypes = new Dictionary<TOKEN_TYPE, INTERACTION_TYPE> {
            {TOKEN_TYPE.CHARACTER, INTERACTION_TYPE.INSTIGATOR_CHARACTER_ENCOUNTER},
            {TOKEN_TYPE.LOCATION, INTERACTION_TYPE.INSTIGATOR_TARGET_LOCATION},
            {TOKEN_TYPE.FACTION, INTERACTION_TYPE.INSTIGATOR_FACTION_FRAME_UP},
        };
    }

    #region Overrides
    protected override void PassiveEffect(Area area) {
        int supplies = area.suppliesInBank;
        supplies /= 2;
        //area.SetSuppliesInBank(supplies);
    }
    protected override bool IsTokenCompatibleWithJob(Token token) {
        if(token.tokenType == TOKEN_TYPE.CHARACTER) {
            CharacterToken characterToken = token as CharacterToken;
            return characterToken.character.IsInOwnParty() && characterToken.character.doNotDisturb <= 0 && characterToken.character.specificLocation.id == _character.specificLocation.id && !characterToken.character.currentParty.icon.isTravelling;
        } else if (token.tokenType == TOKEN_TYPE.LOCATION) {
            LocationToken locationToken = token as LocationToken;
            //If target area and current area have factions, and target area's faction is different from current area's faction, and target area is not the current area - return true
            return locationToken.location.owner != null && _character.specificLocation.owner != null
                && locationToken.location.owner.id != _character.specificLocation.owner.id
                && locationToken.location.id != _character.specificLocation.id;
        } else if (token.tokenType == TOKEN_TYPE.FACTION) {
            FactionToken factionToken = token as FactionToken;
            //If current area has owner and target faction is not the current area's owner - return true
            return _character.specificLocation.owner != null
                && factionToken.faction.id != _character.specificLocation.owner.id;
        }
        return base.IsTokenCompatibleWithJob(token);
    }
    public override void CaptureRandomLandmarkEvent() {
        Area area = _character.specificLocation;
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

        SetJobActionPauseState(true);
        area.SetStopDefaultInteractionsState(true);
        if (result == "Success") {
            SetCreatedInteraction(choices[UnityEngine.Random.Range(0, choices.Count)]);
        } else if (result == "Crit Fail") {
            Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, area);
            _character.specificLocation.coreTile.landmarkOnTile.AddInteraction(interaction);
            SetCreatedInteraction(interaction);
        }
        _createdInteraction.AddEndInteractionAction(() => SetJobActionPauseState(false));
        _createdInteraction.AddEndInteractionAction(() => ForceDefaultAllExistingInteractions());
        InteractionUI.Instance.OpenInteractionUI(_createdInteraction);
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
            Area targetArea = character.specificLocation;
            for (int i = 0; i < targetArea.areaResidents.Count; i++) {
                Character resident = targetArea.areaResidents[i];
                if (resident.forcedInteraction == null && resident.doNotDisturb <= 0 && !resident.currentParty.icon.isTravelling &&
                    !resident.alreadyTargetedByGrudge && !resident.isDefender && (resident.race == RACE.HUMANS || resident.race == RACE.ELVES || resident.race == RACE.GOBLIN) 
                    && resident.specificLocation.id == targetArea.id) {
                    resident.SetAlreadyTargetedByGrudge(true);
                    return resident;
                }
            }
        }
        return _character;
    }
}
