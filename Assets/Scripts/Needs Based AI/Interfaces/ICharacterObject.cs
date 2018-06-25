using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterObject {
    ICharacter icharacter { get; }
    ObjectState currentState { get; }

    ObjectState GetState(string state);
    void ChangeState(ObjectState state);
}
