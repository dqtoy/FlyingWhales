using UnityEngine;
using System.Collections;

public class Role : MonoBehaviour {
	public Citizen citizen;

	internal virtual int[] GetResourceProduction(){return new int[]{ 0, 0, 0, 0, 0 };}
}
