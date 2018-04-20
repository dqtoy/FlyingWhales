using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class CharacterAction {
    protected ACTION_TYPE _actionType;
    protected string _actionName;
    protected ObjectState _state;
    protected ActionFilter[] _filters;
    protected float _foodAdvertisementMod;
    protected float _energyAdvertisementMod;
    protected float _joyAdvertisementMod;
    protected float _prestigeAdvertisementMod;

    #region getters/setters
    public ACTION_TYPE actionType {
        get { return _actionType; }
    }
    public ObjectState state {
        get { return _state; }
    }
    #endregion

    public CharacterAction(ObjectState state) {
        _state = state;
    }

    #region Virtuals
    public virtual void PerformAction(Character character) {
    }
    #endregion

    private int GetFoodAdvertisementValue(Character character) {
        int baseValue = (int)(((float)_state.obj.foodAdvertisementValue * _state.foodAdvertisementMod) * _foodAdvertisementMod);
        //TODO: Add quest advertisement value
        //TODO: Add trait/tag advertisement value
        int finalValue = baseValue;
        return finalValue;
    }
    private int GetEnergyAdvertisementValue(Character character) {
        int baseValue = (int) (((float) _state.obj.energyAdvertisementValue * _state.energyAdvertisementMod) * _energyAdvertisementMod);
        //TODO: Add quest advertisement value
        //TODO: Add trait/tag advertisement value
        int finalValue = baseValue;
        return finalValue;
    }
    private int GetJoyAdvertisementValue(Character character) {
        int baseValue = (int) (((float) _state.obj.joyAdvertisementValue * _state.joyAdvertisementMod) * _joyAdvertisementMod);
        //TODO: Add quest advertisement value
        //TODO: Add trait/tag advertisement value
        int finalValue = baseValue;
        return finalValue;
    }
    private int GetPrestigeAdvertisementValue(Character character) {
        int baseValue = (int) (((float) _state.obj.prestigeAdvertisementValue * _state.prestigeAdvertisementMod) * _prestigeAdvertisementMod);
        //TODO: Add quest advertisement value
        //TODO: Add trait/tag advertisement value
        int finalValue = baseValue;
        return finalValue;
    }

    public int GetTotalAdvertisementValue(Character character) {
        return GetFoodAdvertisementValue(character) + GetEnergyAdvertisementValue(character) + GetJoyAdvertisementValue(character) + GetPrestigeAdvertisementValue(character);
    }

    public Reward GetReward() {
        return _state.actionRewardDictionary[this];
    }
}
