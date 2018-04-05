using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class CombatThread {
	public enum TODO{
		FIND_PATH,
	}

	private TODO _todo;

	private ECS.Combat combat;
	private List<ECS.Character> characterSideA;
	private List<ECS.Character> characterSideB;

	public void StartSimulation(){
		
	}

	public void ReturnResults(){
		
	}
}
