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
                CharacterAction action = CreateNewCharacterAction(state.actions[j].actionType, state);
                state.actions[j].SetCommonData(action);
            }
        }
    }

    public IObject CreateNewObject(OBJECT_TYPE objType, SPECIFIC_OBJECT_TYPE specificObjectType, BaseLandmark location) {
        IObject reference = GetReference(objType, specificObjectType);
        if (reference != null) {
            IObject newObj = reference.Clone();
            location.AddObject(newObj);
        }
        return null;
    }
    public IObject CreateNewObject(SPECIFIC_OBJECT_TYPE specificObjType, BaseLandmark location) {
        return CreateNewObject(GetObjectType(specificObjType), specificObjType, location);
    }
    private IObject GetReference(OBJECT_TYPE objType, SPECIFIC_OBJECT_TYPE specificObjType) {
        for (int i = 0; i < _allObjects.Count; i++) {
            IObject currObject = _allObjects[i];
            if (currObject.objectType == objType && currObject.specificObjType == specificObjType) {
                return currObject;
            }
        }
        return null;
    }
    public OBJECT_TYPE GetObjectType(SPECIFIC_OBJECT_TYPE specificObjType) {
        for (int i = 0; i < _allObjects.Count; i++) {
            IObject currObject = _allObjects[i];
            if (currObject.specificObjType == specificObjType) {
                return currObject.objectType;
            }
        }
        throw new System.Exception("Object with the name " + specificObjType.ToString() + " does not exist!");
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
        }
        return null;
    }
}
