using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEncountered : Interaction {
    public CharacterEncountered(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.CHARACTER_ENCOUNTERED, 0) {

    }
}
