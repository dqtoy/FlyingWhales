using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using System;

[System.Serializable]
public class CharacterAction {
    //protected ObjectState _state;
    protected ActionFilter[] _filters;
    [SerializeField] protected CharacterActionData _actionData;
    protected int _weight;
    protected int _disableCounter; //if has at least 1 point, disable action
    protected int _enableCounter; //remove action from list if this has 0 point
    protected Action onEndAction;

    #region getters/setters
    public ACTION_TYPE actionType {
        get { return _actionData.actionType; }
    }
    public int weight {
        get { return _weight; }
    }
    public int disableCounter {
        get { return _disableCounter; }
    }
    public int enableCounter {
        get { return _enableCounter; }
    }
    public ActionFilter[] filters {
        get { return _filters; }
    }
    //public ObjectState state {
    //    get { return _state; }
    //}
    public CharacterActionData actionData {
        get { return _actionData; }
    }
    #endregion

    public CharacterAction(ACTION_TYPE actionType) {
        //_state = state;
        _actionData.actionType = actionType;
        _actionData.actionName = Utilities.NormalizeStringUpperCaseFirstLetters(actionType.ToString());
        _weight = 100;
    }

    #region Virtuals
    public virtual void Initialize() { }
    public virtual void OnChooseAction(Party iparty, IObject targetObject) { }
    public virtual void OnFirstEncounter(Party party, IObject targetObject) {
        //string startActionLog = GetStartActionString(party);
        //string startActionLog = GetStartActionString(party);
        //if (!string.IsNullOrEmpty(startActionLog) && targetObject != null) {
        //    //Log arriveLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "start_action");
        //    //arriveLog.AddToFillers(party, party.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //    //arriveLog.AddToFillers(null, startActionLog, LOG_IDENTIFIER.ACTION_DESCRIPTION);
        //    //for (int i = 0; i < party.icharacters.Count; i++) {
        //    //    party.icharacters[i].AddHistory(arriveLog);
        //    //}
        //    if (targetObject.objectLocation != null) {
        //        for (int i = 0; i < party.icharacters.Count; i++) {
        //            ICharacter currCharacter = party.icharacters[i];
        //            Log startLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "start_action");
        //            startLog.AddToFillers(party, party.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //            startLog.AddToFillers(targetObject.objectLocation, targetObject.objectLocation.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        //            startLog.AddToFillers(null, startActionLog, LOG_IDENTIFIER.ACTION_DESCRIPTION);
        //            currCharacter.AddHistory(startLog);
        //        }
        //        Log landmarkLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "start_action");
        //        landmarkLog.AddToFillers(party, party.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        landmarkLog.AddToFillers(targetObject.objectLocation, targetObject.objectLocation.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        //        landmarkLog.AddToFillers(null, startActionLog, LOG_IDENTIFIER.ACTION_DESCRIPTION);
        //        targetObject.objectLocation.AddHistory(landmarkLog);
        //    } else {
        //        for (int i = 0; i < party.icharacters.Count; i++) {
        //            ICharacter currCharacter = party.icharacters[i];
        //            Log startLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "start_action");
        //            startLog.AddToFillers(party, party.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //            startLog.AddToFillers(targetObject.specificLocation.tileLocation, targetObject.specificLocation.tileLocation.tileName, LOG_IDENTIFIER.LANDMARK_1);
        //            startLog.AddToFillers(null, startActionLog, LOG_IDENTIFIER.ACTION_DESCRIPTION);
        //            currCharacter.AddHistory(startLog);
        //        }
        //    }
        //}
    }
    public virtual void PerformAction(Party party, IObject targetObject) { }
    public virtual void ActionSuccess(IObject targetObject) {
        if (_actionData.successFunction != null) {
            _actionData.successFunction.Invoke(targetObject);
        }
    }
    public virtual void ActionFail(IObject targetObject) {
        if (_actionData.failFunction != null) {
            _actionData.failFunction.Invoke(targetObject);
        }
    }
    public virtual CharacterAction Clone() {
        CharacterAction clone = new CharacterAction(actionType);
        SetCommonData(clone);
        clone.Initialize();
        return clone;
    }
    public virtual bool CanBeDone(IObject targetObject) {
        return true;
    }
    public virtual bool CanBeDoneBy(Party party, IObject targetObject) {
        return true;
    }
    public virtual void EndAction(Party party, IObject targetObject) {
        party.EndAction();
        if (onEndAction != null) {
            onEndAction();
        }
    }
    public virtual void DoneDuration(Party party, IObject targetObject) { }
    public virtual void SuccessEndAction(Party party) {
        Messenger.Broadcast(Signals.ACTION_SUCCESS, party, this);
    }
    public virtual bool ShouldGoToTargetObjectOnChoose() {
        return true;
    }
    //Give all provided needs to the character regardless of the amount
    public virtual void GiveAllReward(CharacterParty party) {
        for (int i = 0; i < party.characters.Count; i++) {
            Character icharacter = party.characters[i];
            icharacter.role.AdjustFullness(_actionData.providedFullness);
            icharacter.role.AdjustEnergy(_actionData.providedEnergy);
            icharacter.role.AdjustFun(_actionData.providedFun);
            //icharacter.role.AdjustPrestige(_actionData.providedPrestige);
            //icharacter.role.AdjustSanity(_actionData.providedSanity);
            //icharacter.role.AdjustSafety(_actionData.providedSafety);
            if (_actionData.hpRecoveredPercentage != 0f && icharacter.currentHP < icharacter.hp) {
                float hpRecovery = (_actionData.hpRecoveredPercentage / 100f) * (float) icharacter.hp;
                //icharacter.AdjustHP((int) hpRecovery);
            }
        }

    }
    public virtual IObject GetTargetObject(CharacterParty sourceParty) {
        return sourceParty.characterObject;
    }
    public virtual SCHEDULE_ACTION_CATEGORY GetSchedActionCategory() {
        return SCHEDULE_ACTION_CATEGORY.NONE;
    }
    #endregion

    #region Filters
    public void SetFilters(ActionFilter[] filters) {
        _filters = filters;
    }
    public virtual bool MeetsRequirements(CharacterParty party, BaseLandmark landmark) {
        if (filters != null) {
            Character character = party.mainCharacter;
            for (int i = 0; i < filters.Length; i++) {
                ActionFilter currFilter = filters[i];
                if (!currFilter.MeetsRequirements(character, landmark)) {
                    return false; //does not meet a requirement
                }
            }
        }
        return true; //meets all requirements
    }
    public virtual bool MeetsRequirements(string className, BaseLandmark landmark) {
        if (filters != null) {
            for (int i = 0; i < filters.Length; i++) {
                ActionFilter currFilter = filters[i];
                if (!currFilter.MeetsRequirements(className, landmark)) {
                    return false; //does not meet a requirement
                }
            }
        }
        return true; //meets all requirements
    }
    #endregion

    #region Utilities
    public void SetActionData(CharacterActionData data) {
        _actionData = data;
    }
    public void SetActionCategory(ACTION_CATEGORY category) {
        _actionData.actionCategory = category;
    }
    //public void SetObjectState(ObjectState state) {
    //    _state = state;
    //}
    public void GenerateName() {
        _actionData.actionName = Utilities.NormalizeStringUpperCaseFirstLetters(actionType.ToString());
    }
    //Give specific provided need to a character
    public void GiveReward(NEEDS need, CharacterParty party) {
        for (int i = 0; i < party.characters.Count; i++) {
            Character icharacter = party.characters[i];
            switch (need) {
                case NEEDS.FULLNESS:
                icharacter.role.AdjustFullness(_actionData.providedFullness);
                break;
                case NEEDS.ENERGY:
                icharacter.role.AdjustEnergy(_actionData.providedEnergy);
                break;
                case NEEDS.FUN:
                icharacter.role.AdjustFun(_actionData.providedFun);
                break;
                //case NEEDS.PRESTIGE:
                //icharacter.role.AdjustPrestige(_actionData.providedPrestige);
                //break;
                //case NEEDS.SANITY:
                //icharacter.role.AdjustSanity(_actionData.providedSanity);
                //break;
                //case NEEDS.SAFETY:
                //icharacter.role.AdjustSafety(_actionData.providedSafety);
                //break;
            }
        }
    }
    public void SetDuration(int duration) {
        _actionData.duration = duration;
    }

    public void SetCommonData(CharacterAction action) {
        if (this._filters != null) {
            action._filters = new ActionFilter[this._filters.Length];
            for (int i = 0; i < this._filters.Length; i++) {
                action._filters[i] = this._filters[i];
            }
        }
        action._actionData = this._actionData;
        if(action._actionData.prerequisites != null) {
            for (int i = 0; i < action._actionData.prerequisites.Count; i++) {
                action._actionData.prerequisites[i].SetAction(action);
            }
        }
    }
    public void AdjustWeight(int amount) {
        _weight += amount;
    }
    public void AdjustDisableCounter(int amount) {
        _disableCounter += amount;
    }
    public void AdjustEnableCounter(int amount) {
        _enableCounter += amount;
    }
    public void OnAddActionToCharacter(Character character) {
        for (int i = 0; i < character.attributes.Count; i++) {
            character.attributes[i].CharacterHasAction(this);
        }
    }
    public void OnRemoveActionFromCharacter(Character character) {

    }
    public void SetOnEndAction(Action action) {
        onEndAction = action;
    }
    #endregion

    #region Advertisement
    public float GetTotalAdvertisementValue(Character character) {
        return GetFoodAdvertisementValue(character) + GetEnergyAdvertisementValue(character) + GetJoyAdvertisementValue(character);
            //+ GetPrestigeAdvertisementValue(character);
    }
    private float GetAdvertisementValue(float currentNeed, float advertisedNeed) {
        //350, 8
        if(advertisedNeed != 0) {
            float x = currentNeed;
            float y = x + (advertisedNeed * 80f);
            if(y > 1000f) {
                y = 1000f;
            }
            float result = 1000f / y;
            if (y > 0) {
               result = (1000f / x) - (1000f / y);
            }
            //Add quest modifier
            return result;
        }
        return 0f;
    }
    private float GetFoodAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.fullness, _actionData.advertisedFullness);
    }
    private float GetEnergyAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.energy, _actionData.advertisedEnergy);
    }
    private float GetJoyAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.fun, _actionData.advertisedFun);
    }
    //private float GetPrestigeAdvertisementValue(Character character) {
    //    return GetAdvertisementValue(character.role.prestige, _actionData.advertisedPrestige);
    //}
    #endregion

    #region Logs
    public virtual string GetStartActionString(Party party) {
        string file = this.GetType().ToString();
        if (LocalizationManager.Instance.localizedText["CharacterActions"].ContainsKey(file)) {
            return LocalizationManager.Instance.GetLocalizedValue("CharacterActions", file, "start_action");
        }
        Debug.LogWarning("No Localized text for action " + file);
        return string.Empty;
    }
    public virtual string GetArriveActionString(Party party = null) {
        string file = this.GetType().ToString();
        if (LocalizationManager.Instance.localizedText["CharacterActions"].ContainsKey(file)) {
            return LocalizationManager.Instance.GetLocalizedValue("CharacterActions", file, "arrive_action");
        }
       Debug.LogWarning("No Localized text for action " + file);
        return string.Empty;
    }
    public virtual string GetLeaveActionString(Party party = null) {
        string file = this.GetType().ToString();
        if (LocalizationManager.Instance.localizedText["CharacterActions"].ContainsKey(file)) {
            return LocalizationManager.Instance.GetLocalizedValue("CharacterActions", file, "leave_action");
        }
        Debug.LogWarning("No Localized text for action " + file);
        return string.Empty;
    }
    #endregion
}
