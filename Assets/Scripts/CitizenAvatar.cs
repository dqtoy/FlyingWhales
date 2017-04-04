using UnityEngine;
using System.Collections;

public class CitizenAvatar : MonoBehaviour {

	private Citizen citizen;

	internal void AssignCitizen(Citizen citizen){
		this.citizen = citizen;
	}

	internal void MakeCitizenMove(HexTile startTile, HexTile targetTile){
		this.transform.position = Vector3.MoveTowards (startTile.transform.position, targetTile.transform.position, 0.5f);
	}
}
