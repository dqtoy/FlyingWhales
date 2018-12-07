using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderJob : Job {

    public LeaderJob(Character character) : base(character, JOB.LEADER) {
        _actionDuration = 1;
    }
}
