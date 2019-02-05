using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangOutAction : Interaction {

    private Character _targetCharacter;

    private const string Both_Becomes_Cheery = "Both becomes Cheery";
    private const string Both_Becomes_Annoyed = "Both becomes Annoyed";

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

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        bothBecomesCheery.SetEffect(() => BothBecomesCheeryRewardEffect(bothBecomesCheery));
        bothBecomesAnnoyed.SetEffect(() => BothBecomesAnnoyedRewardEffect(bothBecomesAnnoyed));

        _states.Add(startState.name, startState);

        _states.Add(bothBecomesCheery.name, bothBecomesCheery);
        _states.Add(bothBecomesAnnoyed.name, bothBecomesAnnoyed);
        
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
        /*
         Once the actual action is triggered, check if the target character is in the location 
         and if they still have at least one Positive or Neutral relationship. If not, this will no longer be valid.
         */
        if (targetCharacter == null 
            || targetCharacter.isDead 
            || targetCharacter.currentStructure != targetStructure 
            || !characterInvolved.HasRelationshipOfEffectWith(targetCharacter, new List<TRAIT_EFFECT>(){ TRAIT_EFFECT.NEUTRAL, TRAIT_EFFECT.POSITIVE})) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = new WeightedDictionary<RESULT>();
        resultWeights.AddElement(RESULT.SUCCESS, 20);
        resultWeights.AddElement(RESULT.FAIL, 5);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Both_Becomes_Cheery;
                break;
            case RESULT.FAIL:
                nextState = Both_Becomes_Annoyed;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
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
    #endregion


    public void SetTargetCharacter(Character targetCharacter) {
        _targetCharacter = targetCharacter;
        targetStructure = targetCharacter.currentStructure;
        AddToDebugLog("Set " + targetCharacter.name + " at " + targetStructure?.ToString() ?? "Nowhere" + " as target");
    }
}
