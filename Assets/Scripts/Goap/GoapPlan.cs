using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapPlan {

    //public string name { get; private set; }
    public IPointOfInterest target { get { return endNode.poiTarget; } }
    public JobNode startingNode { get; private set; }
    public JobNode endNode { get; private set; }
    public JobNode currentNode { get; private set; }
    public JobNode previousNode { get; private set; }
    //public ActualGoapNode currentActualNode { get; private set; }
    //public GOAP_EFFECT_CONDITION[] goalEffects { get; private set; }
    //public List<IPointOfInterest> goalCharacterTargets { get; private set; } ////Only characters in the same structure and characters in this list are allowed to advertise actions even if they are part of the awareness list of the actor
    public List<JobNode> allNodes { get; private set; }
    public bool isEnd { get; private set; }
    public bool isBeingRecalculated { get; private set; }
    public bool isPersonalPlan { get; private set; }
    public bool doNotRecalculate { get; private set; }
    //public bool hasShownNotification { get; private set; }
    public GOAP_PLAN_STATE state { get; private set; }
    //public GOAP_CATEGORY category { get; private set; }
    public GoapPlanJob job { get; private set; }
    //public bool isPriority { get; private set; }

    public string setPlanStateCallStack;

    public GoapPlan(List<JobNode> nodes, bool isPersonalPlan = true) {
        this.startingNode = nodes[0];
        this.endNode = nodes[nodes.Count - 1];
        this.currentNode = startingNode;
        //this.goalEffects = goalEffects;
        this.isPersonalPlan = isPersonalPlan;
        //this.category = category;
        this.doNotRecalculate = false;
        //hasShownNotification = false;
        allNodes = nodes;
        //ConstructAllNodes();
    }

    public void Reset(JobNode startingNode) {
        this.startingNode = startingNode;
        this.currentNode = startingNode;
        //hasShownNotification = false;
        //ConstructAllNodes();
    }
    public void SetNextNode() {
        for (int i = 0; i < allNodes.Count; i++) {
            if(allNodes[i].actionType == action) {
                currentNode = allNodes[i];
                break;
            }
        }
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
        //if this plan was ended, and it's state has not been set to failed or success, this means that this plan was not completed.
        if (state == GOAP_PLAN_STATE.IN_PROGRESS) SetPlanState(GOAP_PLAN_STATE.CANCELLED);
        //Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnActionInPlanFinished);
        //dropPlanCallStack = StackTraceUtility.ExtractStackTrace();
    }
    //public void InsertAction(GoapAction action) {
    //    if (currentNode != null) {
    //        GoapNode nextNode = currentNode.parent;
    //        GoapNode newNode = new GoapNode(nextNode, action.cost, action);
    //        currentNode.parent = newNode;
    //        newNode.index = currentNode.index;
    //        newNode.actionType.SetParentPlan(this);
    //        //Debug.Log(action.actor.name + " inserted new action " + action.goapName + " to replace action that returned fail. New plan is\n" + GetPlanSummary());
    //    }
    //}

    public void ConstructAllNodes() {
        allNodes.Clear();
        ActualGoapNode node = startingNode;
        //node.actionType.SetParentPlan(this);
        //node.index = allNodes.Count;
        allNodes.Add(node);
        while (node.parent != null) {
            node = node.parent;
            node.actionType.SetParentPlan(this);
            node.index = allNodes.Count;
            allNodes.Add(node);
        }
        endNode = node;
        name = "Plan of " +  endNode.action.actor.name + " to do " + endNode.action.goapName + " targetting " + target.name;
    }
    public int GetNumOfNodes() {
        if(allNodes.Count > 0) {
            return allNodes.Count;
        }
        int count = 1;
        GoapNode node = startingNode;
        while (node.parent != null) {
            node = node.parent;
            count++;
        }
        return count;
    }

    public void SetListOfCharacterAwareness(List<IPointOfInterest> list) {
        goalCharacterTargets = list;
    }
    public void SetIsBeingRecalculated(bool state) {
        isBeingRecalculated = state;
    }
    public void SetDoNotRecalculate(bool state) {
        doNotRecalculate = state;
    }
    public void SetHasShownNotification(bool state) {
        hasShownNotification = state;
    }
    public void SetPlanState(GOAP_PLAN_STATE state) {
        this.state = state;
        setPlanStateCallStack = state.ToString() + " " + StackTraceUtility.ExtractStackTrace();
    }
    public void SetJob(GoapPlanJob job) {
        this.job = job;
        if(this.job != null) {
            for (int i = 0; i < allNodes.Count; i++) {
                allNodes[i].actionType.OnSetJob(this.job);
            }
        }
    }
    public string LogPlan() {
        string log = "\n---------------------NEW PLAN OF " + endNode.action.actor.name + " FOR " + endNode.action.goapName + " WITH TARGET " + target.name + " (" + endNode.action.actor.specificLocation.name + ")--------------------------";
        for (int i = 0; i < allNodes.Count; i++) {
            log += "\n" + (i + 1) + ". " + allNodes[i].actionType.goapName + " - " + allNodes[i].actionType.poiTarget.name;
        }
        return log;
    }
    public string GetPlanSummary() {
        string summary = GetGoalSummary();
        summary += "\nis Priority?: " + isPriority.ToString();
        summary += "\nPlanned Actions are: ";
        for (int i = 0; i < allNodes.Count; i++) {
            summary += "\n" + (i + 1) + ". " + allNodes[i].actionType.goapName + " - " + allNodes[i].actionType.poiTarget.name;
        }
        return summary;
    }
    public string GetGoalSummary() {
        string summary = "Plan with goal: ";
        for (int i = 0; i < goalEffects.Length; i++) {
            summary += goalEffects[i].ToString() + ", ";
        }
        return summary;
    }
    public void OnActionInPlanFinished(Character actor, GoapAction action, string result) {
        if (endNode == null || action == endNode.action) {
            if (result == InteractionManager.Goap_State_Success) {
                SetPlanState(GOAP_PLAN_STATE.SUCCESS);
            } else if (result == InteractionManager.Goap_State_Fail) {
                SetPlanState(GOAP_PLAN_STATE.FAILED);
            }
        }
    }
    public void SetPriorityState(bool state) {
        isPriority = state;
    }
}
