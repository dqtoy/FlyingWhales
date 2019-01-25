using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionData {
    bool isHalted { get; }
    bool isDoneAction { get; }

    void AssignAction(CharacterAction action, IObject targetObject, Quest associatedQuest = null, GameEvent associatedEvent = null);
    void SetIsHalted(bool state);
}
