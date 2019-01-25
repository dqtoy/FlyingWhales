using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : StructureObj {

    private List<string> _availableItems = new List<string>() {
        "Short Sword"
    };

    #region getters/setters
    public List<string> availableItems {
        get { return _availableItems; }
    }
    #endregion

    public Shop() : base() {
        _specificObjectType = LANDMARK_TYPE.SHOP;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Shop clone = new Shop();
        SetCommonData(clone);
        return clone;
    }
    #endregion

    public void AddItemToShop(string item) {
        if (!_availableItems.Contains(item)) {
            _availableItems.Add(item);
        }
    }
    public void RemoveItemFromShop(string item) {
        _availableItems.Remove(item);
    }
}
