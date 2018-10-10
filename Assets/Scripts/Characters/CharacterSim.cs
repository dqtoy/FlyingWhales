using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System.IO;

public class CharacterSim : ICharacterSim {
    [SerializeField] private string _name;
    [SerializeField] private string _className;
    [SerializeField] private string _weaponName;
    [SerializeField] private string _armorName;
    [SerializeField] private string _accessoryName;
    [SerializeField] private string _consumableName;
    [SerializeField] private int _level;
    //[SerializeField] private int _strBuild;
    //[SerializeField] private int _intBuild;
    //[SerializeField] private int _agiBuild;
    //[SerializeField] private int _vitBuild;
    //[SerializeField] private int _str;
    //[SerializeField] private int _int;
    //[SerializeField] private int _agi;
    //[SerializeField] private int _vit;
    [SerializeField] private int _maxHP;
    [SerializeField] private int _maxSP;
    //[SerializeField] private int _defHead;
    //[SerializeField] private int _defBody;
    //[SerializeField] private int _defLegs;
    //[SerializeField] private int _defHands;
    //[SerializeField] private int _defFeet;
    [SerializeField] private GENDER _gender;
    [SerializeField] private List<string> _skillNames;

    private int _id;
    private int _currentHP;
    private int _currentSP;
    private int _currentRow;
    private float _actRate;
    private float _attackPower;
    private float _speed;
    private bool _isDead;
    private SIDES _currentSide;
    private RaceSetting _raceSetting;
    private CharacterClass _characterClass;
    private CharacterBattleTracker _battleTracker;
    private CharacterBattleOnlyTracker _battleOnlyTracker;
    private Weapon _equippedWeapon;
    private Armor _equippedArmor;
    private List<Skill> _skills;
    private List<CombatAttribute> _combatAttributes;
    private List<Attribute> _attributes;
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
    public int maxHP {
        get { return _maxHP; }
    }
    public int maxSP {
        get { return _maxSP; }
    }
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
    public float speed {
        get { return _speed; }
    }
    public float attackPower {
        get { return _attackPower; }
    }
    public int currentHP {
        get { return _currentHP; }
    }
    public int currentSP {
        get { return _currentSP; }
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
    public CharacterClass characterClass {
        get { return _characterClass; }
    }
    public CharacterBattleOnlyTracker battleOnlyTracker {
        get { return _battleOnlyTracker; }
    }
    public CharacterBattleTracker battleTracker {
        get { return _battleTracker; }
    }
    public Weapon equippedWeapon {
        get { return _equippedWeapon; }
    }
    public List<string> skillNames {
        get { return _skillNames; }
    }
    public List<Skill> skills {
        get { return _skills; }
    }
    public List<CombatAttribute> combatAttributes {
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
        _id = Utilities.SetID(this);
        ConstructClass();
        ConstructSkills();
        ResetToFullHP();
        ResetToFullSP();
        _raceSetting = JsonUtility.FromJson<RaceSetting>(System.IO.File.ReadAllText(Utilities.dataPath + "RaceSettings/HUMANS.json"));
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        _battleTracker = new CharacterBattleTracker();
        _elementalWeaknesses = new Dictionary<ELEMENT, float>(CombatSimManager.Instance.elementsChanceDictionary);
        _elementalResistances = new Dictionary<ELEMENT, float>(CombatSimManager.Instance.elementsChanceDictionary);
        _attributes = new List<Attribute>();
        _combatAttributes = new List<CombatAttribute>();
        EquipWeaponArmors();
    }
    public void SetDataFromCharacterPanelUI() {
        _name = CharacterPanelUI.Instance.nameInput.text;
        _className = CharacterPanelUI.Instance.classOptions.options[CharacterPanelUI.Instance.classOptions.value].text;
        _weaponName = CharacterPanelUI.Instance.weaponOptions.options[CharacterPanelUI.Instance.weaponOptions.value].text;
        _armorName = CharacterPanelUI.Instance.armorOptions.options[CharacterPanelUI.Instance.armorOptions.value].text;
        _accessoryName = CharacterPanelUI.Instance.accessoryOptions.options[CharacterPanelUI.Instance.accessoryOptions.value].text;
        _consumableName = CharacterPanelUI.Instance.consumableOptions.options[CharacterPanelUI.Instance.consumableOptions.value].text;

        _gender = (GENDER) System.Enum.Parse(typeof(GENDER), CharacterPanelUI.Instance.genderOptions.options[CharacterPanelUI.Instance.genderOptions.value].text);
        _level = int.Parse(CharacterPanelUI.Instance.levelInput.text);
        _maxHP = CharacterPanelUI.Instance.hp;
        _maxSP = CharacterPanelUI.Instance.sp;
        _attackPower = CharacterPanelUI.Instance.attackPower;
        _speed = CharacterPanelUI.Instance.speed;

        //_defHead = int.Parse(CharacterPanelUI.Instance.dHeadInput.text);
        //_defBody = int.Parse(CharacterPanelUI.Instance.dBodyInput.text);
        //_defLegs = int.Parse(CharacterPanelUI.Instance.dLegsInput.text);
        //_defHands = int.Parse(CharacterPanelUI.Instance.dHandsInput.text);
        //_defFeet = int.Parse(CharacterPanelUI.Instance.dFeetInput.text);

        _skillNames = CharacterPanelUI.Instance.skillNames;
    }
    private void EquipWeaponArmors() {
        if(_weaponName != string.Empty) {
            Weapon weapon = JsonUtility.FromJson<Weapon>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/WEAPON/" + _weaponName + ".json"));
            EquipItem(weapon);
        }
        if (_armorName != string.Empty) {
            Armor armor = JsonUtility.FromJson<Armor>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/ARMOR/" + _armorName + ".json"));
            EquipItem(armor);
        }
    }


    #region Interface
    public void SetSide(SIDES side) {
        _currentSide = side;
    }
    public void SetRowNumber(int row) {
        _currentRow = row;
    }
    public void ResetToFullHP() {
        AdjustHP(_maxHP);
        _isDead = false;
    }
    public void ResetToFullSP() {
        AdjustSP(_maxSP);
    }
    public void AdjustHP(int amount, ICharacter killer = null) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, _maxHP);
        if (previous != this._currentHP) {
            if (this._currentHP == 0) {
                DeathSim();
            }
        }
    }
    public void AdjustSP(int amount) {
        _currentSP += amount;
        _currentSP = Mathf.Clamp(_currentSP, 0, _maxSP);
    }
    public void DeathSim() {
        _isDead = true;
        CombatSimManager.Instance.currentCombat.CharacterDeath(this);
    }
    public void EnableDisableSkills(CombatSim combatSim) {
        //Body part skills / general skills
        for (int i = 0; i < this._skills.Count; i++) {
            Skill skill = this._skills[i];
            skill.isEnabled = true;

            if (skill is AttackSkill) {
                AttackSkill attackSkill = skill as AttackSkill;
                if (attackSkill.spCost > _currentSP) {
                    skill.isEnabled = false;
                    continue;
                }
            } else if (skill is FleeSkill) {
                if (this.currentHP >= (this.maxHP / 2)) {
                    skill.isEnabled = false;
                    continue;
                }
            } 
        }
    }
    #endregion

    #region Utilities
    private void ConstructClass() {
        string path = Utilities.dataPath + "CharacterClasses/" + _className + ".json";
        _characterClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(path));
    }
    private void ConstructSkills() {
        _skills = new List<Skill>();
        string path = string.Empty;
        path = Utilities.dataPath + "Skills/GENERAL/";
        string[] directories = Directory.GetDirectories(path);
        for (int i = 0; i < directories.Length; i++) {
            string skillType = new DirectoryInfo(directories[i]).Name;
            SKILL_TYPE currSkillType = (SKILL_TYPE) System.Enum.Parse(typeof(SKILL_TYPE), skillType);
            string[] files = Directory.GetFiles(directories[i], "*.json");
            for (int j = 0; j < files.Length; j++) {
                string dataAsJson = File.ReadAllText(files[j]);
                switch (currSkillType) {
                    case SKILL_TYPE.ATTACK:
                    AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill>(dataAsJson);
                    _skills.Add(attackSkill);
                    break;
                    case SKILL_TYPE.HEAL:
                    HealSkill healSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
                    _skills.Add(healSkill);
                    break;
                    case SKILL_TYPE.OBTAIN_ITEM:
                    ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
                    _skills.Add(obtainSkill);
                    break;
                    case SKILL_TYPE.FLEE:
                    break;
                    case SKILL_TYPE.MOVE:
                    MoveSkill moveSkill = JsonUtility.FromJson<MoveSkill>(dataAsJson);
                    _skills.Add(moveSkill);
                    break;
                }
            }
        }
        for (int i = 0; i < _skillNames.Count; i++) {
            path = Utilities.dataPath + "Skills/CLASS/ATTACK/" + _skillNames[i] + ".json";
            AttackSkill skill = JsonUtility.FromJson<AttackSkill>(System.IO.File.ReadAllText(path));
            _skills.Add(skill);
        }
    }
    #endregion

    #region Equipment
    public bool EquipItem(Item item) {
        bool hasEquipped = false;
        if (item is Weapon) {
            Weapon weapon = (Weapon) item;
            hasEquipped = TryEquipWeapon(weapon);
        } else if (item is Armor) {
            Armor armor = (Armor) item;
            hasEquipped = TryEquipArmor(armor);
        }
        return hasEquipped;
    }
    //Unequips an item of a character, whether it's a weapon, armor, etc.
    public void UnequipItem(Item item) {
        if (item is Weapon) {
            UnequipWeapon((Weapon) item);
        } else if (item is Armor) {
            UnequipArmor((Armor) item);
        }
    }
    public bool TryEquipWeapon(Weapon weapon) {
        if (!_characterClass.allowedWeaponTypes.Contains(weapon.weaponType)) {
            return false;
        }
        _equippedWeapon = weapon;
        weapon.SetEquipped(true);
        return true;
    }
    public bool TryEquipArmor(Armor armor) {
        _equippedArmor = armor;
        armor.SetEquipped(true);
        return true;
    }
    private void UnequipWeapon(Weapon weapon) {
        _equippedWeapon = null;
        weapon.SetEquipped(false);
    }
    private void UnequipArmor(Armor armor) {
        _equippedArmor = null;
        armor.SetEquipped(false);
    }
    #endregion

    #region Attributes
    public Attribute GetAttribute(string attribute) {
        for (int i = 0; i < _attributes.Count; i++) {
            if (_attributes[i].name.ToLower() == attribute.ToLower()) {
                return _attributes[i];
            }
        }
        return null;
    }
    #endregion
}
