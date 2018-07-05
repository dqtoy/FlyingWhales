using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Woodcutter : Job {
    public Woodcutter(CharacterRole role) : base(role) {
        _jobType = CHARACTER_JOB.WOODCUTTER;
    }
}