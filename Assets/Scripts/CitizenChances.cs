using UnityEngine;
using System.Collections;

[System.Serializable]
public class CitizenChances {
	internal float defaultAccidentChance = 0.03f;
	internal float defaultOldAgeChance = 0.1f;
	internal int defaultMarriageChance = 8;

	public float accidentChance;
	public float oldAgeChance;
	public int marriageChance;

	public CitizenChances(){
		this.accidentChance = this.defaultAccidentChance;
		this.oldAgeChance = this.defaultOldAgeChance;
		this.marriageChance = this.defaultMarriageChance;
	}
}
