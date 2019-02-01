using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumeLife : Interaction {

    private const string Start = "Start";
    private const string Consume_Cancelled = "Consume Cancelled";
    private const string Consume_Proceeds = "Consume Proceeds";
    private const string Normal_Consume = "Normal Consume";

    private Character _targetCharacter;

    public ConsumeLife(Area interactable) : base(interactable, INTERACTION_TYPE.CONSUME_LIFE, 0) {
        _name = "Consume Life";
    }

    #region Overrides
    public override void CreateStates() {
        if (_targetCharacter == null) {
            _targetCharacter = GetTargetCharacter(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState consumeCancelledState = new InteractionState(Consume_Cancelled, this);
        InteractionState consumeProceedsState = new InteractionState(Consume_Proceeds, this);
        InteractionState normalConsumeState = new InteractionState(Normal_Consume, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        consumeCancelledState.SetEffect(() => ConsumeCancelledEffect(consumeCancelledState));
        consumeProceedsState.SetEffect(() => ConsumeProceedsEffect(consumeProceedsState));
        normalConsumeState.SetEffect(() => NormalConsumeEffect(normalConsumeState));

        _states.Add(startState.name, startState);
        _states.Add(consumeCancelledState.name, consumeCancelledState);
        _states.Add(consumeProceedsState.name, consumeProceedsState);
        _states.Add(normalConsumeState.name, normalConsumeState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption preventOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from consuming " + _targetCharacter.name + ".",
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
        if (_targetCharacter == null) {
            _targetCharacter = GetTargetCharacter(character);
        }
        if (_targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Action Options
    private void PreventOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Consume_Cancelled, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Consume_Proceeds, investigatorCharacter.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Normal_Consume]);
    }
    #endregion

    #region State Effects
    private void ConsumeCancelledEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void ConsumeProceedsEffect(InteractionState state) {
        int extractedSupply = (UnityEngine.Random.Range(25, 101)) * _targetCharacter.level;
        _characterInvolved.specificLocation.AdjustSuppliesInBank(extractedSupply);

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(null, extractedSupply.ToString(), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(null, extractedSupply.ToString(), LOG_IDENTIFIER.STRING_1));

        _targetCharacter.Death();
    }
    private void NormalConsumeEffect(InteractionState state) {
        int extractedSupply = (UnityEngine.Random.Range(25, 101)) * _targetCharacter.level;
        _characterInvolved.specificLocation.AdjustSuppliesInBank(extractedSupply);

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(null, extractedSupply.ToString(), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(null, extractedSupply.ToString(), LOG_IDENTIFIER.STRING_1));

        _targetCharacter.Death();
    }
    #endregion

    private Character GetTargetCharacter(Character characterInvolved) {
        //WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        List<Character> targets = new List<Character>();
        List<LocationStructure> insideStructures = characterInvolved.specificLocation.GetStructuresAtLocation(true);
        for (int i = 0; i < insideStructures.Count; i++) {
            for (int j = 0; j < insideStructures[i].charactersHere.Count; j++) {
                Character characterAtLocation = insideStructures[i].charactersHere[j];
                if (characterInvolved.id != characterAtLocation.id && characterAtLocation.GetTraitOr("Restrained", "Abducted") != null) {
                    targets.Add(characterAtLocation);
                }
            }
        }
        if (targets.Count > 0) {
            return targets[UnityEngine.Random.Range(0, targets.Count)];
        }
        return null;
    }
}
