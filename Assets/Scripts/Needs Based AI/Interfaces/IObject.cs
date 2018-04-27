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
    BaseLandmark objectLocation { get; }

    void SetStates(List<ObjectState> states);
    void SetObjectName(string name);
    void ChangeState(ObjectState state);
    void SetObjectLocation(BaseLandmark newLocation);
    ObjectState GetState(string name);
    IObject Clone();
}
