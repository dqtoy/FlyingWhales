/*
 This is the base class for character roles
 such as Chieftain, Village Head, etc.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CharacterRole {
	protected ECS.Character _character;
    protected CHARACTER_ROLE _roleType;
    protected List<ACTION_ALIGNMENT> _allowedQuestAlignments;
    protected List<QUEST_TYPE> _allowedQuestTypes;
	protected List<CharacterTask> _roleTasks;
	protected CharacterTask _defaultRoleTask;
	protected bool _cancelsAllOtherTasks;
	protected bool _isRemoved;
    protected int _fullness, _energy, _fun, _prestige, _faith, _safety;
    protected int _maxFullness, _maxEnergy, _maxFun, _maxPrestige, _maxFaith, _maxSafety;
    protected int _minFullness, _minEnergy, _minFun, _minPrestige, _minFaith, _minSafety;
    protected bool _isHungry, _isFamished, _isTired, _isExhausted, _isSad, _isDepressed, _isAnxious, _isInsecure;
    protected float _happiness;

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
    public int fullness {
        get { return _fullness; }
    }
    public int energy {
        get { return _energy; }
    }
    public int fun {
        get { return _fun; }
    }
    public int prestige {
        get { return _prestige; }
    }
    public int maxFullness {
        get { return _maxFullness; }
    }
    public int maxEnergy {
        get { return _maxEnergy; }
    }
    public int maxFun {
        get { return _maxFun; }
    }
    public int maxPrestige {
        get { return _maxPrestige; }
    }
    public float happiness {
        get { return _happiness; }
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

        _maxFullness = 1000;
        _maxEnergy = 1000;
        _maxFun = 1000;
        _maxPrestige = 1000;
        _maxFaith = 1000;
        _maxSafety = 1000;

        _minFullness = -1000;
        _minEnergy = -1000;
        _minFun = -1000;
        _minPrestige = -1000;
        _minFaith = -1000;
        _minSafety = -1000;
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
    public void DepleteFullness() {
        AdjustFullness(-5);
    }
    public void SetFullness(int amount) {
        _fullness = amount;
    }
    public void AdjustFullness(int amount) {
        _fullness += amount;
        _fullness = Mathf.Clamp(_fullness, _minFullness, _maxFullness);

        if(_fullness <= 100 && !_isFamished) {
            _isFamished = true;
            if (_isHungry) {
                _isHungry = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.HUNGRY);
            }
            _character.AssignTag(CHARACTER_TAG.FAMISHED);
        }
        else if(_fullness > 100 && _fullness <= 300 && !_isHungry) {
            _isHungry = true;
            if (_isFamished) {
                _isFamished = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.FAMISHED);
            }
            _character.AssignTag(CHARACTER_TAG.HUNGRY);
        }
        else if (_fullness > 300) {
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
        AdjustEnergy(-3);
    }
    public void SetEnergy(int amount) {
        _energy = amount;
    }
    public void AdjustEnergy(int amount) {
        _energy += amount;
        _energy = Mathf.Clamp(_energy, _minEnergy, _maxEnergy);

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

    public void DepleteFun() {
        AdjustFun(-3);
    }
    public void SetFun(int amount) {
        _fun = amount;
    }
    public void AdjustFun(int amount) {
        _fun += amount;
        _fun = Mathf.Clamp(_fun, _minFun, _maxFun);
        if (_fun <= 100 && !_isDepressed) {
            _isDepressed = true;
            if (_isSad) {
                _isSad = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.SAD);
            }
            _character.AssignTag(CHARACTER_TAG.DEPRESSED);
        }
        else if (_fun > 100 && _fun <= 300 && !_isSad) {
            _isSad = true;
            if (_isDepressed) {
                _isDepressed = false;
                _character.RemoveCharacterTag(CHARACTER_TAG.DEPRESSED);
            }
            _character.AssignTag(CHARACTER_TAG.SAD);
        }
        else if (_fun > 300) {
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
        AdjustPrestige(-1);
    }
    public void SetPrestige(int amount) {
        _prestige = amount;
    }
    public void AdjustPrestige(int amount) {
        _prestige += amount;
        _prestige = Mathf.Clamp(_prestige, _minPrestige, _maxPrestige);
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
    public void SetFaith(int amount) {
        _faith = amount;
    }
    public void AdjustFaith(int amount) {
        _faith += amount;
        _faith = Mathf.Clamp(_faith, _minFaith, _maxFaith);
    }

    public void SetSafety(int amount) {
        _safety = amount;
    }
    public void AdjustSafety(int amount) {
        _safety += amount;
        _safety = Mathf.Clamp(_safety, _minSafety, _maxSafety);
    }

    public bool IsFull(NEEDS need) {
        switch (need) {
            case NEEDS.FULLNESS:
            return _fullness >= _maxFullness;
            case NEEDS.ENERGY:
            return _energy >= _maxEnergy;
            case NEEDS.FUN:
            return _fun >= _maxFun;
            case NEEDS.PRESTIGE:
            return _prestige >= _maxPrestige;
            case NEEDS.FAITH:
            return _prestige >= _maxPrestige;
            case NEEDS.SAFETY:
            return _prestige >= _maxPrestige;
        }
        return false;
    }

    public float GetTotalHappinessIncrease(CharacterAction characterAction) {
        return GetHappinessIncrease(NEEDS.FULLNESS, characterAction) + GetHappinessIncrease(NEEDS.ENERGY, characterAction) + GetHappinessIncrease(NEEDS.FUN, characterAction)
            + GetHappinessIncrease(NEEDS.PRESTIGE, characterAction) + GetHappinessIncrease(NEEDS.FAITH, characterAction) + GetHappinessIncrease(NEEDS.SAFETY, characterAction);
    }

    delegate float CalculateImpact(int currentNeed);
    private float GetHappinessIncrease(NEEDS need, CharacterAction action) {
        float happinessIncrease = 0f;
        int advertisedAmount = 0;
        int currentAmount = 0;
        CalculateImpact calculateImpact = null;
        switch (need) {
            case NEEDS.FULLNESS:
            currentAmount = _fullness;
            advertisedAmount = action.actionData.advertisedFullness;
            calculateImpact = CalculateFullnessImpact;
            break;
            case NEEDS.ENERGY:
            currentAmount = _energy;
            advertisedAmount = action.actionData.advertisedEnergy;
            calculateImpact = CalculateEnergyImpact;
            break;
            case NEEDS.FUN:
            currentAmount = _fun;
            advertisedAmount = action.actionData.advertisedFun;
            calculateImpact = CalculateFunImpact;
            break;
            case NEEDS.PRESTIGE:
            currentAmount = _prestige;
            advertisedAmount = action.actionData.advertisedPrestige;
            calculateImpact = CalculatePrestigeImpact;
            break;
            case NEEDS.FAITH:
            currentAmount = _faith;
            advertisedAmount = action.actionData.advertisedFaith;
            //calculateImpact = CalculateFullnessImpact;
            break;
            case NEEDS.SAFETY:
            currentAmount = _safety;
            advertisedAmount = action.actionData.advertisedSafety;
            calculateImpact = CalculateSafetyImpact;
            break;
        }
        int futureAmount = currentAmount + advertisedAmount;
        if(calculateImpact != null) {
            happinessIncrease = calculateImpact(futureAmount) - calculateImpact(currentAmount);
        }
        return happinessIncrease;
    }
    //Formula for calculation of happiness based on current fullness, meaning what's the happiness equivalent given the fullness
    private float CalculateFullnessImpact(int currentFullness) {
        return (Mathf.Pow (-1.007f, (float)-currentFullness)) + (float)_maxFullness;
    }

    //Formula for calculation of happiness based on current energy, meaning what's the happiness equivalent given the energy
    private float CalculateEnergyImpact(int currentEnergy) {
        return (-0.4f * (float) -currentEnergy) + 350f;
    }

    //Formula for calculation of happiness based on current fun, meaning what's the happiness equivalent given the fun
    private float CalculateFunImpact(int currentFun) {
        float value = 0.022f * (float)currentFun;
        return (Mathf.Pow(value, 2f)) + 50f;
    }

    //Formula for calculation of happiness based on current prestige, meaning what's the happiness equivalent given the prestige
    private float CalculatePrestigeImpact(int currentPrestige) {
        float value = 0.03f * (float) currentPrestige;
        return (Mathf.Pow(value, 2f)) + 50f;
    }

    //TODO: FAITH IMPACT CALCULATION

    //Formula for calculation of happiness based on current safety, meaning what's the happiness equivalent given the safety
    private float CalculateSafetyImpact(int currentSafety) {
        return (0.2f * (float)currentSafety) + 150f;
    }
    
    #endregion
}
