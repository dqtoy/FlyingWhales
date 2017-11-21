using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

public class Utilities : MonoBehaviour {
	private static System.Random rng = new System.Random(); 
	public static int lastKingdomID = 0;
	public static int lastCitizenID = 0;
	public static int lastCityID = 0;
    public static int lastRegionID = 0;
	public static int lastCampaignID = 0;
	public static int lastEventID = 0;
	public static int lastKingdomColorIndex = 0;
	public static int lastAlliancePoolID = 0;
	public static int lastWarfareID = 0;
    public static int lastLogID = 0;
    public static int defaultCampaignExpiration = 8;
	public static float defenseBuff = 1.20f;
	public static int defaultCityHP = 300;

	public static LANGUAGES defaultLanguage = LANGUAGES.ENGLISH;

	public static string[] accidentCauses = new string[]{
		"because he/she forgot to breath",
		"after falling off a cliff",
		"due to an infection from an arrow in the knee",
		"of heartbreak",
		"from an animal attack",
		"after a boulder rolled over him",
		"after a brick fell on his head",
		"after slipping on the floor",
		"from a landslide",
		"from drinking too much alcohol",
		"from eating poisonous mushrooms"
	};
	public static string[] crisis = new string[]{
		"Food",
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
		} else if (obj is GameEvent) {
			lastEventID += 1;
			return lastEventID;
		}else if(obj is Region) {
            lastRegionID += 1;
            return lastRegionID;
        } else if (obj is Log) {
            lastLogID += 1;
            return lastLogID;
        }
        return 0;
	}

	public static Color GetColorForKingdom(){
		Color chosenColor = kingdomColorCycle[lastKingdomColorIndex];
		lastKingdomColorIndex += 1;
		if (lastKingdomColorIndex >= kingdomColorCycle.Length) {
			lastKingdomColorIndex = 0;
		}
		return chosenColor;
	}

	public static T[] GetEnumValues<T>() where T : struct {
		if (!typeof(T).IsEnum) {
			throw new ArgumentException("GetValues<T> can only be called for types derived from System.Enum", "T");
		}
		return (T[])Enum.GetValues(typeof(T));
	}



    public static List<BIOMES> biomeLayering = new List<BIOMES>() {
        BIOMES.GRASSLAND,
        BIOMES.WOODLAND,
        BIOMES.TUNDRA,
        BIOMES.FOREST,
        BIOMES.DESERT,
        BIOMES.SNOW
    };

	public static Dictionary<BIOMES, SpecialResourceChance> specialResourcesLookup = new Dictionary<BIOMES, SpecialResourceChance> () { 
		{BIOMES.BARE, new SpecialResourceChance(
			new RESOURCE[] {RESOURCE.NONE}, 
			new int[] {100})
		},
        
		{BIOMES.GRASSLAND, new SpecialResourceChance(
            //new RESOURCE[] {RESOURCE.WHEAT, RESOURCE.RICE, RESOURCE.DEER, RESOURCE.CEDAR, RESOURCE.GRANITE, RESOURCE.SLATE, RESOURCE.MITHRIL, RESOURCE.COBALT},
            //new int[] {100, 20, 40, 20, 60, 35, 5, 5})
			new RESOURCE[] {RESOURCE.WHEAT, RESOURCE.CORN, RESOURCE.GRANITE, RESOURCE.SLATE, RESOURCE.MITHRIL},
            new int[] {10, 60, 20, 10, 5})
        },

		{BIOMES.WOODLAND, new SpecialResourceChance(
			//new RESOURCE[] {RESOURCE.CORN, RESOURCE.WHEAT, RESOURCE.DEER, RESOURCE.PIG, RESOURCE.OAK, RESOURCE.EBONY, RESOURCE.GRANITE, RESOURCE.SLATE, RESOURCE.MANA_STONE, RESOURCE.COBALT},
   //         new int[] {40, 12, 65, 25, 90, 22, 60, 12, 5, 5})
			new RESOURCE[] {RESOURCE.PIG, RESOURCE.RICE, RESOURCE.OAK, RESOURCE.GRANITE, RESOURCE.COBALT},
            new int[] {10, 40, 30, 30, 5})
        },


		{BIOMES.FOREST, new SpecialResourceChance(
			//new RESOURCE[] {RESOURCE.EBONY, RESOURCE.DEER, RESOURCE.BEHEMOTH, RESOURCE.MANA_STONE, RESOURCE.MITHRIL, RESOURCE.GOLD}, 
			//new int[] {15, 40, 15, 0, 0, 0})
			new RESOURCE[] {RESOURCE.BEHEMOTH, RESOURCE.DEER, RESOURCE.OAK, RESOURCE.EBONY, RESOURCE.MANA_STONE},
            new int[] {10, 60, 20, 10, 5})
        },

		{BIOMES.DESERT, new SpecialResourceChance(
			//new RESOURCE[] {RESOURCE.DEER, RESOURCE.PIG, RESOURCE.SLATE, RESOURCE.MARBLE, RESOURCE.MITHRIL, RESOURCE.COBALT, RESOURCE.GOLD}, 
			//new int[] {20, 20, 15, 15, 0, 0, 0})
			new RESOURCE[] {RESOURCE.PIG, RESOURCE.GRANITE, RESOURCE.MITHRIL, RESOURCE.BEHEMOTH},
            new int[] {40, 60, 5, 5})
        },

		{BIOMES.TUNDRA, new SpecialResourceChance(
			//new RESOURCE[] {RESOURCE.DEER, RESOURCE.PIG, RESOURCE.CEDAR, RESOURCE.GRANITE, RESOURCE.SLATE, RESOURCE.MANA_STONE, RESOURCE.GOLD}, 
			//new int[] {50, 15, 10, 25, 10, 0, 0})
			new RESOURCE[] {RESOURCE.RICE, RESOURCE.OAK, RESOURCE.MANA_STONE, RESOURCE.BEHEMOTH},
            new int[] {40, 60, 5, 5})
        },

		{BIOMES.SNOW, new SpecialResourceChance(
			//new RESOURCE[] {RESOURCE.CORN, RESOURCE.WHEAT, RESOURCE.DEER, RESOURCE.PIG, RESOURCE.MARBLE, RESOURCE.MITHRIL, RESOURCE.COBALT}, 
			//new int[] {15, 5, 15, 5, 5, 0, 0})
            new RESOURCE[] {RESOURCE.GRANITE, RESOURCE.OAK, RESOURCE.COBALT},
            new int[] {50, 50, 5})
        },

	};

//	public static string CauseOfAccident(){
//		
//	}

    public static BASE_RESOURCE_TYPE GetBasicResourceForRace(RACE race) {
        if (race == RACE.HUMANS) {
            return BASE_RESOURCE_TYPE.STONE;
        } else if (race == RACE.ELVES) {
            return BASE_RESOURCE_TYPE.WOOD;
        } else if (race == RACE.MINGONS) {
            return BASE_RESOURCE_TYPE.WOOD;
        } else {
            return BASE_RESOURCE_TYPE.STONE;
        }
    }
	public static BASE_RESOURCE_TYPE GetBaseResourceType(RESOURCE resourceType){
		if (resourceType == RESOURCE.CORN || resourceType == RESOURCE.WHEAT || resourceType == RESOURCE.RICE ||
		    resourceType == RESOURCE.DEER || resourceType == RESOURCE.PIG || resourceType == RESOURCE.BEHEMOTH) {
			return BASE_RESOURCE_TYPE.FOOD;
		} else if (resourceType == RESOURCE.OAK || resourceType == RESOURCE.EBONY) {
			return BASE_RESOURCE_TYPE.WOOD;
		} else if (resourceType == RESOURCE.GRANITE || resourceType == RESOURCE.SLATE) {
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

	//public static STRUCTURE GetStructureThatProducesResource(RESOURCE resourceType){
	//	if (resourceType == RESOURCE.CORN || resourceType == RESOURCE.WHEAT || resourceType == RESOURCE.RICE) {
	//		return STRUCTURE.FARM;
	//	} else if (resourceType == RESOURCE.DEER || resourceType == RESOURCE.PIG || resourceType == RESOURCE.BEHEMOTH) {
	//		return STRUCTURE.HUNTING_LODGE;
	//	}else if (resourceType == RESOURCE.CEDAR || resourceType == RESOURCE.OAK || resourceType == RESOURCE.EBONY) {
	//		return STRUCTURE.LUMBERYARD;
	//	} else if (resourceType == RESOURCE.GRANITE || resourceType == RESOURCE.SLATE || resourceType == RESOURCE.MARBLE) {
	//		return STRUCTURE.QUARRY;
	//	} else if (resourceType == RESOURCE.MANA_STONE) {
	//		return STRUCTURE.MINES;
	//	} else if (resourceType == RESOURCE.MITHRIL) {
	//		return STRUCTURE.MINES;
	//	} else if (resourceType == RESOURCE.COBALT) {
	//		return STRUCTURE.MINES;
	//	}
	//	return STRUCTURE.NONE;
	//}

    public static STRUCTURE_TYPE GetStructureTypeForResource(RACE race, RESOURCE resourceType) {
        if (resourceType != RESOURCE.NONE) {
            if (Utilities.GetBaseResourceType(resourceType) == BASE_RESOURCE_TYPE.FOOD) {
                if (resourceType == RESOURCE.BEHEMOTH || resourceType == RESOURCE.DEER ||
                    resourceType == RESOURCE.PIG) {
                    return STRUCTURE_TYPE.HUNTING_LODGE;
                } else {
                    return STRUCTURE_TYPE.MINES; //TODO: Change to Farm when farm is available
                }
            } else if (Utilities.GetBaseResourceType(resourceType) == BASE_RESOURCE_TYPE.WOOD) {
                if(race == RACE.HUMANS) {
                    return STRUCTURE_TYPE.LUMBERYARD;
                } else {
                    return STRUCTURE_TYPE.LUMBERYARD;
                }
            } else if (Utilities.GetBaseResourceType(resourceType) == BASE_RESOURCE_TYPE.STONE) {
                if (race == RACE.ELVES) {
                    return STRUCTURE_TYPE.QUARRY;
                } else {
                    return STRUCTURE_TYPE.QUARRY;
                }
            } else {
                return STRUCTURE_TYPE.MINES;
            }
        }
        return STRUCTURE_TYPE.GENERIC;
    }
            
	public static Color GetColorForRelationship(RELATIONSHIP_STATUS status){
		if (status == RELATIONSHIP_STATUS.LOVE) {
			return new Color (0f, (139f/255f), (69f/255f), 1f);
		} else if (status == RELATIONSHIP_STATUS.AFFECTIONATE) {
			return new Color (0f, 1f, (127f/255f), 1f);
		} else if (status == RELATIONSHIP_STATUS.LIKE) {
			return new Color ((118f/255f), (238f/255f), (198f/255f), 1f);
		} else if (status == RELATIONSHIP_STATUS.NEUTRAL) {
			return Color.white;
		} else if (status == RELATIONSHIP_STATUS.DISLIKE) {
			return new Color ((240f/255f), (128f/255f), (128f/255f), 1f);
		} else if (status == RELATIONSHIP_STATUS.HATE) {
			return new Color (1f, (64f/255f), (64f/255f), 1f);
		} else if (status == RELATIONSHIP_STATUS.SPITE) {
			return new Color (1f, 0f, 0f, 1f);
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
	//public static bool AreTwoGeneralsFriendly(General general1, General general2){
	//	if (general1.citizen.city.kingdom.id != general2.citizen.city.kingdom.id) {
	//		if (general1.citizen.city.kingdom.CheckForSpecificWar (general2.citizen.city.kingdom)) {
	//			return false;
	//		}else{
	//			return true;
	//		}
	//	}else{
	//		return true;
	//	}
	//}
	/*public static bool AreTwoGeneralsFriendly(General general1, General general2){
		if(general1.citizen.city.kingdom.id != general2.citizen.city.kingdom.id){
			if(general2.assignedCampaign != null){
				if(general2.assignedCampaign.leader != null){
					if(general1.citizen.city.kingdom.king.supportedCitizen != null){
						if(general1.citizen.city.kingdom.king.supportedCitizen.id != general2.assignedCampaign.leader.id){
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
	}*/

	public static T[] GetComponentsInDirectChildren<T>(GameObject gameObject){
		int indexer = 0;

		foreach (Transform transform in gameObject.transform){
			if (transform.GetComponent<T>() != null){
				indexer++;
			}
		}

		T[] returnArray = new T[indexer];

		indexer = 0;

		foreach (Transform transform in gameObject.transform){
			if (transform.GetComponent<T>() != null){
				returnArray[indexer++] = transform.GetComponent<T>();
			}
		}

		return returnArray;
	}

	public static bool IsItThisGovernor(Citizen governor, List<Citizen> unwantedGovernors){
		for(int i = 0; i < unwantedGovernors.Count; i++){
			if(governor.id == unwantedGovernors[i].id){
				return true;
			}	
		}
		return false;
	}
	public static Color[] kingdomColorCycle = new Color[] {
		new Color32(0xDB, 0x00, 0x00, 0x91), // Red DB000091
		new Color32(0x00, 0x51, 0xF3, 0x91), // Blue 0051F391
		new Color32(0xFF, 0xFF, 0x00, 0x91), // Yellow FFFF00A0
		new Color32(0xFF, 0xFF, 0xFF, 0x91), // White FFFFFF91
		new Color32(0x78, 0xFF, 0x2B, 0x91), // Mint Green 78FF2B91
		new Color32(0xF9, 0x5B, 0xCD, 0x91), // Pink F95BCD91
		new Color32(0x1D, 0x1D, 0x1D, 0x91), // Black 1D1D1D91
		new Color32(0x0F, 0xDD, 0xF0, 0xA0), // Cyan 0FDDF0A0
		new Color32(0xFF, 0x8E, 0x00, 0xA0), // Orange FF8E00A3
		new Color32(0x8D, 0x12, 0xCE, 0x91), // Violet 8D12CE91
		new Color32(0x0E, 0x77, 0x1B, 0x91), // Dark Green 0E771B91
		new Color32(0x8A, 0x07, 0x07, 0x94), // Dark Red 8A070794
		new Color32(0x03, 0x21, 0x8E, 0xA0), // Dark Blue 03218EA0
		new Color32(0xA6, 0x56, 0x00, 0xB9), // Brown A65600B9 
		new Color32(0x8A, 0x80, 0xFD, 0x94), // Light Violet 8A80FD91
		new Color32(0xBC, 0xFF, 0x00, 0xA6) // Yellow Green BCFF00A6
	};

    public static Dictionary<BIOMES, Color> biomeColor = new Dictionary<BIOMES, Color>() {
        {BIOMES.GRASSLAND, new Color(34f/255f, 139f/255f, 34f/255f)},
        {BIOMES.BARE, new Color(106f/255f, 108f/255f, 59f/255f)},
        {BIOMES.DESERT, new Color(93f/255f, 79f/255f, 69f/255f)},
        {BIOMES.FOREST, new Color(34f/255f, 139f/255f, 34f/255f)},
        {BIOMES.SNOW, new Color(255f/255f, 255f/255f, 255f/255f)},
        {BIOMES.TUNDRA, new Color(106f/255f, 108f/255f, 59f/255f)},
        {BIOMES.WOODLAND, new Color(34f/255f, 139f/255f, 34f/255f)}
    };
//	public static string StringReplacer(string text, LogFiller[] logFillers){
//		List<int> specificWordIndexes = new List<int> ();
//		string newText = text;
//		bool hasPeriod = newText.EndsWith (".");
//		if (!string.IsNullOrEmpty (newText)) {
//			string[] words = Utilities.SplitAndKeepDelimiters(newText, new char[]{' ', '.', ','});
//			for (int i = 0; i < words.Length; i++) {
//				if (words [i].Contains ("(%")) {
//					specificWordIndexes.Add (i);
//				}else if(words [i].Contains ("(*")){
//					string strIndex = Utilities.GetStringBetweenTwoChars (words [i], '-', '-');
//					int index = 0;
//					bool isIndex = int.TryParse (strIndex, out index);
//					if(isIndex){
//						words [i] = Utilities.PronounReplacer (words [i], logFillers [index].obj);
//					}
//				}
//			}
//			if(specificWordIndexes.Count == logFillers.Length){
//				for (int i = 0; i < logFillers.Length; i++) {
//					string replacedWord = Utilities.CustomStringReplacer (words [specificWordIndexes [i]], logFillers [i], i);
//					if(!string.IsNullOrEmpty(replacedWord)){
//						words [specificWordIndexes [i]] = replacedWord;
//					}
//				}
//			}
//			newText = string.Empty;
//			for (int i = 0; i < words.Length; i++) {
//				newText += words [i];
//			}
//			newText = newText.Trim (' ');
//		}
//
//		return newText;
//	}
//	public static string LogReplacer(Log log){
//		List<int> specificWordIndexes = new List<int> ();
//		string newText = LocalizationManager.Instance.GetLocalizedValue (log.category, log.file, log.key);
//		bool hasPeriod = newText.EndsWith (".");
//		if (!string.IsNullOrEmpty (newText)) {
//			string[] words = Utilities.SplitAndKeepDelimiters(newText, new char[]{' ', '.', ','});
//			for (int i = 0; i < words.Length; i++) {
//				if (words [i].Contains ("(%")) {
//					specificWordIndexes.Add (i);
//				}else if(words [i].Contains ("(*")){
//					string strIndex = Utilities.GetStringBetweenTwoChars (words [i], '-', '-');
//					int index = 0;
//					bool isIndex = int.TryParse (strIndex, out index);
//					if(isIndex){
//						words [i] = Utilities.PronounReplacer (words [i], log.fillers [index].obj);
//					}
//				}
//			}
//			if(specificWordIndexes.Count == log.fillers.Count){
//				for (int i = 0; i < log.fillers.Count; i++) {
//					string replacedWord = Utilities.CustomStringReplacer (words [specificWordIndexes [i]], log.fillers [i], i);
//					if(!string.IsNullOrEmpty(replacedWord)){
//						words [specificWordIndexes [i]] = replacedWord;
//					}
//				}
//			}
//			newText = string.Empty;
//			for (int i = 0; i < words.Length; i++) {
//				newText += words [i];
//			}
//			newText = newText.Trim (' ');
//		}
//
//		return newText;
//	}
	public static string LogReplacer(Log log){
		if(log == null){
			return string.Empty;
		}
		string replacedWord = string.Empty;
		List<int> specificWordIndexes = new List<int> ();
		string newText = LocalizationManager.Instance.GetLocalizedValue (log.category, log.file, log.key);
		bool hasPeriod = newText.EndsWith (".");

		if (!string.IsNullOrEmpty (newText)) {
			string[] words = Utilities.SplitAndKeepDelimiters(newText, new char[]{' ', '.', ',', '\'', '!'});
			for (int i = 0; i < words.Length; i++) {
				replacedWord = string.Empty;
				if (words [i].StartsWith ("%") && (words[i].EndsWith("%") || words[i].EndsWith("@"))) { //OBJECT
					replacedWord = Utilities.CustomStringReplacer (words[i], ref log.fillers);
				}else if(words [i].StartsWith ("%") && (words[i].EndsWith("a") || words[i].EndsWith("b"))){ //PRONOUN
					replacedWord = Utilities.CustomPronounReplacer (words[i], log.fillers);
				}
				if(!string.IsNullOrEmpty(replacedWord)){
					words [i] = replacedWord;
				}
			}
			newText = string.Empty;
			for (int i = 0; i < words.Length; i++) {
				newText += words [i];
			}
			newText = newText.Trim (' ');
		}

		return newText;
	}
	public static string CustomPronounReplacer(string wordToBeReplaced, List<LogFiller> objectLog){
		LOG_IDENTIFIER identifier = Utilities.logIdentifiers [wordToBeReplaced.Substring(1, 2)];
		string wordToReplace = string.Empty;
//		string value = wordToBeReplaced.Substring(1, 2);
		string strIdentifier = identifier.ToString ();
		string pronouns = Utilities.GetPronoun(strIdentifier.Last (), wordToBeReplaced.Last());

		LOG_IDENTIFIER logIdentifier = LOG_IDENTIFIER.ACTIVE_CHARACTER;
		if(strIdentifier.Contains("KING_1")){
			logIdentifier = LOG_IDENTIFIER.KING_1;
		}else if(strIdentifier.Contains("KING_2")){
			logIdentifier = LOG_IDENTIFIER.KING_2;
		}else if(strIdentifier.Contains("TARGET_CHARACTER")){
			logIdentifier = LOG_IDENTIFIER.TARGET_CHARACTER;
		}else if(strIdentifier.Contains("KING_3")){
			logIdentifier = LOG_IDENTIFIER.KING_3;
		}
		for(int i = 0; i < objectLog.Count; i++){
			if(objectLog[i].identifier == logIdentifier){
				wordToReplace = Utilities.PronounReplacer (pronouns, objectLog [i].obj);
				break;
			}
		}

		return wordToReplace;

	}
	public static string CustomStringReplacer(string wordToBeReplaced, ref List<LogFiller> objectLog){
		string wordToReplace = string.Empty;
		string strLogIdentifier = wordToBeReplaced.Remove(0,1);
		strLogIdentifier = strLogIdentifier.Remove((strLogIdentifier.Length - 1), 1);
		LOG_IDENTIFIER identifier = Utilities.logIdentifiers[strLogIdentifier];
		if(wordToBeReplaced.EndsWith("@")){
			for(int i = 0; i < objectLog.Count; i++){
				if(objectLog[i].identifier == identifier){
					if (objectLog[i].identifier == LOG_IDENTIFIER.RANDOM_GOVERNOR_1 || objectLog[i].identifier == LOG_IDENTIFIER.RANDOM_GOVERNOR_2){
						if(objectLog [i].obj is Kingdom){
							Kingdom kingdom = (Kingdom)objectLog [i].obj;
							Citizen randomGovernor = kingdom.GetRandomGovernorFromKingdom ();
							objectLog [i] = new LogFiller(randomGovernor, randomGovernor.name, objectLog[i].identifier);
						}
					}
					wordToReplace = "[url=" + i.ToString() + "][b]" + objectLog[i].value + "[/b][/url]";
					break;
				}
			}
		}else if(wordToBeReplaced.EndsWith("%")){
			for(int i = 0; i < objectLog.Count; i++){
				if(objectLog[i].identifier == identifier){
					if (objectLog[i].identifier == LOG_IDENTIFIER.RANDOM_GOVERNOR_1 || objectLog[i].identifier == LOG_IDENTIFIER.RANDOM_GOVERNOR_2){
						if(objectLog [i].obj is Kingdom){
							Kingdom kingdom = (Kingdom)objectLog [i].obj;
							Citizen randomGovernor = kingdom.GetRandomGovernorFromKingdom ();
							objectLog [i] = new LogFiller(randomGovernor, randomGovernor.name, objectLog[i].identifier);
						}
					}
					wordToReplace = objectLog[i].value;
					break;
				}
			}
		}

		return wordToReplace;

	}
    //	public static string CustomStringReplacer(string wordToBeReplaced, LogFiller objectLog, int index){
    //		string wordToReplace = string.Empty;
    //		string value = string.Empty;
    //
    //		if(wordToBeReplaced.Contains("@")){
    //			wordToReplace = "[url=" + index.ToString() + "][b]" + objectLog.value + "[/b][/url]";
    //		}else{
    //			wordToReplace = objectLog.value;
    //		}
    //
    //		return wordToReplace;
    //
    //	}

    public static Color darkGreen = new Color(0f / 255f, 100f / 255f, 0f / 255f);
    public static Color lightGreen = new Color(124f / 255f, 252f / 255f, 0f / 255f);
    public static Color darkRed = new Color(139f / 255f, 0f / 255f, 0f / 255f);
    public static Color lightRed = new Color(255f / 255f, 0f / 255f, 0f / 255f);

    public static Color GetColorForTrait(object trait) {
        if (trait is CHARISMA) {
            switch ((CHARISMA)trait) {
                case CHARISMA.CHARISMATIC:
                    return darkGreen;
                case CHARISMA.REPULSIVE:
                    return darkRed;
                default:
                    return Color.white;
            }
        } else if (trait is INTELLIGENCE) {
            switch ((INTELLIGENCE)trait) {
                case INTELLIGENCE.SMART:
                    return darkGreen;
                case INTELLIGENCE.DUMB:
                    return darkRed;
                default:
                    return Color.white;
            }
        } else if (trait is EFFICIENCY) {
            switch ((EFFICIENCY)trait) {
                case EFFICIENCY.EFFICIENT:
                    return darkGreen;
                case EFFICIENCY.INEPT:
                    return darkRed;
                default:
                    return Color.white;
            }
        } else if (trait is SCIENCE) {
            switch ((SCIENCE)trait) {
                case SCIENCE.ERUDITE:
                    return darkGreen;
                case SCIENCE.ACADEMIC:
                    return lightGreen;
                case SCIENCE.IGNORANT:
                    return darkRed;
                default:
                    return Color.white;
            }
        } else if (trait is MILITARY) {
            switch ((MILITARY)trait) {
                case MILITARY.HOSTILE:
                    return darkRed;
                case MILITARY.MILITANT:
                    return lightRed;
                case MILITARY.PACIFIST:
                    return darkGreen;
                default:
                    return Color.white;
            }
        } else if (trait is LOYALTY) {
            switch ((LOYALTY)trait) {
                case LOYALTY.SCHEMING:
                    return darkRed;
                case LOYALTY.LOYAL:
                    return darkGreen;
                default:
                    return Color.white;
            }
        }
        return Color.black;
    }

	public static string GetPronoun(string type, string caseIdentifier){
		if(type == "S"){
			if(caseIdentifier == "a"){
				return "He/She";
			}
			return "he/she";
		}else if(type == "O"){
			if(caseIdentifier == "a"){
				return "Him/Her";
			}
			return "him/her";
		}else if(type == "P"){
			if(caseIdentifier == "a"){
				return "His/Her";
			}
			return "his/her";
		}else if(type == "R"){
			if(caseIdentifier == "a"){
				return "Himself/Herself";
			}
			return "himself/herself";
		}
		return string.Empty;
	}
	public static string GetPronoun(char type, char caseIdentifier){
		if(type == 'S'){
			if(caseIdentifier == 'a'){
				return "He/She";
			}
			return "he/she";
		}else if(type == 'O'){
			if(caseIdentifier == 'a'){
				return "Him/Her";
			}
			return "him/her";
		}else if(type == 'P'){
			if(caseIdentifier == 'a'){
				return "His/Her";
			}
			return "his/her";
		}else if(type == 'R'){
			if(caseIdentifier == 'a'){
				return "Himself/Herself";
			}
			return "himself/herself";
		}
		return string.Empty;
	}
	public static Dictionary<string, LOG_IDENTIFIER> logIdentifiers = new Dictionary<string, LOG_IDENTIFIER> () {
		{"00", LOG_IDENTIFIER.ACTIVE_CHARACTER},
		{"01", LOG_IDENTIFIER.KINGDOM_1},
		{"02", LOG_IDENTIFIER.KING_1},
		{"03", LOG_IDENTIFIER.KING_1_SPOUSE},
		{"04", LOG_IDENTIFIER.CITY_1},
		{"05", LOG_IDENTIFIER.GOVERNOR_1},
		{"06", LOG_IDENTIFIER.RANDOM_CITY_1},
		{"07", LOG_IDENTIFIER.RANDOM_GOVERNOR_1},
		{"10", LOG_IDENTIFIER.TARGET_CHARACTER},
		{"11", LOG_IDENTIFIER.KINGDOM_2},
		{"12", LOG_IDENTIFIER.KING_2},
		{"13", LOG_IDENTIFIER.KING_2_SPOUSE},
		{"14", LOG_IDENTIFIER.CITY_2},
		{"15", LOG_IDENTIFIER.GOVERNOR_2},
		{"16", LOG_IDENTIFIER.RANDOM_CITY_2},
		{"17", LOG_IDENTIFIER.RANDOM_GOVERNOR_2},
		{"20", LOG_IDENTIFIER.CHARACTER_3},
		{"21", LOG_IDENTIFIER.KINGDOM_3},
		{"22", LOG_IDENTIFIER.KING_3},
		{"23", LOG_IDENTIFIER.KING_3_SPOUSE},
		{"24", LOG_IDENTIFIER.CITY_3},
		{"25", LOG_IDENTIFIER.GOVERNOR_3},
		{"26", LOG_IDENTIFIER.RANDOM_CITY_3},
		{"27", LOG_IDENTIFIER.RANDOM_GOVERNOR_3},
		{"81", LOG_IDENTIFIER.TRIGGER_REASON},
		{"82", LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME},
		{"83", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_S},
		{"84", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_O},
		{"85", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_P},
		{"86", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_R},
		{"87", LOG_IDENTIFIER.KING_1_PRONOUN_S},
		{"88", LOG_IDENTIFIER.KING_1_PRONOUN_O},
		{"89", LOG_IDENTIFIER.KING_1_PRONOUN_P},
		{"90", LOG_IDENTIFIER.KING_1_PRONOUN_R},
		{"91", LOG_IDENTIFIER.KING_2_PRONOUN_S},
		{"92", LOG_IDENTIFIER.KING_2_PRONOUN_O},
		{"93", LOG_IDENTIFIER.KING_2_PRONOUN_P},
		{"94", LOG_IDENTIFIER.KING_2_PRONOUN_R},
		{"95", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_S},
		{"96", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_O},
		{"97", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_P},
		{"98", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_R},
		{"99", LOG_IDENTIFIER.SECESSION_CITIES},
		{"100", LOG_IDENTIFIER.GAME_EVENT},
		{"101", LOG_IDENTIFIER.DATE},
		{"102", LOG_IDENTIFIER.KING_3_PRONOUN_S},
		{"103", LOG_IDENTIFIER.KING_3_PRONOUN_O},
		{"104", LOG_IDENTIFIER.KING_3_PRONOUN_P},
		{"105", LOG_IDENTIFIER.KING_3_PRONOUN_R},
		{"106", LOG_IDENTIFIER.OTHER},
		{"107", LOG_IDENTIFIER.CRIME_DETAILS},
		{"108", LOG_IDENTIFIER.CRIME_PUNISHMENT},
		{"109", LOG_IDENTIFIER.LAIR_NAME},
		{"110", LOG_IDENTIFIER.WAR_NAME},
		{"111", LOG_IDENTIFIER.ALLIANCE_NAME},
	};

    public static EVENT_TYPES[] eventsNotToShow = new EVENT_TYPES[] {
        EVENT_TYPES.ADVENTURE,
        EVENT_TYPES.INVASION_PLAN,
        EVENT_TYPES.HUNT_LAIR,
        EVENT_TYPES.EXPANSION,
        EVENT_TYPES.ALTAR_OF_BLESSING,
        EVENT_TYPES.TRIBUTE,
        EVENT_TYPES.PROVOCATION,
        EVENT_TYPES.MUTUAL_DEFENSE_TREATY,
        EVENT_TYPES.MILITARY_ALLIANCE_OFFER,
        EVENT_TYPES.INSTIGATION
    };

	public static string PronounReplacer(string word, object genderSubject){
//		string pronoun = Utilities.GetStringBetweenTwoChars (word, '_', '_');
		string[] pronouns = word.Split ('/');

		if(genderSubject is Citizen){
			GENDER gender = ((Citizen)genderSubject).gender;
			if(gender == GENDER.MALE){
				if(pronouns.Length > 0){
					if(!string.IsNullOrEmpty(pronouns[0])){
						return pronouns [0];
					}
				}
			}else{
				if (pronouns.Length > 1) {
					if (!string.IsNullOrEmpty (pronouns [0])) {
						return pronouns [1];
					}
				}
			}


		}
		return string.Empty;
	}
//	public static string PronounReplacer(string word, object genderSubject){
//		string pronoun = Utilities.GetStringBetweenTwoChars (word, '_', '_');
//		string[] pronouns = pronoun.Split ('/');
//
//		if(genderSubject is Citizen){
//			GENDER gender = ((Citizen)genderSubject).gender;
//			if(gender == GENDER.MALE){
//				if(pronouns.Length > 0){
//					if(!string.IsNullOrEmpty(pronouns[0])){
//						return pronouns [0];
//					}
//				}
//			}else{
//				if (pronouns.Length > 1) {
//					if (!string.IsNullOrEmpty (pronouns [0])) {
//						return pronouns [1];
//					}
//				}
//			}
//
//
//		}
//		return string.Empty;
//	}
	public static string GetStringBetweenTwoChars (string word, char first, char last){
		int indexFirst = word.IndexOf (first);
		int indexLast = word.LastIndexOf (last);

		if(indexFirst == -1 || indexLast == -1){
			return string.Empty;
		}
		indexFirst += 1;
		if(indexFirst >= word.Length){
			return string.Empty;
		}

		return word.Substring (indexFirst, (indexLast - indexFirst));
	}
	public static List<string> GetAllWordsInAString(string wordToFind, string text){
		List<string> words = new List<string> ();
		string word = string.Empty;
		int index = 0;
		int wordCount = 0;
		int startingIndex = index;
		while(index != -1){
			index = text.IndexOf (wordToFind, startingIndex);
			if(index != -1){
				startingIndex = index + 1;
				if(startingIndex > text.Length - 1){
					startingIndex = text.Length - 1;
				}

				wordCount = 0;
				for(int i = index; i < text.Length; i++){
					if(text[i] != ' '){
						wordCount += 1;
					}else{
						break;
					}
				}
				word = text.Substring (index, wordCount);
				words.Add (word);
			}

		}
		return words;
	}

	public static GameDate GetNewDateAfterNumberOfDays(int month, int day, int year, int numOfDaysElapsed){
		GameDate newDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        newDate.SetDate(month, day, year);
        newDate.AddDays(numOfDaysElapsed);

  //      int newDay = day;
  //      int newMonth = month;
  //      int newYear = year;

  //      for (int i = 0; i < numOfDaysElapsed; i++) {
  //          newDay += 1;
  //          if (newDay > GameManager.daysInMonth[newMonth]) {
  //              newDay = 1;
  //              newMonth += 1;
  //              if (newMonth > 12) {
  //                  newMonth = 1;
  //                  newYear += 1;
  //              }
  //          }
  //      }
		//DateTime newDate = new DateTime (newYear, newMonth, newDay);
		//inputDate = inputDate.AddDays (numOfDaysElapsed);
		return newDate;
	}
	public static string[] SplitAndKeepDelimiters(string s, params char[] delimiters){
		var parts = new List<string>();
		if (!string.IsNullOrEmpty(s))
		{
			int iFirst = 0;
			do
			{
				int iLast = s.IndexOfAny(delimiters, iFirst);
				if (iLast >= 0)
				{
					if (iLast > iFirst)
						parts.Add(s.Substring(iFirst, iLast - iFirst)); //part before the delimiter
					parts.Add(new string(s[iLast], 1));//the delimiter
					iFirst = iLast + 1;
					continue;
				}

				//No delimiters were found, but at least one character remains. Add the rest and stop.
				parts.Add(s.Substring(iFirst, s.Length - iFirst));
				break;

			} while (iFirst < s.Length);
		}

		return parts.ToArray();
	}

	public static void SetSpriteSortingLayer(SpriteRenderer sprite, string layerName){
		sprite.sortingLayerName = layerName;
	}

    public static void SetLayerRecursively(GameObject go, int layerNumber) {
        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true)) {
            trans.gameObject.layer = layerNumber;
        }
    }

    public static bool CanReachInTime(EVENT_TYPES eventType, List<HexTile> path, int duration){
		switch (eventType) {
		case EVENT_TYPES.STATE_VISIT:
			return true;
		case EVENT_TYPES.RAID:
			return true;
		case EVENT_TYPES.JOIN_WAR_REQUEST:
			return true;
		case EVENT_TYPES.EXPANSION:
			return true;
        case EVENT_TYPES.TRADE:
            return true;
		case EVENT_TYPES.ATTACK_CITY:
			return true;
        case EVENT_TYPES.REQUEST_PEACE:
            return true;
		case EVENT_TYPES.REINFORCE_CITY:
			return true;
		case EVENT_TYPES.RIOT_WEAPONS:
			return true;
		case EVENT_TYPES.REBELLION:
			return true;
        case EVENT_TYPES.PLAGUE:
            return true;
		case EVENT_TYPES.SCOURGE_CITY:
			return true;
		case EVENT_TYPES.BOON_OF_POWER:
			return true;
		case EVENT_TYPES.PROVOCATION:
			return true;
		case EVENT_TYPES.EVANGELISM:
			return true;
		case EVENT_TYPES.SPOUSE_ABDUCTION:
			return true;
		case EVENT_TYPES.FIRST_AND_KEYSTONE:
			return true;
		case EVENT_TYPES.RUMOR:
			return true;
		case EVENT_TYPES.SLAVES_MERCHANT:
			return true;
		case EVENT_TYPES.HIDDEN_HISTORY_BOOK:
			return true;
        case EVENT_TYPES.HYPNOTISM:
            return true;
        case EVENT_TYPES.KINGDOM_HOLIDAY:
            return true;
		case EVENT_TYPES.SERUM_OF_ALACRITY:
			return true;
        case EVENT_TYPES.DEVELOP_WEAPONS:
            return true;
        case EVENT_TYPES.KINGS_COUNCIL:
            return true;
        case EVENT_TYPES.ADVENTURE:
            return true;
        case EVENT_TYPES.EVIL_INTENT:
            return true;
        case EVENT_TYPES.ALTAR_OF_BLESSING:
			return true;
		case EVENT_TYPES.GREAT_STORM:
			return true;
		case EVENT_TYPES.HUNT_LAIR:
			return true;
		case EVENT_TYPES.ANCIENT_RUIN:
			return true;
        case EVENT_TYPES.MILITARY_ALLIANCE_OFFER:
			return true;
		case EVENT_TYPES.MUTUAL_DEFENSE_TREATY:
			return true;
        case EVENT_TYPES.TRIBUTE:
            return true;
        case EVENT_TYPES.INSTIGATION:
            return true;
		case EVENT_TYPES.SEND_RELIEF_GOODS:
			if(duration == -1){
				return true;
			}else{
				if (path.Count > duration) {
					return false;
				}
				return true;
			}
        case EVENT_TYPES.BORDER_CONFLICT:
		    if(duration == -1){
			    return true;
		    }else{
			    if (path.Count > duration) {
				    return false;
			    }
			    return true;
		    }
		case EVENT_TYPES.DIPLOMATIC_CRISIS:
			if(duration == -1){
				return true;
			}else{
				if (path.Count > duration) {
					return false;
				}
				return true;
			}
		case EVENT_TYPES.INVASION_PLAN:
			if(duration == -1){
				return true;
			}else{
				if (path.Count > duration) {
					return false;
				}
				return true;
			}
		case EVENT_TYPES.SABOTAGE:
			if(duration == -1){
				return true;
			}else{
				if (path.Count > duration) {
					return false;
				}
				return true;
			}
		case EVENT_TYPES.ASSASSINATION:
			if(duration == -1){
				return true;
			}else{
				if (path.Count > duration) {
					return false;
				}
				return true;
			}
		case EVENT_TYPES.SECESSION:
			if(duration == -1){
				return true;
			}else{
				if (path.Count > duration) {
					return false;
				}
				return true;
			}
		}
		return false;
	}

    public static string FirstLetterToUpperCase(string s) {
        if (string.IsNullOrEmpty(s))
            throw new ArgumentException("There is no first letter");

        char[] a = s.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new string(a);
    }

    public static string NormalizeString(string s) {
        s = s.ToLower();
        string[] words = s.Split('_');
        string normalizedString = Utilities.FirstLetterToUpperCase(words.First());
        for (int i = 1; i < words.Length; i++) {
            normalizedString += " " + words[i];
        }
        return normalizedString;
    }

    public static bool IsCurrentDayMultipleOf(int multiple) {
        if ((GameManager.Instance.days % multiple) == 0) {
            return true;
        }
        return false;
    }

    public static List<HexTile> MergeHexLists(List<HexTile> list1, List<HexTile> list2) {
        Dictionary<int, HexTile> dict = list2.ToDictionary(h => h.id, v => v);
        foreach (HexTile h in list1) {
            dict[h.id] = h;
        }
        return dict.Values.ToList();
    }


    /*
     * <summary>
     * Get a random integer given a minimum and maximum range and a minimum and maximum 
     * mean to simulate a bell curve.
     * </summary>
     * <param name="min"> The distributions minimum value [inclusive]</param>
     * <param name="max"> The distributions maximum value [inclusive]</param>
     * <param name="minMean"> The distributions minimum mean value [minimum value for bell curve]</param>
     * <param name="maxMean"> The distributions maximum mean value [maximum value for bell curve]</param>
     * */
    public static int BellCurveRandomRange(int min, int max, int minMean, int maxMean) {
        float rand = UnityEngine.Random.value;

        if (rand <= .3f)
            return UnityEngine.Random.Range(min, minMean);
        if (rand <= .8f)
            return UnityEngine.Random.Range(minMean, maxMean);

        return UnityEngine.Random.Range(maxMean, max);
    }

    #region Lycanthropy
    public static int GetMoonPhase(int year, int month, int day) {
        /*k
          Calculates the moon phase (0-7), accurate to 1 segment.
          0 = > new moon.
          4 => Full moon.
        */

        int g, e;

        if (month == 1) --day;
        else if (month == 2) day += 30;
        else // m >= 3
        {
            day += 28 + (month - 2) * 3059 / 100;

            //// adjust for leap years
            //if (!(year & 3)) ++day;
            //if ((year % 100) == 0) --day;
        }

        g = (year - 1900) % 19 + 1;
        e = (11 * g + 18) % 30;
        if ((e == 25 && g > 11) || e == 24) e++;
        return ((((e + day) * 6 + 11) % 177) / 22 & 7);
    }
    #endregion

    #region Plague
    public static string[] plagueAdjectives = new string[] {
        "Red", "Green", "Yellow", "Black", "Rotting", "Silent", "Screaming", "Trembling", "Sleeping",
        "Cat", "Dog", "Pig", "Lamb", "Lizard", "Bog", "Death", "Stomach", "Eye", "Finger", "Rabid",
        "Fatal", "Blistering", "Icy", "Scaly", "Sexy", "Violent", "Necrotic", "Foul", "Vile", "Nasty",
        "Ghastly", "Malodorous", "Cave", "Phantom", "Wicked", "Strange"
    };

    public static string[] plagueDieseases = new string[] {
        "Sores", "Ebola", "Anthrax", "Pox", "Face", "Sneeze", "Gangrene", "Throat", "Rash", "Warts",
        "Cholera", "Colds", "Ache", "Syndrome", "Tumor", "Chills", "Blisters", "Mouth", "Fever", "Delirium",
        "Measles", "Mutata", "Disease"
    };
    public static string GeneratePlagueName() {
        return plagueAdjectives[UnityEngine.Random.Range(0, plagueAdjectives.Length)] + " " + plagueDieseases[UnityEngine.Random.Range(0, plagueDieseases.Length)];
    }
    #endregion

    public static List<T> Intersect<T> (List<T> firstList, List<T> secondList){
		List<T> newList = new List<T> ();
		for (int i = 0; i < firstList.Count; i++) {
			for (int j = 0; j < secondList.Count; j++) {
				if (firstList[i].Equals(secondList[j])) {
					newList.Add (firstList [i]);
					break;
				}
			}
		}
		return newList;
	}
	public static List<T> Union<T> (List<T> firstList, List<T> secondList){
		bool hasMatched = false;
		List<T> newList = new List<T> ();
		for (int i = 0; i < firstList.Count; i++) {
			newList.Add (firstList [i]);
		}
		for (int i = 0; i < secondList.Count; i++) {
			hasMatched = false;
			for (int j = 0; j < firstList.Count; j++) {
				if (secondList [i].Equals(firstList [j])) {
					hasMatched = true;
					break;
				}
			}
			if(!hasMatched){
				newList.Add (secondList [i]);
			}
		}
		return newList;
	}

    #region AI
    public static Vector2 PickRandomPointInCircle(Vector2 origin, float radius) {
        Vector2 point = UnityEngine.Random.insideUnitCircle * radius;
        point += (Vector2)origin;
        return point;
    }
    #endregion

	public static bool AreTwoCitiesConnected(City sourceCity, City targetCity, PATHFINDING_MODE pathFindingMode, Kingdom kingdom = null){
		List<HexTile> path = PathGenerator.Instance.GetPath (sourceCity.hexTile, targetCity.hexTile, pathFindingMode, kingdom);
		if(path != null){
			return true;
		}
		return false;
	}
	public static bool HasPath(HexTile startingLocation, HexTile targetLocation, PATHFINDING_MODE pathFindingMode, Kingdom kingdom = null){
		List<HexTile> path = PathGenerator.Instance.GetPath (startingLocation, targetLocation, pathFindingMode, kingdom);
		if(path != null){
			return true;
		}
		return false;
	}
}
