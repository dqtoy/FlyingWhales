using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

[System.Serializable]
public class CharacterAction {
    //protected ObjectState _state;
    protected ActionFilter[] _filters;
    [SerializeField] protected CharacterActionData _actionData;

    #region getters/setters
    public ACTION_TYPE actionType {
        get { return _actionData.actionType; }
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
    }

    #region Virtuals
    public virtual void Initialize() {}
    public virtual void OnChooseAction(NewParty iparty, IObject targetObject) {}
    public virtual void OnFirstEncounter(CharacterParty party, IObject targetObject) {
        string arriveActionLog = GetArriveActionString();
        if(arriveActionLog != string.Empty) {
            if (targetObject.objectLocation != null) {
                Log arriveLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "arrive_location");
                arriveLog.AddToFillers(party, party.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                arriveLog.AddToFillers(targetObject.objectLocation, targetObject.objectLocation.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
                arriveLog.AddToFillers(null, arriveActionLog, LOG_IDENTIFIER.ACTION_DESCRIPTION);
                for (int i = 0; i < party.icharacters.Count; i++) {
                    party.icharacters[i].AddHistory(arriveLog);
                }
            } else {
                Log arriveLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "arrive_location");
                arriveLog.AddToFillers(party, party.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                arriveLog.AddToFillers(targetObject.specificLocation.tileLocation, targetObject.specificLocation.tileLocation.tileName, LOG_IDENTIFIER.LANDMARK_1);
                arriveLog.AddToFillers(null, arriveActionLog, LOG_IDENTIFIER.ACTION_DESCRIPTION);
                for (int i = 0; i < party.icharacters.Count; i++) {
                    party.icharacters[i].AddHistory(arriveLog);
                }
            }
        }
    }
    public virtual void PerformAction(CharacterParty party, IObject targetObject) {}
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
    public virtual bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
        return true;
    }
    public virtual void EndAction(CharacterParty party, IObject targetObject) {
        party.actionData.EndAction();
    }
    public virtual void DoneDuration(CharacterParty party, IObject targetObject) { }
    public virtual void SuccessEndAction(CharacterParty party) {
        Messenger.Broadcast(Signals.ACTION_SUCCESS, party, this);
    }
    #endregion

    #region Filters
    public void SetFilters(ActionFilter[] filters) {
        _filters = filters;
    }
    public virtual bool MeetsRequirements(CharacterParty party, BaseLandmark landmark) {
        if (filters != null && party.mainCharacter is Character) {
            Character character = party.mainCharacter as Character;
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
        for (int i = 0; i < party.icharacters.Count; i++) {
            ICharacter icharacter = party.icharacters[i];
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
                case NEEDS.PRESTIGE:
                icharacter.role.AdjustPrestige(_actionData.providedPrestige);
                break;
                case NEEDS.SANITY:
                icharacter.role.AdjustSanity(_actionData.providedSanity);
                break;
                case NEEDS.SAFETY:
                icharacter.role.AdjustSafety(_actionData.providedSafety);
                break;
            }
        }
    }

    //Give all provided needs to the character regardless of the amount
    public void GiveAllReward(CharacterParty party) {
        for (int i = 0; i < party.icharacters.Count; i++) {
            ICharacter icharacter = party.icharacters[i];
            icharacter.role.AdjustFullness(_actionData.providedFullness);
            icharacter.role.AdjustEnergy(_actionData.providedEnergy);
            icharacter.role.AdjustFun(_actionData.providedFun);
            icharacter.role.AdjustPrestige(_actionData.providedPrestige);
            icharacter.role.AdjustSanity(_actionData.providedSanity);
            icharacter.role.AdjustSafety(_actionData.providedSafety);
            if (_actionData.hpRecoveredPercentage != 0f && icharacter.currentHP < icharacter.maxHP) {
                float hpRecovery = (_actionData.hpRecoveredPercentage / 100f) * (float) icharacter.maxHP;
                icharacter.AdjustHP((int) hpRecovery);
            }
        }
        
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
    #endregion

    #region Advertisement
    public float GetTotalAdvertisementValue(Character character) {
        return GetFoodAdvertisementValue(character) + GetEnergyAdvertisementValue(character) + GetJoyAdvertisementValue(character) + GetPrestigeAdvertisementValue(character);
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
    private float GetPrestigeAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.prestige, _actionData.advertisedPrestige);
    }
    #endregion

    #region Logs
    public virtual string GetArriveActionString() {
        string file = this.GetType().ToString();
        if (LocalizationManager.Instance.localizedText["CharacterActions"][file].ContainsKey("arrive_action")) {
            return LocalizationManager.Instance.GetLocalizedValue("CharacterActions", file, "arrive_action");
        }
        return string.Empty;
    }
    public virtual string GetLeaveActionString() {
        string file = this.GetType().ToString();
        if (LocalizationManager.Instance.localizedText["CharacterActions"][file].ContainsKey("leave_action")) {
            return LocalizationManager.Instance.GetLocalizedValue("CharacterActions", file, "leave_action");
        }
        return string.Empty;
    }
    #endregion
}
