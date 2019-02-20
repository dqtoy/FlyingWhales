using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionPeaceNegotiation : Interaction {

    private const string Start = "Start";
    private const string Peace_Negotiations_Successful = "Peace Negotiations Successful";
    private const string Peace_Negotiations_Failed = "Peace Negotiations Failed";
    private const string Do_Nothing = "Do Nothing";

    public MinionPeaceNegotiation(Area interactable) : base(interactable, INTERACTION_TYPE.MINION_PEACE_NEGOTIATION, 0) {
        _name = "Minion Peace Negotiation";
        _jobFilter = new JOB[] { JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState peaceNegotiationsSuccessState = new InteractionState(Peace_Negotiations_Successful, this);
        InteractionState peaceNegotiationsFailedState = new InteractionState(Peace_Negotiations_Failed, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(interactable.owner.leader, interactable.owner.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        peaceNegotiationsSuccessState.SetEffect(() => PeaceNegotiationsSuccessEffect(peaceNegotiationsSuccessState));
        peaceNegotiationsFailedState.SetEffect(() => PeaceNegotiationsFailEffect(peaceNegotiationsFailedState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(peaceNegotiationsSuccessState.name, peaceNegotiationsSuccessState);
        _states.Add(peaceNegotiationsFailedState.name, peaceNegotiationsFailedState);
        _states.Add(doNothingState.name, doNothingState);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption proceedOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Proceed.",
                duration = 0,
                effect = () => ProceedOption(),
            };
            ActionOption dontProceedOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "There will be no peace for now.",
                duration = 0,
                effect = () => DontProceedOption(),
            };

            state.AddActionOption(proceedOption);
            state.AddActionOption(dontProceedOption);
            state.SetDefaultOption(dontProceedOption);
        }
    }
    #endregion

    #region Action Options
    private void ProceedOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Peace_Negotiations_Successful, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Peace_Negotiations_Failed, investigatorCharacter.job.GetFailRate());

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DontProceedOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement("Do Nothing", 25);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region State Effects
    private void PeaceNegotiationsSuccessEffect(InteractionState state) {
        FactionManager.Instance.DeclarePeaceBetween(PlayerManager.Instance.player.playerFaction, interactable.owner);
        //investigatorCharacter.LevelUp();

        state.descriptionLog.AddToFillers(interactable.owner.leader, interactable.owner.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void PeaceNegotiationsFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(interactable.owner.leader, interactable.owner.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void DoNothingEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(interactable.owner.leader, interactable.owner.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion
}
