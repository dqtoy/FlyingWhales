using UnityEngine;
using System.Collections;

public class Relationship<T> {

	public T objectInRelationship;
//	public DECISION previousDecision;
//	public LORD_EVENTS previousInteraction = LORD_EVENTS.NONE;
	public bool isAdjacent;
	public bool isAtWar;
//	public int daysAtWar;

	public Relationship(T objectInRelationship){
		this.objectInRelationship = objectInRelationship;
		this.isAtWar = false;
		this.isAdjacent = false;
	}


}
