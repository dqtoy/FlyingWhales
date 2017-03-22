using UnityEngine;
using System.Collections;

public class Role {
	public Citizen citizen;

	internal virtual int[] GetResourceProduction(){return new int[]{ 0, 0, 0, 0, 0 };}
}
