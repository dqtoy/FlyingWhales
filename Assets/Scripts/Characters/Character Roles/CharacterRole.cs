/*
 This is the base class for character roles
 such as Chieftain, Village Head, etc.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterRole {
	protected ECS.Character _character;
    protected CHARACTER_ROLE _roleType;
    protected List<ROAD_TYPE> _allowedRoadTypes; //states what roads this role can use.
    protected bool _canPassHiddenRoads; //can the character use roads that haven't been discovered yet?
    protected List<ACTION_ALIGNMENT> _allowedQuestAlignments;
    protected List<QUEST_TYPE> _allowedQuestTypes;
	protected List<CharacterTask> _roleTasks;
	protected CharacterTask _defaultRoleTask;
	protected bool _cancelsAllOtherTasks;
	protected bool _isRemoved;

    #region getters/setters
    public CHARACTER_ROLE roleType {
        get { return _roleType; }
    }
	public ECS.Character character{
		get { return _character; }
	}
    public List<ACTION_ALIGNMENT> allowedQuestAlignments {
        get { return _allowedQuestAlignments; }
    }
    public List<QUEST_TYPE> allowedQuestTypes {
        get { return _allowedQuestTypes; }
    }
	public List<CharacterTask> roleTasks {
		get { return _roleTasks; }
	}
	public CharacterTask defaultRoleTask {
		get { return _defaultRoleTask; }
	}
	public bool cancelsAllOtherTasks {
		get { return _cancelsAllOtherTasks; }
	}
	public bool isRemoved {
		get { return _isRemoved; }
	}
    #endregion

	public CharacterRole(ECS.Character character){
		_character = character;
		_cancelsAllOtherTasks = false;
		_isRemoved = false;
        _allowedQuestTypes = new List<QUEST_TYPE>();
		_roleTasks = new List<CharacterTask> ();
		_roleTasks.Add (new RecruitFollowers (this._character, 5));
        _allowedQuestAlignments = new List<ACTION_ALIGNMENT>();
    }


	#region Virtuals
	public virtual void DeathRole(){
		_isRemoved = true;
	}
	public virtual void ChangedRole(){
		_isRemoved = true;
	}
	#endregion

    #region Action Weights
    public virtual void AddTaskWeightsFromRole(WeightedDictionary<CharacterTask> tasks) {
		for (int i = 0; i < _roleTasks.Count; i++) {
			CharacterTask currTask = _roleTasks[i];
			if(currTask.forPlayerOnly || !currTask.AreConditionsMet(_character)){
				continue;
			}
			tasks.AddElement (currTask, currTask.GetSelectionWeight(_character));
		}
    }
    /*
     This is called once a characters _role variable is assigned
         */
    public virtual void OnAssignRole() { }
    #endregion

	#region Role Tasks
	public CharacterTask GetRoleTask(TASK_TYPE taskType){
		for (int i = 0; i < _roleTasks.Count; i++) {
			CharacterTask task = _roleTasks [i];
			if(task.taskType == taskType){
				return task;
			}
		}
		return null;
	}
	#endregion
}
