using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObject {
    //Data
    string objectName { get; }
    OBJECT_TYPE objectType { get; }
    List<ObjectState> states { get; }
    ObjectState currentState { get; }
    bool isInvisible { get; }

    void SetObjectName(string name);
    void ChangeState(ObjectState state);
    ObjectState GetState(string name);
    IObject Clone();
}
