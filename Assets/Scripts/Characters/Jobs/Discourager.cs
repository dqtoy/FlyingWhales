using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Discourager : Job {

    public Discourager(Character character) : base(character, JOB.DISCOURAGER) { }
}
