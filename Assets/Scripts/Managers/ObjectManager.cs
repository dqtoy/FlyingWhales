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
            state.SetObject(iobject);
            for (int j = 0; j < state.actions.Count; j++) {
                CharacterAction action = state.actions[j];
                action.SetObjectState(state);
            }
        }
    }
}
