using UnityEngine;
using System.Collections;

[System.Serializable]
public class CitizenChances {
	internal float defaultAccidentChance = 0.03f;
	internal float defaultOldAgeChance = 0.1f;

	public float accidentChance;
	public float oldAgeChance;

	public CitizenChances(){
		this.accidentChance = this.defaultAccidentChance;
		this.oldAgeChance = this.oldAgeChance;
	}
}
