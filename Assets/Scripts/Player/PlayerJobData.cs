using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJobData {

    public JOB jobType { get; private set; }
	public Character assignedCharacter { get; private set; }
    public List<PlayerJobAction> jobActions { get; private set; }
    public PlayerJobAction activeAction { get; private set; }
    public bool isSlotLocked { get; private set; } //this says whether the job slot can be assigned to.

    public bool hasActionInCooldown {
        get {
            for (int i = 0; i < jobActions.Count; i++) {
                PlayerJobAction currAction = jobActions[i];
                if (currAction.isInCooldown) {
                    return true;
                }
            }
            return false;
        }
    }

    public PlayerJobData(JOB jobType) {
        this.jobType = jobType;
        isSlotLocked = false;
        ConstructRoleActions();
    }
    private void ConstructRoleActions() {
        jobActions = new List<PlayerJobAction>();
        switch (jobType) {
            case JOB.SPY:
                //jobActions.Add(new Track());
                jobActions.Add(new AccessMemories());
                break;
            case JOB.SEDUCER:
                //jobActions.Add(new Corrupt());
                jobActions.Add(new CorruptLycanthropy());
                jobActions.Add(new CorruptKleptomaniac());
                jobActions.Add(new CorruptVampiric());
                jobActions.Add(new CorruptUnfaithful());
                break;
            case JOB.DIPLOMAT:
                jobActions.Add(new ShareIntel());
                break;
            case JOB.DEBILITATOR:
                //jobActions.Add(new Intervene());
                jobActions.Add(new Zap());
                jobActions.Add(new Jolt());
                jobActions.Add(new Spook());
                jobActions.Add(new Enrage());
                jobActions.Add(new Disable());
                break;
            case JOB.INSTIGATOR:
                jobActions.Add(new RileUp());
                jobActions.Add(new Abduct());
                jobActions.Add(new Provoke());
                jobActions.Add(new Destroy());
                jobActions.Add(new RaiseDead());
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
