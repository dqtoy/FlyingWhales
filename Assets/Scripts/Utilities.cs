using UnityEngine;
using System.Collections;

public class Utilities : MonoBehaviour {

	public static int lastKingdomID = 0;
	public static int lastCitizenID = 0;
	public static int lastCityID = 0;

	public static int SetID<T>(T obj){
		if (obj is Kingdom) {
			lastKingdomID += 1;
			return lastKingdomID;
		} else if (obj is City) {
			lastCityID += 1;
			return lastCityID;
		} else if (obj is Citizen) {
			lastCitizenID += 1;
			return lastCitizenID;
		}
		return 0;
	}

}
