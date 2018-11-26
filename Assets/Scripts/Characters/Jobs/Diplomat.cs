using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Diplomat : Job {

	public Diplomat(Character character) : base(character, JOB.DIPLOMAT) { }

}
