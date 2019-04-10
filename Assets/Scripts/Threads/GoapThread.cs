using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapThread : Multithread {
    public Character actor { get; private set; }
    public GoapPlan createdPlan { get; private set; }
    public GoapEffect goal { get; private set; }
    public IPointOfInterest target { get; private set; }
    public bool isPriority { get; private set; }
    public bool isPersonalPlan { get; private set; }
    public GOAP_CATEGORY category { get; private set; }
    public List<CharacterAwareness> characterTargetsAwareness { get; private set; }
    public GoapPlanJob job { get; private set; }
    //public List<INTERACTION_TYPE> actorAllowedActions { get; private set; }
    //public List<GoapAction> usableActions { get; private set; }
    public string log { get; private set; }

    //For recalculation
    public GoapPlan recalculationPlan;

    public GoapThread(Character actor, IPointOfInterest target, GoapEffect goal, GOAP_CATEGORY category, bool isPriority, List<CharacterAwareness> characterTargetsAwareness, bool isPersonalPlan, GoapPlanJob job) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.createdPlan = null;
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goal = goal;
        this.isPriority = isPriority;
        this.characterTargetsAwareness = characterTargetsAwareness;
        //this.actorAllowedActions = actorAllowedActions;
        //this.usableActions = usableActions;
        this.isPersonalPlan = isPersonalPlan;
        this.category = category;
        this.job = job;
    }
    public GoapThread(Character actor, GoapPlan currentPlan) {//, List<GoapAction> usableActions
        this.createdPlan = null;
        this.actor = actor;
        this.recalculationPlan = currentPlan;
        //this.actorAllowedActions = actorAllowedActions;
        //this.usableActions = usableActions;
    }
    #region Overrides
    public override void DoMultithread() {
        base.DoMultithread();
        try {
            CreatePlan();
        }catch(System.Exception e) {
            Debug.LogError("Problem with " + actor.name + "'s GoapThread!\n" + e.Message + "\n" + e.StackTrace);
        }
    }
    public override void FinishMultithread() {
        base.FinishMultithread();
        ReturnPlanFromGoapThread();
    }
    #endregion

    public void CreatePlan() {
        if(recalculationPlan != null) {
            RecalculatePlan();
        } else {
            CreateNewPlan();
        }
    }
    private void CreateNewPlan() {
        log = "-----------------RECEIVING NEW PLAN FROM OTHER THREAD OF " + actor.name + " WITH TARGET " + target.name + " (" + actor.specificLocation.name + ")-----------------------";
        log += "\nGOAL: " + goal.conditionType.ToString() + " - " + goal.conditionString() + ", target: " + goal.targetPOI.ToString();

        List<INTERACTION_TYPE> actorAllowedActions = RaceManager.Instance.GetNPCInteractionsOfRace(actor);
        List<GoapAction> usableActions = new List<GoapAction>();
        foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IAwareness>> kvp in actor.awareness) {
            if (kvp.Key == POINT_OF_INTEREST_TYPE.CHARACTER) {
                for (int i = 0; i < kvp.Value.Count; i++) {
                    Character character = kvp.Value[i].poi as Character;
                    if (character.isDead) {
                        //kvp.Value.RemoveAt(i);
                        //i--;
                    } else {
                        if (character.specificLocation == actor.specificLocation || actor.IsPOIInCharacterAwarenessList(character, characterTargetsAwareness)) {
                            List<GoapAction> awarenessActions = kvp.Value[i].poi.AdvertiseActionsToActor(actor, actorAllowedActions);
                            if (awarenessActions != null && awarenessActions.Count > 0) {
                                usableActions.AddRange(awarenessActions);
                            }
                        }
                    }
                }
            } else {
                for (int i = 0; i < kvp.Value.Count; i++) {
                    List<GoapAction> awarenessActions = kvp.Value[i].poi.AdvertiseActionsToActor(actor, actorAllowedActions);
                    if (awarenessActions != null && awarenessActions.Count > 0) {
                        usableActions.AddRange(awarenessActions);
                    }
                }
            }
        }

        log += "\nUSABLE ACTIONS: ";
        List<GoapPlan> allPlans = new List<GoapPlan>();
        for (int i = 0; i < usableActions.Count; i++) {
            if (i > 0) {
                log += ", ";
            }
            log += usableActions[i].goapName + " (" + usableActions[i].poiTarget.name + ")";
            if (usableActions[i].WillEffectsSatisfyPrecondition(goal)) {
                GoapPlan plan = actor.planner.PlanActions(target, usableActions[i], usableActions, category, isPersonalPlan);
                if (plan != null) {
                    allPlans.Add(plan);
                }
            }
        }

        log += "\nALL GENERATED PLANS: ";
        if (allPlans.Count > 0) {
            GoapPlan shortestPathToGoal = null;
            for (int i = 0; i < allPlans.Count; i++) {
                log += "\n" + (i + 1) + ". (" + allPlans[i].startingNode.runningCost + ") ";
                for (int j = 0; j < allPlans[i].allNodes.Count; j++) {
                    if (j > 0) {
                        log += ", ";
                    }
                    log += allPlans[i].allNodes[j].action.goapName + " - " + allPlans[i].allNodes[j].action.poiTarget.name;
                }
                if (shortestPathToGoal == null) {
                    shortestPathToGoal = allPlans[i];
                } else {
                    if (allPlans[i].startingNode.runningCost < shortestPathToGoal.startingNode.runningCost) {
                        shortestPathToGoal = allPlans[i];
                    }
                }
            }
            shortestPathToGoal.SetListOfCharacterAwareness(characterTargetsAwareness);
            log += shortestPathToGoal.LogPlan();
            createdPlan = shortestPathToGoal;
        } else {
            log += "\nNO PLAN WAS GENERATED! End goap...";
        }
    }
    private void RecalculatePlan() {
        //if (recalculationPlan.isEnd) {
        //    return;
        //}
        log = "-----------------RECALCULATING PLAN OF " + actor.name + " WITH TARGET " + recalculationPlan.target.name + " (" + actor.specificLocation.name + ")-----------------------";
        log += "\nGOAL ACTION: " + recalculationPlan.endNode.action.goapName + " - " + recalculationPlan.endNode.action.poiTarget.name;

        List<GoapAction> usableActions = new List<GoapAction>();
        List<INTERACTION_TYPE> actorAllowedActions = RaceManager.Instance.GetNPCInteractionsOfRace(actor);
        foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IAwareness>> kvp in actor.awareness) {
            if (kvp.Key == POINT_OF_INTEREST_TYPE.CHARACTER) {
                for (int i = 0; i < kvp.Value.Count; i++) {
                    Character character = kvp.Value[i].poi as Character;
                    if (character.isDead) {
                        //kvp.Value.RemoveAt(i);
                        //i--;
                    } else {
                        if (character.gridTileLocation.structure == actor.currentStructure || actor.IsPOIInCharacterAwarenessList(character, recalculationPlan.goalCharacterTargets)) {
                            List<GoapAction> awarenessActions = kvp.Value[i].poi.AdvertiseActionsToActor(actor, actorAllowedActions);
                            if (awarenessActions != null && awarenessActions.Count > 0) {
                                usableActions.AddRange(awarenessActions);
                            }
                        }
                    }
                }
            } else {
                for (int i = 0; i < kvp.Value.Count; i++) {
                    List<GoapAction> awarenessActions = kvp.Value[i].poi.AdvertiseActionsToActor(actor, actorAllowedActions);
                    if (awarenessActions != null && awarenessActions.Count > 0) {
                        usableActions.AddRange(awarenessActions);
                    }
                }
            }
        }

        log += "\nUSABLE ACTIONS: ";
        if (usableActions.Count > 0) {
            for (int i = 0; i < usableActions.Count; i++) {
                if (i > 0) {
                    log += ", ";
                }
                log += usableActions[i].goapName + " (" + usableActions[i].poiTarget.name + ")";
            }
            bool success = actor.planner.RecalculatePathForPlan(recalculationPlan, usableActions);
            if (success) {
                log += "\nSUCCESSFULLY RECALCULATED PLAN!";
                log += recalculationPlan.LogPlan();
                createdPlan = recalculationPlan;
            } else {
                log += "\nFAILED TO RECALCULATE PLAN!";
            }
        } else {
            log += "\nNO USABLE ACTIONS! FAILED TO RECALCULATE PLAN!";
        }
    }
    public void ReturnPlanFromGoapThread() {
        actor.ReceivePlanFromGoapThread(this);
    }
}
