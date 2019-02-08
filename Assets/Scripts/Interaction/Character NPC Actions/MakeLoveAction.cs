using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeLoveAction : Interaction {

    private const string Actor_Disappointed = "Actor Disappointed";
    private const string Actor_Rejected = "Actor Rejected";
    private const string Actor_Accepted = "Actor Accepted";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public MakeLoveAction(Area interactable)
        : base(interactable, INTERACTION_TYPE.MAKE_LOVE_ACTION, 0) {
        _name = "Make Love Action";
        _jobFilter = new JOB[] { JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState actorDisappointed = new InteractionState(Actor_Disappointed, this);
        InteractionState actorRejected = new InteractionState(Actor_Rejected, this);
        InteractionState actorAccepted = new InteractionState(Actor_Accepted, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartRewardEffect(startState), false);
        actorDisappointed.SetEffect(() => ActorDisappointedRewardEffect(actorDisappointed));
        actorRejected.SetEffect(() => ActorRejectedRewardEffect(actorRejected));
        actorAccepted.SetEffect(() => ActorAcceptedRewardEffect(actorAccepted));

        _states.Add(startState.name, startState);
        _states.Add(actorDisappointed.name, actorDisappointed);
        _states.Add(actorRejected.name, actorRejected);
        _states.Add(actorAccepted.name, actorAccepted);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    //public override bool CanInteractionBeDoneBy(Character character) {
    //    if (character.homeArea.id == character.specificLocation.id) {
    //        return false;
    //    }
    //    return base.CanInteractionBeDoneBy(character);
    //}
    public override void SetTargetCharacter(Character character) {
        _targetCharacter = character;
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        string nextState = string.Empty;
        if (_targetCharacter.currentStructure != targetCharacter.homeStructure) {
            nextState = Actor_Disappointed;
        } else {
            if (targetCharacter.GetTrait("Tired") != null 
                || targetCharacter.GetTrait("Exhausted") != null) {
                nextState = Actor_Rejected;
            } else {
                nextState = Actor_Accepted;
            }
        }
        SetCurrentState(_states[Actor_Accepted]);
    }
    #endregion

    #region Reward Effect
    private void StartRewardEffect(InteractionState state) {
        //**Structure**: Move the character to the target's home Dwelling
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.homeStructure);
    }
    private void ActorDisappointedRewardEffect(InteractionState state) {
        //**Mechanics**: Character 1 gains https://trello.com/c/KWmQt7DF/1152-annoyed trait
        _characterInvolved.AddTrait("Annoyed");
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void ActorRejectedRewardEffect(InteractionState state) {
        //**Mechanics**: Character 1 gains https://trello.com/c/KWmQt7DF/1152-annoyed trait
        _characterInvolved.AddTrait("Annoyed");
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void ActorAcceptedRewardEffect(InteractionState state) {
        //**Mechanics**: Both characters will gain https://trello.com/c/OPL4tXyV/1151-cheery Trait for 5 days.
        _characterInvolved.AddTrait("Cheery");
        _targetCharacter.AddTrait("Cheery");
        //**Mechanics**: Both characters lose 25 Tiredness Meter amount, check if Tired or Exhausted trait must be added to either.
        _characterInvolved.AdjustTiredness(-25);
        _targetCharacter.AdjustTiredness(-25);
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
