using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanditReinforcement : Interaction {

    private BaseLandmark landmark;

    private WeightedDictionary<LandmarkDefender> assaultSpawnWeights;
    private WeightedDictionary<LandmarkDefender> firstElementAssaultSpawnWeights; //TODO: Make this more elegant!

    public BanditReinforcement(IInteractable interactable) : base(interactable, INTERACTION_TYPE.BANDIT_REINFORCEMENT) {
        _name = "Bandit Reinforcement";
    }

    #region Overrides
    public override void CreateStates() {
        if (_interactable is BaseLandmark) {
            landmark = _interactable as BaseLandmark;
            ConstructDefenseSpawnWeights();

            InteractionState startState = new InteractionState("State 1", this);
            string startStateDesc = "The bandits are increasing their defensive army.";
            startState.SetDescription(startStateDesc);
            CreateActionOptions(startState);

            //action option states
            InteractionState successCancelState = new InteractionState("Successfully Cancelled Reinforcement", this);
            InteractionState failedCancelState = new InteractionState("Failed to Cancel Reinforcement", this);
            InteractionState giftAcceptedState = new InteractionState("Gift Accepted", this);
            InteractionState giftRejectedState = new InteractionState("Gift Rejected", this);

            successCancelState.SetEndEffect(() => SuccessfullyCalledReinforcementEffect(successCancelState));
            failedCancelState.SetEndEffect(() => FailedToCancelReinforcementEffect(failedCancelState));
            giftAcceptedState.SetEndEffect(() => GiftAcceptedEffect(giftAcceptedState));
            giftRejectedState.SetEndEffect(() => GiftRejectedEffect(giftRejectedState));
            
            _states.Add(startState.name, startState);
            _states.Add(successCancelState.name, successCancelState);
            _states.Add(failedCancelState.name, failedCancelState);
            _states.Add(giftAcceptedState.name, giftAcceptedState);
            _states.Add(giftRejectedState.name, giftRejectedState);

            SetCurrentState(startState);
        }
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "State 1") {
            ActionOption stopThem = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Stop Them.",
                description = "We have sent %minion% to prevent the bandits from raising a new defensive army unit.",
                duration = 0,
                needsMinion = true,
                neededObjects = new List<System.Type>() { typeof(Minion) },
                effect = () => StopThemEffect(state),
                onStartDurationAction = () => SetDefaultActionDurationAsRemainingTicks("Stop Them.", state),
            };
            ActionOption provideOwnUnit = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Provide your own unit instead.",
                description = "We offered %minion% to join the bandits.",
                duration = 5,
                needsMinion = true,
                neededObjects = new List<System.Type>() { typeof(IUnit) },
                effect = () => ProvideOwnUnitEffect(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                description = "The bandits are increasing their defensive army.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingEffect(state),
                onStartDurationAction = () => SetDefaultActionDurationAsRemainingTicks("Do nothing.", state),
            };
            state.AddActionOption(stopThem);
            state.AddActionOption(provideOwnUnit);
            state.AddActionOption(doNothing);

            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddHours(50);
            state.SetTimeSchedule(doNothing, dueDate); //default is do nothing
        }
    }
    #endregion

    private void ConstructDefenseSpawnWeights() {
        assaultSpawnWeights = new WeightedDictionary<LandmarkDefender>();
        firstElementAssaultSpawnWeights = new WeightedDictionary<LandmarkDefender>();

        LandmarkDefender marauder = new LandmarkDefender() {
            className = "Marauder",
            armyCount = 25
        };
        LandmarkDefender bowman = new LandmarkDefender() {
            className = "Bowman",
            armyCount = 25
        };
        LandmarkDefender healer = new LandmarkDefender() {
            className = "Healer",
            armyCount = 25
        };

        firstElementAssaultSpawnWeights.AddElement(marauder, 35);
        firstElementAssaultSpawnWeights.AddElement(bowman, 20);

        assaultSpawnWeights.AddElement(marauder, 35);
        assaultSpawnWeights.AddElement(bowman, 20);
        assaultSpawnWeights.AddElement(healer, 10);
    }
    private CharacterParty CreateAssaultArmy(int unitCount) {
        CharacterParty army = null;
        for (int i = 0; i < unitCount; i++) {
            LandmarkDefender chosenDefender;
            if (i == 0) {
                chosenDefender = firstElementAssaultSpawnWeights.PickRandomElementGivenWeights();
            } else {
                chosenDefender = assaultSpawnWeights.PickRandomElementGivenWeights();
            }
            CharacterArmyUnit armyUnit = CharacterManager.Instance.CreateCharacterArmyUnit(landmark.owner.race, chosenDefender, landmark.owner, landmark);
            if (army == null) {
                army = armyUnit.party as CharacterParty;
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
        if (chosenEffect == "Successfully Cancelled Reinforcement") {
            SuccessfullyCalledReinforcement(state, chosenEffect);
        } else if (chosenEffect == "Failed to Cancel Reinforcement") {
            FailedToCancelReinforcement(state, chosenEffect);
        }
    }
    private void ProvideOwnUnitEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Gift Accepted", 25);
        effectWeights.AddElement("Gift Rejected", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Gift Accepted") {
            GiftAccepted(state, chosenEffect);
        } else if (chosenEffect == "Gift Rejected") {
            GiftRejected(state, chosenEffect);
        }
    }
    private void DoNothingEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Bandit Reinforcement", 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Bandit Reinforcement") {
            Reinforcement(state, chosenEffect);
        }
    }

    private void SuccessfullyCalledReinforcement(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " distracted the bandits with liquor so they ended up forgetting that they were supposed to form a new defensive army unit.");
        SetCurrentState(_states[effectName]);
    }
    private void SuccessfullyCalledReinforcementEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        state.assignedMinion.AdjustExp(1);
    }
    private void FailedToCancelReinforcement(InteractionState state, string effectName) {
        //**Mechanics**: create an Army Unit from Defense Spawn Weights and add it to the Tile Defenders
        CharacterArmyUnit createdUnit = CreateAssaultArmy(1).owner as CharacterArmyUnit;
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " failed to distract the bandits. A new " + Utilities.NormalizeString(createdUnit.race.ToString()) + " " + createdUnit.characterClass.className + " have been formed to defend the camp.");
        SetCurrentState(_states[effectName]);
        landmark.AddDefender(createdUnit);
    }
    private void FailedToCancelReinforcementEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        state.assignedMinion.AdjustExp(1);
    }

    private void GiftAccepted(InteractionState state, string effectName) {
        _states[effectName].SetDescription("You gave them " + state.chosenOption.assignedUnit.name + " to aid in their defenses and they have graciously accepted.");
        SetCurrentState(_states[effectName]);
        //remove assigned unit and add it to the Bandit faction and to their Tile Defenders
        if (state.chosenOption.assignedUnit is Minion) {
            PlayerManager.Instance.player.RemoveMinion(state.chosenOption.assignedUnit as Minion);
            landmark.AddDefender((state.chosenOption.assignedUnit as Minion).icharacter);
        }
    }
    private void GiftAcceptedEffect(InteractionState state) {
        
        
    }
    private void GiftRejected(InteractionState state, string effectName) {
        _states[effectName].SetDescription("You gave them " + state.chosenOption.assignedMinion.icharacter.name + " to aid in their defenses but they are suspicious of your intentions and have rejected your offer.");
        SetCurrentState(_states[effectName]);
    }
    private void GiftRejectedEffect(InteractionState state) {
        
    }

    private void Reinforcement(InteractionState state, string effectName) {
        
    }
    private void BanditReinforcementEffect(InteractionState state) {

    }

}
