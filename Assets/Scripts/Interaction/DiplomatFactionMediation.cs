using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiplomatFactionMediation : Interaction {

    private const string Start = "Start";
    private const string Improve_Relationship = "Improve Relationship";
    private const string Do_Nothing = "Do Nothing";

    private FactionToken _targetFactionToken;

    public DiplomatFactionMediation(Area interactable) : base(interactable, INTERACTION_TYPE.DIPLOMAT_FACTION_MEDIATION, 0) {
        _name = "Diplomat Faction Mediation";
        _jobFilter = new JOB[] { JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        _targetFactionToken = _tokenTrigger as FactionToken;

        InteractionState startState = new InteractionState(Start, this);
        InteractionState improveRelationshipState = new InteractionState(Improve_Relationship, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetFactionToken, _targetFactionToken.ToString(), LOG_IDENTIFIER.STRING_1);
        startStateDescriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1);
        startStateDescriptionLog.AddToFillers(_targetFactionToken.faction, _targetFactionToken.faction.name, LOG_IDENTIFIER.FACTION_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        improveRelationshipState.SetEffect(() => ImproveRelationshipEffect(improveRelationshipState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(improveRelationshipState.name, improveRelationshipState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption improveOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Improve relationship between " + interactable.owner.name + " and " + _targetFactionToken.faction.name + ".",
                enabledTooltipText = "Improve relationship between " + interactable.owner.name + " and " + _targetFactionToken.faction.name + ".",
                effect = () => ImproveOption(state),
            };

            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do Nothing.",
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(improveOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void ImproveOption(InteractionState state) {
        SetCurrentState(_states[Improve_Relationship]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region State Effects
    private void ImproveRelationshipEffect(InteractionState state) {
        investigatorCharacter.LevelUp();

        FactionRelationship relationship = interactable.owner.GetRelationshipWith(_targetFactionToken.faction);
        relationship.AdjustRelationshipStatus(1);

        state.descriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(_targetFactionToken.faction, _targetFactionToken.faction.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(_targetFactionToken.faction, _targetFactionToken.faction.name, LOG_IDENTIFIER.FACTION_2));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(relationship.relationshipStatus.ToString()), LOG_IDENTIFIER.STRING_1));

    }
    private void DoNothingEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(null, _targetFactionToken.ToString(), LOG_IDENTIFIER.STRING_1);
        state.AddLogFiller(new LogFiller(null, _targetFactionToken.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    #endregion
}
