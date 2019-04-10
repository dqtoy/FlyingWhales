using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapPlanJob : JobQueueItem {

    public GoapEffect targetEffect { get; protected set; }
    public GoapPlan assignedPlan { get; protected set; }
    public IPointOfInterest targetPOI { get; protected set; }

    public GoapPlanJob(GoapEffect targetEffect) {
        this.targetEffect = targetEffect;
        this.targetPOI = targetEffect.targetPOI;
    }

    #region Overrides
    public override void UnassignJob() {
        base.UnassignJob();
        if (assignedPlan != null && assignedCharacter != null) {
            assignedCharacter.AdjustIsWaitingForInteraction(1);
            if (assignedCharacter.currentAction != null && assignedCharacter.currentAction.parentPlan == assignedPlan) {
                assignedCharacter.marker.StopMovementOnly();
                if (assignedCharacter.currentAction.isPerformingActualAction && !assignedCharacter.currentAction.isDone) {
                    assignedCharacter.currentAction.currentState.EndPerTickEffect();
                }
                assignedCharacter.SetCurrentAction(null);
                assignedCharacter.DropPlan(assignedPlan);
            } else {
                assignedCharacter.DropPlan(assignedPlan);
            }
            assignedCharacter.AdjustIsWaitingForInteraction(-1);
            SetAssignedCharacter(null);
            SetAssignedPlan(null);
        }
    }
    #endregion

    public void SetAssignedPlan(GoapPlan plan) {
        if (plan != null) {
            plan.SetJob(this);
        }
        if (assignedPlan != null) {
            assignedPlan.SetJob(null);
        }
        assignedPlan = plan;
    }
}
