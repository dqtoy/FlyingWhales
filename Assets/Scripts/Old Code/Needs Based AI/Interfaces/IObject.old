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
    ILocation specificLocation { get; }
    RESOURCE madeOf { get; }
    Dictionary<RESOURCE, int> resourceInventory { get; }

    void SetStates(List<ObjectState> states, bool autoChangeState = true);
    void SetObjectName(string name);
    void ChangeState(ObjectState state);
    void StartState(ObjectState state);
    void EndState(ObjectState state);
    void SetObjectLocation(BaseLandmark newLocation);
    void OnAddToLandmark(BaseLandmark newLocation);
    void AdjustResource(RESOURCE resource, int amount);

    ObjectState GetState(string name);
    IObject Clone();
}
