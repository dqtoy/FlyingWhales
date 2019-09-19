using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Character : ILeader, IPointOfInterest {

    protected string _name;
    protected string _firstName;
    protected string _characterColorCode;
    protected int _id;
    protected int _doNotDisturb;
    protected int _doNotGetHungry;
    protected int _doNotGetTired;
    protected int _doNotGetLonely;
    protected int _numOfWaitingForGoapThread;
    protected float _actRate;
    protected bool _isDead;
    protected bool _hasAlreadyAskedForPlan;
    protected bool _isChatting;
    protected bool _isFlirting;
    protected GENDER _gender;
    public SEXUALITY sexuality { get; private set; }
    protected CharacterClass _characterClass;
    protected RaceSetting _raceSetting;
    protected CharacterRole _role;
    protected Faction _faction;
    protected CharacterParty _ownParty;
    protected CharacterParty _currentParty;
    protected Weapon _equippedWeapon;
    protected Armor _equippedArmor;
    protected Item _equippedAccessory;
    protected Item _equippedConsumable;
    protected PortraitSettings _portraitSettings;
    protected Color _characterColor;
    protected Minion _minion;
    protected CombatCharacter _currentCombatCharacter;
    protected List<Skill> _skills;
    protected List<Log> _history;
    protected List<Trait> _normalTraits; //List of traits that are just normal Traits (Not including relationships)
    //protected Dictionary<ELEMENT, float> _elementalWeaknesses;
    //protected Dictionary<ELEMENT, float> _elementalResistances;
    protected PlayerCharacterItem _playerCharacterItem;
    private LocationStructure _currentStructure; //what structure is this character currently in.

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
    public int attackPowerMod { get; protected set; }
    public int speedMod { get; protected set; }
    public int maxHPMod { get; protected set; }
    public int attackPowerPercentMod { get; protected set; }
    public int speedPercentMod { get; protected set; }
    public int maxHPPercentMod { get; protected set; }

    public Area homeArea { get; protected set; }
    public Dwelling homeStructure { get; protected set; }
    public Area defendingArea { get; private set; }
    public MORALITY morality { get; private set; }

    public Dictionary<AlterEgoData, CharacterRelationshipData> relationships {
        get {
            return currentAlterEgo?.relationships ?? null;
        }
    }
    public List<INTERACTION_TYPE> currentInteractionTypes { get; private set; }
    public List<INTERACTION_TYPE> poiGoapActions { get; private set; }
    public List<GoapPlan> allGoapPlans { get; private set; }
    public GoapPlanner planner { get; set; }
    public int supply { get; set; }
    public int food { get; set; }
    public int isWaitingForInteraction { get; private set; }
    public CharacterMarker marker { get; private set; }
    public GoapAction currentAction { get; private set; }
    public GoapAction previousCurrentAction { get; private set; }
    public Character lastAssaultedCharacter { get; private set; }
    public List<GoapAction> targettedByAction { get; private set; }
    public CharacterStateComponent stateComponent { get; private set; }
    public List<SpecialToken> items { get; private set; }
    public JobQueue jobQueue { get; private set; }
    public JobQueueItem currentJob { get; private set; }
    public List<JobQueueItem> allJobsTargettingThis { get; private set; }
    public int moodValue { get; private set; }
    public bool isCombatant { get; private set; } //This should only be a getter but since we need to know when the value changes it now has a setter
    public List<Trait> traitsNeededToBeRemoved { get; private set; }
    public TrapStructure trapStructure { get; private set; }
    public bool isDisabledByPlayer { get; protected set; }
    public float speedModifier { get; private set; }
    public string deathStr { get; private set; }
    public TileObject tileObjectLocation { get; private set; }
    public BaseLandmark currentLandmark { get; private set; } //current Landmark Location. NOTE: Only has value if character is NOT at an area
    public CharacterTrait defaultCharacterTrait { get; private set; }
    public int isStoppedByOtherCharacter { get; private set; } //this is increased, when the action of another character stops this characters movement

    private List<System.Action> onLeaveAreaActions;
    private POI_STATE _state;

    public Dictionary<int, Combat> combatHistory;

    //Needs
    //Tiredness
    public int tiredness { get; protected set; }
    public int tirednessDecreaseRate { get; protected set; }
    public int tirednessForcedTick { get; protected set; }
    public int currentSleepTicks { get; protected set; }
    public int sleepScheduleJobID { get; protected set; }
    public bool hasCancelledSleepSchedule { get; protected set; }
    private int tirednessLowerBound; //how low can this characters tiredness go
    protected const int TIREDNESS_DEFAULT = 15000;
    protected const int TIREDNESS_THRESHOLD_1 = 10000;
    protected const int TIREDNESS_THRESHOLD_2 = 5000;

    //Fullness
    public int fullness { get; protected set; }
    public int fullnessDecreaseRate { get; protected set; }
    public int fullnessForcedTick { get; protected set; }
    private int fullnessLowerBound; //how low can this characters fullness go
    protected const int FULLNESS_DEFAULT = 15000;
    protected const int FULLNESS_THRESHOLD_1 = 10000;
    protected const int FULLNESS_THRESHOLD_2 = 5000;

    //Happiness
    public int happiness { get; protected set; }
    public int happinessDecreaseRate { get; protected set; }
    private int happinessLowerBound; //how low can this characters happiness go
    protected const int HAPPINESS_DEFAULT = 15000;
    protected const int HAPPINESS_THRESHOLD_1 = 10000;
    protected const int HAPPINESS_THRESHOLD_2 = 5000;
   
    public bool hasForcedFullness { get; protected set; }
    public bool hasForcedTiredness { get; protected set; }
    public TIME_IN_WORDS forcedFullnessRecoveryTimeInWords { get; protected set; }
    public TIME_IN_WORDS forcedTirednessRecoveryTimeInWords { get; protected set; }

    public static readonly int TREE_AWARENESS_LIMIT = 5; //The number of Tree Objects a character can have in his awareness, everytime a character adds a new tree object to his/her awareness list, remove the oldest one if this limit is reached

    //portrait
    public float hSkinColor { get; protected set; }
    public float hHairColor { get; protected set; }
    public float demonColor { get; protected set; }

    //hostility
    public virtual int ignoreHostility { get; protected set; }

    //alter egos
    public string currentAlterEgoName { get; private set; } //this character's currently active alter ego. Usually just Original.
    public Dictionary<string, AlterEgoData> alterEgos { get; private set; }
    public string originalClassName { get; private set; } //the class that this character started with
    private List<Action> pendingActionsAfterMultiThread; //List of actions to perform after a character is finished with all his/her multithread processing (This is to prevent errors while the character has a thread running)

    //misc
    public bool isFollowingPlayerInstruction { get; private set; } //is this character moving/attacking because of the players instruction
    public bool returnedToLife { get; private set; }
    public Tombstone grave { get; private set; }

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
    public bool isFactionless { //is the character part of the neutral faction? or no faction?
        get {
            if (faction == null || FactionManager.Instance.neutralFaction == faction) {
                return true;
            } else {
                return false;
            }
        }
    }
    public bool isIdle {
        get { return _doNotDisturb <= 0 && IsInOwnParty() && !currentParty.icon.isTravelling; }
    }
    public bool isLeader {
        get { return role == CharacterRole.LEADER; }
    }
    public bool isHoldingItem {
        get { return items.Count > 0; }
    }
    public bool isAtHomeArea {
        get { return currentLandmark == null && specificLocation.id == homeArea.id && !currentParty.icon.isTravellingOutside; }
    }
    public bool isPartOfHomeFaction { //is this character part of the faction that owns his home area
        get { return homeArea != null && faction != null && homeArea.region.IsFactionHere(faction); }
    }
    public bool isChatting {
        get { return _isChatting; }
    }
    public bool isFlirting {
        get { return _isFlirting; }
    }
    public GENDER gender {
        get { return _gender; }
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
            int total = (int)((_characterClass.baseSpeed + speedMod) * (1f + ((_raceSetting.speedModifier + speedPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int attackPower {
        get {
            int total = (int)((_characterClass.baseAttackPower + attackPowerMod) * (1f + ((_raceSetting.attackPowerModifier + attackPowerPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int maxHP {
        get {
            return _maxHP;
        }
    }
    public int currentHP {
        get { return this._currentHP; }
    }
    public int attackSpeed {
        get { return _characterClass.baseAttackSpeed; } //in milliseconds, The lower the amount the faster the attack rate
    }
    //public Dictionary<ELEMENT, float> elementalWeaknesses {
    //    get { return _elementalWeaknesses; }
    //}
    //public Dictionary<ELEMENT, float> elementalResistances {
    //    get { return _elementalResistances; }
    //}
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
    public Minion minion {
        get { return _minion; }
    }
    public int doNotDisturb {
        get { return _doNotDisturb; }
    }
    public int doNotGetHungry {
        get { return _doNotGetHungry; }
    }
    public int doNotGetLonely {
        get { return _doNotGetLonely; }
    }
    public int doNotGetTired {
        get { return _doNotGetTired; }
    }
    public bool isDefender {
        get { return defendingArea != null; }
    }
    //returns all normal traits (Non relationship)
    public List<Trait> normalTraits {
        get {
            return _normalTraits;
        }
    }
    public List<RelationshipTrait> relationshipTraits {
        get {
            return GetAllRelationshipTraits();
        }
    }
    public PlayerCharacterItem playerCharacterItem {
        get { return _playerCharacterItem; }
    }
    public CombatCharacter currentCombatCharacter {
        get { return _currentCombatCharacter; }
    }
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.CHARACTER; }
    }
    public LocationGridTile gridTileLocation {
        get {
            if(marker == null) {
                return null;
            }
            if (!IsInOwnParty()) {
                return currentParty.owner.gridTileLocation;
            }
            return GetLocationGridTileByXY(gridTilePosition.x, gridTilePosition.y);
            //if (tile == null) {
            //    LocationGridTile gridTile = specificLocation.areaMap.map[(int) marker.anchoredPos.x, (int) marker.anchoredPos.y];
            //    return gridTile;
            //}
            //return tile;
        }
    }
    public Vector2Int gridTilePosition {
        get {
            if (marker == null) {
                throw new Exception(this.name + " marker is null!");
            }
            return new Vector2Int((int) marker.anchoredPos.x, (int) marker.anchoredPos.y);
        }
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
    public bool isStarving { get { return fullness <= FULLNESS_THRESHOLD_2; } }
    public bool isExhausted { get { return tiredness <= TIREDNESS_THRESHOLD_2; } }
    public bool isForlorn { get { return happiness <= HAPPINESS_THRESHOLD_2; } }
    public bool isHungry { get { return fullness <= FULLNESS_THRESHOLD_1; } }
    public bool isTired { get { return tiredness <= TIREDNESS_THRESHOLD_1; } }
    public bool isLonely { get { return happiness <= HAPPINESS_THRESHOLD_1; } }
    public AlterEgoData currentAlterEgo {
        get {
            if (alterEgos == null || !alterEgos.ContainsKey(currentAlterEgoName)) {
                Debug.LogWarning(this.name + " Alter Ego Relationship Problem! Current alter ego is: " + currentAlterEgoName);
                return null;
            }
            return alterEgos[currentAlterEgoName];
        }
    }
    public Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness {
        get {
            return currentAlterEgo.awareness;
        }
    }
    public LocationStructure currentStructure {
        get {
            if (!IsInOwnParty()) {
                return currentParty.owner.currentStructure;
            }
            return _currentStructure;
        }
    }
    public float walkSpeed {
        get { return raceSetting.walkSpeed + (raceSetting.walkSpeed * characterClass.walkSpeedMod); }
    }
    public float runSpeed {
        get { return raceSetting.runSpeed + (raceSetting.runSpeed * characterClass.runSpeedMod); }
    }
    public bool isInTileObject {
        get { return tileObjectLocation != null; }
    }
    #endregion

    public Character(CharacterRole role, RACE race, GENDER gender) : this() {
        _id = Utilities.SetID(this);
        _gender = gender;
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        AssignRole(role, false);
        _characterClass = CharacterManager.Instance.CreateNewCharacterClass(GetClassForRole(role));
        originalClassName = _characterClass.className;
        SetName(RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender));
        SetMorality(MORALITY.GOOD);
        GenerateSexuality();
        ResetToFullHP();
        InitializeAlterEgos();
    }
    public Character(CharacterRole role, string className, RACE race, GENDER gender) : this() {
        _id = Utilities.SetID(this);
        _gender = gender;
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        AssignRole(role, false);
        _characterClass = CharacterManager.Instance.CreateNewCharacterClass(className);
        originalClassName = _characterClass.className;
        SetName(RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender));
        SetMorality(MORALITY.GOOD);
        GenerateSexuality();
        ResetToFullHP();
        InitializeAlterEgos();
    }
    public Character(CharacterSaveData data) : this() {
        _id = Utilities.SetID(this, data.id);
        _gender = data.gender;
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[data.race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        AssignRole(data.role, false);
        _characterClass = CharacterManager.Instance.CreateNewCharacterClass(data.className);
        originalClassName = _characterClass.className;
        SetName(data.name);
        SetMorality(data.morality);
        GenerateSexuality();
        ResetToFullHP();
        InitializeAlterEgos();
    }
    public Character(SaveDataCharacter data) {
        _id = Utilities.SetID(this, data.id);
        _characterColorCode = data.characterColorCode;
        _gender = data.gender;
        SetSexuality(data.sexuality);
        _characterClass = CharacterManager.Instance.CreateNewCharacterClass(data.className);
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[data.race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        AssignRole(CharacterManager.Instance.GetRoleByRoleType(data.roleType), false);
        SetPortraitSettings(data.portraitSettings);
        _characterColor = data.characterColor;
        SetName(data.name);

        hSkinColor = data.hSkinColor;
        hHairColor = data.hHairColor;
        demonColor = data.demonColor;

        currentAlterEgoName = data.currentAlterEgoName;
        originalClassName = data.originalClassName;
        isStoppedByOtherCharacter = data.isStoppedByOtherCharacter;

        _history = new List<Log>();
        //_elementalWeaknesses = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
        //_elementalResistances = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
        combatHistory = new Dictionary<int, Combat>();
        poiGoapActions = new List<INTERACTION_TYPE>();
        allGoapPlans = new List<GoapPlan>();
        targettedByAction = new List<GoapAction>();
        stateComponent = new CharacterStateComponent(this);
        items = new List<SpecialToken>();
        jobQueue = new JobQueue(this);
        allJobsTargettingThis = new List<JobQueueItem>();
        traitsNeededToBeRemoved = new List<Trait>();
        onLeaveAreaActions = new List<Action>();
        pendingActionsAfterMultiThread = new List<Action>();
        trapStructure = new TrapStructure();
        //for testing
        locationHistory = new List<string>();
        actionHistory = new List<string>();
        planner = new GoapPlanner(this);

        _normalTraits = new List<Trait>();
        alterEgos = new Dictionary<string, AlterEgoData>();
        items = new List<SpecialToken>();
    }
    public Character() {
        SetIsDead(false);
        _history = new List<Log>();
        _normalTraits = new List<Trait>();

        //RPG
        _level = 1;
        SetExperience(0);
        //_elementalWeaknesses = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
        //_elementalResistances = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
        combatHistory = new Dictionary<int, Combat>();
        currentInteractionTypes = new List<INTERACTION_TYPE>();
        poiGoapActions = new List<INTERACTION_TYPE>();
        allGoapPlans = new List<GoapPlan>();
        targettedByAction = new List<GoapAction>();
        stateComponent = new CharacterStateComponent(this);
        items = new List<SpecialToken>();
        jobQueue = new JobQueue(this);
        allJobsTargettingThis = new List<JobQueueItem>();
        traitsNeededToBeRemoved = new List<Trait>();
        onLeaveAreaActions = new List<Action>();
        pendingActionsAfterMultiThread = new List<Action>();
        trapStructure = new TrapStructure();
        tirednessLowerBound = 0;
        fullnessLowerBound = 0;
        happinessLowerBound = 0;
        SetPOIState(POI_STATE.ACTIVE);
        SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LUNCH_TIME);
        SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS.LATE_NIGHT);
        SetFullnessForcedTick();
        SetTirednessForcedTick();
        ResetSleepTicks();

        //for testing
        locationHistory = new List<string>();
        actionHistory = new List<string>();
        planner = new GoapPlanner(this);
       
        //hostiltiy
        ignoreHostility = 0;
    }

    //This is done separately after all traits have been loaded so that the data will be accurate
    //It is because all traits are added again, this would mean that OnAddedTrait will also be called
    //Some values of character are modified by adding traits, so since adding trait will still be processed, it will get modified twice or more
    //For example, the Glutton trait adds fullnessDecreaseRate by 50%
    //Now when the fullnessDecreaseRate value is loaded the value of it already includes the Glutton trait modification
    //But since the Glutton trait will process the add trait function, fullnessDecreaseRate will add by 50% again
    //So for example if the saved value is 150, then the loaded value will be 300 (150+150)
    public void LoadAllStatsOfCharacter(SaveDataCharacter data) {
        _doNotDisturb = data.doNotDisturb;
        _doNotGetHungry = data.doNotGetHungry;
        _doNotGetLonely = data.doNotGetLonely;
        _doNotGetTired = data.doNotGetTired;

        _maxHP = data.maxHP;
        _currentHP = data.currentHP;
        _level = data.level;
        _experience = data.experience;
        _maxExperience = data.maxExperience;
        attackPowerMod = data.attackPowerMod;
        speedMod = data.speedMod;
        maxHPMod = data.maxHPMod;
        attackPowerPercentMod = data.attackPowerPercentMod;
        speedPercentMod = data.speedPercentMod;
        maxHPPercentMod = data.maxHPPercentMod;
        morality = data.morality;

        currentInteractionTypes = data.currentInteractionTypes;
        supply = data.supply;
        moodValue = data.moodValue;
        isCombatant = data.isCombatant;
        isDisabledByPlayer = data.isDisabledByPlayer;
        speedModifier = data.speedModifier;
        deathStr = data.deathStr;
        _state = data.state;

        tiredness = data.tiredness;
        fullness = data.fullness;
        happiness = data.happiness;
        fullnessDecreaseRate = data.fullnessDecreaseRate;
        tirednessDecreaseRate = data.tirednessDecreaseRate;
        happinessDecreaseRate = data.happinessDecreaseRate;

        ignoreHostility = data.ignoreHostility;

        SetForcedFullnessRecoveryTimeInWords(data.forcedFullnessRecoveryTimeInWords);
        SetForcedTirednessRecoveryTimeInWords(data.forcedTirednessRecoveryTimeInWords);
        SetFullnessForcedTick(data.fullnessForcedTick);
        SetTirednessForcedTick(data.tirednessForcedTick);

        returnedToLife = data.returnedToLife;
    }
    /// <summary>
    /// Initialize data for this character that is not safe to put in the constructor.
    /// Usually this is data that is dependent on the character being fully constructed.
    /// </summary>
    public virtual void Initialize() {
        OnUpdateRace();
        OnUpdateCharacterClass();
        UpdateIsCombatantState();

        SetMoodValue(90);
        CreateOwnParty();

        //NOTE: These values will be randomized when this character is placed in his/her area map.
        tiredness = TIREDNESS_DEFAULT;
        fullness = FULLNESS_DEFAULT;
        happiness = HAPPINESS_DEFAULT;


        hSkinColor = UnityEngine.Random.Range(-360f, 360f);
        hHairColor = UnityEngine.Random.Range(-360f, 360f);
        demonColor = UnityEngine.Random.Range(-144f, 144f);

        //supply
        SetSupply(UnityEngine.Random.Range(10, 61)); //Randomize initial supply per character (Random amount between 10 to 60.)
#if !WORLD_CREATION_TOOL
        GetRandomCharacterColor();
#endif
    }
    public void InitialCharacterPlacement(LocationGridTile tile) {
        tiredness = TIREDNESS_DEFAULT;
        if (role.roleType != CHARACTER_ROLE.MINION) {
            ////Fullness value between 2600 and full.
            //SetFullness(UnityEngine.Random.Range(2600, FULLNESS_DEFAULT + 1));
            ////Happiness value between 2600 and full.
            //SetHappiness(UnityEngine.Random.Range(2600, HAPPINESS_DEFAULT + 1));
            fullness = FULLNESS_DEFAULT;
            happiness = HAPPINESS_DEFAULT;
        } else {
            fullness = FULLNESS_DEFAULT;
            happiness = HAPPINESS_DEFAULT;
        }

        ConstructInitialGoapAdvertisementActions();
#if !WORLD_CREATION_TOOL
        GameDate gameDate = GameManager.Instance.Today();
        gameDate.AddTicks(1);
        SchedulingManager.Instance.AddEntry(gameDate, () => PlanGoapActions(), this);
#endif
        marker.InitialPlaceMarkerAt(tile, false); //since normal characters are already placed in their areas.
        AddInitialAwareness();
        SubscribeToSignals();
        for (int i = 0; i < normalTraits.Count; i++) {
            normalTraits[i].OnOwnerInitiallyPlaced(this);
        }
    }

    #region Signals
    protected void SubscribeToSignals() {
        if(minion != null) {
            Debug.LogError(name + " is a minion and has subscribed to the signals!");
        }
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        //Messenger.AddListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        Messenger.AddListener(Signals.TICK_STARTED, PerTickGoapPlanGeneration);
        Messenger.AddListener(Signals.DAY_STARTED, DailyGoapProcesses);
        Messenger.AddListener(Signals.HOUR_STARTED, DecreaseNeeds);
        //Messenger.AddListener<Character, Area, Area>(Signals.CHARACTER_MIGRATED_HOME, OnCharacterMigratedHome);
        Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnLeaveArea);
        Messenger.AddListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnArrivedAtArea);
        Messenger.AddListener<Area, Character>(Signals.CHARACTER_EXITED_AREA, OnCharacterExitedArea);
        Messenger.AddListener<Character, string>(Signals.CANCEL_CURRENT_ACTION, CancelCurrentAction);
        ///Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet); Moved listener for action state set to CharacterManager for optimization <see cref="CharacterManager.OnActionStateSet(GoapAction, GoapActionState)">
        Messenger.AddListener<SpecialToken, LocationGridTile>(Signals.ITEM_PLACED_ON_TILE, OnItemPlacedOnTile);
        Messenger.AddListener<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, OnItemRemovedFromTile);
        Messenger.AddListener<Area>(Signals.SUCCESS_INVASION_AREA, OnSuccessInvadeArea);
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
    }
    public virtual void UnsubscribeSignals() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        //Messenger.RemoveListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickGoapPlanGeneration);
        Messenger.RemoveListener(Signals.DAY_STARTED, DailyGoapProcesses);
        Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseNeeds);
        //Messenger.RemoveListener<Character, Area, Area>(Signals.CHARACTER_MIGRATED_HOME, OnCharacterMigratedHome);
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnLeaveArea);
        Messenger.RemoveListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnArrivedAtArea);
        Messenger.RemoveListener<Area, Character>(Signals.CHARACTER_EXITED_AREA, OnCharacterExitedArea);
        Messenger.RemoveListener<Character, string>(Signals.CANCEL_CURRENT_ACTION, CancelCurrentAction);
        //Messenger.RemoveListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        Messenger.RemoveListener<SpecialToken, LocationGridTile>(Signals.ITEM_PLACED_ON_TILE, OnItemPlacedOnTile);
        Messenger.RemoveListener<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, OnItemRemovedFromTile);
        Messenger.RemoveListener<Area>(Signals.SUCCESS_INVASION_AREA, OnSuccessInvadeArea);
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
    }
    #endregion

    #region Listeners
    private void OnCharacterExitedArea(Area area, Character character) {
        if (character.id == this.id) {
            //Clear terrifying characters of this character if he/she leaves the area
            marker.ClearTerrifyingObjects();
        } else {
            if (marker == null) {
                throw new Exception("Marker of " + this.name + " is null!");
            }
            //remove the character that left the area from anyone elses list of terrifying characters.
            if (marker.terrifyingObjects.Count > 0) {
                for (int i = 0; i < party.characters.Count; i++) {
                    marker.RemoveTerrifyingObject(party.characters[i]);
                }
            }
        }
    }
    /// <summary>
    /// Listener for when the player successfully invades an area. And this character is still alive.
    /// </summary>
    /// <param name="area">The invaded area.</param>
    protected virtual void OnSuccessInvadeArea(Area area) {
        if (specificLocation == area && minion == null) {
            StopCurrentAction(false);
            if (stateComponent.currentState != null) {
                stateComponent.currentState.OnExitThisState();
            } else if (stateComponent.stateToDo != null) {
                stateComponent.SetStateToDo(null);
            }
            specificLocation.RemoveCharacterFromLocation(this);
            //marker.ClearAvoidInRange(false);
            //marker.ClearHostilesInRange(false);
            //marker.ClearPOIsInVisionRange();

            UnsubscribeSignals();
            RemoveAllNonPersistentTraits();
            ClearAllAwareness();
            CancelAllJobsAndPlans();
            SchedulingManager.Instance.ClearAllSchedulesBy(this);
            if (marker != null) {
                DestroyMarker();
            }
        }
    }
    #endregion

    #region Sexuality
    private void GenerateSexuality() {
        if (role.roleType == CHARACTER_ROLE.BEAST) {
            //For beasts:
            //100 % straight
            sexuality = SEXUALITY.STRAIGHT;
        } else {
            //For sapient creatures:
            //80 % straight
            //10 % bisexual
            //10 % gay
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 80) {
                sexuality = SEXUALITY.STRAIGHT;
            } else if (chance >= 80 && chance < 90) {
                sexuality = SEXUALITY.BISEXUAL;
            } else {
                sexuality = SEXUALITY.GAY;
            }
        }
    }
    public void SetSexuality(SEXUALITY sexuality) {
        this.sexuality = sexuality;
    }
    #endregion

    #region Marker
    public void CreateMarker() {
        GameObject portraitGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterMarker", Vector3.zero, Quaternion.identity, InteriorMapManager.Instance.transform);
        //RectTransform rect = portraitGO.transform as RectTransform;
        //portraitGO.transform.localPosition = pos;
        CharacterMarker marker = portraitGO.GetComponent<CharacterMarker>();
        marker.SetCharacter(this);
        marker.SetHoverAction(OnHoverMarker, OnHoverExit);
        SetCharacterMarker(marker);
    }
    public void DestroyMarker(LocationGridTile destroyedAt = null) {
        if (destroyedAt == null) {
            gridTileLocation.RemoveCharacterHere(this);
        } else {
            destroyedAt.RemoveCharacterHere(this);
        }
        ObjectPoolManager.Instance.DestroyObject(marker.gameObject);
        SetCharacterMarker(null);
    }
    public void DisableMarker() {
        marker.gameObject.SetActive(false);
        gridTileLocation.RemoveCharacterHere(this);
    }
    public void SetCharacterMarker(CharacterMarker marker) {
        this.marker = marker;
        //Debug.Log("Set marker of " + name + " to " + marker?.name ?? "null");
    }
    private void OnHoverMarker(Character character, LocationGridTile location) {
        //InteriorMapManager.Instance.ShowTileData(this, gridTileLocation);
        location.parentAreaMap.SetHoveredCharacter(character);
    }
    private void OnHoverExit(Character character, LocationGridTile location) {
        //InteriorMapManager.Instance.ShowTileData(this, gridTileLocation);
        location.parentAreaMap.SetHoveredCharacter(null);
    }
    public void AdjustSpeedModifier(float amount) {
        speedModifier += amount;
        if (marker != null) {
            marker.UpdateSpeed();
        }
    }
    public void PerTickDuringMovement() {
        for (int i = 0; i  < normalTraits.Count; i++) {
            Trait trait = normalTraits[i];
            if (!trait.isDisabled) {
                if (trait.PerTickOwnerMovement()) {
                    break;
                }
            }
        }
    }
    #endregion

    //Changes row number of this character
    public void SetRowNumber(int rowNumber) {
        this._currentRow = rowNumber;
    }
    //Changes character's side
    public void SetSide(SIDES side) {
        this._currentSide = side;
    }
    //Character's death
    public void SetIsDead(bool isDead) {
        _isDead = isDead;
    }
    public void RaiseFromDeath(int level = 1, System.Action<Character> onReturnToLifeAction = null, Faction faction = null, RACE race = RACE.SKELETON, string className = "") {
        if (faction == null) {
            GameManager.Instance.StartCoroutine(Raise(this, level, onReturnToLifeAction, FactionManager.Instance.neutralFaction, race, className));
        } else {
            GameManager.Instance.StartCoroutine(Raise(this, level, onReturnToLifeAction, faction, race, className));
        }
        
    }
    private IEnumerator Raise(Character target, int level, System.Action<Character> onReturnToLifeAction, Faction faction, RACE race, string className) {
        target.marker.PlayAnimation("Raise Dead");
        yield return new WaitForSeconds(0.7f);
        target.ReturnToLife(faction, race, className);
        target.SetLevel(level);
        yield return null;
        onReturnToLifeAction?.Invoke(this);
    }
    private void ReturnToLife(Faction faction, RACE race, string className) {
        if (_isDead) {
            returnedToLife = true;
            SetIsDead(false);
            SubscribeToSignals();
            ResetToFullHP();
            SetPOIState(POI_STATE.ACTIVE);
            ChangeFactionTo(faction);
            ChangeRace(race);
            AssignRole(CharacterRole.SOLDIER);
            if (string.IsNullOrEmpty(className)) {
                AssignClassByRole(this.role);
            } else {
                AssignClass(className);
            }
            ResetFullnessMeter();
            ResetTirednessMeter();
            ResetHappinessMeter();
            _ownParty.ReturnToLife();
            marker.OnReturnToLife();
            if (grave != null) {
                marker.PlaceMarkerAt(grave.gridTileLocation);
                grave.gridTileLocation.structure.RemovePOI(grave);
                SetGrave(null);
            }
            RemoveTrait("Dead");
            for (int i = 0; i < normalTraits.Count; i++) {
                normalTraits[i].OnReturnToLife(this);
            }
            //RemoveAllNonPersistentTraits();
            ClearAllAwareness();
            //Area gloomhollow = LandmarkManager.Instance.GetAreaByName("Gloomhollow");
            MigrateHomeStructureTo(null);
            //MigrateHomeTo(null);
            //AddInitialAwareness(gloomhollow);
            Messenger.Broadcast(Signals.CHARACTER_RETURNED_TO_LIFE, this);
        }
    }
    public virtual void Death(string cause = "normal", GoapAction deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null) {
        if(minion != null) {
            minion.Death(cause, deathFromAction, responsibleCharacter);
            return;
        }
        if (!_isDead) {
            if (currentAlterEgoName != CharacterManager.Original_Alter_Ego) {
                SwitchAlterEgo(CharacterManager.Original_Alter_Ego); //revert the character to his/her original alter ego
            }
            SetIsChatting(false);
            SetIsFlirting(false);
            Area deathLocation = ownParty.specificLocation;
            LocationStructure deathStructure = currentStructure;
            LocationGridTile deathTile = gridTileLocation;
            SetIsDead(true);
            UnsubscribeSignals();
            SetPOIState(POI_STATE.INACTIVE);
            CombatManager.Instance.ReturnCharacterColorToPool(_characterColor);

            if (currentParty.specificLocation == null) {
                throw new Exception("Specific location of " + this.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }
            if (stateComponent.currentState != null) {
                stateComponent.currentState.OnExitThisState();
                if (stateComponent.currentState != null) {
                    stateComponent.currentState.OnExitThisState();
                }
            } else if (stateComponent.stateToDo != null) {
                stateComponent.SetStateToDo(null);
            }
            if (deathFromAction != null) { //if this character died from an action, do not cancel the action that he/she died from. so that the action will just end as normal.
                CancelAllJobsTargettingThisCharacterExcept(deathFromAction, "target is already dead", false);
            } else {
                CancelAllJobsTargettingThisCharacter("target is already dead", false);
            }
            Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, this, "target is already dead");
            if (currentAction != null && !currentAction.cannotCancelAction) {
                currentAction.StopAction();
            }
            if(jobQueue.jobsInQueue.Count > 0) {
                jobQueue.CancelAllJobs();
            }
            if (ownParty.specificLocation != null && isHoldingItem) {
                DropAllTokens(ownParty.specificLocation, currentStructure, deathTile, true);
            }

            //clear traits that need to be removed
            traitsNeededToBeRemoved.Clear();

            if (currentLandmark != null) {
                currentLandmark.tileLocation.region.RemoveCharacterFromLocation(this);
            }

            if (!IsInOwnParty()) {
                _currentParty.RemoveCharacter(this);
            }
            _ownParty.PartyDeath();

            //if (this.race != RACE.SKELETON) {
            //    deathLocation.AddCorpse(this, deathStructure, deathTile);
            //}


            if (faction != null) {
                faction.LeaveFaction(this); //remove this character from it's factions list of characters
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

            //List<Character> characterRels = new List<Character>(this.relationships.Keys.ToList());
            //for (int i = 0; i < characterRels.Count; i++) {
            //    RemoveRelationship(characterRels[i]);
            //}

            //if (_minion != null) {
            //    PlayerManager.Instance.player.RemoveMinion(_minion);
            //}

            //ObjectPoolManager.Instance.DestroyObject(marker.gameObject);
            //deathTile.RemoveCharacterHere(this);

            //RemoveAllTraitsByType(TRAIT_TYPE.CRIMINAL); //remove all criminal type traits

            for (int i = 0; i < normalTraits.Count; i++) {
                normalTraits[i].OnDeath(this);
            }

            //RemoveAllNonPersistentTraits();

            SetHP(0);

            marker.OnDeath(deathTile);

            SetNumWaitingForGoapThread(0); //for raise dead
            Dead dead = new Dead();
            dead.SetCharacterResponsibleForTrait(responsibleCharacter);
            AddTrait(dead, gainedFromDoing: deathFromAction);

            CancelAllJobsAndPlans();

            PrintLogIfActive(GameManager.Instance.TodayLogString() + this.name + " died of " + cause);
            Log deathLog;
            if (_deathLog == null) {
                deathLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "death_" + cause);
                deathLog.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                if (responsibleCharacter != null) {
                    deathLog.AddToFillers(responsibleCharacter, responsibleCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                }
                //will only add death log to history if no death log is provided. NOTE: This assumes that if a death log is provided, it has already been added to this characters history.
                AddHistory(deathLog);
                //specificLocation.AddHistory(deathLog);
                PlayerManager.Instance.player.ShowNotification(deathLog);
            } else {
                deathLog = _deathLog;
            }
            deathStr = Utilities.LogReplacer(deathLog);
            Messenger.Broadcast(Signals.CHARACTER_DEATH, this);
        }
    }
    public void SetGrave(Tombstone grave) {
        this.grave = grave;
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
    public void AssignRole(CharacterRole role, bool updateCombatantState = true) {
        bool wasRoleChanged = false;
        if (_role != null) {
            if (_role.roleType == role.roleType) {
                //If character role is being changed to same role, do not change it
                return;
            }
            _role.OnChange(this);
            wasRoleChanged = true;
        }
        _role = role;
        if (_role != null) {
            _role.OnAssign(this);
        }
        if (wasRoleChanged) {
            Messenger.Broadcast(Signals.ROLE_CHANGED, this);
        }
        if (updateCombatantState) {
            UpdateIsCombatantState();
        }
    }
    #endregion

    #region Character Class
    public virtual string GetClassForRole(CharacterRole role) {
        if (role == CharacterRole.BEAST) {
            return Utilities.GetRespectiveBeastClassNameFromByRace(race);
        } else {
            string className = CharacterManager.Instance.GetRandomClassByIdentifier(role.classNameOrIdentifier);
            if (className != string.Empty) {
                return className;
            } else {
                return role.classNameOrIdentifier;
            }
        }
    }
    public void AssignClassByRole(CharacterRole role) {
        AssignClass(GetClassForRole(role));
    }
    public void RemoveClass() {
        if (_characterClass == null) { return; }
        RemoveTraitsFromClass();
        _characterClass = null;
    }
    private void AssignClass(string className) {
        if (CharacterManager.Instance.classesDictionary.ContainsKey(className)) {
            if (_characterClass != null) {
                //This means that the character currently has a class and it will be replaced with a new class
                RemoveTraitsFromClass();
            }
            _characterClass = CharacterManager.Instance.CreateNewCharacterClass(className);
            //_skills = new List<Skill>();
            //_skills.Add(_characterClass.skill);
            //EquipItemsByClass();
            OnUpdateCharacterClass();
        } else {
            throw new Exception("There is no class named " + className + " but it is being assigned to " + this.name);
        }
    }
    protected void OnUpdateCharacterClass() {
        SetTraitsFromClass();
        if (marker != null) {
            marker.UpdateMarkerVisuals();
        }
    }
    public void AssignClass(CharacterClass characterClass) {
        if (_characterClass != null) {
            //This means that the character currently has a class and it will be replaced with a new class
            RemoveTraitsFromClass();
        }
        _characterClass = characterClass;
        OnUpdateCharacterClass();
    }
    #endregion

    #region Jobs
    public void SetCurrentJob(JobQueueItem job) {
        currentJob = job;
    }
    public void AddJobTargettingThis(JobQueueItem job) {
        allJobsTargettingThis.Add(job);
    }
    public bool RemoveJobTargettingThis(JobQueueItem job) {
        return allJobsTargettingThis.Remove(job);
    }
    public void CancelAllJobsTargettingThisCharacter(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.jobType == jobType) {
                if (job.jobQueueParent.CancelJob(job)) {
                    i--;
                }
            }
        }
    }
    /// <summary>
    /// Cancel all jobs of type that is targetting this character, except jobs that have the provided character assigned to it.
    /// </summary>
    /// <param name="jobType">The type of job to cancel.</param>
    /// <param name="otherCharacter">The character exception.</param>
    public void CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE jobType, Character otherCharacter) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.jobType == jobType && job.assignedCharacter != otherCharacter) {
                if (job.jobQueueParent.CancelJob(job)) {
                    i--;
                }
            }
        }
    }
    public void CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE jobType, object conditionKey, Character otherCharacter) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.targetEffect.conditionKey == conditionKey && job.assignedCharacter != otherCharacter) {
                    if (job.jobQueueParent.CancelJob(job)) {
                        i--;
                    }
                }
            }
        }
    }
    public void CancelAllJobsTargettingThisCharacter(JOB_TYPE jobType, JobQueueItem except) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.jobType == jobType && job != except) {
                if (job.jobQueueParent.CancelJob(job)) {
                    i--;
                }
            }
        }
    }
    public void CancelAllJobsTargettingThisCharacter(JOB_TYPE jobType, object conditionKey) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.targetEffect.conditionKey == conditionKey) {
                    if (job.jobQueueParent.CancelJob(job)) {
                        i--;
                    }
                }
            }
        }
    }
    public void CancelAllJobsTargettingThisCharacter(string cause = "", bool shouldDoAfterEffect = true) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.jobQueueParent.CancelJob(job, cause, shouldDoAfterEffect)) {
                i--;
            }
        }
    }
    /// <summary>
    /// Cancel all jobs that are targetting this character, except job that has the given action.
    /// </summary>
    /// <param name="except">The exception.</param>
    /// <param name="cause">The cause for cancelling</param>
    /// <param name="shouldDoAfterEffect">Should the effect of the cancelled action be executed.</param>
    public void CancelAllJobsTargettingThisCharacterExcept(GoapAction except, string cause = "", bool shouldDoAfterEffect = true) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (except.parentPlan != null && except.parentPlan.job == job) {
                continue; //skip
            }
            if (job.jobQueueParent.CancelJob(job, cause, shouldDoAfterEffect)) {
                i--;
            }
        }
    }
    public bool HasJobTargettingThis(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.jobType == jobType) {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Checker if this character has a job of type targeting it that is currently being done.
    /// </summary>
    /// <param name="jobType">The type of job targetting this character.</param>
    /// <returns>True or false.</returns>
    public bool HasActiveJobTargettingThisCharacter(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.jobType == jobType && job.assignedCharacter != null) {
                return true;
            }
        }
        return false;
    }
    public int GetNumOfJobsTargettingThisCharacter(JOB_TYPE jobType) {
        int count = 0;
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.jobType == jobType) {
                count++;
            }
        }
        return count;
    }
    public bool HasJobTargettingThisCharacter(JOB_TYPE jobType, object conditionKey) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.targetEffect.conditionKey == conditionKey) {
                    return true;
                }
            }
        }
        return false;
    }
    public GoapPlanJob GetJobTargettingThisCharacter(JOB_TYPE jobType, object conditionKey) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.targetEffect.conditionKey == conditionKey) {
                    return job;
                }
            }
        }
        return null;
    }
    public List<GoapPlanJob> GetJobsTargettingThisCharacter(JOB_TYPE jobType, object conditionKey) {
        List<GoapPlanJob> jobs = new List<GoapPlanJob>();
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.targetEffect.conditionKey == conditionKey) {
                    jobs.Add(job);
                }
            }
        }
        return jobs;
    }
    private void CheckApprehendRelatedJobsOnLeaveLocation() {
        CancelAllJobsTargettingThisCharacter(JOB_TYPE.APPREHEND);

        //All apprehend jobs that are being done by this character must be unassigned
        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan plan = allGoapPlans[i];
            if (plan.job != null && plan.job.jobType == JOB_TYPE.APPREHEND) {
                plan.job.UnassignJob();
                i--;
            }
        }
    }
    private void CancelOrUnassignRemoveTraitRelatedJobs() {
        CancelAllJobsTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT);

        //All remove trait jobs that are being done by this character must be unassigned
        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan plan = allGoapPlans[i];
            if (plan.job != null && plan.job.jobType == JOB_TYPE.REMOVE_TRAIT) {
                plan.job.UnassignJob();
                i--;
            }
        }
    }
    private bool CreateJobsOnEnterVisionWith(Character targetCharacter, bool bypassInvisibilityCheck = false) {
        string log = name + " saw " + targetCharacter.name + ", will try to create jobs on enter vision...";
        if (!CanCharacterReact()) {
            log += "\nCharacter cannot react!";
            PrintLogIfActive(log);
            return true;
        }
        if (!bypassInvisibilityCheck) {
            Invisible invisible = targetCharacter.GetNormalTrait("Invisible") as Invisible;
            if (invisible != null && !invisible.charactersThatCanSee.Contains(this)) {
                log += "\nCharacter is invisible!";
                PrintLogIfActive(log);
                return true;
            }
        }
        bool hasCreatedJob = false;
        log += "\nChecking source character traits...";
        for (int i = 0; i < normalTraits.Count; i++) {
            log += "\n- " + normalTraits[i].name;
            if (normalTraits[i].CreateJobsOnEnterVisionBasedOnOwnerTrait(targetCharacter, this)) {
                log += ": created a job!";
                hasCreatedJob = true;
            } else {
                log += ": did not create a job!";
            }
        }

        log += "\nChecking target character traits...";
        for (int i = 0; i < targetCharacter.normalTraits.Count; i++) {
            log += "\n- " + targetCharacter.normalTraits[i].name;
            if (targetCharacter.normalTraits[i].CreateJobsOnEnterVisionBasedOnTrait(targetCharacter, this)) {
                hasCreatedJob = true;
                log += ": created a job!";
            } else {
                log += ": did not create a job!";
            }
        }
        log += "\nChecking relationship traits...";
        for (int i = 0; i < relationshipTraits.Count; i++) {
            if (relationshipTraits[i].targetCharacter == targetCharacter) {
                log += "\n- " + relationshipTraits[i].name;
                if (relationshipTraits[i].CreateJobsOnEnterVisionBasedOnTrait(this, this)) {
                    hasCreatedJob = true;
                    log += ": created a job!";
                } else {
                    log += ": did not create a job!";
                }
            }
        }
        PrintLogIfActive(log);
        return hasCreatedJob;
    }
    public bool CreateJobsOnEnterVisionWith(IPointOfInterest targetPOI, bool bypassInvisibilityCheck = false) {
        if(targetPOI is Character) {
            return CreateJobsOnEnterVisionWith(targetPOI as Character, bypassInvisibilityCheck);
        }
        string log = name + " saw " + targetPOI.name + ", will try to create jobs on enter vision...";
        if (!CanCharacterReact()) {
            log += "\nCharacter cannot react!";
            PrintLogIfActive(log);
            return true;
        }
        if (!bypassInvisibilityCheck) {
            Invisible invisible = targetPOI.GetNormalTrait("Invisible") as Invisible;
            if (invisible != null && !invisible.charactersThatCanSee.Contains(this)) {
                log += "\nCharacter is invisible!";
                PrintLogIfActive(log);
                return true;
            }
        }
        bool hasCreatedJob = false;
        log += "\nChecking source character traits...";
        for (int i = 0; i < normalTraits.Count; i++) {
            log += "\n- " + normalTraits[i].name;
            if (normalTraits[i].CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, this)) {
                log += ": created a job!";
                hasCreatedJob = true;
            } else {
                log += ": did not create a job!";
            }
        }
        log += "\nChecking target poi traits...";
        for (int i = 0; i < targetPOI.normalTraits.Count; i++) {
            log += "\n- " + targetPOI.normalTraits[i].name;
            if (targetPOI.normalTraits[i].CreateJobsOnEnterVisionBasedOnTrait(targetPOI, this)) {
                log += ": created a job!";
                hasCreatedJob = true;
            } else {
                log += ": did not create a job!";
            }
        }
        PrintLogIfActive(log);
        return hasCreatedJob;
    }
    private bool NormalJobsOnEnterVision(IPointOfInterest targetPOI) {
        bool hasCreatedJob = false;
        

        return hasCreatedJob;
    }
    /// <summary>
    /// Force this character to create an undermine job towards the target character.
    /// Only cases that this will not happen is:
    /// - if target character is already dead
    /// - this character already has an undermine job in his/her job queue
    /// </summary>
    /// <param name="targetCharacter">The character to undermine.</param>
    public void ForceCreateUndermineJob(Character targetCharacter, string reason) {
        if (!targetCharacter.isDead) {
            CreateUndermineJobOnly(targetCharacter, reason);
        }
    }
    public bool CreateUndermineJobOnly(Character targetCharacter, string reason, SHARE_INTEL_STATUS status = SHARE_INTEL_STATUS.INFORMED) {
        if(jobQueue.HasJob(JOB_TYPE.UNDERMINE_ENEMY, targetCharacter)) {
            return false;
        }
        if(GetNormalTrait("Diplomatic") != null) {
            return false;
        }
        if(status == SHARE_INTEL_STATUS.WITNESSED) {
            //When creating undermine job and the creator of the job witnessed the event that caused him/her to undermine, mutate undermine job to knockout job
            //This means that all undermine jobs that are caused by witnessing an event will become knockout jobs
            return CreateKnockoutJob(targetCharacter);
        }
        WeightedDictionary<string> undermineWeights = new WeightedDictionary<string>();
        undermineWeights.AddElement("negative trait", 50);

        bool hasFriend = false;
        List<Log> crimeMemories = null;
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (currCharacter != targetCharacter && currCharacter != this) {
                if (currCharacter.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.FRIEND)) {
                    hasFriend = true;
                    break;
                }
            }
        }
        if (hasFriend) {
            int dayTo = GameManager.days;
            int dayFrom = dayTo - 3;
            if (dayFrom < 1) {
                dayFrom = 1;
            }
            crimeMemories = GetCrimeMemories(dayFrom, dayTo, targetCharacter);
            if (crimeMemories.Count > 0) {
                undermineWeights.AddElement("destroy friendship", 20);
            }
        }

        bool hasLoverOrParamour = false;
        List<Log> affairMemoriesInvolvingRumoredCharacter = null;
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (currCharacter != targetCharacter && currCharacter != this) {
                if (currCharacter.HasRelationshipOfTypeWith(targetCharacter, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
                    hasLoverOrParamour = true;
                    break;
                }
            }
        }
        if (hasLoverOrParamour) {
            List<Character> loversOrParamours = targetCharacter.GetCharactersWithRelationship(RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR);
            Character chosenLoverOrParamour = loversOrParamours[UnityEngine.Random.Range(0, loversOrParamours.Count)];
            if(chosenLoverOrParamour != null) {
                int dayTo = GameManager.days;
                int dayFrom = dayTo - 3;
                if (dayFrom < 1) {
                    dayFrom = 1;
                }
                List<Log> memories = GetWitnessOrInformedMemories(dayFrom, dayTo, targetCharacter);
                affairMemoriesInvolvingRumoredCharacter = new List<Log>();
                for (int i = 0; i < memories.Count; i++) {
                    Log memory = memories[i];
                    //if the event means Character 2 flirted, asked to make love or made love with another character other than Target, include it
                    if (memory.goapAction.actor != chosenLoverOrParamour && !memory.goapAction.IsTarget(chosenLoverOrParamour)) {
                        if (memory.goapAction.goapType == INTERACTION_TYPE.CHAT_CHARACTER) {
                            ChatCharacter chatAction = memory.goapAction as ChatCharacter;
                            if (chatAction.chatResult == "flirt") {
                                affairMemoriesInvolvingRumoredCharacter.Add(memory);

                            }
                        } else if (memory.goapAction.goapType == INTERACTION_TYPE.MAKE_LOVE) {
                            affairMemoriesInvolvingRumoredCharacter.Add(memory);
                        } else if (memory.goapAction.goapType == INTERACTION_TYPE.INVITE_TO_MAKE_LOVE) {
                            if(memory.goapAction.actor == targetCharacter) {
                                affairMemoriesInvolvingRumoredCharacter.Add(memory);
                            }else if (memory.goapAction.IsTarget(targetCharacter) && memory.goapAction.currentState.name == "Invite Success") {
                                affairMemoriesInvolvingRumoredCharacter.Add(memory);
                            }
                        }
                    }
                }
                if(affairMemoriesInvolvingRumoredCharacter.Count > 0) {
                    undermineWeights.AddElement("destroy love", 20);
                }
            }
        }

        if (undermineWeights.Count > 0) {
            string result = undermineWeights.PickRandomElementGivenWeights();
            GoapPlanJob job = null;
            if (result == "negative trait") {
                job = new GoapPlanJob(JOB_TYPE.UNDERMINE_ENEMY, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = targetCharacter });
            } else if (result == "destroy friendship") {
                job = new GoapPlanJob(JOB_TYPE.UNDERMINE_ENEMY, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Friend", targetPOI = targetCharacter },
                    new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.NONE, new object[] { targetCharacter, crimeMemories } }, });
            } else if (result == "destroy love") {
                job = new GoapPlanJob(JOB_TYPE.UNDERMINE_ENEMY, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Lover", targetPOI = targetCharacter },
                    new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.NONE, new object[] { targetCharacter, affairMemoriesInvolvingRumoredCharacter } }, });
            }

            //job.SetCannotOverrideJob(true);
            Debug.LogWarning(GameManager.Instance.TodayLogString() + "Added an UNDERMINE ENEMY Job: " + result + " to " + this.name + " with target " + targetCharacter.name);
            //job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
            jobQueue.AddJobInQueue(job, false);
            //if (processJobQueue) {
            //    jobQueue.ProcessFirstJobInQueue(this);
            //}

            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", reason + "_and_undermine");
            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddToFillers(null, currentMoodType.ToString().ToLower(), LOG_IDENTIFIER.STRING_1);
            AddHistory(log);

            PlayerManager.Instance.player.ShowNotificationFrom(log, this, false);
            return true;
        }
        return false;
    }
    public void CreateLocationKnockoutJobs(Character targetCharacter, int amount) {
        if (isAtHomeArea && isPartOfHomeFaction && !targetCharacter.isDead && !targetCharacter.isAtHomeArea && !this.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {//&& !targetCharacter.HasTraitOf(TRAIT_TYPE.DISABLER, "Combat Recovery")
            for (int i = 0; i < amount; i++) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.KNOCKOUT, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", targetPOI = targetCharacter });
                job.SetCanTakeThisJobChecker(CanCharacterTakeKnockoutJob);
                homeArea.jobQueue.AddJobInQueue(job);
            }
            //return job;
        }
        //return null;
    }
    public bool CreateKnockoutJob(Character targetCharacter) {
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.KNOCKOUT, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", targetPOI = targetCharacter });
        jobQueue.AddJobInQueue(job);
        PrintLogIfActive(GameManager.Instance.TodayLogString() + "Added a KNOCKOUT Job to " + this.name + " with target " + targetCharacter.name);
        return true;
    }
    private bool CanCharacterTakeKnockoutJob(Character character, Character targetCharacter, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.ADVENTURER; // && !HasRelationshipOfEffectWith(targetCharacter, TRAIT_EFFECT.POSITIVE)
    }
    /// <summary>
    /// Make this character create an apprehend job at his home location targetting a specific character.
    /// </summary>
    /// <param name="targetCharacter">The character to be apprehended.</param>
    /// <returns>The created job.</returns>
    public GoapPlanJob CreateApprehendJobFor(Character targetCharacter, bool assignSelfToJob = false) {
        //if (homeArea.id == specificLocation.id) {
        if (!targetCharacter.HasJobTargettingThis(JOB_TYPE.APPREHEND) && targetCharacter.GetNormalTrait("Restrained") == null && !this.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
            //GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = targetCharacter };
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP_CHARACTER, targetCharacter);
            job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = targetCharacter }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
            //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = this, targetPOI = targetCharacter }, INTERACTION_TYPE.CARRY_CHARACTER);
            //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = targetCharacter }, INTERACTION_TYPE.DROP_CHARACTER);
            job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
            //job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
            homeArea.jobQueue.AddJobInQueue(job);
            if(assignSelfToJob) {
                homeArea.jobQueue.AssignCharacterToJob(job, this);
            }
            return job;
        }
        return null;
        //}
    }
    public GoapPlanJob CreateObtainItemJob(SPECIAL_TOKEN item) {
        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = item.ToString(), targetPOI = this };
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.OBTAIN_ITEM, goapEffect);
        jobQueue.AddJobInQueue(job);
        //Debug.Log(this.name + " created job to obtain item " + item.ToString());
        //Messenger.Broadcast<string, int, UnityEngine.Events.UnityAction>(Signals.SHOW_DEVELOPER_NOTIFICATION, this.name + " created job to obtain item " + item.ToString(), 5, null);
        return job;
    }
    private bool CanCharacterTakeApprehendJob(Character character, Character targetCharacter, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER && character.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE;
    }
    public GoapPlanJob CreateAttemptToStopCurrentActionAndJob(Character targetCharacter, GoapPlanJob jobToStop) {
        if (!targetCharacter.HasJobTargettingThis(JOB_TYPE.ATTEMPT_TO_STOP_JOB)) {
            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_STOP_ACTION_AND_JOB, targetPOI = targetCharacter };
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.ATTEMPT_TO_STOP_JOB, goapEffect,
                new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.ASK_TO_STOP_JOB, new object[] { jobToStop } }, });
            jobQueue.AddJobInQueue(job);
            return job;
        }
        return null;
    }
    public void CreatePersonalJobs() {
        //Claim Item Job
        bool hasCreatedJob = false;
        //if (faction.id != FactionManager.Instance.neutralFaction.id && !jobQueue.HasJob("Claim Item")) {
        //    int numOfItemsOwned = GetNumOfItemsOwned();
        //    if (numOfItemsOwned < 3) {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        int value = 10 - (numOfItemsOwned * 3);
        //        if (chance < value) {
        //            List<SpecialToken> tokens = new List<SpecialToken>();
        //            for (int i = 0; i < specificLocation.possibleSpecialTokenSpawns.Count; i++) {
        //                SpecialToken currToken = specificLocation.possibleSpecialTokenSpawns[i];
        //                if (currToken.characterOwner == null) {
        //                    if (currToken.factionOwner != null && faction.id != currToken.factionOwner.id) {
        //                        continue;
        //                    }
        //                    if (currToken.gridTileLocation != null && currToken.gridTileLocation.structure != null
        //                        && currToken.gridTileLocation.structure.structureType == STRUCTURE_TYPE.WAREHOUSE && GetToken(currToken) == null && !currToken.HasJobTargettingThis("Claim Item")
        //                        && GetAwareness(currToken) != null) {
        //                        tokens.Add(currToken);
        //                    }
        //                }
        //            }
        //            if (tokens.Count > 0) {
        //                SpecialToken chosenToken = tokens[UnityEngine.Random.Range(0, tokens.Count)];
        //                GoapPlanJob job = new GoapPlanJob("Claim Item", INTERACTION_TYPE.PICK_ITEM, chosenToken);
        //                job.SetCancelOnFail(true);
        //                //GameManager.Instance.SetPausedState(true);
        //                Debug.LogWarning(GameManager.Instance.TodayLogString() + "Added a Claim Item Job to " + this.name + " with target " + chosenToken.name + " in " + chosenToken.gridTileLocation.ToString());
        //                jobQueue.AddJobInQueue(job);
        //                hasCreatedJob = true;
        //            }
        //        }
        //    }
        //}

        //Obtain Item job
        //if the character is part of a Faction and he doesnt have an Obtain Item Job in his personal job queue, 
        //there is a 10% chance that the character will create a Obtain Item Job if he has less than four items owned 
        //(sum from items in his inventory and in his home whose owner is this character). 
        //Reduce this chance by 3% for every item he owns (disregard stolen items)
        //NOTE: If he already has all items he needs, he doesnt need to do this job anymore.
        if (!isFactionless && !jobQueue.HasJob(JOB_TYPE.OBTAIN_ITEM) && !role.HasNeededItems(this)) {
            int numOfItemsOwned = GetNumOfItemsOwned();
            if (numOfItemsOwned < 4) {
                //string obtainSummary = name + " will roll to obtain item.";
                int chance = 10 - (3 * numOfItemsOwned);
                chance = Mathf.Max(0, chance);
                int roll = UnityEngine.Random.Range(0, 100);
                //obtainSummary += "\nChance to create job is " + chance.ToString() + ". Roll is " + roll.ToString();
                if (roll < chance) {
                    SPECIAL_TOKEN itemToObtain;
                    if (role.TryGetNeededItem(this, out itemToObtain)) {
                        CreateObtainItemJob(itemToObtain);
                        hasCreatedJob = true;
                        //obtainSummary += "\nCreated job to obtain " + itemToObtain.ToString();
                    } else {
                        //obtainSummary += "\nDoes not have any needed items.";
                    }
                }
                //Debug.Log(obtainSummary);
            }
        }

        //Undermine Enemy Job
        if (!hasCreatedJob && HasRelationshipTraitOf(RELATIONSHIP_TRAIT.ENEMY)) {
            int chance = UnityEngine.Random.Range(0, 100);
            int value = 3;
            CHARACTER_MOOD currentMood = currentMoodType;
            if (currentMood == CHARACTER_MOOD.DARK) {
                value += 1;
            } else if (currentMood == CHARACTER_MOOD.GOOD) {
                value -= 1;
            } else if (currentMood == CHARACTER_MOOD.GREAT) {
                value -= 3;
            }
            if (chance < value) {
                List<Character> enemyCharacters = GetCharactersWithRelationship(RELATIONSHIP_TRAIT.ENEMY);
                Character chosenCharacter = null;
                while (chosenCharacter == null && enemyCharacters.Count > 0) {
                    int index = UnityEngine.Random.Range(0, enemyCharacters.Count);
                    Character character = enemyCharacters[index];
                    if (character.HasJobTargettingThis(JOB_TYPE.UNDERMINE_ENEMY) || jobQueue.HasJob(JOB_TYPE.UNDERMINE_ENEMY, character)) {
                        enemyCharacters.RemoveAt(index);
                    } else {
                        chosenCharacter = character;
                    }
                }
                if (chosenCharacter != null) {
                    hasCreatedJob = CreateUndermineJobOnly(chosenCharacter, "idle");
                    //GoapPlanJob job = new GoapPlanJob("Undermine Enemy", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = chosenCharacter });
                    //job.SetCancelOnFail(true);
                    //job.SetCannotOverrideJob(true);
                    ////GameManager.Instance.SetPausedState(true);
                    //Debug.LogWarning(GameManager.Instance.TodayLogString() + "Added an UNDERMINE ENEMY Job to " + this.name + " with target " + chosenCharacter.name);
                    //jobQueue.AddJobInQueue(job);
                    //hasCreatedJob = true;
                }
            }
        }

        if (!hasCreatedJob && currentStructure is Dwelling) {
            Dwelling dwelling = currentStructure as Dwelling;
            if (dwelling.HasPositiveRelationshipWithAnyResident(this) && dwelling.HasUnoccupiedFurnitureSpot() && poiGoapActions.Contains(INTERACTION_TYPE.CRAFT_FURNITURE)) {
                //- if the character is in a Dwelling structure and he or someone he has a positive relationship with owns it, 
                //and the Dwelling still has an unoccupied Furniture Spot, 5% chance to add a Build Furniture Job.
                if (UnityEngine.Random.Range(0, 100) < 5) {
                    FACILITY_TYPE mostNeededFacility = dwelling.GetMostNeededValidFacility();
                    if (mostNeededFacility != FACILITY_TYPE.NONE) {
                        List<LocationGridTile> validSpots = dwelling.GetUnoccupiedFurnitureSpotsThatCanProvide(mostNeededFacility);
                        LocationGridTile chosenTile = validSpots[UnityEngine.Random.Range(0, validSpots.Count)];
                        FURNITURE_TYPE furnitureToCreate = chosenTile.GetFurnitureThatCanProvide(mostNeededFacility);
                        //check first if the character can build that specific type of furniture
                        if (furnitureToCreate.ConvertFurnitureToTileObject().CanBeCraftedBy(this)) {
                            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.BUILD_FURNITURE, INTERACTION_TYPE.CRAFT_FURNITURE, this, new Dictionary<INTERACTION_TYPE, object[]>() {
                                { INTERACTION_TYPE.CRAFT_FURNITURE, new object[] { chosenTile, furnitureToCreate } }
                            });
                            job.SetCancelOnFail(true);
                            //job.SetCannotOverrideJob(true);
                            jobQueue.AddJobInQueue(job);
                            //Debug.Log(this.name + " created a new build furniture job targetting tile " + chosenTile.ToString() + " with furniture type " + furnitureToCreate.ToString());
                        }
                    }
                }
            }
        }
    }
    public Character troubledCharacter { get; private set; }
    public void CreateAskForHelpJob(Character troubledCharacter, INTERACTION_TYPE helpType, params object[] otherData) {
        //&& troubledCharacter != this
        if (troubledCharacter != null) {
            this.troubledCharacter = troubledCharacter;
            Character targetCharacter = null;
            List<Character> positiveCharacters = GetCharactersWithRelationship(TRAIT_EFFECT.POSITIVE);
            positiveCharacters.Remove(troubledCharacter);
            if (positiveCharacters.Count > 0) {
                targetCharacter = positiveCharacters[UnityEngine.Random.Range(0, positiveCharacters.Count)];
            } else {
                List<Character> nonEnemyCharacters = GetCharactersWithoutRelationship(RELATIONSHIP_TRAIT.ENEMY).Where(x => x.faction.id == faction.id).ToList();
                nonEnemyCharacters.Remove(troubledCharacter);
                if (nonEnemyCharacters.Count > 0) {
                    targetCharacter = nonEnemyCharacters[UnityEngine.Random.Range(0, nonEnemyCharacters.Count)];
                }
            }
            if (targetCharacter != null) {
                JOB_TYPE jobType = (JOB_TYPE) Enum.Parse(typeof(JOB_TYPE), "ASK_FOR_HELP_" + helpType.ToString());
                INTERACTION_TYPE interactionType = (INTERACTION_TYPE)Enum.Parse(typeof(INTERACTION_TYPE), "ASK_FOR_HELP_" + helpType.ToString());
                GoapPlanJob job = new GoapPlanJob(jobType, interactionType, targetCharacter, new Dictionary<INTERACTION_TYPE, object[]>() {
                    { interactionType, otherData }
                });
                jobQueue.AddJobInQueue(job);
            } else {
                RegisterLogAndShowNotifToThisCharacterOnly("Generic", "ask_for_help_fail", troubledCharacter, troubledCharacter.name);
            }
        } else {
            if (troubledCharacter == null) {
                Debug.LogError(name + " cannot create ask for help save character job because troubled character is null!");
            } else {
                Debug.LogError(name + " cannot create ask for help save character job for " + troubledCharacter.name);
            }
        }
    }
    private void CreateAskForHelpSaveCharacterJob(Character troubledCharacter) {
        if (troubledCharacter != null && troubledCharacter != this) {
            this.troubledCharacter = troubledCharacter;
            Character targetCharacter = null;
            List<Character> positiveCharacters = GetCharactersWithRelationship(TRAIT_EFFECT.POSITIVE);
            positiveCharacters.Remove(troubledCharacter);
            if (positiveCharacters.Count > 0) {
                targetCharacter = positiveCharacters[UnityEngine.Random.Range(0, positiveCharacters.Count)];
            } else {
                List<Character> nonEnemyCharacters = GetCharactersWithoutRelationship(RELATIONSHIP_TRAIT.ENEMY).Where(x => x.faction.id == faction.id).ToList();
                nonEnemyCharacters.Remove(troubledCharacter);
                if (nonEnemyCharacters.Count > 0) {
                    targetCharacter = nonEnemyCharacters[UnityEngine.Random.Range(0, nonEnemyCharacters.Count)];
                }
            }
            if (targetCharacter != null) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.ASK_FOR_HELP_SAVE_CHARACTER, INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER, targetCharacter);
                jobQueue.AddJobInQueue(job);
            } else {
                RegisterLogAndShowNotifToThisCharacterOnly("Generic", "ask_for_help_fail", troubledCharacter, troubledCharacter.name);
            }
        } else {
            if (troubledCharacter == null) {
                Debug.LogError(name + " cannot create ask for help save character job because troubled character is null!");
            } else {
                Debug.LogError(name + " cannot create ask for help save character job for " + troubledCharacter.name);
            }
        }
    }
    public void CreateSaveCharacterJob(Character targetCharacter, bool processLogicForPersonalJob = true) {
        if (targetCharacter != null && targetCharacter != this) {
            string log = name + " is creating save character job for " + targetCharacter.name;
            if (role.roleType == CHARACTER_ROLE.CIVILIAN || role.roleType == CHARACTER_ROLE.NOBLE
                || role.roleType == CHARACTER_ROLE.LEADER) {
                CreateAskForHelpSaveCharacterJob(targetCharacter);
                log += "\n" + name + " is either a Civilian/Leader/Noble and cannot save a character, thus, will try to ask for help.";
                return;
            }
            if (!targetCharacter.HasJobTargettingThis(JOB_TYPE.SAVE_CHARACTER)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.SAVE_CHARACTER, INTERACTION_TYPE.DROP_CHARACTER, targetCharacter);
                job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = this, targetPOI = targetCharacter }, INTERACTION_TYPE.CARRY_CHARACTER);
                //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = targetCharacter.homeArea, targetPOI = targetCharacter }, INTERACTION_TYPE.DROP_CHARACTER);
                jobQueue.AddJobInQueue(job, processLogicForPersonalJob);
                log += "\n" + name + " created save character job.";
                //return job;
            } else {
                log += "\n" + targetCharacter.name + " is already being saved by someone.";
            }
            PrintLogIfActive(log);
        } else {
            if (targetCharacter == null) {
                Debug.LogError(name + " cannot create save character job because troubled character is null!");
            } else {
                Debug.LogError(name + " cannot create save character job for " + targetCharacter.name);
            }
        }
        //return null;
    }
    public GoapPlanJob CreateBreakupJob(Character targetCharacter) {
        if (jobQueue.HasJob(JOB_TYPE.BREAK_UP, targetCharacter)) {
            return null; //already has break up job targetting targetCharacter
        }
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.BREAK_UP, INTERACTION_TYPE.BREAK_UP, targetCharacter);
        jobQueue.AddJobInQueue(job);
        return job;
    }
    public void CreateShareInformationJob(Character targetCharacter, GoapAction info) {
        if (!IsHostileWith(targetCharacter) && !jobQueue.HasJobWithOtherData(JOB_TYPE.SHARE_INFORMATION, info)) {
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.SHARE_INFORMATION, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { info }}
                        });
            //job.SetCannotOverrideJob(true);
            job.SetCancelOnFail(true);
            jobQueue.AddJobInQueue(job, false);
        }
    }
    public void CancelAllJobsAndPlansExceptNeedsRecovery() {
        AdjustIsWaitingForInteraction(1);
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            if (jobQueue.jobsInQueue[i].jobType.IsNeedsTypeJob()) {
                continue;
            }
            if (jobQueue.CancelJob(jobQueue.jobsInQueue[i])) {
                i--;
            }
        }
        if (homeArea != null) {
            homeArea.jobQueue.UnassignAllJobsTakenBy(this);
        }

        StopCurrentAction(false);
        for (int i = 0; i < allGoapPlans.Count; i++) {
            if(allGoapPlans[i].job != null && allGoapPlans[i].job.jobType.IsNeedsTypeJob()) {
                if (JustDropPlan(allGoapPlans[i])) {
                    i--;
                }
            } else {
                if (DropPlan(allGoapPlans[i])) {
                    i--;
                }
            }
        }
        AdjustIsWaitingForInteraction(-1);
    }
    public void CancelAllJobsAndPlans() {
        AdjustIsWaitingForInteraction(1);
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            if (jobQueue.CancelJob(jobQueue.jobsInQueue[i])) {
                i--;
            }
        }
        if (homeArea != null) {
            homeArea.jobQueue.UnassignAllJobsTakenBy(this);
        }

        StopCurrentAction(false);
        for (int i = 0; i < allGoapPlans.Count; i++) {
            if (DropPlan(allGoapPlans[i])) {
                i--;
            }
        }
        AdjustIsWaitingForInteraction(-1);
    }
    public void CancelAllPlans() {
        StopCurrentAction(false);
        for (int i = 0; i < allGoapPlans.Count; i++) {
            if (DropPlan(allGoapPlans[i])) {
                i--;
            }
        }
    }
    public void CancelAllJobsAndPlansExcept(params JOB_TYPE[] job) {
        List<JOB_TYPE> exceptions = job.ToList();
        AdjustIsWaitingForInteraction(1);
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem item = jobQueue.jobsInQueue[i];
            if (!exceptions.Contains(item.jobType)) {
                if (jobQueue.CancelJob(jobQueue.jobsInQueue[i])) {
                    i--;
                }
            }
        }
        homeArea.jobQueue.UnassignAllJobsTakenBy(this);

        StopCurrentAction(false);
        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan currPlan = allGoapPlans[i];
            if (currPlan.job == null || !exceptions.Contains(currPlan.job.jobType)) {
                if (DropPlan(allGoapPlans[i])) {
                    i--;
                }
            }
        }
        AdjustIsWaitingForInteraction(-1);
    }
    public bool CanCurrentJobBeOverriddenByJob(JobQueueItem job) {
        //GENERAL RULE: Plans/States that have no jobs are always the lowest priority

        //Current job cannot be overriden by null job
        if (job == null) {
            return false;
        }
        if ((stateComponent.stateToDo != null && stateComponent.stateToDo.characterState == CHARACTER_STATE.BERSERKED)
            || (GetNormalTrait("Berserked") != null)) {
            //Berserked state cannot be overriden
            return false;
        }
        if (stateComponent.currentState == null && this.marker != null && this.marker.hasFleePath) {
            return false; //if the character is only fleeing, but is not in combat state, do not allow overriding.
        }
        if (stateComponent.currentState != null) {
            if (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                //Only override flee or engage state if the job is Berserked State, Berserk overrides all
                if (job is CharacterStateJob) {
                    CharacterStateJob stateJob = job as CharacterStateJob;
                    if (stateJob.targetState == CHARACTER_STATE.BERSERKED) {
                        return true;
                    }
                }
                return false;
            } else {
                //If current state is not Flee or Engage, it is certainly one of the major states since the only minor states are Flee and Engage
                //If current state has no job, it is automatically overridable, otherwise, if the current state's job has a lower job priority (higher number) than the parameter job, it is overridable
                if (stateComponent.currentState.job != null && !stateComponent.currentState.job.cannotOverrideJob && job.priority < stateComponent.currentState.job.priority) {
                    return true;
                } else if (stateComponent.currentState.job == null) {
                    return true;
                }
                return false;
            }
        } else if (stateComponent.stateToDo != null) {
            if (stateComponent.stateToDo.characterState == CHARACTER_STATE.COMBAT) {
                //Only override flee or engage state if the job is Berserked State, Berserk overrides all
                if (job is CharacterStateJob) {
                    CharacterStateJob stateJob = job as CharacterStateJob;
                    if (stateJob.targetState == CHARACTER_STATE.BERSERKED) {
                        return true;
                    }
                }
                return false;
            } else {
                //If current state is not Flee or Engage, it is certainly one of the major states since the only minor states are Flee and Engage
                //If current state has no job, it is automatically overridable, otherwise, if the current state's job has a lower job priority (higher number) than the parameter job, it is overridable
                if (stateComponent.stateToDo.job != null && !stateComponent.stateToDo.job.cannotOverrideJob && job.priority < stateComponent.stateToDo.job.priority) {
                    return true;
                } else if (stateComponent.stateToDo.job == null) {
                    return true;
                }
                return false;
            }
        }
        //Cannot override when resting
        if (GetNormalTrait("Resting") != null) {
            return false;
        }
        //If there is no current state then check the current action
        //Same process applies that if the current action's job has a lower job priority (higher number) than the parameter job, it is overridable
        if(currentAction != null && currentAction.goapType == INTERACTION_TYPE.MAKE_LOVE && !currentAction.isDone) {
            //Cannot override make love
            return false;
        }
        if (currentAction != null && currentAction.parentPlan != null) {
            if(currentAction.parentPlan.job != null && !currentAction.parentPlan.job.cannotOverrideJob
                       && job.priority < currentAction.parentPlan.job.priority) {
                return true;
            } else if (currentAction.parentPlan.job == null) {
                return true;
            }
            return false;
        }

        //If nothing applies, always overridable
        return true;
    }
    private GoapPlanJob CreateSuicideJob() {
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.SUICIDE, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = this });
        job.SetCanTakeThisJobChecker(IsSuicideJobStillValid);
        jobQueue.AddJobInQueue(job);
        return job;
    }
    private bool IsSuicideJobStillValid(Character character, JobQueueItem item) {
        return character.GetNormalTrait("Forlorn") != null;
    }
    /// <summary>
    /// Gets the current priority of the character's current action or state.
    /// If he/she has none, this will return a very high number.
    /// </summary>
    /// <returns></returns>
    public int GetCurrentPriorityValue() {
        if (stateComponent.currentState != null && stateComponent.currentState.job != null) {
            return stateComponent.currentState.job.priority;
        } else if (stateComponent.stateToDo != null && stateComponent.stateToDo.job != null) {
            return stateComponent.stateToDo.job.priority;
        } else if (currentAction != null && currentAction.parentPlan != null && currentAction.parentPlan.job != null) {
            return currentAction.parentPlan.job.priority;
        } else {
            return 999999;
        }
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
        currentAlterEgo.SetFaction(faction);
        OnChangeFaction();
        UpdateTokenOwner();
        if (_faction != null) {
            Messenger.Broadcast<Character>(Signals.FACTION_SET, this);
        }
    }
    public void ChangeFactionTo(Faction newFaction) {
        if (this.faction == newFaction) {
            return; //if the new faction is the same, ignore change
        }
        if (faction != null) {
            faction.LeaveFaction(this);
        }
        newFaction.JoinFaction(this);
    }
    private void OnChangeFaction() {
        //check if this character has a Criminal Trait, if so, remove it
        Trait criminal = GetNormalTrait("Criminal");
        if (criminal != null) {
            RemoveTrait(criminal, false);
        }
    }
    //public void FoundFaction(string factionName, Area location) {
    //    MigrateHomeTo(location);
    //    Faction newFaction = FactionManager.Instance.GetFactionBasedOnName(factionName);
    //    newFaction.SetLeader(this);
    //    ChangeFactionTo(newFaction);
    //    FactionManager.Instance.neutralFaction.RemoveFromOwnedRegions(location);
    //    LandmarkManager.Instance.OwnRegion(newFaction, race, location);
    //    newFaction.SetFactionActiveState(true);
    //}
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
        marker.collisionTrigger.SetMainColliderState(true);
        //if (this.minion != null) {
        //    this.minion.SetEnabledState(true); //reenable this minion, since it could've been disabled because it was part of another party
        //}
    }
    public void OnAddedToParty() {
        if (currentParty.id != ownParty.id) {
            ownParty.specificLocation.RemoveCharacterFromLocation(this);
            //ownParty.icon.SetVisualState(false);
            marker.collisionTrigger.SetMainColliderState(false);
        }
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
    public bool HasOtherCharacterInParty() {
        return ownParty.characters.Count > 1;
    }
    #endregion

    #region Location
    public void SetCurrentStructureLocation(LocationStructure currentStructure, bool broadcast = true) {
        if (currentStructure == this.currentStructure) {
            return; //ignore change;
        }
        LocationStructure previousStructure = this.currentStructure;
        _currentStructure = currentStructure;
        //if (marker != null && currentStructure != null) {
        //    marker.RevalidatePOIsInVisionRange(); //when the character changes structures, revalidate pois in range
        //}
        string summary = string.Empty;
        if (currentStructure != null) {
            summary = GameManager.Instance.TodayLogString() + "Arrived at <color=\"green\">" + currentStructure.ToString() + "</color>";
        } else {
            summary = GameManager.Instance.TodayLogString() + "Left <color=\"red\">" + previousStructure.ToString() + "</color>";
        }
        locationHistory.Add(summary);
        if (locationHistory.Count > 80) {
            locationHistory.RemoveAt(0);
        }

        if (currentStructure != null && broadcast) {
            Messenger.Broadcast(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, this, currentStructure);
        }
    }
    /// <summary>
    /// Move this character to another structure in the same area.
    /// </summary>
    /// <param name="newStructure">New structure the character is going to.</param>
    /// <param name="destinationTile">LocationGridTile where the character will go to (Must be inside the new structure).</param>
    /// <param name="targetPOI">The Point of Interest this character will interact with</param>
    /// <param name="arrivalAction">What should this character do when it reaches its target tile?</param>
    public void MoveToAnotherStructure(LocationStructure newStructure, LocationGridTile destinationTile, IPointOfInterest targetPOI = null, Action arrivalAction = null) {
        //if the character is already at the destination tile, just do the specified arrival action, if any.
        if (gridTileLocation == destinationTile) {
            if (arrivalAction != null) {
                arrivalAction();
            }
            //marker.PlayIdle();
        } else {
            if (destinationTile == null) {
                if (targetPOI != null) {
                    //if destination tile is null, make the charater marker use target poi logic (Usually used for moving targets)
                    marker.GoTo(targetPOI, arrivalAction);
                } else {
                    if (arrivalAction != null) {
                        arrivalAction();
                    }
                }
            } else {
                //if destination tile is not null, got there, regardless of target poi
                marker.GoTo(destinationTile, arrivalAction);
            }

        }
    }
    public void SetGridTileLocation(LocationGridTile tile) {
        //NOTE: Tile location is being computed every time.
        //this.tile = tile;
        //string summary = string.Empty;
        //if (tile == null) {
        //    summary = GameManager.Instance.TodayLogString() + "Set tile location to null";
        //} else {
        //    summary = GameManager.Instance.TodayLogString() + "Set tile location to " + tile.localPlace.ToString();
        //}
        //locationHistory.Add(summary);
        //if (locationHistory.Count > 80) {
        //    locationHistory.RemoveAt(0);
        //}
    }
    public LocationGridTile GetNearestUnoccupiedTileFromThis() {
        if (!isDead && gridTileLocation != null) {
            List<LocationGridTile> unoccupiedNeighbours = gridTileLocation.UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count == 0) {
                return null;
            } else {
                return unoccupiedNeighbours[UnityEngine.Random.Range(0, unoccupiedNeighbours.Count)];
            }
        }
        return null;
    }
    public LocationGridTile GetNearestUnoccupiedEdgeTileFromThis() {
        LocationGridTile currentGridTile = gridTileLocation;
        if (currentGridTile.IsAtEdgeOfWalkableMap() && currentGridTile.structure != null) {
            return currentGridTile;
        }

        LocationGridTile nearestEdgeTile = null;
        List<LocationGridTile> neighbours = gridTileLocation.neighbourList;
        for (int i = 0; i < neighbours.Count; i++) {
            if (neighbours[i].IsAtEdgeOfWalkableMap() && neighbours[i].structure != null && !neighbours[i].isOccupied) {
                nearestEdgeTile = neighbours[i];
                break;
            }
        }
        if (nearestEdgeTile == null) {
            float nearestDist = -999f;
            for (int i = 0; i < specificLocation.areaMap.allEdgeTiles.Count; i++) {
                LocationGridTile currTile = specificLocation.areaMap.allEdgeTiles[i];
                float dist = Vector2.Distance(currTile.localLocation, currentGridTile.localLocation);
                if (nearestDist == -999f || dist < nearestDist) {
                    if(currTile.structure != null) {
                        nearestEdgeTile = currTile;
                        nearestDist = dist;
                    }
                }
            }
        }
        return nearestEdgeTile;
    }
    private void OnLeaveArea(Party party) {
        if (currentParty == party) {
            CheckApprehendRelatedJobsOnLeaveLocation();
            //CancelOrUnassignRemoveTraitRelatedJobs();
            CancelAllJobsTargettingThisCharacter();
            marker.ClearTerrifyingObjects();
            ExecuteLeaveAreaActions();
        } else {
            if (marker.terrifyingObjects.Count > 0) {
                for (int i = 0; i < party.characters.Count; i++) {
                    marker.RemoveTerrifyingObject(party.characters[i]);
                }
            }
            //remove character from awareness
            for (int i = 0; i < party.characters.Count; i++) {
                Character character = party.characters[i];
                RemoveAwareness(character);
            }
        }
    }
    private void OnArrivedAtArea(Party party) {
        if (currentParty == party) {
            //if (isAtHomeArea) {
            //    if (HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
            //        CreateApprehendJob();
            //    }
            //    //for (int i = 0; i < traits.Count; i++) {
            //    //    if (traits[i].name == "Cursed" || traits[i].name == "Sick"
            //    //        || traits[i].name == "Injured" || traits[i].name == "Unconscious") {
            //    //        CreateRemoveTraitJob(traits[i].name);
            //    //    }
            //    //}
            //}
        } else {
            for (int i = 0; i < party.characters.Count; i++) {
                Character character = party.characters[i];
                AddAwareness(character); //become re aware of character
            }
        }
    }
    public void OnArriveAtAreaStopMovement() {
        currentParty.icon.SetTarget(null, null, null, null);
        currentParty.icon.SetOnPathFinished(null);
    }
    public void AddOnLeaveAreaAction(System.Action onLeaveAreaAction) {
        onLeaveAreaActions.Add(onLeaveAreaAction);
    }
    private void ExecuteLeaveAreaActions() {
        for (int i = 0; i < onLeaveAreaActions.Count; i++) {
            onLeaveAreaActions[i].Invoke();
        }
        onLeaveAreaActions.Clear();
    }
    public void SetLandmarkLocation(BaseLandmark landmark) {
        currentLandmark = landmark;
    }
    #endregion

    #region Utilities
    public void ChangeGender(GENDER gender) {
        _gender = gender;
        Messenger.Broadcast(Signals.GENDER_CHANGED, this, gender);
    }
    public bool ChangeRace(RACE race) {
        if (_raceSetting != null) {
            if (_raceSetting.race == race) {
                return false; //current race is already the new race, no change
            }
            RemoveTraitsFromRace();
        }
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        OnUpdateRace();
        Messenger.Broadcast(Signals.CHARACTER_CHANGED_RACE, this);
        return true;
    }
    public void OnUpdateRace() {
        SetTraitsFromRace();
        //Update Portrait to use new race
        _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
        if (marker != null) {
            marker.UpdateMarkerVisuals();
        }
        //update goap interactions that should no longer be valid
        if (race == RACE.SKELETON) {
            poiGoapActions.Remove(INTERACTION_TYPE.DRINK_BLOOD);
            poiGoapActions.Remove(INTERACTION_TYPE.SHARE_INFORMATION);
        }
    }
    public void RemoveRace() {
        if (_raceSetting == null) {
            return;
        }
        RemoveTraitsFromRace();
        _raceSetting = null;
    }
    public void SetRace(RACE race) {
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        OnUpdateRace();
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
        RandomNameGenerator.Instance.RemoveNameAsAvailable(this.gender, this.race, newName);
    }
    //If true, character can't do daily action (onDailyAction), i.e. actions, needs
    //public void SetIsIdle(bool state) {
    //    _isIdle = state;
    //}
    //public bool HasPathToParty(Party partyToJoin) {
    //    return PathGenerator.Instance.GetPath(currLocation, partyToJoin.currLocation, PATHFINDING_MODE.PASSABLE, _faction) != null;
    //}
    public void CenterOnCharacter() {
        if (marker != null) {
            if (currentParty.icon.isTravellingOutside) {
                if (specificLocation.areaMap.isShowing) {
                    InteriorMapManager.Instance.HideAreaMap();
                }
                //CameraMove.Instance.CenterCameraOn(currentParty.icon.travelLine.iconImg.gameObject);
                CameraMove.Instance.CenterCameraOn(homeArea.coreTile.gameObject);
            }else if (currentParty.icon.isTravelling) {
                if (marker.gameObject.activeInHierarchy) {
                    bool instantCenter = InteriorMapManager.Instance.currentlyShowingArea != specificLocation;
                    if (!specificLocation.areaMap.isShowing) {
                        InteriorMapManager.Instance.ShowAreaMap(specificLocation, false);
                    }
                    AreaMapCameraMove.Instance.CenterCameraOn(marker.gameObject, instantCenter);
                }
            } else if (currentLandmark != null) {
                CameraMove.Instance.CenterCameraOn(currentLandmark.tileLocation.gameObject);
            } else {
                bool instantCenter = InteriorMapManager.Instance.currentlyShowingArea != specificLocation;
                if (!specificLocation.areaMap.isShowing) {
                    InteriorMapManager.Instance.ShowAreaMap(specificLocation, false);
                }
                AreaMapCameraMove.Instance.CenterCameraOn(marker.gameObject, instantCenter);
            }
        }
    }
    private void GetRandomCharacterColor() {
        _characterColor = CombatManager.Instance.UseRandomCharacterColor();
        _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
    }
    internal void OnOtherCharacterDied(Character characterThatDied) {
        if (characterThatDied.id != this.id) {
            //RemoveRelationship(characterThatDied); //do not remove relationships when dying
            marker.OnOtherCharacterDied(characterThatDied);
        }
    }
    public void AdjustDoNotDisturb(int amount) {
        _doNotDisturb += amount;
        _doNotDisturb = Math.Max(_doNotDisturb, 0);
        //Debug.Log(GameManager.Instance.TodayLogString() + " adjusted do not disturb of " + this.name + " by " + amount + " new value is " + _doNotDisturb.ToString());
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
                    List<Area> validNeutralAreas = FactionManager.Instance.neutralFaction.ownedRegions.Where(x => x.area != null && !x.area.IsResidentsFull()).Select(x => x.area).ToList();
                    if (validNeutralAreas.Count > 0) {
                        //if it does, pick randomly from those
                        Area chosenArea = validNeutralAreas[UnityEngine.Random.Range(0, validNeutralAreas.Count)];
                        MigrateHomeTo(chosenArea);
                    }
                    //if not, keep the characters current home
                }
            } else { //if it is not, check if his original home is still owned by that faction and it has not yet reached it's resident capacity
                if (ogHome.region.IsFactionHere(ogFaction) && !ogHome.IsResidentsFull()) {
                    //if it meets those requirements, return the character's home to that location
                    MigrateHomeTo(ogHome);
                } else { //if not, get another area owned by his faction that has not yet reached capacity
                    List<Area> validAreas = ogFaction.ownedRegions.Where(x => x.area != null && !x.area.IsResidentsFull()).Select(x => x.area).ToList();
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
            List<Area> validNeutralAreas = FactionManager.Instance.neutralFaction.ownedRegions.Where(x => x.area != null && !x.area.IsResidentsFull()).Select(x => x.area).ToList();
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
    public void AdjustIsWaitingForInteraction(int amount) {
        isWaitingForInteraction += amount;
        if (isWaitingForInteraction < 0) {
            isWaitingForInteraction = 0;
        }
    }
    public LocationGridTile GetLocationGridTileByXY(int x, int y, bool throwOnException = true) {
        try {
            if (throwOnException) {
                return specificLocation.areaMap.map[x, y];
            } else {
                if (Utilities.IsInRange(x, 0, specificLocation.areaMap.map.GetUpperBound(0) + 1) &&
                    Utilities.IsInRange(y, 0, specificLocation.areaMap.map.GetUpperBound(1) + 1)) {
                    return specificLocation.areaMap.map[x, y];
                }
                return null;
            }
        } catch (Exception e) {
            throw new Exception(e.Message + "\n " + this.name + "(" + x.ToString() + ", " + y.ToString() + ")");
        }

    }
    public bool IsDoingCombatActionTowards(Character otherCharacter) {
        //if (marker.currentlyEngaging == otherCharacter) {
        //    return true;
        //}
        if (currentAction != null) {
            return currentAction.goapType == INTERACTION_TYPE.ASSAULT_CHARACTER && currentAction.poiTarget == otherCharacter;
        }
        return false;
    }
    public bool IsNear(IPointOfInterest poi) {
        if (poi is Character) {
            if (ownParty.characters.Contains(poi as Character)) {
                return true;
            }
        }
        return gridTileLocation == poi.gridTileLocation || gridTileLocation.IsAdjacentTo(poi);
    }
    public void UpdateIsCombatantState() {
        bool state = false;
        if (_role.roleType == CHARACTER_ROLE.CIVILIAN || _role.roleType == CHARACTER_ROLE.LEADER || _role.roleType == CHARACTER_ROLE.NOBLE) {
            state = true;
        } else if (GetNormalTrait("Injured") != null) {
            state = true;
        }
        if (isCombatant != state) {
            isCombatant = state;
            if (isCombatant && marker != null) {
                marker.ClearTerrifyingObjects();
            }
        }
    }
    public bool CanCharacterReact() {
        if(this is Summon || minion != null) {
            //Cannot react if summon or minion
            return false;
        }
        if (stateComponent.currentState != null && !stateComponent.currentState.isDone) {
            if (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                //Character must not react if he/she is in flee or engage state
                return false;
            }
            if (stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE) {
                //Character must not react if he/she is in douse fire state
                return false;
            }
        }
        if ((GetNormalTrait("Berserked") != null)
            || (stateComponent.stateToDo != null && stateComponent.stateToDo.characterState == CHARACTER_STATE.BERSERKED && !stateComponent.stateToDo.isDone)) {
            //Character must not react if he/she is in berserked state
            //Returns true so that it will create an impression that the character actually created a job even if he/she didn't, so that the character will not chat, etc.
            return false;
        }
        if(HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        return true;
    }
    public bool IsAble() {
        return currentHP > 0 && !HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER) && !isDead && characterClass.className != "Zombie";
    }
    public void SetIsFollowingPlayerInstruction(bool state) {
        isFollowingPlayerInstruction = state;
    }
    /// <summary>
    /// Can this character be instructed by the player?
    /// </summary>
    /// <returns>True or false.</returns>
    public virtual bool CanBeInstructedByPlayer() {
        if (PlayerManager.Instance.player.playerFaction != this.faction) {
            return false;
        }
        if (isDead) {
            return false;
        }
        if (stateComponent.currentState is CombatState && !(stateComponent.currentState as CombatState).isAttacking) {
            return false; //character is fleeing
        }
        if (HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        return true;
    }
    public void SetTileObjectLocation(TileObject tileObject) {
        tileObjectLocation = tileObject;
    }
    public bool IsDoingEmergencyAction() {
        return currentAction != null && currentAction.goapType.IsEmergencyAction();
    }
    public void AdjustIsStoppedByOtherCharacter(int amount) {
        isStoppedByOtherCharacter += amount;
        isStoppedByOtherCharacter = Mathf.Max(0, isStoppedByOtherCharacter);
        if (marker != null) {
            marker.UpdateAnimation();
        }
    }
    #endregion

    #region Relationships
    public void AddRelationship(Character character, RelationshipTrait newRel) {
        AddRelationship(character.currentAlterEgo, newRel);
    }
    public void AddRelationship(AlterEgoData alterEgo, RelationshipTrait newRel) {
        currentAlterEgo.AddRelationship(alterEgo, newRel);

        //if (!relationships.ContainsKey(alterEgo)) {
        //    relationships.Add(alterEgo, new CharacterRelationshipData(this, alterEgo.owner, alterEgo));
        //}
        //relationships[alterEgo].AddRelationship(newRel);
        //OnRelationshipWithCharacterAdded(alterEgo.owner, newRel);
        //Messenger.Broadcast(Signals.RELATIONSHIP_ADDED, this, newRel);
    }
    //public void RemoveRelationship(Character character) {
    //    if (relationships.ContainsKey(character.currentAlterEgo)) {
    //        List<Trait> traits = relationships[character.currentAlterEgo].rels.Select(x => x as Trait).ToList();
    //        relationships[character.currentAlterEgo].RemoveListeners();
    //        relationships.Remove(character.currentAlterEgo);
    //        RemoveTrait(traits);
    //    }
    //}
    //public bool RemoveRelationship(Character character, RelationshipTrait rel) {
    //    if (relationships.ContainsKey(character.currentAlterEgo)) {
    //        return relationships[character.currentAlterEgo].RemoveRelationship(rel);
    //        //not removing relationship data now when rel count is 0. Because relationship data can contain other data that is still valid even without relationships
    //        //if (relationships[character].rels.Count == 0) {
    //        //    RemoveRelationship(character);
    //        //}
    //    }
    //    return false;
    //}
    public void RemoveRelationshipWith(Character character, RELATIONSHIP_TRAIT rel) {
        //if (relationships.ContainsKey(character.currentAlterEgo)) {
        //    relationships[character.currentAlterEgo].RemoveRelationship(rel);
        //}
        RemoveRelationshipWith(character.currentAlterEgo, rel);
    }
    public void RemoveRelationshipWith(AlterEgoData alterEgo, RELATIONSHIP_TRAIT rel) {
        //if (relationships.ContainsKey(alterEgo)) {
        //    relationships[alterEgo].RemoveRelationship(rel);
        //}
        currentAlterEgo.RemoveRelationship(alterEgo, rel);
    }
    //public void RemoveAllRelationships(bool triggerOnRemove = true) {
    //    List<Character> targetCharacters = relationships.Keys.Select(x => x.owner).ToList();
    //    while (targetCharacters.Count > 0) {
    //        CharacterManager.Instance.RemoveRelationshipBetween(this, targetCharacters[0], triggerOnRemove);
    //        targetCharacters.RemoveAt(0);
    //    }
    //}
    public RelationshipTrait GetRelationshipTraitWith(Character character, RELATIONSHIP_TRAIT type, bool useDisabled = false) {
        return GetRelationshipTraitWith(character.currentAlterEgo, type, useDisabled);
    }
    public RelationshipTrait GetRelationshipTraitWith(AlterEgoData alterEgo, RELATIONSHIP_TRAIT type, bool useDisabled = false) {
        if (HasRelationshipWith(alterEgo, useDisabled)) {
            return relationships[alterEgo].GetRelationshipTrait(type);
        }
        return null;
    }
    public List<RelationshipTrait> GetAllRelationshipTraitWith(Character character) {
        return GetAllRelationshipTraitWith(character.currentAlterEgo);
    }
    public List<RelationshipTrait> GetAllRelationshipTraitWith(AlterEgoData alterEgo) {
        if (HasRelationshipWith(alterEgo)) {
            return relationships[alterEgo].GetAllRelationshipTraits();
        }
        return null;
    }
    public List<RelationshipTrait> GetAllRelationshipOfEffectWith(Character character, TRAIT_EFFECT effect) {
        return GetAllRelationshipOfEffectWith(character.currentAlterEgo, effect);
    }
    public List<RelationshipTrait> GetAllRelationshipOfEffectWith(AlterEgoData alterEgo, TRAIT_EFFECT effect) {
        List<RelationshipTrait> rels = new List<RelationshipTrait>();
        if (HasRelationshipWith(alterEgo)) {
            for (int i = 0; i < relationships[alterEgo].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[alterEgo].rels[i];
                if (currTrait.effect == effect && !currTrait.isDisabled) {
                    rels.Add(currTrait);
                }
            }
        }
        return rels;
    }
    public List<RELATIONSHIP_TRAIT> GetAllRelationshipTraitTypesWith(Character character) {
        return GetAllRelationshipTraitTypesWith(character.currentAlterEgo);
    }
    public List<RELATIONSHIP_TRAIT> GetAllRelationshipTraitTypesWith(AlterEgoData alterEgo) {
        if (HasRelationshipWith(alterEgo)) {
            return new List<RELATIONSHIP_TRAIT>(relationships[alterEgo].rels.Where(x => !x.isDisabled).Select(x => x.relType));
        }
        return null;
    }
    public List<RelationshipTrait> GetAllRelationshipsOfType(Character except, params RELATIONSHIP_TRAIT[] type) {
        List<RelationshipTrait> relationshipTraits = new List<RelationshipTrait>();
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in relationships) {
            if(except != null && kvp.Key.owner == except) {
                continue;
            }
            for (int i = 0; i < type.Length; i++) {
                if (!kvp.Value.isDisabled) {
                    RelationshipTrait relTrait = kvp.Value.GetRelationshipTrait(type[i]);
                    if (relTrait != null) {
                        relationshipTraits.Add(relTrait);
                    }
                }
            }
        }
        return relationshipTraits;
    }
    public List<Character> GetCharactersWithRelationship(params RELATIONSHIP_TRAIT[] type) {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in relationships) {
            for (int i = 0; i < type.Length; i++) {
                if (!kvp.Value.isDisabled && kvp.Value.HasRelationshipTrait(type[i])) {
                    if (characters.Contains(kvp.Key.owner)) {
                        continue;
                    }
                    characters.Add(kvp.Key.owner);
                }
            }
        }
        return characters;
    }
    public List<Character> GetViableCharactersWithRelationship(GENDER gender, params RELATIONSHIP_TRAIT[] type) {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in relationships) {
            for (int i = 0; i < type.Length; i++) {
                if (!kvp.Value.isDisabled && kvp.Value.HasRelationshipTrait(type[i])) {
                    if (kvp.Key.owner.isDead ||  kvp.Key.owner.gender != gender || kvp.Key.owner.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER) || kvp.Key.owner.HasTraitOf(TRAIT_TYPE.CRIMINAL) || characters.Contains(kvp.Key.owner)) {
                        continue;
                    }
                    characters.Add(kvp.Key.owner);
                }
            }
        }
        return characters;
    }
    public List<Character> GetCharactersWithoutRelationship(RELATIONSHIP_TRAIT type) {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in relationships) {
            if (!kvp.Value.isDisabled && !kvp.Value.HasRelationshipTrait(type)) {
                if (characters.Contains(kvp.Key.owner)) {
                    continue;
                }
                characters.Add(kvp.Key.owner);
            }
        }
        return characters;
    }
    public List<Character> GetCharactersWithRelationship(TRAIT_EFFECT effect) {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in relationships) {
            if (!kvp.Value.isDisabled && kvp.Value.HasRelationshipOfEffect(effect)) {
                if (characters.Contains(kvp.Key.owner)) {
                    continue;
                }
                characters.Add(kvp.Key.owner);
            }
        }
        return characters;
    }
    public Character GetCharacterWithRelationship(params RELATIONSHIP_TRAIT[] type) {
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in relationships) {
            for (int i = 0; i < type.Length; i++) {
                if (!kvp.Value.isDisabled && kvp.Value.HasRelationshipTrait(type[i])) {
                    return kvp.Key.owner;
                }
            }
        }
        return null;
    }
    public List<Character> GetAllCharactersThatHasRelationship() {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in relationships) {
            if (!kvp.Value.isDisabled) {
                if (characters.Contains(kvp.Key.owner)) {
                    continue;
                }
                characters.Add(kvp.Key.owner);
            }
        }
        return characters;
    }
    public bool CanHaveRelationshipWith(RELATIONSHIP_TRAIT type, Character target) {
        //NOTE: This is only one way checking. This character will only check itself, if he/she meets the requirements of a given relationship
        if (target.characterClass.className == "Zombie" || this.characterClass.className == "Zombie") {
            return false; //Zombies cannot create relationships
        }
        List<RELATIONSHIP_TRAIT> relationshipsWithTarget = GetAllRelationshipTraitTypesWith(target);
        //if(relationshipsWithTarget == null) { return true; }
        switch (type) {
            case RELATIONSHIP_TRAIT.ENEMY:
                return relationshipsWithTarget == null || (relationshipsWithTarget != null && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.ENEMY) && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.FRIEND) && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.LOVER)); //check that the target character is not already this characters enemy and that this character is also not his friend or his lover
            case RELATIONSHIP_TRAIT.FRIEND:
                return relationshipsWithTarget == null || (relationshipsWithTarget != null && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.FRIEND) && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.ENEMY)); //check that the target character is not already this characters friend and that this character is also not his enemy
            case RELATIONSHIP_TRAIT.LOVER:
                //- **Lover:** Positive, Permanent (Can only have 1)
                //check if this character already has a lover and that the target character is not his/her paramour
                if (GetCharacterWithRelationship(type) != null) {
                    return false;
                }
                if (relationshipsWithTarget != null && 
                    (relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.PARAMOUR) || relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.ENEMY))) {
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
                //Comment Reason: Allowed multiple paramours
                //if (GetCharacterWithRelationship(type) != null) {
                //    return false;
                //}
                if (relationshipsWithTarget != null && relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.LOVER)) {
                    return false;
                }
                //one of the characters must have a lover
                if (!target.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER) && !HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER)) {
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
    internal void OnRelationshipWithCharacterAdded(Character targetCharacter, RelationshipTrait newRel) {
#if !WORLD_CREATION_TOOL
         //check if they share the same home, then migrate them accordingly
        if (newRel.relType == RELATIONSHIP_TRAIT.LOVER
            && this.homeArea.id == targetCharacter.homeArea.id
            && this.homeStructure != targetCharacter.homeStructure) {
            //Lover conquers all, even if one character is factionless they will be together, meaning the factionless character will still have home structure
            homeArea.AssignCharacterToDwellingInArea(this, targetCharacter.homeStructure);
        }
#endif
    }
    /// <summary>
    /// Does this character have a relationship of effect with the provided character?
    /// </summary>
    /// <param name="character">Other character.</param>
    /// <param name="effect">Relationship effect (Positive, Negative, Neutral)</param>
    /// <param name="include">Relationship type to exclude from checking</param>
    /// <returns></returns>
    public bool HasRelationshipOfEffectWith(Character character, TRAIT_EFFECT effect, RELATIONSHIP_TRAIT include = RELATIONSHIP_TRAIT.NONE) {
        return HasRelationshipOfEffectWith(character.currentAlterEgo, effect, include);
    }
    public bool HasRelationshipOfEffectWith(AlterEgoData alterEgo, TRAIT_EFFECT effect, RELATIONSHIP_TRAIT include = RELATIONSHIP_TRAIT.NONE) {
        if (HasRelationshipWith(alterEgo)) {
            for (int i = 0; i < relationships[alterEgo].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[alterEgo].rels[i];
                if ((currTrait.effect == effect || currTrait.relType == include) && !currTrait.isDisabled) {
                    return true;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// Does this character have any relationship type with a target character among the given relationship types.
    /// </summary>
    /// <param name="character">The target character.</param>
    /// <param name="useDisabled">Use disabled relationships?</param>
    /// <param name="objs">The list of relationships.</param>
    /// <returns>true or false.</returns>
    public bool HasAnyRelationshipOfTypeWith(Character character, bool useDisabled = false, params RELATIONSHIP_TRAIT[] objs) {
        return HasAnyRelationshipOfTypeWith(character.currentAlterEgo, useDisabled, objs);
    }
    public bool HasAnyRelationshipOfTypeWith(AlterEgoData alterEgo, bool useDisabled = false, params RELATIONSHIP_TRAIT[] objs) {
        if (HasRelationshipWith(alterEgo, useDisabled)) {
            List<RELATIONSHIP_TRAIT> rels = objs.ToList();
            for (int i = 0; i < relationships[alterEgo].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[alterEgo].rels[i];
                if (rels.Contains(currTrait.relType)) {
                    if (!currTrait.isDisabled || (currTrait.isDisabled && useDisabled)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfTypeWith(Character character, RELATIONSHIP_TRAIT relType, bool useDisabled = false) {
        return HasRelationshipOfTypeWith(character.currentAlterEgo, relType, useDisabled);
    }
    public bool HasRelationshipOfTypeWith(AlterEgoData alterEgo, RELATIONSHIP_TRAIT relType, bool useDisabled = false) {
        if (HasRelationshipWith(alterEgo, useDisabled)) {
            for (int i = 0; i < relationships[alterEgo].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[alterEgo].rels[i];
                if (currTrait.relType == relType && !currTrait.isDisabled) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfTypeWith(Character character, bool useDisabled = false, params RELATIONSHIP_TRAIT[] rels) {
        return HasRelationshipOfTypeWith(character.currentAlterEgo, useDisabled, rels);
    }
    public bool HasRelationshipOfTypeWith(AlterEgoData alterEgo, bool useDisabled = false, params RELATIONSHIP_TRAIT[] rels) {
        if (HasRelationshipWith(alterEgo)) {
            for (int i = 0; i < relationships[alterEgo].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[alterEgo].rels[i];
                if (!useDisabled && currTrait.isDisabled) {
                    continue; //skip
                }
                for (int j = 0; j < rels.Length; j++) {
                    if (rels[j] == currTrait.relType) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public CharacterRelationshipData GetCharacterRelationshipData(Character character) {
        return GetCharacterRelationshipData(character.currentAlterEgo);
    }
    public CharacterRelationshipData GetCharacterRelationshipData(AlterEgoData alterEgo) {
        if (HasRelationshipWith(alterEgo)) {
            return relationships[alterEgo];
        }
        return null;
    }
    public bool HasRelationshipWith(Character character, bool useDisabled = false) {
        return HasRelationshipWith(character.currentAlterEgo, useDisabled);
    }
    public bool HasRelationshipWith(AlterEgoData alterEgo, bool useDisabled = false) {
        return currentAlterEgo.HasRelationshipWith(alterEgo, useDisabled);
    }
    public bool HasTwoWayRelationshipWith(Character character, bool useDisabled = false) {
        return HasRelationshipWith(character.currentAlterEgo, useDisabled) && character.HasRelationshipWith(this.currentAlterEgo, useDisabled);
    }
    public int GetAllRelationshipCountExcept(List<RELATIONSHIP_TRAIT> except = null) {
        int count = 0;
        for (int i = 0; i < relationshipTraits.Count; i++) {
            if (relationshipTraits[i] is RelationshipTrait) {
                RelationshipTrait relTrait = relationshipTraits[i] as RelationshipTrait;
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
    public void FlirtWith(Character otherCharacter) {
        if (!relationships.ContainsKey(otherCharacter.currentAlterEgo)) {
            relationships.Add(otherCharacter.currentAlterEgo, new CharacterRelationshipData(this, otherCharacter, otherCharacter.currentAlterEgo));
        }
        relationships[otherCharacter.currentAlterEgo].IncreaseFlirtationCount();
    }
    /// <summary>
    /// Get the type of relationship that this character has with the other character.
    /// </summary>
    /// <param name="otherCharacter">Character to check</param>
    /// <param name="useDisabled">Should this checking use disabled relationships?</param>
    /// <returns>POSITIVE, NEGATIVE, NONE</returns>
    public RELATIONSHIP_EFFECT GetRelationshipEffectWith(Character otherCharacter, bool useDisabled = false) {
        return GetRelationshipEffectWith(otherCharacter.currentAlterEgo, useDisabled);
    }
    public RELATIONSHIP_EFFECT GetRelationshipEffectWith(AlterEgoData alterEgo, bool useDisabled = false) {
        if (HasRelationshipWith(alterEgo, useDisabled)) {
            for (int i = 0; i < relationships[alterEgo].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[alterEgo].rels[i];
                if(currTrait.relType == RELATIONSHIP_TRAIT.LOVER || currTrait.relType == RELATIONSHIP_TRAIT.PARAMOUR) {
                    if(jobQueue.HasJob(JOB_TYPE.BREAK_UP, alterEgo.owner)) {
                        continue;
                    }
                }
                if (currTrait.effect == TRAIT_EFFECT.NEGATIVE) {
                    return RELATIONSHIP_EFFECT.NEGATIVE;
                }
            }
            return RELATIONSHIP_EFFECT.POSITIVE;
        }
        return RELATIONSHIP_EFFECT.NONE;
    }
    #endregion

    #region History/Logs
    public bool AddHistory(Log log) {
        if (!_history.Contains(log)) {
            _history.Add(log);
            //if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && this.id == UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id) {
            //    Debug.Log("Added log to history of " + this.name + ". " + log.isInspected);
            //}
            if (this._history.Count > 300) {
                if (this._history[0].goapAction != null) {
                    this._history[0].goapAction.AdjustReferenceCount(-1);
                }
                this._history.RemoveAt(0);
            }
            if(log.goapAction != null) {
                log.goapAction.AdjustReferenceCount(1);
            }
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
            return true;
        }
        return false;
    }
    //Add log to this character and show notif of that log only if this character is clicked or tracked, otherwise, add log only
    public void RegisterLogAndShowNotifToThisCharacterOnly(string fileName, string key, object target = null, string targetName = "", GoapAction goapAction = null, bool onlyClickedCharacter = true) {
        if (!GameManager.Instance.gameHasStarted) {
            return;
        }
        if (key == "remove_trait" && isDead) {
            return;
        }
        Log addLog = new Log(GameManager.Instance.Today(), "Character", fileName, key, goapAction);
        addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        if (targetName != "") {
            addLog.AddToFillers(target, targetName, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        addLog.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFrom(addLog, this, onlyClickedCharacter);
    }
    public void RegisterLogAndShowNotifToThisCharacterOnly(Log addLog, GoapAction goapAction = null, bool onlyClickedCharacter = true) {
        if (!GameManager.Instance.gameHasStarted) {
            return;
        }
        addLog.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFrom(addLog, this, onlyClickedCharacter);
    }
    public virtual void OnActionStateSet(GoapAction action, GoapActionState state) {
        //IPointOfInterest target = null;
        //if (action.goapType == INTERACTION_TYPE.MAKE_LOVE) {
        //    target = (action as MakeLove).targetCharacter;
        //} else {
        //    target = action.poiTarget;
        //}
        //if (action.actor != this && target != this) {
        //    if (action.goapType == INTERACTION_TYPE.WATCH) {
        //        //Cannot witness/watch a watch action
        //        return;
        //    }
        //    if (GetNormalTrait("Unconscious", "Resting") != null) {
        //        return;
        //    }
        //    if (marker.inVisionCharacters.Contains(action.actor)) {
        //        ThisCharacterWitnessedEvent(action);
        //        ThisCharacterWatchEvent(null, action, state);
        //    }
        //}
        ///Moved all needed checking <see cref="CharacterManager.OnActionStateSet(GoapAction, GoapActionState)"/>
        ThisCharacterWitnessedEvent(action);
        //ThisCharacterWatchEvent(null, action, state);
    }

    //Returns the list of goap actions to be witnessed by this character
    public virtual List<GoapAction> ThisCharacterSaw(IPointOfInterest target) {
        if (GetNormalTrait("Unconscious", "Resting") != null || isDead) {
            return null;
        }
        
        if (currentAction != null && currentAction.poiTarget == target && currentAction.isStealth) {
            //Upon seeing the target while performing a stealth job action, check if it can do the action
            if (!marker.CanDoStealthActionToTarget(target)) {
                currentAction.parentPlan.job.jobQueueParent.CancelJob(currentAction.parentPlan.job);
            }
        }
        if (target.targettedByAction.Count > 0) {
            //Upon seeing a character and that character is being targetted by a stealth job and the actor of that job is not this one, and the actor is performing that job and the actor already sees the target
            List<GoapAction> allActionsTargettingTargetCharacter = new List<GoapAction>(target.targettedByAction);
            for (int i = 0; i < allActionsTargettingTargetCharacter.Count; i++) {
                GoapAction action = allActionsTargettingTargetCharacter[i];
                if (action.isStealth && action.actor != this) {
                    if (!action.isDone && !action.isPerformingActualAction && action.actor.currentAction == action
                        && action.actor.marker.inVisionPOIs.Contains(target)) {
                        action.StopAction(true);
                    }
                }
            }
        }
        if (target is Character) {
            Character targetCharacter = target as Character;
            targetCharacter.OnSeenBy(this); //trigger that the target character was seen by this character.
            
            for (int i = 0; i < normalTraits.Count; i++) {
                normalTraits[i].OnSeePOI(target, this);
            }

            List<GoapAction> actionsToWitness = new List<GoapAction>();
            if (target.targettedByAction.Count > 0) {
                //Collect all actions first to avoid duplicates 
                actionsToWitness.AddRange(target.targettedByAction);
            }
            //Must not witness/watch if spooked
            if (targetCharacter.currentAction != null && targetCharacter.currentAction.isPerformingActualAction && !targetCharacter.currentAction.isDone && targetCharacter.currentAction.goapType != INTERACTION_TYPE.WATCH) {
                //Cannot witness/watch a watch action
                IPointOfInterest poiTarget = null;
                if (targetCharacter.currentAction.goapType == INTERACTION_TYPE.MAKE_LOVE) {
                    poiTarget = (targetCharacter.currentAction as MakeLove).targetCharacter;
                } else {
                    poiTarget = targetCharacter.currentAction.poiTarget;
                }
                if (targetCharacter.currentAction.actor != this && poiTarget != this) {
                    actionsToWitness.Add(targetCharacter.currentAction);
                    //ThisCharacterWitnessedEvent(targetCharacter.currentAction);
                    //ThisCharacterWatchEvent(targetCharacter, targetCharacter.currentAction, targetCharacter.currentAction.currentState);
                }
            }
            //This will only happen if target is in combat
            ThisCharacterWatchEvent(targetCharacter, null, null);
            return actionsToWitness;
        } else {
            for (int i = 0; i < normalTraits.Count; i++) {
                normalTraits[i].OnSeePOI(target, this);
            }
        }
        return null;
    }
    /// <summary>
    /// What should happen if another character sees this character?
    /// </summary>
    /// <param name="character">The character that saw this character.</param>
    protected virtual void OnSeenBy(Character character) { }
    public List<Log> GetMemories(int dayFrom, int dayTo, bool eventMemoriesOnly = false){
        List<Log> memories = new List<Log>();
        if (eventMemoriesOnly) {
            for (int i = 0; i < _history.Count; i++) {
                if (_history[i].goapAction != null) {
                    if (_history[i].day >= dayFrom && _history[i].day <= dayTo) {
                        memories.Add(_history[i]);
                    }
                }
            }
        } else {
            for (int i = 0; i < _history.Count; i++) {
                if(_history[i].day >= dayFrom && _history[i].day <= dayTo) {
                    memories.Add(_history[i]);
                }
            }
        }
        return memories;
    }
    public List<Log> GetWitnessOrInformedMemories(int dayFrom, int dayTo, Character involvedCharacter = null) {
        List<Log> memories = new List<Log>();
        for (int i = 0; i < _history.Count; i++) {
            Log historyLog = _history[i];
            if (historyLog.goapAction != null && (historyLog.key == "witness_event" || historyLog.key == "informed_event")) {
                if (historyLog.day >= dayFrom && historyLog.day <= dayTo) {
                    if(involvedCharacter != null) {
                        if (historyLog.goapAction.actor == involvedCharacter || historyLog.goapAction.IsTarget(involvedCharacter)) {
                            memories.Add(historyLog);
                        }
                    } else {
                        memories.Add(historyLog);
                    }
                }
            }
        }
        return memories;
    }
    public List<Log> GetCrimeMemories(int dayFrom, int dayTo, Character involvedCharacter = null) {
        List<Log> memories = new List<Log>();
        for (int i = 0; i < _history.Count; i++) {
            Log historyLog = _history[i];
            if (historyLog.goapAction != null && historyLog.goapAction.awareCharactersOfThisAction.Contains(this) && historyLog.goapAction.committedCrime != CRIME.NONE) {
                if (historyLog.day >= dayFrom && historyLog.day <= dayTo) {
                    if(involvedCharacter != null) {
                        for (int j = 0; j < historyLog.goapAction.crimeCommitters.Length; j++) {
                            Character criminal = historyLog.goapAction.crimeCommitters[j];
                            if (criminal == involvedCharacter) {
                                memories.Add(historyLog);
                                break;
                            }
                        }
                    } else {
                        memories.Add(historyLog);
                    }
                }
            }
        }
        return memories;
    }
    public void CreateInformedEventLog(GoapAction eventToBeInformed, bool invokeShareIntelReaction) {
        Log informedLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "informed_event", eventToBeInformed);
        informedLog.AddToFillers(eventToBeInformed.currentState.descriptionLog.fillers);
        informedLog.AddToFillers(this, this.name, LOG_IDENTIFIER.OTHER);
        informedLog.AddToFillers(null, Utilities.LogDontReplace(eventToBeInformed.currentState.descriptionLog), LOG_IDENTIFIER.APPEND);
        AddHistory(informedLog);

        if (invokeShareIntelReaction) {
            if (eventToBeInformed.currentState.shareIntelReaction != null) {
                eventToBeInformed.currentState.shareIntelReaction.Invoke(this, null, SHARE_INTEL_STATUS.INFORMED);
            }
        }
        eventToBeInformed.AddAwareCharacter(this);

        ////If a character sees or informed about a lover performing Making Love or Ask to Make Love, they will feel Betrayed
        //if (eventToBeInformed.actor != this && !eventToBeInformed.IsTarget(this)) {
        //    Character target = eventToBeInformed.poiTarget as Character;
        //    if (eventToBeInformed.goapType == INTERACTION_TYPE.MAKE_LOVE) {
        //        target = (eventToBeInformed as MakeLove).targetCharacter; //NOTE: Changed this, because technically the Make Love Action targets the bed, and the target character is stored in the event itself.
        //        if (HasRelationshipOfTypeWith(eventToBeInformed.actor, RELATIONSHIP_TRAIT.LOVER) || HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
        //            Betrayed betrayed = new Betrayed();
        //            AddTrait(betrayed);
        //            CharacterManager.Instance.RelationshipDegradation(eventToBeInformed.actor, this, eventToBeInformed);
        //            CharacterManager.Instance.RelationshipDegradation(target, this, eventToBeInformed);
        //        }
        //    } else if (eventToBeInformed.goapType == INTERACTION_TYPE.INVITE_TO_MAKE_LOVE) {
        //        if (HasRelationshipOfTypeWith(eventToBeInformed.actor, RELATIONSHIP_TRAIT.LOVER)) {
        //            Betrayed betrayed = new Betrayed();
        //            AddTrait(betrayed);
        //            CharacterManager.Instance.RelationshipDegradation(eventToBeInformed.actor, this, eventToBeInformed);
        //            CharacterManager.Instance.RelationshipDegradation(target, this, eventToBeInformed);
        //        } else if (HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
        //            if (eventToBeInformed.currentState.name == "Invite Success") {
        //                Betrayed betrayed = new Betrayed();
        //                AddTrait(betrayed);
        //                CharacterManager.Instance.RelationshipDegradation(eventToBeInformed.actor, this, eventToBeInformed);
        //                CharacterManager.Instance.RelationshipDegradation(target, this, eventToBeInformed);
        //            }
        //        }
        //    }
        //}
    }
    public void ThisCharacterWitnessedEvent(GoapAction witnessedEvent) {
        if (witnessedEvent.currentState == null) {
            throw new System.Exception(GameManager.Instance.TodayLogString() + this.name + " witnessed event " + witnessedEvent.goapName + " by " + witnessedEvent.actor.name + " but it does not have a current state!");
        }
        Log witnessLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "witness_event", witnessedEvent);
        witnessLog.AddToFillers(this, name, LOG_IDENTIFIER.OTHER);
        witnessLog.AddToFillers(null, Utilities.LogDontReplace(witnessedEvent.currentState.descriptionLog), LOG_IDENTIFIER.APPEND);
        witnessLog.AddToFillers(witnessedEvent.currentState.descriptionLog.fillers);
        AddHistory(witnessLog);

        if(faction == PlayerManager.Instance.player.playerFaction) {
            //Player characters cannot react to witnessed events
            return;
        }
        if (witnessedEvent.currentState.shareIntelReaction != null && !isFactionless) {
            List<string> reactions = witnessedEvent.currentState.shareIntelReaction.Invoke(this, null, SHARE_INTEL_STATUS.WITNESSED);
            if(reactions != null) {
                string reactionLog = name + " witnessed event: " + witnessedEvent.goapName;
                reactionLog += "\nREACTION:";
                for (int i = 0; i < reactions.Count; i++) {
                    reactionLog += "\n" + reactions[i];
                }
                PrintLogIfActive(reactionLog);
            }
        }

        witnessedEvent.AddAwareCharacter(this);

        //If a character sees or informed about a lover performing Making Love or Ask to Make Love, they will feel Betrayed
        if (witnessedEvent.actor != this && !witnessedEvent.IsTarget(this)) {
            Character target = witnessedEvent.poiTarget as Character;
            if (witnessedEvent.goapType == INTERACTION_TYPE.MAKE_LOVE) {
                target = (witnessedEvent as MakeLove).targetCharacter;
                if (HasRelationshipOfTypeWith(witnessedEvent.actor, RELATIONSHIP_TRAIT.LOVER) || HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
                    Betrayed betrayed = new Betrayed();
                    AddTrait(betrayed);
                    //CharacterManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
                    //CharacterManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
                } 
            } else if (witnessedEvent.goapType == INTERACTION_TYPE.INVITE_TO_MAKE_LOVE) {
                if (HasRelationshipOfTypeWith(witnessedEvent.actor, RELATIONSHIP_TRAIT.LOVER)) {
                    Betrayed betrayed = new Betrayed();
                    AddTrait(betrayed);
                    //CharacterManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
                    //CharacterManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
                } else if (HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
                    if (witnessedEvent.currentState.name == "Invite Success") {
                        Betrayed betrayed = new Betrayed();
                        AddTrait(betrayed);
                        //CharacterManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
                        //CharacterManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
                    }
                }
            }
        }
    }
    /// <summary>
    /// This character watched an action happen.
    /// </summary>
    /// <param name="targetCharacter">The character that was performing the action.</param>
    /// <param name="action">The action that was performed.</param>
    /// <param name="state">The state the action was in when this character watched it.</param>
    public void ThisCharacterWatchEvent(Character targetCharacter, GoapAction action, GoapActionState state) {
        if(faction == PlayerManager.Instance.player.playerFaction) {
            //Player characters cannot watch events
            return;
        }
        if (action == null) {
            if (targetCharacter != null && targetCharacter.stateComponent.currentState != null && !targetCharacter.stateComponent.currentState.isDone && targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT
                && targetCharacter.faction == faction) {
                CombatState targetCombatState = targetCharacter.stateComponent.currentState as CombatState;
                if (targetCombatState.currentClosestHostile != null && targetCombatState.currentClosestHostile != this) {
                    Character currentHostileOfTargetCharacter = targetCombatState.currentClosestHostile;
                    if(currentHostileOfTargetCharacter.stateComponent.currentState != null 
                        && !currentHostileOfTargetCharacter.stateComponent.currentState.isDone 
                        && currentHostileOfTargetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                        CombatState combatStateOfCurrentHostileOfTargetCharacter = currentHostileOfTargetCharacter.stateComponent.currentState as CombatState;
                        if(combatStateOfCurrentHostileOfTargetCharacter.currentClosestHostile != null 
                            && combatStateOfCurrentHostileOfTargetCharacter.currentClosestHostile == this) {
                            //If character 1 is supposed to watch/join the combat of character 2 against character 3
                            //but character 3 is already in combat and his current target is already character 1
                            //then character 1 should not react
                            return;
                        }
                    }

                    Invisible invisible = targetCombatState.currentClosestHostile.GetNormalTrait("Invisible") as Invisible;
                    if (invisible != null && !invisible.charactersThatCanSee.Contains(this)) {
                        CreateWatchEvent(null, targetCombatState, targetCharacter);
                    } else {
                        if (targetCombatState.currentClosestHostile.faction == faction) {
                            RELATIONSHIP_EFFECT relEffectTowardsTarget = GetRelationshipEffectWith(targetCharacter);
                            RELATIONSHIP_EFFECT relEffectTowardsTargetOfCombat = GetRelationshipEffectWith(targetCombatState.currentClosestHostile);

                            if (relEffectTowardsTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                                if (relEffectTowardsTargetOfCombat == RELATIONSHIP_EFFECT.POSITIVE) {
                                    CreateWatchEvent(null, targetCombatState, targetCharacter);
                                } else {
                                    if (marker.AddHostileInRange(targetCombatState.currentClosestHostile, false, false, isLethal: targetCharacter.marker.IsLethalCombatForTarget(targetCombatState.currentClosestHostile))) {
                                        if (!marker.avoidInRange.Contains(targetCharacter)) {
                                            //TODO: Do process combat behavior first for this character, if the current closest hostile
                                            //of the combat state of this character is also the targetCombatState.currentClosestHostile
                                            //Then that's only when we apply the join combat log and notif
                                            //Because if not, it means that this character is already in combat with someone else, and thus
                                            //should not product join combat log anymore
                                            List<RELATIONSHIP_TRAIT> rels = GetAllRelationshipTraitTypesWith(targetCharacter).OrderByDescending(x => (int) x).ToList(); //so that the first relationship to be returned is the one with higher importance.
                                            Log joinLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "join_combat");
                                            joinLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                            joinLog.AddToFillers(targetCombatState.currentClosestHostile, targetCombatState.currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                                            joinLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
                                            joinLog.AddToFillers(null, Utilities.NormalizeString(rels.First().ToString()), LOG_IDENTIFIER.STRING_1);
                                            joinLog.AddLogToSpecificObjects(LOG_IDENTIFIER.ACTIVE_CHARACTER, LOG_IDENTIFIER.TARGET_CHARACTER);
                                            PlayerManager.Instance.player.ShowNotification(joinLog);
                                        }
                                        //marker.ProcessCombatBehavior();
                                    }
                                }
                            } else {
                                if (relEffectTowardsTargetOfCombat == RELATIONSHIP_EFFECT.POSITIVE) {
                                    if (marker.AddHostileInRange(targetCharacter, false, false, isLethal: targetCharacter.marker.IsLethalCombatForTarget(targetCombatState.currentClosestHostile))) {
                                        if (!marker.avoidInRange.Contains(targetCharacter)) {
                                            //TODO: Do process combat behavior first for this character, if the current closest hostile
                                            //of the combat state of this character is also the targetCombatState.currentClosestHostile
                                            //Then that's only when we apply the join combat log and notif
                                            //Because if not, it means that this character is already in combat with someone else, and thus
                                            //should not product join combat log anymore
                                            List<RELATIONSHIP_TRAIT> rels = GetAllRelationshipTraitTypesWith(targetCombatState.currentClosestHostile).OrderByDescending(x => (int) x).ToList(); //so that the first relationship to be returned is the one with higher importance.
                                            Log joinLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "join_combat");
                                            joinLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                            joinLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                                            joinLog.AddToFillers(targetCombatState.currentClosestHostile, targetCombatState.currentClosestHostile.name, LOG_IDENTIFIER.CHARACTER_3);
                                            joinLog.AddToFillers(null, Utilities.NormalizeString(rels.First().ToString()), LOG_IDENTIFIER.STRING_1);
                                            joinLog.AddLogToSpecificObjects(LOG_IDENTIFIER.ACTIVE_CHARACTER, LOG_IDENTIFIER.TARGET_CHARACTER);
                                            PlayerManager.Instance.player.ShowNotification(joinLog);
                                        }
                                        //marker.ProcessCombatBehavior();
                                    }
                                } else {
                                    CreateWatchEvent(null, targetCombatState, targetCharacter);
                                }
                            }
                        } else {
                            //the target of the combat state is not part of this character's faction
                            if (marker.AddHostileInRange(targetCombatState.currentClosestHostile, false, false, isLethal: targetCharacter.marker.IsLethalCombatForTarget(targetCombatState.currentClosestHostile))) {
                                if (!marker.avoidInRange.Contains(targetCharacter)) {
                                    //TODO: Do process combat behavior first for this character, if the current closest hostile
                                    //of the combat state of this character is also the targetCombatState.currentClosestHostile
                                    //Then that's only when we apply the join combat log and notif
                                    //Because if not, it means that this character is already in combat with someone else, and thus
                                    //should not product join combat log anymore
                                    Log joinLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "join_combat_faction");
                                    joinLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                    joinLog.AddToFillers(targetCombatState.currentClosestHostile, targetCombatState.currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                                    joinLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
                                    joinLog.AddLogToSpecificObjects(LOG_IDENTIFIER.ACTIVE_CHARACTER, LOG_IDENTIFIER.TARGET_CHARACTER);
                                    PlayerManager.Instance.player.ShowNotification(joinLog);
                                }
                                //marker.ProcessCombatBehavior();
                            }
                        }
                    }
                }
            }
        } 
        //else if (!action.isDone) {
        //    if (action.goapType == INTERACTION_TYPE.MAKE_LOVE && state.name == "Make Love Success") {
        //        MakeLove makeLove = action as MakeLove;
        //        Character target = makeLove.targetCharacter;
        //        if (HasRelationshipOfTypeWith(action.actor, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
        //            CreateWatchEvent(action, null, action.actor);
        //        } else if (HasRelationshipOfTypeWith(target, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
        //            CreateWatchEvent(action, null, target);
        //        } else {
        //            marker.AddAvoidInRange(action.actor, false);
        //            marker.AddAvoidInRange(target);
        //        }
        //    } else if (action.goapType == INTERACTION_TYPE.PLAY_GUITAR && state.name == "Play Success" && GetNormalTrait("MusicHater") == null) {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        if (chance < 25) { //25
        //            if (!HasRelationshipOfTypeWith(action.actor, RELATIONSHIP_TRAIT.ENEMY)) {
        //                CreateWatchEvent(action, null, action.actor);
        //            }
        //        }
        //    } else if (action.goapType == INTERACTION_TYPE.TABLE_POISON) {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        if (chance < 35) {
        //            CreateWatchEvent(action, null, action.actor);
        //        }
        //    } else if (action.goapType == INTERACTION_TYPE.CURSE_CHARACTER && state.name == "Curse Success") {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        if (chance < 35) {
        //            CreateWatchEvent(action, null, action.actor);
        //        }
        //    } else if ((action.goapType == INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM || action.goapType == INTERACTION_TYPE.REVERT_TO_NORMAL_FORM) && state.name == "Transform Success") {
        //        if (faction == action.actor.faction) {
        //            CreateWatchEvent(action, null, action.actor);
        //        }
        //    }
        //}
    }
    //In watch event, it's either the character watch an action or combat state, it cannot be both. (NOTE: Since 9/2/2019 Enabled watching of other states other than Combat)
    public void CreateWatchEvent(GoapAction actionToWatch, CharacterState stateToWatch, Character targetCharacter) {
        string summary = "Creating watch event for " + name + " with target " + targetCharacter.name;
        if(actionToWatch != null) {
            summary += " involving " + actionToWatch.goapName;
        }else if (stateToWatch != null) {
            if (stateToWatch is CombatState) {
                summary += " involving Combat";
            } else if (stateToWatch is DouseFireState) {
                summary += " involving Douse Fire";
            }
            
        }
        if (currentAction != null && !currentAction.isDone && currentAction.goapType == INTERACTION_TYPE.WATCH) {
            summary += "\n-Already watching an action, will not watch another one...";
            PrintLogIfActive(summary);
            return;
        }
        if (stateComponent.currentState != null && (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT || stateComponent.currentState.characterState == CHARACTER_STATE.BERSERKED || stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE)) {
            summary += "\n-In combat state/berserked state/douse fire state, must not watch...";
            PrintLogIfActive(summary);
            return;
        }
        if (HasPlanWithType(INTERACTION_TYPE.WATCH)) {
            summary += "\n-Already has watch action in queue, will not watch another one...";
            PrintLogIfActive(summary);
            return;
        }
        int watchJobPriority = InteractionManager.Instance.GetInitialPriority(JOB_TYPE.WATCH);
        if (stateComponent.currentState != null && stateComponent.currentState.job != null && stateComponent.currentState.job.priority <= watchJobPriority) {
            summary += "\n-Current state job " + stateComponent.currentState.job.name + " priority: " + stateComponent.currentState.job.priority + " is higher or equal than Watch Job priority " + watchJobPriority + ", will not watch...";
            PrintLogIfActive(summary);
            return;
        } else if (stateComponent.stateToDo != null && stateComponent.stateToDo.job != null && stateComponent.stateToDo.job.priority <= watchJobPriority) {
            summary += "\n-State to do job " + stateComponent.stateToDo.job.name + " priority: " + stateComponent.stateToDo.job.priority + " is higher or equal than Watch Job priority " + watchJobPriority + ", will not watch...";
            PrintLogIfActive(summary);
            return;
        } else if (currentAction != null && currentAction.parentPlan != null && currentAction.parentPlan.job != null && currentAction.parentPlan.job.priority <= watchJobPriority) {
            summary += "\n-Current action job " + currentAction.parentPlan.job.name + " priority: " + currentAction.parentPlan.job.priority + " is higher or equal than Watch Job priority " + watchJobPriority + ", will not watch...";
            PrintLogIfActive(summary);
            return;
        }
        //if (stateComponent.currentState != null) {
        //    summary += "\nEnding current state " + stateComponent.currentState.stateName + " before watching...";
        //    stateComponent.currentState.OnExitThisState();
        //    //This call is doubled so that it will also exit the previous major state if there's any
        //    if (stateComponent.currentState != null) {
        //        stateComponent.currentState.OnExitThisState();
        //    }
        //} else if (stateComponent.stateToDo != null) {
        //    summary += "\nEnding state to do " + stateComponent.stateToDo.stateName + " before watching...";
        //    stateComponent.SetStateToDo(null);
        //} else {
        //    if (currentParty.icon.isTravelling) {
        //        summary += "\nStopping movement before watching...";
        //        if (currentParty.icon.travelLine == null) {
        //            marker.StopMovement();
        //        } else {
        //            currentParty.icon.SetOnArriveAction(() => OnArriveAtAreaStopMovement());
        //        }
        //    }
        //    summary += "\nEnding current action (if there's any) before watching...";
        //    AdjustIsWaitingForInteraction(1);
        //    StopCurrentAction(false);
        //    AdjustIsWaitingForInteraction(-1);
        //}
        summary += "\nWatch event created.";
        PrintLogIfActive(summary);
        Watch watchAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.WATCH, this, targetCharacter) as Watch;
        if (actionToWatch != null) {
            watchAction.InitializeOtherData(new object[] { actionToWatch });
        } else if (stateToWatch != null) {
            watchAction.InitializeOtherData(new object[] { stateToWatch });
        }
        GoapNode goalNode = new GoapNode(null, watchAction.cost, watchAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.WATCH, goapPlan, this);
        goapPlan.ConstructAllNodes();
        goapPlan.SetDoNotRecalculate(true);
        job.SetCancelOnFail(true);
        job.SetCancelJobOnDropPlan(true);

        jobQueue.AddJobInQueue(job, false);
        jobQueue.AssignCharacterToJobAndCancelCurrentAction(job, this);
        //AddPlan(goapPlan, true);
    }
    #endregion

    #region Combat Handlers
    public void SetCombatCharacter(CombatCharacter combatCharacter) {
        _currentCombatCharacter = combatCharacter;
    }
    public void OnHitByAttackFrom(Character characterThatAttacked, CombatState state, ref string attackSummary) {
        GameManager.Instance.CreateHitEffectAt(this);
        if (this.currentHP <= 0) {
            return; //if hp is already 0, do not deal damage
        }

        //If someone is attacked, relationship should deteriorate
        if (characterThatAttacked.stateComponent.currentState != null && characterThatAttacked.stateComponent.currentState is CombatState) {
            CombatState combat = characterThatAttacked.stateComponent.currentState as CombatState;
            if(combat.currentClosestHostile == this && !combat.allCharactersThatDegradedRel.Contains(this)) {
                CharacterManager.Instance.RelationshipDegradation(characterThatAttacked, this);
                combat.AddCharacterThatDegradedRel(this);
            }
        }

        //TODO: For readjustment, attack power is the old computation
        this.AdjustHP(-characterThatAttacked.attackPower, source: characterThatAttacked);
        attackSummary += "\nDealt damage " + stateComponent.character.attackPower.ToString();
        //If the hostile reaches 0 hp, evalueate if he/she dies, get knock out, or get injured
        if (this.currentHP <= 0) {
            attackSummary += "\n" + this.name + "'s hp has reached 0.";
            WeightedDictionary<string> loserResults = new WeightedDictionary<string>();

            int deathWeight = 70;
            int unconsciousWeight = 30;
            if (!characterThatAttacked.marker.IsLethalCombatForTarget(this)) {
                deathWeight = 5;
                unconsciousWeight = 95;
            }
            string rollLog = GameManager.Instance.TodayLogString() + characterThatAttacked.name + " attacked " + name
                + ", death weight: " + deathWeight + ", unconscious weight: " + unconsciousWeight 
                + ", isLethal: " + characterThatAttacked.marker.IsLethalCombatForTarget(this);

            if (this.GetNormalTrait("Unconscious") == null) {
                loserResults.AddElement("Unconscious", unconsciousWeight);
                rollLog += "\n- Unconscious weight will be added";
            }
            //if (currentClosestHostile.GetNormalTrait("Injured") == null) {
            //    loserResults.AddElement("Injured", 10);
            //}
            if (!isDead) {
                loserResults.AddElement("Death", deathWeight);
                rollLog += "\n- Death weight will be added";
            }

            if (loserResults.Count > 0) {
                string result = loserResults.PickRandomElementGivenWeights();
                rollLog += "\n- Pick result is: " + result;
                characterThatAttacked.PrintLogIfActive(rollLog);
                attackSummary += "\ncombat result is " + result; ;
                switch (result) {
                    case "Unconscious":
                        Unconscious unconscious = new Unconscious();
                        this.AddTrait(unconscious, characterThatAttacked, gainedFromDoing: state.actionThatTriggeredThisState);
                        break;
                    case "Injured":
                        Injured injured = new Injured();
                        this.AddTrait(injured, characterThatAttacked, gainedFromDoing: state.actionThatTriggeredThisState);
                        break;
                    case "Death":
                        this.Death("attacked", deathFromAction: state.actionThatTriggeredThisState, responsibleCharacter: characterThatAttacked);
                        break;
                }
            } else {
                rollLog += "\n- Dictionary is empty, no result!";
                characterThatAttacked.PrintLogIfActive(rollLog);
            }
        } else {
            Invisible invisible = characterThatAttacked.GetNormalTrait("Invisible") as Invisible;
            if (invisible != null) {
                if (invisible.level == 1) {
                    //Level 1 = remove invisible trait
                    characterThatAttacked.RemoveTrait(invisible);
                } else if (invisible.level == 2) {
                    //Level 2 = attacked character will be the only character to see
                    invisible.AddCharacterThatCanSee(this);
                }
                //Level 3 = will not be seen forever
            }
        }

        Messenger.Broadcast(Signals.CHARACTER_WAS_HIT, this, characterThatAttacked);
    }
    /// <summary>
    /// Function to check what a character will do when he/she sees a hostile.
    /// </summary>
    /// <returns>True or False(True if the character will not flee, and false if otherwise).</returns>
    public bool IsCombatReady() {
        if (IsHealthCriticallyLow()) {
            return false;
        }
        if (isStarving && GetNormalTrait("Vampiric") == null) {
            return false; //only characters that are not vampires will flee if they are starving
        }
        if (isExhausted) {
            return false;
        }
        if (GetNormalTrait("Spooked") != null) {
            return false;
        }
        //if (GetNormalTrait("Injured") != null) {
        //    return false;
        //}
        return true;
    }
    #endregion

    #region Portrait Settings
    public void SetPortraitSettings(PortraitSettings settings) {
        _portraitSettings = settings;
    }
    #endregion

    #region RPG
    private bool hpMagicRangedStatMod;
    public virtual void LevelUp() {
        //Only level up once per day
        //if (_lastLevelUpDay == GameManager.Instance.continuousDays) {
        //    return;
        //}
        //_lastLevelUpDay = GameManager.Instance.continuousDays;
        if (_level < CharacterManager.Instance.maxLevel) {
            _level += 1;
            //Add stats per level from class
            if (_characterClass.rangeType == RANGE_TYPE.MELEE) {//_characterClass.attackType == ATTACK_TYPE.PHYSICAL &&  = Commented this because if the character is a MAGICAL MELEE, he cannot level up
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
            UpdateMaxHP();
            //Reset to full health and sp
            ResetToFullHP();

            Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
            if (_playerCharacterItem != null) {
                _playerCharacterItem.UpdateMinionItem();
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
        if (_characterClass.rangeType == RANGE_TYPE.MELEE) {//_characterClass.attackType == ATTACK_TYPE.PHYSICAL &&  = Commented this because if the character is a MAGICAL MELEE, he cannot level up
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

        UpdateMaxHP();
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
        } else if (_level > CharacterManager.Instance.maxLevel) {
            _level = CharacterManager.Instance.maxLevel;
        }

        //Add stats per level from class
        int difference = _level - previousLevel;
        if (_characterClass.rangeType == RANGE_TYPE.MELEE) {//_characterClass.attackType == ATTACK_TYPE.PHYSICAL &&  = Commented this because if the character is a MAGICAL MELEE, he cannot level up
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

        UpdateMaxHP();
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
        if (_currentHP > maxHPMod) {
            _currentHP = maxHPMod;
        }
        if (_sp > _maxSP) {
            _sp = _maxSP;
        }
    }
    public void AdjustExperience(int amount) {
        _experience += amount;
        if (_experience >= _maxExperience) {
            SetExperience(0);
            //LevelUp();
        }
    }
    public void SetExperience(int amount) {
        _experience = amount;
    }
    private void RecomputeMaxExperience() {
        _maxExperience = Mathf.CeilToInt(100f * ((Mathf.Pow((float) _level, 1.25f)) / 1.1f));
    }
    public void SetMaxExperience(int amount) {
        _maxExperience = amount;
    }
    //public void AdjustElementalWeakness(ELEMENT element, float amount) {
    //    _elementalWeaknesses[element] += amount;
    //}
    //public void AdjustElementalResistance(ELEMENT element, float amount) {
    //    _elementalResistances[element] += amount;
    //}
    public void AdjustSP(int amount) {
        _sp += amount;
        _sp = Mathf.Clamp(_sp, 0, _maxSP);
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
    //Adjust current HP based on specified paramater, but HP must not go below 0
    public virtual void AdjustHP(int amount, bool triggerDeath = false, object source = null) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, maxHP);
        marker.UpdateHP();
        Messenger.Broadcast(Signals.ADJUSTED_HP, this);
        if (IsHealthCriticallyLow()) {
            Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, this);
        }
        if (triggerDeath && previous != this._currentHP) {
            if (this._currentHP <= 0) {
                if (source is Character) {
                    Character character = source as Character;
                    Death("attacked", responsibleCharacter: character);
                } else {
                    string cause = "attacked";
                    if (source != null) {
                        cause += "_" + source.ToString();
                    }
                    Death(cause);
                }
                
            }
        }
    }
    public void SetMaxHPMod(int amount) {
        int previousMaxHP = maxHP;
        maxHPMod = amount;
        UpdateMaxHP();
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustAttackMod(int amount) {
        attackPowerMod += amount;
    }
    public void AdjustAttackPercentMod(int amount) {
        attackPowerPercentMod += amount;
    }
    public void AdjustMaxHPMod(int amount) {
        int previousMaxHP = maxHP;
        maxHPMod += amount;
        UpdateMaxHP();
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustMaxHPPercentMod(int amount) {
        int previousMaxHP = maxHP;
        maxHPPercentMod += amount;
        UpdateMaxHP();
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void UpdateMaxHP() {
        _maxHP = (int) (((_characterClass.baseHP + maxHPMod) * (1f + ((_raceSetting.hpModifier + maxHPPercentMod) / 100f))) * 4f);
        if (_maxHP < 0) {
            _maxHP = 1;
        }
    }
    public void HPRecovery(float maxHPPercentage) {
        if(currentHP < maxHP && currentHP > 0) {
            AdjustHP(Mathf.CeilToInt(maxHPPercentage * maxHP));
        }
    }
    public void AdjustSpeedMod(int amount) {
        speedMod += amount;
    }
    public void AdjustSpeedPercentMod(int amount) {
        speedPercentMod += amount;
    }
    public bool IsHealthFull() {
        return _currentHP >= maxHP;
    }
    public bool IsHealthCriticallyLow() {
        //chance based dependent on the character
        return currentHP < (maxHP * 0.2f); //TODO: Change to be class based
    }
    #endregion

    #region Home
    /// <summary>
    /// Set this character's home area data.(Does nothing else)
    /// </summary>
    /// <param name="newHome">The character's new home</param>
    public void SetHome(Area newHome) {
        Area previousHome = homeArea;
        homeArea = newHome;

        //If a character sets his home, add his faction to the factions in the region
        //Subsequently, if character loses his home, remove his faction from the region only if there are no co faction resident in the region anymore
        if(faction != null) {
            if(newHome == null) {
                //If character loses home and he has previous home, remove him faction
                if(previousHome != null) {
                    bool removeFaction = true;
                    for (int i = 0; i < previousHome.areaResidents.Count; i++) {
                        Character resident = previousHome.areaResidents[i];
                        if(resident != this && resident.faction == faction) {
                            removeFaction = false;
                            break;
                        }
                    }
                    if (removeFaction) {
                        previousHome.region.RemoveFactionHere(faction);
                    }
                }
            } else {
                newHome.region.AddFactionHere(faction);
            }
        }
    }
    public void SetHomeStructure(Dwelling homeStructure) {
        this.homeStructure = homeStructure;
        currentAlterEgo.SetHomeStructure(homeStructure);
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
    public bool MigrateHomeTo(Area newHomeArea, Dwelling homeStructure = null, bool broadcast = true) {
        Area previousHome = null;
        if (homeArea != null) {
            previousHome = homeArea;
            homeArea.RemoveResident(this);
        }
        if(newHomeArea.AddResident(this, homeStructure)) {
            if (broadcast) {
                Messenger.Broadcast(Signals.CHARACTER_MIGRATED_HOME, this, previousHome, newHomeArea);
            }
            return true;
        }
        return false;
    }
    public void MigrateHomeStructureTo(Dwelling dwelling) {
        if (this.homeStructure != null) {
            if (this.homeStructure == dwelling) {
                return; //ignore change
            }
            //remove character from his/her old home
            this.homeStructure.RemoveResident(this);
        }
        if (dwelling != null) {
            //Added checking, because character can sometimes change home from dwelling to nothing.
            dwelling.AddResident(this);
        }
#if !WORLD_CREATION_TOOL
        //Debug.Log(GameManager.Instance.TodayLogString() + this.name + " changed home structure to " + dwelling?.ToString() ?? "None");
#endif
    }
    //private void OnCharacterMigratedHome(Character character, Area previousHome, Area homeArea) {
    //    if (character.id != this.id && this.homeArea.id == homeArea.id) {
    //        if (GetAllRelationshipTraitWith(character) != null) {
    //            this.homeArea.AssignCharacterToDwellingInArea(this); //redetermine home, in case new character with relationship has moved area to same area as this character
    //        }
    //    }
    //}
    #endregion

    #region Traits
    public void CreateInitialTraitsByClass() {
        ////Attack Type
        //if (characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
        //    AddTrait("Physical Attacker");
        //} else if (characterClass.attackType == ATTACK_TYPE.MAGICAL) {
        //    AddTrait("Magic User");
        //}

        ////Range Type
        //if (characterClass.rangeType == RANGE_TYPE.MELEE) {
        //    AddTrait("Melee Attack");
        //} else if (characterClass.rangeType == RANGE_TYPE.RANGED) {
        //    AddTrait("Ranged Attack");
        //}

        ////Combat Position
        //if (characterClass.combatPosition == COMBAT_POSITION.FRONTLINE) {
        //    AddTrait("Frontline Combatant");
        //} else if (characterClass.combatPosition == COMBAT_POSITION.BACKLINE) {
        //    AddTrait("Backline Combatant");
        //}

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

        string[] traitPool = new string[] { "Vigilant", "Doctor", "Diplomatic",
            "Fireproof", "Accident Prone", "Unfaithful", "Alcoholic", "Craftsman", "Music Lover", "Music Hater", "Ugly", "Blessed", "Nocturnal",
            "Herbalist", "Optimist", "Pessimist", "Fast", "Prude", "Horny", "Coward", "Lazy", "Hardworking", "Glutton", "Robust", "Suspicious" , "Inspiring", "Pyrophobic",
            "Narcoleptic", "Hothead",
        };
        //"Kleptomaniac","Curious", 

        List<string> buffTraits = new List<string>();
        List<string> flawTraits = new List<string>();
        List<string> neutralTraits = new List<string>();

        //Categorize traits from trait pool
        for (int i = 0; i < traitPool.Length; i++) {
            string currTraitName = traitPool[i];
            if (AttributeManager.Instance.allTraits.ContainsKey(currTraitName)) {
                Trait trait = AttributeManager.Instance.allTraits[currTraitName];
                if(trait.type == TRAIT_TYPE.BUFF) {
                    buffTraits.Add(currTraitName);
                } else if (trait.type == TRAIT_TYPE.FLAW) {
                    flawTraits.Add(currTraitName);
                } else {
                    neutralTraits.Add(currTraitName);
                }
            } else {
                throw new Exception("There is no trait named: " + currTraitName);
            }
        }

        //First trait is random buff trait
        string chosenBuffTraitName = string.Empty;
        if (buffTraits.Count > 0) {
            int index = UnityEngine.Random.Range(0, buffTraits.Count);
            chosenBuffTraitName = buffTraits[index];
            buffTraits.RemoveAt(index);
        } else {
            throw new Exception("There are no buff traits!");
        }

        Trait buffTrait;
        AddTrait(chosenBuffTraitName, out buffTrait);
        if (buffTrait.mutuallyExclusive != null) {
            buffTraits = Utilities.RemoveElements(buffTraits, buffTrait.mutuallyExclusive); //update buff traits pool to accomodate new trait
            neutralTraits = Utilities.RemoveElements(neutralTraits, buffTrait.mutuallyExclusive); //update neutral traits pool to accomodate new trait
            flawTraits = Utilities.RemoveElements(flawTraits, buffTrait.mutuallyExclusive); //update flaw traits pool to accomodate new trait
        }
        

        //Second trait is a random buff or neutral trait
        string chosenBuffOrNeutralTraitName = string.Empty;
        if (buffTraits.Count > 0 && neutralTraits.Count > 0) {
            if(UnityEngine.Random.Range(0, 2) == 0) {
                //Buff trait
                int index = UnityEngine.Random.Range(0, buffTraits.Count);
                chosenBuffOrNeutralTraitName = buffTraits[index];
                buffTraits.RemoveAt(index);
            } else {
                //Neutral trait
                int index = UnityEngine.Random.Range(0, neutralTraits.Count);
                chosenBuffOrNeutralTraitName = neutralTraits[index];
                neutralTraits.RemoveAt(index);
                ///Changed this to use mutual exclusive list per trait <see cref= "Trait.mutuallyExclusive"/>
                //if(chosenBuffOrNeutralTraitName == "Music Lover") {
                //    flawTraits.Remove("Music Hater");
                //}
            }
        } else {
            if(buffTraits.Count > 0) {
                int index = UnityEngine.Random.Range(0, buffTraits.Count);
                chosenBuffOrNeutralTraitName = buffTraits[index];
                buffTraits.RemoveAt(index);
            } else {
                int index = UnityEngine.Random.Range(0, neutralTraits.Count);
                chosenBuffOrNeutralTraitName = neutralTraits[index];
                neutralTraits.RemoveAt(index);
                ///Changed this to use mutual exclusive list per trait <see cref= "Trait.mutuallyExclusive"/>
                //if (chosenBuffOrNeutralTraitName == "Music Lover") {
                //    flawTraits.Remove("Music Hater");
                //}
            }
        }
        Trait buffOrNeutralTrait;
        AddTrait(chosenBuffOrNeutralTraitName, out buffOrNeutralTrait);
        if (buffOrNeutralTrait.mutuallyExclusive != null) {
            buffTraits = Utilities.RemoveElements(buffTraits, buffOrNeutralTrait.mutuallyExclusive); //update buff traits pool to accomodate new trait
            neutralTraits = Utilities.RemoveElements(neutralTraits, buffOrNeutralTrait.mutuallyExclusive); //update neutral traits pool to accomodate new trait
            flawTraits = Utilities.RemoveElements(flawTraits, buffOrNeutralTrait.mutuallyExclusive); //update flaw traits pool to accomodate new trait
        }


        //Third trait is a random neutral or flaw traits
        string chosenFlawOrNeutralTraitName = string.Empty;
        if (flawTraits.Count > 0 && neutralTraits.Count > 0) {
            if (UnityEngine.Random.Range(0, 2) == 0) {
                //Buff trait
                int index = UnityEngine.Random.Range(0, flawTraits.Count);
                chosenFlawOrNeutralTraitName = flawTraits[index];
                flawTraits.RemoveAt(index);
            } else {
                //Neutral trait
                int index = UnityEngine.Random.Range(0, neutralTraits.Count);
                chosenFlawOrNeutralTraitName = neutralTraits[index];
                neutralTraits.RemoveAt(index);
            }
        } else {
            if (flawTraits.Count > 0) {
                int index = UnityEngine.Random.Range(0, flawTraits.Count);
                chosenFlawOrNeutralTraitName = flawTraits[index];
                flawTraits.RemoveAt(index);
            } else {
                int index = UnityEngine.Random.Range(0, neutralTraits.Count);
                chosenFlawOrNeutralTraitName = neutralTraits[index];
                neutralTraits.RemoveAt(index);
            }
        }
        AddTrait(chosenFlawOrNeutralTraitName);
        //AddTrait("Narcoleptic");
        //AddTrait("Glutton");
        AddTrait("Character Trait");
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
    public bool AddTrait(string traitName, out Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if (AttributeManager.Instance.IsInstancedTrait(traitName)) {
            trait = AttributeManager.Instance.CreateNewInstancedTraitClass(traitName);
            return AddTrait(trait, characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
        } else {
            trait = AttributeManager.Instance.allTraits[traitName];
            return AddTrait(trait, characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
        }
    }
    public bool AddTrait(string traitName, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if (AttributeManager.Instance.IsInstancedTrait(traitName)) {
            return AddTrait(AttributeManager.Instance.CreateNewInstancedTraitClass(traitName), characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
        } else {
            return AddTrait(AttributeManager.Instance.allTraits[traitName], characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
        }
    }
    public bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if(trait.mutuallyExclusive != null) {
            //Cannot add trait if there is an existing trait that is mutually exclusive of the trait to be added
            for (int i = 0; i < trait.mutuallyExclusive.Length; i++) {
                if(GetNormalTrait(trait.mutuallyExclusive[i]) != null) {
                    return false;
                }
            }
        }
        if (trait.IsUnique()) {
            Trait oldTrait = GetNormalTrait(trait.name);
            if(oldTrait != null) {
                oldTrait.SetCharacterResponsibleForTrait(characterResponsible);
                oldTrait.AddCharacterResponsibleForTrait(characterResponsible);
                if (oldTrait.broadcastDuplicates) {
                    Messenger.Broadcast(Signals.TRAIT_ADDED, this, oldTrait);
                }
                return false;
            }
        }
        if (!(trait is RelationshipTrait)) {
            //Not adding relationship traits to the list of traits, since the getter will combine the traits list from this list and the relationships dictionary.
            //Did this so that relationships can be swappable without having to call RemoveTrait.
            _normalTraits.Add(trait); 
        }
        if(trait is CharacterTrait) {
            //This is trait is default for all characters that's why it has a special field
            defaultCharacterTrait = trait as CharacterTrait;
        }
        trait.SetGainedFromDoing(gainedFromDoing);
        trait.SetOnRemoveAction(onRemoveAction);
        trait.SetCharacterResponsibleForTrait(characterResponsible);
        trait.AddCharacterResponsibleForTrait(characterResponsible);
        ApplyTraitEffects(trait);
        ApplyPOITraitInteractions(trait);
        if (trait.daysDuration > 0) {
            GameDate removeDate = GameManager.Instance.Today();
            removeDate.AddTicks(trait.daysDuration);
            string ticket = SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTraitOnSchedule(trait), this);
            trait.SetExpiryTicket(this, ticket);
        }
        if (triggerOnAdd) {
            trait.OnAddTrait(this);
        }
        Messenger.Broadcast(Signals.TRAIT_ADDED, this, trait);

#if !WORLD_CREATION_TOOL
        if (GameManager.Instance.gameHasStarted) {
            if (trait.name == "Starving") {
                //Debug.Log("Planning fullness recovery from gain trait");
                PlanFullnessRecoveryActions(true);
            } else if (trait.name == "Forlorn" || trait.name == "Lonely") {
                //Debug.Log("Planning happiness recovery from gain trait");
                PlanHappinessRecoveryActions(true);
            } else if (trait.name == "Exhausted") {
                //Debug.Log("Planning tiredness recovery from gain trait");
                PlanTirednessRecoveryActions(true);
            }
        }
#endif
        if (trait.type == TRAIT_TYPE.CRIMINAL 
            || (trait.effect == TRAIT_EFFECT.NEGATIVE && trait.type == TRAIT_TYPE.DISABLER)) {
            //when a character gains a criminal trait, drop all location jobs that this character is assigned to
            homeArea.jobQueue.UnassignAllJobsTakenBy(this);
        }
        return true;
    }
    private bool RemoveTraitOnSchedule(Trait trait, bool triggerOnRemove = true) {
        if (isDead) {
            return false;
        }
        return RemoveTrait(trait, triggerOnRemove);
    }
    public bool RemoveTrait(Trait trait, bool triggerOnRemove = true, Character removedBy = null) {
        bool removed = false;
        if (trait is RelationshipTrait) {
            removed = true;
        } else {
            removed = _normalTraits.Remove(trait);
        } 
        if (removed) {
            UnapplyTraitEffects(trait);
            UnapplyPOITraitInteractions(trait);
            trait.RemoveExpiryTicket(this);
            if (triggerOnRemove) {
                trait.OnRemoveTrait(this, removedBy);
            }
            Messenger.Broadcast(Signals.TRAIT_REMOVED, this, trait);
            //if (trait is RelationshipTrait) {
            //    RelationshipTrait rel = trait as RelationshipTrait;
            //    RemoveRelationship(rel.targetCharacter, rel);
            //}
        }
        return removed;
    }
    public bool RemoveTrait(string traitName, bool triggerOnRemove = true, Character removedBy = null) {
        Trait trait = GetNormalTrait(traitName);
        if (trait != null) {
            return RemoveTrait(trait, triggerOnRemove, removedBy);
        }
        return false;
    }
    public void RemoveTrait(List<Trait> traits) {
        for (int i = 0; i < traits.Count; i++) {
            RemoveTrait(traits[i]);
        }
    }
    //public void RemoveAllTraits(string traitNameException = "") {
    //    if (traitNameException == "") {
    //        while (allTraits.Count > 0) {
    //            RemoveTrait(allTraits[0]);
    //        }
    //    } else {
    //        for (int i = 0; i < allTraits.Count; i++) {
    //            if (allTraits[i].name != traitNameException) {
    //                RemoveTrait(allTraits[i]);
    //                i--;
    //            }
    //        }
    //    }
    //}
    //public void RemoveAllNonRelationshipTraits(string traitNameException = "") {
    //    if (traitNameException == "") {
    //        for (int i = 0; i < allTraits.Count; i++) {
    //            if (!(allTraits[i] is RelationshipTrait)) {
    //                RemoveTrait(allTraits[i]);
    //                i--;
    //            }
    //        }
    //    } else {
    //        for (int i = 0; i < allTraits.Count; i++) {
    //            if (allTraits[i].name != traitNameException && !(allTraits[i] is RelationshipTrait)) {
    //                RemoveTrait(allTraits[i]);
    //                i--;
    //            }
    //        }
    //    }
    //}
    public Trait GetNormalTrait(params string[] traitNames) {
    	for (int i = 0; i < normalTraits.Count; i++) {
            Trait trait = normalTraits[i];
            for (int j = 0; j < traitNames.Length; j++) {
                if (trait.name == traitNames[j] && !trait.isDisabled) {
                    return trait;
                }
            }
        }
        return null;
    }
    public Trait GetNormalTrait(string traitName) {
        for (int i = 0; i < normalTraits.Count; i++) {
            if ((normalTraits[i].name == traitName || normalTraits[i].GetType().ToString() == traitName) && !normalTraits[i].isDisabled) {
                return normalTraits[i];
            }
        }
        return null;
    }
    /// <summary>
    /// Remove all traits that are not persistent.
    /// NOTE: This does NOT remove relationships!
    /// </summary>
    public void RemoveAllNonPersistentTraits() {
        List<Trait> allTraits = new List<Trait>(this.normalTraits);
        for (int i = 0; i < allTraits.Count; i++) {
            Trait currTrait = allTraits[i];
            //if (currTrait is RelationshipTrait) {
            //    continue; //skip
            //}
            if (!currTrait.isPersistent) {
                RemoveTrait(currTrait);
            }
        }
    }    
    public bool HasTraitOf(TRAIT_TYPE traitType, string traitException = "") {
        for (int i = 0; i < normalTraits.Count; i++) {
            if (traitException != "" && normalTraits[i].name == traitException) { continue; }
            if (normalTraits[i].type == traitType && !normalTraits[i].isDisabled) {
                return true;
            }
        }
        return false;
    }
    public bool HasTraitOf(TRAIT_EFFECT effect, TRAIT_TYPE type) {
        for (int i = 0; i < normalTraits.Count; i++) {
            Trait currTrait = normalTraits[i];
            if (currTrait.effect == effect && currTrait.type == type && !currTrait.isDisabled) {
                return true;
            }
        }
        return false;
    }
    public bool HasTraitOf(TRAIT_EFFECT effect1, TRAIT_EFFECT effect2, TRAIT_TYPE type) {
        for (int i = 0; i < normalTraits.Count; i++) {
            Trait currTrait = normalTraits[i];
            if ((currTrait.effect == effect1 || currTrait.effect == effect2) && currTrait.type == type && !currTrait.isDisabled) {
                return true;
            }
        }
        return false;
    }
    public bool HasTraitOf(TRAIT_EFFECT effect) {
        for (int i = 0; i < normalTraits.Count; i++) {
            Trait currTrait = normalTraits[i];
            if (currTrait.effect == effect && !currTrait.isDisabled) {
                return true;
            }
        }
        return false;
    }
    public Trait GetTraitOf(TRAIT_EFFECT effect, TRAIT_TYPE type) {
        for (int i = 0; i < normalTraits.Count; i++) {
            Trait currTrait = normalTraits[i];
            if (currTrait.effect == effect && currTrait.type == type && !currTrait.isDisabled) {
                return currTrait;
            }
        }
        return null;
    }
    public Trait GetTraitOf(TRAIT_TYPE type) {
        for (int i = 0; i < normalTraits.Count; i++) {
            Trait currTrait = normalTraits[i];
            if (currTrait.type == type && !currTrait.isDisabled) {
                return currTrait;
            }
        }
        return null;
    }
    public List<Trait> GetTraitsOf(TRAIT_TYPE type) {
        List<Trait> traits = new List<Trait>();
        for (int i = 0; i < normalTraits.Count; i++) {
            Trait currTrait = normalTraits[i];
            if (currTrait.type == type && !currTrait.isDisabled) {
                traits.Add(currTrait);
            }
        }
        return traits;
    }
    public bool HasTraitOf(System.Type traitType) {
        for (int i = 0; i < normalTraits.Count; i++) {
            System.Type type = normalTraits[i].GetType();
            if (type == traitType) {
                return true;
            }
        }
        return false;
    }
    public int GetNumberOfTraitOf(TRAIT_EFFECT effect, TRAIT_TYPE type) {
        int count = 0;
        for (int i = 0; i < normalTraits.Count; i++) {
            Trait currTrait = normalTraits[i];
            if (currTrait.effect == effect && currTrait.type == type && !currTrait.isDisabled) {
                count++;
            }
        }
        return count;
    }
    public List<Trait> RemoveAllTraitsByType(TRAIT_TYPE traitType) {
        List<Trait> removedTraits = new List<Trait>();
        if (traitType == TRAIT_TYPE.RELATIONSHIP) {
            for (int i = 0; i < relationshipTraits.Count; i++) {
                if (relationshipTraits[i].type == traitType) {
                    removedTraits.Add(relationshipTraits[i]);
                    RemoveTrait(relationshipTraits[i]);
                    i--;
                }
            }
        } else {
            for (int i = 0; i < normalTraits.Count; i++) {
                if (normalTraits[i].type == traitType) {
                    removedTraits.Add(normalTraits[i]);
                    RemoveTrait(normalTraits[i]);
                    i--;
                }
            }
        }
        
        return removedTraits;
    }
    public Trait GetRandomNormalTrait(TRAIT_EFFECT effect) {
        List<Trait> negativeTraits = new List<Trait>();
        for (int i = 0; i < normalTraits.Count; i++) {
            if (normalTraits[i].effect == effect && !normalTraits[i].isDisabled) {
                negativeTraits.Add(normalTraits[i]);
            }
        }
        if (negativeTraits.Count > 0) {
            return negativeTraits[UnityEngine.Random.Range(0, negativeTraits.Count)];
        }
        return null;
    }
    private void ApplyTraitEffects(Trait trait) {
        if (trait.type == TRAIT_TYPE.DISABLER) {
            AdjustDoNotDisturb(1);
            if (trait.effect == TRAIT_EFFECT.NEGATIVE) {
                AdjustIgnoreHostilities(1);
                CancelAllJobsAndPlansExceptNeedsRecovery();
            }
            if (trait.name != "Combat Recovery") {
                _ownParty.RemoveAllOtherCharacters();
                CancelAllJobsTargettingThisCharacter(JOB_TYPE.KNOCKOUT);
            }
        } else if (trait.type == TRAIT_TYPE.CRIMINAL) {
            CancelOrUnassignRemoveTraitRelatedJobs();
        }
        if (trait.name == "Abducted" || trait.name == "Restrained") {
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
            AdjustMoodValue(-35, trait, trait.gainedFromDoing);
        } else if (trait.name == "Lonely") {
            AdjustMoodValue(-20, trait, trait.gainedFromDoing);
        } else if (trait.name == "Exhausted") {
            marker.AdjustUseWalkSpeed(1);
            AdjustMoodValue(-35, trait, trait.gainedFromDoing);
        } else if (trait.name == "Tired") {
            AdjustSpeedModifier(-0.2f);
            AdjustMoodValue(-10, trait, trait.gainedFromDoing);
        } else if (trait.name == "Starving") {
            AdjustMoodValue(-25, trait, trait.gainedFromDoing);
        } else if (trait.name == "Hungry") {
            AdjustMoodValue(-10, trait, trait.gainedFromDoing);
        } else if (trait.name == "Injured") {
            AdjustMoodValue(-15, trait, trait.gainedFromDoing);
        } else if (trait.name == "Cursed") {
            AdjustMoodValue(-25, trait, trait.gainedFromDoing);
        } else if (trait.name == "Sick") {
            AdjustMoodValue(-15, trait, trait.gainedFromDoing);
        } else if (trait.name == "Cheery") {
            AdjustMoodValue(15, trait, trait.gainedFromDoing);
        } else if (trait.name == "Annoyed") {
            AdjustMoodValue(-15, trait, trait.gainedFromDoing);
        } else if (trait.name == "Lethargic") {
            AdjustMoodValue(-20, trait, trait.gainedFromDoing);
        } else if (trait.name == "Heartbroken") {
            AdjustMoodValue(-25, trait, trait.gainedFromDoing);
        } else if (trait.name == "Griefstricken") {
            AdjustMoodValue(-20, trait, trait.gainedFromDoing);
        } else if (trait.name == "Encumbered") {
            AdjustSpeedModifier(-0.5f);
        } else if (trait.name == "Vampiric") {
            AdjustDoNotGetTired(1);
        } else if (trait.name == "Unconscious") {
            AdjustDoNotGetTired(1);
            AdjustDoNotGetHungry(1);
            AdjustDoNotGetLonely(1);
        } else if (trait.name == "Optimist") {
            AdjustHappinessDecreaseRate(-320); //Reference: https://trello.com/c/Aw8kIbB1/2654-optimist
        } else if (trait.name == "Pessimist") {
            AdjustHappinessDecreaseRate(320); //Reference: https://trello.com/c/lcen0P9l/2653-pessimist
        } else if (trait.name == "Fast") {
            AdjustSpeedModifier(0.25f); //Reference: https://trello.com/c/Gb3kfZEm/2658-fast
        }
        //else if (trait.name == "Hungry") {
        //    CreateFeedJob();
        //} else if (trait.name == "Starving") {
        //    MoveFeedJobToTopPriority();
        //}
        if (trait.effects != null) {
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
    }
    private void UnapplyTraitEffects(Trait trait) {
        if (trait.type == TRAIT_TYPE.DISABLER) {
            AdjustDoNotDisturb(-1);
            if (trait.effect == TRAIT_EFFECT.NEGATIVE) {
                AdjustIgnoreHostilities(-1);
            }
        }
        //else if(trait.type == TRAIT_TYPE.CRIMINAL) {
        //    if(GetTraitOf(TRAIT_TYPE.CRIMINAL) == null) {
        //        for (int i = 0; i < traits.Count; i++) {
        //            if (traits[i].name == "Cursed" || traits[i].name == "Sick"
        //                || traits[i].name == "Injured" || traits[i].name == "Unconscious") {
        //                CreateRemoveTraitJob(traits[i].name);
        //            }
        //        }
        //    }
        //}
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
            AdjustMoodValue(35, trait, trait.gainedFromDoing);
        } else if (trait.name == "Lonely") {
            AdjustMoodValue(20, trait, trait.gainedFromDoing);
        } else if (trait.name == "Exhausted") {
            marker.AdjustUseWalkSpeed(-1);
            AdjustMoodValue(35, trait, trait.gainedFromDoing);
        } else if (trait.name == "Tired") {
            AdjustSpeedModifier(0.2f);
            AdjustMoodValue(10, trait, trait.gainedFromDoing);
        } else if (trait.name == "Starving") {
            AdjustMoodValue(25, trait, trait.gainedFromDoing);
        } else if (trait.name == "Hungry") {
            AdjustMoodValue(10, trait, trait.gainedFromDoing);
        } else if (trait.name == "Injured") {
            AdjustMoodValue(15, trait, trait.gainedFromDoing);
        } else if (trait.name == "Cursed") {
            AdjustMoodValue(25, trait, trait.gainedFromDoing);
        } else if (trait.name == "Sick") {
            AdjustMoodValue(15, trait, trait.gainedFromDoing);
        } else if (trait.name == "Cheery") {
            AdjustMoodValue(-15, trait, trait.gainedFromDoing);
        } else if (trait.name == "Annoyed") {
            AdjustMoodValue(15, trait, trait.gainedFromDoing);
        } else if (trait.name == "Lethargic") {
            AdjustMoodValue(20, trait, trait.gainedFromDoing);
        } else if (trait.name == "Encumbered") {
            AdjustSpeedModifier(0.5f);
        } else if (trait.name == "Vampiric") {
            AdjustDoNotGetTired(-1);
        } else if (trait.name == "Unconscious") {
            AdjustDoNotGetTired(-1);
            AdjustDoNotGetHungry(-1);
            AdjustDoNotGetLonely(-1);
        } else if (trait.name == "Optimist") {
            AdjustHappinessDecreaseRate(320); //Reference: https://trello.com/c/Aw8kIbB1/2654-optimist
        } else if (trait.name == "Pessimist") {
            AdjustHappinessDecreaseRate(-320); //Reference: https://trello.com/c/lcen0P9l/2653-pessimist
        } else if (trait.name == "Fast") {
            AdjustSpeedModifier(-0.25f); //Reference: https://trello.com/c/Gb3kfZEm/2658-fast
        }

        if (trait.effects != null) {
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
    public bool HasRelationshipTraitOf(RELATIONSHIP_TRAIT relType, bool includeDead = true) {
        for (int i = 0; i < relationshipTraits.Count; i++) {
            if (!relationshipTraits[i].isDisabled) {
                RelationshipTrait currTrait = relationshipTraits[i];
                if (currTrait.targetCharacter.isDead && !includeDead) {
                    continue; //skip dead characters
                }
                if (currTrait.relType == relType) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipTraitOf(RELATIONSHIP_TRAIT relType, Faction except) {
        for (int i = 0; i < relationshipTraits.Count; i++) {
            if (!relationshipTraits[i].isDisabled) {
                RelationshipTrait currTrait = relationshipTraits[i];
                if (currTrait.relType == relType
                    && currTrait.targetCharacter.faction.id != except.id) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool ReleaseFromAbduction() {
        Trait trait = GetNormalTrait("Abducted");
        if (trait != null) {
            Abducted abductedTrait = trait as Abducted;
            RemoveTrait(abductedTrait);
            ReturnToOriginalHomeAndFaction(abductedTrait.originalHome, this.faction);
            //MigrateTo(abductedTrait.originalHomeLandmark);

            //Interaction interactionAbducted = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, specificLocation);
            //InduceInteraction(interactionAbducted);
            return true;
        }
        return false;
    }
    public SpecialToken CraftAnItem() {
        Craftsman craftsmanTrait = GetNormalTrait("Craftsman") as Craftsman;
        if (craftsmanTrait != null) {
            //SpecialTokenSettings settings = TokenManager.Instance.GetTokenSettings(craftsmanTrait.craftedItemName);
            //return TokenManager.Instance.CreateSpecialToken(craftsmanTrait.craftedItemName); //, settings.appearanceWeight
        }
        return null;
    }
    public void AddTraitNeededToBeRemoved(Trait trait) {
        traitsNeededToBeRemoved.Add(trait);
    }
    public void RemoveTraitNeededToBeRemoved(Trait trait) {
        traitsNeededToBeRemoved.Remove(trait);
    }
    private List<RelationshipTrait> GetAllRelationshipTraits() {
        List<RelationshipTrait> allRels = new List<RelationshipTrait>();
        for (int i = 0; i < relationships.Values.Count; i++) {
            allRels.AddRange(relationships.Values.ElementAt(i).rels);
        }
        return allRels;
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
        //UnsubscribeSignals(); //Removed this since character's listeners are not on by default now.
    }
    public void RecruitAsMinion() {
        if (stateComponent.currentState != null) {
            stateComponent.currentState.OnExitThisState();
        } else if (stateComponent.stateToDo != null) {
            stateComponent.SetStateToDo(null);
        }

        CancelAllJobsTargettingThisCharacter("target became a minion", false);
        Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, this, "target became a minion");
        if (currentAction != null && !currentAction.cannotCancelAction) {
            currentAction.StopAction();
        }

        if (!IsInOwnParty()) {
            _currentParty.RemoveCharacter(this);
        }
        MigrateHomeTo(PlayerManager.Instance.player.playerArea);

        specificLocation.RemoveCharacterFromLocation(this.currentParty);
        if (currentLandmark != null) {
            currentLandmark.tileLocation.region.RemoveCharacterFromLocation(this);
        }
        ResetFullnessMeter();
        ResetHappinessMeter();
        ResetTirednessMeter();
        //PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(this.currentParty);

        ChangeFactionTo(PlayerManager.Instance.player.playerFaction);

        Minion newMinion = PlayerManager.Instance.player.CreateNewMinion(this);
        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new Minion!", () => PlayerManager.Instance.player.AddMinion(newMinion, true));
        //PlayerManager.Instance.player.AddMinion(newMinion);



        //if (!characterToken.isObtainedByPlayer) {
        //    PlayerManager.Instance.player.AddToken(characterToken);
        //}
    }
    #endregion

    public void SetPlayerCharacterItem(PlayerCharacterItem item) {
        _playerCharacterItem = item;
    }

    #region Interaction
    public void AddInteractionType(INTERACTION_TYPE type) {
        if (!currentInteractionTypes.Contains(type)) {
            currentInteractionTypes.Add(type);
        }
    }
    public void RemoveInteractionType(INTERACTION_TYPE type) {
        currentInteractionTypes.Remove(type);
    }
    protected virtual void PerTickGoapPlanGeneration() {
        if(isDead || minion != null) {
            return;
        }
        //Out of combat hp recovery
        if(stateComponent.currentState == null || stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT) {
            HPRecovery(0.0025f);
        }

        if(isStoppedByOtherCharacter > 0) {
            return;
        }
        //Check Trap Structure
        trapStructure.IncrementCurrentDuration(1);

        PlanForcedFullnessRecovery();
        PlanForcedTirednessRecovery();

        //This is to ensure that this character will not be idle forever
        //If at the start of the tick, the character is not currently doing any action, and is not waiting for any new plans, it means that the character will no longer perform any actions
        //so start doing actions again
        SetHasAlreadyAskedForPlan(false);
        if (CanPlanGoap()) {
            PlanGoapActions();
        }
    }
    private void DailyGoapProcesses() {
        hasForcedFullness = false;
        hasForcedTiredness = false;
    }
    public bool CanPlanGoap() {
        return currentAction == null && _numOfWaitingForGoapThread <= 0;
    }
    public void PlanGoapActions() {
        if (!IsInOwnParty() || isDefender || ownParty.icon.isTravelling || _doNotDisturb > 0 || isWaitingForInteraction > 0 || isDead || marker.hasFleePath) {
            return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
        }
        if (stateComponent.currentState != null) {
            //Debug.LogWarning(name + " is currently in " + stateComponent.currentState.stateName + " state, can't plan actions!");
            return;
        }
        if (stateComponent.stateToDo != null) {
            Debug.LogWarning("Will about to do " + stateComponent.stateToDo.stateName + " state, can't plan actions!");
            return;
        }
        //if(specificAction != null) {
        //    WillAboutToDoAction(specificAction);
        //    return;
        //}
        if (allGoapPlans.Count > 0) {
            //StopDailyGoapPlanGeneration();
            PerformGoapPlans();
            //SchedulePerformGoapPlans();
        } else {
            IdlePlans();
        }
    }
    protected virtual void IdlePlans() {
        if (_hasAlreadyAskedForPlan || isDead) {
            return;
        }
        if (returnedToLife) {
            //characters that have returned to life will just stroll.
            PlanIdleStrollOutside(currentStructure);
            return;
        }
        SetHasAlreadyAskedForPlan(true);
        if (!PlanJobQueueFirst()) {
            if (!PlanFullnessRecoveryActions(true)) {
                if (!PlanTirednessRecoveryActions(true)) {
                    if (!PlanHappinessRecoveryActions(true)) {
                        if (!PlanWorkActions()) {
                            string idleLog = OtherIdlePlans();
                            PrintLogIfActive(idleLog);
                        }
                    }
                }
            }
        }
    }
    private bool OtherPlanCreations() {
        int chance = UnityEngine.Random.Range(0, 100);
        if (GetNormalTrait("Berserker") != null) {
            if (chance < 15) {
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
    public bool PlanFullnessRecoveryActions(bool processOverrideLogic) {
        if(doNotDisturb > 0 || isWaitingForInteraction > 0) {
            return false;
        }
        if (isStarving) {
            //If there is already a HUNGER_RECOVERY JOB and the character becomes Starving, replace HUNGER_RECOVERY with HUNGER_RECOVERY_STARVING only if that character is not doing the job already
            JobQueueItem hungerRecoveryJob = jobQueue.GetJob(JOB_TYPE.HUNGER_RECOVERY);
            if (hungerRecoveryJob != null) {
                //Replace this with Hunger Recovery Starving only if the character is not doing the Hunger Recovery Job already
                if (currentAction != null && currentAction.parentPlan != null && currentAction.parentPlan.job != null && currentAction.parentPlan.job == hungerRecoveryJob) {
                    return false;
                } else {
                    jobQueue.CancelJob(hungerRecoveryJob);
                }
            }
            if (!jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
                JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY_STARVING;
                GoapPlanJob job = new GoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this });
                if (GetNormalTrait("Vampiric") != null) {
                    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                } else if (GetNormalTrait("Cannibal") != null) {
                    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                }
                job.SetCancelOnFail(true);
                jobQueue.AddJobInQueue(job, processOverrideLogic);
                return true;
            }
        }
        else if (isHungry) {
            if(UnityEngine.Random.Range(0,2) == 0 && GetNormalTrait("Glutton") != null) {
                JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY;
                GoapPlanJob job = new GoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this });
                if (GetNormalTrait("Vampiric") != null) {
                    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                } else if (GetNormalTrait("Cannibal") != null) {
                    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                }
                job.SetCancelOnFail(true);
                jobQueue.AddJobInQueue(job, processOverrideLogic);
                return true;
            }
        }
        return false;
    }
    public bool PlanTirednessRecoveryActions(bool processOverrideLogic) {
        if (doNotDisturb > 0 || isWaitingForInteraction > 0) {
            return false;
        }
        if (isExhausted) {
            //If there is already a TIREDNESS_RECOVERY JOB and the character becomes Exhausted, replace TIREDNESS_RECOVERY with TIREDNESS_RECOVERY_STARVING only if that character is not doing the job already
            JobQueueItem tirednessRecoveryJob = jobQueue.GetJob(JOB_TYPE.TIREDNESS_RECOVERY);
            if (tirednessRecoveryJob != null) {
                //Replace this with Tiredness Recovery Exhausted only if the character is not doing the Tiredness Recovery Job already
                if (currentAction != null && currentAction.parentPlan != null && currentAction.parentPlan.job != null && currentAction.parentPlan.job == tirednessRecoveryJob) {
                    return false;
                } else {
                    jobQueue.CancelJob(tirednessRecoveryJob);
                }
            }
            if (!jobQueue.HasJob(JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
                GoapPlanJob job = new GoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = this });
                job.SetCancelOnFail(true);
                jobQueue.AddJobInQueue(job, processOverrideLogic);
                return true;
            } 
        }
        return false;
    }
    public bool ForcePlanHappinessRecoveryActions() {
        if (!jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
            JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY;
            if (isForlorn) {
                jobType = JOB_TYPE.HAPPINESS_RECOVERY_FORLORN;
            }
            GoapPlanJob job = new GoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = this });
            job.SetCancelOnFail(true);
            jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool PlanHappinessRecoveryActions(bool processOverrideLogic) {
        if (doNotDisturb > 0 || isWaitingForInteraction > 0) {
            return false;
        }
        if (isForlorn) {
            //If there is already a HUNGER_RECOVERY JOB and the character becomes Starving, replace HUNGER_RECOVERY with HUNGER_RECOVERY_STARVING only if that character is not doing the job already
            JobQueueItem happinessRecoveryJob = jobQueue.GetJob(JOB_TYPE.HAPPINESS_RECOVERY);
            if (happinessRecoveryJob != null) {
                //Replace this with Hunger Recovery Starving only if the character is not doing the Hunger Recovery Job already
                if (currentAction != null && currentAction.parentPlan != null && currentAction.parentPlan.job != null && currentAction.parentPlan.job == happinessRecoveryJob) {
                    return false;
                } else {
                    jobQueue.CancelJob(happinessRecoveryJob);
                }
            }
            if (!jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                Hardworking hardworking = GetNormalTrait("Hardworking") as Hardworking;
                if (hardworking != null) {
                    bool isPlanningRecoveryProcessed = false;
                    if (hardworking.ProcessHardworkingTrait(this, ref isPlanningRecoveryProcessed)) {
                        return isPlanningRecoveryProcessed;
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY_FORLORN;
                GoapPlanJob job = new GoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = this });
                job.SetCancelOnFail(true);
                jobQueue.AddJobInQueue(job, processOverrideLogic);
                return true;
            }
        } else if (isLonely) {
            if (!jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY;
                Hardworking hardworking = GetNormalTrait("Hardworking") as Hardworking;
                if(hardworking != null) {
                    bool isPlanningRecoveryProcessed = false;
                    if(hardworking.ProcessHardworkingTrait(this, ref isPlanningRecoveryProcessed)) {
                        return isPlanningRecoveryProcessed;
                    }
                }
                int chance = UnityEngine.Random.Range(0, 100);
                int value = 0;
                TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick();
                if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                    value = 30;
                } else if (currentTimeInWords == TIME_IN_WORDS.LUNCH_TIME) {
                    value = 45;
                } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                    value = 45;
                } else if (currentTimeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
                    value = 45;
                } else if (currentTimeInWords == TIME_IN_WORDS.LATE_NIGHT) {
                    value = 30;
                }
                if (chance < value) {
                    GoapPlanJob job = new GoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = this });
                    job.SetCancelOnFail(true);
                    jobQueue.AddJobInQueue(job, processOverrideLogic);
                    return true;
                }
            }
        }
        return false;
    }
    private void PlanForcedFullnessRecovery() {
        if (!hasForcedFullness && fullnessForcedTick != 0 && GameManager.Instance.tick >= fullnessForcedTick && _doNotDisturb <= 0) {
            if (!jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
                JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY;
                if (isStarving) {
                    jobType = JOB_TYPE.HUNGER_RECOVERY_STARVING;
                }
                GoapPlanJob job = new GoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this });
                if (GetNormalTrait("Vampiric") != null) {
                    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                } else if (GetNormalTrait("Cannibal") != null) {
                    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                }
                job.SetCancelOnFail(true);
                bool willNotProcess = _numOfWaitingForGoapThread > 0 || !IsInOwnParty() || isDefender || isWaitingForInteraction > 0
                    || stateComponent.currentState != null || stateComponent.stateToDo != null;
                jobQueue.AddJobInQueue(job, !willNotProcess);
            }
            hasForcedFullness = true;
            SetFullnessForcedTick();
        }
    }
    private void PlanForcedTirednessRecovery() {
        if (!hasForcedTiredness && tirednessForcedTick != 0 && GameManager.Instance.tick >= tirednessForcedTick && _doNotDisturb <= 0) {
            if (!jobQueue.HasJob(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY;
                if (isExhausted) {
                    jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
                }
                GoapPlanJob job = new GoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = this });
                job.SetCancelOnFail(true);
                sleepScheduleJobID = job.id;
                bool willNotProcess = _numOfWaitingForGoapThread > 0 || !IsInOwnParty() || isDefender || isWaitingForInteraction > 0
                    || stateComponent.currentState != null || stateComponent.stateToDo != null;
                jobQueue.AddJobInQueue(job, !willNotProcess);
            }
            hasForcedTiredness = true;
            SetTirednessForcedTick();
        }
        //If a character current sleep ticks is less than the default, this means that the character already started sleeping but was awaken midway that is why he/she did not finish the allotted sleeping time
        //When this happens, make sure to queue tiredness recovery again so he can finish the sleeping time
        else if((hasCancelledSleepSchedule || currentSleepTicks < CharacterManager.Instance.defaultSleepTicks) && _doNotDisturb <= 0) {
            if (!jobQueue.HasJob(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY;
                if (isExhausted) {
                    jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
                }
                GoapPlanJob job = new GoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = this });
                job.SetCancelOnFail(true);
                sleepScheduleJobID = job.id;
                //bool willNotProcess = _numOfWaitingForGoapThread > 0 || !IsInOwnParty() || isDefender || isWaitingForInteraction > 0
                //    || stateComponent.currentState != null || stateComponent.stateToDo != null;
                jobQueue.AddJobInQueue(job); //!willNotProcess
            }
            SetHasCancelledSleepSchedule(false);
        }
    }
    public void SetHasCancelledSleepSchedule(bool state) {
        hasCancelledSleepSchedule = state;
    }
    private bool PlanWorkActions() { //ref bool hasAddedToGoapPlans
        if (GetPlanByCategory(GOAP_CATEGORY.WORK) == null) {
            if (!jobQueue.ProcessFirstJobInQueue(this)) {
                if (isAtHomeArea && isPartOfHomeFaction) { //&& this.faction.id != FactionManager.Instance.neutralFaction.id
                    if(GetNormalTrait("Lazy") != null) {
                        if(UnityEngine.Random.Range(0, 100) < 35) {
                            if (!ForcePlanHappinessRecoveryActions()) {
                                PrintLogIfActive(GameManager.Instance.TodayLogString() + "Triggered LAZY happiness recovery but " + name + " already has that job type in queue and will not do it anymore!");
                            }
                        } else {
                            if (!homeArea.jobQueue.ProcessFirstJobInQueue(this)) {
                                if(faction != null && faction.activeQuest != null) {
                                    return faction.activeQuest.jobQueue.ProcessFirstJobInQueue(this);
                                }
                                return false;
                            } else {
                                return true;
                            }
                        }
                    } else {
                        if (!homeArea.jobQueue.ProcessFirstJobInQueue(this)) {
                            if (faction != null && faction.activeQuest != null) {
                                return faction.activeQuest.jobQueue.ProcessFirstJobInQueue(this);
                            }
                            return false;
                        } else {
                            return true;
                        }
                    }
                } else {
                    return false;
                }
            } else {
                return true;
            }
        }
        return false;
    }
    private bool PlanJobQueueFirst() {
        if (GetPlanByCategory(GOAP_CATEGORY.WORK) == null && !isStarving && !isExhausted && !isForlorn) {
            if (!jobQueue.ProcessFirstJobInQueue(this)) {
                if (isAtHomeArea && isPartOfHomeFaction) {
                    if (GetNormalTrait("Lazy") != null) {
                        if (UnityEngine.Random.Range(0, 100) < 35) {
                            if (!ForcePlanHappinessRecoveryActions()) {
                                PrintLogIfActive(GameManager.Instance.TodayLogString() + "Triggered LAZY happiness recovery but " + name + " already has that job type in queue and will not do it anymore!");
                            }
                        } else {
                            if (!homeArea.jobQueue.ProcessFirstJobInQueue(this)) {
                                if (faction != null && faction.activeQuest != null) {
                                    return faction.activeQuest.jobQueue.ProcessFirstJobInQueue(this);
                                }
                                return false;
                            } else {
                                return true;
                            }
                        }
                    } else {
                        if (!homeArea.jobQueue.ProcessFirstJobInQueue(this)) {
                            if (faction != null && faction.activeQuest != null) {
                                return faction.activeQuest.jobQueue.ProcessFirstJobInQueue(this);
                            }
                            return false;
                        } else {
                            return true;
                        }
                    }
                } else {
                    return false;
                }
            } else {
                return true;
            }
        }
        return false;
    }
    public bool PlanIdleStroll(LocationStructure targetStructure, LocationGridTile targetTile = null) {
        if (currentStructure == targetStructure) {
            stateComponent.SwitchToState(CHARACTER_STATE.STROLL);
        } else {
            MoveToAnotherStructure(targetStructure, targetTile, null, () => stateComponent.SwitchToState(CHARACTER_STATE.STROLL));
        }
        return true;
    }
    public bool PlanIdleStrollOutside(LocationStructure targetStructure, LocationGridTile targetTile = null) {
        if (currentStructure == targetStructure) {
            stateComponent.SwitchToState(CHARACTER_STATE.STROLL_OUTSIDE);
        } else {
            MoveToAnotherStructure(targetStructure, targetTile, null, () => stateComponent.SwitchToState(CHARACTER_STATE.STROLL_OUTSIDE));
        }
        return true;
    }
    public bool PlanIdleReturnHome() {
        //if (GetTrait("Berserker") != null) {
        //    //Return home becomes stroll if the character has berserker trait
        //    PlanIdleStroll(currentStructure);
        //} else {
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.RETURN_HOME, this, this);
        goapAction.SetTargetStructure();
        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        goapPlan.ConstructAllNodes();
        //AddPlan(goapPlan, true);
        AddPlan(goapPlan);
        //PlanGoapActions(goapAction);
        //}
        return true;
    }
    private string OtherIdlePlans() {
        string log = GameManager.Instance.TodayLogString() + " IDLE PLAN FOR " + name;
        if (isDead) {
            log += this.name + " is already dead not planning other idle plans.";
            return log;
        }
        if (faction.id != FactionManager.Instance.neutralFaction.id) {
            CreatePersonalJobs();
            //NPC with Faction Idle
            log += "\n-" + name + " has a faction";
            if (previousCurrentAction != null && previousCurrentAction.goapType == INTERACTION_TYPE.RETURN_HOME && currentStructure == homeStructure) {
                log += "\n-" + name + " is in home structure and just returned home";
                TileObject deskOrTable = currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                log += "\n-Sit if there is still an unoccupied Table or Desk in the current location";
                if (deskOrTable != null) {
                    log += "\n  -" + name + " will do action Sit on " + deskOrTable.ToString();
                    PlanIdle(INTERACTION_TYPE.SIT, deskOrTable);
                } else {
                    log += "\n-Otherwise, stand idle";
                    log += "\n  -" + name + " will do action Stand";
                    PlanIdle(INTERACTION_TYPE.STAND, this);
                }
                return log;
            } else if (currentStructure == homeStructure) {
                log += "\n-" + name + " is in home structure and previous action is not returned home";
                TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick();

                log += "\n-If it is Early Night, 35% chance to go to the current Inn and then set it as the Base Structure for 2.5 hours";
                if (currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += "\n  -RNG roll: " + chance;
                    if (chance < 35) {
                        //StartGOAP(INTERACTION_TYPE.DRINK, null, GOAP_CATEGORY.IDLE);
                        LocationStructure structure = specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.INN);
                        if(structure != null) {
                            log += "\n  -Early Night: " + name + " will go to Inn and set Base Structure for 2.5 hours";
                            LocationGridTile gridTile = structure.GetRandomTile();
                            marker.GoTo(gridTile, () => trapStructure.SetStructureAndDuration(structure, GameManager.Instance.GetTicksBasedOnHour(2) + GameManager.Instance.GetTicksBasedOnMinutes(30)));
                            return log;
                        } else {
                            log += "\n  -No Inn Structure in the area";
                        }
                    }
                } else {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                }
                log += "\n-Otherwise, if it is Afternoon, 25% chance to nap if there is still an unoccupied Bed in the house";
                if (currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += "\n  -RNG roll: " + chance;
                    if (chance < 25) {
                        TileObject bed = currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                        if (bed != null) {
                            if(GetNormalTrait("Vampiric") != null) {
                                log += "\n  -Character is vampiric, cannot do nap action";
                            } else {
                                log += "\n  -Afternoon: " + name + " will do action Nap on " + bed.ToString();
                                PlanIdle(INTERACTION_TYPE.NAP, bed);
                                return log;
                            }
                        } else {
                            log += "\n  -No unoccupied bed in the current structure";
                        }
                    }
                } else {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                }
                log += "\n-Otherwise, if it is Morning or Afternoon or Early Night, 25% chance to enter Stroll Outside Mode for 1 hour";
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += "\n  -RNG roll: " + chance;
                    if (chance < 25) {
                        log += "\n  -Morning, Afternoon, or Early Night: " + name + " will enter Stroll Outside Mode";
                        PlanIdleStrollOutside(currentStructure);
                        return log;
                    }
                } else {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                }
                log += "\n-Otherwise, if it is Morning or Afternoon, 25% chance to someone with a positive relationship in current location and then set it as the Base Structure for 2.5 hours";
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += "\n  -RNG roll: " + chance;
                    if (chance < 25) {
                        List<Character> positiveCharacters = GetCharactersWithRelationship(TRAIT_EFFECT.POSITIVE);
                        if(positiveCharacters.Count > 0) {
                            Character chosenCharacter = positiveCharacters[UnityEngine.Random.Range(0, positiveCharacters.Count)];
                            if (chosenCharacter.homeStructure != null) {
                                log += "\n  -Morning or Afternoon: " + name + " will go to dwelling of character with positive relationship and set Base Structure for 2.5 hours";
                                LocationGridTile gridTile = chosenCharacter.homeStructure.GetRandomTile();
                                marker.GoTo(gridTile, () => trapStructure.SetStructureAndDuration(chosenCharacter.homeStructure, GameManager.Instance.GetTicksBasedOnHour(2) + GameManager.Instance.GetTicksBasedOnMinutes(30)));
                                return log;
                            } else {
                                log += "\n  -Chosen Character: " + chosenCharacter.name + " has no home structure";
                            }
                        } else {
                            log += "\n  -No character with positive relationship";
                        }
                    }
                } else {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                }
                //if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                //    int chance = UnityEngine.Random.Range(0, 100);
                //    if (chance < 15) {
                //        log += "\n-Morning or Afternoon: " + name + " will do action Daydream";
                //        PlanIdle(INTERACTION_TYPE.DAYDREAM, this);
                //        return log;
                //    }
                //}
                //int guitarChance = UnityEngine.Random.Range(0, 100);
                //if (guitarChance < 15) {
                //    TileObject guitar = GetUnoccupiedHomeTileObject(TILE_OBJECT_TYPE.GUITAR);
                //    if (guitar != null) {
                //        log += "\n-" + name + " will do action Play Guitar on " + guitar.ToString();
                //        PlanIdle(INTERACTION_TYPE.PLAY_GUITAR, guitar);
                //        return log;
                //    }
                //}

                log += "\n-Otherwise, sit if there is still an unoccupied Table or Desk";
                TileObject deskOrTable = currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                if (deskOrTable != null) {
                    log += "\n  -" + name + " will do action Sit on " + deskOrTable.ToString();
                    PlanIdle(INTERACTION_TYPE.SIT, deskOrTable);
                    return log;
                } else {
                    log += "\n  -No unoccupied Table or Desk";
                }

                log += "\n-Otherwise, stand idle";
                log += "\n  -" + name + " will do action Stand";
                PlanIdle(INTERACTION_TYPE.STAND, this);
                //PlanIdleStroll(currentStructure);
                return log;
            } else if ((currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA || currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || currentStructure.structureType == STRUCTURE_TYPE.CEMETERY) && specificLocation == homeArea) {
                log += "\n-" + name + " is in the Work Area/Wilderness/Cemetery of home location";

                log += "\n-If it is Morning or Afternoon, 25% chance to enter Stroll Outside Mode";
                TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick();
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += "\n  -RNG roll: " + chance;
                    if (chance < 25) {
                        log += "\n  -Morning or Afternoon: " + name + " will enter Stroll Outside State";
                        PlanIdleStrollOutside(currentStructure);
                        return log;
                    }
                } else {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                }
                //log += "\n-Otherwise, if it is Early Night, 35% chance to drink at the Inn";
                //if (currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                //    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                //    int chance = UnityEngine.Random.Range(0, 100);
                //    log += "\n  -RNG roll: " + chance;
                //    if (chance < 35) {
                //        log += "\n  -Early Night: " + name + " will do action Drink (multithreaded)";
                //        StartGOAP(INTERACTION_TYPE.DRINK, null, GOAP_CATEGORY.IDLE);
                //        return log;
                //    }
                //} else {
                //    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                //}
                log += "\n-Otherwise, return home";
                log += "\n  -" + name + " will do action Return Home";
                PlanIdleReturnHome();
                return log;
            } else if (trapStructure.structure != null && currentStructure == trapStructure.structure) {
                log += "\n-" + name + "'s Base Structure is not empty and current structure is the Base Structure";
                log += "\n-15% chance to trigger a Chat conversation if there is anyone chat-compatible in range";
                int chance = UnityEngine.Random.Range(0, 100);
                log += "\n  -RNG roll: " + chance;
                if (chance < 15) {
                    if(marker.inVisionCharacters.Count > 0) {
                        bool hasForcedChat = false;
                        for (int i = 0; i < marker.inVisionCharacters.Count; i++) {
                            Character targetCharacter = marker.inVisionCharacters[i];
                            if (marker.visionCollision.ForceChatHandling(targetCharacter)) {
                                log += "\n  -Chat with: " + targetCharacter.name;
                                hasForcedChat = true;
                                break;
                            }
                        }
                        if (hasForcedChat) {
                            return log;
                        } else {
                            log += "\n  -Could not chat with anyone in vision";
                        }
                    } else {
                        log += "\n  -No characters in vision";
                    }
                }
                log += "\n-Sit if there is still an unoccupied Table or Desk";
                TileObject deskOrTable = currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                if (deskOrTable != null) {
                    log += "\n  -" + name + " will do action Sit on " + deskOrTable.ToString();
                    PlanIdle(INTERACTION_TYPE.SIT, deskOrTable);
                    return log;
                } else {
                    log += "\n  -No unoccupied Table or Desk";
                }
                log += "\n-Otherwise, stand idle";
                log += "\n  -" + name + " will do action Stand";
                PlanIdle(INTERACTION_TYPE.STAND, this);
                return log;
            } else if (((currentStructure.structureType == STRUCTURE_TYPE.DWELLING && currentStructure != homeStructure) 
                || currentStructure.structureType == STRUCTURE_TYPE.INN 
                || currentStructure.structureType == STRUCTURE_TYPE.WAREHOUSE
                || currentStructure.structureType == STRUCTURE_TYPE.PRISON
                || currentStructure.structureType == STRUCTURE_TYPE.CEMETERY) 
                && trapStructure.structure == null) {
                log += "\n-" + name + " is in another Dwelling/Inn/Warehouse/Prison/Cemetery and Base Structure is empty";
                log += "\n-100% chance to return home";
                PlanIdleReturnHome();
                return log;
            } else if (specificLocation != homeArea && trapStructure.structure == null) {
                log += "\n-" + name + " is in another area and Base Structure is empty";
                log += "\n-100% chance to return home";
                PlanIdleReturnHome();
                return log;
            }
        } else {
            //Unaligned NPC Idle
            log += "\n-" + name + " has no faction";
            if (!isAtHomeArea) {
                log += "\n-" + name + " is in another area";
                log += "\n-100% chance to return home";
                PlanIdleReturnHome();
                return log;
            } else {
                log += "\n-" + name + " is in home area";
                log += "\n-If it is Morning or Afternoon, 25% chance to play";
                TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick();
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += "\n  -RNG roll: " + chance;
                    if (chance < 25) {
                        log += "\n  -Morning or Afternoon: " + name + " will do action Play";
                        PlanIdle(INTERACTION_TYPE.PLAY, this);
                        return log;
                    }
                } else {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                }

                log += "\n-Otherwise, enter stroll mode";
                log += "\n  -" + name + " will enter Stroll Mode";
                PlanIdleStroll(currentStructure);
                return log;
            }
        }
        return log;

        //if (homeStructure != null) {
        //    if(currentStructure.structureType == STRUCTURE_TYPE.DWELLING) {
        //        if(currentStructure != homeStructure) {
        //            PlanIdleReturnHome();
        //        } else {
        //            PlanIdleStroll(currentStructure);
        //        }
        //    } else {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        int returnHomeChance = 0;
        //        if (specificLocation == homeArea && currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA) {
        //            returnHomeChance = 25;
        //        } else {
        //            returnHomeChance = 80;
        //        }
        //        if (chance < returnHomeChance) {
        //            PlanIdleReturnHome();
        //        } else {
        //            PlanIdleStroll(currentStructure);
        //        }
        //    }
        //} else {
        //    PlanIdleStroll(currentStructure);
        //}
    }
    private void PlanIdle(INTERACTION_TYPE type, IPointOfInterest target) {
        if (target != this) {
            AddAwareness(target);
        }
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(type, this, target);
        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        goapPlan.ConstructAllNodes();
        AddPlan(goapPlan);
        //PlanGoapActions(goapAction);
    }
    //public bool AssaultCharacter(Character target) {
    //    //Debug.Log(this.name + " will assault " + target.name);
    //    hasAssaultPlan = true;
    //    lastAssaultedCharacter = target;
    //    //Debug.Log("---------" + GameManager.Instance.TodayLogString() + "CREATING IDLE STROLL ACTION FOR " + name + "-------------");
    //    //if(currentStructure.unoccupiedTiles.Count > 0) {
    //    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.ASSAULT_ACTION_NPC, this, target);
    //    goapAction.SetEndAction(OnAssaultActionReturn);
    //    //goapAction.SetTargetStructure(target);
    //    GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
    //    GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.DEATH }, GOAP_CATEGORY.REACTION);
    //    goapPlan.ConstructAllNodes();
    //    goapPlan.SetDoNotRecalculate(true);
    //    AddPlan(goapPlan, true);
    //    if (currentAction != null) {
    //        currentAction.StopAction();
    //    }
    //    return true;
    //    //}
    //    //return false;
    //}
    private void SetLastAssaultedCharacter(Character character) {
        lastAssaultedCharacter = character;
        if (character != null) {
            //cooldown
            GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.ticksPerHour);
            SchedulingManager.Instance.AddEntry(dueDate, () => RemoveLastAssaultedCharacter(character), this);
        }
    }
    private void RemoveLastAssaultedCharacter(Character characterToRemove) {
        if (lastAssaultedCharacter == characterToRemove) {
            SetLastAssaultedCharacter(null);
        }
    }
    public void ChatCharacter(Character targetCharacter) {
        ChatCharacter chatAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.CHAT_CHARACTER, this, targetCharacter) as ChatCharacter;
        chatAction.PerformActualAction();
    }
    public float GetFlirtationWeightWith(Character targetCharacter, CharacterRelationshipData relData, params CHARACTER_MOOD[] moods) {
        float positiveFlirtationWeight = 0;
        float negativeFlirtationWeight = 0;
        for (int i = 0; i < moods.Length; i++) {
            CHARACTER_MOOD mood = moods[i];
            switch (mood) {
                case CHARACTER_MOOD.DARK:
                    //-100 Weight per Dark Mood
                    negativeFlirtationWeight -= 100;
                    break;
                case CHARACTER_MOOD.BAD:
                    //-50 Weight per Bad Mood
                    negativeFlirtationWeight -= 50;
                    break;
                case CHARACTER_MOOD.GOOD:
                    //+10 Weight per Good Mood
                    positiveFlirtationWeight += 10;
                    break;
                case CHARACTER_MOOD.GREAT:
                    //+30 Weight per Great Mood
                    positiveFlirtationWeight += 30;
                    break;
            }
        }
        if (relData != null) {
            //+10 Weight per previous flirtation
            positiveFlirtationWeight += 10 * relData.flirtationCount;
        }
        //x2 all positive modifiers per Drunk
        if (GetNormalTrait("Drunk") != null) {
            positiveFlirtationWeight *= 2;
        }
        if (targetCharacter.GetNormalTrait("Drunk") != null) {
            positiveFlirtationWeight *= 2;
        }

        Unfaithful unfaithful = GetNormalTrait("Unfaithful") as Unfaithful;
        //x0.5 all positive modifiers per negative relationship
        if (GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
            positiveFlirtationWeight *= 0.5f;
        }
        if (targetCharacter.GetRelationshipEffectWith(this) == RELATIONSHIP_EFFECT.NEGATIVE) {
            positiveFlirtationWeight *= 0.5f;
        }
        //x0.1 all positive modifiers per sexually incompatible
        if (!CharacterManager.Instance.IsSexuallyCompatibleOneSided(this, targetCharacter)) {
            positiveFlirtationWeight *= 0.1f;
        }
        // x6 if initiator is Unfaithful and already has a lover
        else if (unfaithful != null && (relData == null || !relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.LOVER))) {
            positiveFlirtationWeight *= 6f;
            positiveFlirtationWeight *= unfaithful.affairChanceMultiplier;
        }
        if (!CharacterManager.Instance.IsSexuallyCompatibleOneSided(targetCharacter, this)) {
            positiveFlirtationWeight *= 0.1f;
        }
        bool thisIsUgly = GetNormalTrait("Ugly") != null;
        bool otherIsUgly = targetCharacter.GetNormalTrait("Ugly") != null;
        if (thisIsUgly != otherIsUgly) { //if at least one of the characters are ugly
            positiveFlirtationWeight *= 0.75f;
        }
        return positiveFlirtationWeight + negativeFlirtationWeight;
    }
    public float GetBecomeLoversWeightWith(Character targetCharacter, CharacterRelationshipData relData, params CHARACTER_MOOD[] moods) {
        float positiveWeight = 0;
        float negativeWeight = 0;
        if (GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.NEGATIVE && targetCharacter.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.NEGATIVE
            && CanHaveRelationshipWith(RELATIONSHIP_TRAIT.LOVER, targetCharacter) && targetCharacter.CanHaveRelationshipWith(RELATIONSHIP_TRAIT.LOVER, this)
            && role.roleType != CHARACTER_ROLE.BEAST && targetCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
            for (int i = 0; i < moods.Length; i++) {
                CHARACTER_MOOD mood = moods[i];
                switch (mood) {
                    case CHARACTER_MOOD.DARK:
                        //-30 Weight per Dark Mood
                        negativeWeight -= 30;
                        break;
                    case CHARACTER_MOOD.BAD:
                        //-10 Weight per Bad Mood
                        negativeWeight -= 10;
                        break;
                    case CHARACTER_MOOD.GOOD:
                        //+5 Weight per Good Mood
                        positiveWeight += 5;
                        break;
                    case CHARACTER_MOOD.GREAT:
                        //+10 Weight per Great Mood
                        positiveWeight += 10;
                        break;
                }
            }
            if (relData != null) {
                //+30 Weight per previous flirtation
                positiveWeight += 30 * relData.flirtationCount;
            }
            //x2 all positive modifiers per Drunk
            if (GetNormalTrait("Drunk") != null) {
                positiveWeight *= 2;
            }
            if (targetCharacter.GetNormalTrait("Drunk") != null) {
                positiveWeight *= 2;
            }
            //x0.1 all positive modifiers per sexually incompatible
            if (!CharacterManager.Instance.IsSexuallyCompatibleOneSided(this, targetCharacter)) {
                positiveWeight *= 0.1f;
            }
            if (!CharacterManager.Instance.IsSexuallyCompatibleOneSided(targetCharacter, this)) {
                positiveWeight *= 0.1f;
            }
            //x0 if a character is a beast
            //added to initial checking instead.

            bool thisIsUgly = GetNormalTrait("Ugly") != null;
            bool otherIsUgly = targetCharacter.GetNormalTrait("Ugly") != null;
            if (thisIsUgly != otherIsUgly) { //if at least one of the characters are ugly
                positiveWeight *= 0.75f;
            }
        }
        return positiveWeight + negativeWeight;
    }
    public float GetBecomeParamoursWeightWith(Character targetCharacter, CharacterRelationshipData relData, params CHARACTER_MOOD[] moods) {
        //**if they dont have a negative relationship and at least one of them has a lover, they may become paramours**
        float positiveWeight = 0;
        float negativeWeight = 0;
        if (GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.NEGATIVE && targetCharacter.GetRelationshipEffectWith(this) != RELATIONSHIP_EFFECT.NEGATIVE
            && CanHaveRelationshipWith(RELATIONSHIP_TRAIT.PARAMOUR, targetCharacter) && targetCharacter.CanHaveRelationshipWith(RELATIONSHIP_TRAIT.PARAMOUR, this)
            && role.roleType != CHARACTER_ROLE.BEAST && targetCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
            for (int i = 0; i < moods.Length; i++) {
                CHARACTER_MOOD mood = moods[i];
                switch (mood) {
                    case CHARACTER_MOOD.DARK:
                        //-30 Weight per Dark Mood
                        negativeWeight -= 30;
                        break;
                    case CHARACTER_MOOD.BAD:
                        //-10 Weight per Bad Mood
                        negativeWeight -= 10;
                        break;
                    case CHARACTER_MOOD.GOOD:
                        //+5 Weight per Good Mood
                        positiveWeight += 5;
                        break;
                    case CHARACTER_MOOD.GREAT:
                        //+10 Weight per Great Mood
                        positiveWeight += 20;
                        break;
                }
            }
            if (relData != null) {
                //+30 Weight per previous flirtation
                positiveWeight += 50 * relData.flirtationCount;
            }
            Unfaithful unfaithful = GetNormalTrait("Unfaithful") as Unfaithful;
            //x2 all positive modifiers per Drunk
            if (GetNormalTrait("Drunk") != null) {
                positiveWeight *= 2.5f;
            }
            if (targetCharacter.GetNormalTrait("Drunk") != null) {
                positiveWeight *= 2.5f;
            }
            //x0.1 all positive modifiers per sexually incompatible
            if (!CharacterManager.Instance.IsSexuallyCompatibleOneSided(this, targetCharacter)) {
                positiveWeight *= 0.1f;
            }
            // x4 if initiator is Unfaithful and already has a lover
            else if (unfaithful != null && (relData == null || !relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.LOVER))) {
                positiveWeight *= 4f;
                positiveWeight *= unfaithful.affairChanceMultiplier;
            }

            if (!CharacterManager.Instance.IsSexuallyCompatibleOneSided(targetCharacter, this)) {
                positiveWeight *= 0.1f;
            }
            Character lover = GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
            //x3 all positive modifiers if character considers lover as Enemy
            if (lover != null && HasRelationshipOfTypeWith(lover, RELATIONSHIP_TRAIT.ENEMY)) {
                positiveWeight *= 3f;
            }
            if (HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.RELATIVE)) {
                positiveWeight *= 0.01f;
            }
            if (lover != null && lover.GetNormalTrait("Ugly") != null) { //if lover is ugly
                positiveWeight += positiveWeight * 0.75f;
            }
            //x0 if a character has a lover and does not have the Unfaithful trait
            if ((HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER, false) && GetNormalTrait("Unfaithful") == null) 
                || (targetCharacter.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER, false) && targetCharacter.GetNormalTrait("Unfaithful") == null)) {
                positiveWeight *= 0;
                negativeWeight *= 0;
            }
            //x0 if a character is a beast
            //added to initial checking instead.
        }
        return positiveWeight + negativeWeight;
    }
    public void EndChatCharacter() {
        SetIsChatting(false);
        //targetCharacter.SetIsChatting(false);
        SetIsFlirting(false);
        //targetCharacter.SetIsFlirting(false);
        marker.UpdateActionIcon();
        //targetCharacter.marker.UpdateActionIcon();
    }
    public void SetIsChatting(bool state) {
        _isChatting = state;
    }
    public void SetIsFlirting(bool state) {
        _isFlirting = state;
    }
    public void AddAdvertisedAction(INTERACTION_TYPE type) {
        poiGoapActions.Add(type);
    }
    public void RemoveAdvertisedAction(INTERACTION_TYPE type) {
        poiGoapActions.Add(type);
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
            } else {
                if (token.characterOwner == null) {
                    token.SetCharacterOwner(this);
                }
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
        token.OnConsumeToken(this);
        if (token.uses <= 0) {
            return RemoveToken(token);
        }
        return false;
    }
    private bool AddToken(SpecialToken token) {
        if (!items.Contains(token)) {
            items.Add(token);
            Messenger.Broadcast(Signals.CHARACTER_OBTAINED_ITEM, token, this);
            return true;
        }
        return false;
    }
    private bool RemoveToken(SpecialToken token) {
        if (items.Remove(token)) {
            Messenger.Broadcast(Signals.CHARACTER_LOST_ITEM, token, this);
            return true;
        }
        return false;
    }
    public void DropToken(SpecialToken token, Area location, LocationStructure structure, LocationGridTile gridTile = null, bool clearOwner = true) {
        if (UnobtainToken(token)) {
            if (token.specialTokenType.CreatesObjectWhenDropped()) {
                if (location.AddSpecialTokenToLocation(token, structure, gridTile)) {
                    //When items are dropped into the warehouse, make all residents aware of it.
                    if (structure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
                        for (int i = 0; i < structure.location.areaResidents.Count; i++) {
                            Character resident = structure.location.areaResidents[i];
                            resident.AddAwareness(token);
                        }
                    }
                }
            }
            //if (structure != homeStructure) {
            //    //if this character drops this at a structure that is not his/her home structure, set the owner of the item to null
            //    token.SetCharacterOwner(null);
            //}
            if (clearOwner) {
                token.SetCharacterOwner(null);
            }
        }
    }
    public void DropAllTokens(Area location, LocationStructure structure, LocationGridTile tile, bool removeFactionOwner = false) {
        while (isHoldingItem) {
            SpecialToken token = items[0];
            if (UnobtainToken(token)) {
                if (removeFactionOwner) {
                    token.SetOwner(null);
                }
                if (token.specialTokenType.CreatesObjectWhenDropped()) {
                    LocationGridTile targetTile = tile.GetNearestUnoccupiedTileFromThis();
                    location.AddSpecialTokenToLocation(token, structure, targetTile);
                    if (structure != homeStructure) {
                        //if this character drops this at a structure that is not his/her home structure, set the owner of the item to null
                        token.SetCharacterOwner(null);
                    }
                } else {
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
            if (items[i] == token) {
                return items[i];
            }
        }
        return null;
    }
    public SpecialToken GetToken(SPECIAL_TOKEN token) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].specialTokenType == token) {
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
    public List<SpecialToken> GetItemsOwned() {
        List<SpecialToken> itemsOwned = new List<SpecialToken>();
        //for (int i = 0; i < homeArea.possibleSpecialTokenSpawns.Count; i++) {
        //    SpecialToken token = homeArea.possibleSpecialTokenSpawns[i];
        //    if (token.characterOwner == this) {
        //        itemsOwned.Add(token);
        //    }
        //}
        for (int i = 0; i < homeStructure.itemsInStructure.Count; i++) {
            SpecialToken token = homeStructure.itemsInStructure[i];
            if (token.characterOwner == this) {
                itemsOwned.Add(token);
            }
        }
        for (int i = 0; i < items.Count; i++) {
            SpecialToken token = items[i];
            if (token.characterOwner == this) {
                itemsOwned.Add(token);
            }
        }
        return itemsOwned;
    }
    public int GetNumOfItemsOwned() {
        int count = 0;
        //for (int i = 0; i < homeArea.possibleSpecialTokenSpawns.Count; i++) {
        //    SpecialToken token = homeArea.possibleSpecialTokenSpawns[i];
        //    if (token.characterOwner == this) {
        //        count++;
        //    }
        //}
        for (int i = 0; i < homeStructure.itemsInStructure.Count; i++) {
            SpecialToken token = homeStructure.itemsInStructure[i];
            if (token.characterOwner == this) {
                count++;
            }
        }
        
        for (int i = 0; i < items.Count; i++) {
            SpecialToken token = items[i];
            if (token.characterOwner == this) {
                count++;
            }
        }
        return count;
    }
    public bool HasTokenInInventory(SPECIAL_TOKEN tokenType) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].specialTokenType == tokenType) {
                return true;
            }
        }
        return false;
    }
    public int GetTokenCountInInventory(SPECIAL_TOKEN tokenType) {
        int count = 0;
        for (int i = 0; i < items.Count; i++) {
            if (items[i].specialTokenType == tokenType) {
                count++;
            }
        }
        return count;
    }
    public bool HasExtraTokenInInventory(SPECIAL_TOKEN tokenType) {
        if (role.IsRequiredItem(tokenType)) {
            //if the specified token type is required by this character's role, check if this character has any extras
            int requiredAmount = role.GetRequiredItemAmount(tokenType);
            if (GetTokenCountInInventory(tokenType) > requiredAmount) {
                return true;
            }
            return false;
        } else {
            return HasTokenInInventory(tokenType);
        }
    }
    public bool OwnsItemOfType(SPECIAL_TOKEN tokenType) {
        for (int i = 0; i < homeStructure.itemsInStructure.Count; i++) {
            SpecialToken token = homeStructure.itemsInStructure[i];
            if (token.characterOwner == this && token.specialTokenType == tokenType) {
                return true;
            }
        }
        for (int i = 0; i < items.Count; i++) {
            SpecialToken token = items[i];
            if (token.characterOwner == this && token.specialTokenType == tokenType) {
                return true;
            }
        }
        return false;
    }
    private void OnItemRemovedFromTile(SpecialToken removedItem, LocationGridTile removedFrom) {
        //whenever an item is removed from a tile, remove all characters that are aware of it to prevent this issue. 
        //https://trello.com/c/JEg9o5Ox/2352-scrapping-healing-potion-in-outskirts
        RemoveAwareness(removedItem); 
    }
    private void OnItemPlacedOnTile(SpecialToken addedItem, LocationGridTile addedTo) {
        //if an item is dropped at a warehouse, inform all residents of that area
        if (addedTo.structure.structureType == STRUCTURE_TYPE.WAREHOUSE && addedTo.parentAreaMap.area == this.specificLocation) { 
            AddAwareness(addedItem);
        }
    }
    #endregion

    #region Needs
    public bool HasNeeds() {
        return race != RACE.SKELETON && characterClass.className != "Zombie" && !returnedToLife;
    }
    protected void DecreaseNeeds() {
        if (!HasNeeds()) {
            return;
        }
        if (_doNotGetHungry <= 0) {
            AdjustFullness(-(CharacterManager.FULLNESS_DECREASE_RATE + fullnessDecreaseRate));
        }
        if (_doNotGetTired <= 0) {
            AdjustTiredness(-(CharacterManager.TIREDNESS_DECREASE_RATE + tirednessDecreaseRate));
        }
        if (_doNotGetLonely <= 0) {
            AdjustHappiness(-(CharacterManager.HAPPINESS_DECREASE_RATE + happinessDecreaseRate));
        }
    }
    public string GetNeedsSummary() {
        string summary = "Fullness: " + fullness.ToString() + "/" + FULLNESS_DEFAULT.ToString();
        summary += "\nTiredness: " + tiredness + "/" + TIREDNESS_DEFAULT.ToString();
        summary += "\nHappiness: " + happiness + "/" + HAPPINESS_DEFAULT.ToString();
        return summary;
    }
    public void AdjustFullnessDecreaseRate(int amount) {
        fullnessDecreaseRate += amount;
    }
    public void AdjustTirednessDecreaseRate(int amount) {
        tirednessDecreaseRate += amount;
    }
    public void AdjustHappinessDecreaseRate(int amount) {
        happinessDecreaseRate += amount;
    }
    //public void SetFullnessDecreaseRate(int amount) {
    //    fullnessDecreaseRate = amount;
    //}
    //public void SetTirednessDecreaseRate(int amount) {
    //    tirednessDecreaseRate = amount;
    //}
    //public void SetHappinessDecreaseRate(int amount) {
    //    happinessDecreaseRate = amount;
    //}
    private void SetTirednessLowerBound(int amount) {
        tirednessLowerBound = amount;
    }
    private void SetFullnessLowerBound(int amount) {
        fullnessLowerBound = amount;
    }
    private void SetHappinessLowerBound(int amount) {
        happinessLowerBound = amount;
    }
    #endregion

    #region Tiredness
    public void ResetTirednessMeter() {
        tiredness = TIREDNESS_DEFAULT;
        RemoveTiredOrExhausted();
    }
    public void AdjustTiredness(int adjustment) {
        tiredness += adjustment;
        tiredness = Mathf.Clamp(tiredness, tirednessLowerBound, TIREDNESS_DEFAULT);
        if (tiredness == 0) {
            Death("exhaustion");
        } else if (isExhausted) {
            RemoveTrait("Tired");
            if (AddTrait("Exhausted")) {
                Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, this);
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "exhausted");
            }
            //PlanTirednessRecoveryActions();
        } else if (isTired) {
            RemoveTrait("Exhausted");
            if (AddTrait("Tired")) {
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "tired");
            }
            //PlanTirednessRecoveryActions();
        } else {
            //tiredness is higher than both thresholds
            RemoveTiredOrExhausted();
        }
    }
    public void ExhaustCharacter() {
        if (!isExhausted) {
            int diff = tiredness - TIREDNESS_THRESHOLD_2;
            AdjustTiredness(-diff);
        }
    }
    public void DecreaseTirednessMeter() { //this is used for when tiredness is only decreased by 1 (I did this for optimization, so as not to check for traits everytime)
        tiredness -= 1;
        tiredness = Mathf.Clamp(tiredness, tirednessLowerBound, TIREDNESS_DEFAULT);
        if (tiredness == 0) {
            Death("exhaustion");
        } else if (isExhausted) {
            if (tiredness == TIREDNESS_THRESHOLD_2) {
                RemoveTrait("Tired");
                if (AddTrait("Exhausted")) {
                    //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "exhausted");
                }
            }
            //PlanTirednessRecoveryActions();
        } else if (isTired) {
            if (tiredness == TIREDNESS_THRESHOLD_1) {
                if (AddTrait("Tired")) {
                    //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "tired");
                }
            }
            //PlanTirednessRecoveryActions();
        }
    }
    public void SetTiredness(int amount) {
        tiredness = amount;
        tiredness = Mathf.Clamp(tiredness, tirednessLowerBound, TIREDNESS_DEFAULT);
        if (tiredness == 0) {
            Death("exhaustion");
        } else if (isExhausted) {
            RemoveTrait("Tired");
            if (AddTrait("Exhausted")) {
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "exhausted");
            }
            //PlanTirednessRecoveryActions();
        } else if (isTired) {
            RemoveTrait("Exhausted");
            if (AddTrait("Tired")) {
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "tired");
            }
            //PlanTirednessRecoveryActions();
        } else {
            //tiredness is higher than both thresholds
            RemoveTiredOrExhausted();
        }
    }
    private void RemoveTiredOrExhausted() {
        if (!RemoveTrait("Tired")) {
            if (RemoveTrait("Exhausted")) {
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, "exhausted");
            }
        } else {
            //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, "tired");
        }
    }
    public void SetTirednessForcedTick() {
        if (!hasForcedTiredness) {
            if (forcedTirednessRecoveryTimeInWords == GameManager.GetCurrentTimeInWordsOfTick()) {
                //If the forced recovery job has not been done yet and the character is already on the time of day when it is supposed to be done,
                //the tick that will be assigned will be ensured that the character will not miss it
                //Example if the time of day is Afternoon, the supposed tick range for it is 145 - 204
                //So if the current tick of the game is already in 160, the range must be adjusted to 161 - 204, so as to ensure that the character will hit it
                //But if the current tick of the game is already in 204, it cannot be 204 - 204, so, it will revert back to 145 - 204 
                int newTick = GameManager.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords, GameManager.Instance.tick + 1);
                TIME_IN_WORDS timeInWords = GameManager.GetTimeInWordsOfTick(newTick);
                if(timeInWords != forcedTirednessRecoveryTimeInWords) {
                    newTick = GameManager.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords);
                }
                tirednessForcedTick = newTick;
                return;
            }
        }
        tirednessForcedTick = GameManager.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords);
    }
    public void SetTirednessForcedTick(int tick) {
        tirednessForcedTick = tick;
    }
    public void SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS timeInWords) {
        forcedTirednessRecoveryTimeInWords = timeInWords;
    }
    #endregion

    #region Fullness
    public void ResetFullnessMeter() {
        fullness = FULLNESS_DEFAULT;
        RemoveHungryOrStarving();
    }
    public void AdjustFullness(int adjustment) {
        fullness += adjustment;
        fullness = Mathf.Clamp(fullness, fullnessLowerBound, FULLNESS_DEFAULT);
        if(adjustment > 0) {
            HPRecovery(0.02f);
        }
        if (fullness == 0) {
            Death("starvation");
        } else if (isStarving) {
            RemoveTrait("Hungry");
            if (AddTrait("Starving") && GetNormalTrait("Vampiric") == null) { //only characters that are not vampires will flee when they are starving
                Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, this);
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "starving");
            }
            //PlanFullnessRecoveryActions();
        } else if (isHungry) {
            RemoveTrait("Starving");
            if (AddTrait("Hungry")) {
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "hungry");
            }
            //PlanFullnessRecoveryActions();
        } else {
            //fullness is higher than both thresholds
            RemoveHungryOrStarving();
        }
    }
    public void DecreaseFullnessMeter() { //this is used for when fullness is only decreased by 1 (I did this for optimization, so as not to check for traits everytime)
        fullness -= 1;
        fullness = Mathf.Clamp(fullness, fullnessLowerBound, FULLNESS_DEFAULT);
        if (fullness == 0) {
            Death("starvation");
        } else if (isStarving) {
            if (fullness == FULLNESS_THRESHOLD_2) {
                RemoveTrait("Hungry");
                if (AddTrait("Starving")) {
                    //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "starving");
                }
            }
            //PlanFullnessRecoveryActions();
        } else if (isHungry) {
            if (fullness == FULLNESS_THRESHOLD_1) {
                if (AddTrait("Hungry")) {
                    //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "hungry");
                }
            }
            //PlanFullnessRecoveryActions();
        }
    }
    public void SetFullness(int amount) {
        fullness = amount;
        fullness = Mathf.Clamp(fullness, fullnessLowerBound, FULLNESS_DEFAULT);
        if (fullness == 0) {
            Death("starvation");
        } else if (isStarving) {
            RemoveTrait("Hungry");
            if (AddTrait("Starving")) {
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "starving");
            }
            //PlanFullnessRecoveryActions();
        } else if (isHungry) {
            RemoveTrait("Starving");
            if (AddTrait("Hungry")) {
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "hungry");
            }
            //PlanFullnessRecoveryActions();
        } else {
            //fullness is higher than both thresholds
            RemoveHungryOrStarving();
        }
    }
    private void RemoveHungryOrStarving() {
        if (!RemoveTrait("Hungry")) {
            if (RemoveTrait("Starving")) {
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, "starving");
            }
        } else {
            //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, "hungry");
        }
    }
    public void SetFullnessForcedTick() {
        if (!hasForcedFullness) {
            if (forcedFullnessRecoveryTimeInWords == GameManager.GetCurrentTimeInWordsOfTick()) {
                //If the forced recovery job has not been done yet and the character is already on the time of day when it is supposed to be done,
                //the tick that will be assigned will be ensured that the character will not miss it
                //Example if the time of day is Afternoon, the supposed tick range for it is 145 - 204
                //So if the current tick of the game is already in 160, the range must be adjusted to 161 - 204, so as to ensure that the character will hit it
                //But if the current tick of the game is already in 204, it cannot be 204 - 204, so, it will revert back to 145 - 204 
                int newTick = GameManager.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords, GameManager.Instance.tick + 1);
                TIME_IN_WORDS timeInWords = GameManager.GetTimeInWordsOfTick(newTick);
                if (timeInWords != forcedFullnessRecoveryTimeInWords) {
                    newTick = GameManager.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords);
                }
                fullnessForcedTick = newTick;
                return;
            }
        }
        fullnessForcedTick = GameManager.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords);
    }
    public void SetFullnessForcedTick(int tick) {
        fullnessForcedTick = tick;
    }
    public void SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS timeInWords) {
        forcedFullnessRecoveryTimeInWords = timeInWords;
    }
    #endregion

    #region Happiness
    public void ResetHappinessMeter() {
        happiness = HAPPINESS_DEFAULT;
        OnHappinessAdjusted();
    }
    public void AdjustHappiness(int adjustment) {
        happiness += adjustment;
        happiness = Mathf.Clamp(happiness, happinessLowerBound, HAPPINESS_DEFAULT);
        OnHappinessAdjusted();
    }
    public void SetHappiness(int amount) {
        happiness = amount;
        happiness = Mathf.Clamp(happiness, happinessLowerBound, HAPPINESS_DEFAULT);
        OnHappinessAdjusted();
    }
    private void RemoveLonelyOrForlorn() {
        if (!RemoveTrait("Lonely")) {
            if (RemoveTrait("Forlorn")) {
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, "depressed");
            }
        } else {
            //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, "lonely");
        }
    }
    private void OnHappinessAdjusted() {
        if (isForlorn) {
            RemoveTrait("Lonely");
            if (AddTrait("Forlorn")) {
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "depressed");
            }
            //PlanHappinessRecoveryActions();
        } else if (isLonely) {
            RemoveTrait("Forlorn");
            if (AddTrait("Lonely")) {
                //RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "lonely");
            }
            //PlanHappinessRecoveryActions();
        } else {
            RemoveLonelyOrForlorn();
        }
        JobQueueItem suicideJob = jobQueue.GetJob(JOB_TYPE.SUICIDE);
        if (happiness <= 0 && suicideJob == null) {
            //When Happiness meter is reduced to 0, the character will create a Commit Suicide Job.
            Debug.Log(GameManager.Instance.TodayLogString() + this.name + "'s happiness is " + happiness.ToString() + ", creating suicide job");
            CreateSuicideJob();
        } else if (happiness > HAPPINESS_THRESHOLD_2 && suicideJob != null) {
            Debug.Log(GameManager.Instance.TodayLogString() + this.name + "'s happiness is " + happiness.ToString() + ", canceling suicide job");
            if (!jobQueue.CancelJob(suicideJob, "no longer forlorn", false)) {
                suicideJob.UnassignJob(false);
                jobQueue.RemoveJobInQueue(suicideJob);
            }
        }
    }
    #endregion

    #region Share Intel
    public List<string> ShareIntel(Intel intel) {
        List<string> dialogReactions = new List<string>();
        if (intel is TileObjectIntel) {
            TileObjectIntel toi = intel as TileObjectIntel;
            if (HasAwareness(toi.obj)) {
                dialogReactions.Add("I already know about that.");
            } else {
                AddAwareness(toi.obj);
                dialogReactions.Add("There is an " + toi.obj.name + " in " + toi.knownLocation.structure.location.name + "? Thanks for letting me know.");
            }
        } else if (intel is EventIntel) {
            EventIntel ei = intel as EventIntel;
            if (ei.action.endedAtState != null && ei.action.endedAtState.shareIntelReaction != null) {
                dialogReactions.AddRange(ei.action.endedAtState.shareIntelReaction.Invoke(this, ei, SHARE_INTEL_STATUS.INFORMED));
            }
            //if the determined reactions list is empty, check the default reactions
            if (dialogReactions.Count == 0) {
                bool doesNotConcernMe = false;
                //If the event's actor and target do not have any relationship with the recipient and are not part of his faction, 
                //and if no item involved is owned by the recipient: "This does not concern me."
                if (!this.HasRelationshipWith(ei.action.actor)
                    && ei.action.actor.faction != this.faction) {
                    if (ei.action.poiTarget is Character) {
                        Character otherCharacter = ei.action.poiTarget as Character;
                        if (!this.HasRelationshipWith(otherCharacter)
                            && otherCharacter.faction != this.faction) {
                            doesNotConcernMe = true;
                        }
                    } else if (ei.action.poiTarget is TileObject) {
                        TileObject obj = ei.action.poiTarget as TileObject;
                        if (!obj.IsOwnedBy(this)) {
                            doesNotConcernMe = true;
                        }
                    } else if (ei.action.poiTarget is SpecialToken) {
                        SpecialToken obj = ei.action.poiTarget as SpecialToken;
                        if (obj.characterOwner != this) {
                            doesNotConcernMe = true;
                        }
                    }
                }

                if (ei.action.actor == this) {
                    //If the actor and the recipient is the same: "I know what I did."
                    dialogReactions.Add("I know what I did.");
                } else {
                    if (doesNotConcernMe) {
                        //The following events are too unimportant to merit any meaningful response: "What will I do with this random tidbit?"
                        //-character picked up an item(not stealing)
                        //-character prayed, daydreamed, played
                        //- character slept
                        //- character mined or chopped wood
                        switch (ei.action.goapType) {
                            case INTERACTION_TYPE.PICK_ITEM_GOAP:
                            case INTERACTION_TYPE.PRAY:
                            case INTERACTION_TYPE.DAYDREAM:
                            case INTERACTION_TYPE.PLAY:
                            case INTERACTION_TYPE.SLEEP:
                            case INTERACTION_TYPE.SLEEP_OUTSIDE:
                            case INTERACTION_TYPE.MINE_GOAP:
                            case INTERACTION_TYPE.CHOP_WOOD:
                                dialogReactions.Add("What will I do with this random tidbit?");
                                break;
                            default:
                                dialogReactions.Add("This does not concern me.");
                                break;
                        }

                    } else {
                        //Otherwise: "A proper response to this information has not been implemented yet."
                        dialogReactions.Add("A proper response to this information has not been implemented yet.");
                    }
                }
            }
            CreateInformedEventLog(intel.intelLog.goapAction, false);
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
    public bool AddAwareness(IPointOfInterest pointOfInterest) {
        return currentAlterEgo.AddAwareness(pointOfInterest);
    }
    public void RemoveAwareness(IPointOfInterest pointOfInterest) {
        if (_numOfWaitingForGoapThread > 0) {
            pendingActionsAfterMultiThread.Add(() => RemoveAwareness(pointOfInterest));
            return;
        }
        currentAlterEgo.RemoveAwareness(pointOfInterest);
        //Debug.Log(GameManager.Instance.TodayLogString() + this.name + " removed awareness of " + pointOfInterest.name);
        //if (awareness.ContainsKey(pointOfInterest.poiType)) {
        //    List<IAwareness> awarenesses = awareness[pointOfInterest.poiType];
        //    for (int i = 0; i < awarenesses.Count; i++) {
        //        IAwareness iawareness = awarenesses[i];
        //        if (iawareness.poi == pointOfInterest) {
        //            awarenesses.RemoveAt(i);
        //            iawareness.OnRemoveAwareness(this);
        //            break;
        //        }
        //    }
        //}
    }
    public bool HasAwareness(IPointOfInterest poi) {
        return currentAlterEgo.HasAwareness(poi);
        //if (awareness.ContainsKey(poi.poiType)) {
        //    List<IAwareness> awarenesses = awareness[poi.poiType];
        //    for (int i = 0; i < awarenesses.Count; i++) {
        //        IAwareness iawareness = awarenesses[i];
        //        if (iawareness.poi == poi) {
        //            return iawareness;
        //        }
        //    }
        //    return null;
        //}
        //return null;
    }
    //private IAwareness CreateNewAwareness(IPointOfInterest poi) {
    //    if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
    //        return new CharacterAwareness(poi as Character);
    //    } else if (poi.poiType == POINT_OF_INTEREST_TYPE.ITEM) {
    //        return new ItemAwareness(poi as SpecialToken);
    //    } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
    //        return new TileObjectAwareness(poi);
    //    }//TODO: Structure Awareness
    //    return null;
    //}
    public void AddInitialAwareness() {
        AddAwareness(this);
        if (faction == FactionManager.Instance.neutralFaction) {
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
            List<IPointOfInterest> treeObjects = new List<IPointOfInterest>();
            foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in specificLocation.structures) {
                for (int i = 0; i < keyValuePair.Value.Count; i++) {
                    LocationStructure structure = keyValuePair.Value[i];
                    for (int j = 0; j < structure.pointsOfInterest.Count; j++) {
                        IPointOfInterest poi = structure.pointsOfInterest[j];
                        if (poi != this) {
                            if (poi is TreeObject) {
                                treeObjects.Add(poi);
                                continue;
                            }
                            AddAwareness(poi);
                        }
                    }
                }
                //order the tree objects, then only add the first n to this character
                treeObjects = treeObjects.OrderBy(x => Vector3Int.Distance(x.gridTileLocation.localPlace, this.gridTileLocation.localPlace)).ToList();
                for (int i = 0; i < TREE_AWARENESS_LIMIT; i++) {
                    if (treeObjects.Count <= i) {
                        break; //no more tree objects left
                    }
                    IPointOfInterest tree = treeObjects[i];
                    AddAwareness(tree);
                }

            }

        }
    }
    public void AddInitialAwareness(Area area) {
        AddAwareness(this);
        if (faction == FactionManager.Instance.neutralFaction) {
            foreach (List<LocationStructure> structures in area.structures.Values) {
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
            List<IPointOfInterest> treeObjects = new List<IPointOfInterest>();
            foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in specificLocation.structures) {
                for (int i = 0; i < keyValuePair.Value.Count; i++) {
                    LocationStructure structure = keyValuePair.Value[i];
                    for (int j = 0; j < structure.pointsOfInterest.Count; j++) {
                        IPointOfInterest poi = structure.pointsOfInterest[j];
                        if (poi != this) {
                            if (poi is TreeObject) {
                                treeObjects.Add(poi);
                                continue;
                            }
                            AddAwareness(poi);
                        }
                    }
                }
                //order the tree objects, then only add the first n to this character
                treeObjects = treeObjects.OrderBy(x => Vector3Int.Distance(x.gridTileLocation.localPlace, this.gridTileLocation.localPlace)).ToList();
                for (int i = 0; i < TREE_AWARENESS_LIMIT; i++) {
                    if (treeObjects.Count <= i) {
                        break; //no more tree objects left
                    }
                    IPointOfInterest tree = treeObjects[i];
                    AddAwareness(tree);
                }

            }

        }
    }
    public void LogAwarenessList() {
        string log = "--------------AWARENESS LIST OF " + name + "-----------------";
        foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> kvp in awareness) {
            log += "\n" + kvp.Key.ToString() + ": ";
            for (int i = 0; i < kvp.Value.Count; i++) {
                if (i > 0) {
                    log += ", ";
                }
                log += kvp.Value[i].name;
            }
        }
        Debug.Log(log);
    }
    public void ClearAllAwareness() {
        currentAlterEgo.ClearAllAwareness();
        //Debug.Log("Cleared all awareness of " + this.name);
    }
    public void ClearAllAwarenessOfType(params POINT_OF_INTEREST_TYPE[] types) {
        for (int i = 0; i < types.Length; i++) {
            POINT_OF_INTEREST_TYPE currType = types[i];
            currentAlterEgo.RemoveAwareness(currType);
            //Debug.Log("Cleared all awareness of type " + currType.ToString() + " of " + this.name);
        }
    }
    #endregion

    #region Point Of Interest
    public List<GoapAction> AdvertiseActionsToActor(Character actor, Dictionary<INTERACTION_TYPE, object[]> otherData) {
        if (poiGoapActions != null && poiGoapActions.Count > 0 && IsAvailable() && !isDead) {
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                INTERACTION_TYPE currType = poiGoapActions[i];
                if (RaceManager.Instance.CanCharacterDoGoapAction(actor, currType)) {
                    object[] data = null;
                    if(otherData != null) {
                        if (otherData.ContainsKey(currType)) {
                            data = otherData[currType];
                        } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                            data = otherData[INTERACTION_TYPE.NONE];
                        }
                    }

                    if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currType, actor, this, data)
                        && InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(currType, actor, this, data)) {
                        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                        if (goapAction != null) {
                            if (data != null) {
                                goapAction.InitializeOtherData(data);
                            }
                            usableActions.Add(goapAction);
                        } else {
                            throw new System.Exception("Goap action " + currType.ToString() + " is null!");
                        }
                    }
                    //if (currType == INTERACTION_TYPE.CRAFT_ITEM) {
                    //    Craftsman craftsman = GetNormalTrait("Craftsman") as Craftsman;
                    //    for (int j = 0; j < craftsman.craftedItemNames.Length; j++) {
                    //        //CraftItemGoap goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this, false) as CraftItemGoap;
                    //        //goapAction.SetCraftedItem(craftsman.craftedItemNames[j]);
                    //        //goapAction.Initialize();
                    //        object[] otherData = new object[] { craftsman.craftedItemNames[j] };
                    //        if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currType, actor, this, otherData)
                    //            && InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(currType, actor, this, otherData) {
                    //            usableActions.Add(currType);
                    //        }
                    //    }
                    //} else {
                    //    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                    //    if (goapAction == null) {
                    //        throw new Exception("Goap action " + currType.ToString() + " is null!");
                    //    }
                    //    if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currType, actor, this, null)
                    //            && InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(currType, actor, this, null)) {
                    //        usableActions.Add(goapAction);
                    //    }
                    //}
                }
            }
            return usableActions;
        }
        return null;
    }
    public List<GoapAction> AdvertiseActionsToActorFromDeadCharacter(Character actor, Dictionary<INTERACTION_TYPE, object[]> otherData) {
        if (poiGoapActions != null && poiGoapActions.Count > 0) {
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                INTERACTION_TYPE currType = poiGoapActions[i];
                if (RaceManager.Instance.CanCharacterDoGoapAction(actor, currType)) {
                    object[] data = null;
                    if (otherData != null) {
                        if (otherData.ContainsKey(currType)) {
                            data = otherData[currType];
                        } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                            data = otherData[INTERACTION_TYPE.NONE];
                        }
                    }
                    //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                    //if (goapAction == null) {
                    //    throw new Exception("Goap action " + currType.ToString() + " is null!");
                    //}
                    if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currType, actor, this, data)
                        && InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(currType, actor, this, data)) {

                        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                        if (goapAction != null) {
                            if (data != null) {
                                goapAction.InitializeOtherData(data);
                            }
                            usableActions.Add(goapAction);
                        } else {
                            throw new System.Exception("Goap action " + currType.ToString() + " is null!");
                        }
                    }
                    //if (currType == INTERACTION_TYPE.CRAFT_ITEM) {
                    //    Craftsman craftsman = GetTrait("Craftsman") as Craftsman;
                    //    for (int j = 0; j < craftsman.craftedItemNames.Length; j++) {
                    //        CraftItemGoap goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this, false) as CraftItemGoap;
                    //        goapAction.SetCraftedItem(craftsman.craftedItemNames[j]);
                    //        goapAction.Initialize();
                    //        if (goapAction.CanSatisfyRequirements()) {
                    //            usableActions.Add(goapAction);
                    //        }
                    //    }
                    //} else {

                    //}
                }
            }
            return usableActions;
        }
        return null;
    }
    public void SetPOIState(POI_STATE state) {
        _state = state;
    }
    public bool IsAvailable() {
        return _state != POI_STATE.INACTIVE && !isDisabledByPlayer;
    }
    public void SetIsDisabledByPlayer(bool state) {
        isDisabledByPlayer = state;
    }
    #endregion

    #region Goap
    public void SetNumWaitingForGoapThread(int amount) {
        _numOfWaitingForGoapThread = amount;
    }
    public void ConstructInitialGoapAdvertisementActions() {
        //poiGoapActions = new List<INTERACTION_TYPE>();
        poiGoapActions.Add(INTERACTION_TYPE.CARRY_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.ASSAULT_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.DROP_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.ABDUCT_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.RESTRAIN_CHARACTER);
        //poiGoapActions.Add(INTERACTION_TYPE.STROLL);
        poiGoapActions.Add(INTERACTION_TYPE.DAYDREAM);
        poiGoapActions.Add(INTERACTION_TYPE.SLEEP_OUTSIDE);
        poiGoapActions.Add(INTERACTION_TYPE.PRAY);
        //poiGoapActions.Add(INTERACTION_TYPE.EXPLORE);
        //poiGoapActions.Add(INTERACTION_TYPE.PATROL);
        //poiGoapActions.Add(INTERACTION_TYPE.TRAVEL);
        poiGoapActions.Add(INTERACTION_TYPE.RETURN_HOME_LOCATION);
        //poiGoapActions.Add(INTERACTION_TYPE.HUNT_ACTION);
        poiGoapActions.Add(INTERACTION_TYPE.PLAY);
        poiGoapActions.Add(INTERACTION_TYPE.REPORT_CRIME);
        poiGoapActions.Add(INTERACTION_TYPE.REPORT_HOSTILE);
        poiGoapActions.Add(INTERACTION_TYPE.STEAL_FROM_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.JUDGE_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.CURSE_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE);
        poiGoapActions.Add(INTERACTION_TYPE.BURY_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.CARRY_CORPSE);
        poiGoapActions.Add(INTERACTION_TYPE.DROP_ITEM_WAREHOUSE);
        poiGoapActions.Add(INTERACTION_TYPE.INVITE_TO_MAKE_LOVE);
        poiGoapActions.Add(INTERACTION_TYPE.REPLACE_TILE_OBJECT);
        poiGoapActions.Add(INTERACTION_TYPE.TANTRUM);
        poiGoapActions.Add(INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_FRIENDSHIP);
        poiGoapActions.Add(INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_LOVE);
        poiGoapActions.Add(INTERACTION_TYPE.BREAK_UP);
        poiGoapActions.Add(INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
        poiGoapActions.Add(INTERACTION_TYPE.ROAMING_TO_STEAL);
        poiGoapActions.Add(INTERACTION_TYPE.PUKE);
        poiGoapActions.Add(INTERACTION_TYPE.SEPTIC_SHOCK);
        poiGoapActions.Add(INTERACTION_TYPE.CARRY);
        poiGoapActions.Add(INTERACTION_TYPE.DROP);
        poiGoapActions.Add(INTERACTION_TYPE.RESOLVE_CONFLICT);
        poiGoapActions.Add(INTERACTION_TYPE.ASK_TO_STOP_JOB);
        poiGoapActions.Add(INTERACTION_TYPE.STRANGLE);
        poiGoapActions.Add(INTERACTION_TYPE.PRIORITIZED_SHOCK);
        poiGoapActions.Add(INTERACTION_TYPE.PRIORITIZED_CRY);
        poiGoapActions.Add(INTERACTION_TYPE.CRY);

        if (race != RACE.SKELETON) {
            poiGoapActions.Add(INTERACTION_TYPE.SHARE_INFORMATION);
            poiGoapActions.Add(INTERACTION_TYPE.DRINK_BLOOD);
            poiGoapActions.Add(INTERACTION_TYPE.EAT_CHARACTER);
        }
    }
    public void StartGOAP(GoapEffect goal, IPointOfInterest target, GOAP_CATEGORY category, bool isPriority = false, List<Character> otherCharactePOIs = null, bool isPersonalPlan = true, GoapPlanJob job = null, Dictionary<INTERACTION_TYPE, object[]> otherData = null, bool allowDeadTargets = false) {
        List<IPointOfInterest> characterTargetsAwareness = new List<IPointOfInterest>();
        if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            AddAwareness(target);
            characterTargetsAwareness.Add(target);
        }

        if (otherCharactePOIs != null) {
            for (int i = 0; i < otherCharactePOIs.Count; i++) {
                AddAwareness(otherCharactePOIs[i]);
                characterTargetsAwareness.Add(otherCharactePOIs[i]);
            }
        }
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        _numOfWaitingForGoapThread++;
        //Debug.LogWarning(name + " sent a plan to other thread(" + _numOfWaitingForGoapThread + ")");
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, target, goal, category, isPriority, characterTargetsAwareness, isPersonalPlan, job, allowDeadTargets, otherData));
    }
    public void StartGOAP(GoapAction goal, IPointOfInterest target, GOAP_CATEGORY category, bool isPriority = false, List<Character> otherCharactePOIs = null, bool isPersonalPlan = true, GoapPlanJob job = null, bool allowDeadTargets = false) {
        List<IPointOfInterest> characterTargetsAwareness = new List<IPointOfInterest>();
        if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            AddAwareness(target);
            characterTargetsAwareness.Add(target);
        }

        if (otherCharactePOIs != null) {
            for (int i = 0; i < otherCharactePOIs.Count; i++) {
                AddAwareness(otherCharactePOIs[i]);
                characterTargetsAwareness.Add(otherCharactePOIs[i]);
            }
        }
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        _numOfWaitingForGoapThread++;
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, target, goal, category, isPriority, characterTargetsAwareness, isPersonalPlan, job, allowDeadTargets));
    }
    public void StartGOAP(INTERACTION_TYPE goalType, IPointOfInterest target, GOAP_CATEGORY category, bool isPriority = false, List<Character> otherCharactePOIs = null, bool isPersonalPlan = true, GoapPlanJob job = null, Dictionary<INTERACTION_TYPE, object[]> otherData = null, bool allowDeadTargets = false) {
        List<IPointOfInterest> characterTargetsAwareness = new List<IPointOfInterest>();
        if (target != null && target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            AddAwareness(target);
            characterTargetsAwareness.Add(target);
        }
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        _numOfWaitingForGoapThread++;
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, goalType, target, category, isPriority, characterTargetsAwareness, isPersonalPlan, job, allowDeadTargets, otherData));
    }
    /// <summary>
    /// This should only be used for plans that come/constructed from the outside.
    /// </summary>
    /// <param name="plan">Plan to be added</param>
    public void AddPlan(GoapPlan plan, bool isPriority = false, bool processPlanSpecialCases = true) {
        if (!allGoapPlans.Contains(plan)) {
            plan.SetPriorityState(isPriority);
            if (isPriority) {
                allGoapPlans.Insert(0, plan);
            } else {
                bool hasBeenInserted = false;
                if (plan.job != null) {
                    for (int i = 0; i < allGoapPlans.Count; i++) {
                        GoapPlan currentPlan = allGoapPlans[i];
                        if (currentPlan.isPriority) {
                            continue;
                        }
                        if (currentPlan.job == null || plan.job.priority < currentPlan.job.priority) {
                            allGoapPlans.Insert(i, plan);
                            hasBeenInserted = true;
                            break;
                        }
                    }
                }
                if (!hasBeenInserted) {
                    allGoapPlans.Add(plan);
                }
            }

            if (processPlanSpecialCases) {
                //If a character is strolling or idly returning home and a plan is added to this character, end the action/state
                if (stateComponent.currentState != null && (stateComponent.currentState.characterState == CHARACTER_STATE.STROLL
                    || stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE
                    || stateComponent.currentState.characterState == CHARACTER_STATE.PATROL)) {
                    stateComponent.currentState.OnExitThisState();
                } else if (stateComponent.stateToDo != null && (stateComponent.stateToDo.characterState == CHARACTER_STATE.STROLL
                     || stateComponent.stateToDo.characterState == CHARACTER_STATE.STROLL_OUTSIDE
                     || stateComponent.stateToDo.characterState == CHARACTER_STATE.PATROL)) {
                    stateComponent.SetStateToDo(null);
                } else if (currentAction != null && currentAction.goapType == INTERACTION_TYPE.RETURN_HOME) {
                    if (currentAction.parentPlan == null || currentAction.parentPlan.category == GOAP_CATEGORY.IDLE) {
                        currentAction.StopAction();
                    }
                }

                if (plan.job != null && (plan.job.jobType.IsNeedsTypeJob() || plan.job.jobType.IsEmergencyTypeJob())) {
                    //Unassign Location Job if character decides to rest, eat or have fun.
                    homeArea.jobQueue.UnassignAllJobsTakenBy(this);
                    faction.activeQuest?.jobQueue.UnassignAllJobsTakenBy(this);
                }
            }
        }
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
        if (isDead || marker == null) {
            return;
        }
        if (goapThread.recalculationPlan != null && goapThread.recalculationPlan.isEnd) {
            return;
        }
        if (goapThread.recalculationPlan == null) {
            _numOfWaitingForGoapThread--;
        }
        if (_numOfWaitingForGoapThread == 0) {
            for (int i = 0; i < pendingActionsAfterMultiThread.Count; i++) {
                pendingActionsAfterMultiThread[i].Invoke();
            }
        }
        PrintLogIfActive(goapThread.log);
        if (goapThread.createdPlan != null) {
            if (goapThread.recalculationPlan == null) {
                int count = GetNumberOfTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
                if (count >= 2 || (count == 1 && GetNormalTrait("Paralyzed") == null)) {
                    PrintLogIfActive(GameManager.Instance.TodayLogString() + name + " is scrapping plan since " + name + " has a negative disabler trait. " + goapThread.job.name + " is the job.");
                    if (goapThread.job != null) {
                        if (goapThread.job.assignedCharacter == this) {
                            goapThread.job.SetAssignedCharacter(null);
                            goapThread.job.SetAssignedPlan(null);
                            goapThread.job.jobQueueParent.RemoveJobInQueue(goapThread.job);
                        }
                    }
                    return;
                }
                if (goapThread.job != null) {
                    if (goapThread.job.assignedCharacter != this) {
                        PrintLogIfActive(GameManager.Instance.TodayLogString() + name + " is scrapping plan since " + goapThread.job.name + " job's assigned character is no longer him/her. New assigned character is " + (goapThread.job.assignedCharacter != null ? goapThread.job.assignedCharacter.name : "None"));
                        return;
                    }
                    goapThread.job.SetAssignedPlan(goapThread.createdPlan);

                    //If the created plan contains a carry component, that plan cannot be overridden
                    //for (int i = 0; i < goapThread.createdPlan.allNodes.Count; i++) {
                    //    if (goapThread.createdPlan.allNodes[i].action.goapType == INTERACTION_TYPE.CARRY_CHARACTER
                    //        || goapThread.createdPlan.allNodes[i].action.goapType == INTERACTION_TYPE.CARRY_CORPSE
                    //        || goapThread.createdPlan.allNodes[i].action.goapType == INTERACTION_TYPE.INVITE_TO_MAKE_LOVE) {
                    //        goapThread.createdPlan.job.SetCannotOverrideJob(true);
                    //        break;
                    //    }
                    //}
                }
                AddPlan(goapThread.createdPlan);
                if (CanCurrentJobBeOverriddenByJob(goapThread.job)) {
                    //AddPlan(goapThread.createdPlan, true);

                    if (stateComponent.currentState != null) {
                        stateComponent.currentState.OnExitThisState();
                        //This call is doubled so that it will also exit the previous major state if there's any
                        if (stateComponent.currentState != null) {
                            stateComponent.currentState.OnExitThisState();
                        }
                        ////- berserk, flee, and engage are the highest priority, they cannot be overridden. character must finish the state before doing anything else.
                        //if (stateComponent.currentState.characterState != CHARACTER_STATE.ENGAGE && stateComponent.currentState.characterState != CHARACTER_STATE.FLEE && stateComponent.currentState.characterState != CHARACTER_STATE.BERSERKED) {
                        //    stateComponent.currentState.OnExitThisState();
                        //}
                    } else if (stateComponent.currentState != null) {
                        stateComponent.SetStateToDo(null);
                    } else {
                        if (currentParty.icon.isTravelling) {
                            if (currentParty.icon.travelLine == null) {
                                marker.StopMovement();
                            } else {
                                currentParty.icon.SetOnArriveAction(() => OnArriveAtAreaStopMovement());
                            }
                        }
                        AdjustIsWaitingForInteraction(1);
                        StopCurrentAction(false);
                        AdjustIsWaitingForInteraction(-1);
                    }
                    //return;
                }
            } else {
                //Receive plan recalculation
                goapThread.createdPlan.SetIsBeingRecalculated(false);
                int count = GetNumberOfTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
                if (count >= 2 || (count == 1 && GetNormalTrait("Paralyzed") == null)) {
                    PrintLogIfActive(GameManager.Instance.TodayLogString() + name + " is scrapping recalculated plan since " + name + " has a negative disabler trait. " + goapThread.job.name + " is the job.");
                    DropPlan(goapThread.recalculationPlan, true);
                    return;
                }
                if (goapThread.createdPlan.job != null) {
                    if (goapThread.createdPlan.job.assignedCharacter != this) {
                        PrintLogIfActive(GameManager.Instance.TodayLogString() + name + " is scrapping recalculated plan since " + goapThread.createdPlan.job.name + " job's assigned character is no longer him/her. New assigned character is " + (goapThread.createdPlan.job.assignedCharacter != null ? goapThread.createdPlan.job.assignedCharacter.name : "None"));
                        DropPlan(goapThread.recalculationPlan, true);
                        return;
                    }
                }
            }
        } else {
            if (goapThread.job != null && goapThread.job.jobType.IsNeedsTypeJob()) {
                //If unable to do a Need while in a Trapped Structure, remove Trap Structure.
                if (trapStructure.structure != null) {
                    trapStructure.SetStructureAndDuration(null, 0);
                }
            }
            if (goapThread.recalculationPlan != null) {
                //This means that the recalculation has failed
                DropPlan(goapThread.recalculationPlan);
            } else {
                if (goapThread.job != null) {
                    goapThread.job.SetAssignedCharacter(null);
                    if (goapThread.job.jobQueueParent.character != null) {
                        //If no plan was generated, automatically remove job from queue if it is a personal job
                        goapThread.job.jobQueueParent.RemoveJobInQueue(goapThread.job);
                        if (goapThread.job.jobType == JOB_TYPE.REMOVE_FIRE) {
                            if (goapThread.job.targetPOI.gridTileLocation != null) { //this happens because sometimes the target that was burning is now put out.
                                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cancel_job_no_plan");
                                log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                log.AddToFillers(null, goapThread.job.GetJobDetailString(), LOG_IDENTIFIER.STRING_1);
                                RegisterLogAndShowNotifToThisCharacterOnly(log);
                            }
                        } else {
                            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cancel_job_no_plan");
                            log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            log.AddToFillers(null, goapThread.job.GetJobDetailString(), LOG_IDENTIFIER.STRING_1);
                            RegisterLogAndShowNotifToThisCharacterOnly(log);
                        }
                        if (goapThread.job.canBeDoneInLocation) {
                            //If a personal job can be done in location job queue, add it once no plan is generated
                            specificLocation.jobQueue.AddJobInQueue(goapThread.job);
                        }
                    } else {
                        goapThread.job.AddBlacklistedCharacter(this);
                    }
                }
            }
        }
    }
    public bool IsPOIInCharacterAwarenessList(IPointOfInterest poi, List<IPointOfInterest> awarenesses) {
        if (awarenesses != null && awarenesses.Count > 0) {
            for (int i = 0; i < awarenesses.Count; i++) {
                if (awarenesses[i] == poi) {
                    return true;
                }
            }
        }
        return false;
    }
    //private void SchedulePerformGoapPlans() {
    //    if (!IsInOwnParty() || isDefender || ownParty.icon.isTravelling || _doNotDisturb > 0 || _job == null || isWaitingForInteraction > 0) {
    //        StartDailyGoapPlanGeneration();
    //        return;
    //    }
    //    if (allGoapPlans.Count > 0) {
    //        StopDailyGoapPlanGeneration();
    //        GameDate dueDate = GameManager.Instance.Today();
    //        dueDate.AddTicks(1);
    //        SchedulingManager.Instance.AddEntry(dueDate, PerformGoapPlans);
    //    }
    //}
    public void PerformGoapPlans() {
        string log = GameManager.Instance.TodayLogString() + "PERFORMING GOAP PLANS OF " + name;
        if (currentAction != null) {
            log += "\n" + name + " can't perform another action because he/she is currently performing " + currentAction.goapName;
            PrintLogIfActive(log);
            return;
        }
        //List<INTERACTION_TYPE> actorAllowedActions = RaceManager.Instance.GetNPCInteractionsOfCharacter(this);
        bool willGoIdleState = true;
        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan plan = allGoapPlans[i];
            if (plan.currentNode == null) {
                throw new Exception(this.name + "'s current node in plan is null! Plan is: " + plan.name + "\nCall stack: " + plan.setPlanStateCallStack + "\n");
            }
            log += "\n" + plan.currentNode.action.goapName;
            if (plan.isBeingRecalculated) {
                log += "\n - Plan is currently being recalculated, skipping...";
                continue; //skip plan
            }
            if (RaceManager.Instance.CanCharacterDoGoapAction(this, plan.currentNode.action.goapType) 
                && InteractionManager.Instance.CanSatisfyGoapActionRequirements(plan.currentNode.action.goapType, plan.currentNode.action.actor, plan.currentNode.action.poiTarget, plan.currentNode.action.otherData)) {
                //if (plan.isBeingRecalculated) {
                //    log += "\n - Plan is currently being recalculated, skipping...";
                //    continue; //skip plan
                //}
                //if (IsPlanCancelledDueToInjury(plan.currentNode.action)) {
                //    log += "\n - Action's plan is cancelled due to injury, dropping plan...";
                //    PrintLogIfActive(log);
                //    if (allGoapPlans.Count == 1) {
                //        DropPlan(plan, true);
                //        willGoIdleState = false;
                //        break;
                //    } else {
                //        DropPlan(plan, true);
                //        i--;
                //        continue;
                //    }
                //}
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
                    if (plan.currentNode.action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                        Character targetCharacter = plan.currentNode.action.poiTarget as Character;
                        if(targetCharacter != this) {
                            Invisible invisible = targetCharacter.GetNormalTrait("Invisible") as Invisible;
                            if (invisible != null && !invisible.charactersThatCanSee.Contains(this)) {
                                log += "\n - " + targetCharacter.name + " is invisible, drop plan and remove job in queue...";
                                PrintLogIfActive(log);
                                if (allGoapPlans.Count == 1) {
                                    DropPlan(plan, true);
                                    willGoIdleState = false;
                                    break;
                                } else {
                                    DropPlan(plan, true);
                                    i--;
                                    continue;
                                }
                            }
                        }

                        if (!targetCharacter.IsInOwnParty() && targetCharacter.currentParty != _ownParty) {
                            log += "\n - " + targetCharacter.name + " is not in its own party, waiting and skipping...";
                            PrintLogIfActive(log);
                            continue;
                        }
                    }
                    if(plan.currentNode.action.poiTarget != this && plan.currentNode.action.isStealth) {
                        //When performing a stealth job action to a character check if that character is already in vision range, if it is, check if the character doesn't have anyone other than this character in vision, if it is, skip it
                        if (marker.inVisionPOIs.Contains(plan.currentNode.action.poiTarget) && !marker.CanDoStealthActionToTarget(plan.currentNode.action.poiTarget)) {
                            continue;
                        }
                    }
                    log += "\n - Action's preconditions are all satisfied, doing action...";
                    PrintLogIfActive(log);
                    Messenger.Broadcast(Signals.CHARACTER_WILL_DO_PLAN, this, plan);
                    //if (plan.currentNode.parent != null && plan.currentNode.parent.action.CanSatisfyAllPreconditions() && plan.currentNode.parent.action.CanSatisfyRequirements()) {
                    //    log += "\n - All Preconditions of next action in plan already met, skipping action: " + plan.currentNode.action.goapName;
                    //    //set next node to parent node instead
                    //    plan.SetNextNode();
                    //    log += "\n - Next action is: " + plan.currentNode.action.goapName;
                    //}
                    plan.currentNode.action.DoAction();
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
    private bool IsPlanCancelledDueToInjury(GoapAction action) {
        if (action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character target = action.poiTarget as Character;
            if (IsHostileWith(target) && GetNormalTrait("Injured") != null) {
                AdjustIsWaitingForInteraction(1);
                DropPlan(action.parentPlan, true);
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
    public void PerformGoapAction() {
        string log = string.Empty;
        if (currentAction == null) {
            log = GameManager.Instance.TodayLogString() + name + " cannot PerformGoapAction because there is no current action!";
            PrintLogIfActive(log);
            //Debug.LogError(log);
            //if (!DropPlan(plan)) {
            //    //PlanGoapActions();
            //}
            //StartDailyGoapPlanGeneration();
            return;
        }
        log = GameManager.Instance.TodayLogString() + name + " is performing goap action: " + currentAction.goapName;
        FaceTarget(currentAction.poiTarget);
        if (currentAction.isStopped) {
            log += "\n Action is stopped! Dropping plan...";
            PrintLogIfActive(log);
            SetCurrentAction(null);
            if (!DropPlan(currentAction.parentPlan)) {
                //PlanGoapActions();
            }
        } else {
            bool willStillContinueAction = true;
            OnStartPerformGoapAction(currentAction, ref willStillContinueAction);
            if (!willStillContinueAction) {
                return;
            }
            if (currentAction.IsHalted()) {
                log += "\n Action is waiting! Not doing action...";
                //if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //    Character targetCharacter = currentAction.poiTarget as Character;
                //    targetCharacter.AdjustIsWaitingForInteraction(-1);
                //}
                SetCurrentAction(null);
                return;
            }
            if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currentAction.goapType, currentAction.actor, currentAction.poiTarget, currentAction.otherData) 
                && currentAction.CanSatisfyAllPreconditions()) {
                log += "\nAction satisfies all requirements and preconditions, proceeding to perform actual action: " + currentAction.goapName + " to " + currentAction.poiTarget.name + " at " + currentAction.poiTarget.gridTileLocation?.ToString() ?? "No Tile Location";
                PrintLogIfActive(log);
                currentAction.PerformActualAction();
            } else {
                log += "\nAction did not meet all requirements and preconditions. Will try to recalculate plan...";
                //if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //    Character targetCharacter = currentAction.poiTarget as Character;
                //    targetCharacter.AdjustIsWaitingForInteraction(-1);
                //}
                GoapPlan plan = currentAction.parentPlan;
                SetCurrentAction(null);
                if (plan.doNotRecalculate) {
                    log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
                    PrintLogIfActive(log);
                    if (!DropPlan(plan)) {
                        //PlanGoapActions();
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
        action.poiTarget.RemoveTargettedByAction(action);

        GoapPlan plan = action.parentPlan;
        if (isDead) {
            log += "\n" + name + " is dead!";
            if (plan.job != null) {
                if (result == InteractionManager.Goap_State_Success) {
                    if (plan.currentNode.parent == null && plan.job.jobQueueParent != null) {
                        log += "This plan has a job and the result of action " + action.goapName + " is " + result + " and this is the last action for this plan, removing job in job queue...";
                        plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
                    }
                }
                log += "\nSetting assigned character and plan to null...";
                plan.job.SetAssignedCharacter(null);
                plan.job.SetAssignedPlan(null);
            }
            PrintLogIfActive(log);
            DropPlan(plan);
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
                //PlanGoapActions();
            }
            return;
        }
        if (plan.state == GOAP_PLAN_STATE.CANCELLED || plan.currentNode == null) {
            log += "\nPlan was cancelled.";
            if (plan.job != null) {
                log += "\nRemoving job in queue...";
                plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
            }
            PrintLogIfActive(log);
            if (!DropPlan(plan)) {
                //PlanGoapActions();
            }
            return;
        }

        //Reason: https://trello.com/c/58aGENsO/1867-attempt-to-find-another-nearby-chair-first-instead-of-dropping-drink-eat-sit-down-actions
        if (result == InteractionManager.Goap_State_Fail) {
            //if the last action of the plan failed and that action type can be replaced
            if (action.goapType.CanBeReplaced()) {
                //find a similar action that is advertised by another object, in the same structure
                //if there is any, insert that action into the current plan, then do that next
                List<TileObject> objs = currentStructure.GetTileObjectsThatAdvertise(action.goapType);
                if (objs.Count > 0) {
                    TileObject chosenObject = objs[UnityEngine.Random.Range(0, objs.Count)];
                    GoapAction newAction = chosenObject.Advertise(action.goapType, this);
                    if (newAction != null) {
                        plan.InsertAction(newAction);
                    } else {
                        Debug.LogWarning(chosenObject.ToString() + " did not return an action of type " + action.goapType.ToString());
                    }
                }
            }
        }
        log += "\nPlan is setting next action to be done...";
        plan.SetNextNode();
        if (plan.currentNode == null) {
            log += "\nThis action is the end of plan.";
            if (plan.job != null && plan.job.jobQueueParent != null) {
                log += "\nRemoving job in queue...";
                if (plan.job.jobQueueParent.isAreaOrQuestJobQueue && GetNormalTrait("Hardworking") != null) {
                    log += "\nFinished a settlement job and character is hardworking, increase happiness by 3000...";
                    AdjustHappiness(3000);
                }
                plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
            }
            PrintLogIfActive(log);
            //this means that this is the end goal so end this plan now
            if (!DropPlan(plan)) {
                //PlanGoapActions();
            }
        } else {
            log += "\nNext action for this plan: " + plan.currentNode.action.goapName;
            if (plan.job != null && plan.job.assignedCharacter != this) {
                log += "\nPlan has a job: " + plan.job.name + ". Assigned character " + (plan.job.assignedCharacter != null ? plan.job.assignedCharacter.name : "None") + " does not match with " + name + ".";
                log += "Drop plan because this character is no longer the one assigned";
                DropPlan(plan);
            }
            PrintLogIfActive(log);
            //PlanGoapActions();
        }
        action.OnResultReturnedToActor();
        //if (result == InteractionManager.Goap_State_Success) {
        //    log += "\nAction performed is a success!";
        //    plan.SetNextNode();
        //    if (plan.currentNode == null) {
        //        log += "\nThis action is the end of plan.";
        //        if (plan.job != null) {
        //            log += "\nRemoving job in queue...";
        //            plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
        //        }
        //        PrintLogIfActive(log);
        //        //this means that this is the end goal so end this plan now
        //        if (!DropPlan(plan)) {
        //            //PlanGoapActions();
        //        }
        //    } else {
        //        log += "\nNext action for this plan: " + plan.currentNode.action.goapName;
        //        PrintLogIfActive(log);
        //        //PlanGoapActions();
        //    }
        //} else if(result == InteractionManager.Goap_State_Fail) {
        //    if(plan.endNode.action == action) {
        //        log += "\nAction performed has failed. Since this action is the end/goal action, it will not recalculate anymore. Dropping plan...";
        //        PrintLogIfActive(log);
        //        if (!DropPlan(plan)) {
        //            //PlanGoapActions();
        //        }
        //    } else {
        //        log += "\nAction performed has failed. Will try to recalculate plan...";
        //        if (plan.doNotRecalculate) {
        //            log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
        //            PrintLogIfActive(log);
        //            if (!DropPlan(plan)) {
        //                //PlanGoapActions();
        //            }
        //        } else {
        //            PrintLogIfActive(log);
        //            RecalculatePlan(plan);
        //            //IdlePlans();
        //        }
        //    }
        //}
    }
    public bool DropPlan(GoapPlan plan, bool forceCancelJob = false) {
        if (allGoapPlans.Remove(plan)) {
            Debug.Log(GameManager.Instance.TodayLogString() + plan.name + " was removed from " + this.name + "'s plan list");
            plan.EndPlan();
            if (plan.job != null) {
                if (plan.job.cancelJobOnFail || plan.job.cancelJobOnDropPlan || forceCancelJob) {
                    plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
                }
                plan.job.SetAssignedCharacter(null);
                plan.job.SetAssignedPlan(null);
            }
            //if (allGoapPlans.Count <= 0) {
            //    PlanGoapActions();
            //    return true;
            //    //StartDailyGoapPlanGeneration();
            //}
            return true;
        }
        return false;
    }
    public bool JustDropPlan(GoapPlan plan) {
        if (allGoapPlans.Remove(plan)) {
            Debug.Log(GameManager.Instance.TodayLogString() + plan.name + " was removed from " + this.name + "'s plan list");
            plan.EndPlan();
            if (plan.job != null) {
                if (plan.job.cancelJobOnDropPlan) {
                    plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
                }
                plan.job.SetAssignedCharacter(null);
                plan.job.SetAssignedPlan(null);
            }
            return true;
        }
        return false;
    }
    public void DropAllPlans(GoapPlan planException = null) {
        if (planException == null) {
            while (allGoapPlans.Count > 0) {
                DropPlan(allGoapPlans[0]);
            }
        } else {
            for (int i = 0; i < allGoapPlans.Count; i++) {
                if (allGoapPlans[i] != planException) {
                    DropPlan(allGoapPlans[i]);
                    i--;
                }
            }
        }
    }
    public void JustDropAllPlansOfType(INTERACTION_TYPE type) {
        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan currPlan = allGoapPlans[i];
            if (currPlan.endNode.action != null && currPlan.endNode.action.goapType == type) {
                if (JustDropPlan(currPlan)) {
                    i--;
                }
            }
        }
    }
    public void DropAllPlansOfType(INTERACTION_TYPE type) {
        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan currPlan = allGoapPlans[i];
            if (currPlan.endNode.action != null && currPlan.endNode.action.goapType == type) {
                if (DropPlan(currPlan)) {
                    i--;
                }
            }
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
            List<IPointOfInterest> characterAwarenesses = awareness[POINT_OF_INTEREST_TYPE.CHARACTER];
            Character randomTarget = characterAwarenesses[UnityEngine.Random.Range(0, characterAwarenesses.Count)] as Character;
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
        if (this != target && !this.isDead && gridTileLocation != null) {
            if (target is Character) {
                Character targetCharacter = target as Character;
                if (targetCharacter.isDead) {
                    return;
                }
                CharacterMarker lookAtMarker = targetCharacter.currentParty.owner.marker;
                if (lookAtMarker.character != this) {
                    marker.LookAt(lookAtMarker.transform.position);
                }
            } else {
                if (target.gridTileLocation == null) {
                    return;
                }
                marker.LookAt(target.gridTileLocation.centeredWorldLocation);
            }
        }
    }
    public void SetCurrentAction(GoapAction action) {
        //if(currentAction != null && action == null) {
        //    //This means that the current action of this character is being set to null, when this happens, the poi target of the current action must not wait for interaction anymore
        //    if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //        Character targetCharacter = currentAction.poiTarget as Character;
        //        if (targetCharacter != currentAction.actor) {
        //            targetCharacter.AdjustIsWaitingForInteraction(-1);
        //            targetCharacter.RemoveTargettedByAction(currentAction);
        //        }
        //    }
        //}
        if (currentAction != null) {
            previousCurrentAction = currentAction;
        }
        currentAction = action;
        if (currentAction != null) {
            PrintLogIfActive(GameManager.Instance.TodayLogString() + this.name + " will do action " + action.goapType.ToString() + " to " + action.poiTarget.ToString());
            if (currentAction.goapType.IsHostileAction()) { //if the character will do a combat action, remove all ignore hostilities value
                ClearIgnoreHostilities();
            }
            stateComponent.SetStateToDo(null, stopMovement: false);
        }
        string summary = GameManager.Instance.TodayLogString() + "Set current action to ";
        if (currentAction == null) {
            summary += "null";
        } else {
            summary += currentAction.goapName + " targetting " + currentAction.poiTarget.name;
        }
        //summary += "\n StackTrace: " + StackTraceUtility.ExtractStackTrace();

        actionHistory.Add(summary);
        if (actionHistory.Count > 10) {
            actionHistory.RemoveAt(0);
        }
    }
    public void SetHasAlreadyAskedForPlan(bool state) {
        _hasAlreadyAskedForPlan = state;
    }
    public void PrintLogIfActive(string log) {
        if (InteriorMapManager.Instance.currentlyShowingArea == specificLocation) {//UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == this
            Debug.Log(log);
        }
    }
    public void AddTargettedByAction(GoapAction action) {
        if (this != action.actor) { // && !isDead
            targettedByAction.Add(action);
            if (marker != null) {
                marker.OnCharacterTargettedByAction(action);
            }
        }
    }
    public void RemoveTargettedByAction(GoapAction action) {
        if (targettedByAction.Remove(action)) {
            if (marker != null) {
                marker.OnCharacterRemovedTargettedByAction(action);
            }
        }
    }
    //This stops and removes from job queue all action targetting this character
    public void StopAllActionTargettingThis() {
        for (int i = 0; i < targettedByAction.Count; i++) {
            if (!targettedByAction[i].isDone) {
                targettedByAction[i].StopAction(true);
                i--;
            }
        }
    }
    private void CancelCurrentAction(Character target, string cause) {
        if (this != target && !isDead && currentAction != null && currentAction.poiTarget == target && !currentAction.cannotCancelAction && currentAction.goapType != INTERACTION_TYPE.WATCH) {
            RegisterLogAndShowNotifToThisCharacterOnly("Generic", "action_cancelled_cause", null, cause);
            currentAction.StopAction();
        }
    }
    //This will only stop the current action of this character, this is different from StopAction because this will not drop the plan if the actor is not performing it but is on the way
    //This does not stop the movement of this character, call StopMovement separately to stop movement
    public void StopCurrentAction(bool shouldDoAfterEffect = true) {
        if(currentAction != null) {
            //Debug.Log("Stopped action of " + name + " which is " + currentAction.goapName);
            if (currentAction.isPerformingActualAction && !currentAction.isDone) {
                if (!shouldDoAfterEffect) {
                    currentAction.OnStopActionDuringCurrentState();
                }
                if(currentAction.currentState != null) {
                    currentAction.currentState.EndPerTickEffect(shouldDoAfterEffect);
                } else {
                    SetCurrentAction(null);
                }
            } else {
                currentAction.OnStopActionWhileTravelling();
                SetCurrentAction(null);
            }
        }
    }
    public void OnStartPerformGoapAction(GoapAction action, ref bool willStillContinueAction) {
        bool stillContinueCurrentAction = true;
        for (int i = 0; i < normalTraits.Count; i++) {
            Trait trait = normalTraits[i];
            if (!trait.isDisabled) {
                if (trait.OnStartPerformGoapAction(action, ref stillContinueCurrentAction)) {
                    willStillContinueAction = stillContinueCurrentAction;
                    break;
                } else {
                    stillContinueCurrentAction = true;
                }
            }
        }
    }
    public void ResetSleepTicks() {
        currentSleepTicks = CharacterManager.Instance.defaultSleepTicks;
    }
    public void AdjustSleepTicks(int amount) {
        currentSleepTicks += amount;
        if(currentSleepTicks <= 0) {
            ResetSleepTicks();
        }
    }
    #endregion

    #region Supply
    public void AdjustSupply(int amount) {
        supply += amount;
        if (supply < 0) {
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

    #region Food
    public void AdjustFood(int amount) {
        food += amount;
        if (food < 0) {
            food = 0;
        }
    }
    public void SetFood(int amount) {
        food = amount;
        if (food < 0) {
            food = 0;
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
    public bool IsHostileWith(Character character, bool checkIgnoreHostility = true) {
        //return true;
        if (character.isDead || this.isDead) {
            return false;
        }

        //if (character.ignoreHostility > 0) {
        //    //if the other character is set to ignore hostilities, check if the character's current action is a combat action or state is a combat state
        //    //if it is, waive the ignore hostility value
        //    if (true) {

        //    }
        //}

        if (checkIgnoreHostility && (character.ignoreHostility > 0 || this.ignoreHostility > 0)) {
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
        //Debug.Log(GameManager.Instance.TodayLogString() + "Adjusted " + name + "'s ignore hostilities by " + amount + ". Ignore hostiles value is " + ignoreHostility.ToString());
    }
    public void SetIgnoreHostilities(int amount) {
        ignoreHostility = amount;
        ignoreHostility = Mathf.Max(0, ignoreHostility);
        //Debug.Log(GameManager.Instance.TodayLogString() + "Adjusted " + name + "'s ignore hostilities by " + amount + ". Ignore hostiles value is " + ignoreHostility.ToString());
    }
    public void ClearIgnoreHostilities() {
        ignoreHostility = 0;
        //Debug.Log(GameManager.Instance.TodayLogString() + name + " clreared ignore hostiles.");
    }
    /// <summary>
    /// Is the other character an outsider. (Not part of this character's faction)
    /// </summary>
    /// <param name="otherCharacter"></param>
    /// <returns></returns>
    public bool IsHostileOutsider(Character otherCharacter) {
        //return true;
        if (otherCharacter.isDead || this.isDead) {
            return false;
        }
        return this.faction.id != otherCharacter.faction.id;
    }
    #endregion

    #region Crime System
    /// <summary>
    /// Make this character react to a crime that he/she witnessed.
    /// </summary>
    /// <param name="witnessedCrime">Witnessed Crime.</param>
    public void ReactToCrime(GoapAction witnessedCrime, ref bool hasRelationshipDegraded) {
        ReactToCrime(witnessedCrime.committedCrime, witnessedCrime, witnessedCrime.actorAlterEgo, ref hasRelationshipDegraded, witnessedCrime);
        witnessedCrime.OnWitnessedBy(this);
    }
    /// <summary>
    /// A variation of react to crime in which the parameter SHARE_INTEL_STATUS will be the one to determine if it is informed or witnessed crime
    /// Returns true or false, if the relationship between the reactor and the criminal has degraded
    /// </summary>
    public bool ReactToCrime(CRIME committedCrime, GoapAction crimeAction, AlterEgoData criminal, SHARE_INTEL_STATUS status) {
        bool hasRelationshipDegraded = false;
        if (status == SHARE_INTEL_STATUS.WITNESSED) {
            ReactToCrime(committedCrime, crimeAction, criminal, ref hasRelationshipDegraded, crimeAction, null);
        }else if (status == SHARE_INTEL_STATUS.INFORMED) {
            ReactToCrime(committedCrime, crimeAction, criminal, ref hasRelationshipDegraded, null, crimeAction);
        } else {
            Debug.LogError("The share intel status is neither INFORMED or WITNESSED");
        }
        return hasRelationshipDegraded;
    }
    /// <summary>
    /// Base function for crime reactions
    /// </summary>
    /// <param name="committedCrime">The type of crime that was committed.</param>
    /// <param name="criminal">The character that committed the crime</param>
    /// <param name="witnessedCrime">The crime witnessed by this character, if this is null, character was only informed of the crime by someone else.</param>
    /// <param name="informedCrime">The crime this character was informed of. NOTE: Should only have value if Share Intel</param>
    public void ReactToCrime(CRIME committedCrime, GoapAction crimeAction, AlterEgoData criminal, ref bool hasRelationshipDegraded, GoapAction witnessedCrime = null, GoapAction informedCrime = null) {
        //NOTE: Moved this to be per action specific. See GoapAction.IsConsideredACrimeBy and GoapAction.CanReactToThisCrime for necessary mechanics.
        //if (witnessedCrime != null) {
        //    //if the action that should be considered a crime is part of a job from this character's area, do not consider it a crime
        //    if (witnessedCrime.parentPlan.job != null
        //        && homeArea.jobQueue.jobsInQueue.Contains(witnessedCrime.parentPlan.job)) {
        //        return;
        //    }
        //    //if the witnessed crime is targetting this character, this character should not react to the crime if the crime's doesNotStopTargetCharacter is true
        //    if (witnessedCrime.poiTarget == this && witnessedCrime.doesNotStopTargetCharacter) {
        //        return;
        //    }
        //}

        string reactSummary = GameManager.Instance.TodayLogString() + this.name + " will react to crime committed by " + criminal.owner.name;
        if(committedCrime == CRIME.NONE) {
            reactSummary += "\nNo reaction because committed crime is " + committedCrime.ToString();
            PrintLogIfActive(reactSummary);
            return;
        }
        Log witnessLog = null;
        Log reportLog = null;
        RELATIONSHIP_EFFECT relationshipEfffectWithCriminal = GetRelationshipEffectWith(criminal);
        CRIME_CATEGORY category = committedCrime.GetCategory();

        //If character witnessed an Infraction crime:
        if (category == CRIME_CATEGORY.INFRACTIONS) {
            //-Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]."
            //- Report / Share Intel Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]."
            //- no additional response
            reactSummary += "\nCrime committed is infraction.";
            witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed");
            reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed");
        }
        //If character has a positive relationship (Friend, Lover, Paramour) with the criminal
        else if (relationshipEfffectWithCriminal == RELATIONSHIP_EFFECT.POSITIVE) {
            reactSummary += "\n" + this.name + " has a positive relationship with " + criminal.owner.name;
            //and crime severity is a Misdemeanor:
            if (category == CRIME_CATEGORY.MISDEMEANOR) {
                reactSummary += "\nCrime committed is misdemeanor.";
                //- Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder] but did not do anything due to their relationship."
                //-Report / Share Intel Log: "[Character Name] was informed that [Criminal Name] committed [Theft/Assault/Murder] but did not do anything due to their relationship."
                witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "do_nothing");
                reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_do_nothing");
            }
            //and crime severity is Serious Crimes or worse:
            else if (category.IsGreaterThanOrEqual(CRIME_CATEGORY.SERIOUS)) {
                reactSummary += "\nCrime committed is serious or worse. Removing positive relationships.";
                //- Relationship Degradation between Character and Criminal
                hasRelationshipDegraded = CharacterManager.Instance.RelationshipDegradation(criminal.owner, this, witnessedCrime);
                if (hasRelationshipDegraded) {
                    witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed_degraded");
                    reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_witnessed_degraded");
                    PerRoleCrimeReaction(committedCrime, crimeAction, criminal, witnessedCrime, informedCrime);
                } else {
                    if (witnessedCrime != null) {
                        if (marker.inVisionCharacters.Contains(criminal.owner)) {
                            marker.AddAvoidInRange(criminal.owner);
                        }
                    }
                    witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "do_nothing");
                    reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_do_nothing");
                }

            }
        }
        //If character has no relationships with the criminal or they are enemies and the crime is a Misdemeanor or worse:
        else if ((!this.HasRelationshipWith(criminal) || this.HasRelationshipOfTypeWith(criminal, RELATIONSHIP_TRAIT.ENEMY)) 
            && category.IsGreaterThanOrEqual(CRIME_CATEGORY.MISDEMEANOR)) {
            reactSummary += "\n" + this.name + " does not have a relationship with or is an enemy of " + criminal.name + " and the committed crime is misdemeanor or worse";
            //- Relationship Degradation between Character and Criminal
            hasRelationshipDegraded = CharacterManager.Instance.RelationshipDegradation(criminal.owner, this, witnessedCrime);
            //- Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]!"
            if (hasRelationshipDegraded) {
                witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed_degraded");
                reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_witnessed_degraded");
                PerRoleCrimeReaction(committedCrime, crimeAction, criminal, witnessedCrime, informedCrime);
            } else {
                if (witnessedCrime != null) {
                    if (marker.inVisionCharacters.Contains(criminal.owner)) {
                        marker.AddAvoidInRange(criminal.owner);
                    }
                }
                witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "do_nothing");
                reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_do_nothing");
            }
        }

        //if (witnessedCrime != null) {
        //    if (witnessLog != null) {
        //        witnessLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        witnessLog.AddToFillers(criminal.owner, criminal.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //        witnessLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(committedCrime.ToString()), LOG_IDENTIFIER.STRING_1);
        //        if (this != witnessedCrime.poiTarget) {
        //            PlayerManager.Instance.player.ShowNotificationFrom(this, witnessLog);
        //        }
        //    }
        //} else {
        //    if (reportLog != null) {
        //        reportLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        reportLog.AddToFillers(criminal.owner, criminal.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //        reportLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(committedCrime.ToString()), LOG_IDENTIFIER.STRING_1);
        //        PlayerManager.Instance.player.ShowNotificationFrom(this, reportLog);
        //    }
        //}

        PrintLogIfActive(reactSummary);
    }
    /// <summary>
    /// Crime reactions per role.
    /// </summary>
    /// <param name="committedCrime">The type of crime that was committed.</param>
    /// <param name="criminal">The character that committed the crime</param>
    /// <param name="witnessedCrime">The crime witnessed by this character, if this is null, character was only informed of the crime by someone else.</param>
    /// <param name="informedCrime">The crime this character was informed of. NOTE: Should only have value if Share Intel</param>
    private void PerRoleCrimeReaction(CRIME committedCrime, GoapAction crimeAction, AlterEgoData criminal, GoapAction witnessedCrime = null, GoapAction informedCrime = null) {
        GoapPlanJob job = null;
        switch (role.roleType) {
            case CHARACTER_ROLE.CIVILIAN:
            case CHARACTER_ROLE.ADVENTURER:
                //- If the character is a Civilian or Adventurer, he will enter Flee mode (fleeing the criminal) and will create a Report Crime Job Type in his personal job queue
                if (this.faction != FactionManager.Instance.neutralFaction && criminal.faction == this.faction) {
                    //only make character flee, if he/she actually witnessed the crime (not share intel)
                    GoapAction crimeToReport = informedCrime;
                    if (witnessedCrime != null) {
                        crimeToReport = witnessedCrime;
                        //if a character has no negative disabler traits. Do not Flee. This is so that the character will not also add a Report hostile job
                        if (!this.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) { 
                            this.marker.AddHostileInRange(criminal.owner, false);
                        }
                    }
                    job = new GoapPlanJob(JOB_TYPE.REPORT_CRIME, INTERACTION_TYPE.REPORT_CRIME, new Dictionary<INTERACTION_TYPE, object[]>() {
                        { INTERACTION_TYPE.REPORT_CRIME,  new object[] { committedCrime, criminal, crimeToReport }}
                    });
                    //job.SetCannotOverrideJob(true);
                    jobQueue.AddJobInQueue(job);
                }
                break;
            case CHARACTER_ROLE.LEADER:
            case CHARACTER_ROLE.NOBLE:
                //- If the character is a Noble or Faction Leader, the criminal will gain the relevant Crime-type trait
                //If he is a Noble or Faction Leader, he will create the Apprehend Job Type in the Location job queue instead.
                if (this.faction != FactionManager.Instance.neutralFaction && criminal.faction == this.faction) {
                    //only add apprehend job if the criminal is part of this characters faction
                    criminal.owner.AddCriminalTrait(committedCrime, crimeAction);
                    CreateApprehendJobFor(criminal.owner);
                    //job = new GoapPlanJob("Apprehend", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = actor });
                    //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = actor }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                    //job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
                    //homeArea.jobQueue.AddJobInQueue(job);
                }

                break;
            case CHARACTER_ROLE.SOLDIER:
            case CHARACTER_ROLE.BANDIT:
                //- If the character is a Soldier, the criminal will gain the relevant Crime-type trait
                if (this.faction != FactionManager.Instance.neutralFaction && criminal.faction == this.faction) {
                    //only add apprehend job if the criminal is part of this characters faction
                    criminal.owner.AddCriminalTrait(committedCrime, crimeAction);
                    //- If the character is a Soldier, he will also create an Apprehend Job Type in his personal job queue.
                    //job = new GoapPlanJob("Apprehend", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = actor });
                    //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = actor }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                    //job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
                    //homeArea.jobQueue.AddJobInQueue(job);
                    job = CreateApprehendJobFor(criminal.owner);
                    if (job != null) {
                        homeArea.jobQueue.ForceAssignCharacterToJob(job, this);
                    }
                }
                break;
            default:
                break;
        }
    }
    public void AddCriminalTrait(CRIME crime, GoapAction crimeAction) {
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
            case CRIME.ATTEMPTED_MURDER:
                trait = new AttemptedMurderer();
                break;
            case CRIME.ABERRATION:
                trait = new Aberration();
                break;
            default:
                break;
        }
        if (trait != null) {
            AddTrait(trait, null, null, crimeAction);
        }
    }
    public bool CanReactToCrime() {

        if (stateComponent.currentState == null) {
            return GetNormalTrait("Resting", "Unconscious") != null;
        } else {
            if (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                return false;
            }
        }
        return GetNormalTrait("Resting", "Unconscious") != null;
    }
    #endregion

    #region Mood
    public void SetMoodValue(int amount) {
        moodValue = amount;
        moodValue = Mathf.Clamp(moodValue, 1, 100);
    }
    public void AdjustMoodValue(int amount, Trait fromTrait, GoapAction triggerAction = null) {
        moodValue += amount;
        moodValue = Mathf.Clamp(moodValue, 1, 100);
        if(amount < 0 && currentMoodType == CHARACTER_MOOD.DARK) {
            if (doNotDisturb > 0) {
                return;
            }
            if(currentAction != null && currentAction.goapType == INTERACTION_TYPE.TANTRUM) {
                return;
            }
            string tantrumReason = "Became " + fromTrait.nameInUI;
            if (triggerAction != null) {
                tantrumReason = Utilities.LogReplacer(triggerAction.currentState.descriptionLog);
            }

            //string tantrumLog = this.name + "'s mood was adjusted by " + amount.ToString() + " and current mood is " + currentMoodType.ToString() + ".";
            //tantrumLog += "Reason: " + tantrumReason;
            //tantrumLog += "\nRolling for Tantrum..."; 

            int chance = UnityEngine.Random.Range(0, 100);

            //tantrumLog += "\nRolled: " + chance.ToString();

            if (chance < 10) {
                CancelAllJobsAndPlans();
                //Create Tantrum action
                GoapPlanJob tantrum = new GoapPlanJob(JOB_TYPE.TANTRUM, INTERACTION_TYPE.TANTRUM, this, new Dictionary<INTERACTION_TYPE, object[]>() {
                    { INTERACTION_TYPE.TANTRUM, new object[] { tantrumReason } }
                });
                //tantrum.SetCannotOverrideJob(true);
                //tantrum.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                jobQueue.AddJobInQueue(tantrum);
                //jobQueue.ProcessFirstJobInQueue(this);
                //tantrumLog += "\n" + this.name + " started having a tantrum!";
            }
            //Debug.Log(tantrumLog);
        }
    }
    public CHARACTER_MOOD ConvertCurrentMoodValueToType() {
        return ConvertMoodValueToType(moodValue);
    }
    public CHARACTER_MOOD ConvertMoodValueToType(int amount) {
        if (amount >= 1 && amount < 26) {
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

    #region Explore Items
    public ExploreState lastExploreState { get; private set; }
    /// <summary>
    /// Set the last explore state that ths character did.
    /// </summary>
    /// <param name="es">The explore state.</param>
    public void SetLastExploreState(ExploreState es) {
        lastExploreState = es;
    }
    public void OnReturnHome() {
        if (lastExploreState != null && lastExploreState.itemsCollected.Count > 0 && role.roleType == CHARACTER_ROLE.ADVENTURER) {
            //create deliver treasure job that will deposit the items that the character collected during his/her last explore action.
            lastExploreState.CreateDeliverTreasureJob();
        }
    }
    #endregion

    #region Pathfinding
    public List<LocationGridTile> GetTilesInRadius(int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        int mapSizeX = specificLocation.areaMap.map.GetUpperBound(0);
        int mapSizeY = specificLocation.areaMap.map.GetUpperBound(1);
        int x = gridTileLocation.localPlace.x;
        int y = gridTileLocation.localPlace.y;
        if (includeCenterTile) {
            tiles.Add(gridTileLocation);
        }
        int xLimitLower = x - radiusLimit;
        int xLimitUpper = x + radiusLimit;
        int yLimitLower = y - radiusLimit;
        int yLimitUpper = y + radiusLimit;


        for (int dx = x - radius; dx <= x + radius; dx++) {
            for (int dy = y - radius; dy <= y + radius; dy++) {
                if (dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                    if (dx == x && dy == y) {
                        continue;
                    }
                    if (radiusLimit > 0 && dx > xLimitLower && dx < xLimitUpper && dy > yLimitLower && dy < yLimitUpper) {
                        continue;
                    }
                    LocationGridTile result = specificLocation.areaMap.map[dx, dy];
                    if (!includeTilesInDifferentStructure && result.structure != gridTileLocation.structure) { continue; }
                    tiles.Add(result);
                }
            }
        }
        return tiles;
    }
    #endregion

    #region States
    private const float Combat_Signalled_Distance = 4f;
    private void OnCharacterStartedState(Character characterThatStartedState, CharacterState state) {
        if (characterThatStartedState == this) {
            marker.UpdateActionIcon();
            if (state.characterState.IsCombatState()) {
                ClearIgnoreHostilities();
            }
            if (state.characterState == CHARACTER_STATE.MOVE_OUT) {
                SetTirednessLowerBound(TIREDNESS_THRESHOLD_2);
                SetFullnessLowerBound(FULLNESS_THRESHOLD_2);
                SetHappinessLowerBound(HAPPINESS_THRESHOLD_2);
            }
        } else {
            if (state.characterState == CHARACTER_STATE.COMBAT && this.GetNormalTrait("Unconscious", "Resting") == null && isAtHomeArea && !ownParty.icon.isTravellingOutside) {
                //Reference: https://trello.com/c/2ZppIBiI/2428-combat-available-npcs-should-be-able-to-be-aware-of-hostiles-quickly
                CombatState combatState = state as CombatState;
                float distance = Vector2.Distance(this.marker.transform.position, characterThatStartedState.marker.transform.position);
                Character targetCharacter;
                if (combatState.isAttacking) {
                    targetCharacter = combatState.currentClosestHostile;
                } else {
                    targetCharacter = characterThatStartedState.marker.GetNearestValidAvoid();
                }
                //Debug.Log(this.name + " distance with " + characterThatStartedState.name + " is " + distance.ToString());
                if (targetCharacter != null && this.isPartOfHomeFaction && characterThatStartedState.isAtHomeArea && characterThatStartedState.isPartOfHomeFaction && this.IsCombatReady()
                    && this.IsHostileOutsider(targetCharacter) && (this.GetRelationshipEffectWith(characterThatStartedState) == RELATIONSHIP_EFFECT.POSITIVE || characterThatStartedState.role.roleType == CHARACTER_ROLE.SOLDIER)
                    && distance <= Combat_Signalled_Distance) {
                    if (marker.AddHostileInRange(targetCharacter, processCombatBehavior: false)) {
                        if (!marker.avoidInRange.Contains(targetCharacter)) {
                            Log joinLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "join_combat_signaled");
                            joinLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            joinLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                            joinLog.AddToFillers(characterThatStartedState, characterThatStartedState.name, LOG_IDENTIFIER.CHARACTER_3);
                            joinLog.AddLogToSpecificObjects(LOG_IDENTIFIER.ACTIVE_CHARACTER, LOG_IDENTIFIER.TARGET_CHARACTER);
                            PlayerManager.Instance.player.ShowNotification(joinLog);
                        }
                        //marker.ProcessCombatBehavior();
                        return; //do not do watch.
                    }
                }
                if (marker.inVisionCharacters.Contains(characterThatStartedState)) {
                    ThisCharacterWatchEvent(characterThatStartedState, null, null);
                }
            }
        }
    }
    public void OnCharacterEndedState(Character character, CharacterState state) {
        if (character == this) {
            if (state is CombatState && marker != null) {
                marker.OnThisCharacterEndedCombatState();
            }
            if (state.characterState == CHARACTER_STATE.MOVE_OUT) {
                SetTirednessLowerBound(0);
                SetFullnessLowerBound(0);
                SetHappinessLowerBound(0);
            }
        }
    }
    #endregion

    #region Alter Egos
    private void InitializeAlterEgos() {
        alterEgos = new Dictionary<string, AlterEgoData>();
        alterEgos.Add(CharacterManager.Original_Alter_Ego, new AlterEgoData(this, CharacterManager.Original_Alter_Ego));
        currentAlterEgoName = CharacterManager.Original_Alter_Ego;
        currentAlterEgo.SetFaction(faction);
        currentAlterEgo.SetCharacterClass(characterClass);
        currentAlterEgo.SetRace(race);
        currentAlterEgo.SetRole(role);
        currentAlterEgo.SetHomeStructure(homeStructure);
    }
    public AlterEgoData CreateNewAlterEgo(string alterEgoName) {
        if (alterEgos.ContainsKey(alterEgoName)) {
            throw new Exception(this.name + " already has an alter ego named " + alterEgoName + " but something is trying to create a new one!");
        }
        AlterEgoData newData = new AlterEgoData(this, alterEgoName);
        AddAlterEgo(newData);
        return newData;
    }
    public void AddAlterEgo(AlterEgoData data) {
        if (!alterEgos.ContainsKey(data.name)) {
            alterEgos.Add(data.name, data);
        }
    }
    public void RemoveAlterEgo(string alterEgoName) {
        if (alterEgoName == CharacterManager.Original_Alter_Ego) {
            throw new Exception("Something is trying to remove " + this.name + "'s original alter ego! This should not happen!");
        }
        if (currentAlterEgoName == alterEgoName) {
            //switch to the original alter ego
            SwitchAlterEgo(CharacterManager.Original_Alter_Ego);
        }
        if (alterEgos.ContainsKey(alterEgoName)) {
            alterEgos.Remove(alterEgoName);
        }
    }
    public bool isSwitchingAlterEgo { get; private set; } //is this character in the process of switching alter egos?
    public void SwitchAlterEgo(string alterEgoName) {
        if (currentAlterEgoName == alterEgoName) {
            return; //ignore change
        }
        if (alterEgos.ContainsKey(alterEgoName)) {
            isSwitchingAlterEgo = true;
            //apply all alter ego changes here
            AlterEgoData alterEgoData = alterEgos[alterEgoName];
            currentAlterEgo.CopySpecialTraits();

            //Drop all plans except for the current action
            AdjustIsWaitingForInteraction(1);
            if (currentAction != null && currentAction.parentPlan != null) {
                DropAllPlans(currentAction.parentPlan);
            } else {
                DropAllPlans();
            }
            AdjustIsWaitingForInteraction(-1);

            ResetFullnessMeter();
            ResetHappinessMeter();
            ResetTirednessMeter();
            RemoveAllNonPersistentTraits();

            SetHomeStructure(alterEgoData.homeStructure);
            ChangeFactionTo(alterEgoData.faction);
            AssignRole(alterEgoData.role);
            AssignClass(alterEgoData.characterClass);
            ChangeRace(alterEgoData.race);
            SetLevel(alterEgoData.level);

            CancelAllJobsTargettingThisCharacter("target is not found", false);
            Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, this, "target is not found");

            for (int i = 0; i < alterEgoData.traits.Count; i++) {
                AddTrait(alterEgoData.traits[i]);
            }
            currentAlterEgoName = alterEgoName;
            isSwitchingAlterEgo = false;
            Messenger.Broadcast(Signals.CHARACTER_SWITCHED_ALTER_EGO, this);
        } else {
            throw new Exception(this.name + " is trying to switch to alter ego " + alterEgoName + " but doesn't have an alter ego of that name!");
        }
    }
    public AlterEgoData GetAlterEgoData(string alterEgoName) {
        if (alterEgos.ContainsKey(alterEgoName)) {
            return alterEgos[alterEgoName];
        }
        return null;
    }
    #endregion
}