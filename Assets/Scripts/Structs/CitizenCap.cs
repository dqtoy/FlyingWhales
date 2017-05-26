using UnityEngine;
using System.Collections;

[System.Serializable]
public struct CitizenCap{
	public ROLE role;
	public int cap;
	public int rate; // number of days it needs to fill its growth meter, once full, the city can produce a new citizen of this type once the gold is available

	public CitizenCap(ROLE role, int cap, int rate){
		this.role = role;
		this.cap = cap;
		this.rate = rate;
	}
}