using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Miner : Job {
    public Miner(CharacterRole role) : base(role) {
        _jobType = CHARACTER_JOB.MINER;
    }
}