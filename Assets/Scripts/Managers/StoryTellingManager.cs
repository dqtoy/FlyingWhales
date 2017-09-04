using UnityEngine;
using System.Collections;

public class StoryTellingManager : MonoBehaviour {

	public static StoryTellingManager Instance = null;

	void Awake(){
		Instance = this;
	}

//	public TRAIT GenerateHonestyTrait(Citizen citizen){
//		//switch (citizen.city.kingdom.kingdomType) {
//		//}
//		if (citizen.city.kingdom.horoscope [0] == 0) {
//			return TRAIT.HONEST;
//		} else {
//			return TRAIT.SCHEMING;
//		}
//	}
//
//	public TRAIT GenerateHostilityTrait(Citizen citizen){
//		if (citizen.city.kingdom.horoscope [1] == 0) {
//			return TRAIT.WARMONGER;
//		} else {
//			return TRAIT.PACIFIST;
//		}
//	}

//	public TRAIT GenerateMiscTrait(Citizen citizen){
//        //if (citizen.horoscope [2] == 0) {
//        //	return TRAIT.SMART;
//        //} else {
//        //	return TRAIT.STUPID;
//        //}
//        int chance = Random.Range(0, 100);
//        if (chance < 40) {
//            return TRAIT.SMART;
//        }else if(chance >= 40 && chance < 80) {
//            return TRAIT.STUPID;
//        } else {
//            return TRAIT.AMBITIOUS;
//        }
//	}

	// Set the new kingdom type of a kingdom
	public KingdomTypeData InitializeKingdomType(Kingdom kingdom) {
		// if the kingdom's type is null, this means that this is the first time
		if (kingdom.kingdomTypeData == null) {
//			Debug.Log ("IKT: " + kingdom.kingdomTypeData + ". " + kingdom.sourceKingdom);
			// if the kingdom's sourceKingdom is null, set the kingdom type based on chance
			if (kingdom.sourceKingdom == null) {
				int randomizer = Random.Range (0, 100);
				if (randomizer < 40) {
					return KingdomManager.Instance.kingdomTypeBarbaric;
				} else if (randomizer < 70) {
					return KingdomManager.Instance.kingdomTypeNaive;
				} else {
					return KingdomManager.Instance.kingdomTypeOpportunistic;
				}
			} else {
				// otherwise, set the kingdom based on the kingdom's sourceKingdom type
				return kingdom.sourceKingdom.kingdomTypeData;
			}		
		} else {
			int randomizer = Random.Range (0, 100);
			// otherwise, this means the kingdom already has an existing type and will just be changed based on the previous king	
			switch (kingdom.kingdomType) {
			case (KINGDOM_TYPE.BARBARIC_TRIBE):
				if (kingdom.cities.Count > 4) {
					if (randomizer < 60) {
						return KingdomManager.Instance.kingdomTypeNoble;
					} else {
						return KingdomManager.Instance.kingdomTypeEvil;
					}
				}
				return KingdomManager.Instance.kingdomTypeBarbaric;

			case (KINGDOM_TYPE.NAIVE_TRIBE):
				if (kingdom.cities.Count >= 5) {
					if (randomizer < 60) {
						return KingdomManager.Instance.kingdomTypeMerchant;
					} else {
						return KingdomManager.Instance.kingdomTypeChaotic;
					}
				}
				return KingdomManager.Instance.kingdomTypeNaive;

			case (KINGDOM_TYPE.OPPORTUNISTIC_TRIBE):
				if (kingdom.cities.Count > 4) {
					if (randomizer < 50) {
						return KingdomManager.Instance.kingdomTypeEvil;
					} else {
						return KingdomManager.Instance.kingdomTypeChaotic;
					}
				}
				return KingdomManager.Instance.kingdomTypeOpportunistic;

			case (KINGDOM_TYPE.NOBLE_KINGDOM):
				if (kingdom.cities.Count <= 4) {
					if (randomizer < 50) {
						return KingdomManager.Instance.kingdomTypeBarbaric;
					} else {
						return KingdomManager.Instance.kingdomTypeNaive;
					}
				} else if (kingdom.cities.Count > 8) {
					return KingdomManager.Instance.kingdomTypeRighteous;
				}
				return KingdomManager.Instance.kingdomTypeNoble;

			case (KINGDOM_TYPE.EVIL_EMPIRE):
				if (kingdom.cities.Count <= 4) {
					if (randomizer < 50) {
						return KingdomManager.Instance.kingdomTypeBarbaric;
					} else {
						return KingdomManager.Instance.kingdomTypeOpportunistic;
					}
				} else if (kingdom.cities.Count > 8) {
					return KingdomManager.Instance.kingdomTypeWicked;
				}
				return KingdomManager.Instance.kingdomTypeEvil;

			case (KINGDOM_TYPE.MERCHANT_NATION):
				if (kingdom.cities.Count <= 4) {
					if (randomizer < 50) {
						return KingdomManager.Instance.kingdomTypeOpportunistic;
					} else {
						return KingdomManager.Instance.kingdomTypeNaive;
					}
				} else if (kingdom.cities.Count > 8) {
					return KingdomManager.Instance.kingdomTypeRighteous;
				}
				return KingdomManager.Instance.kingdomTypeMerchant;

			case (KINGDOM_TYPE.CHAOTIC_STATE):
				if (kingdom.cities.Count <= 4) {
					if (randomizer < 50) {
						return KingdomManager.Instance.kingdomTypeOpportunistic;
					} else {
						return KingdomManager.Instance.kingdomTypeBarbaric;
					}
				} else if (kingdom.cities.Count > 8) {
					return KingdomManager.Instance.kingdomTypeWicked;
				}
				return KingdomManager.Instance.kingdomTypeChaotic;

			case (KINGDOM_TYPE.RIGHTEOUS_SUPERPOWER):
				if (kingdom.cities.Count <= 4) {
					if (randomizer < 50) {
						return KingdomManager.Instance.kingdomTypeNaive;
					} else {
						return KingdomManager.Instance.kingdomTypeBarbaric;
					}
				} else if (kingdom.cities.Count > 4 && kingdom.cities.Count <= 8) {
					if (randomizer < 50) {
						return KingdomManager.Instance.kingdomTypeMerchant;
					} else {
						return KingdomManager.Instance.kingdomTypeNoble;
					}
				}
				return KingdomManager.Instance.kingdomTypeRighteous;

			case (KINGDOM_TYPE.WICKED_SUPERPOWER):
				if (kingdom.cities.Count <= 4) {
					if (randomizer < 50) {
						return KingdomManager.Instance.kingdomTypeOpportunistic;
					} else {
						return KingdomManager.Instance.kingdomTypeBarbaric;
					}
				} else if (kingdom.cities.Count > 4 && kingdom.cities.Count <= 8) {
					if (randomizer < 50) {
						return KingdomManager.Instance.kingdomTypeEvil;
					} else {
						return KingdomManager.Instance.kingdomTypeChaotic;
					}
				}
				return KingdomManager.Instance.kingdomTypeWicked;
			}


		}
		return null;
	}
}
