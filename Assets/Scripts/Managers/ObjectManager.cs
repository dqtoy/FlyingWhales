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
        for (int i = 0; i < monsterObjectComponents.Count; i++) {
            MonsterObjectComponent currComp = monsterObjectComponents[i];
            MonsterObj monsterObject = ConvertComponentToMonsterObject(currComp);
            SetInitialDataOfObjects(currComp, monsterObject, monsterObjectComponents[i].gameObject.name);
            _monsterObjects.Add(monsterObject);
            _allObjects.Add(monsterObject);
        }
    }
    private void SetInitialDataOfObjects(ObjectComponent objComp, IObject iobject, string objectName = "") {
        if(objectName != string.Empty) {
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

        }
        return null;
    }
    public StructureObj ConvertComponentToStructureObject(StructureObjectComponent component) {
        StructureObj structureObj = null;
        switch (component.specificObjectType) {
            case LANDMARK_TYPE.DEMONIC_PORTAL:
                structureObj = new DemonicPortal();
                break;
            case LANDMARK_TYPE.ELVEN_SETTLEMENT:
                structureObj = new ElvenSettlement();
                break;
            case LANDMARK_TYPE.HUMAN_SETTLEMENT:
                structureObj = new HumanSettlement();
                break;
            case LANDMARK_TYPE.GARRISON:
                structureObj = new Garrison();
                break;
            case LANDMARK_TYPE.OAK_FORTIFICATION:
                structureObj = new OakFortification();
                break;
            case LANDMARK_TYPE.IRON_FORTIFICATION:
                structureObj = new IronFortification();
                break;
            case LANDMARK_TYPE.OAK_LUMBERYARD:
                structureObj = new OakLumberyard();
                break;
            case LANDMARK_TYPE.IRON_MINES:
                structureObj = new IronMines();
                break;
            case LANDMARK_TYPE.INN:
                structureObj = new Inn();
                break;
            case LANDMARK_TYPE.PUB:
                structureObj = new Pub();
                break;
            case LANDMARK_TYPE.TEMPLE:
                structureObj = new Temple();
                break;
            case LANDMARK_TYPE.HUNTING_GROUNDS:
                structureObj = new HuntingGrounds();
                break;
            case LANDMARK_TYPE.ELVEN_HOUSES:
                structureObj = new ElvenHouses();
                break;
            case LANDMARK_TYPE.HUMAN_HOUSES:
                structureObj = new HumanHouses();
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
