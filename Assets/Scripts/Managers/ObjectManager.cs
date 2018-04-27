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

    private List<StructureObj> _structureObjects;
    private List<CharacterObj> _characterObjects;
    private List<ItemObj> _itemObjects;
    private List<NPCObj> _npcObjects;
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
        for (int i = 0; i < structureObjectComponents.Count; i++) {
            StructureObjectComponent currComp = structureObjectComponents[i];
            StructureObj structureObject = currComp.structureObject;
            SetInitialDataOfObjects(currComp, structureObject, structureObjectComponents[i].gameObject.name);
            _structureObjects.Add(structureObject);
            _allObjects.Add(structureObject);
        }
        for (int i = 0; i < characterObjectComponents.Count; i++) {
            CharacterObjectComponent currComp = characterObjectComponents[i];
            CharacterObj characterObject = characterObjectComponents[i].characterObject;
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
    }
    private void SetInitialDataOfObjects(ObjectComponent objComp, IObject iobject, string objectName) {
        iobject.SetObjectName(objectName);
        iobject.SetStates(objComp.states);
        for (int i = 0; i < iobject.states.Count; i++) {
            ObjectState state = iobject.states[i];
            state.SetObject(iobject);
            for (int j = 0; j < state.actions.Count; j++) {
                CharacterAction originalAction = state.actions[j];
                ConstructActionFilters(originalAction);
                originalAction.GenerateName();
                CharacterAction action = CreateNewCharacterAction(originalAction.actionType, state);
                originalAction.SetCommonData(action);
                action.SetFilters(originalAction.filters);
                originalAction = action;
            }
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
    private ActionFilter CreateLandmarkFilter(ActionFilterData data) {
        switch (data.condition) {
            case ACTION_FILTER_CONDITION.IS:
                return new LandmarkMustBeState(data.objects[0]);
            default:
                return null;
        }
    }

    public IObject CreateNewObject(OBJECT_TYPE objType, string objectName, BaseLandmark location) {
        IObject reference = GetReference(objType, objectName);
        if (reference != null) {
            IObject newObj = reference.Clone();
            location.AddObject(newObj);
        }
        return null;
    }
    public IObject CreateNewObject(string objectName, BaseLandmark location) {
        return CreateNewObject(GetObjectType(objectName), objectName, location);
    }
    private IObject GetReference(OBJECT_TYPE objType, string objectName) {
        for (int i = 0; i < _allObjects.Count; i++) {
            IObject currObject = _allObjects[i];
            if (currObject.objectType == objType && currObject.objectName.Equals(objectName)) {
                return currObject;
            }
        }
        return null;
    }
    public OBJECT_TYPE GetObjectType(string objectName) {
        for (int i = 0; i < _allObjects.Count; i++) {
            IObject currObject = _allObjects[i];
            if (currObject.objectName.Equals(objectName)) {
                return currObject.objectType;
            }
        }
        throw new System.Exception("Object with the name " + objectName + " does not exist!");
    }

    public CharacterAction CreateNewCharacterAction(ACTION_TYPE actionType, ObjectState state) {
        switch (actionType) {
            case ACTION_TYPE.BUILD:
            return new BuildAction(state);
            case ACTION_TYPE.DESTROY:
            return new DestroyAction(state);
            case ACTION_TYPE.REST:
            return new RestAction(state);
            case ACTION_TYPE.HUNT:
            return new HuntAction(state);
            case ACTION_TYPE.EAT:
            return new EatAction(state);
            case ACTION_TYPE.DRINK:
            return new DrinkAction(state);
            case ACTION_TYPE.IDLE:
            return new IdleAction(state);
        }
        return null;
    }
    public StructureObj GetNewStructureObject(string name) {
        for (int i = 0; i < _structureObjects.Count; i++) {
            if(_structureObjects[i].objectName == name) {
                return _structureObjects[i].Clone() as StructureObj;
            }
        }
        return null;
    }
}
