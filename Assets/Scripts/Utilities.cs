using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Utilities : MonoBehaviour {

	public static int lastKingdomID = 0;
	public static int lastCitizenID = 0;
	public static int lastCityID = 0;

	public static string[] accidentCauses = new string[]{
		"He died because he forgot to breath.",
		"He died after falling off a cliff.",
		"He died due to an infection from an arrow in the knee.",
		"He died of heartbreak.",
		"He died from an animal attack.",
		"He died after a boulder rolled over him.",
		"He died after a brick fell on his head.",
		"He died after slipping on the floor.",
		"He died from a landslide.",
		"He died from drinking too much alcohol.",
		"He died from eating poisonous mushrooms."
	};

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

//	public static string CauseOfAccident(){
//		
//	}

}
