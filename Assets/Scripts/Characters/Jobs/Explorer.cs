using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Explorer : Job {

    public Explorer(Character character) : base(character, JOB.EXPLORER) { }
}
