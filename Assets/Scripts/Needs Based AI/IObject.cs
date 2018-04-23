using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObject {
    //Data
    string objectName { get; }
    List<ObjectState> states { get; }
    ObjectState currentState { get; }
    bool isInvisible { get; }
    int foodAdvertisementValue { get; }
    int energyAdvertisementValue { get; }
    int joyAdvertisementValue { get; }
    int prestigeAdvertisementValue { get; }

    void ChangeState(ObjectState state);
    CharacterAction GetAction(ACTION_TYPE type);

}
