using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Utilities : MonoBehaviour {

	public static int lastKingdomID = 0;
	public static int lastCitizenID = 0;
	public static int lastCityID = 0;

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
		}
		return 0;
	}


	public static Dictionary<BIOMES, SpecialResourceChance[]> specialResourcesLookup = new Dictionary<BIOMES, SpecialResourceChance[]> () { 
		{BIOMES.BARE, new SpecialResourceChance[]{
				new SpecialResourceChance(RESOURCE.NONE, 100),
			}
		
		},

		{BIOMES.GRASSLAND, new SpecialResourceChance[]{
				new SpecialResourceChance(RESOURCE.WHEAT, 100),
				new SpecialResourceChance(RESOURCE.RICE, 20),
				new SpecialResourceChance(RESOURCE.DEER, 40),
				new SpecialResourceChance(RESOURCE.CEDAR, 20),
				new SpecialResourceChance(RESOURCE.GRANITE, 60),
				new SpecialResourceChance(RESOURCE.SLATE, 35),
				new SpecialResourceChance(RESOURCE.MITHRIL, 5),
				new SpecialResourceChance(RESOURCE.COBALT, 5),
			}

		},

		{BIOMES.WOODLAND, new SpecialResourceChance[]{
				new SpecialResourceChance(RESOURCE.CORN, 40),
				new SpecialResourceChance(RESOURCE.WHEAT, 12),
				new SpecialResourceChance(RESOURCE.DEER, 65),
				new SpecialResourceChance(RESOURCE.PIG, 25),
				new SpecialResourceChance(RESOURCE.OAK, 90),
				new SpecialResourceChance(RESOURCE.EBONY, 22),
				new SpecialResourceChance(RESOURCE.GRANITE, 60),
				new SpecialResourceChance(RESOURCE.SLATE, 12),
				new SpecialResourceChance(RESOURCE.MANA_STONE, 5),
				new SpecialResourceChance(RESOURCE.COBALT, 5),
			}

		},


		{BIOMES.FOREST, new SpecialResourceChance[]{
				new SpecialResourceChance(RESOURCE.EBONY, 15),
				new SpecialResourceChance(RESOURCE.DEER, 40),
				new SpecialResourceChance(RESOURCE.BEHEMOTH, 15),
				new SpecialResourceChance(RESOURCE.MANA_STONE, 12),
				new SpecialResourceChance(RESOURCE.MITHRIL, 8),
				new SpecialResourceChance(RESOURCE.GOLD, 8),
			}

		},

		{BIOMES.DESERT, new SpecialResourceChance[]{
				new SpecialResourceChance(RESOURCE.DEER, 20),
				new SpecialResourceChance(RESOURCE.PIG, 20),
				new SpecialResourceChance(RESOURCE.SLATE, 15),
				new SpecialResourceChance(RESOURCE.MARBLE, 15),
				new SpecialResourceChance(RESOURCE.MITHRIL, 10),
				new SpecialResourceChance(RESOURCE.COBALT, 10),
				new SpecialResourceChance(RESOURCE.GOLD, 10),
			}

		},

		{BIOMES.TUNDRA, new SpecialResourceChance[]{
				new SpecialResourceChance(RESOURCE.DEER, 50),
				new SpecialResourceChance(RESOURCE.PIG, 15),
				new SpecialResourceChance(RESOURCE.CEDAR, 10),
				new SpecialResourceChance(RESOURCE.GRANITE, 25),
				new SpecialResourceChance(RESOURCE.SLATE, 10),
				new SpecialResourceChance(RESOURCE.MANA_STONE, 5),
				new SpecialResourceChance(RESOURCE.GOLD, 5),
			}

		},

		{BIOMES.SNOW, new SpecialResourceChance[]{
				new SpecialResourceChance(RESOURCE.CORN, 15),
				new SpecialResourceChance(RESOURCE.WHEAT, 5),
				new SpecialResourceChance(RESOURCE.DEER, 15),
				new SpecialResourceChance(RESOURCE.PIG, 5),
				new SpecialResourceChance(RESOURCE.MARBLE, 5),
				new SpecialResourceChance(RESOURCE.MITHRIL, 3),
				new SpecialResourceChance(RESOURCE.COBALT, 3),
			}

		},

	};


	public static BASE_RESOURCE_TYPE GetBaseResourceType(RESOURCE resourceType){
		if (resourceType == RESOURCE.CORN || resourceType == RESOURCE.WHEAT || resourceType == RESOURCE.RICE ||
			resourceType == RESOURCE.DEER || resourceType == RESOURCE.PIG || resourceType == RESOURCE.BEHEMOTH) {
			return BASE_RESOURCE_TYPE.FOOD;
		} else if (resourceType == RESOURCE.CEDAR || resourceType == RESOURCE.OAK || resourceType == RESOURCE.EBONY) {
			return BASE_RESOURCE_TYPE.WOOD;
		} else if (resourceType == RESOURCE.GRANITE || resourceType == RESOURCE.SLATE || resourceType == RESOURCE.MARBLE) {
			return BASE_RESOURCE_TYPE.STONE;
		}
		return BASE_RESOURCE_TYPE.SPECIAL;
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
}
