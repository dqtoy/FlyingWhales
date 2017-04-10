using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class Utilities : MonoBehaviour {
	private static System.Random rng = new System.Random(); 
	public static int lastKingdomID = 0;
	public static int lastCitizenID = 0;
	public static int lastCityID = 0;
	public static int lastCampaignID = 0;
	public static int lastEventID = 0;

	public static string[] accidentCauses = new string[]{
		"because he/she forgot to breath.",
		"after falling off a cliff.",
		"due to an infection from an arrow in the knee.",
		"of heartbreak.",
		"from an animal attack.",
		"after a boulder rolled over him.",
		"after a brick fell on his head.",
		"after slipping on the floor.",
		"from a landslide.",
		"from drinking too much alcohol.",
		"from eating poisonous mushrooms."
	};
		
	public static int specialResourceCount = 0;
	
	/*
	 * Set unique id
	 * */
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
		} else if (obj is Campaign) {
			lastCampaignID += 1;
			return lastCampaignID;
		} else if (obj is Event) {
			lastEventID += 1;
			return lastEventID;
		}
		return 0;
	}

	public static T[] GetEnumValues<T>() where T : struct {
		if (!typeof(T).IsEnum) {
			throw new ArgumentException("GetValues<T> can only be called for types derived from System.Enum", "T");
		}
		return (T[])Enum.GetValues(typeof(T));
	}

	public static Dictionary<BIOMES, SpecialResourceChance> specialResourcesLookup = new Dictionary<BIOMES, SpecialResourceChance> () { 
		{BIOMES.BARE, new SpecialResourceChance(
			new RESOURCE[] {RESOURCE.NONE}, 
			new int[] {100})
		
		},

		{BIOMES.GRASSLAND, new SpecialResourceChance(
			new RESOURCE[] {RESOURCE.WHEAT, RESOURCE.RICE, RESOURCE.DEER, RESOURCE.CEDAR, RESOURCE.GRANITE, RESOURCE.SLATE, RESOURCE.MITHRIL, RESOURCE.COBALT}, 
			new int[] {100, 20, 40, 20, 60, 35, 5, 5})

		},

		{BIOMES.WOODLAND, new SpecialResourceChance(
			new RESOURCE[] {RESOURCE.CORN, RESOURCE.WHEAT, RESOURCE.DEER, RESOURCE.PIG, RESOURCE.OAK, RESOURCE.EBONY, RESOURCE.GRANITE, RESOURCE.SLATE, RESOURCE.MANA_STONE, RESOURCE.COBALT}, 
			new int[] {40, 12, 65, 25, 90, 22, 60, 12, 5, 5})

		},


		{BIOMES.FOREST, new SpecialResourceChance(
			new RESOURCE[] {RESOURCE.EBONY, RESOURCE.DEER, RESOURCE.BEHEMOTH, RESOURCE.MANA_STONE, RESOURCE.MITHRIL, RESOURCE.GOLD}, 
			new int[] {15, 40, 15, 12, 8, 8})

		},

		{BIOMES.DESERT, new SpecialResourceChance(
			new RESOURCE[] {RESOURCE.DEER, RESOURCE.PIG, RESOURCE.SLATE, RESOURCE.MARBLE, RESOURCE.MITHRIL, RESOURCE.COBALT, RESOURCE.GOLD}, 
			new int[] {20, 20, 15, 15, 10, 10, 10})

		},

		{BIOMES.TUNDRA, new SpecialResourceChance(
			new RESOURCE[] {RESOURCE.DEER, RESOURCE.PIG, RESOURCE.CEDAR, RESOURCE.GRANITE, RESOURCE.SLATE, RESOURCE.MANA_STONE, RESOURCE.GOLD}, 
			new int[] {50, 15, 10, 25, 10, 5, 5})
				
		},

		{BIOMES.SNOW, new SpecialResourceChance(
			new RESOURCE[] {RESOURCE.CORN, RESOURCE.WHEAT, RESOURCE.DEER, RESOURCE.PIG, RESOURCE.MARBLE, RESOURCE.MITHRIL, RESOURCE.COBALT}, 
			new int[] {15, 5, 15, 5, 5, 3, 3})

		},

	};

	public static Dictionary<ROLE, int> defaultCitizenCreationTable = new Dictionary<ROLE, int>(){
		{ROLE.TRADER, 2},
		{ROLE.GENERAL, 2},
		{ROLE.SPY, 1},
		{ROLE.ENVOY, 1},
		{ROLE.GUARDIAN, 1}
	};

	public static Dictionary<BEHAVIOR_TRAIT, Dictionary<ROLE, int>> citizenCreationTable = new Dictionary<BEHAVIOR_TRAIT, Dictionary<ROLE, int>>(){
		{BEHAVIOR_TRAIT.SCHEMING, new Dictionary<ROLE, int>(){
				{ROLE.TRADER, 0},
				{ROLE.GENERAL, 0},
				{ROLE.SPY, 1},
				{ROLE.ENVOY, 0},
				{ROLE.GUARDIAN, -10}
			}
		},
		{BEHAVIOR_TRAIT.NAIVE, new Dictionary<ROLE, int>(){
				{ROLE.TRADER, 0},
				{ROLE.GENERAL, 0},
				{ROLE.SPY, -10},
				{ROLE.ENVOY, 1},
				{ROLE.GUARDIAN, 0}
			}
		},
		{BEHAVIOR_TRAIT.WARMONGER, new Dictionary<ROLE, int>(){
				{ROLE.TRADER, -1},
				{ROLE.GENERAL, 1},
				{ROLE.SPY, 1},
				{ROLE.ENVOY, -10},
				{ROLE.GUARDIAN, 0}
			}
		},
		{BEHAVIOR_TRAIT.PACIFIST, new Dictionary<ROLE, int>(){
				{ROLE.TRADER, 1},
				{ROLE.GENERAL, -1},
				{ROLE.SPY, -10},
				{ROLE.ENVOY, 0},
				{ROLE.GUARDIAN, 1}
			}
		},
		{BEHAVIOR_TRAIT.CHARISMATIC, new Dictionary<ROLE, int>(){
				{ROLE.TRADER, 0},
				{ROLE.GENERAL, 0},
				{ROLE.SPY, 0},
				{ROLE.ENVOY, 0},
				{ROLE.GUARDIAN, 0}
			}
		},
		{BEHAVIOR_TRAIT.REPULSIVE, new Dictionary<ROLE, int>(){
				{ROLE.TRADER, 0},
				{ROLE.GENERAL, 0},
				{ROLE.SPY, 0},
				{ROLE.ENVOY, 0},
				{ROLE.GUARDIAN, 0}
			}
		},
		{BEHAVIOR_TRAIT.AGGRESSIVE, new Dictionary<ROLE, int>(){
				{ROLE.TRADER, 0},
				{ROLE.GENERAL, 0},
				{ROLE.SPY, 0},
				{ROLE.ENVOY, 0},
				{ROLE.GUARDIAN, 0}
			}
		},
		{BEHAVIOR_TRAIT.DEFENSIVE, new Dictionary<ROLE, int>(){
				{ROLE.TRADER, 0},
				{ROLE.GENERAL, 0},
				{ROLE.SPY, 0},
				{ROLE.ENVOY, 0},
				{ROLE.GUARDIAN, 0}
			}
		},
	};

//	public static string CauseOfAccident(){
//		
//	}

	public static BASE_RESOURCE_TYPE GetBaseResourceType(RESOURCE resourceType){
		if (resourceType == RESOURCE.CORN || resourceType == RESOURCE.WHEAT || resourceType == RESOURCE.RICE ||
		    resourceType == RESOURCE.DEER || resourceType == RESOURCE.PIG || resourceType == RESOURCE.BEHEMOTH) {
			return BASE_RESOURCE_TYPE.FOOD;
		} else if (resourceType == RESOURCE.CEDAR || resourceType == RESOURCE.OAK || resourceType == RESOURCE.EBONY) {
			return BASE_RESOURCE_TYPE.WOOD;
		} else if (resourceType == RESOURCE.GRANITE || resourceType == RESOURCE.SLATE || resourceType == RESOURCE.MARBLE) {
			return BASE_RESOURCE_TYPE.STONE;
		} else if (resourceType == RESOURCE.MANA_STONE) {
			return BASE_RESOURCE_TYPE.MANA_STONE;
		} else if (resourceType == RESOURCE.MITHRIL) {
			return BASE_RESOURCE_TYPE.MITHRIL;
		} else if (resourceType == RESOURCE.COBALT) {
			return BASE_RESOURCE_TYPE.COBALT;
		}
		return BASE_RESOURCE_TYPE.NONE;
	}

	public static STRUCTURE GetStructureThatProducesResource(RESOURCE resourceType){
		if (resourceType == RESOURCE.CORN || resourceType == RESOURCE.WHEAT || resourceType == RESOURCE.RICE) {
			return STRUCTURE.FARM;
		} else if (resourceType == RESOURCE.DEER || resourceType == RESOURCE.PIG || resourceType == RESOURCE.BEHEMOTH) {
			return STRUCTURE.HUNTING_LODGE;
		}else if (resourceType == RESOURCE.CEDAR || resourceType == RESOURCE.OAK || resourceType == RESOURCE.EBONY) {
			return STRUCTURE.LUMBERYARD;
		} else if (resourceType == RESOURCE.GRANITE || resourceType == RESOURCE.SLATE || resourceType == RESOURCE.MARBLE) {
			return STRUCTURE.QUARRY;
		} else if (resourceType == RESOURCE.MANA_STONE) {
			return STRUCTURE.MINES;
		} else if (resourceType == RESOURCE.MITHRIL) {
			return STRUCTURE.MINES;
		} else if (resourceType == RESOURCE.COBALT) {
			return STRUCTURE.MINES;
		}
		return STRUCTURE.NONE;
	}


	public static ROLE GetRoleThatProducesResource(BASE_RESOURCE_TYPE resourceType){
		if (resourceType == BASE_RESOURCE_TYPE.FOOD) {
			return ROLE.FOODIE;
		} else if (resourceType == BASE_RESOURCE_TYPE.STONE || resourceType == BASE_RESOURCE_TYPE.WOOD) {
			return ROLE.GATHERER;
		} else if (resourceType == BASE_RESOURCE_TYPE.MANA_STONE || resourceType == BASE_RESOURCE_TYPE.MITHRIL || resourceType == BASE_RESOURCE_TYPE.COBALT) {
			return ROLE.MINER;
		}
		return ROLE.UNTRAINED;
	}

	public static Color GetColorForRelationship(RELATIONSHIP_STATUS status){
		if (status == RELATIONSHIP_STATUS.ALLY) {
			return new Color (0, 139, 69);
		} else if (status == RELATIONSHIP_STATUS.FRIEND) {
			return new Color (0, 255, 127);
		} else if (status == RELATIONSHIP_STATUS.WARM) {
			return new Color (118, 238, 198);
		} else if (status == RELATIONSHIP_STATUS.NEUTRAL) {
			return Color.white;
		} else if (status == RELATIONSHIP_STATUS.COLD) {
			return new Color (240, 128, 128);
		} else if (status == RELATIONSHIP_STATUS.ENEMY) {
			return new Color (255, 64, 64);
		} else if (status == RELATIONSHIP_STATUS.RIVAL) {
			return new Color (255, 0, 0);
		}
		return Color.white;
	}

	#region Pathfinding
	public static List<Point> EvenNeighbours {
		get {
			return new List<Point> {
				new Point(-1, 1),
				new Point(0, 1),
				new Point(1, 0),
				new Point(0, -1),
				new Point(-1, -1),
				new Point(-1, 0),

			};
		}
	}

	public static List<Point> OddNeighbours {
		get {
			return new List<Point> {
				new Point(0, 1),
				new Point(1, 1),
				new Point(1, 0),
				new Point(1, -1),
				new Point(0, -1),
				new Point(-1, 0),
			};
		}
	}
	#endregion

	public static void ChangeDescendantsRecursively(Citizen royalty, bool isDescendant){
		royalty.isDirectDescendant = isDescendant;


		for(int i = 0; i < royalty.children.Count; i++){
			if(royalty.children[i] != null){
				ChangeDescendantsRecursively (royalty.children [i], isDescendant);
			}
		}
	}

	public static List<T> Shuffle<T>(List<T> list)  
	{
		List<T> newList = new List<T>(list);
		int n = newList.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			T value = newList[k];  
			newList[k] = newList[n];  
			newList[n] = value;  
		} 
		return newList;
	}

	public static bool AreTwoGeneralsFriendly(General general1, General general2){
		if(general1.citizen.city.kingdom.id != general2.citizen.city.kingdom.id){
			if(general2.warLeader != null){
				if(general1.citizen.city.kingdom.king.supportedCitizen != null){
					if(general1.citizen.city.kingdom.king.supportedCitizen.id != general2.warLeader.id){
						if(general2.citizen.city.governor.supportedCitizen != null){
							if(general1.citizen.city.kingdom.king.supportedCitizen.id != general2.citizen.city.governor.supportedCitizen.id){
								//CHECK VICE VERSA
								if (general1.citizen.city.kingdom.CheckForSpecificWar (general2.citizen.city.kingdom)) {
									return false;
								}
							}
						}else{
							if (general1.citizen.city.kingdom.king.supportedCitizen.city.kingdom.id != general2.citizen.city.kingdom.id) {
								if (general1.citizen.city.kingdom.CheckForSpecificWar (general2.citizen.city.kingdom)) {
									return false;
								}
							}else{
								if(!general1.citizen.city.kingdom.king.supportedCitizen.isHeir){
									if (general1.citizen.city.kingdom.CheckForSpecificWar (general2.citizen.city.kingdom)) {
										return false;
									}
								}
							}
						}
					}
				}else{
					//CHECK VICE VERSA
					if (general1.citizen.city.kingdom.CheckForSpecificWar (general2.citizen.city.kingdom)) {
						return false;
					}
				}

			}else{
				if (general1.citizen.city.kingdom.king.supportedCitizen != null) {
					if(general2.citizen.city.governor.supportedCitizen != null){
						if(general1.citizen.city.kingdom.king.supportedCitizen.id != general2.citizen.city.governor.supportedCitizen.id){
							//CHECK VICE VERSA
							if (general1.citizen.city.kingdom.CheckForSpecificWar (general2.citizen.city.kingdom)) {
								return false;
							}
						}
					}else{
						if (general1.citizen.city.kingdom.king.supportedCitizen.city.kingdom.id != general2.citizen.city.kingdom.id) {
							if (general1.citizen.city.kingdom.CheckForSpecificWar (general2.citizen.city.kingdom)) {
								return false;
							}
						}else{
							if(!general1.citizen.city.kingdom.king.supportedCitizen.isHeir){
								if (general1.citizen.city.kingdom.CheckForSpecificWar (general2.citizen.city.kingdom)) {
									return false;
								}
							}
						}
					}
				}else{
					if(general1.citizen.city.kingdom.CheckForSpecificWar(general2.citizen.city.kingdom)){
						return false;
					}
				}

			}
		}else{
			if (general1.citizen.city.governor.id != general2.citizen.city.governor.id) {
				if(general1.citizen.city.governor.supportedCitizen != null && general2.citizen.city.governor.supportedCitizen != null){
					if(general1.citizen.city.governor.supportedCitizen.id != general2.citizen.city.governor.supportedCitizen.id){
						return false;
					}
				}else{
					if(general1.citizen.city.governor.supportedCitizen == null && general2.citizen.city.governor.supportedCitizen == null){
						//BLANK ONLY
					}else{
						return false;
					}
				}
			}
		}

		return true;
	}
}
