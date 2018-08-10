using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System.IO;

public class CharacterSim : ICharacterSim {
    [SerializeField] private string _name;
    [SerializeField] private string _className;
    [SerializeField] private string _weaponName;
    [SerializeField] private string _headArmorName;
    [SerializeField] private string _bodyArmorName;
    [SerializeField] private string _legsArmorName;
    [SerializeField] private string _leftHandArmorName;
    [SerializeField] private string _rightHandArmorName;
    [SerializeField] private string _leftFootArmorName;
    [SerializeField] private string _rightFootArmorName;
    [SerializeField] private int _level;
    [SerializeField] private int _strBuild;
    [SerializeField] private int _intBuild;
    [SerializeField] private int _agiBuild;
    [SerializeField] private int _vitBuild;
    [SerializeField] private int _str;
    [SerializeField] private int _int;
    [SerializeField] private int _agi;
    [SerializeField] private int _vit;
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
    private int _actRate;
    private float _critChance;
    private float _critDamage;
    private float _bonusDef;
    private float _bonusDefPercent;
    private bool _isDead;
    private SIDES _currentSide;
    private RaceSetting _raceSetting;
    private CharacterClass _characterClass;
    private CharacterBattleTracker _battleTracker;
    private CharacterBattleOnlyTracker _battleOnlyTracker;
    private Weapon _equippedWeapon;
    private List<Skill> _skills;
    private List<BodyPart> _bodyParts;
    private List<Item> _equippedItems;
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
    public string headArmorName {
        get { return _headArmorName; }
    }
    public string bodyArmorName {
        get { return _bodyArmorName; }
    }
    public string legsArmorName {
        get { return _legsArmorName; }
    }
    public string leftHandArmorName {
        get { return _leftHandArmorName; }
    }
    public string rightHandArmorName {
        get { return _rightHandArmorName; }
    }
    public string leftFootArmorName {
        get { return _leftFootArmorName; }
    }
    public string rightFootArmorName {
        get { return _rightFootArmorName; }
    }
    public int id {
        get { return _id; }
    }
    public int level {
        get { return _level; }
    }
    public int strength {
        get { return _str; }
    }
    public int intelligence {
        get { return _int; }
    }
    public int agility {
        get { return _agi; }
    }
    public int vitality {
        get { return _vit; }
    }
    public int maxHP {
        get { return _maxHP; }
    }
    public int maxSP {
        get { return _maxSP; }
    }
    public int strBuild {
        get { return _strBuild; }
    }
    public int intBuild {
        get { return _intBuild; }
    }
    public int agiBuild {
        get { return _agiBuild; }
    }
    public int vitBuild {
        get { return _vitBuild; }
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
    public int actRate {
        get { return _actRate; }
        set { _actRate = value; }
    }
    public int speed {
        get {
            float agi = (float) agility;
            return (int) ((100f * (1f + ((agi / 5f) / 100f))) + (float) level + (agi / 3f)); //TODO: + passive speed bonus
        }
    }
    public int currentHP {
        get { return _currentHP; }
    }
    public int currentSP {
        get { return _currentSP; }
    }
    public int pFinalAttack {
        get {
            float str = (float) strength;
            return (int) (((_equippedWeapon.attackPower + (str / 3f)) * (1f + ((str / 10f) / 100f))) + ((float) level * 4f)); //TODO: + passive bonus attack
        }
    }
    public int mFinalAttack {
        get {
            float intl = (float) intelligence;
            return (int) (((_equippedWeapon.attackPower + (intl / 3f)) * (1f + ((intl / 10f) / 100f))) + ((float) level * 4f)); //TODO: + passive bonus attack
        }
    }
    public float critChance {
        get { return _critChance; }
    }
    public float critDamage {
        get { return _critDamage; }
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
    public List<BodyPart> bodyParts {
        get { return _bodyParts; }
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
        _bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
        _equippedItems = new List<Item>();
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        _battleTracker = new CharacterBattleTracker();
        _elementalWeaknesses = new Dictionary<ELEMENT, float>(CombatSimManager.Instance.elementsChanceDictionary);
        _elementalResistances = new Dictionary<ELEMENT, float>(CombatSimManager.Instance.elementsChanceDictionary);
        EquipWeaponArmors();
    }
    public void SetDataFromCharacterPanelUI() {
        _name = CharacterPanelUI.Instance.nameInput.text;
        _className = CharacterPanelUI.Instance.classOptions.options[CharacterPanelUI.Instance.classOptions.value].text;
        _weaponName = CharacterPanelUI.Instance.weaponOptions.options[CharacterPanelUI.Instance.weaponOptions.value].text;
        _headArmorName = CharacterPanelUI.Instance.headArmorOptions.options[CharacterPanelUI.Instance.headArmorOptions.value].text;
        _bodyArmorName = CharacterPanelUI.Instance.bodyArmorOptions.options[CharacterPanelUI.Instance.bodyArmorOptions.value].text;
        _legsArmorName = CharacterPanelUI.Instance.legsArmorOptions.options[CharacterPanelUI.Instance.legsArmorOptions.value].text;
        _leftHandArmorName = CharacterPanelUI.Instance.leftHandArmorOptions.options[CharacterPanelUI.Instance.leftHandArmorOptions.value].text;
        _rightHandArmorName = CharacterPanelUI.Instance.rightHandArmorOptions.options[CharacterPanelUI.Instance.rightHandArmorOptions.value].text;
        _leftFootArmorName = CharacterPanelUI.Instance.leftFootArmorOptions.options[CharacterPanelUI.Instance.leftFootArmorOptions.value].text;
        _rightFootArmorName = CharacterPanelUI.Instance.rightFootArmorOptions.options[CharacterPanelUI.Instance.rightFootArmorOptions.value].text;

        _gender = (GENDER) System.Enum.Parse(typeof(GENDER), CharacterPanelUI.Instance.genderOptions.options[CharacterPanelUI.Instance.genderOptions.value].text);
        _level = int.Parse(CharacterPanelUI.Instance.levelInput.text);
        _strBuild = CharacterPanelUI.Instance.strBuild;
        _intBuild = CharacterPanelUI.Instance.intBuild;
        _agiBuild = CharacterPanelUI.Instance.agiBuild;
        _vitBuild = CharacterPanelUI.Instance.vitBuild;
        _str = CharacterPanelUI.Instance.str;
        _int = CharacterPanelUI.Instance.intl;
        _agi = CharacterPanelUI.Instance.agi;
        _vit = CharacterPanelUI.Instance.vit;
        _maxHP = CharacterPanelUI.Instance.hp;
        _maxSP = CharacterPanelUI.Instance.sp;

        //_defHead = int.Parse(CharacterPanelUI.Instance.dHeadInput.text);
        //_defBody = int.Parse(CharacterPanelUI.Instance.dBodyInput.text);
        //_defLegs = int.Parse(CharacterPanelUI.Instance.dLegsInput.text);
        //_defHands = int.Parse(CharacterPanelUI.Instance.dHandsInput.text);
        //_defFeet = int.Parse(CharacterPanelUI.Instance.dFeetInput.text);

        _skillNames = CharacterPanelUI.Instance.skillNames;
    }
    private void EquipWeaponArmors() {
        Weapon weapon = JsonUtility.FromJson<Weapon>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/WEAPON/" + _weaponName + ".json"));
        EquipItem(weapon);

        if (_headArmorName != "None") {
            Armor headArmor = JsonUtility.FromJson<Armor>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/ARMOR/" + _headArmorName + ".json"));
            EquipItem(headArmor);
        }
        if (_bodyArmorName != "None") {
            Armor bodyArmor = JsonUtility.FromJson<Armor>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/ARMOR/" + _bodyArmorName + ".json"));
            EquipItem(bodyArmor);
        }
        if (_legsArmorName != "None") {
            Armor legArmor = JsonUtility.FromJson<Armor>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/ARMOR/" + _legsArmorName + ".json"));
            EquipItem(legArmor);
        }
        if (_leftHandArmorName != "None") {
            Armor leftHandArmor = JsonUtility.FromJson<Armor>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/ARMOR/" + _leftHandArmorName + ".json"));
            EquipItem(leftHandArmor);
        }
        if (_rightHandArmorName != "None") {
            Armor rightHandArmor = JsonUtility.FromJson<Armor>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/ARMOR/" + _rightHandArmorName + ".json"));
            EquipItem(rightHandArmor);
        }
        if (_leftFootArmorName != "None") {
            Armor leftFootArmor = JsonUtility.FromJson<Armor>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/ARMOR/" + _leftFootArmorName + ".json"));
            EquipItem(leftFootArmor);
        }
        if (_rightFootArmorName != "None") {
            Armor rightFootArmor = JsonUtility.FromJson<Armor>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/ARMOR/" + _rightFootArmorName + ".json"));
            EquipItem(rightFootArmor);
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
    public int GetDef() {
        float vit = (float) vitality;
        return (int) ((((_bonusDef * (1f + ((vit / 5) / 100f)))) * (1f + (_bonusDefPercent / 100f))) + (vit / 4f)); //TODO: + passive skill def bonus
    }
    public void EnableDisableSkills(CombatSim combatSim) {
        //bool isAllAttacksInRange = true;
        //bool isAttackInRange = false;

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
                //isAttackInRange = combatSim.HasTargetInRangeForSkill(skill, this);
                //if (!isAttackInRange) {
                //    isAllAttacksInRange = false;
                //    skill.isEnabled = false;
                //    continue;
                //}
            } else if (skill is FleeSkill) {
                if (this.currentHP >= (this.maxHP / 2)) {
                    skill.isEnabled = false;
                    continue;
                }
            } 
            //else if (skill is MoveSkill) {
            //    skill.isEnabled = false;
            //    continue;
            //}
        }

        //for (int i = 0; i < this._skills.Count; i++) {
        //    Skill skill = this._skills[i];
        //    if (skill is MoveSkill) {
        //        skill.isEnabled = true;
        //        if (isAllAttacksInRange) {
        //            skill.isEnabled = false;
        //            continue;
        //        }
        //        if (skill.skillName == "MoveLeft") {
        //            if (this._currentRow == 1) {
        //                skill.isEnabled = false;
        //                continue;
        //            } else {
        //                bool hasEnemyOnLeft = false;
        //                if (combatSim.charactersSideA.Contains(this)) {
        //                    for (int j = 0; j < combatSim.charactersSideB.Count; j++) {
        //                        ICharacterSim enemy = combatSim.charactersSideB[j];
        //                        if (enemy.currentRow < this._currentRow) {
        //                            hasEnemyOnLeft = true;
        //                            break;
        //                        }
        //                    }
        //                } else {
        //                    for (int j = 0; j < combatSim.charactersSideA.Count; j++) {
        //                        ICharacterSim enemy = combatSim.charactersSideA[j];
        //                        if (enemy.currentRow < this._currentRow) {
        //                            hasEnemyOnLeft = true;
        //                            break;
        //                        }
        //                    }
        //                }
        //                if (!hasEnemyOnLeft) {
        //                    skill.isEnabled = false;
        //                    continue;
        //                }
        //            }
        //        } else if (skill.skillName == "MoveRight") {
        //            if (this._currentRow == 5) {
        //                skill.isEnabled = false;
        //            } else {
        //                bool hasEnemyOnRight = false;
        //                if (combatSim.charactersSideA.Contains(this)) {
        //                    for (int j = 0; j < combatSim.charactersSideB.Count; j++) {
        //                        ICharacterSim enemy = combatSim.charactersSideB[j];
        //                        if (enemy.currentRow > this._currentRow) {
        //                            hasEnemyOnRight = true;
        //                            break;
        //                        }
        //                    }
        //                } else {
        //                    for (int j = 0; j < combatSim.charactersSideA.Count; j++) {
        //                        ICharacterSim enemy = combatSim.charactersSideA[j];
        //                        if (enemy.currentRow > this._currentRow) {
        //                            hasEnemyOnRight = true;
        //                            break;
        //                        }
        //                    }
        //                }
        //                if (!hasEnemyOnRight) {
        //                    skill.isEnabled = false;
        //                    continue;
        //                }
        //            }
        //        }
        //    }
        //}
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
        RemoveEquippedItem(item);
    }
    public bool TryEquipWeapon(Weapon weapon) {
        if (!_characterClass.allowedWeaponTypes.Contains(weapon.weaponType)) {
            return false;
        }
        for (int j = 0; j < CombatSimManager.Instance.weaponTypeData[weapon.weaponType].equipRequirements.Count; j++) {
            IBodyPart.ATTRIBUTE currReq = CombatSimManager.Instance.weaponTypeData[weapon.weaponType].equipRequirements[j];
            if (!AttachWeaponToBodyPart(weapon, currReq)) {
                DetachWeaponFromBodyParts(weapon);
                return false;
            }
        }
        AddEquippedItem(weapon);
        _equippedWeapon = weapon;
        return true;
    }
    public bool TryEquipArmor(Armor armor) {
        IBodyPart bodyPartToEquip = GetBodyPartForArmor(armor);
        if (bodyPartToEquip == null) {
            return false;
        }
        bodyPartToEquip.AttachItem(armor, Utilities.GetNeededAttributeForArmor(armor));
        AddEquippedItem(armor);
        return true;
    }
    private void UnequipWeapon(Weapon weapon) {
        DetachWeaponFromBodyParts(weapon);
        _equippedWeapon = null;
    }
    private void UnequipArmor(Armor armor) {
        armor.bodyPartAttached.DettachItem(armor, Utilities.GetNeededAttributeForArmor(armor));
    }
    private bool AttachWeaponToBodyPart(Weapon weapon, IBodyPart.ATTRIBUTE req) {
        for (int i = 0; i < this._bodyParts.Count; i++) {
            BodyPart currBodyPart = this._bodyParts[i];
            if (currBodyPart.AttachItem(weapon, req)) {
                return true;
            }
            for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
                if (currBodyPart.secondaryBodyParts[j].AttachItem(weapon, req)) {
                    return true;
                }
            }
        }
        return false;
    }
    private void DetachWeaponFromBodyParts(Weapon weapon) {
        for (int i = 0; i < CombatSimManager.Instance.weaponTypeData[weapon.weaponType].equipRequirements.Count; i++) {
            IBodyPart.ATTRIBUTE req = CombatSimManager.Instance.weaponTypeData[weapon.weaponType].equipRequirements[i];
            for (int j = 0; j < weapon.bodyPartsAttached.Count; j++) {
                if (weapon.bodyPartsAttached[j].DettachItem(weapon, req)) {
                    break;
                }
            }
        }
    }
    internal IBodyPart GetBodyPartForArmor(Armor armor) {
        IBodyPart.ATTRIBUTE neededAttribute = Utilities.GetNeededAttributeForArmor(armor);
        for (int i = 0; i < bodyParts.Count; i++) {
            BodyPart currBodyPart = bodyParts[i];
            //check if currBodyPart can equip the armor
            if (currBodyPart.HasUnusedAttribute(neededAttribute)) {
                return currBodyPart;
            }
            for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
                //check if currBodyPart can equip the armor
                if (currBodyPart.secondaryBodyParts[j].HasUnusedAttribute(neededAttribute)) {
                    return currBodyPart.secondaryBodyParts[j];
                }
            }
        }
        return null;
    }
    public void AddEquippedItem(Item newItem) {
        this._equippedItems.Add(newItem);
        newItem.SetEquipped(true);
        AddItemBonuses(newItem);
    }
    public void RemoveEquippedItem(Item newItem) {
        if (this._equippedItems.Remove(newItem)) {
            newItem.SetEquipped(false);
            RemoveItemBonuses(newItem);
        }
    }
    private void AddItemBonuses(Item item) {
        if (item.itemType == ITEM_TYPE.ARMOR) {
            Armor armor = (Armor) item;
            _bonusDef += armor.def;
            _bonusDefPercent += (armor.prefix.bonusDefPercent + armor.suffix.bonusDefPercent);
        }
    }
    private void RemoveItemBonuses(Item item) {
        if (item.itemType == ITEM_TYPE.ARMOR) {
            Armor armor = (Armor) item;
            _bonusDef -= armor.def;
            _bonusDefPercent -= (armor.prefix.bonusDefPercent + armor.suffix.bonusDefPercent);
        }
    }
    #endregion
}
