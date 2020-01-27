using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatComponent {
	public Character owner { get; private set; }

	public CombatComponent(Character owner) {
		this.owner = owner;
	}
}
