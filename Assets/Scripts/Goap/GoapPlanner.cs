using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapPlanner {
    public Character actor { get; set; }

    public GoapPlanner(Character actor) {
        this.actor = actor;
    }

    public GoapPlan PlanActions(IPointOfInterest target, GoapAction goalAction, List<GoapAction> usableActions, GOAP_CATEGORY category, bool isPersonalPlan, ref string log, GoapPlanJob job = null) {
        //List of all starting nodes that can do the goal
        List<GoapNode> startingNodes = new List<GoapNode>();

        GoapNode goalNode = new GoapNode(null, goalAction.cost, goalAction);
        bool success = BuildGoapTree(goalNode, startingNodes, usableActions, ref log, job);
        if (!success) {
            return null;
        }

        GoapNode cheapestStartingNode = null;
        for (int i = 0; i < startingNodes.Count; i++) {
            if(cheapestStartingNode == null) {
                cheapestStartingNode = startingNodes[i];
            } else {
                if(startingNodes[i].runningCost < cheapestStartingNode.runningCost) {
                    cheapestStartingNode = startingNodes[i];
                }
            }
        }
        GOAP_EFFECT_CONDITION[] goalEffects = new GOAP_EFFECT_CONDITION[goalAction.expectedEffects.Count];
        for (int i = 0; i < goalAction.expectedEffects.Count; i++) {
            goalEffects[i] = goalAction.expectedEffects[i].conditionType;
        }
        GoapPlan plan = new GoapPlan(cheapestStartingNode, goalEffects, category, isPersonalPlan);
        return plan;
    }
    public bool RecalculatePathForPlan(GoapPlan currentPlan, List<GoapAction> usableActions) {
        //List of all starting nodes that can do the goal
        List<GoapNode> startingNodes = new List<GoapNode>();
        bool success = false;
        if (currentPlan.isPersonalPlan) {
            string log = string.Empty;
            success = BuildGoapTree(currentPlan.endNode, startingNodes, usableActions, ref log, currentPlan.job);
        } else {
            GoapNode currentLeafNode = null;
            bool hasUsableAction = false;
            success = true;
            for (int i = currentPlan.endNode.index; i >= 0; i--) {
                GoapNode failedNode = currentPlan.allNodes[i];
                hasUsableAction = false;
                for (int j = 0; j < usableActions.Count; j++) {
                    GoapAction currentUsableAction = usableActions[j];
                    if(failedNode.action.goapType == currentUsableAction.goapType && failedNode.action.poiTarget == currentUsableAction.poiTarget) {
                        hasUsableAction = true;
                        if(currentLeafNode == null) {
                            currentLeafNode = new GoapNode(failedNode.parent, failedNode.parent.runningCost + currentUsableAction.cost, currentUsableAction);
                        } else {
                            GoapNode leafNode = new GoapNode(currentLeafNode.parent, currentLeafNode.parent.runningCost + currentUsableAction.cost, currentUsableAction);
                            currentLeafNode = leafNode;
                        }
                        break;
                    }
                }
                if (!hasUsableAction) {
                    //No usable action for the current failed node, fail recalculation
                    success = false;
                    break;
                }
            }
            if(currentLeafNode != null) {
                startingNodes.Add(currentLeafNode);
            } else {
                success = false;
            }
        }
        if (!success) {
            return false;
        }

        GoapNode cheapestStartingNode = null;
        for (int i = 0; i < startingNodes.Count; i++) {
            if (cheapestStartingNode == null) {
                cheapestStartingNode = startingNodes[i];
            } else {
                if (startingNodes[i].runningCost < cheapestStartingNode.runningCost) {
                    cheapestStartingNode = startingNodes[i];
                }
            }
        }
        currentPlan.Reset(cheapestStartingNode);
        return true;
    }

    private bool BuildGoapTree(GoapNode parent, List<GoapNode> startingNodes, List<GoapAction> usableActions, ref string log, GoapPlanJob job = null) {
        if (parent == null) {
            return false;
        }
        log += "\nBuilding goap tree with parent " + parent.action.goapName;
        if(parent.action.preconditions.Count > 0) {
            List<Precondition> unsatisfiedPreconditions = new List<Precondition>();
            for (int i = 0; i < parent.action.preconditions.Count; i++) {
                Precondition precondition = parent.action.preconditions[i];
                if (!precondition.CanSatisfyCondition()) {
                    //Pool all unsatisfied preconditions
                    unsatisfiedPreconditions.Add(precondition);
                }
            }
            if (unsatisfiedPreconditions.Count > 0) {
                log += "\nChecking unsatisfied preconditions: ";
                for (int j = 0; j < unsatisfiedPreconditions.Count; j++) {
                    Precondition precon = unsatisfiedPreconditions[j];
                    log += "\n\t" + precon.goapEffect.conditionType.ToString() + " " + precon.goapEffect.conditionKey?.ToString() + " " + precon.goapEffect.targetPOI?.ToString() + " - ";
                }
                //Look for an action that can satisfy all unsatisfied preconditions
                //if one precondition cannot be satisfied, skip that action
                //if all preconditions can be satisfied, create a new goap tree for it
                log += "\nActions: ";
                List<GoapAction> shuffledActions = Utilities.Shuffle(usableActions);
                for (int i = 0; i < shuffledActions.Count; i++) {
                    GoapAction usableAction = shuffledActions[i];
                    bool canSatisfyAllPreconditions = true;
                    log += "\n\t" + usableAction.goapName;
                    for (int j = 0; j < unsatisfiedPreconditions.Count; j++) {
                        Precondition precon = unsatisfiedPreconditions[j];
                        if (!usableAction.WillEffectsSatisfyPrecondition(precon.goapEffect)) {
                            canSatisfyAllPreconditions = false;
                            break;
                        } else {
                            //if there is a provided job, check if it has any forced actions
                            //if it does, only allow the action that it allows
                            if (job != null && job.forcedActions.Count > 0) {
                                bool satisfiedForcedActions = true;
                                ForcedActionsComparer comparer = new ForcedActionsComparer();
                                foreach (KeyValuePair<GoapEffect, INTERACTION_TYPE> kvp in job.forcedActions) {
                                    if(comparer.Equals(kvp.Key, precon.goapEffect)) {
                                        if(kvp.Value != usableAction.goapType) {
                                            satisfiedForcedActions = false;
                                            break;
                                        }
                                    }
                                }
                                if (!satisfiedForcedActions) {
                                    canSatisfyAllPreconditions = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (canSatisfyAllPreconditions) {
                        GoapNode leafNode = new GoapNode(parent, parent.runningCost + usableAction.cost, usableAction);
                        log += " - Satisfied"; 
                        bool success = BuildGoapTree(leafNode, startingNodes, usableActions, ref log, job);
                        return success;
                    }
                }
                return false;
            } else {
                log += "\nNo unsatisfied preconditions";
                startingNodes.Add(parent);
                return true;
            }
        } else {
            log += "\nNo preconditions.";
            startingNodes.Add(parent);
            return true;
        }
        //return false;
    }

    private List<GoapAction> GetOrderedUsableActionsThatCanSatisfyPrecondition(GoapNode parent, GoapEffect precondition, List<GoapAction> usableActions) {
        List<GoapAction> arrangedUsableActions = new List<GoapAction>();
        for (int i = 0; i < usableActions.Count; i++) {
            GoapAction usableAction = usableActions[i];
            if (usableAction.WillEffectsSatisfyPrecondition(precondition)) {
                int cost = usableAction.cost;
                if (arrangedUsableActions.Count <= 0) {
                    arrangedUsableActions.Add(usableAction);
                } else {
                    bool isInserted = false;
                    for (int j = 0; j < arrangedUsableActions.Count; j++) {
                        if (parent.runningCost + cost < parent.runningCost + arrangedUsableActions[0].cost) {
                            isInserted = true;
                            arrangedUsableActions.Insert(0, usableAction);
                            break;
                        }
                    }
                    if (!isInserted) {
                        arrangedUsableActions.Add(usableAction);
                    }
                }
            }
        }
        return arrangedUsableActions;
    }
}