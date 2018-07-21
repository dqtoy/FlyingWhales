﻿/*
 This is the base class for character roles
 such as Chieftain, Village Head, etc.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ECS;

public class CharacterRole {
	protected Character _character;
    protected CHARACTER_ROLE _roleType;
	protected bool _cancelsAllOtherTasks;
	protected bool _isRemoved;
    protected bool _isHungry, _isFamished, _isTired, _isExhausted, _isSad, _isDepressed, _isAnxious, _isInsecure;
    protected float _fullness, _energy, _fun, _prestige, _sanity, _safety;
    protected float _maxFullness, _maxEnergy, _maxFun, _maxPrestige, _maxSanity, _maxSafety;
    protected float _minFullness, _minEnergy, _minFun, _minPrestige, _minSanity, _minSafety;
    protected float _happiness;

    protected float _constantSanityBuff, _constantFunBuff;

    #region getters/setters
    public CHARACTER_ROLE roleType {
        get { return _roleType; }
    }
	public Character character{
		get { return _character; }
	}
    //public List<ACTION_ALIGNMENT> allowedQuestAlignments {
    //    get { return _allowedQuestAlignments; }
    //}
    //public List<QUEST_TYPE> allowedQuestTypes {
    //    get { return _allowedQuestTypes; }
    //}
	//public List<CharacterTask> roleTasks {
	//	get { return _roleTasks; }
	//}
	//public CharacterTask defaultRoleTask {
	//	get { return _defaultRoleTask; }
	//}
	public bool cancelsAllOtherTasks {
		get { return _cancelsAllOtherTasks; }
	}
	public bool isRemoved {
		get { return _isRemoved; }
	}
    public float fullness {
        get { return _fullness; }
    }
    public float energy {
        get { return _energy; }
    }
    public float fun {
        get { return Mathf.Clamp(_fun + _constantFunBuff, _minFun, _maxFun); }
    }
    public float prestige {
        get { return _prestige; }
    }
    public float sanity {
        get { return Mathf.Clamp(_sanity + _constantSanityBuff, _minSanity, _maxSanity); }
    }
    public float safety {
        get { return _safety; }
    }
    public float maxFullness {
        get { return _maxFullness; }
    }
    public float maxEnergy {
        get { return _maxEnergy; }
    }
    public float maxFun {
        get { return _maxFun; }
    }
    public float maxPrestige {
        get { return _maxPrestige; }
    }
    public float maxSanity {
        get { return _maxSanity; }
    }
    public float maxSafety {
        get { return _maxSafety; }
    }
    public float happiness {
        get { return _happiness; }
    }
    #endregion

    public CharacterRole(Character character){
		_character = character;
		_cancelsAllOtherTasks = false;
		_isRemoved = false;

        _maxFullness = 100f;
        _maxEnergy = 100f;
        _maxFun = 100f;
        _maxPrestige = 100f;
        _maxSanity = 100f;
        _maxSafety = 100f;

        _minFullness = -100f;
        _minEnergy = -100f;
        _minFun = -100f;
        _minPrestige = -100f;
        _minSanity = -100f;
        _minSafety = -100f;
    }


    #region Virtuals
    public virtual void DeathRole(){
		_isRemoved = true;
	}
	public virtual void ChangedRole(){
		_isRemoved = true;
	}
    public virtual void OnAssignRole() { }
    #endregion

    #region Needs
    public void SetNeedValue(NEEDS need, float newValue) {
        switch (need) {
            case NEEDS.FULLNESS:
                SetFullness(newValue);
                break;
            case NEEDS.ENERGY:
                SetEnergy(newValue);
                break;
            case NEEDS.FUN:
                SetFun(newValue);
                break;
            case NEEDS.PRESTIGE:
                SetPrestige(newValue);
                break;
            case NEEDS.SANITY:
                SetSanity(newValue);
                break;
            case NEEDS.SAFETY:
                SetSafety(newValue);
                break;
            default:
                break;
        }
        UpdateHappiness();
    }
    public float GetNeedValue(NEEDS need) {
        switch (need) {
            case NEEDS.FULLNESS:
                return _fullness;
            case NEEDS.ENERGY:
                return _energy;
            case NEEDS.FUN:
                return _fun;
            case NEEDS.PRESTIGE:
                return _prestige;
            case NEEDS.SANITY:
                return _sanity;
            case NEEDS.SAFETY:
                return _safety;
            default:
                return 0;
        }
    }

    public void DepleteFullness() {
        if (_fullness > 0) {
            AdjustFullness(-1.04f);
        } else {
            AdjustFullness(-0.347f);
        }
    }
    public void SetFullness(float amount) {
        _fullness = amount;
        OnFullnessEdited();
    }
    public void AdjustFullness(float amount) {
        float previous = _fullness;
        _fullness += amount;
        _fullness = Mathf.Clamp(_fullness, _minFullness, _maxFullness);
        if(previous != _fullness) {
            UpdateHappiness();
            OnFullnessEdited();
        }
        //if (_fullness <= 10f && !_isFamished) {
        //    _isFamished = true;
        //    if (_isHungry) {
        //        _isHungry = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.HUNGRY);
        //    }
        //    _character.AssignTag(CHARACTER_TAG.FAMISHED);
        //}
        //else if(_fullness > 10f && _fullness <= 30f && !_isHungry) {
        //    _isHungry = true;
        //    if (_isFamished) {
        //        _isFamished = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.FAMISHED);
        //    }
        //    _character.AssignTag(CHARACTER_TAG.HUNGRY);
        //}
        //else if (_fullness > 30f) {
        //    if (_isHungry) {
        //        _isHungry = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.HUNGRY);
        //    }
        //    if (_isFamished) {
        //        _isFamished = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.FAMISHED);
        //    }
        //}
    }
    private void OnFullnessEdited() {
        if (_fullness < 0 && _fullness >= -75) {
            //Character gains Hungry tag when Fullness is below 0 to -75.
            _character.AssignTag(CHARACTER_TAG.HUNGRY);
            _character.RemoveCharacterTag(CHARACTER_TAG.STARVING);
            //Character gains Wounded tag when Fullness is below 0 to -75.
            _character.AssignTag(CHARACTER_TAG.WOUNDED);
            _character.RemoveCharacterTag(CHARACTER_TAG.WRECKED);
        } else if (_fullness < -75) {
            //Character gains Starving tag when Fullness is below -75
            _character.AssignTag(CHARACTER_TAG.STARVING);
            _character.RemoveCharacterTag(CHARACTER_TAG.HUNGRY);
            //Character gains Wrecked tag when Fullness is below -75
            _character.AssignTag(CHARACTER_TAG.WRECKED);
            _character.RemoveCharacterTag(CHARACTER_TAG.WOUNDED);
        } else {
            _character.RemoveCharacterTag(new List<CHARACTER_TAG>() { CHARACTER_TAG.HUNGRY, CHARACTER_TAG.STARVING, CHARACTER_TAG.WOUNDED, CHARACTER_TAG.WRECKED });
        }
    }

    public void DepleteEnergy() {
        AdjustEnergy(-0.694f);
    }
    public void SetEnergy(float amount) {
        _energy = amount;
        OnEnergyEdited();
    }
    public void AdjustEnergy(float amount) {
        float previous = _energy;
        _energy += amount;
        _energy = Mathf.Clamp(_energy, _minEnergy, _maxEnergy);
        if (previous != _energy) {
            UpdateHappiness();
            OnEnergyEdited();
        }

        //if (_energy <= 10f && !_isExhausted) {
        //    _isExhausted = true;
        //    if (_isTired) {
        //        _isTired = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.TIRED);
        //    }
        //    _character.AssignTag(CHARACTER_TAG.EXHAUSTED);
        //}
        //else if (_energy > 10f && _energy <= 30f && !_isTired) {
        //    _isTired = true;
        //    if (_isExhausted) {
        //        _isExhausted = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.EXHAUSTED);
        //    }
        //    _character.AssignTag(CHARACTER_TAG.TIRED);
        //}
        //else if (_energy > 30f) {
        //    if (_isTired) {
        //        _isTired = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.TIRED);
        //    }
        //    if (_isExhausted) {
        //        _isExhausted = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.EXHAUSTED);
        //    }
        //}
    }
    private void OnEnergyEdited() {
        if (_energy < 0 && _energy >= -75) {
            //Character gains Tired tag when Energy is below 0 to -75.
            _character.AssignTag(CHARACTER_TAG.TIRED);
            _character.RemoveCharacterTag(CHARACTER_TAG.EXHAUSTED);
        } else if (_energy < -75) {
            //Character gains Crazed tag when Energy is below -75
            _character.AssignTag(CHARACTER_TAG.EXHAUSTED);
            _character.RemoveCharacterTag(CHARACTER_TAG.TIRED);
        } else {
            _character.RemoveCharacterTag(new List<CHARACTER_TAG>() { CHARACTER_TAG.TIRED, CHARACTER_TAG.EXHAUSTED });
        }
    }

    public void DepleteFun() {
        if(_fun > 0) {
            AdjustFun(-0.416f);
        } else {
            AdjustFun(-0.297f);
        }
    }
    public void SetFun(float amount) {
        _fun = amount;
        OnFunEdited();
    }
    public void AdjustFun(float amount) {
        float previous = _fun;
        _fun += amount;
        _fun = Mathf.Clamp(_fun, _minFun, _maxFun);
        if (previous != _fun) {
            UpdateHappiness();
            OnFunEdited();
        }
        
        //if (_fun <= 10f && !_isDepressed) {
        //    _isDepressed = true;
        //    if (_isSad) {
        //        _isSad = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.SAD);
        //    }
        //    _character.AssignTag(CHARACTER_TAG.DEPRESSED);
        //}
        //else if (_fun > 10f && _fun <= 30f && !_isSad) {
        //    _isSad = true;
        //    if (_isDepressed) {
        //        _isDepressed = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.DEPRESSED);
        //    }
        //    _character.AssignTag(CHARACTER_TAG.SAD);
        //}
        //else if (_fun > 30f) {
        //    if (_isSad) {
        //        _isSad = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.SAD);
        //    }
        //    if (_isDepressed) {
        //        _isDepressed = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.DEPRESSED);
        //    }
        //}
    }
    private void OnFunEdited() {
        if (_fun < 0 && _fun >= -75) {
            //Character gains Sad tag when Fun is below 0 to -75.
            _character.AssignTag(CHARACTER_TAG.SAD);
            _character.RemoveCharacterTag(CHARACTER_TAG.DEPRESSED);
        } else if (_fun < -75) {
            //Character gains Depressed tag when Fun is below -75
            _character.AssignTag(CHARACTER_TAG.DEPRESSED);
            _character.RemoveCharacterTag(CHARACTER_TAG.SAD);
        } else {
            _character.RemoveCharacterTag(new List<CHARACTER_TAG>() { CHARACTER_TAG.SAD, CHARACTER_TAG.DEPRESSED });
        }
    }

    public void DepletePrestige() {
        if(_prestige > 0) {
            AdjustPrestige(-1.39f);
        } else {
            AdjustPrestige(-0.245f);
        }
    }
    public void SetPrestige(float amount) {
        _prestige = amount;
        OnPrestigeEdited();
    }
    public void AdjustPrestige(float amount) {
        float previous = _prestige;
        _prestige += amount;
        _prestige = Mathf.Clamp(_prestige, _minPrestige, _maxPrestige);
        if (previous != _prestige) {
            UpdateHappiness();
            OnPrestigeEdited();
        }
        //if (_prestige <= 0f && !_isInsecure) {
        //    _isInsecure = true;
        //    if (_isAnxious) {
        //        _isAnxious = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.ANXIOUS);
        //    }
        //    _character.AssignTag(CHARACTER_TAG.INSECURE);
        //}
        //else if (_prestige > 10f && _prestige <= 30f && !_isAnxious) {
        //    _isAnxious = true;
        //    if (_isInsecure) {
        //        _isInsecure = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.INSECURE);
        //    }
        //    _character.AssignTag(CHARACTER_TAG.ANXIOUS);
        //}
        //else if (_prestige > 30f) {
        //    if (_isAnxious) {
        //        _isAnxious = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.ANXIOUS);
        //    }
        //    if (_isInsecure) {
        //        _isInsecure = false;
        //        _character.RemoveCharacterTag(CHARACTER_TAG.INSECURE);
        //    }
        //}
    }
    private void OnPrestigeEdited() {
        if (_prestige < 0 && _prestige >= -75) {
            //Character gains Anxious tag when Prestige is below 0 to -75.
            _character.AssignTag(CHARACTER_TAG.ANXIOUS);
            _character.RemoveCharacterTag(CHARACTER_TAG.DEMORALIZED);
        } else if (_prestige < -75) {
            //Character gains Demoralized tag when Sanity is below -75
            _character.AssignTag(CHARACTER_TAG.DEMORALIZED);
            _character.RemoveCharacterTag(CHARACTER_TAG.ANXIOUS);
        } else {
            _character.RemoveCharacterTag(new List<CHARACTER_TAG>() { CHARACTER_TAG.ANXIOUS, CHARACTER_TAG.DEMORALIZED });
        }
    }

    public void DepleteSanity() {
        AdjustSanity(-0.2f);
    }
    public void SetSanity(float amount) {
        _sanity = amount;
        OnSanityEdited();
    }
    public void AdjustSanity(float amount) {
        float previous = _sanity;
        _sanity += amount;
        _sanity = Mathf.Clamp(_sanity, _minSanity, _maxSanity);
        if (previous != _sanity) {
            UpdateHappiness();
            OnSanityEdited();
        }
    }
    private void OnSanityEdited() {
        if (_sanity < 0 && _sanity >= -75) {
            //Character gains Disturbed tag when Sanity is below 0 to -75.
            _character.AssignTag(CHARACTER_TAG.DISTURBED);
            _character.RemoveCharacterTag(CHARACTER_TAG.CRAZED);
        } else if (_sanity < -75) {
            //Character gains Crazed tag when Sanity is below -75
            _character.AssignTag(CHARACTER_TAG.CRAZED);
            _character.RemoveCharacterTag(CHARACTER_TAG.DISTURBED);
        } else {
            _character.RemoveCharacterTag(new List<CHARACTER_TAG>() { CHARACTER_TAG.DISTURBED, CHARACTER_TAG.CRAZED });
        }
    }

    public void SetSafety(float amount) {
        _safety = amount;
        OnSafetyEdited();
    }
    public void AdjustSafety(float amount) {
        float previous = _safety;
        _safety += amount;
        _safety = Mathf.Clamp(_safety, _minSafety, _maxSafety);
        if (previous != _safety) {
            UpdateHappiness();
            OnSafetyEdited();
        }
    }
    public void UpdateSafety() {
        float hpPercent = (float) character.currentHP / (float) character.maxHP;
        float newSafety = (hpPercent * (_maxSafety - _minSafety)) + _minSafety;
        SetSafety(newSafety);
    }
    private void OnSafetyEdited() {

    }

    public void AdjustConstantSanityBuff(int adjustment) {
        _constantSanityBuff += adjustment;
    }
    public void AdjustConstantFunBuff(int adjustment) {
        _constantFunBuff += adjustment;
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
            case NEEDS.SANITY:
            return _sanity >= _maxSanity;
            case NEEDS.SAFETY:
            return _safety >= _maxSafety;
        }
        return false;
    }

    public void UpdateHappiness() {
        _happiness = CalculateFullnessImpact(_fullness) + CalculateEnergyImpact(_energy) + CalculateFunImpact(_fun)
            + CalculatePrestigeImpact(_prestige) + CalculateSanityImpact(_sanity) + CalculateSafetyImpact(_safety);
    }

    public float GetTotalHappinessIncrease(CharacterAction characterAction, IObject targetObject) {
        float result = (GetHappinessIncrease(NEEDS.FULLNESS, characterAction) + GetHappinessIncrease(NEEDS.ENERGY, characterAction) + GetHappinessIncrease(NEEDS.FUN, characterAction)
            + GetHappinessIncrease(NEEDS.PRESTIGE, characterAction) + GetHappinessIncrease(NEEDS.SANITY, characterAction) + GetHappinessIncrease(NEEDS.SAFETY, characterAction)); // GetDistanceModifier(_character.specificLocation.tileLocation, characterAction.state.obj.specificLocation.tileLocation);

        if (targetObject.objectType == OBJECT_TYPE.STRUCTURE) {
            Area areaOfStructure = targetObject.objectLocation.tileLocation.areaOfTile;
            if (areaOfStructure != null && _character.home != null && areaOfStructure.id == _character.home.id) {
                result *= _character.party.actionData.homeMultiplier;
            }
        }else if (characterAction.actionType == ACTION_TYPE.ATTACK && targetObject is ICharacterObject) {
            ICharacterObject icharacterObject = targetObject as ICharacterObject;
            float myPower = _character.party.computedPower;
            float enemyPower = icharacterObject.iparty.computedPower;
            float powerDiff = enemyPower - myPower;
            float powerDivisor = myPower;
            if(powerDiff < 0f) {
                powerDivisor = enemyPower;
            }
            float powerDiffPercent = (powerDiff / powerDivisor) * 100f;
            if(powerDiffPercent > 20f) {
                result = 0f;
            }else if (powerDiffPercent >= -51f && powerDiffPercent <= -20f) {
                result *= 1.5f;
            } else if (powerDiffPercent >= -76f && powerDiffPercent <= -50f) {
                result *= 0.5f;
            } else if (powerDiffPercent <= -75f) {
                result *= 0.1f;
            }
        }
        return result;
    }

    delegate float CalculateImpact(float currentNeed);
    private float GetHappinessIncrease(NEEDS need, CharacterAction action) {
        float happinessIncrease = 0f;
        float advertisedAmount = 0f;
        float currentAmount = 0f;
        float maxAmount = 0f;
        CalculateImpact calculateImpact = null;
        switch (need) {
            case NEEDS.FULLNESS:
            currentAmount = _fullness;
            maxAmount = _maxFullness;
            advertisedAmount = action.actionData.advertisedFullness;
            calculateImpact = CalculateFullnessImpact;
            break;
            case NEEDS.ENERGY:
            currentAmount = _energy;
            maxAmount = _maxEnergy;
            advertisedAmount = action.actionData.advertisedEnergy;
            calculateImpact = CalculateEnergyImpact;
            break;
            case NEEDS.FUN:
            currentAmount = _fun;
            maxAmount = _maxFun;
            advertisedAmount = action.actionData.advertisedFun;
            calculateImpact = CalculateFunImpact;
            break;
            case NEEDS.PRESTIGE:
            currentAmount = _prestige;
            maxAmount = _maxPrestige;
            advertisedAmount = action.actionData.advertisedPrestige;
            calculateImpact = CalculatePrestigeImpact;
            break;
            case NEEDS.SANITY:
            currentAmount = _sanity;
            maxAmount = _maxSanity;
            advertisedAmount = action.actionData.advertisedSanity;
            calculateImpact = CalculateSanityImpact;
            break;
            case NEEDS.SAFETY:
            currentAmount = _safety;
            maxAmount = _maxSafety;
            advertisedAmount = action.actionData.advertisedSafety;
            calculateImpact = CalculateSafetyImpact;
            break;
        }
        float futureAmount = currentAmount + advertisedAmount;
        if(calculateImpact != null) {
            if(futureAmount > maxAmount) {
                futureAmount = maxAmount;
            }
            float futureImpact = calculateImpact(futureAmount);
            float currentImpact = calculateImpact(currentAmount);
            happinessIncrease = futureImpact - currentImpact;
        }
        return happinessIncrease;
    }
    //Formula for calculation of happiness based on current fullness, meaning what's the happiness equivalent given the fullness
    private float CalculateFullnessImpact(float currentFullness) {
        //return (-(Mathf.Pow (1.007f, (float) -currentFullness))) + (float)_maxFullness;
        //float result = (Mathf.Pow(1.05f, -currentFullness)) + 20f;
        float result = 0f;
        if(currentFullness >= 0f) {
            result = (currentFullness / 10f) + 30f;
        } else {
            result = (0.65f * (currentFullness / 25f)) * -currentFullness;
        }
        //if (currentFullness < 0) { result *= -1f; }
        return result;
    }

    //Formula for calculation of happiness based on current energy, meaning what's the happiness equivalent given the energy
    private float CalculateEnergyImpact(float currentEnergy) {
        //return (-0.4f * ((float) -currentEnergy)) + 350f;
        //float result = (0.5f * -currentEnergy) + 50f;
        //if (currentEnergy < 0) { result *= -1f; }

        float result = 0f;
        if (currentEnergy >= 0f) {
            result = (currentEnergy / 2.5f) + 30f;
        } else {
            result = currentEnergy;
        }
        return result;
    }

    //Formula for calculation of happiness based on current fun, meaning what's the happiness equivalent given the fun
    private float CalculateFunImpact(float currentFun) {
        //float value = 1.09f; //* currentFun;
        //float result = (Mathf.Pow(value, -currentFun)) + 10f;
        //if (currentFun < 0) { result *= -1f; }

        float result = 0f;
        if (currentFun >= 0f) {
            //result = (((3f * currentFun) / 10f) * 2f) + 40f;
            result = (currentFun / 2.5f) + 40;
        } else {
            //result = currentFun / 2f;
            result = ((0.4f * (currentFun / 11f)) * -currentFun) + 40;
        }
        return result;
    }

    //Formula for calculation of happiness based on current prestige, meaning what's the happiness equivalent given the prestige
    private float CalculatePrestigeImpact(float currentPrestige) {
        //float value = 1.07f;// * currentPrestige;
        //float result = (Mathf.Pow(value, -currentPrestige)) + 15f;
        //if (currentPrestige < 0) { result *= -1f; }
        float result = 0f;
        if (currentPrestige >= 0f) {
            result = currentPrestige - 50f;
        } else {
            result = currentPrestige * 2f;
        }
        return result;
    }

    //Formula for calculation of happiness based on current sanity, meaning what's the happiness equivalent given the sanity
    private float CalculateSanityImpact(float currentSanity) {
        //float value = 1.08f;// * currentSanity;
        //float result = Mathf.Pow(value, -currentSanity);
        //if (currentSanity < 0) { result *= -1f; }
        float result = 0f;
        if (currentSanity >= 0f) {
            result = ((4f * currentSanity) / 20f) + 70f;
        } else {
            result = Mathf.Pow((currentSanity / 5f), 2f);
        }
        return result;
    }

    //Formula for calculation of happiness based on current safety, meaning what's the happiness equivalent given the safety
    private float CalculateSafetyImpact(float currentSafety) {
        //return (0.2f * ((float)currentSafety)) + 150f;
        //float result = (Mathf.Pow(1.045f, -currentSafety)) + 5f;
        //if (currentSafety < 0) { result *= -1f; }
        float result = 0f;
        if (currentSafety >= 0f) {
            result = 20f;
        } else {
            result = ((100f * currentSafety) / 25f) - 50f;
        }
        return result;
    }
    private float GetDistanceModifier(HexTile from, HexTile to) {
        int distance = PathGenerator.Instance.GetDistanceBetweenTwoTiles(from, to);
        if(distance == 99999) {
            return 1f;
        }
        if (distance > 10) {
            return 0.5f;
        }else if (distance > 5) {
            return 0.8f;
        }
        return 1f;
    }
    #endregion
}
