using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Spy : Job {

    public Spy(Character character) : base(character, JOB.SPY) { }
}
