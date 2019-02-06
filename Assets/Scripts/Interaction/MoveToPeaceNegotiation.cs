using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPeaceNegotiation : Interaction {

    private Faction sourceFaction;
    private Faction targetFaction;
    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }

    private const string Diplomat_Killed_No_Witness = "Diplomat Killed No Witness";
    private const string Diplomat_Killed_Witnessed = "Diplomat Killed Witnessed";
    private const string Diplomat_Survives_Minion_Flees = "Diplomat Survives Minion Flees";
    private const string Diplomat_Survives_Minion_Dies = "Diplomat Survives Minion Dies";
    private const string Faction_Leader_Pursuaded = "Faction Leader Pursuaded";
    private const string Faction_Leader_Rejected = "Faction Leader Rejected";
    private const string Do_Nothing = "Do nothing";

    public override INTERACTION_TYPE pairedInteractionType {
        get { return INTERACTION_TYPE.CHARACTER_PEACE_NEGOTIATION; }
    }

    public MoveToPeaceNegotiation(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_PEACE_NEGOTIATION, 0) {
        _name = "Move to Peace Negotiation";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState diplomatKilledNoWitness = new InteractionState(Diplomat_Killed_No_Witness, this);
        InteractionState diplomatKilledWitnessed = new InteractionState(Diplomat_Killed_Witnessed, this);
        InteractionState diplomatSurvivesMinionFlees = new InteractionState(Diplomat_Survives_Minion_Flees, this);
        InteractionState diplomatSurvivesMinionDies = new InteractionState(Diplomat_Survives_Minion_Dies, this);
        InteractionState factionLeaderPursuaded = new InteractionState(Faction_Leader_Pursuaded, this);
        InteractionState factionLeaderRejected = new InteractionState(Faction_Leader_Rejected, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        sourceFaction = _characterInvolved.faction;
        targetFaction = GetTargetFaction();
        _targetArea = targetFaction.ownedAreas[Random.Range(0, targetFaction.ownedAreas.Count)];
        //**Text Description**: [Character Name] is about to leave for [Location Name 1] to scavenge for supplies.
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        diplomatKilledNoWitness.SetEffect(() => DiplomatKilledNoWitnessRewardEffect(diplomatKilledNoWitness));
        diplomatKilledWitnessed.SetEffect(() => DiplomatKilledWithWitnessRewardEffect(diplomatKilledWitnessed));
        diplomatSurvivesMinionFlees.SetEffect(() => DiplomatSurvivesMinionFleesRewardEffect(diplomatSurvivesMinionFlees));
        diplomatSurvivesMinionDies.SetEffect(() => DiplomatSurvivesMinionDiesRewardEffect(diplomatSurvivesMinionDies));
        factionLeaderPursuaded.SetEffect(() => FactionLeaderPursuadedRewardEffect(factionLeaderPursuaded));
        factionLeaderRejected.SetEffect(() => FactionLeaderRejectedRewardEffect(factionLeaderRejected));
        doNothing.SetEffect(() => DoNothingRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        _states.Add(diplomatKilledNoWitness.name, diplomatKilledNoWitness);
        _states.Add(diplomatKilledWitnessed.name, diplomatKilledWitnessed);
        _states.Add(diplomatSurvivesMinionFlees.name, diplomatSurvivesMinionFlees);
        _states.Add(diplomatSurvivesMinionDies.name, diplomatSurvivesMinionDies);
        _states.Add(factionLeaderPursuaded.name, factionLeaderPursuaded);
        _states.Add(factionLeaderRejected.name, factionLeaderRejected);
        _states.Add(doNothing.name, doNothing);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption kill = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Kill the diplomat.",
                duration = 0,
                effect = () => KillDiplomatOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Must have instigator minion.",
            };
            ActionOption convince = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Convince " + interactable.owner.leader.name + " otherwise.",
                duration = 0,
                effect = () => ConvinceOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Must have diplomat minion.",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(kill);
            state.AddActionOption(convince);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override void DoActionUponMoveToArrival() {
        CreateConnectedEvent(INTERACTION_TYPE.CHARACTER_PEACE_NEGOTIATION, targetArea);
    }
    #endregion

    #region Action Option Effect
    private void KillDiplomatOptionEffect(InteractionState state) {
        //Combat computation between the Minion and the Diplomat
        Combat combat = investigatorCharacter.currentParty.CreateCombatWith(_characterInvolved.party);
        combat.Fight(() => AttackCombatResult(combat));
    }
    private void ConvinceOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Faction_Leader_Pursuaded;
                break;
            case RESULT.FAIL:
                nextState = Faction_Leader_Rejected;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    private void AttackCombatResult(Combat combat) {
        WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        if (combat.winningSide == investigatorCharacter.currentSide) {
            //Minion won
            resultWeights.AddElement(Diplomat_Killed_No_Witness, 30);
            resultWeights.AddElement(Diplomat_Killed_Witnessed, 30);
        } else {
            //Diplomat won
            resultWeights.AddElement(Diplomat_Survives_Minion_Flees, 30);
            resultWeights.AddElement(Diplomat_Survives_Minion_Dies, 30);
        }
        string nextState = resultWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[nextState]);
    }

    #region Reward Effects
    private void DiplomatKilledNoWitnessRewardEffect(InteractionState state) {
        //**Mechanic**: Diplomat Dies, peace declaration cancelled.
        _characterInvolved.Death();
        //**Level Up**: Instigator Minion +1
        investigatorCharacter.LevelUp();

        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void DiplomatKilledWithWitnessRewardEffect(InteractionState state) {
        //**Mechanic**: Diplomat Dies, peace declaration cancelled, Player Favor Count -2 on Diplomat's Faction
        _characterInvolved.Death();
        _characterInvolved.faction.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, -2);

        //**Level Up**: Instigator Minion +1
        investigatorCharacter.LevelUp();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void DiplomatSurvivesMinionFleesRewardEffect(InteractionState state) {
        //**Mechanic**: Diplomat travels to [Location] for Peace Negotiation, Player Favor Count -2 on Diplomat's Faction
        StartMoveToAction();
        _characterInvolved.faction.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, -2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void DiplomatSurvivesMinionDiesRewardEffect(InteractionState state) {
        //**Mechanic**: Diplomat travels to [Location] for Peace Negotiation, Player Favor Count -2 on Diplomat's Faction
        StartMoveToAction();
        _characterInvolved.faction.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, -2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void FactionLeaderPursuadedRewardEffect(InteractionState state) {
        //**Level Up**: Diplomat Minion +1
        investigatorCharacter.LevelUp();
    }
    private void FactionLeaderRejectedRewardEffect(InteractionState state) {
        //**Mechanic**: Diplomat travels to [Location] for Peace Negotiation
        StartMoveToAction();

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //**Mechanic**: Diplomat travels to [Location] for Peace Negotiation
        StartMoveToAction();

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    #endregion

    //private void CreateEvent() {
    //    Interaction peaceInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CHARACTER_PEACE_NEGOTIATION, targetArea);
    //    _characterInvolved.SetForcedInteraction(peaceInteraction);
    //}

    private Faction GetTargetFaction() {
        List<Faction> choices = new List<Faction>();
        foreach (KeyValuePair<Faction, FactionRelationship> keyValuePair in sourceFaction.relationships) {
            if (keyValuePair.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY && keyValuePair.Value.currentWarCombatCount >= 3) {
                choices.Add(keyValuePair.Key);
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }
}
