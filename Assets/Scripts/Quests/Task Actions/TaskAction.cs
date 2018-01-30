using UnityEngine;
using System.Collections;

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
    public virtual void InititalizeAction(HexTile target) { }
    public virtual void InititalizeAction(Region target) { }
    public virtual void InititalizeAction(ECS.Character target) { }
    public virtual void InititalizeAction(Settlement target) { }
    public virtual void InititalizeAction(int days) { }

    public virtual void DoAction(ECS.Character partyLeader) {
        _actionDoer = partyLeader;
        if (onTaskDoAction != null) {
            onTaskDoAction();
        }
    }
    public virtual void ActionDone(TASK_ACTION_RESULT result) {
        _isDone = true;
        _result = result;
        if(onTaskActionDone != null) {
            onTaskActionDone();
        }
    }
    #endregion

}
