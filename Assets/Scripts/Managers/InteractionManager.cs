
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
            case INTERACTION_TYPE.RETURN_HOME:
                createdInteraction = new ReturnHome(interactable);
                break;
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
            case INTERACTION_TYPE.FACTION_UPGRADE:
                createdInteraction = new FactionUpgrade(interactable);
                break;
        }
        return createdInteraction;
    }
    public bool CanCreateInteraction(INTERACTION_TYPE interactionType, BaseLandmark landmark) {
        switch (interactionType) {
            case INTERACTION_TYPE.SPAWN_CHARACTER:
            case INTERACTION_TYPE.SPAWN_NEUTRAL_CHARACTER:
                return landmark.tileLocation.areaOfTile.areaResidents.Count < landmark.tileLocation.areaOfTile.residentCapacity;
            case INTERACTION_TYPE.BANDIT_RAID:
                //Random event that occurs on Bandit Camps. Requires at least 3 characters or army units in the Bandit Camp 
                //character list owned by the Faction owner.
                return landmark.GetIdleResidents().Count >= 3;
            case INTERACTION_TYPE.MOVE_TO_ATTACK:
                Area target = GetAttackTarget(landmark.tileLocation.areaOfTile);
                return target != null;
            case INTERACTION_TYPE.MINION_PEACE_NEGOTIATION:
                FactionRelationship relationship = PlayerManager.Instance.player.playerFaction.GetRelationshipWith(landmark.tileLocation.areaOfTile.owner);
                if(relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.AT_WAR && landmark.tileLocation.areaOfTile.owner.leader.specificLocation.tileLocation.areaOfTile.id == landmark.tileLocation.areaOfTile.id) {
                    return true;
                }
                return false;
            default:
                return true;
        }
    }
    public bool CanCreateInteraction(INTERACTION_TYPE interactionType, Character character) {
        switch (interactionType) {
            case INTERACTION_TYPE.RETURN_HOME:
            case INTERACTION_TYPE.CHARACTER_TRACKING:
                return character.specificLocation != character.homeLandmark;
            case INTERACTION_TYPE.MOVE_TO_SCAVENGE:
                //check if there are any unowned areas
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    Area currArea = LandmarkManager.Instance.allAreas[i];
                    if (currArea.owner == null) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_RAID:
                //check if there are any areas owned by factions other than your own
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    Area currArea = LandmarkManager.Instance.allAreas[i];
                    if (currArea.owner != null && currArea.owner.id != character.faction.id) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.INDUCE_WAR:
                if (character.specificLocation.tileLocation.landmarkOnTile.owner != null) {
                    foreach (KeyValuePair<Faction, int> kvp in character.specificLocation.tileLocation.landmarkOnTile.owner.favor) {
                        if (kvp.Key.id != PlayerManager.Instance.player.playerFaction.id
                            && kvp.Value <= -10 && character.specificLocation.tileLocation.landmarkOnTile.owner.GetRelationshipWith(kvp.Key).relationshipStatus != FACTION_RELATIONSHIP_STATUS.AT_WAR) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_PEACE_NEGOTIATION:
                if (character.specificLocation.tileLocation.landmarkOnTile.owner != null) {
                    foreach (KeyValuePair<Faction, FactionRelationship> keyValuePair in character.specificLocation.tileLocation.landmarkOnTile.owner.relationships) {
                        if (keyValuePair.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.AT_WAR && keyValuePair.Value.currentWarCombatCount >= 3) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.FACTION_UPGRADE:
                return character.specificLocation.tileLocation.areaOfTile.id == character.homeLandmark.tileLocation.areaOfTile.id && character.specificLocation.tileLocation.areaOfTile.suppliesInBank >= 100;
            default:
                return true;
        }
    }
    private Area GetAttackTarget(Area areaToAttack) {
        Area targetArea = null;
        List<Faction> enemyFaction = new List<Faction>();
        List<Area> enemyAreas = new List<Area>();
        foreach (Faction otherFaction in areaToAttack.owner.relationships.Keys) {
            FactionRelationship factionRelationship = areaToAttack.owner.relationships[otherFaction];
            if(factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.AT_WAR) {
                enemyFaction.Add(otherFaction);
                enemyAreas.AddRange(otherFaction.ownedAreas);
            }
        }
        if(enemyFaction.Count > 0) {
            //If at war with other factions
            List<Character> residentsAtArea = new List<Character>();
            for (int i = 0; i < areaToAttack.areaResidents.Count; i++) {
                Character resident = areaToAttack.areaResidents[i];
                if(resident.specificLocation.tileLocation.areaOfTile.id == areaToAttack.id) {
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
                        if (winChance > 30f) {
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

    public void UnlockAllIntel() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (!currCharacter.isDefender) {
                PlayerManager.Instance.player.AddIntel(currCharacter.characterIntel);
            }
        }
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            PlayerManager.Instance.player.AddIntel(currArea.locationIntel);
            PlayerManager.Instance.player.AddIntel(currArea.defenderIntel);
        }
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction currFaction = FactionManager.Instance.allFactions[i];
            PlayerManager.Instance.player.AddIntel(currFaction.factionIntel);
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