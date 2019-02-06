using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToArgue : Interaction {
    private const string Start = "Start";
    private const string Argue_Cancelled = "Argue Cancelled";
    private const string Argue_Proceeds = "Argue Proceeds";
    private const string Normal_Argue = "Normal Argue";

    private Character _targetCharacter;

    public override Area targetArea {
        get { return _targetCharacter.homeArea; }
    }

    public MoveToArgue(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_ARGUE_ACTION, 0) {
        _name = "Move To Argue";
        //_categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.SOCIAL };
        //_alignment = INTERACTION_ALIGNMENT.NEUTRAL;
    }

    #region Overrides
    public override void CreateStates() {
        if (_targetCharacter == null) {
            _targetCharacter = GetTargetCharacter(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState argueCancelledState = new InteractionState(Argue_Cancelled, this);
        InteractionState argueProceedsState = new InteractionState(Argue_Proceeds, this);
        InteractionState normalArgueState = new InteractionState(Normal_Argue, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        argueCancelledState.SetEffect(() => ArgueCancelledEffect(argueCancelledState));
        argueProceedsState.SetEffect(() => ArgueProceedsEffect(argueProceedsState));
        normalArgueState.SetEffect(() => NormalArgueEffect(normalArgueState));

        _states.Add(startState.name, startState);
        _states.Add(argueCancelledState.name, argueCancelledState);
        _states.Add(argueProceedsState.name, argueProceedsState);
        _states.Add(normalArgueState.name, normalArgueState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption preventOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from leaving.",
                duration = 0,
                jobNeeded = JOB.DEBILITATOR,
                disabledTooltipText = "Must be a Dissuader.",
                effect = () => PreventOption(),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(preventOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if(_targetCharacter == null) {
            _targetCharacter = GetTargetCharacter(character);
        }
        if (_targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void DoActionUponMoveToArrival() {
        CreateArgueAction();
    }
    #endregion

    #region Action Options
    private void PreventOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Argue_Cancelled, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Argue_Proceeds, investigatorCharacter.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Normal_Argue]);
    }
    #endregion

    #region State Effects
    private void ArgueCancelledEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
    }
    private void ArgueProceedsEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    private void NormalArgueEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    #endregion

    private void CreateArgueAction() {
        AddToDebugLog(_characterInvolved.name + " will now create argue action");
        ArgueAction argue = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.ARGUE_ACTION, _characterInvolved.specificLocation) as ArgueAction;
        argue.SetTargetCharacter(_targetCharacter);
        _characterInvolved.SetForcedInteraction(argue);
    }
    private Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < characterInvolved.traits.Count; i++) {
            if(characterInvolved.traits[i] is RelationshipTrait) {
                RelationshipTrait currTrait = characterInvolved.traits[i] as RelationshipTrait;
                int weight = 0;
                if (currTrait.relType == RELATIONSHIP_TRAIT.ENEMY) {
                    weight += 80;
                }
                if (currTrait.relType == RELATIONSHIP_TRAIT.FRIEND) {
                    weight += 10;
                }
                if (currTrait.relType == RELATIONSHIP_TRAIT.LOVER) {
                    weight += 30;
                }
                if (currTrait.relType == RELATIONSHIP_TRAIT.SERVANT) {
                    weight += 20;
                }
                if (currTrait.relType == RELATIONSHIP_TRAIT.RELATIVE) {
                    weight += 30;
                }
                if (weight > 0) {
                    characterWeights.AddElement(currTrait.targetCharacter, weight);
                }
            }
        }
        if (characterWeights.GetTotalOfWeights() > 0) {
            return characterWeights.PickRandomElementGivenWeights();
        }
        return null;
    }
}
