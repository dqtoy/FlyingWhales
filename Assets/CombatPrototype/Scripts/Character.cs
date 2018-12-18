using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

public class Character : ICharacter, ILeader, IInteractable, IQuestGiver {
    public delegate void OnCharacterDeath();
    public OnCharacterDeath onCharacterDeath;

    public delegate void DailyAction();
    public DailyAction onDailyAction;

    protected string _name;
    protected string _characterColorCode;
    protected int _id;
    protected int _gold;
    protected int _currentInteractionTick;
    protected int _lastLevelUpDay;
    protected float _actRate;
    protected bool _isDead;
    protected bool _isFainted;
    protected bool _isInCombat;
    protected bool _doNotDisturb;
    protected bool _isBeingInspected;
    protected bool _hasBeenInspected;
    protected bool _alreadyTargetedByGrudge;
    protected bool _isLeader;
    protected GENDER _gender;
    protected MODE _currentMode;
    protected CharacterClass _characterClass;
    protected RaceSetting _raceSetting;
    protected CharacterRole _role;
    protected Job _job;
    protected Faction _faction;
    protected CharacterParty _ownParty;
    protected CharacterParty _currentParty;
    protected BaseLandmark _homeLandmark;
    protected BaseLandmark _workplace;
    protected Region _currentRegion;
    protected Weapon _equippedWeapon;
    protected Armor _equippedArmor;
    protected Item _equippedAccessory;
    protected Item _equippedConsumable;
    protected CharacterBattleTracker _battleTracker;
    protected CharacterBattleOnlyTracker _battleOnlyTracker;
    protected PortraitSettings _portraitSettings;
    protected CharacterPortrait _characterPortrait;
    protected Color _characterColor;
    protected CharacterAction _genericWorkAction;
    protected Minion _minion;
    protected Interaction _forcedInteraction;
    protected PairCombatStats[] _pairCombatStats;
    protected List<Item> _inventory;
    protected List<Skill> _skills;
    protected List<CharacterAttribute> _attributes;
    protected List<Log> _history;
    protected List<BaseLandmark> _exploredLandmarks; //Currently only storing explored landmarks that were explored for the last 6 months
    protected List<Trait> _traits;
    protected CharacterActionQueue<ActionQueueItem> _actionQueue;
    protected List<CharacterAction> _miscActions;
    protected List<Interaction> _currentInteractions;
    //protected Dictionary<Character, Relationship> _relationships;
    protected Dictionary<ELEMENT, float> _elementalWeaknesses;
    protected Dictionary<ELEMENT, float> _elementalResistances;
    protected Dictionary<Character, List<string>> _traceInfo;
    protected PlayerCharacterItem _playerCharacterItem;

    //Stats
    protected SIDES _currentSide;
    protected int _currentHP;
    protected int _maxHP;
    protected int _currentRow;
    protected int _level;
    protected int _experience;
    protected int _maxExperience;
    protected int _sp;
    protected int _maxSP;
    protected int _attackPower;
    protected int _speed;
    protected int _combatBaseAttack;
    protected int _combatBaseSpeed;
    protected int _combatBaseHP;
    protected int _combatAttackFlat;
    protected int _combatAttackMultiplier;
    protected int _combatSpeedFlat;
    protected int _combatSpeedMultiplier;
    protected int _combatHPFlat;
    protected int _combatHPMultiplier;
    protected int _combatPowerFlat;
    protected int _combatPowerMultiplier;

    public CharacterSchedule schedule { get; private set; }
    public Quest currentQuest { get; private set; }
    public CharacterEventSchedule eventSchedule { get; private set; }
    public CharacterUIData uiData { get; private set; }
    public Area defendingArea { get; private set; }
    public MORALITY morality { get; private set; }
    public CharacterToken characterToken { get; private set; }
    public WeightedDictionary<INTERACTION_TYPE> interactionWeights { get; private set; }
    public WeightedDictionary<bool> eventTriggerWeights { get; private set; }
    public List<SpecialToken> tokenInventory { get; private set; }

    private Dictionary<STAT, float> _buffs;

    public Dictionary<int, Combat> combatHistory;

    #region getters / setters
    public string firstName {
        get { return name.Split(' ')[0]; }
    }
    public virtual string name {
        get {
            //if(_minion != null) {
            //    return _minion.name;
            //}
            return this._name;
        }
    }
    public string coloredName {
        get { return "<color=#" + this._characterColorCode + ">" + name + "</color>"; }
    }
    public string urlName {
        get { return "<link=" + '"' + this._id.ToString() + "_character" + '"' + ">" + name + "</link>"; }
    }
    public string coloredUrlName {
        get { return "<link=" + '"' + this._id.ToString() + "_character" + '"' + ">" + "<color=#" + this._characterColorCode + ">" + name + "</color></link>"; }
    }
    public int id {
        get { return _id; }
    }
    public GENDER gender {
        get { return _gender; }
    }
    public MODE currentMode {
        get { return _currentMode; }
    }
    public RACE race {
        get { return _raceSetting.race; }
    }
    public List<CharacterAttribute> attributes {
        get { return _attributes; }
    }
    //public Dictionary<Character, Relationship> relationships {
    //    get { return _relationships; }
    //}
    public CharacterClass characterClass {
        get { return this._characterClass; }
    }
    public RaceSetting raceSetting {
        get { return _raceSetting; }
    }
    public CharacterRole role {
        get { return _role; }
    }
    public Job job {
        get { return _job; }
    }
    public Faction faction {
        get { return _faction; }
    }
    public virtual Party ownParty {
        get { return _ownParty; }
    }
    public CharacterParty party {
        get { return _ownParty; }
    }
    public virtual Party currentParty {
        get { return _currentParty; }
    }
    public CharacterAction genericWorkAction {
        get { return _genericWorkAction; }
    }
    public HexTile currLocation {
        get { return (_currentParty.specificLocation != null ? _currentParty.specificLocation.tileLocation : null); }
    }
    public ILocation specificLocation {
        get { return _currentParty.specificLocation; }
    }
    //public List<BodyPart> bodyParts {
    //    get { return this._bodyParts; }
    //}
    //public List<Item> equippedItems {
    //    get { return this._equippedItems; }
    //}
    public List<Item> inventory {
        get { return this._inventory; }
    }
    public List<Skill> skills {
        get { return this._skills; }
    }
    public int currentRow {
        get { return this._currentRow; }
    }
    public SIDES currentSide {
        get { return this._currentSide; }
    }
    public bool isDead {
        get { return this._isDead; }
    }
    public bool isFainted {
        get { return this._isFainted; }
    }
    public Color characterColor {
        get { return _characterColor; }
    }
    public string characterColorCode {
        get { return _characterColorCode; }
    }
    public BaseLandmark homeLandmark {
        get { return _homeLandmark; }
    }
    public BaseLandmark workplace {
        get { return _workplace; }
    }
    public float remainingHP { //Percentage of remaining HP this character has
        get { return (float) currentHP / (float) maxHP; }
    }
    public int remainingHPPercent {
        get { return (int) (remainingHP * 100); }
    }
    public List<Log> history {
        get { return this._history; }
    }
    public int gold {
        get { return _gold; }
    }
    public List<BaseLandmark> exploredLandmarks {
        get { return _exploredLandmarks; }
    }
    public bool isInCombat {
        get {
            return _isInCombat;
        }
    }
    public bool isFactionless { //is the character part of the neutral faction? or no faction?
        get {
            if (FactionManager.Instance.neutralFaction == null) {
                if (faction != null) {
                    return false;
                } else {
                    return true;
                }
            } else {
                if (faction != null && FactionManager.Instance.neutralFaction.id == faction.id) {
                    return true;
                } else {
                    return false;
                }
            }
        }
    }
    public Dictionary<Character, List<string>> traceInfo {
        get { return _traceInfo; }
    }
    public PortraitSettings portraitSettings {
        get { return _portraitSettings; }
    }
    public CharacterPortrait characterPortrait {
        get { return _characterPortrait; }
    }
    public int level {
        get { return _level; }
    }
    public int currentSP {
        get { return _sp; }
    }
    public int maxSP {
        get { return _maxSP; }
    }
    public int experience {
        get { return _experience; }
    }
    public int maxExperience {
        get { return _maxExperience; }
    }
    public int speed {
        get { return _speed + GetModifiedSpeed(); }
    }
    public int attackPower {
        get { return _attackPower + GetModifiedAttack(); }
    }
    public int hp {
        get { return _maxHP + GetModifiedHP(); }
    }
    public int maxHP {
        get { return this._maxHP; }
    }
    public int combatBaseAttack {
        get { return _combatBaseAttack; }
        set { _combatBaseAttack = value; }
    }
    public int combatBaseSpeed {
        get { return _combatBaseSpeed; }
        set { _combatBaseSpeed = value; }
    }
    public int combatBaseHP {
        get { return _combatBaseHP; }
        set { _combatBaseHP = value; }
    }
    public int combatAttackFlat {
        get { return _combatAttackFlat; }
        set { _combatAttackFlat = value; }
    }
    public int combatAttackMultiplier {
        get { return _combatAttackMultiplier; }
        set { _combatAttackMultiplier = value; }
    }
    public int combatSpeedFlat {
        get { return _combatSpeedFlat; }
        set { _combatSpeedFlat = value; }
    }
    public int combatSpeedMultiplier {
        get { return _combatSpeedMultiplier; }
        set { _combatSpeedMultiplier = value; }
    }
    public int combatHPFlat {
        get { return _combatHPFlat; }
        set { _combatHPFlat = value; }
    }
    public int combatHPMultiplier {
        get { return _combatHPMultiplier; }
        set { _combatHPMultiplier = value; }
    }
    public int combatPowerFlat {
        get { return _combatPowerFlat; }
        set { _combatPowerFlat = value; }
    }
    public int combatPowerMultiplier {
        get { return _combatPowerMultiplier; }
        set { _combatPowerMultiplier = value; }
    }
    public int currentHP {
        get { return this._currentHP; }
    }
    public PairCombatStats[] pairCombatStats {
        get { return _pairCombatStats; }
        set { _pairCombatStats = value; }
    }
    public Dictionary<ELEMENT, float> elementalWeaknesses {
        get { return _elementalWeaknesses; }
    }
    public Dictionary<ELEMENT, float> elementalResistances {
        get { return _elementalResistances; }
    }
    public float actRate {
        get { return _actRate; }
        set { _actRate = value; }
    }
    public Weapon equippedWeapon {
        get { return _equippedWeapon; }
    }
    public Armor equippedArmor {
        get { return _equippedArmor; }
    }
    public Item equippedAccessory {
        get { return _equippedAccessory; }
    }
    public Item equippedConsumable {
        get { return _equippedConsumable; }
    }
    public CharacterBattleTracker battleTracker {
        get { return _battleTracker; }
    }
    public CharacterBattleOnlyTracker battleOnlyTracker {
        get { return _battleOnlyTracker; }
    }
    public float computedPower {
        get { return GetComputedPower(); }
    }
    public ICHARACTER_TYPE icharacterType {
        get { return ICHARACTER_TYPE.CHARACTER; }
    }
    public List<CharacterAction> miscActions {
        get { return _miscActions; }
    }
    public Minion minion {
        get { return _minion; }
    }
    public CharacterActionQueue<ActionQueueItem> actionQueue {
        get { return _actionQueue; }
    }
    public bool doNotDisturb {
        get { return _doNotDisturb; }
    }
    public bool isBeingInspected {
        get { return _isBeingInspected; }
    }
    public bool hasBeenInspected {
        get { return _hasBeenInspected; }
    }
    public bool alreadyTargetedByGrudge {
        get { return _alreadyTargetedByGrudge; }
    }
    public bool isLeader {
        get { return _isLeader; }
    }
    public IObject questGiverObj {
        get { return currentParty.icharacterObject; }
    }
    public QUEST_GIVER_TYPE questGiverType {
        get { return QUEST_GIVER_TYPE.CHARACTER; }
    }
    public bool isDefender {
        get { return defendingArea != null; }
    }
    public List<Trait> traits {
        get { return _traits; }
    }
    public List<Interaction> currentInteractions {
        get { return _currentInteractions; }
    }
    public Dictionary<STAT, float> buffs {
        get { return _buffs; }
    }
    public PlayerCharacterItem playerCharacterItem {
        get { return _playerCharacterItem; }
    }
    #endregion

    public Character(string className, RACE race, GENDER gender) : this() {
        _id = Utilities.SetID(this);
        //_characterClass = CharacterManager.Instance.classesDictionary[className].CreateNewCopy();
        _raceSetting = RaceManager.Instance.racesDictionary[race.ToString()].CreateNewCopy();
        if (CharacterManager.Instance.classesDictionary.ContainsKey(className)) {
            AssignClass(CharacterManager.Instance.classesDictionary[className]);
        } else {
            throw new Exception("There is no class named " + className + " but it is being assigned to " + this.name);
        }
        _gender = gender;
        _name = RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender);
        if (this is CharacterArmyUnit) {
            _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(RACE.HUMANS, GENDER.MALE);
        } else {
            _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
        }
        if (_characterClass.roleType != CHARACTER_ROLE.NONE) {
            AssignRole(_characterClass.roleType);
        }
        SetMorality(MORALITY.GOOD);
        //_skills = GetGeneralSkills();

        //_bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
        //ConstructBodyPartDict(_raceSetting.bodyParts);

        //AllocateStatPoints(10);
        AllocateStats();
        SetTraitsFromRace();
        //CharacterSetup setup = CombatManager.Instance.GetBaseCharacterSetup(className);
        //if(setup != null) {
        //    GenerateSetupAttributes(setup);
        //    if(setup.optionalRole != CHARACTER_ROLE.NONE) {
        //        AssignRole(setup.optionalRole);
        //    }
        //}
    }
    public Character(CharacterSaveData data) : this() {
        _id = Utilities.SetID(this, data.id);
        //_characterClass = CharacterManager.Instance.classesDictionary[data.className].CreateNewCopy();
        _raceSetting = RaceManager.Instance.racesDictionary[data.race.ToString()].CreateNewCopy();
        AssignClass(CharacterManager.Instance.classesDictionary[data.className]);
        _gender = data.gender;
        _name = data.name;
        //LoadRelationships(data.relationshipsData);
        _portraitSettings = data.portraitSettings;
        if (_characterClass.roleType != CHARACTER_ROLE.NONE) {
            AssignRole(_characterClass.roleType);
        }
        SetMorality(data.morality);
#if !WORLD_CREATION_TOOL
        GameObject portraitGO = UIManager.Instance.InstantiateUIObject(CharacterManager.Instance.characterPortraitPrefab.name, UIManager.Instance.characterPortraitsParent);
        _characterPortrait = portraitGO.GetComponent<CharacterPortrait>();
        _characterPortrait.GeneratePortrait(this, data.role);
        portraitGO.SetActive(false);
#endif

        //_bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
        //ConstructBodyPartDict(_raceSetting.bodyParts);
        //_skills = GetGeneralSkills();
        //_skills = new List<Skill>();
        //_skills.Add(_characterClass.skill);
        //_skills.AddRange (GetBodyPartSkills ());
        if (data.attributes != null) {
            AddAttributes(data.attributes);
        }
        //GenerateSetupTags(baseSetup);

        //AllocateStatPoints(10);
        AllocateStats();
        //EquipItemsByClass();
        //SetTraitsFromClass();
        SetTraitsFromRace();
        //EquipPreEquippedItems(baseSetup);
        CharacterSetup setup = CombatManager.Instance.GetBaseCharacterSetup(data.className);
        if (setup != null) {
            GenerateSetupAttributes(setup);
            //if (setup.optionalRole != CHARACTER_ROLE.NONE) {
            //    AssignRole(setup.optionalRole);
            //}
        }
        //DetermineAllowedMiscActions();
    }
    public Character() {
        _attributes = new List<CharacterAttribute>();
        _exploredLandmarks = new List<BaseLandmark>();
        _isDead = false;
        _isFainted = false;
        //_isDefeated = false;
        //_isIdle = false;
        _traceInfo = new Dictionary<Character, List<string>>();
        _history = new List<Log>();
        //_questData = new List<CharacterQuestData>();
        _actionQueue = new CharacterActionQueue<ActionQueueItem>(this);
        //previousActions = new Dictionary<CharacterTask, string>();
        //_relationships = new Dictionary<Character, Relationship>();
        _genericWorkAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.WORKING);
        _traits = new List<Trait>();

        //_actionData = new ActionData(this);


        //RPG
        _level = 1;
        _experience = 0;
        _elementalWeaknesses = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
        _elementalResistances = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
        _battleTracker = new CharacterBattleTracker();
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        //_equippedItems = new List<Item>();
        _inventory = new List<Item>();
        combatHistory = new Dictionary<int, Combat>();
        _currentInteractions = new List<Interaction>();
        eventSchedule = new CharacterEventSchedule(this);
        uiData = new CharacterUIData();
        characterToken = new CharacterToken(this);
        tokenInventory = new List<SpecialToken>();
        interactionWeights = new WeightedDictionary<INTERACTION_TYPE>();
        eventTriggerWeights = new WeightedDictionary<bool>();
        eventTriggerWeights.AddElement(true, 200); //Hard coded for now
        eventTriggerWeights.AddElement(false, 1000);

        //AllocateStats();
        //EquipItemsByClass();
        //ConstructBuffs();
        GetRandomCharacterColor();
        ConstructDefaultMiscActions();
        //_combatHistoryID = 0;
#if !WORLD_CREATION_TOOL
        SetDailyInteractionGenerationTick();
#endif
        SubscribeToSignals();
    }
    public void Initialize() { }

    #region Signals
    private void SubscribeToSignals() {
        //Messenger.AddListener<Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        //Messenger.AddListener(Signals.HOUR_ENDED, EverydayAction);
        //Messenger.AddListener<StructureObj, int>("CiviliansDeath", CiviliansDiedReduceSanity);
        //Messenger.AddListener<Character>(Signals.CHARACTER_REMOVED, RemoveRelationshipWith);
        //Messenger.AddListener<Area>(Signals.AREA_DELETED, OnAreaDeleted);
        Messenger.AddListener<BaseLandmark>(Signals.DESTROY_LANDMARK, OnDestroyLandmark);
        Messenger.AddListener(Signals.DAY_STARTED, DailyInteractionGeneration);
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, RemoveRelationshipWith);
    }
    public void UnsubscribeSignals() {
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        //Messenger.RemoveListener(Signals.HOUR_ENDED, EverydayAction);
        //Messenger.RemoveListener<StructureObj, int>("CiviliansDeath", CiviliansDiedReduceSanity);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_REMOVED, RemoveRelationshipWith);
        //Messenger.RemoveListener<Area>(Signals.AREA_DELETED, OnAreaDeleted);
        Messenger.RemoveListener<BaseLandmark>(Signals.DESTROY_LANDMARK, OnDestroyLandmark);
        Messenger.RemoveListener(Signals.DAY_STARTED, DailyInteractionGeneration);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, RemoveRelationshipWith);
        if (Messenger.eventTable.ContainsKey(Signals.DAY_ENDED)) {
            Messenger.RemoveListener(Signals.DAY_ENDED, EventEveryTick);
        }
    }
    #endregion

    private void AllocateStats() {
        _attackPower = _raceSetting.baseAttackPower;
        _speed = _raceSetting.baseSpeed;
        SetMaxHP(_raceSetting.baseHP);
        _maxSP = _characterClass.baseSP;
    }
    //      private void AllocateStatPoints(int statAllocation){
    //          _baseStrength = 0;
    //          _baseIntelligence = 0;
    //          _baseAgility = 0;
    //          _baseVitality = 0;

    //	WeightedDictionary<string> statWeights = new WeightedDictionary<string> ();
    //	statWeights.AddElement ("strength", (int) _characterClass.strWeightAllocation);
    //	statWeights.AddElement ("intelligence", (int) _characterClass.intWeightAllocation);
    //	statWeights.AddElement ("agility", (int) _characterClass.agiWeightAllocation);
    //	statWeights.AddElement ("vitality", (int) _characterClass.vitWeightAllocation);

    //	if(statWeights.GetTotalOfWeights() > 0){
    //		string chosenStat = string.Empty;
    //		for (int i = 0; i < statAllocation; i++) {
    //			chosenStat = statWeights.PickRandomElementGivenWeights ();
    //			if (chosenStat == "strength") {
    //				_baseStrength += 1;
    //			}else if (chosenStat == "intelligence") {
    //				_baseIntelligence += 1;
    //			}else if (chosenStat == "agility") {
    //				_baseAgility += 1;
    //			}else if (chosenStat == "vitality") {
    //				_baseVitality += 1;
    //			}
    //		}
    //	}
    //}
    //Enables or Disables skills based on skill requirements
    public void EnableDisableSkills(Combat combat) {
        //bool isAllAttacksInRange = true;
        //bool isAttackInRange = false;

        //Body part skills / general skills
        for (int i = 0; i < this._skills.Count; i++) {
            Skill skill = this._skills[i];
            skill.isEnabled = true;

            //            if (skill is AttackSkill){
            //                AttackSkill attackSkill = skill as AttackSkill;
            //                if(attackSkill.spCost > _sp) {
            //                    skill.isEnabled = false;
            //                    continue;
            //                }
            //} else 
            if (skill is FleeSkill) {
                skill.isEnabled = false;
                //if (this.currentHP >= (this.maxHP / 2)) {
                //    skill.isEnabled = false;
                //    continue;
                //}
            }
        }

        //Character class skills
        //if(_equippedWeapon != null) {
        //    for (int i = 0; i < _level; i++) {
        //        if(i < _characterClass.skillsPerLevel.Count) {
        //            if (_characterClass.skillsPerLevel[i] != null) {
        //                for (int j = 0; j < _characterClass.skillsPerLevel[i].Length; j++) {
        //                    Skill skill = _characterClass.skillsPerLevel[i][j];
        //                    skill.isEnabled = true;

        //                    //Check for allowed weapon types
        //                    if (skill.allowedWeaponTypes != null) {
        //                        for (int k = 0; k < skill.allowedWeaponTypes.Length; k++) {
        //                            if (!skill.allowedWeaponTypes.Contains(_equippedWeapon.weaponType)) {
        //                                skill.isEnabled = false;
        //                                continue;
        //                            }
        //                        }
        //                    }

        //                    if (skill is AttackSkill) {
        //                        AttackSkill attackSkill = skill as AttackSkill;
        //                        if (attackSkill.spCost > _sp) {
        //                            skill.isEnabled = false;
        //                            continue;
        //                        }
        //                    }
        //                }
        //            }
        //        } else {
        //            break;
        //        }
        //    }

        //}

    }
    //Changes row number of this character
    public void SetRowNumber(int rowNumber) {
        this._currentRow = rowNumber;
    }
    //Changes character's side
    public void SetSide(SIDES side) {
        this._currentSide = side;
    }
    //Adjust current HP based on specified paramater, but HP must not go below 0
    //public virtual void AdjustHP(int amount, ICharacter killer = null) {
    //    int previous = this._currentHP;
    //    this._currentHP += amount;
    //    this._currentHP = Mathf.Clamp(this._currentHP, 0, maxHP);
    //    if (previous != this._currentHP) {
    //        if (this._currentHP == 0) {
    //            FaintOrDeath(killer);
    //        }
    //    }
    //}
    public void SetHP(int amount) {
        this._currentHP = amount;
    }
    private string GetFaintOrDeath() {
        return "die";
        //WeightedDictionary<string> faintDieDict = new WeightedDictionary<string> ();
        //int faintWeight = 100;
        //int dieWeight = 50;
        //if(HasTrait(TRAIT.GRITTY)){
        //	faintWeight += 50;
        //}
        //if(HasTrait(TRAIT.ROBUST)){
        //	faintWeight += 50;
        //}
        //if(HasTrait(TRAIT.FRAGILE)){
        //	dieWeight += 50;
        //}
        //faintDieDict.AddElement ("faint", 100);
        //faintDieDict.AddElement ("die", 50);

        //return faintDieDict.PickRandomElementGivenWeights ();
    }
    //public void FaintOrDeath(ICharacter killer) {
    //    string pickedWeight = GetFaintOrDeath();
    //    if (pickedWeight == "faint") {
    //        if (currentParty.currentCombat == null) {
    //            Faint();
    //        } else {
    //            currentParty.currentCombat.CharacterFainted(this);
    //        }
    //    } else if (pickedWeight == "die") {
    //        if (currentParty.currentCombat != null) {
    //            currentParty.currentCombat.CharacterDeath(this, killer);
    //        }
    //        Death();
    //        //            if (this.currentCombat == null){
    //        //	Death ();
    //        //}else{
    //        //	this.currentCombat.CharacterDeath (this);
    //        //}
    //    }
    //}
    //When character will faint
    internal void Faint() {
        if (!_isFainted) {
            _isFainted = true;
            SetHP(1);
            ////Set Task to Fainted
            //Faint faintTask = new Faint(this);
            //faintTask.OnChooseTask(this)
            ; }
    }
    internal void Unfaint() {
        if (_isFainted) {
            _isFainted = false;
            SetHP(1);
        }
    }
    public void Imprison() {
        //if(_ownParty.icharacters.Count > 1) {
        //    CreateOwnParty();
        //}
        if (_ownParty.characterObject.currentState.stateName != "Imprisoned") {
            ObjectState imprisonedState = _ownParty.characterObject.GetState("Imprisoned");
            _ownParty.characterObject.ChangeState(imprisonedState);

            _ownParty.SetIsIdle(true); //this makes the character not do any action, and needs are halted
            //Do other things when imprisoned
        }
    }
    //Character's death
    public void Death() {
        if (!_isDead) {
            _isDead = true;
            UnsubscribeSignals();

            CombatManager.Instance.ReturnCharacterColorToPool(_characterColor);

            if (currentParty.specificLocation == null) {
                throw new Exception("Specific location of " + this.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }

            //if (currentParty.specificLocation != null && currentParty.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
            //    Log deathLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "death");
            //    deathLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //    AddHistory(deathLog);
            //    (currentParty.specificLocation as BaseLandmark).AddHistory(deathLog);
            //}

            //Drop all Items
            //            while (_equippedItems.Count > 0) {
            //	ThrowItem (_equippedItems [0]);
            //}
            while (_inventory.Count > 0) {
                ThrowItem(_inventory[0]);
            }

            if (IsInOwnParty()) {
                if (_ownParty.actionData.currentAction != null) {
                    _ownParty.actionData.currentAction.EndAction(_ownParty, _ownParty.actionData.currentTargetObject);
                }
            }
            _currentParty.RemoveCharacter(this);
            _ownParty.PartyDeath();
            //if (currentParty.id != _ownParty.id) {
            //}
            //_ownParty.PartyDeath();
            //Remove ActionData
            //_actionData.DetachActionData();

            //if(_home != null){
            //                //Remove character home on landmark
            //	_home.RemoveCharacterHomeOnLandmark (this);
            //}

            if (this._faction != null) {
                this._faction.RemoveCharacter(this); //remove this character from it's factions list of characters
            }

            //if (_specificLocation != null) {
            //    _specificLocation.RemoveCharacterFromLocation(this);
            //}
            //if (_avatar != null) {
            //    if (_avatar.mainCharacter.id == this.id) {
            //        DestroyAvatar();
            //    } else {
            //        _avatar.RemoveCharacter(this); //if the character has an avatar, remove it from the list of characters
            //    }
            //}
            //if (_isPrisoner){
            //	PrisonerDeath ();
            //}
            if (_role != null) {
                _role.DeathRole();
            }
            if (_homeLandmark != null) {
                _homeLandmark.RemoveCharacterHomeOnLandmark(this);
            }
            //while(_tags.Count > 0){
            //	RemoveCharacterAttribute (_tags [0]);
            //}
            //while (questData.Count != 0) {
            //    questData[0].AbandonQuest();
            //}
            //				if(Messenger.eventTable.ContainsKey("CharacterDeath")){
            //					Messenger.Broadcast ("CharacterDeath", this);
            //				}
            if (schedule != null) {
                schedule.OnOwnerDied();
            }
            if (_minion != null) {
                PlayerManager.Instance.player.RemoveMinion(_minion);
            }
            if (onCharacterDeath != null) {
                onCharacterDeath();
            }
            onCharacterDeath = null;
            Messenger.Broadcast(Signals.CHARACTER_DEATH, this);
            //if (killer != null) {
            //    Messenger.Broadcast(Signals.CHARACTER_KILLED, killer, this);
            //}
            if (_characterPortrait != null) {
                GameObject.Destroy(_characterPortrait.gameObject);
                _characterPortrait = null;
            }


            //ObjectState deadState = _characterObject.GetState("Dead");
            //_characterObject.ChangeState(deadState);

            //GameObject.Destroy(_icon.gameObject);
            //_icon = null;

            Debug.Log(this.name + " died!");
        }
    }
    public void Assassinate(Character assassin) {
        Debug.Log(assassin.name + " assassinated " + name);
        Death();
    }
    internal void AddActionOnDeath(OnCharacterDeath onDeathAction) {
        onCharacterDeath += onDeathAction;
    }
    internal void RemoveActionOnDeath(OnCharacterDeath onDeathAction) {
        onCharacterDeath -= onDeathAction;
    }

    #region Items
    //If a character picks up an item, it is automatically added to his/her inventory
    internal void PickupItem(Item item, bool broadcast = true) {
        Item newItem = item;
        if (_inventory.Contains(newItem)) {
            throw new Exception(this.name + " already has an instance of " + newItem.itemName);
        }
        this._inventory.Add(newItem);
        //newItem.SetPossessor (this);
        if (newItem.owner == null) {
            OwnItem(newItem);
        }
#if !WORLD_CREATION_TOOL
        Log obtainLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "obtain_item");
        obtainLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        obtainLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
        AddHistory(obtainLog);
        if (_ownParty.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
            _ownParty.specificLocation.tileLocation.areaOfTile.AddHistory(obtainLog);
        }
#endif
        if (broadcast) {
            Messenger.Broadcast(Signals.ITEM_OBTAINED, newItem, this);
        }
        newItem.OnItemPutInInventory(this);
    }
    internal void ThrowItem(Item item, bool addInLandmark = true) {
        if (item.isEquipped) {
            UnequipItem(item);
        }
        //item.SetPossessor (null);
        this._inventory.Remove(item);
        //item.exploreWeight = 15;
        if (addInLandmark) {
            ILocation location = _ownParty.specificLocation;
            if (location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
                BaseLandmark landmark = location as BaseLandmark;
                landmark.AddItem(item);
            }
        }
        Messenger.Broadcast(Signals.ITEM_THROWN, item, this);
    }
    internal void ThrowItem(string itemName, int quantity, bool addInLandmark = true) {
        for (int i = 0; i < quantity; i++) {
            if (HasItem(itemName)) {
                ThrowItem(GetItemInInventory(itemName), addInLandmark);
            }
        }
    }
    internal void DropItem(Item item) {
        ThrowItem(item);
        ILocation location = _ownParty.specificLocation;
        if (location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
            //BaseLandmark landmark = location as BaseLandmark;
            Log dropLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "drop_item");
            dropLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            dropLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
            dropLog.AddToFillers(location.tileLocation.areaOfTile, location.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_1);
            AddHistory(dropLog);
            location.tileLocation.areaOfTile.AddHistory(dropLog);
        }

    }
    internal void CheckForItemDrop() {
        ILocation location = _ownParty.specificLocation;
        if (location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
            if (UnityEngine.Random.Range(0, 100) < 3) {
                Dictionary<Item, Character> itemPool = new Dictionary<Item, Character>();
                List<Character> charactersToCheck = new List<Character>();
                charactersToCheck.Add(this);

                for (int i = 0; i < charactersToCheck.Count; i++) {
                    Character currCharacter = charactersToCheck[i];
                    for (int j = 0; j < currCharacter.inventory.Count; j++) {
                        itemPool.Add(currCharacter.inventory[j], currCharacter);
                    }
                }

                //Drop a random item from the pool
                if (itemPool.Count > 0) {
                    Item chosenItem = itemPool.Keys.ElementAt(UnityEngine.Random.Range(0, itemPool.Count));
                    Character owner = itemPool[chosenItem];
                    owner.DropItem(chosenItem);
                }
            }
        }
    }
    public void EquipItem(string itemName) {
        Item item = ItemManager.Instance.CreateNewItemInstance(itemName);
        if (item != null) {
            EquipItem(item);
        }
    }

    /*
        this is the real way to equip an item
        this will return a boolean whether the character successfully equipped
        the item or not.
            */
    internal bool EquipItem(Item item) {
        bool hasEquipped = false;
        if (item.itemType == ITEM_TYPE.WEAPON) {
            Weapon weapon = item as Weapon;
            hasEquipped = TryEquipWeapon(weapon);
        } else if (item.itemType == ITEM_TYPE.ARMOR) {
            Armor armor = item as Armor;
            hasEquipped = TryEquipArmor(armor);
        } else if (item.itemType == ITEM_TYPE.ACCESSORY) {
            hasEquipped = TryEquipAccessory(item);
        } else if (item.itemType == ITEM_TYPE.CONSUMABLE) {
            hasEquipped = TryEquipConsumable(item);
        }
        if (hasEquipped) {
            if (item.attributeNames != null) {
                for (int i = 0; i < item.attributeNames.Count; i++) {
                    Trait newTrait = AttributeManager.Instance.allTraits[item.attributeNames[i]];
                    AddTrait(newTrait);
                }
            }
#if !WORLD_CREATION_TOOL
            Log equipLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "equip_item");
            equipLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            equipLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
            AddHistory(equipLog);
#endif
            Messenger.Broadcast(Signals.ITEM_EQUIPPED, item, this);
        }
        return hasEquipped;
    }
    //Unequips an item of a character, whether it's a weapon, armor, etc.
    public void UnequipItem(Item item) {
        if (item.itemType == ITEM_TYPE.WEAPON) {
            UnequipWeapon(item as Weapon);
        } else if (item.itemType == ITEM_TYPE.ARMOR) {
            UnequipArmor(item as Armor);
        } else if (item.itemType == ITEM_TYPE.ACCESSORY) {
            UnequipAccessory(item);
        } else if (item.itemType == ITEM_TYPE.CONSUMABLE) {
            UnequipConsumable(item);
        }
        if (item.attributeNames != null) {
            for (int i = 0; i < item.attributeNames.Count; i++) {
                Trait newTrait = AttributeManager.Instance.allTraits[item.attributeNames[i]];
                RemoveTrait(newTrait);
            }
        }
        Messenger.Broadcast(Signals.ITEM_UNEQUIPPED, item, this);
    }
    //Own an Item
    internal void OwnItem(Item item) {
        item.SetOwner(this);
    }
    //Transfer item ownership
    internal void TransferItemOwnership(Item item, Character newOwner) {
        newOwner.OwnItem(item);
    }
    //Try to equip a weapon to a body part of this character and add it to the list of items this character have
    internal bool TryEquipWeapon(Weapon weapon) {
        //if (!_characterClass.allowedWeaponTypes.Contains(weapon.weaponType)) {
        //    return false;
        //}
        _equippedWeapon = weapon;
        weapon.SetEquipped(true);
        return true;
    }
    //Unequips weapon of a character
    private void UnequipWeapon(Weapon weapon) {
        weapon.SetEquipped(false);
        _equippedWeapon = null;
    }
    //Try to equip an armor to a body part of this character and add it to the list of items this character have
    internal bool TryEquipArmor(Armor armor) {
        armor.SetEquipped(true);
        _equippedArmor = armor;
        return true;
    }
    //Unequips armor of a character
    private void UnequipArmor(Armor armor) {
        armor.SetEquipped(false);
        _equippedArmor = null;
    }
    //Try to equip an accessory
    internal bool TryEquipAccessory(Item accessory) {
        accessory.SetEquipped(true);
        _equippedAccessory = accessory;
        return true;
    }
    //Unequips accessory of a character
    private void UnequipAccessory(Item accessory) {
        accessory.SetEquipped(false);
        _equippedAccessory = null;
    }
    //Try to equip an consumable
    internal bool TryEquipConsumable(Item consumable) {
        consumable.SetEquipped(true);
        _equippedConsumable = consumable;
        return true;
    }
    //Unequips consumable of a character
    private void UnequipConsumable(Item consumable) {
        consumable.SetEquipped(false);
        _equippedConsumable = null;
    }
    internal bool HasItem(string itemName) {
        if (_equippedWeapon != null && _equippedWeapon.itemName == itemName) {
            return true;
        } else if (_equippedArmor != null && _equippedArmor.itemName == itemName) {
            return true;
        } else if (_equippedAccessory != null && _equippedAccessory.itemName == itemName) {
            return true;
        } else if (_equippedConsumable != null && _equippedConsumable.itemName == itemName) {
            return true;
        }
        for (int i = 0; i < _inventory.Count; i++) {
            Item currItem = _inventory[i];
            if (currItem.itemName.Equals(itemName)) {
                return true;
            }
        }
        return false;
    }
    internal bool HasItem(Item item) {
        if (_equippedWeapon != null && _equippedWeapon.itemName == item.itemName) {
            return true;
        } else if (_equippedArmor != null && _equippedArmor.itemName == item.itemName) {
            return true;
        } else if (_equippedAccessory != null && _equippedAccessory.itemName == item.itemName) {
            return true;
        } else if (_equippedConsumable != null && _equippedConsumable.itemName == item.itemName) {
            return true;
        }
        if (inventory.Contains(item)) {
            return true;
        }
        return false;
    }
    /*
        Does this character have an item that is like the required item.
        For example, if you want to check if the character has any scrolls,
        without specifying the types of scrolls.
            */
    internal bool HasItemLike(string itemName, int quantity) {
        int counter = 0;
        if (_equippedWeapon != null && _equippedWeapon.itemName == itemName) {
            counter++;
        } else if (_equippedArmor != null && _equippedArmor.itemName == itemName) {
            counter++;
        } else if (_equippedAccessory != null && _equippedAccessory.itemName == itemName) {
            counter++;
        } else if (_equippedConsumable != null && _equippedConsumable.itemName == itemName) {
            counter++;
        }
        for (int i = 0; i < _inventory.Count; i++) {
            Item currItem = _inventory[i];
            if (currItem.itemName.Contains(itemName)) {
                counter++;
            }
        }
        if (counter >= quantity) {
            return true;
        } else {
            return false;
        }
    }
    public List<Item> GetItemsLike(string itemName) {
        List<Item> items = new List<Item>();
        if (_equippedWeapon != null && _equippedWeapon.itemName == itemName) {
            items.Add(_equippedWeapon);
        } else if (_equippedArmor != null && _equippedArmor.itemName == itemName) {
            items.Add(_equippedArmor);
        } else if (_equippedAccessory != null && _equippedAccessory.itemName == itemName) {
            items.Add(_equippedAccessory);
        } else if (_equippedConsumable != null && _equippedConsumable.itemName == itemName) {
            items.Add(_equippedConsumable);
        }
        for (int i = 0; i < _inventory.Count; i++) {
            Item currItem = _inventory[i];
            if (currItem.itemName.Contains(itemName)) {
                items.Add(currItem);
            }
        }
        return items;
    }
    internal Item GetItemInInventory(string itemName) {
        for (int i = 0; i < _inventory.Count; i++) {
            Item currItem = _inventory[i];
            if (currItem.itemName.Equals(itemName)) {
                return currItem;
            }
        }
        return null;
    }
    public void GiveItemsTo(List<Item> items, Character otherCharacter) {
        for (int i = 0; i < items.Count; i++) {
            Item currItem = items[i];
            if (this.HasItem(currItem)) { //check if the character still has the item that he wants to give
                this.ThrowItem(currItem, false);
                otherCharacter.PickupItem(currItem);
                Debug.Log(this.name + " gave item " + currItem.itemName + " to " + otherCharacter.name);
            }
        }
    }
    private void EquipItemsByClass() {
        if (_characterClass != null) {
            if (_characterClass.weaponTierNames != null && _characterClass.weaponTierNames.Count > 0) {
                EquipItem(_characterClass.weaponTierNames[0]);
            }
            if (_characterClass.armorTierNames != null && _characterClass.armorTierNames.Count > 0) {
                EquipItem(_characterClass.armorTierNames[0]);
            }
            if (_characterClass.accessoryTierNames != null && _characterClass.accessoryTierNames.Count > 0) {
                EquipItem(_characterClass.accessoryTierNames[0]);
            }
        }
    }
    public void UpgradeWeapon() {
        if (_characterClass != null && _equippedWeapon != null) {
            if (_characterClass.weaponTierNames != null && _characterClass.weaponTierNames.Count > 0) {
                bool foundEquipped = false;
                for (int i = 0; i < _characterClass.weaponTierNames.Count; i++) {
                    if (foundEquipped) {
                        //Found equipped item, now equip next on the list for upgrade
                        EquipItem(_characterClass.weaponTierNames[i]);
                        break;
                    } else {
                        if (_equippedWeapon.itemName == _characterClass.weaponTierNames[i]) {
                            foundEquipped = true;
                        }
                    }
                }
            }
        }
    }
    public void UpgradeArmor() {
        if (_characterClass != null && _equippedArmor != null) {
            if (_characterClass.armorTierNames != null && _characterClass.armorTierNames.Count > 0) {
                bool foundEquipped = false;
                for (int i = 0; i < _characterClass.armorTierNames.Count; i++) {
                    if (foundEquipped) {
                        //Found equipped item, now equip next on the list for upgrade
                        EquipItem(_characterClass.armorTierNames[i]);
                        break;
                    } else {
                        if (_equippedArmor.itemName == _characterClass.armorTierNames[i]) {
                            foundEquipped = true;
                        }
                    }
                }
            }
        }
    }
    public void UpgradeAccessory() {
        if (_characterClass != null && _equippedAccessory != null) {
            if (_characterClass.accessoryTierNames != null && _characterClass.accessoryTierNames.Count > 0) {
                bool foundEquipped = false;
                for (int i = 0; i < _characterClass.accessoryTierNames.Count; i++) {
                    if (foundEquipped) {
                        //Found equipped weapon, now equip next on the list for upgrade
                        EquipItem(_characterClass.accessoryTierNames[i]);
                        break;
                    } else {
                        if (_equippedAccessory.itemName == _characterClass.accessoryTierNames[i]) {
                            foundEquipped = true;
                        }
                    }
                }
            }
        }
    }
    #endregion

    //#region Status Effects
    //internal void AddStatusEffect(STATUS_EFFECT statusEffect) {
    //    this._statusEffects.Add(statusEffect);
    //}
    //internal void RemoveStatusEffect(STATUS_EFFECT statusEffect) {
    //    this._statusEffects.Remove(statusEffect);
    //}
    //internal void CureStatusEffects() {
    //    for (int i = 0; i < _statusEffects.Count; i++) {
    //        STATUS_EFFECT statusEffect = _statusEffects[i];
    //        int chance = Utilities.rng.Next(0, 100);
    //        if (chance < 15) {
    //            _ownParty.currentCombat.AddCombatLog(this.name + " is cured from " + statusEffect.ToString().ToLower() + ".", this.currentSide);
    //            RemoveStatusEffect(statusEffect);
    //            i--;
    //        }
    //    }
    //}
    //internal bool HasStatusEffect(STATUS_EFFECT statusEffect) {
    //    if (_statusEffects.Contains(statusEffect)) {
    //        return true;
    //    }
    //    return false;
    //}
    //#endregion

    #region Skills
    //private List<Skill> GetGeneralSkills(){
    //          List<Skill> allGeneralSkills = new List<Skill>();
    //          foreach (Skill skill in SkillManager.Instance.generalSkills.Values) {
    //              allGeneralSkills.Add(skill.CreateNewCopy());
    //          }
    //          return allGeneralSkills;
    //}
    //public List<Skill> GetClassSkills() {
    //    List<Skill> skills = new List<Skill>();
    //    for (int i = 0; i < level; i++) {
    //        if (i < characterClass.skillsPerLevel.Count) {
    //            if (characterClass.skillsPerLevel[i] != null) {
    //                for (int j = 0; j < characterClass.skillsPerLevel[i].Length; j++) {
    //                    Skill skill = characterClass.skillsPerLevel[i][j];
    //                    skills.Add(skill);
    //                }
    //            }
    //        }
    //    }
    //    return skills;
    //}
    //public List<AttackSkill> GetClassAttackSkills() {
    //    List<AttackSkill> skills = new List<AttackSkill>();
    //    for (int i = 0; i < level; i++) {
    //        if (i < characterClass.skillsPerLevel.Count) {
    //            if (characterClass.skillsPerLevel[i] != null) {
    //                for (int j = 0; j < characterClass.skillsPerLevel[i].Length; j++) {
    //                    Skill skill = characterClass.skillsPerLevel[i][j];
    //                    if(skill is AttackSkill) {
    //                        skills.Add(skill as AttackSkill);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return skills;
    //}
    //private List<Skill> GetBodyPartSkills(){
    //	List<Skill> allBodyPartSkills = new List<Skill>();
    //	foreach (Skill skill in SkillManager.Instance.bodyPartSkills.Values) {
    //		bool requirementsPassed = true;
    //		//Skill skill	= SkillManager.Instance.bodyPartSkills [skillName];
    //		for (int j = 0; j < skill.skillRequirements.Length; j++) {
    //			if(!HasAttribute(skill.skillRequirements[j].attributeRequired, skill.skillRequirements[j].itemQuantity)){
    //				requirementsPassed = false;
    //				break;
    //			}
    //		}
    //		if(requirementsPassed){
    //			allBodyPartSkills.Add (skill.CreateNewCopy ());
    //		}
    //	}
    //	return allBodyPartSkills;
    //}
    #endregion

    #region Roles
    public void AssignRole(CHARACTER_ROLE role) {
        bool wasRoleChanged = false;
        if (_role != null) {
            _role.ChangedRole();
#if !WORLD_CREATION_TOOL
            Log roleChangeLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "change_role");
            roleChangeLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            AddHistory(roleChangeLog);
#endif
            wasRoleChanged = true;
        }
        switch (role) {
            case CHARACTER_ROLE.HERO:
            _role = new Hero(this);
            break;
            case CHARACTER_ROLE.VILLAIN:
            _role = new Villain(this);
            break;
            case CHARACTER_ROLE.CIVILIAN:
            _role = new Civilian(this);
            break;
            case CHARACTER_ROLE.KING:
            _role = new King(this);
            break;
            case CHARACTER_ROLE.PLAYER:
            _role = new PlayerRole(this);
            break;
            case CHARACTER_ROLE.GUARDIAN:
            _role = new Guardian(this);
            break;
            case CHARACTER_ROLE.BEAST:
            _role = new Beast(this);
            break;
            case CHARACTER_ROLE.LEADER:
            _role = new Leader(this);
            break;
            case CHARACTER_ROLE.BANDIT:
            _role = new Bandit(this);
            break;
            case CHARACTER_ROLE.ARMY:
            _role = new Army(this);
            SetName(this.characterClass.className);
            break;
            default:
            break;
        }
        if (_role != null) {
            _role.OnAssignRole();
#if !WORLD_CREATION_TOOL
            AddDefaultInteractions();
#endif
        }
        if (wasRoleChanged) {
            Messenger.Broadcast(Signals.ROLE_CHANGED, this);
        }
    }
    #endregion

    #region Character Class
    public void AssignClass(CharacterClass charClass) {
        _characterClass = charClass.CreateNewCopy();
        _skills = new List<Skill>();
        _skills.Add(_characterClass.skill);
        EquipItemsByClass();
        SetTraitsFromClass();
        AssignJob(_characterClass.jobType);
    }
    #endregion

    #region Job
    public void AssignJob(JOB jobType) {
        switch (jobType) {
            case JOB.SPY:
                _job = new Spy(this);
                break;
            case JOB.RAIDER:
                _job = new Raider(this);
                break;
            case JOB.INSTIGATOR:
                _job = new Instigator(this);
                break;
            case JOB.EXPLORER:
                _job = new Explorer(this);
                break;
            case JOB.DISSUADER:
                _job = new Dissuader(this);
                break;
            case JOB.DIPLOMAT:
                _job = new Diplomat(this);
                break;
            case JOB.RECRUITER:
                _job = new Recruiter(this);
                break;
            case JOB.LEADER:
                _job = new LeaderJob(this);
                break;
            case JOB.WORKER:
                _job = new Worker(this);
                break;
            default:
                _job = new Job(this, JOB.NONE);
                break;
        }
        _job.OnAssignJob();
    }
    #endregion

    #region Character Tags
    //public void AssignInitialAttributes() {
    //    int tagChance = UnityEngine.Random.Range(0, 100);
    //    ATTRIBUTE[] initialTags = (ATTRIBUTE[])System.Enum.GetValues(typeof(ATTRIBUTE));
    //    for (int j = 0; j < initialTags.Length; j++) {
    //        ATTRIBUTE tag = initialTags[j];
    //        if (tagChance < Utilities.GetTagWorldGenChance(tag)) {
    //            AddAttribute(tag);
    //        }
    //    }
    //}
    //      public CharacterAttribute AddAttribute(ATTRIBUTE tag) {
    //	if(HasAttribute(tag)){
    //		return null;
    //	}
    //	CharacterAttribute charTag = null;
    //	switch (tag) {
    //          case ATTRIBUTE.HUNGRY:
    //              charTag = new Hungry(this);
    //              break;
    //          case ATTRIBUTE.FAMISHED:
    //              charTag = new Famished(this);
    //              break;
    //          case ATTRIBUTE.TIRED:
    //              charTag = new Tired(this);
    //              break;
    //          case ATTRIBUTE.EXHAUSTED:
    //              charTag = new Exhausted(this);
    //              break;
    //          case ATTRIBUTE.SAD:
    //              charTag = new Sad(this);
    //              break;
    //          case ATTRIBUTE.DEPRESSED:
    //              charTag = new Depressed(this);
    //              break;
    //          case ATTRIBUTE.ANXIOUS:
    //              charTag = new Anxious(this);
    //              break;
    //          case ATTRIBUTE.INSECURE:
    //              charTag = new Insecure(this);
    //              break;
    //          case ATTRIBUTE.DRUNK:
    //              charTag = new Drunk(this);
    //              break;
    //          case ATTRIBUTE.DISTURBED:
    //              charTag = new Disturbed(this);
    //              break;
    //          case ATTRIBUTE.CRAZED:
    //              charTag = new Crazed(this);
    //              break;
    //          case ATTRIBUTE.DEMORALIZED:
    //              charTag = new Demoralized(this);
    //              break;
    //          case ATTRIBUTE.STARVING:
    //              charTag = new Starving(this);
    //              break;
    //          case ATTRIBUTE.WOUNDED:
    //              charTag = new Wounded(this);
    //              break;
    //          case ATTRIBUTE.WRECKED:
    //              charTag = new Wrecked(this);
    //              break;
    //          case ATTRIBUTE.IMPULSIVE:
    //              charTag = new Impulsive(this);
    //              break;
    //          case ATTRIBUTE.BETRAYED:
    //              charTag = new Betrayed(this);
    //              break;
    //          case ATTRIBUTE.HEARTBROKEN:
    //              charTag = new Heartbroken(this);
    //              break;
    //          }
    //	if(charTag != null){
    //		AddCharacterAttribute (charTag);
    //	}
    //          return charTag;
    //}
    public CharacterAttribute CreateAttribute(ATTRIBUTE type) {
        switch (type) {
            case ATTRIBUTE.GREGARIOUS:
            return new Gregarious();
            case ATTRIBUTE.BOOKWORM:
            return new Bookworm();
            case ATTRIBUTE.SINGER:
            return new Singer();
            case ATTRIBUTE.DAYDREAMER:
            return new Daydreamer();
            case ATTRIBUTE.MEDITATOR:
            return new Meditator();
            case ATTRIBUTE.CLEANER:
            return new Cleaner();
            case ATTRIBUTE.INTROVERT:
            return new Introvert();
            case ATTRIBUTE.EXTROVERT:
            return new Extrovert();
            case ATTRIBUTE.BELLIGERENT:
            return new Belligerent();
            case ATTRIBUTE.LIBERATED:
            return new Liberated();
            case ATTRIBUTE.UNFAITHFUL:
            return new Unfaithful();
            case ATTRIBUTE.DEAFENED:
            return new Deafened();
            case ATTRIBUTE.MUTE:
            return new Mute();
            case ATTRIBUTE.ROYALTY:
            return new Royalty();
            case ATTRIBUTE.STALKER:
            return new Stalker();
            case ATTRIBUTE.SPOOKED:
            return new Spooked();
            case ATTRIBUTE.HUMAN:
            return new Human();
            case ATTRIBUTE.HUNGRY:
            return new Hungry();
            case ATTRIBUTE.FAMISHED:
            return new Famished();
            case ATTRIBUTE.TIRED:
            return new Tired();
            case ATTRIBUTE.EXHAUSTED:
            return new Exhausted();
            case ATTRIBUTE.SAD:
            return new Sad();
            case ATTRIBUTE.DEPRESSED:
            return new Depressed();
            case ATTRIBUTE.ANXIOUS:
            return new Anxious();
            case ATTRIBUTE.INSECURE:
            return new Insecure();
            case ATTRIBUTE.DRUNK:
            return new Drunk();
            case ATTRIBUTE.DISTURBED:
            return new Disturbed();
            case ATTRIBUTE.CRAZED:
            return new Crazed();
            case ATTRIBUTE.DEMORALIZED:
            return new Demoralized();
            case ATTRIBUTE.STARVING:
            return new Starving();
            case ATTRIBUTE.WOUNDED:
            return new Wounded();
            case ATTRIBUTE.WRECKED:
            return new Wrecked();
            case ATTRIBUTE.IMPULSIVE:
            return new Impulsive();
            case ATTRIBUTE.BETRAYED:
            return new Betrayed();
            case ATTRIBUTE.HEARTBROKEN:
            return new Heartbroken();
            case ATTRIBUTE.MARKED:
            return new Marked();
        }
        return null;
    }
    private void GenerateSetupAttributes(CharacterSetup setup) {
        for (int i = 0; i < setup.tags.Count; i++) {
            AddAttribute(setup.tags[i]);
        }
    }
    public bool HasAttributes(ATTRIBUTE[] tagsToHave, bool mustHaveAll = false) {
        return DoesHaveAttributes(this, tagsToHave, mustHaveAll);
    }
    private bool DoesHaveAttributes(Character currCharacter, ATTRIBUTE[] tagsToHave, bool mustHaveAll = false) {
        if (mustHaveAll) {
            int tagsCount = 0;
            for (int i = 0; i < currCharacter.attributes.Count; i++) {
                for (int j = 0; j < tagsToHave.Length; j++) {
                    if (tagsToHave[j] == currCharacter.attributes[i].attribute) {
                        tagsCount++;
                        break;
                    }
                }
                if (tagsCount >= tagsToHave.Length) {
                    return true;
                }
            }
        } else {
            for (int i = 0; i < currCharacter.attributes.Count; i++) {
                for (int j = 0; j < tagsToHave.Length; j++) {
                    if (tagsToHave[j] == currCharacter.attributes[i].attribute) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public CharacterAttribute GetAttribute(ATTRIBUTE attribute) {
        for (int i = 0; i < _attributes.Count; i++) {
            if (_attributes[i].attribute == attribute) {
                return _attributes[i];
            }
        }
        return null;
    }
    public CharacterAttribute GetAttribute(string attribute) {
        for (int i = 0; i < _attributes.Count; i++) {
            if (_attributes[i].name.ToLower() == attribute.ToLower()) {
                return _attributes[i];
            }
        }
        return null;
    }
    public CharacterAttribute AddAttribute(ATTRIBUTE attribute) {
        if (GetAttribute(attribute) == null) {
            CharacterAttribute newAttribute = CreateAttribute(attribute);
            _attributes.Add(newAttribute);
#if !WORLD_CREATION_TOOL
            newAttribute.OnAddAttribute(this);
#endif
            Messenger.Broadcast<Character, CharacterAttribute>(Signals.ATTRIBUTE_ADDED, this, newAttribute);
            return newAttribute;
        }
        return null;
    }
    public void AddAttributes(List<ATTRIBUTE> attributes) {
        for (int i = 0; i < attributes.Count; i++) {
            AddAttribute(attributes[i]);
        }
    }
    public void RemoveAttributes(List<ATTRIBUTE> attributes) {
        for (int i = 0; i < attributes.Count; i++) {
            RemoveAttribute(attributes[i]);
        }
    }
    public bool RemoveAttribute(ATTRIBUTE attribute) {
        for (int i = 0; i < _attributes.Count; i++) {
            if (_attributes[i].attribute == attribute) {
                CharacterAttribute removedAttribute = _attributes[i];
#if !WORLD_CREATION_TOOL
                removedAttribute.OnRemoveAttribute();
#endif
                _attributes.RemoveAt(i);
                Messenger.Broadcast<Character, CharacterAttribute>(Signals.ATTRIBUTE_REMOVED, this, removedAttribute);
                return true;
            }
        }
        return false;
    }
    public bool RemoveAttribute(CharacterAttribute attribute) {
        CharacterAttribute attributeToBeRemoved = attribute;
        if (_attributes.Remove(attribute)) {
#if !WORLD_CREATION_TOOL
            attributeToBeRemoved.OnRemoveAttribute();
#endif
            Messenger.Broadcast<Character, CharacterAttribute>(Signals.ATTRIBUTE_REMOVED, this, attributeToBeRemoved);
            return true;
        }
        return false;
    }
    #endregion

    #region Faction
    public void SetFaction(Faction faction) {
        _faction = faction;
        if (_faction != null) {
            Messenger.Broadcast<Character>(Signals.FACTION_SET, this);
        }
    }
    #endregion

    #region Party
    /*
        Create a new Party with this character as the leader.
            */
    public virtual Party CreateOwnParty() {
        if (_ownParty != null) {
            _ownParty.RemoveCharacter(this);
        }
        CharacterParty newParty = new CharacterParty(this);
        SetOwnedParty(newParty);
        newParty.AddCharacter(this);
        newParty.CreateCharacterObject();
        return newParty;
    }
    public virtual void SetOwnedParty(Party party) {
        _ownParty = party as CharacterParty;
    }
    public virtual void SetCurrentParty(Party party) {
        _currentParty = party as CharacterParty;
    }
    public void OnRemovedFromParty() {
        SetCurrentParty(ownParty); //set the character's party to it's own party
        if (ownParty is CharacterParty) {
            if ((ownParty as CharacterParty).actionData.currentAction != null) {
                (ownParty as CharacterParty).actionData.currentAction.EndAction(ownParty, (ownParty as CharacterParty).actionData.currentTargetObject);
            }
        }
        if (this.minion != null) {
            this.minion.SetEnabledState(true); //reenable this minion, since it could've been disabled because it was part of another party
        }
    }
    public void OnAddedToParty() {
        if (currentParty.id != ownParty.id) {
            if (ownParty.specificLocation is BaseLandmark) {
                ownParty.specificLocation.RemoveCharacterFromLocation(ownParty);
            }
            //ownParty.icon.SetVisualState(false);
        }
    }
    public void OnAddedToPlayer() {
        if (ownParty.specificLocation is BaseLandmark) {
            ownParty.specificLocation.RemoveCharacterFromLocation(ownParty);
        }
        PlayerManager.Instance.player.playerArea.coreTile.landmarkOnTile.AddCharacterToLocation(ownParty);
        if (this.homeLandmark != null) {
            this.homeLandmark.RemoveCharacterHomeOnLandmark(this);
        }
        PlayerManager.Instance.player.playerArea.coreTile.landmarkOnTile.AddCharacterHomeOnLandmark(this);
    }
    public bool IsInParty() {
        if (currentParty.characters.Count > 1) {
            return true; //if the character is in a party that has more than 1 characters
        }
        return false;
    }
    public bool IsInOwnParty() {
        if (currentParty.id == ownParty.id) {
            return true;
        }
        return false;
    }
    public bool InviteToParty(Character inviter) {
        if (IsInParty()) {
            return false;
        }
        if (party.isIdle) {
            return false;
        }
        if (this.party.actionData.currentAction == null || this.party.actionData.currentAction.actionData.actionType == ACTION_TYPE.WAIT_FOR_PARTY) {
            //accept invitation
            //this.actionQueue.Clear();
            this.party.actionData.ForceDoAction(inviter.ownParty.icharacterObject.currentState.GetAction(ACTION_TYPE.JOIN_PARTY), inviter.ownParty.icharacterObject);
            return true;
        }
        return false;
    }
    #endregion

    #region Location
    public bool IsCharacterInAdjacentRegionOfThis(Character targetCharacter) {
        for (int i = 0; i < _currentRegion.adjacentRegionsViaRoad.Count; i++) {
            if (targetCharacter.party.currentRegion.id == _currentRegion.adjacentRegionsViaRoad[i].id) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Quests
    public bool HasQuest() {
        return currentQuest != null;
    }
    public void SetQuest(Quest quest) {
        currentQuest = quest;
        if (currentQuest != null) {
            currentQuest.OnAcceptQuest(this);
            Debug.Log("Set " + this.name + "'s quest to " + currentQuest.name);
        }
    }
    #endregion

    #region HP
    public bool IsHealthFull() {
        return _currentHP >= maxHP;
    }
    #endregion

    #region Utilities
    public void ChangeGender(GENDER gender) {
        _gender = gender;
        Messenger.Broadcast(Signals.GENDER_CHANGED, this, gender);
    }
    public void ChangeRace(RACE race) {
        if (_raceSetting.race == race) {
            return; //current race is already the new race, no change
        }
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
    }
    public void ChangeClass(string className) {
        //TODO: Change data as needed
        string previousClassName = _characterClass.className;
        CharacterClass charClass = CharacterManager.Instance.classesDictionary[className];
        AssignClass(charClass);
        //_characterClass = charClass.CreateNewCopy();
        OnCharacterClassChange();

#if !WORLD_CREATION_TOOL
        //_homeLandmark.tileLocation.areaOfTile.excessClasses.Remove(previousClassName);
        //_homeLandmark.tileLocation.areaOfTile.missingClasses.Remove(_characterClass.className);

        //Log log = new Log(GameManager.Instance.Today(), "CharacterActions", "ChangeClassAction", "change_class");
        //log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //log.AddToFillers(null, previousClassName, LOG_IDENTIFIER.STRING_1);
        //log.AddToFillers(null, _characterClass.className, LOG_IDENTIFIER.STRING_2);
        //AddHistory(log);
        //check equipped items
#endif

    }
    public void SetName(string newName) {
        _name = newName;
    }
    //If true, character can't do daily action (onDailyAction), i.e. actions, needs
    //public void SetIsIdle(bool state) {
    //    _isIdle = state;
    //}
    //public bool HasPathToParty(Party partyToJoin) {
    //    return PathGenerator.Instance.GetPath(currLocation, partyToJoin.currLocation, PATHFINDING_MODE.PASSABLE, _faction) != null;
    //}
    public void CenterOnCharacter() {
        if (!this.isDead) {
            CameraMove.Instance.CenterCameraOn(currentParty.specificLocation.tileLocation.gameObject);
        }
    }
    private void GetRandomCharacterColor() {
        _characterColor = CombatManager.Instance.UseRandomCharacterColor();
        _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
    }
    public void SetCharacterColor(Color color) {
        _characterColor = color;
        _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
    }
    //public void EverydayAction() {
    //    if (onDailyAction != null) {
    //        onDailyAction();
    //    }
    //    CheckForPPDeath();
    //}
    //public void AdvertiseSelf(ActionThread actionThread) {
    //    if(actionThread.character.id != this.id && _currentRegion.id == actionThread.character.party.currentRegion.id) {
    //        actionThread.AddToChoices(_characterObject);
    //    }
    //}
    public bool CanObtainResource(List<RESOURCE> resources) {
        if (this.role != null) {//characters without a role cannot get actions, and therefore cannot obtain resources
            for (int i = 0; i < _ownParty.currentRegion.landmarks.Count; i++) {
                BaseLandmark landmark = _ownParty.currentRegion.landmarks[i];
                StructureObj iobject = landmark.landmarkObj;
                if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
                    for (int k = 0; k < iobject.currentState.actions.Count; k++) {
                        CharacterAction action = iobject.currentState.actions[k];
                        if (action.actionData.resourceGiven != RESOURCE.NONE && resources.Contains(action.actionData.resourceGiven)) { //does the action grant a resource, and is that a resource that is needed
                            if (action.MeetsRequirements(_ownParty, landmark) && action.CanBeDone(iobject) && action.CanBeDoneBy(_ownParty, iobject)) { //Filter
                                //if the character can do an action that yields a needed resource, return true
                                return true;
                            }
                        }

                    }
                }
            }
        }
        return false;
    }
    public bool IsSpecialCivilian() {
        if (this.characterClass != null) {
            if (this.characterClass.className.Equals("Farmer") || this.characterClass.className.Equals("Miner") || this.characterClass.className.Equals("Retired Hero") ||
                this.characterClass.className.Equals("Shopkeeper") || this.characterClass.className.Equals("Woodcutter")) {
                return true;
            }
        }
        return false;
    }
    //private void OnOtherCharacterDied(Character character) {
    //    if (character.id != this.id) {
    //        if (IsCharacterLovedOne(character)) { //A character gains heartbroken tag for 15 days when a family member or a loved one dies.
    //            AddAttribute(ATTRIBUTE.HEARTBROKEN);
    //        }
    //        //RemoveRelationshipWith(character);
    //    }
    //}
    //public bool IsCharacterLovedOne(Character otherCharacter) {
    //    Relationship rel = GetRelationshipWith(otherCharacter);
    //    if (rel != null) {
    //        CHARACTER_RELATIONSHIP[] lovedOneStatuses = new CHARACTER_RELATIONSHIP[] {
    //            CHARACTER_RELATIONSHIP.FATHER,
    //            CHARACTER_RELATIONSHIP.MOTHER,
    //            CHARACTER_RELATIONSHIP.BROTHER,
    //            CHARACTER_RELATIONSHIP.SISTER,
    //            CHARACTER_RELATIONSHIP.SON,
    //            CHARACTER_RELATIONSHIP.DAUGHTER,
    //            CHARACTER_RELATIONSHIP.LOVER,
    //            CHARACTER_RELATIONSHIP.HUSBAND,
    //            CHARACTER_RELATIONSHIP.WIFE,
    //        };
    //        if (rel.HasAnyStatus(lovedOneStatuses)) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    public void SetMode(MODE mode) {
        _currentMode = mode;
    }
    public void SetDoNotDisturb(bool state) {
        _doNotDisturb = state;
    }
    public void SetAlreadyTargetedByGrudge(bool state) {
        _alreadyTargetedByGrudge = state;
    }
    public void SetIsLeader(bool state) {
        _isLeader = state;
    }
    public void AttackAnArea(Area target) {
        Interaction attackInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.ATTACK, target.coreTile.landmarkOnTile);
        attackInteraction.AddEndInteractionAction(() => _ownParty.GoHomeAndDisband());
        _ownParty.GoToLocation(target.coreTile.landmarkOnTile, PATHFINDING_MODE.NORMAL, () => SetForcedInteraction(attackInteraction));
    }
    public void GoToAreaToMakePeaceWithFaction(Area target) {
        Interaction peaceInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CHARACTER_PEACE_NEGOTIATION, target.coreTile.landmarkOnTile);
        peaceInteraction.AddEndInteractionAction(() => _ownParty.GoHome());
        _ownParty.GoToLocation(target.coreTile.landmarkOnTile, PATHFINDING_MODE.NORMAL, () => SetForcedInteraction(peaceInteraction));
    }
    #endregion

    #region Relationships
    //public void AddNewRelationship(Character relWith, Relationship relationship) {
    //    if (!_relationships.ContainsKey(relWith)) {
    //        _relationships.Add(relWith, relationship);
    //        Messenger.Broadcast<Relationship>(Signals.RELATIONSHIP_CREATED, relationship);
    //    } else {
    //        throw new Exception(this.name + " already has a relationship with " + relWith.name + ", but something is trying to create a new one!");
    //    }
    //}
    //public void RemoveRelationshipWith(Character relWith) {
    //    if (_relationships.ContainsKey(relWith)) {
    //        Relationship rel = _relationships[relWith];
    //        _relationships.Remove(relWith);
    //        Messenger.Broadcast<Relationship>(Signals.RELATIONSHIP_REMOVED, rel);
    //    }
    //}
    //public Relationship GetRelationshipWith(Character character) {
    //    if (_relationships.ContainsKey(character)) {
    //        return _relationships[character];
    //    }
    //    return null;
    //}
    //public bool AlreadyHasRelationshipStatus(CHARACTER_RELATIONSHIP relStat) {
    //    foreach (KeyValuePair<Character, Relationship> kvp in relationships) {
    //        if (kvp.Value.HasStatus(relStat)) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //public Character GetCharacterWithRelationshipStatus(CHARACTER_RELATIONSHIP relStat) {
    //    foreach (KeyValuePair<Character, Relationship> kvp in relationships) {
    //        if (kvp.Value.HasStatus(relStat)) {
    //            return kvp.Key;
    //        }
    //    }
    //    return null;
    //}
    //public List<Character> GetCharactersWithRelationshipStatus(CHARACTER_RELATIONSHIP relStat) {
    //    List<Character> characters = new List<Character>();
    //    foreach (KeyValuePair<Character, Relationship> kvp in relationships) {
    //        if (kvp.Value.HasStatus(relStat) && characters.Contains(kvp.Key)) {
    //            characters.Add(kvp.Key);
    //        }
    //    }
    //    return characters;
    //}
    //public void LoadRelationships(List<RelationshipSaveData> data) {
    //    _relationships = new Dictionary<Character, Relationship>();
    //    for (int i = 0; i < data.Count; i++) {
    //        RelationshipSaveData currData = data[i];
    //        Character otherCharacter = CharacterManager.Instance.GetCharacterByID(currData.targetCharacterID);
    //        Relationship rel = new Relationship(this, otherCharacter);
    //        rel.AddRelationshipStatus(currData.relationshipStatuses);
    //        _relationships.Add(otherCharacter, rel);

    //    }
    //}
    //public Character GetPartner() {
    //    foreach (KeyValuePair<Character, Relationship> kvp in _relationships) {
    //        for (int i = 0; i < kvp.Value.relationshipStatuses.Count; i++) {
    //            CHARACTER_RELATIONSHIP status = kvp.Value.relationshipStatuses[i];
    //            if (status == CHARACTER_RELATIONSHIP.HUSBAND || status == CHARACTER_RELATIONSHIP.WIFE) {
    //                return kvp.Key;
    //            }
    //        }
    //    }
    //    return null;
    //}
    //public bool HasRelationshipWith(Character otherCharacter) {
    //    return _relationships.ContainsKey(otherCharacter);
    //}
    //public bool HasRelationshipStatusWith(Character otherCharacter, CHARACTER_RELATIONSHIP relStat) {
    //    return _relationships[otherCharacter].HasStatus(relStat);
    //}
    #endregion

    #region History
    public void AddHistory(Log log) {
        if (!_history.Contains(log)) {
            _history.Add(log);
            //if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && this.id == UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id) {
            //    Debug.Log("Added log to history of " + this.name + ". " + log.isInspected);
            //}
            if (this._history.Count > 60) {
                this._history.RemoveAt(0);
            }
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
        }

    }
    #endregion

    #region Character
    public bool IsHostileWith(Character character) {
        if (this.faction == null) {
            return true; //this character has no faction
        }
        //if (this.currentAction != null && this.currentAction.HasHostilitiesBecauseOfTask(combatInitializer)) {
        //    return true;
        //}
        //Check here if the combatInitializer is hostile with this character, if yes, return true
        Faction factionOfEnemy = character.faction;

        //if (combatInitializer.icharacterType == ICHARACTER_TYPE.CHARACTER) {
        //    factionOfEnemy = (combatInitializer as Character).faction;
        //}else if(combatInitializer is Party) {
        //    factionOfEnemy = (combatInitializer as Party).faction;
        //}
        if (factionOfEnemy != null) {
            if (factionOfEnemy.id == this.faction.id) {
                return false; //characters are of same faction
            }
            FactionRelationship rel = this.faction.GetRelationshipWith(factionOfEnemy);
            if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                return true; //factions of combatants are hostile
            }
            return false;
        } else {
            return true; //enemy has no faction
        }

    }
    public STANCE GetCurrentStance() {
        return STANCE.NEUTRAL;
    }
    #endregion

    #region Combat Handlers
    public void SetIsInCombat(bool state) {
        _isInCombat = state;
    }
    #endregion

    #region Landmarks
    public void AddExploredLandmark(BaseLandmark landmark) {
        _exploredLandmarks.Add(landmark); //did not add checking if landmark is already in list, since I want to allow duplicates
        //schedule removal of landmark after 6 months
        GameDate expiration = GameManager.Instance.Today();
        expiration.AddMonths(6);
        SchedulingManager.Instance.AddEntry(expiration, () => RemoveLandmarkAsExplored(landmark));
    }
    private void RemoveLandmarkAsExplored(BaseLandmark landmark) {
        _exploredLandmarks.Remove(landmark);
    }
    private void OnDestroyLandmark(BaseLandmark landmark) {
        if (specificLocation.tileLocation.landmarkOnTile != null && specificLocation.tileLocation.landmarkOnTile.id == landmark.id) {
            if (!_isDead && _currentParty.icon.isTravelling) {
                return;
            }
            Death();
        }
    }
    #endregion

    #region Traces
    public void LeaveTraceOnLandmark() {
        ILocation location = _ownParty.specificLocation;
        if (location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
            BaseLandmark landmark = location as BaseLandmark;
            int chance = UnityEngine.Random.Range(0, 100);
            int value = GetLeaveTraceChance();
            if (chance < value) {
                landmark.AddTrace(this);
            }
        }
    }
    private int GetLeaveTraceChance() {
        STANCE stance = GetCurrentStance();
        switch (stance) {
            case STANCE.COMBAT:
            return 100;
            case STANCE.NEUTRAL:
            return 50;
            case STANCE.STEALTHY:
            return 25;
        }
        return 0;
    }
    public void AddTraceInfo(Character character, string identifier, bool isUnique) {
        if (isUnique) {
            Character previousTrace = GetCharacterFromTraceInfo(identifier);
            if (previousTrace != null) { //If there is an existing trace of the item, replace it with this new trace
                if (previousTrace.id != character.id) {
                    _traceInfo[previousTrace].Remove(identifier);
                    if (_traceInfo[previousTrace].Count <= 0) {
                        _traceInfo.Remove(previousTrace);
                    }
                    Debug.Log(this.name + " REMOVED TRACE INFO OF " + previousTrace.name + " FOR " + identifier);
                } else {
                    return;
                }
            }
        }
        if (_traceInfo.ContainsKey(character)) {
            if (!_traceInfo[character].Contains(identifier)) {
                _traceInfo[character].Add(identifier);
                Debug.Log(this.name + " ADDED TRACE INFO OF " + character.name + " FOR " + identifier);
            }
        } else {
            _traceInfo.Add(character, new List<string>() { identifier });
            Debug.Log(this.name + " ADDED TRACE INFO OF " + character.name + " FOR " + identifier);
        }
    }
    public Character GetCharacterFromTraceInfo(string info) {
        foreach (Character character in _traceInfo.Keys) {
            if (_traceInfo[character].Contains(info)) {
                return character;
            }
        }
        return null;
    }
    #endregion

    #region Action Queue
    public bool DoesSatisfiesPrerequisite(IPrerequisite prerequisite) {
        if (prerequisite.prerequisiteType == PREREQUISITE.RESOURCE) {
            ResourcePrerequisite resourcePrerequisite = prerequisite as ResourcePrerequisite;
            if (resourcePrerequisite.resourceType != RESOURCE.NONE && _ownParty.characterObject.resourceInventory[resourcePrerequisite.resourceType] >= resourcePrerequisite.amount) {
                return true;
            }
        }
        return false;
    }
    #endregion

    //#region Needs
    //private void CiviliansDiedReduceSanity(StructureObj whereCiviliansDied, int amount) {
    //    if(_currentRegion.id == whereCiviliansDied.objectLocation.tileLocation.region.id) {
    //        ILocation location = _ownParty.specificLocation;
    //        if (location.tileLocation.id == whereCiviliansDied.objectLocation.tileLocation.id || whereCiviliansDied.objectLocation.tileLocation.neighbourDirections.ContainsValue(location.tileLocation)){
    //            int sanityToReduce = amount * 5;
    //            //this.role.AdjustSanity(-sanityToReduce);
    //            Debug.Log(this.name + " has reduced its sanity by " + sanityToReduce + " because " + amount + " civilians died in " + whereCiviliansDied.objectLocation.tileLocation.tileName + " (" + whereCiviliansDied.objectLocation.tileLocation.coordinates + ")");
    //        }
    //    }
    //}
    //#endregion

    #region Portrait Settings
    public void SetPortraitSettings(PortraitSettings settings) {
        _portraitSettings = settings;
    }
    #endregion

    #region RPG
    public void LevelUp() {
        //Only level up once per day
        if (_lastLevelUpDay == GameManager.Instance.continuousDays) {
            return;
        }
        _lastLevelUpDay = GameManager.Instance.continuousDays;
        if (_level < CharacterManager.Instance.maxLevel) {
            _level += 1;
            //_experience = 0;
            //RecomputeMaxExperience();
            //Add stats per level from class
            _attackPower += (int) ((_characterClass.attackPowerPerLevel / 100f) * (float) _raceSetting.baseAttackPower);
            _speed += (int) ((_characterClass.speedPerLevel / 100f) * (float) _raceSetting.baseSpeed);
            AdjustMaxHP((int) ((_characterClass.hpPerLevel / 100f) * (float) _raceSetting.baseHP));
            //_maxSP += _characterClass.spPerLevel;
            //Add stats per level from race
            if (_level > 1) {
                if (_raceSetting.hpPerLevel.Length > 0) {
                    int hpIndex = _level % _raceSetting.hpPerLevel.Length;
                    hpIndex = hpIndex == 0 ? _raceSetting.hpPerLevel.Length : hpIndex;
                    AdjustMaxHP(_raceSetting.hpPerLevel[hpIndex - 1]);
                }
                if (_raceSetting.attackPerLevel.Length > 0) {
                    int attackIndex = _level % _raceSetting.attackPerLevel.Length;
                    attackIndex = attackIndex == 0 ? _raceSetting.attackPerLevel.Length : attackIndex;
                    _attackPower += _raceSetting.attackPerLevel[attackIndex - 1];
                }
            }

            //Reset to full health and sp
            ResetToFullHP();
            //ResetToFullSP();
            if(_playerCharacterItem != null) {
                _playerCharacterItem.UpdateMinionItem();
                Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
            }
        }
    }
    public void LevelUp(int amount) {
        //Only level up once per day
        if (_lastLevelUpDay == GameManager.Instance.continuousDays) {
            return;
        }
        _lastLevelUpDay = GameManager.Instance.continuousDays;
        int supposedLevel = _level + amount;
        if (supposedLevel > CharacterManager.Instance.maxLevel) {
            amount = CharacterManager.Instance.maxLevel - level;
        }
        _level += amount;
        //_experience = 0;
        //RecomputeMaxExperience();
        //Add stats per level from class
        _attackPower += (amount * (int) ((_characterClass.attackPowerPerLevel / 100f) * (float) _raceSetting.baseAttackPower));
        _speed += (amount * (int) ((_characterClass.speedPerLevel / 100f) * (float) _raceSetting.baseSpeed));
        AdjustMaxHP((amount * (int) ((_characterClass.hpPerLevel / 100f) * (float) _raceSetting.baseHP)));
        //_maxSP += _characterClass.spPerLevel;
        //Add stats per level from race
        if (_level > 1) {
            if (_raceSetting.hpPerLevel.Length > 0) {
                int hpIndex = _level % _raceSetting.hpPerLevel.Length;
                hpIndex = hpIndex == 0 ? _raceSetting.hpPerLevel.Length : hpIndex;
                AdjustMaxHP(_raceSetting.hpPerLevel[hpIndex - 1]);
            }
            if (_raceSetting.attackPerLevel.Length > 0) {
                int attackIndex = _level % _raceSetting.attackPerLevel.Length;
                attackIndex = attackIndex == 0 ? _raceSetting.attackPerLevel.Length : attackIndex;
                _attackPower += _raceSetting.attackPerLevel[attackIndex - 1];
            }
        }

        //Reset to full health and sp
        ResetToFullHP();
        //ResetToFullSP();
        Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
        if (_playerCharacterItem != null) {
            _playerCharacterItem.UpdateMinionItem();
        }
    }
    public void SetLevel(int amount) {
        _level = amount;
        if (_level < 1) {
            _level = 1;
        }
        AllocateStats();
        int multiplier = _level - 1;

        //Add stats per level from class
        _attackPower += (multiplier * (int) ((_characterClass.attackPowerPerLevel / 100f) * (float) _raceSetting.baseAttackPower));
        _speed += (multiplier * (int) ((_characterClass.speedPerLevel / 100f) * (float) _raceSetting.baseSpeed));
        AdjustMaxHP((multiplier * (int) ((_characterClass.hpPerLevel / 100f) * (float) _raceSetting.baseHP)));
        //_maxSP += _characterClass.spPerLevel;
        //Add stats per level from race
        if (_level > 1) {
            if (_raceSetting.hpPerLevel.Length > 0) {
                int hpIndex = _level % _raceSetting.hpPerLevel.Length;
                hpIndex = hpIndex == 0 ? _raceSetting.hpPerLevel.Length : hpIndex;
                AdjustMaxHP(_raceSetting.hpPerLevel[hpIndex - 1]);
            }
            if (_raceSetting.attackPerLevel.Length > 0) {
                int attackIndex = _level % _raceSetting.attackPerLevel.Length;
                attackIndex = attackIndex == 0 ? _raceSetting.attackPerLevel.Length : attackIndex;
                _attackPower += _raceSetting.attackPerLevel[attackIndex - 1];
            }
        }

        //Reset to full health and sp
        ResetToFullHP();
        //ResetToFullSP();
        Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
        if (_playerCharacterItem != null) {
            _playerCharacterItem.UpdateMinionItem();
        }

        //Reset Experience
        //_experience = 0;
        //RecomputeMaxExperience();
    }
    public void OnCharacterClassChange() {
        if (_currentHP > _maxHP) {
            _currentHP = _maxHP;
        }
        if (_sp > _maxSP) {
            _sp = _maxSP;
        }
    }
    public void AdjustExperience(int amount) {
        _experience += amount;
        if (_experience >= _maxExperience) {
            _experience = 0;
            //LevelUp();
        }
    }
    public void AdjustElementalWeakness(ELEMENT element, float amount) {
        _elementalWeaknesses[element] += amount;
    }
    public void AdjustElementalResistance(ELEMENT element, float amount) {
        _elementalResistances[element] += amount;
    }
    public void AdjustSP(int amount) {
        _sp += amount;
        _sp = Mathf.Clamp(_sp, 0, _maxSP);
    }
    private void RecomputeMaxExperience() {
        _maxExperience = Mathf.CeilToInt(100f * ((Mathf.Pow((float) _level, 1.25f)) / 1.1f));
    }
    public void ResetToFullHP() {
        SetHP(maxHP);
    }
    public void ResetToFullSP() {
        AdjustSP(_maxSP);
    }
    private float GetComputedPower() {
        float compPower = 0f;
        for (int i = 0; i < currentParty.characters.Count; i++) {
            compPower += currentParty.characters[i].attackPower;
        }
        return compPower;
    }
    public void SetMaxHP(int amount) {
        int previousMaxHP = maxHP;
        _maxHP = amount;
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustMaxHP(int amount) {
        int previousMaxHP = maxHP;
        _maxHP += amount;
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    #endregion

    #region Player/Character Actions
    private void ConstructDefaultMiscActions() {
        _miscActions = new List<CharacterAction>();
        CharacterAction read = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.READ);
        CharacterAction foolingAround = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.FOOLING_AROUND);
        CharacterAction argue = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ARGUE);
        CharacterAction chat = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.CHAT);

        read.SetActionCategory(ACTION_CATEGORY.MISC);
        foolingAround.SetActionCategory(ACTION_CATEGORY.MISC);
        chat.SetActionCategory(ACTION_CATEGORY.MISC);
        argue.SetActionCategory(ACTION_CATEGORY.MISC);

        AddMiscAction(read);
        AddMiscAction(foolingAround);
        AddMiscAction(chat);
        AddMiscAction(argue);
        //_miscActions.Add(read);
        //_miscActions.Add(foolingAround);
        //_idleActions.Add(chat);
    }
    public CharacterAction GetRandomMiscAction(ref IObject targetObject) {
        targetObject = _ownParty.characterObject;
        CharacterAction chosenAction = _miscActions[UnityEngine.Random.Range(0, _miscActions.Count)];
        if (chosenAction is ChatAction) {
            List<CharacterParty> partyPool = new List<CharacterParty>();
            CharacterParty chosenParty = GetPriority1TargetChatAction(partyPool);
            if (chosenParty == null) {
                chosenParty = GetPriority2TargetChatAction(partyPool);
                if (chosenParty == null) {
                    chosenParty = GetPriority3TargetChatAction(partyPool);
                    if (chosenParty == null) {
                        chosenParty = GetPriority4TargetChatAction(partyPool);
                    }
                }
            }
            if (chosenParty == null) {
                _miscActions.Remove(chosenAction);
                CharacterAction newChosenAction = _miscActions[Utilities.rng.Next(0, _miscActions.Count)];
                _miscActions.Add(chosenAction);
                return newChosenAction;
            } else {
                targetObject = chosenParty.icharacterObject;
            }
        }
        return chosenAction;
    }
    public CharacterAction GetWeightedMiscAction(ref IObject targetObject, ref string actionLog) {
        WeightedDictionary<ActionAndTarget> miscActionOptions = new WeightedDictionary<ActionAndTarget>();
        for (int i = 0; i < _miscActions.Count; i++) {
            CharacterAction currentAction = _miscActions[i];
            if (currentAction.disableCounter <= 0 && currentAction.enableCounter > 0) {
                IObject currentTarget = currentAction.GetTargetObject(_ownParty);
                if (currentTarget != null) {
                    ActionAndTarget actionAndTarget = new ActionAndTarget() {
                        action = currentAction,
                        target = currentTarget
                    };
                    miscActionOptions.AddElement(actionAndTarget, currentAction.weight);
                }
            }
        }
        actionLog += "\n" + miscActionOptions.GetWeightsSummary("Misc Action Weights: ");
        ActionAndTarget chosen = miscActionOptions.PickRandomElementGivenWeights();
        targetObject = chosen.target;
        return chosen.action;
    }
    public CharacterAction GetMiscAction(ACTION_TYPE type) {
        for (int i = 0; i < _miscActions.Count; i++) {
            if (_miscActions[i].actionData.actionType == type) {
                return _miscActions[i];
            }
        }
        return null;
    }
    public void AddMiscAction(CharacterAction characterAction) {
        CharacterAction sameAction = GetMiscAction(characterAction.actionType);
        if (sameAction == null) {
            _miscActions.Add(characterAction);
            characterAction.AdjustEnableCounter(1);
            characterAction.OnAddActionToCharacter(this);
        } else {
            sameAction.AdjustEnableCounter(1);
        }
    }

    public void RemoveMiscAction(ACTION_TYPE actionType) {
        CharacterAction sameAction = GetMiscAction(actionType);
        if (sameAction != null) {
            if (sameAction.enableCounter > 1) {
                sameAction.AdjustEnableCounter(-1);
            } else {
                if (_miscActions.Remove(sameAction)) {
                    sameAction.OnRemoveActionFromCharacter(this);
                }
            }
        }
    }
    private CharacterParty GetPriority1TargetChatAction(List<CharacterParty> partyPool) {
        //random parties within same faction within settlements
        partyPool.Clear();
        for (int i = 0; i < faction.characters.Count; i++) {
            CharacterParty party = faction.characters[i].party;
            if (party.id != this._ownParty.id && party.characterObject.currentState.stateName == "Alive" && party.landmarkLocation != null && faction.ownedAreas.Contains(party.specificLocation.tileLocation.areaOfTile) && !partyPool.Contains(party)) {
                partyPool.Add(party);
            }
        }
        if (partyPool.Count > 0) {
            return partyPool[Utilities.rng.Next(0, partyPool.Count)];
        }
        return null;
    }
    private CharacterParty GetPriority2TargetChatAction(List<CharacterParty> partyPool) {
        //random parties within non-hostile factions within settlements
        partyPool.Clear();
        List<Faction> nonHostileFactions = FactionManager.Instance.GetFactionsWithByStatus(faction, FACTION_RELATIONSHIP_STATUS.ALLY);
        for (int i = 0; i < nonHostileFactions.Count; i++) {
            Faction nonHostileFaction = nonHostileFactions[i];
            for (int k = 0; k < nonHostileFaction.characters.Count; k++) {
                CharacterParty party = nonHostileFaction.characters[k].party;
                if (party.id != this._ownParty.id && party.characterObject.currentState.stateName == "Alive" && party.landmarkLocation != null && faction.ownedAreas.Contains(party.landmarkLocation.tileLocation.areaOfTile) && !partyPool.Contains(party)) {
                    partyPool.Add(party);
                }
            }
        }
        if (partyPool.Count > 0) {
            return partyPool[Utilities.rng.Next(0, partyPool.Count)];
        }
        return null;
    }
    private CharacterParty GetPriority3TargetChatAction(List<CharacterParty> partyPool) {
        //random parties within same faction outside settlements currently performing an Idle Action
        partyPool.Clear();
        for (int i = 0; i < faction.characters.Count; i++) {
            CharacterParty party = faction.characters[i].party;
            if (party.id != this._ownParty.id && party.characterObject.currentState.stateName == "Alive" && party.landmarkLocation != null
                && !faction.ownedAreas.Contains(party.landmarkLocation.tileLocation.areaOfTile) && !partyPool.Contains(party)
                && party.actionData.currentAction != null && party.actionData.currentAction.actionData.actionCategory == ACTION_CATEGORY.MISC) {
                partyPool.Add(party);
            }
        }
        if (partyPool.Count > 0) {
            return partyPool[Utilities.rng.Next(0, partyPool.Count)];
        }
        return null;
    }
    private CharacterParty GetPriority4TargetChatAction(List<CharacterParty> partyPool) {
        //random parties within non-hostile faction outside settlements currently performing an Idle Action
        partyPool.Clear();
        List<Faction> nonHostileFactions = FactionManager.Instance.GetFactionsWithByStatus(faction, FACTION_RELATIONSHIP_STATUS.ALLY);
        for (int i = 0; i < nonHostileFactions.Count; i++) {
            Faction nonHostileFaction = nonHostileFactions[i];
            for (int k = 0; k < nonHostileFaction.characters.Count; k++) {
                CharacterParty party = nonHostileFaction.characters[k].party;
                if (party.id != this._ownParty.id && party.characterObject.currentState.stateName == "Alive" && party.landmarkLocation != null
                    && !faction.ownedAreas.Contains(party.landmarkLocation.tileLocation.areaOfTile) && !partyPool.Contains(party)
                    && party.actionData.currentAction != null && party.actionData.currentAction.actionData.actionCategory == ACTION_CATEGORY.MISC) {
                    partyPool.Add(party);
                }
            }
        }
        if (partyPool.Count > 0) {
            return partyPool[Utilities.rng.Next(0, partyPool.Count)];
        }
        return null;
    }
    #endregion

    #region Home
    public void LookForNewHome() {
        //Try to get a new home structure from this character's area
        BaseLandmark landmark = GetNewHomeFromArea(_homeLandmark.tileLocation.areaOfTile);
        if (landmark != null) {
            _homeLandmark.RemoveCharacterHomeOnLandmark(this);
            landmark.AddCharacterHomeOnLandmark(this);
        } else {
            //If there is no available structure, look for it in other areas of the faction and migrate there
            landmark = GetNewHomeFromFaction();
            if (landmark != null) {
                //SetHome(landmark.tileLocation.areaOfTile);
                _homeLandmark.RemoveCharacterHomeOnLandmark(this);
                landmark.AddCharacterHomeOnLandmark(this);
            } else {
                //TODO: For future update, migrate to another friendly faction's structure
            }
        }
    }
    private BaseLandmark GetNewHomeFromArea(Area area) {
        BaseLandmark chosenLandmark = null;
        for (int i = 0; i < area.landmarks.Count; i++) {
            BaseLandmark landmark = area.landmarks[i];
            if (landmark != _homeLandmark && landmark.landmarkObj.specificObjectType == _homeLandmark.landmarkObj.specificObjectType) {
                if (chosenLandmark == null) {
                    chosenLandmark = landmark;
                } else {
                    if (landmark.charactersWithHomeOnLandmark.Count < chosenLandmark.charactersWithHomeOnLandmark.Count) {
                        chosenLandmark = landmark;
                    }
                }
            }
        }
        return chosenLandmark;
    }
    private BaseLandmark GetNewHomeFromFaction() {
        BaseLandmark chosenLandmark = null;
        if (_faction != null) {
            for (int i = 0; i < _faction.ownedAreas.Count; i++) {
                Area area = _faction.ownedAreas[i];
                if (area.id != _homeLandmark.tileLocation.areaOfTile.id) {
                    chosenLandmark = GetNewHomeFromArea(area);
                    if (chosenLandmark != null) {
                        break;
                    }
                }
            }
        }
        return chosenLandmark;
    }
    public void SetHomeLandmark(BaseLandmark newHomeLandmark, bool ignoreAreaResidentCapacity = false) {
        BaseLandmark previousHome = _homeLandmark;
        this._homeLandmark = newHomeLandmark;
        if (!(this is CharacterArmyUnit)) {
            if (previousHome != null) {
                previousHome.tileLocation.areaOfTile.RemoveResident(this);
                if (_homeLandmark != null) {
                    _homeLandmark.tileLocation.areaOfTile.AddResident(this, ignoreAreaResidentCapacity);
                    if (_homeLandmark.tileLocation.areaOfTile.id != previousHome.tileLocation.areaOfTile.id) {
#if !WORLD_CREATION_TOOL
                        LookForNewWorkplace();
#endif
                    }
                }

            } else {
                if (_homeLandmark != null) {
                    if (_homeLandmark.tileLocation.areaOfTile != null) {
                        _homeLandmark.tileLocation.areaOfTile.AddResident(this);
                    }
#if !WORLD_CREATION_TOOL
                    LookForNewWorkplace();
#endif
                }
            }
        }
    }
    #endregion

    #region Work
    public bool LookForNewWorkplace() {
        if (_characterClass.workActionType == ACTION_TYPE.WORKING) {
            _workplace = _homeLandmark;
            return true;
        } else {
            List<BaseLandmark> workplaceChoices = new List<BaseLandmark>();
            for (int i = 0; i < _homeLandmark.tileLocation.areaOfTile.landmarks.Count; i++) {
                StructureObj structure = _homeLandmark.tileLocation.areaOfTile.landmarks[i].landmarkObj;
                for (int j = 0; j < structure.currentState.actions.Count; j++) {
                    if (structure.currentState.actions[j].actionType == _characterClass.workActionType) {
                        workplaceChoices.Add(_homeLandmark.tileLocation.areaOfTile.landmarks[i]);
                        break;
                    }
                }
            }
            if (workplaceChoices.Count != 0) {
                _workplace = workplaceChoices[UnityEngine.Random.Range(0, workplaceChoices.Count)];
                return true;
            }
            //throw new Exception("Could not find workplace for " + this.name);
        }
        return false;
    }
    #endregion

    #region Action Queue
    public void AddActionToQueue(CharacterAction action, IObject targetObject, Quest associatedQuest = null, int position = -1) {
        if (position == -1) {
            //add action to end
            _actionQueue.Enqueue(new ActionQueueItem(action, targetObject, associatedQuest));
        } else {
            //Insert action to specified position
            _actionQueue.Enqueue(new ActionQueueItem(action, targetObject, associatedQuest), position);
        }
    }
    public void AddActionToQueue(EventAction eventAction, GameEvent associatedEvent, int position = -1) {
        if (position == -1) {
            //add action to end
            _actionQueue.Enqueue(new ActionQueueItem(eventAction.action, eventAction.targetObject, null, associatedEvent));
        } else {
            //Insert action to specified position
            _actionQueue.Enqueue(new ActionQueueItem(eventAction.action, eventAction.targetObject, null, associatedEvent), position);
        }
    }
    public void RemoveActionFromQueue(ActionQueueItem item) {
        _actionQueue.Remove(item);
    }
    public void RemoveActionFromQueue(CharacterAction action) {
        for (int i = 0; i < _actionQueue.Count; i++) {
            ActionQueueItem queueItem = _actionQueue.GetBasedOnIndex(i);
            if (queueItem.action == action) {
                _actionQueue.RemoveAt(i);
                break;
            }
        }
    }
    #endregion

    #region Schedule
    public void SetSchedule(CharacterSchedule schedule) {
        this.schedule = schedule;
        if (schedule != null) {
            schedule.Initialize();
        }
    }
    public void OnSchedulePhaseStarted(SCHEDULE_ACTION_CATEGORY startedPhase) {
        return; //disable scheduled movement
        Debug.Log(this.name + " started phase " + startedPhase.ToString());
        //once a new phase has started
        //check if it is a different phase from the one before it
        if (schedule.previousPhase != SCHEDULE_ACTION_CATEGORY.NONE && startedPhase != schedule.previousPhase) {//if it is
            if (_ownParty.actionData.currentAction != null && _ownParty.actionData.currentAction.actionType != ACTION_TYPE.DEFENDER) {
                _ownParty.actionData.EndAction(); //end the current action
                _ownParty.actionData.LookForAction(); //then look for a new action that is part of the phase category (Work/Rest)
            }
        }
        //if it is not, do nothing (continue current action)
    }
    public int GetWorkDeadlineTick() {
        return 0;
        //return this.dailySchedule.currentPhase.startTick + 7; //start of work phase + 1 hour(6 ticks) + 1 tick (because if other character arrives at exactly the work deadline, he/she will not be included in party, even though they are technically not late)
    }
    #endregion

    #region Event Schedule
    private GameEvent nextScheduledEvent;
    private DateRange nextScheduledEventDate;
    public void AddScheduledEvent(DateRange dateRange, GameEvent gameEvent) {
        Debug.Log("[" + GameManager.Instance.continuousDays + "]" + this.name + " will schedule an event to " + dateRange.ToString());
        if (eventSchedule.HasConflictingSchedule(dateRange)) {
            //There is a conflict in the current schedule of the character, move the new event to a new schedule.
            GameDate nextFreeDate = eventSchedule.GetNextFreeDateForEvent(gameEvent);
            GameDate endDate = nextFreeDate;
            endDate.AddDays(gameEvent.GetEventDurationRoughEstimateInTicks());
            DateRange newSched = new DateRange(nextFreeDate, endDate);
            eventSchedule.AddElement(newSched, gameEvent);
            Debug.Log("[" + GameManager.Instance.continuousDays + "]" + this.name + " has a conflicting schedule. Rescheduled event to " + newSched.ToString());
        } else {
            eventSchedule.AddElement(dateRange, gameEvent);
            Debug.Log("[" + GameManager.Instance.continuousDays + "]" + this.name + " added scehduled event " + gameEvent.name + " on " + dateRange.ToString());

            GameDate checkDate = dateRange.startDate;
            checkDate.ReduceHours(GameManager.hoursPerDay);
            if (checkDate.IsBefore(GameManager.Instance.Today())) { //if the check date is before today
                //start check on the next tick
                checkDate = GameManager.Instance.Today();
                checkDate.AddDays(1);
            }
            //Once event has been scheduled, schedule every tick checking 144 ticks before the start date of the new event
            SchedulingManager.Instance.AddEntry(checkDate, () => StartEveryTickCheckForEvent());
            Debug.Log(this.name + " scheduled every tick check for event " + gameEvent.name + " on " + checkDate.GetDayAndTicksString());
        }
    }
    public void AddScheduledEvent(GameDate startDate, GameEvent gameEvent) {
        GameDate endDate = startDate;
        endDate.AddDays(gameEvent.GetEventDurationRoughEstimateInTicks());

        DateRange dateRange = new DateRange(startDate, endDate);
        AddScheduledEvent(dateRange, gameEvent);
    }
    public void AddScheduledEvent(GameEvent gameEvent) {
        //schedule a game event without specifing a date
        GameDate nextFreeDate = eventSchedule.GetNextFreeDateForEvent(gameEvent);
        GameDate endDate = nextFreeDate;
        endDate.AddDays(gameEvent.GetEventDurationRoughEstimateInTicks());
        DateRange newSched = new DateRange(nextFreeDate, endDate);
        AddScheduledEvent(newSched, gameEvent);
    }
    public void ForceEvent(GameEvent gameEvent) {
        //if we need to force an event to happen (i.e. Defend Action for Monster Attacks Event)
        //
    }
    public bool HasEventScheduled(GameDate date) {
        return eventSchedule[date] != null;
    }
    public bool HasEventScheduled(GAME_EVENT eventType) {
        return eventSchedule.HasEventOfType(eventType);
    }
    public GameEvent GetScheduledEvent(GameDate date) {
        return eventSchedule[date];
    }
    private void StartEveryTickCheckForEvent() {
        nextScheduledEvent = eventSchedule.GetNextEvent(); //Set next game event variable to the next event, to prevent checking the schecule every tick
        nextScheduledEventDate = eventSchedule.GetDateRangeForEvent(nextScheduledEvent);
        Debug.Log(this.name + " started checking every tick for event " + nextScheduledEvent.name);
        Messenger.AddListener(Signals.DAY_ENDED, EventEveryTick); //Add every tick listener for events
    }
    private void EventEveryTick() {
        HexTile currentLoc = this.currentParty.specificLocation.tileLocation; //check the character's current location
        EventAction nextEventAction = nextScheduledEvent.PeekNextEventAction(this);
        int travelTime = PathGenerator.Instance.GetTravelTimeInTicks(this.specificLocation, nextEventAction.targetLocation, PATHFINDING_MODE.PASSABLE);

        GameDate eventArrivalDate = GameManager.Instance.Today();
        eventArrivalDate.AddDays(travelTime); //given the start date and the travel time, check if the character has to leave now to reach the event in time
        if (!eventArrivalDate.IsBefore(nextScheduledEventDate.startDate) && !party.actionData.isCurrentActionFromEvent) { //if the estimated arrival date is NOT before the next events' scheduled start date
            if (party.actionData.isCurrentActionFromEvent) { //if this character's current action is from an event, do not perform the action from the next scheduled event.
                Debug.LogWarning(this.name + " did not perform the next event action, since their current action is already from an event");
            } else { //else if this character's action is not from an event
                     //leave now and do the event action
                AddActionToQueue(nextScheduledEvent.GetNextEventAction(this), nextScheduledEvent); //queue the event action
                _ownParty.actionData.EndCurrentAction();  //then end their current action
            }
            Messenger.RemoveListener(Signals.DAY_ENDED, EventEveryTick);
            Debug.Log(GameManager.Instance.TodayLogString() + this.name + " stopped checking every tick for event " + nextScheduledEvent.name);
            nextScheduledEvent = null;

        }
    }
    #endregion

    #region IInteractable
    public void SetIsBeingInspected(bool state) {
        _isBeingInspected = state;
        if (_currentParty.icon != null) {
            _currentParty.icon.UpdateVisualState();
        }
        //if (_currentParty.specificLocation != null && _currentParty.specificLocation.tileLocation.landmarkOnTile != null) {
        //    _currentParty.specificLocation.tileLocation.landmarkOnTile.landmarkVisual.ToggleCharactersVisibility();
        //}
    }
    public void SetHasBeenInspected(bool state) {
        _hasBeenInspected = state;
    }
    public void EndedInspection() {
        uiData.UpdateData(this);
    }
    public void AddInteraction(Interaction interaction) {
        //_currentInteractions.Add(interaction);
        interaction.SetCharacterInvolved(this);
        interaction.interactable.AddInteraction(interaction);
        //interaction.Initialize(this);
        //Messenger.Broadcast(Signals.ADDED_INTERACTION, this as IInteractable, interaction);
    }
    public void RemoveInteraction(Interaction interaction) {
        //if (_currentInteractions.Remove(interaction)) {
        interaction.interactable.RemoveInteraction(interaction);
        //Messenger.Broadcast(Signals.REMOVED_INTERACTION, this as IInteractable, interaction);
        //}
    }
    #endregion

    #region Defender
    public void OnSetAsDefender(Area defending) {
        defendingArea = defending;
        this.ownParty.specificLocation.RemoveCharacterFromLocation(this.ownParty, false);
        ownParty.SetSpecificLocation(defending.coreTile.landmarkOnTile);
#if !WORLD_CREATION_TOOL
        if (ownParty is CharacterParty) {
            (ownParty as CharacterParty).actionData.ForceDoAction(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.DEFENDER), defending.coreTile.landmarkOnTile.landmarkObj);
        }
#endif
    }
    public void OnRemoveAsDefender() {
        defendingArea.coreTile.landmarkOnTile.AddCharacterToLocation(this.ownParty);
        defendingArea = null;
#if !WORLD_CREATION_TOOL
        _ownParty.actionData.EndCurrentAction(); //end the defender action
#endif
    }
    public bool IsDefending(BaseLandmark landmark) {
        if (defendingArea != null && defendingArea.id == landmark.id) {
            return true;
        }
        return false;
    }
    #endregion

    #region Combat Attributes
    public void AddTrait(Trait trait) {
        if (GetTrait(trait.name) == null) {
            _traits.Add(trait);
            ApplyFlatTraitEffects(trait);
            if(trait.daysDuration > 0) {
                GameDate removeDate = GameManager.Instance.Today();
                removeDate.AddDays(trait.daysDuration);
                SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTrait(trait));
            }
        }
    }
    public bool RemoveTrait(Trait trait) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].name == trait.name) {
                _traits.RemoveAt(i);
                UnapplyFlatTraitEffects(trait);
                return true;
            }
        }
        return false;
    }
    public Trait GetTrait(string traitName) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].name == traitName) {
                return _traits[i];
            }
        }
        return null;
    }
    public Trait GetRandomNegativeTrait() {
        List<Trait> negativeTraits = new List<Trait>();
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].type == TRAIT_TYPE.NEGATIVE) {
                negativeTraits.Add(_traits[i]);
            }
        }
        if (negativeTraits.Count > 0) {
            return negativeTraits[UnityEngine.Random.Range(0, negativeTraits.Count)];
        }
        return null;
    }
    private void ApplyFlatTraitEffects(Trait trait) {
        for (int i = 0; i < trait.effects.Count; i++) {
            TraitEffect traitEffect = trait.effects[i];
            if (!traitEffect.hasRequirement && !traitEffect.isPercentage && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                if (traitEffect.stat == STAT.ATTACK) {
                    _attackPower += (int) traitEffect.amount;
                } else if (traitEffect.stat == STAT.HP) {
                    AdjustMaxHP((int) traitEffect.amount);
                } else if (traitEffect.stat == STAT.SPEED) {
                    _speed += (int) traitEffect.amount;
                }
            }
        }
    }
    private void UnapplyFlatTraitEffects(Trait trait) {
        for (int i = 0; i < trait.effects.Count; i++) {
            TraitEffect traitEffect = trait.effects[i];
            if (!traitEffect.hasRequirement && !traitEffect.isPercentage && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                if (traitEffect.stat == STAT.ATTACK) {
                    _attackPower -= (int) traitEffect.amount;
                } else if (traitEffect.stat == STAT.HP) {
                    AdjustMaxHP(-(int) traitEffect.amount);
                } else if (traitEffect.stat == STAT.SPEED) {
                    _speed -= (int) traitEffect.amount;
                }
            }
        }
    }
    private int GetModifiedAttack() {
        float modifier = 0f;
        for (int i = 0; i < _traits.Count; i++) {
            for (int j = 0; j < _traits[i].effects.Count; j++) {
                TraitEffect traitEffect = _traits[i].effects[j];
                if (traitEffect.stat == STAT.ATTACK && !traitEffect.hasRequirement && traitEffect.isPercentage && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                    modifier += traitEffect.amount;
                }
            }
        }
        return (int) (_attackPower * (modifier / 100f));
    }
    private int GetModifiedSpeed() {
        float modifier = 0f;
        for (int i = 0; i < _traits.Count; i++) {
            for (int j = 0; j < _traits[i].effects.Count; j++) {
                TraitEffect traitEffect = _traits[i].effects[j];
                if (traitEffect.stat == STAT.SPEED && !traitEffect.hasRequirement && traitEffect.isPercentage && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                    modifier += traitEffect.amount;
                }
            }
        }
        return (int) (_speed * (modifier / 100f));
    }
    private int GetModifiedHP() {
        float modifier = 0f;
        for (int i = 0; i < _traits.Count; i++) {
            for (int j = 0; j < _traits[i].effects.Count; j++) {
                TraitEffect traitEffect = _traits[i].effects[j];
                if (traitEffect.stat == STAT.HP && !traitEffect.hasRequirement && traitEffect.isPercentage && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                    modifier += traitEffect.amount;
                }
            }
        }
        return (int) (_maxHP * (modifier / 100f));
    }
    private void SetTraitsFromClass() {
        if (_characterClass.traitNames != null) {
            for (int i = 0; i < _characterClass.traitNames.Length; i++) {
                Trait trait = AttributeManager.Instance.allTraits[_characterClass.traitNames[i]];
                AddTrait(trait);
            }
        }
    }
    private void SetTraitsFromRace() {
        if (_raceSetting.traitNames != null) {
            for (int i = 0; i < _raceSetting.traitNames.Length; i++) {
                Trait trait = AttributeManager.Instance.allTraits[_raceSetting.traitNames[i]];
                AddTrait(trait);
            }
        }
    }
    #endregion

    #region Morality
    public void SetMorality(MORALITY morality) {
        this.morality = morality;
    }
    #endregion

    #region Minion
    public void SetMinion(Minion minion) {
        _minion = minion;
        UnsubscribeSignals();
    }
    public void RecruitAsMinion() {
        if (!IsInOwnParty()) {
            _currentParty.RemoveCharacter(this);
        }
        _homeLandmark.RemoveCharacterHomeOnLandmark(this);
        PlayerManager.Instance.player.demonicPortal.AddCharacterHomeOnLandmark(this);

        specificLocation.RemoveCharacterFromLocation(this.currentParty);
        PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(this.currentParty);

        faction.RemoveCharacter(this);
        PlayerManager.Instance.player.playerFaction.AddNewCharacter(this);

        Minion newMinion = PlayerManager.Instance.player.CreateNewMinion(this);
        PlayerManager.Instance.player.AddMinion(newMinion);

        if (!characterToken.isObtainedByPlayer) {
            PlayerManager.Instance.player.AddToken(characterToken);
        }
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

    #region Interaction
    public void DisableInteractionGeneration() {
        Messenger.RemoveListener(Signals.DAY_STARTED, DailyInteractionGeneration);
    }
    public void AddInteractionWeight(INTERACTION_TYPE type, int weight) {
        interactionWeights.AddElement(type, weight);
    }
    public void RemoveInteractionFromWeights(INTERACTION_TYPE type, int weight) {
        interactionWeights.RemoveElement(type);
    }
    public void SetDailyInteractionGenerationTick() {
        int remainingDaysInWeek = GameManager.Instance.continuousDays % 7;
        int startDay = GameManager.Instance.continuousDays + remainingDaysInWeek + 1;
        _currentInteractionTick = UnityEngine.Random.Range(startDay, startDay + 7);
    }
    public void DailyInteractionGeneration() {
        if (_currentInteractionTick == GameManager.Instance.continuousDays) {
            //if(job.jobType != JOB.NONE) {
            //    job.CreateRandomInteractionForNonMinionCharacters();
            //}
            GenerateDailyInteraction();
            SetDailyInteractionGenerationTick();
        } else if (_currentInteractionTick > GameManager.Instance.continuousDays) {
            SetDailyInteractionGenerationTick();
        }
    }
    public void GenerateDailyInteraction() {
        if (!IsInOwnParty() || isDefender || ownParty.icon.isTravelling || _doNotDisturb || _job == null) {
            return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
        }
        if (job.jobType == JOB.NONE) {
            return;
            //_job.CreateRandomInteractionForNonMinionCharacters();
        }
        string interactionLog = GameManager.Instance.TodayLogString() + "Generating daily interaction for " + this.name;
        if (_forcedInteraction != null) {
            interactionLog += "\nUsing forced interaction: " + _forcedInteraction.type.ToString();
            if(_forcedInteraction.CanInteractionBeDone()) {
                AddInteraction(_forcedInteraction);
            } else {
                interactionLog += "\nCan't do forced interaction: " + _forcedInteraction.type.ToString();
            }
            _forcedInteraction = null;
        } else {
            //Only go here if away from home, then choose from the character interactions away from home
            //TODO
            if(InteractionManager.Instance.CanCreateInteraction(INTERACTION_TYPE.RETURN_HOME, this)) {
                WeightedDictionary<string> awayFromHomeInteractionWeights = new WeightedDictionary<string>();
                awayFromHomeInteractionWeights.AddElement("Return", 100);

                for (int i = 0; i < tokenInventory.Count; i++) {
                    if (tokenInventory[i].CanBeUsed()) {
                        awayFromHomeInteractionWeights.AddElement(tokenInventory[i].tokenName, 100);
                    }
                }
                string result = awayFromHomeInteractionWeights.PickRandomElementGivenWeights();
                if(result == "Return") {
                    Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, specificLocation as BaseLandmark);
                    AddInteraction(interaction);
                } else {
                    UseItemOnCharacter interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.USE_ITEM_ON_CHARACTER, specificLocation as BaseLandmark) as UseItemOnCharacter;
                    interaction.SetItemToken(GetTokenByName(result));
                    AddInteraction(interaction);
                }
            }


            //int chance = UnityEngine.Random.Range(0, 100);
            //if(chance >= 15) {
            //    //Character will not perform
            //    return;
            //}
            //WeightedDictionary<INTERACTION_TYPE> validInteractions = GetValidInteractionWeights();
            //if (validInteractions != null) {
            //    if (validInteractions.GetTotalOfWeights() > 0) {
            //        interactionLog += "\n" + validInteractions.GetWeightsSummary("Generating interaction:");
            //        INTERACTION_TYPE chosenInteraction = validInteractions.PickRandomElementGivenWeights();
            //        //create interaction of type
            //        BaseLandmark interactable = specificLocation as BaseLandmark;
            //        if (interactable == null) {
            //            throw new Exception(GameManager.Instance.TodayLogString() + this.name + "'s specific location (" + specificLocation.locationName + ") is not a landmark!");
            //        }
            //        Interaction createdInteraction = InteractionManager.Instance.CreateNewInteraction(chosenInteraction, specificLocation as BaseLandmark);

            //        if (job.jobType == JOB.LEADER) {
            //            //For Faction Upgrade Interaction Only
            //            Area area = _homeLandmark.tileLocation.areaOfTile;
            //            area.AdjustSuppliesInBank(-100);
            //            createdInteraction.SetMinionSuccessAction(() => area.AdjustSuppliesInBank(100));
            //        }

            //        AddInteraction(createdInteraction);
            //    } else {
            //        interactionLog += "\nCannot generate interaction because weights are not greater than zero";
            //    }
            //} else {
            //    interactionLog += "\nCannot generate interaction because there are no interactions for job: " + job.jobType.ToString();
            //}
        }
        //Debug.Log(interactionLog);
    }
    public void SetForcedInteraction(Interaction interaction) {
        _forcedInteraction = interaction;
    }
    private void DefaultAllExistingInteractions() {
        for (int i = 0; i < _currentInteractions.Count; i++) {
            if (!_currentInteractions[i].hasActivatedTimeOut) {
                _currentInteractions[i].TimedOutRunDefault();
                i--;
            }
        }
    }
    public Interaction GetInteractionOfType(INTERACTION_TYPE type) {
        for (int i = 0; i < _currentInteractions.Count; i++) {
            Interaction currInteraction = _currentInteractions[i];
            if (currInteraction.type == type) {
                return currInteraction;
            }
        }
        return null;
    }
    private WeightedDictionary<INTERACTION_TYPE> GetValidInteractionWeights() {
        List<CharacterInteractionWeight> jobInteractions = InteractionManager.Instance.GetJobNPCInteractionWeights(job.jobType);
        WeightedDictionary<INTERACTION_TYPE> weights = new WeightedDictionary<INTERACTION_TYPE>();
        if (jobInteractions != null) {
            for (int i = 0; i < jobInteractions.Count; i++) {
                if (GetInteractionOfType(jobInteractions[i].interactionType) == null && InteractionManager.Instance.CanCreateInteraction(jobInteractions[i].interactionType, this)) {
                    weights.AddElement(jobInteractions[i].interactionType, jobInteractions[i].weight);
                }
            }
        }
        if (InteractionManager.Instance.CanCreateInteraction(INTERACTION_TYPE.RETURN_HOME, this)) {
            weights.AddElement(INTERACTION_TYPE.RETURN_HOME, 10);
        }
        return weights;
    }
    private void AddDefaultInteractions() {
        List<CharacterInteractionWeight> defaultInteractions = InteractionManager.Instance.GetDefaultInteractionWeightsForRole(this.role.roleType);
        if (defaultInteractions != null) {
            for (int i = 0; i < defaultInteractions.Count; i++) {
                CharacterInteractionWeight currWeight = defaultInteractions[i];
                interactionWeights.AddElement(currWeight.interactionType, currWeight.weight);
            }
        }
    }
    public void ClaimReward(Reward reward) {
        switch (reward.rewardType) {
            case REWARD.LEVEL:
            LevelUp(reward.amount);
            break;
            case REWARD.SUPPLY:
            if(minion != null) {
                PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, reward.amount);
            } else {
                homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(reward.amount);
            }
            break;
            default:
            break;
        }
    }
    #endregion

    #region Token Inventory
    public void ObtainToken(SpecialToken token) {
        if (!tokenInventory.Contains(token)) {
            tokenInventory.Add(token);
            token.AdjustQuantity(-1);
        }
    }
    public void ConsumeToken(SpecialToken token) {
        tokenInventory.Remove(token);
    }
    public SpecialToken GetTokenByName(string name) {
        for (int i = 0; i < tokenInventory.Count; i++) {
            if (tokenInventory[i].tokenName == name) {
                return tokenInventory[i];
            }
        }
        return null;
    }
    #endregion
}
