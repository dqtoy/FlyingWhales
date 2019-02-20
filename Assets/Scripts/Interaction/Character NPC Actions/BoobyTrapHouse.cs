using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoobyTrapHouse : Interaction {

    private const string Booby_Trap_Cancelled = "Booby Trap Cancelled";
    private const string Booby_Trap_Continues = "Booby Trap Continues";
    private const string Character_Booby_Trap = "Character Booby Trap";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public BoobyTrapHouse(Area interactable)
        : base(interactable, INTERACTION_TYPE.BOOBY_TRAP_HOUSE, 0) {
        _name = "Booby Trap House";
        _jobFilter = new JOB[] { JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState trapCancelled = new InteractionState(Booby_Trap_Cancelled, this);
        InteractionState trapContinues = new InteractionState(Booby_Trap_Continues, this);
        InteractionState characterTrap = new InteractionState(Character_Booby_Trap, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartRewardEffect(startState), false);
        trapCancelled.SetEffect(() => TrapCancelledRewardEffect(trapCancelled));
        trapContinues.SetEffect(() => TrapContinuesRewardEffect(trapContinues));
        characterTrap.SetEffect(() => CharacterTrapRewardEffect(characterTrap));

        _states.Add(startState.name, startState);
        _states.Add(trapCancelled.name, trapCancelled);
        _states.Add(trapContinues.name, trapContinues);
        _states.Add(characterTrap.name, characterTrap);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from booby trapping.",
                duration = 0,
                effect = () => PreventFromLeavingOptionEffect(state),
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Must have debilitator minion.",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(prevent);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (character.homeArea.id != character.specificLocation.id) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character character) {
        _targetCharacter = character;
    }
    #endregion

    #region Option Effect
    private void PreventFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Booby_Trap_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Booby_Trap_Continues;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Character_Booby_Trap]);
    }
    #endregion

    #region Reward Effect
    private void StartRewardEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.homeStructure);
    }
    private void TrapCancelledRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void TrapContinuesRewardEffect(InteractionState state) {
        //**Mechanics**: Add https://trello.com/c/bJodHyHF/1084-booby-trapped Trait to the current Structure
        _targetCharacter.homeStructure.AddTrait("Booby Trapped");
        state.descriptionLog.AddToFillers(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void CharacterTrapRewardEffect(InteractionState state) {
        //**Mechanics**: Add https://trello.com/c/bJodHyHF/1084-booby-trapped Trait to the current Structure
        _targetCharacter.homeStructure.AddTrait("Booby Trapped");
        state.descriptionLog.AddToFillers(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
