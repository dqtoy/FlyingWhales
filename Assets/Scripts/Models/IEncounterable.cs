using UnityEngine;
using System.Collections;

public interface IEncounterable {

    string encounterName { get;}

    bool StartEncounter(ECS.Character encounteredBy); //will return true/false if the encounter was successful or not
    bool StartEncounter(Party encounteredBy);
}
