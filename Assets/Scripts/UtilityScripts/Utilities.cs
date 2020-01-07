﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;
using UnityEngine.UI;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

public class Utilities : MonoBehaviour {
    public static System.Random rng = new System.Random();
    public static int lastKingdomColorIndex = 0;
    public static int lastFactionColorIndex = 0;
    public static int lastAlliancePoolID = 0;
    public static int lastWarfareID = 0;
    public static int lastLogID = 0;
    public static int lastLandmarkID = 0;
    public static int lastFactionID = 0;
    public static int lastCharacterID = 0;
    public static int lastItemID = 0;
    public static int lastAreaID = 0;
    public static int lastMonsterID = 0;
    public static int lastPartyID = 0;
    public static int lastSquadID = 0;
    public static int lastCharacterSimID = 0;
    //public static int lastInteractionID = 0;
    public static int lastTileObjectID = 0;
    public static int lastStructureID = 0;
    public static int lastRegionID = 0;
    public static int lastJobID = 0;
    public static int lastBurningSourceID = 0;
    public static int lastSpecialObjectID = 0;

    public static float defenseBuff = 1.20f;
    public static int defaultCityHP = 300;

    public static LANGUAGES defaultLanguage = LANGUAGES.ENGLISH;
    public static string dataPath {
        get {
#if UNITY_EDITOR
            return Application.dataPath + "/Resources/Data/";
#elif UNITY_STANDALONE
            return Application.streamingAssetsPath + "/Data/";
#endif
        }
    }
    public static string worldConfigsSavePath { get { return Application.persistentDataPath + "/Saves/"; } }
    public static string gameSavePath { get { return Application.persistentDataPath + "/GameSaves/"; } }
    public static string worldConfigsTemplatesPath { get { return Application.streamingAssetsPath + "/WorldTemplates/"; } }
    public static string worldConfigFileExt { get { return ".worldConfig"; } }
    public static string portraitsSavePath { get { return dataPath + "PortraitSettings/"; } }
    public static string portraitFileExt { get { return ".portraitSetting"; } }

    /*
	 * Set unique id
	 * */
    public static int SetID<T>(T obj) {
        if (obj is Log) {
            lastLogID += 1;
            return lastLogID;
        } else if (obj is BaseLandmark) {
            lastLandmarkID += 1;
            return lastLandmarkID;
        } else if (obj is Faction) {
            lastFactionID += 1;
            return lastFactionID;
        } else if (obj is Character) {
            lastCharacterID += 1;
            return lastCharacterID;
        } else if (obj is SpecialToken) {
            lastItemID += 1;
            return lastItemID;
        } else if (obj is Area) {
            lastAreaID += 1;
            return lastAreaID;
        } else if (obj is Party) {
            lastPartyID += 1;
            return lastPartyID;
        } else if (obj is CharacterSim) {
            lastCharacterSimID += 1;
            return lastCharacterSimID;
        } 
        //else if (obj is Interaction) {
        //    lastInteractionID += 1;
        //    return lastInteractionID;
        //} 
        else if (obj is TileObject) {
            lastTileObjectID += 1;
            return lastTileObjectID;
        } else if (obj is LocationStructure) {
            lastStructureID += 1;
            return lastStructureID;
        } else if (obj is Region) {
            lastRegionID += 1;
            return lastRegionID;
        } else if (obj is JobQueueItem) {
            lastJobID += 1;
            return lastJobID;
        } else if (obj is BurningSource) {
            lastBurningSourceID += 1;
            return lastBurningSourceID;
        } else if (obj is SpecialObject) {
            lastSpecialObjectID += 1;
            return lastSpecialObjectID;
        }
        return 0;
    }

    //A checker is added before setting the last id of any object so that we can be sure that the last id value is always the highest number
    //This is mostly needed when loading data from save file because we don't know the order it will be loaded so we must be sure that last id is the highest number not the id of the last object to load
    public static int SetID<T>(T obj, int idToUse) {
        if (obj is Log) {
            if (lastLogID <= idToUse) { lastLogID = idToUse; }
        } else if (obj is BaseLandmark) {
            if (lastLandmarkID <= idToUse) { lastLandmarkID = idToUse; }
        } else if (obj is Faction) {
            if (lastFactionID <= idToUse) { lastFactionID = idToUse; }
        } else if (obj is Character) {
            if (lastCharacterID <= idToUse) { lastCharacterID = idToUse; }
        } else if (obj is SpecialToken) {
            if (lastItemID <= idToUse) { lastItemID = idToUse; }
        } else if (obj is Area) {
            if (lastAreaID <= idToUse) { lastAreaID = idToUse; }
        } else if (obj is Party) {
            if (lastPartyID <= idToUse) { lastPartyID = idToUse; }
        } else if (obj is CharacterSim) {
            if (lastCharacterSimID <= idToUse) { lastCharacterSimID = idToUse; }
        } else if (obj is LocationStructure) {
            if (lastStructureID <= idToUse) { lastStructureID = idToUse; }
        } else if (obj is TileObject) {
            if (lastTileObjectID <= idToUse) { lastTileObjectID = idToUse; }
        } else if (obj is Region) {
            if (lastRegionID <= idToUse) { lastRegionID = idToUse; }
        } else if (obj is JobQueueItem) {
            if (lastJobID <= idToUse) { lastJobID = idToUse; }
        } else if (obj is BurningSource) {
            if (lastBurningSourceID <= idToUse) { lastBurningSourceID = idToUse; }
        } else if (obj is SpecialObject) {
            if (lastSpecialObjectID <= idToUse) { lastSpecialObjectID = idToUse; }
        }
        //else if (obj is Interaction) {
        //    lastInteractionID = idToUse;
        //}
        return idToUse;
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

    #region Color Utilities
    public static Color darkGreen = new Color(0f / 255f, 100f / 255f, 0f / 255f);
    public static Color lightGreen = new Color(124f / 255f, 252f / 255f, 0f / 255f);
    public static Color darkRed = new Color(139f / 255f, 0f / 255f, 0f / 255f);
    public static Color lightRed = new Color(255f / 255f, 0f / 255f, 0f / 255f);
    public static Color[] factionColorCycle = new Color[] {
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
        //{BIOMES.WOODLAND, new Color(34f/255f, 139f/255f, 34f/255f)}
    };
    public static Color GetColorForFaction() {
        Color chosenColor = factionColorCycle[lastFactionColorIndex];
        lastFactionColorIndex += 1;
        if (lastFactionColorIndex >= factionColorCycle.Length) {
            lastFactionColorIndex = 0;
        }
        return chosenColor;
    }
    #endregion

    #region Log Utilities
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
    public static string LogReplacer(Log log) {
        if (log == null) {
            return string.Empty;
        }
        if(log.logText != string.Empty) {
            return log.logText;
        }
        List<int> specificWordIndexes = new List<int>();
        string newText;
        if (string.IsNullOrEmpty(log.message)) {
            newText = LocalizationManager.Instance.GetLocalizedValue(log.category, log.file, log.key);
        } else {
            newText = log.message;
        }
        
        //bool hasPeriod = newText.EndsWith(".");

        if (!string.IsNullOrEmpty(newText)) {
            string[] words = SplitAndKeepDelimiters(newText, new char[] { ' ', '.', ',', '\'', '!', '"', ':' });
            for (int i = 0; i < words.Length; i++) {
                var replacedWord = string.Empty;
                if (words[i].StartsWith("%") && (words[i].EndsWith("%") || words[i].EndsWith("@"))) { //OBJECT
                    replacedWord = CustomStringReplacer(words[i], log.fillers);
                } else if (words[i].StartsWith("%") && (words[i].EndsWith("a") || words[i].EndsWith("b"))) { //PRONOUN
                    replacedWord = CustomPronounReplacer(words[i], log.fillers);
                }
                if (!string.IsNullOrEmpty(replacedWord)) {
                    words[i] = replacedWord;
                }
            }
            newText = string.Empty;
            for (int i = 0; i < words.Length; i++) {
                newText += words[i];
            }
            newText = newText.Trim(' ');
        }
        log.SetLogText(newText);
        return newText;
    }
    public static string StringReplacer(string text, List<LogFiller> fillers) {
        if (string.IsNullOrEmpty(text)) {
            return string.Empty;
        }
        string replacedWord = string.Empty;
        List<int> specificWordIndexes = new List<int>();
        string newText = text;
        //bool hasPeriod = newText.EndsWith(".");

        if (!string.IsNullOrEmpty(newText)) {
            string[] words = SplitAndKeepDelimiters(newText, new char[] { ' ', '.', ',', '\'', '!', '"', ':' });
            for (int i = 0; i < words.Length; i++) {
                replacedWord = string.Empty;
                if (words[i].StartsWith("%") && (words[i].EndsWith("%") || words[i].EndsWith("@"))) { //OBJECT
                    replacedWord = CustomStringReplacer(words[i], fillers);
                } else if (words[i].StartsWith("%") && (words[i].EndsWith("a") || words[i].EndsWith("b"))) { //PRONOUN
                    replacedWord = CustomPronounReplacer(words[i], fillers);
                }
                if (!string.IsNullOrEmpty(replacedWord)) {
                    words[i] = replacedWord;
                }
            }
            newText = string.Empty;
            for (int i = 0; i < words.Length; i++) {
                newText += words[i];
            }
            newText = newText.Trim(' ');
        }
        return newText;
    }
    public static string LogDontReplace(Log log) {
        if (log == null) {
            return string.Empty;
        }
        string newText = LocalizationManager.Instance.GetLocalizedValue(log.category, log.file, log.key);
        return newText;
    }
    public static string CustomPronounReplacer(string wordToBeReplaced, List<LogFiller> objectLog) {
        LOG_IDENTIFIER identifier = Utilities.logIdentifiers[wordToBeReplaced.Substring(1, wordToBeReplaced.Length - 2)];
        string wordToReplace = string.Empty;
        //		string value = wordToBeReplaced.Substring(1, 2);
        string strIdentifier = identifier.ToString();
        string pronouns = GetPronoun(strIdentifier.Last(), wordToBeReplaced.Last());

        LOG_IDENTIFIER logIdentifier = LOG_IDENTIFIER.ACTIVE_CHARACTER;
        if (strIdentifier.Contains("FACTION_LEADER_1")) {
            logIdentifier = LOG_IDENTIFIER.FACTION_LEADER_1;
        } else if (strIdentifier.Contains("FACTION_LEADER_2")) {
            logIdentifier = LOG_IDENTIFIER.FACTION_LEADER_2;
        } else if (strIdentifier.Contains("TARGET_CHARACTER")) {
            logIdentifier = LOG_IDENTIFIER.TARGET_CHARACTER;
        } else if (strIdentifier.Contains("FACTION_LEADER_3")) {
            logIdentifier = LOG_IDENTIFIER.FACTION_LEADER_3;
        } else if (strIdentifier.Contains("MINION_1")) {
            logIdentifier = LOG_IDENTIFIER.MINION_1;
        } else if (strIdentifier.Contains("MINION_2")) {
            logIdentifier = LOG_IDENTIFIER.MINION_2;
        }
        for (int i = 0; i < objectLog.Count; i++) {
            if (objectLog[i].identifier == logIdentifier) {
                wordToReplace = Utilities.PronounReplacer(pronouns, objectLog[i].obj);
                break;
            }
        }

        return wordToReplace;

    }
    public static string CustomStringReplacer(string wordToBeReplaced, List<LogFiller> objectLog) {
        string wordToReplace = string.Empty;
        string strLogIdentifier = wordToBeReplaced.Substring(1, wordToBeReplaced.Length - 2);
        //strLogIdentifier = strLogIdentifier.Remove((strLogIdentifier.Length - 1), 1);
        LOG_IDENTIFIER identifier = Utilities.logIdentifiers[strLogIdentifier];
        if (identifier.ToString().Contains("LIST")) {
            int listCount = 0;
            for (int i = 0; i < objectLog.Count; i++) {
                if (objectLog[i].identifier == identifier) {
                    if (wordToReplace != string.Empty) {
                        wordToReplace += ", ";
                    }
                    if(objectLog[i].obj != null) {
                        wordToReplace += "<b><link=" + '"' + i.ToString() + '"' + ">" + objectLog[i].value + "</link></b>";
                    } else {
                        wordToReplace += "<b>" + objectLog[i].value + "</b>";
                    }
                    listCount++;
                }
            }
            if(listCount > 1) {
                //Add 'and' after last comma
                int commaLastIndex = wordToReplace.LastIndexOf(',');
                wordToReplace = wordToReplace.Insert(commaLastIndex + 1, " and");
            }
        } else if (identifier == LOG_IDENTIFIER.APPEND) {
            for (int i = 0; i < objectLog.Count; i++) {
                if (objectLog[i].identifier == identifier) {
                    wordToReplace = StringReplacer(objectLog[i].value, objectLog);
                    break;
                }
            }
        } else {
            for (int i = 0; i < objectLog.Count; i++) {
                if (objectLog[i].identifier == identifier) {
                    if (objectLog[i].obj != null) {
                        wordToReplace = "<b><link=" + '"' + i.ToString() + '"' + ">" + objectLog[i].value + "</link></b>";
                    } else {
                        wordToReplace = "<b>" + objectLog[i].value + "</b>";
                    }
                    break;
                }
            }
        }
        //if (wordToBeReplaced.EndsWith("@")) {

        //}
        //else if (wordToBeReplaced.EndsWith("%")) {
        //    if (identifier.ToString().Contains("LIST")) {
        //        int listCount = 0;
        //        for (int i = 0; i < objectLog.Count; i++) {
        //            if (objectLog[i].identifier == identifier) {
        //                if (wordToReplace != string.Empty) {
        //                    wordToReplace += ", ";
        //                }
        //                wordToReplace += "<b>" + objectLog[i].value + "</b>";
        //                listCount++;
        //            }
        //        }
        //        if (listCount > 1) {
        //            //Add 'and' after last comma
        //            int commaLastIndex = wordToReplace.LastIndexOf(',');
        //            wordToReplace = wordToReplace.Insert(commaLastIndex + 1, " and");
        //        }
        //    } else {
        //        for (int i = 0; i < objectLog.Count; i++) {
        //            if (objectLog[i].identifier == identifier) {
        //                wordToReplace = "<b>" + objectLog[i].value + "</b>";
        //                break;
        //            }
        //        }
        //    }
        //}

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
    public static Dictionary<string, LOG_IDENTIFIER> logIdentifiers = new Dictionary<string, LOG_IDENTIFIER>() {
        {"00", LOG_IDENTIFIER.ACTIVE_CHARACTER},
        {"01", LOG_IDENTIFIER.FACTION_1},
        {"02", LOG_IDENTIFIER.FACTION_LEADER_1},
        {"04", LOG_IDENTIFIER.LANDMARK_1},
        {"05", LOG_IDENTIFIER.PARTY_1},
        {"06", LOG_IDENTIFIER.STRUCTURE_1},
        {"07", LOG_IDENTIFIER.STRUCTURE_2},
        {"08", LOG_IDENTIFIER.STRUCTURE_3},
		//{"06", LOG_IDENTIFIER.RANDOM_CITY_1},
		//{"07", LOG_IDENTIFIER.RANDOM_GOVERNOR_1},
		{"10", LOG_IDENTIFIER.TARGET_CHARACTER},
        {"11", LOG_IDENTIFIER.FACTION_2},
        {"12", LOG_IDENTIFIER.FACTION_LEADER_2},
		//{"13", LOG_IDENTIFIER.KING_2_SPOUSE},
		{"14", LOG_IDENTIFIER.LANDMARK_2},
        {"15", LOG_IDENTIFIER.PARTY_2},
		//{"16", LOG_IDENTIFIER.RANDOM_CITY_2},
		//{"17", LOG_IDENTIFIER.RANDOM_GOVERNOR_2},
		{"20", LOG_IDENTIFIER.CHARACTER_3},
        {"21", LOG_IDENTIFIER.FACTION_3},
        {"22", LOG_IDENTIFIER.FACTION_LEADER_3},
		//{"23", LOG_IDENTIFIER.KING_3_SPOUSE},
		{"24", LOG_IDENTIFIER.LANDMARK_3},
        {"25", LOG_IDENTIFIER.PARTY_3},
		//{"26", LOG_IDENTIFIER.RANDOM_CITY_3},
		//{"27", LOG_IDENTIFIER.RANDOM_GOVERNOR_3},
		{"81", LOG_IDENTIFIER.ACTION_DESCRIPTION},
        {"82", LOG_IDENTIFIER.QUEST_NAME},
        {"83", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_S},
        {"84", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_O},
        {"85", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_P},
        {"86", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_R},
        {"87", LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_S},
        {"88", LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_O},
        {"89", LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_P},
        {"90", LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_R},
        {"91", LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_S},
        {"92", LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_O},
        {"93", LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_P},
        {"94", LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_R},
        {"95", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_S},
        {"96", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_O},
        {"97", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_P},
        {"98", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_R},
		//{"99", LOG_IDENTIFIER.SECESSION_CITIES},
		{"100", LOG_IDENTIFIER.TASK},
        {"101", LOG_IDENTIFIER.DATE},
        {"102", LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_S},
        {"103", LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_O},
        {"104", LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_P},
        {"105", LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_R},
        {"106", LOG_IDENTIFIER.OTHER},
        {"107", LOG_IDENTIFIER.ITEM_1},
        {"108", LOG_IDENTIFIER.ITEM_2},
        {"109", LOG_IDENTIFIER.ITEM_3},
        {"110", LOG_IDENTIFIER.COMBAT},
        {"111", LOG_IDENTIFIER.STRING_1},
        {"112", LOG_IDENTIFIER.STRING_2},
        {"113", LOG_IDENTIFIER.MINION_1},
        {"114", LOG_IDENTIFIER.MINION_1_PRONOUN_S},
        {"115", LOG_IDENTIFIER.MINION_1_PRONOUN_O},
        {"116", LOG_IDENTIFIER.MINION_1_PRONOUN_P},
        {"117", LOG_IDENTIFIER.MINION_1_PRONOUN_R},
        {"118", LOG_IDENTIFIER.MINION_2},
        {"119", LOG_IDENTIFIER.MINION_2_PRONOUN_S},
        {"120", LOG_IDENTIFIER.MINION_2_PRONOUN_O},
        {"121", LOG_IDENTIFIER.MINION_2_PRONOUN_P},
        {"122", LOG_IDENTIFIER.MINION_2_PRONOUN_R},
        {"123", LOG_IDENTIFIER.CHARACTER_LIST_1},
        {"124", LOG_IDENTIFIER.CHARACTER_LIST_2},
        {"125", LOG_IDENTIFIER.APPEND},
        {"126", LOG_IDENTIFIER.OTHER_2},
		//{"111", LOG_IDENTIFIER.PARTY_NAME},
	};
    public static string GetStringForIdentifier(LOG_IDENTIFIER identifier) {
        foreach (KeyValuePair<string, LOG_IDENTIFIER> item in logIdentifiers) {
            if (item.Value == identifier) {
                string key = item.Key;
                key = "%" + key;
                if (identifier.ToString().Contains("PRONOUN")) {
                    key += "b";
                    return key;
                }
                switch (identifier) {
                    case LOG_IDENTIFIER.ACTIVE_CHARACTER:
                    case LOG_IDENTIFIER.FACTION_1:
                    case LOG_IDENTIFIER.FACTION_LEADER_1:
                    case LOG_IDENTIFIER.LANDMARK_1:
                    case LOG_IDENTIFIER.PARTY_1:
                    case LOG_IDENTIFIER.TARGET_CHARACTER:
                    case LOG_IDENTIFIER.FACTION_2:
                    case LOG_IDENTIFIER.FACTION_LEADER_2:
                    case LOG_IDENTIFIER.LANDMARK_2:
                    case LOG_IDENTIFIER.PARTY_2:
                    case LOG_IDENTIFIER.CHARACTER_3:
                    case LOG_IDENTIFIER.FACTION_3:
                    case LOG_IDENTIFIER.FACTION_LEADER_3:
                    case LOG_IDENTIFIER.LANDMARK_3:
                    case LOG_IDENTIFIER.PARTY_3:
                    case LOG_IDENTIFIER.TASK:
                    case LOG_IDENTIFIER.COMBAT:
                        key += "@";
                        break;
                    default:
                        key += "%";
                        break;
                }
                return key;
            }
        }
        return string.Empty;
    }
    public static string PronounReplacer(string word, object genderSubject) {
        //		string pronoun = Utilities.GetStringBetweenTwoChars (word, '_', '_');
        string[] pronouns = word.Split('/');
        GENDER gender = GENDER.MALE;
        if (genderSubject is Character) {
            gender = (genderSubject as Character).gender;
        }else if (genderSubject is Minion) {
            gender = (genderSubject as Minion).character.gender;
        }
        if (gender == GENDER.MALE) {
            if (pronouns.Length > 0) {
                if (!string.IsNullOrEmpty(pronouns[0])) {
                    return pronouns[0];
                }
            }
        } else {
            if (pronouns.Length > 1) {
                if (!string.IsNullOrEmpty(pronouns[0])) {
                    return pronouns[1];
                }
            }
        }
        return string.Empty;
    }
    public static string GetPronoun(string type, string caseIdentifier) {
        if (type == "S") {
            if (caseIdentifier == "a") {
                return "He/She";
            }
            return "he/she";
        } else if (type == "O") {
            if (caseIdentifier == "a") {
                return "Him/Her";
            }
            return "him/her";
        } else if (type == "P") {
            if (caseIdentifier == "a") {
                return "His/Her";
            }
            return "his/her";
        } else if (type == "R") {
            if (caseIdentifier == "a") {
                return "Himself/Herself";
            }
            return "himself/herself";
        }
        return string.Empty;
    }
    public static string GetPronoun(char type, char caseIdentifier) {
        if (type == 'S') {
            if (caseIdentifier == 'a') {
                return "He/She";
            }
            return "he/she";
        } else if (type == 'O') {
            if (caseIdentifier == 'a') {
                return "Him/Her";
            }
            return "him/her";
        } else if (type == 'P') {
            if (caseIdentifier == 'a') {
                return "His/Her";
            }
            return "his/her";
        } else if (type == 'R') {
            if (caseIdentifier == 'a') {
                return "Himself/Herself";
            }
            return "himself/herself";
        }
        return string.Empty;
    }
    public static string GetPronounString(GENDER gender, PRONOUN_TYPE type, bool isUppercaseFirstLetter) {
        string pronoun = string.Empty;
        if(gender == GENDER.MALE) {
            if (type == PRONOUN_TYPE.SUBJECTIVE) {
                pronoun = "he";
            } else if (type == PRONOUN_TYPE.OBJECTIVE) {
                pronoun = "him";
            } else if (type == PRONOUN_TYPE.POSSESSIVE) {
                pronoun = "his";
            } else if (type == PRONOUN_TYPE.REFLEXIVE) {
                pronoun = "himself";
            }
        } else {
            if (type == PRONOUN_TYPE.SUBJECTIVE) {
                pronoun = "she";
            } else if (type == PRONOUN_TYPE.OBJECTIVE) {
                pronoun = "her";
            } else if (type == PRONOUN_TYPE.POSSESSIVE) {
                pronoun = "her";
            } else if (type == PRONOUN_TYPE.REFLEXIVE) {
                pronoun = "herself";
            }
        }
        if (isUppercaseFirstLetter) {
            pronoun = FirstLetterToUpperCase(pronoun);
        }
        return pronoun;
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
    public static string GetStringBetweenTwoChars(string word, char first, char last) {
        int indexFirst = word.IndexOf(first);
        int indexLast = word.LastIndexOf(last);

        if (indexFirst == -1 || indexLast == -1) {
            return string.Empty;
        }
        indexFirst += 1;
        if (indexFirst >= word.Length) {
            return string.Empty;
        }

        return word.Substring(indexFirst, (indexLast - indexFirst));
    }
    public static List<string> GetAllWordsInAString(string wordToFind, string text) {
        List<string> words = new List<string>();
        string word = string.Empty;
        int index = 0;
        int wordCount = 0;
        int startingIndex = index;
        while (index != -1) {
            index = text.IndexOf(wordToFind, startingIndex);
            if (index != -1) {
                startingIndex = index + 1;
                if (startingIndex > text.Length - 1) {
                    startingIndex = text.Length - 1;
                }

                wordCount = 0;
                for (int i = index; i < text.Length; i++) {
                    if (text[i] != ' ') {
                        wordCount += 1;
                    } else {
                        break;
                    }
                }
                word = text.Substring(index, wordCount);
                words.Add(word);
            }

        }
        return words;
    }
    public static string[] SplitAndKeepDelimiters(string s, params char[] delimiters) {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(s)) {
            int iFirst = 0;
            do {
                int iLast = s.IndexOfAny(delimiters, iFirst);
                if (iLast >= 0) {
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
    #endregion

    #region Sprite Utilities
    public static void SetSpriteSortingLayer(SpriteRenderer sprite, string layerName) {
        sprite.sortingLayerName = layerName;
    }
    public static void SetLayerRecursively(GameObject go, int layerNumber) {
        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true)) {
            trans.gameObject.layer = layerNumber;
        }
    }
    public static List<BIOMES> biomeLayering = new List<BIOMES>() {
        BIOMES.GRASSLAND,
        //BIOMES.WOODLAND,
        BIOMES.TUNDRA,
        BIOMES.FOREST,
        BIOMES.DESERT,
        BIOMES.SNOW
    };
    #endregion

    #region String Utilities
    public static string NormalizeNoSpaceString(string s) {
        string[] words = System.Text.RegularExpressions.Regex.Split(s, @"(?<!^)(?=[A-Z])");
        string normalizedString = Utilities.FirstLetterToUpperCase(words.First());
        for (int i = 1; i < words.Length; i++) {
            normalizedString += " " + words[i];
        }
        return normalizedString;
    }
    public static string NormalizeString(string s) {
        s = s.ToLower();
        string[] words = s.Split('_');
        string normalizedString = Utilities.FirstLetterToUpperCase(words[0]);
        for (int i = 1; i < words.Length; i++) {
            normalizedString += " " + words[i];
        }
        return normalizedString;
    }
    public static string NormalizeStringUpperCaseFirstLetters(string s) {
        s = s.ToLower();
        string[] words = s.Split('_');
        string normalizedString = Utilities.FirstLetterToUpperCase(words[0]);
        for (int i = 1; i < words.Length; i++) {
            normalizedString += " " + Utilities.FirstLetterToUpperCase(words[i]);
        }
        return normalizedString;
    }
    public static string NormalizeStringUpperCaseFirstLettersNoSpace(string s) {
        s = s.ToLower();
        string[] words = s.Split('_');
        string normalizedString = Utilities.FirstLetterToUpperCase(words[0]);
        for (int i = 1; i < words.Length; i++) {
            normalizedString += Utilities.FirstLetterToUpperCase(words[i]);
        }
        return normalizedString;
    }
    public static string NotNormalizedConversionEnumToString(string s) {
        return s.Replace('_', ' ');
    }
    public static string NotNormalizedConversionStringToEnum(string s) {
        return s.Replace(' ', '_');
    }
    public static string FirstLetterToUpperCase(string s) {
        if (string.IsNullOrEmpty(s))
            throw new ArgumentException("There is no first letter");

        char[] a = s.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new string(a);
    }
    public static List<string> GetEnumChoices<T>(bool includeNone = false, List<T> exclude = null) {
        List<string> options = new List<string>();
        T[] values = (T[]) Enum.GetValues(typeof(T));
        for (int i = 0; i < values.Length; i++) {
            T currOption = values[i];
            if (!includeNone) { //do not include none
                if (currOption.ToString().Equals("NONE")) {
                    continue;
                }
            }
            if (exclude != null && exclude.Contains(currOption)) {
                continue; //skip excluded items
            }
            options.Add(values[i].ToString());
        }
        return options;
    }
    public static string GetPossessivePronounForCharacter(Character character, bool capitalized = true) {
        if (character.gender == GENDER.MALE) {
            if (capitalized) {
                return "His";
            } else {
                return "his";
            }
        } else {
            if (capitalized) {
                return "Her";
            } else {
                return "her";
            }
        }
    }
    public static List<string> ExtractFromString(string text, string startString, string endString) {
        List<string> matched = new List<string>();
        int indexStart = 0, indexEnd = 0;
        bool exit = false;
        while (!exit) {
            indexStart = text.IndexOf(startString);
            indexEnd = text.IndexOf(endString);
            if (indexStart != -1 && indexEnd != -1) {
                matched.Add(text.Substring(indexStart + startString.Length,
                    indexEnd - indexStart - startString.Length));
                text = text.Substring(indexEnd + endString.Length);
            } else
                exit = true;
        }
        return matched;
    }
    public static string GetArticleForWord(string word, bool capitalized = false) {
        char firstCharacter = word.ToLower().First();
        if (firstCharacter == 'a' || firstCharacter == 'e' || firstCharacter == 'i' || firstCharacter == 'o' || firstCharacter == 'u') {
            if (capitalized) {
                return "An";
            }
            return "an";
        } else {
            if (capitalized) {
                return "A";
            }
            return "a";
        }
    }
    public static string GetRelationshipPlural(RELATIONSHIP_TYPE rel) {
        switch (rel) {
            case RELATIONSHIP_TYPE.RELATIVE:
                return "Relatives";
            case RELATIONSHIP_TYPE.LOVER:
                return "Lovers";
            case RELATIONSHIP_TYPE.PARAMOUR:
                return "Paramours";
            default:
                return NormalizeStringUpperCaseFirstLetters(rel.ToString());
        }
    }
    public static string RemoveAllWhiteSpace(string str) {
        var len = str.Length;
        var src = str.ToCharArray();
        int dstIdx = 0;

        for (int i = 0; i < len; i++) {
            var ch = src[i];
            switch (ch) {
                case '\u0020':
                case '\u00A0':
                case '\u1680':
                case '\u2000':
                case '\u2001':
                case '\u2002':
                case '\u2003':
                case '\u2004':
                case '\u2005':
                case '\u2006':
                case '\u2007':
                case '\u2008':
                case '\u2009':
                case '\u200A':
                case '\u202F':
                case '\u205F':
                case '\u3000':
                case '\u2028':
                case '\u2029':
                case '\u0009':
                case '\u000A':
                case '\u000B':
                case '\u000C':
                case '\u000D':
                case '\u0085':
                    continue;

                default:
                    src[dstIdx++] = ch;
                    break;
            }
        }
        return new string(src, 0, dstIdx);
    }
    public static string[] ConvertStringToArray(string str, char separator) {
        string[] arr = str.Split(',');
        return arr;
    }
    public static string ConvertArrayToString(string[] str, char separator) {
        string joinedStr = string.Empty;
        for (int i = 0; i < str.Length; i++) {
            if(i > 0) {
                joinedStr += separator;
            }
            joinedStr += str[i];
        }
        return joinedStr;
    }
    #endregion

    #region Weighted Dictionary
    public static Dictionary<T, int> MergeWeightedActionDictionaries<T>(Dictionary<T, int> dict1, Dictionary<T, int> dict2) {
        Dictionary<T, int> mergedDict = new Dictionary<T, int>();
        foreach (KeyValuePair<T, int> kvp in dict1) {
            T currKey = kvp.Key;
            int currValue = kvp.Value;
            if (dict2.ContainsKey(currKey)) {
                currValue += dict2[currKey];
            }
            mergedDict.Add(currKey, currValue);
        }
        foreach (KeyValuePair<T, int> kvp in dict2) {
            T currKey = kvp.Key;
            int currValue = kvp.Value;
            if (dict1.ContainsKey(currKey)) {
                currValue += dict1[currKey];
            }
            if (!mergedDict.ContainsKey(currKey)) {
                mergedDict.Add(currKey, currValue);
            }

        }
        return mergedDict;
    }
    public static Dictionary<T, Dictionary<T, int>> MergeWeightedActionDictionaries<T>(Dictionary<T, Dictionary<T, int>> dict1, Dictionary<T, Dictionary<T, int>> dict2) {
        Dictionary<T, Dictionary<T, int>> mergedDict = new Dictionary<T, Dictionary<T, int>>();
        foreach (KeyValuePair<T, Dictionary<T, int>> kvp in dict1) {
            T currKey = kvp.Key;
            Dictionary<T, int> currValue = kvp.Value;
            if (dict2.ContainsKey(currKey)) {
                currValue = MergeWeightedActionDictionaries<T>(currValue, dict2[currKey]);
            }
            mergedDict.Add(currKey, currValue);
        }
        foreach (KeyValuePair<T, Dictionary<T, int>> kvp in dict2) {
            T currKey = kvp.Key;
            Dictionary<T, int> currValue = kvp.Value;
            if (!mergedDict.ContainsKey(currKey)) {
                if (dict1.ContainsKey(currKey)) {
                    currValue = MergeWeightedActionDictionaries<T>(currValue, dict1[currKey]);
                }
                mergedDict.Add(currKey, currValue);
            }
        }
        return mergedDict;
    }
    public static T PickRandomElementWithWeights<T>(Dictionary<T, int> weights) {
        int totalOfAllWeights = GetTotalOfWeights(weights);
        int chance = rng.Next(0, totalOfAllWeights);
        int upperBound = 0;
        int lowerBound = 0;
        foreach (KeyValuePair<T, int> kvp in weights) {
            T currElementType = kvp.Key;
            int weightOfCurrElement = kvp.Value;
            if (weightOfCurrElement <= 0) {
                continue;
            }
            upperBound += weightOfCurrElement;
            if (chance >= lowerBound && chance < upperBound) {
                return currElementType;
            }
            lowerBound = upperBound;
        }
        throw new Exception("Could not pick element in weights");
    }
    public static T PickRandomElementWithWeights<T>(Dictionary<T, float> weights) {
        float totalOfAllWeights = GetTotalOfWeights(weights);
        int chance = rng.Next(0, (int) totalOfAllWeights);
        float upperBound = 0;
        float lowerBound = 0;
        foreach (KeyValuePair<T, float> kvp in weights) {
            T currElementType = kvp.Key;
            float weightOfCurrElement = kvp.Value;
            if (weightOfCurrElement <= 0) {
                continue;
            }
            upperBound += weightOfCurrElement;
            if (chance >= lowerBound && chance < upperBound) {
                return currElementType;
            }
            lowerBound = upperBound;
        }
        throw new Exception("Could not pick element in weights");
    }
    /*
     * This will return an array that has 2 elements 
     * of the same type. The 1st element is the first key in the dictionary, and the
     * 2nd element is the second key.
     * */
    public static T[] PickRandomElementWithWeights<T>(Dictionary<T, Dictionary<T, int>> weights) {
        int totalOfAllWeights = GetTotalOfWeights(weights);
        int chance = UnityEngine.Random.Range(0, totalOfAllWeights);
        int upperBound = 0;
        int lowerBound = 0;
        foreach (KeyValuePair<T, Dictionary<T, int>> kvp in weights) {
            T currElementType = kvp.Key;
            foreach (KeyValuePair<T, int> pair in kvp.Value) {
                T otherElement = pair.Key;
                int weightOfOtherElement = pair.Value;
                upperBound += weightOfOtherElement;
                if (chance >= lowerBound && chance < upperBound) {
                    return new T[] { currElementType, otherElement };
                }
                lowerBound = upperBound;
            }
        }
        throw new Exception("Could not pick element in weights");
    }
    public static int GetTotalOfWeights<T>(Dictionary<T, Dictionary<T, int>> weights) {
        int totalOfAllWeights = 0;
        foreach (KeyValuePair<T, Dictionary<T, int>> kvp in weights) {
            foreach (KeyValuePair<T, int> pair in kvp.Value) {
                totalOfAllWeights += pair.Value;
            }
        }
        return totalOfAllWeights;
    }
    public static int GetTotalOfWeights<T>(Dictionary<T, int> weights) {
        return weights.Sum(x => x.Value);
    }
    public static float GetTotalOfWeights<T>(Dictionary<T, float> weights) {
        float totalOfAllWeights = 0f;
        foreach (float weight in weights.Values) {
            totalOfAllWeights += weight;
        }
        return totalOfAllWeights;
    }
    public static string GetWeightsSummary<T>(Dictionary<T, int> weights, string title = "Weights Summary: ") {
        string actionWeightsSummary = title;
        foreach (KeyValuePair<T, int> kvp in weights) {
            T key = kvp.Key;
            int value = kvp.Value;
            if (key is Character) {
                actionWeightsSummary += "\n" + (key as Character).name + " - " + kvp.Value.ToString();
            } else if (key is BaseLandmark) {
                actionWeightsSummary += "\n" + (key as BaseLandmark).landmarkName + " - " + kvp.Value.ToString();
            } else {
                actionWeightsSummary += "\n" + kvp.Key.ToString() + " - " + kvp.Value.ToString();
            }

        }
        return actionWeightsSummary;
    }
    public static string GetWeightsSummary<T>(Dictionary<T, float> weights, string title = "Weights Summary: ") {
        string actionWeightsSummary = title;
        foreach (KeyValuePair<T, float> kvp in weights) {
            T key = kvp.Key;
            float value = kvp.Value;
            if (key is Character) {
                actionWeightsSummary += "\n" + (key as Character).name + " - " + kvp.Value.ToString();
            } else if (key is BaseLandmark) {
                actionWeightsSummary += "\n" + (key as BaseLandmark).landmarkName + " - " + kvp.Value.ToString();
            } else {
                actionWeightsSummary += "\n" + kvp.Key.ToString() + " - " + kvp.Value.ToString();
            }

        }
        return actionWeightsSummary;
    }
    public static string GetWeightsSummary<T>(Dictionary<T, Dictionary<T, int>> weights, string title = "Weights Summary: ") {
        string actionWeightsSummary = title;
        foreach (KeyValuePair<T, Dictionary<T, int>> kvp in weights) {
            actionWeightsSummary += "\n" + kvp.Key.ToString() + " : ";
            foreach (KeyValuePair<T, int> pair in kvp.Value) {
                actionWeightsSummary += "\n     " + pair.Key.ToString() + " - " + pair.Value.ToString();
            }
        }
        return actionWeightsSummary;
    }
    //public static string GetWeightsSummary(Dictionary<Kingdom, int> weights, string title = "Weights Summary: ") {
    //    string actionWeightsSummary = title;
    //    foreach (KeyValuePair<Kingdom, int> kvp in weights) {
    //        actionWeightsSummary += "\n" + kvp.Key.name + " - " + kvp.Value.ToString();
    //    }
    //    return actionWeightsSummary;
    //}

    //public static string GetWeightsSummary(Dictionary<Kingdom, Dictionary<Kingdom, int>> weights, string title = "Weights Summary: ") {
    //    string actionWeightsSummary = title;
    //    foreach (KeyValuePair<Kingdom, Dictionary<Kingdom, int>> kvp in weights) {
    //        actionWeightsSummary += "\n" + kvp.Key.name + " : ";
    //        foreach (KeyValuePair<Kingdom, int> pair in kvp.Value) {
    //            actionWeightsSummary += "\n     " + pair.Key.name + " - " + pair.Value.ToString();
    //        }
    //    }
    //    return actionWeightsSummary;
    //}
    #endregion

    #region Collection Utilities
    public static T GetNextElementCyclic<T>(List<T> collection, int index) {
        if (index > collection.Count) {
            throw new ArgumentOutOfRangeException("Trying to get next element cyclic, but provided index is greater than the size of the collection!");
        }
        if (index == collection.Count - 1) {
            //if index provided is equal to the number of elements in the list,
            //then the next element is the first element
            return collection[0]; 
        }
        return collection[index + 1];
    }
    public static bool ContainsRange<T>(List<T> sourceList, List<T> otherList) {
        //this is used to check whether a list has all the values in another list
        for (int i = 0; i < otherList.Count; i++) {
            if (!sourceList.Contains(otherList[i])) {
                return false;
            }
        }
        return true;
    }
    public static T[] GetEnumValues<T>() where T : struct {
        if (!typeof(T).IsEnum) {
            throw new ArgumentException("GetValues<T> can only be called for types derived from System.Enum", "T");
        }
        return (T[]) Enum.GetValues(typeof(T));
    }
    public static List<T> Shuffle<T>(List<T> list) {
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
    public static List<T> Intersect<T>(List<T> firstList, List<T> secondList) {
        List<T> newList = new List<T>();
        for (int i = 0; i < firstList.Count; i++) {
            for (int j = 0; j < secondList.Count; j++) {
                if (firstList[i].Equals(secondList[j])) {
                    newList.Add(firstList[i]);
                    break;
                }
            }
        }
        return newList;
    }
    public static List<T> Union<T>(List<T> firstList, List<T> secondList) {
        bool hasMatched = false;
        List<T> newList = new List<T>();
        for (int i = 0; i < firstList.Count; i++) {
            newList.Add(firstList[i]);
        }
        for (int i = 0; i < secondList.Count; i++) {
            hasMatched = false;
            for (int j = 0; j < firstList.Count; j++) {
                if (secondList[i].Equals(firstList[j])) {
                    hasMatched = true;
                    break;
                }
            }
            if (!hasMatched) {
                newList.Add(secondList[i]);
            }
        }
        return newList;
    }
    public static void ListRemoveRange<T>(List<T> sourceList, List<T> itemsToRemove) {
        for (int i = 0; i < itemsToRemove.Count; i++) {
            T currItem = itemsToRemove[i];
            sourceList.Remove(currItem);
        }
    }
    public static List<HexTile> MergeHexLists(List<HexTile> list1, List<HexTile> list2) {
        Dictionary<int, HexTile> dict = list2.ToDictionary(h => h.id, v => v);
        foreach (HexTile h in list1) {
            dict[h.id] = h;
        }
        return dict.Values.ToList();
    }
    public static void LogDictionary<T, V>(Dictionary<T, V> dict) {
        string log = string.Empty;
        foreach (KeyValuePair<T, V> kvp in dict) {
            log += kvp.Key.ToString() + " - " + kvp.Value.ToString();
        }
        Debug.Log("Dictionary: " + log);
    }
    public static T[] CreateCopyOfArray<T>(T[] sourceArray) {
        T[] copy = new T[sourceArray.Length];
        for (int i = 0; i < sourceArray.Length; i++) {
            copy[i] = sourceArray[i];
        }
        return copy;
    }
    public static List<T> RemoveElements<T>(List<T> sourceList, List<T> elementsToRemove) {
        List<T> newList = new List<T>();
        for (int i = 0; i < sourceList.Count; i++) {
            T currElement = sourceList[i];
            if (elementsToRemove.Contains(currElement)) {
                continue; //do not add that element to new list
            }
            newList.Add(currElement);
        }
        return newList;
    }
    public static List<T> RemoveElements<T>(List<T> sourceList, T[] elementsToRemove) {
        List<T> newList = new List<T>();
        for (int i = 0; i < sourceList.Count; i++) {
            T currElement = sourceList[i];
            if (elementsToRemove.Contains(currElement)) {
                continue; //do not add that element to new list
            }
            newList.Add(currElement);
        }
        return newList;
    }
    /// <summary>
    /// Get a random index from the given list.
    /// This will return -1 if the list has no elements.
    /// </summary>
    /// <param name="list">The sample list.</param>
    /// <returns>An integer.</returns>
    public static int GetRandomIndexInList<T>(List<T> list) {
        if (list.Count == 0) {
            return -1;
        }
        return rng.Next(0, list.Count);
    }
    public static T GetRandomElement<T>(List<T> list) {
        return list[rng.Next(0, list.Count)];
    }
    public static T GetRandomElement<T>(T[] list) {
        return list[rng.Next(0, list.Length)];
    }
    #endregion

    #region Game Utilities
    public static string GetDateString(GameDate date) {
        return date.month + "/" + date.day + "/" + date.year;
    }
    //public static int GetRangeInTicks(GameDate startDate, GameDate endDate) {
    //    int range = 0;
    //    GameDate lowerDate;
    //    GameDate higherDate;
    //    if (startDate.IsBefore(endDate)) {
    //        lowerDate = startDate;
    //        higherDate = endDate;
    //    } else {
    //        lowerDate = endDate;
    //        higherDate = startDate;
    //    }
    //    //GameDate startDate = this.startDate;
    //    //GameDate endDate = this.endDate;
    //    while (!lowerDate.IsSameDate(higherDate)) {
    //        lowerDate.AddHours(1);
    //        range++;
    //    }
    //    return range;
    //}
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
    public static string GetNormalizedSingularRace(RACE race) {
        switch (race) {
            case RACE.HUMANS:
            return "Human";
            case RACE.ELVES:
            return "Elf";
            case RACE.MINGONS:
            return "Mingon";
            case RACE.CROMADS:
            return "Cromad";
            case RACE.GOBLIN:
            return "Goblin";
            case RACE.TROLL:
            return "Troll";
            case RACE.DRAGON:
            return "Dragon";
            default:
            return Utilities.NormalizeString(race.ToString());
        }
    }
    public static string GetNormalizedRaceAdjective(RACE race) {
        switch (race) {
            case RACE.HUMANS:
            return "Human";
            case RACE.ELVES:
            return "Elven";
            case RACE.MINGONS:
            return "Mingon";
            case RACE.CROMADS:
            return "Cromad";
            case RACE.GOBLIN:
            return "Goblin";
            case RACE.TROLL:
            return "Troll";
            case RACE.DRAGON:
            return "Dragon";
            default:
            return Utilities.NormalizeString(race.ToString());
        }
    }
    public static HexTile GetCenterTile(List<HexTile> tiles, HexTile[,] map, int width, int height) {
        int maxXCoordinate = tiles.Max(x => x.xCoordinate);
        int minXCoordinate = tiles.Min(x => x.xCoordinate);
        int maxYCoordinate = tiles.Max(x => x.yCoordinate);
        int minYCoordinate = tiles.Min(x => x.yCoordinate);

        int midPointX = (minXCoordinate + maxXCoordinate) / 2;
        int midPointY = (minYCoordinate + maxYCoordinate) / 2;

        if (width - 2 >= midPointX) {
            midPointX -= 2;
        }
        if (height - 2 >= midPointY) {
            midPointY -= 2;
        }
        if (midPointX >= 2) {
            midPointX += 2;
        }
        if (midPointY >= 2) {
            midPointY += 2;
        }
        midPointX = Mathf.Clamp(midPointX, 0, width - 1);
        midPointY = Mathf.Clamp(midPointY, 0, height - 1);

        try {
            HexTile newCenterOfMass = map[midPointX, midPointY];
            return newCenterOfMass;
        } catch {
            throw new Exception("Cannot Recompute center. Computed new center is " + midPointX.ToString() + ", " + midPointY.ToString());
        }

    }
    static Texture2D _whiteTexture;
    public static Texture2D WhiteTexture {
        get {
            if (_whiteTexture == null) {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            return _whiteTexture;
        }
    }
    public static void DrawScreenRect(Rect rect, Color color) {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }
    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color) {
        // Top
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }
    public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2) {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }
    public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2) {
        var v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
        var v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
        var min = Vector3.Min(v1, v2);
        var max = Vector3.Max(v1, v2);
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }
    public static bool IsVisibleFrom(Renderer renderer, Camera camera) {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
    public static T[] GetComponentsInDirectChildren<T>(GameObject gameObject) {
        int indexer = 0;

        foreach (Transform transform in gameObject.transform) {
            if (transform.GetComponent<T>() != null) {
                indexer++;
            }
        }

        T[] returnArray = new T[indexer];

        indexer = 0;

        foreach (Transform transform in gameObject.transform) {
            if (transform.GetComponent<T>() != null) {
                returnArray[indexer++] = transform.GetComponent<T>();
            }
        }

        return returnArray;
    }

    public static int GetOptionIndex(Dropdown dropdown, string option) {
        for (int i = 0; i < dropdown.options.Count; i++) {
            if (dropdown.options[i].text.Equals(option)) {
                return i;
            }
        }
        return -1;
    }
    public static List<HexTile> GetTilesFromIDs(List<int> ids) {
        List<HexTile> tiles = new List<HexTile>();
        for (int i = 0; i < ids.Count; i++) {
            int currID = ids[i];
#if WORLD_CREATION_TOOL
            HexTile tile = worldcreator.WorldCreatorManager.Instance.GetHexTile(currID);
            tiles.Add(tile);
#else
            HexTile tile = GridMap.Instance.GetHexTile(currID);
            tiles.Add(tile);
#endif
        }
        return tiles;
    }
    //public static float GetPowerComparison(Party party1, Party party2) {
    //    if (party1.computedPower <= party2.computedPower) { //party1 power is higher than or equal party2 power
    //        //Percent increase = [(new value - original value)/original value] * 100
    //        return ((party2.computedPower - party1.computedPower) / party1.computedPower) * 100;
    //    } else { //party1 power is lower than party2 power
    //        //Percent decrease = [(original value - new value)/original value] * 100
    //        return ((party1.computedPower - party2.computedPower) / party1.computedPower) * 100;
    //    }
    //}

    public static List<RACE> beastRaces = new List<RACE>() {
        RACE.DRAGON,
        RACE.WOLF,
        //RACE.BEAST,
        RACE.SPIDER
    };
    public static List<RACE> nonBeastRaces = new List<RACE>() {
        RACE.HUMANS,
        RACE.ELVES,
        RACE.GOBLIN,
        RACE.FAERY,
        RACE.SKELETON,
    };
    public static bool IsRaceBeast(RACE race) {
        return beastRaces.Contains(race);
    }
    public static string GetRespectiveBeastClassNameFromByRace(RACE race) {
        if(race == RACE.GOLEM) {
            return "Abomination";
        } else if(race == RACE.DRAGON) {
            return "Dragon";
        } else if (race == RACE.SPIDER) {
            return "Spinner";
        } else if (race == RACE.WOLF) {
            return "Ravager";
        }
        throw new Exception("No beast class for " + race.ToString() + " Race!");
    }
    public static GameObject FindParentWithTag(GameObject childObject, string tag) {
        Transform t = childObject.transform;
        while (t.parent != null) {
            if (t.parent.tag == tag) {
                return t.parent.gameObject;
            }
            t = t.parent.transform;
        }
        return null; // Could not find a parent with given tag.
    }
    public static int GetTicksInBetweenDates(GameDate date1, GameDate date2) {
        int yearDiff = Mathf.Abs(date1.year - date2.year);
        int monthDiff = Mathf.Abs(date1.month - date2.month);
        int daysDiff = Mathf.Abs(date1.day - date2.day);
        int ticksDiff = date2.tick - date1.tick;

        int totalTickDiff = yearDiff * ((GameManager.ticksPerDay * GameManager.daysPerMonth) * 12);
        totalTickDiff += monthDiff * (GameManager.ticksPerDay * GameManager.daysPerMonth);
        totalTickDiff += daysDiff * GameManager.ticksPerDay;
        totalTickDiff += ticksDiff;
        
        return totalTickDiff;
    }
    #endregion

    #region Landmarks
    // public static BASE_LANDMARK_TYPE GetBaseLandmarkType(LANDMARK_TYPE landmarkType) {
    //     switch (landmarkType) {
    //         case LANDMARK_TYPE.CLAY:
    //         case LANDMARK_TYPE.LIMESTONE:
    //         case LANDMARK_TYPE.GRANITE:
    //         case LANDMARK_TYPE.MARBLE:
    //         case LANDMARK_TYPE.SILK:
    //         case LANDMARK_TYPE.COTTON:
    //         case LANDMARK_TYPE.FLAX:
    //         case LANDMARK_TYPE.CORN:
    //         case LANDMARK_TYPE.RICE:
    //         case LANDMARK_TYPE.PIGMEAT:
    //         case LANDMARK_TYPE.COWMEAT:
    //         case LANDMARK_TYPE.GOATHIDE:
    //         case LANDMARK_TYPE.DEERHIDE:
    //         case LANDMARK_TYPE.BEHEMOTHHIDE:
    //         case LANDMARK_TYPE.OAK:
    //         case LANDMARK_TYPE.YEW:
    //         case LANDMARK_TYPE.EBONY:
    //         case LANDMARK_TYPE.IRON:
    //         case LANDMARK_TYPE.COBALT:
    //         case LANDMARK_TYPE.MITHRIL:
    //             return BASE_LANDMARK_TYPE.RESOURCE;
    //         case LANDMARK_TYPE.ANCIENT_RUIN:
    //         case LANDMARK_TYPE.VAMPIRE_TOMB:
    //         case LANDMARK_TYPE.ANCIENT_REACTOR:
    //         case LANDMARK_TYPE.CAVE:
    //         case LANDMARK_TYPE.WILDLANDS:
    //case LANDMARK_TYPE.RITUAL_STONES:
    //             return BASE_LANDMARK_TYPE.DUNGEON;
    //case LANDMARK_TYPE.CITY:
    //case LANDMARK_TYPE.GOBLIN_CAMP:
    //         case LANDMARK_TYPE.HUT:
    //case LANDMARK_TYPE.CRATER:
    //             return BASE_LANDMARK_TYPE.SETTLEMENT;
    //         default:
    //             return BASE_LANDMARK_TYPE.NONE;
    //     }
    // }
    public static LOCATION_TYPE RandomSettlementType() {
        if (UnityEngine.Random.Range(0, 2) == 0) {
            return LOCATION_TYPE.ELVEN_SETTLEMENT;
        }
        return LOCATION_TYPE.HUMAN_SETTLEMENT;
    }
    #endregion

    #region File Utilities
    public static bool DoesFileExist(string path) {
        return System.IO.File.Exists(path);
    }
    public static List<string> GetFileChoices(string path, string searchPattern) {
        List<string> choices = new List<string>();
        string[] classes = System.IO.Directory.GetFiles(path, searchPattern);
        for (int i = 0; i < classes.Length; i++) {
            CharacterClass currentClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(classes[i]));
            choices.Add(currentClass.className);
        }
        return choices;
    }
    #endregion

    #region Characters
    //This is the list of armor, set by priority, change if needed
    public static List<ARMOR_TYPE> orderedArmorTypes = new List<ARMOR_TYPE>() {
            ARMOR_TYPE.SHIRT,
            ARMOR_TYPE.LEGGINGS,
            ARMOR_TYPE.HELMET,
            ARMOR_TYPE.BOOT,
            ARMOR_TYPE.BRACER
    };
    public static WeightedDictionary<ARMOR_TYPE> weightedArmorTypes;
    public static WeightedDictionary<ARMOR_TYPE> GetWeightedArmorTypes() {
        if (weightedArmorTypes == null) {
            weightedArmorTypes = new WeightedDictionary<ARMOR_TYPE>();
            weightedArmorTypes.AddElement(ARMOR_TYPE.SHIRT, 100);
            weightedArmorTypes.AddElement(ARMOR_TYPE.LEGGINGS, 80);
            weightedArmorTypes.AddElement(ARMOR_TYPE.HELMET, 60);
            weightedArmorTypes.AddElement(ARMOR_TYPE.BRACER, 40);
            weightedArmorTypes.AddElement(ARMOR_TYPE.BOOT, 20);
        }
        return weightedArmorTypes;
    }
    //public static bool IsRoleClassless(CHARACTER_ROLE role) {
    //    //if (role == CHARACTER_ROLE.WORKER) {
    //    //    return true;
    //    //}
    //    return false;
    //}
    public static ITEM_TYPE GetItemTypeOfEquipment(EQUIPMENT_TYPE equipmentType) {
        switch (equipmentType) {
            case EQUIPMENT_TYPE.SWORD:
            case EQUIPMENT_TYPE.DAGGER:
            case EQUIPMENT_TYPE.SPEAR:
            case EQUIPMENT_TYPE.BOW:
            case EQUIPMENT_TYPE.STAFF:
            case EQUIPMENT_TYPE.AXE:
            return ITEM_TYPE.WEAPON;
            case EQUIPMENT_TYPE.SHIRT:
            case EQUIPMENT_TYPE.BRACER:
            case EQUIPMENT_TYPE.HELMET:
            case EQUIPMENT_TYPE.LEGGINGS:
            case EQUIPMENT_TYPE.BOOT:
            return ITEM_TYPE.ARMOR;
            default:
            return ITEM_TYPE.WEAPON;
        }
    }
    public static ATTACK_CATEGORY GetAttackCategoryByClass(Character character) {
        switch (character.characterClass.className) {
            case "Warrior":
            case "Barbarian":
            return ATTACK_CATEGORY.PHYSICAL;
            case "Arcanist":
            case "Mage":
            return ATTACK_CATEGORY.MAGICAL;
        }
        return ATTACK_CATEGORY.PHYSICAL;
    }
    //0 = 0% combat/no combat, 50 = 50% combat, 100 = 100% combat/combat
    public static Dictionary<MODE, Dictionary<MODE, int>> combatChanceGrid = new Dictionary<MODE, Dictionary<MODE, int>>() {
        {MODE.DEFAULT,
            new Dictionary<MODE, int>(){
                {MODE.DEFAULT, 100},
                {MODE.ALERT, 100},
                {MODE.STEALTH, 0},
            }
        },
        {MODE.ALERT,
            new Dictionary<MODE, int>(){
                {MODE.DEFAULT, 100},
                {MODE.ALERT, 100},
                {MODE.STEALTH, 50},
            }
        },
        {MODE.STEALTH,
            new Dictionary<MODE, int>(){
                {MODE.DEFAULT, 0},
                {MODE.ALERT, 50},
                {MODE.STEALTH, 0},
            }
        },
    };
    public static List<string> specialClasses = new List<string>() {
        "Necromancer",
        "Archmage",
        "Witch",
        "Beastmaster",
        "Tempest",
    };
    public static SEXUALITY GetCompatibleSexuality(SEXUALITY sexuality) {
        int chance = rng.Next(0, 100);
        if (sexuality == SEXUALITY.STRAIGHT) {
            if (chance < 80) {
                return SEXUALITY.STRAIGHT;
            }
            return SEXUALITY.BISEXUAL;
        } else if (sexuality == SEXUALITY.BISEXUAL) {
            if (chance < 80) {
                return SEXUALITY.STRAIGHT;
            } else if (chance >= 80 && chance < 90) {
                return SEXUALITY.GAY;
            }
            return SEXUALITY.BISEXUAL;
        } else if (sexuality == SEXUALITY.GAY) {
            if (chance < 50) {
                return SEXUALITY.GAY;
            }
            return SEXUALITY.BISEXUAL;
        }
        return sexuality;
    }
    public static GENDER GetRandomGender() {
        if (UnityEngine.Random.Range(0, 2) == 0) {
            return GENDER.MALE;
        }
        return GENDER.FEMALE;
    }
    public static GENDER GetOppositeGender(GENDER gender) {
        if (gender == GENDER.FEMALE) {
            return GENDER.MALE;
        }
        return GENDER.FEMALE;
    }
    #endregion

    #region Character Tags
    public static int GetTagWorldGenChance(ATTRIBUTE tag){
		switch(tag){
		//case ATTRIBUTE.HERBALIST:
		//case ATTRIBUTE.RITUALIST:
		//	return 13;
		default:
			return 0;
		}
	}
    #endregion

    #region Number Utilities
    public static bool IsEven(int num) {
        return num % 2 == 0;
    }
    public static bool IsPositive(float num) {
        if (num > 0) {
            return true;
        }
        return false;
    }
    public static bool IsInRange(int value, int lowerBound, int upperBound) {
        if (value >= lowerBound && value < upperBound) {
            return true;
        }
        return false;
    }
    #endregion

    #region Character Relationship
    //   public static Dictionary<CHARACTER_RELATIONSHIP, CHARACTER_RELATIONSHIP_CATEGORY> charRelationshipCategory = new Dictionary<CHARACTER_RELATIONSHIP, CHARACTER_RELATIONSHIP_CATEGORY> () {
    //	{CHARACTER_RELATIONSHIP.RIVAL, CHARACTER_RELATIONSHIP_CATEGORY.NEGATIVE},
    //	{CHARACTER_RELATIONSHIP.FRIEND, CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE},
    //	{CHARACTER_RELATIONSHIP.ENEMY, CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL},
    //	{CHARACTER_RELATIONSHIP.SIBLING, CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL},
    //	{CHARACTER_RELATIONSHIP.PARENT, CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL},
    //	{CHARACTER_RELATIONSHIP.CHILD, CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL},
    //	{CHARACTER_RELATIONSHIP.LOVER, CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE},
    //	{CHARACTER_RELATIONSHIP.EX_LOVER, CHARACTER_RELATIONSHIP_CATEGORY.NEUTRAL},
    //	{CHARACTER_RELATIONSHIP.APPRENTICE, CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE},
    //	{CHARACTER_RELATIONSHIP.MENTOR, CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE},
    //	{CHARACTER_RELATIONSHIP.ACQUAINTANCE, CHARACTER_RELATIONSHIP_CATEGORY.NEUTRAL},
    //};
    #endregion

    #region UI Utilities
    public static void DestroyChildren(Transform parent) {
        Transform[] children = GetComponentsInDirectChildren<Transform>(parent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            Transform currTransform = children[i];
            if (currTransform.gameObject.GetComponent<EZObjectPools.PooledObject>() == null) {
                if (Application.isEditor) {
                    GameObject.DestroyImmediate(currTransform.gameObject);
                } else {
                    GameObject.Destroy(currTransform.gameObject);
                }
            } else {
                ObjectPoolManager.Instance.DestroyObject(currTransform.gameObject);
            }
            
        }
    }
    public static bool IsUIElementInsideScreen(RectTransform uiElement, Canvas canvas) {
        Vector3[] objectCorners = new Vector3[4];
        uiElement.GetWorldCorners(objectCorners);
        Rect screenRect = Camera.main.pixelRect;
        for (int i = 0; i < objectCorners.Length; i++) {
            Vector3 currCorner = objectCorners[i];
            if (!screenRect.Contains(currCorner)) {
                return false;
            }
        }
        return true;
    }
    public static void ScrolRectSnapTo(ScrollRect scrollRect, RectTransform target) {
        Canvas.ForceUpdateCanvases();

        scrollRect.content.anchoredPosition =
            (Vector2)scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
            - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
    }
    public static void GetAnchorMinMax(TextAnchor type, ref Vector2 anchorMin, ref Vector2 anchorMax) {
        switch (type) {
            case TextAnchor.UpperLeft:
                anchorMin = new Vector2(0f, 1f);
                anchorMax = new Vector2(0f, 1f);
                break;
            case TextAnchor.UpperCenter:
                anchorMin = new Vector2(0.5f, 1f);
                anchorMax = new Vector2(0.5f, 1f);
                break;
            case TextAnchor.UpperRight:
                anchorMin = new Vector2(1f, 1f);
                anchorMax = new Vector2(1f, 1f);
                break;
            case TextAnchor.MiddleLeft:
                anchorMin = new Vector2(0f, 0.5f);
                anchorMax = new Vector2(0f, 0.5f);
                break;
            case TextAnchor.MiddleCenter:
                anchorMin = new Vector2(0.5f, 0.5f);
                anchorMax = new Vector2(0.5f, 0.5f);
                break;
            case TextAnchor.MiddleRight:
                anchorMin = new Vector2(1f, 0.5f);
                anchorMax = new Vector2(1f, 0.5f);
                break;
            case TextAnchor.LowerLeft:
                anchorMin = new Vector2(0f, 0f);
                anchorMax = new Vector2(0f, 0f);
                break;
            case TextAnchor.LowerCenter:
                anchorMin = new Vector2(0.5f, 0f);
                anchorMax = new Vector2(0.5f, 0f);
                break;
            case TextAnchor.LowerRight:
                anchorMin = new Vector2(1f, 0f);
                anchorMax = new Vector2(1f, 0f);
                break;
            default:
                anchorMin = new Vector2(1f, 1f);
                anchorMax = new Vector2(1f, 1f);
                break;
        }
    }
    #endregion

    #region Dictionary
    public static TValue GetRandomValueFromDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict) {
        List<TValue> values = Enumerable.ToList(dict.Values);
        return values[rng.Next(dict.Count)];
    }
    public static TKey GetRandomKeyFromDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict) {
        List<TKey> keys = Enumerable.ToList(dict.Keys);
        return keys[rng.Next(dict.Count)];
    }
    #endregion

    #region Magic
    public static int GetMagicAmountByAbundance(ABUNDANCE abundance) {
        switch (abundance) {
            case ABUNDANCE.HIGH:
            return rng.Next(80,101);
            case ABUNDANCE.MED:
            return rng.Next(30, 80);
            case ABUNDANCE.LOW:
            return rng.Next(5, 30);
        }
        return 0;
    }
    #endregion

    #region Interaction
    public static INTERACTION_TYPE[] interactionPriorityList = new INTERACTION_TYPE[] {
        //Faction Created Events
        //INTERACTION_TYPE.DEFENSE_MOBILIZATION,
        //INTERACTION_TYPE.DEFENSE_UPGRADE,
        //INTERACTION_TYPE.MOVE_TO_ATTACK,

        //character targeted actions
        //INTERACTION_TYPE.ABDUCT_ACTION,
        //INTERACTION_TYPE.CHANCE_ENCOUNTER,
        //INTERACTION_TYPE.CHARM_ACTION,
        //INTERACTION_TYPE.EAT_DEFENSELESS,
        //INTERACTION_TYPE.HUNT_ACTION,
        //INTERACTION_TYPE.PATROL_ACTION,
        //INTERACTION_TYPE.RECRUIT_ACTION,
        //INTERACTION_TYPE.REANIMATE_ACTION,
        //INTERACTION_TYPE.STEAL_ACTION,
        //INTERACTION_TYPE.TORTURE_ACTION,

        //Character Departure
        //INTERACTION_TYPE.RETURN_HOME,
        //INTERACTION_TYPE.MOVE_TO_SCAVENGE,
        //INTERACTION_TYPE.MOVE_TO_RAID,
        //INTERACTION_TYPE.MOVE_TO_PEACE_NEGOTIATION,
        //INTERACTION_TYPE.MOVE_TO_EXPLORE,
        //INTERACTION_TYPE.MOVE_TO_EXPAND,
        //INTERACTION_TYPE.MOVE_TO_RAID_EVENT,
        //INTERACTION_TYPE.MOVE_TO_ATTACK,
        //INTERACTION_TYPE.MOVE_TO_PEACE_NEGOTIATION,
        //INTERACTION_TYPE.MOVE_TO_EXPLORE_EVENT,
        //INTERACTION_TYPE.MOVE_TO_EXPANSION_EVENT,
        //INTERACTION_TYPE.MOVE_TO_RETURN_HOME,
        //INTERACTION_TYPE.MOVE_TO_IMPROVE_RELATIONS_EVENT,
        //INTERACTION_TYPE.MOVE_TO_RECRUIT_ACTION,
        //INTERACTION_TYPE.MOVE_TO_CHARM_ACTION,
        //INTERACTION_TYPE.MOVE_TO_ABDUCT_ACTION,
        //INTERACTION_TYPE.MOVE_TO_STEAL_ACTION,
        //INTERACTION_TYPE.MOVE_TO_HUNT_ACTION,
        //INTERACTION_TYPE.MOVE_TO_REANIMATE_ACTION,

        //Combat
        //INTERACTION_TYPE.ATTACK,

        //Expansion

        //Peace
        //INTERACTION_TYPE.CHARACTER_PEACE_NEGOTIATION,
        //INTERACTION_TYPE.MINION_PEACE_NEGOTIATION,

        //Character Arrival
        //INTERACTION_TYPE.CHARACTER_PEACE_NEGOTIATION,
    };
    public static int GetInteractionPriorityIndex(INTERACTION_TYPE interactionType) {
        for (int i = 0; i < interactionPriorityList.Length; i++) {
            if(interactionType == interactionPriorityList[i]) {
                return i;
            }
        }
        return -1;
    }
    #endregion

    /// <summary>
    /// Method to create lists containing possible combinations of an input list of items. This is 
    /// basically copied from code by user "jaolho" on this thread:
    /// http://stackoverflow.com/questions/7802822/all-possible-combinations-of-a-list-of-values
    /// </summary>
    /// <typeparam name="T">type of the items on the input list</typeparam>
    /// <param name="inputList">list of items</param>
    /// <param name="minimumItems">minimum number of items wanted in the generated combinations, 
    ///                            if zero the empty combination is included,
    ///                            default is one</param>
    /// <param name="maximumItems">maximum number of items wanted in the generated combinations,
    ///                            default is no maximum limit</param>
    /// <returns>list of lists for possible combinations of the input items</returns>
    public static List<List<T>> ItemCombinations<T>(List<T> inputList, int numOfResults = 0, int minimumItems = 1,
                                                  int maximumItems = int.MaxValue) {
        int nonEmptyCombinations = (int) Math.Pow(2, inputList.Count) - 1;
        List<List<T>> listOfLists = new List<List<T>>(nonEmptyCombinations + 1);

        if(numOfResults <= 0) {
            // Optimize generation of empty combination, if empty combination is wanted
            if (minimumItems == 0)
                listOfLists.Add(new List<T>());

            if (minimumItems <= 1 && maximumItems >= inputList.Count) {
                // Simple case, generate all possible non-empty combinations
                for (int bitPattern = 1; bitPattern <= nonEmptyCombinations; bitPattern++)
                    listOfLists.Add(GenerateCombination(inputList, bitPattern));
            } else {
                // Not-so-simple case, avoid generating the unwanted combinations
                for (int bitPattern = 1; bitPattern <= nonEmptyCombinations; bitPattern++) {
                    int bitCount = CountBits(bitPattern);
                    if (bitCount >= minimumItems && bitCount <= maximumItems)
                        listOfLists.Add(GenerateCombination(inputList, bitPattern));
                }
            }
        } else {
            // Optimize generation of empty combination, if empty combination is wanted
            if (minimumItems == 0)
                listOfLists.Add(new List<T>());
            if(listOfLists.Count >= numOfResults) { return listOfLists; }

            if (minimumItems <= 1 && maximumItems >= inputList.Count) {
                // Simple case, generate all possible non-empty combinations
                for (int bitPattern = 1; bitPattern <= nonEmptyCombinations; bitPattern++)
                    listOfLists.Add(GenerateCombination(inputList, bitPattern));
                    if (listOfLists.Count >= numOfResults) { return listOfLists; }

            } else {
                // Not-so-simple case, avoid generating the unwanted combinations
                for (int bitPattern = 1; bitPattern <= nonEmptyCombinations; bitPattern++) {
                    int bitCount = CountBits(bitPattern);
                    if (bitCount >= minimumItems && bitCount <= maximumItems)
                        listOfLists.Add(GenerateCombination(inputList, bitPattern));
                        if (listOfLists.Count >= numOfResults) { return listOfLists; }
                }
            }
        }
        return listOfLists;
    }

    /// <summary>
    /// Sub-method of ItemCombinations() method to generate a combination based on a bit pattern.
    /// </summary>
    private static List<T> GenerateCombination<T>(List<T> inputList, int bitPattern) {
        List<T> thisCombination = new List<T>(inputList.Count);
        for (int j = 0; j < inputList.Count; j++) {
            if ((bitPattern >> j & 1) == 1)
                thisCombination.Add(inputList[j]);
        }
        return thisCombination;
    }

    /// <summary>
    /// Sub-method of ItemCombinations() method to count the bits in a bit pattern. Based on this:
    /// https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetKernighan
    /// </summary>
    private static int CountBits(int bitPattern) {
        int numberBits = 0;
        while (bitPattern != 0) {
            numberBits++;
            bitPattern &= bitPattern - 1;
        }
        return numberBits;
    }

    private static RACE[,] opposingRaces = new RACE[,] { { RACE.HUMANS, RACE.ELVES }, { RACE.FAERY, RACE.GOBLIN } };
    public static bool AreTwoCharactersFromOpposingRaces(Character character1, Character character2) {
        if(character1.race != character2.race) {
            int outerLength = opposingRaces.GetLength(0);
            for (int i = 0; i < outerLength; i++) {
                if((character1.race == opposingRaces[i, 0] || character1.race == opposingRaces[i, 1])
                    && (character2.race == opposingRaces[i, 0] || character2.race == opposingRaces[i, 1])) {
                    return true;
                }
            }
        }
        return false;
    }
    public static T GetRandomEnumValue<T>() {
        var values = Enum.GetValues(typeof(T));
        int random = UnityEngine.Random.Range(0, values.Length);
        return (T) values.GetValue(random);
    }
}
