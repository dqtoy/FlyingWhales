using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorSuffix {
    protected ARMOR_SUFFIX _armorSuffix;
    protected string _name;
    protected float _bonusDefPercent;

    #region getters/setters
    public ARMOR_SUFFIX armorSuffix {
        get { return _armorSuffix; }
    }
    public string name {
        get { return _name; }
    }
    public float bonusDefPercent {
        get { return _bonusDefPercent; }
    }
    #endregion

    public ArmorSuffix(ARMOR_SUFFIX type) {
        _armorSuffix = type;
        _name = Utilities.NormalizeStringUpperCaseFirstLetters(_armorSuffix.ToString());
    }
}
