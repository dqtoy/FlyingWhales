using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable {
    HiddenDesire hiddenDesire { get; }
    bool isBeingInspected { get; }
    bool hasBeenInspected { get; }

    void SetIsBeingInspected(bool state);
    void SetHasBeenInspected(bool state);
    void EndedInspection();
}
