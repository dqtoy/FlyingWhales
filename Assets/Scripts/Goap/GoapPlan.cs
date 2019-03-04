using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapPlan {
    public int Count {
        get {
            return GetNodeCount();
        }
    }

    public IPointOfInterest target { get; private set; }
    public GoapNode startingNode { get; private set; }
    public GoapNode currentNode { get; private set; }
    public GoapNode previousNode { get; private set; }
    public GOAP_EFFECT_CONDITION[] goalEffects { get; private set; }
    public List<CharacterAwareness> goalCharacterTargets { get; private set; } ////Only characters in the same structure and characters in this list are allowed to advertise actions even if they are part of the awareness list of the actor
    public bool isEnd { get; private set; }

    public GoapPlan(IPointOfInterest target, GoapNode startingNode, GOAP_EFFECT_CONDITION[] goalEffects) {
        this.target = target;
        this.startingNode = startingNode;
        this.currentNode = startingNode;
        this.goalEffects = goalEffects;
    }

    public void Reset(GoapNode startingNode) {
        this.startingNode = startingNode;
        this.currentNode = startingNode;
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
    }

    private int GetNodeCount() {
        //Gets all node count of plan, includes start and end node
        GoapNode node = startingNode;
        int count = 0;
        while(node.parent != null) {
            count++;
            node = node.parent;
        }
        return count;
    }

    public void SetListOfCharacterAwareness(List<CharacterAwareness> list) {
        goalCharacterTargets = list;
    }
}
