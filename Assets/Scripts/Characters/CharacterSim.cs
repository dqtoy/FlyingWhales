using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Traits;

public class CharacterSim : ICharacterSim {
    [SerializeField] private string _name;
    [SerializeField] private string _className;
    [SerializeField] private string _raceName;
    [SerializeField] private string _weaponName;
    [SerializeField] private string _armorName;
    [SerializeField] private string _accessoryName;
    [SerializeField] private string _consumableName;
    [SerializeField] private string _skillName;
    [SerializeField] private int _level;
    [SerializeField] private int _armyCount;
    [SerializeField] private bool _isArmy;
    [SerializeField] private GENDER _gender;

    //[SerializeField] private int _strBuild;
    //[SerializeField] private int _intBuild;
    //[SerializeField] private int _agiBuild;
    //[SerializeField] private int _vitBuild;
    //[SerializeField] private int _str;
    //[SerializeField] private int _int;
    //[SerializeField] private int _agi;
    //[SerializeField] private int _vit;
    //[SerializeField] private int _defHead;
    //[SerializeField] private int _defBody;
    //[SerializeField] private int _defLegs;
    //[SerializeField] private int _defHands;
    //[SerializeField] private int _defFeet;

    private int _id;
    private int _currentHP;
    //private int _currentSP;
    private int _currentRow;
    private int _hp;
    //private int _maxSP;
    private int _attackPower;
    private int _speed;
    private int _army;
    private float _actRate;
    //private bool _isDead;
    private SIDES _currentSide;
    private RaceSetting _raceSetting;
    private CharacterClass _characterClass;
    private CharacterBattleTracker _battleTracker;
    private CharacterBattleOnlyTracker _battleOnlyTracker;
    private List<Trait> _combatAttributes;
    //private List<CharacterAttribute> _attributes;
    //private List<BodyPart> _bodyParts;
    //private List<Item> _equippedItems;
    private Dictionary<ELEMENT, float> _elementalWeaknesses;
    private Dictionary<ELEMENT, float> _elementalResistances;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public string idName {
        get { return "[" + _id + "]" + this._name; }
    }
    public string className {
        get { return _className; }
    }
    public string raceName {
        get { return _raceName; }
    }
    public string weaponName {
        get { return _weaponName; }
    }
    public string armorName {
        get { return _armorName; }
    }
    public string accessoryName {
        get { return _accessoryName; }
    }
    public string consumableName {
        get { return _consumableName; }
    }
    public int id {
        get { return _id; }
    }
    public int level {
        get { return _level; }
    }
    //public int maxSP {
    //    get { return _maxSP; }
    //}
    //public int defHead {
    //    get { return _defHead; }
    //}
    //public int defBody {
    //    get { return _defBody; }
    //}
    //public int defLegs {
    //    get { return _defLegs; }
    //}
    //public int defHands {
    //    get { return _defHands; }
    //}
    //public int defFeet {
    //    get { return _defFeet; }
    //}
    public int currentRow {
        get { return _currentRow; }
    }
    public float actRate {
        get { return _actRate; }
        set { _actRate = value; }
    }
    public int attackPower {
        get { return _attackPower; }
    }
    public int speed {
        get { return _speed; }
    }
    public int maxHP {
        get { return _hp; }
    }
    public int currentHP {
        get { return _currentHP; }
    }
    //public int currentSP {
    //    get { return _currentSP; }
    //}
    public int armyCount {
        get { return _army; }
    }
    public bool isArmy {
        get { return _isArmy; }
    }
    public SIDES currentSide {
        get { return _currentSide; }
    }
    public ICHARACTER_TYPE icharacterType {
        get { return ICHARACTER_TYPE.CHARACTER; }
    }
    public GENDER gender {
        get { return _gender; }
    }
    public RACE race {
        get { return _raceSetting.race; }
    }
    public CharacterClass characterClass {
        get { return _characterClass; }
    }
    public CharacterBattleOnlyTracker battleOnlyTracker {
        get { return _battleOnlyTracker; }
    }
    public CharacterBattleTracker battleTracker {
        get { return _battleTracker; }
    }
    public string skillName {
        get { return _skillName; }
    }
    public List<Trait> normalTraits {
        get { return _combatAttributes; }
    }
    public Dictionary<ELEMENT, float> elementalWeaknesses {
        get { return _elementalWeaknesses; }
    }
    public Dictionary<ELEMENT, float> elementalResistances {
        get { return _elementalResistances; }
    }
    #endregion

    public void InitializeSim() {
        _id = Ruinarch.Utilities.SetID(this);
        ConstructClass();
        _raceSetting = JsonUtility.FromJson<RaceSetting>(System.IO.File.ReadAllText(Ruinarch.Utilities.dataPath + "RaceSettings/" + _raceName + ".json"));
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        _battleTracker = new CharacterBattleTracker();
        _elementalWeaknesses = new Dictionary<ELEMENT, float>(CombatSimManager.Instance.elementsChanceDictionary);
        _elementalResistances = new Dictionary<ELEMENT, float>(CombatSimManager.Instance.elementsChanceDictionary);
        //_attributes = new List<CharacterAttribute>();
        _combatAttributes = new List<Trait>();
        _army = _armyCount;
        AllocateStats();
        LevelUp();
        //ArmyModifier();
        ResetToFullHP();
        ResetToFullSP();
    }
    public void SetDataFromCharacterPanelUI() {
        _name = CharacterPanelUI.Instance.nameInput.text;
        _className = CharacterPanelUI.Instance.classOptions.options[CharacterPanelUI.Instance.classOptions.value].text;
        _raceName = CharacterPanelUI.Instance.raceOptions.options[CharacterPanelUI.Instance.raceOptions.value].text;
        //_weaponName = CharacterPanelUI.Instance.weaponName;
        //_armorName = CharacterPanelUI.Instance.armorName;
        //_accessoryName = CharacterPanelUI.Instance.accessoryName;
        //_consumableName = CharacterPanelUI.Instance.consumableOptions.options[CharacterPanelUI.Instance.consumableOptions.value].text;

        _gender = (GENDER) System.Enum.Parse(typeof(GENDER), CharacterPanelUI.Instance.genderOptions.options[CharacterPanelUI.Instance.genderOptions.value].text);
        _level = int.Parse(CharacterPanelUI.Instance.levelInput.text);
        //_skillName = CharacterPanelUI.Instance.skillName;

        _isArmy = CharacterPanelUI.Instance.toggleArmy.isOn;
        _armyCount = int.Parse(CharacterPanelUI.Instance.armyInput.text);

        //_defHead = int.Parse(CharacterPanelUI.Instance.dHeadInput.text);
        //_defBody = int.Parse(CharacterPanelUI.Instance.dBodyInput.text);
        //_defLegs = int.Parse(CharacterPanelUI.Instance.dLegsInput.text);
        //_defHands = int.Parse(CharacterPanelUI.Instance.dHandsInput.text);
        //_defFeet = int.Parse(CharacterPanelUI.Instance.dFeetInput.text);
    }

    #region Interface
    public void SetSide(SIDES side) {
        _currentSide = side;
    }
    public void SetRowNumber(int row) {
        _currentRow = row;
    }
    public void ResetToFullHP() {
        _currentHP = _hp * _armyCount;
        //_isDead = false;
    }
    public void ResetToFullSP() {
        //AdjustSP(_maxSP);
    }
    public void AdjustHP(int amount, Character killer = null) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, maxHP);
        if (isArmy) {
            int diff = maxHP - _currentHP;
            if (diff > 0) {
                int armyLoss = diff / _hp;
                AdjustArmyCount(-armyLoss);
            }
        }
        if (previous != this._currentHP) {
            if (this._currentHP == 0) {
                DeathSim();
            }
        }
    }
    public void AdjustSP(int amount) {
        //_currentSP += amount;
        //_currentSP = Mathf.Clamp(_currentSP, 0, _maxSP);
    }
    public void AdjustArmyCount(int adjustment) {
        _army += adjustment;
        _army = Mathf.Clamp(_army, 0, _armyCount);
    }
    public void DeathSim() {
        //_isDead = true;
        CombatSimManager.Instance.currentCombat.CharacterDeath(this);
    }
    #endregion

    #region Utilities
    private void ConstructClass() {
        string path = Ruinarch.Utilities.dataPath + "CharacterClasses/" + _className + ".json";
        _characterClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(path));
    }
    private void AllocateStats() {
        //_attackPower = _raceSetting.attackPowerModifier;
        //_speed = _raceSetting.speedModifier;
        //_hp = _raceSetting.hpModifier;
        //_sp = characterClass.baseSP;
    }
    private void LevelUp() {
        int multiplier = _level - 1;
        if (multiplier < 0) {
            multiplier = 0;
        }
        _attackPower += (multiplier * (int) ((characterClass.attackPowerPerLevel / 100f) * (float) _raceSetting.attackPowerModifier));
        _speed += (multiplier * (int) ((characterClass.speedPerLevel / 100f) * (float) _raceSetting.speedModifier));
        _hp += (multiplier * (int) ((characterClass.hpPerLevel / 100f) * (float) _raceSetting.hpModifier));
        //_sp += ((int)multiplier * characterClass.spPerLevel);

        ////Add stats per level from race
        //if (level > 1) {
        //    if(_raceSetting.hpPerLevel.Length > 0) {
        //        int hpIndex = level % _raceSetting.hpPerLevel.Length;
        //        hpIndex = hpIndex == 0 ? _raceSetting.hpPerLevel.Length : hpIndex;
        //        _hp += _raceSetting.hpPerLevel[hpIndex - 1];
        //    }
        //    if (_raceSetting.attackPerLevel.Length > 0) {
        //        int attackIndex = level % _raceSetting.attackPerLevel.Length;
        //        attackIndex = attackIndex == 0 ? _raceSetting.attackPerLevel.Length : attackIndex;
        //        _attackPower += _raceSetting.attackPerLevel[attackIndex - 1];
        //    }
        //}
    }
    //private void ArmyModifier() {
    //    if (_isArmy) {
    //        _attackPower = _attackPower * _armyCount;
    //        _speed = _speed * _armyCount;
    //        _maxHP = _hp * _armyCount;
    //    } else {
    //        _attackPower = _attackPower;
    //        _speed = _speed;
    //        _maxHP = _hp;
    //    }
    //    _currentHP = _maxHP;
    //}
    #endregion

    #region Attributes
    //public CharacterAttribute GetAttribute(string attribute) {
    //    for (int i = 0; i < _attributes.Count; i++) {
    //        if (_attributes[i].name.ToLower() == attribute.ToLower()) {
    //            return _attributes[i];
    //        }
    //    }
    //    return null;
    //}
    public void AddCombatAttribute(Trait combatAttribute) {
        if (string.IsNullOrEmpty(GetCombatAttribute(combatAttribute.name).name)) {
            _combatAttributes.Add(combatAttribute);
            ApplyFlatCombatAttributeEffects(combatAttribute);
        }
    }
    public bool RemoveCombatAttribute(Trait combatAttribute) {
        for (int i = 0; i < _combatAttributes.Count; i++) {
            if (_combatAttributes[i].name == combatAttribute.name) {
                _combatAttributes.RemoveAt(i);
                UnapplyFlatCombatAttributeEffects(combatAttribute);
                return true;
            }
        }
        return false;
    }
    public Trait GetCombatAttribute(string attributeName) {
        for (int i = 0; i < _combatAttributes.Count; i++) {
            if (_combatAttributes[i].name == attributeName) {
                return _combatAttributes[i];
            }
        }
        return new Trait();
    }
    private void ApplyFlatCombatAttributeEffects(Trait trait) {
        if (trait.effects != null) {
            for (int i = 0; i < trait.effects.Count; i++) {
                TraitEffect traitEffect = trait.effects[i];
                if (!traitEffect.hasRequirement && !traitEffect.isPercentage) {
                    if (traitEffect.stat == STAT.ATTACK) {
                        _attackPower += (int) traitEffect.amount;
                    } else if (traitEffect.stat == STAT.HP) {
                        _hp += (int) traitEffect.amount;
                    } else if (traitEffect.stat == STAT.SPEED) {
                        _speed += (int) traitEffect.amount;
                    }
                }
            }
        }
    }
    private void UnapplyFlatCombatAttributeEffects(Trait trait) {
        if (trait.effects != null) {
            for (int i = 0; i < trait.effects.Count; i++) {
                TraitEffect traitEffect = trait.effects[i];
                if (!traitEffect.hasRequirement && !traitEffect.isPercentage) {
                    if (traitEffect.stat == STAT.ATTACK) {
                        _attackPower -= (int) traitEffect.amount;
                    } else if (traitEffect.stat == STAT.HP) {
                        _hp -= (int) traitEffect.amount;
                    } else if (traitEffect.stat == STAT.SPEED) {
                        _speed -= (int) traitEffect.amount;
                    }
                }
            }
        }
    }
    #endregion
}
