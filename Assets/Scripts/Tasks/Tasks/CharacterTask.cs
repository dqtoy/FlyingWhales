/*
 This is the base class for anything that a character does.
 Quests, Go Home, Rest, etc.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterTask {

    public delegate void OnTaskInfoChanged();
    public OnTaskInfoChanged onTaskInfoChanged; //For UI, to update quest info if a specific quest changes info

    public delegate void OnTaskLogsChange();
    public OnTaskLogsChange onTaskLogsChange; //For UI, to update when a specific quest adds a new log

    protected TaskCreator _createdBy;
    protected TASK_TYPE _taskType;
	protected string _taskName;
    protected ECS.Character _assignedCharacter;
    protected bool _isDone;
	protected bool _isHalted;
    protected TASK_STATUS _taskStatus;
    protected List<string> _taskLogs; //TODO: Change this to Logs when convenient

    protected STANCE _stance;

    //protected bool _canDoDailyAction = false;
	protected bool _forPlayerOnly;
	protected ILocation _targetLocation;
	protected int _daysLeft;
	protected int _defaultDaysLeft;

    #region getters/setters
    public TASK_TYPE taskType {
        get { return _taskType; }
    }
    public bool isDone {
        get { return _isDone; }
    }
	public bool isHalted {
		get { return _isHalted; }
	}
    public TASK_STATUS taskStatus {
        get { return _taskStatus; }
    }
    public List<string> taskLogs {
        get { return _taskLogs; }
    }
    public STANCE stance {
        get { return _stance; }
    }
//	public int weight{
//		get { return GetTaskWeight (); }
//	}
	public bool forPlayerOnly{
		get { return _forPlayerOnly; }
	}
	public ILocation targetLocation{
		get { return _targetLocation; }
	}
    #endregion

    public CharacterTask(TaskCreator createdBy, TASK_TYPE taskType) {
        _createdBy = createdBy;
        _taskType = taskType;
        _taskLogs = new List<string>();
		_forPlayerOnly = false;
		SetIsHalted (false);
		_isDone = false;
    }

    #region virtual
    public virtual void OnChooseTask(ECS.Character character) {
		if(character.isInCombat){
			character.SetCurrentFunction (() => OnChooseTask (character));
			return;
		}
        _taskStatus = TASK_STATUS.IN_PROGRESS;
        _assignedCharacter = character;
        character.SetCurrentTask(this);
    }
    /*
     Override this to make the character do something when
     he/she chooses to perform this task.
         */
    public virtual void PerformTask() {
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => PerformTask ());
			return;
		}
		if(_isHalted){
			return;
		}
    }
    public virtual void EndTask(TASK_STATUS taskResult) {
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => EndTask (taskResult));
			return;
		}
		if(_isHalted){
			return;
		}
        _taskStatus = taskResult;
        _isDone = true;
        switch (taskResult) {
            case TASK_STATUS.SUCCESS:
                TaskSuccess();
                break;
            case TASK_STATUS.FAIL:
                TaskFail();
                break;
            case TASK_STATUS.CANCEL:
                TaskCancel();
                break;
            default:
                break;
        }
    }
    public virtual void TaskSuccess() {
		_assignedCharacter.DetermineAction();
	}
    public virtual void TaskCancel() {
		_assignedCharacter.DetermineAction();
	}
    public virtual void TaskFail() {
		_assignedCharacter.DetermineAction();
	}
    //public virtual void PerformDailyAction() { }

	public virtual void ResetTask(){
		_assignedCharacter = null;
		_targetLocation = null;
		_isDone = false;
		SetIsHalted (false);
		_taskStatus = TASK_STATUS.IN_PROGRESS;
		_taskLogs.Clear ();
		_daysLeft = _defaultDaysLeft;
	}

	public virtual int GetTaskWeight(ECS.Character character){ return 0; }
    #endregion

    protected void ScheduleTaskEnd(int days, TASK_STATUS result) {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(days);
        SchedulingManager.Instance.AddEntry(dueDate, () => EndTask(result));
    }
    //protected void SetCanDoDailyAction(bool state) {
    //    _canDoDailyAction = state;
    //}

    #region Logs
    internal void AddNewLog(string log) {
        _taskLogs.Add(log);
        if (onTaskLogsChange != null) {
            onTaskLogsChange();
        }
    }
    internal void AddNewLogs(List<string> logs) {
        for (int i = 0; i < logs.Count; i++) {
            _taskLogs.Add(logs[i]);
        }
        if (onTaskLogsChange != null) {
            onTaskLogsChange();
        }
    }
    #endregion

    #region Utilities
    protected void SetStance(STANCE stance) {
        _stance = stance;
    }
	public void SetLocation(ILocation targetLocation){
		_targetLocation = targetLocation;
	}
	public void SetIsHalted (bool state){
		_isHalted = state;
	}
	public void SetDefaultDaysLeft(int days){
		_defaultDaysLeft = days;
	}
	public void SetDaysLeft (int days){
		_daysLeft = days;
	}
    #endregion
}
