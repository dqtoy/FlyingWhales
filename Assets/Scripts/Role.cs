using UnityEngine;
using System.Collections;

public class Role {
	public Citizen citizen;

	public Role(Citizen citizen){
		this.citizen = citizen;
	}
	internal virtual int[] GetResourceProduction(){return new int[]{ 0, 0, 0, 0, 0 };}
}
