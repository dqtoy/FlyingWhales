/*
 This is the base class for character roles
 such as Chieftain, Village Head, etc.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class CharacterRole {
	protected Character _character;
    protected CHARACTER_ROLE _roleType;
	protected bool _cancelsAllOtherTasks;
	protected bool _isRemoved;
    protected bool _isHungry, _isFamished, _isTired, _isExhausted, _isSad, _isDepressed, _isAnxious, _isInsecure;
    protected float _fullness, _energy, _fun;
    //, _prestige, _sanity, _safety;
    protected float _maxFullness, _maxEnergy, _maxFun;
    //, _maxPrestige, _maxSanity, _maxSafety;
    protected float _minFullness, _minEnergy, _minFun;
    //, _minPrestige, _minSanity, _minSafety;
    protected float _happiness;

    protected float _constantSanityBuff, _constantFunBuff;

    private const float Needs_Threshold = 80f;

    #region getters/setters
    public CHARACTER_ROLE roleType {
        get { return _roleType; }
    }
	public Character character{
		get { return _character; }
	}
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
    //public float prestige {
    //    get { return _prestige; }
    //}
    //public float sanity {
    //    get { return Mathf.Clamp(_sanity + _constantSanityBuff, _minSanity, _maxSanity); }
    //}
    //public float safety {
    //    get { return _safety; }
    //}
    public float maxFullness {
        get { return _maxFullness; }
    }
    public float maxEnergy {
        get { return _maxEnergy; }
    }
    public float maxFun {
        get { return _maxFun; }
    }
    //public float maxPrestige {
    //    get { return _maxPrestige; }
    //}
    //public float maxSanity {
    //    get { return _maxSanity; }
    //}
    //public float maxSafety {
    //    get { return _maxSafety; }
    //}
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
        //_maxPrestige = 100f;
        //_maxSanity = 100f;
        //_maxSafety = 100f;

        _minFullness = -100f;
        _minEnergy = -100f;
        _minFun = -100f;
        //_minPrestige = -100f;
        //_minSanity = -100f;
        //_minSafety = -100f;
    }
    
    #region Virtuals
    public virtual void DeathRole(){
		_isRemoved = true;
        //_character.onDailyAction -= StartDepletion;
    }
	public virtual void ChangedRole(){
		_isRemoved = true;
        //_character.onDailyAction -= StartDepletion;
    }
    public virtual void OnAssignRole() {
        //_character.onDailyAction += StartDepletion;
    }
    #endregion

    #region Needs
    private void StartDepletion() {
        DepleteFullness();
        DepleteEnergy();
        DepleteFun();
        //DepletePrestige();
    }
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
            //case NEEDS.PRESTIGE:
            //    SetPrestige(newValue);
            //    break;
            //case NEEDS.SANITY:
            //    SetSanity(newValue);
            //    break;
            //case NEEDS.SAFETY:
            //    SetSafety(newValue);
            //    break;
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
            //case NEEDS.PRESTIGE:
            //    return _prestige;
            //case NEEDS.SANITY:
            //    return _sanity;
            //case NEEDS.SAFETY:
            //    return _safety;
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
        //        _character.RemoveCharacterAttribute(ATTRIBUTE.HUNGRY);
        //    }
        //    _character.AssignTag(ATTRIBUTE.FAMISHED);
        //}
        //else if(_fullness > 10f && _fullness <= 30f && !_isHungry) {
        //    _isHungry = true;
        //    if (_isFamished) {
        //        _isFamished = false;
        //        _character.RemoveCharacterAttribute(ATTRIBUTE.FAMISHED);
        //    }
        //    _character.AssignTag(ATTRIBUTE.HUNGRY);
        //}
        //else if (_fullness > 30f) {
        //    if (_isHungry) {
        //        _isHungry = false;
        //        _character.RemoveCharacterAttribute(ATTRIBUTE.HUNGRY);
        //    }
        //    if (_isFamished) {
        //        _isFamished = false;
        //        _character.RemoveCharacterAttribute(ATTRIBUTE.FAMISHED);
        //    }
        //}
    }
    private void OnFullnessEdited() {
        if (_fullness < 0 && _fullness >= -75) {
            //Character gains Hungry tag when Fullness is below 0 to -75.
            _character.AddAttribute(ATTRIBUTE.HUNGRY);
            _character.RemoveAttribute(ATTRIBUTE.STARVING);
        } else if (_fullness < -75) {
            //Character gains Starving tag when Fullness is below -75   
            _character.AddAttribute(ATTRIBUTE.STARVING);
            _character.RemoveAttribute(ATTRIBUTE.HUNGRY);
        } else {
            _character.RemoveAttributes(new List<ATTRIBUTE>() { ATTRIBUTE.HUNGRY, ATTRIBUTE.STARVING });
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
        //        _character.RemoveCharacterAttribute(ATTRIBUTE.TIRED);
        //    }
        //    _character.AssignTag(ATTRIBUTE.EXHAUSTED);
        //}
        //else if (_energy > 10f && _energy <= 30f && !_isTired) {
        //    _isTired = true;
        //    if (_isExhausted) {
        //        _isExhausted = false;
        //        _character.RemoveCharacterAttribute(ATTRIBUTE.EXHAUSTED);
        //    }
        //    _character.AssignTag(ATTRIBUTE.TIRED);
        //}
        //else if (_energy > 30f) {
        //    if (_isTired) {
        //        _isTired = false;
        //        _character.RemoveCharacterAttribute(ATTRIBUTE.TIRED);
        //    }
        //    if (_isExhausted) {
        //        _isExhausted = false;
        //        _character.RemoveCharacterAttribute(ATTRIBUTE.EXHAUSTED);
        //    }
        //}
    }
    private void OnEnergyEdited() {
        if (_energy < 0 && _energy >= -75) {
            //Character gains Tired tag when Energy is below 0 to -75.
            _character.AddAttribute(ATTRIBUTE.TIRED);
            _character.RemoveAttribute(ATTRIBUTE.EXHAUSTED);
        } else if (_energy < -75) {
            //Character gains Crazed tag when Energy is below -75
            _character.AddAttribute(ATTRIBUTE.EXHAUSTED);
            _character.RemoveAttribute(ATTRIBUTE.TIRED);
        } else {
            _character.RemoveAttributes(new List<ATTRIBUTE>() { ATTRIBUTE.TIRED, ATTRIBUTE.EXHAUSTED });
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
        //        _character.RemoveCharacterAttribute(ATTRIBUTE.SAD);
        //    }
        //    _character.AssignTag(ATTRIBUTE.DEPRESSED);
        //}
        //else if (_fun > 10f && _fun <= 30f && !_isSad) {
        //    _isSad = true;
        //    if (_isDepressed) {
        //        _isDepressed = false;
        //        _character.RemoveCharacterAttribute(ATTRIBUTE.DEPRESSED);
        //    }
        //    _character.AssignTag(ATTRIBUTE.SAD);
        //}
        //else if (_fun > 30f) {
        //    if (_isSad) {
        //        _isSad = false;
        //        _character.RemoveCharacterAttribute(ATTRIBUTE.SAD);
        //    }
        //    if (_isDepressed) {
        //        _isDepressed = false;
        //        _character.RemoveCharacterAttribute(ATTRIBUTE.DEPRESSED);
        //    }
        //}
    }
    private void OnFunEdited() {
        if (_fun < 0 && _fun >= -75) {
            //Character gains Sad tag when Fun is below 0 to -75.
            _character.AddAttribute(ATTRIBUTE.SAD);
            _character.RemoveAttribute(ATTRIBUTE.DEPRESSED);
        } else if (_fun < -75) {
            //Character gains Depressed tag when Fun is below -75
            _character.AddAttribute(ATTRIBUTE.DEPRESSED);
            _character.RemoveAttribute(ATTRIBUTE.SAD);
        } else {
            _character.RemoveAttributes(new List<ATTRIBUTE>() { ATTRIBUTE.SAD, ATTRIBUTE.DEPRESSED });
        }
    }

    //public void DepletePrestige() {
    //    if(_prestige > 0) {
    //        AdjustPrestige(-1.39f);
    //    } else {
    //        AdjustPrestige(-0.245f);
    //    }
    //}
    //public void SetPrestige(float amount) {
    //    _prestige = amount;
    //    OnPrestigeEdited();
    //}
    //public void AdjustPrestige(float amount) {
    //    float previous = _prestige;
    //    _prestige += amount;
    //    _prestige = Mathf.Clamp(_prestige, _minPrestige, _maxPrestige);
    //    if (previous != _prestige) {
    //        UpdateHappiness();
    //        OnPrestigeEdited();
    //    }
    //    //if (_prestige <= 0f && !_isInsecure) {
    //    //    _isInsecure = true;
    //    //    if (_isAnxious) {
    //    //        _isAnxious = false;
    //    //        _character.RemoveCharacterAttribute(ATTRIBUTE.ANXIOUS);
    //    //    }
    //    //    _character.AssignTag(ATTRIBUTE.INSECURE);
    //    //}
    //    //else if (_prestige > 10f && _prestige <= 30f && !_isAnxious) {
    //    //    _isAnxious = true;
    //    //    if (_isInsecure) {
    //    //        _isInsecure = false;
    //    //        _character.RemoveCharacterAttribute(ATTRIBUTE.INSECURE);
    //    //    }
    //    //    _character.AssignTag(ATTRIBUTE.ANXIOUS);
    //    //}
    //    //else if (_prestige > 30f) {
    //    //    if (_isAnxious) {
    //    //        _isAnxious = false;
    //    //        _character.RemoveCharacterAttribute(ATTRIBUTE.ANXIOUS);
    //    //    }
    //    //    if (_isInsecure) {
    //    //        _isInsecure = false;
    //    //        _character.RemoveCharacterAttribute(ATTRIBUTE.INSECURE);
    //    //    }
    //    //}
    //}
    //private void OnPrestigeEdited() {
    //    if (_prestige < 0 && _prestige >= -75) {
    //        //Character gains Anxious tag when Prestige is below 0 to -75.
    //        _character.AssignTag(ATTRIBUTE.ANXIOUS);
    //        _character.RemoveCharacterAttribute(ATTRIBUTE.DEMORALIZED);
    //    } else if (_prestige < -75) {
    //        //Character gains Demoralized tag when Sanity is below -75
    //        _character.AssignTag(ATTRIBUTE.DEMORALIZED);
    //        _character.RemoveCharacterAttribute(ATTRIBUTE.ANXIOUS);
    //    } else {
    //        _character.RemoveCharacterAttribute(new List<ATTRIBUTE>() { ATTRIBUTE.ANXIOUS, ATTRIBUTE.DEMORALIZED });
    //    }
    //}

    //public void DepleteSanity() {
    //    AdjustSanity(-0.2f);
    //}
    //public void SetSanity(float amount) {
    //    _sanity = amount;
    //    OnSanityEdited();
    //}
    //public void AdjustSanity(float amount) {
    //    float previous = _sanity;
    //    _sanity += amount;
    //    _sanity = Mathf.Clamp(_sanity, _minSanity, _maxSanity);
    //    if (previous != _sanity) {
    //        UpdateHappiness();
    //        OnSanityEdited();
    //    }
    //}
    //private void OnSanityEdited() {
    //    if (_sanity < 0 && _sanity >= -75) {
    //        //Character gains Disturbed tag when Sanity is below 0 to -75.
    //        _character.AssignTag(ATTRIBUTE.DISTURBED);
    //        _character.RemoveCharacterAttribute(ATTRIBUTE.CRAZED);
    //    } else if (_sanity < -75) {
    //        //Character gains Crazed tag when Sanity is below -75
    //        _character.AssignTag(ATTRIBUTE.CRAZED);
    //        _character.RemoveCharacterAttribute(ATTRIBUTE.DISTURBED);
    //    } else {
    //        _character.RemoveCharacterAttribute(new List<ATTRIBUTE>() { ATTRIBUTE.DISTURBED, ATTRIBUTE.CRAZED });
    //    }
    //}

    //public void SetSafety(float amount) {
    //    _safety = amount;
    //    OnSafetyEdited();
    //}
    //public void AdjustSafety(float amount) {
    //    float previous = _safety;
    //    _safety += amount;
    //    _safety = Mathf.Clamp(_safety, _minSafety, _maxSafety);
    //    if (previous != _safety) {
    //        UpdateHappiness();
    //        OnSafetyEdited();
    //    }
    //}
    //public void UpdateSafety() {
    //    float hpPercent = (float) character.currentHP / (float) character.maxHP;
    //    float newSafety = (hpPercent * (_maxSafety - _minSafety)) + _minSafety;
    //    SetSafety(newSafety);
    //}
    //private void OnSafetyEdited() {
    //    if (_safety < 0 && _safety >= -75) {
    //        //Character gains Wounded tag when Safety is below 0 to -75.
    //        _character.AssignTag(ATTRIBUTE.WOUNDED);
    //        _character.RemoveCharacterAttribute(ATTRIBUTE.WRECKED);
    //    } else if (_safety < -75) {
    //        //Character gains Wrecked tag when Safety is below -75.
    //        _character.AssignTag(ATTRIBUTE.WRECKED);
    //        _character.RemoveCharacterAttribute(ATTRIBUTE.WOUNDED);
    //    } else {
    //        _character.RemoveCharacterAttribute(new List<ATTRIBUTE>() { ATTRIBUTE.WOUNDED, ATTRIBUTE.WRECKED });
    //    }
    //}

    //public void AdjustConstantSanityBuff(int adjustment) {
    //    _constantSanityBuff += adjustment;
    //}
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
            //case NEEDS.PRESTIGE:
            //return _prestige >= _maxPrestige;
            //case NEEDS.SANITY:
            //return _sanity >= _maxSanity;
            //case NEEDS.SAFETY:
            //return _safety >= _maxSafety;
        }
        return false;
    }

    /*
     This is to check if the average of the 3 needs meet the threshold.
         */
    public bool AreNeedsMet() {
        float average = (fullness + energy + fun) / 3f;
        if (average >= Needs_Threshold) {
            return true; //needs are met
        }
        return false;
    }

    public void UpdateHappiness() {
        _happiness = (CalculateFullnessImpact(_fullness) + CalculateEnergyImpact(_energy) + CalculateFunImpact(_fun)) / 6f;
        //+ CalculatePrestigeImpact(_prestige) + CalculateSanityImpact(_sanity) + CalculateSafetyImpact(_safety)) / 6f;
    }

    public float GetTotalHappinessIncrease(CharacterAction characterAction, IObject targetObject) {
        float result = (GetHappinessIncrease(NEEDS.FULLNESS, characterAction) + GetHappinessIncrease(NEEDS.ENERGY, characterAction) + GetHappinessIncrease(NEEDS.FUN, characterAction));
            //+ GetHappinessIncrease(NEEDS.PRESTIGE, characterAction) + GetHappinessIncrease(NEEDS.SANITY, characterAction) + GetHappinessIncrease(NEEDS.SAFETY, characterAction)); // GetDistanceModifier(_character.specificLocation.tileLocation, characterAction.state.obj.specificLocation.tileLocation);

        if (targetObject.objectType == OBJECT_TYPE.STRUCTURE) {
            Area areaOfStructure = targetObject.objectLocation.tileLocation.areaOfTile;
            if (areaOfStructure != null && _character.homeLandmark.tileLocation.areaOfTile != null && areaOfStructure.id == _character.homeLandmark.tileLocation.areaOfTile.id) {
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
            //case NEEDS.PRESTIGE:
            //currentAmount = _prestige;
            //maxAmount = _maxPrestige;
            //advertisedAmount = action.actionData.advertisedPrestige;
            //calculateImpact = CalculatePrestigeImpact;
            //break;
            //case NEEDS.SANITY:
            //currentAmount = _sanity;
            //maxAmount = _maxSanity;
            //advertisedAmount = action.actionData.advertisedSanity;
            //calculateImpact = CalculateSanityImpact;
            //break;
            //case NEEDS.SAFETY:
            //currentAmount = _safety;
            //maxAmount = _maxSafety;
            //advertisedAmount = action.actionData.advertisedSafety;
            //calculateImpact = CalculateSafetyImpact;
            //break;
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
        //if(currentFullness >= 0f) {
        //    result = (currentFullness / 10f) + 30f;
        //} else {
        //    result = (0.65f * (currentFullness / 25f)) * -currentFullness;
        //}
        //if (currentFullness < 0) { result *= -1f; }
        if(currentFullness >= -25f) {
            result = (currentFullness / 0.5f) + 50f;
        }else if (currentFullness <= -50f && currentFullness >= -75f) {
            result = currentFullness + 50f;
        } else if (currentFullness < -75f) {
            result = (currentFullness * 3f) - 200f;
        }
        if (result > 100f) {
            result = 100f;
        } else if (result < -100f) {
            result = -100f;
        }
        return result;
    }

    //Formula for calculation of happiness based on current energy, meaning what's the happiness equivalent given the energy
    private float CalculateEnergyImpact(float currentEnergy) {
        //return (-0.4f * ((float) -currentEnergy)) + 350f;
        //float result = (0.5f * -currentEnergy) + 50f;
        //if (currentEnergy < 0) { result *= -1f; }

        float result = currentEnergy;
        //if (currentEnergy >= 0f) {
        //    result = (currentEnergy / 2.5f) + 30f;
        //} else {
        //    result = currentEnergy;
        //}
        return result;
    }

    //Formula for calculation of happiness based on current fun, meaning what's the happiness equivalent given the fun
    private float CalculateFunImpact(float currentFun) {
        //float value = 1.09f; //* currentFun;
        //float result = (Mathf.Pow(value, -currentFun)) + 10f;
        //if (currentFun < 0) { result *= -1f; }

        float result = 0f;
        //if (currentFun >= 0f) {
        //    //result = (((3f * currentFun) / 10f) * 2f) + 40f;
        //    result = (currentFun / 2.5f) + 40;
        //} else {
        //    //result = currentFun / 2f;
        //    result = ((0.4f * (currentFun / 11f)) * -currentFun) + 40;
        //}
        if(currentFun >= 0f) {
            result = (currentFun / 0.5f) + 50f;
        }else if(currentFun >= -50f && currentFun < 0f) {
            result = currentFun + 50f;
        } else {
            result = (currentFun / 0.5f) - 100f;
        }
        if (result > 100f) {
            result = 100f;
        } else if (result < -100f) {
            result = -100f;
        }
        return result;
    }

    ////Formula for calculation of happiness based on current prestige, meaning what's the happiness equivalent given the prestige
    //private float CalculatePrestigeImpact(float currentPrestige) {
    //    //float value = 1.07f;// * currentPrestige;
    //    //float result = (Mathf.Pow(value, -currentPrestige)) + 15f;
    //    //if (currentPrestige < 0) { result *= -1f; }
    //    float result = 0f;
    //    if (currentPrestige >= 50f) {
    //        //result = currentPrestige - 50f;
    //        result = (currentPrestige / 0.5f) - 100f;
    //    } else {
    //        //result = currentPrestige * 2f;
    //        result = currentPrestige - 50f;
    //    }
    //    if (result > 100f) {
    //        result = 100f;
    //    } else if (result < -100f) {
    //        result = -100f;
    //    }
    //    return result;
    //}

    ////Formula for calculation of happiness based on current sanity, meaning what's the happiness equivalent given the sanity
    //private float CalculateSanityImpact(float currentSanity) {
    //    //float value = 1.08f;// * currentSanity;
    //    //float result = Mathf.Pow(value, -currentSanity);
    //    //if (currentSanity < 0) { result *= -1f; }
    //    float result = 0f;
    //    if (currentSanity > 0f) {
    //        //result = ((4f * currentSanity) / 20f) + 70f;
    //        result = currentSanity * 2f;
    //    } else {
    //        //result = Mathf.Pow((currentSanity / 5f), 2f);
    //        result = currentSanity;
    //    }
    //    if(result > 100f) {
    //        result = 100f;
    //    }else if(result < -100f) {
    //        result = -100f;
    //    }
    //    return result;
    //}

    ////Formula for calculation of happiness based on current safety, meaning what's the happiness equivalent given the safety
    //private float CalculateSafetyImpact(float currentSafety) {
    //    //return (0.2f * ((float)currentSafety)) + 150f;
    //    //float result = (Mathf.Pow(1.045f, -currentSafety)) + 5f;
    //    //if (currentSafety < 0) { result *= -1f; }
    //    float result = 0f;
    //    if (currentSafety <= 50f && currentSafety >= -50f) {
    //        //result = 20f;
    //        result = currentSafety;
    //    } else {
    //        //result = ((100f * currentSafety) / 25f) - 50f;
    //        result = (currentSafety / 0.5f) - 50f;
    //    }
    //    if (result > 100f) {
    //        result = 100f;
    //    } else if (result < -100f) {
    //        result = -100f;
    //    }
    //    return result;
    //}
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
