using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System.IO;

public class Monster : ICharacter, ICharacterSim, IInteractable {
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
    [SerializeField] private int _def;
    [SerializeField] private float _dodgeChance;
    [SerializeField] private float _hitChance;
    [SerializeField] private float _critChance;
    [SerializeField] private bool _isSleepingOnSpawn;
    [SerializeField] private List<string> _skillNames;
    [SerializeField] private List<ElementChance> _elementChanceWeaknesses;
    [SerializeField] private List<ElementChance> _elementChanceResistances;
    [SerializeField] private List<ItemDrop> _itemDrops;

    //To add item drops and their chances
    private string _characterColorCode;
    private int _id;
    private int _currentHP;
    private int _currentSP;
    private int _actRate;
    private int _currentRow;
    private bool _isDead;
    private bool _isBeingInspected;
    private bool _hasBeenInspected;
    private bool _isSleeping;
    private MODE _currentMode;
    private Color _characterColor;
    private CharacterBattleOnlyTracker _battleOnlyTracker;
    private BaseLandmark _homeLandmark;
    private RaceSetting _raceSetting;
    private MonsterParty _ownParty;
    private CharacterPortrait _characterPortrait;
    private PortraitSettings _portraitSettings;
    //private Combat _currentCombat;
    private SIDES _currentSide;
    private List<BodyPart> _bodyParts;
    private List<CharacterAction> _miscActions;
    private List<Skill> _skills;
    private Dictionary<ELEMENT, float> _elementalWeaknesses;
    private Dictionary<ELEMENT, float> _elementalResistances;
    private Dictionary<string, float> _itemDropsLookup;
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
        get {
            return agility;
            //float agi = (float) agility;
            //return (int) (100f * ((1f + ((agi / 5f) / 100f)) + (float) level + (agi / 3f)));
        }
    }
    public int def {
        get { return _def; }
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
        get { return GetComputedPower(); }
    }
    public bool isDead {
        get { return _isDead; }
    }
    public bool isBeingInspected {
        get { return _isBeingInspected; }
    }
    public bool hasBeenInspected {
        get { return _hasBeenInspected; }
    }
    public bool isSleeping {
        get { return _isSleeping; }
    }
    public bool isSleepingOnSpawn {
        get { return _isSleepingOnSpawn; }
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
    public MODE currentMode {
        get { return _currentMode; }
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
    public HiddenDesire hiddenDesire {
        get { return null; }
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
    public List<ItemDrop> itemDrops {
        get { return _itemDrops; }
    }
    public Dictionary<ELEMENT, float> elementalWeaknesses {
        get { return _elementalWeaknesses; }
    }
    public Dictionary<ELEMENT, float> elementalResistances {
        get { return _elementalResistances; }
    }
    public Dictionary<string, float> itemDropsLookup {
        get { return _itemDropsLookup; }
    }
    public NewParty ownParty {
        get { return _ownParty; }
    }
    public List<CharacterAction> miscActions {
        get { return _miscActions; }
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
        newMonster._def = this._def;
        newMonster._dodgeChance = this._dodgeChance;
        newMonster._hitChance = this._hitChance;
        newMonster._critChance = this._critChance;
        newMonster._isSleepingOnSpawn = this._isSleepingOnSpawn;
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
        newMonster._itemDropsLookup = new Dictionary<string, float>(this._itemDropsLookup);

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
        this._def = monsterComponent.def;
        this._dodgeChance = monsterComponent.dodgeChance;
        this._hitChance = monsterComponent.hitChance;
        this._critChance = monsterComponent.critChance;
        this._isSleepingOnSpawn = monsterComponent.isSleepingOnSpawn;
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
        this._def = int.Parse(MonsterPanelUI.Instance.defInput.text);
        this._dodgeChance = float.Parse(MonsterPanelUI.Instance.dodgeInput.text);
        this._hitChance = float.Parse(MonsterPanelUI.Instance.hitInput.text);
        this._critChance = float.Parse(MonsterPanelUI.Instance.critInput.text);
        this._isSleepingOnSpawn = MonsterPanelUI.Instance.isSleepingOnSpawnToggle.isOn;
        this._skillNames = MonsterPanelUI.Instance.allSkills;
        this._elementChanceWeaknesses = new List<ElementChance>();
        this._elementChanceResistances = new List<ElementChance>();
        this._itemDrops = MonsterPanelUI.Instance.itemDrops;
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
        ConstructItemDrops();
    }
    private void ConstructItemDrops() {
        this._itemDropsLookup = new Dictionary<string, float>();
        for (int i = 0; i < _itemDrops.Count; i++) {
            ItemDrop itemDrop = _itemDrops[i];
            this._itemDropsLookup.Add(itemDrop.itemName, itemDrop.dropRate);
        }
        _itemDrops.Clear();
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
    private float GetComputedPower() {
        float totalAttack = (float) attackPower;
        float maxHPFloat = (float) maxHP;
        float maxSPFloat = (float) maxSP;
        //TODO: totalAttack += final damage bonus
        float compPower = (totalAttack + (float) GetDef() + (float) (speed - 100) + (maxHPFloat / 10f) + (maxSPFloat / 10f)) * ((((float) currentHP / maxHPFloat) + ((float) currentSP / maxSPFloat)) / 2f);
        return compPower *= party.icharacters.Count;
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
    public void SetSleeping(bool state) {
        _isSleeping = state;
    }
    public void TryToSleep() {
        if (!_isSleeping && _currentParty.specificLocation.tileLocation.id == _homeLandmark.id) {
            SetSleeping(true);
        }
    }
    #endregion

    #region Interface
    private void BaseInitialize() {
        _isDead = false;
        _miscActions = new List<CharacterAction>();
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
        SetSleeping(_isSleepingOnSpawn);

#if !WORLD_CREATION_TOOL
        GameObject portraitGO = UIManager.Instance.InstantiateUIObject(CharacterManager.Instance.characterPortraitPrefab.name, UIManager.Instance.characterPortraitsParent);
        _characterPortrait = portraitGO.GetComponent<CharacterPortrait>();
        _characterPortrait.GeneratePortrait(this, IMAGE_SIZE.X36, true);
        portraitGO.SetActive(false);
#endif
    }
    private void BaseInitializeSim() {
        _isDead = false;
        _miscActions = new List<CharacterAction>();
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
    public void AdjustHP(int amount, ICharacter killer = null) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, _maxHP);
        if (previous != this._currentHP) {
            if (this._currentHP == 0) {
                FaintOrDeath(killer);
            }
        }
    }
    public void FaintOrDeath(ICharacter killer) {
        if (CombatSimManager.Instance == null) {
            if (_ownParty.currentCombat != null) {
                _ownParty.currentCombat.CharacterDeath(this, killer);
            }
            Death();
        } else {
            DeathSim();
        }
    }
    public int GetDef() {
        return _def;
    }
    public void ResetToFullHP() {
        AdjustHP(_maxHP);
    }
    public void ResetToFullSP() {
        AdjustSP(_maxSP);
    }
    //public void EverydayAction() {
    //    //No daily/tick action
    //}
    public void SetHomeLandmark(BaseLandmark newHomeLandmark) {
        this._homeLandmark = newHomeLandmark;
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
    public Attribute AddAttribute(ATTRIBUTE tag) {
        //No tag assignment
        return null;
    }
    public void AddHistory(Log log) {
        //No history
    }
    public CharacterAction GetRandomMiscAction(ref IObject targetObject) {
        return _miscActions[Utilities.rng.Next(0, _miscActions.Count)];
    }
    public CharacterAction GetMiscAction(ACTION_TYPE type) {
        for (int i = 0; i < _miscActions.Count; i++) {
            if (_miscActions[i].actionData.actionType == type) {
                return _miscActions[i];
            }
        }
        return null;
    }
    public void DeathSim() {
        _isDead = true;
        CombatSimManager.Instance.currentCombat.CharacterDeath(this);
    }
    public void SetMode(MODE mode) {
        _currentMode = mode;
    }
    public void SetIsBeingInspected(bool state) {
        _isBeingInspected = state;
    }
    public void SetHasBeenInspected(bool state) {
        _hasBeenInspected = true;
    }
    public void EnableDisableSkills(Combat combat) {
        //bool isAllAttacksInRange = true;
        //bool isAttackInRange = false;

        //Body part skills / general skills
        for (int i = 0; i < this._skills.Count; i++) {
            Skill skill = this._skills[i];
            skill.isEnabled = true;

            if (skill is FleeSkill) {
                if (this.currentHP >= (this.maxHP / 2)) {
                    skill.isEnabled = false;
                    continue;
                }
            }
            //if (skill is AttackSkill) {
            //    isAttackInRange = combat.HasTargetInRangeForSkill(skill, this);
            //    if (!isAttackInRange) {
            //        isAllAttacksInRange = false;
            //        skill.isEnabled = false;
            //        continue;
            //    }
            //} else 
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
        //                if (combat.charactersSideA.Contains(this)) {
        //                    for (int j = 0; j < combat.charactersSideB.Count; j++) {
        //                        ICharacter enemy = combat.charactersSideB[j];
        //                        if (enemy.currentRow < this._currentRow) {
        //                            hasEnemyOnLeft = true;
        //                            break;
        //                        }
        //                    }
        //                } else {
        //                    for (int j = 0; j < combat.charactersSideA.Count; j++) {
        //                        ICharacter enemy = combat.charactersSideA[j];
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
        //                if (combat.charactersSideA.Contains(this)) {
        //                    for (int j = 0; j < combat.charactersSideB.Count; j++) {
        //                        ICharacter enemy = combat.charactersSideB[j];
        //                        if (enemy.currentRow > this._currentRow) {
        //                            hasEnemyOnRight = true;
        //                            break;
        //                        }
        //                    }
        //                } else {
        //                    for (int j = 0; j < combat.charactersSideA.Count; j++) {
        //                        ICharacter enemy = combat.charactersSideA[j];
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
    public void EnableDisableSkills(CombatSim combat) {
        bool isAllAttacksInRange = true;
        //bool isAttackInRange = false;

        //Body part skills / general skills
        for (int i = 0; i < this._skills.Count; i++) {
            Skill skill = this._skills[i];
            skill.isEnabled = true;

            //if (skill is AttackSkill) {
            //    isAttackInRange = combat.HasTargetInRangeForSkill(skill, this);
            //    if (!isAttackInRange) {
            //        isAllAttacksInRange = false;
            //        skill.isEnabled = false;
            //        continue;
            //    }
            //} else
            if (skill is FleeSkill) {
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
        //                if (combat.charactersSideA.Contains(this)) {
        //                    for (int j = 0; j < combat.charactersSideB.Count; j++) {
        //                        ICharacterSim enemy = combat.charactersSideB[j];
        //                        if (enemy.currentRow < this._currentRow) {
        //                            hasEnemyOnLeft = true;
        //                            break;
        //                        }
        //                    }
        //                } else {
        //                    for (int j = 0; j < combat.charactersSideA.Count; j++) {
        //                        ICharacterSim enemy = combat.charactersSideA[j];
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
        //                if (combat.charactersSideA.Contains(this)) {
        //                    for (int j = 0; j < combat.charactersSideB.Count; j++) {
        //                        ICharacterSim enemy = combat.charactersSideB[j];
        //                        if (enemy.currentRow > this._currentRow) {
        //                            hasEnemyOnRight = true;
        //                            break;
        //                        }
        //                    }
        //                } else {
        //                    for (int j = 0; j < combat.charactersSideA.Count; j++) {
        //                        ICharacterSim enemy = combat.charactersSideA[j];
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
    public bool IsInOwnParty() {
        return true;
    }
    #endregion

    #region Squads
    public void SetSquad(Squad squad) {
        _squad = squad;
    }
    #endregion

    #region Action Queue
    public void AddActionToQueue(CharacterAction action, IObject targetObject, Quest associatedQuest = null, int position = -1) {  }
    public void RemoveActionFromQueue(ActionQueueItem item) {    }
    #endregion

    #region Item Drops
    public List<string> GetRandomDroppedItems() {
        List<string> drops = new List<string>();
        foreach (KeyValuePair<string, float> kvp in itemDropsLookup) {
            if (UnityEngine.Random.Range(0f, 100f) < kvp.Value) {
                drops.Add(kvp.Key);
            }
        }
        return drops;
    }
    #endregion
}
