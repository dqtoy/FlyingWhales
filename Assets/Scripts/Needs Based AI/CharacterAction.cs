using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

[System.Serializable]
public class CharacterAction {
    protected ObjectState _state;
    protected ActionFilter[] _filters;
    [SerializeField] protected CharacterActionData _actionData;

    #region getters/setters
    public ACTION_TYPE actionType {
        get { return _actionData.actionType; }
    }
    public ObjectState state {
        get { return _state; }
    }
    public CharacterActionData actionData {
        get { return _actionData; }
    }
    #endregion

    public CharacterAction(ObjectState state, ACTION_TYPE actionType) {
        _state = state;
        _actionData.actionType = actionType;
        _actionData.actionName = Utilities.NormalizeStringUpperCaseFirstLetters(actionType.ToString());
    }

    public CharacterAction Clone(ObjectState state) {
        CharacterAction clone = new CharacterAction(state, this.actionType);
        if (this._filters != null) {
            clone._filters = new ActionFilter[this._filters.Length];
            for (int i = 0; i < this._filters.Length; i++) {
                clone._filters[i] = this._filters[i];
            }
        }
        clone._actionData = this._actionData;
        return clone;
    }

    #region Virtuals
    public virtual void PerformAction(Character character) {

    }
    public virtual void ActionSuccess() {
        if (_actionData.successFunction != null) {
            _actionData.successFunction.Invoke();
        }
    }
    public virtual void ActionFail() {
        if (_actionData.failFunction != null) {
            _actionData.failFunction.Invoke();
        }
    }
    #endregion

    #region Utilities
    public void SetObjectState(ObjectState state) {
        _state = state;
    }
    public void GenerateName() {
        _actionData.actionName = Utilities.NormalizeStringUpperCaseFirstLetters(actionType.ToString());
    }
    public void EndAction(Character character) {
        character.actionData.EndAction();
    }
    public void GiveReward(NEEDS need, Character character) {
        switch (need) {
            case NEEDS.FULLNESS:
            character.role.AdjustFullness(_actionData.providedFullness);
            break;
            case NEEDS.ENERGY:
            character.role.AdjustEnergy(_actionData.providedEnergy);
            break;
            case NEEDS.JOY:
            character.role.AdjustJoy(_actionData.providedJoy);
            break;
            case NEEDS.PRESTIGE:
            character.role.AdjustPrestige(_actionData.providedPrestige);
            break;
        }
    }
    #endregion

    #region Advertisement
    public int GetTotalAdvertisementValue(Character character) {
        return GetFoodAdvertisementValue(character) + GetEnergyAdvertisementValue(character) + GetJoyAdvertisementValue(character) + GetPrestigeAdvertisementValue(character);
    }
    private int GetAdvertisementValue(int currentNeed, int advertisedNeed) {
        if(advertisedNeed != 0) {
            float x = (float) currentNeed;
            float y = x + ((float) advertisedNeed * 80f);
            if(y > 1000f) {
                y = 1000f;
            }
            float result = (1000f / x) - (1000f / y);

            //Add quest modifier
            return (int) result;
        }
        return 0;
    }
    private int GetFoodAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.fullness, _actionData.advertisedFullness);
    }
    private int GetEnergyAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.energy, _actionData.advertisedEnergy);
    }
    private int GetJoyAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.joy, _actionData.advertisedJoy);
    }
    private int GetPrestigeAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.prestige, _actionData.advertisedPrestige);
    }
    #endregion

    #region Action Perform

    #endregion
}
