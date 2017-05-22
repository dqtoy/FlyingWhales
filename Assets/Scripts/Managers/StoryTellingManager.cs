using UnityEngine;
using System.Collections;

public class StoryTellingManager : MonoBehaviour {

	public static StoryTellingManager Instance = null;

	void Awake(){
		Instance = this;
	}

	public TRAIT GenerateHonestyTrait(Citizen citizen){
		switch (citizen.city.kingdom.kingdomType) {
		}
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
	public KingdomTypeData InitializeKingdomType(Kingdom kingdom) {
		// if the kingdom's type is null, this means that this is the first time
		if (kingdom.kingdomTypeData == null) {
			Debug.Log ("IKT: " + kingdom.kingdomTypeData + ". " + kingdom.sourceKingdom);
			// if the kingdom's sourceKingdom is null, set the kingdom type based on chance
			if (kingdom.sourceKingdom == null) {
				int randomizer = Random.Range (0, 100);
				if (randomizer < 30) {
					return KingdomManager.Instance.kingdomTypeBarbaric;
				} else if (randomizer < 45) {
					return KingdomManager.Instance.kingdomTypeHermit;
				} else if (randomizer < 70) {
					return KingdomManager.Instance.kingdomTypeReligious;
				} else {
					return KingdomManager.Instance.kingdomTypeOpportunistic;
				}
			} else {
				// otherwise, set the kingdom based on the kingdom's sourceKingdom type
				return kingdom.sourceKingdom.kingdomTypeData;
			}		
		} else {
			// otherwise, this means the kingdom already has an existing type and will just be changed based on the previous king	
			switch (kingdom.kingdomType) {
			case (KINGDOM_TYPE.BARBARIC_TRIBE):
				if (kingdom.cities.Count > 4) {
					if (kingdom.king.hasTrait (TRAIT.HONEST)) {
						return KingdomManager.Instance.kingdomTypeNoble;
					} else {
						return KingdomManager.Instance.kingdomTypeEvil;
					}
				}
				return KingdomManager.Instance.kingdomTypeBarbaric;
				break;

			case (KINGDOM_TYPE.HERMIT_TRIBE):
				if (kingdom.cities.Count >= 5) {
					if (kingdom.king.hasTrait (TRAIT.HONEST)) {
						return KingdomManager.Instance.kingdomTypeMerchant;
					} else {
						return KingdomManager.Instance.kingdomTypeChaotic;
					}
				}
				return KingdomManager.Instance.kingdomTypeHermit;
				break;

			case (KINGDOM_TYPE.RELIGIOUS_TRIBE):
				if (kingdom.cities.Count > 4) {
					if (kingdom.king.hasTrait (TRAIT.WARMONGER)) {
						return KingdomManager.Instance.kingdomTypeNoble;
					} else {
						return KingdomManager.Instance.kingdomTypeMerchant;
					}
				}
				return KingdomManager.Instance.kingdomTypeReligious;
				break;

			case (KINGDOM_TYPE.OPPORTUNISTIC_TRIBE):
				if (kingdom.cities.Count > 4) {
					if (kingdom.king.hasTrait (TRAIT.WARMONGER)) {
						return KingdomManager.Instance.kingdomTypeEvil;
					} else {
						return KingdomManager.Instance.kingdomTypeChaotic;
					}
				}
				return KingdomManager.Instance.kingdomTypeOpportunistic;
				break;

			case (KINGDOM_TYPE.NOBLE_KINGDOM):
				if (kingdom.cities.Count <= 4) {
					if (Random.Range (0, 1) == 0) {
						return KingdomManager.Instance.kingdomTypeBarbaric;
					} else {
						return KingdomManager.Instance.kingdomTypeReligious;
					}
				} else if (kingdom.cities.Count > 8) {
					return KingdomManager.Instance.kingdomTypeRighteous;
				}
				return KingdomManager.Instance.kingdomTypeNoble;
				break;

			case (KINGDOM_TYPE.EVIL_EMPIRE):
				if (kingdom.cities.Count <= 4) {
					if (Random.Range (0, 1) == 0) {
						return KingdomManager.Instance.kingdomTypeBarbaric;
					} else {
						return KingdomManager.Instance.kingdomTypeOpportunistic;
					}
				} else if (kingdom.cities.Count > 8) {
					return KingdomManager.Instance.kingdomTypeWicked;
				}
				return KingdomManager.Instance.kingdomTypeEvil;
				break;

			case (KINGDOM_TYPE.MERCHANT_NATION):
				if (kingdom.cities.Count <= 4) {
					if (Random.Range (0, 1) == 0) {
						return KingdomManager.Instance.kingdomTypeReligious;
					} else {
						return KingdomManager.Instance.kingdomTypeHermit;
					}
				} else if (kingdom.cities.Count > 8) {
					return KingdomManager.Instance.kingdomTypeRighteous;
				}
				return KingdomManager.Instance.kingdomTypeMerchant;
				break;

			case (KINGDOM_TYPE.CHAOTIC_STATE):
				if (kingdom.cities.Count <= 4) {
					if (Random.Range (0, 1) == 0) {
						return KingdomManager.Instance.kingdomTypeOpportunistic;
					} else {
						return KingdomManager.Instance.kingdomTypeHermit;
					}
				} else if (kingdom.cities.Count > 8) {
					return KingdomManager.Instance.kingdomTypeWicked;
				}
				return KingdomManager.Instance.kingdomTypeChaotic;
				break;

			case (KINGDOM_TYPE.RIGHTEOUS_SUPERPOWER):
				if (kingdom.cities.Count <= 4) {
					if (kingdom.king.hasTrait (TRAIT.HONEST)) {
						return KingdomManager.Instance.kingdomTypeReligious;
					} else {
						return KingdomManager.Instance.kingdomTypeHermit;
					}
				} else if (kingdom.cities.Count > 4 && kingdom.cities.Count <= 8) {
					if (kingdom.king.hasTrait (TRAIT.PACIFIST)) {
						return KingdomManager.Instance.kingdomTypeMerchant;
					} else {
						return KingdomManager.Instance.kingdomTypeNoble;
					}
				}
				return KingdomManager.Instance.kingdomTypeRighteous;
				break;

			case (KINGDOM_TYPE.WICKED_SUPERPOWER):
				if (kingdom.cities.Count <= 4) {
					if (kingdom.king.hasTrait (TRAIT.SCHEMING)) {
						return KingdomManager.Instance.kingdomTypeOpportunistic;
					} else {
						return KingdomManager.Instance.kingdomTypeBarbaric;
					}
				} else if (kingdom.cities.Count > 4 && kingdom.cities.Count <= 8) {
					if (kingdom.king.hasTrait (TRAIT.WARMONGER)) {
						return KingdomManager.Instance.kingdomTypeEvil;
					} else {
						return KingdomManager.Instance.kingdomTypeChaotic;
					}
				}
				return KingdomManager.Instance.kingdomTypeWicked;
				break;
			}


		}
		return null;
	}
}
