using UnityEngine;
using System.Collections;

public class Spy : Role {

	public int successfulMissions;
	public int unsuccessfulMissions;

	public Spy(Citizen citizen): base(citizen){
		this.successfulMissions = 0;
		this.unsuccessfulMissions = 0;
	}
}
