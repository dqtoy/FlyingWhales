using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinBanditReinforcements : Interaction {

    private BaseLandmark landmark;

    private WeightedDictionary<LandmarkDefender> assaultSpawnWeights;

    public GoblinBanditReinforcements(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.GOBLIN_BANDIT_REINFORCEMENTS, 50) {
        _name = "Goblin Bandit Reinforcements";
    }

    #region Overrides
    public override void CreateStates() {
        if (_interactable is BaseLandmark) {
            //CreateExploreStates();
            //CreateWhatToDoNextState("What do you want %minion% to do next?");
            landmark = _interactable as BaseLandmark;
            ConstructDefenseSpawnWeights();

            InteractionState startState = new InteractionState("Start", this);
            //string startStateDesc = "The bandits are increasing their defensive army.";
            //startState.SetDescription(startStateDesc);
            
            //action option states
            InteractionState successCancelState = new InteractionState("Successfully Cancelled Reinforcement", this);
            InteractionState failedCancelState = new InteractionState("Failed to Cancel Reinforcement", this);
            InteractionState unitStolenState = new InteractionState("Unit Stolen", this);
            InteractionState doNothingState = new InteractionState("Do nothing", this);

            CreateActionOptions(startState);
            //CreateActionOptions(successCancelState);
            //CreateActionOptions(failedCancelState);
            //CreateActionOptions(giftRejectedState);

            successCancelState.SetEndEffect(() => SuccessfullyCalledReinforcementRewardEffect(successCancelState));
            failedCancelState.SetEndEffect(() => FailedToCancelReinforcementRewardEffect(failedCancelState));
            unitStolenState.SetEndEffect(() => UnitStolenRewardEffect(unitStolenState));
            doNothingState.SetEndEffect(() => DoNothingRewardEffect(doNothingState));

            _states.Add(startState.name, startState);
            _states.Add(successCancelState.name, successCancelState);
            _states.Add(failedCancelState.name, failedCancelState);
            _states.Add(unitStolenState.name, unitStolenState);
            _states.Add(doNothingState.name, doNothingState);

            SetCurrentState(startState);
        }
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopThem = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Stop Them.",
                duration = 0,
                needsMinion = false,
                effect = () => StopThemEffect(state),
            };
            ActionOption takeUnit = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Take the unit they will produce.",
                duration = 0,
                needsMinion = false,
                effect = () => TakeUnitEffect(state),
                canBeDoneAction = () => AssignedMinionIsOfClass(new List<string>() { "Wrath", "Pride" })
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(stopThem);
            state.AddActionOption(takeUnit);
            state.AddActionOption(doNothing);
        }
    }
    #endregion

    private void ConstructDefenseSpawnWeights() {
        assaultSpawnWeights = new WeightedDictionary<LandmarkDefender>();

        LandmarkDefender marauder = new LandmarkDefender() {
            className = "Marauders",
        };
        LandmarkDefender bowman = new LandmarkDefender() {
            className = "Shamans",
        };

        assaultSpawnWeights.AddElement(marauder, 35);
        assaultSpawnWeights.AddElement(bowman, 10);
    }
    private CharacterParty CreateAssaultArmy(int unitCount) {
        CharacterParty army = null;
        for (int i = 0; i < unitCount; i++) {
            LandmarkDefender chosenDefender = assaultSpawnWeights.PickRandomElementGivenWeights();
            Character armyUnit = CharacterManager.Instance.CreateNewCharacter(chosenDefender.className, landmark.owner.race, GENDER.MALE, landmark.owner, landmark);
            if (army == null) {
                army = armyUnit.party;
            } else {
                army.AddCharacter(armyUnit);
            }
        }
        return army;
    }

    private void StopThemEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Successfully Cancelled Reinforcement", 25);
        effectWeights.AddElement("Failed to Cancel Reinforcement", 10);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void TakeUnitEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Unit Stolen", 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states["Do nothing"]);
    }

    private void SuccessfullyCalledReinforcementRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        state.AddLogFiller(new LogFiller(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void FailedToCancelReinforcementRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //**Mechanics**: create an Army Unit from Defense Spawn Weights and add it to the Tile Defenders if not yet full or Character List if already full
        ICharacter createdUnit = CreateAssaultArmy(1).owner;
        if (!landmark.defenders.isFull) {
            landmark.AddDefender(createdUnit);
        }
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(createdUnit.race), LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(createdUnit.role.roleType.ToString()), LOG_IDENTIFIER.STRING_2);
        }
        state.AddLogFiller(new LogFiller(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.GetNormalizedSingularRace(createdUnit.race), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(createdUnit.role.roleType.ToString()), LOG_IDENTIFIER.STRING_2));
    }
    private void UnitStolenRewardEffect(InteractionState state) {
        //**Mechanics**: create an Army Unit from Defense Spawn Weights and add it to the player's Minion List.
        ICharacter createdUnit = CreateAssaultArmy(1).owner;
        //Add unit to players minion list
        PlayerManager.Instance.player.AddNewCharacter(createdUnit);
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(createdUnit.race), LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(createdUnit.role.roleType.ToString()), LOG_IDENTIFIER.STRING_2);
        }
        state.AddLogFiller(new LogFiller(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.GetNormalizedSingularRace(createdUnit.race), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(createdUnit.role.roleType.ToString()), LOG_IDENTIFIER.STRING_2));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //**Mechanics**: create an Army Unit from Defense Spawn Weights and add it to the Tile Defenders if not yet full or Character List if already full
        ICharacter createdUnit = CreateAssaultArmy(1).owner;
        if (!landmark.defenders.isFull) {
            landmark.AddDefender(createdUnit);
        }
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(createdUnit.race), LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(createdUnit.role.roleType.ToString()), LOG_IDENTIFIER.STRING_2);
        }
        state.AddLogFiller(new LogFiller(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.GetNormalizedSingularRace(createdUnit.race), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(createdUnit.role.roleType.ToString()), LOG_IDENTIFIER.STRING_2));
    }
}
