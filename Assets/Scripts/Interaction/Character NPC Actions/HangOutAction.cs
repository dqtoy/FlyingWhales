using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangOutAction : Interaction {

    private const string Both_Becomes_Cheery = "Both becomes Cheery";
    private const string Both_Becomes_Annoyed = "Both becomes Annoyed";
    private const string Target_Missing = "Target Missing";
    private const string Target_Unavailable = "Target Unavailable";

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
        InteractionState targetUnavailable = new InteractionState(Target_Unavailable, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        startState.SetEffect(() => StartRewardEffect(startState), false);
        bothBecomesCheery.SetEffect(() => BothBecomesCheeryRewardEffect(bothBecomesCheery));
        bothBecomesAnnoyed.SetEffect(() => BothBecomesAnnoyedRewardEffect(bothBecomesAnnoyed));
        targetMissing.SetEffect(() => TargetMissingRewardEffect(targetMissing));
        targetUnavailable.SetEffect(() => TargetUnavailableRewardEffect(targetUnavailable));

        _states.Add(startState.name, startState);
        _states.Add(bothBecomesCheery.name, bothBecomesCheery);
        _states.Add(bothBecomesAnnoyed.name, bothBecomesAnnoyed);
        _states.Add(targetMissing.name, targetMissing);
        _states.Add(targetUnavailable.name, targetUnavailable);

        //SetCurrentState(startState);
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
        _targetStructure = targetCharacter.currentStructure;
        targetGridLocation = _targetCharacter.GetNearestUnoccupiedTileFromCharacter(_targetStructure);
        AddToDebugLog("Set " + targetCharacter.name + " at " + targetStructure?.ToString() ?? "Nowhere" + " as target");
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        string nextState = string.Empty;
        if (targetCharacter.currentStructure == targetStructure) {
            if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER) 
                || targetCharacter.HasTraitOf(TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER)) {
                nextState = Target_Unavailable;
            } else {
                WeightedDictionary<string> result = new WeightedDictionary<string>();
                result.AddElement(Both_Becomes_Cheery, 20);
                result.AddElement(Both_Becomes_Annoyed, 5);
                nextState = result.PickRandomElementGivenWeights();
            }
        } else {
            nextState = Target_Missing;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void StartRewardEffect(InteractionState state) {
        //**Structure**: Move the character to the target's Structure
        _characterInvolved.MoveToAnotherStructure(_targetStructure, _targetCharacter.GetNearestUnoccupiedTileFromCharacter(_targetStructure));
    }
    private void BothBecomesCheeryRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Both characters will gain Cheery Trait for 5 days.
        _characterInvolved.AddTrait("Happy");
        _targetCharacter.AddTrait("Happy");
        //**Mechanics**: If either character has a negative status-type Trait, add it to the other character's **character trouble** list for his relationship data
        Trait targetNegative = targetCharacter.GetTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
        Trait characterNegative = characterInvolved.GetTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
        if (targetNegative != null) {
            characterInvolved.GetCharacterRelationshipData(targetCharacter).AddTrouble(targetNegative);
        }
        if (characterNegative != null) {
            targetCharacter.GetCharacterRelationshipData(characterInvolved).AddTrouble(characterNegative);
        }
    }
    private void BothBecomesAnnoyedRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Both characters will gain Annoyed Trait for 5 days.
        _characterInvolved.AddTrait("Annoyed");
        _targetCharacter.AddTrait("Annoyed");

        //**Mechanics**: If either character has a negative status-type Trait, add it to the other character's **character trouble** list for his relationship data
        Trait targetNegative = targetCharacter.GetTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
        Trait characterNegative = characterInvolved.GetTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
        if (targetNegative != null) {
            characterInvolved.GetCharacterRelationshipData(targetCharacter).AddTrouble(targetNegative);
        }
        if (characterNegative != null) {
            targetCharacter.GetCharacterRelationshipData(characterInvolved).AddTrouble(characterNegative);
        }
    }
    private void TargetMissingRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void TargetUnavailableRewardEffect(InteractionState state) {
        //**Mechanics**: If the target has a negative Disabler-type Trait, add it to the other character's **character trouble** list for his relationship data
        Trait negative = targetCharacter.GetTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
        if (negative != null) {
            characterInvolved.GetCharacterRelationshipData(targetCharacter).AddTrouble(negative);
        }
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
