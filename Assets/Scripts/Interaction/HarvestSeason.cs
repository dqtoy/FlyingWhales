using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestSeason : Interaction {

    private BaseLandmark farm;

    public HarvestSeason(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.HARVEST_SEASON, 70) {
        _name = "Harvest Season";
    }

    #region Overrides
    public override void CreateStates() {
        //CreateExploreStates();
        //CreateWhatToDoNextState("What do you want %minion% to do next?");
        farm = interactable;

        InteractionState startState = new InteractionState("Start", this);
        //string startStateDesc = "%minion% has reported that the farmers will soon be able to harvest their crops. A sizable amount of the harvest will be given to their troops, providing them with needed Supplies.";
        //startState.SetDescription(startStateDesc);
        CreateActionOptions(startState);

        //action option states
        InteractionState poisonedHarvestState = new InteractionState("Poisoned Harvest", this);
        InteractionState farmerKilledState = new InteractionState("Farmer Killed", this);
        InteractionState obtainHarvestState = new InteractionState("Obtain Harvest", this); 
        InteractionState demonDiscoveredState = new InteractionState("Demon Discovered", this);
        InteractionState demonKilledState = new InteractionState("Demon Killed", this);
        InteractionState doNothingState = new InteractionState("Do nothing", this);


        //CreateActionOptions(poisonedHarvestState);
        //CreateActionOptions(farmerKilledState);
        //CreateActionOptions(obtainHarvestState);
        //CreateActionOptions(demonDiscoveredState);

        farmerKilledState.SetEndEffect(() => FarmerKilledRewardEffect(farmerKilledState));
        obtainHarvestState.SetEndEffect(() => ObtainHarvestRewardEffect(obtainHarvestState));
        demonDiscoveredState.SetEndEffect(() => DemonDiscoveredRewardEffect(demonDiscoveredState));
        demonKilledState.SetEndEffect(() => DemonKilledRewardEffect(demonKilledState));
        doNothingState.SetEndEffect(() => DoNothingRewardEffect(doNothingState));
        poisonedHarvestState.SetEndEffect(() => PoisonedHarvestRewardEffect(poisonedHarvestState));

        _states.Add(startState.name, startState);
        _states.Add(poisonedHarvestState.name, poisonedHarvestState);
        _states.Add(farmerKilledState.name, farmerKilledState);
        _states.Add(obtainHarvestState.name, obtainHarvestState);
        _states.Add(demonDiscoveredState.name, demonDiscoveredState);
        _states.Add(demonKilledState.name, demonKilledState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption sendOutDemon = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 30, currency = CURRENCY.SUPPLY },
                name = "Send out a Demon to disrupt the harvest.",
                //description = "We have sent %minion% to disrupt the harvest. It should take him a short time to execute the task.",
                duration = 0,
                needsMinion = false,
                effect = () => SendOutDemonEffect(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                name = "Do nothing.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingEffect(state),
            };

            state.AddActionOption(sendOutDemon);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        } 
        //else {
        //    ActionOption continueSurveillance = new ActionOption {
        //        interactionState = state,
        //        cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
        //        name = "Continue surveillance of the area.",
        //        duration = 0,
        //        needsMinion = false,
        //        effect = () => ExploreContinuesOption(state),
        //    };
        //    ActionOption returnToMe = new ActionOption {
        //        interactionState = state,
        //        name = "Return to me.",
        //        duration = 0,
        //        needsMinion = false,
        //        effect = () => ExploreEndsOption(state),
        //    };
        //    state.AddActionOption(continueSurveillance);
        //    state.AddActionOption(returnToMe);
        //    state.SetDefaultOption(returnToMe);
        //}
    }
    #endregion

    #region Action Option Effects
    private void SendOutDemonEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Poisoned Harvest", 20);
        if (farm.tileLocation.areaOfTile.HasResidentWithClass("Farmer")) {
            //**Requirement**: City that farm is a part of must have a farmer
            effectWeights.AddElement("Farmer Killed", 10);
        }
        effectWeights.AddElement("Obtain Harvest", 10);
        if (FactionManager.Instance.GetRelationshipStatusBetween(farm.tileLocation.areaOfTile.owner,
            PlayerManager.Instance.player.playerFaction) != FACTION_RELATIONSHIP_STATUS.AT_WAR) {
            //**Requirement**: Tile Owner Faction must not be at war with the player
            effectWeights.AddElement("Demon Discovered", 5);
        }

        effectWeights.AddElement("Demon Killed", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states["Do nothing"]);
    }
    #endregion

    #region Poisoned Harvest
    //private void PoisonedHarvest(InteractionState state, string effectName) {
    //    //_states[effectName]
    //    //    .SetDescription("After a significant amount of stealthy effort, " + explorerMinion.name + 
    //    //    " managed to secretly poison the crops. The farmers will not be able to provide extra Supply to the city. " +
    //    //    "Furthermore, the poison has rendered the soil toxic, preventing the Farm from producing more Supplies for 5 days. " +
    //    //    "What do you want " + explorerMinion.name + " to do next?");
    //    SetCurrentState(_states[effectName]);
    //    //Farm stops producing Supply for 5 days
    //    //GameDate dueDate = GameManager.Instance.Today();
    //    //dueDate.AddDays(5);
    //    //farm.DisableSupplyProductionUntil(dueDate);
    //    PoisonedHarvestRewardEffect(_states[effectName]);
    //}
    private void PoisonedHarvestRewardEffect(InteractionState state) {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(5);
        farm.DisableSupplyProductionUntil(dueDate);
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    #endregion

    #region Farmer Killed
    //private void FarmerKilled(InteractionState state, string effectName) {
    //    //**Effect**: Kill a random Farmer staying at that farm, City gains Supply Cache 1
    //    //List<ICharacter> farmers = farm.tileLocation.areaOfTile.GetResidentsWithClass("Farmer");
    //    //ICharacter chosenFarmer = farmers[Random.Range(0, farmers.Count)];
    //    //_states[effectName].SetDescription(explorerMinion.name + " entered the farm at night and was about to poison " +
    //    //    "the crops when a farmer named " + chosenFarmer.name + " discovered him. He managed to slay the farmer before being forced to flee. " +
    //    //    "What do you want him to do next?");
    //    SetCurrentState(_states[effectName]);
    //    //chosenFarmer.Death();
    //    //farm.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
    //    FarmerKilledRewardEffect(_states[effectName]);
    //}
    private void FarmerKilledRewardEffect(InteractionState state) {
        //**Effect**: Kill a random Farmer staying at that farm, City gains Supply Cache 1
        List<ICharacter> farmers = farm.tileLocation.areaOfTile.GetResidentsWithClass("Farmer");
        ICharacter chosenFarmer = farmers[Random.Range(0, farmers.Count)];
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(chosenFarmer, chosenFarmer.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(chosenFarmer, chosenFarmer.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        chosenFarmer.Death();
        farm.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    #endregion

    #region Obtain Harvest
    //private void ObtainHarvest(InteractionState state, string effectName) {
    //    //_states[effectName].SetDescription(explorerMinion.name + " stole the harvest in the dead of night, " +
    //    //    "providing us with much needed Supply. What do you want him to do next?");
    //    SetCurrentState(_states[effectName]);
    //    ObtainHarvestRewardEffect(_states[effectName]);
    //}
    private void ObtainHarvestRewardEffect(InteractionState state) {
        //**Reward**: Supply Cache 1, Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        Reward reward = InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1);
        PlayerManager.Instance.player.ClaimReward(reward);
        farm.tileLocation.areaOfTile.PayForReward(reward);
    }
    #endregion

    #region Demon Discovered
    //private void DemonDiscovered(InteractionState state, string effectName) {
    //    //_states[effectName].SetDescription(explorerMinion.name + " was discovered by some farmers! " +
    //    //    "He managed to run away unscathed but " + farm.tileLocation.areaOfTile.owner.name + " is now aware of our sabotage " +
    //    //    "attempts and have declared war upon us. What do you want him to do next?");
    //    SetCurrentState(_states[effectName]);
    //    ////**Effect**: Faction declares war vs player, City gains Supply Cache 1
    //    //FactionManager.Instance.DeclareWarBetween(farm.tileLocation.areaOfTile.owner, PlayerManager.Instance.player.playerFaction);
    //    //farm.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
    //    DemonDiscoveredRewardEffect(_states[effectName]);
    //}
    private void DemonDiscoveredRewardEffect(InteractionState state) {
        //**Effect**: Faction declares war vs player, City gains Supply Cache 1
        FactionManager.Instance.DeclareWarBetween(farm.tileLocation.areaOfTile.owner, PlayerManager.Instance.player.playerFaction);
        farm.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(farm.tileLocation.areaOfTile.owner, farm.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(farm.tileLocation.areaOfTile.owner, farm.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
    }
    #endregion

    #region Demon Killed
    //private void DemonKilled(InteractionState state, string effectName) {
    //    //_states[effectName].SetDescription(explorerMinion.name + " was caught by some guards and was slain in combat. What a weakling. He deserved that.");
    //    SetCurrentState(_states[effectName]);
    //    DemonKilledRewardEffect(_states[effectName]);
    //}
    private void DemonKilledRewardEffect(InteractionState state) {
        //City gains Supply Cache 1
        farm.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
        //**Effect**: Demon is removed from Minion List
        PlayerManager.Instance.player.RemoveMinion(explorerMinion);
    }
    #endregion

    #region Do Nothing
    //private void DoNothing(InteractionState state, string effectName) {
    //    SetCurrentState(_states[effectName]);
    //    DoNothingRewardEffect(_states[effectName]);
    //}
    private void DoNothingRewardEffect(InteractionState state) {
        //**Effect**: City gains Supply Cache 1
        farm.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
    }
    #endregion

}
