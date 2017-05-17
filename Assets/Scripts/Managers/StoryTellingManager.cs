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

	// Set the new kingdom type of a kingdom
	public KINGDOM_TYPE InitializeKingdomType(Kingdom kingdom) {
		// if the kingdom's type is null, this means that this is the first time
		if (kingdom.kingdomType == KINGDOM_TYPE.NONE) {
			// if the kingdom's sourceKingdom is null, set the kingdom type based on chance
			if (kingdom.sourceKingdom == null) {
				int randomizer = Random.Range (0, 100);
				if (randomizer < 30) {
					return KINGDOM_TYPE.BARBARIC_TRIBE;
				} else if (randomizer < 45) {
					return KINGDOM_TYPE.HERMIT_TRIBE;
				} else if (randomizer < 70) {
					return KINGDOM_TYPE.RELIGIOUS_TRIBE;
				} else {
					return KINGDOM_TYPE.OPPORTUNISTIC_TRIBE;
				}
			} else {
				// otherwise, set the kingdom based on the kingdom's sourceKingdom type
				return kingdom.sourceKingdom.kingdomType;
			}		
		} else {
			// otherwise, this means the kingdom already has an existing type and will just be changed based on the previous king	
			switch (kingdom.kingdomType) {
			case (KINGDOM_TYPE.BARBARIC_TRIBE):
				if (kingdom.cities.Count >= 5) {
					if (kingdom.king.hasTrait (TRAIT.HONEST)) {
						return KINGDOM_TYPE.NOBLE_KINGDOM;
					} else {
						return KINGDOM_TYPE.EVIL_EMPIRE;
					}
				}
				return KINGDOM_TYPE.BARBARIC_TRIBE;
				break;

			case (KINGDOM_TYPE.HERMIT_TRIBE):
				if (kingdom.cities.Count >= 5) {
					if (kingdom.king.hasTrait (TRAIT.HONEST)) {
						return KINGDOM_TYPE.MERCHANT_NATION;
					} else {
						return KINGDOM_TYPE.CHAOTIC_STATE;
					}
				}
				return KINGDOM_TYPE.HERMIT_TRIBE;
				break;

			case (KINGDOM_TYPE.RELIGIOUS_TRIBE):
				if (kingdom.cities.Count >= 5) {
					if (kingdom.king.hasTrait (TRAIT.WARMONGER)) {
						return KINGDOM_TYPE.NOBLE_KINGDOM;
					} else {
						return KINGDOM_TYPE.MERCHANT_NATION;
					}
				}
				return KINGDOM_TYPE.RELIGIOUS_TRIBE;
				break;

			case (KINGDOM_TYPE.OPPORTUNISTIC_TRIBE):
				if (kingdom.cities.Count >= 5) {
					if (kingdom.king.hasTrait (TRAIT.WARMONGER)) {
						return KINGDOM_TYPE.EVIL_EMPIRE;
					} else {
						return KINGDOM_TYPE.CHAOTIC_STATE;
					}
				}
				return KINGDOM_TYPE.OPPORTUNISTIC_TRIBE;				
				break;

			case (KINGDOM_TYPE.NOBLE_KINGDOM):
				if (kingdom.cities.Count <= 4) {
					if (Random.Range (0, 1) == 0) {
						return KINGDOM_TYPE.BARBARIC_TRIBE;
					} else {
						return KINGDOM_TYPE.RELIGIOUS_TRIBE;
					}
				}
				return KINGDOM_TYPE.NOBLE_KINGDOM;				
				break;

			case (KINGDOM_TYPE.EVIL_EMPIRE):
				if (kingdom.cities.Count <= 4) {
					if (Random.Range (0, 1) == 0) {
						return KINGDOM_TYPE.BARBARIC_TRIBE;
					} else {
						return KINGDOM_TYPE.OPPORTUNISTIC_TRIBE;
					}
				}
				return KINGDOM_TYPE.EVIL_EMPIRE;					
				break;

			case (KINGDOM_TYPE.MERCHANT_NATION):
				if (kingdom.cities.Count <= 4) {
					if (Random.Range (0, 1) == 0) {
						return KINGDOM_TYPE.RELIGIOUS_TRIBE;
					} else {
						return KINGDOM_TYPE.HERMIT_TRIBE;
					}
				}
				return KINGDOM_TYPE.MERCHANT_NATION;					
				break;

			case (KINGDOM_TYPE.CHAOTIC_STATE):
				if (kingdom.cities.Count <= 4) {
					if (Random.Range (0, 1) == 0) {
						return KINGDOM_TYPE.OPPORTUNISTIC_TRIBE;
					} else {
						return KINGDOM_TYPE.HERMIT_TRIBE;
					}
				}
				return KINGDOM_TYPE.CHAOTIC_STATE;	
				break;
			}
		}
		return KINGDOM_TYPE.NONE;
	}
}
