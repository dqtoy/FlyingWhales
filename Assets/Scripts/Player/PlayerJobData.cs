using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJobData {

    public JOB jobType { get; private set; }
	public Character assignedCharacter { get; private set; }
    public List<PlayerJobAction> jobActions { get; private set; }
    public PlayerJobAction activeAction { get; private set; }
    public bool isSlotLocked { get; private set; } //this says whether the job slot can be assigned to.

    public PlayerJobData(JOB jobType) {
        this.jobType = jobType;
        isSlotLocked = false;
        ConstructRoleActions();
    }
    private void ConstructRoleActions() {
        jobActions = new List<PlayerJobAction>();
        switch (jobType) {
            case JOB.SPY:
                jobActions.Add(new Track());
                break;
            case JOB.RECRUITER:
                jobActions.Add(new Recruit());
                break;
            case JOB.DIPLOMAT:
                jobActions.Add(new ShareIntel());
                break;
            case JOB.DEBILITATOR:
                jobActions.Add(new Intervene());
                break;
            case JOB.INSTIGATOR:
                jobActions.Add(new RileUp());
                break;
        }
        for (int i = 0; i < jobActions.Count; i++) {
            jobActions[i].SetParentData(this);
        }
    }


    public void AssignCharacter(Character character) {
        assignedCharacter = character;
    }
    public void SetActiveAction(PlayerJobAction action) {
        activeAction = action;
    }

    public PlayerJobAction GetAction(System.Type type) {
        for (int i = 0; i < jobActions.Count; i++) {
            PlayerJobAction currAction = jobActions[i];
            if (currAction.GetType() == type) {
                return currAction;
            }
        }
        return null;
    }

    public void SetLockedState(bool locked) {
        if (locked == isSlotLocked) {
            return; //ignore change
        }
        isSlotLocked = locked;
        Messenger.Broadcast(Signals.JOB_SLOT_LOCK_CHANGED, jobType, isSlotLocked);
    }

}
