using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCurse : Interaction {
    private const string Start = "Start";
    private const string Curse_Cancelled = "Curse Cancelled";
    private const string Curse_Proceeds = "Curse Proceeds";
    private const string Normal_Curse = "Normal Curse";

    private Character _targetCharacter;

    public override Area targetArea {
        get { return _targetCharacter.homeArea; }
    }

    public MoveToCurse(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_CURSE, 0) {
        _name = "Move To Curse";
        //_categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.OFFENSE };
        //_alignment = INTERACTION_ALIGNMENT.EVIL;
    }

    #region Overrides
    public override void CreateStates() {
        if (_targetCharacter == null) {
            _targetCharacter = GetTargetCharacter(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState curseCancelledState = new InteractionState(Curse_Cancelled, this);
        InteractionState curseProceedsState = new InteractionState(Curse_Proceeds, this);
        InteractionState normalCurseState = new InteractionState(Normal_Curse, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        //startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        curseCancelledState.SetEffect(() => CurseCancelledEffect(curseCancelledState));
        curseProceedsState.SetEffect(() => CurseProceedsEffect(curseProceedsState));
        normalCurseState.SetEffect(() => NormalCurseEffect(normalCurseState));

        _states.Add(startState.name, startState);
        _states.Add(curseCancelledState.name, curseCancelledState);
        _states.Add(curseProceedsState.name, curseProceedsState);
        _states.Add(normalCurseState.name, normalCurseState);

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
        CreateCurseAction();
    }
    #endregion

    #region Action Options
    private void PreventOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Curse_Cancelled, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Curse_Proceeds, investigatorCharacter.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Normal_Curse]);
    }
    #endregion

    #region State Effects
    private void CurseCancelledEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
    }
    private void CurseProceedsEffect(InteractionState state) {
        //state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        //state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    private void NormalCurseEffect(InteractionState state) {
        //state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        //state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    #endregion

    private void CreateCurseAction() {
        AddToDebugLog(_characterInvolved.name + " will now create curse action");
        CurseAction curse = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CURSE_ACTION, _characterInvolved.specificLocation) as CurseAction;
        curse.SetTargetCharacter(_targetCharacter);
        _characterInvolved.SetForcedInteraction(curse);
    }
    private Character GetTargetCharacter(Character characterInvolved) {
        //WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        List<Character> enemies = new List<Character>();
        for (int i = 0; i < characterInvolved.traits.Count; i++) {
            if(characterInvolved.traits[i] is RelationshipTrait) {
                RelationshipTrait currTrait = characterInvolved.traits[i] as RelationshipTrait;
                if (currTrait.relType == RELATIONSHIP_TRAIT.ENEMY) {
                    enemies.Add(currTrait.targetCharacter);
                }
            }
        }
        if (enemies.Count > 0) {
            return enemies[UnityEngine.Random.Range(0, enemies.Count)];
        }
        return null;
    }
}
