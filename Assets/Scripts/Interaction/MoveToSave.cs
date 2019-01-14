using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveToSave : Interaction {

    private const string Save_Cancelled = "Save Cancelled";
    private const string Save_Proceeds = "Save Proceeds";
    private const string Normal_Save = "Normal Save";

    public Area targetLocation { get; private set; }
    public Character targetCharacter { get; private set; }

    public MoveToSave(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.MOVE_TO_SAVE, 0) {
        _name = "Move To Save";
        _jobFilter = new JOB[] { JOB.DISSUADER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState saveCancelled = new InteractionState(Save_Cancelled, this);
        InteractionState saveProceeds = new InteractionState(Save_Proceeds, this);
        InteractionState normalSave = new InteractionState(Normal_Save, this);

        targetLocation = targetCharacter.specificLocation.tileLocation.areaOfTile;

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        saveCancelled.SetEffect(() => SaveCancelledRewardEffect(saveCancelled));
        saveProceeds.SetEffect(() => SaveProceedsRewardEffect(saveProceeds));
        normalSave.SetEffect(() => NormalSaveRewardEffect(normalSave));

        _states.Add(startState.name, startState);
        _states.Add(saveCancelled.name, saveCancelled);
        _states.Add(saveProceeds.name, saveProceeds);
        _states.Add(normalSave.name, normalSave);

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
    public override bool CanInteractionBeDoneBy(Character character) {
        targetCharacter = GetTargetCharacter(character);
        if (targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effects
    private void PreventFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorMinion.character.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Save_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Save_Proceeds;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Normal_Save]);
    }
    #endregion

    #region Reward Effects
    private void SaveCancelledRewardEffect(InteractionState state) {
        //**Mechanics**: Character will no longer leave.
        //**Level Up**: Dissuader Minion +1
        investigatorMinion.LevelUp();
        MinionSuccess();
    }
    private void SaveProceedsRewardEffect(InteractionState state) {
        GoToTargetLocation();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2);
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2));
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalSaveRewardEffect(InteractionState state) {
        GoToTargetLocation();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2);
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2));
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion

    private void GoToTargetLocation() {
        _characterInvolved.ownParty.GoToLocation(targetLocation.coreTile.landmarkOnTile, PATHFINDING_MODE.NORMAL, () => CreateEvent());
    }

    private void CreateEvent() {
        SaveAction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.SAVE_ACTION, _characterInvolved.specificLocation.tileLocation.landmarkOnTile) as SaveAction;
        interaction.SetTargetCharacter(targetCharacter);
        _characterInvolved.SetForcedInteraction(interaction);
    }

    private Character GetTargetCharacter(Character character) {
        /*
         A character travels to an area that his faction does not own that has at least one Abducted character that is not part of that area's faction.
         */
        List<Area> otherAreas = new List<Area>(LandmarkManager.Instance.allAreas.Where(x => x.owner != null && x.owner.id != character.faction.id)); //get all owned areas that are not owned by the characters faction
        WeightedDictionary<Character> choices = new WeightedDictionary<Character>();
        for (int i = 0; i < otherAreas.Count; i++) {
            Area currArea = otherAreas[i];
            for (int j = 0; j < currArea.charactersAtLocation.Count; j++) {
                Character currCharacter = currArea.charactersAtLocation[j];
                Abducted abductedTrait = currCharacter.GetTrait("Abducted") as Abducted;
                if (abductedTrait != null && currArea.owner.id != currCharacter.faction.id) { //check if character is abducted and that the area he/she is in is not owned by their faction
                    int weight = 0;
                    if (currCharacter.faction.id == character.faction.id) {
                        weight += 35; //- Abducted character is part of this character's faction and is located in another area not owned by the character's faction: Weight +35
                    } else {
                        FactionRelationship rel = character.faction.GetRelationshipWith(currCharacter.faction);
                        switch (rel.relationshipStatus) {
                            case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
                                weight += 10; //- Abducted character is part of a Neutral relationship faction and is located in another area not owned by the character's faction: Weight: +10
                                break;
                            case FACTION_RELATIONSHIP_STATUS.FRIEND:
                            case FACTION_RELATIONSHIP_STATUS.ALLY:
                                weight += 20; //- Abducted character is part of a Friend or Ally faction and is located in another area not owned by the character's faction: Weight: +20
                                break;
                            default:
                                break;
                        }
                    }

                    if (currCharacter.GetFriendTraitWith(character) != null) {
                        weight += 20; //- Abducted character is Friend of this character: Weight: +20
                    } else if (currCharacter.GetEnemyTraitWith(character) != null) {
                        weight -= 20; //- Abducted character is Enemy of this character: Weight: -20
                    }

                    if (weight > 0) {
                        choices.AddElement(currCharacter, weight);
                    }
                }

            }
        }
        if (choices.GetTotalOfWeights() > 0) {
            return choices.PickRandomElementGivenWeights();
        }
        return null;
    }
}
