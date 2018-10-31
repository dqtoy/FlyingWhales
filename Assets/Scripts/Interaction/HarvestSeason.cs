using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestSeason : Interaction {

    private BaseLandmark farm;

    public HarvestSeason(IInteractable interactable) : base(interactable, INTERACTION_TYPE.HARVEST_SEASON) {
        _name = "Harvest Season";
    }

    #region Overrides
    public override void CreateStates() {
        if (_interactable is BaseLandmark) {
            farm = interactable as BaseLandmark;

            InteractionState startState = new InteractionState("State 1", this);
            string startStateDesc = "%minion% has reported that the farmers will soon be able to harvest their crops. A sizable amount of the harvest will be given to their troops, providing them with needed Supplies.";
            startState.SetDescription(startStateDesc);
            CreateActionOptions(startState);

            //action option states
            InteractionState poisonedHarvestState = new InteractionState("Poisoned Harvest", this);
            InteractionState farmerKilledState = new InteractionState("Farmer Killed", this);
            InteractionState obtainHarvestState = new InteractionState("Obtain Harvest", this); 
            InteractionState demonDiscoveredState = new InteractionState("Demon Discovered", this);
            InteractionState demonKilledState = new InteractionState("Demon Killed", this);
            InteractionState whatToDoNextState = new InteractionState("What To Do Next", this);
            InteractionState exploreContinuesState = new InteractionState("Explore Continues", this);
            InteractionState exploreEndsState = new InteractionState("Explore Ends", this);


            //poisonedHarvestState.SetEndEffect(() => PoisonedHarvestEffect(poisonedHarvestState));
            farmerKilledState.SetEndEffect(() => FarmerKilledEffect(farmerKilledState));
            obtainHarvestState.SetEndEffect(() => ObtainHarvestEffect(obtainHarvestState));
            demonDiscoveredState.SetEndEffect(() => DemonDiscoveredEffect(demonDiscoveredState));
            demonKilledState.SetEndEffect(() => DemonKilledEffect(demonKilledState));
            exploreContinuesState.SetEndEffect(() => ExploreContinuesEffect(exploreContinuesState));
            exploreEndsState.SetEndEffect(() => ExploreEndsEffect(exploreEndsState));
            //whatToDoNextState.SetEndEffect(() => WhatToDoNextEffect(demonKilledState));


            _states.Add(startState.name, startState);
            _states.Add(poisonedHarvestState.name, poisonedHarvestState);
            _states.Add(farmerKilledState.name, farmerKilledState);
            _states.Add(obtainHarvestState.name, obtainHarvestState);
            _states.Add(demonDiscoveredState.name, demonDiscoveredState);
            _states.Add(demonKilledState.name, demonKilledState);
            _states.Add(whatToDoNextState.name, whatToDoNextState);
            _states.Add(exploreContinuesState.name, exploreContinuesState);
            _states.Add(exploreEndsState.name, exploreEndsState);

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
                duration = 10,
                needsMinion = true,
                effect = () => SendOutDemonEffect(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                name = "Do nothing.",
                duration = 10,
                needsMinion = false,
                effect = () => DoNothingEffect(state),
            };

            state.AddActionOption(sendOutDemon);
            state.AddActionOption(doNothing);

            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddHours(70);
            state.SetTimeSchedule(state.actionOptions[1], dueDate); //default is do nothing
        } else if (state.name == "Poisoned Harvest" || state.name == "Farmer Killed" || state.name == "Obtain Harvest" ||
            state.name == "Demon Discovered" || state.name == "What To Do Next") {
            ActionOption continueSurveillance = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Continue surveillance of the area.",
                duration = 10,
                needsMinion = true,
                neededObjects = new List<System.Type>() { typeof(Minion) },
                effect = () => ContinueSurveillanceEffect(state),
            };
            ActionOption returnToMe = new ActionOption {
                interactionState = state,
                name = "Return to me.",
                duration = 10,
                needsMinion = false,
                effect = () => ReturnToMeEffect(state),
            };
            state.AddActionOption(continueSurveillance);
            state.AddActionOption(returnToMe);
            if (state.name == "What To Do Next") {
                state.SetDefaultOption(returnToMe); //default is do nothing
            }
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
    private void DoNothingEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("What To Do Next", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "What To Do Next") {
            WhatToDoNext(state, chosenEffect);
        }
    }
    private void ContinueSurveillanceEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Explore Continues", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Explore Continues") {
            ExploreContinues(state, chosenEffect);
        }
    }
    private void ReturnToMeEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Explore Ends", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Explore Ends") {
            ExploreEnds(state, chosenEffect);
        }
    }
    #endregion

    #region Poisoned Harvest
    private void PoisonedHarvest(InteractionState state, string effectName) {
        _states[effectName]
            .SetDescription("After a significant amount of stealthy effort, " + state.chosenOption.assignedMinion.name + 
            " managed to secretly poison the crops. The farmers will not be able to provide extra Supply to the city. " +
            "Furthermore, the poison has rendered the soil toxic, preventing the Farm from producing more Supplies for 5 days. " +
            "What do you want " + state.chosenOption.assignedMinion.name + " to do next?");
        SetCurrentState(_states[effectName]);
        //Farm stops producing Supply for 5 days
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(5);
        farm.DisableSupplyProductionUntil(dueDate);
    }
    private void PoisonedHarvestEffect(InteractionState state) {
        state.assignedMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    #endregion

    #region Farmer Killed
    private void FarmerKilled(InteractionState state, string effectName) {
        List<ICharacter> farmers = farm.tileLocation.areaOfTile.GetResidentsWithClass("Farmer");
        ICharacter chosenFarmer = farmers[Random.Range(0, farmers.Count)];
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.name + " entered the farm at night and was about to poison " +
            "the crops when a farmer named [Character Name] discovered him. He managed to slay the farmer before being forced to flee. " +
            "What do you want him to do next?");
        SetCurrentState(_states[effectName]);
        chosenFarmer.Death();
    }
    private void FarmerKilledEffect(InteractionState state) {
        state.assignedMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    #endregion

    #region Obtain Harvest
    private void ObtainHarvest(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " stole the harvest in the dead of night, " +
            "providing us with much needed Supply. What do you want him to do next?");
        SetCurrentState(_states[effectName]);
    }
    private void ObtainHarvestEffect(InteractionState state) {
        //**Reward**: Supply Cache 1, Demon gains Exp 1
        state.assignedMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        Reward reward = InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1);
        PlayerManager.Instance.player.ClaimReward(reward);
        farm.tileLocation.areaOfTile.PayForReward(reward);
    }
    #endregion

    #region Demon Discovered
    private void DemonDiscovered(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " was discovered by some farmers! " +
            "He managed to run away unscathed but " + farm.tileLocation.areaOfTile.owner.name + " is now aware of our sabotage " +
            "attempts and have declared war upon us. What do you want him to do next?");
        SetCurrentState(_states[effectName]);
        //**Effect**: Faction declares war vs player
        FactionManager.Instance.DeclareWarBetween(farm.tileLocation.areaOfTile.owner, PlayerManager.Instance.player.playerFaction);
    }
    private void DemonDiscoveredEffect(InteractionState state) {
        state.assignedMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    #endregion

    #region Demon Killed
    private void DemonKilled(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " was caught by some guards and was slain in combat. What a weakling. He deserved that.");
        SetCurrentState(_states[effectName]);
    }
    private void DemonKilledEffect(InteractionState state) {
        //**Effect**: Demon is removed from Minion List
        PlayerManager.Instance.player.RemoveMinion(state.assignedMinion);
    }
    #endregion

    #region What To Do Next
    private void WhatToDoNext(InteractionState state, string effectName) {
        _states[effectName].SetDescription("What do you want " + state.chosenOption.assignedMinion.name + " to do next?");
        SetCurrentState(_states[effectName]);
    }
    private void WhatToDoNextEffect(InteractionState state) {
        
    }
    #endregion

    #region Explore Continues
    private void ExploreContinues(InteractionState state, string effectName) {
        _states[effectName].SetDescription("We've instructed " + state.chosenOption.assignedMinion.name + " to continue its surveillance of the area.");
        SetCurrentState(_states[effectName]);
    }
    private void ExploreContinuesEffect(InteractionState state) {
        //**Mechanics**: Explore resets, any other extra Minions will travel back to Portal
    }
    #endregion

    #region Explore Ends
    private void ExploreEnds(InteractionState state, string effectName) {
        _states[effectName].SetDescription("We've instructed " + state.chosenOption.assignedMinion.name + " to return.");
        SetCurrentState(_states[effectName]);
    }
    private void ExploreEndsEffect(InteractionState state) {
        //**Mechanics**: Demon Minion travels back to Portal
    }
    #endregion

}
