using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSuffix {
    protected WEAPON_SUFFIX _weaponSuffix;
    protected string _name;
    protected float _flatModifier;
    protected float _percentModifier;

    #region getters/setters
    public WEAPON_SUFFIX weaponSuffix {
        get { return _weaponSuffix; }
    }
    public string name {
        get { return _name; }
    }
    public float flatModifier {
        get { return _flatModifier; }
    }
    public float percentModifier {
        get { return _percentModifier; }
    }
    #endregion

    public WeaponSuffix(WEAPON_SUFFIX type) {
        _weaponSuffix = type;
        _name = Utilities.NormalizeStringUpperCaseFirstLetters(_weaponSuffix.ToString());
    }
}
