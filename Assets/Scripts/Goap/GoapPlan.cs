using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapPlan {
    public int Count {
        get {
            return GetNodeCount();
        }
    }

    public GoapNode startingNode { get; private set; }
    public GoapNode currentNode { get; private set; }

    public GoapPlan(GoapNode startingNode) {
        this.startingNode = startingNode;
        this.currentNode = startingNode;
    }

    public GoapNode GetNextNode() {
        return currentNode.parent;
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
}
