using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Raider : Job {

    private string _action; //Raid or Scavenge

    #region getters/setters
    public string action {
        get { return _action; }
    }
    #endregion

    public Raider(Character character) : base(character, JOB.RAIDER) {
        _actionDuration = -1;
        _hasCaptureEvent = true;
        //_characterInteractions = new INTERACTION_TYPE[] { INTERACTION_TYPE.MOVE_TO_SCAVENGE };
        _tokenInteractionTypes = new Dictionary<TOKEN_TYPE, INTERACTION_TYPE> {
            {TOKEN_TYPE.CHARACTER, INTERACTION_TYPE.RAIDER_CHARACTER_ENCOUNTER},
            {TOKEN_TYPE.LOCATION, INTERACTION_TYPE.RAIDER_TARGET_LOCATION},
        };
    }

    #region Overrides
    protected override void PassiveEffect(Area area) {
        int takenSupplies = (int)(area.suppliesInBank * 0.25f);
        area.AdjustSuppliesInBank(-takenSupplies);
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, takenSupplies);
    }
    protected override bool IsTokenCompatibleWithJob(Token token) {
        if (token.tokenType == TOKEN_TYPE.CHARACTER) {
            CharacterToken characterToken = token as CharacterToken;
            return characterToken.character.IsInOwnParty() && characterToken.character.doNotDisturb <= 0 && characterToken.character.specificLocation.id == _character.specificLocation.id && !characterToken.character.currentParty.icon.isTravelling;
        } else if (token.tokenType == TOKEN_TYPE.LOCATION) {
            LocationToken locationToken = token as LocationToken;
            //If current area has faction, and target area's faction is different from current area's faction or target area has no faction, and target area is not the current area - return true
            return _character.specificLocation.owner != null
                && (locationToken.location.owner == null || (locationToken.location.owner != null && locationToken.location.owner.id != _character.specificLocation.owner.id))
                && locationToken.location.id != _character.specificLocation.id;
        }
        return base.IsTokenCompatibleWithJob(token);
    }
    public override void DoJobAction() {
        base.DoJobAction();

        int baseSuccessRate = 50;
        int baseFailRate = 40;
        int criticalFailRate = 12;

        //Success Rate +1 per level starting at Level 6
        baseSuccessRate += (Mathf.Max(character.level - 5, 0));
        //Critical Fail Rate -1 per mult of 4 level starting at Level 6
        if (character.level > 6) {
            criticalFailRate -= Mathf.FloorToInt(character.level / 4);
        }

        WeightedDictionary<RESULT> rateWeights = new WeightedDictionary<RESULT>();
        rateWeights.AddElement(RESULT.SUCCESS, baseSuccessRate);
        rateWeights.AddElement(RESULT.FAIL, baseFailRate);
        rateWeights.AddElement(RESULT.CRITICAL_FAIL, criticalFailRate);

        if (character.specificLocation.coreTile.landmarkOnTile.owner == null) {
            _action = "scavenge";
        } else {
            _action = "raid";
        }

        if (rateWeights.GetTotalOfWeights() > 0) {
            RESULT chosenResult = rateWeights.PickRandomElementGivenWeights();
            switch (chosenResult) {
                case RESULT.SUCCESS:
                    RaidSuccess();
                    break;
                case RESULT.FAIL:
                    RaidFail();
                    break;
                case RESULT.CRITICAL_FAIL:
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
    public override int GetSuccessRate() {
        int baseRate = 60;
        int multiplier = _character.level - 5;
        if (multiplier < 0) {
            multiplier = 0;
        }
        return baseRate + multiplier;
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
    #endregion

    private void RaidSuccess() {
        int obtainedSupply = GetSupplyObtained(character.specificLocation);
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.RAID_SUCCESS, character.specificLocation);
        interaction.AddEndInteractionAction(() => GoBackHomeSuccess(obtainedSupply));
        interaction.ScheduleSecondTimeOut();
        interaction.SetOtherData(new object[] { obtainedSupply });
        character.AddInteraction(interaction);
        SetCreatedInteraction(interaction);
        //When a raid succeeds, the target Faction's Favor Count towards the raider is reduced by -2. 
        //FavorEffects(-2);
    }
    private void RaidFail() {
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, character.specificLocation);
        interaction.AddEndInteractionAction(() => GoBackHome());
        interaction.ScheduleSecondTimeOut();
        character.AddInteraction(interaction);
        SetCreatedInteraction(interaction);
        //When a raid fails, the target Faction's Favor Count towards the raider is reduced by -1. The raider will not get anything.
        FavorEffects(-1);
        //GoBackHome();
    }
    private void CriticalRaidFail() {
        //When a raid critically fails, the target Faction's Favor Count towards the raider is reduced by -1. The raider will also perish.
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, character.specificLocation);
        interaction.AddEndInteractionAction(() => GoBackHome());
        interaction.ScheduleSecondTimeOut();
        character.AddInteraction(interaction);
        SetCreatedInteraction(interaction);
        FavorEffects(-1);
        //GoBackHome();
    }

    private void GoBackHome() {
        if (character.minion != null) {
            character.specificLocation.areaInvestigation.RecallMinion("explore");
        } else {
            character.currentParty.GoHome();
        }
        
    }
    private void GoBackHomeSuccess(int supplyObtained) {
        if (character.minion != null) {
            PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, supplyObtained);
            //character.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(supplyObtained);
        } else {
            character.homeArea.AdjustSuppliesInBank(supplyObtained);
        }
        GoBackHome();
    }

    private void FavorEffects(int amount) {
        Area targetArea = character.specificLocation;
        if (targetArea.owner != null) {
            targetArea.owner.AdjustRelationshipFor(character.faction, amount);
        }
    }
}
