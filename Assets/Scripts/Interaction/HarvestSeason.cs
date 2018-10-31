using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestSeason : Interaction {

    private BaseLandmark farm;

    public HarvestSeason(IInteractable interactable) : base(interactable, INTERACTION_TYPE.HARVEST_SEASON, 70) {
        _name = "Harvest Season";
    }

    #region Overrides
    public override void CreateStates() {
        if (_interactable is BaseLandmark) {
            CreateExploreStates();
            farm = interactable as BaseLandmark;

            InteractionState startState = new InteractionState("State 1", this);
            string startStateDesc = _interactable.explorerMinion.name + " has reported that the farmers will soon be able to harvest their crops. A sizable amount of the harvest will be given to their troops, providing them with needed Supplies.";
            startState.SetDescription(startStateDesc);
            CreateActionOptions(startState);

            //action option states
            InteractionState poisonedHarvestState = new InteractionState("Poisoned Harvest", this);
            InteractionState farmerKilledState = new InteractionState("Farmer Killed", this);
            InteractionState obtainHarvestState = new InteractionState("Obtain Harvest", this); 
            InteractionState demonDiscoveredState = new InteractionState("Demon Discovered", this);
            InteractionState demonKilledState = new InteractionState("Demon Killed", this);
            CreateWhatToDoNextState("What do you want " + _interactable.explorerMinion.name + " to do next?");

            CreateActionOptions(poisonedHarvestState);
            CreateActionOptions(farmerKilledState);
            CreateActionOptions(obtainHarvestState);
            CreateActionOptions(demonDiscoveredState);

            //farmerKilledState.SetEndEffect(() => FarmerKilledEffect(farmerKilledState));
            //obtainHarvestState.SetEndEffect(() => ObtainHarvestEffect(obtainHarvestState));
            //demonDiscoveredState.SetEndEffect(() => DemonDiscoveredEffect(demonDiscoveredState));
            demonKilledState.SetEndEffect(() => DemonKilledRewardEffect(demonKilledState));

            _states.Add(startState.name, startState);
            _states.Add(poisonedHarvestState.name, poisonedHarvestState);
            _states.Add(farmerKilledState.name, farmerKilledState);
            _states.Add(obtainHarvestState.name, obtainHarvestState);
            _states.Add(demonDiscoveredState.name, demonDiscoveredState);
            _states.Add(demonKilledState.name, demonKilledState);

            SetCurrentState(startState);
        }
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "State 1") {
            ActionOption sendOutDemon = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 30, currency = CURRENCY.SUPPLY },
                name = "Send out a Demon to disrupt the harvest.",
                description = "We have sent %minion% to disrupt the harvest. It should take him a short time to execute the task.",
                duration = 0,
                needsMinion = false,
                effect = () => SendOutDemonEffect(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                name = "Do nothing.",
                duration = 0,
                needsMinion = false,
                effect = () => WhatToDoNextState(),
            };

            state.AddActionOption(sendOutDemon);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        } else {
            ActionOption continueSurveillance = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Continue surveillance of the area.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreContinuesOption(state),
            };
            ActionOption returnToMe = new ActionOption {
                interactionState = state,
                name = "Return to me.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreEndsOption(state),
            };
            state.AddActionOption(continueSurveillance);
            state.AddActionOption(returnToMe);
        }
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
        if (chosenEffect == "Poisoned Harvest") {
            PoisonedHarvest(state, chosenEffect);
        } else if (chosenEffect == "Farmer Killed") {
            FarmerKilled(state, chosenEffect);
        } else if (chosenEffect == "Obtain Harvest") {
            ObtainHarvest(state, chosenEffect);
        } else if (chosenEffect == "Demon Discovered") {
            DemonDiscovered(state, chosenEffect);
        } else if (chosenEffect == "Demon Killed") {
            DemonKilled(state, chosenEffect);
        }
    }
    #endregion

    #region Poisoned Harvest
    private void PoisonedHarvest(InteractionState state, string effectName) {
        _states[effectName]
            .SetDescription("After a significant amount of stealthy effort, " + _interactable.explorerMinion.name + 
            " managed to secretly poison the crops. The farmers will not be able to provide extra Supply to the city. " +
            "Furthermore, the poison has rendered the soil toxic, preventing the Farm from producing more Supplies for 5 days. " +
            "What do you want " + _interactable.explorerMinion.name + " to do next?");
        SetCurrentState(_states[effectName]);
        //Farm stops producing Supply for 5 days
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(5);
        farm.DisableSupplyProductionUntil(dueDate);
        PoisonedHarvestRewardEffect(_states[effectName]);
    }
    private void PoisonedHarvestRewardEffect(InteractionState state) {
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    #endregion

    #region Farmer Killed
    private void FarmerKilled(InteractionState state, string effectName) {
        List<ICharacter> farmers = farm.tileLocation.areaOfTile.GetResidentsWithClass("Farmer");
        ICharacter chosenFarmer = farmers[Random.Range(0, farmers.Count)];
        _states[effectName].SetDescription(_interactable.explorerMinion.name + " entered the farm at night and was about to poison " +
            "the crops when a farmer named " + chosenFarmer.name + " discovered him. He managed to slay the farmer before being forced to flee. " +
            "What do you want him to do next?");
        SetCurrentState(_states[effectName]);
        chosenFarmer.Death();
        FarmerKilledRewardEffect(_states[effectName]);
    }
    private void FarmerKilledRewardEffect(InteractionState state) {
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    #endregion

    #region Obtain Harvest
    private void ObtainHarvest(InteractionState state, string effectName) {
        _states[effectName].SetDescription(_interactable.explorerMinion.name + " stole the harvest in the dead of night, " +
            "providing us with much needed Supply. What do you want him to do next?");
        SetCurrentState(_states[effectName]);
        ObtainHarvestRewardEffect(_states[effectName]);
    }
    private void ObtainHarvestRewardEffect(InteractionState state) {
        //**Reward**: Supply Cache 1, Demon gains Exp 1
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        Reward reward = InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1);
        PlayerManager.Instance.player.ClaimReward(reward);
        farm.tileLocation.areaOfTile.PayForReward(reward);
    }
    #endregion

    #region Demon Discovered
    private void DemonDiscovered(InteractionState state, string effectName) {
        _states[effectName].SetDescription(_interactable.explorerMinion.name + " was discovered by some farmers! " +
            "He managed to run away unscathed but " + farm.tileLocation.areaOfTile.owner.name + " is now aware of our sabotage " +
            "attempts and have declared war upon us. What do you want him to do next?");
        SetCurrentState(_states[effectName]);
        //**Effect**: Faction declares war vs player
        FactionManager.Instance.DeclareWarBetween(farm.tileLocation.areaOfTile.owner, PlayerManager.Instance.player.playerFaction);
        DemonDiscoveredRewardEffect(_states[effectName]);
    }
    private void DemonDiscoveredRewardEffect(InteractionState state) {
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    #endregion

    #region Demon Killed
    private void DemonKilled(InteractionState state, string effectName) {
        _states[effectName].SetDescription(_interactable.explorerMinion.name + " was caught by some guards and was slain in combat. What a weakling. He deserved that.");
        SetCurrentState(_states[effectName]);
        DemonKilledRewardEffect(_states[effectName]);
    }
    private void DemonKilledRewardEffect(InteractionState state) {
        //**Effect**: Demon is removed from Minion List
        PlayerManager.Instance.player.RemoveMinion(_interactable.explorerMinion);
    }
    #endregion

}
