using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Farmer : Job {
    public Farmer(CharacterRole role) : base(role) {
        _jobType = CHARACTER_JOB.FARMER;
    }
}
