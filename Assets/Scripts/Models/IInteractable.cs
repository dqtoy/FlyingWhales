using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable {
    HiddenDesire hiddenDesire { get; }
    bool isBeingInspected { get; }
    bool hasBeenInspected { get; }
    ILocation specificLocation { get; }
    List<Interaction> currentInteractions { get; }

    void SetIsBeingInspected(bool state);
    void SetHasBeenInspected(bool state);
    void AddInteraction(Interaction interaction);
    void RemoveInteraction(Interaction interaction);
    void EndedInspection();
}
