using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToHangOut : Interaction {

    private const string Hang_Out_Cancelled = "Hang Out Cancelled";
    private const string Hang_Out_Continues = "Hang Out Continues";
    private const string Do_Nothing = "Do Nothing";

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }
    public override INTERACTION_TYPE pairedInteractionType {
        get { return INTERACTION_TYPE.HANG_OUT_ACTION; }
    }
    public Character targetCharacter { get; private set; }

    public MoveToHangOut(Area interactable) 
        : base(interactable, INTERACTION_TYPE.MOVE_TO_HANG_OUT_ACTION, 0) {
        _name = "Move To Hang Out";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState hangOutCancelled = new InteractionState(Hang_Out_Cancelled, this);
        InteractionState hangOutContinues = new InteractionState(Hang_Out_Continues, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        targetCharacter = GetTargetCharacter(_characterInvolved);
        _targetArea = targetCharacter.specificLocation;

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        startStateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        hangOutCancelled.SetEffect(() => HangOutCancelledRewardEffect(hangOutCancelled));
        hangOutContinues.SetEffect(() => HangOutContinuesRewardEffect(hangOutContinues));
        doNothing.SetEffect(() => DoNothingRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        _states.Add(hangOutCancelled.name, hangOutCancelled);
        _states.Add(hangOutContinues.name, hangOutContinues);
        _states.Add(doNothing.name, doNothing);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from leaving.",
                duration = 0,
                effect = () => PreventFromLeavingOptionEffect(state),
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Must have debilitator minion.",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(prevent);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (GetTargetCharacter(character) == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void DoActionUponMoveToArrival() {
        Interaction interaction = CreateConnectedEvent(INTERACTION_TYPE.HANG_OUT_ACTION, _targetArea);
        (interaction as HangOutAction).SetTargetCharacter(targetCharacter);
    }
    #endregion

    #region Option Effects
    private void PreventFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Hang_Out_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Hang_Out_Continues;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region Reward Effects
    private void HangOutCancelledRewardEffect(InteractionState state) {
        //**Mechanics**: Character will no longer leave.
        //**Level Up**: Dissuader Minion +1
        investigatorCharacter.LevelUp();
        MinionSuccess();
    }
    private void HangOutContinuesRewardEffect(InteractionState state) {
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion

    //private void CreateEvent() {
    //    Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.HANG_OUT_ACTION, _targetArea);
    //    (interaction as HangOutAction).SetTargetCharacter(targetCharacter);
    //    _characterInvolved.SetForcedInteraction(interaction);
    //}
    private Character GetTargetCharacter(Character character) {
        WeightedDictionary<Character> weights = new WeightedDictionary<Character>();
        foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in character.relationships) {
            int weight = 0;
            for (int i = 0; i < kvp.Value.rels.Count; i++) {
                RelationshipTrait currRel = kvp.Value.rels[i];
                switch (currRel.effect) {
                    case TRAIT_EFFECT.NEUTRAL:
                        weight += 25; //- for each neutral relationship with the character: +25 weight
                        break;
                    case TRAIT_EFFECT.POSITIVE:
                        weight += 50; //- for each positive relationship with the character: +50 weight
                        break;
                    case TRAIT_EFFECT.NEGATIVE:
                        weight -= 25; //- for each negative relationship with the character: -25 weight
                        break;
                    default:
                        break;
                }
            }
            if (weight > 0) {
                weights.AddElement(kvp.Key, weight);
            }
        }

        if (weights.GetTotalOfWeights() > 0) {
            return weights.PickRandomElementGivenWeights();
        }
        return null;
    }
}
