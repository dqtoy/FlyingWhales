using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Interaction {

    public Attack(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.ATTACK, 0) {
        _name = "Attack";
        _jobFilter = new JOB[] { JOB.DIPLOMAT };
    }
}
