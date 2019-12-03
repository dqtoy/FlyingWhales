using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is for characters
//If this is not empty the character will do idle actions and needs inside the structure referenced here
//If the duration for this is zero (0), the data will not be cleared out until it is forced to be cleared out
public class TrapStructure {
    public LocationStructure structure { get; private set; }
    public int duration { get; private set; }
    public int currentDuration { get; private set; }

    //This will set the structure and its duration, as well as reset the current duration
    public void SetStructureAndDuration(LocationStructure structure, int duration) {
        this.structure = structure;
        this.duration = duration;
        currentDuration = 0;
    }

    public void IncrementCurrentDuration(int amount) {
        if(duration > 0 && structure != null) {
            currentDuration += amount;
            if (currentDuration >= duration) {
                //Clear out data, duration is reached;
                SetStructureAndDuration(null, 0);
            }
        }
    }
}
