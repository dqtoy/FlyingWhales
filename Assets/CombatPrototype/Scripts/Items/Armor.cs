using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Armor : Item {
    [SerializeField] private ARMOR_TYPE _armorType;
    [SerializeField] private int _def;
    [SerializeField] private ARMOR_PREFIX _prefix;
    [SerializeField] private ARMOR_SUFFIX _suffix;
    //public MATERIAL material;
    //public QUALITY quality;
    //public float baseDamageMitigation;
    //public float damageNullificationChance;
    //public List<ATTACK_TYPE> ineffectiveAttackTypes;
    //public List<ATTACK_TYPE> effectiveAttackTypes;
    //public List<IBodyPart.ATTRIBUTE> attributes;

    #region getters/setters
    public ARMOR_TYPE armorType {
        get { return _armorType; }
        set { _armorType = value; }
    }
    public ARMOR_PREFIX prefixType {
        get { return _prefix; }
    }
    public ARMOR_SUFFIX suffixType {
        get { return _suffix; }
    }
    public int def {
        get { return _def; }
        set { _def = value; }
    }
    public ArmorPrefix prefix {
        get {
            if (ItemManager.Instance != null) {
                return ItemManager.Instance.armorPrefixes[_prefix];
            }
            return CombatSimManager.Instance.armorPrefixes[_prefix];
        }
    }
    public ArmorSuffix suffix {
        get {
            if (ItemManager.Instance != null) {
                return ItemManager.Instance.armorSuffixes[_suffix];
            }
            return CombatSimManager.Instance.armorSuffixes[_suffix];
        }
    }
    #endregion

    #region overrides
    public override Item CreateNewCopy() {
        if (isStackable) {
            return ItemManager.Instance.allWeapons[itemName];
        }
        Armor copy = new Armor();
        copy.armorType = armorType;
        copy._def = _def;
        copy._prefix = _prefix;
        copy._suffix = _suffix;
        SetCommonData(copy);
        return copy;
    }
    #endregion

    public void SetPrefix(ARMOR_PREFIX prefix) {
        _prefix = prefix;
    }
    public void SetSuffix(ARMOR_SUFFIX suffix) {
        _suffix = suffix;
    }
}