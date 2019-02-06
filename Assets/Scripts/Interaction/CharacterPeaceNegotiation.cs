using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPeaceNegotiation : Interaction {

    private const string Start = "Start";
    private const string Negotiations_Not_Disrupted = "Negotiations Not Disrupted";
    private const string Negotiations_Disrupted = "Negotiations Disrupted";
    private const string Negotiations_Improved = "Negotiations Improved";
    private const string Negotiations_Not_Improved = "Negotiations Not Improved";
    private const string Normal_Negotiations_Success = "Normal Negotiations Success";
    private const string Normal_Negotiations_Fail = "Normal Negotiations Fail";

    public CharacterPeaceNegotiation(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_PEACE_NEGOTIATION, 0) {
        _name = "Character Peace Negotiation";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState negotiationsNotDisruptedState = new InteractionState(Negotiations_Not_Disrupted, this);
        InteractionState negotiationsDisruptedState = new InteractionState(Negotiations_Disrupted, this);
        InteractionState negotiationsImprovedState = new InteractionState(Negotiations_Improved, this);
        InteractionState negotiationsNotImprovedState = new InteractionState(Negotiations_Not_Improved, this);
        InteractionState normalNegotiationsSuccessState = new InteractionState(Normal_Negotiations_Success, this);
        InteractionState normalNegotiationsFailState = new InteractionState(Normal_Negotiations_Fail, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);


        CreateActionOptions(startState);

        negotiationsNotDisruptedState.SetEffect(() => NegotiationsNotDisruptedEffect(negotiationsNotDisruptedState));
        negotiationsDisruptedState.SetEffect(() => NegotiationsDisruptedEffect(negotiationsDisruptedState));
        negotiationsImprovedState.SetEffect(() => NegotiationsImprovedEffect(negotiationsImprovedState));
        negotiationsNotImprovedState.SetEffect(() => NegotiationsNotImprovedEffect(negotiationsNotImprovedState));
        normalNegotiationsSuccessState.SetEffect(() => NormalNegotiationsSuccessEffect(normalNegotiationsSuccessState));
        normalNegotiationsFailState.SetEffect(() => NormalNegotiationsFailEffect(normalNegotiationsFailState));

        _states.Add(startState.name, startState);
        _states.Add(negotiationsNotDisruptedState.name, negotiationsNotDisruptedState);
        _states.Add(negotiationsDisruptedState.name, negotiationsDisruptedState);
        _states.Add(negotiationsImprovedState.name, negotiationsImprovedState);
        _states.Add(negotiationsNotImprovedState.name, negotiationsNotImprovedState);
        _states.Add(normalNegotiationsSuccessState.name, normalNegotiationsSuccessState);
        _states.Add(normalNegotiationsFailState.name, normalNegotiationsFailState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption disruptOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Disrupt the negotiations.",
                duration = 0,
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Must have instigator minion.",
                effect = () => DisruptOption(),
            };
            ActionOption ensureOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.MANA },
                name = "Ensure the negotiations go by smoothly.",
                duration = 0,
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Must have diplomat minion.",
                effect = () => EnsureOption(),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(disruptOption);
            state.AddActionOption(ensureOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void DisruptOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Success", investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement("Fail", investigatorCharacter.job.GetFailRate());
        string instigatorResult = effectWeights.PickRandomElementGivenWeights();

        int failModifier = 0;
        if(instigatorResult == "Success") {
            failModifier = 30;
        }
        effectWeights.Clear();
        effectWeights.AddElement(Negotiations_Not_Disrupted, characterInvolved.job.GetSuccessRate());
        effectWeights.AddElement(Negotiations_Disrupted, characterInvolved.job.GetFailRate() + failModifier);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void EnsureOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Success", investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement("Fail", investigatorCharacter.job.GetFailRate());
        string instigatorResult = effectWeights.PickRandomElementGivenWeights();

        int successModifier = 0;
        if (instigatorResult == "Success") {
            successModifier = 30;
        }
        effectWeights.Clear();
        effectWeights.AddElement(Negotiations_Improved, characterInvolved.job.GetSuccessRate() + successModifier);
        effectWeights.AddElement(Negotiations_Not_Improved, characterInvolved.job.GetFailRate());

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Normal_Negotiations_Success, characterInvolved.job.GetSuccessRate());
        effectWeights.AddElement(Normal_Negotiations_Fail, characterInvolved.job.GetFailRate());

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    #endregion

    #region State Effects
    private void NegotiationsNotDisruptedEffect(InteractionState state) {
        FactionManager.Instance.DeclarePeaceBetween(characterInvolved.faction, interactable.owner);
        //characterInvolved.LevelUp();

        state.descriptionLog.AddToFillers(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void NegotiationsDisruptedEffect(InteractionState state) {
        //investigatorCharacter.LevelUp();

        state.descriptionLog.AddToFillers(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void NegotiationsImprovedEffect(InteractionState state) {
        FactionManager.Instance.DeclarePeaceBetween(characterInvolved.faction, interactable.owner);
        //investigatorCharacter.LevelUp();
        //characterInvolved.LevelUp();

        state.descriptionLog.AddToFillers(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void NegotiationsNotImprovedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void NormalNegotiationsSuccessEffect(InteractionState state) {
        FactionManager.Instance.DeclarePeaceBetween(characterInvolved.faction, interactable.owner);
        //characterInvolved.LevelUp();

        state.descriptionLog.AddToFillers(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void NormalNegotiationsFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_2));
    }
    #endregion
}
