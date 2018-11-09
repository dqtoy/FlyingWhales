using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanBanditReinforcements : Interaction {

    private BaseLandmark landmark;

    private WeightedDictionary<LandmarkDefender> assaultSpawnWeights;

    public HumanBanditReinforcements(IInteractable interactable) : base(interactable, INTERACTION_TYPE.HUMAN_BANDIT_REINFORCEMENTS, 50) {
        _name = "Human Bandit Reinforcements";
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
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Stop Them.",
                duration = 0,
                needsMinion = false,
                effect = () => StopThemEffect(state),
            };
            ActionOption takeUnit = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Take the unit they will produce.",
                duration = 0,
                needsMinion = false,
                effect = () => TakeUnitEffect(state),
                canBeDoneAction = () => AssignedMinionIsOfType(new List<DEMON_TYPE>() { DEMON_TYPE.GREED, DEMON_TYPE.ENVY }),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
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
            className = "Rogues",
        };
        LandmarkDefender mage = new LandmarkDefender() {
            className = "Mages",
        };

        assaultSpawnWeights.AddElement(marauder, 35);
        assaultSpawnWeights.AddElement(bowman, 20);
        assaultSpawnWeights.AddElement(mage, 10);
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
        //if (chosenEffect == "Successfully Cancelled Reinforcement") {
        //    SuccessfullyCalledReinforcement(state, chosenEffect);
        //} else if (chosenEffect == "Failed to Cancel Reinforcement") {
        //    FailedToCancelReinforcement(state, chosenEffect);
        //}
    }
    private void TakeUnitEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Unit Stolen", 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
        //if (chosenEffect == "Unit Stolen") {
        //    UnitStolen(state, chosenEffect);
        //}
    }
    private void DoNothingEffect(InteractionState state) {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement("Bandit Reinforcement", 25);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states["Do nothing"]);
        //if (chosenEffect == "Bandit Reinforcement") {
        //    Reinforcement(state, chosenEffect);
        //}
    }

    //private void SuccessfullyCalledReinforcement(InteractionState state, string effectName) {
    //    //_states[effectName].SetDescription(explorerMinion.name + " distracted the bandits with liquor so they ended up " +
    //    //    "forgetting that they were supposed to form a new defensive army unit. What do you want " + explorerMinion.name + " to do next?");
    //    SetCurrentState(_states[effectName]);
    //    SuccessfullyCalledReinforcementRewardEffect(_states[effectName]);
    //}
    private void SuccessfullyCalledReinforcementRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        if (state.minionLog != null) {
            state.minionLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        if (state.landmarkLog != null) {
            state.landmarkLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
    }
    //private void FailedToCancelReinforcement(InteractionState state, string effectName) {
    //    ////**Mechanics**: create an Army Unit from Defense Spawn Weights and add it to the Tile Defenders
    //    //CharacterArmyUnit createdUnit = CreateAssaultArmy(1).owner as CharacterArmyUnit;
    //    //_states[effectName].SetDescription(explorerMinion.name + " failed to distract the bandits. " +
    //    //    "A new " + Utilities.NormalizeString(createdUnit.race.ToString()) + " " + createdUnit.characterClass.className + " have been formed " +
    //    //    "to defend the camp. What do you want " + explorerMinion.name + " to do next?");
    //    SetCurrentState(_states[effectName]);
    //    //landmark.AddDefender(createdUnit);
    //    FailedToCancelReinforcementRewardEffect(_states[effectName]);
    //}
    private void FailedToCancelReinforcementRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //**Mechanics**: create an Army Unit from Defense Spawn Weights and add it to the Tile Defenders if not yet full or Character List if already full
        ICharacter createdUnit = CreateAssaultArmy(1).owner;
        if (!landmark.defenders.isFull) {
            landmark.AddDefender(createdUnit);
        }
        if (state.minionLog != null) {
            state.minionLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.minionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(createdUnit.race), LOG_IDENTIFIER.STRING_1);
            state.minionLog.AddToFillers(null, createdUnit.characterClass.className, LOG_IDENTIFIER.STRING_2);
        }
        if (state.landmarkLog != null) {
            state.landmarkLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.landmarkLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(createdUnit.race), LOG_IDENTIFIER.STRING_1);
            state.landmarkLog.AddToFillers(null, createdUnit.characterClass.className, LOG_IDENTIFIER.STRING_2);
        }
    }

    //private void GiftAccepted(InteractionState state, string effectName) {
    //    //_states[effectName].SetDescription("You gave them " + state.chosenOption.assignedUnit.name + " to aid in their defenses and they have graciously accepted.");
    //    SetCurrentState(_states[effectName]);
    //    //remove assigned unit and add it to the Bandit faction and to their Tile Defenders
    //    if (state.chosenOption.assignedUnit is Minion) {
    //        PlayerManager.Instance.player.RemoveMinion(state.chosenOption.assignedUnit as Minion);
    //        landmark.AddDefender((state.chosenOption.assignedUnit as Minion).icharacter);
    //    }
    //}
    //private void GiftAcceptedRewardEffect(InteractionState state) {


    //}
    //private void UnitStolen(InteractionState state, string effectName) {
    //    //_states[effectName].SetDescription("You gave them " + explorerMinion.name + " to aid in their defenses " +
    //    //    "but they are suspicious of your intentions and have rejected your offer. " +
    //    //    "What do you want " + explorerMinion.name + " to do next?");
    //    SetCurrentState(_states[effectName]);
    //}
    private void UnitStolenRewardEffect(InteractionState state) {
        //**Mechanics**: create an Army Unit from Defense Spawn Weights and add it to the player's Minion List.
        ICharacter createdUnit = CreateAssaultArmy(1).owner;
        //Add unit to players minion list
        PlayerManager.Instance.player.AddNewCharacter(createdUnit);
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        if (state.minionLog != null) {
            state.minionLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.minionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(createdUnit.race), LOG_IDENTIFIER.STRING_1);
            state.minionLog.AddToFillers(null, createdUnit.characterClass.className, LOG_IDENTIFIER.STRING_2);
        }
        if (state.landmarkLog != null) {
            state.landmarkLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.landmarkLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(createdUnit.race), LOG_IDENTIFIER.STRING_1);
            state.landmarkLog.AddToFillers(null, createdUnit.characterClass.className, LOG_IDENTIFIER.STRING_2);
        }
    }

    //private void Reinforcement(InteractionState state, string effectName) {

    //}
    private void DoNothingRewardEffect(InteractionState state) {
        //**Mechanics**: create an Army Unit from Defense Spawn Weights and add it to the Tile Defenders if not yet full or Character List if already full
        ICharacter createdUnit = CreateAssaultArmy(1).owner;
        if (!landmark.defenders.isFull) {
            landmark.AddDefender(createdUnit);
        }
        if (state.minionLog != null) {
            state.minionLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.minionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(createdUnit.race), LOG_IDENTIFIER.STRING_1);
            state.minionLog.AddToFillers(null, createdUnit.characterClass.className, LOG_IDENTIFIER.STRING_2);
        }
        if (state.landmarkLog != null) {
            state.landmarkLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.landmarkLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(createdUnit.race), LOG_IDENTIFIER.STRING_1);
            state.landmarkLog.AddToFillers(null, createdUnit.characterClass.className, LOG_IDENTIFIER.STRING_2);
        }
    }

}
