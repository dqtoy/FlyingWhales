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
            StructureObj structureObject = structureObjectComponents[i].structureObject;
            SetInitialDataOfObjects(structureObject, structureObjectComponents[i].gameObject.name);
            _structureObjects.Add(structureObject);
            _allObjects.Add(structureObject);
        }
        for (int i = 0; i < characterObjectComponents.Count; i++) {
            CharacterObj characterObject = characterObjectComponents[i].characterObject;
            SetInitialDataOfObjects(characterObject, characterObjectComponents[i].gameObject.name);
            _characterObjects.Add(characterObject);
            _allObjects.Add(characterObject);
        }
        for (int i = 0; i < itemObjectComponents.Count; i++) {
            ItemObj itemObject = itemObjectComponents[i].itemObject;
            SetInitialDataOfObjects(itemObject, itemObjectComponents[i].gameObject.name);
            _itemObjects.Add(itemObject);
            _allObjects.Add(itemObject);
        }
        for (int i = 0; i < npcObjectComponents.Count; i++) {
            NPCObj npcObject = npcObjectComponents[i].npcObject;
            SetInitialDataOfObjects(npcObject, npcObjectComponents[i].gameObject.name);
            _npcObjects.Add(npcObject);
            _allObjects.Add(npcObject);
        }
    }

    private void SetInitialDataOfObjects(IObject iobject, string objectName) {
        iobject.SetObjectName(objectName);
        for (int i = 0; i < iobject.states.Count; i++) {
            ObjectState state = iobject.states[i];
            state.SetObject(iobject);
            for (int j = 0; j < state.actions.Count; j++) {
                CharacterAction action = state.actions[j];
                action.SetObjectState(state);
                action.GenerateName();
            }
        }
    }

    private IObject GetReference(OBJECT_TYPE objType, string objName) {
        for (int i = 0; i < _allObjects.Count; i++) {
            IObject currObject = _allObjects[i];
            if (currObject.objectType == objType && currObject.objectName.Equals(objName)) {
                return currObject;
            }
        }
        return null;
    }
    public OBJECT_TYPE GetObjectType(string objName) {
        for (int i = 0; i < _allObjects.Count; i++) {
            IObject currObject = _allObjects[i];
            if (currObject.objectName.Equals(objName)) {
                return currObject.objectType;
            }
        }
        throw new System.Exception("Object with the name " + objName + " does not exist!");
    }


    public IObject CreateNewObject(OBJECT_TYPE objType, string objName, BaseLandmark location) {
        IObject reference = GetReference(objType, objName);
        if (reference != null) {
            IObject newObj = reference.Clone();
            location.AddObject(newObj);
        }
        return null;
    }
    public IObject CreateNewObject(string objName, BaseLandmark location) {
        return CreateNewObject(GetObjectType(objName), objName, location);
    }

    #region Wild Pigs
    public void WildPigsChangeToDepleted(ObjectState state) {
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < 15) {

        }
    }
    public void WildPigsChangeToTeeming() {

    }
    #endregion
}
