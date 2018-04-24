using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour {
    public static ObjectManager Instance;

    public List<StructureObj> structureObjects;
    public List<CharacterObj> characterObjects;
    public List<ItemObj> itemObjects;
    public List<NPCObj> npcObjects;

    private List<IObject> _allObjects;

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        _allObjects = new List<IObject>();
        for (int i = 0; i < structureObjects.Count; i++) {
            SetInitialDataOfObjects(structureObjects[i]);
            _allObjects.Add(structureObjects[i]);
        }
        for (int i = 0; i < characterObjects.Count; i++) {
            SetInitialDataOfObjects(characterObjects[i]);
            _allObjects.Add(characterObjects[i]);
        }
        for (int i = 0; i < itemObjects.Count; i++) {
            SetInitialDataOfObjects(itemObjects[i]);
            _allObjects.Add(itemObjects[i]);
        }
        for (int i = 0; i < npcObjects.Count; i++) {
            SetInitialDataOfObjects(npcObjects[i]);
            _allObjects.Add(npcObjects[i]);
        }
    }

    private void SetInitialDataOfObjects(IObject iobject) {
        for (int i = 0; i < iobject.states.Count; i++) {
            ObjectState state = iobject.states[i];
            //state.SetObject(iobject);
            for (int j = 0; j < state.actions.Count; j++) {
                CharacterAction action = state.actions[j];
                //action.SetObjectState(state);
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
}
