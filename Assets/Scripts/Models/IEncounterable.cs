using UnityEngine;
using System.Collections;

public interface IEncounterable {

    string encounterName { get;}

    void StartEncounter(ECS.Character encounteredBy); //will return true/false if the encounter was successful or not
    void StartEncounter(Party encounteredBy);
	void ReturnResults(object result);
}
