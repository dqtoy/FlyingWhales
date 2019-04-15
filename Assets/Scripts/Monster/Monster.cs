using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class Monster : ICharacter, ICharacterSim, IInteractable {
    //Serialized fields
    [SerializeField] protected string _name;
    [SerializeField] protected MONSTER_TYPE _type;
    [SerializeField] protected MONSTER_CATEGORY _category;
    [SerializeField] protected int _level;
    [SerializeField] protected int _experienceDrop;
    [SerializeField] protected int _maxHP;
    [SerializeField] protected int _maxSP;
    [SerializeField] protected int _attackPower;
    [SerializeField] protected int _speed;
    [SerializeField] protected int _startingArmyCount;
    [SerializeField] protected float _dodgeChance;
    [SerializeField] protected float _hitChance;
    [SerializeField] protected float _critChance;
    [SerializeField] protected bool _isSleepingOnSpawn;
    [SerializeField] protected List<string> _skillNames;
    [SerializeField] protected List<ElementChance> _elementChanceWeaknesses;
    [SerializeField] protected List<ElementChance> _elementChanceResistances;
    [SerializeField] protected List<ItemDrop> _itemDrops;

    //To add item drops and their chances
    protected string _characterColorCode;
    protected int _id;
    protected int _currentHP;
    protected int _currentSP;
    protected float _actRate;
    protected int _currentRow;
    protected bool _isDead;
    protected bool _isBeingInspected;
    protected bool _hasBeenInspected;
    protected bool _isSleeping;
    protected MODE _currentMode;
    protected SIDES _currentSide;
    protected Color _characterColor;
    protected CharacterBattleOnlyTracker _battleOnlyTracker;
    protected Area _homeArea;
    protected RaceSetting _raceSetting;
    protected MonsterParty _ownParty;
    protected CharacterPortrait _characterPortrait;
    protected PortraitSettings _portraitSettings;
    protected Minion _minion;
    //private Combat _currentCombat;
    //private List<BodyPart> _bodyParts;
    protected List<Skill> _skills;
    protected List<Interaction> _currentInteractions;
    protected Dictionary<ELEMENT, float> _elementalWeaknesses;
    protected Dictionary<ELEMENT, float> _elementalResistances;
    protected Dictionary<string, float> _itemDropsLookup;
    protected Party _currentParty;
    protected Dictionary<STAT, float> _buffs;
    protected PlayerCharacterItem _playerCharacterItem;
    protected int _currentInteractionTick;

    public CharacterUIData uiData { get; private set; }
    public WeightedDictionary<INTERACTION_TYPE> interactionWeights { get; private set; }
    public WeightedDictionary<bool> eventTriggerWeights { get; private set; }
    public CharacterToken characterToken { get; private set; }

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
    public virtual int attackPower {
        get { return _attackPower; }
    }
    public float actRate {
        get { return _actRate; }
        set { _actRate = value; }
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
    public virtual int speed {
        get { return _speed; }
    }
    public int startingArmyCount {
        get { return _startingArmyCount; }
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
    public float computedPower {
        get { return GetComputedPower(); }
    }
    public int experience {
        get { return 0; }
    }
    public int maxExperience {
        get { return 0; }
    }
    public int combatBaseAttack {
        get { return 0; }
        set { }
    }
    public int combatBaseSpeed {
        get { return 0; }
        set { }
    }
    public int combatBaseHP {
        get { return 0; }
        set { }
    }
    public int combatAttackFlat {
        get { return 0; }
        set { }
    }
    public int combatAttackMultiplier {
        get { return 0; }
        set { }
    }
    public int combatSpeedFlat {
        get { return 0; }
        set { }
    }
    public int combatSpeedMultiplier {
        get { return 0; }
        set { }
    }
    public int combatHPFlat {
        get { return 0; }
        set { }
    }
    public int combatHPMultiplier {
        get { return 0; }
        set { }
    }
    public int combatPowerFlat {
        get { return 0; }
        set { }
    }
    public int combatPowerMultiplier {
        get { return 0; }
        set { }
    }
    public PairCombatStats[] pairCombatStats {
        get { return null; }
        set { }
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
    public MONSTER_CATEGORY category {
        get { return _category; }
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
    public RACE race {
        get { return _raceSetting.race; }
    }
    public Area specificLocation {
        get { return _currentParty.specificLocation; }
    }
    public CharacterBattleOnlyTracker battleOnlyTracker {
        get { return _battleOnlyTracker; }
    }
    public Faction faction {
        get { return null; }
    }
    public Area homeArea {
        get { return _homeArea; }
    }
    //public Area home {
    //    get { return null; }
    //}
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
    public Job job {
        get { return null; }
    }
    public CharacterPortrait characterPortrait {
        get { return _characterPortrait; }
    }
    //public HiddenDesire hiddenDesire {
    //    get { return null; }
    //}
    public Weapon equippedWeapon {
        get { return null; }
    }
    public Armor equippedArmor {
        get { return null; }
    }
    public Item equippedAccessory {
        get { return null; }
    }
    public Item equippedConsumable {
        get { return null; }
    }
    public Minion minion {
        get { return _minion; }
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
    public List<ItemDrop> itemDrops {
        get { return _itemDrops; }
    }
    public List<Item> inventory {
        get { return null; }
    }
    //public List<CharacterAttribute> attributes {
    //    get { return null; }
    //}
    public List<Log> history {
        get { return null; }
    }
    public List<Trait> traits {
        get { return null; }
    }
    public List<Interaction> currentInteractions {
        get { return _currentInteractions; }
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
    public Dictionary<Character, Relationship> relationships {
        get { return null; }
    }
    public Party ownParty {
        get { return _ownParty; }
    }
    public Party currentParty {
        get { return _currentParty; }
    }
    public Dictionary<STAT, float> buffs {
        get { return _buffs; }
    }
    public PlayerCharacterItem playerCharacterItem {
        get { return _playerCharacterItem; }
    }
    #endregion

    //public Monster (Monster data): this() {
    //    _name = data.name;
    //    _type = data.type;
    //    _category = data.category;
    //    _level = data.level;
    //    _experienceDrop = data.experienceDrop;
    //    _maxHP = data._maxHP;
    //    _maxSP = data.maxSP;
    //    _attackPower = data._attackPower;
    //    _speed = data._speed;
    //    _dodgeChance = data.dodgeChance;
    //    _hitChance = data.hitChance;
    //    _critChance = data.critChance;
    //    _isSleepingOnSpawn = data.isSleepingOnSpawn;

    //}
    //public Monster() {

    //}

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
        newMonster._dodgeChance = this._dodgeChance;
        newMonster._hitChance = this._hitChance;
        newMonster._critChance = this._critChance;
        newMonster._isSleepingOnSpawn = this._isSleepingOnSpawn;
        newMonster._startingArmyCount = this._startingArmyCount;
        newMonster._portraitSettings = this._portraitSettings.CreateNewCopy();
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
        //this._attackPower = monsterComponent.attackPower;
        //this._speed = monsterComponent.speed;
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
        this._dodgeChance = float.Parse(MonsterPanelUI.Instance.dodgeInput.text);
        this._hitChance = float.Parse(MonsterPanelUI.Instance.hitInput.text);
        this._critChance = float.Parse(MonsterPanelUI.Instance.critInput.text);
        this._startingArmyCount = int.Parse(MonsterPanelUI.Instance.armyCountInput.text);
        this._isSleepingOnSpawn = MonsterPanelUI.Instance.isSleepingOnSpawnToggle.isOn;
        this._skillNames = MonsterPanelUI.Instance.allSkills;
        this._elementChanceWeaknesses = new List<ElementChance>();
        this._elementChanceResistances = new List<ElementChance>();
        this._itemDrops = MonsterPanelUI.Instance.itemDrops;
    }

    public virtual void ConstructMonsterData() {
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
        if (CharacterManager.Instance != null) {
            _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(RACE.HUMANS, GENDER.MALE);
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
    public void Death(string cause = "normal") {
        _isDead = true;
        //Messenger.RemoveListener<BaseLandmark>(Signals.DESTROY_LANDMARK, OnDestroyLandmark);
        Messenger.Broadcast(Signals.MONSTER_DEATH, this);
        if(_currentParty.id != _ownParty.id) {
            //_currentParty.RemoveCharacter(this);
        }
        _ownParty.PartyDeath();
        MonsterManager.Instance.allMonsters.Remove(this);

        GameObject.Destroy(_characterPortrait.gameObject);
        _characterPortrait = null;
    }
    private float GetComputedPower() {
        float compPower = 0f;
        //for (int i = 0; i < currentParty.icharacters.Count; i++) {
        //    compPower += currentParty.icharacters[i].attackPower;
        //}
        return compPower;
    }
    //private List<Skill> GetGeneralSkills() {
    //    List<Skill> allGeneralSkills = new List<Skill>();
    //    foreach (Skill skill in SkillManager.Instance.generalSkills.Values) {
    //        if(skill is FleeSkill) {
    //            continue;
    //        }
    //        allGeneralSkills.Add(skill.CreateNewCopy());
    //    }
    //    return allGeneralSkills;
    //}
    private void ConstructSkills() {
        _skills = new List<Skill>();
        string path = string.Empty;
        //path = Utilities.dataPath + "Skills/GENERAL/";
        //string[] directories = Directory.GetDirectories(path);
        //for (int i = 0; i < directories.Length; i++) {
        //    string skillType = new DirectoryInfo(directories[i]).Name;
        //    SKILL_TYPE currSkillType = (SKILL_TYPE) System.Enum.Parse(typeof(SKILL_TYPE), skillType);
        //    string[] files = Directory.GetFiles(directories[i], "*.json");
        //    for (int j = 0; j < files.Length; j++) {
        //        string dataAsJson = File.ReadAllText(files[j]);
        //        switch (currSkillType) {
        //            case SKILL_TYPE.ATTACK:
        //            AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill>(dataAsJson);
        //            _skills.Add(attackSkill);
        //            break;
        //            case SKILL_TYPE.HEAL:
        //            HealSkill healSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
        //            _skills.Add(healSkill);
        //            break;
        //            case SKILL_TYPE.OBTAIN_ITEM:
        //            ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
        //            _skills.Add(obtainSkill);
        //            break;
        //            case SKILL_TYPE.FLEE:
        //            break;
        //            case SKILL_TYPE.MOVE:
        //            MoveSkill moveSkill = JsonUtility.FromJson<MoveSkill>(dataAsJson);
        //            _skills.Add(moveSkill);
        //            break;
        //        }
        //    }
        //}
        for (int i = 0; i < _skillNames.Count; i++) {
            path = Utilities.dataPath + "Skills/" + _skillNames[i] + ".json";
            Skill skill = JsonUtility.FromJson<Skill>(System.IO.File.ReadAllText(path));
            _skills.Add(skill);
        }
    }
    public void SetSleeping(bool state) {
        _isSleeping = state;
    }
    #endregion

    #region Interface
    public void SetName(string name) {
        _name = name;
    }
    private void BaseInitialize() {
        _isDead = false;
        _raceSetting = RaceManager.Instance.racesDictionary[_type.ToString()].CreateNewCopy();
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        _currentInteractions = new List<Interaction>();
        //characterIntel = new CharacterIntel(this);
        if (_skills == null) {
            _skills = new List<Skill>();
        }
        //_skills.AddRange(GetGeneralSkills());
        ResetToFullHP();
        //ResetToFullSP();
        SetCharacterColor(Color.red);
        SetSleeping(_isSleepingOnSpawn);
        uiData = new CharacterUIData();
        //ConstructBuffs();
        //ConstructMiscActions();

#if !WORLD_CREATION_TOOL
        //GameObject portraitGO = UIManager.Instance.InstantiateUIObject(CharacterManager.Instance.characterPortraitPrefab.name, UIManager.Instance.characterPortraitsParent);
        //_characterPortrait = portraitGO.GetComponent<CharacterPortrait>();
        //_characterPortrait.GeneratePortrait(this, 36);
        //portraitGO.SetActive(false);
#endif

        //Messenger.AddListener<BaseLandmark>(Signals.DESTROY_LANDMARK, OnDestroyLandmark);
    }
    private void BaseInitializeSim() {
        _isDead = false;
        _raceSetting = JsonUtility.FromJson<RaceSetting>(System.IO.File.ReadAllText(Utilities.dataPath + "RaceSettings/" + _type.ToString() +".json"));
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        _currentInteractions = new List<Interaction>();
        ResetToFullHP();
        //ResetToFullSP();
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
    public virtual void AdjustHP(int amount, Character killer = null) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, maxHP);
        if (previous != this._currentHP) {
            if (this._currentHP == 0) {
                FaintOrDeath(killer);
            }
        }
    }
    public void SetHP(int amount) {
        _currentHP = amount;
    }
    public void SetMaxHP(int amount) {
        //int previousMaxHP = maxHP;
        _maxHP = amount;
        //int currentMaxHP = maxHP; 
        //if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
        //    _currentHP = currentMaxHP;
        //}
    }
    public void AdjustMaxHP(int amount) {
        //int previousMaxHP = maxHP;
        _maxHP += amount;
        //int currentMaxHP = maxHP;
        //if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
        //    _currentHP = currentMaxHP;
        //}
    }
    public void FaintOrDeath(Character killer) {
        if (CombatSimManager.Instance == null) {
            //if (_ownParty.currentCombat != null) {
            //    _ownParty.currentCombat.CharacterDeath(this, killer);
            //}
            Death();
        } else {
            DeathSim();
        }
    }
    public void ResetToFullHP() {
        //SetHP(maxHP);
    }
    public void ResetToFullSP() {
        AdjustSP(_maxSP);
    }
    //public void EverydayAction() {
    //    //No daily/tick action
    //}
    public void SetHome(Area newHome) {
        this._homeArea = newHome;
    }
    public Party CreateOwnParty() {
        //if (_ownParty != null) {
        //    _ownParty.RemoveCharacter(this);
        //}
        //MonsterParty newParty = new MonsterParty();
        //SetOwnedParty(newParty);
        //newParty.AddCharacter(this);
        //return newParty;
        return null;
    }
    public void SetOwnedParty(Party party) {
        _ownParty = party as MonsterParty;
    }
    //public CharacterAttribute AddAttribute(ATTRIBUTE tag) {
    //    //No tag assignment
    //    return null;
    //}
    public void AddHistory(Log log) {
        //No history
        Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
    }
    public void DeathSim() {
        _isDead = true;
        CombatSimManager.Instance.currentCombat.CharacterDeath(this);
    }
    public void SetMode(MODE mode) {
        _currentMode = mode;
    }
    public void EnableDisableSkills(Combat combat) {
        //bool isAllAttacksInRange = true;
        //bool isAttackInRange = false;

        //Body part skills / general skills
        for (int i = 0; i < this._skills.Count; i++) {
            Skill skill = this._skills[i];
            skill.isEnabled = true;

            if (skill is FleeSkill) {
                skill.isEnabled = false;
                continue;
                //if (this.currentHP >= (this.maxHP / 2)) {
                //    skill.isEnabled = false;
                //    continue;
                //}
            }
        }
    }
    public void EnableDisableSkills(CombatSim combat) {
        for (int i = 0; i < this._skills.Count; i++) {
            Skill skill = this._skills[i];
            skill.isEnabled = true;
            if (skill is FleeSkill) {
                skill.isEnabled = false;
                continue;
                //if (this.currentHP >= (this.maxHP / 2)) {
                //    skill.isEnabled = false;
                //    continue;
                //}
            }
        }
    }
    public void SetCurrentParty(Party party) {
        _currentParty = party;
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
    public void OnAddedToPlayer() {
        ownParty.specificLocation.RemoveCharacterFromLocation(ownParty);
        PlayerManager.Instance.player.playerArea.AddCharacterToLocation(ownParty, null, true);
    }
    public bool InviteToParty(Character inviter) {
        return false;
    }
    public bool IsInParty() {
        if (currentParty.characters.Count > 1) {
            return true; //if the character is in a party that has more than 1 characters
        }
        return false;
    }
    public bool IsInOwnParty() {
        return true;
    }
    //public CharacterAttribute GetAttribute(string attribute) {
    //    return null;
    //}
    public void Assassinate(Character assassin) {
        Debug.Log(assassin.name + " assassinated " + name);
        Death();
    }
    public void SetLevel(int amount) {
        //Not applicable
    }
    public void LevelUp() {
        //Not applicable
    }
    public void LevelUp(int amount) {
        //Not applicable
    }
    public bool AddTrait(Trait combatAttribute, Character responsibleCharacter = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null) {
        //Not applicable
        return false;
    }
    public bool RemoveTrait(Trait combatAttribute, bool triggerOnRemove = true) {
        //Not applicable
        return false;
    }
    public Trait GetTrait(string name) {
        return null;
    }

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

    #region IInteractable
    public void SetIsBeingInspected(bool state) {
        _isBeingInspected = state;
    }
    public void SetHasBeenInspected(bool state) {
        _hasBeenInspected = true;
    }
    public void EndedInspection() {
        //uiData.UpdateData(this);
    }
    public void AddInteraction(Interaction interaction) {
        _currentInteractions.Add(interaction);
        //interaction.Initialize();
        //Messenger.Broadcast(Signals.ADDED_INTERACTION, this as IInteractable, interaction);
    }
    public void RemoveInteraction(Interaction interaction) {
        if (_currentInteractions.Remove(interaction)) {
            //Messenger.Broadcast(Signals.REMOVED_INTERACTION, this as IInteractable, interaction);
        }
    }
    #endregion

    #region Minion
    public void SetMinion(Minion minion) {
        _minion = minion;
    }
    #endregion

    #region Buffs
    public void ConstructBuffs() {
        _buffs = new Dictionary<STAT, float>();
        STAT[] stats = Utilities.GetEnumValues<STAT>();
        for (int i = 0; i < stats.Length; i++) {
            _buffs.Add(stats[i], 0f);
        }
    }
    public void AddBuff(Buff buff) {
        if (_buffs.ContainsKey(buff.buffedStat)) {
            _buffs[buff.buffedStat] += buff.percentage;
        }
    }
    public void RemoveBuff(Buff buff) {
        if (_buffs.ContainsKey(buff.buffedStat)) {
            _buffs[buff.buffedStat] -= buff.percentage;
        }
    }
    #endregion

    public void SetPlayerCharacterItem(PlayerCharacterItem item) {
        _playerCharacterItem = item;
    }

    #region Interaction Generation
    public void DisableInteractionGeneration() {
        Messenger.RemoveListener(Signals.TICK_STARTED, DailyInteractionGeneration);
    }
    public void AddInteractionWeight(INTERACTION_TYPE type, int weight) {
        interactionWeights.AddElement(type, weight);
    }
    public void RemoveInteractionFromWeights(INTERACTION_TYPE type, int weight) {
        interactionWeights.RemoveElement(type);
    }
    public void SetDailyInteractionGenerationTick() {
        _currentInteractionTick = UnityEngine.Random.Range(1, GameManager.daysPerMonth + 1);
    }
    public void DailyInteractionGeneration() {
        if (_currentInteractionTick == GameManager.Instance.days) {
            GenerateDailyInteraction();
            SetDailyInteractionGenerationTick();
        }
    }
    public void GenerateDailyInteraction() {
        if (!IsInOwnParty() || ownParty.icon.isTravelling) {
            return; //if this character is not in own party, is a defender or is travelling, do not generate interaction
        }
        if (eventTriggerWeights.PickRandomElementGivenWeights()) {
            WeightedDictionary<INTERACTION_TYPE> validInteractions = GetValidInteractionWeights();
            if (validInteractions.GetTotalOfWeights() > 0) {
                INTERACTION_TYPE chosenInteraction = validInteractions.PickRandomElementGivenWeights();
                //create interaction of type
                Interaction createdInteraction = InteractionManager.Instance.CreateNewInteraction(chosenInteraction, this.specificLocation);
                if (createdInteraction != null) {
                    (this.specificLocation.coreTile.landmarkOnTile).AddInteraction(createdInteraction);
                }
            }
        }
    }
    private WeightedDictionary<INTERACTION_TYPE> GetValidInteractionWeights() {
        WeightedDictionary<INTERACTION_TYPE> weights = new WeightedDictionary<INTERACTION_TYPE>();
        //foreach (KeyValuePair<INTERACTION_TYPE, int> kvp in interactionWeights.dictionary) {
        //    if (InteractionManager.Instance.CanCreateInteraction(kvp.Key, this)) {
        //        weights.AddElement(kvp.Key, kvp.Value);
        //    }
        //}
        return weights;
    }
    #endregion
}
