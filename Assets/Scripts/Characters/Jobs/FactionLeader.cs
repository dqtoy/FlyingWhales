using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionLeader : Job {

    public FactionLeader(Character character) : base(character, JOB.FACTION_LEADER) {
        _actionDuration = 1;
    }
}
