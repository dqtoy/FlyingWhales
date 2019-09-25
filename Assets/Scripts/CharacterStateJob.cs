using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateJob : JobQueueItem {

    public CHARACTER_STATE targetState { get; protected set; }
    public CharacterState assignedState { get; protected set; }
    public Area targetArea { get; protected set; }

    public List<System.Action<Character>> onUnassignActions { get; private set; }

    public CharacterStateJob(JOB_TYPE jobType, CHARACTER_STATE state, Area targetArea) : base(jobType) {
        this.targetState = state;
        this.targetArea = targetArea;
        onUnassignActions = new List<System.Action<Character>>();
    }
    public CharacterStateJob(SaveDataCharacterStateJob data) : base(data) {
        targetState = data.targetState;
        if(data.targetAreaID != -1) {
            targetArea = LandmarkManager.Instance.GetAreaByID(data.targetAreaID);
        } else {
            targetArea = null;
        }
        onUnassignActions = new List<System.Action<Character>>();
    }

    #region Overrides
    public override void UnassignJob(bool shouldDoAfterEffect = true) {
        base.UnassignJob(shouldDoAfterEffect);
        if(assignedState != null && assignedCharacter != null) {
            if(assignedCharacter.stateComponent.stateToDo == assignedState) {
                assignedCharacter.stateComponent.SetStateToDo(null);
            } else if (assignedCharacter.stateComponent.currentState == assignedState) {
                assignedCharacter.stateComponent.currentState.OnExitThisState();
            } else {
                if(assignedCharacter.stateComponent.previousMajorState == assignedState) {
                    Character character = assignedCharacter;
                    character.stateComponent.currentState.OnExitThisState();
                    if(character.stateComponent.currentState != null) {
                        //This happens because the character switched back to the previous major state
                        character.stateComponent.currentState.OnExitThisState();
                    }
                }
            }
        }
    }
    protected override bool CanTakeJob(Character character) {
        if(targetState == CHARACTER_STATE.PATROL) {
            if(character.role.roleType == CHARACTER_ROLE.SOLDIER) {
                return true;
            }
            return false;
        } else if (targetState == CHARACTER_STATE.EXPLORE) {
            if(character.role.roleType == CHARACTER_ROLE.ADVENTURER) {
                return true;
            }
            return false;
        }
        return base.CanTakeJob(character);
    }
    public override void OnCharacterUnassignedToJob(Character character) {
        base.OnCharacterAssignedToJob(character);
        ExecuteUnassignActions(character);
    }
    #endregion

    public void SetAssignedState(CharacterState state) {
        if (state != null) {
            state.SetJob(this);
        }
        if (assignedState != null) {
            assignedState.SetJob(null);
        }
        assignedState = state;
    }

    #region Unassign Actions
    public void AddOnUnassignAction(System.Action<Character> unassignAction) {
        onUnassignActions.Add(unassignAction);
    }
    /// <summary>
    /// Execute external actions that are triggered when this job becomes unassigned. 
    /// NOTE: This also clears the list of said actions.
    /// </summary>
    private void ExecuteUnassignActions(Character unassignedCharacter) {
        for (int i = 0; i < onUnassignActions.Count; i++) {
            onUnassignActions[i].Invoke(unassignedCharacter);
        }
        onUnassignActions.Clear();
    }
    #endregion

}

public class SaveDataCharacterStateJob : SaveDataJobQueueItem {
    public CHARACTER_STATE targetState;
    public int targetAreaID;

    public override void Save(JobQueueItem job) {
        base.Save(job);
        CharacterStateJob stateJob = job as CharacterStateJob;
        targetState = stateJob.targetState;
        if(stateJob.targetArea != null) {
            targetAreaID = stateJob.targetArea.id;
        } else {
            targetAreaID = -1;
        }
    }

    //public override JobQueueItem Load() {
    //    return base.Load();
    //}
}