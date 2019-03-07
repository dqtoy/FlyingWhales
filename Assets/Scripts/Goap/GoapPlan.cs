using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapPlan {
    public IPointOfInterest target { get { return endNode.action.poiTarget; } }
    public GoapNode startingNode { get; private set; }
    public GoapNode endNode { get; private set; }
    public GoapNode currentNode { get; private set; }
    public GoapNode previousNode { get; private set; }
    public GOAP_EFFECT_CONDITION[] goalEffects { get; private set; }
    public List<CharacterAwareness> goalCharacterTargets { get; private set; } ////Only characters in the same structure and characters in this list are allowed to advertise actions even if they are part of the awareness list of the actor
    public List<GoapNode> allNodes { get; private set; }
    public bool isEnd { get; private set; }
    public bool isBeingRecalculated { get; private set; }

    public GoapPlan(GoapNode startingNode, GOAP_EFFECT_CONDITION[] goalEffects) {
        this.startingNode = startingNode;
        this.currentNode = startingNode;
        this.goalEffects = goalEffects;
        allNodes = new List<GoapNode>();
        ConstructAllNodes();
    }

    public void Reset(GoapNode startingNode) {
        this.startingNode = startingNode;
        this.currentNode = startingNode;
        ConstructAllNodes();
    }

    public void SetNextNode() {
        previousNode = currentNode;
        currentNode = currentNode.parent;
    }

    public void EndPlan() {
        isEnd = true;
        startingNode = null;
        currentNode = null;
        previousNode = null;
        endNode = null;
        allNodes.Clear();
    }

    private void ConstructAllNodes() {
        allNodes.Clear();
        GoapNode node = startingNode;
        allNodes.Add(node);
        while (node.parent != null) {
            node = node.parent;
            node.action.SetParentPlan(this);
            allNodes.Add(node);
        }
        endNode = node;
    }

    public void SetListOfCharacterAwareness(List<CharacterAwareness> list) {
        goalCharacterTargets = list;
    }

    public void SetIsBeingRecalculated(bool state) {
        isBeingRecalculated = state;
    }

    public string LogPlan() {
        string log = "\n---------------------NEW PLAN OF " + endNode.action.actor.name + " FOR " + endNode.action.goapName + " WITH TARGET " + target.name + " (" + endNode.action.actor.specificLocation.name + ")--------------------------";
        for (int i = 0; i < allNodes.Count; i++) {
            log += "\n" + (i + 1) + ". " + allNodes[i].action.goapName + " - " + allNodes[i].action.poiTarget.name;
        }
        return log;
    }
}
