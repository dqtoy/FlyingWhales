using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Shopkeeper : Job {
    public Shopkeeper(CharacterRole role) : base(role) {
        _jobType = CHARACTER_JOB.SHOPKEEPER;
    }
}
