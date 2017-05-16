using UnityEngine;
using System.Collections;

public class StoryTellingManager : MonoBehaviour {

	public static StoryTellingManager Instance = null;

	void Awake(){
		Instance = this;
	}

	public TRAIT GenerateHonestyTrait(Citizen citizen){
		if (citizen.city.kingdom.horoscope [0] == 0) {
			return TRAIT.HONEST;
		} else {
			return TRAIT.SCHEMING;
		}
	}

	public TRAIT GenerateHostilityTrait(Citizen citizen){
		if (citizen.city.kingdom.horoscope [1] == 0) {
			return TRAIT.WARMONGER;
		} else {
			return TRAIT.PACIFIST;
		}
	}

	public TRAIT GenerateIntelligenceTrait(Citizen citizen){
		if (citizen.horoscope [2] == 0) {
			return TRAIT.SMART;
		} else {
			return TRAIT.STUPID;
		}
	}
}
