using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TaskAction {

    public delegate void OnTaskActionDone();
    public OnTaskActionDone onTaskActionDone;

    public delegate void OnTaskDoAction();
    public OnTaskDoAction onTaskDoAction;

    protected ECS.Character _actionDoer;
    protected CharacterTask _task;
    protected TASK_ACTION_RESULT _result;
    protected bool _isDone;

    #region getters/setters
    public ECS.Character actionDoer {
        get { return _actionDoer; }
    }
    #endregion

    public TaskAction(CharacterTask task) {
        _task = task;
    }

    #region virtuals
    public virtual void InititalizeAction(ILocation target) { }
    public virtual void InititalizeAction(Region target) { }
    public virtual void InititalizeAction(ECS.Character target) { }
    public virtual void InititalizeAction(int value) { }
    public virtual void InititalizeAction(Dictionary<RACE, int> civilians) { }

    public virtual void DoAction(ECS.Character partyLeader) {
		if(partyLeader.party != null){
			if(partyLeader.party.isInCombat){
				partyLeader.party.SetCurrentFunction (() => DoAction (partyLeader));
				return;
			}
		}else{
			if(partyLeader.isInCombat){
				partyLeader.SetCurrentFunction (() => DoAction (partyLeader));
				return;
			}
		}
        _actionDoer = partyLeader;
        if (onTaskDoAction != null) {
            onTaskDoAction();
        }
    }
    public virtual void ActionDone(TASK_ACTION_RESULT result) {
		if(_actionDoer.party != null){
			if(_actionDoer.party.isInCombat){
				_actionDoer.party.SetCurrentFunction (() => ActionDone (result));
				return;
			}
		}else{
			if(_actionDoer.isInCombat){
				_actionDoer.SetCurrentFunction (() => ActionDone (result));
				return;
			}
		}
        _isDone = true;
        _result = result;
        if(onTaskActionDone != null) {
            onTaskActionDone();
        }
    }
    #endregion

}
