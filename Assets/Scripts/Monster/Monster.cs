using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System.IO;

public class Monster : ICharacter, ICharacterSim {
    //Serialized fields
    [SerializeField] private string _name;
    [SerializeField] private MONSTER_TYPE _type;
    [SerializeField] private MONSTER_CATEGORY _category;
    [SerializeField] private int _level;
    [SerializeField] private int _experienceDrop;
    [SerializeField] private int _maxHP;
    [SerializeField] private int _maxSP;
    [SerializeField] private int _attackPower;
    [SerializeField] private int _speed;
    [SerializeField] private int _pDef;
    [SerializeField] private int _mDef;
    [SerializeField] private float _dodgeChance;
    [SerializeField] private float _hitChance;
    [SerializeField] private float _critChance;
    [SerializeField] private List<string> _skillNames;
    [SerializeField] private List<ElementChance> _elementChanceWeaknesses;
    [SerializeField] private List<ElementChance> _elementChanceResistances;

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
    private MonsterParty _ownParty;
    private CharacterPortrait _characterPortrait;
    private PortraitSettings _portraitSettings;
    //private Combat _currentCombat;
    private SIDES _currentSide;
    private List<BodyPart> _bodyParts;
    private List<CharacterAction> _desperateActions;
    private List<CharacterAction> _idleActions;
    private List<Skill> _skills;
    private Dictionary<ELEMENT, float> _elementalWeaknesses;
    private Dictionary<ELEMENT, float> _elementalResistances;
    private Squad _squad;
    private NewParty _currentParty;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public string urlName {
        get { return "<link=" + '"' + this._id.ToString() + "_monster" + '"' + ">" + "[" + _id + "]" + this._name + "</link>"; }
    }
    public string coloredUrlName {
        get { return "<link=" + '"' + this._id.ToString() + "_monster" + '"' + ">" + "<color=#" + this._characterColorCode + ">" + "[" + _id + "]" + this._name + "</color></link>"; }
    }
    public string idName {
        get { return "[" + _id + "]" + this._name; }
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
    public int maxSP {
        get { return _maxSP; }
    }
    public int experienceDrop {
        get { return _experienceDrop; }
    }
    public int pFinalAttack {
        get { return attackPower; }
    }
    public int mFinalAttack {
        get { return attackPower; }
    }
    public int speed {
        get { return agility + level; }
    }
    public int pDef {
        get { return _pDef; }
    }
    public int mDef {
        get { return _mDef; }
    }
    public float critChance {
        get { return _critChance; }
    }
    public float dodgeChance {
        get { return _dodgeChance; }
    }
    public float hitChance {
        get { return _hitChance; }
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
    public MONSTER_TYPE type {
        get { return _type; }
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
        get { return _ownParty; }
    }
    public CharacterRole role {
        get { return null; }
    }
    public CharacterClass characterClass {
        get { return null; }
    }
    public CharacterPortrait characterPortrait {
        get { return _characterPortrait; }
    }
    //public Combat currentCombat {
    //    get { return _currentCombat; }
    //    set { _currentCombat = value; }
    //}
    public List<Skill> skills {
        get { return _skills; }
    }
    public List<string> skillNames {
        get { return _skillNames; }
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
    public NewParty ownParty {
        get { return _ownParty; }
    }
    public List<CharacterAction> desperateActions {
        get { return _desperateActions; }
    }
    public List<CharacterAction> idleActions {
        get { return _idleActions; }
    }
    public Squad squad {
        get { return _squad; }
    }
    public NewParty currentParty {
        get { return _currentParty; }
    }
    public CharacterActionQueue<ActionQueueItem> actionQueue {
        get { return null; }
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
        newMonster._portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(RACE.HUMANS, GENDER.MALE);
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
        this._skillNames = monsterComponent.skillNames;
        this._elementChanceWeaknesses = monsterComponent.elementChanceWeaknesses;
        this._elementChanceResistances = monsterComponent.elementChanceResistances;
    }

    public void SetDataFromMonsterPanelUI() {
        this._name = MonsterPanelUI.Instance.nameInput.text;
        this._type = (MONSTER_TYPE) System.Enum.Parse(typeof(MONSTER_TYPE), MonsterPanelUI.Instance.typeOptions.options[MonsterPanelUI.Instance.typeOptions.value].text);
        this._category = MONSTER_CATEGORY.NORMAL;
        this._experienceDrop = int.Parse(MonsterPanelUI.Instance.expInput.text);
        this._level = int.Parse(MonsterPanelUI.Instance.levelInput.text);
        if(this._level < 1) {
            this._level = 1;
        }else if (this._level > 100) {
            this._level = 100;
        }
        this._maxHP = int.Parse(MonsterPanelUI.Instance.hpInput.text);
        this._maxSP = int.Parse(MonsterPanelUI.Instance.spInput.text);
        this._attackPower = int.Parse(MonsterPanelUI.Instance.powerInput.text);
        this._speed = int.Parse(MonsterPanelUI.Instance.speedInput.text);
        this._pDef = int.Parse(MonsterPanelUI.Instance.pdefInput.text);
        this._mDef = int.Parse(MonsterPanelUI.Instance.mdefInput.text);
        this._dodgeChance = float.Parse(MonsterPanelUI.Instance.dodgeInput.text);
        this._hitChance = float.Parse(MonsterPanelUI.Instance.hitInput.text);
        this._critChance = float.Parse(MonsterPanelUI.Instance.critInput.text);
        this._skillNames = MonsterPanelUI.Instance.allSkills;
        this._elementChanceWeaknesses = new List<ElementChance>();
        this._elementChanceResistances = new List<ElementChance>();
    }

    public void ConstructMonsterData() {
        if(SkillManager.Instance != null) {
            this._skills = new List<Skill>();
            for (int i = 0; i < _skillNames.Count; i++) {
                this._skills.Add(SkillManager.Instance.allSkills[_skillNames[i]]);
            }
        }
        this._elementalWeaknesses = new Dictionary<ELEMENT, float>();
        for (int i = 0; i < _elementChanceWeaknesses.Count; i++) {
            ElementChance elementChance = _elementChanceWeaknesses[i];
            this._elementalWeaknesses.Add(elementChance.element, elementChance.chance);
        }

        this._elementalResistances = new Dictionary<ELEMENT, float>();
        for (int i = 0; i < _elementChanceResistances.Count; i++) {
            ElementChance elementChance = _elementChanceResistances[i];
            this._elementalResistances.Add(elementChance.element, elementChance.chance);
        }
        _skillNames.Clear();
        _elementChanceWeaknesses.Clear();
        _elementChanceResistances.Clear();
    }

    #region Utilities
    public void SetCharacterColor(Color color) {
        _characterColor = color;
        _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
    }
    public void Death() {
        _isDead = true;
        Messenger.Broadcast(Signals.MONSTER_DEATH, this);
        _ownParty.RemoveCharacter(this);
        MonsterManager.Instance.allMonsters.Remove(this);

        GameObject.Destroy(_characterPortrait.gameObject);
        _characterPortrait = null;
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

    #region Interface
    private void BaseInitialize() {
        _isDead = false;
        _desperateActions = new List<CharacterAction>();
        _idleActions = new List<CharacterAction>();
        _raceSetting = RaceManager.Instance.racesDictionary[_type.ToString()].CreateNewCopy();
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        _bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
        if (_skills == null) {
            _skills = new List<Skill>();
        }
        _skills.AddRange(GetGeneralSkills());
        _currentHP = _maxHP;
        _currentSP = _maxSP;
        SetCharacterColor(Color.red);

#if !WORLD_CREATION_TOOL
        GameObject portraitGO = UIManager.Instance.InstantiateUIObject(CharacterManager.Instance.characterPortraitPrefab.name, UIManager.Instance.characterPortraitsParent);
        _characterPortrait = portraitGO.GetComponent<CharacterPortrait>();
        _characterPortrait.GeneratePortrait(this, IMAGE_SIZE.X36, true);
        portraitGO.SetActive(false);
#endif
    }
    private void BaseInitializeSim() {
        _isDead = false;
        _desperateActions = new List<CharacterAction>();
        _idleActions = new List<CharacterAction>();
        _raceSetting = JsonUtility.FromJson<RaceSetting>(System.IO.File.ReadAllText(Utilities.dataPath + "RaceSettings/" + _type.ToString() +".json"));
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        _bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
        _currentHP = _maxHP;
        _currentSP = _maxSP;
        ConstructSkills();
        SetCharacterColor(Color.red);
    }
    public void Initialize() {
        _id = Utilities.SetID(this);
        BaseInitialize();
    }
    public void InitializeSim() {
        _id = Utilities.SetID(this);
        BaseInitializeSim();
        ConstructMonsterData();
    }
    //public void Initialize(MonsterSaveData data){
    //    _id = Utilities.SetID(this, data.id);
    //    BaseInitialize();
    //}
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
        if (CombatSimManager.Instance == null) {
            _ownParty.currentCombat.CharacterDeath(this);
            Death();
        } else {
            DeathSim();
        }
    }
    public int GetPDef(ICharacter enemy) {
        return _pDef;
    }
    public int GetMDef(ICharacter enemy) {
        return _mDef;
    }
    public int GetPDef(ICharacterSim enemy) {
        return _pDef;
    }
    public int GetMDef(ICharacterSim enemy) {
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
    public NewParty CreateOwnParty() {
        if (_ownParty != null) {
            _ownParty.RemoveCharacter(this);
        }
        MonsterParty newParty = new MonsterParty();
        SetOwnedParty(newParty);
        newParty.AddCharacter(this);
        return newParty;
    }
    public void SetOwnedParty(NewParty party) {
        _ownParty = party as MonsterParty;
    }
    public CharacterTag AssignTag(CHARACTER_TAG tag) {
        //No tag assignment
        return null;
    }
    public void AddHistory(Log log) {
        //No history
    }
    public CharacterAction GetRandomDesperateAction(ref IObject targetObject) {
        return _desperateActions[Utilities.rng.Next(0, _desperateActions.Count)];
    }
    public CharacterAction GetRandomIdleAction(ref IObject targetObject) {
        return _idleActions[Utilities.rng.Next(0, _idleActions.Count)];
    }
    public void DeathSim() {
        _isDead = true;
        CombatSimManager.Instance.currentCombat.CharacterDeath(this);
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
    public void EnableDisableSkills(CombatSim combat) {
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
                                ICharacterSim enemy = combat.charactersSideB[j];
                                if (enemy.currentRow < this._currentRow) {
                                    hasEnemyOnLeft = true;
                                    break;
                                }
                            }
                        } else {
                            for (int j = 0; j < combat.charactersSideA.Count; j++) {
                                ICharacterSim enemy = combat.charactersSideA[j];
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
                                ICharacterSim enemy = combat.charactersSideB[j];
                                if (enemy.currentRow > this._currentRow) {
                                    hasEnemyOnRight = true;
                                    break;
                                }
                            }
                        } else {
                            for (int j = 0; j < combat.charactersSideA.Count; j++) {
                                ICharacterSim enemy = combat.charactersSideA[j];
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
    public void SetCurrentParty(NewParty party) {
        _currentParty = party as CharacterParty;
    }
    public void OnRemovedFromParty() {
        SetCurrentParty(_ownParty); //set the character's party to it's own party
    }
    public void OnAddedToParty() {
        //if (this.currentParty.id != _ownParty.id) {
        //    if (_ownParty.specificLocation is BaseLandmark) {
        //        _ownParty.specificLocation.RemoveCharacterFromLocation(_ownParty);
        //    }
        //    _ownParty.icon.SetVisualState(false);
        //}
    }
    public bool InviteToParty(ICharacter inviter) {
        return false;
    }
    #endregion

    #region Squads
    public void SetSquad(Squad squad) {
        _squad = squad;
    }
    #endregion

    #region Action Queue
    public void AddActionToQueue(CharacterAction action, IObject targetObject, CharacterQuestData associatedQuestData = null, int position = -1) {  }
    public void RemoveActionFromQueue(ActionQueueItem item) {    }
    #endregion
}
