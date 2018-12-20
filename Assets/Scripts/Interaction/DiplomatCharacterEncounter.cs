using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiplomatCharacterEncounter : Interaction {

    private const string Start = "Start";
    private const string Train_Character = "Train Character";
    private const string Do_Nothing = "Do Nothing";

    public DiplomatCharacterEncounter(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.DIPLOMAT_CHARACTER_ENCOUNTER, 0) {
        _name = "Diplomat Character Encounter";
        _jobFilter = new JOB[] { JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState trainCharacterState = new InteractionState(Train_Character, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);

        trainCharacterState.SetEffect(() => TrainCharacterEffect(trainCharacterState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(trainCharacterState.name, trainCharacterState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption useTokenOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Use a token.",
                enabledTooltipText = "Check token details.",
                disabledTooltipText = "This token cannot be used here.",
                neededObjects = new List<System.Type>() { typeof(SpecialToken) },
                effect = () => UseTokenOption(state),
            };
            ActionOption trainOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Train " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + ".",
                enabledTooltipText = _characterInvolved.name + " will level up.",
                disabledTooltipText = _characterInvolved.name + " is already more powerful than " + investigatorMinion.name + ".",
                effect = () => TrainOption(),
            };
            trainOption.canBeDoneAction = () => CanBeTrained(trainOption);

            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Leave " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " alone.",
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(useTokenOption);
            state.AddActionOption(trainOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private bool CanBeTrained(ActionOption option) {
        if(_characterInvolved.level >= investigatorMinion.character.level) {
            return false;
        }
        return true;
    }
    private void UseTokenOption(InteractionState state) {
        SpecialToken specialToken = state.assignedSpecialToken;
        specialToken.CreateJointInteractionStates(this, investigatorMinion.character, _characterInvolved);
        SetCurrentState(_states[specialToken.Item_Used]);
    }
    private void TrainOption() {
        SetCurrentState(_states[Train_Character]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region State Effects
    private void TrainCharacterEffect(InteractionState state) {
        investigatorMinion.LevelUp();
        characterInvolved.LevelUp();
    }
    private void DoNothingEffect(InteractionState state) {
    }
    #endregion
}
