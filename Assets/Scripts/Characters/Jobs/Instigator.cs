using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Instigator : Job {

    public Instigator(Character character) : base(character, JOB.INSTIGATOR) { }
}
