using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToExplore : Interaction {

    public MoveToExplore(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_EXPLORE, 0) {
        _name = "Move to Explore";
        _jobFilter = new JOB[] { JOB.DISSUADER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        //InteractionState diplomatKilledNoWitness = new InteractionState(Diplomat_Killed_No_Witness, this);
        //InteractionState diplomatKilledWitnessed = new InteractionState(Diplomat_Killed_Witnessed, this);
        //InteractionState diplomatSurvivesMinionFlees = new InteractionState(Diplomat_Survives_Minion_Flees, this);
        //InteractionState diplomatSurvivesMinionDies = new InteractionState(Diplomat_Survives_Minion_Dies, this);
        //InteractionState factionLeaderPursuaded = new InteractionState(Faction_Leader_Pursuaded, this);
        //InteractionState factionLeaderRejected = new InteractionState(Faction_Leader_Rejected, this);
        //InteractionState doNothing = new InteractionState(Do_Nothing, this);

        //sourceFaction = _characterInvolved.faction;
        //targetFaction = GetTargetFaction();
        //targetLocation = targetFaction.ownedAreas[Random.Range(0, targetFaction.ownedAreas.Count)];
        ////**Text Description**: [Character Name] is about to leave for [Location Name 1] to scavenge for supplies.
        //Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        //startStateDescriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_1);
        //startState.OverrideDescriptionLog(startStateDescriptionLog);

        //CreateActionOptions(startState);
        //diplomatKilledNoWitness.SetEffect(() => DiplomatKilledNoWitnessRewardEffect(diplomatKilledNoWitness));
        //diplomatKilledWitnessed.SetEffect(() => DiplomatKilledWithWitnessRewardEffect(diplomatKilledWitnessed));
        //diplomatSurvivesMinionFlees.SetEffect(() => DiplomatSurvivesMinionFleesRewardEffect(diplomatSurvivesMinionFlees));
        //diplomatSurvivesMinionDies.SetEffect(() => DiplomatSurvivesMinionDiesRewardEffect(diplomatSurvivesMinionDies));
        //factionLeaderPursuaded.SetEffect(() => FactionLeaderPursuadedRewardEffect(factionLeaderPursuaded));
        //factionLeaderRejected.SetEffect(() => FactionLeaderRejectedRewardEffect(factionLeaderRejected));
        //doNothing.SetEffect(() => DoNothingRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        //_states.Add(diplomatKilledNoWitness.name, diplomatKilledNoWitness);
        //_states.Add(diplomatKilledWitnessed.name, diplomatKilledWitnessed);
        //_states.Add(diplomatSurvivesMinionFlees.name, diplomatSurvivesMinionFlees);
        //_states.Add(diplomatSurvivesMinionDies.name, diplomatSurvivesMinionDies);
        //_states.Add(factionLeaderPursuaded.name, factionLeaderPursuaded);
        //_states.Add(factionLeaderRejected.name, factionLeaderRejected);
        //_states.Add(doNothing.name, doNothing);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption kill = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Kill the diplomat.",
                duration = 0,
                //effect = () => KillDiplomatOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Minion must be a instigator",
            };
            ActionOption convince = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Convince " + interactable.owner.leader.name + " otherwise.",
                duration = 0,
                //effect = () => ConvinceOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Minion must be a diplomat",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                //effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(kill);
            state.AddActionOption(convince);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion
}
