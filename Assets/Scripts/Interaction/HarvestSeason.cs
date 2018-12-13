
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestSeason : Interaction {

    private BaseLandmark farm;

    private const string Burn_Farm_Success = "Burn Farm Success";
    private const string Burn_Farm_Fail = "Burn Farm Fail";
    private const string Poison_Crops_Success = "Poison Crops Success";
    private const string Poison_Crops_Fail = "Poison Crops Fail";
    private const string Steal_Crops_Success = "Steal Crops Success";
    private const string Steal_Crops_Fail = "Steal Crops Fail";
    private const string Steal_Crops_Critical_Fail = "Steal Crops Critical Fail";
    private const string Do_Nothing = "Do nothing";

    public HarvestSeason(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.HARVEST_SEASON, 70) {
        _name = "Harvest Season";
    }

    #region Overrides
    public override void CreateStates() {
        farm = interactable;

        InteractionState startState = new InteractionState("Start", this);
        InteractionState burnFarmSuccess = new InteractionState(Burn_Farm_Success, this);
        InteractionState burnFarmFail = new InteractionState(Burn_Farm_Fail, this);
        InteractionState poisonCropsSuccess = new InteractionState(Poison_Crops_Success, this); 
        InteractionState poisonCropsFail = new InteractionState(Poison_Crops_Fail, this);
        InteractionState stealCropsSuccess = new InteractionState(Steal_Crops_Success, this);
        InteractionState stealCropsFail = new InteractionState(Steal_Crops_Fail, this);
        InteractionState stealCropsCriticalFail = new InteractionState(Steal_Crops_Critical_Fail, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);

        burnFarmSuccess.SetEffect(() => BurnFarmSuccessRewardEffect(burnFarmSuccess));
        burnFarmFail.SetEffect(() => BurnFarmFailRewardEffect(burnFarmFail));
        poisonCropsSuccess.SetEffect(() => PoisonCropsSuccessRewardEffect(poisonCropsSuccess));
        poisonCropsFail.SetEffect(() => PoisonCropsFailRewardEffect(poisonCropsFail));
        stealCropsSuccess.SetEffect(() => StealCropsSuccessRewardEffect(stealCropsSuccess));
        stealCropsFail.SetEffect(() => StealCropsFailRewardEffect(stealCropsFail));
        stealCropsCriticalFail.SetEffect(() => StealCropsCriticalFailRewardEffect(stealCropsCriticalFail));
        doNothingState.SetEffect(() => DoNothingRewardEffect(doNothingState));
        

        _states.Add(startState.name, startState);
        _states.Add(burnFarmSuccess.name, burnFarmSuccess);
        _states.Add(burnFarmFail.name, burnFarmFail);
        _states.Add(poisonCropsSuccess.name, poisonCropsSuccess);
        _states.Add(poisonCropsFail.name, poisonCropsFail);
        _states.Add(stealCropsSuccess.name, stealCropsSuccess);
        _states.Add(stealCropsFail.name, stealCropsFail);
        _states.Add(stealCropsCriticalFail.name, stealCropsCriticalFail);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption burn = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Burn as much farmland as possible.",
                effect = () => BurnOptionEffect(state),
            };
            ActionOption poison = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Poison the harvested crops.",
                effect = () => PoisonOptionEffect(state),
            };
            ActionOption steal = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Steal the harvested crops.",
                effect = () => StealOptionEffect(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOptionEffect(state),
            };

            state.AddActionOption(burn);
            state.AddActionOption(poison);
            state.AddActionOption(steal);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Action Option Effects
    private void BurnOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = explorerMinion.character.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);
        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Burn_Farm_Success;
                break;
            case RESULT.FAIL:
                nextState = Burn_Farm_Fail;
                break;
            default:
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void PoisonOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = explorerMinion.character.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);
        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Poison_Crops_Success;
                break;
            case RESULT.FAIL:
                nextState = Poison_Crops_Fail;
                break;
            default:
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void StealOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = explorerMinion.character.job.GetJobRateWeights();
        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Steal_Crops_Success;
                break;
            case RESULT.FAIL:
                nextState = Steal_Crops_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Steal_Crops_Critical_Fail;
                break;
            default:
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states["Do nothing"]);
    }
    #endregion

    #region Reward Effects
    private void BurnFarmSuccessRewardEffect(InteractionState state) {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddMonths(5);
        //farm.DisableSupplyProductionUntil(dueDate);
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    private void BurnFarmFailRewardEffect(InteractionState state) {
        //**Effect**: Kill a random Farmer staying at that farm, City gains Supply Cache 1
        List<Character> farmers = farm.tileLocation.areaOfTile.GetResidentsWithClass("Farmer");
        Character chosenFarmer = farmers[Random.Range(0, farmers.Count)];
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(chosenFarmer, chosenFarmer.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(chosenFarmer, chosenFarmer.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        chosenFarmer.Death();
        farm.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    private void PoisonCropsSuccessRewardEffect(InteractionState state) {
        //**Reward**: Supply Cache 1, Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        Reward reward = InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1);
        PlayerManager.Instance.player.ClaimReward(reward);
        farm.tileLocation.areaOfTile.PayForReward(reward);
    }
    private void PoisonCropsFailRewardEffect(InteractionState state) {
        //**Effect**: Faction declares war vs player, City gains Supply Cache 1
        FactionManager.Instance.DeclareWarBetween(farm.tileLocation.areaOfTile.owner, PlayerManager.Instance.player.playerFaction);
        farm.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1)); //**Reward**: Demon gains Exp 1
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(farm.tileLocation.areaOfTile.owner, farm.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(farm.tileLocation.areaOfTile.owner, farm.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void StealCropsSuccessRewardEffect(InteractionState state) {
        //City gains Supply Cache 1
        farm.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
        //**Effect**: Demon is removed from Minion List
        PlayerManager.Instance.player.RemoveMinion(explorerMinion);
    }
    private void StealCropsFailRewardEffect(InteractionState state) {
        //City gains Supply Cache 1
        farm.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
        //**Effect**: Demon is removed from Minion List
        PlayerManager.Instance.player.RemoveMinion(explorerMinion);
    }
    private void StealCropsCriticalFailRewardEffect(InteractionState state) {
        //City gains Supply Cache 1
        farm.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
        //**Effect**: Demon is removed from Minion List
        PlayerManager.Instance.player.RemoveMinion(explorerMinion);
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //**Effect**: City gains Supply Cache 1
        farm.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
    }
    #endregion
}
