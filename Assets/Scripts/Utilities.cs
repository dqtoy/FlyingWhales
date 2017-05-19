using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class Utilities : MonoBehaviour {
	private static System.Random rng = new System.Random(); 
	public static int lastKingdomID = 0;
	public static int lastCitizenID = 0;
	public static int lastCityID = 0;
	public static int lastCampaignID = 0;
	public static int lastEventID = 0;
	public static int lastKingdomColorIndex = 0;
	public static int defaultCampaignExpiration = 8;
	public static float defenseBuff = 1.20f;
	public static LANGUAGES defaultLanguage = LANGUAGES.ENGLISH;
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
		} else if (obj is GameEvent) {
			lastEventID += 1;
			return lastEventID;
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

	public static Dictionary<BIOMES, SpecialResourceChance> specialResourcesLookup = new Dictionary<BIOMES, SpecialResourceChance> () { 
		{BIOMES.BARE, new SpecialResourceChance(
			new RESOURCE[] {RESOURCE.NONE}, 
			new int[] {100})
		},
        
		{BIOMES.GRASSLAND, new SpecialResourceChance(
            //new RESOURCE[] {RESOURCE.WHEAT, RESOURCE.RICE, RESOURCE.DEER, RESOURCE.CEDAR, RESOURCE.GRANITE, RESOURCE.SLATE, RESOURCE.MITHRIL, RESOURCE.COBALT},
            //new int[] {100, 20, 40, 20, 60, 35, 5, 5})
            new RESOURCE[] {RESOURCE.CORN, RESOURCE.CEDAR, RESOURCE.GRANITE, RESOURCE.MITHRIL},
            new int[] {20, 30, 70, 3})
        },

		{BIOMES.WOODLAND, new SpecialResourceChance(
			//new RESOURCE[] {RESOURCE.CORN, RESOURCE.WHEAT, RESOURCE.DEER, RESOURCE.PIG, RESOURCE.OAK, RESOURCE.EBONY, RESOURCE.GRANITE, RESOURCE.SLATE, RESOURCE.MANA_STONE, RESOURCE.COBALT},
   //         new int[] {40, 12, 65, 25, 90, 22, 60, 12, 5, 5})
            new RESOURCE[] {RESOURCE.RICE, RESOURCE.CEDAR, RESOURCE.GRANITE, RESOURCE.COBALT},
            new int[] {20, 70, 30, 4})
        },


		{BIOMES.FOREST, new SpecialResourceChance(
			//new RESOURCE[] {RESOURCE.EBONY, RESOURCE.DEER, RESOURCE.BEHEMOTH, RESOURCE.MANA_STONE, RESOURCE.MITHRIL, RESOURCE.GOLD}, 
			//new int[] {15, 40, 15, 0, 0, 0})
            new RESOURCE[] {RESOURCE.DEER, RESOURCE.CEDAR, RESOURCE.MANA_STONE},
            new int[] {20, 100, 4})
        },

		{BIOMES.DESERT, new SpecialResourceChance(
			//new RESOURCE[] {RESOURCE.DEER, RESOURCE.PIG, RESOURCE.SLATE, RESOURCE.MARBLE, RESOURCE.MITHRIL, RESOURCE.COBALT, RESOURCE.GOLD}, 
			//new int[] {20, 20, 15, 15, 0, 0, 0})
            new RESOURCE[] {RESOURCE.PIG, RESOURCE.GRANITE, RESOURCE.COBALT, RESOURCE.MITHRIL},
            new int[] {20, 100, 4, 4})
        },

		{BIOMES.TUNDRA, new SpecialResourceChance(
			//new RESOURCE[] {RESOURCE.DEER, RESOURCE.PIG, RESOURCE.CEDAR, RESOURCE.GRANITE, RESOURCE.SLATE, RESOURCE.MANA_STONE, RESOURCE.GOLD}, 
			//new int[] {50, 15, 10, 25, 10, 0, 0})
            new RESOURCE[] {RESOURCE.WHEAT, RESOURCE.CEDAR, RESOURCE.GRANITE, RESOURCE.MANA_STONE},
            new int[] {20, 50, 50, 4})
        },

		{BIOMES.SNOW, new SpecialResourceChance(
			//new RESOURCE[] {RESOURCE.CORN, RESOURCE.WHEAT, RESOURCE.DEER, RESOURCE.PIG, RESOURCE.MARBLE, RESOURCE.MITHRIL, RESOURCE.COBALT}, 
			//new int[] {15, 5, 15, 5, 5, 0, 0})
            new RESOURCE[] {RESOURCE.BEHEMOTH, RESOURCE.GRANITE, RESOURCE.CEDAR, RESOURCE.COBALT},
            new int[] {20, 50, 50, 4})
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
			return new Color (0f, (139f/255f), (69f/255f), 1f);
		} else if (status == RELATIONSHIP_STATUS.FRIEND) {
			return new Color (0f, 1f, (127f/255f), 1f);
		} else if (status == RELATIONSHIP_STATUS.WARM) {
			return new Color ((118f/255f), (238f/255f), (198f/255f), 1f);
		} else if (status == RELATIONSHIP_STATUS.NEUTRAL) {
			return Color.white;
		} else if (status == RELATIONSHIP_STATUS.COLD) {
			return new Color ((240f/255f), (128f/255f), (128f/255f), 1f);
		} else if (status == RELATIONSHIP_STATUS.ENEMY) {
			return new Color (1f, (64f/255f), (64f/255f), 1f);
		} else if (status == RELATIONSHIP_STATUS.RIVAL) {
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

	public static void ChangePossiblePretendersRecursively(Citizen parent, Citizen descendant){
		if(!descendant.isDead){
			parent.possiblePretenders.Add (descendant);
		}

		for(int i = 0; i < descendant.children.Count; i++){
			if(descendant.children[i] != null){
				ChangePossiblePretendersRecursively (parent, descendant.children [i]);
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
	}

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
	public static List<Citizen> GetUnwantedGovernors(Citizen king){
		List<Citizen> unwantedGovernors = new List<Citizen> ();
		for(int i = 0; i < king.civilWars.Count; i++){
			if(king.civilWars[i].isGovernor){
				unwantedGovernors.Add (king.civilWars [i]);
			}
		}
		for(int i = 0; i < king.successionWars.Count; i++){
			if(king.successionWars[i].isGovernor){
				unwantedGovernors.Add (king.successionWars [i]);
			}
		}
		for(int i = 0; i < king.city.kingdom.cities.Count; i++){
			if(king.city.kingdom.cities[i].governor.supportedCitizen != null){
				unwantedGovernors.Add (king.city.kingdom.cities [i].governor);
			}
		}

		return unwantedGovernors;
	}

	public static Color[] kingdomColorCycle = new Color[] {
		new Color32(0x10, 0x2F, 0xB2, 0xFF),//Dark Blue
		new Color32(0xF3, 0xFF, 0x2C, 0xFF),//Yellow
		new Color32(0xAA, 0xFF, 0x88, 0xFF),//Mint Green
		new Color32(0xFF, 0xB2, 0x2C, 0xFF),//Orange
		new Color32(0xF2, 0xF2, 0xF2, 0xFF),//White
		new Color32(0xBB, 0x74, 0x17, 0xFF),//Brown
		new Color32(0xB2, 0x10, 0x10, 0xFF),//Crimson
		new Color32(0x37, 0x37, 0x37, 0xFF),//Black
		new Color32(0x88, 0xFF, 0xE2, 0xFF),//Cyan
		new Color32(0xFC, 0x9F, 0xFF, 0xFF)//Pink
	};

	public static string LogReplacer(Log log){
		List<int> specificWordIndexes = new List<int> ();
		string newText = LocalizationManager.Instance.GetLocalizedValue (log.category, log.file, log.key);
		bool hasPeriod = newText.EndsWith (".");
		if (!string.IsNullOrEmpty (newText)) {
			string[] words = Utilities.SplitAndKeepDelimiters(newText, new char[]{' ', '.', ','});
			for (int i = 0; i < words.Length; i++) {
				if (words [i].Contains ("(%")) {
					specificWordIndexes.Add (i);
				}else if(words [i].Contains ("(*")){
					string strIndex = Utilities.GetStringBetweenTwoChars (words [i], '-', '-');
					int index = 0;
					bool isIndex = int.TryParse (strIndex, out index);
					if(isIndex){
						words [i] = Utilities.PronounReplacer (words [i], log.fillers [index].obj);
					}
				}
			}
			if(specificWordIndexes.Count == log.fillers.Count){
				for (int i = 0; i < log.fillers.Count; i++) {
					string replacedWord = Utilities.CustomStringReplacer (words [specificWordIndexes [i]], log.fillers [i], i);
					if(!string.IsNullOrEmpty(replacedWord)){
						words [specificWordIndexes [i]] = replacedWord;
					}
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
	public static string CustomStringReplacer(string wordToBeReplaced, LogFiller objectLog, int index){
		string wordToReplace = string.Empty;
		string value = string.Empty;

		if(wordToBeReplaced.Contains("@")){
			wordToReplace = "[url=" + index.ToString() + "][b]" + objectLog.value + "[/b][/url]";
		}else{
			wordToReplace = objectLog.value;
		}

		return wordToReplace;

	}
	public static string PronounReplacer(string word, object genderSubject){
		string pronoun = Utilities.GetStringBetweenTwoChars (word, '_', '_');
		string[] pronouns = pronoun.Split ('/');

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
}
