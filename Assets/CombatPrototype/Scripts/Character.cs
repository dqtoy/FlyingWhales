using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

public class Character : ICharacter, ILeader, IInteractable, IPointOfInterest {
    public delegate void OnCharacterDeath();
    public OnCharacterDeath onCharacterDeath;

    public delegate void DailyAction();
    public DailyAction onDailyAction;

    protected string _name;
    protected string _firstName;
    protected string _characterColorCode;
    protected int _id;
    protected int _gold;
    protected int _currentInteractionTick;
    protected int _doNotDisturb;
    protected int _doNotGetHungry;
    protected int _doNotGetTired;
    protected int _doNotGetLonely;
    protected int _numOfWaitingForGoapThread;
    protected float _actRate;
    protected bool _isDead;
    protected bool _isFainted;
    protected bool _isInCombat;
    protected bool _isBeingInspected;
    protected bool _hasBeenInspected;
    protected bool _alreadyTargetedByGrudge;
    protected bool _isTracked;
    protected bool _activateDailyGoapPlanInteraction;
    protected bool _hasAlreadyAskedForPlan;
    protected bool _isChatting;
    protected GENDER _gender;
    protected MODE _currentMode;
    protected CharacterClass _characterClass;
    protected RaceSetting _raceSetting;
    protected CharacterRole _role;
    protected Job _job;
    protected Faction _faction;
    protected CharacterParty _ownParty;
    protected CharacterParty _currentParty;
    protected Region _currentRegion;
    protected Weapon _equippedWeapon;
    protected Armor _equippedArmor;
    protected Item _equippedAccessory;
    protected Item _equippedConsumable;
    protected PortraitSettings _portraitSettings;
    protected Color _characterColor;
    protected Minion _minion;
    protected Interaction _forcedInteraction;
    protected CombatCharacter _currentCombatCharacter;
    protected PairCombatStats[] _pairCombatStats;
    protected List<Skill> _skills;
    protected List<Log> _history;
    protected List<Trait> _traits;
    protected List<Interaction> _currentInteractions;
    protected Dictionary<ELEMENT, float> _elementalWeaknesses;
    protected Dictionary<ELEMENT, float> _elementalResistances;
    protected PlayerCharacterItem _playerCharacterItem;

    //Stats
    protected SIDES _currentSide;
    protected int _currentHP;
    protected int _currentRow;
    protected int _level;
    protected int _experience;
    protected int _maxExperience;
    protected int _sp;
    protected int _maxSP;
    protected int _attackPowerMod;
    protected int _speedMod;
    protected int _maxHPMod;
    protected int _attackPowerPercentMod;
    protected int _speedPercentMod;
    protected int _maxHPPercentMod;
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

    public Area homeArea { get; protected set; }
    public Dwelling homeStructure { get; protected set; }
    public LocationStructure currentStructure { get; private set; } //what structure is this character currently in.
    public Area defendingArea { get; private set; }
    public MORALITY morality { get; private set; }
    public CharacterToken characterToken { get; private set; }
    public WeightedDictionary<INTERACTION_TYPE> interactionWeights { get; private set; }
    public Dictionary<Character, CharacterRelationshipData> relationships { get; private set; }
    public List<INTERACTION_TYPE> currentInteractionTypes { get; private set; }
    public Interaction plannedInteraction { get; private set; }
    public Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>> awareness { get; private set; }
    public List<INTERACTION_TYPE> poiGoapActions { get; private set; }
    public List<GoapPlan> allGoapPlans { get; private set; }
    public GoapPlanner planner { get; set; }
    public int supply { get; set; }
    public int isWaitingForInteraction { get; private set; }
    public CharacterMarker marker { get; private set; }
    public GoapAction currentAction { get; private set; }
    public bool hasAssaultPlan { get; private set; }
    public Character lastAssaultedCharacter { get; private set; }
    public List<GoapAction> targettedByAction { get; private set; }
    public CharacterStateComponent stateComponent { get; private set; }
    public List<SpecialToken> items { get; private set; }
    public JobQueue jobQueue { get; private set; }
    public JobQueueItem currentJob { get; private set; }
    public List<JobQueueItem> allJobsTargettingThis { get; private set; }
    public int moodValue { get; private set; }

    private LocationGridTile tile; //what tile in the structure is this character currently in.
    private POI_STATE _state;

    private Dictionary<STAT, float> _buffs;
    public Dictionary<int, Combat> combatHistory;

    //Needs
    public int tiredness { get; private set; }
    private const int TIREDNESS_DEFAULT = 1200;
    private const int TIREDNESS_THRESHOLD_1 = 1040;
    private const int TIREDNESS_THRESHOLD_2 = 880;
    public int fullness { get; private set; }
    private const int FULLNESS_DEFAULT = 1440;
    private const int FULLNESS_THRESHOLD_1 = 1390;
    private const int FULLNESS_THRESHOLD_2 = 1200;
    public int happiness { get; private set; }
    private const int HAPPINESS_DEFAULT = 240;
    private const int HAPPINESS_THRESHOLD_1 = 160;
    private const int HAPPINESS_THRESHOLD_2 = 0;

    //portrait
    public float hSkinColor { get; private set; }
    public float hHairColor { get; private set; }
    public float demonColor { get; private set; }

    //hostility
    public int ignoreHostility { get; private set; }

    //For Testing
    public List<string> locationHistory { get; private set; }
    public List<string> actionHistory { get; private set; }

    #region getters / setters
    public string firstName {
        get { return _firstName; }
    }
    public virtual string name {
        get {
            //if(_minion != null) {
            //    return _minion.name;
            //}
            return _firstName;
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
    public string raceClassName {
        get {
            if (Utilities.IsRaceBeast(race)) {
                return Utilities.NormalizeString(race.ToString()) + " " + role.name;
            }
            //if(role.name == characterClass.className) {
                return Utilities.GetNormalizedRaceAdjective(race) + " " + characterClass.className;
            //}
            //return Utilities.GetNormalizedRaceAdjective(race) + " " + role.name + " " + characterClass.className;
        }
    }
    public int id {
        get { return _id; }
    }
    public bool isDead {
        get { return this._isDead; }
    }
    public bool isFainted {
        get { return this._isFainted; }
    }
    public bool isInCombat {
        get {
            return _isInCombat;
        }
    }
    public bool isFactionless { //is the character part of the neutral faction? or no faction?
        get {
            if (faction == null || FactionManager.Instance.neutralFaction.id == faction.id) {
                return true;
            } else {
                return false;
            }
        }
    }
    public bool isIdle {
        get { return _forcedInteraction == null && _doNotDisturb <= 0 && IsInOwnParty() && !currentParty.icon.isTravelling; }
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
        get { return job.jobType == JOB.LEADER; }
    }
    public bool isHoldingItem {
        get { return items.Count > 0; }
    }
    public bool isAtHomeStructure {
        get { return currentStructure == homeStructure; }
    }
    public bool isAtHomeArea {
        get { return specificLocation.id == homeArea.id; }
    }
    public bool isTracked {
        get {
            if (GameManager.Instance.inspectAll) {
                return true;
            }
            return _isTracked;
        }
    }
    public bool isChatting {
        get { return _isChatting; }
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
    public Faction factionOwner {
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
    public HexTile currLocation {
        get { return (_currentParty.specificLocation != null ? _currentParty.specificLocation.coreTile : null); }
    }
    public Area specificLocation {
        get { return _currentParty.specificLocation; }
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
    public Color characterColor {
        get { return _characterColor; }
    }
    public string characterColorCode {
        get { return _characterColorCode; }
    }
    public List<Log> history {
        get { return this._history; }
    }
    public int gold {
        get { return _gold; }
    }
    public PortraitSettings portraitSettings {
        get { return _portraitSettings; }
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
        get {
            int total = (int) ((_characterClass.baseSpeed + _speedMod) * (1f + ((_raceSetting.speedModifier + _speedPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int attackPower {
        get {
            int total = (int) ((_characterClass.baseAttackPower + _attackPowerMod) * (1f + ((_raceSetting.attackPowerModifier + _attackPowerPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int maxHP {
        get {
            int total = (int) ((_characterClass.baseHP + _maxHPMod) * (1f + ((_raceSetting.hpModifier + _maxHPPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    //public int combatBaseAttack {
    //    get { return _combatBaseAttack; }
    //    set { _combatBaseAttack = value; }
    //}
    //public int combatBaseSpeed {
    //    get { return _combatBaseSpeed; }
    //    set { _combatBaseSpeed = value; }
    //}
    //public int combatBaseHP {
    //    get { return _combatBaseHP; }
    //    set { _combatBaseHP = value; }
    //}
    //public int combatAttackFlat {
    //    get { return _combatAttackFlat; }
    //    set { _combatAttackFlat = value; }
    //}
    //public int combatAttackMultiplier {
    //    get { return _combatAttackMultiplier; }
    //    set { _combatAttackMultiplier = value; }
    //}
    //public int combatSpeedFlat {
    //    get { return _combatSpeedFlat; }
    //    set { _combatSpeedFlat = value; }
    //}
    //public int combatSpeedMultiplier {
    //    get { return _combatSpeedMultiplier; }
    //    set { _combatSpeedMultiplier = value; }
    //}
    //public int combatHPFlat {
    //    get { return _combatHPFlat; }
    //    set { _combatHPFlat = value; }
    //}
    //public int combatHPMultiplier {
    //    get { return _combatHPMultiplier; }
    //    set { _combatHPMultiplier = value; }
    //}
    //public int combatPowerFlat {
    //    get { return _combatPowerFlat; }
    //    set { _combatPowerFlat = value; }
    //}
    //public int combatPowerMultiplier {
    //    get { return _combatPowerMultiplier; }
    //    set { _combatPowerMultiplier = value; }
    //}
    public int currentHP {
        get { return this._currentHP; }
    }
    //public PairCombatStats[] pairCombatStats {
    //    get { return _pairCombatStats; }
    //    set { _pairCombatStats = value; }
    //}
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
    public float computedPower {
        get { return GetComputedPower(); }
    }
    public ICHARACTER_TYPE icharacterType {
        get { return ICHARACTER_TYPE.CHARACTER; }
    }
    public Minion minion {
        get { return _minion; }
    }
    public int doNotDisturb {
        get { return _doNotDisturb; }
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
    public Interaction forcedInteraction {
        get { return _forcedInteraction; }
    }
    public int currentInteractionTick {
        get { return _currentInteractionTick; }
    }
    public CombatCharacter currentCombatCharacter {
        get { return _currentCombatCharacter; }
    }
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.CHARACTER; }
    }
    public LocationGridTile gridTileLocation {
        get {
            return GetLocationGridTileByXY(gridTilePosition.x, gridTilePosition.y);
            //if (tile == null) {
            //    LocationGridTile gridTile = specificLocation.areaMap.map[(int) marker.anchoredPos.x, (int) marker.anchoredPos.y];
            //    return gridTile;
            //}
            //return tile;
        }
    }
    public Vector2Int gridTilePosition {
        get { return new Vector2Int((int) marker.anchoredPos.x, (int) marker.anchoredPos.y); }
    }
    public POI_STATE state {
        get { return _state; }
    }
    public POICollisionTrigger collisionTrigger {
        get { return marker.collisionTrigger; }
    }
    public CHARACTER_MOOD currentMoodType {
        get { return ConvertCurrentMoodValueToType(); }
    }
    #endregion

    public Character(CharacterRole role, RACE race, GENDER gender) : this() {
        _id = Utilities.SetID(this);
        _gender = gender;
        SetRace(race);
        //_raceSetting = RaceManager.Instance.racesDictionary[race.ToString()].CreateNewCopy();
        AssignRole(role);
        AssignClassByRole(role);
        SetName(RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender));
        //_portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
        AssignRandomJob();
        SetMorality(MORALITY.GOOD);
        //SetTraitsFromRace();
        ResetToFullHP();
    }
    public Character(CharacterRole role, string className, RACE race, GENDER gender) : this() {
        _id = Utilities.SetID(this);
        _gender = gender;
        SetRace(race);
        //_raceSetting = RaceManager.Instance.racesDictionary[race.ToString()].CreateNewCopy();
        AssignRole(role);
        AssignClass(className);
        SetName(RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender));
        //_portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
        AssignRandomJob();
        SetMorality(MORALITY.GOOD);
        //SetTraitsFromRace();
        ResetToFullHP();
    }
    public Character(CharacterSaveData data) : this() {
        _id = Utilities.SetID(this, data.id);
        _gender = data.gender;
        SetRace(race);
        //_raceSetting = RaceManager.Instance.racesDictionary[data.race.ToString()].CreateNewCopy();
        AssignRole(data.role);
        AssignClass(data.className);
        SetName(data.name);
        //_portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
        //if (_characterClass.roleType != CHARACTER_ROLE.NONE) {
        //    AssignRole(_characterClass.roleType);
        //}
        AssignRandomJob();
        SetMorality(data.morality);
        //SetTraitsFromRace();
        ResetToFullHP();
    }
    public Character() {
        SetIsDead(false);
        _isFainted = false;
        _history = new List<Log>();
        _traits = new List<Trait>();

        //RPG
        _level = 1;
        _experience = 0;
        _elementalWeaknesses = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
        _elementalResistances = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
        combatHistory = new Dictionary<int, Combat>();
        _currentInteractions = new List<Interaction>();
        currentInteractionTypes = new List<INTERACTION_TYPE>();
        characterToken = new CharacterToken(this);
        interactionWeights = new WeightedDictionary<INTERACTION_TYPE>();
        relationships = new Dictionary<Character, CharacterRelationshipData>();
        poiGoapActions = new List<INTERACTION_TYPE>();
        allGoapPlans = new List<GoapPlan>();
        hasAssaultPlan = false;
        targettedByAction = new List<GoapAction>();
        stateComponent = new CharacterStateComponent(this);
        items = new List<SpecialToken>();
        jobQueue = new JobQueue();
        allJobsTargettingThis = new List<JobQueueItem>();
        SetMoodValue(90);

        tiredness = TIREDNESS_DEFAULT;
        //Fullness value between 1300 and 1440.
        SetFullness(UnityEngine.Random.Range(1300, FULLNESS_DEFAULT + 1));
        //Happiness value between 100 and 240.
        SetHappiness(UnityEngine.Random.Range(100, HAPPINESS_DEFAULT + 1));

        hSkinColor = UnityEngine.Random.Range(-360f, 360f);
        hHairColor = UnityEngine.Random.Range(-360f, 360f);
        demonColor = UnityEngine.Random.Range(-144f, 144f);

        //for testing
        locationHistory = new List<string>();
        actionHistory = new List<string>();

        //If this is a minion, this should not be initiated
        awareness = new Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>>();
        planner = new GoapPlanner(this);

        //supply
        SetSupply(UnityEngine.Random.Range(10, 61)); //Randomize initial supply per character (Random amount between 10 to 60.)

        //hostiltiy
        ignoreHostility = 0;

        GetRandomCharacterColor();
#if !WORLD_CREATION_TOOL
        //SetDailyInteractionGenerationTick(); //UnityEngine.Random.Range(1, 13)
#endif
        ConstructInitialGoapAdvertisementActions();
        SubscribeToSignals();
        //StartDailyGoapPlanGeneration();
        GameDate gameDate = GameManager.Instance.Today();
        gameDate.AddTicks(1);
        SchedulingManager.Instance.AddEntry(gameDate, () => PlanGoapActions());
    }
    public void Initialize() {
    }

    #region Signals
    private void SubscribeToSignals() {
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        //Messenger.AddListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        Messenger.AddListener(Signals.TICK_STARTED, DailyGoapPlanGeneration);
        Messenger.AddListener(Signals.HOUR_STARTED, DecreaseNeeds);
        Messenger.AddListener<Character, Area, Area>(Signals.CHARACTER_MIGRATED_HOME, OnCharacterMigratedHome);
        Messenger.AddListener<Interaction>(Signals.INTERACTION_ENDED, OnInteractionEnded);
        Messenger.AddListener(Signals.DAY_STARTED, DayStartedRemoveOverrideInteraction);
        Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnLeaveArea);
        Messenger.AddListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnArrivedAtArea);
    }
    public void UnsubscribeSignals() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        //Messenger.RemoveListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        Messenger.RemoveListener(Signals.TICK_STARTED, DailyGoapPlanGeneration);
        Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseNeeds);
        Messenger.RemoveListener<Character, Area, Area>(Signals.CHARACTER_MIGRATED_HOME, OnCharacterMigratedHome);
        Messenger.RemoveListener(Signals.DAY_STARTED, DayStartedRemoveOverrideInteraction);
        Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnLeaveArea);
        Messenger.RemoveListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnArrivedAtArea);
    }
    #endregion

    public void SetCharacterMarker(CharacterMarker marker) {
        this.marker = marker;
    }
    public void ShowTileData(Character character, LocationGridTile location) {
        InteriorMapManager.Instance.ShowTileData(this, gridTileLocation);
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
    public virtual void AdjustHP(int amount, bool triggerDeath = false) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, maxHP);
        Messenger.Broadcast(Signals.ADJUSTED_HP, this);
        if (triggerDeath && previous != this._currentHP) {
            if (this._currentHP == 0) {
                Death();
            }
        }
    }

    private string GetFaintOrDeath() {
        return "die";
    }
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
    //Character's death
    public void SetIsDead(bool isDead) {
        _isDead = isDead;
    }
    public void ReturnToLife() {
        if (_isDead) {
            SetIsDead(false);
            SubscribeToSignals();
            _ownParty.ReturnToLife();
        }
    }
    public void Death(string cause = "normal") {
        if (!_isDead) {
            //if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == this.id) {
            //    UIManager.Instance.characterInfoUI.OnClickCloseMenu();
            //}
            SetIsDead(true);
            UnsubscribeSignals();

            CombatManager.Instance.ReturnCharacterColorToPool(_characterColor);

            if (currentParty.specificLocation == null) {
                throw new Exception("Specific location of " + this.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }
            if(stateComponent.currentState != null) {
                stateComponent.currentState.OnExitThisState();
            }

            if (ownParty.specificLocation != null && isHoldingItem) {
                DropAllTokens(ownParty.specificLocation, currentStructure, true);
            }
            Area deathLocation = ownParty.specificLocation;
            LocationStructure deathStructure = currentStructure;
            LocationGridTile deathTile = gridTileLocation;

            if (!IsInOwnParty()) {
                _currentParty.RemoveCharacter(this);
            }
            _ownParty.PartyDeath();

            if (this.race != RACE.SKELETON) {
                deathLocation.AddCorpse(this, deathStructure, deathTile);
            }

            if (this._faction != null) {
                this._faction.RemoveCharacter(this); //remove this character from it's factions list of characters
            }

            if (_role != null) {
                _role.OnDeath(this);
            }
            if (homeArea != null) {
                Area home = homeArea;
                Dwelling homeStructure = this.homeStructure;
                homeArea.RemoveResident(this);
                SetHome(home); //keep this data with character to prevent errors
                SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            }

            List<Character> characterRels = new List<Character>(this.relationships.Keys.ToList());
            for (int i = 0; i < characterRels.Count; i++) {
                RemoveRelationship(characterRels[i]);
            }

            if (_minion != null) {
                PlayerManager.Instance.player.RemoveMinion(_minion);
            }

            //gridTileLocation.SetPrefabHere(null);
            ObjectPoolManager.Instance.DestroyObject(marker.gameObject);

            if (onCharacterDeath != null) {
                onCharacterDeath();
            }
            onCharacterDeath = null;
            Messenger.Broadcast(Signals.CHARACTER_DEATH, this);

            Debug.Log(GameManager.Instance.TodayLogString() + this.name + " died of " + cause);
            Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "death_" + cause);
            //Log log = null;
            //if (isTracked) {
            //    log = new Log(GameManager.Instance.Today(), "Character", "Generic", "death_" + cause);
            //} else {
            //    log = new Log(GameManager.Instance.Today(), "Character", "Generic", "something_happened");
            //}
            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            AddHistory(log);
            specificLocation.AddHistory(log);

            //switch (cause) {
            //    case "exhaustion":
            //    case "starvation":
            //        //deathLocation.areaMap.ShowEventPopupAt(deathTile, log);
            //        break;
            //    default:
            //        break;
            //}

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
        return items;
    }
    #endregion

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
    public void AssignRole(CharacterRole role) {
        bool wasRoleChanged = false;
        if (_role != null) {
            _role.OnChange(this);
//#if !WORLD_CREATION_TOOL
//            Log roleChangeLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "change_role");
//            roleChangeLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
//            AddHistory(roleChangeLog);
//#endif
            wasRoleChanged = true;
        }
        _role = role;
        //switch (role) {
        //    case CHARACTER_ROLE.NOBLE:
        //    _role = new Noble(this);
        //    break;
        //    case CHARACTER_ROLE.ADVENTURER:
        //    _role = new Adventurer(this);
        //    break;
        //    case CHARACTER_ROLE.CIVILIAN:
        //    _role = new Civilian(this);
        //    break;
        //    case CHARACTER_ROLE.MINION:
        //    _role = new MinionRole(this);
        //    break;
        //    case CHARACTER_ROLE.PLAYER:
        //    _role = new PlayerRole(this);
        //    break;
        //    case CHARACTER_ROLE.SOLDIER:
        //    _role = new Soldier(this);
        //    break;
        //    case CHARACTER_ROLE.BEAST:
        //    _role = new Beast(this);
        //    break;
        //    case CHARACTER_ROLE.LEADER:
        //    _role = new Leader(this);
        //    break;
        //    case CHARACTER_ROLE.BANDIT:
        //    _role = new Bandit(this);
        //    break;
        //    SetName(this.characterClass.className);
        //    break;
        //    default:
        //    break;
        //}
        if (_role != null) {
            _role.OnAssign(this);
        }
        if (wasRoleChanged) {
            Messenger.Broadcast(Signals.ROLE_CHANGED, this);
        }
    }
    #endregion

    #region Character Class
    public void AssignClassByRole(CharacterRole role) {
        if(role == CharacterRole.BEAST) {
            AssignClass(Utilities.GetRespectiveBeastClassNameFromByRace(race));
        } else {
            string className = CharacterManager.Instance.GetRandomClassByIdentifier(role.classNameOrIdentifier);
            if (className != string.Empty) {
                AssignClass(className);
            } else {
                AssignClass(role.classNameOrIdentifier);
            }
        }
    }
    public void RemoveClass() {
        if(_characterClass == null) { return; }
        RemoveTraitsFromClass();
        _characterClass = null;
    }
    private void AssignClass(string className) {
        if (CharacterManager.Instance.classesDictionary.ContainsKey(className)) {
            if (_characterClass != null) {
                //This means that the character currently has a class and it will be replaced with a new class
                RemoveTraitsFromClass();
            }
            _characterClass = CharacterManager.Instance.classesDictionary[className].CreateNewCopy();
            //_skills = new List<Skill>();
            //_skills.Add(_characterClass.skill);
            //EquipItemsByClass();
            SetTraitsFromClass();
        } else {
            throw new Exception("There is no class named " + className + " but it is being assigned to " + this.name);
        }
    }
    public void AssignClass(CharacterClass characterClass) {
        if (_characterClass != null) {
            //This means that the character currently has a class and it will be replaced with a new class
            RemoveTraitsFromClass();
        }
        _characterClass = characterClass;
        SetTraitsFromClass();
    }
    #endregion

    #region Jobs
    private void AssignRandomJob() {
        if (CharacterManager.Instance.IsClassADeadlySin(_characterClass.className)) {
            AssignJob(_characterClass.jobType);
        } else {
            JOB[] jobs = new JOB[] { JOB.DIPLOMAT, JOB.DEBILITATOR, JOB.EXPLORER, JOB.INSTIGATOR, JOB.RAIDER, JOB.RECRUITER, JOB.SPY };
            AssignJob(jobs[UnityEngine.Random.Range(0, jobs.Length)]);
            //AssignJob(JOB.RAIDER);
        }
    }
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
            case JOB.DEBILITATOR:
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
    public void SetCurrentJob(JobQueueItem job) {
        currentJob = job;
    }
    public void AddJobTargettingThisCharacter(JobQueueItem job) {
        allJobsTargettingThis.Add(job);
    }
    public bool RemoveJobTargettingThisCharacter(JobQueueItem job) {
        return allJobsTargettingThis.Remove(job);
    }
    public void CancelAllJobsTargettingThisCharacter(string jobName) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.name == jobName) {
                if (job.jobQueueParent.CancelJob(job)) {
                    i--;
                }
            }
        }
    }
    public bool HasJobTargettingThisCharacter(string jobName) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.name == jobName) {
                return true;
            }
        }
        return false;
    }
    public bool HasJobTargettingThisCharacter(string jobName, object conditionKey) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if(allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.name == jobName && job.targetEffect.conditionKey == conditionKey) {
                    return true;
                }
            }
        }
        return false;
    }
    private void CheckApprehendRelatedJobsOnLeaveLocation() {
        CancelAllJobsTargettingThisCharacter("Apprehend");

        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan plan = allGoapPlans[i];
            if(plan.job != null && plan.job.name == "Apprehend") {
                plan.job.UnassignJob();
                i--;
            }
        }
    }
    public void CreateApprehendJob() {
        if (homeArea.id == specificLocation.id) {
            if (!HasJobTargettingThisCharacter("Apprehend")) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = this };
                GoapPlanJob job = new GoapPlanJob("Apprehend", goapEffect);
                job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
                homeArea.jobQueue.AddJobInQueue(job);
            }
        }
    }
    private bool CanCharacterTakeApprehendJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    #endregion

    #region Faction
    public void SetFaction(Faction newFaction) {
        if (_faction != null 
            && newFaction != null
            && _faction.id == newFaction.id) {
            //ignore change, because character is already part of that faction
            return;
        }
        _faction = newFaction;
        OnChangeFaction();
        UpdateTokenOwner();
        if (_faction != null) {
            Messenger.Broadcast<Character>(Signals.FACTION_SET, this);
        }
    }
    public void ChangeFactionTo(Faction newFaction) {
        if (this.faction.id == newFaction.id) {
            return; //if the new faction is the same, ignore change
        }
        faction.RemoveCharacter(this);
        newFaction.AddNewCharacter(this);
    }
    private void OnChangeFaction() {
        //check if this character has a Criminal Trait, if so, remove it
        Trait criminal = GetTrait("Criminal");
        if (criminal != null) {
            RemoveTrait(criminal, false);
        }
    }
    public void FoundFaction(string factionName, Area location) {
        SetForcedInteraction(null);
        MigrateHomeTo(location);
        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName(factionName);
        newFaction.SetLeader(this);
        ChangeFactionTo(newFaction);
        FactionManager.Instance.neutralFaction.RemoveFromOwnedAreas(location);
        LandmarkManager.Instance.OwnArea(newFaction, race, location);
        newFaction.SetFactionActiveState(true);
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
        newParty.AddCharacter(this, true);
        //newParty.CreateCharacterObject();
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
        //if (ownParty is CharacterParty) {
        //    if ((ownParty as CharacterParty).actionData.currentAction != null) {
        //        (ownParty as CharacterParty).actionData.currentAction.EndAction(ownParty, (ownParty as CharacterParty).actionData.currentTargetObject);
        //    }
        //}
        if (this.minion != null) {
            this.minion.SetEnabledState(true); //reenable this minion, since it could've been disabled because it was part of another party
        }
    }
    public void OnAddedToParty() {
        if (currentParty.id != ownParty.id) {
            ownParty.specificLocation.RemoveCharacterFromLocation(this);
            //ownParty.icon.SetVisualState(false);
        }
    }
    public void OnAddedToPlayer() {
        //if (ownParty.specificLocation is BaseLandmark) {
        //    ownParty.specificLocation.RemoveCharacterFromLocation(ownParty);
        //}
        PlayerManager.Instance.player.playerArea.AddCharacterToLocation(ownParty, null, true);
        //if (this.homeArea != null) {
        //    this.homeArea.RemoveResident(this);
        //}
        //PlayerManager.Instance.player.playerArea.AddResident(this);
        MigrateHomeTo(PlayerManager.Instance.player.playerArea);
    }
    public bool IsInParty() {
        if (currentParty.characters.Count > 1) {
            return true; //if the character is in a party that has more than 1 characters
        }
        return false;
    }
    public bool IsInPartyOf(Character character) {
        return currentParty == character.ownParty;
    }
    public bool IsInOwnParty() {
        if (currentParty.id == ownParty.id) {
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
    public void SetCurrentStructureLocation(LocationStructure currentStructure, bool broadcast = true) {
        if (currentStructure == this.currentStructure) {
            return; //ignore change;
        }
        LocationStructure previousStructure = this.currentStructure;
        this.currentStructure = currentStructure;
        if (marker != null && currentStructure != null) {
            marker.RevalidatePOIsInVisionRange(); //when the character changes structures, revalidate pois in range
        }
        string summary = string.Empty;
        if (currentStructure != null) {
            summary = GameManager.Instance.TodayLogString() + "Arrived at <color=\"green\">" + currentStructure.ToString() + "</color>";
            if (forcedInteraction != null) {
                summary += " to perform <b>" + forcedInteraction.name + "</b> scheduled at " + _currentInteractionTick.ToString();
            }
        } else {
            summary = GameManager.Instance.TodayLogString() + "Left <color=\"red\">" + previousStructure.ToString() + "</color>";
            if (forcedInteraction != null) {
                summary += " to perform <b>" + forcedInteraction.name + "</b> at " + forcedInteraction.interactable.name;
            }
        }
        locationHistory.Add(summary + "\n" + StackTraceUtility.ExtractStackTrace());
        if (locationHistory.Count > 80) {
            locationHistory.RemoveAt(0);
        }

        if (currentStructure != null && broadcast) {
            Messenger.Broadcast(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, this, currentStructure);
        }
    }
    public void MoveToRandomStructureInArea() {
        LocationStructure locationStructure = specificLocation.GetRandomStructure();
        MoveToAnotherStructure(locationStructure);
    }
    public void MoveToRandomStructureInArea(STRUCTURE_TYPE structureType) {
        if (specificLocation.HasStructure(structureType)) {
            LocationStructure newStructure = specificLocation.GetRandomStructureOfType(structureType);
            MoveToAnotherStructure(newStructure);
        } else {
            throw new Exception("Can't move " + name + " to a " + structureType.ToString() + " because " + specificLocation.name + " does not have that structure!");
        }
    }
    /// <summary>
    /// Move this character to another structure in the same area.
    /// </summary>
    /// <param name="newStructure">New structure the character is going to.</param>
    /// <param name="destinationTile">LocationGridTile where the character will go to (Must be inside the new structure).</param>
    /// <param name="targetPOI">The Point of Interest this character will interact with</param>
    /// <param name="arrivalAction">What should this character do when it reaches its target tile?</param>
    public void MoveToAnotherStructure(LocationStructure newStructure, LocationGridTile destinationTile = null, IPointOfInterest targetPOI = null, Action arrivalAction = null) {
        //if the provided destination tile is null (Default)
        if (destinationTile == null) {
            //and the target Point of Interest is not null
            if (targetPOI != null) {
                //check if this character's current location is already the nearest tile from it's target Point of Interest
                float ogDistance = targetPOI.gridTileLocation.GetDistanceTo(gridTileLocation);
                if(ogDistance <= 1f) {
                    destinationTile = gridTileLocation;
                } else {
                    //get nearest tile from poi
                    LocationGridTile newNearestTile = targetPOI.GetNearestUnoccupiedTileFromThis();
                    if (newNearestTile != null) {
                        destinationTile = newNearestTile;
                    }
                }
            }
        }

        //if the destination tile is still null
        if (destinationTile == null) {
            if (currentStructure != null) {
                List<LocationGridTile> tilesToUse;
                if (newStructure.location.areaType == AREA_TYPE.DEMONIC_INTRUSION) { //player area
                    tilesToUse = newStructure.tiles;
                } else {
                    tilesToUse = newStructure.unoccupiedTiles;
                }
                if (tilesToUse.Count > 0) {
                    destinationTile = tilesToUse[UnityEngine.Random.Range(0, tilesToUse.Count)];
                } else {
                    throw new Exception ("There are no tiles at " + newStructure.structureType.ToString() + " at " + newStructure.location.name + " for " + name);
                }
            } else {
                newStructure.AddCharacterAtLocation(this, destinationTile);
                if (arrivalAction != null) {
                    arrivalAction();
                }
                return;
            }
        }

        //if the character is already at the destination tile, just do the specified arrival action, if any.
        if (gridTileLocation == destinationTile) {
            if (arrivalAction != null) {
                arrivalAction();
            }
        } else {
            //trigger the characters marker to go to the target tile.
            marker.GoToTile(destinationTile, targetPOI, arrivalAction);
        }
    }
    public void SetGridTileLocation(LocationGridTile tile) {
        this.tile = tile;
        string summary = string.Empty;
        if (tile == null) {
            summary = GameManager.Instance.TodayLogString() + "Set tile location to null";
        } else {
            summary = GameManager.Instance.TodayLogString() + "Set tile location to " + tile.localPlace.ToString();
        }
        locationHistory.Add(summary + "\n" + StackTraceUtility.ExtractStackTrace());
        if (locationHistory.Count > 80) {
            locationHistory.RemoveAt(0);
        }
    }
    public LocationGridTile GetNearestUnoccupiedTileFromThis() {
        if (!isDead  && gridTileLocation != null) {
            List<LocationGridTile> unoccupiedNeighbours = gridTileLocation.UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count == 0) {
                return null;
            } else {
                return unoccupiedNeighbours[UnityEngine.Random.Range(0, unoccupiedNeighbours.Count)];
            }
        }
        return null;
    }
    private void OnLeaveArea(Party party) {
        if(currentParty == party) {
            CheckApprehendRelatedJobsOnLeaveLocation();
        }
    }
    private void OnArrivedAtArea(Party party) {
        if (currentParty == party) {
            if (HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                CreateApprehendJob();
            }
        }
    }
    #endregion

    #region Utilities
    public void ChangeGender(GENDER gender) {
        _gender = gender;
        Messenger.Broadcast(Signals.GENDER_CHANGED, this, gender);
    }
    public void ChangeRace(RACE race) {
        if(_raceSetting != null) {
            if (_raceSetting.race == race) {
                return; //current race is already the new race, no change
            }
            RemoveTraitsFromRace();
        }
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        SetTraitsFromRace();
        //Update Portrait to use new race
        _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
        if(marker != null) {
            marker.UpdateMarkerVisuals();
        }
        Messenger.Broadcast(Signals.CHARACTER_CHANGED_RACE, this);
    }
    public void RemoveRace() {
        if(_raceSetting == null) {
            return;
        }
        RemoveTraitsFromRace();
        _raceSetting = null;
    }
    public void SetRace(RACE race) {
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        SetTraitsFromRace();
        //Update Portrait to use new race
        _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
        if(marker != null) {
            marker.UpdateMarkerVisuals();
        }
    }
    public void ChangeClass(string className) {
        string previousClassName = _characterClass.className;
        AssignClass(className);
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
        _firstName = _name.Split(' ')[0];
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
            if (currentParty.icon.isTravelling) {
                if(currentParty.icon.travelLine != null) {
                    if (specificLocation.areaMap.isShowing) {
                        InteriorMapManager.Instance.HideAreaMap();
                        currentParty.icon.travelLine.OnClickTravelLine();
                    }
                    CameraMove.Instance.CenterCameraOn(currentParty.icon.travelLine.iconImg.gameObject);
                } else {
                    if (!specificLocation.areaMap.isShowing) {
                        InteriorMapManager.Instance.ShowAreaMap(specificLocation);
                    }
                    AreaMapCameraMove.Instance.CenterCameraOn(marker.gameObject);
                }
            } else {
                if (!specificLocation.areaMap.isShowing) {
                    InteriorMapManager.Instance.ShowAreaMap(specificLocation);
                }
                AreaMapCameraMove.Instance.CenterCameraOn(marker.gameObject);
            }
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
    public bool IsSpecialCivilian() {
        if (this.characterClass != null) {
            if (this.characterClass.className.Equals("Farmer") || this.characterClass.className.Equals("Miner") || this.characterClass.className.Equals("Retired Hero") ||
                this.characterClass.className.Equals("Shopkeeper") || this.characterClass.className.Equals("Woodcutter")) {
                return true;
            }
        }
        return false;
    }
    private void OnOtherCharacterDied(Character characterThatDied) {
        if (characterThatDied.id != this.id) {
            RemoveRelationship(characterThatDied);
            marker.OnOtherCharacterDied(characterThatDied);
        }
    }
    public void SetMode(MODE mode) {
        _currentMode = mode;
    }
    public void AdjustDoNotDisturb(int amount) {
        _doNotDisturb += amount;
        _doNotDisturb = Math.Max(_doNotDisturb, 0);
    }
    public void AdjustDoNotGetHungry(int amount) {
        _doNotGetHungry += amount;
        _doNotGetHungry = Math.Max(_doNotGetHungry, 0);
    }
    public void AdjustDoNotGetLonely(int amount) {
        _doNotGetLonely += amount;
        _doNotGetLonely = Math.Max(_doNotGetLonely, 0);
    }
    public void AdjustDoNotGetTired(int amount) {
        _doNotGetTired += amount;
        _doNotGetTired = Math.Max(_doNotGetTired, 0);
    }
    public void SetAlreadyTargetedByGrudge(bool state) {
        _alreadyTargetedByGrudge = state;
    }
    public void AttackAnArea(Area target) {
        Interaction attackInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.ATTACK, target);
        attackInteraction.AddEndInteractionAction(() => _ownParty.GoHomeAndDisband());
        attackInteraction.SetCanInteractionBeDoneAction(() => IsTargetStillViable(target));
        _ownParty.GoToLocation(target, PATHFINDING_MODE.NORMAL, null, () => SetForcedInteraction(attackInteraction));
    }
    private bool IsTargetStillViable(Area target) {
        return target.owner != null;
    }
    public void ReturnToOriginalHomeAndFaction(Area ogHome, Faction ogFaction) { 
        //first, check if the character's original faction is still alive
        if (!ogFaction.isDestroyed) { //if it is, 
            this.ChangeFactionTo(ogFaction);  //transfer the character to his original faction
            if (ogFaction.id == FactionManager.Instance.neutralFaction.id) { //if the character's original faction is the neutral faction
                if (ogHome.owner == null && !ogHome.IsResidentsFull()) { //check if his original home is still unowned
                    //if it is and it has not reached it's resident capacity, return him to his original home
                    MigrateHomeTo(ogHome);
                } else { //if it does not meet those requirements
                    //check if the neutral faction still has any available areas that have not reached capacity yet
                    List<Area> validNeutralAreas = FactionManager.Instance.neutralFaction.ownedAreas.Where(x => !x.IsResidentsFull()).ToList();
                    if (validNeutralAreas.Count > 0) {
                        //if it does, pick randomly from those
                        Area chosenArea = validNeutralAreas[UnityEngine.Random.Range(0, validNeutralAreas.Count)];
                        MigrateHomeTo(chosenArea);
                    }
                    //if not, keep the characters current home
                }
            } else { //if it is not, check if his original home is still owned by that faction and it has not yet reached it's resident capacity
                if (ogHome.owner == ogFaction && !ogHome.IsResidentsFull()) {
                    //if it meets those requirements, return the character's home to that location
                    MigrateHomeTo(ogHome);
                } else { //if not, get another area owned by his faction that has not yet reached capacity
                    List<Area> validAreas = ogFaction.ownedAreas.Where(x => !x.IsResidentsFull()).ToList();
                    if (validAreas.Count > 0) {
                        Area chosenArea = validAreas[UnityEngine.Random.Range(0, validAreas.Count)];
                        MigrateHomeTo(chosenArea);
                    }
                    //if there are still no areas that can be his home, keep his current one.
                }
            }
        } else { //if not
            //transfer the character to the neutral faction instead
            this.ChangeFactionTo(FactionManager.Instance.neutralFaction);
            List<Area> validNeutralAreas = FactionManager.Instance.neutralFaction.ownedAreas.Where(x => !x.IsResidentsFull()).ToList();
            if (validNeutralAreas.Count > 0) {  //then check if the neutral faction has any owned areas that have not yet reached area capacity
                //if it does, pick from any of those, then set it as the characters new home
                Area chosenArea = validNeutralAreas[UnityEngine.Random.Range(0, validNeutralAreas.Count)];
                MigrateHomeTo(chosenArea);
            }
            //if it does not, keep the characters current home
        }
    }
    public override string ToString() {
        return name;
    }
    public void SetTracked(bool state) {
        _isTracked = state;
        if (_isTracked) {
            Messenger.Broadcast(Signals.CHARACTER_TRACKED, this);
        }
    }
    public void AdjustIsWaitingForInteraction(int amount) {
        isWaitingForInteraction += amount;
        if(isWaitingForInteraction < 0) {
            isWaitingForInteraction = 0;
        }
    }
    public LocationGridTile GetLocationGridTileByXY(int x, int y) {
        return specificLocation.areaMap.map[x, y];
    }
    public bool IsDoingCombatAction() {
        if (marker.currentlyEngaging != null) {
            return true;
        }
        //if (currentAction != null) {
        //    return currentAction.goapType == INTERACTION_TYPE.ASSAULT_ACTION_NPC;
        //}
        return false;
    }
    public bool IsDoingCombatActionTowards(Character otherCharacter) {
        //if (marker.currentlyEngaging == otherCharacter) {
        //    return true;
        //}
        if (currentAction != null) {
            return currentAction.goapType == INTERACTION_TYPE.ASSAULT_ACTION_NPC && currentAction.poiTarget == otherCharacter; 
        }
        return false;
    }
    #endregion

    #region Relationships
    private void AddRelationship(Character character, RelationshipTrait newRel) {
        if (!relationships.ContainsKey(character)) {
            relationships.Add(character, new CharacterRelationshipData(this, character));
        }
        relationships[character].AddRelationship(newRel);
        OnRelationshipWithCharacterAdded(character, newRel);
        Messenger.Broadcast(Signals.RELATIONSHIP_ADDED, this, newRel);
    }
    public void RemoveRelationship(Character character) {
        if (relationships.ContainsKey(character)) {
            List<Trait> traits = relationships[character].rels.Select(x => x as Trait).ToList();
            relationships[character].RemoveListeners();
            relationships.Remove(character);
            RemoveTrait(traits);
        }
    }
    public void RemoveRelationship(Character character, RelationshipTrait rel) {
        if (relationships.ContainsKey(character)) {
            relationships[character].RemoveRelationship(rel);
            if (relationships[character].rels.Count == 0) {
                RemoveRelationship(character);
            }
        }
    }
    public void RemoveAllRelationships() {
        List<Character> targetCharacters = relationships.Keys.ToList();
        while(targetCharacters.Count > 0) {
            CharacterManager.Instance.RemoveRelationshipBetween(this, targetCharacters[0]);
            targetCharacters.RemoveAt(0);
        }
    }
    public void ReEstablishRelationships(List<RelationshipLycanthropyData> prevRelationships) {
        for (int i = 0; i < prevRelationships.Count; i++) {
            RelationshipLycanthropyData relLycanData = prevRelationships[i];
            if (!relationships.ContainsKey(relLycanData.target)) {
                relationships.Add(relLycanData.target, relLycanData.characterToTargetRelData);
            }
            if (!relLycanData.target.relationships.ContainsKey(this)) {
                relLycanData.target.relationships.Add(this, relLycanData.targetToCharacterRelData);
            }

            List<RelationshipTrait> rels = new List<RelationshipTrait>(relLycanData.characterToTargetRelData.rels);
            relLycanData.characterToTargetRelData.rels.Clear();
            relLycanData.targetToCharacterRelData.rels.Clear();

            for (int j = 0; j < rels.Count; j++) {
                CharacterManager.Instance.CreateNewRelationshipBetween(this, relLycanData.target, rels[i].relType);
            }
        }
    }
    public RelationshipTrait GetRelationshipTraitWith(Character character, RELATIONSHIP_TRAIT type) {
        if (relationships.ContainsKey(character)) {
            return relationships[character].GetRelationshipTrait(type);
        }
        return null;
    }
    public List<RelationshipTrait> GetAllRelationshipTraitWith(Character character) {
        if (relationships.ContainsKey(character)) {
            return new List<RelationshipTrait>(relationships[character].rels);
        }
        return null;
    }
    public List<RelationshipTrait> GetAllRelationshipOfEffectWith(Character character, TRAIT_EFFECT effect, RELATIONSHIP_TRAIT include = RELATIONSHIP_TRAIT.NONE) {
        List<RelationshipTrait> rels = new List<RelationshipTrait>();
        if (relationships.ContainsKey(character)) {
            for (int i = 0; i < relationships[character].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[character].rels[i];
                if (currTrait.effect == effect || currTrait.relType == include) {
                    rels.Add(currTrait);
                }
            }
        }
        return rels;
    }
    public List<RELATIONSHIP_TRAIT> GetAllRelationshipTraitTypesWith(Character character) {
        if (relationships.ContainsKey(character)) {
            return new List<RELATIONSHIP_TRAIT>(relationships[character].rels.Select(x => x.relType));
        }
        return null;
    }
    public List<Character> GetCharactersWithRelationship(RELATIONSHIP_TRAIT type) {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationshipTrait(type)) {
                characters.Add(kvp.Key);
            }
        }
        return characters;
    }
    public Character GetCharacterWithRelationship(RELATIONSHIP_TRAIT type) {
        foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationshipTrait(type)) {
                return kvp.Key;
            }
        }
        return null;
    }
    public bool CanHaveRelationshipWith(RELATIONSHIP_TRAIT type, Character target) {
        //NOTE: This is only one way checking. This character will only check itself, if he/she meets the requirements of a given relationship
        List<RELATIONSHIP_TRAIT> relationshipsWithTarget = GetAllRelationshipTraitTypesWith(target);
        //if(relationshipsWithTarget == null) { return true; }
        switch (type) {
            case RELATIONSHIP_TRAIT.ENEMY:
                return relationshipsWithTarget == null || (relationshipsWithTarget!= null && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.ENEMY) && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.FRIEND)); //check that the target character is not already this characters enemy and that this character is also not his friend
            case RELATIONSHIP_TRAIT.FRIEND:
                return relationshipsWithTarget == null || (relationshipsWithTarget!= null && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.FRIEND) && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.ENEMY)); //check that the target character is not already this characters friend and that this character is also not his enemy
            case RELATIONSHIP_TRAIT.LOVER:
                //- **Lover:** Positive, Permanent (Can only have 1)
                //check if this character already has a lover and that the target character is not his/her paramour
                if (GetCharacterWithRelationship(type) != null) {
                    return false;
                }
                if (relationshipsWithTarget!= null && relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.PARAMOUR)) {
                    return false;
                }
                return true;

                //if (GetCharacterWithRelationship(type) == null
                //    && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.PARAMOUR)) {
                //    return true;
                //}
                //return false;
            case RELATIONSHIP_TRAIT.PARAMOUR:
                //- **Paramour:** Positive, Transient (Can only have 1)
                //check if this character already has a paramour and that the target character is not his/her lover
                if (GetCharacterWithRelationship(type) != null) {
                    return false;
                }
                if (relationshipsWithTarget!= null && relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.LOVER)) {
                    return false;
                }
                return true;
                //if (GetCharacterWithRelationship(type) == null 
                //    && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.LOVER)) { 
                //    return true;
                //}
                //return false;
            case RELATIONSHIP_TRAIT.MASTER: 
                //this means that the target character will be this characters master, therefore making this character his/her servant
                //so check if this character isn't already serving a master, or that this character is not a master himself
                if (GetCharacterWithRelationship(RELATIONSHIP_TRAIT.MASTER) == null 
                    && GetCharacterWithRelationship(RELATIONSHIP_TRAIT.SERVANT) == null) {
                    return true;
                }
                return false;
            case RELATIONSHIP_TRAIT.SERVANT:
                //this means that the target character will be this characters servant, therefore making this character his/her master
                //so check that this character is not a servant
                if (GetCharacterWithRelationship(RELATIONSHIP_TRAIT.MASTER) == null) {
                    return true;
                }
                return false;
        }
        return true;
    }
    private void OnRelationshipWithCharacterAdded(Character targetCharacter, RelationshipTrait newRel) {
        //check if they share the same home, then migrate them accordingly
        if (newRel.relType == RELATIONSHIP_TRAIT.LOVER 
            && this.homeArea.id == targetCharacter.homeArea.id
            && this.homeStructure != targetCharacter.homeStructure) {
            //Lover conquers all, even if one character is factionless they will be together, meaning the factionless character will still have home structure
            homeArea.AssignCharacterToDwellingInArea(this);
            //homeArea.AssignCharacterToDwellingInArea(targetCharacter);
        }
    }
    /// <summary>
    /// Does this character have a relationship of effect with the provided character?
    /// </summary>
    /// <param name="character">Other character.</param>
    /// <param name="effect">Relationship effect (Positive, Negative, Neutral)</param>
    /// <param name="include">Relationship type to exclude from checking</param>
    /// <returns></returns>
    public bool HasRelationshipOfEffectWith(Character character, TRAIT_EFFECT effect, RELATIONSHIP_TRAIT include = RELATIONSHIP_TRAIT.NONE) {
        if (relationships.ContainsKey(character)) {
            for (int i = 0; i < relationships[character].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[character].rels[i];
                if (currTrait.effect == effect || currTrait.relType == include) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfEffectWith(Character character, List<TRAIT_EFFECT> effect) {
        if (relationships.ContainsKey(character)) {
            for (int i = 0; i < relationships[character].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[character].rels[i];
                if (effect.Contains(currTrait.effect)) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfEffect(TRAIT_EFFECT effect) {
        foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in relationships) {
            for (int i = 0; i < kvp.Value.rels.Count; i++) {
                if (effect == kvp.Value.rels[i].effect) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfEffect(List<TRAIT_EFFECT> effect) {
        foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in relationships) {
            for (int i = 0; i < kvp.Value.rels.Count; i++) {
                if (effect.Contains(kvp.Value.rels[i].effect)) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfTypeWith(Character character, RELATIONSHIP_TRAIT relType) {
        if (relationships.ContainsKey(character)) {
            for (int i = 0; i < relationships[character].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[character].rels[i];
                if (currTrait.relType == relType) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfTypeWith(Character character, RELATIONSHIP_TRAIT relType1, RELATIONSHIP_TRAIT relType2) {
        if (relationships.ContainsKey(character)) {
            for (int i = 0; i < relationships[character].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[character].rels[i];
                if (currTrait.relType == relType1 || currTrait.relType == relType2) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfTypeWith(Character character, RELATIONSHIP_TRAIT relType1, RELATIONSHIP_TRAIT relType2, RELATIONSHIP_TRAIT relType3) {
        if (relationships.ContainsKey(character)) {
            for (int i = 0; i < relationships[character].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[character].rels[i];
                if (currTrait.relType == relType1 || currTrait.relType == relType2 || currTrait.relType == relType3) {
                    return true;
                }
            }
        }
        return false;
    }
    public CharacterRelationshipData GetCharacterRelationshipData(Character character) {
        if (relationships.ContainsKey(character)) {
            return relationships[character];
        }
        return null;
    }
    public bool HasRelationshipWith(Character character) {
        return relationships.ContainsKey(character);
    }
    public int GetAllRelationshipCount(List<RELATIONSHIP_TRAIT> except = null) {
        int count = 0;
        for (int i = 0; i < traits.Count; i++) {
            if (traits[i] is RelationshipTrait) {
                RelationshipTrait relTrait = traits[i] as RelationshipTrait;
                if (except != null) {
                    if (except.Contains(relTrait.relType)) {
                        continue; //skip
                    }
                }
                count++;
            }
        }
        return count;
    }
    #endregion

    #region History
    public void AddHistory(Log log) {
        if (!_history.Contains(log)) {
            _history.Add(log);
            //if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && this.id == UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id) {
            //    Debug.Log("Added log to history of " + this.name + ". " + log.isInspected);
            //}
            if (this._history.Count > 300) {
                this._history.RemoveAt(0);
            }
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
        }

    }
    #endregion

    #region Character
    public STANCE GetCurrentStance() {
        return STANCE.NEUTRAL;
    }
    #endregion

    #region Combat Handlers
    public void SetIsInCombat(bool state) {
        _isInCombat = state;
    }
    public void SetCombatCharacter(CombatCharacter combatCharacter) {
        _currentCombatCharacter = combatCharacter;
    }
    #endregion

    #region Portrait Settings
    public void SetPortraitSettings(PortraitSettings settings) {
        _portraitSettings = settings;
    }
    #endregion

    #region RPG
    private bool hpMagicRangedStatMod;
    public void LevelUp() {
        //Only level up once per day
        //if (_lastLevelUpDay == GameManager.Instance.continuousDays) {
        //    return;
        //}
        //_lastLevelUpDay = GameManager.Instance.continuousDays;
        if (_level < CharacterManager.Instance.maxLevel) {
            _level += 1;
            //Add stats per level from class
            if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.MELEE) {
                AdjustAttackMod(_characterClass.attackPowerPerLevel);
                AdjustSpeedMod(_characterClass.speedPerLevel);
                AdjustMaxHPMod(_characterClass.hpPerLevel);
            } else if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
                if (_level % 2 == 0) {
                    //even
                    AdjustMaxHPMod(_characterClass.hpPerLevel);
                } else {
                    //odd
                    AdjustAttackMod(_characterClass.attackPowerPerLevel);
                }
                AdjustSpeedMod(_characterClass.speedPerLevel);
            } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
                if (!hpMagicRangedStatMod) {
                    AdjustAttackMod(_characterClass.attackPowerPerLevel);
                } else {
                    AdjustMaxHPMod(_characterClass.hpPerLevel);
                }
                if ((_level - 1) % 2 == 0) {
                    hpMagicRangedStatMod = !hpMagicRangedStatMod;
                }
                AdjustSpeedMod(_characterClass.speedPerLevel);
            }

            //Reset to full health and sp
            ResetToFullHP();

            if(_playerCharacterItem != null) {
                _playerCharacterItem.UpdateMinionItem();
                Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
            }
        }
    }
    public void LevelUp(int amount) {
        //Only level up once per day
        //if (_lastLevelUpDay == GameManager.Instance.continuousDays) {
        //    return;
        //}
        //_lastLevelUpDay = GameManager.Instance.continuousDays;
        int supposedLevel = _level + amount;
        if (supposedLevel > CharacterManager.Instance.maxLevel) {
            amount = CharacterManager.Instance.maxLevel - level;
        } else if (supposedLevel < 0) {
            amount -= (supposedLevel - 1);
        }
        //Add stats per level from class
        if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.MELEE) {
            AdjustAttackMod(_characterClass.attackPowerPerLevel * amount);
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
            AdjustMaxHPMod(_characterClass.hpPerLevel * amount);
        } else if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (amount < 0 ? -1 : 1);
            int range = amount * multiplier;
            for (int i = 0; i < range; i++) {
                if (i % 2 == 0) {
                    //even
                    AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                } else {
                    //odd
                    AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
        } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (amount < 0 ? -1 : 1);
            int range = amount * multiplier;
            for (int i = _level; i <= _level + range; i++) {
                if (!hpMagicRangedStatMod) {
                    AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                } else {
                    AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                }
                if (i != 1 && (i - 1) % 2 == 0) {
                    hpMagicRangedStatMod = !hpMagicRangedStatMod;
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
        }
        _level += amount;

        //Reset to full health and sp
        ResetToFullHP();
        //ResetToFullSP();
        Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
        if (_playerCharacterItem != null) {
            _playerCharacterItem.UpdateMinionItem();
        }
    }
    public void SetLevel(int amount) {
        int previousLevel = _level;
        _level = amount;
        if (_level < 1) {
            _level = 1;
        }else if (_level > CharacterManager.Instance.maxLevel) {
            _level = CharacterManager.Instance.maxLevel;
        }

        //Add stats per level from class
        int difference = _level - previousLevel;
        if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.MELEE) {
            AdjustAttackMod(_characterClass.attackPowerPerLevel * difference);
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
            AdjustMaxHPMod(_characterClass.hpPerLevel * difference);
        } else if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (difference < 0 ? -1 : 1);
            int range = difference * multiplier;
            for (int i = 0; i < range; i++) {
                if (i % 2 == 0) {
                    //even
                    AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                } else {
                    //odd
                    AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
        } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (difference < 0 ? -1 : 1);
            int range = difference * multiplier;
            for (int i = _level; i <= _level + range; i++) {
                if (!hpMagicRangedStatMod) {
                    AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                } else {
                    AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                }
                if (i != 1 && (i - 1) % 2 == 0) {
                    hpMagicRangedStatMod = !hpMagicRangedStatMod;
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
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
        if (_currentHP > _maxHPMod) {
            _currentHP = _maxHPMod;
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
    public void SetHP(int amount) {
        this._currentHP = amount;
    }
    public void SetMaxHPMod(int amount) {
        int previousMaxHP = maxHP;
        _maxHPMod = amount;
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustAttackMod(int amount) {
        _attackPowerMod += amount;
    }
    public void AdjustAttackPercentMod(int amount) {
        _attackPowerPercentMod += amount;
    }
    public void AdjustMaxHPMod(int amount) {
        int previousMaxHP = maxHP;
        _maxHPMod += amount;
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustMaxHPPercentMod(int amount) {
        int previousMaxHP = maxHP;
        _maxHPPercentMod += amount;
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustSpeedMod(int amount) {
        _speedMod += amount;
    }
    public void AdjustSpeedPercentMod(int amount) {
        _speedPercentMod += amount;
    }
    public bool IsHealthFull() {
        return _currentHP >= maxHP;
    }
    #endregion

    #region Home
    public void SetHome(Area newHome) {
        this.homeArea = newHome;
    }
    public void SetHomeStructure(Dwelling homeStructure) {
        this.homeStructure = homeStructure;
    }
    public bool IsLivingWith(RELATIONSHIP_TRAIT type) {
        if (homeStructure != null && homeStructure.residents.Count > 1) {
            Character relTarget = GetCharacterWithRelationship(type);
            if (homeStructure.residents.Contains(relTarget)) {
                return true;
            }
        }
        return false;
    }
    public void MigrateHomeTo(Area newHomeArea, bool broadcast = true) {
        Area previousHome = null;
        if (homeArea != null) {
            previousHome = homeArea;
            homeArea.RemoveResident(this);
        }
        newHomeArea.AddResident(this);
        if (broadcast) {
            Messenger.Broadcast(Signals.CHARACTER_MIGRATED_HOME, this, previousHome, newHomeArea);
        }
    }
    public void MigrateHomeStructureTo(Dwelling dwelling) {
        if (this.homeStructure != null) {
            if (this.homeStructure == dwelling) {
                return; //ignore change
            }
            //remove character from his/her old home
            this.homeStructure.RemoveResident(this);
        }
        dwelling.AddResident(this);
    }
    private void OnCharacterMigratedHome(Character character, Area previousHome, Area homeArea) {
        if (character.id != this.id && this.homeArea.id == homeArea.id) {
            if (GetAllRelationshipTraitWith(character) != null) {
                this.homeArea.AssignCharacterToDwellingInArea(this); //redetermine home, in case new character with relationship has moved area to same area as this character
            }
        }
    }
    #endregion

    #region IInteractable
    public void SetIsBeingInspected(bool state) {
        _isBeingInspected = state;
        if (_currentParty.icon != null) {
            _currentParty.icon.UpdateVisualState();
        }
        //if (_currentParty.specificLocation != null && _currentParty.specificLocation.coreTile.landmarkOnTile != null) {
        //    _currentParty.specificLocation.coreTile.landmarkOnTile.landmarkVisual.ToggleCharactersVisibility();
        //}
    }
    public void SetHasBeenInspected(bool state) {
        _hasBeenInspected = state;
    }
    public void EndedInspection() {
        
    }
    public void AddInteraction(Interaction interaction) {
        //_currentInteractions.Add(interaction);
        if (interaction == null) {
            throw new Exception("Something is trying to add null interaction");
        }
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
        //this.ownParty.specificLocation.RemoveCharacterFromLocation(this.ownParty, false);
        //ownParty.SetSpecificLocation(defending.coreTile.landmarkOnTile);
    }
    public void OnRemoveAsDefender() {
        //defendingArea.coreTile.landmarkOnTile.AddCharacterToLocation(this.ownParty);
        defendingArea = null;
    }
    public bool IsDefending(BaseLandmark landmark) {
        if (defendingArea != null && defendingArea.id == landmark.id) {
            return true;
        }
        return false;
    }
    #endregion

    #region Traits
    public void CreateInitialTraitsByClass() {
        //Attack Type
        if (characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
            AddTrait("Physical Attacker");
        } else if (characterClass.attackType == ATTACK_TYPE.MAGICAL) {
            AddTrait("Magic User");
        }

        //Range Type
        if (characterClass.rangeType == RANGE_TYPE.MELEE) {
            AddTrait("Melee Attack");
        } else if (characterClass.rangeType == RANGE_TYPE.RANGED) {
            AddTrait("Ranged Attack");
        }

        //Combat Position
        if (characterClass.combatPosition == COMBAT_POSITION.FRONTLINE) {
            AddTrait("Frontline Combatant");
        } else if (characterClass.combatPosition == COMBAT_POSITION.BACKLINE) {
            AddTrait("Backline Combatant");
        }

        ////Class Name
        //if (characterClass.className == "Knight" || characterClass.className == "Marauder" || characterClass.className == "Barbarian") {
        //    AddTrait(AttributeManager.Instance.allTraits["Melee Trait"]);
        //} else if (characterClass.className == "Stalker" || characterClass.className == "Archer" || characterClass.className == "Hunter") {
        //    AddTrait(AttributeManager.Instance.allTraits["Ranged Trait"]);
        //} else if (characterClass.className == "Druid" || characterClass.className == "Mage" || characterClass.className == "Shaman") {
        //    AddTrait(AttributeManager.Instance.allTraits["Magic Trait"]);
        //} else if (characterClass.className == "Spinner" || characterClass.className == "Abomination") {
        //    AddTrait(AttributeManager.Instance.allTraits["Melee Vulnerable"]);
        //} else if (characterClass.className == "Ravager") {
        //    AddTrait(AttributeManager.Instance.allTraits["Ranged Vulnerable"]);
        //} else if (characterClass.className == "Dragon") {
        //    AddTrait(AttributeManager.Instance.allTraits["Dragon Trait"]);
        //} else if (characterClass.className == "Greed") {
        //    AddTrait(AttributeManager.Instance.allTraits["Greed Trait"]);
        //} else if (characterClass.className == "Lust") {
        //    AddTrait(AttributeManager.Instance.allTraits["Lust Trait"]);
        //} else if (characterClass.className == "Envy") {
        //    AddTrait(AttributeManager.Instance.allTraits["Envy Trait"]);
        //}

        //Random Traits
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < 10) {
            AddTrait(new Craftsman());
        }
    }
    public void CreateInitialTraitsByRace() {
        if (race == RACE.HUMANS) {
            AddTrait("Beast Slayer");
        } else if (race == RACE.ELVES) {
            AddTrait("Anti Magic Aura");
        } else if (race == RACE.GOBLIN) {
            AddTrait("Soft Target");
        } else if (race == RACE.FAERY) {
            AddTrait("Melee Slayer");
        } else if (race == RACE.SKELETON) {
            AddTrait("Brittle Bones");
        } else if (race == RACE.DRAGON) {
            AddTrait("Steely Hide");
        } else if (race == RACE.SPIDER) {
            AddTrait("Faery Slayer");
        } else if (race == RACE.WOLF) {
            AddTrait("Goblin Slayer");
        } else if (race == RACE.ABOMINATION) {
            AddTrait("Elf Slayer");
        }
    }
    public bool AddTrait(string traitName, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null) {
        return AddTrait(AttributeManager.Instance.allTraits[traitName], characterResponsible, onRemoveAction, gainedFromDoing);
    }
    public bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null) {
        if (trait.IsUnique() && GetTrait(trait.name) != null) {
            trait.SetCharacterResponsibleForTrait(characterResponsible);
            if (trait.name == "Injured") { //TODO: Make this more elegant!(Need a way to still broadcast added traits even if it is a duplicate)
                marker.OnCharacterGainedTrait(this, GetTrait(trait.name));
            }
            return false;
        }
        _traits.Add(trait);
        trait.SetGainedFromDoing(gainedFromDoing);
        trait.SetOnRemoveAction(onRemoveAction);
        trait.SetCharacterResponsibleForTrait(characterResponsible);
        ApplyTraitEffects(trait);
        ApplyPOITraitInteractions(trait);
        if (trait.daysDuration > 0) {
            GameDate removeDate = GameManager.Instance.Today();
            removeDate.AddTicks(trait.daysDuration);
            SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTrait(trait));
        }
        trait.OnAddTrait(this);
        Messenger.Broadcast(Signals.TRAIT_ADDED, this, trait);
        if (trait is RelationshipTrait) {
            RelationshipTrait rel = trait as RelationshipTrait;
            AddRelationship(rel.targetCharacter, rel);
        }
        return true;
    }
    public bool RemoveTrait(Trait trait, bool triggerOnRemove = true) {
        if (_traits.Remove(trait)) {
            UnapplyTraitEffects(trait);
            UnapplyPOITraitInteractions(trait);
            if (triggerOnRemove) {
                trait.OnRemoveTrait(this);
            }
            Messenger.Broadcast(Signals.TRAIT_REMOVED, this, trait);
            if (trait is RelationshipTrait) {
                RelationshipTrait rel = trait as RelationshipTrait;
                RemoveRelationship(rel.targetCharacter, rel);
            }
            return true;
        }
        return false;
    }
    public bool RemoveTrait(string traitName, bool triggerOnRemove = true) {
        Trait trait = GetTrait(traitName);
        if (trait != null) {
            return RemoveTrait(trait, triggerOnRemove);
        }
        return false;
    }
    public void RemoveTrait(List<Trait> traits) {
        for (int i = 0; i < traits.Count; i++) {
            RemoveTrait(traits[i]);
        }
    }
    public void RemoveAllTraits(string traitNameException = "") {
        if(traitNameException == "") {
            while (traits.Count > 0) {
                RemoveTrait(traits[0]);
            }
        } else {
            for (int i = 0; i < traits.Count; i++) {
                if(traits[i].name != traitNameException) {
                    RemoveTrait(traits[i]);
                    i--;
                }
            }
        }
    }
    public Trait GetTrait(string traitName) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].name == traitName) {
                return _traits[i];
            }
        }
        return null;
    }
    public Trait GetTraitOr(string traitName1, string traitName2) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].name == traitName1 || _traits[i].name == traitName2) {
                return _traits[i];
            }
        }
        return null;
    }
    public Trait GetTraitOr(string traitName1, string traitName2, string traitName3) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].name == traitName1 || _traits[i].name == traitName2 || _traits[i].name == traitName3) {
                return _traits[i];
            }
        }
        return null;
    }
    public Trait GetTraitOr(string traitName1, string traitName2, string traitName3, string traitName4) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].name == traitName1 || _traits[i].name == traitName2 || _traits[i].name == traitName3 || _traits[i].name == traitName4) {
                return _traits[i];
            }
        }
        return null;
    }
    public bool HasTraitOf(TRAIT_TYPE traitType) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].type == traitType) {
                return true;
            }
        }
        return false;
    }
    public bool HasTraitOf(TRAIT_EFFECT effect, TRAIT_TYPE type) {
        for (int i = 0; i < traits.Count; i++) {
            Trait currTrait = traits[i];
            if (currTrait.effect == effect && currTrait.type == type) {
                return true;
            }
        }
        return false;
    }
    public bool HasTraitOf(TRAIT_EFFECT effect1, TRAIT_EFFECT effect2, TRAIT_TYPE type) {
        for (int i = 0; i < traits.Count; i++) {
            Trait currTrait = traits[i];
            if ((currTrait.effect == effect1 || currTrait.effect == effect2) && currTrait.type == type) {
                return true;
            }
        }
        return false;
    }
    public bool HasTraitOf(TRAIT_EFFECT effect) {
        for (int i = 0; i < traits.Count; i++) {
            Trait currTrait = traits[i];
            if (currTrait.effect == effect) {
                return true;
            }
        }
        return false;
    }
    public Trait GetTraitOf(TRAIT_EFFECT effect, TRAIT_TYPE type) {
        for (int i = 0; i < traits.Count; i++) {
            Trait currTrait = traits[i];
            if (currTrait.effect == effect && currTrait.type == type) {
                return currTrait;
            }
        }
        return null;
    }
    public Trait GetTraitOf(TRAIT_TYPE type) {
        for (int i = 0; i < traits.Count; i++) {
            Trait currTrait = traits[i];
            if (currTrait.type == type) {
                return currTrait;
            }
        }
        return null;
    }
    public List<Trait> RemoveAllTraitsByType(TRAIT_TYPE traitType) {
        List<Trait> removedTraits = new List<Trait>();
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].type == traitType) {
                removedTraits.Add(_traits[i]);
                _traits.RemoveAt(i);
                i--;
            }
        }
        return removedTraits;
    }
    public Trait GetRandomTrait(TRAIT_EFFECT effect) {
        List<Trait> negativeTraits = new List<Trait>();
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].effect == effect) {
                negativeTraits.Add(_traits[i]);
            }
        }
        if (negativeTraits.Count > 0) {
            return negativeTraits[UnityEngine.Random.Range(0, negativeTraits.Count)];
        }
        return null;
    }
    private void ApplyTraitEffects(Trait trait) {
        if(trait.type == TRAIT_TYPE.DISABLER) {
            AdjustDoNotDisturb(1);
            if(trait.effect == TRAIT_EFFECT.NEGATIVE) {
                AdjustIgnoreHostilities(1);
            }
        }
        if(trait.name == "Abducted" || trait.name == "Restrained") {
            AdjustDoNotGetTired(1);
        } else if (trait.name == "Packaged" || trait.name == "Hibernating" || trait.name == "Reanimated") {
            AdjustDoNotGetTired(1);
            AdjustDoNotGetHungry(1);
            AdjustDoNotGetLonely(1);
        } else if (trait.name == "Eating") {
            AdjustDoNotGetHungry(1);
        } else if (trait.name == "Resting") {
            AdjustDoNotGetTired(1);
            AdjustDoNotGetHungry(1);
            AdjustDoNotGetLonely(1);
        } else if (trait.name == "Charmed") {
            AdjustDoNotGetLonely(1);
        } else if (trait.name == "Daydreaming") {
            AdjustDoNotGetTired(1);
            AdjustDoNotGetLonely(1);
        } else if (trait.name == "Forlorn") {
            AdjustMoodValue(-35);
        } else if (trait.name == "Sad") {
            AdjustMoodValue(-20);
        } else if (trait.name == "Exhausted") {
            AdjustMoodValue(-35);
        } else if (trait.name == "Tired") {
            AdjustMoodValue(-10);
        } else if (trait.name == "Starving") {
            AdjustMoodValue(-25);
        } else if (trait.name == "Hungry") {
            AdjustMoodValue(-10);
        } else if (trait.name == "Injured") {
            AdjustMoodValue(-15);
        } else if (trait.name == "Cursed") {
            AdjustMoodValue(-25);
        } else if (trait.name == "Sick") {
            AdjustMoodValue(-15);
        }
        //else if (trait.name == "Hungry") {
        //    CreateFeedJob();
        //} else if (trait.name == "Starving") {
        //    MoveFeedJobToTopPriority();
        //}
        for (int i = 0; i < trait.effects.Count; i++) {
            TraitEffect traitEffect = trait.effects[i];
            if (!traitEffect.hasRequirement && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                if (traitEffect.isPercentage) {
                    if (traitEffect.stat == STAT.ATTACK) {
                        AdjustAttackPercentMod((int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.HP) {
                        AdjustMaxHPPercentMod((int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.SPEED) {
                        AdjustSpeedPercentMod((int) traitEffect.amount);
                    }
                } else {
                    if (traitEffect.stat == STAT.ATTACK) {
                        AdjustAttackMod((int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.HP) {
                        AdjustMaxHPMod((int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.SPEED) {
                        AdjustSpeedMod((int) traitEffect.amount);
                    }
                }
            }
        }
    }
    private void UnapplyTraitEffects(Trait trait) {
        if (trait.type == TRAIT_TYPE.DISABLER) {
            AdjustDoNotDisturb(-1);
            if (trait.effect == TRAIT_EFFECT.NEGATIVE) {
                AdjustIgnoreHostilities(-1);
            }
        }
        if (trait.name == "Abducted" || trait.name == "Restrained") {
            AdjustDoNotGetTired(-1);
        } else if (trait.name == "Packaged" || trait.name == "Hibernating" || trait.name == "Reanimated") {
            AdjustDoNotGetTired(-1);
            AdjustDoNotGetHungry(-1);
            AdjustDoNotGetLonely(-1);
        } else if (trait.name == "Eating") {
            AdjustDoNotGetHungry(-1);
        } else if (trait.name == "Resting") {
            AdjustDoNotGetTired(-1);
            AdjustDoNotGetHungry(-1);
            AdjustDoNotGetLonely(-1);
        } else if (trait.name == "Charmed") {
            AdjustDoNotGetLonely(-1);
        } else if (trait.name == "Daydreaming") {
            AdjustDoNotGetTired(-1);
            AdjustDoNotGetLonely(-1);
        } else if (trait.name == "Forlorn") {
            AdjustMoodValue(35);
        } else if (trait.name == "Sad") {
            AdjustMoodValue(20);
        } else if (trait.name == "Exhausted") {
            AdjustMoodValue(35);
        } else if (trait.name == "Tired") {
            AdjustMoodValue(10);
        } else if (trait.name == "Starving") {
            AdjustMoodValue(25);
        } else if (trait.name == "Hungry") {
            AdjustMoodValue(10);
        } else if (trait.name == "Injured") {
            AdjustMoodValue(15);
        } else if (trait.name == "Cursed") {
            AdjustMoodValue(25);
        } else if (trait.name == "Sick") {
            AdjustMoodValue(15);
        }
        for (int i = 0; i < trait.effects.Count; i++) {
            TraitEffect traitEffect = trait.effects[i];
            if (!traitEffect.hasRequirement && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                if (traitEffect.isPercentage) {
                    if (traitEffect.stat == STAT.ATTACK) {
                        AdjustAttackPercentMod(-(int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.HP) {
                        AdjustMaxHPPercentMod(-(int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.SPEED) {
                        AdjustSpeedPercentMod(-(int) traitEffect.amount);
                    }
                } else {
                    if (traitEffect.stat == STAT.ATTACK) {
                        AdjustAttackMod(-(int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.HP) {
                        AdjustMaxHPMod(-(int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.SPEED) {
                        AdjustSpeedMod(-(int) traitEffect.amount);
                    }
                }
            }
        }
    }
    private void ApplyPOITraitInteractions(Trait trait) {
        if (trait.advertisedInteractions != null) {
            for (int i = 0; i < trait.advertisedInteractions.Count; i++) {
                poiGoapActions.Add(trait.advertisedInteractions[i]);
            }
        }
    }
    private void UnapplyPOITraitInteractions(Trait trait) {
        if (trait.advertisedInteractions != null) {
            for (int i = 0; i < trait.advertisedInteractions.Count; i++) {
                poiGoapActions.Remove(trait.advertisedInteractions[i]);
            }
        }
    }
    private void SetTraitsFromClass() {
        if (_characterClass.traitNames != null) {
            for (int i = 0; i < _characterClass.traitNames.Length; i++) {
                Trait trait = AttributeManager.Instance.allTraits[_characterClass.traitNames[i]];
                AddTrait(trait);
            }
        }
    }
    public void RemoveTraitsFromClass() {
        if (_characterClass.traitNames != null) {
            for (int i = 0; i < _characterClass.traitNames.Length; i++) {
                Trait trait = AttributeManager.Instance.allTraits[_characterClass.traitNames[i]];
                RemoveTrait(trait);
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
    public void RemoveTraitsFromRace() {
        if (_raceSetting.traitNames != null) {
            for (int i = 0; i < _raceSetting.traitNames.Length; i++) {
                Trait trait = AttributeManager.Instance.allTraits[_raceSetting.traitNames[i]];
                RemoveTrait(trait);
            }
        }
    }
    public Friend GetFriendTraitWith(Character character) {
        for (int i = 0; i < _traits.Count; i++) {
            if(_traits[i] is Friend) {
                Friend friendTrait = _traits[i] as Friend;
                if(friendTrait.targetCharacter.id == character.id) {
                    return friendTrait;
                }
            }
        }
        return null;
    }
    public Enemy GetEnemyTraitWith(Character character) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i] is Enemy) {
                Enemy enemyTrait = _traits[i] as Enemy;
                if (enemyTrait.targetCharacter == character) {
                    return enemyTrait;
                }
            }
        }
        return null;
    }
    public bool HasRelationshipTraitOf(RELATIONSHIP_TRAIT relType) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i] is RelationshipTrait) {
                RelationshipTrait currTrait = _traits[i] as RelationshipTrait;
                if (currTrait.relType == relType) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipTraitOf(RELATIONSHIP_TRAIT relType, Faction except) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i] is RelationshipTrait) {
                RelationshipTrait currTrait = _traits[i] as RelationshipTrait;
                if (currTrait.relType == relType 
                    && currTrait.targetCharacter.faction.id != except.id) {
                    return true;
                }
            }
        }
        return false;
    }
    public void GenerateRandomTraits() {
        
    }
    public bool ReleaseFromAbduction() {
        Trait trait = GetTrait("Abducted");
        if (trait != null) {
            Abducted abductedTrait = trait as Abducted;
            RemoveTrait(abductedTrait);
            ReturnToOriginalHomeAndFaction(abductedTrait.originalHome, this.faction);
            //MigrateTo(abductedTrait.originalHomeLandmark);

            Interaction interactionAbducted = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, specificLocation);
            InduceInteraction(interactionAbducted);
            return true;
        }
        return false;
    }
    public SpecialToken CraftAnItem() {
        Craftsman craftsmanTrait = GetTrait("Craftsman") as Craftsman;
        if(craftsmanTrait != null) {
            //SpecialTokenSettings settings = TokenManager.Instance.GetTokenSettings(craftsmanTrait.craftedItemName);
            //return TokenManager.Instance.CreateSpecialToken(craftsmanTrait.craftedItemName); //, settings.appearanceWeight
        }
        return null;
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
        MigrateHomeTo(PlayerManager.Instance.player.playerArea);

        specificLocation.RemoveCharacterFromLocation(this.currentParty);
        //PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(this.currentParty);

        ChangeFactionTo(PlayerManager.Instance.player.playerFaction);

        Minion newMinion = PlayerManager.Instance.player.CreateNewMinion(this);
        PlayerManager.Instance.player.AddMinion(newMinion);

        SetForcedInteraction(null);

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
    public void AddInteractionType(INTERACTION_TYPE type) {
        currentInteractionTypes.Add(type);
    }
    public void RemoveInteractionType(INTERACTION_TYPE type) {
        currentInteractionTypes.Remove(type);
    }
    private int GetMonthInteractionTick() {
        int daysInMonth = 15; //GameManager.daysInMonth[GameManager.Instance.month]
        int remainingDaysInMonth = GameManager.Instance.continuousDays % daysInMonth;
        int startDay = GameManager.Instance.continuousDays + remainingDaysInMonth + 1;
        return UnityEngine.Random.Range(startDay, startDay + daysInMonth);
    }
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
        //if(specificLocation == null || specificLocation.id == homeArea.id) {
        //    _currentInteractionTick = GetMonthInteractionTick();
        //} else {
        //    int remainingDaysInWeek = GameManager.Instance.continuousDays % 7;
        //    int startDay = GameManager.Instance.continuousDays + remainingDaysInWeek + 1;
        //    _currentInteractionTick = UnityEngine.Random.Range(startDay, startDay + 7);
        //}
        int remainingDaysInWeek = GameManager.ticksPerTimeInWords - (GameManager.Instance.tick % GameManager.ticksPerTimeInWords);
        if(remainingDaysInWeek == GameManager.ticksPerTimeInWords) {
            remainingDaysInWeek = 0;
        }
        int startDay = GameManager.Instance.tick + remainingDaysInWeek + 1;
        if(startDay > GameManager.ticksPerDay) {
            startDay -= GameManager.ticksPerDay;
        }
        //if (startDay < 25) {
        //    startDay = 25;
        //}
        _currentInteractionTick = UnityEngine.Random.Range(startDay, startDay + GameManager.ticksPerTimeInWords);
    }
    public void AdjustDailyInteractionGenerationTick() {
        //Adjusts the tick trigger for this character to compensate for the 5 day delay interaction trigger
        //Characters trigger AI once every 12 ticks but exclude any tick that will be reached by the 5-tick wait time of the preceding tick.
        int remainingDaysInWeek = GameManager.ticksPerTimeInWords - (GameManager.Instance.tick % GameManager.ticksPerTimeInWords);
        if (remainingDaysInWeek < GameManager.ticksPerTimeInWords) {
            int startDay = GameManager.Instance.tick + remainingDaysInWeek + 1;
            _currentInteractionTick = UnityEngine.Random.Range(GameManager.Instance.tick + 1, startDay);
        }
    }
    public void SetDailyInteractionGenerationTick(int tick) {
        _currentInteractionTick = tick;
    } 
    public void DailyInteractionGeneration() {
        if (_currentInteractionTick == GameManager.Instance.tick) {
            GenerateDailyInteraction();
            SetDailyInteractionGenerationTick();
        } 
    }
    public void GenerateDailyInteraction() {
        if (!IsInOwnParty() || isDefender || ownParty.icon.isTravelling || _doNotDisturb > 0 || _job == null) {
            return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
        }
        string interactionLog = GameManager.Instance.TodayLogString() + "Generating daily interaction for " + this.name;
        if (_forcedInteraction != null && GameManager.GetCurrentTimeInWordsOfTick() != TIME_IN_WORDS.AFTER_MIDNIGHT && GetTraitOr("Starving", "Exhausted") == null) {
            //Only do override actions on morning, afternoon, or night
            //the current day is valid for the override's next action component
            //the character is not Starving and not Exhausted
            interactionLog += "\nUsing forced interaction: " + _forcedInteraction.type.ToString();
            AddInteraction(_forcedInteraction);
            _forcedInteraction = null;
        } else {
            CharacterPersonalActions();
        }
    }
    public void StartDailyGoapPlanGeneration() {
        if (!_activateDailyGoapPlanInteraction) {
            _activateDailyGoapPlanInteraction = true;
            PlanGoapActions();
            //SetDailyGoapGenerationTick();
            //Messenger.AddListener(Signals.TICK_STARTED, DailyGoapPlanGeneration);
        }
        //_currentInteractionTick = GameManager.Instance.tick;
        //DailyGoapPlanGeneration();
    }
    public void StopDailyGoapPlanGeneration() {
        if (_activateDailyGoapPlanInteraction) {
            _activateDailyGoapPlanInteraction = false;
            //Messenger.RemoveListener(Signals.TICK_STARTED, DailyGoapPlanGeneration);
        }
    }
    private void DailyGoapPlanGeneration() {
        //This is to ensure that this character will not be idle forever
        //If at the start of the tick, the character is not currently doing any action, and is not waiting for any new plans, it means that the character will no longer perform any actions
        //so start doing actions again
        SetHasAlreadyAskedForPlan(false);
        if(currentAction == null && _numOfWaitingForGoapThread <= 0) {
            PlanGoapActions();
        }
        //if(_currentInteractionTick == GameManager.Instance.tick) {
        //    PlanGoapActions();
        //    SetDailyGoapGenerationTick();
        //}
    }
    public void SetDailyGoapGenerationTick() {
        int remainingDaysInWeek = 6 - (GameManager.Instance.tick % 6);
        if (remainingDaysInWeek == 6) {
            remainingDaysInWeek = 0;
        }
        int startDay = GameManager.Instance.tick + remainingDaysInWeek + 1;
        if (startDay > GameManager.ticksPerDay) {
            startDay -= GameManager.ticksPerDay;
        }
        _currentInteractionTick = UnityEngine.Random.Range(startDay, startDay + 6);
    }
    public void PlanGoapActions(GoapAction specificAction = null) {
        if (isDead) {
            //StopDailyGoapPlanGeneration();
            return;
        }
        if (minion != null || !IsInOwnParty() || isDefender || ownParty.icon.isTravelling || _doNotDisturb > 0 || _job == null || isWaitingForInteraction > 0 || marker.pathfindingThread != null) {
            return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
        }
        if(stateComponent.currentState != null) {
            Debug.LogWarning("Currently in " + stateComponent.currentState.stateName + " state, can't plan actions!");
            return;
        }
        if(specificAction != null) {
            WillAboutToDoAction(specificAction);
        }
        if(allGoapPlans.Count > 0) {
            //StopDailyGoapPlanGeneration();
            PerformGoapPlans();
            //SchedulePerformGoapPlans();
        } else {
            IdlePlans();
        }
    }
    private void IdlePlans() {
        if (_hasAlreadyAskedForPlan) {
            return;
        }
        SetHasAlreadyAskedForPlan(true);
        if (!OtherPlanCreations()) {
            if (!PlanFullnessRecoveryActions()) {
                if (!PlanTirednessRecoveryActions()) {
                    if (!PlanHappinessRecoveryActions()) {
                        //bool hasAddedToGoapPlans = false;
                        if (!PlanWorkActions()) { //ref hasAddedToGoapPlans
                            OtherIdlePlans();
                        } 
                        //else {
                        //    if (hasAddedToGoapPlans) {
                        //        PlanGoapActions();
                        //    }
                        //}
                    }
                }
            }
        }
    }
    private bool OtherPlanCreations() {
        int chance = UnityEngine.Random.Range(0, 100);
        if (GetTrait("Berserker") != null) {
            if(chance < 15) {
                Character target = specificLocation.GetRandomCharacterAtLocationExcept(this);
                if (target != null) {
                    StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = target }, target, GOAP_CATEGORY.NONE);
                    return true;
                }
            } else {
                chance = UnityEngine.Random.Range(0, 100);
                if (chance < 15) {
                    IPointOfInterest target = specificLocation.GetRandomTileObject();
                    if (target != null) {
                        StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DESTROY, conditionKey = target, targetPOI = target }, target, GOAP_CATEGORY.NONE);
                        return true;
                    }
                }
            }
        }
        return false;
    }
    private bool PlanFullnessRecoveryActions() {
        TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        Trait hungryOrStarving = GetTraitOr("Starving", "Hungry");

        if (hungryOrStarving != null && GetPlanByCategory(GOAP_CATEGORY.FULLNESS) == null) {
            int chance = UnityEngine.Random.Range(0, 100);
            int value = 0;
            if (hungryOrStarving.name == "Starving") {
                value = 100;
            } else {
                if(currentTimeInWords == TIME_IN_WORDS.MORNING) {
                    value = 25;
                }else if (currentTimeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
                    value = 50;
                }
            }
            if (chance < value) {
                StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, this, GOAP_CATEGORY.FULLNESS, true);
                return true;
            }
        }
        return false;
    }
    private bool PlanTirednessRecoveryActions() {
        TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        Trait tiredOrExhausted = GetTraitOr("Exhausted", "Tired");

        if (tiredOrExhausted != null && GetPlanByCategory(GOAP_CATEGORY.TIREDNESS) == null) {
            int chance = UnityEngine.Random.Range(0, 100);
            int value = 0;
            if (tiredOrExhausted.name == "Exhausted") {
                value = 100;
            } else {
                if (currentTimeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
                    value = 15;
                } else if (currentTimeInWords == TIME_IN_WORDS.LATE_NIGHT) {
                    value = 65;
                } else if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                    value = 65;
                }
            }
            if (chance < value) {
                StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = this }, this, GOAP_CATEGORY.TIREDNESS, true);
                return true;
            }
        }
        return false;
    }
    private bool PlanHappinessRecoveryActions() {
        TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        Trait lonelyOrForlorn = GetTraitOr("Forlorn", "Lonely");

        if (lonelyOrForlorn != null && GetPlanByCategory(GOAP_CATEGORY.HAPPINESS) == null) {
            int chance = UnityEngine.Random.Range(0, 100);
            int value = 0;
            if (lonelyOrForlorn.name == "Forlorn") {
                value = 100;
            } else {
                if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                    value = 25;
                } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                    value = 40;
                } else if (currentTimeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
                    value = 40;
                } else if (currentTimeInWords == TIME_IN_WORDS.LATE_NIGHT) {
                    value = 25;
                }
            }
            if (chance < value) {
                StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = this }, this, GOAP_CATEGORY.HAPPINESS, true);
                return true;
            }
        }
        return false;
    }
    private bool PlanWorkActions() { //ref bool hasAddedToGoapPlans
        if (this.faction.id != FactionManager.Instance.neutralFaction.id && GetPlanByCategory(GOAP_CATEGORY.WORK) == null) {
            if(GetTrait("Berserker") != null) {
                return false;
            }
            if (!jobQueue.ProcessFirstJobInQueue(this)) {
                if (specificLocation.id == homeArea.id) {
                    return homeArea.jobQueue.ProcessFirstJobInQueue(this);
                } else {
                    return false;
                }
            } else {
                return true;
            }
            //WeightedDictionary<INTERACTION_TYPE> weightedDictionary = new WeightedDictionary<INTERACTION_TYPE>();
            ////Drop Supply Plan
            //if (supply > role.reservedSupply) {
            //    weightedDictionary.AddElement(INTERACTION_TYPE.DROP_SUPPLY, 2);
            //}
            ////Obtain Supply Plan
            //if (role.roleType == CHARACTER_ROLE.CIVILIAN) {
            //    SupplyPile supplyPile = homeArea.supplyPile;
            //    if (supplyPile.suppliesInPile < 200) {
            //        weightedDictionary.AddElement(INTERACTION_TYPE.GET_SUPPLY, 4);
            //    }
            //} else {
            //    if (supply < role.reservedSupply) {
            //        weightedDictionary.AddElement(INTERACTION_TYPE.GET_SUPPLY, 4);
            //    }
            //}

            //role.AddRoleWorkPlansToCharacterWeights(weightedDictionary);

            //if (weightedDictionary.Count > 0) {
            //    INTERACTION_TYPE result = weightedDictionary.PickRandomElementGivenWeights();
            //    SupplyPile supplyPile = homeArea.supplyPile;
            //    if (result == INTERACTION_TYPE.DROP_SUPPLY) {
            //        StartGOAP(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_SUPPLY, supply, supplyPile), supplyPile, GOAP_CATEGORY.WORK);
            //    } else if (result == INTERACTION_TYPE.GET_SUPPLY) {
            //        StartGOAP(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_SUPPLY, supplyPile.suppliesInPile, this), this, GOAP_CATEGORY.WORK);
            //    } else {
            //        //Role work plans
            //        if(result == INTERACTION_TYPE.PATROL) {
            //            stateComponent.SwitchToState(CHARACTER_STATE.PATROL);
            //        } else if (result == INTERACTION_TYPE.EXPLORE) {
            //            stateComponent.SwitchToState(CHARACTER_STATE.EXPLORE);
            //        } else {
            //            GoapPlan plan = role.PickRoleWorkPlanFromCharacterWeights(result, this);
            //            if (plan != null) {
            //                allGoapPlans.Add(plan);
            //                hasAddedToGoapPlans = true;
            //            }
            //        }
            //    }
            //    return true;
            //} else {
            //    return false;
            //}
        }
        return false;
    }
    public bool PlanIdleStroll(LocationStructure targetStructure) {
        //Debug.Log("---------" + GameManager.Instance.TodayLogString() + "CREATING IDLE STROLL ACTION FOR " + name + "-------------");
        //if(currentStructure.unoccupiedTiles.Count > 0) {
        Stroll goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.STROLL, this, this) as Stroll;
        goapAction.SetTargetStructure(targetStructure);
        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        allGoapPlans.Add(goapPlan);
        PlanGoapActions(goapAction);
        return true;
        //}
        //return false;
    }
    public bool PlanIdleReturnHome() {
        if (GetTrait("Berserker") != null) {
            //Return home becomes stroll if the character has berserker trait
            PlanIdleStroll(currentStructure);
        } else {
            GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.RETURN_HOME, this, this);
            goapAction.SetTargetStructure();
            GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
            GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
            allGoapPlans.Add(goapPlan);
            PlanGoapActions(goapAction);
        }
        return true;
    }
    private void OtherIdlePlans() {
        if (homeStructure != null && currentStructure.structureType == STRUCTURE_TYPE.DWELLING && currentStructure != homeStructure) {
            PlanIdleReturnHome();
        } else if (homeStructure != null) {
            int chance = UnityEngine.Random.Range(0, 100);
            int returnHomeChance = 0;
            if(currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || currentStructure.structureType == STRUCTURE_TYPE.DUNGEON || currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA) {
                returnHomeChance = 25;
            }else if (currentStructure.structureType == STRUCTURE_TYPE.WAREHOUSE || currentStructure.structureType == STRUCTURE_TYPE.INN) {
                returnHomeChance = 75;
            }
            if(chance < returnHomeChance) {
                PlanIdleReturnHome();
            } else {
                PlanIdleStroll(currentStructure);
            }
        } else {
            PlanIdleStroll(currentStructure);
        }
        //PlanGoapActions();
    }
    public bool AssaultCharacter(Character target) {
        //Debug.Log(this.name + " will assault " + target.name);
        hasAssaultPlan = true;
        lastAssaultedCharacter = target;
        //Debug.Log("---------" + GameManager.Instance.TodayLogString() + "CREATING IDLE STROLL ACTION FOR " + name + "-------------");
        //if(currentStructure.unoccupiedTiles.Count > 0) {
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.ASSAULT_ACTION_NPC, this, target);
        goapAction.SetEndAction(OnAssaultActionReturn);
        //goapAction.SetTargetStructure(target);
        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.DEATH }, GOAP_CATEGORY.REACTION);
        goapPlan.SetDoNotRecalculate(true);
        AddPlanAsPriority(goapPlan);
        if (currentAction != null) {
            currentAction.StopAction();
        }
        return true;
        //}
        //return false;
    }
    private void SetLastAssaultedCharacter(Character character) {
        lastAssaultedCharacter = character;
        if (character != null) {
            //cooldown
            GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.ticksPerHour);
            SchedulingManager.Instance.AddEntry(dueDate, () => RemoveLastAssaultedCharacter(character));
        }
    }
    private void RemoveLastAssaultedCharacter(Character characterToRemove) {
        if (lastAssaultedCharacter == characterToRemove) {
            SetLastAssaultedCharacter(null);
        }
    }
    private void OnAssaultActionReturn(string result, GoapAction action) {
        hasAssaultPlan = false;
        SetLastAssaultedCharacter(action.poiTarget as Character);
        GoapActionResult(result, action);
        //if (action == currentAction) {
        //    SetCurrentAction(null);
        //}
        //if (isDead) {
        //    return;
        //}
        //GoapPlan plan = GetPlanWithAction(action);
        //if (action.isStopped) {
        //    if (!DropPlan(plan)) {
        //        PlanGoapActions();
        //    }
        //    return;
        //}
        //if (plan == null) {
        //    PlanGoapActions();
        //    return;
        //}
        //if (result == InteractionManager.Goap_State_Success) {
        //    plan.SetNextNode();
        //    if (plan.currentNode == null) {
        //        //this means that this is the end goal so end this plan now
        //        if (!DropPlan(plan)) {
        //            PlanGoapActions();
        //        }
        //    } else {
        //        PlanGoapActions();
        //    }
        //} else if (result == InteractionManager.Goap_State_Fail) {
        //    if (plan.endNode.action == action) {
        //        if (!DropPlan(plan)) {
        //            PlanGoapActions();
        //        }
        //    } else {
        //        if (plan.doNotRecalculate) {
        //            if (!DropPlan(plan)) {
        //                PlanGoapActions();
        //            }
        //        } else {
        //            RecalculatePlan(plan);
        //        }
        //    }
        //}
    }
    public void ChatCharacter(Character targetCharacter) {
        SetIsChatting(true);
        targetCharacter.SetIsChatting(true);
        marker.UpdateActionIcon();
        targetCharacter.marker.UpdateActionIcon();

        Log chatLog = new Log(GameManager.Instance.Today(), "GoapAction", "ChatCharacter", "chat success_description");
        chatLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        chatLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        chatLog.AddLogToInvolvedObjects();

        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(4);
        SchedulingManager.Instance.AddEntry(dueDate, () => EndChatCharacter(targetCharacter));
    }
    private void EndChatCharacter(Character targetCharacter) {
        SetIsChatting(false);
        targetCharacter.SetIsChatting(false);
        marker.UpdateActionIcon();
        targetCharacter.marker.UpdateActionIcon();
    }
    public void SetIsChatting(bool state) {
        _isChatting = state;
    }
    public void SetForcedInteraction(Interaction interaction) {
        _forcedInteraction = interaction;
    }
    public void InduceInteraction(Interaction interaction) {
        SetForcedInteraction(interaction);
        SetDailyInteractionGenerationTick(GameManager.Instance.continuousDays + 1);
    }
    private void DayStartedRemoveOverrideInteraction() {
        if(_forcedInteraction != null && !_forcedInteraction.cannotBeClearedOut) {
            SetForcedInteraction(null);
        }
    }
    private void CharacterPersonalActions() {
        //Checker of Disabler trait is on GenerateDailyInteraction()
        string interactionLog = GameManager.Instance.TodayLogString() + "GENERATING CHARACTER PERSONAL ACTIONS FOR " + this.name + " in " + specificLocation.name;
        WeightedDictionary<INTERACTION_TYPE> personalActionWeights = new WeightedDictionary<INTERACTION_TYPE>();
        Character targetCharacter = null;
        Character otherCharacter = null;
        INTERACTION_TYPE chosenRelationshipInteraction = INTERACTION_TYPE.NONE;
        List<string> allNegativeTraitNames = new List<string>();
        //string timeInWordsString = GameManager.GetCurrentTimeInWordsOfTick().ToString();   
        TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick();

        bool isHungry = false, isStarving = false, isTired = false, isExhausted = false;
        for (int i = 0; i < _traits.Count; i++) {
            if(_traits[i].name == "Hungry") {
                isHungry = true;
            }else if (_traits[i].name == "Starving") {
                isStarving = true;
            } else if (_traits[i].name == "Tired") {
                isTired = true;
            } else if (_traits[i].name == "Exhausted") {
                isExhausted = true;
            }
        }
        interactionLog += "\nisHungry: " + isHungry + ", isStarving: " + isStarving + ", isTired: " + isTired + ", isExhausted: " + isExhausted;
        interactionLog += "\n------------------ WEIGHTS ----------------";
        //**B. If the character is Hungry or Starving, Fullness Recovery-type weight is increased**
        if (isHungry) {
            List<INTERACTION_TYPE> fullnessRecoveryInteractions = RaceManager.Instance.GetNPCInteractionsOfRace(this, INTERACTION_CATEGORY.FULLNESS_RECOVERY);
            if(fullnessRecoveryInteractions.Count > 0) {
                int weight = 0;
                if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                    weight += 0;
                } else if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                    weight += 100;
                } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                    weight += 500;
                } else {
                    weight += 0;
                }
                if (weight > 0) {
                    INTERACTION_TYPE chosenType = fullnessRecoveryInteractions[UnityEngine.Random.Range(0, fullnessRecoveryInteractions.Count)];
                    personalActionWeights.AddElement(chosenType, weight);
                    interactionLog += "\nFULLNESS RECOVERY: " + chosenType.ToString() + " - " + weight;
                }
            }
        } else if (isStarving) {
            List<INTERACTION_TYPE> fullnessRecoveryInteractions = RaceManager.Instance.GetNPCInteractionsOfRace(this, INTERACTION_CATEGORY.FULLNESS_RECOVERY);
            if (fullnessRecoveryInteractions.Count > 0) {
                int weight = 0;
                if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                    weight += 0;
                } else if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                    weight += 100;
                } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                    weight += 1000;
                } else {
                    weight += 0;
                }
                if(weight > 0) {
                    INTERACTION_TYPE chosenType = fullnessRecoveryInteractions[UnityEngine.Random.Range(0, fullnessRecoveryInteractions.Count)];
                    personalActionWeights.AddElement(chosenType, weight);
                    interactionLog += "\nFULLNESS RECOVERY: " + chosenType.ToString() + " - " + weight;
                }
            }
        }

        //**C.If the character is Tired or Exhausted, Tiredness Recovery-type weight is increased**
        if (isTired) {
            List<INTERACTION_TYPE> tirednessRecoveryInteractions = RaceManager.Instance.GetNPCInteractionsOfRace(this, INTERACTION_CATEGORY.TIREDNESS_RECOVERY);
            if (tirednessRecoveryInteractions.Count > 0) {
                int weight = 0;
                if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                    weight += 100;
                } else if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                    weight += 0;
                } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                    weight += 0;
                } else {
                    weight += 500;
                }
                if (weight > 0) {
                    INTERACTION_TYPE chosenType = tirednessRecoveryInteractions[UnityEngine.Random.Range(0, tirednessRecoveryInteractions.Count)];
                    personalActionWeights.AddElement(chosenType, weight);
                    interactionLog += "\nTIREDNESS RECOVERY: " + chosenType.ToString() + " - " + weight;
                }
            }
        } else if (isExhausted) {
            List<INTERACTION_TYPE> tirednessRecoveryInteractions = RaceManager.Instance.GetNPCInteractionsOfRace(this, INTERACTION_CATEGORY.TIREDNESS_RECOVERY);
            if (tirednessRecoveryInteractions.Count > 0) {
                int weight = 0;
                if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                    weight += 100;
                } else if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                    weight += 0;
                } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                    weight += 0;
                } else {
                    weight += 1000;
                }
                if (weight > 0) {
                    INTERACTION_TYPE chosenType = tirednessRecoveryInteractions[UnityEngine.Random.Range(0, tirednessRecoveryInteractions.Count)];
                    personalActionWeights.AddElement(chosenType, weight);
                    interactionLog += "\nTIREDNESS RECOVERY: " + chosenType.ToString() + " - " + weight;
                }
            }
        }

        //**D. if character is non-Beast, non-Skeleton and not Charmed, loop through relationships and decide what action to do per character then determine weights**
        if(role.roleType != CHARACTER_ROLE.BEAST && race != RACE.SKELETON && !isStarving && !isExhausted && GetTrait("Charmed") == null) {
            WeightedDictionary<CharacterRelationshipData> characterWeights = new WeightedDictionary<CharacterRelationshipData>();
            interactionLog += "\n\n----CHARACTER NPC ACTION TYPES----";
            interactionLog += "\nPOSSIBLE TARGETS:\n";
            foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in relationships) {
                if (specificLocation.id == kvp.Key.specificLocation.id && !kvp.Key.currentParty.icon.isTravelling && !kvp.Key.isDefender && kvp.Value.knownStructure != null && kvp.Value.knownStructure.location.id == specificLocation.id) {
                    int weight = kvp.Value.GetTotalRelationshipWeight();
                    if (kvp.Value.isCharacterMissing && !kvp.Value.HasRelationshipTrait(RELATIONSHIP_TRAIT.ENEMY)) {
                        if (kvp.Value.isCharacterLocated) {
                            weight += 25;
                        } else {
                            weight = 0;
                        }
                    }
                    if (kvp.Value.encounterMultiplier > 0f && weight > 0) {
                        weight += (int)(weight * kvp.Value.encounterMultiplier);
                    }
                    interactionLog += kvp.Key.name + "(weight=" + weight + "), ";
                    if (weight > 0) {
                        characterWeights.AddElement(kvp.Value, weight);
                    }
                }
            }
            if(characterWeights.Count > 0) {
                CharacterRelationshipData chosenData = characterWeights.PickRandomElementGivenWeights();
                int weight = 0;
                targetCharacter = chosenData.targetCharacter;
                INTERACTION_CATEGORY category = INTERACTION_CATEGORY.OTHER;
                interactionLog += "\nCHOSEN TARGET: " + targetCharacter.name;
                interactionLog += "\n---WEIGHTS---";
                chosenRelationshipInteraction = CharacterNPCActionTypes(chosenData, ref weight, ref targetCharacter, ref otherCharacter, ref interactionLog, ref category);
                if (chosenRelationshipInteraction != INTERACTION_TYPE.NONE) {
                    if(category == INTERACTION_CATEGORY.SUBTERFUGE) {
                        if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                            weight += 20;
                        } else if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                            weight += 20;
                        } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                            weight += 20;
                        } else {
                            weight += 100;
                        }
                    } else {
                        if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                            weight += 0;
                        } else if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                            weight += 100;
                        } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                            weight += 100;
                        } else {
                            weight += 20;
                        }
                    }
                    if(!chosenData.isCharacterMissing && !chosenData.HasRelationshipTrait(RELATIONSHIP_TRAIT.ENEMY)) {
                        weight *= 3;
                    }
                    if (weight > 0) {
                        personalActionWeights.AddElement(chosenRelationshipInteraction, weight);
                        interactionLog += "\nCHOSEN INTERACTION FOR CHARACTER NPC ACTION TYPES: " + chosenRelationshipInteraction.ToString() + " - " + weight;
                    }
                }
            }
            interactionLog += "\n----END CHARACTER NPC ACTION TYPES----\n";
        }

        //**E. loop through relevant traits then add relevant weights per associated action**
        for (int i = 0; i < _traits.Count; i++) {
            Trait trait = _traits[i];
            if(trait.name == "Lycanthrope") {
                //Special case for Lycanthrope trait
                if(currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                    if(race == RACE.WOLF) {
                        personalActionWeights.AddElement(INTERACTION_TYPE.REVERT_TO_NORMAL, 100);
                        interactionLog += "\nTRAIT ACTION: REVERT_TO_NORMAL - 100";
                    } else {
                        personalActionWeights.AddElement(INTERACTION_TYPE.TRANSFORM_TO_WOLF, 100);
                        interactionLog += "\nTRAIT ACTION: TURN_TO_WOLF - 100";
                    }
                }
            } else {
                if (trait.associatedInteraction != INTERACTION_TYPE.NONE && InteractionManager.Instance.CanCreateInteraction(trait.associatedInteraction, this, targetCharacter)) {
                    personalActionWeights.AddElement(trait.associatedInteraction, 100);
                    interactionLog += "\nTRAIT ACTION: " + trait.associatedInteraction.ToString() + " - 100";
                }
            }
            
            if(trait.effect == TRAIT_EFFECT.NEGATIVE) {
                allNegativeTraitNames.Add(trait.name);
            }
        }

        //**F. compute Item handling weight**
        if (!isStarving && !isExhausted) {
            if (isHoldingItem) {
                if (isAtHomeStructure) {
                    int weight = 0;
                    if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                        weight += 0;
                    } else if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                        weight += 10;
                    } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                        weight += 10;
                    } else {
                        weight += 5;
                    }
                    if (weight > 0) {
                        personalActionWeights.AddElement(INTERACTION_TYPE.DROP_ITEM, weight);
                        interactionLog += "\nDROP_ITEM - " + weight;
                    }
                }
            } else {
                if (specificLocation.owner != null && specificLocation.owner == faction && (isAtHomeStructure || specificLocation.HasStructure(STRUCTURE_TYPE.WAREHOUSE))) {
                    int weight = 0;
                    if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                        weight += 0;
                    } else if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                        weight += 20;
                    } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                        weight += 20;
                    } else {
                        weight += 5;
                    }
                    if(weight > 0) {
                        personalActionWeights.AddElement(INTERACTION_TYPE.PICK_ITEM, weight);
                        interactionLog += "\nPICK_ITEM - " + weight;
                    }
                }
            }
        }

        if(specificLocation.id != homeArea.id) {
            //**G. if away from home, compute Return Home weight**
            if(_forcedInteraction == null || _forcedInteraction.interactable.id != specificLocation.id) { //If character has an override that must be done in current location
                int returnHomeWeight = 0;
                if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                    returnHomeWeight += 10;
                } else if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                    returnHomeWeight += 150;
                } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                    returnHomeWeight += 50;
                } else {
                    returnHomeWeight += 10;
                }
                if (isHungry || isTired) {
                    returnHomeWeight *= 2;
                }
                personalActionWeights.AddElement(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, returnHomeWeight);
                interactionLog += "\nMOVE_TO_RETURN_HOME - " + returnHomeWeight;
            }

            //**H. if away from home, Transfer Home weight**
            if (specificLocation.owner != null && specificLocation.owner == faction) {
                int transferHomeWeight = 0;

                Character lover = GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
                if (lover != null && lover.homeArea.id == specificLocation.id) {
                    transferHomeWeight += 100;
                }

                int availableResidentCapacityDifference = specificLocation.GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE.DWELLING) - homeArea.GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE.DWELLING);
                if (availableResidentCapacityDifference > 0) {
                    transferHomeWeight += (availableResidentCapacityDifference * 20);
                }
                if(transferHomeWeight > 0) {
                    personalActionWeights.AddElement(INTERACTION_TYPE.TRANSFER_HOME, transferHomeWeight);
                    interactionLog += "\nTRANSFER_HOME - " + transferHomeWeight;
                }
            }
        } else {
            //**J. if at home, compute weight to simply visit other locations**
            if (!isStarving && !isExhausted) {
                int weight = 0;
                if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                    weight += 0;
                } else if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                    weight += 25;
                } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                    weight += 10;
                } else {
                    weight += 5;
                }
                if(weight > 0) {
                    personalActionWeights.AddElement(INTERACTION_TYPE.MOVE_TO_VISIT, weight);
                    interactionLog += "\nMOVE_TO_VISIT - " + weight;
                }
            }
        }

        //**I. non-busy characters will work**
        if (!isStarving && !isExhausted) {
            List<INTERACTION_TYPE> workActions = RaceManager.Instance.GetNPCInteractionsOfRace(this, INTERACTION_CATEGORY.WORK);
            if (workActions.Count > 0) {
                int weight = 0;
                if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                    weight += 0;
                } else if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                    weight += 100;
                } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                    weight += 100;
                } else {
                    weight += 100;
                }
                if(weight > 0) {
                    INTERACTION_TYPE chosenType = workActions[UnityEngine.Random.Range(0, workActions.Count)];
                    personalActionWeights.AddElement(chosenType, weight);
                    interactionLog += "\nWORK: " + chosenType.ToString() + " - " + weight;
                }
            }
        }

        //**K. characters may also perform actions to empower themselves**
        List<INTERACTION_TYPE> personalEmpowermentInteractions = RaceManager.Instance.GetNPCInteractionsOfRace(this, INTERACTION_CATEGORY.PERSONAL_EMPOWERMENT);
        if (personalEmpowermentInteractions.Count > 0) {
            int weight = 0;
            if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                weight += 0;
            } else if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                weight += 25;
            } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                weight += 25;
            } else {
                weight += 25;
            }
            if(weight > 0) {
                INTERACTION_TYPE chosenType = personalEmpowermentInteractions[UnityEngine.Random.Range(0, personalEmpowermentInteractions.Count)];
                personalActionWeights.AddElement(chosenType, weight);
                interactionLog += "\nPERSONAL EMPOWERMENT: " + chosenType.ToString() + " - " + weight;
            }
        }

        //**L.characters may also perform actions to save themselves**
        if (allNegativeTraitNames.Count > 0) {
            List<INTERACTION_TYPE> allSaveSelfInteractions = RaceManager.Instance.GetNPCInteractionsOfRace(this, INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, allNegativeTraitNames.ToArray(), false);
            if (allSaveSelfInteractions.Count > 0) {
                INTERACTION_TYPE chosenType = allSaveSelfInteractions[UnityEngine.Random.Range(0, allSaveSelfInteractions.Count)];
                personalActionWeights.AddElement(chosenType, 100);
                interactionLog += "\nSAVE SELF: " + chosenType.ToString() + " - 100";
            } else {
                INTERACTION_TYPE useItemOnCharacterType = RaceManager.Instance.CheckNPCInteractionOfRace(this, INTERACTION_TYPE.USE_ITEM_ON_SELF, INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, allNegativeTraitNames.ToArray(), true, this);
                if(useItemOnCharacterType != INTERACTION_TYPE.NONE) {
                    personalActionWeights.AddElement(useItemOnCharacterType, 100);
                    interactionLog += "\nSAVE SELF: USE_ITEM_ON_CHARACTER - 100";
                }
            }
        }


        //**M. compute Do Nothing weight**
        if (!isStarving && !isExhausted) {
            int weight = 150;
            if (!isAtHomeArea) {
                weight = 50;
            }
            personalActionWeights.AddElement(INTERACTION_TYPE.NONE, weight);
            interactionLog += "\nDO_NOTHING - " + weight;
        }


        //---------------------------------------------------------- PICK PERSONAL ACTION ---------------------------------------------------
        interactionLog += "\nCHOSEN PERSONAL ACTION: ";
        if (personalActionWeights.Count > 0) {
            INTERACTION_TYPE chosenPersonalAction = personalActionWeights.PickRandomElementGivenWeights();
            if(chosenPersonalAction == INTERACTION_TYPE.NONE) {
                interactionLog += "DO_NOTHING";
            } else {
                if (chosenPersonalAction == INTERACTION_TYPE.USE_ITEM_ON_SELF) {
                    chosenPersonalAction = INTERACTION_TYPE.USE_ITEM_ON_CHARACTER;
                    targetCharacter = this;
                }
                interactionLog += chosenPersonalAction.ToString();
            }
            if (chosenPersonalAction != INTERACTION_TYPE.NONE) {
                Interaction interaction = InteractionManager.Instance.CreateNewInteraction(chosenPersonalAction, specificLocation);
                //if (interaction.type == INTERACTION_TYPE.USE_ITEM_ON_CHARACTER) {
                //    (interaction as UseItemOnCharacter).SetItemToken(tokenInInventory);
                //    interactionLog += "\nITEM: " + tokenInInventory.name;
                //}
                if (targetCharacter != null) {
                    if(chosenPersonalAction == chosenRelationshipInteraction) {
                        interactionLog += "\nTARGET CHARACTER: " + targetCharacter.name;
                        CharacterRelationshipData relationship = GetCharacterRelationshipData(targetCharacter);
                        if (relationship != null) {
                            interactionLog += "\nResetting encounter multiplier for target: " + targetCharacter.name;
                            relationship.ResetEncounterMultiplier();
                        }
                    }
                    interaction.SetTargetCharacter(targetCharacter, this);
                }
                if (otherCharacter != null) {
                    interactionLog += "\nOTHER CHARACTER: " + otherCharacter.name;
                    interaction.SetOtherCharacter(otherCharacter);
                }
                AddInteraction(interaction);
            }
        } else {
            interactionLog += "\nCAN'T CHOOSE PERSONAL ACTION BECAUSE THERE ARE NO WEIGHTS AVAILABLE!";
        }
        interactionLog += "\n----------------------END CHARACTER PERSONAL ACTIONS-----------------------";
        //Debug.Log(interactionLog);
    }
    private INTERACTION_TYPE CharacterNPCActionTypes(CharacterRelationshipData relationshipData, ref int weight, ref Character targetCharacter, ref Character otherCharacter, ref string interactionLog, ref INTERACTION_CATEGORY category) {
        WeightedDictionary<INTERACTION_CATEGORY> npcActionWeights = new WeightedDictionary<INTERACTION_CATEGORY>();

        if (relationshipData.HasRelationshipTrait(RELATIONSHIP_TRAIT.ENEMY)) {
            //Offense
            npcActionWeights.AddElement(INTERACTION_CATEGORY.OFFENSE, 100);
            interactionLog += "\nOFFENSE: 100";
            //Subterfuge
            npcActionWeights.AddElement(INTERACTION_CATEGORY.SUBTERFUGE, 50);
            interactionLog += "\nSUBTERFUGE: 50";

            //TODO: Sabotage
        } else {
            //TODO: Save
            if(relationshipData.trouble != null && relationshipData.trouble.Count > 0) {
                string[] allTroubleNames = relationshipData.trouble.Select(x => x.name).ToArray();
                List<INTERACTION_TYPE> allSaveInteractionsThatCanBeDone = RaceManager.Instance.GetNPCInteractionsOfRace(this, INTERACTION_CATEGORY.SAVE, 
                    INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, allTroubleNames, true, targetCharacter);
                if (allSaveInteractionsThatCanBeDone != null && allSaveInteractionsThatCanBeDone.Count > 0) {
                    weight += 300;
                    interactionLog += "\nCAN DO SAVE ACTION, EXITING CHARACTER NPC ACTION TYPES";
                    category = INTERACTION_CATEGORY.SAVE;
                    return allSaveInteractionsThatCanBeDone[UnityEngine.Random.Range(0, allSaveInteractionsThatCanBeDone.Count)];
                } else {
                    List<Character> characterChoices = new List<Character>();
                    for (int i = 0; i < specificLocation.charactersAtLocation.Count; i++) {
                        Character characterAtLocation = specificLocation.charactersAtLocation[i];
                        if(id != characterAtLocation.id && faction.id != FactionManager.Instance.neutralFaction.id && characterAtLocation.faction.id != FactionManager.Instance.neutralFaction.id) {
                            if(!HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER) && GetRelationshipTraitWith(characterAtLocation, RELATIONSHIP_TRAIT.ENEMY) == null) {
                                FactionRelationship factionRel = faction.GetRelationshipWith(characterAtLocation.faction);
                                if(factionRel != null && factionRel.relationshipStatus != FACTION_RELATIONSHIP_STATUS.ENEMY && factionRel.relationshipStatus != FACTION_RELATIONSHIP_STATUS.AT_WAR) {
                                    allSaveInteractionsThatCanBeDone = RaceManager.Instance.GetNPCInteractionsOfRace(characterAtLocation, INTERACTION_CATEGORY.SAVE,
                                        INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, allTroubleNames, true, targetCharacter);
                                    if (allSaveInteractionsThatCanBeDone != null && allSaveInteractionsThatCanBeDone.Count > 0) {
                                        characterChoices.Add(characterAtLocation);
                                    }
                                }
                            }
                        }
                    }
                    if(characterChoices.Count > 0) {
                        weight += 200;
                        targetCharacter = characterChoices[UnityEngine.Random.Range(0, characterChoices.Count)];
                        otherCharacter = relationshipData.targetCharacter;
                        interactionLog += "\nCAN DO ASK FOR HELP ACTION, EXITING CHARACTER NPC ACTION TYPES";
                        category = InteractionManager.Instance.GetCategoryAndAlignment(INTERACTION_TYPE.ASK_FOR_HELP, this).categories[0];
                        return INTERACTION_TYPE.ASK_FOR_HELP;
                    }
                }
            }

            //TODO: Assistance

        }

        //Social
        npcActionWeights.AddElement(INTERACTION_CATEGORY.SOCIAL, 100);
        interactionLog += "\nSOCIAL: 100";

        //Romantic
        if (relationshipData.HasRelationshipTrait(RELATIONSHIP_TRAIT.LOVER)) {
            npcActionWeights.AddElement(INTERACTION_CATEGORY.ROMANTIC, 50);
            interactionLog += "\nROMANTIC: 50";
        } else if (relationshipData.HasRelationshipTrait(RELATIONSHIP_TRAIT.PARAMOUR)) {
            npcActionWeights.AddElement(INTERACTION_CATEGORY.ROMANTIC, 80);
            interactionLog += "\nROMANTIC: 80";
        }

        //TODO: Other
        //npcActionWeights.AddElement(INTERACTION_CATEGORY.OTHER, 50);

        INTERACTION_TYPE chosenType = INTERACTION_TYPE.NONE;
        while(chosenType == INTERACTION_TYPE.NONE && npcActionWeights.Count > 0) {
            category = npcActionWeights.PickRandomElementGivenWeights();
            List<INTERACTION_TYPE> allInteractionsThatCanBeDone = RaceManager.Instance.GetNPCInteractionsOfRace(this, category, targetCharacter);
            if(allInteractionsThatCanBeDone.Count > 0) {
                interactionLog += "\nCHOSEN CATEGORY: " + category.ToString();
                chosenType = allInteractionsThatCanBeDone[UnityEngine.Random.Range(0, allInteractionsThatCanBeDone.Count)];
            } else {
                npcActionWeights.RemoveElement(category);
            }
        }
        return chosenType;
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
    public void ClaimReward(Reward reward) {
        switch (reward.rewardType) {
            case REWARD.LEVEL:
            LevelUp(reward.amount);
            break;
            case REWARD.SUPPLY:
            if(minion != null) {
                PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, reward.amount);
            } else {
                homeArea.AdjustSuppliesInBank(reward.amount);
            }
            break;
            default:
            break;
        }
    }
    public void SetPlannedAction(Interaction interaction) {
        plannedInteraction = interaction;
    }
    public void OnInteractionEnded(Interaction interaction) {
        if (plannedInteraction != null && plannedInteraction == interaction) {
            SetPlannedAction(null);
        }
    }
    public void OnForcedInteractionSubmitted(Interaction interaction) {
        if (this.gridTileLocation == null) {
            return;
        }
        if (interaction.targetStructure == null) {
            Debug.LogWarning(this.name + "'s target structure from " + interaction.name + " is null! Not drawing inside structure line");
        }
        if (interaction.targetStructure != null) {
            if (interaction.targetStructure.location.id == this.specificLocation.id) {
                LocationGridTile targetTile = interaction.targetGridLocation;
                if (targetTile == null) {
                    targetTile =  interaction.targetStructure.unoccupiedTiles[UnityEngine.Random.Range(0, interaction.targetStructure.unoccupiedTiles.Count)];
                }
                if (targetTile == gridTileLocation) {
                    //already at location, do not draw line 
                    return;
                }
                this.specificLocation.areaMap.DrawLine(this.gridTileLocation, targetTile, this);
                Debug.Log(this.name + " is drawing an inside structure travel line at " + this.specificLocation.name);
            } else {
                this.specificLocation.areaMap.DrawLineToExit(this.gridTileLocation, this);
            }
        } else if (interaction.targetArea != null && interaction.targetArea.id != this.specificLocation.id) {
            this.specificLocation.areaMap.DrawLineToExit(this.gridTileLocation, this);
        }
    }
    public void AssignQueueActionsToCharacter(Character targetCharacter) {
        INTERACTION_TYPE chosenInteractionType = INTERACTION_TYPE.NONE; //TODO
        //_goapInteractions.Clear();

    }
    public void AssignGoapInteractionsRecursively(INTERACTION_TYPE type, Character targetCharacter) {
        InteractionAttributes attributes = InteractionManager.Instance.GetCategoryAndAlignment(type, this);
        //use came_from to track down the path
        if(attributes.preconditions != null && attributes.preconditions.Length > 0) {

        } else {
            //_goapInteractions.Add(type);
        }
    }
    public bool HasPlanWithType(INTERACTION_TYPE type) {
        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan currPlan = allGoapPlans[i];
            if (currPlan.endNode.action != null && currPlan.endNode.action.goapType == type) {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// This should only be used for plans that come/constructed from the outside.
    /// </summary>
    /// <param name="plan">Plan to be added</param>
    public void AddPlanFromOutside(GoapPlan plan) {
        if (!allGoapPlans.Contains(plan)) {
            allGoapPlans.Add(plan);
        }
    }
    #endregion

    #region Token Inventory
    public bool ObtainTokenFrom(Character target, SpecialToken token, bool changeCharacterOwnership = true) {
        if (target.UnobtainToken(token)) {
            ObtainToken(token, changeCharacterOwnership);
            return true;
        }
        return false;
    }
    public bool ObtainToken(SpecialToken token, bool changeCharacterOwnership = true) {
        if (AddToken(token)) {
            token.SetOwner(this.faction);
            token.OnObtainToken(this);
            if (changeCharacterOwnership) {
                token.SetCharacterOwner(this);
            }
            return true;
        }
        return false;
        //token.AdjustQuantity(-1);
    }
    public bool UnobtainToken(SpecialToken token) {
        if (RemoveToken(token)) {
            token.OnUnobtainToken(this);
            return true;
        }
        return false;
    }
    public bool ConsumeToken(SpecialToken token) {
        if (RemoveToken(token)) {
            token.OnConsumeToken(this);
            return true;
        }
        return false;
    }
    private bool AddToken(SpecialToken token) {
        if (!items.Contains(token)) {
            items.Add(token);
            return true;
        }
        return false;
    }
    private bool RemoveToken(SpecialToken token) {
        return items.Remove(token);
    }
    public void DropToken(SpecialToken token, Area location, LocationStructure structure) {
        if (UnobtainToken(token)) {
            location.AddSpecialTokenToLocation(token, structure);
            if (structure != homeStructure) {
                //if this character drops this at a structure that is not his/her home structure, set the owner of the item to null
                token.SetCharacterOwner(null);
            }
        }
    }
    public void DropAllTokens(Area location, LocationStructure structure, bool removeFactionOwner = false) {
        while(isHoldingItem) {
            SpecialToken token = items[0];
            if (UnobtainToken(token)) {
                if (removeFactionOwner) {
                    token.SetOwner(null);
                }
                location.AddSpecialTokenToLocation(token, structure);
                if (structure != homeStructure) {
                    //if this character drops this at a structure that is not his/her home structure, set the owner of the item to null
                    token.SetCharacterOwner(null);
                }
            }
        }
    }
    public void PickUpToken(SpecialToken token, bool changeCharacterOwnership = true) {
        if (ObtainToken(token, changeCharacterOwnership)) {
            token.gridTileLocation.structure.location.RemoveSpecialTokenFromLocation(token);
        }
    }
    //public void PickUpRandomToken(Area location) {
    //    WeightedDictionary<SpecialToken> pickWeights = new WeightedDictionary<SpecialToken>();
    //    for (int i = 0; i < location.possibleSpecialTokenSpawns.Count; i++) {
    //        SpecialToken token = location.possibleSpecialTokenSpawns[i];
    //        if (token.npcAssociatedInteractionType != INTERACTION_TYPE.USE_ITEM_ON_SELF) {
    //            pickWeights.AddElement(token, 60);
    //        } else if (token.CanBeUsedBy(this)) {
    //            pickWeights.AddElement(token, 100);
    //        }
    //    }
    //    if (pickWeights.Count > 0) {
    //        SpecialToken chosenToken = pickWeights.PickRandomElementGivenWeights();
    //        PickUpToken(chosenToken);
    //    }
    //}
    public void DestroyToken(SpecialToken token) {
        token.gridTileLocation.structure.location.RemoveSpecialTokenFromLocation(token);
    }
    private void UpdateTokenOwner() {
        for (int i = 0; i < items.Count; i++) {
            SpecialToken token = items[i];
            token.SetOwner(this.faction);
        }
    }
    public SpecialToken GetToken(SpecialToken token) {
        for (int i = 0; i < items.Count; i++) {
            if(items[i] == token) {
                return items[i];
            }
        }
        return null;
    }
    public SpecialToken GetToken(string tokenName) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].tokenName.ToLower() == tokenName.ToLower()) {
                return items[i];
            }
        }
        return null;
    }
    #endregion

    #region Needs
    private void DecreaseNeeds() {
        if(race == RACE.SKELETON) {
            return;
        }
        if(_doNotGetHungry <= 0) {
            AdjustFullness(-10);
        }
        if(_doNotGetTired <= 0) {
            AdjustTiredness(-10);
        }
        if (_doNotGetLonely <= 0) {
            AdjustHappiness(-7);
        }
    }
    public string GetNeedsSummary() {
        string summary = "Fullness: " + fullness.ToString() + "/" + FULLNESS_DEFAULT.ToString();
        summary += "\nTiredness: " + tiredness + "/" + TIREDNESS_DEFAULT.ToString();
        summary += "\nHappiness: " + happiness + "/" + HAPPINESS_DEFAULT.ToString();
        return summary;
    }
    #endregion

    #region Tiredness
    public void ResetTirednessMeter() {
        tiredness = TIREDNESS_DEFAULT;
        RemoveTrait("Tired");
        RemoveTrait("Exhausted");
    }
    public void AdjustTiredness(int adjustment) {
        tiredness += adjustment;
        tiredness = Mathf.Clamp(tiredness, 0, TIREDNESS_DEFAULT);
        if (tiredness == 0) {
            Death("exhaustion");
        } else if (tiredness <= TIREDNESS_THRESHOLD_2) {
            RemoveTrait("Tired");
            AddTrait("Exhausted");
            //PlanTirednessRecoveryActions();
        } else if (tiredness <= TIREDNESS_THRESHOLD_1) {
            AddTrait("Tired");
            RemoveTrait("Exhausted");
            //PlanTirednessRecoveryActions();
        } else {
            //tiredness is higher than both thresholds
            RemoveTrait("Tired");
            RemoveTrait("Exhausted");
        }
    }
    public void DecreaseTirednessMeter() { //this is used for when tiredness is only decreased by 1 (I did this for optimization, so as not to check for traits everytime)
        tiredness -= 1;
        tiredness = Mathf.Clamp(tiredness, 0, TIREDNESS_DEFAULT);
        if (tiredness == 0) {
            Death("exhaustion");
        } else if (tiredness <= TIREDNESS_THRESHOLD_2) {
            if (tiredness == TIREDNESS_THRESHOLD_2) {
                RemoveTrait("Tired");
                AddTrait("Exhausted");
            }
            //PlanTirednessRecoveryActions();
        } else if (tiredness <= TIREDNESS_THRESHOLD_1) {
            if (tiredness == TIREDNESS_THRESHOLD_1) {
                AddTrait("Tired");
            }
            //PlanTirednessRecoveryActions();
        }
    }
    public void SetTiredness(int amount) {
        tiredness = amount;
        tiredness = Mathf.Clamp(tiredness, 0, TIREDNESS_DEFAULT);
        if (tiredness == 0) {
            Death("exhaustion");
        } else if (tiredness <= TIREDNESS_THRESHOLD_2) {
            RemoveTrait("Tired");
            AddTrait("Exhausted");
            //PlanTirednessRecoveryActions();
        } else if (tiredness <= TIREDNESS_THRESHOLD_1) {
            AddTrait("Tired");
            RemoveTrait("Exhausted");
            //PlanTirednessRecoveryActions();
        } else {
            //tiredness is higher than both thresholds
            RemoveTrait("Tired");
            RemoveTrait("Exhausted");
        }
    }
    #endregion

    #region Fullness
    public void ResetFullnessMeter() {
        fullness = FULLNESS_DEFAULT;
        RemoveTrait("Hungry");
        RemoveTrait("Starving");
    }
    public void AdjustFullness(int adjustment) {
        fullness += adjustment;
        fullness = Mathf.Clamp(fullness, 0, FULLNESS_DEFAULT);
        if (fullness == 0) {
            Death("starvation");
        } else if (fullness <= FULLNESS_THRESHOLD_2) {
            RemoveTrait("Hungry");
            AddTrait("Starving");
            //PlanFullnessRecoveryActions();
        } else if (fullness <= FULLNESS_THRESHOLD_1) {
            RemoveTrait("Starving");
            AddTrait("Hungry");
            //PlanFullnessRecoveryActions();
        } else {
            //fullness is higher than both thresholds
            RemoveTrait("Hungry");
            RemoveTrait("Starving");
        }
    }
    public void DecreaseFullnessMeter() { //this is used for when fullness is only decreased by 1 (I did this for optimization, so as not to check for traits everytime)
        fullness -= 1;
        fullness = Mathf.Clamp(fullness, 0, FULLNESS_DEFAULT);
        if (fullness == 0) {
            Death("starvation");
        } else if (fullness <= FULLNESS_THRESHOLD_2) {
            if (fullness == FULLNESS_THRESHOLD_2) {
                RemoveTrait("Hungry");
                AddTrait("Starving");
            }
            //PlanFullnessRecoveryActions();
        } else if (fullness <= FULLNESS_THRESHOLD_1) {
            if (fullness == FULLNESS_THRESHOLD_1) {
                AddTrait("Hungry");
            }
            //PlanFullnessRecoveryActions();
        }
    }
    public void SetFullness(int amount) {
        fullness = amount;
        fullness = Mathf.Clamp(fullness, 0, FULLNESS_DEFAULT);
        if (fullness == 0) {
            Death("starvation");
        } else if (fullness <= FULLNESS_THRESHOLD_2) {
            RemoveTrait("Hungry");
            AddTrait("Starving");
            //PlanFullnessRecoveryActions();
        } else if (fullness <= FULLNESS_THRESHOLD_1) {
            RemoveTrait("Starving");
            AddTrait("Hungry");
            //PlanFullnessRecoveryActions();
        } else {
            //fullness is higher than both thresholds
            RemoveTrait("Hungry");
            RemoveTrait("Starving");
        }
    }
    #endregion

    #region Happiness
    public void ResetHappinessMeter() {
        happiness = HAPPINESS_DEFAULT;
        RemoveTrait("Lonely");
        RemoveTrait("Forlorn");
    }
    public void AdjustHappiness(int adjustment) {
        happiness += adjustment;
        happiness = Mathf.Clamp(happiness, 0, HAPPINESS_DEFAULT);
        if (happiness <= HAPPINESS_THRESHOLD_2) {
            RemoveTrait("Lonely");
            AddTrait("Forlorn");
            //PlanHappinessRecoveryActions();
        } else if (happiness <= HAPPINESS_THRESHOLD_1) {
            AddTrait("Lonely");
            RemoveTrait("Forlorn");
            //PlanHappinessRecoveryActions();
        } else {
            RemoveTrait("Lonely");
            RemoveTrait("Forlorn");
        }
    }
    public void SetHappiness(int amount) {
        happiness = amount;
        happiness = Mathf.Clamp(happiness, 0, HAPPINESS_DEFAULT);
        if (happiness <= HAPPINESS_THRESHOLD_2) {
            RemoveTrait("Lonely");
            AddTrait("Forlorn");
            //PlanHappinessRecoveryActions();
        } else if (happiness <= HAPPINESS_THRESHOLD_1) {
            AddTrait("Lonely");
            RemoveTrait("Forlorn");
            //PlanHappinessRecoveryActions();
        } else {
            RemoveTrait("Lonely");
            RemoveTrait("Forlorn");
        }
    }
    #endregion

    #region Share Intel
    public List<string> ShareIntel(Intel intel) {
        List<string> dialogReactions = new List<string>();
        if (intel is TileObjectIntel) {
            TileObjectIntel toi = intel as TileObjectIntel;
            if (GetAwareness(toi.obj) != null) {
                dialogReactions.Add("I already know about that.");
            } else {
                AddAwareness(toi.obj);
                dialogReactions.Add("There is an " + toi.obj.name + " in " + toi.knownLocation.structure.location.name + "? Thanks for letting me know.");
            }
        } else if (intel is EventIntel) {
            EventIntel ei = intel as EventIntel;
            if (ei.action.endedAtState != null && ei.action.endedAtState.shareIntelReaction != null) {
                dialogReactions.AddRange(ei.action.endedAtState.shareIntelReaction.Invoke(this, ei));
            }            
            //Dictionary<ActionEffectReaction, GoapEffect> reactions = new Dictionary<ActionEffectReaction, GoapEffect>();
            //for (int i = 0; i < ei.action.actualEffects.Count; i++) {
            //    GoapEffect currEffect = ei.action.actualEffects[i];
            //    if (ActionEffectReactionDB.eventIntelReactions.ContainsKey(currEffect)) {
            //        reactions.Add(ActionEffectReactionDB.eventIntelReactions[currEffect], currEffect);
            //    }
            //}
            //foreach (KeyValuePair<ActionEffectReaction, GoapEffect> keyValuePair in reactions) {
            //    dialogReactions.Add(keyValuePair.Key.GetReactionFrom(this, intel, keyValuePair.Value));
            //}
        }
        PlayerManager.Instance.player.RemoveIntel(intel);
        return dialogReactions;
        //if (relationships.ContainsKey(intel.actor)) {
        //    if (!intel.isCompleted) {
        //        relationships[intel.actor].SetPlannedActionIntel(intel);
        //    } else {
        //        Debug.Log(GameManager.Instance.TodayLogString() + "The intel given to " + this.name + " regarding " + intel.actor.name + " has already been completed, not setting planned action...");
        //    }
        //    relationships[intel.actor].OnIntelGivenToCharacter(intel);
        //    PlayerManager.Instance.player.RemoveIntel(intel);
        //} else {
        //    Debug.Log(GameManager.Instance.TodayLogString() + this.name + " does not have a relationship with " + intel.actor.name + ". He/she doesn't care about any intel you give that is about " + intel.actor.name);
        //}
        //if (intel.target is Character) {
        //    Character target = intel.target as Character;
        //    if (relationships.ContainsKey(target)) {
        //        relationships[target].OnIntelGivenToCharacter(intel);
        //        PlayerManager.Instance.player.RemoveIntel(intel);
        //    }
        //}

    }
    #endregion

    #region Awareness
    public IAwareness AddAwareness(IPointOfInterest pointOfInterest) {
        IAwareness iawareness = GetAwareness(pointOfInterest);
        if (iawareness == null) {
            iawareness = CreateNewAwareness(pointOfInterest);
            if(iawareness != null) {
                if (!awareness.ContainsKey(pointOfInterest.poiType)) {
                    awareness.Add(pointOfInterest.poiType, new List<IAwareness>());
                }
                awareness[pointOfInterest.poiType].Add(iawareness);
                iawareness.OnAddAwareness(this);
            }
        } else {
            if (pointOfInterest.gridTileLocation != null) {
                //if already has awareness for that poi, just update it's known location. 
                //Except if it's null, because if it's null, it ususally means the poi is travelling 
                //and setting it's location to null should be ignored to prevent unexpected behaviour.
                iawareness.SetKnownGridLocation(pointOfInterest.gridTileLocation);
            }
            
        }
        return iawareness;
    }
    public void RemoveAwareness(IPointOfInterest pointOfInterest) {
        if (awareness.ContainsKey(pointOfInterest.poiType)) {
            List<IAwareness> awarenesses = awareness[pointOfInterest.poiType];
            for (int i = 0; i < awarenesses.Count; i++) {
                IAwareness iawareness = awarenesses[i];
                if (iawareness.poi == pointOfInterest) {
                    awarenesses.RemoveAt(i);
                    iawareness.OnRemoveAwareness(this);
                    break;
                }
            }
        }
    }
    public IAwareness GetAwareness(IPointOfInterest poi) {
        if (awareness.ContainsKey(poi.poiType)) {
            List<IAwareness> awarenesses = awareness[poi.poiType];
            for (int i = 0; i < awarenesses.Count; i++) {
                IAwareness iawareness = awarenesses[i];
                if(iawareness.poi == poi) {
                    return iawareness;
                }
            }
            return null;
        }
        return null;
    }
    private IAwareness CreateNewAwareness(IPointOfInterest poi) {
        if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            return new CharacterAwareness(poi as Character);
        }else if (poi.poiType == POINT_OF_INTEREST_TYPE.ITEM) {
            return new ItemAwareness(poi as SpecialToken);
        }else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            return new TileObjectAwareness(poi);
        }//TODO: Structure Awareness
        return null;
    }
    public void ScanForAwareness() {
        List<LocationGridTile> tilesInRadius = specificLocation.areaMap.GetTilesInRadius(gridTileLocation, 3);
        for (int i = 0; i < tilesInRadius.Count; i++) {
            if(tilesInRadius[i].objHere != null) {
                AddAwareness(tilesInRadius[i].objHere);
            }
            for (int j = 0; j < tilesInRadius[i].charactersHere.Count; j++) {
                AddAwareness(tilesInRadius[i].charactersHere[j]);
            }
        }
    }
    public void AddInitialAwareness() {
        if(faction == FactionManager.Instance.neutralFaction) {
            foreach (List<LocationStructure> structures in specificLocation.structures.Values) {
                for (int i = 0; i < structures.Count; i++) {
                    for (int j = 0; j < structures[i].pointsOfInterest.Count; j++) {
                        IPointOfInterest poi = structures[i].pointsOfInterest[j];
                        if (poi != this) {
                            AddAwareness(poi);
                        }
                    }
                }
            }
        } else {
            foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in specificLocation.structures) {
                for (int i = 0; i < keyValuePair.Value.Count; i++) {
                    LocationStructure structure = keyValuePair.Value[i];
                    for (int j = 0; j < structure.pointsOfInterest.Count; j++) {
                        IPointOfInterest poi = structure.pointsOfInterest[j];
                        if (poi != this && !(poi is Tree)) {
                            AddAwareness(poi);
                        }
                    }
                }
            }
            
            //foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in specificLocation.structures) {
            //    for (int i = 0; i < keyValuePair.Value.Count; i++) {
            //        LocationStructure structure = keyValuePair.Value[i];
            //        for (int j = 0; j < structure.pointsOfInterest.Count; j++) {
            //            IPointOfInterest poi = structure.pointsOfInterest[j];
            //            if (poi != this) {
            //                AddAwareness(poi);
            //            }
            //        }
            //    }
            //}
        }
    }
    public void LogAwarenessList() {
        string log = "--------------AWARENESS LIST OF " + name + "-----------------";
        foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IAwareness>> kvp in awareness) {
            log += "\n" + kvp.Key.ToString() + ": ";
            for (int i = 0; i < kvp.Value.Count; i++) {
                if(i > 0) {
                    log += ", ";
                }
                log += kvp.Value[i].poi.name;
            }
        }
        Debug.Log(log);
    }
    public void CopyAwareness(Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>> newAwareness) {
        this.awareness = newAwareness;
    }
    #endregion

    #region Point Of Interest
    public List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if(poiGoapActions != null && poiGoapActions.Count > 0 && state == POI_STATE.ACTIVE && !isDead) {
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                INTERACTION_TYPE currType = poiGoapActions[i];
                if (actorAllowedInteractions.Contains(currType)){
                    if(currType == INTERACTION_TYPE.CRAFT_ITEM) {
                        Craftsman craftsman = GetTrait("Craftsman") as Craftsman;
                        for (int j = 0; j < craftsman.craftedItemNames.Length; j++) {
                            CraftItemGoap goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this, false) as CraftItemGoap;
                            goapAction.SetCraftedItem(craftsman.craftedItemNames[j]);
                            goapAction.Initialize();
                            if (goapAction.CanSatisfyRequirements()) {
                                usableActions.Add(goapAction);
                            }
                        }
                    } else {
                        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                        if (goapAction == null) {
                            throw new Exception("Goap action " + currType.ToString() + " is null!");
                        }
                        if (goapAction.CanSatisfyRequirements()) {
                            usableActions.Add(goapAction);
                        }
                    } 
                }
            }
            return usableActions;
        }
        return null;
    }
    public void SetPOIState(POI_STATE state) {
        _state = state;
    }
    #endregion

    #region Goap
    private void ConstructInitialGoapAdvertisementActions() {
        poiGoapActions = new List<INTERACTION_TYPE>();
        poiGoapActions.Add(INTERACTION_TYPE.CARRY_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.ASSAULT_ACTION_NPC);
        poiGoapActions.Add(INTERACTION_TYPE.DROP_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.ABDUCT_ACTION);
        poiGoapActions.Add(INTERACTION_TYPE.STROLL);
        poiGoapActions.Add(INTERACTION_TYPE.DAYDREAM);
        poiGoapActions.Add(INTERACTION_TYPE.SLEEP_OUTSIDE);
        poiGoapActions.Add(INTERACTION_TYPE.PRAY);
        poiGoapActions.Add(INTERACTION_TYPE.EXPLORE);
        poiGoapActions.Add(INTERACTION_TYPE.PATROL);
        poiGoapActions.Add(INTERACTION_TYPE.TRAVEL);
        poiGoapActions.Add(INTERACTION_TYPE.RETURN_HOME_LOCATION);
        poiGoapActions.Add(INTERACTION_TYPE.HUNT_ACTION);
        poiGoapActions.Add(INTERACTION_TYPE.PLAY);
        poiGoapActions.Add(INTERACTION_TYPE.REPORT_CRIME);
        poiGoapActions.Add(INTERACTION_TYPE.STEAL_CHARACTER);
    }
    public void StartGOAP(GoapEffect goal, IPointOfInterest target, GOAP_CATEGORY category, bool isPriority = false, List<Character> otherCharactePOIs = null, bool isPersonalPlan = true, GoapPlanJob job = null) {
        List<CharacterAwareness> characterTargetsAwareness = new List<CharacterAwareness>();
        if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            CharacterAwareness characterAwareness = AddAwareness(target) as CharacterAwareness;
            if (characterAwareness != null) {
                characterTargetsAwareness.Add(characterAwareness);
            }
        }

        if (otherCharactePOIs != null) {
            for (int i = 0; i < otherCharactePOIs.Count; i++) {
                CharacterAwareness characterAwareness = AddAwareness(otherCharactePOIs[i]) as CharacterAwareness;
                if (characterAwareness != null) {
                    characterTargetsAwareness.Add(characterAwareness);
                }
            }
        }
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        _numOfWaitingForGoapThread ++;
        //Debug.LogWarning(name + " sent a plan to other thread(" + _numOfWaitingForGoapThread + ")");
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, target, goal, category, isPriority, characterTargetsAwareness, isPersonalPlan, job));
    }
    public void StartGOAP(GoapAction goal, IPointOfInterest target, GOAP_CATEGORY category, bool isPriority = false, List<Character> otherCharactePOIs = null, bool isPersonalPlan = true, GoapPlanJob job = null) {
        List<CharacterAwareness> characterTargetsAwareness = new List<CharacterAwareness>();
        if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            CharacterAwareness characterAwareness = AddAwareness(target) as CharacterAwareness;
            if (characterAwareness != null) {
                characterTargetsAwareness.Add(characterAwareness);
            }
        }

        if (otherCharactePOIs != null) {
            for (int i = 0; i < otherCharactePOIs.Count; i++) {
                CharacterAwareness characterAwareness = AddAwareness(otherCharactePOIs[i]) as CharacterAwareness;
                if (characterAwareness != null) {
                    characterTargetsAwareness.Add(characterAwareness);
                }
            }
        }
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        _numOfWaitingForGoapThread++;
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, target, goal, category, isPriority, characterTargetsAwareness, isPersonalPlan, job));
    }
    public void StartGOAP(INTERACTION_TYPE goalType, IPointOfInterest target, GOAP_CATEGORY category, bool isPriority = false, List<Character> otherCharactePOIs = null, bool isPersonalPlan = true, GoapPlanJob job = null, object[] otherData = null) {
        List<CharacterAwareness> characterTargetsAwareness = new List<CharacterAwareness>();
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        _numOfWaitingForGoapThread++;
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, goalType, category, isPriority, characterTargetsAwareness, isPersonalPlan, job, otherData));
    }
    public void RecalculatePlan(GoapPlan currentPlan) {
        currentPlan.SetIsBeingRecalculated(true);
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, currentPlan));
    }
    public void ReceivePlanFromGoapThread(GoapThread goapThread) {
        //string log = name + " received a plan from other thread(" + _numOfWaitingForGoapThread + ")";
        //if(goapThread.recalculationPlan == null) {
        //    log += " - thread has no recalculation plan";
        //}
        //Debug.LogWarning(log);
        if (isDead) {
            return;
        }
        if (goapThread.recalculationPlan != null && goapThread.recalculationPlan.isEnd) {
            return;
        }
        if (goapThread.recalculationPlan == null) {
            _numOfWaitingForGoapThread--;
        }
        PrintLogIfActive(goapThread.log);
        if (goapThread.createdPlan != null) {
            if (goapThread.recalculationPlan == null) {
                if (goapThread.isPriority) {
                    allGoapPlans.Insert(0, goapThread.createdPlan);
                } else {
                    allGoapPlans.Add(goapThread.createdPlan);
                }
                if (goapThread.job != null) {
                    goapThread.job.SetAssignedPlan(goapThread.createdPlan);
                }
                PlanGoapActions();
            } else {
                //Receive plan recalculation
                goapThread.createdPlan.SetIsBeingRecalculated(false);
            }
            //if (allGoapPlans.Count == 1) {
            //    //Start this plan immediately since this is the only plan
            //    SchedulePerformGoapPlans();
            //} else {
            //    StartDailyGoapPlanGeneration();
            //}
        } else {
            if (goapThread.recalculationPlan != null) {
                //This means that the recalculation has failed
                DropPlan(goapThread.recalculationPlan);
            } else {
                if (goapThread.job != null) {
                    goapThread.job.SetAssignedCharacter(null);
                }
                if (allGoapPlans.Count <= 0) {
                    //StartDailyGoapPlanGeneration();
                    PlanGoapActions();
                }
            }
        }
    }
    public bool IsPOIInCharacterAwarenessList(IPointOfInterest poi, List<CharacterAwareness> awarenesses) {
        for (int i = 0; i < awarenesses.Count; i++) {
            if (awarenesses[i].poi == poi) {
                return true;
            }
        }
        return false;
    }
    private void SchedulePerformGoapPlans() {
        if (!IsInOwnParty() || isDefender || ownParty.icon.isTravelling || _doNotDisturb > 0 || _job == null || isWaitingForInteraction > 0) {
            StartDailyGoapPlanGeneration();
            return;
        }
        if (allGoapPlans.Count > 0) {
            StopDailyGoapPlanGeneration();
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(1);
            SchedulingManager.Instance.AddEntry(dueDate, PerformGoapPlans);
        }
    }
    public void PerformGoapPlans() {
        string log = GameManager.Instance.TodayLogString() + "PERFORMING GOAP PLANS OF " + name;
        if (currentAction != null) {
            log += "\n" + name + " can't perform another action because he/she is currently performing " + currentAction.goapName;
            PrintLogIfActive(log);
            return;
        }
        List<INTERACTION_TYPE> actorAllowedActions = RaceManager.Instance.GetNPCInteractionsOfRace(this);
        bool willGoIdleState = true;
        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan plan = allGoapPlans[i];
            log += "\n" + plan.currentNode.action.goapName;
            if (plan.isBeingRecalculated) {
                log += "\n - Plan is currently being recalculated, skipping...";
                continue; //skip plan
            }
            if (actorAllowedActions.Contains(plan.currentNode.action.goapType) && plan.currentNode.action.CanSatisfyRequirements() && plan.currentNode.action.targetTile != null) {
                //if (plan.isBeingRecalculated) {
                //    log += "\n - Plan is currently being recalculated, skipping...";
                //    continue; //skip plan
                //}
                if (IsPlanCancelledDueToInjury(plan.currentNode.action)) {
                    i--;
                    continue;
                }
                if (plan.currentNode.action.IsHalted()) {
                    log += "\n - Action " + plan.currentNode.action.goapName + " is waiting, skipping...";
                    continue;
                }
                bool preconditionsSatisfied = plan.currentNode.action.CanSatisfyAllPreconditions();
                if (!preconditionsSatisfied) {
                    log += "\n - Action's preconditions are not all satisfied, trying to recalculate plan...";
                    if (plan.doNotRecalculate) {
                        log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
                        PrintLogIfActive(log);
                        if (allGoapPlans.Count == 1) {
                            DropPlan(plan);
                            willGoIdleState = false;
                            break;
                        } else {
                            DropPlan(plan);
                            i--;
                        }
                    } else {
                        PrintLogIfActive(log);
                        RecalculatePlan(plan);
                        willGoIdleState = false;
                    }
                } else {
                    //Do not perform action if the target character is still in another character's party, this means that he/she is probably being abducted
                    //Wait for the character to be in its own party before doing the action
                    if(plan.currentNode.action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                        Character targetCharacter = plan.currentNode.action.poiTarget as Character;
                        if (!targetCharacter.IsInOwnParty() && targetCharacter.currentParty != _ownParty) {
                            log += "\n - " + targetCharacter.name + " is not in its own party, waiting and skipping...";
                            PrintLogIfActive(log);
                            continue;
                        }
                    }
                    log += "\n - Action's preconditions are all satisfied, doing action...";
                    PrintLogIfActive(log);
                    Messenger.Broadcast(Signals.CHARACTER_WILL_DO_PLAN, this, plan);
                    plan.currentNode.action.DoAction(plan);
                    willGoIdleState = false;
                    break;
                }
            } else {
                log += "\n - Action did not meet current requirements and allowed actions, dropping plan...";
                PrintLogIfActive(log);
                if (allGoapPlans.Count == 1) {
                    DropPlan(plan);
                    willGoIdleState = false;
                    break;
                } else {
                    DropPlan(plan);
                    i--;
                }
            }
        }
        if (willGoIdleState) {
            log += "\n - Character will go into idle state";
            PrintLogIfActive(log);
            IdlePlans();
        }
    }
    private void WillAboutToDoAction(GoapAction action) {
        string log = GameManager.Instance.TodayLogString() + "PERFORMING GOAP PLANS OF " + name;
        List<INTERACTION_TYPE> actorAllowedActions = RaceManager.Instance.GetNPCInteractionsOfRace(this);
        bool willGoIdleState = true;
        if (action.parentPlan.isBeingRecalculated) {
            log += "\n - Plan is currently being recalculated, skipping...";
            return; //skip plan
        }
        if (actorAllowedActions.Contains(action.goapType) && action.CanSatisfyRequirements() && action.targetTile != null) {
            if (IsPlanCancelledDueToInjury(action)) {
                return;
            }
            if (action.IsHalted()) {
                log += "\n - Action " + action.goapName + " is waiting, skipping...";
                return;
            }
            bool preconditionsSatisfied = action.CanSatisfyAllPreconditions();
            if (!preconditionsSatisfied) {
                log += "\n - Action's preconditions are not all satisfied, trying to recalculate plan...";
                if (action.parentPlan.doNotRecalculate) {
                    log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
                    PrintLogIfActive(log);
                    DropPlan(action.parentPlan);
                } else {
                    PrintLogIfActive(log);
                    RecalculatePlan(action.parentPlan);
                }
                willGoIdleState = false;
            } else {
                log += "\n - Action's preconditions are all satisfied, doing action...";
                PrintLogIfActive(log);
                Messenger.Broadcast(Signals.CHARACTER_WILL_DO_PLAN, this, action.parentPlan);
                action.DoAction(action.parentPlan);
                willGoIdleState = false;
            }
        } else {
            log += "\n - Action did not meet current requirements and allowed actions, dropping plan...";
            PrintLogIfActive(log);
            DropPlan(action.parentPlan);
            willGoIdleState = false;
        }
        if (willGoIdleState) {
            log += "\n - Character will go into idle state";
            PrintLogIfActive(log);
            IdlePlans();
        }
    }
    private bool IsPlanCancelledDueToInjury(GoapAction action) {
        if(action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character target = action.poiTarget as Character;
            if(IsHostileWith(target) && GetTrait("Injured") != null) {
                AdjustIsWaitingForInteraction(1);
                DropPlan(action.parentPlan);
                AdjustIsWaitingForInteraction(-1);

                Log addLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "plan_cancelled_injury");
                addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                addLog.AddLogToInvolvedObjects();
                PlayerManager.Instance.player.ShowNotificationFrom(this, addLog);
                return true;
            }
        }
        return false;
    }
    public void PerformGoapAction(GoapPlan plan) {
        string log = string.Empty;
        if (currentAction == null) {
            log = GameManager.Instance.TodayLogString() + name + " cancelled current action!";
            PrintLogIfActive(log);
            if (!DropPlan(plan)) {
                PlanGoapActions();
            }
            //StartDailyGoapPlanGeneration();
            return;
        }
        log = GameManager.Instance.TodayLogString() + name + " is performing goap action: " + currentAction.goapName;
        FaceTarget(currentAction.poiTarget);
        if (currentAction.isStopped) {
            log += "\n Action is stopped! Dropping plan...";
            PrintLogIfActive(log);
            SetCurrentAction(null);
            if (!DropPlan(plan)) {
                PlanGoapActions();
            }
        } else {
            if (currentAction.IsHalted()) {
                log += "\n Action is waiting! Not doing action...";
                //if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //    Character targetCharacter = currentAction.poiTarget as Character;
                //    targetCharacter.AdjustIsWaitingForInteraction(-1);
                //}
                SetCurrentAction(null);
                return;
            }
            if (currentAction.CanSatisfyRequirements() && currentAction.CanSatisfyAllPreconditions()) {
                log += "\nAction satisfies all requirements and preconditions, proceeding to perform actual action: " + currentAction.goapName + " to " + currentAction.poiTarget.name + " at " + currentAction.poiTarget.gridTileLocation?.ToString() ?? "No Tile Location";
                PrintLogIfActive(log);
                currentAction.PerformActualAction();
            } else {
                log += "\nAction did not meet all requirements and preconditions. Will try to recalculate plan...";
                //if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //    Character targetCharacter = currentAction.poiTarget as Character;
                //    targetCharacter.AdjustIsWaitingForInteraction(-1);
                //}
                SetCurrentAction(null);
                if (plan.doNotRecalculate) {
                    log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
                    PrintLogIfActive(log);
                    if (!DropPlan(plan)) {
                        PlanGoapActions();
                    }
                } else {
                    PrintLogIfActive(log);
                    RecalculatePlan(plan);
                    //IdlePlans();
                }
            }
        }
    }
    public void GoapActionResult(string result, GoapAction action) {
        string log = GameManager.Instance.TodayLogString() + name + " is done performing goap action: " + action.goapName;
        //Debug.Log(log);
        if (action == currentAction) {
            SetCurrentAction(null);
        }
        if (action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character targetCharacter = action.poiTarget as Character;
            targetCharacter.RemoveTargettedByAction(action);
        }
        GoapPlan plan = action.parentPlan;
        if (isDead) {
            log += "\n" + name + " is dead!";
            if(plan.job != null) {
                if (result == InteractionManager.Goap_State_Success) {
                    if (plan.currentNode.parent == null) {
                        log += "This plan has a job and the result of action " + action.goapName + " is " + result + " and this is the last action for this plan, removing job in job queue...";
                        plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
                    }
                }
                log += "\nSetting assigned character and plan to null...";
                plan.job.SetAssignedCharacter(null);
                plan.job.SetAssignedPlan(null);
            }
            PrintLogIfActive(log);
            return;
        }
        //if (plan == null) {
        //    log += "\nAction " + action.goapName + " no longer has a plan, current plans are: ";
        //    for (int i = 0; i < allGoapPlans.Count; i++) {
        //        if(i > 0) {
        //            log += ", ";
        //        }
        //        log += allGoapPlans[i].endNode.action.goapName;
        //    }
        //    PrintLogIfActive(log);
        //    PlanGoapActions();
        //    return;
        //}
        if (action.isStopped) {
            log += "\nAction is stopped!";
            PrintLogIfActive(log);
            if (!DropPlan(plan)) {
                PlanGoapActions();
            }
            return;
        }
        if (result == InteractionManager.Goap_State_Success) {
            log += "\nAction performed is a success!";
            plan.SetNextNode();
            if (plan.currentNode == null) {
                log += "\nThis action is the end of plan.";
                if (plan.job != null) {
                    log += "\nRemoving job in queue...";
                    plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
                }
                PrintLogIfActive(log);
                //this means that this is the end goal so end this plan now
                if (!DropPlan(plan)) {
                    PlanGoapActions();
                }
            } else {
                log += "\nNext action for this plan: " + plan.currentNode.action.goapName;
                PrintLogIfActive(log);
                PlanGoapActions();
            }
        } else if(result == InteractionManager.Goap_State_Fail) {
            if(plan.endNode.action == action) {
                log += "\nAction performed has failed. Since this action is the end/goal action, it will not recalculate anymore. Dropping plan...";
                PrintLogIfActive(log);
                if (!DropPlan(plan)) {
                    PlanGoapActions();
                }
            } else {
                log += "\nAction performed has failed. Will try to recalculate plan...";
                if (plan.doNotRecalculate) {
                    log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
                    PrintLogIfActive(log);
                    if (!DropPlan(plan)) {
                        PlanGoapActions();
                    }
                } else {
                    PrintLogIfActive(log);
                    RecalculatePlan(plan);
                    //IdlePlans();
                }
            }
        }
    }
    public bool DropPlan(GoapPlan plan) {
        if (allGoapPlans.Remove(plan)) {
            plan.EndPlan();
            if(plan.job != null) {
                plan.job.SetAssignedCharacter(null);
                plan.job.SetAssignedPlan(null);
            }
            if (allGoapPlans.Count <= 0) {
                PlanGoapActions();
                return true;
                //StartDailyGoapPlanGeneration();
            }
        }
        return false;
    }
    public void DropAllPlans(GoapPlan planException = null) {
        if(planException == null) {
            while (allGoapPlans.Count > 0) {
                DropPlan(allGoapPlans[0]);
            }
        } else {
            for (int i = 0; i < allGoapPlans.Count; i++) {
                if(allGoapPlans[i] != planException) {
                    DropPlan(allGoapPlans[i]);
                    i--;
                }
            }
        }
    }
    public GoapPlan GetPlanWithGoalEffect(GOAP_EFFECT_CONDITION goalEffect) {
        for (int i = 0; i < allGoapPlans.Count; i++) {
            if (allGoapPlans[i].goalEffects.Contains(goalEffect)) {
                return allGoapPlans[i];
            }
        }
        return null;
    }
    public GoapPlan GetPlanByCategory(GOAP_CATEGORY category) {
        for (int i = 0; i < allGoapPlans.Count; i++) {
            if (allGoapPlans[i].category == category) {
                return allGoapPlans[i];
            }
        }
        return null;
    }
    //For testing: Drop Character
    public void DropACharacter() {
        if (awareness.ContainsKey(POINT_OF_INTEREST_TYPE.CHARACTER)) {
            List<IAwareness> characterAwarenesses = awareness[POINT_OF_INTEREST_TYPE.CHARACTER];
            Character randomTarget = characterAwarenesses[UnityEngine.Random.Range(0, characterAwarenesses.Count)].poi as Character;
            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = randomTarget };
            StartGOAP(goapEffect, randomTarget, GOAP_CATEGORY.REACTION);
        }
    }
    public GoapPlan GetPlanWithAction(GoapAction action) {
        for (int i = 0; i < allGoapPlans.Count; i++) {
            for (int j = 0; j < allGoapPlans[i].allNodes.Count; j++) {
                if (allGoapPlans[i].allNodes[j].action == action) {
                    return allGoapPlans[i];
                }
            }
        }
        return null;
    }
    public void OnCharacterDoAction(GoapAction action) {
        Messenger.Broadcast(Signals.CHARACTER_DID_ACTION, this, action);
    }
    public void FaceTarget(IPointOfInterest target) {
        if (this != target && !this.isDead && target.gridTileLocation != null && gridTileLocation != null) {
            if (target is Character && (target as Character).isDead) {
                return;
            }
            if (target is Character) {
                marker.LookAt((target as Character).marker.transform.position);
            } else {
                marker.LookAt(target.gridTileLocation.centeredWorldLocation);
            }            
        }
    }
    public void SetCurrentAction(GoapAction action) {
        if(currentAction != null && action == null) {
            //This means that the current action of this character is being set to null, when this happens, the poi target of the current action must not wait for interaction anymore
            if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                Character targetCharacter = currentAction.poiTarget as Character;
                if (targetCharacter != currentAction.actor) {
                    targetCharacter.AdjustIsWaitingForInteraction(-1);
                    targetCharacter.RemoveTargettedByAction(currentAction);
                }
            }
        }
        currentAction = action;
        if (currentAction != null) {
            PrintLogIfActive(GameManager.Instance.TodayLogString() + this.name + " will do action " + action.goapType.ToString() + " to " + action.poiTarget.ToString());
        }
        string summary = GameManager.Instance.TodayLogString() + "Set current action to ";
        if (currentAction == null) {
            summary += "null";
        } else {
            summary += currentAction.goapName + " targetting " + currentAction.poiTarget.name;
        }
        summary += "\n StackTrace: " + StackTraceUtility.ExtractStackTrace();

        actionHistory.Add(summary);
        if (actionHistory.Count > 10) {
            actionHistory.RemoveAt(0);
        }

    }
    public void SetHasAlreadyAskedForPlan(bool state) {
        _hasAlreadyAskedForPlan = state;
    }
    public void PrintLogIfActive(string log) {
        if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == this) {
            Debug.Log(log);
        }
    }
    private void AddPlanAsPriority(GoapPlan plan) {
        allGoapPlans.Insert(0, plan);
    }
    public void OnTargettedByAction(GoapAction action) {
        targettedByAction.Add(action);
        //targettedByAction = action;
        if (marker != null) {
            marker.OnCharacterTargettedByAction();
        }
    }
    public void RemoveTargettedByAction(GoapAction action) {
        if (targettedByAction.Remove(action)) {
            //targettedByAction = null;
            if (marker != null) {
                marker.OnCharacterRemovedTargettedByAction();
            }
        }
    }
    private void OnCharacterFinishedAction(Character character, GoapAction action, string result) {
        RemoveTargettedByAction(action);
    }
    #endregion

    #region Supply
    public void AdjustSupply(int amount) {
        supply += amount;
        if(supply < 0) {
            supply = 0;
        }
    }
    public void SetSupply(int amount) {
        supply = amount;
        if (supply < 0) {
            supply = 0;
        }
    }
    #endregion

    #region Collision
    //Most of collision is handled by the character's marker
    public void InitializeCollisionTrigger() { }
    public void PlaceCollisionTriggerAt(LocationGridTile tile) { }
    public void DisableCollisionTrigger() { }
    public void SetCollisionTrigger(POICollisionTrigger trigger) { }
    public void PlaceGhostCollisionTriggerAt(LocationGridTile tile) { }
    #endregion

    #region Hostility
    /// <summary>
    /// Function to encapsulate, whether or not this character treats another character as hostile.
    /// </summary>
    /// <param name="character">Character in question.</param>
    public bool IsHostileWith(Character character) {
        //return true;
        if (character.ignoreHostility > 0 || this.ignoreHostility > 0) {
            //if either the character in question or this character should ignore hostility, return false.
            return false;
        }
        if (this.faction.id == FactionManager.Instance.neutralFaction.id) {
            //this character is unaligned
            //if unaligned, hostile to all other characters, except those of same race
            return character.race != this.race;
        } else {
            //this character has a faction
            //if has a faction, is hostile to characters of every other faction
            return this.faction.id != character.faction.id;
        }
    }
    /// <summary>
    /// Adjusts value that ignores hostility. This character will ignore hostiles and will be ignored by other hostiles if
    /// the value is greater than 0.
    /// </summary>
    /// <param name="amount">Amount to increase/decrease</param>
    public void AdjustIgnoreHostilities(int amount) {
        ignoreHostility += amount;
        ignoreHostility = Mathf.Max(0, ignoreHostility);
    }
    #endregion

    #region Crime System
    /// <summary>
    /// Make this character react to a crime that he/she witnessed.
    /// </summary>
    /// <param name="witnessedCrime">Witnessed Crime.</param>
    /// <param name="notifyPlayer">Should the player be notified when this happens?</param>
    public void ReactToCrime(GoapAction witnessedCrime, bool notifyPlayer = true) {
        ReactToCrime(witnessedCrime.committedCrime, witnessedCrime.actor, witnessedCrime, notifyPlayer);
    }
    /// <summary>
    /// Base function for crime reactions
    /// </summary>
    /// <param name="committedCrime">The type of crime that was committed.</param>
    /// <param name="actor">The character that committed the crime</param>
    /// <param name="witnessedCrime">The crime witnessed by this character, if this is null, character was only informed of the crime by someone else.</param>
    /// <param name="notifyPlayer">Should the player be notified when this happens?</param>
    public void ReactToCrime(CRIME committedCrime, Character actor, GoapAction witnessedCrime = null, bool notifyPlayer = true) {
        string reactSummary = GameManager.Instance.TodayLogString() + this.name + " will react to crime committed by " + actor.name;
        Log witnessLog = null;
        //If character has a positive relationship (Friend, Lover, Paramour) with the criminal
        if (this.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            reactSummary += "\n" + this.name + " has a positive relationship with " + actor.name;
            CRIME_CATEGORY category = committedCrime.GetCategory();
            //and crime severity is less than Serious Crimes:
            if (category.IsLessThan(CRIME_CATEGORY.SERIOUS)) {
                reactSummary += "\nCrime committed is less than serious, " + this.name + " will not do anything.";
                //-Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder] but did not do anything due to their relationship."
                witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "do_nothing");
            }
            //and crime severity is Serious Crimes or worse:
            else if (category.IsGreaterThanOrEqual(CRIME_CATEGORY.SERIOUS)) {
                reactSummary += "\nCrime committed is serious or worse. Removing positive relationships.";
                //- Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]! They are no longer [Friends/Lovers/Paramours]."
                List<RelationshipTrait> traitsToRemove = GetAllRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE);
                CharacterManager.Instance.RemoveRelationshipBetween(this, actor, traitsToRemove);

                string removedTraitsSummary = string.Empty;
                for (int i = 0; i < traitsToRemove.Count; i++) {
                    RelationshipTrait currTrait = traitsToRemove[i];
                    if (i + 1 == traitsToRemove.Count && traitsToRemove.Count != 1) removedTraitsSummary += " and ";  //this is the last element
                    else if (i > 0) removedTraitsSummary += ", ";

                    removedTraitsSummary += Utilities.GetRelationshipPlural(currTrait.relType);
                }
                reactSummary += "\nRemoved relationships: " + removedTraitsSummary;

                if (traitsToRemove.Count > 0) {
                    //if traits were removed, use remove relationship version of log
                    witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "remove_relationship");
                    witnessLog.AddToFillers(null, removedTraitsSummary, LOG_IDENTIFIER.STRING_2);
                } else {
                    //else use normal witnessed log
                    witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed");
                }
                PerRoleCrimeReaction(committedCrime, actor, witnessedCrime);
            }
        }
        //If character has no relationships with the criminal or they are enemies:
        else if (!this.HasRelationshipWith(actor) || this.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
            reactSummary += "\n" + this.name + " does not have a relationship with or is an enemy of " + actor.name;
            //-Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]!"
            witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed");
            PerRoleCrimeReaction(committedCrime, actor, witnessedCrime);
        }

        if (witnessLog != null) {
            witnessLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            witnessLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            witnessLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(committedCrime.ToString()), LOG_IDENTIFIER.STRING_1);
            if (notifyPlayer) {
                PlayerManager.Instance.player.ShowNotificationFrom(this, witnessLog);
            }
        }
        Debug.Log(reactSummary);
    }
    /// <summary>
    /// Crime reactions per role.
    /// </summary>
    /// <param name="committedCrime">The type of crime that was committed.</param>
    /// <param name="actor">The character that committed the crime</param>
    /// <param name="witnessedCrime">The crime witnessed by this character, if this is null, character was only informed of the crime by someone else.</param>
    private void PerRoleCrimeReaction(CRIME committedCrime, Character actor, GoapAction witnessedCrime = null) {
        GoapPlanJob job = null;
        switch (role.roleType) {
            case CHARACTER_ROLE.CIVILIAN:
            case CHARACTER_ROLE.ADVENTURER:
                //- If the character is a Civilian or Adventurer, he will enter Flee mode (fleeing the criminal) and will create a Report Crime Job Type in his personal job queue
                if (witnessedCrime != null && this.faction != FactionManager.Instance.neutralFaction && actor.faction == this.faction) {
                    //only make character flee, if he/she actually witnessed the crime (not share intel)
                    this.marker.AddHostileInRange(actor, CHARACTER_STATE.FLEE);
                    job = new GoapPlanJob("Report Crime", INTERACTION_TYPE.REPORT_CRIME, new object[] { witnessedCrime });
                    jobQueue.AddJobInQueue(job);
                }
                break;
            case CHARACTER_ROLE.LEADER:
            case CHARACTER_ROLE.NOBLE:
                //- If the character is a Noble or Faction Leader, the criminal will gain the relevant Crime-type trait
                //If he is a Noble or Faction Leader, he will create the Apprehend Job Type in the Location job queue instead.
                if (this.faction != FactionManager.Instance.neutralFaction && actor.faction == this.faction) {
                    //only add apprehend job if the criminal is part of this characters faction
                    actor.AddCriminalTrait(committedCrime);
                    job = new GoapPlanJob("Apprehend", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = actor });
                    job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
                    homeArea.jobQueue.AddJobInQueue(job);
                }
                
                break;
            case CHARACTER_ROLE.SOLDIER:
                //- If the character is a Soldier, the criminal will gain the relevant Crime-type trait
                if (this.faction != FactionManager.Instance.neutralFaction && actor.faction == this.faction) {
                    //only add apprehend job if the criminal is part of this characters faction
                    actor.AddCriminalTrait(committedCrime);
                    //- If the character is a Soldier, he will also create an Apprehend Job Type in his personal job queue.
                    job = new GoapPlanJob("Apprehend", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = actor });
                    job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
                    homeArea.jobQueue.AddJobInQueue(job);
                    homeArea.jobQueue.AssignCharacterToJob(job, this);
                }
                break;
            default:
                break;
        }
    }
    public void AddCriminalTrait(CRIME crime) {
        Trait trait = null;
        switch (crime) {
            case CRIME.THEFT:
                trait = new Thief();
                break;
            case CRIME.ASSAULT:
                trait = new Assaulter();
                break;
            case CRIME.MURDER:
                trait = new Murderer();
                break;
            default:
                break;
        }
        if (trait != null) {
            AddTrait(trait);
        }
    }
    public bool CanReactToCrime() {
        if (stateComponent.currentState == null) {
            return true;
        } else {
            if (stateComponent.currentState.characterState == CHARACTER_STATE.FLEE || stateComponent.currentState.characterState == CHARACTER_STATE.ENGAGE) {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Mood
    public void SetMoodValue(int amount) {
        moodValue = amount;
        moodValue = Mathf.Clamp(moodValue, 1, 100);
    }
    public void AdjustMoodValue(int amount) {
        moodValue += amount;
        moodValue = Mathf.Clamp(moodValue, 1, 100);
    }
    public CHARACTER_MOOD ConvertCurrentMoodValueToType() {
        return ConvertMoodValueToType(moodValue);
    }
    public CHARACTER_MOOD ConvertMoodValueToType(int amount) {
        if(amount >= 1 && amount < 26) {
            return CHARACTER_MOOD.DARK;
        } else if (amount >= 26 && amount < 51) {
            return CHARACTER_MOOD.BAD;
        } else if (amount >= 51 && amount < 76) {
            return CHARACTER_MOOD.GOOD;
        } else {
            return CHARACTER_MOOD.GREAT;
        }
    }
    #endregion
}