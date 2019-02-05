using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private string dailyInteractionSummary;

    public Dictionary<INTERACTION_TYPE, InteractionAttributes> interactionCategoryAndAlignment { get; private set; }

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
    public void Initialize() {
        Messenger.AddListener(Signals.DAY_ENDED_2, TryExecuteInteractionsDefault);
        ConstructInteractionCategoryAndAlignment();
    }

    #region Interaction Category And Alignment
    private void ConstructInteractionCategoryAndAlignment() {
        interactionCategoryAndAlignment = new Dictionary<INTERACTION_TYPE, InteractionAttributes>() {
            { INTERACTION_TYPE.EAT_DEFENSELESS, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.OFFENSE },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.MOVE_TO_CHARM_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.CHARM_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.MOVE_TO_EXPLORE_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.EXPLORE_EVENT_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.MOVE_TO_ABDUCT, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT, INTERACTION_CATEGORY.SUBTERFUGE },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.ABDUCT_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT, INTERACTION_CATEGORY.SUBTERFUGE },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.MOVE_TO_ARGUE, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.SOCIAL },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.ARGUE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.SOCIAL },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.MOVE_TO_CURSE, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.SUBTERFUGE },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.CURSE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.SUBTERFUGE },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.MOVE_TO_HUNT, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.OFFENSE },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.HUNT_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.OFFENSE },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.MOVE_TO_LOOT, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.LOOT_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.MOVE_TO_TAME_BEAST, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.TAME_BEAST_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.TORTURE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.CRAFT_ITEM, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.MOVE_TO_RAID_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.RAID_EVENT_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.MOVE_TO_STEAL_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.STEAL_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.MOVE_TO_RECRUIT_FRIEND_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.GOOD,
            } },
            { INTERACTION_TYPE.RECRUIT_FRIEND_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.MOVE_TO_ASSASSINATE_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.ASSASSINATE_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.MOVE_TO_REANIMATE, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.REANIMATE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.MOVE_TO_SCAVENGE_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.SCAVENGE_EVENT_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.BERSERK_ATTACK, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.OFFENSE },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.MOVE_TO_OCCUPY_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.EXPANSION },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.OCCUPY_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
             { INTERACTION_TYPE.MOVE_TO_MINE, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY },
                alignment = INTERACTION_ALIGNMENT.GOOD,
            } },
             { INTERACTION_TYPE.MINE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.MOVE_TO_HARVEST, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY },
                alignment = INTERACTION_ALIGNMENT.GOOD,
            } },
            { INTERACTION_TYPE.HARVEST_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.SCRAP_ITEM, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.PATROL_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.DEFENSE },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
            { INTERACTION_TYPE.CONSUME_LIFE, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY },
                alignment = INTERACTION_ALIGNMENT.EVIL,
            } },
            { INTERACTION_TYPE.MOVE_TO_RETURN_HOME, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            } },
        };
    }
    public InteractionAttributes GetCategoryAndAlignment (INTERACTION_TYPE type) {
        if (interactionCategoryAndAlignment.ContainsKey(type)) {
            return interactionCategoryAndAlignment[type];
        }
        throw new System.Exception("No category and alignment for " + type.ToString());
        //return null;
    }
    #endregion

    public Interaction CreateNewInteraction(INTERACTION_TYPE interactionType, Area interactable) {
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
            case INTERACTION_TYPE.MINION_RECRUIT_CHARACTER:
                createdInteraction = new MinionRecruitCharacter(interactable);
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
            case INTERACTION_TYPE.DROP_ITEM:
                createdInteraction = new DropItem(interactable);
                break;
            case INTERACTION_TYPE.PICK_ITEM:
                createdInteraction = new PickItem(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_CHARM:
                createdInteraction = new MoveToCharm(interactable);
                break;
            case INTERACTION_TYPE.CHARM_ACTION:
                createdInteraction = new CharmAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_STEAL:
                createdInteraction = new MoveToSteal(interactable);
                break;
            case INTERACTION_TYPE.STEAL_ACTION:
                createdInteraction = new StealAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_ABDUCT:
                createdInteraction = new MoveToAbduct(interactable);
                break;
            case INTERACTION_TYPE.ABDUCT_ACTION:
                createdInteraction = new AbductAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_HUNT:
                createdInteraction = new MoveToHunt(interactable);
                break;
            case INTERACTION_TYPE.HUNT_ACTION:
                createdInteraction = new HuntAction(interactable);
                break;
            case INTERACTION_TYPE.USE_ITEM_ON_LOCATION:
                createdInteraction = new UseItemOnLocation(interactable);
                break;
            case INTERACTION_TYPE.EAT_DEFENSELESS:
                createdInteraction = new EatDefenseless(interactable);
                break;
            case INTERACTION_TYPE.TORTURE_ACTION:
                createdInteraction = new TortureAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_REANIMATE:
                createdInteraction = new MoveToReanimate(interactable);
                break;
            case INTERACTION_TYPE.REANIMATE_ACTION:
                createdInteraction = new ReanimateAction(interactable);
                break;
            case INTERACTION_TYPE.CHANCE_ENCOUNTER:
                createdInteraction = new ChanceEncounter(interactable);
                break;
            case INTERACTION_TYPE.FOUND_LUCARETH:
                createdInteraction = new FoundLucareth(interactable);
                break;
            case INTERACTION_TYPE.FOUND_BESTALIA:
                createdInteraction = new FoundBestalia(interactable);
                break;
            case INTERACTION_TYPE.FOUND_MAGUS:
                createdInteraction = new FoundMagus(interactable);
                break;
            case INTERACTION_TYPE.FOUND_ZIRANNA:
                createdInteraction = new FoundZiranna(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_SAVE:
                createdInteraction = new MoveToSave(interactable);
                break;
            case INTERACTION_TYPE.SAVE_ACTION:
                createdInteraction = new SaveAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_VISIT:
                createdInteraction = new MoveToVisit(interactable);
                break;
            case INTERACTION_TYPE.TRANSFER_HOME:
                createdInteraction = new TransferHome(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_STEAL_FACTION:
                createdInteraction = new MoveToStealFaction(interactable);
                break;
            case INTERACTION_TYPE.STEAL_ACTION_FACTION:
                createdInteraction = new StealActionFaction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_RECRUIT_FRIEND_FACTION:
                createdInteraction = new MoveToRecruitFriendFaction(interactable);
                break;
            case INTERACTION_TYPE.RECRUIT_FRIEND_ACTION_FACTION:
                createdInteraction = new RecruitFriendActionFaction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_ASSASSINATE_FACTION:
                createdInteraction = new MoveToAssassinateFaction(interactable);
                break;
            case INTERACTION_TYPE.ASSASSINATE_ACTION_FACTION:
                createdInteraction = new AssassinateActionFaction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_LOOT:
                createdInteraction = new MoveToLoot(interactable);
                break;
            case INTERACTION_TYPE.LOOT_ACTION:
                createdInteraction = new LootAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_TAME_BEAST:
                createdInteraction = new MoveToTameBeast(interactable);
                break;
            case INTERACTION_TYPE.TAME_BEAST_ACTION:
                createdInteraction = new TameBeastAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_HANG_OUT:
                createdInteraction = new MoveToHangOut(interactable);
                break;
            case INTERACTION_TYPE.HANG_OUT_ACTION:
                createdInteraction = new HangOutAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_CHARM_FACTION:
                createdInteraction = new MoveToCharmFaction(interactable);
                break;
            case INTERACTION_TYPE.CHARM_ACTION_FACTION:
                createdInteraction = new CharmActionFaction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_ARGUE:
                createdInteraction = new MoveToArgue(interactable);
                break;
            case INTERACTION_TYPE.ARGUE_ACTION:
                createdInteraction = new ArgueAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_CURSE:
                createdInteraction = new MoveToCurse(interactable);
                break;
            case INTERACTION_TYPE.CURSE_ACTION:
                createdInteraction = new CurseAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_SCAVENGE_FACTION:
                createdInteraction = new MoveToScavengeFaction(interactable);
                break;
            case INTERACTION_TYPE.SCAVENGE_EVENT_FACTION:
                createdInteraction = new ScavengeEventFaction(interactable);
                break;
            case INTERACTION_TYPE.CRAFT_ITEM:
                createdInteraction = new CraftItem(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_RAID_FACTION:
                createdInteraction = new MoveToRaidFaction(interactable);
                break;
            case INTERACTION_TYPE.RAID_EVENT_FACTION:
                createdInteraction = new RaidEventFaction(interactable);
                break;
            case INTERACTION_TYPE.BERSERK_ATTACK:
                createdInteraction = new BerserkAttack(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_OCCUPY_FACTION:
                createdInteraction = new MoveToOccupyFaction(interactable);
                break;
            case INTERACTION_TYPE.OCCUPY_ACTION_FACTION:
                createdInteraction = new OccupyActionFaction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_MINE:
                createdInteraction = new MoveToMine(interactable);
                break;
            case INTERACTION_TYPE.MINE_ACTION:
                createdInteraction = new MineAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_HARVEST:
                createdInteraction = new MoveToHarvest(interactable);
                break;
            case INTERACTION_TYPE.HARVEST_ACTION:
                createdInteraction = new HarvestAction(interactable);
                break;
            case INTERACTION_TYPE.SCRAP_ITEM:
                createdInteraction = new ScrapItem(interactable);
                break;
            case INTERACTION_TYPE.CONSUME_LIFE:
                createdInteraction = new ConsumeLife(interactable);
                break;
            case INTERACTION_TYPE.PATROL_ACTION_FACTION:
                createdInteraction = new PatrolActionFaction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_EXPLORE_FACTION:
                createdInteraction = new MoveToExploreFaction(interactable);
                break;
            case INTERACTION_TYPE.EXPLORE_EVENT_FACTION:
                createdInteraction = new ExploreEventFaction(interactable);
                break;
            case INTERACTION_TYPE.ASK_FOR_HELP:
                createdInteraction = new AskForHelp(interactable);
                break;
        }
        return createdInteraction;
    }
    public bool CanCreateInteraction(INTERACTION_TYPE interactionType, Area location) {
        int count = 0;
        FactionRelationship relationship = null;
        switch (interactionType) {
            case INTERACTION_TYPE.SPAWN_CHARACTER:
            case INTERACTION_TYPE.SPAWN_NEUTRAL_CHARACTER:
                return !location.IsResidentsFull() && location.raceType != RACE.NONE;
            //case INTERACTION_TYPE.BANDIT_RAID:
            //    //Random event that occurs on Bandit Camps. Requires at least 3 characters or army units in the Bandit Camp 
            //    //character list owned by the Faction owner.
            //    return landmark.GetIdleResidents().Count >= 3;
            case INTERACTION_TYPE.MOVE_TO_ATTACK:
                Area target = GetAttackTarget(location);
                return target != null;
            case INTERACTION_TYPE.MINION_PEACE_NEGOTIATION:
                if(location.owner.id != PlayerManager.Instance.player.playerFaction.id) {
                    relationship = PlayerManager.Instance.player.playerFaction.GetRelationshipWith(location.owner);
                    if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY && location.owner.leader.specificLocation.id == location.id) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.DEFENSE_MOBILIZATION:
                if(location.defenderGroups.Count < location.maxDefenderGroups) {
                    int idleCharactersCount = 0;
                    for (int i = 0; i < location.areaResidents.Count; i++) {
                        Character resident = location.areaResidents[i];
                        if (resident.forcedInteraction == null && resident.doNotDisturb <= 0 && resident.IsInOwnParty() && !resident.isLeader && !resident.isDefender && !resident.currentParty.icon.isTravelling && resident.role.roleType != CHARACTER_ROLE.CIVILIAN && resident.specificLocation.id == location.id) {
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
                return location.name == "Tessellated Triangle" || location.name == "Gloomhollow Crypts";
            case INTERACTION_TYPE.SPY_SPAWN_INTERACTION_1:
                count = 0;
                for (int i = 0; i < location.charactersAtLocation.Count; i++) {
                    Character character = location.charactersAtLocation[i];
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
                for (int i = 0; i < location.charactersAtLocation.Count; i++) {
                    Character character = location.charactersAtLocation[i];
                    if (character.faction.id != PlayerManager.Instance.player.playerFaction.id) {
                        count++;
                        if (count >= 3) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.SPY_SPAWN_INTERACTION_3:
                for (int i = 0; i < location.charactersAtLocation.Count; i++) {
                    Character character = location.charactersAtLocation[i];
                    if (character.faction.id != PlayerManager.Instance.player.playerFaction.id) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.SPY_SPAWN_INTERACTION_4:
                for (int i = 0; i < location.charactersAtLocation.Count; i++) {
                    Character character = location.charactersAtLocation[i];
                    if (character.faction.id != PlayerManager.Instance.player.playerFaction.id && character.faction.id != FactionManager.Instance.neutralFaction.id && character.homeArea.id != location.id) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.EXPLORER_SPAWN_INTERACTION_1:
                return location.possibleSpecialTokenSpawns.Count > 0;
            default:
                return true;
        }
    }
    public bool CanCreateInteraction(INTERACTION_TYPE interactionType, Character character) {
        Area area = null;
        FactionRelationship relationship = null;
        switch (interactionType) {
            case INTERACTION_TYPE.RETURN_HOME:
                return character.specificLocation.id != character.homeArea.id;
            case INTERACTION_TYPE.CHARACTER_TRACKING:
                return character.specificLocation.id != character.homeArea.id;
            case INTERACTION_TYPE.INDUCE_WAR:
                if (character.specificLocation.owner != null) {
                    return character.specificLocation.owner.GetFactionsWithRelationship(FACTION_RELATIONSHIP_STATUS.DISLIKED).Count > 0;
                }
                return false;
            case INTERACTION_TYPE.FACTION_UPGRADE:
                return character.specificLocation.id == character.homeArea.id && character.specificLocation.suppliesInBank >= 100;
            case INTERACTION_TYPE.WORK_EVENT:
                //if character is at home, allow
                return character.specificLocation.id == character.homeArea.id;
            case INTERACTION_TYPE.INDUCE_GRUDGE:
                Area targetArea = character.specificLocation;
                for (int i = 0; i < targetArea.areaResidents.Count; i++) {
                    Character resident = targetArea.areaResidents[i];
                    if(resident.forcedInteraction == null && resident.doNotDisturb <= 0 && !resident.currentParty.icon.isTravelling && 
                    !resident.alreadyTargetedByGrudge && !resident.isDefender && (resident.race == RACE.HUMANS || resident.race == RACE.ELVES || resident.race == RACE.GOBLIN) && 
                    resident.specificLocation.id == targetArea.id) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MYSTERIOUS_SARCOPHAGUS:
                return character.specificLocation.name == "Tessellated Triangle" || character.specificLocation.name == "Gloomhollow Crypts";
            case INTERACTION_TYPE.INFLICT_ILLNESS:
                /*You can inflict a random illness on a character. Trigger requirements:
                - there must be at least one character in the location
                - the player must have intel of at least one of these characters*/
                area = character.specificLocation;
                List<Character> choices = new List<Character>(area.charactersAtLocation);
                choices.Remove(character);
                for (int i = 0; i < choices.Count; i++) {
                    Character currCharacter = choices[i];
                    if (currCharacter.characterToken.isObtainedByPlayer) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_SCAVENGE:
                //check if there are any unowned areas
                //if (character.race == RACE.FAERY || character.race == RACE.HUMANS || character.race == RACE.ELVES || character.race == RACE.GOBLIN) {
                    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                        Area currArea = LandmarkManager.Instance.allAreas[i];
                        if (currArea.owner == null) {
                            return true;
                        }
                    }
                //}
                return false;
            case INTERACTION_TYPE.MOVE_TO_RAID:
                //There must be at least one other location that is occupied but not owned by the character's Faction and not owned by an Ally or a Friend faction
                //if (character.race == RACE.GOBLIN || character.race == RACE.SKELETON || character.race == RACE.HUMANS) {
                    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                        Area currArea = LandmarkManager.Instance.allAreas[i];
                        if (currArea.owner != null
                            && currArea.owner.id != character.faction.id
                            && currArea.id != PlayerManager.Instance.player.playerArea.id) {
                            relationship = character.faction.GetRelationshipWith(currArea.owner);
                            if (relationship.relationshipStatus != FACTION_RELATIONSHIP_STATUS.ALLY && relationship.relationshipStatus != FACTION_RELATIONSHIP_STATUS.FRIEND) {
                                return true;
                            }
                        }
                    }
                //}
                return false;
            case INTERACTION_TYPE.MOVE_TO_PEACE_NEGOTIATION:
                if (character.specificLocation.owner != null) {
                    foreach (KeyValuePair<Faction, FactionRelationship> keyValuePair in character.specificLocation.owner.relationships) {
                        if (keyValuePair.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY && keyValuePair.Value.currentWarCombatCount >= 3) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_EXPAND:
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    area = LandmarkManager.Instance.allAreas[i];
                    if (area.id != character.specificLocation.id && area.owner == null && area.possibleOccupants.Contains(character.race)) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_IMPROVE_RELATIONS:
                    //check if there are any areas owned by factions other than your own
                    //if(character.race == RACE.ELVES || character.race == RACE.HUMANS) {
                        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                            Faction faction = FactionManager.Instance.allFactions[i];
                            if (faction.ownedAreas.Count == 0) { //skip factions that don't have owned areas
                                continue; //skip
                            }
                            if (faction.id != PlayerManager.Instance.player.playerFaction.id && faction.id != character.faction.id && faction.isActive) {
                                relationship = character.faction.GetRelationshipWith(faction);
                                if (relationship.relationshipStatus != FACTION_RELATIONSHIP_STATUS.ALLY) {
                                    return true;

                                }
                            }
                        }
                    //}
                    return false;
            case INTERACTION_TYPE.MOVE_TO_RECRUIT:
                //if (character.race == RACE.ELVES || character.race == RACE.HUMANS) {
                    if (character.homeArea.IsResidentsFull()) { //check if resident capacity is full
                        return false;
                    }
                    //**Trigger Criteria 1**: There must be at least one other unaligned character or the character must have a personal friend not from the same faction
                    for (int i = 0; i < character.traits.Count; i++) {
                        Trait trait = character.traits[i];
                        if (trait is Friend) {
                            Friend friend = trait as Friend;
                            if (friend.targetCharacter == null) {
                                throw new System.Exception("target friend is null!");
                            }
                            if (friend.targetCharacter.faction.id != character.faction.id) {
                                return true;
                            }
                        }
                    }
                    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                        Character currCharacter = CharacterManager.Instance.allCharacters[i];
                        if (currCharacter.id != character.id && !currCharacter.isDead) {
                            if (currCharacter.isFactionless) {
                                //Unaligned?
                                return true;
                            } 
                            //else {
                            //    if (currCharacter.faction.id != character.faction.id) {
                            //        relationship = currCharacter.faction.GetRelationshipWith(character.faction);
                            //        if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL
                            //            || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                            //            return true;
                            //        }
                            //    }
                            //}
                        }
                    }
                //}
                return false;
            case INTERACTION_TYPE.MOVE_TO_RETURN_HOME:
                //if character is NOT at home, allow
                return character.specificLocation.id != character.homeArea.id;
            case INTERACTION_TYPE.MOVE_TO_EXPLORE:
                if(character.tokenInInventory == null) {
                    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                        area = LandmarkManager.Instance.allAreas[i];
                        if (area.id != character.specificLocation.id && (area.owner == null || (area.owner != null && area.owner.id != character.faction.id && area.owner.id != PlayerManager.Instance.player.playerFaction.id))) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_CHARM:
                //if (character.race == RACE.FAERY) {
                    if (!character.homeArea.IsResidentsFull()) {
                        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                            Character currCharacter = CharacterManager.Instance.allCharacters[i];
                            if(currCharacter.id != character.id && !currCharacter.isDead) {
                                if(currCharacter.isFactionless) {
                                    //Unaligned?
                                    return true;
                                } else if(currCharacter.faction.id != character.faction.id && currCharacter.faction.ownedAreas.Count > 0) {
                                        relationship = currCharacter.faction.GetRelationshipWith(character.faction);
                                        if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED 
                                            || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL
                                            || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                                            return true;
                                        }
                                }
                            }
                        }
                    }
                //}
                return false;
            case INTERACTION_TYPE.MOVE_TO_ABDUCT:
                //if (character.race == RACE.GOBLIN || character.race == RACE.SPIDER) {
                    if (!character.homeArea.IsResidentsFull()) {
                            for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                                Area currArea = LandmarkManager.Instance.allAreas[i];
                                if (currArea.owner == null || currArea.owner.id != PlayerManager.Instance.player.playerFaction.id && currArea.owner.id != character.faction.id) {
                                    for (int j = 0; j < currArea.charactersAtLocation.Count; j++) {
                                        Character characterAtLocation = currArea.charactersAtLocation[j];
                                        if (characterAtLocation.id != character.id && characterAtLocation.IsInOwnParty() && !characterAtLocation.currentParty.icon.isTravelling
                                            && (characterAtLocation.isFactionless || characterAtLocation.faction.id != character.faction.id)) {
                                            return true;
                                        }
                                    }
                                }
                            }
                            //for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                            //    Character currCharacter = CharacterManager.Instance.allCharacters[i];
                            //    if (currCharacter.id != character.id && !currCharacter.isDead && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty()) {
                            //        if (currCharacter.isFactionless || currCharacter.faction.id != character.faction.id) {
                            //            return true;
                            //        }
                            //    }
                            //}
                        }
                //}
                return false;
            case INTERACTION_TYPE.MOVE_TO_STEAL:
                //if (character.race == RACE.GOBLIN || character.race == RACE.SKELETON || character.race == RACE.FAERY || character.race == RACE.DRAGON) {
                    if (character.tokenInInventory == null) {
                        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                            Character currCharacter = CharacterManager.Instance.allCharacters[i];
                            if (currCharacter.id != character.id && !currCharacter.isDead && currCharacter.tokenInInventory != null) {
                                if (currCharacter.isFactionless || currCharacter.faction.id != character.faction.id) {
                                    return true;
                                }
                            }
                        }
                    }
                //}
                return false;
            case INTERACTION_TYPE.MOVE_TO_STEAL_FACTION:
                //**Trigger Criteria 1**: This character must not have an item
                return character.tokenInInventory == null;
            case INTERACTION_TYPE.MOVE_TO_HUNT:
                //if(character.race == RACE.WOLF || character.race == RACE.SPIDER || character.race == RACE.DRAGON) {
                    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                        area = LandmarkManager.Instance.allAreas[i];
                        if (area.id != character.specificLocation.id && (area.owner == null || (area.owner != null && area.owner.id != character.faction.id && area.owner.id != PlayerManager.Instance.player.playerFaction.id))) {
                            return true;
                        }
                    }
                //}
                return false;
            case INTERACTION_TYPE.MOVE_TO_SAVE:
                return CanCreateMoveToSave(character);
            case INTERACTION_TYPE.FOUND_LUCARETH:
                return character.characterClass.className == "Witch" && character.specificLocation.owner == null 
                    && character.specificLocation.possibleOccupants.Contains(character.race) && !FactionManager.Instance.GetFactionBasedOnName("Lucareth").isActive;
            case INTERACTION_TYPE.FOUND_BESTALIA:
                return character.characterClass.className == "Beastmaster" && character.specificLocation.owner == null
                    && character.specificLocation.possibleOccupants.Contains(character.race) && !FactionManager.Instance.GetFactionBasedOnName("Bestalia").isActive;
            case INTERACTION_TYPE.FOUND_MAGUS:
                return character.characterClass.className == "Archmage" && character.specificLocation.owner == null
                    && character.specificLocation.possibleOccupants.Contains(character.race) && !FactionManager.Instance.GetFactionBasedOnName("Magus").isActive;
            case INTERACTION_TYPE.FOUND_ZIRANNA:
                return character.characterClass.className == "Necromancer" && character.specificLocation.owner == null
                    && character.specificLocation.possibleOccupants.Contains(character.race) && !FactionManager.Instance.GetFactionBasedOnName("Ziranna").isActive;
            case INTERACTION_TYPE.EAT_DEFENSELESS:
                //if (character.race == RACE.GOBLIN || character.race == RACE.SPIDER || character.race == RACE.WOLF) {
                    for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
                        Character characterAtLocation = character.specificLocation.charactersAtLocation[i];
                        if (characterAtLocation.id != character.id && !characterAtLocation.currentParty.icon.isTravelling && characterAtLocation.IsInOwnParty() 
                        && characterAtLocation.GetTraitOr("Abducted", "Unconscious") != null ) {
                            return true;
                        }
                    }
                //}
                return false;
            case INTERACTION_TYPE.TORTURE_ACTION:
                //if (character.race == RACE.GOBLIN || character.race == RACE.HUMANS || character.race == RACE.SKELETON) {
                    for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
                        Character characterAtLocation = character.specificLocation.charactersAtLocation[i];
                        if (characterAtLocation.id != character.id && !characterAtLocation.currentParty.icon.isTravelling && characterAtLocation.IsInOwnParty() && characterAtLocation.GetTrait("Abducted") != null) {
                            return true;
                        }
                    }
                //}
                return false;
            case INTERACTION_TYPE.MOVE_TO_REANIMATE:
                if (!character.homeArea.IsResidentsFull()) { //character.race == RACE.SKELETON && 
                    //**Trigger Criteria 1**: There must be at least one dead corpse in any area
                    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                        Area currArea = LandmarkManager.Instance.allAreas[i];
                        if (currArea.id != character.specificLocation.id && currArea.corpsesInArea.Count > 1) { 
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.CHANCE_ENCOUNTER:
                if (character.characterClass.roleType != CHARACTER_ROLE.BEAST) { // && character.race != RACE.SKELETON
                    for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
                        Character currCharacter = character.specificLocation.charactersAtLocation[i];
                        if (currCharacter.id != character.id && currCharacter.characterClass.roleType != CHARACTER_ROLE.BEAST && currCharacter.race != RACE.SKELETON) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.USE_ITEM_ON_CHARACTER:
                if (character.tokenInInventory != null) {
                    return character.tokenInInventory.GetTargetCharacterFor(character) != null;
                }
                return false;
            case INTERACTION_TYPE.STEAL_ACTION:
                if(character.specificLocation.id == character.homeArea.id) {
                    //If At Home
                    if(character.GetTrait("Crooked") != null) {
                        for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
                            Character currCharacter = character.specificLocation.charactersAtLocation[i];
                            if (currCharacter.id != character.id && currCharacter.isHoldingItem) {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                return true;
            case INTERACTION_TYPE.TRANSFER_HOME:
                if(character.specificLocation.id != character.homeArea.id
                    && character.specificLocation.owner == character.faction) {
                    int targetRemainingResidentCap = character.specificLocation.residentCapacity - character.specificLocation.areaResidents.Count;
                    int homeRemainingResidentCap = character.homeArea.residentCapacity - character.homeArea.areaResidents.Count;
                    if(targetRemainingResidentCap - homeRemainingResidentCap >= 3) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_RECRUIT_FRIEND_FACTION:
            case INTERACTION_TYPE.MOVE_TO_CHARM_FACTION:
                if (character.homeArea.IsResidentsFull()) { //check if resident capacity is full
                    return false;
                }
                return true;
            case INTERACTION_TYPE.MOVE_TO_ASSASSINATE_FACTION:
                //**Trigger Criteria 1**: There must be at least one non-Warded character belonging to an Enemy or War faction.
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    Area currArea = LandmarkManager.Instance.allAreas[i];
                    if (currArea.id == PlayerManager.Instance.player.playerArea.id) {
                        continue; //skip
                    }
                    for (int j = 0; j < currArea.charactersAtLocation.Count; j++) {
                        Character currCharacter = currArea.charactersAtLocation[j];
                        if (currCharacter.GetTrait("Warded") == null
                            && !currCharacter.currentParty.icon.isTravelling
                            && currCharacter.faction.id != character.faction.id) {
                            switch (currCharacter.faction.GetRelationshipWith(character.faction).relationshipStatus) {
                                case FACTION_RELATIONSHIP_STATUS.AT_WAR:
                                case FACTION_RELATIONSHIP_STATUS.ENEMY:
                                    return true;

                            }
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_LOOT:
                return !character.isHoldingItem;
            case INTERACTION_TYPE.MOVE_TO_TAME_BEAST:
                if (!character.homeArea.IsResidentsFull()) {
                    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                        Character currCharacter = CharacterManager.Instance.allCharacters[i];
                        if (currCharacter.role.roleType == CHARACTER_ROLE.BEAST && currCharacter.faction == FactionManager.Instance.neutralFaction) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_HANG_OUT:
                return character.HasRelationshipOfEffect(new List<TRAIT_EFFECT>() { TRAIT_EFFECT.NEUTRAL, TRAIT_EFFECT.POSITIVE });
            case INTERACTION_TYPE.MOVE_TO_SCAVENGE_FACTION:
                //**Trigger Criteria 1**: There must be at least one unoccupied location with a dungeon or a warehouse
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    Area currArea = LandmarkManager.Instance.allAreas[i];
                    if (currArea.id == PlayerManager.Instance.player.playerArea.id || currArea.owner != null) {
                        continue;
                    }
                    if (currArea.HasStructure(STRUCTURE_TYPE.DUNGEON) 
                        || currArea.HasStructure(STRUCTURE_TYPE.WAREHOUSE)) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.CRAFT_ITEM:
                return character.GetTrait("Craftsman") != null && character.specificLocation.HasStructure(STRUCTURE_TYPE.WORK_AREA);
            case INTERACTION_TYPE.SCRAP_ITEM:
                return character.specificLocation.possibleSpecialTokenSpawns.Count > 0;
            case INTERACTION_TYPE.MOVE_TO_RAID_FACTION:
                /***Trigger Criteria 1**: There must be at least one other location that is occupied 
                 * but not owned by the character's Faction and not owned by an Ally or a Friend faction*/
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    Area currArea = LandmarkManager.Instance.allAreas[i];
                    if (currArea.id == PlayerManager.Instance.player.playerArea.id || currArea.owner == null || !currArea.HasStructure(STRUCTURE_TYPE.WAREHOUSE)) {
                        continue;
                    }
                    if (currArea.owner.id != character.faction.id) {
                        switch (currArea.owner.GetRelationshipWith(character.faction).relationshipStatus) {
                            case FACTION_RELATIONSHIP_STATUS.AT_WAR:
                            case FACTION_RELATIONSHIP_STATUS.ENEMY:
                            case FACTION_RELATIONSHIP_STATUS.DISLIKED:
                            case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
                                return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_MINE:
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    Area currArea = LandmarkManager.Instance.allAreas[i];
                    if (currArea.id != PlayerManager.Instance.player.playerArea.id && currArea.coreTile.landmarkOnTile.specificLandmarkType.ToString().Contains("MINE")) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_HARVEST:
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    Area currArea = LandmarkManager.Instance.allAreas[i];
                    if (currArea.id != PlayerManager.Instance.player.playerArea.id && currArea.coreTile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.FARM) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_OCCUPY_FACTION:
                //**Trigger Criteria 1**: There must be at least one other unoccupied location that is a valid expansion target for the character's race.
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    area = LandmarkManager.Instance.allAreas[i];
                    if (area.id != character.specificLocation.id 
                        && area.id != PlayerManager.Instance.player.playerArea.id
                        && area.owner == null 
                        && area.possibleOccupants.Contains(character.race)) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_CURSE:
                return character.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.ENEMY);
            case INTERACTION_TYPE.CONSUME_LIFE:
                List<LocationStructure> insideStructures = character.specificLocation.GetStructuresAtLocation(true);
                for (int i = 0; i < insideStructures.Count; i++) {
                    for (int j = 0; j < insideStructures[i].charactersHere.Count; j++) {
                        Character characterAtLocation = insideStructures[i].charactersHere[j];
                        if(character.id != characterAtLocation.id && characterAtLocation.GetTraitOr("Restrained", "Abducted") != null) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_EXPLORE_FACTION:
                //**Trigger Criteria 1**: The character must not be holding an item
                return character.tokenInInventory == null;
            default:
                return true;
        }
    }
    private Area GetAttackTarget(Area areaToAttack) {
        Area targetArea = null;
        List<Area> enemyAreas = new List<Area>();
        if(areaToAttack.owner != null) {
            foreach (KeyValuePair<Faction, FactionRelationship> kvp in areaToAttack.owner.relationships) {
                FactionRelationship factionRelationship = kvp.Value;
                if (kvp.Key.isActive && factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.AT_WAR) {
                    enemyAreas.AddRange(kvp.Key.ownedAreas);
                }
            }
        } else {
            //Neutral Area will act as if its at war with all areas, meaning all other areas can be a target
            for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                Area area = LandmarkManager.Instance.allAreas[i];
                if(area.id != areaToAttack.id 
                    && PlayerManager.Instance.player.playerArea.id != area.id) {
                    enemyAreas.Add(area);
                }
            }
        }
       
        if(enemyAreas.Count > 0) {
            targetArea = enemyAreas[UnityEngine.Random.Range(0, enemyAreas.Count)];
            List<Character> attackers = areaToAttack.FormCombatCharacters();
            if(attackers.Count > 0) {
                areaToAttack.SetAttackTargetAndCharacters(targetArea, attackers);
            } else {
                return null;
            }

            ////If at war with other factions
            //List<Character> residentsAtArea = new List<Character>();
            //for (int i = 0; i < areaToAttack.areaResidents.Count; i++) {
            //    Character resident = areaToAttack.areaResidents[i];
            //    if(resident.forcedInteraction == null && resident.doNotDisturb <= 0 && resident.IsInOwnParty() && !resident.isLeader 
            //        && resident.role.roleType != CHARACTER_ROLE.CIVILIAN && !resident.currentParty.icon.isTravelling 
            //        && !resident.isDefender && resident.specificLocation.id == areaToAttack.id
            //        && resident.faction == areaToAttack.owner) {
            //        residentsAtArea.Add(resident);
            //    }
            //}
            //if(residentsAtArea.Count >= 3) {
            //    //If has at least 3 residents in area
            //    int numOfMembers = 3;
            //    if(residentsAtArea.Count >= 4) {
            //        numOfMembers = 4;
            //    }
            //    List<List<Character>> characterCombinations = Utilities.ItemCombinations(residentsAtArea, 5, numOfMembers, numOfMembers);
            //    if(characterCombinations.Count > 0) {
            //        List<Character> currentAttackCharacters = null;
            //        Area currentTargetArea = null;
            //        float highestWinChance = 0f;
            //        for (int i = 0; i < characterCombinations.Count; i++) {
            //            List<Character> attackCharacters = characterCombinations[i];
            //            Area target = enemyAreas[UnityEngine.Random.Range(0, enemyAreas.Count)];
            //            DefenderGroup defender = target.GetFirstDefenderGroup();
            //            float winChance = 0f;
            //            float loseChance = 0f;
            //            if(defender != null && defender.party != null) {
            //                CombatManager.Instance.GetCombatChanceOfTwoLists(attackCharacters, defender.party.characters, out winChance, out loseChance);
            //            } else {
            //                CombatManager.Instance.GetCombatChanceOfTwoLists(attackCharacters, null, out winChance, out loseChance);
            //            }
            //            if (winChance > 40f) {
            //                if(currentTargetArea == null) {
            //                    currentTargetArea = target;
            //                    currentAttackCharacters = attackCharacters;
            //                    highestWinChance = winChance;
            //                } else {
            //                    if(winChance > highestWinChance) {
            //                        currentTargetArea = target;
            //                        currentAttackCharacters = attackCharacters;
            //                        highestWinChance = winChance;
            //                    }
            //                }
            //            }
            //        }
            //        targetArea = currentTargetArea;
            //        areaToAttack.SetAttackTargetAndCharacters(currentTargetArea, currentAttackCharacters);
            //    }
            //}
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

    private void TryExecuteInteractionsDefault() {
        List<Character> trackedCharacters = PlayerManager.Instance.player.GetTrackedCharacters();
        if (trackedCharacters.Count > 0) {
            for (int i = 0; i < trackedCharacters.Count; i++) {
                Character currCharacter = trackedCharacters[i];
                List<Interaction> interactionsInvolved = GetAllValidInteractionsInvolving(currCharacter);
                for (int j = 0; j < interactionsInvolved.Count; j++) {
                    Interaction interaction = interactionsInvolved[j];
                    if (!interactionUIQueue.Contains(interaction)) {
                        AddToInteractionQueue(interaction);
                        Debug.Log(GameManager.Instance.TodayLogString() + " Added " + interaction.name + " from " + currCharacter.name + " to execute interactions queue.");
                    }
                }
            }
        }

        List<Area> trackedAreas = PlayerManager.Instance.player.GetTrackedAreas();
        if (trackedAreas.Count > 0) {
            for (int i = 0; i < trackedAreas.Count; i++) {
                Area currArea = trackedAreas[i];
                List<Interaction> interactionsInvolved = GetAllValidInteractionsInvolving(currArea).Where(x => !interactionUIQueue.Contains(x)).ToList();
                if (interactionsInvolved.Count > 0) {
                    Interaction chosen = interactionsInvolved[Random.Range(0, interactionsInvolved.Count)];
                    AddToInteractionQueue(chosen);
                    Debug.Log(GameManager.Instance.TodayLogString() + " Added " + chosen.name + " from " + currArea.name + " to execute interactions queue.");
                }
            }
        }

        if (interactionUIQueue.Count > 0) {
            //set the last interaction to execute all defaults after it
            Interaction lastInteraction = interactionUIQueue.Last();
            lastInteraction.AddEndInteractionAction(() => ExecuteInteractionsDefault());
            //then show the first interaction, that will then start the line of queues
            Interaction interactionToShow = interactionUIQueue.Dequeue();
            InteractionUI.Instance.OpenInteractionUI(interactionToShow);
            GameManager.Instance.pauseDayEnded2 = true;
        } else {
            ExecuteInteractionsDefault();
        }
    }

    private List<Interaction> GetAllValidInteractionsInvolving(Character character) {
        List<Interaction> interactions = new List<Interaction>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            for (int j = 0; j < currArea.currentInteractions.Count; j++) {
                Interaction currInteraction = currArea.currentInteractions[j];
                if (currInteraction.type == INTERACTION_TYPE.MOVE_TO_RETURN_HOME
                    || currInteraction.type == INTERACTION_TYPE.CHARACTER_FLEES 
                    || !currInteraction.CanInteractionBeDoneBy(currInteraction.characterInvolved)) {
                    continue;
                }
                if ((currInteraction.targetCharacter != null && currInteraction.targetCharacter.id == character.id)
                    || (currInteraction.characterInvolved != null && currInteraction.characterInvolved.id == character.id)) {
                    interactions.Add(currInteraction);
                }
            }
        }
        return interactions;
    }
    private List<Interaction> GetAllValidInteractionsInvolving(Area currArea) {
        List<Interaction> interactions = new List<Interaction>();
        for (int i = 0; i < currArea.currentInteractions.Count; i++) {
            Interaction currInteraction = currArea.currentInteractions[i];
            if (currInteraction.type == INTERACTION_TYPE.MOVE_TO_RETURN_HOME
                || currInteraction.type == INTERACTION_TYPE.CHARACTER_FLEES
                || !currInteraction.CanInteractionBeDoneBy(currInteraction.characterInvolved)) {
                continue;
            }
            interactions.Add(currInteraction);
        }
        return interactions;
    }

    private void ExecuteInteractionsDefault() {
        GameManager.Instance.pauseDayEnded2 = false;
        dailyInteractionSummary = GameManager.Instance.TodayLogString() + "Executing interactions";
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            DefaultInteractionsInArea(currArea, ref dailyInteractionSummary);
            //StartCoroutine(DefaultInteractionsInAreaCoroutine(currArea, AddToDailySummary));
        }
        dailyInteractionSummary += "\n==========Done==========";
        Debug.Log(dailyInteractionSummary);
    }
    public void DefaultInteractionsInArea(Area area, ref string log) {
        log += "\n==========Executing <b>" + area.name + "'s</b> interactions==========";
        if (area.stopDefaultAllExistingInteractions) {
            log += "\nCannot run areas default interactions because area interactions have been disabled";
            return; //skip
        }
        List<Interaction> interactionsInArea = new List<Interaction>(area.currentInteractions);
        if (interactionsInArea.Count == 0) {
            log += "\nNo interactions in area";
            return;
        }

        for (int j = 0; j < interactionsInArea.Count; j++) {
            Interaction currInteraction = interactionsInArea[j];
            Character character = currInteraction.characterInvolved;
            if (character != null) {
                log += "\n<b><color=green>" + character.name + "</color></b> triggered his/her day tick to perform <b>" + currInteraction.name + "</b>";
            }
        }

        for (int j = 0; j < interactionsInArea.Count; j++) {
            Interaction currInteraction = interactionsInArea[j];
            Character character = currInteraction.characterInvolved;
            if (!currInteraction.hasActivatedTimeOut) {
                if (character == null || (!character.isDead && currInteraction.CanInteractionBeDoneBy(character))) {
                    log += "\nRunning interaction default " + currInteraction.type.ToString();
                    if (character != null) {
                        log += " Involving <b><color=green>" + character.name + "</color></b>";
                    }
                    currInteraction.TimedOutRunDefault(ref log);
                    log += "\n";
                } else {
                    //area.RemoveInteraction(currInteraction);
                    currInteraction.EndInteraction();
                    log += "\n<color=red>" + character.name + " is unable to perform " + currInteraction.name + "!</color>";
                    //Unable to perform
                    UnableToPerform unable = CreateNewInteraction(INTERACTION_TYPE.UNABLE_TO_PERFORM, area) as UnableToPerform;
                    unable.SetActionNameThatCannotBePerformed(currInteraction.name);
                    character.AddInteraction(unable);
                    unable.TimedOutRunDefault(ref log);
                    log += "\n";
                }
            }
        }
    }

    //private void AddToDailySummary(string log) {
    //    dailyInteractionSummary += log;
    //}
    //public IEnumerator DefaultInteractionsInAreaCoroutine(Area area, System.Action<string> addToLog) {
    //    string log = "\n==========Executing " + area.name + "'s interactions==========";
    //    if (area.stopDefaultAllExistingInteractions) {
    //        log += "\nCannot run areas default interactions because area interactions have been disabled";
    //        addToLog(log);
    //        yield return null; //skip
    //    }
    //    List<Interaction> interactionsInArea = new List<Interaction>(area.currentInteractions);
    //    if (interactionsInArea.Count == 0) {
    //        log += "\nNo interactions in area";
    //        addToLog(log);
    //        yield return null;
    //    }

    //    for (int j = 0; j < interactionsInArea.Count; j++) {
    //        Interaction currInteraction = interactionsInArea[j];
    //        Character character = currInteraction.characterInvolved;
    //        if (!currInteraction.hasActivatedTimeOut) {
    //            if (character == null || (!character.isDead && currInteraction.CanInteractionBeDoneBy(character))) {
    //                log += "\nRunning interaction default " + currInteraction.type.ToString();
    //                if (character != null) {
    //                    log += " Involving " + character.name;
    //                }
    //                currInteraction.TimedOutRunDefault(ref log);
    //                log += "\n";
    //            } else {
    //                //area.RemoveInteraction(currInteraction);
    //                currInteraction.EndInteraction();
    //                log += "\n" + character.name + " is unable to perform " + currInteraction.name + "!";
    //                //Unable to perform
    //                Interaction unable = CreateNewInteraction(INTERACTION_TYPE.UNABLE_TO_PERFORM, area.coreTile.landmarkOnTile);
    //                character.AddInteraction(unable);
    //                unable.TimedOutRunDefault(ref log);
    //                log += "\n";
    //            }
    //        }
    //    }
    //    addToLog(log);
    //    yield return null;
    //}

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

    #region Move To Save
    private bool CanCreateMoveToSave(Character character) {
        /*
         * Trigger Criteria 1: There is at least one other area that his faction does not own that has at least one Abducted character 
         * that is not part of that area's faction that is either from this character's faction or from a Neutral, Friend or Ally factions
         */
        if (character.race == RACE.HUMANS || character.race == RACE.ELVES || character.race == RACE.SPIDER) {
            List<Area> otherAreas = new List<Area>(LandmarkManager.Instance.allAreas.Where(x => x.owner != null && x.owner.id != character.faction.id));
            for (int i = 0; i < otherAreas.Count; i++) {
                Area currArea = otherAreas[i];
                for (int j = 0; j < currArea.charactersAtLocation.Count; j++) {
                    Character currCharacter = currArea.charactersAtLocation[j];
                    Abducted abductedTrait = currCharacter.GetTrait("Abducted") as Abducted;
                    if (abductedTrait != null && currArea.owner.id != currCharacter.faction.id) { //check if character is abducted and that the area he/she is in is not owned by their faction
                        if (currCharacter.faction.id == character.faction.id) {
                            return true;
                        } else {
                            FactionRelationship rel = character.faction.GetRelationshipWith(currCharacter.faction);
                            switch (rel.relationshipStatus) {
                                case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
                                case FACTION_RELATIONSHIP_STATUS.FRIEND:
                                case FACTION_RELATIONSHIP_STATUS.ALLY:
                                    return true;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
    #endregion
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

public struct InteractionAttributes {
    public INTERACTION_CATEGORY[] categories;
    public INTERACTION_ALIGNMENT alignment;
    public InteractionActorEffect[] actorEffect;
    public InteractionTargetCharacterEffect[] targetCharacterEffect;
}
public struct InteractionActorEffect {
    public INTERACTION_CHARACTER_EFFECT effect;
    public string[] effectString;
}
public struct InteractionTargetCharacterEffect {
    public INTERACTION_CHARACTER_EFFECT effect;
    public string[] effectString;
}