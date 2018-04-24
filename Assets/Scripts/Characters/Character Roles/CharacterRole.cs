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
    protected List<ACTION_ALIGNMENT> _allowedQuestAlignments;
    protected List<QUEST_TYPE> _allowedQuestTypes;
	protected List<CharacterTask> _roleTasks;
	protected CharacterTask _defaultRoleTask;
	protected bool _cancelsAllOtherTasks;
	protected bool _isRemoved;
    protected int _food;
    protected int _energy;
    protected int _joy;
    protected int _prestige;
    protected bool _isHungry;
    protected bool _isFamished;
    protected bool _isTired;
    protected bool _isExhausted;
    protected bool _isSad;
    protected bool _isDepressed;
    protected bool _isAnxious;
    protected bool _isInsecure;

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
    public int food {
        get { return _food; }
    }
    public int energy {
        get { return _energy; }
    }
    public int joy {
        get { return _joy; }
    }
    public int prestige {
        get { return _prestige; }
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

    #region Needs
    public void DepleteFood() {
        AdjustFood(-25);
    }
    public void SetFood(int amount) {
        _food = amount;
    }
    public void AdjustFood(int amount) {
        _food += amount;
        _food = Mathf.Clamp(_food, 0, 1000);

        if(_food <= 100 && !_isFamished) {
            _isFamished = true;
            if (_isHungry) {
                _isHungry = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.HUNGRY);
            }
            _character.AssignTag(CHARACTER_TAG.FAMISHED);
        }
        else if(_food > 100 && _food <= 300 && !_isHungry) {
            _isHungry = true;
            if (_isFamished) {
                _isFamished = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.FAMISHED);
            }
            _character.AssignTag(CHARACTER_TAG.HUNGRY);
        }
        else if (_food > 300) {
            if (_isHungry) {
                _isHungry = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.HUNGRY);
            }
            if (_isFamished) {
                _isFamished = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.FAMISHED);
            }
        }
    }

    public void DepleteEnergy() {
        AdjustEnergy(-12);
    }
    public void SetEnergy(int amount) {
        _energy = amount;
    }
    public void AdjustEnergy(int amount) {
        _energy += amount;
        _energy = Mathf.Clamp(_energy, 0, 1000);

        if (_energy <= 100 && !_isExhausted) {
            _isExhausted = true;
            if (_isTired) {
                _isTired = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.TIRED);
            }
            _character.AssignTag(CHARACTER_TAG.EXHAUSTED);
        }
        else if (_energy > 100 && _energy <= 300 && !_isTired) {
            _isTired = true;
            if (_isExhausted) {
                _isExhausted = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.EXHAUSTED);
            }
            _character.AssignTag(CHARACTER_TAG.TIRED);
        }
        else if (_energy > 300) {
            if (_isTired) {
                _isTired = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.TIRED);
            }
            if (_isExhausted) {
                _isExhausted = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.EXHAUSTED);
            }
        }
    }

    public void DepleteJoy() {
        AdjustJoy(-7);
    }
    public void SetJoy(int amount) {
        _joy = amount;
    }
    public void AdjustJoy(int amount) {
        _joy += amount;
        _joy = Mathf.Clamp(_joy, 0, 1000);
        if (_joy <= 100 && !_isDepressed) {
            _isDepressed = true;
            if (_isSad) {
                _isSad = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.SAD);
            }
            _character.AssignTag(CHARACTER_TAG.DEPRESSED);
        }
        else if (_joy > 100 && _joy <= 300 && !_isSad) {
            _isSad = true;
            if (_isDepressed) {
                _isDepressed = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.DEPRESSED);
            }
            _character.AssignTag(CHARACTER_TAG.SAD);
        }
        else if (_joy > 300) {
            if (_isSad) {
                _isSad = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.SAD);
            }
            if (_isDepressed) {
                _isDepressed = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.DEPRESSED);
            }
        }
    }

    public void DepletePrestige() {
        AdjustPrestige(-4);
    }
    public void SetPrestige(int amount) {
        _prestige = amount;
    }
    public void AdjustPrestige(int amount) {
        _prestige += amount;
        _prestige = Mathf.Clamp(_prestige, 0, 1000);
        if (_prestige <= 100 && !_isInsecure) {
            _isInsecure = true;
            if (_isAnxious) {
                _isAnxious = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.ANXIOUS);
            }
            _character.AssignTag(CHARACTER_TAG.INSECURE);
        }
        else if (_prestige > 100 && _prestige <= 300 && !_isAnxious) {
            _isAnxious = true;
            if (_isInsecure) {
                _isInsecure = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.INSECURE);
            }
            _character.AssignTag(CHARACTER_TAG.ANXIOUS);
        }
        else if (_prestige > 300) {
            if (_isAnxious) {
                _isAnxious = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.ANXIOUS);
            }
            if (_isInsecure) {
                _isInsecure = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.INSECURE);
            }
        }
    }
    #endregion
}
