using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FactionManager : MonoBehaviour {

    public static FactionManager Instance = null;

    [SerializeField] private RACE[] inititalRaces;

    public List<Faction> allFactions = new List<Faction>();
    public Dictionary<RACE, List<TECHNOLOGY>> inititalRaceTechnologies = new Dictionary<RACE, List<TECHNOLOGY>>() {
        { RACE.HUMANS, new List<TECHNOLOGY>(){
            TECHNOLOGY.BASIC_FARMING,
            TECHNOLOGY.BASIC_MINING,
            TECHNOLOGY.SWORDSMAN_CLASS,
            TECHNOLOGY.SPEARMAN_CLASS,
            TECHNOLOGY.WILDLING_CLASS,
            TECHNOLOGY.SWORD_MAKING,
            TECHNOLOGY.SPEAR_MAKING,
            TECHNOLOGY.AXE_MAKING,
            TECHNOLOGY.BASIC_SMITHING
        }},
        { RACE.ELVES, new List<TECHNOLOGY>(){
            TECHNOLOGY.BASIC_HUNTING,
            TECHNOLOGY.BASIC_WOODCUTTING,
            TECHNOLOGY.ARCHER_CLASS,
            TECHNOLOGY.ROGUE_CLASS,
            TECHNOLOGY.MAGE_CLASS,
            TECHNOLOGY.BOW_MAKING,
            TECHNOLOGY.DAGGER_MAKING,
            TECHNOLOGY.STAFF_MAKING,
            TECHNOLOGY.BASIC_WOODCRAFTING
        }}
    };

    [Space(10)]
    [Header("Visuals")]
    [SerializeField] private List<Sprite> _emblemBGs;
    [SerializeField] private List<Sprite> _emblems;
    [SerializeField] private List<Sprite> usedEmblems = new List<Sprite>();

    [Space(10)]
    [Header("Kingdom Size Modifiers")]
    [SerializeField] internal float smallToMediumReqPercentage;
    [SerializeField] internal float mediumToLargeReqPercentage;
    [SerializeField] internal int smallToMediumReq;
    [SerializeField] internal int mediumToLargeReq;

    private void Awake() {
        Instance = this;
    }

    #region Faction Generation
    /*
     Generate the initital factions,
     races are specified in the inspector (inititalRaces)
     */
    public void GenerateInititalFactions() {
        smallToMediumReq = Mathf.FloorToInt((float)GridMap.Instance.numOfRegions * (smallToMediumReqPercentage / 100f));
        mediumToLargeReq = Mathf.FloorToInt((float)GridMap.Instance.numOfRegions * (mediumToLargeReqPercentage / 100f));
        List<Region> allRegions = new List<Region>(GridMap.Instance.allRegions);
        for (int i = 0; i < inititalRaces.Length; i++) {
            RACE inititalRace = inititalRaces[i];
            Faction newFaction = CreateNewFaction(typeof(Tribe), inititalRace);
            Region regionForFaction = allRegions[Random.Range(0, allRegions.Count)];
            allRegions.Remove(regionForFaction);
            Utilities.ListRemoveRange(allRegions, regionForFaction.adjacentRegions);
            LandmarkManager.Instance.OccupyLandmark(regionForFaction, newFaction);
            regionForFaction.centerOfMass.landmarkOnTile.AdjustPopulation(100); //Capital Cities that spawn at world generation starts with 100 Population each.
            CreateInititalFactionCharacters(newFaction);
        }
    }
    /*
     Initital tribes should have a chieftain and a village head.
         */
    private void CreateInititalFactionCharacters(Faction faction) {
        Settlement baseSettlement = faction.settlements[0];
        baseSettlement.CreateNewCharacter(CHARACTER_ROLE.CHIEFTAIN, CHARACTER_CLASS.SWORDSMAN);
        baseSettlement.CreateNewCharacter(CHARACTER_ROLE.VILLAGE_HEAD, CHARACTER_CLASS.SWORDSMAN);
    }
    public Faction CreateNewFaction(System.Type factionType, RACE race) {
        if (factionType == typeof(Tribe)) {
            Tribe newTribe = new Tribe(race);
            allFactions.Add(newTribe);
            return newTribe;
        } else if(factionType == typeof(Camp)) {
            Camp newCamp = new Camp(race);
            allFactions.Add(newCamp);
            return newCamp;
        }
        return null;
    }
    #endregion

    #region Emblem
    /*
     * Generate an emblem for a kingdom.
     * This will return a sprite and set that sprite as used.
     * Will return an error if there are no more available emblems.
     * */
    internal Sprite GenerateFactionEmblem(Faction faction) {
        List<Sprite> emblemsToUse = new List<Sprite>(_emblems);
        for (int i = 0; i < emblemsToUse.Count; i++) {
            Sprite currSprite = emblemsToUse[i];
            if (!usedEmblems.Contains(currSprite)) {
                AddEmblemAsUsed(currSprite);
                return currSprite;
            }
        }
        throw new System.Exception("There are no more emblems for kingdom: " + faction.name);
    }
    internal Sprite GenerateFactionEmblemBG() {
        return _emblemBGs[Random.Range(0, _emblemBGs.Count)];
    }
    internal void AddEmblemAsUsed(Sprite emblem) {
        if (!usedEmblems.Contains(emblem)) {
            usedEmblems.Add(emblem);
        } else {
            throw new System.Exception("Emblem " + emblem.name + " is already being used!");
        }
    }
    internal void RemoveEmblemAsUsed(Sprite emblem) {
        usedEmblems.Remove(emblem);
    }
    #endregion

    #region Characters
    public List<Character> GetAllCharactersOfType(CHARACTER_ROLE role) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            characters.AddRange(currFaction.GetCharactersOfType(role));
        }
        return characters;
    }
    #endregion
}
