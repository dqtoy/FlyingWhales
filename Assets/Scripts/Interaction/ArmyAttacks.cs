using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyAttacks : Interaction {

    private BaseLandmark landmark;

    private const string stopSuccessful = "Stop Successful";
    private const string stopFailure = "Stop Failure";
    private const string stopCriticalFailure = "Stop Critical Failure";
    private const string redirectionSuccessful = "Redirection Successful";
    private const string redirectionFailure = "Redirection Failure";

    public ArmyAttacks(IInteractable interactable) : base(interactable, INTERACTION_TYPE.ARMY_ATTACKS, 150) {
        _name = "Army Attacks";
    }

    #region Overrides
    public override void CreateStates() {
        if (_interactable is BaseLandmark) {
            landmark = _interactable as BaseLandmark;
            CreateExploreStates();
            CreateWhatToDoNextState("%minion% will not intervene with the Garrison's planned attack. Do you want him to continue surveillance of " + landmark.landmarkName + "?");

            InteractionState startState = new InteractionState("State 1", this);
            //string startStateDesc = "The Garrison is preparing to attack " + landmark.name + ".";
            //startState.SetDescription(startStateDesc);
            CreateActionOptions(startState);
            

            //action option states
            InteractionState stopSuccessfulState = new InteractionState(stopSuccessful, this);
            InteractionState stopFailureState = new InteractionState(stopFailure, this);
            InteractionState stopCriticalFailureState = new InteractionState(stopCriticalFailure, this);
            InteractionState redirectionSuccessfulState = new InteractionState(redirectionSuccessful, this);
            InteractionState redirectionFailureState = new InteractionState(redirectionFailure, this);

            //stopSuccessfulState.SetEndEffect(() => StopSuccessfulEffect(stopSuccessfulState));
            //stopFailureState.SetEndEffect(() => StopFailureEffect(stopFailureState));
            stopCriticalFailureState.SetEndEffect(() => StopCriticalFailureRewardEffect(stopCriticalFailureState));
            //redirectionSuccessfulState.SetEndEffect(() => RedirectionSuccessfulEffect(redirectionSuccessfulState));
            //redirectionFailureState.SetEndEffect(() => RedirectionFailureEffect(redirectionFailureState));

            CreateActionOptions(stopSuccessfulState);
            CreateActionOptions(stopFailureState);
            CreateActionOptions(redirectionSuccessfulState);
            CreateActionOptions(redirectionFailureState);

            _states.Add(startState.name, startState);
            _states.Add(stopSuccessfulState.name, stopSuccessfulState);
            _states.Add(stopFailureState.name, stopFailureState);
            _states.Add(stopCriticalFailureState.name, stopCriticalFailureState);
            _states.Add(redirectionSuccessfulState.name, redirectionSuccessfulState);
            _states.Add(redirectionFailureState.name, redirectionFailureState);

            SetCurrentState(startState);
        }
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "State 1") {
            ActionOption stopThem = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Stop Them.",
                duration = 0,
                needsMinion = false,
                effect = () => StopThemEffect(state),
            };
            ActionOption redirectAttack = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 30, currency = CURRENCY.SUPPLY },
                name = "Redirect their attack.",
                duration = 0,
                needsMinion = false,
                neededObjects = new List<System.Type>() { typeof(LocationIntel) },
                effect = () => RedirectAttackEffect(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                //description = "The bandits are increasing their defensive army.",
                duration = 0,
                needsMinion = false,
                effect = () => WhatToDoNextState(),
            };
            state.AddActionOption(stopThem);
            state.AddActionOption(redirectAttack);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        } else if (state.name == stopSuccessful) {
            ActionOption yes = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Yes, please.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreContinuesOption(state),
            };
            ActionOption no = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Send him back to us",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreEndsOption(state),
            };
            state.AddActionOption(yes);
            state.AddActionOption(no);
            state.SetDefaultOption(no);
        } else  {
            ActionOption yes = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Continue surveillance of the area.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreContinuesOption(state),
            };
            ActionOption no = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Return to me.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreEndsOption(state),
            };
            state.AddActionOption(yes);
            state.AddActionOption(no);
            state.SetDefaultOption(no);
        }
    }
    #endregion

    private void StopThemEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(stopSuccessful, 30);
        effectWeights.AddElement(stopFailure, 10);
        effectWeights.AddElement(stopCriticalFailure, 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == stopSuccessful) {
            StopSuccessful(state, chosenEffect);
        } else if (chosenEffect == stopFailure) {
            StopFailure(state, chosenEffect);
        } else if (chosenEffect == stopCriticalFailure) {
            StopCriticalFailure(state, chosenEffect);
        }
    }
    private void RedirectAttackEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(redirectionSuccessful, 30);
        effectWeights.AddElement(redirectionFailure, 10);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == redirectionSuccessful) {
            RedirectionSuccessful(state, chosenEffect);
        } else if (chosenEffect == redirectionFailure) {
            RedirectionFailure(state, chosenEffect);
        }
    }

    private void StopSuccessful(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(_interactable.explorerMinion.name + " disguised himself and talked to the Army General, " +
        //    "eventually convincing him to cancel their attack. With that done, we can have him maintain surveillance " +
        //    "of the area if you want.");
        SetCurrentState(_states[effectName]);
        StopSuccessfulRewardEffect(_states[effectName]);
    }
    private void StopSuccessfulRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
    private void StopFailure(InteractionState state, string effectName) {
        Faction enemyFaction = landmark.owner.GetFactionWithRelationship(FACTION_RELATIONSHIP_STATUS.AT_WAR);
        Area targetArea = enemyFaction.ownedAreas[Random.Range(0, enemyFaction.ownedAreas.Count)];
        BaseLandmark target = targetArea.GetRandomExposedLandmark();
        //**Mechanics**: Army Unit with most occupied slots will Attack the selected enemy location.
        //_states[effectName].SetDescription(_interactable.explorerMinion.name + " disguised himself and talked to the Army General, but was unable to convince him to cancel their attack. What do you want him to do next?");
        SetCurrentState(_states[effectName]);

        CharacterParty army = landmark.GetArmyWithMostOccupiedSlots();
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        army.iactionData.AssignAction(characterAction, target.landmarkObj);
        StopFailureRewardEffect(_states[effectName]);
    }
    private void StopFailureRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
    private void StopCriticalFailure(InteractionState state, string effectName) {
        //**Mechanics**: Army Unit with most occupied slots will Attack a player location. Demon ends Explore and must return to Portal.
        BaseLandmark target = PlayerManager.Instance.player.playerArea.GetRandomExposedLandmark();
        //_states[effectName].SetDescription(_interactable.explorerMinion.name + " disguised himself and talked to the Army General, " +
        //    "but was unable to convince him to cancel their attack. Annoyed with the demon, the General redirected the attack to us! " +
        //    _interactable.explorerMinion.name + " was also forced to flee the area.");
        SetCurrentState(_states[effectName]);

        CharacterParty army = landmark.GetArmyWithMostOccupiedSlots();
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        army.iactionData.AssignAction(characterAction, target.landmarkObj);
        landmark.landmarkInvestigation.MinionGoBackFromAssignment(null);
    }
    private void StopCriticalFailureRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }

    private void RedirectionSuccessful(InteractionState state, string effectName) {
        //**Mechanics**: Army Unit with most occupied slots will Attack assigned Location Intel.
        BaseLandmark target = state.chosenOption.assignedLocation.location.GetRandomExposedLandmark();
        //_states[effectName].SetDescription(_interactable.explorerMinion.name + " disguised himself and talked to the Army General," +
        //    " eventually convincing him to redirect their attack to " + target.name + ". What do you want him to do next?");
        SetCurrentState(_states[effectName]);

        CharacterParty army = landmark.GetArmyWithMostOccupiedSlots();
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        army.iactionData.AssignAction(characterAction, target.landmarkObj);
        RedirectionSuccessfulRewardEffect(_states[effectName]);
    }
    private void RedirectionSuccessfulRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
    private void RedirectionFailure(InteractionState state, string effectName) {
        //**Mechanics**: Army Unit with most occupied slots will Attack a player location.
        BaseLandmark target = PlayerManager.Instance.player.playerArea.GetRandomExposedLandmark();
        //_states[effectName].SetDescription(_interactable.explorerMinion.name + " disguised himself and talked to the Army General, " +
        //    "but failed to convince him to redirect their attack to " + target.name + ". What do you want him to do next?");
        SetCurrentState(_states[effectName]);

        CharacterParty army = landmark.GetArmyWithMostOccupiedSlots();
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        army.iactionData.AssignAction(characterAction, target.landmarkObj);
        RedirectionFailureRewardEffect(_states[effectName]);
    }
    private void RedirectionFailureRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
}
