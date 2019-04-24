using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapPlanner {
    public Character actor { get; set; }

    public GoapPlanner(Character actor) {
        this.actor = actor;
    }

    public GoapPlan PlanActions(IPointOfInterest target, GoapAction goalAction, List<GoapAction> usableActions, GOAP_CATEGORY category, bool isPersonalPlan, GoapPlanJob job = null) {
        //List of all starting nodes that can do the goal
        List<GoapNode> startingNodes = new List<GoapNode>();

        GoapNode goalNode = new GoapNode(null, goalAction.cost, goalAction);
        bool success = BuildGoapTree(goalNode, startingNodes, usableActions, job);
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
            success = BuildGoapTree(currentPlan.currentNode, startingNodes, usableActions);
        } else {
            GoapNode currentLeafNode = null;
            bool hasUsableAction = false;
            success = true;
            for (int i = currentPlan.currentNode.index; i >= 0; i--) {
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

    private bool BuildGoapTree(GoapNode parent, List<GoapNode> startingNodes, List<GoapAction> usableActions, GoapPlanJob job = null) {
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
                //Look for an action that can satisfy all unsatisfied preconditions
                //if one precondition cannot be satisfied, skip that action
                //if all preconditions can be satisfied, create a new goap tree for it
                for (int i = 0; i < usableActions.Count; i++) {
                    GoapAction usableAction = usableActions[i];
                    bool canSatisfyAllPreconditions = true;
                    for (int j = 0; j < unsatisfiedPreconditions.Count; j++) {
                        Precondition precon = unsatisfiedPreconditions[j];
                        if (job != null && job.forcedActions.ContainsKey(precon.goapEffect)) {
                            //if there is a provided job, check if it has any forced actions
                            //if it does, only allow the action that it allows
                            if (usableAction.goapType != job.forcedActions[precon.goapEffect]) {
                                canSatisfyAllPreconditions = false;
                                break;
                            }
                        } else if (!usableAction.WillEffectsSatisfyPrecondition(precon.goapEffect)) {
                            canSatisfyAllPreconditions = false;
                            break;
                        }

                    }
                    if (canSatisfyAllPreconditions) {
                        GoapNode leafNode = new GoapNode(parent, parent.runningCost + usableAction.cost, usableAction);
                        bool success = BuildGoapTree(leafNode, startingNodes, usableActions, job);
                        return success;
                    }
                }
                return false;
            } else {
                startingNodes.Add(parent);
                return true;
            }
        } else {
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

public class GoapNode {
    public GoapNode parent;
    public int runningCost;
    public int index;
    public GoapAction action;

    public GoapNode(GoapNode parent, int runningCost, GoapAction action) {
        this.parent = parent;
        this.runningCost = runningCost;
        this.action = action;
    }
}

