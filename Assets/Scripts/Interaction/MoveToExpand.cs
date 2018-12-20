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

    public void SetTargetLocation(Area area) {
        targetLocation = area;
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState characterExpandCancelled = new InteractionState(Character_Expand_Cancelled, this);
        InteractionState characterExpandContinues = new InteractionState(Character_Expand_Continues, this);
        InteractionState characterNormalExpand = new InteractionState(Character_Normal_Expand, this);

        if(targetLocation == null) {
            targetLocation = GetTargetLocation();
        }
        targetLocation.AddEventTargettingThis(this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(null, Utilities.NormalizeString(_characterInvolved.race.ToString()), LOG_IDENTIFIER.STRING_1);
        startStateDescriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        characterExpandCancelled.SetEffect(() => CharacterExpandCancelledRewardEffect(characterExpandCancelled));
        characterExpandContinues.SetEffect(() => CharacterExpandContinuesRewardEffect(characterExpandContinues));
        characterNormalExpand.SetEffect(() => CharacterNormalExpandRewardEffect(characterNormalExpand));

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
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from leaving.",
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
        WeightedDictionary<RESULT> resultWeights = investigatorMinion.character.job.GetJobRateWeights();
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
    private void CharacterExpandCancelledRewardEffect(InteractionState state) {
        //**Mechanics**: Character will no longer leave.
        //**Level Up**: Dissuader Minion +1
        investigatorMinion.LevelUp();
        MinionSuccess();
    }
    private void CharacterExpandContinuesRewardEffect(InteractionState state) {
        //**Mechanics**: Character travels to the Location to start an Expansion event.
        GoToTargetLocation();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void CharacterNormalExpandRewardEffect(InteractionState state) {
        //**Mechanics**: Character travels to the Location to start an Expansion event.
        GoToTargetLocation();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    #endregion

    private void GoToTargetLocation() {
        _characterInvolved.ownParty.GoToLocation(targetLocation.coreTile.landmarkOnTile, PATHFINDING_MODE.NORMAL, () => CreateExpansionEvent());
    }

    private void CreateExpansionEvent() {
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.EXPANSION_EVENT, _characterInvolved.specificLocation.tileLocation.landmarkOnTile);
        _characterInvolved.SetForcedInteraction(interaction);
        interaction.SetCanInteractionBeDoneAction(IsExpansionStillValid);
        targetLocation.RemoveEventTargettingThis(this);
    }

    private bool IsExpansionStillValid() {
        return _characterInvolved.specificLocation.tileLocation.areaOfTile != null && _characterInvolved.specificLocation.tileLocation.areaOfTile.owner == null;
    }

    private Area GetTargetLocation() {
        //List<Area> choices = _characterInvolved.homeLandmark.tileLocation.areaOfTile.GetElligibleExpansionTargets(_characterInvolved);
        //if (choices.Count > 0) {
        //    return choices[Random.Range(0, choices.Count)];
        //}
        List<Area> choices = new List<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area area = LandmarkManager.Instance.allAreas[i];
            if (area.id != _characterInvolved.specificLocation.tileLocation.areaOfTile.id 
                && area.owner == null && area.possibleOccupants.Contains(_characterInvolved.race)) {
                choices.Add(area);
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        throw new System.Exception(_characterInvolved.name + " Could not find target location for expand for " + _characterInvolved.faction.name);
    }
}
