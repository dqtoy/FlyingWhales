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
            string startStateDesc = "The harvest season is upon this farmlands.";
            startState.SetDescription(startStateDesc);
            CreateActionOptions(startState);
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddHours(300);
            startState.SetTimeSchedule(startState.actionOptions[1], dueDate); //default is do nothing

            //action option states
            InteractionState poisonedHarvestState = new InteractionState("Poisoned Harvest", this);
            InteractionState farmerKilledState = new InteractionState("Farmer Killed", this);
            InteractionState obtainHarvestState = new InteractionState("Obtain Harvest", this); 
            InteractionState demonDiscoveredState = new InteractionState("Demon Discovered", this);
            InteractionState demonKilledState = new InteractionState("Demon Killed", this);


            poisonedHarvestState.SetEndEffect(() => PoisonedHarvestEffect(poisonedHarvestState));
            farmerKilledState.SetEndEffect(() => FarmerKilledEffect(farmerKilledState));
            obtainHarvestState.SetEndEffect(() => ObtainHarvestEffect(obtainHarvestState));
            demonDiscoveredState.SetEndEffect(() => DemonDiscoveredEffect(demonDiscoveredState));
            demonKilledState.SetEndEffect(() => DemonKilledEffect(demonKilledState));
            

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
                name = "Send out a demon.",
                description = "We have sent %minion% to disrupt the harvest. It should take him a short time to execute the task.",
                duration = 10,
                needsMinion = true,
                neededObjects = new List<System.Type>() { typeof(Minion) },
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
        }
    }
    #endregion

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
        state.EndResult();
    }

    #region Poisoned Harvest
    private void PoisonedHarvest(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " managed to secretly poison the crops. The farm will not be able to produce Supply for 5 days.");
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
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " slayed a farmer named " + chosenFarmer.name + ".");
        SetCurrentState(_states[effectName]);
        chosenFarmer.Death();
    }
    private void FarmerKilledEffect(InteractionState state) {
        state.assignedMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    #endregion

    #region Obtain Harvest
    private void ObtainHarvest(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " stole the harvest in the dead of night, providing us with much needed Supply.");
        SetCurrentState(_states[effectName]);
    }
    private void ObtainHarvestEffect(InteractionState state) {
        state.assignedMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
        Reward reward = InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1);
        PlayerManager.Instance.player.ClaimReward(reward);
        farm.tileLocation.areaOfTile.PayForReward(reward);
    }
    #endregion

    #region Demon Discovered
    private void DemonDiscovered(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " was discovered by some farmers! He managed to run away unscathed but " + farm.tileLocation.areaOfTile.owner.name + " is now aware of our sabotage attempts and have declared war upon us.");
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

}
