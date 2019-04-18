using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccupyActionFaction : Interaction {

    private const string Normal_Expansion = "Normal Expansion";

    public OccupyActionFaction(Area interactable)
        : base(interactable, INTERACTION_TYPE.OCCUPY_ACTION_FACTION, 0) {
        _name = "Occupy Action Faction";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState normalExpansionSuccess = new InteractionState(Normal_Expansion, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        normalExpansionSuccess.SetEffect(() => NormalExpansionSuccessRewardEffect(normalExpansionSuccess));

        _states.Add(startState.name, startState);
        _states.Add(normalExpansionSuccess.name, normalExpansionSuccess);

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
        if (interactable.owner != null 
            && interactable.IsResidentsFull(character)) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Normal_Expansion]);
    }
    #endregion

    #region Reward Effect
    private void NormalExpansionSuccessRewardEffect(InteractionState state) {
        //**Mechanic**: Location becomes part of Character's faction and its Race will be set as Character's Race
        OwnArea(_characterInvolved);
        //**Mechanic**: Character home will be transferred to this location and his current structure will be his new home.
        _characterInvolved.MigrateHomeTo(interactable);
    }
    #endregion

    private void OwnArea(Character character) {
        Area area = interactable;
        if (area.owner == null) {
            FactionManager.Instance.neutralFaction.RemoveFromOwnedAreas(area);
        }
        LandmarkManager.Instance.OwnArea(character.faction, character.race, area);
    }
}
