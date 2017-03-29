using UnityEngine;
using System.Collections;

public class Envoy : Role {

	public int successfulMissions;
	public int unsuccessfulMissions;

	public Envoy(Citizen citizen): base(citizen){
		this.successfulMissions = 0;
		this.unsuccessfulMissions = 0;
	}
}
