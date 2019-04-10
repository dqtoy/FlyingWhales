using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueueItem {
    public JobQueue jobQueueParent { get; private set; }
    public IPointOfInterest targetPOI { get; private set; }
    public GoapEffect targetEffect { get; private set; }

    public Character assignedCharacter { get; private set; }
    public GoapPlan assignedPlan { get; private set; }

    private System.Func<Character, bool> _canTakeThisJob;

    public JobQueueItem(GoapEffect targetEffect) {
        this.targetEffect = targetEffect;
        this.targetPOI = targetEffect.targetPOI;
    }
    public void SetJobQueueParent(JobQueue parent) {
        jobQueueParent = parent;
    }
    public void SetAssignedCharacter(Character character) {
        assignedCharacter = character;
    }
    public void SetAssignedPlan(GoapPlan plan) {
        if (plan != null) {
            plan.SetJob(this);
        }
        if (assignedPlan != null) {
            assignedPlan.SetJob(null);
        }
        assignedPlan = plan;
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, bool> function) {
        _canTakeThisJob = function;
    }

    public bool CanCharacterTakeThisJob(Character character) {
        if(_canTakeThisJob != null) {
            return _canTakeThisJob(character);
        }
        return true;
    }
}
