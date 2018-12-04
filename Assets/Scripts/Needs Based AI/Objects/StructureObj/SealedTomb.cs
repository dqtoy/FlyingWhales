using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SealedTomb : StructureObj {
    private int _enchantmentValue;
    private Character _content;

    #region getters/setters
    public Character content {
        get { return _content; }
    }
    public int enchantmentValue {
        get { return _enchantmentValue; }
    }
    #endregion

    public SealedTomb() : base() {
        //_specificObjectType = SPECIFIC_OBJECT_TYPE.SEALED_TOMB;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        SetEnchantmentValue(100);
        //Create Character
    }

    #region Overrides
    public override IObject Clone() {
        SealedTomb clone = new SealedTomb();
        SetCommonData(clone);
        return clone;
    }
    public override void StartState(ObjectState state) {
        base.StartState(state);
        if(state.stateName == "Ruined") {
            SetEnchantmentValue(0);
        }
    }
    #endregion

    #region Unique Data
    public void SetEnchantmentValue(int amount) {
        _enchantmentValue = amount;
    }
    public void AdjustEnchantmentValue(int amount) {
        int previousValue = _enchantmentValue;
        _enchantmentValue += amount;
        _enchantmentValue = Mathf.Clamp(_enchantmentValue, 0, 100);
        if (previousValue != _enchantmentValue) {
            if (_enchantmentValue == 0 && _currentState.stateName == "Sealed") {
                ObjectState unsealedState = GetState("Unsealed");
                ChangeState(unsealedState);
            } else if (_enchantmentValue == 100 && _currentState.stateName == "Unsealed") {
                ObjectState sealedState = GetState("Sealed");
                ChangeState(sealedState);
            }
        }
    }
    public void ReleaseContent() {
        Debug.Log("RELEASED " + _content.name + " from SEALED TOMB in " + _objectLocation.landmarkName);
        //_objectLocation.AddCharacterToLocation(_content);
        //_content.SetIsIdle(false);

        ObjectState emptyState = GetState("Empty");
        ChangeState(emptyState);

        _content = null;
    }
    public void AddContent(Character character) {
        _content = character;
        //_content.SetIsIdle(true);
        ObjectState unsealedState = GetState("Unsealed");
        ChangeState(unsealedState);
    }
    #endregion
}
