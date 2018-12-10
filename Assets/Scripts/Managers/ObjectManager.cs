using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectManager : MonoBehaviour {
    public static ObjectManager Instance;

    [SerializeField] private List<StructureObjectComponent> structureObjectComponents;
    [SerializeField] private List<CharacterObjectComponent> characterObjectComponents;
    [SerializeField] private List<ItemObjectComponent> itemObjectComponents;
    [SerializeField] private List<NPCObjectComponent> npcObjectComponents;
    [SerializeField] private List<LandmarkObjectComponent> landmarkObjectComponents;
    [SerializeField] private List<MonsterObjectComponent> monsterObjectComponents;

    private List<StructureObj> _structureObjects;
    private List<CharacterObj> _characterObjects;
    private List<ItemObj> _itemObjects;
    private List<NPCObj> _npcObjects;
    private List<LandmarkObj> _landmarkObjects;
    private List<MonsterObj> _monsterObjects;
    private List<IObject> _allObjects;

    #region getters/setters
    public List<StructureObj> structureObjects {
        get { return _structureObjects; }
    }
    public List<CharacterObj> characterObjects {
        get { return _characterObjects; }
    }
    public List<ItemObj> itemObjects {
        get { return _itemObjects; }
    }
    public List<NPCObj> npcObjects {
        get { return _npcObjects; }
    }
    public List<LandmarkObj> landmarkObjects {
        get { return _landmarkObjects; }
    }
    public List<MonsterObj> monsterObjects {
        get { return _monsterObjects; }
    }
    public List<IObject> allObjects {
        get { return _allObjects; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        _allObjects = new List<IObject>();
        _structureObjects = new List<StructureObj>();
        _characterObjects = new List<CharacterObj>();
        _itemObjects = new List<ItemObj>();
        _npcObjects = new List<NPCObj>();
        _landmarkObjects = new List<LandmarkObj>();
        _monsterObjects = new List<MonsterObj>();
        for (int i = 0; i < monsterObjectComponents.Count; i++) {
            MonsterObjectComponent currComp = monsterObjectComponents[i];
            MonsterObj monsterObject = ConvertComponentToMonsterObject(currComp);
            SetInitialDataOfObjects(currComp, monsterObject, monsterObjectComponents[i].gameObject.name);
            _monsterObjects.Add(monsterObject);
            _allObjects.Add(monsterObject);
        }
        for (int i = 0; i < structureObjectComponents.Count; i++) {
            StructureObjectComponent currComp = structureObjectComponents[i];
            StructureObj structureObject = ConvertComponentToStructureObject(currComp);
            SetInitialDataOfObjects(currComp, structureObject);
            _structureObjects.Add(structureObject);
            _allObjects.Add(structureObject);
        }
        for (int i = 0; i < characterObjectComponents.Count; i++) {
            CharacterObjectComponent currComp = characterObjectComponents[i];
            CharacterObj characterObject = ConvertComponentToCharacterObject(currComp);
            SetInitialDataOfObjects(currComp, characterObject, characterObjectComponents[i].gameObject.name);
            _characterObjects.Add(characterObject);
            _allObjects.Add(characterObject);
        }
        for (int i = 0; i < itemObjectComponents.Count; i++) {
            ItemObjectComponent currComp = itemObjectComponents[i];
            ItemObj itemObject = itemObjectComponents[i].itemObject;
            SetInitialDataOfObjects(currComp, itemObject, itemObjectComponents[i].gameObject.name);
            _itemObjects.Add(itemObject);
            _allObjects.Add(itemObject);
        }
        for (int i = 0; i < npcObjectComponents.Count; i++) {
            NPCObjectComponent currComp = npcObjectComponents[i];
            NPCObj npcObject = npcObjectComponents[i].npcObject;
            SetInitialDataOfObjects(currComp, npcObject, npcObjectComponents[i].gameObject.name);
            _npcObjects.Add(npcObject);
            _allObjects.Add(npcObject);
        }
        for (int i = 0; i < landmarkObjectComponents.Count; i++) {
            LandmarkObjectComponent currComp = landmarkObjectComponents[i];
            LandmarkObj landmarkObject = landmarkObjectComponents[i].landmarkObject;
            SetInitialDataOfObjects(currComp, landmarkObject, landmarkObjectComponents[i].gameObject.name);
            landmarkObject.SetObjectName(landmarkObjectComponents[i].gameObject.name);
            _landmarkObjects.Add(landmarkObject);
            _allObjects.Add(landmarkObject);
        }
    }
    private void SetInitialDataOfObjects(ObjectComponent objComp, IObject iobject, string objectName = "") {
        if(!string.IsNullOrEmpty(objectName)) {
            iobject.SetObjectName(objectName);
        }
        for (int i = 0; i < objComp.states.Count; i++) {
            ObjectState state = objComp.states[i];
            state.SetObject(iobject);
            List<CharacterAction> newActions = new List<CharacterAction>();
            for (int j = 0; j < state.actions.Count; j++) {
                CharacterAction originalAction = state.actions[j];
                ConstructActionFilters(originalAction);
                originalAction.GenerateName();
                CharacterAction action = CreateNewCharacterAction(originalAction.actionType);
                originalAction.SetCommonData(action);
                action.Initialize();
                //action.SetFilters(originalAction.filters);
                ConstructPrerequisites(action, iobject);
                newActions.Add(action);
                //originalAction = action;
            }
            state.SetActions(newActions);
        }
        iobject.SetStates(objComp.states, false);
    }

    private void ConstructPrerequisites(CharacterAction action, IObject iobject) {
        if (action.actionData.resourceAmountNeeded > 0) {
            CharacterActionData copy = action.actionData;
            RESOURCE resourceType = action.actionData.resourceNeeded;
            if(resourceType == RESOURCE.NONE) {
                if (iobject.objectType == OBJECT_TYPE.STRUCTURE) {
                    resourceType = (iobject as StructureObj).madeOf;
                }
            }
            ResourcePrerequisite resourcePrerequisite = new ResourcePrerequisite(resourceType, action.actionData.resourceAmountNeeded, action, iobject);
            copy.prerequisites = new List<IPrerequisite>() { resourcePrerequisite };
            action.SetActionData(copy);
        }
    }

    private void ConstructActionFilters(CharacterAction action) {
        ActionFilter[] createdFilters = new ActionFilter[action.actionData.filters.Length];
        for (int i = 0; i < action.actionData.filters.Length; i++) {
            ActionFilterData currData = action.actionData.filters[i];
            ActionFilter createdFilter = CreateActionFilterFromData(currData);
            createdFilters[i] = createdFilter;
        }
        action.SetFilters(createdFilters);
    }

    private ActionFilter CreateActionFilterFromData(ActionFilterData data) {
        switch (data.filterType) {
            case ACTION_FILTER_TYPE.ROLE:
                return CreateRoleFilter(data);
            case ACTION_FILTER_TYPE.LOCATION:
                return CreateLandmarkFilter(data);
            case ACTION_FILTER_TYPE.CLASS:
                return CreateClassFilter(data);
            default:
                return null;
        }
    }

    private ActionFilter CreateRoleFilter(ActionFilterData data) {
        switch (data.condition) {
            case ACTION_FILTER_CONDITION.IS:
                return new MustBeRole(data.objects);
            case ACTION_FILTER_CONDITION.IS_NOT:
                return new MustNotBeRole(data.objects);
            default:
                return null;
        }
    }
    private ActionFilter CreateClassFilter(ActionFilterData data) {
        switch (data.condition) {
            case ACTION_FILTER_CONDITION.IS:
                return new MustBeClass(data.objects);
            case ACTION_FILTER_CONDITION.IS_NOT:
                return new MustNotBeClass(data.objects);
            default:
                return null;
        }
    }
    private ActionFilter CreateLandmarkFilter(ActionFilterData data) {
        switch (data.condition) {
            case ACTION_FILTER_CONDITION.IS:
                return new LandmarkMustBeState(data.objects[0]);
            default:
                return null;
        }
    }

    public IObject CreateNewObject(OBJECT_TYPE objType, string objectName) {
        if(objType == OBJECT_TYPE.STRUCTURE) {
            return GetNewStructureObject(objectName);
        }else if (objType == OBJECT_TYPE.CHARACTER) {
            return GetNewCharacterObject(objectName);
        } else if (objType == OBJECT_TYPE.MONSTER) {
            return GetNewMonsterObject(objectName);
        }
        //IObject reference = GetReference(objType, objectName);
        //if (reference != null) {
        //    IObject newObj = reference.Clone();
        //    //location.AddObject(newObj);
        //    return newObj;
        //}
        return null;
    }
    //public IObject CreateNewObject(string objectName) {
    //    return CreateNewObject(GetObjectType(objectName), objectName);
    //}
    //public IObject CreateNewObject(LANDMARK_TYPE landmarkType) {
    //    string objectName = Utilities.NormalizeStringUpperCaseFirstLetters(landmarkType.ToString());
    //    return CreateNewObject(objectName);
    //}
    //private IObject GetReference(OBJECT_TYPE objType, string objectName) {
    //    for (int i = 0; i < _allObjects.Count; i++) {
    //        IObject currObject = _allObjects[i];
    //        if (currObject.objectType == objType && currObject.objectName.Equals(objectName)) {
    //            return currObject;
    //        }
    //    }
    //    return null;
    //}
    public OBJECT_TYPE GetObjectType(string objectName) {
        for (int i = 0; i < _allObjects.Count; i++) {
            IObject currObject = _allObjects[i];
            if (currObject.objectName.Equals(objectName)) {
                return currObject.objectType;
            }
        }
        throw new System.Exception("Object with the name " + objectName + " does not exist!");
    }

    public CharacterAction CreateNewCharacterAction(ACTION_TYPE actionType) {
        switch (actionType) {
            //case ACTION_TYPE.BUILD:
            //return new BuildAction(state);
            case ACTION_TYPE.DESTROY:
                return new DestroyAction();
            case ACTION_TYPE.REST:
                return new RestAction();
            case ACTION_TYPE.HUNT:
                return new HuntAction();
            case ACTION_TYPE.EAT:
                return new EatAction();
            case ACTION_TYPE.DRINK:
                return new DrinkAction();
            case ACTION_TYPE.IDLE:
                return new IdleAction();
            case ACTION_TYPE.POPULATE:
                return new PopulateAction();
            case ACTION_TYPE.HARVEST:
                return new HarvestAction();
            case ACTION_TYPE.TORTURE:
                return new TortureAction();
            case ACTION_TYPE.PATROL:
                return new PatrolAction();
            case ACTION_TYPE.REPAIR:
                return new RepairAction();
            case ACTION_TYPE.ABDUCT:
                return new AbductAction();
            case ACTION_TYPE.PRAY:
                return new PrayAction();
            case ACTION_TYPE.ATTACK:
                return new AttackAction();
            case ACTION_TYPE.JOIN_BATTLE:
                return new JoinBattleAction();
            case ACTION_TYPE.GO_HOME:
                return new GoHomeAction();
            case ACTION_TYPE.RELEASE:
                return new ReleaseAction();
            case ACTION_TYPE.CLEANSE:
                return new CleanseAction();
            case ACTION_TYPE.ENROLL:
                return new EnrollAction();
            case ACTION_TYPE.TRAIN:
                return new TrainAction();
            case ACTION_TYPE.DAYDREAM:
                return new DaydreamAction();
            case ACTION_TYPE.BERSERK:
                return new BerserkAction();
            case ACTION_TYPE.SELFMUTILATE:
                return new SelfMutilateAction();
            case ACTION_TYPE.FLAIL:
                return new FlailAction();
            case ACTION_TYPE.PLAY:
                return new PlayAction();
            case ACTION_TYPE.CHAT:
                return new ChatAction();
            case ACTION_TYPE.ARGUE:
                return new ArgueAction();
            case ACTION_TYPE.DEPOSIT:
                return new DepositAction();
            case ACTION_TYPE.CHANGE_CLASS:
                return new ChangeClassAction();
            case ACTION_TYPE.WAITING:
                return new WaitingInteractionAction();
            case ACTION_TYPE.FORM_PARTY:
                return new FormPartyAction();
            case ACTION_TYPE.DISBAND_PARTY:
                return new DisbandPartyAction();
            case ACTION_TYPE.IN_PARTY:
                return new InPartyAction();
            case ACTION_TYPE.JOIN_PARTY:
                return new JoinPartyAction();
            case ACTION_TYPE.GRIND:
                return new GrindAction();
            case ACTION_TYPE.MOVE_TO:
                return new MoveToAction();
            case ACTION_TYPE.READ:
                return new ReadAction();
            case ACTION_TYPE.PARTY:
                return new PartyAction();
            case ACTION_TYPE.MINING:
                return new MiningAction();
            case ACTION_TYPE.WOODCUTTING:
                return new WoodcuttingAction();
            case ACTION_TYPE.WORKING:
                return new WorkingAction();
            case ACTION_TYPE.QUESTING:
                return new QuestingAction();
            case ACTION_TYPE.FETCH:
                return new FetchAction();
            case ACTION_TYPE.WAIT_FOR_PARTY:
                return new WaitForPartyAction();
            case ACTION_TYPE.FOOLING_AROUND:
                return new FoolingAroundAction();
            case ACTION_TYPE.TURN_IN_QUEST:
                return new TurnInQuestAction();
            case ACTION_TYPE.SING:
                return new SingAction();
            case ACTION_TYPE.PLAYING_INSTRUMENT:
                return new PlayingInstrumentAction();
            case ACTION_TYPE.SUICIDE:
                return new SuicideAction();
            case ACTION_TYPE.DEFEND:
                return new DefendAction();
            case ACTION_TYPE.RESEARCH:
                return new ResearchAction();
            case ACTION_TYPE.GIVE_ITEM:
                return new GiveItemAction();
            case ACTION_TYPE.MEDITATE:
                return new MeditateAction();
            case ACTION_TYPE.HOUSEKEEPING:
                return new HousekeepingAction();
            case ACTION_TYPE.STALK:
                return new StalkAction();
            case ACTION_TYPE.ATTACK_LANDMARK:
                return new AttackLandmarkAction();
            case ACTION_TYPE.RAID_LANDMARK:
                return new RaidLandmarkAction();
            case ACTION_TYPE.HIBERNATE:
                return new HibernateAction();
            case ACTION_TYPE.DEFENDER:
                return new DefenderAction();
        }
        return null;
    }
    public StructureObj ConvertComponentToStructureObject(StructureObjectComponent component) {
        StructureObj structureObj = null;
        switch (component.specificObjectType) {
            case LANDMARK_TYPE.DEMONIC_PORTAL:
                structureObj = new DemonicPortal();
                break;
            case LANDMARK_TYPE.GARRISON:
                structureObj = new Garrison();
                break;
            case LANDMARK_TYPE.IRON_MINES:
                structureObj = new IronMines();
                break;
            case LANDMARK_TYPE.INN:
                structureObj = new Inn();
                break;
            case LANDMARK_TYPE.HUNTING_GROUNDS:
                structureObj = new HuntingGrounds();
                break;
            case LANDMARK_TYPE.HOUSES:
                structureObj = new Houses();
                break;
            case LANDMARK_TYPE.MONSTER_DEN:
                structureObj = new MonsterDen();
                break;
            case LANDMARK_TYPE.SNATCHER_DEMONS_LAIR:
                structureObj = new SnatcherDemonsLair();
                break;
            case LANDMARK_TYPE.SHOP:
                structureObj = new Shop();
                break;
            case LANDMARK_TYPE.FARM:
                structureObj = new Farm();
                break;
            case LANDMARK_TYPE.GOLD_MINE:
                structureObj = new GoldMine();
                break;
            case LANDMARK_TYPE.LUMBERYARD:
                structureObj = new Lumberyard();
                break;
            case LANDMARK_TYPE.PALACE:
                structureObj = new Palace();
                break;
            case LANDMARK_TYPE.CAMP:
                structureObj = new Camp();
                break;
            case LANDMARK_TYPE.LAIR:
                structureObj = new Lair();
                break;
            case LANDMARK_TYPE.ABANDONED_MINE:
                structureObj = new AbandonedMine();
                break;
            case LANDMARK_TYPE.BANDIT_CAMP:
                structureObj = new BanditCamp();
                break;
            case LANDMARK_TYPE.HERMIT_HUT:
                structureObj = new HermitHut();
                break;
            case LANDMARK_TYPE.CATACOMB:
                structureObj = new Catacomb();
                break;
            case LANDMARK_TYPE.PYRAMID:
                structureObj = new Pyramid();
                break;
            case LANDMARK_TYPE.EXILE_CAMP:
                structureObj = new ExileCamp();
                break;
            case LANDMARK_TYPE.GIANT_SKELETON:
                structureObj = new GiantSkeleton();
                break;
            case LANDMARK_TYPE.ANCIENT_TEMPLE:
                structureObj = new AncientTemple();
                break;
            case LANDMARK_TYPE.CAVE:
                structureObj = new Cave();
                break;
            case LANDMARK_TYPE.ICE_PIT:
                structureObj = new IcePit();
                break;
            case LANDMARK_TYPE.MANA_EXTRACTOR:
                structureObj = new ManaExtractor();
                break;
            case LANDMARK_TYPE.BARRACKS:
                structureObj = new Barracks();
                break;
            case LANDMARK_TYPE.MINIONS_HOLD:
                structureObj = new MinionsHold();
                break;
            case LANDMARK_TYPE.DWELLINGS:
                structureObj = new Dwellings();
                break;
            case LANDMARK_TYPE.RAMPART:
                structureObj = new Rampart();
                break;
            case LANDMARK_TYPE.CORRUPTION_NODE:
                structureObj = new CorruptionNode();
                break;
            case LANDMARK_TYPE.RITUAL_CIRCLE:
                structureObj = new RitualCircle();
                break;
            case LANDMARK_TYPE.DRAGON_CAVE:
                structureObj = new DragonCave();
                break;
            case LANDMARK_TYPE.SKELETON_CEMETERY:
                structureObj = new SkeletonCemetery();
                break;
            case LANDMARK_TYPE.HIVE_LAIR:
                structureObj = new HiveLair();
                break;
            case LANDMARK_TYPE.ZOMBIE_PYRAMID:
                structureObj = new ZombiePyramid();
                break;
            case LANDMARK_TYPE.IMP_KENNEL:
                structureObj = new ImpKennel();
                break;
            case LANDMARK_TYPE.CEMETERY:
                structureObj = new Cemetery();
                break;
            case LANDMARK_TYPE.TRAINING_ARENA:
                structureObj = new TrainingArena();
                break;
            case LANDMARK_TYPE.PENANCE_TEMPLE:
                structureObj = new PenanceTemple();
                break;
            default:
                throw new System.Exception("No structure for " + component.specificObjectType.ToString() + " has been created yet!");
        }
        component.CopyDataToStructureObject(structureObj);
        return structureObj;
    }
    public CharacterObj ConvertComponentToCharacterObject(CharacterObjectComponent component) {
        CharacterObj characterObj = new CharacterObj(null);
        component.CopyDataToCharacterObject(characterObj);
        return characterObj;
    }
    public MonsterObj ConvertComponentToMonsterObject(MonsterObjectComponent component) {
        MonsterObj monsterObj = new MonsterObj(null);
        component.CopyDataToCharacterObject(monsterObj);
        return monsterObj;
    }
    public StructureObj GetNewStructureObject(string name) {
        for (int i = 0; i < _structureObjects.Count; i++) {
            if(_structureObjects[i].objectName == name) {
                return _structureObjects[i].Clone() as StructureObj;
            }
        }
        return null;
    }
    public CharacterObj GetNewCharacterObject(string name) {
        for (int i = 0; i < _characterObjects.Count; i++) {
            if (_characterObjects[i].objectName == name) {
                return _characterObjects[i].Clone() as CharacterObj;
            }
        }
        return null;
    }
    public MonsterObj GetNewMonsterObject(string name) {
        for (int i = 0; i < _monsterObjects.Count; i++) {
            if (_monsterObjects[i].objectName == name) {
                return _monsterObjects[i].Clone() as MonsterObj;
            }
        }
        return null;
    }
}
