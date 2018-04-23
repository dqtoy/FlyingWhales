using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Structure Object", menuName = "Objects/New Structure Object")]
public class StructureObj : ScriptableObject, IStructureObject {
    [SerializeField] private OBJECT_TYPE _objectType;
    [SerializeField] private bool _isInvisible;
    [SerializeField] private int _maxHP;
    [SerializeField] private List<ObjectState> _states;

    private ObjectState _currentState;
    private int _currentHP;

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
    public int maxHP {
        get { return _maxHP; }
    }
    public int currentHP {
        get { return _currentHP; }
    }
    #endregion

    public StructureObj() {
     
    }

    public void ChangeState(ObjectState state) {

    }

    public void AdjustHP(int amount) {
        //When hp reaches 0 or 100 a function will be called
    }
   
}
