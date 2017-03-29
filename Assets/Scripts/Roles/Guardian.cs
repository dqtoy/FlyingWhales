using UnityEngine;
using System.Collections;

public class Guardian : Role {

	public int successfulMissions;
	public int unsuccessfulMissions;

	public Guardian(Citizen citizen): base(citizen){
		this.successfulMissions = 0;
		this.unsuccessfulMissions = 0;
	}
}
