using UnityEngine;
using System.Collections;

public class Governor : Role {

	public City ownedCity;

	public Governor(Citizen citizen): base(citizen){
		this.citizen.city.governor = this.citizen;
		this.citizen.workLocation = this.citizen.city.hexTile;
		this.citizen.isGovernor = true;
		this.citizen.isKing = false;
		this.SetOwnedCity(this.citizen.city);
	}

	internal void SetOwnedCity(City ownedCity){
		this.ownedCity = ownedCity;
	}
}
