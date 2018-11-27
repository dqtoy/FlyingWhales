using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyAttacks : Interaction {

    private BaseLandmark landmark;
    private Area targetArea;
    private BaseLandmark target;

    private const string stopSuccessful = "Stop Successful";
    private const string stopFailure = "Stop Failure";
    private const string stopCriticalFailure = "Stop Critical Failure";
    private const string redirectionSuccessful = "Redirection Successful";
    private const string redirectionFailure = "Redirection Failure";
    private const string doNothing = "Do nothing";

    public ArmyAttacks(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.ARMY_ATTACKS, 150) {
        _name = "Army Attacks";
    }

    #region Overrides
    public override void CreateStates() {
        landmark = _interactable;
        Faction targetFaction = landmark.owner.GetFactionWithRelationship(FACTION_RELATIONSHIP_STATUS.AT_WAR);
        targetArea = targetFaction.ownedAreas[Random.Range(0, targetFaction.ownedAreas.Count)];
        target = targetArea.GetRandomExposedLandmark();
        //CreateExploreStates();
        //CreateWhatToDoNextState("%minion% will not intervene with the Garrison's planned attack. Do you want him to continue surveillance of " + landmark.landmarkName + "?");

        InteractionState startState = new InteractionState("Start", this);
        //string startStateDesc = "The Garrison is preparing to attack " + landmark.name + ".";
        //startState.SetDescription(startStateDesc);
        CreateActionOptions(startState);
            

        //action option states
        InteractionState stopSuccessfulState = new InteractionState(stopSuccessful, this);
        InteractionState stopFailureState = new InteractionState(stopFailure, this);
        InteractionState stopCriticalFailureState = new InteractionState(stopCriticalFailure, this);
        InteractionState redirectionSuccessfulState = new InteractionState(redirectionSuccessful, this);
        InteractionState redirectionFailureState = new InteractionState(redirectionFailure, this);
        InteractionState doNothingState = new InteractionState(doNothing, this);

        stopSuccessfulState.SetEffect(() => StopSuccessfulRewardEffect(stopSuccessfulState));
        stopFailureState.SetEffect(() => StopFailureRewardEffect(stopFailureState));
        stopCriticalFailureState.SetEffect(() => StopCriticalFailureRewardEffect(stopCriticalFailureState));
        redirectionSuccessfulState.SetEffect(() => RedirectionSuccessfulRewardEffect(redirectionSuccessfulState));
        redirectionFailureState.SetEffect(() => RedirectionFailureRewardEffect(redirectionFailureState));
        doNothingState.SetEffect(() => DoNothingRewardEffect(doNothingState));

        //CreateActionOptions(stopSuccessfulState);
        //CreateActionOptions(stopFailureState);
        //CreateActionOptions(redirectionSuccessfulState);
        //CreateActionOptions(redirectionFailureState);

        _states.Add(startState.name, startState);
        _states.Add(stopSuccessfulState.name, stopSuccessfulState);
        _states.Add(stopFailureState.name, stopFailureState);
        _states.Add(stopCriticalFailureState.name, stopCriticalFailureState);
        _states.Add(redirectionSuccessfulState.name, redirectionSuccessfulState);
        _states.Add(redirectionFailureState.name, redirectionFailureState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
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
            ActionOption redirectAttack = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 30, currency = CURRENCY.SUPPLY },
                name = "Redirect their attack.",
                duration = 0,
                needsMinion = false,
                neededObjects = new List<System.Type>() { typeof(LocationIntel) },
                effect = () => RedirectAttackEffect(state),
                canBeDoneAction = () => AssignedMinionIsOfClass("Greed"), //Needs greed minion
                doesNotMeetRequirementsStr = "Minion must be Greed.",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                //description = "The bandits are increasing their defensive army.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(stopThem);
            state.AddActionOption(redirectAttack);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    private void StopThemEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(stopSuccessful, 30);
        effectWeights.AddElement(stopFailure, 10);
        effectWeights.AddElement(stopCriticalFailure, 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void RedirectAttackEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(redirectionSuccessful, 30);
        effectWeights.AddElement(redirectionFailure, 10);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[doNothing]);
    }

    private void StopSuccessfulRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        //    state.minionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //}
        //if (state.landmarkLog != null) {
        //    state.landmarkLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        //    state.landmarkLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //}
        state.AddLogFiller(new LogFiller(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void StopFailureRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //**Mechanics**: Army Unit with most occupied slots will Attack the selected enemy location.
        CharacterParty army = landmark.GetArmyWithMostOccupiedSlots();
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        army.iactionData.AssignAction(characterAction, target.landmarkObj);
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        //    state.minionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //}
        //if (state.landmarkLog != null) {
        //    state.landmarkLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        //    state.landmarkLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //}
        state.AddLogFiller(new LogFiller(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void StopCriticalFailureRewardEffect(InteractionState state) {
        //**Mechanics**: Army Unit with most occupied slots will Attack a player location. Demon ends Explore and must return to Portal.
        BaseLandmark target = PlayerManager.Instance.player.playerArea.GetRandomExposedLandmark();
        CharacterParty army = landmark.GetArmyWithMostOccupiedSlots();
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        army.iactionData.AssignAction(characterAction, target.landmarkObj);
        landmark.tileLocation.areaOfTile.areaInvestigation.RecallMinion("attack");
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        //}
        //if (state.landmarkLog != null) {
        //    state.landmarkLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        //}
        state.AddLogFiller(new LogFiller(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void RedirectionSuccessfulRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //**Mechanics**: Army Unit with most occupied slots will Attack assigned Location Intel.
        BaseLandmark target = state.chosenOption.assignedLocation.location.GetRandomExposedLandmark();
        CharacterParty army = landmark.GetArmyWithMostOccupiedSlots();
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        army.iactionData.AssignAction(characterAction, target.landmarkObj);
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        //    state.minionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //    state.minionLog.AddToFillers(target, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        //}
        //if (state.landmarkLog != null) {
        //    state.landmarkLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        //    state.landmarkLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //    state.landmarkLog.AddToFillers(target, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        //}
        state.AddLogFiller(new LogFiller(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
        state.AddLogFiller(new LogFiller(target.tileLocation.areaOfTile, target.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void RedirectionFailureRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //**Mechanics**: Army Unit with most occupied slots will Attack a player location.
        BaseLandmark target = PlayerManager.Instance.player.playerArea.GetRandomExposedLandmark();
        CharacterParty army = landmark.GetArmyWithMostOccupiedSlots();
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        army.iactionData.AssignAction(characterAction, target.landmarkObj);
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        //    state.minionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //    state.minionLog.AddToFillers(target, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        //}
        //if (state.landmarkLog != null) {
        //    state.landmarkLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        //    state.landmarkLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //    state.landmarkLog.AddToFillers(target, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        //}
        state.AddLogFiller(new LogFiller(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
        state.AddLogFiller(new LogFiller(target.tileLocation.areaOfTile, target.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_2));
    }

    private void DoNothingRewardEffect(InteractionState state) {
        //**Mechanics**: Army Unit with most occupied slots will Attack the selected enemy location.
        CharacterParty army = landmark.GetArmyWithMostOccupiedSlots();
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        army.iactionData.AssignAction(characterAction, target.landmarkObj);
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        //    state.minionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //}
        //if (state.landmarkLog != null) {
        //    state.landmarkLog.AddToFillers(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        //    state.landmarkLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //}
        state.AddLogFiller(new LogFiller(landmark.tileLocation.areaOfTile.owner, landmark.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
    }
}
