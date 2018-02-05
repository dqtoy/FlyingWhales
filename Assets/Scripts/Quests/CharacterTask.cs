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
    protected ECS.Character _assignedCharacter;
    protected bool _isDone;
    protected TASK_STATUS _taskStatus;
    protected List<string> _taskLogs; //TODO: Change this to Logs when convenient

    #region getters/setters
    public TASK_TYPE taskType {
        get { return _taskType; }
    }
    public bool isDone {
        get { return _isDone; }
    }
    public TASK_STATUS taskStatus {
        get { return _taskStatus; }
    }
    public List<string> taskLogs {
        get { return _taskLogs; }
    }
    #endregion

    public CharacterTask(TaskCreator createdBy, TASK_TYPE taskType) {
        _createdBy = createdBy;
        _taskType = taskType;
        _taskLogs = new List<string>();
    }

    #region virtual
    /*
     Override this to make the character do something when
     he/she chooses to perform this task.
         */
    public virtual void PerformTask(ECS.Character character) {
        _taskStatus = TASK_STATUS.IN_PROGRESS;
        _assignedCharacter = character;
    }
    public virtual void EndTask(TASK_STATUS taskResult) {
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
        _assignedCharacter.DetermineAction();
    }
    public virtual void TaskSuccess(){ }
    public virtual void TaskCancel(){ }
    public virtual void TaskFail(){ }
    #endregion

    protected void ScheduleTaskEnd(int days, TASK_STATUS result) {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(days);
        SchedulingManager.Instance.AddEntry(dueDate, () => EndTask(result));
    }

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
}
