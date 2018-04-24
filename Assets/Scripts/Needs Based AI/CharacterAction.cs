using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

[System.Serializable]
public class CharacterAction {
    protected ObjectState _state;
    protected ActionFilter[] _filters;
    public CharacterActionData actionData;

    #region getters/setters
    public ACTION_TYPE actionType {
        get { return actionData.actionType; }
    }
    public ObjectState state {
        get { return _state; }
    }
    #endregion

    public CharacterAction(ObjectState state, ACTION_TYPE actionType) {
        _state = state;
        actionData.actionType = actionType;
        actionData.actionName = Utilities.NormalizeStringUpperCaseFirstLetters(actionType.ToString());
    }

    #region Virtuals
    public virtual void PerformAction(Character character) {

    }
    public virtual void ActionSuccess() {
        if (actionData.successFunction != null) {
            actionData.successFunction.Invoke();
        }
    }
    public virtual void ActionFail() {
        if (actionData.failFunction != null) {
            actionData.failFunction.Invoke();
        }
    }
    #endregion

    #region Utilities
    public void SetObjectState(ObjectState state) {
        _state = state;
    }
    public void GenerateName() {
        actionData.actionName = Utilities.NormalizeStringUpperCaseFirstLetters(actionType.ToString());
    }
    public void EndAction(Character character) {
        character.actionData.EndAction();
    }
    public void GiveReward(NEEDS need, Character character) {
        switch (need) {
            case NEEDS.FULLNESS:
            character.role.AdjustFullness(actionData.providedFullness);
            break;
            case NEEDS.ENERGY:
            character.role.AdjustEnergy(actionData.providedEnergy);
            break;
            case NEEDS.JOY:
            character.role.AdjustJoy(actionData.providedJoy);
            break;
            case NEEDS.PRESTIGE:
            character.role.AdjustPrestige(actionData.providedPrestige);
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
        return GetAdvertisementValue(character.role.fullness, actionData.advertisedFullness);
    }
    private int GetEnergyAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.energy, actionData.advertisedEnergy);
    }
    private int GetJoyAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.joy, actionData.advertisedJoy);
    }
    private int GetPrestigeAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.prestige, actionData.advertisedPrestige);
    }
    #endregion

    #region Action Perform

    #endregion
}
