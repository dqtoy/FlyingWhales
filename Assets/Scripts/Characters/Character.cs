using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;

public class Character : ILeader, IPointOfInterest, IJobOwner {

    protected string _name;
    protected string _firstName;
    protected int _id;
    protected int _doNotDisturb;
    protected float _actRate;
    protected bool _isDead;
    protected bool _isChatting;
    protected bool _isFlirting;
    protected GENDER _gender;
    protected CharacterClass _characterClass;
    protected RaceSetting _raceSetting;
    protected CharacterRole _role;
    protected Faction _faction;
    protected Minion _minion;
    protected List<Log> _history;
    protected LocationStructure _currentStructure; //what structure is this character currently in.
    protected Region _currentRegion;

    //Stats
    protected SIDES _currentSide;
    protected int _currentHP;
    protected int _maxHP;
    protected int _level;
    protected int _experience;
    protected int _maxExperience;
    protected int _sp;

    //visuals
    public CharacterVisuals visuals { get; private set; }
    public IMapObjectVisual mapObjectVisual => marker;
    public int doNotRecoverHP { get; protected set; }
    public SEXUALITY sexuality { get; private set; }
    public int attackPowerMod { get; protected set; }
    public int speedMod { get; protected set; }
    public int maxHPMod { get; protected set; }
    public int attackPowerPercentMod { get; protected set; }
    public int speedPercentMod { get; protected set; }
    public int maxHPPercentMod { get; protected set; }
    public Region homeRegion { get; protected set; }
    //public Area homeArea => homeRegion.area;
    public Dwelling homeStructure { get; protected set; }
    public IRelationshipContainer relationshipContainer => currentAlterEgo.relationshipContainer;
    public IRelationshipValidator relationshipValidator => currentAlterEgo.relationshipValidator;
    public List<INTERACTION_TYPE> advertisedActions { get; private set; }
    public int supply { get; set; }
    public int food { get; set; }
    public CharacterMarker marker { get; private set; }
    public JobQueueItem currentJob { get; private set; }
    public GoapPlan currentPlan { get; private set; }
    public ActualGoapNode currentActionNode { get; private set; }
    public ActualGoapNode previousCurrentActionNode { get; private set; }
    public Character lastAssaultedCharacter { get; private set; }
    public List<SpecialToken> items { get; private set; }
    public JobQueue jobQueue { get; private set; }
    public List<JobQueueItem> allJobsTargettingThis { get; private set; }
    public int moodValue { get; private set; }
    public bool canCombat { get; private set; } //This should only be a getter but since we need to know when the value changes it now has a setter
    public List<Trait> traitsNeededToBeRemoved { get; private set; }
    public TrapStructure trapStructure { get; private set; }
    public bool isDisabledByPlayer { get; protected set; }
    public float speedModifier { get; private set; }
    public string deathStr { get; private set; }
    public TileObject tileObjectLocation { get; private set; }
    public CharacterTrait defaultCharacterTrait { get; private set; }
    public int isStoppedByOtherCharacter { get; private set; } //this is increased, when the action of another character stops this characters movement
    public Party ownParty { get; protected set; }
    public Party currentParty { get; protected set; }

    private List<System.Action> onLeaveAreaActions;
    private POI_STATE _state;
    public Dictionary<int, Combat> combatHistory;

    //limiters
    private int canWitnessValue; //if this is >= 0 then character can witness events
    private int canMoveValue; //if this is >= 0 then character can move
    private int canBeAtttackedValue; //if this is >= 0 then character can be attacked
    public bool canWitness => canWitnessValue >= 0;
    public bool canMove => canMoveValue >= 0;
    public bool canBeAtttacked => canBeAtttackedValue >= 0;

    //Needs
    public CharacterNeedsComponent needsComponent { get; private set; }

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

    //Components / Managers
    public GoapPlanner planner { get; private set; }
    public BuildStructureComponent buildStructureComponent { get; private set; }
    public CharacterStateComponent stateComponent { get; private set; }
    public NonActionEventsComponent nonActionEventsComponent { get; private set; }
    public OpinionComponent opinionComponent { get; private set; }

    #region getters / setters
    public virtual string name => _firstName;
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
    public int id => _id;
    public bool isDead => this._isDead;
    public bool isFactionless { //is the character part of the neutral faction? or no faction?
        get {
            if (faction == null || FactionManager.Instance.neutralFaction == faction) {
                return true;
            } else {
                return false;
            }
        }
    }
    public bool isFriendlyFactionless { //is the character part of the friendly neutral faction? or no faction?
        get {
            if (faction == null || FactionManager.Instance.friendlyNeutralFaction == faction) {
                return true;
            } else {
                return false;
            }
        }
    }
    public bool isLeader => characterClass.className == "Leader";
    public bool isHoldingItem => items.Count > 0;
    public bool isAtHomeRegion => currentRegion == homeRegion && !currentParty.icon.isTravellingOutside;
    public bool isPartOfHomeFaction => homeRegion != null && faction != null && homeRegion.IsFactionHere(faction); //is this character part of the faction that owns his home area
    public bool isChatting => _isChatting;
    public bool isFlirting => _isFlirting;
    public GENDER gender => _gender;
    public RACE race => _raceSetting.race;
    public CharacterClass characterClass => _characterClass;
    public RaceSetting raceSetting => _raceSetting;
    public CharacterRole role => _role;
    public Faction faction => _faction;
    public Faction factionOwner => _faction;
    //public Area currentArea => currentRegion.area;
    public Region currentRegion {
        get {
            if (!IsInOwnParty()) {
                return currentParty.owner.currentRegion;
            }
            return _currentRegion;
        }
    }
    public List<Log> history => _history;
    public int level => _level;
    public int experience => _experience;
    public int maxExperience => _maxExperience;
    public int speed {
        get {
            int total = (int) ((_characterClass.baseSpeed + speedMod) * (1f + ((_raceSetting.speedModifier + speedPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int attackPower {
        get {
            int total = (int) ((_characterClass.baseAttackPower + attackPowerMod) * (1f + ((_raceSetting.attackPowerModifier + attackPowerPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int maxHP => _maxHP;
    public int currentHP => this._currentHP;
    public int attackSpeed => _characterClass.baseAttackSpeed; //in milliseconds, The lower the amount the faster the attack rate
    public Minion minion => _minion;
    public int doNotDisturb => _doNotDisturb;
    public POINT_OF_INTEREST_TYPE poiType => POINT_OF_INTEREST_TYPE.CHARACTER;
    public LocationGridTile gridTileLocation {
        get {
            if (marker == null) {
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
            return new Vector2Int(Mathf.FloorToInt(marker.anchoredPos.x), Mathf.FloorToInt(marker.anchoredPos.y));
        }
    }
    public POI_STATE state => _state;
    public CHARACTER_MOOD currentMoodType => ConvertCurrentMoodValueToType();
    public AlterEgoData currentAlterEgo {
        get {
            if (alterEgos == null || !alterEgos.ContainsKey(currentAlterEgoName)) {
                Debug.LogWarning(this.name + " Alter Ego Relationship Problem! Current alter ego is: " + currentAlterEgoName);
                return null;
            }
            return alterEgos[currentAlterEgoName];
        }
    }
    //public Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness {
    //    get {
    //        return currentAlterEgo.awareness;
    //    }
    //}
    public LocationStructure currentStructure {
        get {
            if (!IsInOwnParty()) {
                return currentParty.owner.currentStructure;
            }
            return _currentStructure;
        }
    }
    public float walkSpeed => raceSetting.walkSpeed + (raceSetting.walkSpeed * characterClass.walkSpeedMod);
    public float runSpeed => raceSetting.runSpeed + (raceSetting.runSpeed * characterClass.runSpeedMod);
    public Vector3 worldPosition => marker.transform.position;
    public ProjectileReceiver projectileReceiver => marker.collisionTrigger.projectileReceiver;
    public JOB_OWNER ownerType => JOB_OWNER.CHARACTER;
    public bool isInCombat => stateComponent.currentState != null && stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT;
    public Transform worldObject => marker.transform;
    public bool isStillConsideredAlive => minion == null && !(this is Summon) && !faction.isPlayerFaction;
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
        GenerateSexuality();
        StartingLevel();
        InitializeAlterEgos();
        visuals = new CharacterVisuals(this);
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
        GenerateSexuality();
        StartingLevel();
        InitializeAlterEgos();
        visuals = new CharacterVisuals(this);
    }
    public Character(CharacterRole role, string className, RACE race, GENDER gender, SEXUALITY sexuality) : this() {
        _id = Utilities.SetID(this);
        _gender = gender;
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        AssignRole(role, false);
        _characterClass = CharacterManager.Instance.CreateNewCharacterClass(className);
        originalClassName = _characterClass.className;
        SetName(RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender));
        SetSexuality(sexuality);
        StartingLevel();
        InitializeAlterEgos();
        visuals = new CharacterVisuals(this);
    }
    public Character(SaveDataCharacter data) {
        _id = Utilities.SetID(this, data.id);
        _gender = data.gender;
        SetSexuality(data.sexuality);
        _characterClass = CharacterManager.Instance.CreateNewCharacterClass(data.className);
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[data.race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        AssignRole(CharacterManager.Instance.GetRoleByRoleType(data.roleType), false);
        SetName(data.name);
        visuals = new CharacterVisuals(data);

        currentAlterEgoName = data.currentAlterEgoName;
        originalClassName = data.originalClassName;
        isStoppedByOtherCharacter = data.isStoppedByOtherCharacter;

        _history = new List<Log>();
        combatHistory = new Dictionary<int, Combat>();
        advertisedActions = new List<INTERACTION_TYPE>();
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

        alterEgos = new Dictionary<string, AlterEgoData>();
        items = new List<SpecialToken>();
        SetIsDead(data.isDead);
    }
    public Character() {
        SetIsDead(false);
        _history = new List<Log>();
        
        //Needs
        needsComponent = new CharacterNeedsComponent(this);
        
        //RPG
        SetExperience(0);

        //Traits
        CreateTraitContainer();

        combatHistory = new Dictionary<int, Combat>();
        advertisedActions = new List<INTERACTION_TYPE>();
        items = new List<SpecialToken>();
        allJobsTargettingThis = new List<JobQueueItem>();
        traitsNeededToBeRemoved = new List<Trait>();
        onLeaveAreaActions = new List<Action>();
        pendingActionsAfterMultiThread = new List<Action>();
        SetPOIState(POI_STATE.ACTIVE);
        needsComponent.ResetSleepTicks();

        //for testing
        locationHistory = new List<string>();
        actionHistory = new List<string>();

        //hostiltiy
        ignoreHostility = 0;

        //Components
        stateComponent = new CharacterStateComponent(this);
        jobQueue = new JobQueue(this);
        trapStructure = new TrapStructure();
        planner = new GoapPlanner(this);
        nonActionEventsComponent = new NonActionEventsComponent(this);
        opinionComponent = new OpinionComponent(this);
    }

    //This is done separately after all traits have been loaded so that the data will be accurate
    //It is because all traits are added again, this would mean that OnAddedTrait will also be called
    //Some values of character are modified by adding traits, so since adding trait will still be processed, it will get modified twice or more
    //For example, the Glutton trait adds fullnessDecreaseRate by 50%
    //Now when the fullnessDecreaseRate value is loaded the value of it already includes the Glutton trait modification
    //But since the Glutton trait will process the add trait function, fullnessDecreaseRate will add by 50% again
    //So for example if the saved value is 150, then the loaded value will be 300 (150+150)
    public void LoadAllStatsOfCharacter(SaveDataCharacter data) {
        //_doNotDisturb = data.doNotDisturb;
        //_doNotGetHungry = data.doNotGetHungry;
        //_doNotGetLonely = data.doNotGetLonely;
        //_doNotGetTired = data.doNotGetTired;

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

        //currentInteractionTypes = data.currentInteractionTypes;
        supply = data.supply;
        moodValue = data.moodValue;
        canCombat = data.isCombatant;
        isDisabledByPlayer = data.isDisabledByPlayer;
        speedModifier = data.speedModifier;
        deathStr = data.deathStr;
        _state = data.state;

        needsComponent.LoadAllStatsOfCharacter(data);

        ignoreHostility = data.ignoreHostility;
        returnedToLife = data.returnedToLife;
        
    }
    /// <summary>
    /// Initialize data for this character that is not safe to put in the constructor.
    /// Usually this is data that is dependent on the character being fully constructed.
    /// </summary>
    public virtual void Initialize() {
        OnUpdateRace();
        OnUpdateCharacterClass();

        SetMoodValue(90);
        CreateOwnParty();

        needsComponent.Initialize();
        
        //supply
        SetSupply(UnityEngine.Random.Range(10, 61)); //Randomize initial supply per character (Random amount between 10 to 60.)
    }
    public void InitialCharacterPlacement(LocationGridTile tile) {
        needsComponent.InitialCharacterPlacement();
        ConstructInitialGoapAdvertisementActions();
        marker.InitialPlaceMarkerAt(tile, false); //since normal characters are already placed in their areas.
        //AddInitialAwareness();
        SubscribeToSignals();
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            traitContainer.allTraits[i].OnOwnerInitiallyPlaced(this);
        }
    }
    public void LoadInitialCharacterPlacement(LocationGridTile tile) {
        ConstructInitialGoapAdvertisementActions();
        //#if !WORLD_CREATION_TOOL
        //        GameDate gameDate = GameManager.Instance.Today();
        //        gameDate.AddTicks(1);
        //        SchedulingManager.Instance.AddEntry(gameDate, () => PlanGoapActions(), this);
        //#endif
        marker.InitialPlaceMarkerAt(tile, false); //since normal characters are already placed in their areas.
        //AddInitialAwareness();
        SubscribeToSignals();
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            traitContainer.allTraits[i].OnOwnerInitiallyPlaced(this);
        }
    }

    #region Signals
    protected void SubscribeToSignals() {
        if (minion != null) {
            Debug.LogError(name + " is a minion and has subscribed to the signals!");
        }
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener(Signals.DAY_STARTED, DailyGoapProcesses);
        Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnLeaveArea);
        Messenger.AddListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnArrivedAtArea);
        Messenger.AddListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETTING_CHARACTER, ForceCancelAllJobsTargettingCharacter);
        Messenger.AddListener<Area>(Signals.SUCCESS_INVASION_AREA, OnSuccessInvadeArea);
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
        Messenger.AddListener<Character>(Signals.SCREAM_FOR_HELP, HeardAScream);
        Messenger.AddListener<string, ActualGoapNode>(Signals.AFTER_ACTION_STATE_SET, OnAfterActionStateSet);
        needsComponent.SubscribeToSignals();
    }
    public virtual void UnsubscribeSignals() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.DAY_STARTED, DailyGoapProcesses);
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnLeaveArea);
        Messenger.RemoveListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnArrivedAtArea);
        Messenger.RemoveListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETTING_CHARACTER, ForceCancelAllJobsTargettingCharacter);
        Messenger.RemoveListener<Area>(Signals.SUCCESS_INVASION_AREA, OnSuccessInvadeArea);
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
        Messenger.RemoveListener<Character>(Signals.SCREAM_FOR_HELP, HeardAScream);
        Messenger.RemoveListener<string, ActualGoapNode>(Signals.AFTER_ACTION_STATE_SET, OnAfterActionStateSet);
        needsComponent.UnsubscribeToSignals();
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
                if (character.IsInOwnParty()) {
                    marker.RemoveTerrifyingObject(character);
                    if (character.ownParty.isCarryingAnyPOI) {
                        marker.RemoveTerrifyingObject(character.ownParty.carriedPOI);
                    }
                } else {
                    marker.RemoveTerrifyingObject(character.currentParty.owner);
                    if (character.currentParty.isCarryingAnyPOI) {
                        marker.RemoveTerrifyingObject(character.currentParty.carriedPOI);
                    }
                }
                //for (int i = 0; i < party.characters.Count; i++) {
                //    marker.RemoveTerrifyingObject(party.characters[i]);
                //}
            }
        }
    }
    /// <summary>
    /// Listener for when the player successfully invades an area. And this character is still alive.
    /// </summary>
    /// <param name="area">The invaded area.</param>
    protected virtual void OnSuccessInvadeArea(Area area) {
        if (currentRegion.area == area && minion == null) {
            StopCurrentActionNode(false);
            if (stateComponent.currentState != null) {
                stateComponent.ExitCurrentState();
            }
            //else if (stateComponent.stateToDo != null) {
            //    stateComponent.SetStateToDo(null);
            //}
            currentRegion.RemoveCharacterFromLocation(this);
            //marker.ClearAvoidInRange(false);
            //marker.ClearHostilesInRange(false);
            //marker.ClearPOIsInVisionRange();

            UnsubscribeSignals();
            traitContainer.RemoveAllNonPersistentTraits(this);
            //ClearAllAwareness();
            CancelAllJobs();
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
        GameObject portraitGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterMarker", Vector3.zero, Quaternion.identity, InnerMapManager.Instance.transform);
        CharacterMarker _marker = portraitGO.GetComponent<CharacterMarker>();
        _marker.SetCharacter(this);
        SetCharacterMarker(_marker);
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
    private void SetCharacterMarker(CharacterMarker marker) {
        this.marker = marker;
    }
    public void AdjustSpeedModifier(float amount) {
        speedModifier += amount;
        if (marker != null) {
            marker.UpdateSpeed();
        }
    }
    public void PerTickDuringMovement() {
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            Traits.Trait trait = traitContainer.allTraits[i];
            if (trait.PerTickOwnerMovement()) {
                break;
            }
        }
    }
    #endregion

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
            needsComponent.ResetFullnessMeter();
            needsComponent.ResetTirednessMeter();
            needsComponent.ResetHappinessMeter();
            ownParty.ReturnToLife();
            marker.OnReturnToLife();
            if (grave != null) {
                marker.PlaceMarkerAt(grave.gridTileLocation);
                grave.gridTileLocation.structure.RemovePOI(grave);
                SetGrave(null);
            }
            traitContainer.RemoveTrait(this, "Dead");
            for (int i = 0; i < traitContainer.allTraits.Count; i++) {
                traitContainer.allTraits[i].OnReturnToLife(this);
            }
            //RemoveAllNonPersistentTraits();
            //ClearAllAwareness();
            //Area gloomhollow = LandmarkManager.Instance.GetAreaByName("Gloomhollow");
            MigrateHomeStructureTo(null);
            needsComponent.SetTirednessForcedTick(0);
            needsComponent.SetFullnessForcedTick(0);
            needsComponent.SetHasCancelledSleepSchedule(false);
            needsComponent.ResetSleepTicks();
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETTING_CHARACTER, this as IPointOfInterest, "");
            //MigrateHomeTo(null);
            //AddInitialAwareness(gloomhollow);
            Messenger.Broadcast(Signals.CHARACTER_RETURNED_TO_LIFE, this);
        }
    }
    public virtual void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] deathLogFillers = null) {
        if (minion != null) {
            minion.Death(cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
            return;
        }
        if (!_isDead) {
            if (currentAlterEgoName != CharacterManager.Original_Alter_Ego) {
                SwitchAlterEgo(CharacterManager.Original_Alter_Ego); //revert the character to his/her original alter ego
            }
            SetIsChatting(false);
            //SetIsFlirting(false);
            Region deathLocation = currentRegion;
            LocationStructure deathStructure = currentStructure;
            LocationGridTile deathTile = gridTileLocation;
            for (int i = 0; i < traitContainer.allTraits.Count; i++) {
                traitContainer.allTraits[i].OnDeath(this);
            }
            //------------------------ Things that are above this line are called before letting the character die so that if we need things done before actually setting the death of character we can do it here like cleaning up necessary things, etc.
            SetIsDead(true);
            UnsubscribeSignals();
            SetPOIState(POI_STATE.INACTIVE);
            //CombatManager.Instance.ReturnCharacterColorToPool(_characterColor);

            if (currentRegion == null) {
                throw new Exception("Current Region Location of " + this.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }
            if (stateComponent.currentState != null) {
                stateComponent.ExitCurrentState();
                //if (stateComponent.currentState != null) {
                //    stateComponent.currentState.OnExitThisState();
                //}
            }
            //else if (stateComponent.stateToDo != null) {
            //    stateComponent.SetStateToDo(null);
            //}
            //if (deathFromAction != null) { //if this character died from an action, do not cancel the action that he/she died from. so that the action will just end as normal.
            //    CancelAllJobsTargettingThisCharacterExcept(deathFromAction, "target is already dead", false);
            //} else {
            //    CancelAllJobsTargettingThisCharacter("target is already dead", false);
            //}
            //StopCurrentActionNode();
            //ForceCancelAllJobsTargettingCharacter(false, "target is already dead");
            ////Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, this, "target is already dead");
            //if (jobQueue.jobsInQueue.Count > 0) {
            //    jobQueue.CancelAllJobs();
            //}
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETTING_CHARACTER, this as IPointOfInterest, "target is already dead");
            CancelAllJobs();

            if (currentRegion.area != null && isHoldingItem) {
                DropAllTokens(currentRegion.area, currentStructure, deathTile, true);
            } else {
                for (int i = 0; i < items.Count; i++) {
                    if (RemoveToken(i)) {
                        i--;
                    }
                }
            }
            //if (currentRegion == null) {
            //    if (currentArea != null && isHoldingItem) {
            //        DropAllTokens(currentArea, currentStructure, deathTile, true);
            //    } else {
            //        for (int i = 0; i < items.Count; i++) {
            //            if (RemoveToken(i)) {
            //                i--;
            //            }
            //        }
            //    }
            //} else {
            //    List<SpecialToken> all = new List<SpecialToken>(items);
            //    for (int i = 0; i < all.Count; i++) {
            //        RemoveToken(all[i]);
            //    }
            //}


            //clear traits that need to be removed
            traitsNeededToBeRemoved.Clear();

            bool wasOutsideSettlement = currentRegion.area == null;

            //bool wasOutsideSettlement = false;
            //if (currentRegion != null) {
            //    wasOutsideSettlement = true;
            //    currentRegion.RemoveCharacterFromLocation(this);
            //}

            if (!IsInOwnParty()) {
                currentParty.RemovePOI(this);
            }
            ownParty.PartyDeath();
            currentRegion?.RemoveCharacterFromLocation(this);
            SetRegionLocation(deathLocation); //set the specific location of this party, to the location it died at
            SetCurrentStructureLocation(deathStructure, false);

            //if (this.race != RACE.SKELETON) {
            //    deathLocation.AddCorpse(this, deathStructure, deathTile);
            //}


            //if (faction != null) {
            //    faction.LeaveFaction(this); //remove this character from it's factions list of characters
            //}
            if (faction != null && faction.leader == this) {
                faction.SetNewLeader();
            }

            if (_role != null) {
                _role.OnDeath(this);
            }

            if (homeRegion != null) {
                Region home = homeRegion;
                Dwelling homeStructure = this.homeStructure;
                homeRegion.RemoveResident(this);
                SetHome(home); //keep this data with character to prevent errors
                SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            }
            //if (homeArea != null) {
            //    Area home = homeArea;
            //    Dwelling homeStructure = this.homeStructure;
            //    homeArea.RemoveResident(this);
            //    SetHome(home); //keep this data with character to prevent errors
            //    SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            //}

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

            //RemoveAllNonPersistentTraits();

            SetHP(0);

            marker.OnDeath(deathTile, wasOutsideSettlement);

            //SetNumWaitingForGoapThread(0); //for raise dead
            //Dead dead = new Dead();
            //dead.SetCharacterResponsibleForTrait(responsibleCharacter);
            traitContainer.AddTrait(this, "Dead", responsibleCharacter, gainedFromDoing: deathFromAction);

            PrintLogIfActive(GameManager.Instance.TodayLogString() + this.name + " died of " + cause);
            Log deathLog;
            if (_deathLog == null) {
                deathLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "death_" + cause);
                deathLog.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                if (responsibleCharacter != null) {
                    deathLog.AddToFillers(responsibleCharacter, responsibleCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                }
                if (deathLogFillers != null) {
                    for (int i = 0; i < deathLogFillers.Length; i++) {
                        deathLog.AddToFillers(deathLogFillers[i]);
                    }
                }
                //will only add death log to history if no death log is provided. NOTE: This assumes that if a death log is provided, it has already been added to this characters history.
                //AddHistory(deathLog);
                deathLog.AddLogToInvolvedObjects();
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
        //if (updateCombatantState) {
        //    UpdateCanCombatantState();
        //}
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
        traitContainer.RemoveTrait(this, traitContainer.GetNormalTrait<Trait>(_characterClass.traitNames)); //Remove traits from class
        _characterClass = null;
    }
    public void AssignClass(string className) {
        if (CharacterManager.Instance.HasCharacterClass(className)) {
            AssignClass(CharacterManager.Instance.CreateNewCharacterClass(className));
        } else {
            throw new Exception("There is no class named " + className + " but it is being assigned to " + this.name);
        }
    }
    protected void OnUpdateCharacterClass() {
        if (_currentHP > maxHP) {
            _currentHP = maxHP;
        }
        //if (_sp > _maxSP) {
        //    _sp = _maxSP;
        //}
        for (int i = 0; i < _characterClass.traitNames.Length; i++) {
            traitContainer.AddTrait(this, _characterClass.traitNames[i]);
        }
        visuals.UpdateAllVisuals(this);
        if (marker != null) {
            marker.UpdateMarkerVisuals();
        }
        if (minion != null) {
            minion.SetAssignedDeadlySinName(_characterClass.className);
        }
        UpdateCanCombatState();
    }
    public void AssignClass(CharacterClass characterClass) {
        CharacterClass previousClass = _characterClass;
        if (previousClass != null) {
            //This means that the character currently has a class and it will be replaced with a new class
            for (int i = 0; i < previousClass.traitNames.Length; i++) {
                traitContainer.RemoveTrait(this, previousClass.traitNames[i]); //Remove traits from class
            }
        }
        _characterClass = characterClass;
        OnUpdateCharacterClass();
        Messenger.Broadcast(Signals.CHARACTER_CLASS_CHANGE, this, previousClass, _characterClass);
    }
    #endregion

    #region Jobs
    public void SetCurrentJob(JobQueueItem job) {
        currentJob = job;
    }
    //public JobQueueItem GetCurrentJob() {
    //    if(currentActionNode != null && jobQueue.jobsInQueue.Count > 0) {
    //        return jobQueue.jobsInQueue[0];
    //    }
    //    return null;
    //}
    public void AddJobTargettingThis(JobQueueItem job) {
        allJobsTargettingThis.Add(job);
    }
    public bool RemoveJobTargettingThis(JobQueueItem job) {
        return allJobsTargettingThis.Remove(job);
    }
    public void ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.jobType == jobType) {
                if (job.ForceCancelJob()) {
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
    //public void CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE jobType, Character otherCharacter, bool forceRemove = true) {
    //    for (int i = 0; i < allJobsTargettingThis.Count; i++) {
    //        JobQueueItem job = allJobsTargettingThis[i];
    //        if (job.jobType == jobType && job.assignedCharacter != otherCharacter) {
    //            if (job.currentOwner.CancelJob(job, forceRemove: forceRemove)) {
    //                i--;
    //            }
    //        }
    //    }
    //}
    public void ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE jobType, string conditionKey, Character otherCharacter) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.assignedCharacter != otherCharacter && job.HasGoalConditionKey(conditionKey)) {
                    if (job.ForceCancelJob()) {
                        i--;
                    }
                }

            }
        }

    }

    //public void CancelAllJobsTargettingThisCharacter(JOB_TYPE jobType, JobQueueItem except, bool forceRemove = true) {
    //    for (int i = 0; i < allJobsTargettingThis.Count; i++) {
    //        JobQueueItem job = allJobsTargettingThis[i];
    //        if (job.jobType == jobType && job != except) {
    //            if (job.currentOwner.CancelJob(job, forceRemove: forceRemove)) {
    //                i--;
    //            }
    //        }
    //    }
    //}
    //public void CancelAllJobsTargettingThisCharacter(JOB_TYPE jobType, object conditionKey, bool forceRemove = true) {
    //    for (int i = 0; i < allJobsTargettingThis.Count; i++) {
    //        if (allJobsTargettingThis[i] is GoapPlanJob) {
    //            GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
    //            if (job.jobType == jobType && job.goals.conditionKey == conditionKey) {
    //                if (job.jobQueueParent.CancelJob(job, forceRemove: forceRemove)) {
    //                    i--;
    //                }
    //            }
    //        }
    //    }
    //}
    public void ForceCancelAllJobsTargettingCharacter(IPointOfInterest target, string reason) {
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem job = jobQueue.jobsInQueue[i];
            if (job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    if (goapJob.ForceCancelJob(false, reason)) {
                        i--;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Cancel all jobs that are targetting this character, except job that has the given action.
    /// </summary>
    /// <param name="except">The exception.</param>
    /// <param name="cause">The cause for cancelling</param>
    /// <param name="shouldDoAfterEffect">Should the effect of the cancelled action be executed.</param>
    //public void CancelAllJobsTargettingThisCharacterExcept(GoapAction except, string cause = "", bool shouldDoAfterEffect = true, bool forceRemove = true) {
    //    for (int i = 0; i < allJobsTargettingThis.Count; i++) {
    //        JobQueueItem job = allJobsTargettingThis[i];
    //        if (except.parentPlan != null && except.parentPlan.job == job) {
    //            continue; //skip
    //        }
    //        if (job.jobQueueParent.CancelJob(job, cause, shouldDoAfterEffect, forceRemove)) {
    //            i--;
    //        }
    //    }
    //}
    public bool HasJobTargettingThis(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            for (int j = 0; j < jobTypes.Length; j++) {
                if (job.jobType == jobTypes[j]) {
                    return true;
                }
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
    public bool HasJobTargettingThisCharacter(JOB_TYPE jobType, string conditionKey) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.HasGoalConditionKey(conditionKey)) {
                    return true;
                }
            }
        }
        return false;
    }
    public GoapPlanJob GetJobTargettingThisCharacter(JOB_TYPE jobType, string conditionKey) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.HasGoalConditionKey(conditionKey)) {
                    return job;
                }
            }
        }
        return null;
    }
    public GoapPlanJob GetJobTargettingThisCharacter(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.jobType == jobType) {
                    return job;
                }
            }
        }
        return null;
    }
    public List<GoapPlanJob> GetJobsTargettingThisCharacter(JOB_TYPE jobType, string conditionKey) {
        List<GoapPlanJob> jobs = new List<GoapPlanJob>();
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.HasGoalConditionKey(conditionKey)) {
                    jobs.Add(job);
                }
            }
        }
        return jobs;
    }
    private void CheckApprehendRelatedJobsOnLeaveLocation() {
        ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.APPREHEND);
        CancelAllJobs(JOB_TYPE.APPREHEND);
        //All apprehend jobs that are being done by this character must be unassigned
        //for (int i = 0; i < allGoapPlans.Count; i++) {
        //    GoapPlan plan = allGoapPlans[i];
        //    if (plan.job != null && plan.job.jobType == JOB_TYPE.APPREHEND) {
        //        plan.job.UnassignJob();
        //        i--;
        //    }
        //}
    }
    public void CancelOrUnassignRemoveTraitRelatedJobs() {
        ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT);
        CancelAllJobs(JOB_TYPE.REMOVE_TRAIT);
        //TODO:
        //All remove trait jobs that are being done by this character must be unassigned
        //for (int i = 0; i < allGoapPlans.Count; i++) {
        //    GoapPlan plan = allGoapPlans[i];
        //    if (plan.job != null && plan.job.jobType == JOB_TYPE.REMOVE_TRAIT) {
        //        plan.job.UnassignJob();
        //        i--;
        //    }
        //}
    }
    private bool CreateJobsOnEnterVisionWithCharacter(Character targetCharacter) {
        string log = name + " saw " + targetCharacter.name + ", will try to create jobs on enter vision...";
        if (!CanCharacterReact(targetCharacter)) {
            log += "\nCharacter cannot react!";
            PrintLogIfActive(log);
            return true;
        }
        bool hasCreatedJob = false;
        log += "\nChecking source character traits...";
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            log += "\n- " + traitContainer.allTraits[i].name;
            if (traitContainer.allTraits[i].CreateJobsOnEnterVisionBasedOnOwnerTrait(targetCharacter, this)) {
                log += ": created a job!";
                hasCreatedJob = true;
            } else {
                log += ": did not create a job!";
            }
        }

        log += "\nChecking target character traits...";
        for (int i = 0; i < targetCharacter.traitContainer.allTraits.Count; i++) {
            log += "\n- " + targetCharacter.traitContainer.allTraits[i].name;
            if (targetCharacter.traitContainer.allTraits[i].CreateJobsOnEnterVisionBasedOnTrait(targetCharacter, this)) {
                hasCreatedJob = true;
                log += ": created a job!";
            } else {
                log += ": did not create a job!";
            }
        }

        POIRelationshipData relData = relationshipContainer.GetRelationshipDataWith(targetCharacter) as POIRelationshipData;
        if (relData != null) {
            relData.OnSeeCharacter(targetCharacter, this);
        }

        //log += "\nChecking relationship traits...";
        //for (int i = 0; i < relationshipTraits.Count; i++) {
        //    if (relationshipTraits[i].targetCharacter == targetCharacter) {
        //        log += "\n- " + relationshipTraits[i].name;
        //        if (relationshipTraits[i].CreateJobsOnEnterVisionBasedOnTrait(this, this)) {
        //            hasCreatedJob = true;
        //            log += ": created a job!";
        //        } else {
        //            log += ": did not create a job!";
        //        }
        //    }
        //}
        PrintLogIfActive(log);
        return hasCreatedJob;
    }
    public bool CreateJobsOnEnterVisionWith(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            return CreateJobsOnEnterVisionWithCharacter(targetPOI as Character);
        }
        string log = name + " saw " + targetPOI.name + ", will try to create jobs on enter vision...";
        if (!CanCharacterReact(targetPOI)) {
            log += "\nCharacter cannot react!";
            PrintLogIfActive(log);
            return true;
        }
        bool hasCreatedJob = false;
        log += "\nChecking source character traits...";
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            log += "\n- " + traitContainer.allTraits[i].name;
            if (traitContainer.allTraits[i].CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, this)) {
                log += ": created a job!";
                hasCreatedJob = true;
            } else {
                log += ": did not create a job!";
            }
        }
        log += "\nChecking target poi traits...";
        for (int i = 0; i < targetPOI.traitContainer.allTraits.Count; i++) {
            log += "\n- " + targetPOI.traitContainer.allTraits[i].name;
            if (targetPOI.traitContainer.allTraits[i].CreateJobsOnEnterVisionBasedOnTrait(targetPOI, this)) {
                log += ": created a job!";
                hasCreatedJob = true;
            } else {
                log += ": did not create a job!";
            }
        }
        PrintLogIfActive(log);
        return hasCreatedJob;
    }
    public bool CreateJobsOnTargetGainTrait(IPointOfInterest targetPOI, Trait traitGained) {
        string log = targetPOI.name + " gained trait " + traitGained.name + ", will try to create jobs based on it...";
        if (!CanCharacterReact(targetPOI)) {
            log += "\nCharacter cannot react!";
            PrintLogIfActive(log);
            return true;
        }
        bool hasCreatedJob = false;
        log += "\nChecking trait...";
        if (traitGained.CreateJobsOnEnterVisionBasedOnTrait(targetPOI, this)) {
            log += ": created a job!";
            hasCreatedJob = true;
        } else {
            log += ": did not create a job!";
        }
        PrintLogIfActive(log);
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
        if (jobQueue.HasJob(JOB_TYPE.UNDERMINE_ENEMY, targetCharacter)) {
            return false;
        }
        if (traitContainer.GetNormalTrait<Trait>("Diplomatic") != null) {
            return false;
        }
        if (status == SHARE_INTEL_STATUS.WITNESSED) {
            //When creating undermine job and the creator of the job witnessed the event that caused him/her to undermine, mutate undermine job to knockout job
            //This means that all undermine jobs that are caused by witnessing an event will become knockout jobs
            return CreateKnockoutJob(targetCharacter);
        }
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.UNDERMINE_ENEMY, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, "Negative", false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, this);
        Debug.LogWarning(GameManager.Instance.TodayLogString() + "Added an UNDERMINE ENEMY Job: negative trait to " + this.name + " with target " + targetCharacter.name);
        jobQueue.AddJobInQueue(job);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", reason + "_and_undermine");
        log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddToFillers(null, currentMoodType.ToString().ToLower(), LOG_IDENTIFIER.STRING_1);
        AddHistory(log);

        PlayerManager.Instance.player.ShowNotificationFrom(log, this, false);
        return true;

        //WeightedDictionary<string> undermineWeights = new WeightedDictionary<string>();
        //undermineWeights.AddElement("negative trait", 50);

        //bool hasFriend = false;
        //List<Log> crimeMemories = null;
        //for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
        //    Character currCharacter = CharacterManager.Instance.allCharacters[i];
        //    if (currCharacter != targetCharacter && currCharacter != this) {
        //        if (currCharacter.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
        //            hasFriend = true;
        //            break;
        //        }
        //    }
        //}
        //if (hasFriend) {
        //    int dayTo = GameManager.days;
        //    int dayFrom = dayTo - 3;
        //    if (dayFrom < 1) {
        //        dayFrom = 1;
        //    }
        //    crimeMemories = GetCrimeMemories(dayFrom, dayTo, targetCharacter);
        //    if (crimeMemories.Count > 0) {
        //        undermineWeights.AddElement("destroy friendship", 20);
        //    }
        //}

        //bool hasLoverOrParamour = false;
        //List<Log> affairMemoriesInvolvingRumoredCharacter = null;
        //for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
        //    Character currCharacter = CharacterManager.Instance.allCharacters[i];
        //    if (currCharacter != targetCharacter && currCharacter != this) {
        //        if (currCharacter.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) 
        //            || currCharacter.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
        //            hasLoverOrParamour = true;
        //            break;
        //        }
        //    }
        //}
        //if (hasLoverOrParamour) {
        //    List<Character> loversOrParamours = targetCharacter.relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR).Select(x => (x as AlterEgoData).owner).ToList(); //TODO: Revise this
        //    Character chosenLoverOrParamour = loversOrParamours[UnityEngine.Random.Range(0, loversOrParamours.Count)];
        //    if(chosenLoverOrParamour != null) {
        //        int dayTo = GameManager.days;
        //        int dayFrom = dayTo - 3;
        //        if (dayFrom < 1) {
        //            dayFrom = 1;
        //        }
        //        List<Log> memories = GetWitnessOrInformedMemories(dayFrom, dayTo, targetCharacter);
        //        affairMemoriesInvolvingRumoredCharacter = new List<Log>();
        //        for (int i = 0; i < memories.Count; i++) {
        //            Log memory = memories[i];
        //            //if the event means Character 2 flirted, asked to make love or made love with another character other than Target, include it
        //            if (memory.goapAction.actor != chosenLoverOrParamour && !memory.goapAction.IsTarget(chosenLoverOrParamour)) {
        //                if (memory.goapAction.goapType == INTERACTION_TYPE.CHAT_CHARACTER) {
        //                    ChatCharacter chatAction = memory.goapAction as ChatCharacter;
        //                    if (chatAction.chatResult == "flirt") {
        //                        affairMemoriesInvolvingRumoredCharacter.Add(memory);

        //                    }
        //                } else if (memory.goapAction.goapType == INTERACTION_TYPE.MAKE_LOVE) {
        //                    affairMemoriesInvolvingRumoredCharacter.Add(memory);
        //                } else if (memory.goapAction.goapType == INTERACTION_TYPE.INVITE) {
        //                    if(memory.goapAction.actor == targetCharacter) {
        //                        affairMemoriesInvolvingRumoredCharacter.Add(memory);
        //                    }else if (memory.goapAction.IsTarget(targetCharacter) && memory.goapAction.currentState.name == "Invite Success") {
        //                        affairMemoriesInvolvingRumoredCharacter.Add(memory);
        //                    }
        //                }
        //            }
        //        }
        //        if(affairMemoriesInvolvingRumoredCharacter.Count > 0) {
        //            undermineWeights.AddElement("destroy love", 20);
        //        }
        //    }
        //}

        //if (undermineWeights.Count > 0) {
        //    string result = undermineWeights.PickRandomElementGivenWeights();
        //    GoapPlanJob job = null;
        //    if (result == "negative trait") {
        //        job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.UNDERMINE_ENEMY, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = targetCharacter });
        //    } else if (result == "destroy friendship") {
        //        job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.UNDERMINE_ENEMY, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Friend", targetPOI = targetCharacter },
        //            new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.NONE, new object[] { targetCharacter, crimeMemories } }, });
        //    } else if (result == "destroy love") {
        //        job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.UNDERMINE_ENEMY, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Lover", targetPOI = targetCharacter },
        //            new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.NONE, new object[] { targetCharacter, affairMemoriesInvolvingRumoredCharacter } }, });
        //    }

        //    //job.SetCannotOverrideJob(true);
        //    Debug.LogWarning(GameManager.Instance.TodayLogString() + "Added an UNDERMINE ENEMY Job: " + result + " to " + this.name + " with target " + targetCharacter.name);
        //    //job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
        //    jobQueue.AddJobInQueue(job, false);
        //    //if (processJobQueue) {
        //    //    jobQueue.ProcessFirstJobInQueue(this);
        //    //}

        //    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", reason + "_and_undermine");
        //    log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //    log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //    log.AddToFillers(null, currentMoodType.ToString().ToLower(), LOG_IDENTIFIER.STRING_1);
        //    AddHistory(log);

        //    PlayerManager.Instance.player.ShowNotificationFrom(log, this, false);
        //    return true;
        //}
        //return false;
    }
    public void CreateLocationKnockoutJobs(Character targetCharacter, int amount) {
        if (isAtHomeRegion && homeRegion.area != null && isPartOfHomeFaction && !targetCharacter.isDead && !targetCharacter.isAtHomeRegion && !traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
            for (int i = 0; i < amount; i++) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KNOCKOUT, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, homeRegion.area);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeKnockoutJob);
                homeRegion.area.AddToAvailableJobs(job);
            }
            //return job;
        }
        //return null;
    }
    public bool CreateKnockoutJob(Character targetCharacter) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KNOCKOUT, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, this);
        jobQueue.AddJobInQueue(job);
        PrintLogIfActive(GameManager.Instance.TodayLogString() + "Added a KNOCKOUT Job to " + this.name + " with target " + targetCharacter.name);
        return true;
    }
    /// <summary>
    /// Make this character create an apprehend job at his home location targetting a specific character.
    /// </summary>
    /// <param name="targetCharacter">The character to be apprehended.</param>
    /// <returns>The created job.</returns>
    public GoapPlanJob CreateApprehendJobFor(Character targetCharacter, bool assignSelfToJob = false) {
        //if (homeArea.id == specificLocation.id) {
        if (isAtHomeRegion && homeRegion.area != null && !targetCharacter.HasJobTargettingThis(JOB_TYPE.APPREHEND) && targetCharacter.traitContainer.GetNormalTrait<Trait>("Restrained") == null && !traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP, targetCharacter, homeRegion.area);
            job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { homeRegion.area.prison });
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeApprehendJob);
            homeRegion.area.AddToAvailableJobs(job);
            if (assignSelfToJob) {
                jobQueue.AddJobInQueue(job);
            }
            return job;
        }
        return null;
        //}
    }
    public GoapPlanJob CreateObtainItemJob(SPECIAL_TOKEN item) {
        GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.HAS_ITEM, item.ToString(), false, GOAP_EFFECT_TARGET.ACTOR);
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OBTAIN_ITEM, goapEffect, this, this);
        jobQueue.AddJobInQueue(job);
        //Debug.Log(this.name + " created job to obtain item " + item.ToString());
        //Messenger.Broadcast<string, int, UnityEngine.Events.UnityAction>(Signals.SHOW_DEVELOPER_NOTIFICATION, this.name + " created job to obtain item " + item.ToString(), 5, null);
        return job;
    }
    public GoapPlanJob CreateAttemptToStopCurrentActionAndJob(Character targetCharacter, GoapPlanJob jobToStop) {
        if (!targetCharacter.HasJobTargettingThis(JOB_TYPE.ATTEMPT_TO_STOP_JOB)) {
            GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.TARGET_STOP_ACTION_AND_JOB, string.Empty, false, GOAP_EFFECT_TARGET.TARGET);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ATTEMPT_TO_STOP_JOB, goapEffect, targetCharacter, this);
            job.AddOtherData(INTERACTION_TYPE.ASK_TO_STOP_JOB, new object[] { jobToStop });
            jobQueue.AddJobInQueue(job);
            return job;
        }
        return null;
    }
    public void CreatePersonalJobs() {
        bool hasCreatedJob = false;

        //build furniture job
        if (!hasCreatedJob && isAtHomeRegion && homeRegion.area != null && currentStructure is Dwelling) {
            Dwelling dwelling = currentStructure as Dwelling;
            if (dwelling.HasUnoccupiedFurnitureSpot() && advertisedActions.Contains(INTERACTION_TYPE.CRAFT_FURNITURE)) {
                if (UnityEngine.Random.Range(0, 100) < 10) { //if the dwelling has a facility deficit(facility at 0) or if chance is met.
                    FACILITY_TYPE mostNeededFacility = dwelling.GetMostNeededValidFacility();
                    if (mostNeededFacility != FACILITY_TYPE.NONE) {
                        List<LocationGridTile> validSpots = dwelling.GetUnoccupiedFurnitureSpotsThatCanProvide(mostNeededFacility);
                        LocationGridTile chosenTile = validSpots[UnityEngine.Random.Range(0, validSpots.Count)];
                        FURNITURE_TYPE furnitureToCreate = chosenTile.GetFurnitureThatCanProvide(mostNeededFacility);
                        TILE_OBJECT_TYPE tileObj = furnitureToCreate.ConvertFurnitureToTileObject();

                        //create new unbuilt furniture on spot, and target that in the job
                        TileObject furniture = InnerMapManager.Instance.CreateNewTileObject<TileObject>(tileObj);
                        dwelling.AddPOI(furniture, chosenTile);
                        furniture.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
                        Debug.Log($"Created new unbuilt {furniture.name} at {chosenTile}");

                        if (tileObj.CanBeCraftedBy(this)) { //check first if the character can build that specific type of furniture
                            if (jobQueue.HasJob(JOB_TYPE.BUILD_FURNITURE, furniture) == false) {
                                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUILD_FURNITURE, INTERACTION_TYPE.CRAFT_TILE_OBJECT, furniture, this);
                                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoCraftFurnitureJob);
                                jobQueue.AddJobInQueue(job);
                                Debug.Log($"{GameManager.Instance.TodayLogString()}{job.ToString()} was added to {this.name}'s jobqueue");
                            }
                        } else {
                            //furniture cannot be crafted by this character, post a job on the area
                            if (homeRegion.area.HasJob(JOB_TYPE.BUILD_FURNITURE, furniture) == false) {
                                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUILD_FURNITURE, INTERACTION_TYPE.CRAFT_TILE_OBJECT, furniture, homeRegion.area);
                                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoCraftFurnitureJob);
                                homeRegion.area.AddToAvailableJobs(job);
                            }
                        }
                    }
                }
            }
        }

        //Obtain Item job
        //if the character is part of a Faction and he doesnt have an Obtain Item Job in his personal job queue, 
        //there is a 10% chance that the character will create a Obtain Item Job if he has less than four items owned 
        //(sum from items in his inventory and in his home whose owner is this character). 
        //Reduce this chance by 3% for every item he owns (disregard stolen items)
        //NOTE: If he already has all items he needs, he doesnt need to do this job anymore.
        if (!isFactionless && !jobQueue.HasJob(JOB_TYPE.OBTAIN_ITEM) && !role.HasNeededItems(this) && isAtHomeRegion) {
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
        List<Character> enemyCharacters = relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TRAIT.ENEMY).Where(x => x is AlterEgoData).Select(x => (x as AlterEgoData).owner).ToList();
        if (!hasCreatedJob && enemyCharacters.Count > 0) {
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
                    //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob("Undermine Enemy", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = chosenCharacter });
                    //job.SetCancelOnFail(true);
                    //job.SetCannotOverrideJob(true);
                    ////GameManager.Instance.SetPausedState(true);
                    //Debug.LogWarning(GameManager.Instance.TodayLogString() + "Added an UNDERMINE ENEMY Job to " + this.name + " with target " + chosenCharacter.name);
                    //jobQueue.AddJobInQueue(job);
                    //hasCreatedJob = true;
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
            List<Character> positiveCharacters = relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_EFFECT.POSITIVE)
                .Where(x => x is AlterEgoData).Select(x => (x as AlterEgoData).owner).ToList(); //TODO: Improve this
            positiveCharacters.Remove(troubledCharacter);
            if (positiveCharacters.Count > 0) {
                targetCharacter = positiveCharacters[UnityEngine.Random.Range(0, positiveCharacters.Count)];
            } else {
                List<Character> nonEnemyCharacters = relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_EFFECT.NEUTRAL)
                    .Where(x => x is AlterEgoData && (x as AlterEgoData).owner.faction.id == faction.id).Select(x => (x as AlterEgoData).owner).ToList(); //TODO: Improve this
                nonEnemyCharacters.Remove(troubledCharacter);
                if (nonEnemyCharacters.Count > 0) {
                    targetCharacter = nonEnemyCharacters[UnityEngine.Random.Range(0, nonEnemyCharacters.Count)];
                }
            }
            if (targetCharacter != null) {
                JOB_TYPE jobType = (JOB_TYPE) Enum.Parse(typeof(JOB_TYPE), "ASK_FOR_HELP_" + helpType.ToString());
                INTERACTION_TYPE interactionType = (INTERACTION_TYPE) Enum.Parse(typeof(INTERACTION_TYPE), "ASK_FOR_HELP_" + helpType.ToString());
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, interactionType, targetCharacter, this);
                job.AddOtherData(interactionType, otherData);
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
            List<Character> positiveCharacters = relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_EFFECT.POSITIVE)
                .Where(x => x is AlterEgoData).Select(x => (x as AlterEgoData).owner).ToList(); //TODO: Improve this
            positiveCharacters.Remove(troubledCharacter);
            if (positiveCharacters.Count > 0) {
                targetCharacter = positiveCharacters[UnityEngine.Random.Range(0, positiveCharacters.Count)];
            } else {
                List<Character> nonEnemyCharacters = relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_EFFECT.NEUTRAL)
                    .Where(x => x is AlterEgoData && (x as AlterEgoData).owner.faction.id == faction.id).Select(x => (x as AlterEgoData).owner).ToList(); //TODO: Improve this
                nonEnemyCharacters.Remove(troubledCharacter);
                if (nonEnemyCharacters.Count > 0) {
                    targetCharacter = nonEnemyCharacters[UnityEngine.Random.Range(0, nonEnemyCharacters.Count)];
                }
            }
            if (targetCharacter != null) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ASK_FOR_HELP_SAVE_CHARACTER, INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER, targetCharacter, this);
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
    //public void CreateSaveCharacterJob(Character targetCharacter) {
    //    if (targetCharacter != null && targetCharacter != this) {
    //        string log = name + " is creating save character job for " + targetCharacter.name;
    //        if (role.roleType == CHARACTER_ROLE.CIVILIAN || role.roleType == CHARACTER_ROLE.NOBLE
    //            || role.roleType == CHARACTER_ROLE.LEADER) {
    //            CreateAskForHelpSaveCharacterJob(targetCharacter);
    //            log += "\n" + name + " is either a Civilian/Leader/Noble and cannot save a character, thus, will try to ask for help.";
    //            return;
    //        }
    //        if (!targetCharacter.HasJobTargettingThis(JOB_TYPE.SAVE_CHARACTER)) {
    //            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP, targetCharacter, new Dictionary<INTERACTION_TYPE, object[]>() {
    //                { INTERACTION_TYPE.DROP, new object[] { specificLocation.prison } }
    //            }, this);
    //            jobQueue.AddJobInQueue(job);
    //            log += "\n" + name + " created save character job.";
    //            //return job;
    //        } else {
    //            log += "\n" + targetCharacter.name + " is already being saved by someone.";
    //        }
    //        PrintLogIfActive(log);
    //    } else {
    //        if (targetCharacter == null) {
    //            Debug.LogError(name + " cannot create save character job because troubled character is null!");
    //        } else {
    //            Debug.LogError(name + " cannot create save character job for " + targetCharacter.name);
    //        }
    //    }
    //    //return null;
    //}

    //public GoapPlanJob CreateBreakupJob(Character targetCharacter) {
    //    if (jobQueue.HasJob(JOB_TYPE.BREAK_UP, targetCharacter)) {
    //        return null; //already has break up job targetting targetCharacter
    //    }
    //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BREAK_UP, INTERACTION_TYPE.BREAK_UP, targetCharacter, this);
    //    jobQueue.AddJobInQueue(job);
    //    return job;
    //}
    public void CreateShareInformationJob(Character targetCharacter, GoapAction info) {
        if (!IsHostileWith(targetCharacter) && !jobQueue.HasJobWithOtherData(JOB_TYPE.SHARE_INFORMATION, info)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SHARE_INFORMATION, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, this);
            job.AddOtherData(INTERACTION_TYPE.SHARE_INFORMATION, new object[] { info });
            jobQueue.AddJobInQueue(job);
        }
    }
    //public void CancelAllJobsAndPlansExceptNeedsRecovery(string reason = "") {
    //    AdjustIsWaitingForInteraction(1);
    //    for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
    //        if (jobQueue.jobsInQueue[i].jobType.IsNeedsTypeJob()) {
    //            continue;
    //        }
    //        if (jobQueue.CancelJob(jobQueue.jobsInQueue[i])) {
    //            i--;
    //        }
    //    }
    //    if (homeArea != null) {
    //        homeArea.jobQueue.UnassignAllJobsTakenBy(this);
    //    }
    //}

    //    StopCurrentAction(false, reason: reason);
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        if(allGoapPlans[i].job != null && allGoapPlans[i].job.jobType.IsNeedsTypeJob()) {
    //            if (JustDropPlan(allGoapPlans[i])) {
    //                i--;
    //            }
    //        } else {
    //            if (DropPlan(allGoapPlans[i])) {
    //                i--;
    //            }
    //        }
    //    }
    //    AdjustIsWaitingForInteraction(-1);
    //}
    public void CancelAllJobs(string reason = "") {
        //AdjustIsWaitingForInteraction(1);
        //StopCurrentActionNode(reason: reason);
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            if (jobQueue.jobsInQueue[i].CancelJob(reason: reason)) {
                i--;
            }
        }
        //if (homeArea != null) {
        //    homeArea.jobQueue.UnassignAllJobsTakenBy(this);
        //}

        //StopCurrentAction(false, reason: reason);
        //for (int i = 0; i < allGoapPlans.Count; i++) {
        //    if (DropPlan(allGoapPlans[i])) {
        //        i--;
        //    }
        //}
        //AdjustIsWaitingForInteraction(-1);
    }
    public void CancelAllJobs(JOB_TYPE jobType) {
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem job = jobQueue.jobsInQueue[i];
            if (job.jobType == jobType) {
                if (job.CancelJob()) {
                    i--;
                }
            }
        }
    }
    public void CancelAllJobsExceptForCurrent(bool shouldDoAfterEffect = true) {
        if (currentJob != null) {
            for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem job = jobQueue.jobsInQueue[i];
                if (job != currentJob) {
                    if (job.CancelJob(shouldDoAfterEffect)) {
                        i--;
                    }
                }
            }
        }
    }
    //public void CancelAllPlans(string reason = "") {
    //    StopCurrentAction(false, reason: reason);
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        if (DropPlan(allGoapPlans[i])) {
    //            i--;
    //        }
    //    }
    //}
    //public void CancelAllJobsAndPlansExcept(string reason = "", params JOB_TYPE[] job) {
    //    //List<JOB_TYPE> exceptions = job.ToList();
    //    AdjustIsWaitingForInteraction(1);
    //    currentActionNode.StopActionNode(reason: reason);
    //    for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
    //        JobQueueItem item = jobQueue.jobsInQueue[i];
    //        if (!job.Contains(item.jobType)) {
    //            if (item.CancelJob()) {
    //                i--;
    //            }
    //        }
    //    }
    //    //homeArea.jobQueue.UnassignAllJobsTakenBy(this);

    //    //StopCurrentAction(false, reason: reason);
    //    //for (int i = 0; i < allGoapPlans.Count; i++) {
    //    //    GoapPlan currPlan = allGoapPlans[i];
    //    //    if (currPlan.job == null || !exceptions.Contains(currPlan.job.jobType)) {
    //    //        if (DropPlan(allGoapPlans[i])) {
    //    //            i--;
    //    //        }
    //    //    }
    //    //}
    //    AdjustIsWaitingForInteraction(-1);
    //}
    public bool CanCurrentJobBeOverriddenByJob(JobQueueItem job) {
        return false;
        ////GENERAL RULE: Plans/States that have no jobs are always the lowest priority
        ////Current job cannot be overriden by null job
        //if (job == null) {
        //    return false;
        //}
        //if (GetNormalTrait<Trait>("Berserked") != null /*||(stateComponent.stateToDo != null && stateComponent.stateToDo.characterState == CHARACTER_STATE.BERSERKED)*/) {
        //    //Berserked state cannot be overriden
        //    return false;
        //}
        //if (stateComponent.currentState == null && this.marker != null && this.marker.hasFleePath) {
        //    return false; //if the character is only fleeing, but is not in combat state, do not allow overriding.
        //}
        //if (stateComponent.currentState != null) {
        //    if (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
        //        //Only override flee or engage state if the job is Berserked State, Berserk overrides all
        //        if (job is CharacterStateJob) {
        //            CharacterStateJob stateJob = job as CharacterStateJob;
        //            if (stateJob.targetState == CHARACTER_STATE.BERSERKED) {
        //                return true;
        //            }
        //        }
        //        return false;
        //    } else {
        //        //If current state is not Flee or Engage, it is certainly one of the major states since the only minor states are Flee and Engage
        //        //If current state has no job, it is automatically overridable, otherwise, if the current state's job has a lower job priority (higher number) than the parameter job, it is overridable
        //        if (stateComponent.currentState.job != null && !stateComponent.currentState.job.cannotOverrideJob && job.priority < stateComponent.currentState.job.priority) {
        //            return true;
        //        } else if (stateComponent.currentState.job == null) {
        //            return true;
        //        }
        //        return false;
        //    }
        //} 
        ////else if (stateComponent.stateToDo != null) {
        ////    if (stateComponent.stateToDo.characterState == CHARACTER_STATE.COMBAT) {
        ////        //Only override flee or engage state if the job is Berserked State, Berserk overrides all
        ////        if (job is CharacterStateJob) {
        ////            CharacterStateJob stateJob = job as CharacterStateJob;
        ////            if (stateJob.targetState == CHARACTER_STATE.BERSERKED) {
        ////                return true;
        ////            }
        ////        }
        ////        return false;
        ////    } else {
        ////        //If current state is not Flee or Engage, it is certainly one of the major states since the only minor states are Flee and Engage
        ////        //If current state has no job, it is automatically overridable, otherwise, if the current state's job has a lower job priority (higher number) than the parameter job, it is overridable
        ////        if (stateComponent.stateToDo.job != null && !stateComponent.stateToDo.job.cannotOverrideJob && job.priority < stateComponent.stateToDo.job.priority) {
        ////            return true;
        ////        } else if (stateComponent.stateToDo.job == null) {
        ////            return true;
        ////        }
        ////        return false;
        ////    }
        ////}
        ////Cannot override when resting
        //if (traitContainer.GetNormalTrait<Trait>("Resting") != null) {
        //    return false;
        //}
        ////If there is no current state then check the current action
        ////Same process applies that if the current action's job has a lower job priority (higher number) than the parameter job, it is overridable
        //if(currentActionNode != null && currentActionNode.goapType == INTERACTION_TYPE.MAKE_LOVE && !currentActionNode.isDone) {
        //    //Cannot override make love
        //    return false;
        //}
        //if (currentActionNode != null && currentActionNode.parentPlan != null) {
        //    if(currentActionNode.parentPlan.job != null && !currentActionNode.parentPlan.job.cannotOverrideJob
        //               && job.priority < currentActionNode.parentPlan.job.priority) {
        //        return true;
        //    } else if (currentActionNode.parentPlan.job == null) {
        //        return true;
        //    }
        //    return false;
        //}

        ////If nothing applies, always overridable
        //return true;
    }
    public GoapPlanJob CreateSuicideJob() {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SUICIDE, new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, this);
        //job.SetCanTakeThisJobChecker(InteractionManager.Instance.IsSuicideJobStillValid);
        jobQueue.AddJobInQueue(job);
        return job;
    }
    /// <summary>
    /// Gets the current priority of the character's current action or state.
    /// If he/she has none, this will return a very high number.
    /// </summary>
    /// <returns></returns>
    public int GetCurrentPriorityValue() {
        if (stateComponent.currentState != null && stateComponent.currentState.job != null) {
            return stateComponent.currentState.job.priority;
        }
        //else if (stateComponent.stateToDo != null && stateComponent.stateToDo.job != null) {
        //    return stateComponent.stateToDo.job.priority;
        //}
        else {
            JobQueueItem job = currentJob;
            if (job != null) {
                return job.priority;
            } else {
                return 999999;
            }
        }

        //else if (currentActionNode != null && currentActionNode.parentPlan != null && currentActionNode.parentPlan.job != null) {
        //    return currentActionNode.parentPlan.job.priority;
        //} else {
        //    return 999999;
        //}
    }
    //public void CreateReplaceTileObjectJob(TileObject removedObj, LocationGridTile removedFrom) {
    //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPLACE_TILE_OBJECT, INTERACTION_TYPE.REPLACE_TILE_OBJECT, new Dictionary<INTERACTION_TYPE, object[]>() {
    //                    { INTERACTION_TYPE.REPLACE_TILE_OBJECT, new object[]{ removedObj, removedFrom } },
    //    });
    //    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeReplaceTileObjectJob);
    //    job.SetCancelOnFail(false);
    //    job.SetCancelJobOnDropPlan(false);
    //    jobQueue.AddJobInQueue(job);
    //}
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
        Trait criminal = traitContainer.GetNormalTrait<Trait>("Criminal");
        if (criminal != null) {
            traitContainer.RemoveTrait(this, criminal); //TODO: RemoveTrait(criminal, false); do not trigger on remove
        }
    }
    #endregion

    #region Party
    /*
        Create a new Party with this character as the leader.
            */
    public virtual Party CreateOwnParty() {
        //if (_ownParty != null) {
        //    _ownParty.RemoveCharacter(this);
        //}
        Party newParty = new Party(this);
        SetOwnedParty(newParty);
        SetCurrentParty(newParty);
        //newParty.AddCharacter(this, true);
        //newParty.CreateCharacterObject();
        return newParty;
    }
    public virtual void SetOwnedParty(Party party) {
        ownParty = party;
    }
    public virtual void SetCurrentParty(Party party) {
        currentParty = party;
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
            //currentRegion.RemoveCharacterFromLocation(this); //Why are we removing the character from location if it is added to a party
            //ownParty.specificLocation.RemoveCharacterFromLocation(this);
            //ownParty.icon.SetVisualState(false);
            marker.collisionTrigger.SetMainColliderState(false);
        }
    }
    public bool IsInParty() {
        return currentParty.isCarryingAnyPOI;
        //if (currentParty.characters.Count > 1) {
        //    return true; //if the character is in a party that has more than 1 characters
        //}
        //return false;
    }
    public bool IsInOwnParty() {
        if (currentParty.id == ownParty.id) {
            return true;
        }
        return false;
    }
    //public bool HasOtherCharacterInParty() {
    //    return ownParty.characters.Count > 1;
    //}
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
            for (int i = 0; i < gridTileLocation.parentMap.allEdgeTiles.Count; i++) {
                LocationGridTile currTile = gridTileLocation.parentMap.allEdgeTiles[i];
                float dist = Vector2.Distance(currTile.localLocation, currentGridTile.localLocation);
                if (nearestDist == -999f || dist < nearestDist) {
                    if (currTile.structure != null) {
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
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETTING_CHARACTER, this as IPointOfInterest, "");
            marker.ClearTerrifyingObjects();
            ExecuteLeaveAreaActions();
            needsComponent.OnCharacterLeftLocation(currentRegion);
        } else {
            if (marker.terrifyingObjects.Count > 0) {
                marker.RemoveTerrifyingObject(party.owner);
                if (party.isCarryingAnyPOI) {
                    marker.RemoveTerrifyingObject(party.carriedPOI);
                }
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
            needsComponent.OnCharacterArrivedAtLocation(currentRegion);
        } else {
            //AddAwareness(party.owner);
            //if (party.isCarryingAnyPOI) {
            //    AddAwareness(party.carriedPOI);
            //}
            //for (int i = 0; i < party.characters.Count; i++) {
            //    Character character = party.characters[i];
            //    AddAwareness(character); //become re aware of character
            //}
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
    public void SetRegionLocation(Region region) {
        _currentRegion = region;
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
            for (int i = 0; i < _raceSetting.traitNames.Length; i++) {
                traitContainer.RemoveTrait(this, _raceSetting.traitNames[i]); //Remove traits from race
            }
        }
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        OnUpdateRace();
        Messenger.Broadcast(Signals.CHARACTER_CHANGED_RACE, this);
        return true;
    }
    public void OnUpdateRace() {
        for (int i = 0; i < _raceSetting.traitNames.Length; i++) {
            traitContainer.AddTrait(this, _raceSetting.traitNames[i]);
        }
        //Update Portrait to use new race
        visuals.UpdateAllVisuals(this);
        if (marker != null) {
            marker.UpdateMarkerVisuals();
        }
        //update goap interactions that should no longer be valid
        if (race == RACE.SKELETON) {
            advertisedActions.Remove(INTERACTION_TYPE.DRINK_BLOOD);
            advertisedActions.Remove(INTERACTION_TYPE.SHARE_INFORMATION);
        }
    }
    public void ChangeClass(string className) {
        //string previousClassName = _characterClass.className;
        AssignClass(className);
        //_characterClass = charClass.CreateNewCopy();

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
                if (InnerMapManager.Instance.IsShowingAreaMap(currentRegion.area)) {
                    InnerMapManager.Instance.HideAreaMap();
                }
                //CameraMove.Instance.CenterCameraOn(currentParty.icon.travelLine.iconImg.gameObject);
                CameraMove.Instance.CenterCameraOn(currentRegion.coreTile.gameObject);
            } else if (currentParty.icon.isTravelling) {
                if (marker.gameObject.activeInHierarchy) {
                    bool instantCenter = !InnerMapManager.Instance.IsShowingAreaMap(currentRegion.area);
                    if (currentRegion.area != null && instantCenter) {
                        InnerMapManager.Instance.ShowAreaMap(currentRegion.area, false);
                    }
                    AreaMapCameraMove.Instance.CenterCameraOn(marker.gameObject, instantCenter);
                }
            } else if (currentRegion.area != null) {
                bool instantCenter = !InnerMapManager.Instance.IsShowingAreaMap(currentRegion.area);
                if (instantCenter) {
                    InnerMapManager.Instance.ShowAreaMap(currentRegion.area, false);
                }
                AreaMapCameraMove.Instance.CenterCameraOn(marker.gameObject, instantCenter);
                //if (currentArea.areaMap != null) {
                //    if (!currentArea.areaMap.isShowing) {
                //        InnerMapManager.Instance.ShowAreaMap(currentArea, false);
                //    }
                //    AreaMapCameraMove.Instance.CenterCameraOn(marker.gameObject, instantCenter);
                //} else {
                //    InnerMapManager.Instance.HideAreaMap();
                //    CameraMove.Instance.CenterCameraOn(currentArea.region.coreTile.gameObject);
                //}
            } else {
                CameraMove.Instance.CenterCameraOn(currentRegion.coreTile.gameObject);
            }
        }
    }
    //private void GetRandomCharacterColor() {
    //    _characterColor = CombatManager.Instance.UseRandomCharacterColor();
    //    _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
    //}
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
    public void AdjustDoNotRecoverHP(int amount) {
        doNotRecoverHP += amount;
        doNotRecoverHP = Math.Max(doNotRecoverHP, 0);
    }
    public void ReturnToOriginalHomeAndFaction(Region ogHome, Faction ogFaction) {
        //first, check if the character's original faction is still alive
        if (!ogFaction.isDestroyed) { //if it is, 
            this.ChangeFactionTo(ogFaction);  //transfer the character to his original faction
            if (ogFaction.id == FactionManager.Instance.neutralFaction.id) { //if the character's original faction is the neutral faction
                if (ogHome.owner == null && (ogHome.area == null || (ogHome.area != null && !ogHome.area.IsResidentsFull()))) { //check if his original home is still unowned
                    //if it is and it has not reached it's resident capacity, return him to his original home
                    MigrateHomeTo(ogHome);
                } else { //if it does not meet those requirements
                    //check if the neutral faction still has any available areas that have not reached capacity yet
                    List<Region> validNeutralAreas = FactionManager.Instance.neutralFaction.ownedRegions.Where(x => x.area == null || (x.area != null && !x.area.IsResidentsFull())).ToList();
                    if (validNeutralAreas.Count > 0) {
                        //if it does, pick randomly from those
                        Region chosenArea = validNeutralAreas[UnityEngine.Random.Range(0, validNeutralAreas.Count)];
                        MigrateHomeTo(chosenArea);
                    }
                    //if not, keep the characters current home
                }
            } else { //if it is not, check if his original home is still owned by that faction and it has not yet reached it's resident capacity
                if (ogHome.IsFactionHere(ogFaction) && (ogHome.area == null || (ogHome.area != null && !ogHome.area.IsResidentsFull()))) {
                    //if it meets those requirements, return the character's home to that location
                    MigrateHomeTo(ogHome);
                } else { //if not, get another area owned by his faction that has not yet reached capacity
                    List<Region> validAreas = ogFaction.ownedRegions.Where(x => x.area == null || (x.area != null && !x.area.IsResidentsFull())).ToList();
                    if (validAreas.Count > 0) {
                        Region chosenArea = validAreas[UnityEngine.Random.Range(0, validAreas.Count)];
                        MigrateHomeTo(chosenArea);
                    }
                    //if there are still no areas that can be his home, keep his current one.
                }
            }
        } else { //if not
            //transfer the character to the neutral faction instead
            this.ChangeFactionTo(FactionManager.Instance.neutralFaction);
            List<Region> validNeutralAreas = FactionManager.Instance.neutralFaction.ownedRegions.Where(x => x.area == null || (x.area != null && !x.area.IsResidentsFull())).ToList();
            if (validNeutralAreas.Count > 0) {  //then check if the neutral faction has any owned areas that have not yet reached area capacity
                //if it does, pick from any of those, then set it as the characters new home
                Region chosenArea = validNeutralAreas[UnityEngine.Random.Range(0, validNeutralAreas.Count)];
                MigrateHomeTo(chosenArea);
            }
            //if it does not, keep the characters current home
        }
    }
    public override string ToString() {
        return name;
    }
    //public void AdjustIsWaitingForInteraction(int amount) {
    //    isWaitingForInteraction += amount;
    //    if (isWaitingForInteraction < 0) {
    //        isWaitingForInteraction = 0;
    //    }
    //}
    public LocationGridTile GetLocationGridTileByXY(int x, int y, bool throwOnException = true) {
        return currentRegion.innerMap.map[x, y];
        //if (currentRegion != null) {
        //    return currentRegion.innerMap.map[x, y];    
        //} else {
        //    return currentArea.innerMap.map[x, y];
        //}
        
        //try {
        //    if (throwOnException) {
        //        return specificLocation.areaMap.map[x, y];
        //    } else {
        //        if (Utilities.IsInRange(x, 0, specificLocation.areaMap.map.GetUpperBound(0) + 1) &&
        //            Utilities.IsInRange(y, 0, specificLocation.areaMap.map.GetUpperBound(1) + 1)) {
        //            return specificLocation.areaMap.map[x, y];
        //        }
        //        return null;
        //    }
        //} catch (Exception e) {
        //    throw new Exception(e.Message + "\n " + this.name + "(" + x.ToString() + ", " + y.ToString() + ")");
        //}

    }
    public void UpdateCanCombatState() {
        bool state = false;
        if (!_characterClass.isNonCombatant && traitContainer.GetNormalTrait<Trait>("Injured") == null) {
            state = true;
        }
        if (canCombat != state) {
            canCombat = state;
            if (canCombat && marker != null) {
                marker.ClearTerrifyingObjects();
            }
        }
    }
    public bool CanCharacterReact(IPointOfInterest targetPOI = null) {
        if (this.canWitness == false) {
            return false; //this character cannot witness
        }
        if (this is Summon || minion != null) {
            //Cannot react if summon or minion
            return false;
        }
        if (defaultCharacterTrait.hasSurvivedApprehension && !isAtHomeRegion) {
            return false; //Must not react because he will only have one thing to do and that is to return home
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
        //if (traitContainer.GetNormalTrait<Trait>("Berserked") != null) { //|| (stateComponent.stateToDo != null && stateComponent.stateToDo.characterState == CHARACTER_STATE.BERSERKED && !stateComponent.stateToDo.isDone)
        //    //Character must not react if he/she is in berserked state
        //    //Returns true so that it will create an impression that the character actually created a job even if he/she didn't, so that the character will not chat, etc.
        //    return false;
        //}
        if (traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
            return false;
        }
        if (targetPOI != null && targetPOI is Character) {
            Character target = targetPOI as Character;
            if (target.faction != null && target.faction.IsHostileWith(faction)) {
                //Cannot react if target charcter is from a hostile faction
                //Only combat those characters that's why they cannot react to their traits, actions, etc.
                return false;
            }
        }
        return true;
    }
    public bool IsAble() {
        return currentHP > 0 && !traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) && !isDead && characterClass.className != "Zombie";
    }
    public void SetIsFollowingPlayerInstruction(bool state) {
        isFollowingPlayerInstruction = state;
    }
    /// <summary>
    /// Can this character be instructed by the player?
    /// </summary>
    /// <returns>True or false.</returns>
    public virtual bool CanBeInstructedByPlayer() {
        if (!faction.isPlayerFaction) {
            return false;
        }
        if (isDead) {
            return false;
        }
        if (stateComponent.currentState is CombatState && !(stateComponent.currentState as CombatState).isAttacking) {
            return false; //character is fleeing
        }
        if (traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
            return false;
        }
        return true;
    }
    public void SetTileObjectLocation(TileObject tileObject) {
        tileObjectLocation = tileObject;
    }
    public void AdjustIsStoppedByOtherCharacter(int amount) {
        isStoppedByOtherCharacter += amount;
        isStoppedByOtherCharacter = Mathf.Max(0, isStoppedByOtherCharacter);
        if (marker != null) {
            marker.UpdateAnimation();
        }
    }
    public virtual bool IsValidCombatTarget() {
        return traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) == false;
    }
    public void ExecutePendingActionsAfterMultithread() {
        for (int i = 0; i < pendingActionsAfterMultiThread.Count; i++) {
            pendingActionsAfterMultiThread[i].Invoke();
        }
        pendingActionsAfterMultiThread.Clear();
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
                //if (this._history[0].goapAction != null) {
                //    this._history[0].goapAction.AdjustReferenceCount(-1);
                //}
                this._history.RemoveAt(0);
            }
            //if(log.goapAction != null) {
            //    log.goapAction.AdjustReferenceCount(1);
            //}
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
            return true;
        }
        return false;
    }
    //Add log to this character and show notif of that log only if this character is clicked or tracked, otherwise, add log only
    public void RegisterLogAndShowNotifToThisCharacterOnly(string fileName, string key, object target = null, string targetName = "", ActualGoapNode node = null, bool onlyClickedCharacter = true) {
        if (!GameManager.Instance.gameHasStarted) {
            return;
        }
        if (key == "remove_trait" && isDead) {
            return;
        }
        Log addLog = new Log(GameManager.Instance.Today(), "Character", fileName, key, node);
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
    public virtual void OnAfterActionStateSet(string stateName, ActualGoapNode node) {
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
        //    if (GetNormalTrait<Trait>("Unconscious", "Resting") != null) {
        //        return;
        //    }
        //    if (marker.inVisionCharacters.Contains(action.actor)) {
        //        ThisCharacterWitnessedEvent(action);
        //        ThisCharacterWatchEvent(null, action, state);
        //    }
        //}
        ///Moved all needed checking <see cref="CharacterManager.OnActionStateSet(GoapAction, GoapActionState)"/>
        ///
        if (isDead || !canWitness) {
            return;
        }
        if (node.action.goapType == INTERACTION_TYPE.WATCH) {
            //Cannot witness/watch a watch action
            return;
        }
        if (node.actor == this || node.poiTarget == this) {
            //Cannot witness if character is part of the action
            return;
        }
        if (!node.action.shouldAddLogs) {
            return;
        }

        //Instead of witnessing the action immediately, it needs to be pooled to avoid duplicates, so add the supposed to be witnessed action to the list and let ProcessAllUnprocessedVisionPOIs in CharacterMarker do its thing
        if (marker != null) { //&& !marker.actionsToWitness.Contains(node)
            if (marker.inVisionCharacters.Contains(node.actor)) {
                marker.actionsToWitness.Add(node);
                //This is done so that the character will react again
                marker.unprocessedVisionPOIs.Add(node.actor);
            } else if (marker.inVisionCharacters.Contains(node.poiTarget)) {
                marker.actionsToWitness.Add(node);
                //This is done so that the character will react again
                marker.unprocessedVisionPOIs.Add(node.poiTarget);
            }
        }

        //ThisCharacterWitnessedEvent(action);
        //ThisCharacterWatchEvent(null, action, state);
    }

    //Returns the list of goap actions to be witnessed by this character
    public virtual List<ActualGoapNode> ThisCharacterSaw(IPointOfInterest target) {
        if (isDead || !canWitness) {
            return null;
        }

        if (currentActionNode != null && currentActionNode.actionStatus == ACTION_STATUS.STARTED && currentActionNode.isStealth) {
            if (currentActionNode.poiTarget == target) {
                //Upon seeing the target while performing a stealth job action, check if it can do the action
                if (!marker.CanDoStealthActionToTarget(target)) {
                    currentJob.CancelJob(reason: "There is a witness around");
                }
            } else {
                //Upon seeing other characters while target of stealth action is already in vision, automatically cancel job
                if (marker.inVisionCharacters.Contains(currentActionNode.poiTarget)) {
                    currentJob.CancelJob(reason: "There is a witness around");
                }
            }

        }
        //if (target.allJobsTargettingThis.Count > 0) {
        //    //Upon seeing a character and that character is being targetted by a stealth job and the actor of that job is not this one, and the actor is performing that job and the actor already sees the target
        //    List<GoapAction> allActionsTargettingTargetCharacter = new List<GoapAction>(target.targettedByAction);
        //    for (int i = 0; i < allActionsTargettingTargetCharacter.Count; i++) {
        //        GoapAction action = allActionsTargettingTargetCharacter[i];
        //        if (action.isStealth && action.actor != this) {
        //            if (!action.isDone && !action.isPerformingActualAction && action.actor.currentAction == action
        //                && action.actor.marker.inVisionPOIs.Contains(target)) {
        //                action.StopAction(true, "There is a witness around");
        //            }
        //        }
        //    }
        //}

        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            traitContainer.allTraits[i].OnSeePOI(target, this);
        }
        if (target is Character) {
            Character targetCharacter = target as Character;
            targetCharacter.OnSeenBy(this); //trigger that the target character was seen by this character.

            List<ActualGoapNode> actionsToWitness = new List<ActualGoapNode>();
            if (target.allJobsTargettingThis.Count > 0) {
                //We get the actions targetting the target character because a character must also witness an action even if he only sees the target and not the actor
                //Collect all actions first to avoid duplicates 
                for (int i = 0; i < target.allJobsTargettingThis.Count; i++) {
                    if (target.allJobsTargettingThis[i] is GoapPlanJob) {
                        GoapPlanJob job = target.allJobsTargettingThis[i] as GoapPlanJob;
                        GoapPlan plan = job.assignedPlan;
                        if (plan != null && plan.currentActualNode.action.shouldAddLogs && plan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING) {
                            actionsToWitness.Add(plan.currentActualNode);
                        }
                    }
                }
            }
            ActualGoapNode node = targetCharacter.currentActionNode;
            if (node != null && node.action.shouldAddLogs && node.actionStatus == ACTION_STATUS.PERFORMING) {
                actionsToWitness.Add(node);
            }
            //if (targetCharacter.currentAction != null && targetCharacter.currentAction.isPerformingActualAction && !targetCharacter.currentAction.isDone && targetCharacter.currentAction.goapType != INTERACTION_TYPE.WATCH) {
            //    //Cannot witness/watch a watch action
            //    IPointOfInterest poiTarget = null;
            //    if (targetCharacter.currentAction.goapType == INTERACTION_TYPE.MAKE_LOVE) {
            //        poiTarget = (targetCharacter.currentAction as MakeLove).targetCharacter;
            //    } else {
            //        poiTarget = targetCharacter.currentAction.poiTarget;
            //    }
            //    if (targetCharacter.currentAction.actor != this && poiTarget != this) {
            //        actionsToWitness.Add(targetCharacter.currentAction);
            //        //ThisCharacterWitnessedEvent(targetCharacter.currentAction);
            //        //ThisCharacterWatchEvent(targetCharacter, targetCharacter.currentAction, targetCharacter.currentAction.currentState);
            //    }
            //} 
            //else if (targetCharacter.currentAction != null && targetCharacter.currentAction.currentState != null && targetCharacter.currentAction.currentState.name == targetCharacter.currentAction.whileMovingState) {
            //    //Must also witness whileMovingState
            //    IPointOfInterest poiTarget = null;
            //    if (targetCharacter.currentAction.goapType == INTERACTION_TYPE.MAKE_LOVE) {
            //        poiTarget = (targetCharacter.currentAction as MakeLove).targetCharacter;
            //    } else {
            //        poiTarget = targetCharacter.currentAction.poiTarget;
            //    }
            //    if (targetCharacter.currentAction.actor != this && poiTarget != this) {
            //        actionsToWitness.Add(targetCharacter.currentAction);
            //        //ThisCharacterWitnessedEvent(targetCharacter.currentAction);
            //        //ThisCharacterWatchEvent(targetCharacter, targetCharacter.currentAction, targetCharacter.currentAction.currentState);
            //    }
            //}
            //This will only happen if target is in combat
            ThisCharacterWatchEvent(targetCharacter, null, null);
            return actionsToWitness;
        }
        //else {
        //    for (int i = 0; i < normalTraits.Count; i++) {
        //        normalTraits[i].OnSeePOI(target, this);
        //    }
        //}
        return null;
    }
    /// <summary>
    /// What should happen if another character sees this character?
    /// </summary>
    /// <param name="character">The character that saw this character.</param>
    protected virtual void OnSeenBy(Character character) { }
    public List<Log> GetMemories(int dayFrom, int dayTo, bool eventMemoriesOnly = false) {
        List<Log> memories = new List<Log>();
        if (eventMemoriesOnly) {
            for (int i = 0; i < _history.Count; i++) {
                if (_history[i].node != null) {
                    if (_history[i].day >= dayFrom && _history[i].day <= dayTo) {
                        memories.Add(_history[i]);
                    }
                }
            }
        } else {
            for (int i = 0; i < _history.Count; i++) {
                if (_history[i].day >= dayFrom && _history[i].day <= dayTo) {
                    memories.Add(_history[i]);
                }
            }
        }
        return memories;
    }
    //public List<Log> GetWitnessOrInformedMemories(int dayFrom, int dayTo, Character involvedCharacter = null) {
    //    List<Log> memories = new List<Log>();
    //    for (int i = 0; i < _history.Count; i++) {
    //        Log historyLog = _history[i];
    //        if (historyLog.goapAction != null && (historyLog.key == "witness_event" || historyLog.key == "informed_event")) {
    //            if (historyLog.day >= dayFrom && historyLog.day <= dayTo) {
    //                if(involvedCharacter != null) {
    //                    if (historyLog.goapAction.actor == involvedCharacter || historyLog.goapAction.IsTarget(involvedCharacter)) {
    //                        memories.Add(historyLog);
    //                    }
    //                } else {
    //                    memories.Add(historyLog);
    //                }
    //            }
    //        }
    //    }
    //    return memories;
    //}
    //public List<Log> GetCrimeMemories(int dayFrom, int dayTo, Character involvedCharacter = null) {
    //    List<Log> memories = new List<Log>();
    //    for (int i = 0; i < _history.Count; i++) {
    //        Log historyLog = _history[i];
    //        if (historyLog.goapAction != null && historyLog.goapAction.awareCharactersOfThisAction.Contains(this) && historyLog.goapAction.committedCrime != CRIME.NONE) {
    //            if (historyLog.day >= dayFrom && historyLog.day <= dayTo) {
    //                if(involvedCharacter != null) {
    //                    for (int j = 0; j < historyLog.goapAction.crimeCommitters.Length; j++) {
    //                        Character criminal = historyLog.goapAction.crimeCommitters[j];
    //                        if (criminal == involvedCharacter) {
    //                            memories.Add(historyLog);
    //                            break;
    //                        }
    //                    }
    //                } else {
    //                    memories.Add(historyLog);
    //                }
    //            }
    //        }
    //    }
    //    return memories;
    //}
    public void CreateInformedEventLog(ActualGoapNode eventToBeInformed, bool invokeShareIntelReaction) {
        Log informedLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "informed_event", eventToBeInformed);
        informedLog.AddToFillers(eventToBeInformed.descriptionLog.fillers);
        informedLog.AddToFillers(this, this.name, LOG_IDENTIFIER.OTHER);
        informedLog.AddToFillers(null, Utilities.LogDontReplace(eventToBeInformed.descriptionLog), LOG_IDENTIFIER.APPEND);
        AddHistory(informedLog);

        //if (invokeShareIntelReaction) {
        //    if (eventToBeInformed.currentState.shareIntelReaction != null) {
        //        eventToBeInformed.currentState.shareIntelReaction.Invoke(this, null, SHARE_INTEL_STATUS.INFORMED);
        //    }
        //}
        //eventToBeInformed.AddAwareCharacter(this);

        ////If a character sees or informed about a lover performing Making Love or Ask to Make Love, they will feel Betrayed
        //if (eventToBeInformed.actor != this && !eventToBeInformed.IsTarget(this)) {
        //    Character target = eventToBeInformed.poiTarget as Character;
        //    if (eventToBeInformed.goapType == INTERACTION_TYPE.MAKE_LOVE) {
        //        target = (eventToBeInformed as MakeLove).targetCharacter; //NOTE: Changed this, because technically the Make Love Action targets the bed, and the target character is stored in the event itself.
        //        if (HasRelationshipOfTypeWith(eventToBeInformed.actor, RELATIONSHIP_TRAIT.LOVER) || HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
        //            Betrayed betrayed = new Betrayed();
        //            AddTrait(betrayed);
        //            RelationshipManager.Instance.RelationshipDegradation(eventToBeInformed.actor, this, eventToBeInformed);
        //            RelationshipManager.Instance.RelationshipDegradation(target, this, eventToBeInformed);
        //        }
        //    } else if (eventToBeInformed.goapType == INTERACTION_TYPE.INVITE_TO_MAKE_LOVE) {
        //        if (HasRelationshipOfTypeWith(eventToBeInformed.actor, RELATIONSHIP_TRAIT.LOVER)) {
        //            Betrayed betrayed = new Betrayed();
        //            AddTrait(betrayed);
        //            RelationshipManager.Instance.RelationshipDegradation(eventToBeInformed.actor, this, eventToBeInformed);
        //            RelationshipManager.Instance.RelationshipDegradation(target, this, eventToBeInformed);
        //        } else if (HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
        //            if (eventToBeInformed.currentState.name == "Invite Success") {
        //                Betrayed betrayed = new Betrayed();
        //                AddTrait(betrayed);
        //                RelationshipManager.Instance.RelationshipDegradation(eventToBeInformed.actor, this, eventToBeInformed);
        //                RelationshipManager.Instance.RelationshipDegradation(target, this, eventToBeInformed);
        //            }
        //        }
        //    }
        //}
    }
    public void ThisCharacterWitnessedEvent(ActualGoapNode witnessedEvent) {
        //if (isDead || !canWitness) {
        //    return;
        //}
        if (faction != witnessedEvent.actor.faction && //only check faction relationship if involved characters are of different factions
            faction.IsHostileWith(witnessedEvent.actor.faction)) {
            //Must not react if the faction of the actor of witnessed action is hostile with the faction of the witness
            return;
        }


        if (witnessedEvent.currentStateName == null) {
            throw new System.Exception(GameManager.Instance.TodayLogString() + this.name + " witnessed event " + witnessedEvent.action.goapName + " by " + witnessedEvent.actor.name + " but it does not have a current state!");
        }
        if (witnessedEvent.descriptionLog == null) {
            throw new Exception(GameManager.Instance.TodayLogString() + this.name + " witnessed event " + witnessedEvent.action.goapName + " by " + witnessedEvent.actor.name + " with state " + witnessedEvent.currentStateName + " but it does not have a description log!");
        }
        Log witnessLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "witness_event", witnessedEvent);
        witnessLog.AddToFillers(this, name, LOG_IDENTIFIER.OTHER);
        witnessLog.AddToFillers(null, Utilities.LogDontReplace(witnessedEvent.descriptionLog), LOG_IDENTIFIER.APPEND);
        witnessLog.AddToFillers(witnessedEvent.descriptionLog.fillers);
        AddHistory(witnessLog);

        if (faction.isPlayerFaction) {
            //Player characters cannot react to witnessed events
            return;
        }
        //if (witnessedEvent.currentState.shareIntelReaction != null && !isFactionless) { //Characters with no faction cannot witness react
        //    List<string> reactions = witnessedEvent.currentState.shareIntelReaction.Invoke(this, null, SHARE_INTEL_STATUS.WITNESSED);
        //    if(reactions != null) {
        //        string reactionLog = name + " witnessed event: " + witnessedEvent.goapName;
        //        reactionLog += "\nREACTION:";
        //        for (int i = 0; i < reactions.Count; i++) {
        //            reactionLog += "\n" + reactions[i];
        //        }
        //        PrintLogIfActive(reactionLog);
        //    }
        //}
        //witnessedEvent.AddAwareCharacter(this);

        //If a character sees or informed about a lover performing Making Love or Ask to Make Love, they will feel Betrayed
        //if (witnessedEvent.actor != this && !witnessedEvent.IsTarget(this)) {
        //    Character target = witnessedEvent.poiTarget as Character;
        //    if (witnessedEvent.goapType == INTERACTION_TYPE.MAKE_LOVE) {
        //        target = (witnessedEvent as MakeLove).targetCharacter;
        //        if (relationshipContainer.HasRelationshipWith(witnessedEvent.actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) || relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
        //            Betrayed betrayed = new Betrayed();
        //            traitContainer.AddTrait(this, betrayed);
        //            //RelationshipManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
        //            //RelationshipManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
        //        } 
        //    } else if (witnessedEvent.goapType == INTERACTION_TYPE.INVITE) {
        //        if (relationshipContainer.HasRelationshipWith(witnessedEvent.actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
        //            Betrayed betrayed = new Betrayed();
        //            traitContainer.AddTrait(this, betrayed);
        //            //RelationshipManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
        //            //RelationshipManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
        //        } else if (relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
        //            if (witnessedEvent.currentState.name == "Invite Success") {
        //                Betrayed betrayed = new Betrayed();
        //                traitContainer.AddTrait(this, betrayed);
        //                //RelationshipManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
        //                //RelationshipManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
        //            }
        //        }
        //    }
        //}
    }
    /// <summary>
    /// This character watched an action happen.
    /// </summary>
    /// <param name="targetCharacter">The character that was performing the action.</param>
    /// <param name="action">The action that was performed.</param>
    /// <param name="state">The state the action was in when this character watched it.</param>
    public void ThisCharacterWatchEvent(Character targetCharacter, GoapAction action, GoapActionState state) {
        if (faction.isPlayerFaction) {
            //Player characters cannot watch events
            return;
        }
        if (action == null) {
            if (targetCharacter != null && targetCharacter.isInCombat
                && targetCharacter.faction == faction) {
                CombatState targetCombatState = targetCharacter.stateComponent.currentState as CombatState;
                if (targetCombatState.currentClosestHostile != null && targetCombatState.currentClosestHostile != this) {
                    if (targetCombatState.currentClosestHostile is Character) {
                        Character currentHostileOfTargetCharacter = targetCombatState.currentClosestHostile as Character;
                        if (currentHostileOfTargetCharacter.stateComponent.currentState != null
                            && !currentHostileOfTargetCharacter.stateComponent.currentState.isDone
                            && currentHostileOfTargetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                            CombatState combatStateOfCurrentHostileOfTargetCharacter = currentHostileOfTargetCharacter.stateComponent.currentState as CombatState;
                            if (combatStateOfCurrentHostileOfTargetCharacter.currentClosestHostile != null
                                && combatStateOfCurrentHostileOfTargetCharacter.currentClosestHostile == this) {
                                //If character 1 is supposed to watch/join the combat of character 2 against character 3
                                //but character 3 is already in combat and his current target is already character 1
                                //then character 1 should not react
                                return;
                            }
                        }

                        if (currentHostileOfTargetCharacter.faction == faction) {
                            RELATIONSHIP_EFFECT relEffectTowardsTarget = relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo);
                            RELATIONSHIP_EFFECT relEffectTowardsTargetOfCombat = relationshipContainer.GetRelationshipEffectWith(currentHostileOfTargetCharacter.currentAlterEgo);

                            if (relEffectTowardsTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                                if (relEffectTowardsTargetOfCombat == RELATIONSHIP_EFFECT.POSITIVE) {
                                    CreateWatchEvent(null, targetCombatState, targetCharacter);
                                } else {
                                    if (marker.AddHostileInRange(targetCombatState.currentClosestHostile, false, false, isLethal: targetCharacter.marker.IsLethalCombatForTarget(currentHostileOfTargetCharacter))) {
                                        if (!marker.avoidInRange.Contains(targetCharacter)) {
                                            //Do process combat behavior first for this character, if the current closest hostile
                                            //of the combat state of this character is also the targetCombatState.currentClosestHostile
                                            //Then that's only when we apply the join combat log and notif
                                            //Because if not, it means that this character is already in combat with someone else, and thus
                                            //should not product join combat log anymore
                                            List<RELATIONSHIP_TRAIT> rels = relationshipContainer.GetRelationshipDataWith(targetCharacter.currentAlterEgo)?.relationships.OrderByDescending(x => (int) x).ToList() ?? null; //so that the first relationship to be returned is the one with higher importance.
                                            Log joinLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "join_combat");
                                            joinLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                            joinLog.AddToFillers(targetCombatState.currentClosestHostile, targetCombatState.currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                                            joinLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
                                            joinLog.AddToFillers(null, Utilities.NormalizeString(rels.First().ToString()), LOG_IDENTIFIER.STRING_1);
                                            joinLog.AddLogToSpecificObjects(LOG_IDENTIFIER.ACTIVE_CHARACTER, LOG_IDENTIFIER.TARGET_CHARACTER);
                                            PlayerManager.Instance.player.ShowNotificationFrom(this, joinLog);
                                        }
                                        //marker.ProcessCombatBehavior();
                                    }
                                }
                            } else {
                                if (relEffectTowardsTargetOfCombat == RELATIONSHIP_EFFECT.POSITIVE) {
                                    if (marker.AddHostileInRange(targetCharacter, false, false, isLethal: targetCharacter.marker.IsLethalCombatForTarget(currentHostileOfTargetCharacter))) {
                                        if (!marker.avoidInRange.Contains(targetCharacter)) {
                                            //Do process combat behavior first for this character, if the current closest hostile
                                            //of the combat state of this character is also the targetCombatState.currentClosestHostile
                                            //Then that's only when we apply the join combat log and notif
                                            //Because if not, it means that this character is already in combat with someone else, and thus
                                            //should not product join combat log anymore
                                            List<RELATIONSHIP_TRAIT> rels = relationshipContainer.GetRelationshipDataWith(currentHostileOfTargetCharacter.currentAlterEgo)?.relationships.OrderByDescending(x => (int) x).ToList() ?? null; //so that the first relationship to be returned is the one with higher importance.
                                            Log joinLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "join_combat");
                                            joinLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                            joinLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                                            joinLog.AddToFillers(targetCombatState.currentClosestHostile, targetCombatState.currentClosestHostile.name, LOG_IDENTIFIER.CHARACTER_3);
                                            joinLog.AddToFillers(null, Utilities.NormalizeString(rels.First().ToString()), LOG_IDENTIFIER.STRING_1);
                                            joinLog.AddLogToSpecificObjects(LOG_IDENTIFIER.ACTIVE_CHARACTER, LOG_IDENTIFIER.TARGET_CHARACTER);
                                            PlayerManager.Instance.player.ShowNotificationFrom(this, joinLog);
                                        }
                                        //marker.ProcessCombatBehavior();
                                    }
                                } else {
                                    CreateWatchEvent(null, targetCombatState, targetCharacter);
                                }
                            }
                        } else {
                            //the target of the combat state is not part of this character's faction
                            if (marker.AddHostileInRange(targetCombatState.currentClosestHostile, false, false, isLethal: targetCharacter.marker.IsLethalCombatForTarget(currentHostileOfTargetCharacter))) {
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
                                    PlayerManager.Instance.player.ShowNotificationFrom(this, joinLog);
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
        //    } else if (action.goapType == INTERACTION_TYPE.PLAY_GUITAR && state.name == "Play Success" && GetNormalTrait<Trait>("MusicHater") == null) {
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
    public void CreateWatchEvent(ActualGoapNode actionToWatch, CharacterState stateToWatch, Character targetCharacter) {
        string summary = "Creating watch event for " + name + " with target " + targetCharacter.name;
        if (actionToWatch != null) {
            summary += " involving " + actionToWatch.goapName;
        } else if (stateToWatch != null) {
            if (stateToWatch is CombatState) {
                summary += " involving Combat";
            } else if (stateToWatch is DouseFireState) {
                summary += " involving Douse Fire";
            }

        }
        if (currentActionNode != null && !currentActionNode.isDone && currentActionNode.action.goapType == INTERACTION_TYPE.WATCH) {
            summary += "\n-Already watching an action, will not watch another one...";
            PrintLogIfActive(summary);
            return;
        }
        if (stateComponent.currentState != null && (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT || stateComponent.currentState.characterState == CHARACTER_STATE.BERSERKED || stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE)) {
            summary += "\n-In combat state/berserked state/douse fire state, must not watch...";
            PrintLogIfActive(summary);
            return;
        }
        //if (HasPlanWithType(INTERACTION_TYPE.WATCH)) {
        //    summary += "\n-Already has watch action in queue, will not watch another one...";
        //    PrintLogIfActive(summary);
        //    return;
        //}
        int watchJobPriority = InteractionManager.Instance.GetInitialPriority(JOB_TYPE.WATCH);
        JobQueueItem currJob = currentJob;
        if (currJob != null) {
            if (watchJobPriority >= currJob.priority) {
                summary += "\n-Current action job " + currJob.name + " priority: " + currJob.priority + " is higher or equal than Watch Job priority " + watchJobPriority + ", will not watch...";
                PrintLogIfActive(summary);
                return;
            }
        }
        //if (stateComponent.currentState != null && stateComponent.currentState.job != null && stateComponent.currentState.job.priority <= watchJobPriority) {
        //    summary += "\n-Current state job " + stateComponent.currentState.job.name + " priority: " + stateComponent.currentState.job.priority + " is higher or equal than Watch Job priority " + watchJobPriority + ", will not watch...";
        //    PrintLogIfActive(summary);
        //    return;
        //} 
        ////else if (stateComponent.stateToDo != null && stateComponent.stateToDo.job != null && stateComponent.stateToDo.job.priority <= watchJobPriority) {
        ////    summary += "\n-State to do job " + stateComponent.stateToDo.job.name + " priority: " + stateComponent.stateToDo.job.priority + " is higher or equal than Watch Job priority " + watchJobPriority + ", will not watch...";
        ////    PrintLogIfActive(summary);
        ////    return;
        ////} 
        //else if (currentActionNode != null && currentActionNode.parentPlan != null && currentActionNode.parentPlan.job != null && currentActionNode.parentPlan.job.priority <= watchJobPriority) {
        //    summary += "\n-Current action job " + currentActionNode.parentPlan.job.name + " priority: " + currentActionNode.parentPlan.job.priority + " is higher or equal than Watch Job priority " + watchJobPriority + ", will not watch...";
        //    PrintLogIfActive(summary);
        //    return;
        //}
        //if (stateComponent.currentState != null) {
        //    summary += "\nEnding current state " + stateComponent.currentState.stateName + " before watching...";
        //    stateComponent.currentState.OnExitThisState();
        //    //This call is doubled so that it will also exit the previous major state if there's any
        //    if (stateComponent.currentState != null) {
        //        stateComponent.currentState.OnExitThisState();
        //    }
        //} 
        ////else if (stateComponent.stateToDo != null) {
        ////    summary += "\nEnding state to do " + stateComponent.stateToDo.stateName + " before watching...";
        ////    stateComponent.SetStateToDo(null);
        ////} 
        //else {
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
        //    StopCurrentAction(false, "Have something important to do");
        //    AdjustIsWaitingForInteraction(-1);
        //}
        summary += "\nWatch event created.";
        PrintLogIfActive(summary);
        ActualGoapNode node = null;
        if (actionToWatch != null) {
            node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.WATCH], this, targetCharacter, new object[] { actionToWatch }, 0);
        } else if (stateToWatch != null) {
            node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.WATCH], this, targetCharacter, new object[] { stateToWatch }, 0);
        }
        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetCharacter);
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.WATCH, INTERACTION_TYPE.WATCH, targetCharacter, this);
        goapPlan.SetDoNotRecalculate(true);
        job.SetCannotBePushedBack(true);
        job.SetAssignedPlan(goapPlan);

        jobQueue.AddJobInQueue(job);

        //Watch watchAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.WATCH, this, targetCharacter) as Watch;
        //if (actionToWatch != null) {
        //    watchAction.InitializeOtherData(new object[] { actionToWatch });
        //} else if (stateToWatch != null) {
        //    watchAction.InitializeOtherData(new object[] { stateToWatch });
        //}
        //GoapNode goalNode = new GoapNode(null, watchAction.cost, watchAction);
        //GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.WATCH, INTERACTION_TYPE.WATCH, this);
        //goapPlan.ConstructAllNodes();
        //goapPlan.SetDoNotRecalculate(true);
        //job.SetAssignedPlan(goapPlan);
        ////job.SetCancelOnFail(true);
        ////job.SetCancelJobOnDropPlan(true);

        //jobQueue.AddJobInQueue(job, false);
        ////jobQueue.AssignCharacterToJobAndCancelCurrentAction(job, this);
        //AddPlan(goapPlan, true);
    }
    #endregion

    #region Combat Handlers
    //public void SetCombatCharacter(CombatCharacter combatCharacter) {
    //    _currentCombatCharacter = combatCharacter;
    //}
    /// <summary>
    /// This character was hit by an attack.
    /// </summary>
    /// <param name="characterThatAttacked">The character that attacked this.</param>
    /// <param name="state">The combat state that the attacker is in.</param>
    /// <param name="attackSummary">reference log of what happened.</param>
    public void OnHitByAttackFrom(Character characterThatAttacked, CombatState state, ref string attackSummary) {
        GameManager.Instance.CreateHitEffectAt(this);
        if (this.currentHP <= 0) {
            return; //if hp is already 0, do not deal damage
        }

        //If someone is attacked, relationship should deteriorate
        //TODO: SAVE THE allCharactersThatDegradeRel list so when loaded they will not be able to degrade rel again
        Character responsibleCharacter = null;
        if (state != null) {
            if (state.currentClosestHostile == this) {
                //Do not set as responsible character for unconscious trait if character is hit unintentionally
                //So, only set responsible character if currentClosestHostile is this character, meaning, this character is really the target
                responsibleCharacter = characterThatAttacked;
                if (!state.allCharactersThatDegradedRel.Contains(this)) {
                    RelationshipManager.Instance.RelationshipDegradation(characterThatAttacked, this);
                    state.AddCharacterThatDegradedRel(this);
                }
            }
        }

        this.AdjustHP(-characterThatAttacked.attackPower, source: characterThatAttacked);
        attackSummary += "\nDealt damage " + stateComponent.character.attackPower.ToString();
        //If the hostile reaches 0 hp, evalueate if he/she dies, get knock out, or get injured
        if (this.currentHP <= 0) {
            attackSummary += "\n" + this.name + "'s hp has reached 0.";
            WeightedDictionary<string> loserResults = new WeightedDictionary<string>();

            int deathWeight = 70; //70
            int unconsciousWeight = 30; //30
            if (!characterThatAttacked.marker.IsLethalCombatForTarget(this)) {
                deathWeight = 5;
                unconsciousWeight = 95;
            }
            string rollLog = GameManager.Instance.TodayLogString() + characterThatAttacked.name + " attacked " + name
                + ", death weight: " + deathWeight + ", unconscious weight: " + unconsciousWeight
                + ", isLethal: " + characterThatAttacked.marker.IsLethalCombatForTarget(this);

            if (minion == null && this.traitContainer.GetNormalTrait<Trait>("Unconscious") == null) {
                loserResults.AddElement("Unconscious", unconsciousWeight);
                rollLog += "\n- Unconscious weight will be added";
            }
            //if (currentClosestHostile.GetNormalTrait<Trait>("Injured") == null) {
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
                        //Unconscious unconscious = new Unconscious();
                        traitContainer.AddTrait(this, "Unconscious", responsibleCharacter);
                        break;
                    case "Injured":
                        //Injured injured = new Injured();
                        traitContainer.AddTrait(this, "Injured", responsibleCharacter);
                        break;
                    case "Death":
                        string deathReason = "attacked";
                        if (!characterThatAttacked.marker.IsLethalCombatForTarget(this)) {
                            deathReason = "accidental_attacked";
                        }
                        this.Death(deathReason, responsibleCharacter: responsibleCharacter);
                        break;
                }
            } else {
                rollLog += "\n- Dictionary is empty, no result!";
                characterThatAttacked.PrintLogIfActive(rollLog);
            }
        }
        //else {
        //    Invisible invisible = characterThatAttacked.GetNormalTrait<Trait>("Invisible") as Invisible;
        //    if (invisible != null) {
        //        if (invisible.level == 1) {
        //            //Level 1 = remove invisible trait
        //            characterThatAttacked.RemoveTrait(invisible);
        //        } else if (invisible.level == 2) {
        //            //Level 2 = attacked character will be the only character to see
        //            invisible.AddCharacterThatCanSee(this);
        //        }
        //        //Level 3 = will not be seen forever
        //    }
        //}

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
        if (needsComponent.isStarving && traitContainer.GetNormalTrait<Trait>("Vampiric") == null) {
            return false; //only characters that are not vampires will flee if they are starving
        }
        if (needsComponent.isExhausted) {
            return false;
        }
        return true;
    }
    #endregion

    #region RPG
    private void StartingLevel() {
        _level = 1;
        UpdateMaxHP();
        ResetToFullHP();
    }

    private bool hpMagicRangedStatMod;
    public virtual void LevelUp() {
        //Only level up once per day
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
            if (range > 0) {
                for (int i = 0; i < range; i++) {
                    if (i % 2 == 0) {
                        //even
                        AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                    } else {
                        //odd
                        AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                    }
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
        } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (amount < 0 ? -1 : 1);
            int range = amount * multiplier;
            if (range > 0) {
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
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
        }
        _level += amount;

        UpdateMaxHP();
        //Reset to full health and sp
        ResetToFullHP();
        //ResetToFullSP();
        Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
        //if (_playerCharacterItem != null) {
        //    _playerCharacterItem.UpdateMinionItem();
        //}
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
            if (range > 0) {
                for (int i = 0; i < range; i++) {
                    if (i % 2 == 0) {
                        //even
                        AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                    } else {
                        //odd
                        AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                    }
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
        } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (difference < 0 ? -1 : 1);
            int range = difference * multiplier;
            if (range > 0) {
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
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
        }

        UpdateMaxHP();
        //Reset to full health and sp
        ResetToFullHP();
        //ResetToFullSP();
        Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
        //if (_playerCharacterItem != null) {
        //    _playerCharacterItem.UpdateMinionItem();
        //}

        //Reset Experience
        //_experience = 0;
        //RecomputeMaxExperience();
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
    //public void AdjustSP(int amount) {
    //    _sp += amount;
    //    _sp = Mathf.Clamp(_sp, 0, _maxSP);
    //}
    public void ResetToFullHP() {
        SetHP(maxHP);
    }
    //public void ResetToFullSP() {
    //    AdjustSP(_maxSP);
    //}
    //private float GetComputedPower() {
    //    float compPower = 0f;
    //    for (int i = 0; i < currentParty.characters.Count; i++) {
    //        compPower += currentParty.characters[i].attackPower;
    //    }
    //    return compPower;
    //}
    public void SetHP(int amount) {
        this._currentHP = amount;
    }
    //Adjust current HP based on specified paramater, but HP must not go below 0
    public virtual void AdjustHP(int amount, bool triggerDeath = false, object source = null) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, maxHP);
        if (marker.hpBarGO.activeSelf) {
            marker.UpdateHP();
        } else {
            if (amount < 0 && _currentHP > 0) {
                //only show hp bar if hp was reduced and hp is greater than 0
                marker.QuickShowHPBar();
            }
        }
        Messenger.Broadcast(Signals.ADJUSTED_HP, this);
        if (IsHealthCriticallyLow()) {
            Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, this, "critically low health");
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
    public void AdjustAttackMod(int amount) {
        attackPowerMod += amount;
        currentAlterEgo.SetAttackPowerMod(attackPowerMod);
    }
    public void AdjustAttackPercentMod(int amount) {
        attackPowerPercentMod += amount;
        currentAlterEgo.SetAttackPowerPercentMod(attackPowerPercentMod);
    }
    public void SetAttackMod(int amount) {//, bool includeAlterEgo = true
        attackPowerMod = amount;
        //if (includeAlterEgo) {
        currentAlterEgo.SetAttackPowerMod(attackPowerMod);
        //}
    }
    public void SetAttackPercentMod(int amount) {//, bool includeAlterEgo = true
        attackPowerPercentMod = amount;
        //if (includeAlterEgo) {
        currentAlterEgo.SetAttackPowerPercentMod(attackPowerPercentMod);
        //}
    }
    public void AdjustMaxHPMod(int amount) {
        int previousMaxHP = maxHP;
        maxHPMod += amount;
        currentAlterEgo.SetMaxHPMod(maxHPMod);
        UpdateMaxHP();
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustMaxHPPercentMod(int amount) {
        int previousMaxHP = maxHP;
        maxHPPercentMod += amount;
        currentAlterEgo.SetMaxHPPercentMod(maxHPPercentMod);
        UpdateMaxHP();
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void SetMaxHPMod(int amount) {//, bool includeAlterEgo = true
        int previousMaxHP = maxHP;
        maxHPMod = amount;
        //if (includeAlterEgo) {
        currentAlterEgo.SetMaxHPMod(maxHPMod);
        //}
        UpdateMaxHP();
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void SetMaxHPPercentMod(int amount) {//, bool includeAlterEgo = true
        int previousMaxHP = maxHP;
        maxHPPercentMod = amount;
        //if (includeAlterEgo) {
        currentAlterEgo.SetMaxHPPercentMod(maxHPPercentMod);
        //}
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
        if (doNotRecoverHP <= 0 && currentHP < maxHP && currentHP > 0) {
            AdjustHP(Mathf.CeilToInt(maxHPPercentage * maxHP));
        }
    }
    public void AdjustSpeedMod(int amount) {
        speedMod += amount;
        currentAlterEgo.SetSpeedMod(speedMod);
    }
    public void AdjustSpeedPercentMod(int amount) {
        speedPercentMod += amount;
        currentAlterEgo.SetSpeedPercentMod(speedPercentMod);
    }
    public void SetSpeedMod(int amount) { //, bool includeAlterEgo = true
        speedMod = amount;
        //if (includeAlterEgo) {
        currentAlterEgo.SetSpeedMod(speedMod);
        //}
    }
    public void SetSpeedPercentMod(int amount) { //, bool includeAlterEgo = true
        speedPercentMod = amount;
        //if (includeAlterEgo) {
        currentAlterEgo.SetSpeedPercentMod(speedPercentMod);
        //}
    }
    public bool IsHealthFull() {
        return _currentHP >= maxHP;
    }
    public bool IsHealthCriticallyLow() {
        //chance based dependent on the character
        return currentHP < (maxHP * 0.2f);
    }
    #endregion

    #region Home
    /// <summary>
    /// Set this character's home area data.(Does nothing else)
    /// </summary>
    /// <param name="newHome">The character's new home</param>
    public void SetHome(Region newHome) {
        Region previousHome = homeRegion;
        homeRegion = newHome;

        //If a character sets his home, add his faction to the factions in the region
        //Subsequently, if character loses his home, remove his faction from the region only if there are no co faction resident in the region anymore
        if (faction != null) {
            if (newHome == null) {
                //If character loses home and he has previous home, remove him faction
                if (previousHome != null) {
                    bool removeFaction = true;
                    for (int i = 0; i < previousHome.residents.Count; i++) {
                        Character resident = previousHome.residents[i];
                        if (resident != this && resident.faction == faction) {
                            removeFaction = false;
                            break;
                        }
                    }
                    if (removeFaction) {
                        previousHome.RemoveFactionHere(faction);
                    }
                }
            } else {
                newHome.AddFactionHere(faction);
            }
        }
    }
    public void SetHomeStructure(Dwelling homeStructure) {
        this.homeStructure = homeStructure;
        currentAlterEgo.SetHomeStructure(homeStructure);
    }
    public bool MigrateHomeTo(Region newHomeRegion, Dwelling homeStructure = null, bool broadcast = true) {
        Region previousHome = null;
        if (homeRegion != null) {
            previousHome = homeRegion;
            homeRegion.RemoveResident(this);
        }
        if (newHomeRegion.AddResident(this, homeStructure)) {
            if (broadcast) {
                Messenger.Broadcast(Signals.CHARACTER_MIGRATED_HOME, this, previousHome, newHomeRegion);
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
    #endregion

    #region Traits
    public ITraitContainer traitContainer { get; private set; }
    public TraitProcessor traitProcessor => TraitManager.characterTraitProcessor;
    public void CreateTraitContainer() {
        traitContainer = new TraitContainer();
    }
    public void CreateInitialTraitsByClass() {
        if (role.roleType != CHARACTER_ROLE.MINION && !(this is Summon)) { //only generate buffs and flaws for non minion characters. Reference: https://trello.com/c/pC9hBih0/2781-demonic-minions-should-not-have-pregenerated-buff-and-flaw-traits
            string[] traitPool = new string[] { "Vigilant", "Diplomatic",
            "Fireproof", "Accident Prone", "Unfaithful", "Drunkard", "Music Lover", "Music Hater", "Ugly", "Blessed", "Nocturnal",
            "Herbalist", "Optimist", "Pessimist", "Fast", "Chaste", "Lustful", "Coward", "Lazy", "Hardworking", "Glutton", "Robust", "Suspicious" , "Inspiring", "Pyrophobic",
            "Narcoleptic", "Hothead", "Evil", "Treacherous", "Disillusioned", "Ambitious", "Authoritative", "Healer"
            };
            //"Kleptomaniac","Curious", "Craftsman"

            List<string> buffTraits = new List<string>();
            List<string> flawTraits = new List<string>();
            List<string> neutralTraits = new List<string>();

            //Categorize traits from trait pool
            for (int i = 0; i < traitPool.Length; i++) {
                string currTraitName = traitPool[i];
                if (TraitManager.Instance.allTraits.ContainsKey(currTraitName)) {
                    Trait trait = TraitManager.Instance.allTraits[currTraitName];
                    if (trait.type == TRAIT_TYPE.BUFF) {
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


            traitContainer.AddTrait(this, chosenBuffTraitName);
            Trait buffTrait = traitContainer.GetNormalTrait<Trait>(chosenBuffTraitName);
            if (buffTrait.mutuallyExclusive != null) {
                buffTraits = Utilities.RemoveElements(buffTraits, buffTrait.mutuallyExclusive); //update buff traits pool to accomodate new trait
                neutralTraits = Utilities.RemoveElements(neutralTraits, buffTrait.mutuallyExclusive); //update neutral traits pool to accomodate new trait
                flawTraits = Utilities.RemoveElements(flawTraits, buffTrait.mutuallyExclusive); //update flaw traits pool to accomodate new trait
            }


            //Second trait is a random buff or neutral trait
            string chosenBuffOrNeutralTraitName = string.Empty;
            if (buffTraits.Count > 0 && neutralTraits.Count > 0) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
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
                if (buffTraits.Count > 0) {
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

            traitContainer.AddTrait(this, chosenBuffOrNeutralTraitName);
            Trait buffOrNeutralTrait = traitContainer.GetNormalTrait<Trait>(chosenBuffOrNeutralTraitName);
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
            traitContainer.AddTrait(this, chosenFlawOrNeutralTraitName);
        }

        traitContainer.AddTrait(this, "Character Trait");
        traitContainer.AddTrait(this, "Flammable");
        //traitContainer.AddTrait(this, "Accident Prone");

        defaultCharacterTrait = traitContainer.GetNormalTrait<Trait>("Character Trait") as CharacterTrait;
    }
    public void AddTraitNeededToBeRemoved(Trait trait) {
        traitsNeededToBeRemoved.Add(trait);
    }
    public void RemoveTraitNeededToBeRemoved(Trait trait) {
        traitsNeededToBeRemoved.Remove(trait);
    }
    #endregion

    #region Minion
    public void SetMinion(Minion minion) {
        if (_minion != null && minion == null) {
            Messenger.Broadcast(Signals.CHARACTER_BECOMES_NON_MINION_OR_SUMMON, this);
        } else if (_minion == null && minion != null) {
            Messenger.Broadcast(Signals.CHARACTER_BECOMES_MINION_OR_SUMMON, this);
        }
        _minion = minion;
        visuals.CreateWholeImageMaterial();
        //UnsubscribeSignals(); //Removed this since character's listeners are not on by default now.
    }
    public void RecruitAsMinion(UnsummonedMinionData minionData) {
        if (stateComponent.currentState != null) {
            stateComponent.ExitCurrentState();
        }
        //else if (stateComponent.stateToDo != null) {
        //    stateComponent.SetStateToDo(null);
        //}

        //ForceCancelAllJobsTargettingCharacter(false, "target became a minion");
        //StopCurrentActionNode(reason: "Became a minion");
        //if (currentActionNode != null && !currentActionNode.cannotCancelAction) {
        //    currentActionNode.StopAction(reason: "Became a minion");
        //}
        Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETTING_CHARACTER, this as IPointOfInterest, "target became a minion");
        CancelAllJobs();

        if (!IsInOwnParty()) {
            currentParty.RemovePOI(this);
        }
        if (ownParty.isCarryingAnyPOI) {
            ownParty.RemoveCarriedPOI();
        }
        ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
        MigrateHomeTo(PlayerManager.Instance.player.playerArea.region);

        //currentArea.RemoveCharacterFromLocation(this.currentParty);
        currentRegion?.RemoveCharacterFromLocation(this);

        needsComponent.ResetFullnessMeter();
        needsComponent.ResetHappinessMeter();
        needsComponent.ResetTirednessMeter();
        //PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(this.currentParty);

        Minion newMinion = PlayerManager.Instance.player.CreateNewMinion(this, false);
        newMinion.character.SetName(minionData.minionName);
        ChangeRace(RACE.DEMON);
        ChangeClass(minionData.className);
        AssignRole(CharacterRole.MINION);
        newMinion.SetRandomResearchInterventionAbilities(minionData.interventionAbilitiesToResearch);
        newMinion.SetCombatAbility(minionData.combatAbility);


        PlayerManager.Instance.player.playerArea.AddCharacterToLocation(this);

        if (PlayerManager.Instance.player.minions.Count < Player.MAX_MINIONS) {
            PlayerManager.Instance.player.AddMinion(newMinion);
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new Minion!", null);
        } else {
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new Minion!", () => PlayerManager.Instance.player.AddMinion(newMinion, true));
        }
    }
    #endregion

    //public void SetPlayerCharacterItem(PlayerCharacterItem item) {
    //    _playerCharacterItem = item;
    //}

    #region Interaction
    //public void AddInteractionType(INTERACTION_TYPE type) {
    //    if (!currentInteractionTypes.Contains(type)) {
    //        currentInteractionTypes.Add(type);
    //    }
    //}
    //public void RemoveInteractionType(INTERACTION_TYPE type) {
    //    currentInteractionTypes.Remove(type);
    //}
    #endregion

    #region Action Planning and Job Processing
    private void DailyGoapProcesses() {
        needsComponent.DailyGoapProcesses();
    }
    protected virtual void OnTickStarted() {
        //What happens every start of tick

        //Check Trap Structure
        trapStructure.IncrementCurrentDuration(1);

        //Out of combat hp recovery
        if (!isDead && !isInCombat) {
            HPRecovery(0.0025f);
        }

        StartTickGoapPlanGeneration();
    }
    protected virtual void OnTickEnded() {
        stateComponent.OnTickEnded();
        EndTickPerformJobs();
    }
    protected void StartTickGoapPlanGeneration() {
        //This is to ensure that this character will not be idle forever
        //If at the start of the tick, the character is not currently doing any action, and is not waiting for any new plans, it means that the character will no longer perform any actions
        //so start doing actions again
        //SetHasAlreadyAskedForPlan(false);
        PlanScheduledFullnessRecovery();
        PlanScheduledTirednessRecovery();
        if (CanPlanGoap()) {
            PerStartTickActionPlanning();
        }
    }
    public bool CanPlanGoap() {
        //If there is no area, it means that there is no inner map, so character must not do goap actions, jobs, and plans
        //characters that cannot witness, cannot plan actions.
        return minion == null && homeRegion.area != null && !isDead && doNotDisturb <= 0 && isStoppedByOtherCharacter <= 0 && canWitness
            && currentActionNode == null && planner.status == GOAP_PLANNING_STATUS.NONE && jobQueue.jobsInQueue.Count <= 0
            && !marker.hasFleePath && stateComponent.currentState == null && IsInOwnParty();
    }

    protected void EndTickPerformJobs() {
        if (CanPerformEndTickJobs()) {
            if (!jobQueue.jobsInQueue[0].ProcessJob()) {
                PerformTopPriorityJob();
            }
        }
    }
    private bool CanPerformEndTickJobs() {
        return minion == null && homeRegion.area != null && !isDead && doNotDisturb <= 0 && isStoppedByOtherCharacter <= 0 && canWitness
            && currentActionNode == null && planner.status == GOAP_PLANNING_STATUS.NONE && jobQueue.jobsInQueue.Count > 0
            && !marker.hasFleePath && stateComponent.currentState == null && IsInOwnParty();
    }
    //public void PlanGoapActions() {
    //    if (!IsInOwnParty() || ownParty.icon.isTravelling || _doNotDisturb > 0 /*|| isWaitingForInteraction > 0 */ || isDead || marker.hasFleePath) {
    //        return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
    //    }
    //    if (stateComponent.currentState != null) {
    //        //Debug.LogWarning(name + " is currently in " + stateComponent.currentState.stateName + " state, can't plan actions!");
    //        return;
    //    }
    //    //if (stateComponent.stateToDo != null) {
    //    //    Debug.LogWarning("Will about to do " + stateComponent.stateToDo.stateName + " state, can't plan actions!");
    //    //    return;
    //    //}
    //    //if(specificAction != null) {
    //    //    WillAboutToDoAction(specificAction);
    //    //    return;
    //    //}
    //    if (allGoapPlans.Count > 0) {
    //        //StopDailyGoapPlanGeneration();
    //        PerformGoapPlans();
    //        //SchedulePerformGoapPlans();
    //    } else {
    //        PlanPerTickGoap();
    //    }
    //}
    protected void PerStartTickActionPlanning() {
        //if (_hasAlreadyAskedForPlan || isDead) {
        //    return;
        //}
        //SetHasAlreadyAskedForPlan(true);
        if (returnedToLife) {
            //characters that have returned to life will just stroll.
            PlanIdleStrollOutside(); //currentStructure
            return;
        }
        string idleLog = OtherIdlePlans();
        PrintLogIfActive(idleLog);
        //if (!PlanJobQueueFirst()) {
        //    if (!PlanFullnessRecoveryActions()) {
        //        if (!PlanTirednessRecoveryActions()) {
        //            if (!PlanHappinessRecoveryActions()) {
        //                if (!PlanWorkActions()) {
        //                    string idleLog = OtherIdlePlans();
        //                    PrintLogIfActive(idleLog);
        //                }
        //            }
        //        }
        //    }
        //}
    }
    //private bool OtherPlanCreations() {
    //    int chance = UnityEngine.Random.Range(0, 100);
    //    if (traitContainer.GetNormalTrait<Trait>("Berserked") != null) {
    //        if (chance < 15) {
    //            Character target = specificLocation.GetRandomCharacterAtLocationExcept(this);
    //            if (target != null) {
    //                StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = target }, target, GOAP_CATEGORY.NONE);
    //                return true;
    //            }
    //        } else {
    //            chance = UnityEngine.Random.Range(0, 100);
    //            if (chance < 15) {
    //                IPointOfInterest target = specificLocation.GetRandomTileObject();
    //                if (target != null) {
    //                    StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DESTROY, conditionKey = target, targetPOI = target }, target, GOAP_CATEGORY.NONE);
    //                    return true;
    //                }
    //            }
    //        }
    //    }
    //    return false;
    //}
    public bool PlanFullnessRecoveryActions() {
        if (doNotDisturb > 0 || !canWitness) {
            return false;
        }
        if (needsComponent.isStarving) {
            if (!jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
                //If there is already a HUNGER_RECOVERY JOB and the character becomes Starving, replace HUNGER_RECOVERY with HUNGER_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem hungerRecoveryJob = jobQueue.GetJob(JOB_TYPE.HUNGER_RECOVERY);
                if (hungerRecoveryJob != null) {
                    //Replace this with Hunger Recovery Starving only if the character is not doing the Hunger Recovery Job already
                    JobQueueItem currJob = currentJob;
                    if (currJob == hungerRecoveryJob) {
                        return false;
                    } else {
                        hungerRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY_STARVING;
                bool triggerGrieving = false;
                Griefstricken griefstricken = traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
                if (griefstricken != null) {
                    triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerGrieving) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, this);
                    //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
                    //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                    //}
                    //else if (GetNormalTrait<Trait>("Cannibal") != null) {
                    //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                    //}
                    //job.SetCancelOnFail(true);
                    jobQueue.AddJobInQueue(job);
                } else {
                    griefstricken.TriggerGrieving();
                }
                return true;
            }
        } else if (needsComponent.isHungry) {
            if (UnityEngine.Random.Range(0, 2) == 0 && traitContainer.GetNormalTrait<Trait>("Glutton") != null) {
                if (!jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY)) {
                    JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY;
                    bool triggerGrieving = false;
                    Griefstricken griefstricken = traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
                    if (griefstricken != null) {
                        triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
                    }
                    if (!triggerGrieving) {
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, this);
                        //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
                        //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                        //}
                        //else if (GetNormalTrait<Trait>("Cannibal") != null) {
                        //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                        //}
                        //job.SetCancelOnFail(true);
                        jobQueue.AddJobInQueue(job);
                    } else {
                        griefstricken.TriggerGrieving();
                    }
                    return true;
                }
            }
        }
        return false;
    }
    public bool PlanTirednessRecoveryActions() {
        if (doNotDisturb > 0 || !canWitness) {
            return false;
        }
        if (needsComponent.isExhausted) {
            if (!jobQueue.HasJob(JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                //If there is already a TIREDNESS_RECOVERY JOB and the character becomes Exhausted, replace TIREDNESS_RECOVERY with TIREDNESS_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem tirednessRecoveryJob = jobQueue.GetJob(JOB_TYPE.TIREDNESS_RECOVERY);
                if (tirednessRecoveryJob != null) {
                    //Replace this with Tiredness Recovery Exhausted only if the character is not doing the Tiredness Recovery Job already
                    JobQueueItem currJob = currentJob;
                    if (currJob == tirednessRecoveryJob) {
                        return false;
                    } else {
                        tirednessRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
                bool triggerSpooked = false;
                Spooked spooked = traitContainer.GetNormalTrait<Trait>("Spooked") as Spooked;
                if (spooked != null) {
                    triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerSpooked) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, this);
                    //job.SetCancelOnFail(true);
                    jobQueue.AddJobInQueue(job);
                } else {
                    spooked.TriggerFeelingSpooked();
                }
                return true;
            }
        }
        return false;
    }
    public bool PlanHappinessRecoveryActions() {
        if (doNotDisturb > 0 || !canWitness) {
            return false;
        }
        if (needsComponent.isForlorn) {
            if (!jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                //If there is already a HUNGER_RECOVERY JOB and the character becomes Starving, replace HUNGER_RECOVERY with HUNGER_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem happinessRecoveryJob = jobQueue.GetJob(JOB_TYPE.HAPPINESS_RECOVERY);
                if (happinessRecoveryJob != null) {
                    //Replace this with Hunger Recovery Starving only if the character is not doing the Hunger Recovery Job already
                    JobQueueItem currJob = currentJob;
                    if (currJob == happinessRecoveryJob) {
                        return false;
                    } else {
                        happinessRecoveryJob.CancelJob();
                    }
                }
                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = traitContainer.GetNormalTrait<Trait>("Heartbroken") as Heartbroken;
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerBrokenhearted) {
                    Hardworking hardworking = traitContainer.GetNormalTrait<Trait>("Hardworking") as Hardworking;
                    if (hardworking != null) {
                        bool isPlanningRecoveryProcessed = false;
                        if (hardworking.ProcessHardworkingTrait(this, ref isPlanningRecoveryProcessed)) {
                            return isPlanningRecoveryProcessed;
                        }
                    }
                    JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY_FORLORN;
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, this);
                    //job.SetCancelOnFail(true);
                    jobQueue.AddJobInQueue(job);
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
                return true;
            }
        } else if (needsComponent.isLonely) {
            if (!jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY;
                int chance = UnityEngine.Random.Range(0, 100);
                int value = 0;
                TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick(this);
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
                    bool triggerBrokenhearted = false;
                    Heartbroken heartbroken = traitContainer.GetNormalTrait<Trait>("Heartbroken") as Heartbroken;
                    if (heartbroken != null) {
                        triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                    }
                    if (!triggerBrokenhearted) {
                        Hardworking hardworking = traitContainer.GetNormalTrait<Trait>("Hardworking") as Hardworking;
                        if (hardworking != null) {
                            bool isPlanningRecoveryProcessed = false;
                            if (hardworking.ProcessHardworkingTrait(this, ref isPlanningRecoveryProcessed)) {
                                return isPlanningRecoveryProcessed;
                            }
                        }
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, this);
                        //job.SetCancelOnFail(true);
                        jobQueue.AddJobInQueue(job);
                    } else {
                        heartbroken.TriggerBrokenhearted();
                    }
                    return true;
                }
            }
        }
        return false;
    }
    private void PlanScheduledFullnessRecovery() {
        if (!needsComponent.hasForcedFullness && needsComponent.fullnessForcedTick != 0 && GameManager.Instance.tick >= needsComponent.fullnessForcedTick && _doNotDisturb <= 0 && needsComponent.doNotGetHungry <= 0) {
            if (!jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
                JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY;
                if (needsComponent.isStarving) {
                    jobType = JOB_TYPE.HUNGER_RECOVERY_STARVING;
                }
                bool triggerGrieving = false;
                Griefstricken griefstricken = traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
                if (griefstricken != null) {
                    triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerGrieving) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, this);
                    //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
                    //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                    //}
                    //else if (GetNormalTrait<Trait>("Cannibal") != null) {
                    //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                    //}
                    //job.SetCancelOnFail(true);
                    //bool willNotProcess = _numOfWaitingForGoapThread > 0 || !IsInOwnParty() || isDefender || isWaitingForInteraction > 0 /*|| stateComponent.stateToDo != null*/;
                    jobQueue.AddJobInQueue(job); //, !willNotProcess
                } else {
                    griefstricken.TriggerGrieving();
                }
            }
            needsComponent.hasForcedFullness = true;
            needsComponent.SetFullnessForcedTick();
        }
    }
    private void PlanScheduledTirednessRecovery() {
        if (!needsComponent.hasForcedTiredness && needsComponent.tirednessForcedTick != 0 && GameManager.Instance.tick >= needsComponent.tirednessForcedTick && _doNotDisturb <= 0 && needsComponent.doNotGetTired <= 0) {
            if (!jobQueue.HasJob(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY;
                if (needsComponent.isExhausted) {
                    jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
                }

                bool triggerSpooked = false;
                Spooked spooked = traitContainer.GetNormalTrait<Trait>("Spooked") as Spooked;
                if (spooked != null) {
                    triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerSpooked) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, this);
                    //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = this });
                    //job.SetCancelOnFail(true);
                    needsComponent.sleepScheduleJobID = job.id;
                    //bool willNotProcess = _numOfWaitingForGoapThread > 0 || !IsInOwnParty() || isDefender || isWaitingForInteraction > 0 /*|| stateComponent.stateToDo != null*/;
                    jobQueue.AddJobInQueue(job); //, !willNotProcess
                } else {
                    spooked.TriggerFeelingSpooked();
                }
            }
            needsComponent.hasForcedTiredness = true;
            needsComponent.SetTirednessForcedTick();
        }
        //If a character current sleep ticks is less than the default, this means that the character already started sleeping but was awaken midway that is why he/she did not finish the allotted sleeping time
        //When this happens, make sure to queue tiredness recovery again so he can finish the sleeping time
        else if ((needsComponent.hasCancelledSleepSchedule || needsComponent.currentSleepTicks < CharacterManager.Instance.defaultSleepTicks) && _doNotDisturb <= 0) {
            if (!jobQueue.HasJob(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY;
                if (needsComponent.isExhausted) {
                    jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
                }
                bool triggerSpooked = false;
                Spooked spooked = traitContainer.GetNormalTrait<Trait>("Spooked") as Spooked;
                if (spooked != null) {
                    triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerSpooked) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, this);
                    //job.SetCancelOnFail(true);
                    needsComponent.sleepScheduleJobID = job.id;
                    //bool willNotProcess = _numOfWaitingForGoapThread > 0 || !IsInOwnParty() || isDefender || isWaitingForInteraction > 0
                    //    || stateComponent.currentState != null || stateComponent.stateToDo != null;
                    jobQueue.AddJobInQueue(job); //!willNotProcess
                } else {
                    spooked.TriggerFeelingSpooked();
                }
            }
            needsComponent.SetHasCancelledSleepSchedule(false);
        }
    }
    /// <summary>
    /// Make this character plan a starving fullness recovery job, regardless of actual
    /// fullness level. NOTE: This will also cancel any existing fullness recovery jobs
    /// <param name="jobType">The type of job to create. Default is HUNGER_RECOVERY_STARVING but can set other job type to prevent overriding.</param>
    /// </summary>
    public void TriggerFlawFullnessRecovery() {
        //if (jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
        //    jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING);
        //}
        bool triggerGrieving = false;
        Griefstricken griefstricken = traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
        if (griefstricken != null) {
            triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
        }
        if (!triggerGrieving) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, this);
            //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
            //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
            //}
            //else if (GetNormalTrait<Trait>("Cannibal") != null) {
            //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
            //}
            jobQueue.AddJobInQueue(job);
        } else {
            griefstricken.TriggerGrieving();
        }
    }
    public bool PlanWorkActions() { //ref bool hasAddedToGoapPlans
        if (isAtHomeRegion && homeRegion.area != null && isPartOfHomeFaction) { //&& this.faction.id != FactionManager.Instance.neutralFaction.id
            bool triggerLazy = false;
            Lazy lazy = traitContainer.GetNormalTrait<Trait>("Lazy") as Lazy;
            if (lazy != null) {
                triggerLazy = UnityEngine.Random.Range(0, 100) < 35;
            }
            if (triggerLazy) {
                if (lazy.TriggerLazy()) {
                    return true;
                } else {
                    PrintLogIfActive(GameManager.Instance.TodayLogString() + "Triggered LAZY happiness recovery but " + name + " already has that job type in queue and will not do it anymore!");
                }
            }
            if (!homeRegion.area.AddFirstUnassignedJobToCharacterJob(this)) {
                if (faction != null && faction.activeQuest != null) {
                    return faction.activeQuest.AddFirstUnassignedJobToCharacterJob(this);
                }
                return false;
            } else {
                return true;
            }
        } else {
            return false;
        }
        //if (!jobQueue.ProcessFirstJobInQueue()) {
        //    if (isAtHomeRegion && isPartOfHomeFaction) { //&& this.faction.id != FactionManager.Instance.neutralFaction.id
        //        bool triggerLazy = false;
        //        Lazy lazy = traitContainer.GetNormalTrait<Trait>("Lazy") as Lazy;
        //        if (lazy != null) {
        //            triggerLazy = UnityEngine.Random.Range(0, 100) < 35;
        //        }
        //        if (triggerLazy) {
        //            if (!lazy.TriggerLazy()) {
        //                PrintLogIfActive(GameManager.Instance.TodayLogString() + "Triggered LAZY happiness recovery but " + name + " already has that job type in queue and will not do it anymore!");
        //            }
        //        } else {
        //            if (!homeArea.jobQueue.ProcessFirstJobInQueue(this)) {
        //                if (faction != null && faction.activeQuest != null) {
        //                    return faction.activeQuest.jobQueue.ProcessFirstJobInQueue(this);
        //                }
        //                return false;
        //            } else {
        //                return true;
        //            }
        //        }
        //    } else {
        //        return false;
        //    }
        //}
    }
    public bool PlanJobQueueFirst() {
        if (!needsComponent.isStarving && !needsComponent.isExhausted && !needsComponent.isForlorn) {
            return PlanWorkActions();
        }
        return false;
    }
    public bool PlanIdleStroll(LocationStructure targetStructure, LocationGridTile targetTile = null) {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.STROLL, CHARACTER_STATE.STROLL, this);
        jobQueue.AddJobInQueue(job);
        //if (currentStructure == targetStructure) {
        //    stateComponent.SwitchToState(CHARACTER_STATE.STROLL);
        //} else {
        //    MoveToAnotherStructure(targetStructure, targetTile, null, () => stateComponent.SwitchToState(CHARACTER_STATE.STROLL));
        //}
        return true;
    }
    public bool PlanIdleStrollOutside() {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.STROLL, CHARACTER_STATE.STROLL_OUTSIDE, this);
        jobQueue.AddJobInQueue(job);
        //if (currentStructure == targetStructure) {
        //    stateComponent.SwitchToState(CHARACTER_STATE.STROLL_OUTSIDE);
        //} else {
        //    MoveToAnotherStructure(targetStructure, targetTile, null, () => stateComponent.SwitchToState(CHARACTER_STATE.STROLL_OUTSIDE));
        //}
        return true;
    }
    public bool PlanIdleReturnHome() { //bool forceDoAction = false
        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], this, this, null, 0);
        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, this);
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.RETURN_HOME, this, this);
        goapPlan.SetDoNotRecalculate(true);
        job.SetCannotBePushedBack(true);
        job.SetAssignedPlan(goapPlan);
        jobQueue.AddJobInQueue(job);
        //if (GetTrait("Berserker") != null) {
        //    //Return home becomes stroll if the character has berserker trait
        //    PlanIdleStroll(currentStructure);
        //} else {
        //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.RETURN_HOME, this, this);
        //goapAction.SetTargetStructure();
        //GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        //GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        //goapPlan.ConstructAllNodes();
        ////AddPlan(goapPlan, true);
        //AddPlan(goapPlan);
        //if (forceDoAction) {
        //    PerformTopPriorityJob();
        //}
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
        if (!isFactionless) {
            CreatePersonalJobs();
        }
        string classIdlePlanLog = CharacterManager.Instance.RunCharacterIdlePlan(this);
        log += "\n" + classIdlePlanLog;
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
    public void PlanIdle(INTERACTION_TYPE type, IPointOfInterest target, object[] otherData = null) {
        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[type], this, target, otherData, 0);
        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, target);
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, type, target, this);
        goapPlan.SetDoNotRecalculate(true);
        job.SetCannotBePushedBack(true);
        job.SetAssignedPlan(goapPlan);
        jobQueue.AddJobInQueue(job);

        //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(type, this, target);
        //GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        //GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        //goapPlan.ConstructAllNodes();
        //AddPlan(goapPlan);
        //PlanGoapActions(goapAction);
    }
    public void PlanIdle(GoapEffect effect, IPointOfInterest target) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, effect, target, this);
        jobQueue.AddJobInQueue(job);
        //if (effect.targetPOI != null && effect.targetPOI != this) {
        //    AddAwareness(effect.targetPOI);
        //}
        //StartGOAP(effect, effect.targetPOI, GOAP_CATEGORY.IDLE);

        //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(type, this, target);
        //GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        //GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        //goapPlan.ConstructAllNodes();
        //AddPlan(goapPlan);
        //PlanGoapActions(goapAction);
    }
    public void PlanAction(JOB_TYPE jobType, INTERACTION_TYPE type, IPointOfInterest target, object[] otherData = null) {
        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[type], this, target, otherData, 0);
        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, target);
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, type, target, this);
        goapPlan.SetDoNotRecalculate(true);
        job.SetCannotBePushedBack(true);
        job.SetAssignedPlan(goapPlan);
        jobQueue.AddJobInQueue(job);
    }
    public Character GetParalyzedOrCatatonicCharacterToCheckOut() {
        List<Character> charactersWithRel = relationshipContainer.relationships.Keys.Where(x => x is AlterEgoData).Select(x => (x as AlterEgoData).owner).ToList();
        if (charactersWithRel.Count > 0) {
            List<Character> positiveCharactersWithParalyzedOrCatatonic = new List<Character>();
            for (int i = 0; i < charactersWithRel.Count; i++) {
                Character character = charactersWithRel[i];
                if (relationshipContainer.GetRelationshipEffectWith(character.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
                    Trait trait = character.traitContainer.GetNormalTrait<Trait>("Paralyzed", "Catatonic");
                    if (trait != null) {
                        if (trait is Paralyzed && (trait as Paralyzed).charactersThatKnow.Contains(this)) {
                            positiveCharactersWithParalyzedOrCatatonic.Add(character);
                        } else if (trait is Catatonic && (trait as Catatonic).charactersThatKnow.Contains(this)) {
                            positiveCharactersWithParalyzedOrCatatonic.Add(character);
                        }
                    }
                }
            }
            if (positiveCharactersWithParalyzedOrCatatonic.Count > 0) {
                return positiveCharactersWithParalyzedOrCatatonic[UnityEngine.Random.Range(0, positiveCharactersWithParalyzedOrCatatonic.Count)];
            }
        }
        return null;
    }
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
    //public bool ChatCharacter(Character targetCharacter, int chatChance) {
    //    if (targetCharacter.isDead
    //        || !targetCharacter.canWitness
    //        || !canWitness
    //        || targetCharacter.role.roleType == CHARACTER_ROLE.BEAST
    //        || role.roleType == CHARACTER_ROLE.BEAST
    //        || targetCharacter.faction == PlayerManager.Instance.player.playerFaction
    //        || faction == PlayerManager.Instance.player.playerFaction
    //        || targetCharacter.characterClass.className == "Zombie"
    //        || characterClass.className == "Zombie"
    //        || (currentActionNode != null && currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
    //        || (targetCharacter.currentActionNode != null && targetCharacter.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)) {
    //        return false;
    //    }
    //    if (!IsHostileWith(targetCharacter)) {
    //        int roll = UnityEngine.Random.Range(0, 100);
    //        int chance = chatChance;
    //        if (roll < chance) {
    //            int chatPriority = InteractionManager.Instance.GetInitialPriority(JOB_TYPE.CHAT);
    //            JobQueueItem currJob = currentJob;
    //            if (currJob != null) {
    //                if (chatPriority >= currJob.priority) {
    //                    return false;
    //                }
    //            }
    //            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.CHAT_CHARACTER], this, targetCharacter, null, 0);
    //            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetCharacter);
    //            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CHAT, INTERACTION_TYPE.CHAT_CHARACTER, targetCharacter, this);
    //            goapPlan.SetDoNotRecalculate(true);
    //            job.SetCannotBePushedBack(true);
    //            job.SetAssignedPlan(goapPlan);
    //            jobQueue.AddJobInQueue(job);
    //            return true;
    //            //ChatCharacter chatAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.CHAT_CHARACTER, this, targetCharacter) as ChatCharacter;
    //            //chatAction.Perform();
    //        }
    //    }
    //    return false;
    //}
    public float GetFlirtationWeightWith(Character targetCharacter, IRelationshipData relData, params CHARACTER_MOOD[] moods) {
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
            //positiveFlirtationWeight += 10 * relData.flirtationCount; //TODO
        }
        //x2 all positive modifiers per Drunk
        if (traitContainer.GetNormalTrait<Trait>("Drunk") != null) {
            positiveFlirtationWeight *= 2;
        }
        if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Drunk") != null) {
            positiveFlirtationWeight *= 2;
        }

        Unfaithful unfaithful = traitContainer.GetNormalTrait<Trait>("Unfaithful") as Unfaithful;
        //x0.5 all positive modifiers per negative relationship
        if (relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
            positiveFlirtationWeight *= 0.5f;
        }
        if (targetCharacter.relationshipContainer.GetRelationshipEffectWith(this.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
            positiveFlirtationWeight *= 0.5f;
        }
        //x0.1 all positive modifiers per sexually incompatible
        if (!RelationshipManager.Instance.IsSexuallyCompatibleOneSided(this, targetCharacter)) {
            positiveFlirtationWeight *= 0.1f;
        }
        // x6 if initiator is Unfaithful and already has a lover
        else if (unfaithful != null && (relData == null || !relData.HasRelationship(RELATIONSHIP_TRAIT.LOVER))) {
            positiveFlirtationWeight *= 6f;
            positiveFlirtationWeight *= unfaithful.affairChanceMultiplier;
        }
        if (!RelationshipManager.Instance.IsSexuallyCompatibleOneSided(targetCharacter, this)) {
            positiveFlirtationWeight *= 0.1f;
        }
        bool thisIsUgly = traitContainer.GetNormalTrait<Trait>("Ugly") != null;
        bool otherIsUgly = targetCharacter.traitContainer.GetNormalTrait<Trait>("Ugly") != null;
        if (thisIsUgly != otherIsUgly) { //if at least one of the characters are ugly
            positiveFlirtationWeight *= 0.75f;
        }
        return positiveFlirtationWeight + negativeFlirtationWeight;
    }
    public float GetBecomeLoversWeightWith(Character targetCharacter, IRelationshipData relData, params CHARACTER_MOOD[] moods) {
        float positiveWeight = 0;
        float negativeWeight = 0;
        if (relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) != RELATIONSHIP_EFFECT.NEGATIVE && targetCharacter.relationshipContainer.GetRelationshipEffectWith(this.currentAlterEgo) != RELATIONSHIP_EFFECT.NEGATIVE
            && relationshipValidator.CanHaveRelationship(this.currentAlterEgo, targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) && targetCharacter.relationshipValidator.CanHaveRelationship(targetCharacter.currentAlterEgo, this.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)
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
                //positiveWeight += 30 * relData.flirtationCount;//TODO
            }
            //x2 all positive modifiers per Drunk
            if (traitContainer.GetNormalTrait<Trait>("Drunk") != null) {
                positiveWeight *= 2;
            }
            if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Drunk") != null) {
                positiveWeight *= 2;
            }
            //x0.1 all positive modifiers per sexually incompatible
            if (!RelationshipManager.Instance.IsSexuallyCompatibleOneSided(this, targetCharacter)) {
                positiveWeight *= 0.1f;
            }
            if (!RelationshipManager.Instance.IsSexuallyCompatibleOneSided(targetCharacter, this)) {
                positiveWeight *= 0.1f;
            }
            //x0 if a character is a beast
            //added to initial checking instead.

            bool thisIsUgly = traitContainer.GetNormalTrait<Trait>("Ugly") != null;
            bool otherIsUgly = targetCharacter.traitContainer.GetNormalTrait<Trait>("Ugly") != null;
            if (thisIsUgly != otherIsUgly) { //if at least one of the characters are ugly
                positiveWeight *= 0.75f;
            }
        }
        return positiveWeight + negativeWeight;
    }
    public float GetBecomeParamoursWeightWith(Character targetCharacter, IRelationshipData relData, params CHARACTER_MOOD[] moods) {
        //**if they dont have a negative relationship and at least one of them has a lover, they may become paramours**
        float positiveWeight = 0;
        float negativeWeight = 0;
        if (relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) != RELATIONSHIP_EFFECT.NEGATIVE && targetCharacter.relationshipContainer.GetRelationshipEffectWith(this.currentAlterEgo) != RELATIONSHIP_EFFECT.NEGATIVE
            && relationshipValidator.CanHaveRelationship(this.currentAlterEgo, targetCharacter.currentAlterEgo,  RELATIONSHIP_TRAIT.PARAMOUR) && targetCharacter.relationshipValidator.CanHaveRelationship(targetCharacter.currentAlterEgo, this.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)
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
                //positiveWeight += 50 * relData.flirtationCount; //TODO
            }
            Unfaithful unfaithful = traitContainer.GetNormalTrait<Trait>("Unfaithful") as Unfaithful;
            //x2 all positive modifiers per Drunk
            if (traitContainer.GetNormalTrait<Trait>("Drunk") != null) {
                positiveWeight *= 2.5f;
            }
            if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Drunk") != null) {
                positiveWeight *= 2.5f;
            }
            //x0.1 all positive modifiers per sexually incompatible
            if (!RelationshipManager.Instance.IsSexuallyCompatibleOneSided(this, targetCharacter)) {
                positiveWeight *= 0.1f;
            }
            // x4 if initiator is Unfaithful and already has a lover
            else if (unfaithful != null && (relData == null || !relData.HasRelationship(RELATIONSHIP_TRAIT.LOVER))) {
                positiveWeight *= 4f;
                positiveWeight *= unfaithful.affairChanceMultiplier;
            }

            if (!RelationshipManager.Instance.IsSexuallyCompatibleOneSided(targetCharacter, this)) {
                positiveWeight *= 0.1f;
            }
            Relatable lover = relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TRAIT.LOVER).FirstOrDefault();
            //x3 all positive modifiers if character considers lover as Enemy
            if (lover != null && relationshipContainer.HasRelationshipWith(lover, RELATIONSHIP_TRAIT.ENEMY)) {
                positiveWeight *= 3f;
            }
            if (relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.RELATIVE)) {
                positiveWeight *= 0.01f;
            }
            if (lover != null && lover is ITraitable && (lover as ITraitable).traitContainer.GetNormalTrait<Trait>("Ugly") != null) { //if lover is ugly
                positiveWeight += positiveWeight * 0.75f;
            }
            //x0 if a character has a lover and does not have the Unfaithful trait
            if ((relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TRAIT.LOVER).Count > 0 && traitContainer.GetNormalTrait<Trait>("Unfaithful") == null) 
                || (targetCharacter.relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TRAIT.LOVER).Count > 0 && targetCharacter.traitContainer.GetNormalTrait<Trait>("Unfaithful") == null)) {
                positiveWeight *= 0;
                negativeWeight *= 0;
            }
            //x0 if a character is a beast
            //added to initial checking instead.
        }
        return positiveWeight + negativeWeight;
    }
    //public void EndChatCharacter() {
    //    SetIsChatting(false);
    //    //targetCharacter.SetIsChatting(false);
    //    //SetIsFlirting(false);
    //    //targetCharacter.SetIsFlirting(false);
    //    marker.UpdateActionIcon();
    //    //targetCharacter.marker.UpdateActionIcon();
    //}
    public void SetIsChatting(bool state) {
        _isChatting = state;
        if(marker != null) {
            marker.UpdateActionIcon();
        }
    }
    //public void SetIsFlirting(bool state) {
    //    _isFlirting = state;
    //}
    public void AddAdvertisedAction(INTERACTION_TYPE type) {
        advertisedActions.Add(type);
    }
    public void RemoveAdvertisedAction(INTERACTION_TYPE type) {
        advertisedActions.Remove(type);
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
            token.SetCarriedByCharacter(this);
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
            token.SetCarriedByCharacter(null);
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
    private bool RemoveToken(int index) {
        SpecialToken token = items[index];
        if (token != null) {
            items.RemoveAt(index);
            Messenger.Broadcast(Signals.CHARACTER_LOST_ITEM, token, this);
            return true;
        }
        return false;
    }
    public void DropToken(SpecialToken token, ILocation location, LocationStructure structure, LocationGridTile gridTile = null, bool clearOwner = true) {
        if (UnobtainToken(token)) {
            if (token.specialTokenType.CreatesObjectWhenDropped()) {
                location.AddSpecialTokenToLocation(token, structure, gridTile);
            }
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
        if (token.carriedByCharacter != null) {
            token.carriedByCharacter.UnobtainToken(token);
        }
        if (ObtainToken(token, changeCharacterOwnership)) {
            if (token.gridTileLocation != null) {
                token.gridTileLocation.structure.location.RemoveSpecialTokenFromLocation(token);
            }
        }
    }
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
        if(homeStructure == null) {
            Debug.LogError(name + " error in GetItemsOwned no homestructure!");
        }
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
    #endregion

    #region Share Intel
    public List<string> ShareIntel(Intel intel) {
        List<string> dialogReactions = new List<string>();
        //if (intel is EventIntel) {
        //    EventIntel ei = intel as EventIntel;
        //    if (ei.action.currentState != null && ei.action.currentState.shareIntelReaction != null) {
        //        dialogReactions.AddRange(ei.action.currentState.shareIntelReaction.Invoke(this, ei, SHARE_INTEL_STATUS.INFORMED));
        //    }
        //    //if the determined reactions list is empty, check the default reactions
        //    if (dialogReactions.Count == 0) {
        //        bool doesNotConcernMe = false;
        //        //If the event's actor and target do not have any relationship with the recipient and are not part of his faction, 
        //        //and if no item involved is owned by the recipient: "This does not concern me."
        //        if (!this.relationshipContainer.HasRelationshipWith(ei.action.actorAlterEgo)
        //            && ei.action.actor.faction != this.faction) {
        //            if (ei.action.poiTarget is Character) {
        //                Character otherCharacter = ei.action.poiTarget as Character;
        //                if (!this.relationshipContainer.HasRelationshipWith(ei.action.poiTargetAlterEgo)
        //                    && otherCharacter.faction != this.faction) {
        //                    doesNotConcernMe = true;
        //                }
        //            } else if (ei.action.poiTarget is TileObject) {
        //                TileObject obj = ei.action.poiTarget as TileObject;
        //                if (!obj.IsOwnedBy(this)) {
        //                    doesNotConcernMe = true;
        //                }
        //            } else if (ei.action.poiTarget is SpecialToken) {
        //                SpecialToken obj = ei.action.poiTarget as SpecialToken;
        //                if (obj.characterOwner != this) {
        //                    doesNotConcernMe = true;
        //                }
        //            }
        //        }

        //        if (ei.action.actor == this) {
        //            //If the actor and the recipient is the same: "I know what I did."
        //            dialogReactions.Add("I know what I did.");
        //        } else {
        //            if (doesNotConcernMe) {
        //                //The following events are too unimportant to merit any meaningful response: "What will I do with this random tidbit?"
        //                //-character picked up an item(not stealing)
        //                //-character prayed, daydreamed, played
        //                //- character slept
        //                //- character mined or chopped wood
        //                switch (ei.action.goapType) {
        //                    case INTERACTION_TYPE.PICK_UP:
        //                    case INTERACTION_TYPE.PRAY:
        //                    case INTERACTION_TYPE.DAYDREAM:
        //                    case INTERACTION_TYPE.PLAY:
        //                    case INTERACTION_TYPE.SLEEP:
        //                    case INTERACTION_TYPE.SLEEP_OUTSIDE:
        //                    case INTERACTION_TYPE.MINE:
        //                    case INTERACTION_TYPE.CHOP_WOOD:
        //                        dialogReactions.Add("What will I do with this random tidbit?");
        //                        break;
        //                    default:
        //                        dialogReactions.Add("This does not concern me.");
        //                        break;
        //                }

        //            } else {
        //                //Otherwise: "A proper response to this information has not been implemented yet."
        //                dialogReactions.Add("A proper response to this information has not been implemented yet.");
        //            }
        //        }
        //    }
        //    CreateInformedEventLog(intel.intelLog.node, false);
        //}
        //PlayerManager.Instance.player.RemoveIntel(intel);
        dialogReactions.Add("A proper response to this information has not been implemented yet.");
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
    public void LogAwarenessList() {
        string log = "--------------AWARENESS LIST OF " + name + "-----------------";
        foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> kvp in currentRegion.awareness) {
            log += "\n" + kvp.Key.ToString() + ": ";
            for (int i = 0; i < kvp.Value.Count; i++) {
                if (i > 0) {
                    log += ", ";
                }
                log += kvp.Value[i].ToString();
            }
        }
        Debug.Log(log);
    }
    #endregion

    #region Point Of Interest
    //Returns the chosen action for the plan
    public GoapAction AdvertiseActionsToActor(Character actor, GoapEffect precondition, Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost) {
        GoapAction chosenAction = null;
        if (advertisedActions != null && advertisedActions.Count > 0) {//&& IsAvailable()
            bool isCharacterAvailable = IsAvailable();
            //List<GoapAction> usableActions = new List<GoapAction>();
            GoapAction lowestCostAction = null;
            int currentLowestCost = 0;
            for (int i = 0; i < advertisedActions.Count; i++) {
                INTERACTION_TYPE currType = advertisedActions[i];
                GoapAction action = InteractionManager.Instance.goapActionData[currType];
                if (!isCharacterAvailable && !action.canBeAdvertisedEvenIfActorIsUnavailable) {
                    //if this character is not available, check if the current action type can be advertised even when the character is inactive.
                    continue; //skip
                }
                if (RaceManager.Instance.CanCharacterDoGoapAction(actor, currType)) {
                    object[] data = null;
                    if (otherData != null) {
                        if (otherData.ContainsKey(currType)) {
                            data = otherData[currType];
                        } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                            data = otherData[INTERACTION_TYPE.NONE];
                        }
                    }
                    //object[] otherActionData = null;
                    //if (otherData.ContainsKey(currType)) {
                    //    otherActionData = otherData[currType];
                    //}
                    if (action.CanSatisfyRequirements(actor, this, data)
                        && action.WillEffectsSatisfyPrecondition(precondition, actor, this, data)) { //&& InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(currType, actor, this, data)
                        int actionCost = action.GetCost(actor, this, data);
                        if (lowestCostAction == null|| actionCost < currentLowestCost) {
                            lowestCostAction = action;
                            currentLowestCost = actionCost;
                        }
                        //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                        //if (goapAction != null) {
                        //    if (data != null) {
                        //        goapAction.InitializeOtherData(data);
                        //    }
                        //    usableActions.Add(goapAction);
                        //} else {
                        //    throw new System.Exception("Goap action " + currType.ToString() + " is null!");
                        //}
                    }
                }
            }
            cost = currentLowestCost;
            chosenAction = lowestCostAction;
            //return usableActions;
        }
        return chosenAction;
    }
    public bool CanAdvertiseActionToActor(Character actor, GoapAction action, Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost) {
        if((IsAvailable() || action.canBeAdvertisedEvenIfActorIsUnavailable) 
            && advertisedActions != null && advertisedActions.Contains(action.goapType)
            && RaceManager.Instance.CanCharacterDoGoapAction(actor, action.goapType)) {
            object[] data = null;
            if (otherData != null) {
                if (otherData.ContainsKey(action.goapType)) {
                    data = otherData[action.goapType];
                } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                    data = otherData[INTERACTION_TYPE.NONE];
                }
            }
            if (action.CanSatisfyRequirements(actor, this, data)) {
                cost = action.GetCost(actor, this, data);
                return true;
            }
        }
        return false;
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
    public void OnPlacePOI() { /*FOR INTERFACE ONLY*/ }
    public void OnDestroyPOI() { /*FOR INTERFACE ONLY*/ }
    public virtual bool IsStillConsideredPartOfAwarenessByCharacter(Character character) {
        if(character.currentRegion == currentRegion) {
            if (!isDead && currentParty.icon.isTravellingOutside) {
                return false;
            }
            return true;
        }
        return false;
    }
    #endregion

    #region Goap
    //public void SetNumWaitingForGoapThread(int amount) {
    //    _numOfWaitingForGoapThread = amount;
    //}
    public void ConstructInitialGoapAdvertisementActions() {
        //poiGoapActions = new List<INTERACTION_TYPE>();
        advertisedActions.Add(INTERACTION_TYPE.ASSAULT);
        advertisedActions.Add(INTERACTION_TYPE.RESTRAIN_CHARACTER);
        //poiGoapActions.Add(INTERACTION_TYPE.STROLL);
        advertisedActions.Add(INTERACTION_TYPE.DAYDREAM);
        advertisedActions.Add(INTERACTION_TYPE.SLEEP_OUTSIDE);
        advertisedActions.Add(INTERACTION_TYPE.PRAY);
        //poiGoapActions.Add(INTERACTION_TYPE.EXPLORE);
        //poiGoapActions.Add(INTERACTION_TYPE.PATROL);
        //poiGoapActions.Add(INTERACTION_TYPE.TRAVEL);
        //poiGoapActions.Add(INTERACTION_TYPE.RETURN_HOME_LOCATION);
        //poiGoapActions.Add(INTERACTION_TYPE.HUNT_ACTION);
        advertisedActions.Add(INTERACTION_TYPE.PLAY);
        advertisedActions.Add(INTERACTION_TYPE.JUDGE_CHARACTER);
        advertisedActions.Add(INTERACTION_TYPE.CURSE_CHARACTER);
        advertisedActions.Add(INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER);
        advertisedActions.Add(INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE);
        advertisedActions.Add(INTERACTION_TYPE.BURY_CHARACTER);
        advertisedActions.Add(INTERACTION_TYPE.INVITE);
        advertisedActions.Add(INTERACTION_TYPE.MAKE_LOVE);
        advertisedActions.Add(INTERACTION_TYPE.TANTRUM);
        //advertisedActions.Add(INTERACTION_TYPE.BREAK_UP);
        advertisedActions.Add(INTERACTION_TYPE.PUKE);
        advertisedActions.Add(INTERACTION_TYPE.SEPTIC_SHOCK);
        advertisedActions.Add(INTERACTION_TYPE.CARRY);
        advertisedActions.Add(INTERACTION_TYPE.DROP);
        advertisedActions.Add(INTERACTION_TYPE.RESOLVE_CONFLICT);
        advertisedActions.Add(INTERACTION_TYPE.ASK_TO_STOP_JOB);
        advertisedActions.Add(INTERACTION_TYPE.STRANGLE);
        advertisedActions.Add(INTERACTION_TYPE.SHOCK);
        advertisedActions.Add(INTERACTION_TYPE.CRY);
        advertisedActions.Add(INTERACTION_TYPE.CRY);
        advertisedActions.Add(INTERACTION_TYPE.HAVE_AFFAIR);
        advertisedActions.Add(INTERACTION_TYPE.SLAY_CHARACTER);
        advertisedActions.Add(INTERACTION_TYPE.FEELING_CONCERNED);
        advertisedActions.Add(INTERACTION_TYPE.LAUGH_AT);
        advertisedActions.Add(INTERACTION_TYPE.TEASE);
        advertisedActions.Add(INTERACTION_TYPE.FEELING_SPOOKED);
        advertisedActions.Add(INTERACTION_TYPE.FEELING_BROKENHEARTED);
        advertisedActions.Add(INTERACTION_TYPE.GRIEVING);
        advertisedActions.Add(INTERACTION_TYPE.DANCE);
        advertisedActions.Add(INTERACTION_TYPE.SING);
        advertisedActions.Add(INTERACTION_TYPE.GO_TO);
        advertisedActions.Add(INTERACTION_TYPE.SCREAM_FOR_HELP);
        advertisedActions.Add(INTERACTION_TYPE.RESOLVE_COMBAT);
        advertisedActions.Add(INTERACTION_TYPE.RETURN_HOME);
        //advertisedActions.Add(INTERACTION_TYPE.RETURN_HOME_LOCATION);
        advertisedActions.Add(INTERACTION_TYPE.CHAT_CHARACTER);
        advertisedActions.Add(INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM);
        advertisedActions.Add(INTERACTION_TYPE.REVERT_TO_NORMAL_FORM);
        advertisedActions.Add(INTERACTION_TYPE.CHANGE_CLASS);
        //advertisedActions.Add(INTERACTION_TYPE.STAND);
        //advertisedActions.Add(INTERACTION_TYPE.VISIT);
        advertisedActions.Add(INTERACTION_TYPE.CRAFT_FURNITURE);

        if (race != RACE.SKELETON) {
            advertisedActions.Add(INTERACTION_TYPE.SHARE_INFORMATION);
            advertisedActions.Add(INTERACTION_TYPE.DRINK_BLOOD);
            advertisedActions.Add(INTERACTION_TYPE.KNOCKOUT_CHARACTER);
            advertisedActions.Add(INTERACTION_TYPE.BUTCHER);
        }
    }
    /// <summary>
    /// This should only be used for plans that come/constructed from the outside.
    /// </summary>
    /// <param name="plan">Plan to be added</param>
    //public void AddPlan(GoapPlan plan, bool isPriority = false, bool processPlanSpecialCases = true) {
    //    if (!allGoapPlans.Contains(plan)) {
    //        plan.SetPriorityState(isPriority);
    //        if (isPriority) {
    //            allGoapPlans.Insert(0, plan);
    //        } else {
    //            bool hasBeenInserted = false;
    //            if (plan.job != null) {
    //                for (int i = 0; i < allGoapPlans.Count; i++) {
    //                    GoapPlan currentPlan = allGoapPlans[i];
    //                    if (currentPlan.isPriority) {
    //                        continue;
    //                    }
    //                    if (currentPlan.job == null || plan.job.priority < currentPlan.job.priority) {
    //                        allGoapPlans.Insert(i, plan);
    //                        hasBeenInserted = true;
    //                        break;
    //                    }
    //                }
    //            }
    //            if (!hasBeenInserted) {
    //                allGoapPlans.Add(plan);
    //            }
    //        }

    //        if (processPlanSpecialCases) {
    //            ////If a character is strolling or idly returning home and a plan is added to this character, end the action/state
    //            //if (stateComponent.currentState != null && (stateComponent.currentState.characterState == CHARACTER_STATE.STROLL
    //            //    || stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE
    //            //    || stateComponent.currentState.characterState == CHARACTER_STATE.PATROL)) {
    //            //    stateComponent.currentState.OnExitThisState();
    //            //} else if (stateComponent.stateToDo != null && (stateComponent.stateToDo.characterState == CHARACTER_STATE.STROLL
    //            //     || stateComponent.stateToDo.characterState == CHARACTER_STATE.STROLL_OUTSIDE
    //            //     || stateComponent.stateToDo.characterState == CHARACTER_STATE.PATROL)) {
    //            //    stateComponent.SetStateToDo(null);
    //            //} else if (currentAction != null && currentAction.goapType == INTERACTION_TYPE.RETURN_HOME) {
    //            //    if (currentAction.parentPlan == null || currentAction.parentPlan.category == GOAP_CATEGORY.IDLE) {
    //            //        currentAction.StopAction();
    //            //    }
    //            //}

    //            if (plan.job != null && (plan.job.jobType.IsNeedsTypeJob() || plan.job.jobType.IsEmergencyTypeJob())) {
    //                //Unassign Location Job if character decides to rest, eat or have fun.
    //                homeArea.jobQueue.UnassignAllJobsTakenBy(this);
    //                faction.activeQuest?.jobQueue.UnassignAllJobsTakenBy(this);
    //            }
    //        }
    //    }
    //}
    //public void RecalculateJob(GoapPlanJob job) {
    //    if(job.assignedPlan != null) {
    //        job.assignedPlan.SetIsBeingRecalculated(true);
    //        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, job.assignedPlan, job));
    //    }
    //}
    //public bool IsPOIInCharacterAwarenessList(IPointOfInterest poi, List<IPointOfInterest> awarenesses) {
    //    if (awarenesses != null && awarenesses.Count > 0) {
    //        for (int i = 0; i < awarenesses.Count; i++) {
    //            if (awarenesses[i] == poi) {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}
    public void PerformTopPriorityJob() {
        string log = GameManager.Instance.TodayLogString() + "PERFORMING GOAP PLANS OF " + name;
        if (currentActionNode != null) {
            log += "\n" + name + " can't perform another action because he/she is currently performing " + currentActionNode.action.goapName;
            PrintLogIfActive(log);
            return;
        }
        
        //List<INTERACTION_TYPE> actorAllowedActions = RaceManager.Instance.GetNPCInteractionsOfCharacter(this);
        //bool willGoIdleState = true;
        if(jobQueue.jobsInQueue[0] is GoapPlanJob) {
            GoapPlanJob currentTopPrioJob = jobQueue.jobsInQueue[0] as GoapPlanJob;
            if(currentTopPrioJob.assignedPlan != null) {
                GoapPlan plan = currentTopPrioJob.assignedPlan;
                ActualGoapNode currentNode = plan.currentActualNode;
                if (RaceManager.Instance.CanCharacterDoGoapAction(this, currentNode.action.goapType)
                && InteractionManager.Instance.CanSatisfyGoapActionRequirements(currentNode.action.goapType, currentNode.actor, currentNode.poiTarget, currentNode.otherData)) {
                    bool preconditionsSatisfied = plan.currentActualNode.action.CanSatisfyAllPreconditions(currentNode.actor, currentNode.poiTarget, currentNode.otherData);
                    if (!preconditionsSatisfied) {
                        log += "\n - Action's preconditions are not all satisfied, trying to recalculate plan...";
                        if (plan.doNotRecalculate) {
                            log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
                            PrintLogIfActive(log);
                            currentTopPrioJob.CancelJob(false);
                        } else {
                            PrintLogIfActive(log);
                            planner.RecalculateJob(currentTopPrioJob);
                        }
                    } else {
                        //Do not perform action if the target character is still in another character's party, this means that he/she is probably being abducted
                        //Wait for the character to be in its own party before doing the action
                        if (currentNode.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                            Character targetCharacter = currentNode.poiTarget as Character;
                            if (!targetCharacter.IsInOwnParty() && targetCharacter.currentParty != ownParty) {
                                log += "\n - " + targetCharacter.name + " is not in its own party, waiting and skipping...";
                                PrintLogIfActive(log);
                                return;
                            }
                        }
                        if (currentNode.poiTarget != this && currentNode.isStealth) {
                            //When performing a stealth job action to a character check if that character is already in vision range, if it is, check if the character doesn't have anyone other than this character in vision, if it is, skip it
                            if (marker.inVisionPOIs.Contains(currentNode.poiTarget) && !marker.CanDoStealthActionToTarget(currentNode.poiTarget)) {
                                log += "\n - Action is stealth and character cannot do stealth action right now...";
                                return;
                            }
                        }
                        log += "\n - Action's preconditions are all satisfied, doing action...";
                        PrintLogIfActive(log);
                        Messenger.Broadcast(Signals.CHARACTER_WILL_DO_PLAN, this, plan);
                        currentNode.DoAction(currentTopPrioJob, plan);
                    }
                } else {
                    log += "\n - Action did not meet current requirements and allowed actions, dropping plan...";
                    PrintLogIfActive(log);
                    currentTopPrioJob.CancelJob(false);
                }
            }
            
        }

        //for (int i = 0; i < allGoapPlans.Count; i++) {
        //    GoapPlan plan = allGoapPlans[i];
        //    if (plan.currentNode == null) {
        //        throw new Exception(this.name + "'s current node in plan is null! Plan is: " + plan.name + "\nCall stack: " + plan.setPlanStateCallStack + "\n");
        //    }
        //    log += "\n" + plan.currentNode.action.goapName;
        //    if (plan.isBeingRecalculated) {
        //        log += "\n - Plan is currently being recalculated, skipping...";
        //        continue; //skip plan
        //    }
        //    if (RaceManager.Instance.CanCharacterDoGoapAction(this, plan.currentNode.action.goapType) 
        //        && InteractionManager.Instance.CanSatisfyGoapActionRequirements(plan.currentNode.action.goapType, plan.currentNode.action.actor, plan.currentNode.action.poiTarget, plan.currentNode.action.otherData)) {
        //        //if (plan.isBeingRecalculated) {
        //        //    log += "\n - Plan is currently being recalculated, skipping...";
        //        //    continue; //skip plan
        //        //}
        //        //if (IsPlanCancelledDueToInjury(plan.currentNode.action)) {
        //        //    log += "\n - Action's plan is cancelled due to injury, dropping plan...";
        //        //    PrintLogIfActive(log);
        //        //    if (allGoapPlans.Count == 1) {
        //        //        DropPlan(plan, true);
        //        //        willGoIdleState = false;
        //        //        break;
        //        //    } else {
        //        //        DropPlan(plan, true);
        //        //        i--;
        //        //        continue;
        //        //    }
        //        //}
        //        if (plan.currentNode.action.IsHalted()) {
        //            log += "\n - Action " + plan.currentNode.action.goapName + " is waiting, skipping...";
        //            continue;
        //        }
        //        bool preconditionsSatisfied = plan.currentNode.action.CanSatisfyAllPreconditions();
        //        if (!preconditionsSatisfied) {
        //            log += "\n - Action's preconditions are not all satisfied, trying to recalculate plan...";
        //            if (plan.doNotRecalculate) {
        //                log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
        //                PrintLogIfActive(log);
        //                if (allGoapPlans.Count == 1) {
        //                    DropPlan(plan);
        //                    willGoIdleState = false;
        //                    break;
        //                } else {
        //                    DropPlan(plan);
        //                    i--;
        //                }
        //            } else {
        //                PrintLogIfActive(log);
        //                planner.RecalculateJob(plan);
        //                willGoIdleState = false;
        //            }
        //        } else {
        //            //Do not perform action if the target character is still in another character's party, this means that he/she is probably being abducted
        //            //Wait for the character to be in its own party before doing the action
        //            if (plan.currentNode.action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //                Character targetCharacter = plan.currentNode.action.poiTarget as Character;
        //                if (!targetCharacter.IsInOwnParty() && targetCharacter.currentParty != _ownParty) {
        //                    log += "\n - " + targetCharacter.name + " is not in its own party, waiting and skipping...";
        //                    PrintLogIfActive(log);
        //                    continue;
        //                }
        //            }
        //            if(plan.currentNode.action.poiTarget != this && plan.currentNode.action.isStealth) {
        //                //When performing a stealth job action to a character check if that character is already in vision range, if it is, check if the character doesn't have anyone other than this character in vision, if it is, skip it
        //                if (marker.inVisionPOIs.Contains(plan.currentNode.action.poiTarget) && !marker.CanDoStealthActionToTarget(plan.currentNode.action.poiTarget)) {
        //                    log += "\n - Action is stealth and character cannot do stealth action right now...";
        //                    continue;
        //                }
        //            }
        //            log += "\n - Action's preconditions are all satisfied, doing action...";
        //            PrintLogIfActive(log);
        //            Messenger.Broadcast(Signals.CHARACTER_WILL_DO_PLAN, this, plan);
        //            //if (plan.currentNode.parent != null && plan.currentNode.parent.action.CanSatisfyAllPreconditions() && plan.currentNode.parent.action.CanSatisfyRequirements()) {
        //            //    log += "\n - All Preconditions of next action in plan already met, skipping action: " + plan.currentNode.action.goapName;
        //            //    //set next node to parent node instead
        //            //    plan.SetNextNode();
        //            //    log += "\n - Next action is: " + plan.currentNode.action.goapName;
        //            //}
        //            plan.currentNode.action.DoAction();
        //            willGoIdleState = false;
        //            break;
        //        }
        //    } else {
        //        log += "\n - Action did not meet current requirements and allowed actions, dropping plan...";
        //        PrintLogIfActive(log);
        //        if (allGoapPlans.Count == 1) {
        //            DropPlan(plan);
        //            willGoIdleState = false;
        //            break;
        //        } else {
        //            DropPlan(plan);
        //            i--;
        //        }
        //    }
        //}
        //if (willGoIdleState) {
        //    log += "\n - Character will go into idle state";
        //    PrintLogIfActive(log);
        //    PerStartTickActionPlanning();
        //}
    }
    public void PerformGoapAction() {
        string log = string.Empty;
        if (currentActionNode == null) {
            log = GameManager.Instance.TodayLogString() + name + " cannot PerformGoapAction because there is no current action!";
            PrintLogIfActive(log);
            //Debug.LogError(log);
            //if (!DropPlan(plan)) {
            //    //PlanGoapActions();
            //}
            //StartDailyGoapPlanGeneration();
            return;
        }
        log = GameManager.Instance.TodayLogString() + name + " is performing goap action: " + currentActionNode.action.goapName;
        FaceTarget(currentActionNode.poiTarget);
        bool willStillContinueAction = true;
        OnStartPerformGoapAction(currentActionNode, ref willStillContinueAction);
        if (!willStillContinueAction) {
            return;
        }
        if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currentActionNode.action.goapType, currentActionNode.actor, currentActionNode.poiTarget, currentActionNode.otherData)
            && currentActionNode.action.CanSatisfyAllPreconditions(currentActionNode.actor, currentActionNode.poiTarget, currentActionNode.otherData)) {
            log += "\nAction satisfies all requirements and preconditions, proceeding to perform actual action: " + currentActionNode.action.goapName + " to " + currentActionNode.poiTarget.name + " at " + currentActionNode.poiTarget.gridTileLocation?.ToString() ?? "No Tile Location";
            PrintLogIfActive(log);
            currentActionNode.PerformAction();
        } else {
            log += "\nAction did not meet all requirements and preconditions. Will try to recalculate plan...";
            GoapPlan plan = currentPlan;
            if (plan.doNotRecalculate) {
                log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
                PrintLogIfActive(log);
                currentJob.CancelJob(false);
            } else {
                PrintLogIfActive(log);
                UnityEngine.Assertions.Assert.IsNotNull(currentJob);
                UnityEngine.Assertions.Assert.IsTrue(currentJob is GoapPlanJob);
                planner.RecalculateJob(currentJob as GoapPlanJob);
            }
            SetCurrentActionNode(null, null, null);
        }
        //if (currentActionNode.isStopped) {
        //    log += "\n Action is stopped! Dropping plan...";
        //    PrintLogIfActive(log);
        //    SetCurrentActionNode(null);
        //    if (!DropPlan(currentActionNode.parentPlan)) {
        //        //PlanGoapActions();
        //    }
        //} else {
        //    bool willStillContinueAction = true;
        //    OnStartPerformGoapAction(currentActionNode, ref willStillContinueAction);
        //    if (!willStillContinueAction) {
        //        return;
        //    }
        //    if (currentActionNode.IsHalted()) {
        //        log += "\n Action is waiting! Not doing action...";
        //        //if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //        //    Character targetCharacter = currentAction.poiTarget as Character;
        //        //    targetCharacter.AdjustIsWaitingForInteraction(-1);
        //        //}
        //        SetCurrentActionNode(null);
        //        return;
        //    }
        //    if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currentActionNode.goapType, currentActionNode.actor, currentActionNode.poiTarget, currentActionNode.otherData) 
        //        && currentActionNode.CanSatisfyAllPreconditions()) {
        //        //if (currentAction.poiTarget != this && currentAction.isStealth) {
        //        //    //When performing a stealth job action to a character check if that character is already in vision range, if it is, check if the character doesn't have anyone other than this character in vision, if it is, skip it
        //        //    if (marker.inVisionPOIs.Contains(currentAction.poiTarget) && !marker.CanDoStealthActionToTarget(currentAction.poiTarget)) {
        //        //        log += "\n - Action is stealth and character cannot do stealth action right now...";
        //        //        PrintLogIfActive(log);
        //        //        return;
        //        //    }
        //        //}
        //        log += "\nAction satisfies all requirements and preconditions, proceeding to perform actual action: " + currentActionNode.goapName + " to " + currentActionNode.poiTarget.name + " at " + currentActionNode.poiTarget.gridTileLocation?.ToString() ?? "No Tile Location";
        //        PrintLogIfActive(log);
        //        currentActionNode.Perform();
        //    } else {
        //        log += "\nAction did not meet all requirements and preconditions. Will try to recalculate plan...";
        //        //if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //        //    Character targetCharacter = currentAction.poiTarget as Character;
        //        //    targetCharacter.AdjustIsWaitingForInteraction(-1);
        //        //}
        //        GoapPlan plan = currentActionNode.parentPlan;
        //        SetCurrentActionNode(null);
        //        if (plan.doNotRecalculate) {
        //            log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
        //            PrintLogIfActive(log);
        //            if (!DropPlan(plan)) {
        //                //PlanGoapActions();
        //            }
        //        } else {
        //            PrintLogIfActive(log);
        //            planner.RecalculateJob(plan);
        //            //IdlePlans();
        //        }
        //    }
        //}
    }
    public void GoapActionResult(string result, ActualGoapNode actionNode) {
        string log = GameManager.Instance.TodayLogString() + name + " is done performing goap action: " + actionNode.action.goapName;
        //Debug.Log(log);
        GoapPlan plan = currentPlan;
        GoapPlanJob job = currentJob as GoapPlanJob;

        if (actionNode == currentActionNode) {
            SetCurrentActionNode(null, null, null);
        }
        //actionNode.poiTarget.RemoveTargettedByAction(actionNode);

        if (isDead || !canWitness) {
            log += "\n" + name + " is dead! Do not do GoapActionResult, automatically CancelJob";
            PrintLogIfActive(log);
            job.CancelJob(false);
            //bool forceRemoveJobInQueue = false;
            //if (plan.job != null) {
            //    if (result == InteractionManager.Goap_State_Success) {
            //        if (plan.currentNode.parent == null && plan.job.jobQueueParent != null) {
            //            log += "This plan has a job and the result of action " + actionNode.goapName + " is " + result + " and this is the last action for this plan, removing job in job queue...";
            //            forceRemoveJobInQueue = true;
            //        }
            //    }
            //}
            //PrintLogIfActive(log);
            //DropPlan(plan, forceRemoveJobInQueue, true);
            return;
        }
        //if (actionNode.isStopped) {
        //    log += "\nAction is stopped!";
        //    PrintLogIfActive(log);
        //    DropPlan(plan);
        //    return;
        //}

        //Myk, para san to?
        //if (plan.state == GOAP_PLAN_STATE.CANCELLED || plan.currentNode == null) {
        //    log += "\nPlan was cancelled.";
        //    bool forceRemoveJobInQueue = false;
        //    if (plan.job != null) {
        //        if (!plan.job.jobQueueParent.isAreaOrQuestJobQueue) {
        //            log += "\nRemoving job in queue...";
        //            forceRemoveJobInQueue = true;
        //            //plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
        //        } else {
        //            log += "\nPlan's job is either an area or faction job, returning it to jobQueue...";
        //        }
        //    }
        //    PrintLogIfActive(log);
        //    DropPlan(plan, forceRemoveJobInQueue);
        //    return;
        //}
        if(result == InteractionManager.Goap_State_Success) {
            log += "\nPlan is setting next action to be done...";
            plan.SetNextNode();
            if (plan.currentNode == null) {
                log += "\nThis action is the end of plan.";
                if (job.originalOwner.ownerType != JOB_OWNER.CHARACTER && traitContainer.GetNormalTrait<Trait>("Hardworking") != null) {
                    log += "\nFinished a settlement job and character is hardworking, increase happiness by 3000...";
                    needsComponent.AdjustHappiness(3000); //TODO: Move this to hardworking trait.
                }
                PrintLogIfActive(log);
                //bool forceRemoveJobInQueue = true;
                ////If an action is stopped as current action (meaning it was cancelled) and it is a settlement/faction job, do not remove it from the queue
                //if (actionNode.isStoppedAsCurrentAction && plan != null && plan.job != null && plan.job.jobQueueParent.isAreaOrQuestJobQueue) {
                //    forceRemoveJobInQueue = false;
                //}
                //this means that this is the end goal so end this plan now
                job.ForceCancelJob(false);

                //after doing an extreme needs type job, check again if the character needs to recover more of that need.
                if (job.jobType == JOB_TYPE.HAPPINESS_RECOVERY_FORLORN) {
                    PlanHappinessRecoveryActions();
                } else if (job.jobType == JOB_TYPE.HUNGER_RECOVERY_STARVING) {
                    PlanFullnessRecoveryActions();
                } else if (job.jobType == JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED) {
                    PlanTirednessRecoveryActions();
                }
            } else {
                log += "\nNext action for this plan: " + plan.currentActualNode.goapName;
                //if (plan.job != null && plan.job.assignedCharacter != this) {
                //    log += "\nPlan has a job: " + plan.job.name + ". Assigned character " + (plan.job.assignedCharacter != null ? plan.job.assignedCharacter.name : "None") + " does not match with " + name + ".";
                //    log += "Drop plan because this character is no longer the one assigned";
                //    DropPlan(plan);
                //}
                PrintLogIfActive(log);
                //PlanGoapActions();
            }
        }
        //Reason: https://trello.com/c/58aGENsO/1867-attempt-to-find-another-nearby-chair-first-instead-of-dropping-drink-eat-sit-down-actions
        else if (result == InteractionManager.Goap_State_Fail) {
            if (plan.doNotRecalculate) {
                job.CancelJob(false);
            } else {
                planner.RecalculateJob(job as GoapPlanJob);
            }
            //if the last action of the plan failed and that action type can be replaced
            //if (actionNode.goapType.CanBeReplaced()) {
            //    //find a similar action that is advertised by another object, in the same structure
            //    //if there is any, insert that action into the current plan, then do that next
            //    List<TileObject> objs = currentStructure.GetTileObjectsThatAdvertise(actionNode.goapType);
            //    if (objs.Count > 0) {
            //        TileObject chosenObject = objs[UnityEngine.Random.Range(0, objs.Count)];
            //        GoapAction newAction = chosenObject.Advertise(actionNode.goapType, this);
            //        if (newAction != null) {
            //            plan.InsertAction(newAction);
            //        } else {
            //            Debug.LogWarning(chosenObject.ToString() + " did not return an action of type " + actionNode.goapType.ToString());
            //        }
            //    }
            //}
        }
        //log += "\nPlan is setting next action to be done...";
        //plan.SetNextNode(actionNode);
        //if (plan.currentNode == null) {
        //    log += "\nThis action is the end of plan.";
        //    if (plan.job != null && plan.job.jobQueueParent != null) {
        //        log += "\nRemoving job in queue...";
        //        if (plan.job.jobQueueParent.isAreaOrQuestJobQueue && traitContainer.GetNormalTrait<Trait>("Hardworking") != null) {
        //            log += "\nFinished a settlement job and character is hardworking, increase happiness by 3000...";
        //            AdjustHappiness(3000);
        //        }
        //        //plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
        //    }
        //    PrintLogIfActive(log);
        //    bool forceRemoveJobInQueue = true;
        //    //If an action is stopped as current action (meaning it was cancelled) and it is a settlement/faction job, do not remove it from the queue
        //    if (actionNode.isStoppedAsCurrentAction && plan != null && plan.job != null && plan.job.jobQueueParent.isAreaOrQuestJobQueue) {
        //        forceRemoveJobInQueue = false;
        //    }
        //    //this means that this is the end goal so end this plan now
        //    DropPlan(plan, forceRemoveJobInQueue);
        //} else {
        //    log += "\nNext action for this plan: " + plan.currentNode.action.goapName;
        //    if (plan.job != null && plan.job.assignedCharacter != this) {
        //        log += "\nPlan has a job: " + plan.job.name + ". Assigned character " + (plan.job.assignedCharacter != null ? plan.job.assignedCharacter.name : "None") + " does not match with " + name + ".";
        //        log += "Drop plan because this character is no longer the one assigned";
        //        DropPlan(plan);
        //    }
        //    PrintLogIfActive(log);
        //    //PlanGoapActions();
        //}
        //action.OnResultReturnedToActor();
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
    //public bool DropPlan(GoapPlan plan, bool forceRemoveJob = false, bool forceProcessPlanJob = false) {
    //    bool hasBeenRemoved = false;
    //    if (allGoapPlans.Remove(plan)) {
    //        Debug.Log(GameManager.Instance.TodayLogString() + plan.name + " was removed from " + this.name + "'s plan list");
    //        plan.EndPlan();
    //        hasBeenRemoved = true;
    //    }
    //    if(hasBeenRemoved || forceProcessPlanJob) {
    //        if (plan.job != null) {
    //            if (plan.job.cancelJobOnFail || plan.job.cancelJobOnDropPlan || forceRemoveJob) {
    //                plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
    //            }
    //            plan.job.SetAssignedCharacter(null);
    //            plan.job.SetAssignedPlan(null);
    //        }
    //    }
    //    return hasBeenRemoved;
    //}
    //public bool JustDropPlan(GoapPlan plan, bool forceRemoveJob = false, bool forceProcessPlanJob = false) {
    //    bool hasBeenRemoved = false;
    //    if (allGoapPlans.Remove(plan)) {
    //        Debug.Log(GameManager.Instance.TodayLogString() + plan.name + " was removed from " + this.name + "'s plan list");
    //        plan.EndPlan();
    //        hasBeenRemoved = true;
    //    }
    //    if (hasBeenRemoved || forceProcessPlanJob) {
    //        if (plan.job != null) {
    //            if (plan.job.cancelJobOnDropPlan || forceRemoveJob) {
    //                plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
    //            }
    //            plan.job.SetAssignedCharacter(null);
    //            plan.job.SetAssignedPlan(null);
    //        }
    //    }
    //    return hasBeenRemoved;
    //}
    //public void DropAllPlans(GoapPlan planException = null) {
    //    if (planException == null) {
    //        while (allGoapPlans.Count > 0) {
    //            DropPlan(allGoapPlans[0]);
    //        }
    //    } else {
    //        for (int i = 0; i < allGoapPlans.Count; i++) {
    //            if (allGoapPlans[i] != planException) {
    //                DropPlan(allGoapPlans[i]);
    //                i--;
    //            }
    //        }
    //    }
    //}
    //public void JustDropAllPlansOfType(INTERACTION_TYPE type) {
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        GoapPlan currPlan = allGoapPlans[i];
    //        if (currPlan.endNode.action != null && currPlan.endNode.action.goapType == type) {
    //            if (JustDropPlan(currPlan)) {
    //                i--;
    //            }
    //        }
    //    }
    //}
    //public void DropAllPlansOfType(INTERACTION_TYPE type) {
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        GoapPlan currPlan = allGoapPlans[i];
    //        if (currPlan.endNode.action != null && currPlan.endNode.action.goapType == type) {
    //            if (DropPlan(currPlan)) {
    //                i--;
    //            }
    //        }
    //    }
    //}
    //public bool HasPlanWithType(INTERACTION_TYPE type) {
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        GoapPlan currPlan = allGoapPlans[i];
    //        if (currPlan.endNode.action != null && currPlan.endNode.action.goapType == type) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //public GoapPlan GetPlanWithGoalEffect(GOAP_EFFECT_CONDITION goalEffect) {
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        if (allGoapPlans[i].goalEffects.Contains(goalEffect)) {
    //            return allGoapPlans[i];
    //        }
    //    }
    //    return null;
    //}
    //public GoapPlan GetPlanByCategory(GOAP_CATEGORY category) {
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        if (allGoapPlans[i].category == category) {
    //            return allGoapPlans[i];
    //        }
    //    }
    //    return null;
    //}
    //For testing: Drop Character
    //public void DropACharacter() {
    //    if (awareness.ContainsKey(POINT_OF_INTEREST_TYPE.CHARACTER)) {
    //        List<IPointOfInterest> characterAwarenesses = awareness[POINT_OF_INTEREST_TYPE.CHARACTER];
    //        Character randomTarget = characterAwarenesses[UnityEngine.Random.Range(0, characterAwarenesses.Count)] as Character;
    //        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeRegion, targetPOI = randomTarget };
    //        StartGOAP(goapEffect, randomTarget, GOAP_CATEGORY.REACTION);
    //    }
    //}
    //public GoapPlan GetPlanWithAction(GoapAction action) {
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        for (int j = 0; j < allGoapPlans[i].allNodes.Count; j++) {
    //            if (allGoapPlans[i].allNodes[j].actionType == action) {
    //                return allGoapPlans[i];
    //            }
    //        }
    //    }
    //    return null;
    //}
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
    public void SetCurrentActionNode(ActualGoapNode actionNode, JobQueueItem job, GoapPlan plan) {
        if (currentActionNode != null) {
            previousCurrentActionNode = currentActionNode;
        }
        currentActionNode = actionNode;
        if (currentActionNode != null) {
            PrintLogIfActive(GameManager.Instance.TodayLogString() + this.name + " will do action " + actionNode.action.goapType.ToString() + " to " + actionNode.poiTarget.ToString());
            if (currentActionNode.action.goapType.IsHostileAction()) { //if the character will do a combat action, remove all ignore hostilities value
                ClearIgnoreHostilities();
            }
            //stateComponent.SetStateToDo(null, stopMovement: false);

            //Current Job must always be the job in the top prio, if there is inconsistency with the currentActionNode, then the problem lies on what you set as the currentActionNode
        }
        SetCurrentJob(job);
        SetCurrentPlan(plan);
        
        string summary = GameManager.Instance.TodayLogString() + "Set current action to ";
        if (currentActionNode == null) {
            summary += "null";
        } else {
            summary += currentActionNode.action.goapName + " targetting " + currentActionNode.poiTarget.name;
        }
        //summary += "\n StackTrace: " + StackTraceUtility.ExtractStackTrace();

        actionHistory.Add(summary);
        if (actionHistory.Count > 10) {
            actionHistory.RemoveAt(0);
        }
    }
    public void SetCurrentPlan(GoapPlan plan) {
        currentPlan = plan;
    }
    //Only stop an action node if it is the current action node
    ///Stopping action node does not mean that the job will be cancelled, if you want to cancel job at the same time call <see cref="StopCurrentActionNodeAndCancelItsJob">
    public bool StopCurrentActionNode(bool shouldDoAfterEffect = false, string reason = "") {
        if(currentActionNode == null) {
            return false;
        }
        bool shouldLogReason = true;
        if (reason != "" && currentActionNode.poiTarget != this && !currentJob.IsAnInterruptionJob() && currentJob.jobType != JOB_TYPE.WATCH) {
            //if(currentActionNode.poiTarget is Character) {
            //    Trait targetDeadTrait = currentActionNode.poiTarget.traitContainer.GetNormalTrait<Trait>("Dead");
            //    if(targetDeadTrait.gainedFromDoing == currentActionNode) {
            //        shouldLogReason = false;
            //    }
            //}
        } else {
            shouldLogReason = false;
        }
        if (shouldLogReason) {
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "current_action_abandoned_reason");
            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, currentActionNode.action.goapName, LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_2);
            RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        }
        //if (actor.currentAction != null && actor.currentAction.parentPlan != null && actor.currentAction.parentPlan.job != null && actor.currentAction == this) {
        //    if (reason != "") {
        //        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "current_action_abandoned_reason");
        //        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        log.AddToFillers(null, actor.currentAction.goapName, LOG_IDENTIFIER.STRING_1);
        //        log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_2);
        //        actor.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        //    }
        //}
        if (currentParty.icon.isTravelling) {
            if (currentParty.icon.travelLine == null) {
                //This means that the actor currently travelling to another tile in tilemap
                marker.StopMovement();
            } else {
                //This means that the actor is currently travelling to another area
                currentParty.icon.SetOnArriveAction(() => OnArriveAtAreaStopMovement());
            }
        }
        //if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //    Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        //    Messenger.RemoveListener<TileObject, Character>(Signals.TILE_OBJECT_DISABLED, OnTileObjectDisabled);
        //}

        //SetIsStopped(true);
        currentActionNode.StopActionNode(shouldDoAfterEffect);
        SetCurrentActionNode(null, null, null);
        //JobQueueItem job = parentPlan.job;

        //Remove job in queue if job is personal job and removeJobInQueue value is true
        //if (removeJobInQueue && job != null && !job.jobQueueParent.isAreaOrQuestJobQueue) {
        //    job.jobQueueParent.RemoveJobInQueue(job);
        //}
        if (UIManager.Instance.characterInfoUI.isShowing) {
            UIManager.Instance.characterInfoUI.UpdateBasicInfo();
        }
        //Messenger.Broadcast<GoapAction>(Signals.STOP_ACTION, this);
        PrintLogIfActive(GameManager.Instance.TodayLogString() + "Stopped action of " + name + " which is " + previousCurrentActionNode.action.goapName + " targetting " + previousCurrentActionNode.poiTarget.name + "!");
        return true;
    }
    //public void SetHasAlreadyAskedForPlan(bool state) {
    //    _hasAlreadyAskedForPlan = state;
    //}
    public void PrintLogIfActive(string log) {
        //if (InteriorMapManager.Instance.currentlyShowingArea == specificLocation) {//UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == this
        Debug.Log(log);
        //}
    }
    //public void AddTargettedByAction(GoapAction action) {
    //    if (this != action.actor) { // && !isDead
    //        targettedByAction.Add(action);
    //        if (marker != null) {
    //            marker.OnCharacterTargettedByAction(action);
    //        }
    //    }
    //}
    //public void RemoveTargettedByAction(GoapAction action) {
    //    if (targettedByAction.Remove(action)) {
    //        if (marker != null) {
    //            marker.OnCharacterRemovedTargettedByAction(action);
    //        }
    //    }
    //}
    //This will only stop the current action of this character, this is different from StopAction because this will not drop the plan if the actor is not performing it but is on the way
    //This does not stop the movement of this character, call StopMovement separately to stop movement
    //public void StopCurrentAction(bool shouldDoAfterEffect = true, string reason = "") {
    //    if(currentActionNode != null) {
    //        //Debug.Log("Stopped action of " + name + " which is " + currentAction.goapName);
    //        if (currentActionNode.parentPlan != null && currentActionNode.parentPlan.job != null && reason != "") {
    //            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "current_action_abandoned_reason");
    //            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //            log.AddToFillers(null, currentActionNode.goapName, LOG_IDENTIFIER.STRING_1);
    //            log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_2);
    //            RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
    //        }
    //        if (currentActionNode.isPerformingActualAction && !currentActionNode.isDone) {
    //            if (!shouldDoAfterEffect) {
    //                currentActionNode.OnStopActionDuringCurrentState();
    //            }
    //            if(currentActionNode.currentState != null) {
    //                currentActionNode.SetIsStoppedAsCurrentAction(true);
    //                currentActionNode.currentState.EndPerTickEffect(shouldDoAfterEffect);
    //            } else {
    //                SetCurrentActionNode(null);
    //            }
    //        } else {
    //            currentActionNode.OnStopActionWhileTravelling();
    //            SetCurrentActionNode(null);
    //        }
    //    }
    //}
    public void OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) {
        bool stillContinueCurrentAction = true;
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            Trait trait = traitContainer.allTraits[i];
            if (trait.OnStartPerformGoapAction(node, ref stillContinueCurrentAction)) {
                willStillContinueAction = stillContinueCurrentAction;
                break;
            } else {
                stillContinueCurrentAction = true;
            }
        }
    }
    private void HeardAScream(Character characterThatScreamed) {
        if(doNotDisturb > 0) {
            //Do not react to scream if character has disabler trait
            return;
        }
        if(gridTileLocation != null && characterThatScreamed.gridTileLocation != null) {
            float dist = gridTileLocation.GetDistanceTo(characterThatScreamed.gridTileLocation);
            PrintLogIfActive(name + " distance to " + characterThatScreamed.name + " is " + dist);
            float distanceChecker = 5f;
            //if (currentStructure != characterThatScreamed.currentStructure) {
            //    distanceChecker = 2f;
            //}
            if (dist > distanceChecker) {
                //Do not react to scream if character is too far
                return;
            }
        }
        if(jobQueue.HasJob(JOB_TYPE.REACT_TO_SCREAM, characterThatScreamed)) {
            //Do not react if character will already react to a scream;
            return;
        }
        if (!CanCharacterReact(characterThatScreamed)) {
            return;
        }
        ReactToScream(characterThatScreamed);
    }
    private void ReactToScream(Character characterThatScreamed) {
        //If you are wondering why the job is not just simply added to the job queue and let the job queue processing work if the job can override current job, is because
        //in this situation, we want the job not to be added to the job queue if it cannot override the current job
        //If we let the AddJobInQueue simply process the job, it will still be added regardless if it cannot override the current job, it means that it will just be pushed back in queue and will be done by the character when the time comes
        //We don't want that because we want to have a spontaneous reaction from this character, so the only way that the character will react is if he can do it immediately

        string log = GameManager.Instance.TodayLogString() + name + " heard the scream of " + characterThatScreamed.name + ", reacting...";

        bool canReact = true;
        int reactJobPriority = InteractionManager.Instance.GetInitialPriority(JOB_TYPE.REACT_TO_SCREAM);
        if (stateComponent.currentState != null && stateComponent.currentState.job != null && stateComponent.currentState.job.priority <= reactJobPriority) {
            canReact = false;
        } 
        //else if (stateComponent.stateToDo != null && stateComponent.stateToDo.job != null && stateComponent.stateToDo.job.priority <= reactJobPriority) {
        //    canReact = false;
        //} 
        else if (currentJob != null && currentJob.priority <= reactJobPriority) {
            canReact = false;
        }
        if (canReact) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REACT_TO_SCREAM, INTERACTION_TYPE.GO_TO, characterThatScreamed, this);
            jobQueue.AddJobInQueue(job);
            //if (CanCurrentJobBeOverriddenByJob(job)) {
            //    jobQueue.AddJobInQueue(job, false);
            //    jobQueue.CurrentTopPriorityIsPushedBackBy(job, this);
            //    log += "\n" + name + " will go to " + characterThatScreamed.name;
            //} else {
            //    log += "\n" + name + " cannot react because there is still something important that he/she will do.";
            //}
        } else {
            log += "\n" + name + " cannot react because there is still something important that he/she will do.";
        }
        PrintLogIfActive(log);
    }
    #endregion

    #region Reesources
    public void AdjustResource(RESOURCE resource, int amount) {
        switch (resource) {
            case RESOURCE.FOOD:
                AdjustFood(amount);
                break;
            case RESOURCE.WOOD:
                AdjustSupply(amount);
                break;
            case RESOURCE.STONE:
                throw new NotImplementedException();
            case RESOURCE.METAL:
                throw new NotImplementedException();
            default:
                break;
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
        if (isFactionless || character.isFactionless) {
            //this character is unaligned
            //if unaligned, hostile to all other characters, except those of same race
            return character.race != this.race;
        } else {
            //this character has a faction
            //if has a faction, is hostile to characters of every other faction
            //return this.faction.id != character.faction.id;
            if(faction == character.faction) {
                return false;
            }
            return faction.GetRelationshipWith(character.faction).relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE;
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
        //TODO: Crime System Handling!
        //ReactToCrime(witnessedCrime.committedCrime, witnessedCrime, witnessedCrime.actorAlterEgo, ref hasRelationshipDegraded, witnessedCrime);
        //witnessedCrime.OnWitnessedBy(this);
    }
    /// <summary>
    /// A variation of react to crime in which the parameter SHARE_INTEL_STATUS will be the one to determine if it is informed or witnessed crime
    /// Returns true or false, if the relationship between the reactor and the criminal has degraded
    /// </summary>
    public bool ReactToCrime(CRIME committedCrime, ActualGoapNode crimeAction, AlterEgoData criminal, SHARE_INTEL_STATUS status) {
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
    public void ReactToCrime(CRIME committedCrime, ActualGoapNode crimeAction, AlterEgoData criminal, ref bool hasRelationshipDegraded, ActualGoapNode witnessedCrime = null, ActualGoapNode informedCrime = null) {
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
        //Log witnessLog = null;
        //Log reportLog = null;
        RELATIONSHIP_EFFECT relationshipEfffectWithCriminal = relationshipContainer.GetRelationshipEffectWith(criminal);
        CRIME_CATEGORY category = committedCrime.GetCategory();

        //If character witnessed an Infraction crime:
        if (category == CRIME_CATEGORY.INFRACTIONS) {
            //-Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]."
            //- Report / Share Intel Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]."
            //- no additional response
            reactSummary += "\nCrime committed is infraction.";
            //witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed");
            //reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed");
        }
        //If character has a positive relationship (Friend, Lover, Paramour) with the criminal
        else if (relationshipEfffectWithCriminal == RELATIONSHIP_EFFECT.POSITIVE) {
            reactSummary += "\n" + this.name + " has a positive relationship with " + criminal.owner.name;
            //and crime severity is a Misdemeanor:
            if (category == CRIME_CATEGORY.MISDEMEANOR) {
                reactSummary += "\nCrime committed is misdemeanor.";
                //- Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder] but did not do anything due to their relationship."
                //-Report / Share Intel Log: "[Character Name] was informed that [Criminal Name] committed [Theft/Assault/Murder] but did not do anything due to their relationship."
                //witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "do_nothing");
                //reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_do_nothing");
            }
            //and crime severity is Serious Crimes or worse:
            else if (category.IsGreaterThanOrEqual(CRIME_CATEGORY.SERIOUS)) {
                reactSummary += "\nCrime committed is serious or worse. Removing positive relationships.";
                //- Relationship Degradation between Character and Criminal
                hasRelationshipDegraded = RelationshipManager.Instance.RelationshipDegradation(criminal.owner, this, witnessedCrime);
                if (hasRelationshipDegraded) {
                    //witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed_degraded");
                    //reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_witnessed_degraded");
                    PerRoleCrimeReaction(committedCrime, crimeAction, criminal, witnessedCrime, informedCrime);
                } else {
                    if (witnessedCrime != null) {
                        if (marker.inVisionCharacters.Contains(criminal.owner)) {
                            marker.AddAvoidInRange(criminal.owner);
                        }
                    }
                    //witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "do_nothing");
                    //reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_do_nothing");
                }

            }
        }
        //If character has no relationships with the criminal or they are enemies and the crime is a Misdemeanor or worse:
        else if ((!this.relationshipContainer.HasRelationshipWith(criminal) || this.relationshipContainer.HasRelationshipWith(criminal, RELATIONSHIP_TRAIT.ENEMY)) 
            && category.IsGreaterThanOrEqual(CRIME_CATEGORY.MISDEMEANOR)) {
            reactSummary += "\n" + this.name + " does not have a relationship with or is an enemy of " + criminal.name + " and the committed crime is misdemeanor or worse";
            //- Relationship Degradation between Character and Criminal
            hasRelationshipDegraded = RelationshipManager.Instance.RelationshipDegradation(criminal.owner, this, witnessedCrime);
            //- Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]!"
            if (hasRelationshipDegraded) {
                //witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed_degraded");
                //reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_witnessed_degraded");
                PerRoleCrimeReaction(committedCrime, crimeAction, criminal, witnessedCrime, informedCrime);
            } else {
                if (witnessedCrime != null) {
                    if (marker.inVisionCharacters.Contains(criminal.owner)) {
                        marker.AddAvoidInRange(criminal.owner);
                    }
                }
                //witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "do_nothing");
                //reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_do_nothing");
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
    private void PerRoleCrimeReaction(CRIME committedCrime, ActualGoapNode crimeAction, AlterEgoData criminal, ActualGoapNode witnessedCrime = null, ActualGoapNode informedCrime = null) {
        //GoapPlanJob job = null;
        switch (role.roleType) {
            case CHARACTER_ROLE.CIVILIAN:
            case CHARACTER_ROLE.ADVENTURER:
                //- If the character is a Civilian or Adventurer, he will enter Flee mode (fleeing the criminal) and will create a Report Crime Job Type in his personal job queue
                //if (this.faction != FactionManager.Instance.neutralFaction && criminal.faction == this.faction) {
                //    //only make character flee, if he/she actually witnessed the crime (not share intel)
                //    //GoapAction crimeToReport = informedCrime;
                //    //if (witnessedCrime != null) {
                //    //    crimeToReport = witnessedCrime;
                //    //    ////if a character has no negative disabler traits. Do not Flee. This is so that the character will not also add a Report hostile job
                //    //    //if (!this.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) { 
                //    //    //    this.marker.AddHostileInRange(criminal.owner, false);
                //    //    //}
                //    //}
                //    //TODO: job = CreateReportCrimeJob(committedCrime, crimeToReport, criminal);
                //}
                break;
            case CHARACTER_ROLE.LEADER:
            case CHARACTER_ROLE.NOBLE:
                //- If the character is a Noble or Faction Leader, the criminal will gain the relevant Crime-type trait
                //If he is a Noble or Faction Leader, he will create the Apprehend Job Type in the Location job queue instead.
                if (!isFactionless && criminal.faction == this.faction) {
                    //only add apprehend job if the criminal is part of this characters faction
                    criminal.owner.AddCriminalTrait(committedCrime, crimeAction);
                    CreateApprehendJobFor(criminal.owner);
                    //crimeAction.OnReportCrime();
                    //job = JobManager.Instance.CreateNewGoapPlanJob("Apprehend", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = actor });
                    //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = actor }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                    //job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
                    //homeArea.jobQueue.AddJobInQueue(job);
                }

                break;
            case CHARACTER_ROLE.SOLDIER:
            case CHARACTER_ROLE.BANDIT:
                //- If the character is a Soldier, the criminal will gain the relevant Crime-type trait
                if (!isFactionless && criminal.faction == this.faction) {
                    //only add apprehend job if the criminal is part of this characters faction
                    criminal.owner.AddCriminalTrait(committedCrime, crimeAction);
                    //- If the character is a Soldier, he will also create an Apprehend Job Type in his personal job queue.
                    //job = JobManager.Instance.CreateNewGoapPlanJob("Apprehend", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = actor });
                    //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = actor }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                    //job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
                    //homeArea.jobQueue.AddJobInQueue(job);
                    CreateApprehendJobFor(criminal.owner, true); //job =
                    //if (job != null) {
                    //    homeArea.jobQueue.ForceAssignCharacterToJob(job, this);
                    //}
                    //crimeAction.OnReportCrime();
                }
                break;
            default:
                break;
        }
    }
    public void AddCriminalTrait(CRIME crime, ActualGoapNode crimeAction) {
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
            case CRIME.HERETIC:
                trait = new Heretic();
                break;
            default:
                break;
        }
        if (trait != null) {
            traitContainer.AddTrait(this, trait, null, crimeAction);
        }
    }
    #endregion

    #region Mood
    public void SetMoodValue(int amount) {
        moodValue = amount;
        moodValue = Mathf.Clamp(moodValue, 1, 100);
    }
    public void AdjustMoodValue(int amount, Trait fromTrait, ActualGoapNode triggerAction = null) {
        moodValue += amount;
        moodValue = Mathf.Clamp(moodValue, 1, 100);
        if(amount < 0 && currentMoodType == CHARACTER_MOOD.DARK) {
            if (doNotDisturb > 0) {
                return;
            }
            if(currentActionNode != null && currentActionNode.action.goapType == INTERACTION_TYPE.TANTRUM) {
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
                //Note: Do not cancel jobs and plans anymore, let the job priority decide if the character will do tantrum already
                //CancelAllJobsAndPlans();
                //Create Tantrum action
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TANTRUM, INTERACTION_TYPE.TANTRUM, this, this);
                job.AddOtherData(INTERACTION_TYPE.TANTRUM, new object[] { tantrumReason });

                //tantrum.SetCannotOverrideJob(true);
                //tantrum.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                jobQueue.AddJobInQueue(job);
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

    #region Pathfinding
    public List<LocationGridTile> GetTilesInRadius(int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
        if(currentRegion.area == null) { return null; }
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        int mapSizeX = currentRegion.area.areaMap.map.GetUpperBound(0);
        int mapSizeY = currentRegion.area.areaMap.map.GetUpperBound(1);
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
                    LocationGridTile result = currentRegion.area.areaMap.map[dx, dy];
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
        } else {
            if (state.characterState == CHARACTER_STATE.COMBAT && traitContainer.GetNormalTrait<Trait>("Unconscious", "Resting") == null && isAtHomeRegion && !ownParty.icon.isTravellingOutside) {
                //Reference: https://trello.com/c/2ZppIBiI/2428-combat-available-npcs-should-be-able-to-be-aware-of-hostiles-quickly
                CombatState combatState = state as CombatState;
                float distance = Vector2.Distance(this.marker.transform.position, characterThatStartedState.marker.transform.position);
                Character targetCharacter = null;
                if (combatState.isAttacking && combatState.currentClosestHostile is Character) {
                    targetCharacter = combatState.currentClosestHostile as Character;
                }
                //Debug.Log(this.name + " distance with " + characterThatStartedState.name + " is " + distance.ToString());
                if (targetCharacter != null && this.isPartOfHomeFaction && characterThatStartedState.isAtHomeRegion && characterThatStartedState.isPartOfHomeFaction && this.IsCombatReady()
                    && this.IsHostileOutsider(targetCharacter) && (this.relationshipContainer.GetRelationshipEffectWith(characterThatStartedState.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE || characterThatStartedState.role.roleType == CHARACTER_ROLE.SOLDIER)
                    && distance <= Combat_Signalled_Distance) {
                    if (marker.AddHostileInRange(targetCharacter, processCombatBehavior: false)) {
                        if (!marker.avoidInRange.Contains(targetCharacter)) {
                            Log joinLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "join_combat_signaled");
                            joinLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            joinLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                            joinLog.AddToFillers(characterThatStartedState, characterThatStartedState.name, LOG_IDENTIFIER.CHARACTER_3);
                            joinLog.AddLogToSpecificObjects(LOG_IDENTIFIER.ACTIVE_CHARACTER, LOG_IDENTIFIER.TARGET_CHARACTER);
                            PlayerManager.Instance.player.ShowNotificationFrom(this, joinLog);
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
        }
    }
    #endregion

    #region Alter Egos
    private void InitializeAlterEgos() {
        alterEgos = new Dictionary<string, AlterEgoData> {
            {CharacterManager.Original_Alter_Ego, new AlterEgoData(this, CharacterManager.Original_Alter_Ego)}
        };
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
    private void AddAlterEgo(AlterEgoData data) {
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
            //for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            //    Trait currTrait = traitContainer.allTraits[i];
            //    if (currTrait.isRemovedOnSwitchAlterEgo) {
            //        if (traitContainer.RemoveTrait(this, currTrait)) {
            //            i--;
            //        }
            //    }
            //}
            //apply all alter ego changes here
            AlterEgoData alterEgoData = alterEgos[alterEgoName];
            //currentAlterEgo.CopySpecialTraits();

            //Drop all plans except for the current action
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETTING_CHARACTER, this as IPointOfInterest, "target is not found");
            if (currentActionNode != null) {
                CancelAllJobsExceptForCurrent();
            } else {
                CancelAllJobs();
            }

            if(alterEgoName == "Lycanthrope") {
                needsComponent.hasForcedTiredness = true;
            }
            needsComponent.SetHasCancelledSleepSchedule(false);
            needsComponent.ResetSleepTicks();
            needsComponent.ResetFullnessMeter();
            needsComponent.ResetHappinessMeter();
            needsComponent.ResetTirednessMeter();

            SetHomeStructure(alterEgoData.homeStructure);
            ChangeFactionTo(alterEgoData.faction);
            AssignRole(alterEgoData.role);
            AssignClass(alterEgoData.characterClass);
            ChangeRace(alterEgoData.race);
            SetLevel(alterEgoData.level);
            SetMaxHPMod(alterEgoData.maxHPMod);
            SetMaxHPPercentMod(alterEgoData.maxHPPercentMod);
            SetAttackMod(alterEgoData.attackPowerMod);
            SetAttackPercentMod(alterEgoData.attackPowerPercentMod);
            SetSpeedMod(alterEgoData.speedMod);
            SetSpeedPercentMod(alterEgoData.speedPercentMod);
            traitContainer.RemoveAllNonPersistentTraits(this); //remove all non persistent traits (include alter ego: false)

            //ForceCancelAllJobsTargettingCharacter(false, "target is not found");

            for (int i = 0; i < alterEgoData.traits.Count; i++) {
                traitContainer.AddTrait(this, alterEgoData.traits[i]);
            }
            currentAlterEgoName = alterEgoName;
            isSwitchingAlterEgo = false;
            visuals.UpdateAllVisuals(this);
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

    #region Converters
    public static implicit operator Relatable(Character d) => d.currentAlterEgo;
    #endregion

    #region Limiters
    public void IncreaseCanWitness() {
        canWitnessValue++;
    }
    public void DecreaseCanWitness() {
        canWitnessValue--;
    }
    public void IncreaseCanMove() {
        canMoveValue++;
    }
    public void DecreaseCanMove() {
        canMoveValue--;
    }
    public void IncreaseCanBeAttacked() {
        canBeAtttackedValue++;
    }
    public void DecreaseCanBeAttacked() {
        canBeAtttackedValue--;
    }
    #endregion

    #region IJobOwner
    public void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character) {
    }
    public void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character) {
        JobManager.Instance.OnFinishJob(job);
    }
    public bool ForceCancelJob(JobQueueItem job) {
        //JobManager.Instance.OnFinishGoapPlanJob(job);
        return true;
    }
    #endregion

    #region Build Structure Component
    public void AssignBuildStructureComponent() {
        buildStructureComponent = new BuildStructureComponent(this);
    }
    public void UnassignBuildStructureComponent() {
        buildStructureComponent = null;
    }
    #endregion

    #region IDamageable
    public bool CanBeDamaged() {
        return true;
    }
    #endregion
}