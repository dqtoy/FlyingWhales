using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System.IO;

public class CharacterSim : ICharacterSim {
    [SerializeField] private string _name;
    [SerializeField] private string _className;
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
    [SerializeField] private int _pDefHead;
    [SerializeField] private int _pDefBody;
    [SerializeField] private int _pDefLegs;
    [SerializeField] private int _pDefHands;
    [SerializeField] private int _pDefFeet;
    [SerializeField] private int _mDefHead;
    [SerializeField] private int _mDefBody;
    [SerializeField] private int _mDefLegs;
    [SerializeField] private int _mDefHands;
    [SerializeField] private int _mDefFeet;
    [SerializeField] private float _weaponAttack;
    [SerializeField] private GENDER _gender;
    [SerializeField] private List<string> _skillNames;

    private int _id;
    private int _currentHP;
    private int _currentSP;
    private int _currentRow;
    private int _actRate;
    private float _critChance;
    private float _critDamage;
    private float _bonusPDefPercent;
    private float _bonusMDefPercent;
    private bool _isDead;
    private SIDES _currentSide;
    private RaceSetting _raceSetting;
    private CharacterClass _characterClass;
    private CharacterBattleTracker _battleTracker;
    private CharacterBattleOnlyTracker _battleOnlyTracker;
    private List<Skill> _skills;
    private List<BodyPart> _bodyParts;
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
    public float weaponAttack {
        get { return _weaponAttack; }
    }
    public int pDefHead {
        get { return _pDefHead; }
    }
    public int pDefBody {
        get { return _pDefBody; }
    }
    public int pDefLegs {
        get { return _pDefLegs; }
    }
    public int pDefHands {
        get { return _pDefHands; }
    }
    public int pDefFeet {
        get { return _pDefFeet; }
    }
    public int mDefHead {
        get { return _mDefHead; }
    }
    public int mDefBody {
        get { return _mDefBody; }
    }
    public int mDefLegs {
        get { return _mDefLegs; }
    }
    public int mDefHands {
        get { return _mDefHands; }
    }
    public int mDefFeet {
        get { return _mDefFeet; }
    }
    public int currentRow {
        get { return _currentRow; }
    }
    public int actRate {
        get { return _actRate; }
        set { _actRate = value; }
    }
    public int speed {
        get { return agility + level; }
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
            return (int) (((weaponAttack + str) * (1f + (str / 20f))) * (1f + ((float) level / 100f)));
        }
    }
    public int mFinalAttack {
        get {
            float intl = (float) intelligence;
            return (int) (((weaponAttack + intl) * (1f + (intl / 20f))) * (1f + ((float) level / 100f)));
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
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        _battleTracker = new CharacterBattleTracker();
        _elementalWeaknesses = new Dictionary<ELEMENT, float>(CombatSimManager.Instance.elementsChanceDictionary);
        _elementalResistances = new Dictionary<ELEMENT, float>(CombatSimManager.Instance.elementsChanceDictionary);
    }
    public void SetDataFromCharacterPanelUI() {
        _name = CharacterPanelUI.Instance.nameInput.text;
        _className = CharacterPanelUI.Instance.classOptions.options[CharacterPanelUI.Instance.classOptions.value].text;
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
        _weaponAttack = float.Parse(CharacterPanelUI.Instance.weaponAttackInput.text);

        _pDefHead = int.Parse(CharacterPanelUI.Instance.pHeadInput.text);
        _pDefBody = int.Parse(CharacterPanelUI.Instance.pBodyInput.text);
        _pDefLegs = int.Parse(CharacterPanelUI.Instance.pLegsInput.text);
        _pDefHands = int.Parse(CharacterPanelUI.Instance.pHandsInput.text);
        _pDefFeet = int.Parse(CharacterPanelUI.Instance.pFeetInput.text);

        _mDefHead = int.Parse(CharacterPanelUI.Instance.mHeadInput.text);
        _mDefBody = int.Parse(CharacterPanelUI.Instance.mBodyInput.text);
        _mDefLegs = int.Parse(CharacterPanelUI.Instance.mLegsInput.text);
        _mDefHands = int.Parse(CharacterPanelUI.Instance.mHandsInput.text);
        _mDefFeet = int.Parse(CharacterPanelUI.Instance.mFeetInput.text);

        _skillNames = CharacterPanelUI.Instance.skillNames;
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
    public void AdjustHP(int amount) {
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
    public int GetPDef(ICharacterSim enemy) {
        float levelDiff = (float) (enemy.level - level);
        return (int) ((((float) (GetPDefBonus() + (strength + (vitality * 2)))) * (1f + (_bonusPDefPercent / 100f))) * (1f + ((levelDiff < 0 ? 0 : levelDiff) / 20f)));
    }
    public int GetMDef(ICharacterSim enemy) {
        float levelDiff = (float) (enemy.level - level);
        return (int) ((((float) (GetMDefBonus() + (intelligence + (vitality * 2)))) * (1f + (_bonusMDefPercent / 100f))) * (1f + ((levelDiff < 0 ? 0 : levelDiff) / 20f)));
    }
    public void EnableDisableSkills(CombatSim combatSim) {
        bool isAllAttacksInRange = true;
        bool isAttackInRange = false;

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
                isAttackInRange = combatSim.HasTargetInRangeForSkill(skill, this);
                if (!isAttackInRange) {
                    isAllAttacksInRange = false;
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

        for (int i = 0; i < this._skills.Count; i++) {
            Skill skill = this._skills[i];
            if (skill is MoveSkill) {
                skill.isEnabled = true;
                if (isAllAttacksInRange) {
                    skill.isEnabled = false;
                    continue;
                }
                if (skill.skillName == "MoveLeft") {
                    if (this._currentRow == 1) {
                        skill.isEnabled = false;
                        continue;
                    } else {
                        bool hasEnemyOnLeft = false;
                        if (combatSim.charactersSideA.Contains(this)) {
                            for (int j = 0; j < combatSim.charactersSideB.Count; j++) {
                                ICharacterSim enemy = combatSim.charactersSideB[j];
                                if (enemy.currentRow < this._currentRow) {
                                    hasEnemyOnLeft = true;
                                    break;
                                }
                            }
                        } else {
                            for (int j = 0; j < combatSim.charactersSideA.Count; j++) {
                                ICharacterSim enemy = combatSim.charactersSideA[j];
                                if (enemy.currentRow < this._currentRow) {
                                    hasEnemyOnLeft = true;
                                    break;
                                }
                            }
                        }
                        if (!hasEnemyOnLeft) {
                            skill.isEnabled = false;
                            continue;
                        }
                    }
                } else if (skill.skillName == "MoveRight") {
                    if (this._currentRow == 5) {
                        skill.isEnabled = false;
                    } else {
                        bool hasEnemyOnRight = false;
                        if (combatSim.charactersSideA.Contains(this)) {
                            for (int j = 0; j < combatSim.charactersSideB.Count; j++) {
                                ICharacterSim enemy = combatSim.charactersSideB[j];
                                if (enemy.currentRow > this._currentRow) {
                                    hasEnemyOnRight = true;
                                    break;
                                }
                            }
                        } else {
                            for (int j = 0; j < combatSim.charactersSideA.Count; j++) {
                                ICharacterSim enemy = combatSim.charactersSideA[j];
                                if (enemy.currentRow > this._currentRow) {
                                    hasEnemyOnRight = true;
                                    break;
                                }
                            }
                        }
                        if (!hasEnemyOnRight) {
                            skill.isEnabled = false;
                            continue;
                        }
                    }
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
    private int GetPDefBonus() {
        return _pDefHead + _pDefBody + _pDefLegs + _pDefHands + _pDefFeet;
    }
    private int GetMDefBonus() {
        return _mDefHead + _mDefBody + _mDefLegs + _mDefHands + _mDefFeet;
    }
    #endregion
}
