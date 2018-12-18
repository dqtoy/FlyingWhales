using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolAction : Interaction {

    private const string Revealed_Patroller_Killed_Character = "Revealed Patroller Killed Character";
    private const string Revealed_Patroller_Injured_Character = "Revealed Patroller Injured Character";
    private const string Revealed_Character_Killed_Patroller = "Revealed Character Killed Patroller";
    private const string Revealed_Character_Injured_Patroller = "Revealed Character Injured Patroller";
    private const string Pursuaded_Patrol_Stopped = "Pursuaded Patrol Stopped";
    private const string Pursuaded_Patroller_Killed_Character = "Pursuaded Patroller Killed Character";
    private const string Pursuaded_Patroller_Injured_Character = "Pursuaded Patroller Injured Character";
    private const string Pursuaded_Character_Killed_Patroller = "Pursuaded Character Killed Patroller";
    private const string Pursuaded_Character_Injured_Patroller = "Pursuaded Character Injured Patroller";
    private const string Pursuaded_Patrol_Failed = "Pursuaded Patrol Failed";
    private const string Normal_Patroller_Killed_Character = "Normal Patroller Killed Character";
    private const string Normal_Patroller_Injured_Character = "Normal Patroller Injured Character";
    private const string Normal_Character_Killed_Patroller = "Normal Character Killed Patroller";
    private const string Normal_Character_Injured_Patroller = "Normal Character Injured Patroller";
    private const string Normal_Patrol_Failed = "Normal Patrol Failed";

    public PatrolAction(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.PATROL_ACTION, 0) {
        _name = "Patrol Action";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState revealedPatrollerKilledCharacter = new InteractionState(Revealed_Patroller_Killed_Character, this);
        InteractionState revealedPatrollerInjuredCharacter = new InteractionState(Revealed_Patroller_Injured_Character, this);
        InteractionState revealedCharacterKilledPatroller = new InteractionState(Revealed_Character_Killed_Patroller, this);
        InteractionState revealedCharacterInjuredPatroller = new InteractionState(Revealed_Character_Injured_Patroller, this);
        InteractionState pursuadedPatrolStopped = new InteractionState(Pursuaded_Patrol_Stopped, this);
        InteractionState pursuadedPatrollerKilledCharacter = new InteractionState(Pursuaded_Patroller_Killed_Character, this);
        InteractionState pursuadedPatrollerInjuredCharacter = new InteractionState(Pursuaded_Patroller_Injured_Character, this);
        InteractionState pursuadedCharacterKilledPatroller = new InteractionState(Pursuaded_Character_Killed_Patroller, this);
        InteractionState pursuadedCharacterInjuredPatroller = new InteractionState(Pursuaded_Character_Injured_Patroller, this);
        InteractionState pursuadedPatrolFailed = new InteractionState(Pursuaded_Patrol_Failed, this);
        InteractionState normalPatrollerKilledCharacter = new InteractionState(Normal_Patroller_Killed_Character, this);
        InteractionState normalPatrollerInjuredCharacter = new InteractionState(Normal_Patroller_Injured_Character, this);
        InteractionState normalCharacterKilledPatroller = new InteractionState(Normal_Character_Killed_Patroller, this);
        InteractionState normalCharacterInjuredPatroller = new InteractionState(Normal_Character_Injured_Patroller, this);
        InteractionState normalPatrolFailed = new InteractionState(Normal_Patrol_Failed, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        //revealedPatrollerKilledCharacter.SetEffect(() => DisruptedImproveRelationsSuccessRewardEffect(revealedPatrollerKilledCharacter));
        //revealedPatrollerInjuredCharacter.SetEffect(() => DisruptedImproveRelationsFailRewardEffect(revealedPatrollerInjuredCharacter));
        //revealedCharacterKilledPatroller.SetEffect(() => DisruptedImproveRelationsCriticallyFailRewardEffect(revealedPatrollerInjuredCharacter));
        //revealedCharacterInjuredPatroller.SetEffect(() => AssistedImproveRelationsSuccessRewardEffect(revealedCharacterInjuredPatroller));
        //pursuadedPatrolStopped.SetEffect(() => AssistedImproveRelationsFailRewardEffect(pursuadedPatrolStopped));
        //pursuadedPatrollerKilledCharacter.SetEffect(() => AssistedImproveRelationsCriticallyFailRewardEffect(pursuadedPatrolStopped));
        //pursuadedPatrollerInjuredCharacter.SetEffect(() => NormalImproveRelationsSuccessRewardEffect(pursuadedPatrollerInjuredCharacter));
        //pursuadedCharacterKilledPatroller.SetEffect(() => NormalImproveRelationsFailRewardEffect(pursuadedCharacterKilledPatroller));
        //pursuadedCharacterInjuredPatroller.SetEffect(() => NormalImproveRelationsCriticallyFailRewardEffect(pursuadedCharacterInjuredPatroller));

        _states.Add(startState.name, startState);
        _states.Add(revealedPatrollerKilledCharacter.name, revealedPatrollerKilledCharacter);
        _states.Add(revealedPatrollerInjuredCharacter.name, revealedPatrollerInjuredCharacter);
        _states.Add(revealedCharacterKilledPatroller.name, revealedCharacterKilledPatroller);
        _states.Add(revealedCharacterInjuredPatroller.name, revealedCharacterInjuredPatroller);
        _states.Add(pursuadedPatrolStopped.name, pursuadedPatrolStopped);
        _states.Add(pursuadedPatrollerKilledCharacter.name, pursuadedPatrollerKilledCharacter);
        _states.Add(pursuadedPatrollerInjuredCharacter.name, pursuadedPatrollerInjuredCharacter);
        _states.Add(pursuadedCharacterKilledPatroller.name, pursuadedCharacterKilledPatroller);
        _states.Add(pursuadedCharacterInjuredPatroller.name, pursuadedCharacterInjuredPatroller);
        _states.Add(pursuadedPatrolFailed.name, pursuadedPatrolFailed);
        _states.Add(normalPatrollerKilledCharacter.name, normalPatrollerKilledCharacter);
        _states.Add(normalPatrollerInjuredCharacter.name, normalPatrollerInjuredCharacter);
        _states.Add(normalCharacterKilledPatroller.name, normalCharacterKilledPatroller);
        _states.Add(normalCharacterInjuredPatroller.name, normalCharacterInjuredPatroller);
        _states.Add(normalPatrolFailed.name, normalPatrolFailed);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption reveal = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Reveal an enemy's whereabouts.",
                effect = () => PursuadeOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Minion must be an instigator",
            };
            ActionOption pursuade = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Pursuade to stop " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.POSSESSIVE, false) + " patrol.",
                //effect = () => AssistOptionEffect(state),
                jobNeeded = JOB.DISSUADER,
                doesNotMeetRequirementsStr = "Minion must be a dissuader",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                //effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(reveal);
            state.AddActionOption(pursuade);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Option Effects
    private void PursuadeOptionEffect(InteractionState state) {

    }
    #endregion
}
