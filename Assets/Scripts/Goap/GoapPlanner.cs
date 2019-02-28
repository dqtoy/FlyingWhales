using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapPlanner {
    public Character actor { get; set; }

    public GoapPlanner(Character actor) {
        this.actor = actor;
    }

    public List<GoapNode> PlanActions(IPointOfInterest target, GoapAction goalAction, List<GoapAction> usableActions) {
        //List of all starting nodes that can do the goal
        List<GoapNode> startingNodes = new List<GoapNode>();

        GoapNode goalNode = new GoapNode(null, 0, goalAction);

        return null;
    }

    private bool BuildGoapTree(GoapNode parent, List<GoapNode> startingNodes, List<GoapAction> usableActions) {
        if(parent.action.preconditions.Count > 0) {
            for (int i = 0; i < parent.action.preconditions.Count; i++) {
                Precondition precondition = parent.action.preconditions[i];
                if (!precondition.CanSatisfyCondition()) {
                    for (int j = 0; j < usableActions.Count; j++) {
                        GoapAction usableAction = usableActions[j];
                        if (usableAction.WillEffectsSatisfyPrecondition(precondition.goapEffect)) {
                            GoapNode leafNode = new GoapNode(parent, parent.runningCost + usableAction.cost, usableAction);
                            bool success = BuildGoapTree(leafNode, startingNodes, usableActions);
                        }
                    }
                }
            }
        } else {
            startingNodes.Add(parent);
            return true;
        }

        return false;
    }
}

public class GoapNode {
    public GoapNode parent;
    public int runningCost;
    public GoapAction action;

    public GoapNode(GoapNode parent, int runningCost, GoapAction action) {
        this.parent = parent;
        this.runningCost = runningCost;
        this.action = action;
    }
}

