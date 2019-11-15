using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapPlanner {
    public Character actor { get; private set; }
    public GOAP_PLANNING_STATUS status { get; private set; }

    public GoapPlanner(Character actor) {
        this.actor = actor;
    }
    public void StartGOAP(GoapEffect goal, IPointOfInterest target, GOAP_CATEGORY category, GoapPlanJob job, bool isPersonalPlan = true) {
        //List<IPointOfInterest> characterTargetsAwareness = new List<IPointOfInterest>();
        if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            actor.AddAwareness(target);
            //characterTargetsAwareness.Add(target);
        }

        //if (otherCharactePOIs != null) {
        //    for (int i = 0; i < otherCharactePOIs.Count; i++) {
        //        AddAwareness(otherCharactePOIs[i]);
        //        characterTargetsAwareness.Add(otherCharactePOIs[i]);
        //    }
        //}
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        status = GOAP_PLANNING_STATUS.RUNNING;
        //_numOfWaitingForGoapThread++;
        //Debug.LogWarning(name + " sent a plan to other thread(" + _numOfWaitingForGoapThread + ")");
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(actor, target, goal, category, isPersonalPlan, job));
    }
    public void StartGOAP(GoapAction goal, IPointOfInterest target, GOAP_CATEGORY category, GoapPlanJob job, bool isPersonalPlan = true) {
        //List<IPointOfInterest> characterTargetsAwareness = new List<IPointOfInterest>();
        if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            actor.AddAwareness(target);
            //characterTargetsAwareness.Add(target);
        }

        //if (otherCharactePOIs != null) {
        //    for (int i = 0; i < otherCharactePOIs.Count; i++) {
        //        AddAwareness(otherCharactePOIs[i]);
        //        characterTargetsAwareness.Add(otherCharactePOIs[i]);
        //    }
        //}
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        //_numOfWaitingForGoapThread++;
        status = GOAP_PLANNING_STATUS.RUNNING;
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(actor, target, goal, category, isPersonalPlan, job));
    }
    public void StartGOAP(INTERACTION_TYPE goalType, IPointOfInterest target, GOAP_CATEGORY category, GoapPlanJob job, bool isPersonalPlan = true) {
        //List<IPointOfInterest> characterTargetsAwareness = new List<IPointOfInterest>();
        if (target != null && target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            actor.AddAwareness(target);
            //characterTargetsAwareness.Add(target);
        }
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        //_numOfWaitingForGoapThread++;
        status = GOAP_PLANNING_STATUS.RUNNING;
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(actor, goalType, target, category, isPersonalPlan, job));
    }
    //public GoapPlan PlanActions(IPointOfInterest target, GoapAction goalAction, List<GoapAction> usableActions, GOAP_CATEGORY category, bool isPersonalPlan, ref string log, GoapPlanJob job = null) {
    //    //List of all starting nodes that can do the goal
    //    List<GoapNode> startingNodes = new List<GoapNode>();

    //    GoapNode goalNode = new GoapNode(null, goalAction.cost, goalAction);
    //    bool success = BuildGoapTree(goalNode, startingNodes, usableActions, ref log, job);
    //    if (!success) {
    //        return null;
    //    }

    //    GoapNode cheapestStartingNode = null;
    //    for (int i = 0; i < startingNodes.Count; i++) {
    //        if(cheapestStartingNode == null) {
    //            cheapestStartingNode = startingNodes[i];
    //        } else {
    //            if(startingNodes[i].cost < cheapestStartingNode.cost) {
    //                cheapestStartingNode = startingNodes[i];
    //            }
    //        }
    //    }
    //    GOAP_EFFECT_CONDITION[] goalEffects = new GOAP_EFFECT_CONDITION[goalAction.expectedEffects.Count];
    //    for (int i = 0; i < goalAction.expectedEffects.Count; i++) {
    //        goalEffects[i] = goalAction.expectedEffects[i].conditionType;
    //    }
    //    GoapPlan plan = new GoapPlan(cheapestStartingNode, goalEffects, category, isPersonalPlan);
    //    return plan;
    //}
    public GoapPlan PlanActions(IPointOfInterest target, GoapEffect goalEffect, GOAP_CATEGORY category, bool isPersonalPlan, ref string log, GoapPlanJob job) {
        //Cache all needed data
        Dictionary<POINT_OF_INTEREST_TYPE, List<GoapAction>> allGoapActionAdvertisements = InteractionManager.Instance.allGoapActionAdvertisements;
        Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness = actor.awareness;
        Dictionary<INTERACTION_TYPE, object[]> otherData = job.otherData;
        List<GoapNode> rawPlan = null; //The plan that will be created will be stored here
        if (goalEffect.target == GOAP_EFFECT_TARGET.TARGET) {
            //if precondition's target is TARGET, then the one who will advertise must be the target only
            int cost = 0;
            //Get action with the lowest cost that the actor can do that satisfies the goal effect
            GoapAction currentAction = target.AdvertiseActionsToActor(actor, goalEffect, otherData, ref cost);
            if (currentAction != null) {
                //If an action is found, make it the goal node and start building the plan
                GoapNode goalNode = new GoapNode(cost, 0, currentAction, target);
                rawPlan = new List<GoapNode>();
                BuildGoapTree(goalNode, actor, job, rawPlan, allGoapActionAdvertisements, awareness); //, ref log
            }
        } else if (goalEffect.target == GOAP_EFFECT_TARGET.ACTOR) {
            //If precondition's target is ACTOR, get the lowest action that the actor can do that will satisfy the goal effect
            GoapAction lowestCostAction = null;
            IPointOfInterest lowestCostTarget = null;
            int lowestCost = 0;
            foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<GoapAction>> kvp in allGoapActionAdvertisements) {
                //First loop through all actions that can be advertised (it is grouped by the poi type that can advertise the actions)
                if (awareness.ContainsKey(kvp.Key)) {
                    List<GoapAction> actionList = kvp.Value;
                    for (int j = 0; j < actionList.Count; j++) {
                        GoapAction currentAction = actionList[j];
                        if (currentAction.WillEffectsSatisfyPrecondition(goalEffect)) {
                            //Further optimize this by creating a list of all poi that currently advertises this action, and loop that
                            List<IPointOfInterest> poisThatAdvertisesCurrentAction = awareness[kvp.Key];
                            for (int k = 0; k < poisThatAdvertisesCurrentAction.Count; k++) {
                                IPointOfInterest poiTarget = poisThatAdvertisesCurrentAction[k];
                                int cost = 0;
                                bool canDoAction = poiTarget.CanAdvertiseActionsToActor(actor, currentAction, otherData, ref cost);
                                if (canDoAction) {
                                    if (lowestCostAction == null || cost < lowestCost) {
                                        lowestCostAction = currentAction;
                                        lowestCostTarget = poiTarget;
                                        lowestCost = cost;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (lowestCostAction != null) {
                GoapNode leafNode = new GoapNode(lowestCost, 0, lowestCostAction, lowestCostTarget);
                rawPlan = new List<GoapNode>();
                BuildGoapTree(leafNode, actor, job, rawPlan, allGoapActionAdvertisements, awareness); //, ref log
            }
        }
        if(rawPlan != null && rawPlan.Count > 0) {
            //has a created plan
            List<JobNode> actualNodes = TransformRawPlanToActualNodes(rawPlan, otherData);
            GoapPlan plan = new GoapPlan(actualNodes, target, isPersonalPlan);
            return plan;
        }
        return null;
    }
    public GoapPlan PlanActions(IPointOfInterest target, GoapAction goalAction, GOAP_CATEGORY category, bool isPersonalPlan, ref string log, GoapPlanJob job) {
        Dictionary<POINT_OF_INTEREST_TYPE, List<GoapAction>> allGoapActionAdvertisements = InteractionManager.Instance.allGoapActionAdvertisements;
        Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness = actor.awareness;
        Dictionary<INTERACTION_TYPE, object[]> otherData = job.otherData;
        List<GoapNode> rawPlan = new List<GoapNode>();
        object[] data = null;
        if (otherData != null) {
            if (otherData.ContainsKey(goalAction.goapType)) {
                data = otherData[goalAction.goapType];
            } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                data = otherData[INTERACTION_TYPE.NONE];
            }
        }
        int cost = goalAction.GetCost(actor, target, data);
        GoapNode goalNode = new GoapNode(cost, 0, goalAction, target);
        BuildGoapTree(goalNode, actor, job, rawPlan, allGoapActionAdvertisements, awareness); //, ref log
        if (rawPlan != null && rawPlan.Count > 0) {
            //has a created plan
            List<JobNode> actualNodes = TransformRawPlanToActualNodes(rawPlan, job.otherData);
            GoapPlan plan = new GoapPlan(actualNodes, target, isPersonalPlan);
            return plan;
        }
        return null;
    }
    public bool RecalculatePathForPlan(GoapPlan currentPlan, GoapPlanJob job) {
        //In plan recalculation, only recalculate nodes starting from the previous node, because this means that the current node does not satisfy all preconditions, which in turn, means that somewhere in the previous nodes, the character failed to do the action
        //That is why we recalculate from the previous node up to the starting node
        Dictionary<POINT_OF_INTEREST_TYPE, List<GoapAction>> allGoapActionAdvertisements = InteractionManager.Instance.allGoapActionAdvertisements;
        Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness = actor.awareness;
        List<GoapNode> rawPlan = new List<GoapNode>();
        JobNode currentJobNode = currentPlan.currentNode;
        if (currentJobNode.singleNode != null) {
            //Recalculate for single node
            ActualGoapNode actualNode = currentJobNode.singleNode;
            GoapAction goalAction = actualNode.action;
            IPointOfInterest target = actualNode.poiTarget;
            object[] otherData = actualNode.otherData;
            GoapNode goalNode = new GoapNode(actualNode.cost, currentPlan.currentNodeIndex, goalAction, target);
            BuildGoapTree(goalNode, actor, job, rawPlan, allGoapActionAdvertisements, awareness); //, ref log
            if (rawPlan.Count > 0) {
                //has a created plan
                List<JobNode> plannedNodes = TransformRawPlanToActualNodes(rawPlan, job.otherData, currentPlan);
                currentPlan.Reset(plannedNodes);
                return true;
            }
        } else {
            //Recalculate for multi node
            ActualGoapNode[] actualNodes = currentJobNode.multiNode;
            for (int i = 0; i < actualNodes.Length; i++) {
                ActualGoapNode actualNode = actualNodes[i];
                GoapAction goalAction = actualNode.action;
                IPointOfInterest target = actualNode.poiTarget;
                object[] otherData = actualNode.otherData;
                GoapNode goalNode = new GoapNode(actualNode.cost, currentPlan.currentNodeIndex, goalAction, target);
                BuildGoapTree(goalNode, actor, job, rawPlan, allGoapActionAdvertisements, awareness); //, ref log
            }
            if (rawPlan.Count > 0) {
                //has a created plan
                List<JobNode> plannedNodes = TransformRawPlanToActualNodes(rawPlan, job.otherData, currentPlan);
                currentPlan.Reset(plannedNodes);
                return true;
            }
        }
        return false;
        //List of all starting nodes that can do the goal
        //List<GoapNode> startingNodes = new List<GoapNode>();
        //bool success = false;
        //if (currentPlan.isPersonalPlan) {
        //    string log = string.Empty;
        //    success = BuildGoapTree(currentPlan.endNode, startingNodes, usableActions, ref log, currentPlan.job);
        //} else {
        //    GoapNode currentLeafNode = null;
        //    bool hasUsableAction = false;
        //    success = true;
        //    for (int i = currentPlan.endNode.index; i >= 0; i--) {
        //        GoapNode failedNode = currentPlan.allNodes[i];
        //        hasUsableAction = false;
        //        for (int j = 0; j < usableActions.Count; j++) {
        //            GoapAction currentUsableAction = usableActions[j];
        //            if(failedNode.action.goapType == currentUsableAction.goapType && failedNode.action.poiTarget == currentUsableAction.poiTarget) {
        //                hasUsableAction = true;
        //                if(currentLeafNode == null) {
        //                    currentLeafNode = new GoapNode(failedNode.parent, failedNode.parent.runningCost + currentUsableAction.cost, currentUsableAction);
        //                } else {
        //                    GoapNode leafNode = new GoapNode(currentLeafNode.parent, currentLeafNode.parent.runningCost + currentUsableAction.cost, currentUsableAction);
        //                    currentLeafNode = leafNode;
        //                }
        //                break;
        //            }
        //        }
        //        if (!hasUsableAction) {
        //            //No usable action for the current failed node, fail recalculation
        //            success = false;
        //            break;
        //        }
        //    }
        //    if(currentLeafNode != null) {
        //        startingNodes.Add(currentLeafNode);
        //    } else {
        //        success = false;
        //    }
        //}
        //if (!success) {
        //    return false;
        //}

        //GoapNode cheapestStartingNode = null;
        //for (int i = 0; i < startingNodes.Count; i++) {
        //    if (cheapestStartingNode == null) {
        //        cheapestStartingNode = startingNodes[i];
        //    } else {
        //        if (startingNodes[i].cost < cheapestStartingNode.cost) {
        //            cheapestStartingNode = startingNodes[i];
        //        }
        //    }
        //}
        //currentPlan.Reset(cheapestStartingNode);
        //return true;
    }
    //Note: The target specified here is the target for the precondition not the job itself
    private void BuildGoapTree(GoapNode node, Character actor, GoapPlanJob job, List<GoapNode> rawPlan, Dictionary<POINT_OF_INTEREST_TYPE, List<GoapAction>> allGoapActionAdvertisements
        , Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness) { //, ref string log
        GoapAction action = node.action;
        IPointOfInterest target = node.target;
        rawPlan.Add(node);
        if (action.preconditions.Count > 0) {
            for (int i = 0; i < action.preconditions.Count; i++) {
                Precondition precondition = action.preconditions[i];
                if (!precondition.CanSatisfyCondition(actor, target)) {
                    GoapEffect preconditionEffect = precondition.goapEffect;
                    if (preconditionEffect.target == GOAP_EFFECT_TARGET.TARGET) {
                        //if precondition's target is the target, then the one who will advertise must be the target only
                        int cost = 0;
                        GoapAction currentAction = target.AdvertiseActionsToActor(actor, preconditionEffect, job.otherData, ref cost);
                        if (currentAction != null) {
                            GoapNode leafNode = new GoapNode(cost, node.level + 1, currentAction, target);
                            BuildGoapTree(leafNode, actor, job, rawPlan, allGoapActionAdvertisements, awareness); //, ref log
                        }
                    } else if (preconditionEffect.target == GOAP_EFFECT_TARGET.ACTOR) {
                        GoapAction lowestCostAction = null;
                        IPointOfInterest lowestCostTarget = null;
                        int lowestCost = 0;
                        foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<GoapAction>> kvp in allGoapActionAdvertisements) {
                            if (awareness.ContainsKey(kvp.Key)) {
                                List<GoapAction> actionList = kvp.Value;
                                for (int j = 0; j < actionList.Count; j++) {
                                    GoapAction currentAction = actionList[j];
                                    if (currentAction.WillEffectsSatisfyPrecondition(preconditionEffect)) {
                                        //Further optimize this by creating a list of all poi that currently advertises this action, and loop that
                                        List<IPointOfInterest> poisThatAdvertisesCurrentAction = awareness[kvp.Key];
                                        for (int k = 0; k < poisThatAdvertisesCurrentAction.Count; k++) {
                                            IPointOfInterest poiTarget = poisThatAdvertisesCurrentAction[k];
                                            int cost = 0;
                                            bool canDoAction = poiTarget.CanAdvertiseActionsToActor(actor, currentAction, job.otherData, ref cost);
                                            if (canDoAction) {
                                                if (lowestCostAction == null || cost < lowestCost) {
                                                    lowestCostAction = currentAction;
                                                    lowestCostTarget = poiTarget;
                                                    lowestCost = cost;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if(lowestCostAction != null) {
                            GoapNode leafNode = new GoapNode(lowestCost, node.level + 1, lowestCostAction, lowestCostTarget);
                            BuildGoapTree(leafNode, actor, job, rawPlan, allGoapActionAdvertisements, awareness); //, ref log
                        }
                        //foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness in actor.awareness) {
                        //    List<IPointOfInterest> awarenessList = awareness.Value;
                        //    for (int i = 0; i < awarenessList.Count; i++) {
                        //        IPointOfInterest poiTarget = awarenessList[i];
                        //        int cost = 0;
                        //        INTERACTION_TYPE actionType = poiTarget.AdvertiseActionsToActor(actor, preconditionEffect, job.otherData, ref cost);
                        //        if (actionType != INTERACTION_TYPE.NONE) {
                        //            GoapAction leafAction = InteractionManager.Instance.goapActionData[actionType];
                        //            GoapNode leafNode = new GoapNode(cost, goalNode.level + 1, leafAction, poiTarget);
                        //            BuildGoapTree(leafNode, rawPlan, usableActions, ref log, job);
                        //        }
                        //    }
                        //}
                    }
                }
            }
        }
        //if (parent == null) {
        //    return false;
        //}
        //log += "\nBuilding goap tree with parent " + parent.actionType.goapName;
        //if(precondition.actionType.preconditions.Count > 0) {
        //    List<Precondition> unsatisfiedPreconditions = new List<Precondition>();
        //    for (int i = 0; i < precondition.actionType.preconditions.Count; i++) {
        //        Precondition precondition = precondition.actionType.preconditions[i];
        //        if (!precondition.CanSatisfyCondition(actor, target)) {
        //            //Pool all unsatisfied preconditions
        //            unsatisfiedPreconditions.Add(precondition);
        //        }
        //    }
        //    if (unsatisfiedPreconditions.Count > 0) {
        //        log += "\nChecking unsatisfied preconditions: ";
        //        for (int j = 0; j < unsatisfiedPreconditions.Count; j++) {
        //            Precondition precon = unsatisfiedPreconditions[j];
        //            log += "\n\t" + precon.goapEffect.conditionType.ToString() + " " + precon.goapEffect.conditionKey?.ToString() + " " + precon.goapEffect.targetPOI?.ToString() + " - ";
        //        }
        //        //Look for an action that can satisfy all unsatisfied preconditions
        //        //if one precondition cannot be satisfied, skip that action
        //        //if all preconditions can be satisfied, create a new goap tree for it
        //        log += "\nActions: ";
        //        List<GoapAction> shuffledActions = Utilities.Shuffle(usableActions);
        //        for (int i = 0; i < shuffledActions.Count; i++) {
        //            GoapAction usableAction = shuffledActions[i];
        //            bool canSatisfyAllPreconditions = true;
        //            log += "\n\t" + usableAction.goapName;
        //            for (int j = 0; j < unsatisfiedPreconditions.Count; j++) {
        //                Precondition precon = unsatisfiedPreconditions[j];
        //                if (!usableAction.WillEffectsSatisfyPrecondition(precon.goapEffect)) {
        //                    canSatisfyAllPreconditions = false;
        //                    break;
        //                } else {
        //                    //if there is a provided job, check if it has any forced actions
        //                    //if it does, only allow the action that it allows
        //                    if (job != null && job.forcedActions.Count > 0) {
        //                        bool satisfiedForcedActions = true;
        //                        ForcedActionsComparer comparer = new ForcedActionsComparer();
        //                        foreach (KeyValuePair<GoapEffect, INTERACTION_TYPE> kvp in job.forcedActions) {
        //                            if(comparer.Equals(kvp.Key, precon.goapEffect)) {
        //                                if(kvp.Value != usableAction.goapType) {
        //                                    satisfiedForcedActions = false;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        if (!satisfiedForcedActions) {
        //                            canSatisfyAllPreconditions = false;
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            if (canSatisfyAllPreconditions) {
        //                GoapNode leafNode = new GoapNode(precondition, precondition.runningCost + usableAction.cost, usableAction);
        //                log += " - Satisfied"; 
        //                bool success = BuildGoapTree(leafNode, startingNodes, usableActions, ref log, job);
        //                return success;
        //            }
        //        }
        //        return false;
        //    } else {
        //        log += "\nNo unsatisfied preconditions";
        //        startingNodes.Add(precondition);
        //        return true;
        //    }
        //} else {
        //    log += "\nNo preconditions.";
        //    startingNodes.Add(precondition);
        //    return true;
        //}
        //return false;
    }

    private List<JobNode> TransformRawPlanToActualNodes(List<GoapNode> rawPlan, Dictionary<INTERACTION_TYPE, object[]> otherData, GoapPlan currentPlan = null) { //actualPlan is for recalculation only, so that it will no longer create a new list, since in recalculation we already have a list of job nodes
        List<JobNode> actualPlan = null;
        if (currentPlan == null) {
            actualPlan = new List<JobNode>();
        } else {
            actualPlan = currentPlan.allNodes;
            actualPlan.RemoveRange(0, currentPlan.currentNodeIndex + 1); //It's +1 because we want to remove also the current node of the actual plan since it is already in the rawPlan
        }
        List<int> tempNodeIndexHolder = new List<int>();
        while (rawPlan.Count > 0) {
            int index = actualPlan.Count;
            tempNodeIndexHolder.Clear();
            for (int i = 0; i < rawPlan.Count; i++) {
                GoapNode rawNode = rawPlan[i];
                if(rawNode.level == index) {
                    tempNodeIndexHolder.Add(i);
                }
            }
            if(tempNodeIndexHolder.Count > 0) {
                if(tempNodeIndexHolder.Count == 1) {
                    //Single Job Node
                    int nodeIndex = tempNodeIndexHolder[0];
                    GoapNode rawNode = rawPlan[nodeIndex];
                    object[] data = null;
                    if (otherData != null) {
                        if (otherData.ContainsKey(rawNode.action.goapType)) {
                            data = otherData[rawNode.action.goapType];
                        } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                            data = otherData[INTERACTION_TYPE.NONE];
                        }
                    }
                    ActualGoapNode actualNode = new ActualGoapNode(rawNode.action, actor, rawNode.target, data, rawNode.cost);
                    SingleJobNode singleJobNode = new SingleJobNode(actualNode);
                    actualPlan.Insert(0, singleJobNode);
                    rawPlan.RemoveAt(nodeIndex);
                } else {
                    //Multi Job Node
                    ActualGoapNode[] actualNodes = new ActualGoapNode[tempNodeIndexHolder.Count];
                    for (int i = 0; i < tempNodeIndexHolder.Count; i++) {
                        int nodeIndex = tempNodeIndexHolder[i];
                        GoapNode rawNode = rawPlan[nodeIndex];
                        object[] data = null;
                        if (otherData != null) {
                            if (otherData.ContainsKey(rawNode.action.goapType)) {
                                data = otherData[rawNode.action.goapType];
                            } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                                data = otherData[INTERACTION_TYPE.NONE];
                            }
                        }
                        ActualGoapNode actualNode = new ActualGoapNode(rawNode.action, actor, rawNode.target, data, rawNode.cost);
                        actualNodes[i] = actualNode;
                        rawPlan.RemoveAt(nodeIndex);
                    }
                    MultiJobNode multiJobNode = new MultiJobNode(actualNodes);
                    actualPlan.Insert(0, multiJobNode);
                }
            }
        }
        return actualPlan;
    }
}