using UnityEngine;
using System.Collections;

public class King : Role {

	public Kingdom ownedKingdom;

	public King(Citizen citizen): base(citizen){

	}

	internal void SetOwnedKingdom(Kingdom ownedKingdom){
		this.ownedKingdom = ownedKingdom;
	}
}
