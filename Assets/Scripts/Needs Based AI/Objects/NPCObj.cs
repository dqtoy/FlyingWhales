using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Object", menuName = "Objects/New NPC Object")]
public class NPCObj : ScriptableObject, ICharacterObject {
    [SerializeField] private OBJECT_TYPE _objectType;
    [SerializeField] private bool _isInvisible;
    [SerializeField] private List<ObjectState> _states;

    private ObjectState _currentState;

    #region getters/setters
    public string objectName {
        get { return this.name; }
    }
    public OBJECT_TYPE objectType {
        get { return _objectType; }
    }
    public List<ObjectState> states {
        get { return _states; }
    }
    public ObjectState currentState {
        get { return _currentState; }
    }
    public bool isInvisible {
        get { return _isInvisible; }
    }
    #endregion

    public NPCObj() {
     
    }

    public void ChangeState(ObjectState state) {

    }
}
