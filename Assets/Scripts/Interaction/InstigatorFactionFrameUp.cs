using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstigatorFactionFrameUp : Interaction {

    private const string Start = "Start";
    private const string Incite_Anger = "Incite Anger";
    private const string Do_Nothing = "Do Nothing";

    private FactionToken _targetFactionToken;

    public InstigatorFactionFrameUp(Area interactable) : base(interactable, INTERACTION_TYPE.INSTIGATOR_FACTION_FRAME_UP, 0) {
        _name = "Instigator Faction Frame Up";
        _jobFilter = new JOB[] { JOB.INSTIGATOR };
    }

    #region Overrides
    public override void CreateStates() {
        _targetFactionToken = _tokenTrigger as FactionToken;

        InteractionState startState = new InteractionState(Start, this);
        InteractionState inciteAngerState = new InteractionState(Incite_Anger, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetFactionToken, _targetFactionToken.ToString(), LOG_IDENTIFIER.STRING_1);
        startStateDescriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1);
        startStateDescriptionLog.AddToFillers(_targetFactionToken.faction, _targetFactionToken.faction.name , LOG_IDENTIFIER.FACTION_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        inciteAngerState.SetEffect(() => InciteAngerEffect(inciteAngerState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(inciteAngerState.name, inciteAngerState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption inciteOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Incite anger against " + _targetFactionToken.nameInBold + ".",
                enabledTooltipText = "Reduce relationship between " + interactable.owner.name + " and " + _targetFactionToken.faction.name + ".",
                effect = () => InciteOption(state),
            };

            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do Nothing.",
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(inciteOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void InciteOption(InteractionState state) {
        SetCurrentState(_states[Incite_Anger]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region State Effects
    private void InciteAngerEffect(InteractionState state) {
        //investigatorCharacter.LevelUp();

        FactionRelationship relationship = interactable.owner.GetRelationshipWith(_targetFactionToken.faction);
        relationship.AdjustRelationshipStatus(-1);

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
