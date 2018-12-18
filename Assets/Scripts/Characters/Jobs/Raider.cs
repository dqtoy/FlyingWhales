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
        _actionDuration = 50;
        _hasCaptureEvent = false;
        //_characterInteractions = new INTERACTION_TYPE[] { INTERACTION_TYPE.MOVE_TO_SCAVENGE };
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
            return characterToken.character.specificLocation.tileLocation.areaOfTile.id == _character.specificLocation.tileLocation.areaOfTile.id;
        } else if (token.tokenType == TOKEN_TYPE.LOCATION) {
            LocationToken locationToken = token as LocationToken;
            //If current area has factions, and target area's faction is different from current area's faction or target area has no faction, and target area is not the current area - return true
            return _character.specificLocation.tileLocation.areaOfTile.owner != null
                && (locationToken.location.owner == null || (locationToken.location.owner != null && locationToken.location.owner.id != _character.specificLocation.tileLocation.areaOfTile.owner.id))
                && locationToken.location.id != _character.specificLocation.tileLocation.areaOfTile.id;
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

        if (character.specificLocation.tileLocation.landmarkOnTile.owner == null) {
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
    #endregion

    private void RaidSuccess() {
        int obtainedSupply = GetSupplyObtained(character.specificLocation.tileLocation.areaOfTile);
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.RAID_SUCCESS, character.specificLocation.tileLocation.landmarkOnTile);
        interaction.AddEndInteractionAction(() => GoBackHomeSuccess(obtainedSupply));
        interaction.ScheduleSecondTimeOut();
        interaction.SetOtherData(new object[] { obtainedSupply });
        character.AddInteraction(interaction);
        SetCreatedInteraction(interaction);
        //When a raid succeeds, the target Faction's Favor Count towards the raider is reduced by -2. 
        //FavorEffects(-2);
    }
    private void RaidFail() {
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, character.specificLocation.tileLocation.landmarkOnTile);
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
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, character.specificLocation.tileLocation.landmarkOnTile);
        interaction.AddEndInteractionAction(() => GoBackHome());
        interaction.ScheduleSecondTimeOut();
        character.AddInteraction(interaction);
        SetCreatedInteraction(interaction);
        FavorEffects(-1);
        //GoBackHome();
    }

    private void GoBackHome() {
        if (character.minion != null) {
            character.specificLocation.tileLocation.areaOfTile.areaInvestigation.RecallMinion("explore");
        } else {
            character.currentParty.GoHome();
        }
        
    }
    private void GoBackHomeSuccess(int supplyObtained) {
        if (character.minion != null) {
            PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, supplyObtained);
            //character.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(supplyObtained);
        } else {
            character.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(supplyObtained);
        }
        GoBackHome();
    }

    private void FavorEffects(int amount) {
        Area targetArea = character.specificLocation.tileLocation.areaOfTile;
        if (targetArea.owner != null) {
            targetArea.owner.AdjustRelationshipFor(character.faction, amount);
        }
    }
}
