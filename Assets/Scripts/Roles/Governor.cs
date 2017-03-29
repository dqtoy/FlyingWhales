using UnityEngine;
using System.Collections;

public class Governor : Role {

	public City ownedCity;

	public Governor(Citizen citizen): base(citizen){

	}

	internal void SetOwnedCity(City ownedCity){
		this.ownedCity = ownedCity;
	}
}
