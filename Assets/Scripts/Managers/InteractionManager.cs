using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour {

    public static InteractionManager Instance = null;

    public static readonly string Supply_Cache_Reward_1 = "SupplyCacheReward1";
    public static readonly string Mana_Cache_Reward_1 = "ManaCacheReward1";
    public static readonly string Mana_Cache_Reward_2 = "ManaCacheReward2";
    public static readonly string Level_Reward_1 = "LevelReward1";
    public static readonly string Level_Reward_2 = "LevelReward2";

    public Queue<Interaction> interactionUIQueue { get; private set; }

    [SerializeField] private RoleInteractionsListDictionary roleDefaultInteractions;
    [SerializeField] private JobInteractionsListDictionary jobNPCInteractions;

    public Dictionary<string, RewardConfig> rewardConfig = new Dictionary<string, RewardConfig>(){
        { Supply_Cache_Reward_1, new RewardConfig(){ rewardType = REWARD.SUPPLY, lowerRange = 50, higherRange = 250 } },
        { Mana_Cache_Reward_1, new RewardConfig(){ rewardType = REWARD.MANA, lowerRange = 5, higherRange = 30 } },
        { Mana_Cache_Reward_2, new RewardConfig(){ rewardType = REWARD.MANA, lowerRange = 30, higherRange = 50 } },
        { Level_Reward_1, new RewardConfig(){ rewardType = REWARD.LEVEL, lowerRange = 1, higherRange = 1 } },
        { Level_Reward_2, new RewardConfig(){ rewardType = REWARD.LEVEL, lowerRange = 2, higherRange = 2 } },
    };

    private void Awake() {
        Instance = this;
    }
    private void Start() {
        interactionUIQueue = new Queue<Interaction>();
        Messenger.AddListener<Interaction>(Signals.CLICKED_INTERACTION_BUTTON, OnClickInteraction);
    }
    public Interaction CreateNewInteraction(INTERACTION_TYPE interactionType, BaseLandmark interactable) {
        Interaction createdInteraction = null;
        switch (interactionType) {
            case INTERACTION_TYPE.BANDIT_RAID:
                createdInteraction = new BanditRaid(interactable);
                break;
            //case INTERACTION_TYPE.INVESTIGATE:
            //    createdInteraction = new InvestigateInteraction(interactable);
            //    break;
            case INTERACTION_TYPE.ABANDONED_HOUSE:
                createdInteraction = new AbandonedHouse(interactable);
                break;
            case INTERACTION_TYPE.UNEXPLORED_CAVE:
                createdInteraction = new UnexploredCave(interactable);
                break;
            case INTERACTION_TYPE.HARVEST_SEASON:
                createdInteraction = new HarvestSeason(interactable);
                break;
            case INTERACTION_TYPE.SPIDER_QUEEN:
                createdInteraction = new TheSpiderQueen(interactable);
                break;
            case INTERACTION_TYPE.HUMAN_BANDIT_REINFORCEMENTS:
                createdInteraction = new HumanBanditReinforcements(interactable);
                break;
            case INTERACTION_TYPE.GOBLIN_BANDIT_REINFORCEMENTS:
                createdInteraction = new GoblinBanditReinforcements(interactable);
                break;
            case INTERACTION_TYPE.MYSTERY_HUM:
                createdInteraction = new MysteryHum(interactable);
                break;
            case INTERACTION_TYPE.ARMY_UNIT_TRAINING:
                createdInteraction = new ArmyUnitTraining(interactable);
                break;
            case INTERACTION_TYPE.ARMY_MOBILIZATION:
                createdInteraction = new ArmyMobilization(interactable);
                break;
            case INTERACTION_TYPE.UNFINISHED_CURSE:
                createdInteraction = new UnfinishedCurse(interactable);
                break;
            case INTERACTION_TYPE.ARMY_ATTACKS:
                createdInteraction = new ArmyAttacks(interactable);
                break;
            case INTERACTION_TYPE.SUSPICIOUS_SOLDIER_MEETING:
                createdInteraction = new SuspiciousSoldierMeeting(interactable);
                break;
            case INTERACTION_TYPE.KILLER_ON_THE_LOOSE:
                createdInteraction = new KillerOnTheLoose(interactable);
                break;
            case INTERACTION_TYPE.MYSTERIOUS_SARCOPHAGUS:
                createdInteraction = new MysteriousSarcophagus(interactable);
                break;
            case INTERACTION_TYPE.NOTHING_HAPPENED:
                createdInteraction = new NothingHappened(interactable);
                break;
            case INTERACTION_TYPE.CHARACTER_EXPLORES:
                createdInteraction = new CharacterExplores(interactable);
                break;
            case INTERACTION_TYPE.CHARACTER_TRACKING:
                createdInteraction = new CharacterTracking(interactable);
                break;
            //case INTERACTION_TYPE.RETURN_HOME:
            //    createdInteraction = new ReturnHome(interactable);
            //    break;
            case INTERACTION_TYPE.FACTION_ATTACKS:
                createdInteraction = new FactionAttacks(interactable);
                break;
            case INTERACTION_TYPE.RAID_SUCCESS:
                createdInteraction = new RaidSuccess(interactable);
                break;
            case INTERACTION_TYPE.MINION_FAILED:
                createdInteraction = new MinionFailed(interactable);
                break;
            case INTERACTION_TYPE.MINION_CRITICAL_FAIL:
                createdInteraction = new MinionCriticalFail(interactable);
                break;
            case INTERACTION_TYPE.FACTION_DISCOVERED:
                createdInteraction = new FactionDiscovered(interactable);
                break;
            case INTERACTION_TYPE.LOCATION_OBSERVED:
                createdInteraction = new LocationObserved(interactable);
                break;
            case INTERACTION_TYPE.DEFENDERS_REVEALED:
                createdInteraction = new DefendersRevealed(interactable);
                break;
            case INTERACTION_TYPE.FRIENDLY_CHARACTER_ENCOUNTERED:
                createdInteraction = new FriendlyCharacterEncountered(interactable);
                break;
            case INTERACTION_TYPE.SPAWN_CHARACTER:
                createdInteraction = new SpawnCharacter(interactable);
                break;
            case INTERACTION_TYPE.SPAWN_NEUTRAL_CHARACTER:
                createdInteraction = new SpawnNeutralCharacter(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_SCAVENGE:
                createdInteraction = new MoveToScavenge(interactable);
                break;
            case INTERACTION_TYPE.SCAVENGE_EVENT:
                createdInteraction = new ScavengeEvent(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_RAID:
                createdInteraction = new MoveToRaid(interactable);
                break;
            case INTERACTION_TYPE.RAID_EVENT:
                createdInteraction = new RaidEvent(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_ATTACK:
                createdInteraction = new MoveToAttack(interactable);
                break;
            case INTERACTION_TYPE.ATTACK:
                createdInteraction = new Attack(interactable);
                break;
            case INTERACTION_TYPE.INDUCE_WAR:
                createdInteraction = new InduceWar(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_PEACE_NEGOTIATION:
                createdInteraction = new MoveToPeaceNegotiation(interactable);
                break;
            case INTERACTION_TYPE.CHARACTER_PEACE_NEGOTIATION:
                createdInteraction = new CharacterPeaceNegotiation(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_EXPLORE:
                createdInteraction = new MoveToExplore(interactable);
                break;
            case INTERACTION_TYPE.MINION_PEACE_NEGOTIATION:
                createdInteraction = new MinionPeaceNegotiation(interactable);
                break;
            case INTERACTION_TYPE.DEFENSE_UPGRADE:
                createdInteraction = new DefenseUpgrade(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_EXPAND:
                createdInteraction = new MoveToExpand(interactable);
                break;
            case INTERACTION_TYPE.FACTION_UPGRADE:
                createdInteraction = new FactionUpgrade(interactable);
                break;
            case INTERACTION_TYPE.DEFENSE_MOBILIZATION:
                createdInteraction = new DefenseMobilization(interactable);
                break;
            case INTERACTION_TYPE.WORK_EVENT:
                createdInteraction = new WorkEvent(interactable);
                break;
            case INTERACTION_TYPE.INDUCE_GRUDGE:
                createdInteraction = new InduceGrudge(interactable);
                break;
            case INTERACTION_TYPE.INFLICT_ILLNESS:
                createdInteraction = new InflictIllness(interactable);
                break;
            case INTERACTION_TYPE.SPY_SPAWN_INTERACTION_1:
                createdInteraction = new SpySpawnInteraction1(interactable);
                break;
            case INTERACTION_TYPE.SPY_SPAWN_INTERACTION_2:
                createdInteraction = new SpySpawnInteraction2(interactable);
                break;
            case INTERACTION_TYPE.SPY_SPAWN_INTERACTION_3:
                createdInteraction = new SpySpawnInteraction3(interactable);
                break;
            case INTERACTION_TYPE.SPY_SPAWN_INTERACTION_4:
                createdInteraction = new SpySpawnInteraction4(interactable);
                break;
            case INTERACTION_TYPE.EXPLORER_SPAWN_INTERACTION_1:
                createdInteraction = new ExplorerSpawnInteraction1(interactable);
                break;
            case INTERACTION_TYPE.INSTIGATOR_CHARACTER_ENCOUNTER:
                createdInteraction = new InstigatorCharacterEncounter(interactable);
                break;
            case INTERACTION_TYPE.CREATE_NECROMANCER:
                createdInteraction = new CreateNecromancer(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_RETURN_HOME:
                createdInteraction = new MoveToReturnHome(interactable);
                break;
            case INTERACTION_TYPE.EXPLORE_EVENT:
                createdInteraction = new ExploreEvent(interactable);
                break;
            case INTERACTION_TYPE.INSTIGATOR_TARGET_LOCATION:
                createdInteraction = new InstigatorTargetLocation(interactable);
                break;
            case INTERACTION_TYPE.INSTIGATOR_FACTION_FRAME_UP:
                createdInteraction = new InstigatorFactionFrameUp(interactable);
                break;
            case INTERACTION_TYPE.EXPANSION_EVENT:
                createdInteraction = new ExpansionEvent(interactable);
                break;
            case INTERACTION_TYPE.RAIDER_CHARACTER_ENCOUNTER:
                createdInteraction = new RaiderCharacterEncounter(interactable);
                break;
            case INTERACTION_TYPE.RAIDER_TARGET_LOCATION:
                createdInteraction = new RaiderTargetLocation(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_RECRUIT:
                createdInteraction = new MoveToRecruit(interactable);
                break;
            case INTERACTION_TYPE.RECRUIT_ACTION:
                createdInteraction = new RecruitAction(interactable);
                break;
            case INTERACTION_TYPE.DIPLOMAT_CHARACTER_ENCOUNTER:
                createdInteraction = new DiplomatCharacterEncounter(interactable);
                break;
            case INTERACTION_TYPE.DIPLOMAT_TARGET_LOCATION:
                createdInteraction = new DiplomatTargetLocation(interactable);
                break;
            case INTERACTION_TYPE.DIPLOMAT_FACTION_MEDIATION:
                createdInteraction = new DiplomatFactionMediation(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_IMPROVE_RELATIONS:
                createdInteraction = new MoveToImproveRelations(interactable);
                break;
            case INTERACTION_TYPE.IMPROVE_RELATIONS_EVENT:
                createdInteraction = new ImproveRelationsEvent(interactable);
                break;
            case INTERACTION_TYPE.RECRUITER_CHARACTER_ENCOUNTER:
                createdInteraction = new RecruiterCharacterEncounter(interactable);
                break;
            case INTERACTION_TYPE.UNABLE_TO_PERFORM:
                createdInteraction = new UnableToPerform(interactable);
                break;
            case INTERACTION_TYPE.PATROL_ACTION:
                createdInteraction = new PatrolAction(interactable);
                break;
            case INTERACTION_TYPE.CHARACTER_FLEES:
                createdInteraction = new CharacterFlees(interactable);
                break;
            case INTERACTION_TYPE.USE_ITEM_ON_CHARACTER:
                createdInteraction = new UseItemOnCharacter(interactable);
                break;
            case INTERACTION_TYPE.USE_ITEM_ON_SELF:
                createdInteraction = new UseItemOnSelf(interactable);
                break;
        }
        return createdInteraction;
    }
    public bool CanCreateInteraction(INTERACTION_TYPE interactionType, BaseLandmark landmark) {
        int count = 0;
        FactionRelationship relationship = null;
        switch (interactionType) {
            case INTERACTION_TYPE.SPAWN_CHARACTER:
            case INTERACTION_TYPE.SPAWN_NEUTRAL_CHARACTER:
                return landmark.tileLocation.areaOfTile.areaResidents.Count < landmark.tileLocation.areaOfTile.residentCapacity && landmark.tileLocation.areaOfTile.race.race != RACE.NONE;
            case INTERACTION_TYPE.BANDIT_RAID:
                //Random event that occurs on Bandit Camps. Requires at least 3 characters or army units in the Bandit Camp 
                //character list owned by the Faction owner.
                return landmark.GetIdleResidents().Count >= 3;
            case INTERACTION_TYPE.MOVE_TO_ATTACK:
                Area target = GetAttackTarget(landmark.tileLocation.areaOfTile);
                return target != null;
            case INTERACTION_TYPE.MINION_PEACE_NEGOTIATION:
                if(landmark.tileLocation.areaOfTile.owner.id != PlayerManager.Instance.player.playerFaction.id) {
                    relationship = PlayerManager.Instance.player.playerFaction.GetRelationshipWith(landmark.tileLocation.areaOfTile.owner);
                    if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY && landmark.tileLocation.areaOfTile.owner.leader.specificLocation.tileLocation.areaOfTile.id == landmark.tileLocation.areaOfTile.id) {
                        return true;
                    }
                }
                
                return false;
            case INTERACTION_TYPE.DEFENSE_MOBILIZATION:
                if(landmark.tileLocation.areaOfTile.defenderGroups.Count < landmark.tileLocation.areaOfTile.maxDefenderGroups) {
                    int idleCharactersCount = 0;
                    for (int i = 0; i < landmark.tileLocation.areaOfTile.areaResidents.Count; i++) {
                        Character resident = landmark.tileLocation.areaOfTile.areaResidents[i];
                        if (resident.IsInOwnParty() && !resident.isLeader && !resident.isDefender && !resident.currentParty.icon.isTravelling && resident.role.roleType != CHARACTER_ROLE.CIVILIAN && resident.specificLocation.tileLocation.areaOfTile.id == landmark.tileLocation.areaOfTile.id) {
                            idleCharactersCount++;
                            if (idleCharactersCount >= 4) {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                return false;
            case INTERACTION_TYPE.MYSTERIOUS_SARCOPHAGUS:
                return landmark.specificLocation.tileLocation.areaOfTile.name == "Tessellated Triangle" || landmark.specificLocation.tileLocation.areaOfTile.name == "Gloomhollow Crypts";
            case INTERACTION_TYPE.SPY_SPAWN_INTERACTION_1:
                count = 0;
                for (int i = 0; i < landmark.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
                    Character character = landmark.tileLocation.areaOfTile.charactersAtLocation[i];
                    if(character.faction.id != PlayerManager.Instance.player.playerFaction.id) {
                        count++;
                        if(count >= 2) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.SPY_SPAWN_INTERACTION_2:
                count = 0;
                for (int i = 0; i < landmark.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
                    Character character = landmark.tileLocation.areaOfTile.charactersAtLocation[i];
                    if (character.faction.id != PlayerManager.Instance.player.playerFaction.id) {
                        count++;
                        if (count >= 3) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.SPY_SPAWN_INTERACTION_3:
                for (int i = 0; i < landmark.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
                    Character character = landmark.tileLocation.areaOfTile.charactersAtLocation[i];
                    if (character.faction.id != PlayerManager.Instance.player.playerFaction.id) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.SPY_SPAWN_INTERACTION_4:
                for (int i = 0; i < landmark.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
                    Character character = landmark.tileLocation.areaOfTile.charactersAtLocation[i];
                    if (character.faction.id != PlayerManager.Instance.player.playerFaction.id && character.faction.id != FactionManager.Instance.neutralFaction.id && character.homeLandmark.tileLocation.areaOfTile.id != landmark.tileLocation.areaOfTile.id) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.EXPLORER_SPAWN_INTERACTION_1:
                return landmark.tileLocation.areaOfTile.possibleSpecialTokenSpawns.Count > 0;
            default:
                return true;
        }
    }
    public bool CanCreateInteraction(INTERACTION_TYPE interactionType, Character character) {
        Area area = null;
        FactionRelationship relationship = null;
        switch (interactionType) {
            case INTERACTION_TYPE.RETURN_HOME:
                return character.specificLocation.tileLocation.areaOfTile.id != character.homeLandmark.tileLocation.areaOfTile.id;
            case INTERACTION_TYPE.CHARACTER_TRACKING:
                return character.specificLocation != character.homeLandmark;
            case INTERACTION_TYPE.MOVE_TO_SCAVENGE:
                //check if there are any unowned areas
                if (character.race == RACE.FAERY || character.race == RACE.HUMANS || character.race == RACE.ELVES || character.race == RACE.GOBLIN) {
                    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                        Area currArea = LandmarkManager.Instance.allAreas[i];
                        if (currArea.owner == null) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_RAID:
                //check if there are any areas owned by factions other than your own
                if(character.race == RACE.GOBLIN || character.race == RACE.SKELETON || character.race == RACE.HUMANS) {
                    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                        Area currArea = LandmarkManager.Instance.allAreas[i];
                        if (currArea.owner != null 
                            && currArea.owner.isActive 
                            && currArea.owner.id != character.specificLocation.tileLocation.areaOfTile.owner.id && currArea.owner.id != character.faction.id) {
                            relationship = character.specificLocation.tileLocation.areaOfTile.owner.GetRelationshipWith(currArea.owner);
                            if (relationship.relationshipStatus != FACTION_RELATIONSHIP_STATUS.ALLY && relationship.relationshipStatus != FACTION_RELATIONSHIP_STATUS.FRIEND) {
                                return true;
                            }
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.INDUCE_WAR:
                if (character.specificLocation.tileLocation.landmarkOnTile.owner != null) {
                    return character.specificLocation.tileLocation.landmarkOnTile.owner.GetFactionsWithRelationship(FACTION_RELATIONSHIP_STATUS.DISLIKED).Count > 0;
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_PEACE_NEGOTIATION:
                if (character.specificLocation.tileLocation.landmarkOnTile.owner != null) {
                    foreach (KeyValuePair<Faction, FactionRelationship> keyValuePair in character.specificLocation.tileLocation.landmarkOnTile.owner.relationships) {
                        if (keyValuePair.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY && keyValuePair.Value.currentWarCombatCount >= 3) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_EXPAND:
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    area = LandmarkManager.Instance.allAreas[i];
                    if (area.id != character.specificLocation.tileLocation.areaOfTile.id && area.owner == null && area.possibleOccupants.Contains(character.race)) {
                        return true;
                    }
                }

                ///* - the character is at his home Area
                // * - the Area has at least 100 Supply
                // * - there is an unoccupied Area that is compatible with the character's race
                // * - ensure that no other active Expand event targets the same location. */
                //if (character.homeLandmark.tileLocation.areaOfTile.id == character.specificLocation.tileLocation.areaOfTile.id) {
                //    Area homeArea = character.homeLandmark.tileLocation.areaOfTile;
                //    if (homeArea.suppliesInBank >= 100) {
                //        List<Area> expansionTargets = homeArea.GetElligibleExpansionTargets(character);
                //        if (expansionTargets.Count > 0) {
                //            return true;
                //        }
                //    }
                //}
                return false;
            case INTERACTION_TYPE.FACTION_UPGRADE:
                return character.specificLocation.tileLocation.areaOfTile.id == character.homeLandmark.tileLocation.areaOfTile.id && character.specificLocation.tileLocation.areaOfTile.suppliesInBank >= 100;
            case INTERACTION_TYPE.WORK_EVENT:
                //if character is at home, allow
                return character.specificLocation.tileLocation.areaOfTile.id == character.homeLandmark.tileLocation.areaOfTile.id;
            case INTERACTION_TYPE.INDUCE_GRUDGE:
                Area targetArea = character.specificLocation.tileLocation.areaOfTile;
                for (int i = 0; i < targetArea.areaResidents.Count; i++) {
                    Character resident = targetArea.areaResidents[i];
                    if(!resident.alreadyTargetedByGrudge && !resident.isDefender && (resident.race == RACE.HUMANS || resident.race == RACE.ELVES || resident.race == RACE.GOBLIN) && resident.specificLocation.tileLocation.areaOfTile.id == targetArea.id) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MYSTERIOUS_SARCOPHAGUS:
                return character.specificLocation.tileLocation.areaOfTile.name == "Tessellated Triangle" || character.specificLocation.tileLocation.areaOfTile.name == "Gloomhollow Crypts";
            case INTERACTION_TYPE.INFLICT_ILLNESS:
                /*You can inflict a random illness on a character. Trigger requirements:
                - there must be at least one character in the location
                - the player must have intel of at least one of these characters*/
                area = character.specificLocation.tileLocation.areaOfTile;
                List<Character> choices = new List<Character>(area.charactersAtLocation);
                choices.Remove(character);
                for (int i = 0; i < choices.Count; i++) {
                    Character currCharacter = choices[i];
                    if (currCharacter.characterToken.isObtainedByPlayer) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_IMPROVE_RELATIONS:
                //check if there are any areas owned by factions other than your own
                if(character.race == RACE.ELVES || character.race == RACE.HUMANS) {
                    for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                        Faction faction = FactionManager.Instance.allFactions[i];
                        if (faction.id != PlayerManager.Instance.player.playerFaction.id && faction.id != character.faction.id && faction.isActive) {
                            relationship = character.faction.GetRelationshipWith(faction);
                            if (relationship.relationshipStatus != FACTION_RELATIONSHIP_STATUS.ALLY) {
                                return true;

                            }
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_RECRUIT:
                if (character.race == RACE.ELVES || character.race == RACE.HUMANS) {
                    if (character.homeLandmark.tileLocation.areaOfTile.IsResidentsFull()) { //check if resident capacity is full
                        return false;
                    }
                    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                        Character currCharacter = CharacterManager.Instance.allCharacters[i];
                        if (currCharacter.id != character.id) {
                            if (currCharacter.faction == null) {
                                //Unaligned?
                                return true;
                            } else {
                                if (currCharacter.faction.id != character.faction.id) {
                                    relationship = currCharacter.faction.GetRelationshipWith(character.faction);
                                    if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL
                                        || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_RETURN_HOME:
                //if character is NOT at home, allow
                return character.specificLocation.tileLocation.areaOfTile.id != character.homeLandmark.tileLocation.areaOfTile.id;
            case INTERACTION_TYPE.MOVE_TO_EXPLORE:
                if(character.tokenInInventory == null) {
                    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                        area = LandmarkManager.Instance.allAreas[i];
                        if (area.id != character.specificLocation.tileLocation.areaOfTile.id && (area.owner == null || (area.owner != null && area.owner.id != character.faction.id && area.owner.id != PlayerManager.Instance.player.playerFaction.id))) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_CHARM:
                if (character.race == RACE.FAERY || character.race == RACE.ELVES) {
                    if (!character.homeLandmark.tileLocation.areaOfTile.IsResidentsFull()) {
                        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                            Character currCharacter = CharacterManager.Instance.allCharacters[i];
                            if(currCharacter.id != character.id) {
                                if(currCharacter.faction == null) {
                                    //Unaligned?
                                    return true;
                                } else {
                                    if(currCharacter.faction.id != character.faction.id) {
                                        relationship = currCharacter.faction.GetRelationshipWith(character.faction);
                                        if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL
                                            || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_ABDUCT:
                if (character.race == RACE.GOBLIN || character.race == RACE.SPIDER) {
                    if (!character.homeLandmark.tileLocation.areaOfTile.IsResidentsFull()) {
                        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                            Character currCharacter = CharacterManager.Instance.allCharacters[i];
                            if (currCharacter.id != character.id) {
                                if (currCharacter.faction == null || currCharacter.faction.id != character.faction.id) {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_STEAL:
                if (character.race == RACE.GOBLIN || character.race == RACE.SKELETON || character.race == RACE.FAERY || character.race == RACE.DRAGON) {
                    if (character.tokenInInventory == null) {
                        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                            Character currCharacter = CharacterManager.Instance.allCharacters[i];
                            if (currCharacter.id != character.id && currCharacter.tokenInInventory != null) {
                                if (currCharacter.faction == null || currCharacter.faction.id != character.faction.id) {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_HUNT:
                return character.race == RACE.WOLF || character.race == RACE.SPIDER || character.race == RACE.DRAGON;
            default:
                return true;
        }
    }
    private Area GetAttackTarget(Area areaToAttack) {
        Area targetArea = null;
        List<Faction> enemyFaction = new List<Faction>();
        List<Area> enemyAreas = new List<Area>();
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in areaToAttack.owner.relationships) {
            FactionRelationship factionRelationship = kvp.Value;
            if(factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                enemyFaction.Add(kvp.Key);
                enemyAreas.AddRange(kvp.Key.ownedAreas);
            }
        }
        if(enemyFaction.Count > 0) {
            //If at war with other factions
            List<Character> residentsAtArea = new List<Character>();
            for (int i = 0; i < areaToAttack.areaResidents.Count; i++) {
                Character resident = areaToAttack.areaResidents[i];
                if(resident.IsInOwnParty() && !resident.isLeader && resident.role.roleType != CHARACTER_ROLE.CIVILIAN && !resident.currentParty.icon.isTravelling && !resident.isDefender && resident.specificLocation.tileLocation.areaOfTile.id == areaToAttack.id) {
                    residentsAtArea.Add(resident);
                }
            }
            if(residentsAtArea.Count >= 3) {
                //If has at least 3 residents in area
                int numOfMembers = 3;
                if(residentsAtArea.Count >= 4) {
                    numOfMembers = 4;
                }
                List<List<Character>> characterCombinations = Utilities.ItemCombinations(residentsAtArea, 5, numOfMembers, numOfMembers);
                if(characterCombinations.Count > 0) {
                    List<Character> currentAttackCharacters = null;
                    Area currentTargetArea = null;
                    float highestWinChance = 0f;
                    for (int i = 0; i < characterCombinations.Count; i++) {
                        List<Character> attackCharacters = characterCombinations[i];
                        Area target = enemyAreas[UnityEngine.Random.Range(0, enemyAreas.Count)];
                        DefenderGroup defender = target.GetFirstDefenderGroup();
                        float winChance = 0f;
                        float loseChance = 0f;
                        if(defender != null) {
                            CombatManager.Instance.GetCombatChanceOfTwoLists(attackCharacters, defender.party.characters, out winChance, out loseChance);
                        } else {
                            CombatManager.Instance.GetCombatChanceOfTwoLists(attackCharacters, null, out winChance, out loseChance);
                        }
                        if (winChance > 40f) {
                            if(currentTargetArea == null) {
                                currentTargetArea = target;
                                currentAttackCharacters = attackCharacters;
                                highestWinChance = winChance;
                            } else {
                                if(winChance > highestWinChance) {
                                    currentTargetArea = target;
                                    currentAttackCharacters = attackCharacters;
                                    highestWinChance = winChance;
                                }
                            }
                        }
                    }
                    targetArea = currentTargetArea;
                    areaToAttack.SetAttackTargetAndCharacters(currentTargetArea, currentAttackCharacters);
                }
            }
        }
        return targetArea;
    }
    public Reward GetReward(string rewardName) {
        if (rewardConfig.ContainsKey(rewardName)) {
            RewardConfig config = rewardConfig[rewardName];
            return new Reward { rewardType = config.rewardType, amount = Random.Range(config.lowerRange, config.higherRange + 1) };
        }
        throw new System.Exception("There is no reward configuration with name " + rewardName);
    }

    public List<CharacterInteractionWeight> GetDefaultInteractionWeightsForRole(CHARACTER_ROLE role) {
        if (roleDefaultInteractions.ContainsKey(role)) {
            return roleDefaultInteractions[role];
        }
        return null;
    }
    public List<CharacterInteractionWeight> GetJobNPCInteractionWeights(JOB jobType) {
        if (jobNPCInteractions.ContainsKey(jobType)) {
            return jobNPCInteractions[jobType];
        }
        return null;
    }
    public void OnClickInteraction(Interaction interaction) {
        if(interaction != null) {
            interaction.CancelSecondTimeOut();
            InteractionUI.Instance.OpenInteractionUI(interaction);
        }
    }
    public void AddToInteractionQueue(Interaction interaction) {
        interactionUIQueue.Enqueue(interaction);
    }

    //public List<T> GetAllCurrentInteractionsOfType<T>(INTERACTION_TYPE type) {
    //    List<T> interactionsOfType = new List<T>();
    //    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
    //        Area currArea = LandmarkManager.Instance.allAreas[i];
    //        for (int j = 0; j < currArea.currentInteractions.Count; j++) {
    //            Interaction currInteraction = currArea.currentInteractions[j];
    //            if (currInteraction.type == type && currInteraction is T) {
    //                interactionsOfType.Add((T)System.Convert.ChangeType(currInteraction, typeof(T)));
    //            }
    //        }
    //    }
    //    return interactionsOfType;
    //}

    public void UnlockAllTokens() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (!currCharacter.isDefender) {
                PlayerManager.Instance.player.AddToken(currCharacter.characterToken);
            }
        }
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            PlayerManager.Instance.player.AddToken(currArea.locationToken);
            PlayerManager.Instance.player.AddToken(currArea.defenderToken);
        }
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction currFaction = FactionManager.Instance.allFactions[i];
            PlayerManager.Instance.player.AddToken(currFaction.factionToken);
        }
    }
}

public struct RewardConfig {
    public REWARD rewardType;
    public int lowerRange;
    public int higherRange;
}
public struct Reward {
    public REWARD rewardType;
    public int amount;
}
[System.Serializable]
public struct CharacterInteractionWeight {
    public INTERACTION_TYPE interactionType;
    public int weight;
}