using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Monster : ICharacter {
    //Serialized fields
    private string _name;
    private MONSTER_TYPE _type;
    private MONSTER_CATEGORY _category;
    private int _level;
    private int _experienceDrop;
    private int _maxHP;
    private int _maxSP;
    private int _attackPower;
    private int _speed;
    private int _pDef;
    private int _mDef;
    private float _dodgeChance;
    private float _hitChance;
    private float _critChance;
    private List<Skill> _skills;
    private Dictionary<ELEMENT, float> _elementalWeaknesses;
    private Dictionary<ELEMENT, float> _elementalResistances;
    //To add item drops and their chances

    private string _characterColorCode;
    private int _id;
    private int _currentHP;
    private int _currentSP;
    private int _actRate;
    private int _currentRow;
    private bool _isDead;
    private Color _characterColor;
    private CharacterBattleOnlyTracker _battleOnlyTracker;
    private BaseLandmark _homeLandmark;
    private StructureObj _homeStructure;
    private RaceSetting _raceSetting;
    private MonsterParty _party;
    private Combat _currentCombat;
    private SIDES _currentSide;
    private List<BodyPart> _bodyParts;
    private List<CharacterAction> _desperateActions;
    private List<CharacterAction> _idleActions;
    private PortraitSettings _portraitSettings;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public string urlName {
        get { return "<link=" + '"' + this._id.ToString() + "_monster" + '"' + ">" + this._name + "</link>"; }
    }
    public string coloredUrlName {
        get { return "<link=" + '"' + this._id.ToString() + "_monster" + '"' + ">" + "<color=#" + this._characterColorCode + ">" + this._name + "</color></link>"; }
    }
    public int id {
        get { return _id; }
    }
    public int attackPower {
        get { return _attackPower; }
    }
    public int actRate {
        get { return _actRate; }
        set { _actRate = value; }
    }
    public int strength {
        get { return 0; }
    }
    public int intelligence {
        get { return 0; }
    }
    public int vitality {
        get { return 0; }
    }
    public int agility {
        get { return _speed; }
    }
    public int baseAgility {
        get { return _speed; }
    }
    public int level {
        get { return _level; }
    }
    public int currentHP {
        get { return _currentHP; }
    }
    public int maxHP {
        get { return _maxHP; }
    }
    public int currentRow {
        get { return _currentRow; }
    }
    public int currentSP {
        get { return _currentSP; }
    }
    public int experienceDrop {
        get { return _experienceDrop; }
    }
    public float critChance {
        get { return _critChance; }
    }
    public float critDamage {
        get { return 0f; }
    }
    public float computedPower {
        get { return GetAttackPower() + GetDefensePower(); }
    }
    public bool isDead {
        get { return _isDead; }
    }
    public GENDER gender {
        get { return GENDER.MALE; }
    }
    public SIDES currentSide {
        get { return _currentSide; }
    }
    public ICHARACTER_TYPE icharacterType {
        get { return ICHARACTER_TYPE.MONSTER; }
    }
    public CharacterBattleOnlyTracker battleOnlyTracker {
        get { return _battleOnlyTracker; }
    }
    public Faction faction {
        get { return null; }
    }
    public BaseLandmark homeLandmark {
        get { return _homeLandmark; }
    }
    public StructureObj homeStructure {
        get { return _homeStructure; }
    }
    public Area home {
        get { return null; }
    }
    public PortraitSettings portraitSettings {
        get { return _portraitSettings; }
    }
    public MonsterParty party {
        get { return _party; }
    }
    public CharacterRole role {
        get { return null; }
    }
    public Combat currentCombat {
        get { return _currentCombat; }
        set { _currentCombat = value; }
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
    public IParty iparty {
        get { return _party; }
    }
    public List<CharacterAction> desperateActions {
        get { return _desperateActions; }
    }
    public List<CharacterAction> idleActions {
        get { return _idleActions; }
    }
    #endregion

    public Monster CreateNewCopy() {
        Monster newMonster = new Monster();
        newMonster._name = this._name;
        newMonster._type = this._type;
        newMonster._category = this._category;
        newMonster._level = this._level;
        newMonster._experienceDrop = this._experienceDrop;
        newMonster._maxHP = this._maxHP;
        newMonster._maxSP = this._maxSP;
        newMonster._attackPower = this._attackPower;
        newMonster._speed = this._speed;
        newMonster._pDef = this._pDef;
        newMonster._mDef = this._mDef;
        newMonster._dodgeChance = this._dodgeChance;
        newMonster._hitChance = this._hitChance;
        newMonster._critChance = this._critChance;
        newMonster._portraitSettings = CharacterManager.Instance.GenerateRandomPortrait();
//#if !WORLD_CREATION_TOOL
//        newMonster._monsterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.MONSTER, "MonsterObject") as MonsterObj;
//        newMonster._monsterObj.SetMonster(newMonster);
//#endif
        newMonster._skills = new List<Skill>();
        for (int i = 0; i < this._skills.Count; i++) {
            newMonster._skills.Add(_skills[i].CreateNewCopy());
        }
        newMonster._elementalWeaknesses = new Dictionary<ELEMENT, float>(this._elementalWeaknesses);
        newMonster._elementalResistances = new Dictionary<ELEMENT, float>(this._elementalResistances);

        return newMonster;
    }

    public void SetData(MonsterComponent monsterComponent) {
        this._name = monsterComponent.monsterName;
        this._type = monsterComponent.type;
        this._category = monsterComponent.category;
        this._experienceDrop = monsterComponent.experienceDrop;
        this._level = monsterComponent.level;
        this._maxHP = monsterComponent.maxHP;
        this._maxSP = monsterComponent.maxSP;
        this._attackPower = monsterComponent.attackPower;
        this._speed = monsterComponent.speed;
        this._pDef = monsterComponent.pDef;
        this._mDef = monsterComponent.mDef;
        this._dodgeChance = monsterComponent.dodgeChance;
        this._hitChance = monsterComponent.hitChance;
        this._critChance = monsterComponent.critChance;

        this._skills = new List<Skill>();
        for (int i = 0; i < monsterComponent.skillNames.Count; i++) {
            _skills.Add(SkillManager.Instance.allSkills[monsterComponent.skillNames[i]]);
        }

        this._elementalWeaknesses = new Dictionary<ELEMENT, float>();
        for (int i = 0; i < monsterComponent.elementChanceWeaknesses.Count; i++) {
            ElementChance elementChance = monsterComponent.elementChanceWeaknesses[i];
            this._elementalWeaknesses.Add(elementChance.element, elementChance.chance);
        }

        this._elementalResistances = new Dictionary<ELEMENT, float>();
        for (int i = 0; i < monsterComponent.elementChanceResistances.Count; i++) {
            ElementChance elementChance = monsterComponent.elementChanceResistances[i];
            this._elementalResistances.Add(elementChance.element, elementChance.chance);
        }
    }

    #region Utilities
    public void SetCharacterColor(Color color) {
        _characterColor = color;
        _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
    }
    public void Death() {
        _isDead = true;
        Messenger.Broadcast(Signals.MONSTER_DEATH, this);
        _party.RemoveCharacter(this);
    }
    private float GetAttackPower() {
        //float statUsed = (float) Utilities.GetStatByClass(this);
        float weaponAttack = (float)attackPower;
        return (((weaponAttack + (float)strength) * (1f + ((float) strength / 100f))) * (1f + ((float) agility / 100f))) * (1f + ((float) level / 100f));
    }
    private float GetDefensePower() {
        return ((float) (strength + intelligence + _pDef + _mDef + maxHP + (2 * vitality)) * (1f + ((float) level / 100f))) * (1f + ((float) agility / 100f));
    }
    private List<Skill> GetGeneralSkills() {
        List<Skill> allGeneralSkills = new List<Skill>();
        foreach (Skill skill in SkillManager.Instance.generalSkills.Values) {
            if(skill is FleeSkill) {
                continue;
            }
            allGeneralSkills.Add(skill.CreateNewCopy());
        }
        return allGeneralSkills;
    }
    #endregion

    #region Interface
    private void BaseInitialize() {
        _isDead = false;
        CharacterSetup setup = CombatManager.Instance.GetBaseCharacterSetup(Utilities.NormalizeString(_type.ToString()));
        _raceSetting = setup.raceSetting.CreateNewCopy();
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        _bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
        if (_skills == null) {
            _skills = new List<Skill>();
        }
        _skills.AddRange(GetGeneralSkills());
        _currentHP = _maxHP;
        _currentSP = _maxSP;
        SetCharacterColor(Color.red);
    }
    public void Initialize() {
        _id = Utilities.SetID(this);
        BaseInitialize();
    }
    public void Initialize(MonsterSaveData data){
        _id = Utilities.SetID(this, data.id);
        BaseInitialize();
    }
    public void SetSide(SIDES side) {
        _currentSide = side;
    }
    public void SetRowNumber(int rowNumber) {
        _currentRow = rowNumber;
    }
    public void AdjustExperience(int amount) {
        //Monster no exp
    }
    public void AdjustSP(int amount) {
        _currentSP += amount;
        _currentSP = Mathf.Clamp(_currentSP, 0, _maxSP);
    }
    public void AdjustHP(int amount) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, _maxHP);
        if (previous != this._currentHP) {
            if (this._currentHP == 0) {
                FaintOrDeath();
            }
        }
    }
    public void FaintOrDeath() {
        _currentCombat.CharacterDeath(this);
        Death();
    }
    public int GetPDef(ICharacter enemy) {
        return _pDef;
    }
    public int GetMDef(ICharacter enemy) {
        return _mDef;
    }
    public void ResetToFullHP() {
        AdjustHP(_maxHP);
    }
    public void ResetToFullSP() {
        AdjustSP(_maxSP);
    }
    public void EverydayAction() {
        //No daily/tick action
    }
    public void SetHomeLandmark(BaseLandmark newHomeLandmark) {
        this._homeLandmark = newHomeLandmark;
    }
    public void SetHomeStructure(StructureObj newHomeStructure) {
        if (_homeStructure != null) {
            _homeStructure.AdjustNumOfResidents(-1);
        }
        _homeStructure = newHomeStructure;
        newHomeStructure.AdjustNumOfResidents(1);
    }
    public MonsterParty CreateNewParty() {
        if (_party != null) {
            _party.RemoveCharacter(this);
        }
        MonsterParty newParty = new MonsterParty();
        newParty.AddCharacter(this);
        return newParty;
    }
    public void SetParty(IParty party) {
        _party = party as MonsterParty;
    }
    public CharacterTag AssignTag(CHARACTER_TAG tag) {
        //No tag assignment
        return null;
    }
    public void AddHistory(Log log) {
        //No history
    }
    public CharacterAction GetRandomDesperateAction() {
        return _desperateActions[Utilities.rng.Next(0, _desperateActions.Count)];
    }
    public CharacterAction GetRandomIdleAction() {
        return _idleActions[Utilities.rng.Next(0, _idleActions.Count)];
    }
    public void EnableDisableSkills(Combat combat) {
        bool isAllAttacksInRange = true;
        bool isAttackInRange = false;

        //Body part skills / general skills
        for (int i = 0; i < this._skills.Count; i++) {
            Skill skill = this._skills[i];
            skill.isEnabled = true;

            if (skill is AttackSkill) {
                isAttackInRange = combat.HasTargetInRangeForSkill(skill, this);
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
                        if (combat.charactersSideA.Contains(this)) {
                            for (int j = 0; j < combat.charactersSideB.Count; j++) {
                                ICharacter enemy = combat.charactersSideB[j];
                                if (enemy.currentRow < this._currentRow) {
                                    hasEnemyOnLeft = true;
                                    break;
                                }
                            }
                        } else {
                            for (int j = 0; j < combat.charactersSideA.Count; j++) {
                                ICharacter enemy = combat.charactersSideA[j];
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
                        if (combat.charactersSideA.Contains(this)) {
                            for (int j = 0; j < combat.charactersSideB.Count; j++) {
                                ICharacter enemy = combat.charactersSideB[j];
                                if (enemy.currentRow > this._currentRow) {
                                    hasEnemyOnRight = true;
                                    break;
                                }
                            }
                        } else {
                            for (int j = 0; j < combat.charactersSideA.Count; j++) {
                                ICharacter enemy = combat.charactersSideA[j];
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
}
