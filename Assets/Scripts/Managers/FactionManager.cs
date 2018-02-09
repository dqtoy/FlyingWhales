using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FactionManager : MonoBehaviour {

    public static FactionManager Instance = null;

    [SerializeField] private RACE[] inititalRaces;

    private ORDER_BY orderBy = ORDER_BY.CITIES;

    public List<QuestTypeSetup> questTypeSetups;

    public List<Faction> allFactions = new List<Faction>();
	public List<Tribe> allTribes = new List<Tribe>();
    public List<Faction> orderedFactions = new List<Faction>();

    public List<Quest> allQuests = new List<Quest>();

    public Dictionary<RACE, List<TECHNOLOGY>> initialRaceTechnologies = new Dictionary<RACE, List<TECHNOLOGY>>() {
        { RACE.HUMANS, new List<TECHNOLOGY>(){
            TECHNOLOGY.BASIC_FARMING,
            TECHNOLOGY.BASIC_MINING,
            TECHNOLOGY.SWORDSMAN_CLASS,
            TECHNOLOGY.SPEARMAN_CLASS,
            TECHNOLOGY.WILDLING_CLASS,
            TECHNOLOGY.SWORD_MAKING,
            TECHNOLOGY.SPEAR_MAKING,
            TECHNOLOGY.AXE_MAKING,
            TECHNOLOGY.CHEST_ARMOR_MAKING,
            TECHNOLOGY.HELMET_MAKING
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
            TECHNOLOGY.LEGGINGS_MAKING,
            TECHNOLOGY.GLOVE_MAKING
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
            CreateInitialResourcesForSettlement(newFaction.settlements.First());
        }
    }
    /*
     Initital tribes should have a chieftain and a village head.
         */
    private void CreateInititalFactionCharacters(Faction faction) {
        Settlement baseSettlement = faction.settlements[0];
		ECS.Character chieftain = baseSettlement.CreateNewCharacter(CHARACTER_ROLE.CHIEFTAIN, "Swordsman");
        ECS.Character villageHead = baseSettlement.CreateNewCharacter(CHARACTER_ROLE.VILLAGE_HEAD, "Swordsman");
		ECS.Character worker = baseSettlement.CreateNewCharacter(CHARACTER_ROLE.WORKER, "Classless");

        faction.SetLeader(chieftain);
        baseSettlement.SetHead(villageHead);

        //Create 2 adventurers
        baseSettlement.CreateNewCharacter(CHARACTER_ROLE.ADVENTURER, "Swordsman");
        baseSettlement.CreateNewCharacter(CHARACTER_ROLE.ADVENTURER, "Swordsman");
    }
    public Faction CreateNewFaction(System.Type factionType, RACE race) {
        Faction newFaction = null;
        if (factionType == typeof(Tribe)) {
            newFaction = new Tribe(race);
			allTribes.Add ((Tribe)newFaction);
        } else if(factionType == typeof(Camp)) {
            newFaction = new Camp(race);
        }
        allFactions.Add(newFaction);
        CreateRelationshipsForNewFaction(newFaction);
        UIManager.Instance.UpdateFactionSummary();
        return newFaction;
    }
    /*
     factions have initial resources, that depends on the material preferences, 
     the initial settlement will have 200 of each preferred material. NOTE: This stacks
         */
    private void CreateInitialResourcesForSettlement(Settlement initialSettlement) {
        Faction owner = initialSettlement.owner;
        PRODUCTION_TYPE[] productionTypes = Utilities.GetEnumValues<PRODUCTION_TYPE>();
        for (int i = 0; i < productionTypes.Length; i++) {
            PRODUCTION_TYPE currProdType = productionTypes[i];
            MATERIAL prefMat = owner.GetHighestMaterialPriority(currProdType);
            initialSettlement.AdjustMaterial(prefMat, 200);
        }
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
    public Faction GetFactionBasedOnName(string name) {
        for (int i = 0; i < allFactions.Count; i++) {
            if (allFactions[i].name.ToLower() == name.ToLower()) {
                return allFactions[i];
            }
        }
        return null;
    }
    #endregion

    #region Relationships
    public void CreateRelationshipsForNewFaction(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if(otherFaction.id != faction.id) {
                CreateNewRelationshipBetween(otherFaction, faction);
            }
        }
    }
    public void RemoveRelationshipsWith(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if (otherFaction.id != faction.id) {
                otherFaction.RemoveRelationshipWith(faction);
            }
        }
    }
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

    #region Quests
    public Quest GetQuestByID(int id) {
        for (int i = 0; i < allQuests.Count; i++) {
            Quest currQuest = allQuests[i];
            if (currQuest.id == id) {
                return currQuest;
            }
        }
        return null;
    }
    public void AddQuest(Quest quest) {
        allQuests.Add(quest);
    }
    public void RemoveQuest(Quest quest) {
        allQuests.Remove(quest);
    }
    /*
     Check if a quest can cause harmful effects to the owner
     of the region.
         */
    public bool IsQuestHarmful(QUEST_TYPE questType) {
        for (int i = 0; i < questTypeSetups.Count; i++) {
            QuestTypeSetup currSetup = questTypeSetups[i];
            if(currSetup.questType == questType) {
                return currSetup.isHarmful;
            }
        }
        return false;
    }
    ///*
    // Can a quest type be accepted by characters from another faction.
    //     */
    //public bool CanQuestBeAcceptedOutsideFaction(QUEST_TYPE questType) {
    //    for (int i = 0; i < questTypeSetups.Count; i++) {
    //        QuestTypeSetup currSetup = questTypeSetups[i];
    //        if (currSetup.questType == questType) {
    //            return currSetup.canBeAcceptedOutsideFaction;
    //        }
    //    }
    //    return false;
    //}
    public QuestTypeSetup GetQuestTypeSetup(QUEST_TYPE questType) {
        for (int i = 0; i < questTypeSetups.Count; i++) {
            QuestTypeSetup currSetup = questTypeSetups[i];
            if (currSetup.questType == questType) {
                return currSetup;
            }
        }
        return null;
    }
    #endregion

    #region International Incidents
    public void InternationalIncidentOccured(Faction aggrievedFaction, Faction aggressorFaction, INTERNATIONAL_INCIDENT_TYPE incidentType, object data) {
        Debug.Log("An international incident has occured between " + aggrievedFaction.name + " and " + aggressorFaction.name + "/" + incidentType.ToString());
        FactionRelationship rel = GetRelationshipBetween(aggrievedFaction, aggressorFaction);
        if(rel.relationshipStatus != RELATIONSHIP_STATUS.HOSTILE) {//Incidents will not occur if two factions are already hostile
            rel.AdjustSharedOpinion(-5); //Each time an international incident occurs, Shared Opinion decreases by 5.

            /*When an incident occurs, war may be declared. The likelihood of this happening is dependent on 
            Tribal Opinion and Chieftain Traits. Any Tribe that declare war should remove existing 
            International Incidents, Trade Deals and Alliances between them.*/
            WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> actionWeights = GetInternationalIncidentWeights(aggrievedFaction, aggressorFaction, rel, incidentType, data);
            INTERNATIONAL_INCIDENT_ACTION chosenAction = actionWeights.PickRandomElementGivenWeights();
            if (chosenAction == INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR) {
                DecalreWar(aggressorFaction, aggressorFaction, incidentType, data);
            }
        }
    }
    private WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> GetInternationalIncidentWeights(Faction aggrievedFaction, Faction aggressorFaction, 
        FactionRelationship relationship, INTERNATIONAL_INCIDENT_TYPE incidentType, object data) {

        WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> actionWeights = new WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION>();
        actionWeights.AddElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, 95); //95 Weight to do nothing
        actionWeights.AddElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 5); //5 Weight to Declare War

        //Character Death
        if(incidentType == INTERNATIONAL_INCIDENT_TYPE.CHARACTER_DEATH) {
            CHARACTER_ROLE diedRole = ((ECS.Character)data).role.roleType;
            switch (diedRole) {
                case CHARACTER_ROLE.CHIEFTAIN:
                    actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 150); //- Chieftain: Add 150 Weight to Declare War
                    break;
                case CHARACTER_ROLE.WARLORD:
                    actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 20); //-Warlord: Add 20 Weight to Declare War
                    break;
                case CHARACTER_ROLE.HERO:
                    actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 20); //-Hero: Add 20 Weight to Declare War
                    break;
                case CHARACTER_ROLE.VILLAGE_HEAD:
                    actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 50); //-Village Head: Add 50 Weight to Declare War
                    break;
                default:
                    actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 10); //-Others: Add 10 Weight to Declare War
                    break;
            }
        }
        //Negative Quest
        else if (incidentType == INTERNATIONAL_INCIDENT_TYPE.HARMFUL_QUEST) {
            //Add Weight to Declare War as listed on the Quest Type
            QuestTypeSetup qts = GetQuestTypeSetup(((Quest)data).questType);
            actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, qts.declareWarWeight);
        }

        //Opinions
        if(relationship.sharedOpinion > 0) {
            //- Add 1 Weight to Do Nothing per 1 Positive Point of Shared Opinion
            actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, relationship.sharedOpinion);
        } else if(relationship.sharedOpinion < 0) {
            //- Add 1 Weight to Declare War per 1 Negative Point of Shared Opinion
            actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, Mathf.Abs(relationship.sharedOpinion)); 
        }

        //Traits
        if(aggrievedFaction.leader != null) {
            for (int i = 0; i < aggrievedFaction.leader.traits.Count; i++) {
                Trait currTrait = aggrievedFaction.leader.traits[i];
                WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> traitWeight = currTrait
                    .GetInternationalIncidentReactionWeight(incidentType, relationship, aggressorFaction);
                if(traitWeight != null) {
                    actionWeights.AddElements(traitWeight);
                }
            }
        }

        //All
        //Add 50 Weight to Do Nothing if Ally
        if(relationship.relationshipStatus == RELATIONSHIP_STATUS.FRIENDLY) {
            actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, 50);
        }

        //Add 100 Weight to Do Nothing per active wars I have
        actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, 100 * aggrievedFaction.activeWars); 

        Relationship chieftainRel = CharacterManager.Instance.GetRelationshipBetween(aggressorFaction.leader, aggrievedFaction.leader);
        if(chieftainRel != null) {
            if (chieftainRel.totalValue > 0) {
                //Add 2 Weight to Do Nothing per Positive Opinion the Chieftain has towards the other Chieftain(if they have a relationship)
                actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, 2 * chieftainRel.totalValue);
            } else if(chieftainRel.totalValue < 0) {
                //Add 1 Weight to Declare per Negative Opinion the Chieftain has towards the other Chieftain(if they have a relationship)
                actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, Mathf.Abs(chieftainRel.totalValue));
            }
        }
        return actionWeights;
    }
    public void DecalreWar(Faction faction1, Faction faction2, INTERNATIONAL_INCIDENT_TYPE reason, object data) {
        Debug.Log(faction1.name + " declares war on " + faction2.name);
        FactionRelationship rel = GetRelationshipBetween(faction1, faction2);
        rel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.HOSTILE); //Set relationship with each other as hostile
        rel.SetWarStatus(true);
        if (reason == INTERNATIONAL_INCIDENT_TYPE.CHARACTER_DEATH) {
            Log declareWarLog = new Log(GameManager.Instance.Today(), "General", "Faction", "declare_war_character_death");
            declareWarLog.AddToFillers(faction1, faction1.name, LOG_IDENTIFIER.KINGDOM_1);
            declareWarLog.AddToFillers(faction2, faction2.name, LOG_IDENTIFIER.KINGDOM_2);
            ECS.Character character = (ECS.Character)data;
            declareWarLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            declareWarLog.AddToFillers(character.currLocation.region.centerOfMass.landmarkOnTile, character.currLocation.region.centerOfMass.landmarkOnTile.landmarkName, LOG_IDENTIFIER.CITY_1);
            UIManager.Instance.ShowNotification(declareWarLog);
        } else {
            Log declareWarLog = new Log(GameManager.Instance.Today(), "General", "Faction", "declare_war_quest");
            declareWarLog.AddToFillers(faction1, faction1.name, LOG_IDENTIFIER.KINGDOM_1);
            declareWarLog.AddToFillers(faction2, faction2.name, LOG_IDENTIFIER.KINGDOM_2);
            Quest quest = (Quest)data;
            declareWarLog.AddToFillers(quest.assignedParty.partyLeader, quest.assignedParty.partyLeader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            declareWarLog.AddToFillers(quest, Utilities.NormalizeString(quest.questType.ToString()), LOG_IDENTIFIER.OTHER);
            declareWarLog.AddToFillers(quest.assignedParty.currLocation.region.centerOfMass.landmarkOnTile, quest.assignedParty.currLocation.region.centerOfMass.landmarkOnTile.landmarkName, LOG_IDENTIFIER.CITY_1);
            UIManager.Instance.ShowNotification(declareWarLog);
        }
        

        /* When war is declared, Friends of the two Tribes have to determine what to do. 
         In determining which side to consider in case he is Friends with both, 
         choose the one that he has highest positive Opinion of. If same, randomize. */
        List<Faction> faction1Friends = faction1.GetMajorFactionsWithRelationshipStatus(RELATIONSHIP_STATUS.FRIENDLY);
        List<Faction> faction2Friends = faction2.GetMajorFactionsWithRelationshipStatus(RELATIONSHIP_STATUS.FRIENDLY);
        List<Faction> commonFriends = Utilities.Intersect(faction1Friends, faction2Friends);
        for (int i = 0; i < commonFriends.Count; i++) {
            Faction commonFriend = commonFriends[i];
            FactionRelationship relWithFac1 = commonFriend.GetRelationshipWith(faction1);
            FactionRelationship relWithFac2 = commonFriend.GetRelationshipWith(faction2);
            if(relWithFac1.sharedOpinion > relWithFac2.sharedOpinion) {
                //more friends with faction 1
                faction2Friends.Remove(commonFriend);
            } else if(relWithFac1.sharedOpinion < relWithFac2.sharedOpinion) {
                //more friends with faction 2
                faction1Friends.Remove(commonFriend);
            } else {
                //opinions are equal. Randomize
                if(UnityEngine.Random.Range(0,2) == 0) {
                    faction1Friends.Remove(commonFriend);
                } else {
                    faction2Friends.Remove(commonFriend);
                }
            }
        }

        for (int i = 0; i < faction1Friends.Count; i++) {
            Faction currAlly = faction1Friends[i];
            WeightedDictionary<ALLY_WAR_REACTION> allyReactionWeights = GetAllyWarReaction(currAlly, faction1, faction2);
            ALLY_WAR_REACTION chosenReaction = allyReactionWeights.PickRandomElementGivenWeights();
            switch (chosenReaction) {
                case ALLY_WAR_REACTION.JOIN_WAR:
                    //Join the war, side with faction 1
                    FactionRelationship enemyRel = currAlly.GetRelationshipWith(faction2);
                    enemyRel.SetWarStatus(true); //Set rel with faction 2 as at war
                    enemyRel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.HOSTILE); //Set rel with faction 2 as HOSTILE
                    ShowJoinWarLog(currAlly, faction2);
                    break;
                case ALLY_WAR_REACTION.BETRAY:
                    //Join the war, side with faction 2
                    FactionRelationship friendRel = currAlly.GetRelationshipWith(faction2);
                    friendRel.SetWarStatus(true); //Set rel with faction 1 as at war
                    friendRel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.HOSTILE); //Set rel with faction 1 as HOSTILE
                    ShowBetrayalLog(currAlly, faction1);
                    break;
                default:
                    break;
            }
        }

        for (int i = 0; i < faction2Friends.Count; i++) {
            Faction currAlly = faction2Friends[i];
            WeightedDictionary<ALLY_WAR_REACTION> allyReactionWeights = GetAllyWarReaction(currAlly, faction2, faction1);
            ALLY_WAR_REACTION chosenReaction = allyReactionWeights.PickRandomElementGivenWeights();
            switch (chosenReaction) {
                case ALLY_WAR_REACTION.JOIN_WAR:
                    //TODO: Join the war, side with faction 2
                    FactionRelationship enemyRel = currAlly.GetRelationshipWith(faction1);
                    enemyRel.SetWarStatus(true); //Set rel with faction 1 as at war
                    enemyRel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.HOSTILE); //Set rel with faction 1 as HOSTILE
                    ShowJoinWarLog(currAlly, faction1);
                    break;
                case ALLY_WAR_REACTION.BETRAY:
                    //TODO: Join the war, side with faction 1
                    FactionRelationship friendRel = currAlly.GetRelationshipWith(faction2);
                    friendRel.SetWarStatus(true); //Set rel with faction 2 as at war
                    friendRel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.HOSTILE); //Set rel with faction 2 as HOSTILE
                    ShowBetrayalLog(currAlly, faction2);
                    break;
                default:
                    break;
            }
        }
    }
    private void ShowJoinWarLog(Faction faction, Faction enemy) {
        Log declareWarLog = new Log(GameManager.Instance.Today(), "General", "Faction", "declare_war_help_friend");
        declareWarLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.KINGDOM_1);
        declareWarLog.AddToFillers(enemy, enemy.name, LOG_IDENTIFIER.KINGDOM_2);
        UIManager.Instance.ShowNotification(declareWarLog);
    }
    private void ShowBetrayalLog(Faction faction, Faction enemy) {
        Log declareWarLog = new Log(GameManager.Instance.Today(), "General", "Faction", "declare_war_betrayal");
        declareWarLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.KINGDOM_1);
        declareWarLog.AddToFillers(enemy, enemy.name, LOG_IDENTIFIER.KINGDOM_2);
        UIManager.Instance.ShowNotification(declareWarLog);
    }
    private WeightedDictionary<ALLY_WAR_REACTION> GetAllyWarReaction(Faction faction, Faction friend, Faction enemy) {
        WeightedDictionary<ALLY_WAR_REACTION> actionWeights = new WeightedDictionary<ALLY_WAR_REACTION>();
        FactionRelationship relWithFriend = faction.GetRelationshipWith(friend);
        FactionRelationship relWithEnemy = faction.GetRelationshipWith(enemy);
        if (faction.leader != null) {
            for (int i = 0; i < faction.leader.traits.Count; i++) {
                Trait currTrait = faction.leader.traits[i];
                WeightedDictionary<ALLY_WAR_REACTION> traitDict = currTrait.GetAllyReactionWeight(friend, enemy);
                actionWeights.AddElements(traitDict);
            }
        }
        //All
        if(relWithFriend.sharedOpinion > 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.JOIN_WAR, 5 * relWithFriend.sharedOpinion); //+5 Weight to Join War for every Positive Opinion shared with friendly Tribe
        } else if(relWithFriend.sharedOpinion < 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.REMAIN_NEUTRAL, Mathf.Abs(2 * relWithFriend.sharedOpinion));//+2 Weight to Remain Neutral for every Negative Opinion shared with friendly Tribe
        }

        if (relWithEnemy.sharedOpinion < 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.JOIN_WAR, Mathf.Abs(5 * relWithEnemy.sharedOpinion)); //+5 Weight to Join War for every Negative Opinion shared with enemy Tribe
        } else if(relWithEnemy.sharedOpinion > 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.REMAIN_NEUTRAL, 2 * relWithEnemy.sharedOpinion); //+2 Weight to Remain Neutral for every Positive Opinion shared with enemy Tribe
        }

        int relativeStrTowardsEnemy = relWithEnemy.factionLookup[faction.id].relativeStrength;
        if (relativeStrTowardsEnemy > 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.JOIN_WAR, 2 * relativeStrTowardsEnemy);//+2 Weight to Join War for every positive point of Relative Strength I have over the enemy Tribe
        } else if(relativeStrTowardsEnemy < 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.REMAIN_NEUTRAL, Mathf.Abs(2 * relativeStrTowardsEnemy));//+2 Weight to Remain Neutral for every negative point of Relative Strength I have under the enemy Tribe
        }
        
        
        return actionWeights;
    }
    #endregion
}
