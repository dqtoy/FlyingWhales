using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MinionCriticalFail : Interaction {

    public MinionCriticalFail(Area interactable) : base(interactable, INTERACTION_TYPE.MINION_CRITICAL_FAIL, 70) {
        _name = "Minion Critical Fail";
    }

    #region Overrides
    public override void Initialize() {
        SetDefaultInvestigatorCharacter(interactable.areaInvestigation.assignedMinion.character);
        base.Initialize();
    }
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        if (investigatorCharacter.job.jobType == JOB.RAIDER) {
            Raider raider = investigatorCharacter.job as Raider;
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-" + investigatorCharacter.job.jobType.ToString().ToLower() + "-" + raider.action.ToLower() + "_description");
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        } else {
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-" + investigatorCharacter.job.jobType.ToString().ToLower() + "_description");
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        }

        startState.SetEffect(() => StartEffect(startState));

        _states.Add(startState.name, startState);

        //SetCurrentState(startState);
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        if (investigatorCharacter.job.jobType == JOB.RAIDER && interactable.owner != null) {
            interactable.owner.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, -1);
        }
        investigatorCharacter.Death();
    }
    #endregion
}
