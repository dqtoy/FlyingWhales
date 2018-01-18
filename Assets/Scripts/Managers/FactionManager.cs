using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FactionManager : MonoBehaviour {

    public static FactionManager Instance = null;

    [SerializeField] private RACE[] inititalRaces;

    private ORDER_BY orderBy = ORDER_BY.CITIES;

    public List<Faction> allFactions = new List<Faction>();
	public List<Tribe> allTribes = new List<Tribe>();
    public List<Faction> orderedFactions = new List<Faction>();

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

    #region getters
    public List<Faction> majorFactions {
        get { return allFactions.Where(x => x.factionType == FACTION_TYPE.MAJOR).ToList(); }
    }
    public List<Faction> minorFactions {
        get { return allFactions.Where(x => x.factionType == FACTION_TYPE.MINOR).ToList(); }
    }
    #endregion

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
		ECS.Character chieftain = baseSettlement.CreateNewCharacter(CHARACTER_ROLE.CHIEFTAIN, "Swordsman");
        ECS.Character villageHead = baseSettlement.CreateNewCharacter(CHARACTER_ROLE.VILLAGE_HEAD, "Swordsman");
        faction.SetLeader(chieftain);
        baseSettlement.SetHead(villageHead);
    }
    public Faction CreateNewFaction(System.Type factionType, RACE race) {
        if (factionType == typeof(Tribe)) {
            Tribe newTribe = new Tribe(race);
            allFactions.Add(newTribe);
			allTribes.Add (newTribe);
            UIManager.Instance.UpdateFactionSummary();
            return newTribe;
        } else if(factionType == typeof(Camp)) {
            Camp newCamp = new Camp(race);
            allFactions.Add(newCamp);
            UIManager.Instance.UpdateFactionSummary();
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
    public List<ECS.Character> GetAllCharactersOfType(CHARACTER_ROLE role) {
        List<ECS.Character> characters = new List<ECS.Character>();
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            characters.AddRange(currFaction.GetCharactersOfType(role));
        }
        return characters;
    }
    public ECS.Character GetCharacterByID(int id) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            ECS.Character charInFaction = currFaction.GetCharacterByID(id);
            if(charInFaction != null) {
                return charInFaction;
            }
        }
        return null;
    }
    #endregion

    #region Utilities
    public void SetOrderBy(ORDER_BY orderBy) {
        this.orderBy = orderBy;
        UpdateFactionOrderBy();
    }
    public void UpdateFactionOrderBy() {
        if (orderBy == ORDER_BY.CITIES) {
            orderedFactions = majorFactions.OrderBy(x => x.settlements.Count).ToList();
            orderedFactions.AddRange(minorFactions.OrderBy(x => x.settlements.Count));
        } else if (orderBy == ORDER_BY.POPULATION) {
            orderedFactions = majorFactions.OrderBy(x => x.totalPopulation).ToList();
            orderedFactions.AddRange(minorFactions.OrderBy(x => x.totalPopulation));
        } else if (orderBy == ORDER_BY.CHARACTERS) {
            orderedFactions = majorFactions.OrderBy(x => x.characters.Count).ToList();
            orderedFactions.AddRange(minorFactions.OrderBy(x => x.characters.Count));
        }
        UIManager.Instance.UpdateFactionSummary();
    }
    public Faction GetFactionBasedOnID(int id) {
        for (int i = 0; i < allFactions.Count; i++) {
            if (allFactions[i].id == id) {
                return allFactions[i];
            }
        }
        return null;
    }
    #endregion

    #region Relationships
    /*
     Create a new relationship between 2 factions,
     then add add a reference to that relationship, to both of the factions.
         */
    public void CreateNewRelationshipBetween(Faction faction1, Faction faction2) {
        FactionRelationship newRel = new FactionRelationship(faction1, faction2);
        faction1.AddNewRelationship(faction2, newRel);
        faction2.AddNewRelationship(faction1, newRel);
    }
    /*
     Utility Function for getting the relationship between 2 factions,
     this just adds a checking for data consistency if, the 2 factions have the
     same reference to their relationship.
     NOTE: This is probably more performance intensive because of the additional checking.
     User can opt to use each factions GetRelationshipWith() instead.
         */
    public FactionRelationship GetRelationshipBetween(Faction faction1, Faction faction2) {
        FactionRelationship faction1Rel = faction1.GetRelationshipWith(faction2);
        FactionRelationship faction2Rel = faction2.GetRelationshipWith(faction1);
        if (faction1Rel == faction2Rel) {
            return faction1Rel;
        }
        throw new System.Exception(faction1.name + " does not have the same relationship object as " + faction2.name + "!");
    }
    #endregion

}
