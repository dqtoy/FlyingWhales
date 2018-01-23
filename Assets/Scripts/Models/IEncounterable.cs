using UnityEngine;
using System.Collections;

public interface IEncounterable {

    void StartEncounter(ECS.Character encounteredBy);
    void StartEncounter(Party encounteredBy);
}
