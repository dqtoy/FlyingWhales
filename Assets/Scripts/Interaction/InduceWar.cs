using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InduceWar : Interaction {

    private Faction sourceFaction;
    private Faction targetFaction;

    private const string Induce_War_Success = "Induce War Success";
    private const string Induce_War_Fail = "Induce War Fail";
    private const string Do_Nothing = "Do nothing";

    public InduceWar(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.INDUCE_WAR, 0) {
        _name = "Induce War";
        _jobFilter = new JOB[] { JOB.INSTIGATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState induceWarSuccess = new InteractionState(Induce_War_Success, this);
        InteractionState induceWarFail = new InteractionState(Induce_War_Fail, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        sourceFaction = interactable.owner;
        //**Mechanics**: Select another Faction satisfying the conditions.
        targetFaction = GetTargetFaction();

        //**Text Description**: [Demon Name] has concocted a plan that will further enrage the people of [Faction Name 1] 
        //against [Faction Name 2]. On your signal, [he/she] will start setting it up. Tempers are already high between the two 
        //factions. Doing this will likely trigger a war between the two.
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(sourceFaction, sourceFaction.name, LOG_IDENTIFIER.FACTION_1);
        startStateDescriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        induceWarSuccess.SetEffect(() => InduceWarSuccessRewardEffect(induceWarSuccess));
        induceWarFail.SetEffect(() => InduceWarFailRewardEffect(induceWarFail));
        doNothing.SetEffect(() => DoNothingRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        _states.Add(induceWarSuccess.name, induceWarSuccess);
        _states.Add(induceWarFail.name, induceWarFail);
        _states.Add(doNothing.name, doNothing);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNow = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 100, currency = CURRENCY.SUPPLY },
                name = "What are you waiting for? Do it now!",
                duration = 0,
                needsMinion = false,
                effect = () => DoNowOptionEffect(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Not yet.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(doNow);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Action Option Effects
    private void DoNowOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Induce_War_Success;
                break;
            case RESULT.FAIL:
                nextState = Induce_War_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region End Effects
    private void InduceWarSuccessRewardEffect(InteractionState state) {
        //**Mechanics**: Faction 1 declares war on Faction 2
        FactionManager.Instance.DeclareWarBetween(sourceFaction, targetFaction);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(sourceFaction, sourceFaction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        }

        //**Level Up**: Instigator Minion +1
        _characterInvolved.LevelUp();

        state.AddLogFiller(new LogFiller(sourceFaction, sourceFaction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void InduceWarFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(sourceFaction, sourceFaction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(sourceFaction, sourceFaction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void DoNothingRewardEffect(InteractionState state) {

    }
    #endregion

    private Faction GetTargetFaction() {
        List<Faction> choices = new List<Faction>();
        foreach (KeyValuePair<Faction, int> kvp in sourceFaction.favor) {
            if (kvp.Key.id != PlayerManager.Instance.player.playerFaction.id && kvp.Value <= -10) {
                choices.Add(kvp.Key);
            }
        }
        return choices[Random.Range(0, choices.Count)];
    }
}
