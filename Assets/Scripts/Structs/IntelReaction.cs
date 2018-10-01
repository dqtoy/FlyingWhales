using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct IntelReaction {

    public int intelID;
    public GAME_EVENT reaction;

    public IntelReaction(int intelID, GAME_EVENT reaction) {
        this.intelID = intelID;
        this.reaction = reaction;
    }
	
}
