using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable {
    string name { get; }
    bool isBeingInspected { get; }
    bool hasBeenInspected { get; }
    Faction faction { get; }
    Area specificLocation { get; }
    //List<Interaction> currentInteractions { get; }

    void SetIsBeingInspected(bool state);
    void SetHasBeenInspected(bool state);
    //void AddInteraction(Interaction interaction);
    //void RemoveInteraction(Interaction interaction);
    void EndedInspection();
}
