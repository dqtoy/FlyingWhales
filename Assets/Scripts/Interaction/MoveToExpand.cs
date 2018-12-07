using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveToExpand : Interaction {

    private const string Character_Expand_Cancelled = "Character Expand Cancelled";
    private const string Character_Expand_Continues = "Character Expand Continues";
    private const string Character_Normal_Expand = "Character Normal Expand";

    public Area targetLocation { get; private set; }

    public MoveToExpand(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_EXPAND, 0) {
        _name = "Move to Expand";
        _jobFilter = new JOB[] { JOB.DISSUADER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState characterExpandCancelled = new InteractionState(Character_Expand_Cancelled, this);
        InteractionState characterExpandContinues = new InteractionState(Character_Expand_Continues, this);
        InteractionState characterNormalExpand = new InteractionState(Character_Normal_Expand, this);

        targetLocation = GetTargetLocation();
        targetLocation.AddEventTargettingThis(this);
        _characterInvolved.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(-100);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        characterExpandCancelled.SetEffect(() => CharacterExploreCancelledRewardEffect(characterExpandCancelled));
        characterExpandContinues.SetEffect(() => CharacterExploreContinuesRewardEffect(characterExpandContinues));
        characterNormalExpand.SetEffect(() => DoNothingRewardEffect(characterNormalExpand));

        _states.Add(startState.name, startState);
        _states.Add(characterExpandCancelled.name, characterExpandCancelled);
        _states.Add(characterExpandContinues.name, characterExpandContinues);
        _states.Add(characterNormalExpand.name, characterNormalExpand);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Discourage them from leaving.",
                duration = 0,
                effect = () => DiscourageFromLeavingOptionEffect(state),
                jobNeeded = JOB.DISSUADER,
                doesNotMeetRequirementsStr = "Minion must be a dissuader",
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
    #endregion

    #region Option Effects
    private void DiscourageFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = explorerMinion.character.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Character_Expand_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Character_Expand_Continues;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Character_Normal_Expand]);
    }
    #endregion

    #region Reward Effects
    private void CharacterExploreCancelledRewardEffect(InteractionState state) {
        //**Mechanics**: Character will no longer leave.
        //**Level Up**: Dissuader Minion +1
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        explorerMinion.LevelUp();
    }
    private void CharacterExploreContinuesRewardEffect(InteractionState state) {
        //**Mechanics**: Character travels to the Location to start an Expansion event.
        GoToTargetLocation();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //**Mechanics**: Character travels to the Location to start an Expansion event.
        GoToTargetLocation();
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    #endregion

    private void GoToTargetLocation() {
        _characterInvolved.ownParty.GoToLocation(targetLocation.coreTile.landmarkOnTile, PATHFINDING_MODE.NORMAL, () => CreateExpansionEvent());
    }

    private void CreateExpansionEvent() {
        //TODO: Create expansion event
        targetLocation.RemoveEventTargettingThis(this);
    }

    private Area GetTargetLocation() {
        List<Area> choices = _characterInvolved.homeLandmark.tileLocation.areaOfTile.GetElligibleExpansionTargets(_characterInvolved);
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        throw new System.Exception("Could not find target location for expand");
    }
}
