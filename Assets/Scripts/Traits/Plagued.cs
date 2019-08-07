using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plagued : Trait {

    public Character owner { get; private set; } //poi that has the poison

    private float pukeChance;
    private float septicChance;

    private GoapAction stoppedAction;
    private CharacterState pausedState;

    public Plagued() {
        name = "Plagued";
        description = "This character is Plagued.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        daysDuration = GameManager.ticksPerDay * 3;
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            owner = sourceCharacter as Character;
            Messenger.AddListener<Character>(Signals.CHARACTER_STARTED_MOVING, OnCharacterStartedMoving);
            Messenger.AddListener<Character>(Signals.CHARACTER_STOPPED_MOVING, OnCharacterStoppedMoving);
            if (owner.currentParty.icon.isTravelling) {
                OnCharacterStartedMoving(owner);
            }
        }
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        base.OnRemoveTrait(sourceCharacter, removedBy);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_STARTED_MOVING, OnCharacterStartedMoving);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_STOPPED_MOVING, OnCharacterStoppedMoving);
        Messenger.RemoveListener(Signals.TICK_ENDED, PerMovementTick);
    }
    protected override void OnChangeLevel() {
        if (level == 1) {
            pukeChance = 5f;
            septicChance = 0.5f;
        } else if (level == 2) {
            pukeChance = 7f;
            septicChance = 1f;
        } else {
            pukeChance = 9f;
            septicChance = 1.5f;
        }
    }
    #endregion

    private void OnCharacterStartedMoving(Character character) {
        if (character == owner) {
            Messenger.AddListener(Signals.TICK_ENDED, PerMovementTick);
        }
    }
    private void OnCharacterStoppedMoving(Character character) {
        if (character == owner) {
            Messenger.RemoveListener(Signals.TICK_ENDED, PerMovementTick);
        }
    }

    private void PerMovementTick() {
        float pukeRoll = Random.Range(0f, 100f);
        float septicRoll = Random.Range(0f, 100f);
        if (pukeRoll < pukeChance) {
            //do puke action
            if (owner.currentAction != null && owner.currentAction.goapType != INTERACTION_TYPE.PUKE) {
                stoppedAction = owner.currentAction;
                owner.StopCurrentAction(false);
                owner.marker.StopMovement();
                
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
                goapPlan.ConstructAllNodes();

                goapAction.CreateStates();
                owner.SetCurrentAction(goapAction);
                owner.currentAction.SetEndAction(ResumeLastAction);
                owner.currentAction.PerformActualAction();
            } else if (owner.stateComponent.currentState != null) {
                pausedState = owner.stateComponent.currentState;
                owner.stateComponent.currentState.PauseState();
                owner.marker.StopMovement();
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
                goapPlan.ConstructAllNodes();

                goapAction.CreateStates();
                owner.SetCurrentAction(goapAction);
                owner.currentAction.SetEndAction(ResumePausedState);
                owner.currentAction.PerformActualAction();
            }
        } else if (septicRoll < septicChance) {
            if (owner.currentAction != null && owner.currentAction.goapType != INTERACTION_TYPE.SEPTIC_SHOCK) {
                stoppedAction = owner.currentAction;
                owner.StopCurrentAction(false);
                owner.marker.StopMovement();
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.SEPTIC_SHOCK, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
                goapPlan.ConstructAllNodes();

                goapAction.CreateStates();
                owner.SetCurrentAction(goapAction);
                owner.currentAction.PerformActualAction();
            } else if (owner.stateComponent.currentState != null) {
                pausedState = owner.stateComponent.currentState;
                owner.stateComponent.currentState.PauseState();
                owner.marker.StopMovement();
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.SEPTIC_SHOCK, owner, owner);
                
                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
                goapPlan.ConstructAllNodes();

                goapAction.CreateStates();
                owner.SetCurrentAction(goapAction);
                owner.currentAction.PerformActualAction();
            }
        }
    }

    private void ResumeLastAction(string result, GoapAction action) {
        if (stoppedAction.CanSatisfyRequirements()) {
            stoppedAction.DoAction();
        } else {
            owner.GoapActionResult(result, action);
        }
        
    }
    private void ResumePausedState(string result, GoapAction action) {
        pausedState.ResumeState();
    }
}
