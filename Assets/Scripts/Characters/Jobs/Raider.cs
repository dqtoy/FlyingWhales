using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Raider : Job {

    public Raider(Character character) : base(character, JOB.RAIDER) { }
}
