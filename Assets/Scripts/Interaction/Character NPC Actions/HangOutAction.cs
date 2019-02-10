using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangOutAction : Interaction {

    private Character _targetCharacter;

    private const string Both_Becomes_Cheery = "Both becomes Cheery";
    private const string Both_Becomes_Annoyed = "Both becomes Annoyed";
    private const string Target_Missing = "Target Missing";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }
    private LocationStructure targetStructure;

    public HangOutAction(Area interactable)
        : base(interactable, INTERACTION_TYPE.HANG_OUT_ACTION, 0) {
        _name = "Hang Out Action";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState bothBecomesCheery = new InteractionState(Both_Becomes_Cheery, this);
        InteractionState bothBecomesAnnoyed = new InteractionState(Both_Becomes_Annoyed, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        startState.SetEffect(() => StartRewardEffect(startState), false);
        bothBecomesCheery.SetEffect(() => BothBecomesCheeryRewardEffect(bothBecomesCheery));
        bothBecomesAnnoyed.SetEffect(() => BothBecomesAnnoyedRewardEffect(bothBecomesAnnoyed));
        targetMissing.SetEffect(() => TargetMissingRewardEffect(targetMissing));

        _states.Add(startState.name, startState);

        _states.Add(bothBecomesCheery.name, bothBecomesCheery);
        _states.Add(bothBecomesAnnoyed.name, bothBecomesAnnoyed);
        _states.Add(targetMissing.name, targetMissing);

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
    public override bool CanInteractionBeDoneBy(Character character) {
        if (targetCharacter == null 
            || targetCharacter.isDead) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character targetCharacter) {
        _targetCharacter = targetCharacter;
        targetStructure = targetCharacter.currentStructure;
        AddToDebugLog("Set " + targetCharacter.name + " at " + targetStructure?.ToString() ?? "Nowhere" + " as target");
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        string nextState = string.Empty;
        if (targetCharacter.currentStructure == targetStructure) {
            WeightedDictionary<string> result = new WeightedDictionary<string>();
            result.AddElement(Both_Becomes_Cheery, 20);
            result.AddElement(Both_Becomes_Annoyed, 5);
            nextState = result.PickRandomElementGivenWeights();
        } else {
            nextState = Target_Missing;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void StartRewardEffect(InteractionState state) {
        //**Structure**: Move the character to the target's Structure
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.currentStructure);
    }
    private void BothBecomesCheeryRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Both characters will gain Cheery Trait for 5 days.
        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Cheery"]);
        _targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Cheery"]);
    }
    private void BothBecomesAnnoyedRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Both characters will gain Annoyed Trait for 5 days.
        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Annoyed"]);
        _targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Annoyed"]);
    }
    private void TargetMissingRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
