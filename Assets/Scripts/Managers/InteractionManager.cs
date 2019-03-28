using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public class InteractionManager : MonoBehaviour {
    public static InteractionManager Instance = null;

    public delegate T ObjectActivator<T>(params object[] args);

    public static readonly string Supply_Cache_Reward_1 = "SupplyCacheReward1";
    public static readonly string Mana_Cache_Reward_1 = "ManaCacheReward1";
    public static readonly string Mana_Cache_Reward_2 = "ManaCacheReward2";
    public static readonly string Level_Reward_1 = "LevelReward1";
    public static readonly string Level_Reward_2 = "LevelReward2";

    public const string Goap_State_Success = "Success";
    public const string Goap_State_Fail = "Fail";

    public static readonly int Character_Action_Delay = 5;

    public Queue<Interaction> interactionUIQueue { get; private set; }

    private string dailyInteractionSummary;
    //private ObjectActivator<GoapAction> goapActionCreator;

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
        Messenger.AddListener(Signals.TICK_ENDED_2, ExecuteInteractionsDefault); //TryExecuteInteractionsDefault
        ConstructInteractionCategoryAndAlignment();

        //ConstructorInfo ctor = typeof(GoapAction).GetConstructors().First();
        //goapActionCreator = GetActivator<GoapAction>(ctor);
    }

    #region Interaction Category And Alignment
    private void ConstructInteractionCategoryAndAlignment() {
        interactionCategoryAndAlignment = new Dictionary<INTERACTION_TYPE, InteractionAttributes>() {
            { INTERACTION_TYPE.CHARM_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{
                    new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Charmed" },
                    new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.CHANGE_FACTION, effectString = "Actor" },
                },
            } },
            { INTERACTION_TYPE.EXPLORE_EVENT_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_ITEM } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.TORTURE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{
                    new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Injured" },
                    new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.CHANGE_FACTION, effectString = "Actor" },
                },
            } },
            { INTERACTION_TYPE.LOOT_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_ITEM } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.TAME_BEAST_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.CHANGE_FACTION, effectString = "Actor" } },
            } },
            { INTERACTION_TYPE.CRAFT_ITEM, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_ITEM } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.RAID_EVENT_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_SUPPLY } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.STEAL_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_ITEM } },
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.LOSE_ITEM } },
            } },
            { INTERACTION_TYPE.RECRUIT_FRIEND_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.GOOD,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.CHANGE_FACTION, effectString = "Actor" } },
            } },
            { INTERACTION_TYPE.ASSASSINATE_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.DEATH } },
            } },
            { INTERACTION_TYPE.REANIMATE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                targetCharacterEffect = new InteractionCharacterEffect[]{
                    new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Reanimated" },
                    new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.CHANGE_FACTION, effectString = "Actor" },
                },
            } },
            { INTERACTION_TYPE.SCAVENGE_EVENT_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_SUPPLY } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.OCCUPY_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.EXPANSION },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.CHANGE_HOME } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.MINE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY },
                alignment = INTERACTION_ALIGNMENT.GOOD,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_SUPPLY } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.HARVEST_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY },
                alignment = INTERACTION_ALIGNMENT.GOOD,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_SUPPLY } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.SCRAP_ITEM, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_SUPPLY } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.PATROL_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.DEFENSE },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Patrolling" } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.CONSUME_LIFE, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_SUPPLY } },
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.DEATH } },
            } },
            { INTERACTION_TYPE.CONSUME_PRISONER_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_SUPPLY } },
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.DEATH } },
            } },
            { INTERACTION_TYPE.COURTESY_CALL, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.DIPLOMACY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.GIFT_ITEM, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.DIPLOMACY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.GIFT_BEAST, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.DIPLOMACY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = null,
            } },
            //CHARACTER NPC ACTIONS-----------------------------------------------------------------------------------------------------------------------------------------------------------------
            { INTERACTION_TYPE.MOVE_TO_RETURN_HOME, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.OTHER },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.PICK_ITEM, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.OTHER },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_ITEM } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.DROP_ITEM, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.OTHER },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.LOSE_ITEM } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.EAT_DEFENSELESS, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.FULLNESS_RECOVERY },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.FULLNESS_RECOVERY } },
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.DEATH } },
            } },
            { INTERACTION_TYPE.ABDUCT_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT, INTERACTION_CATEGORY.SUBTERFUGE },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Abducted" } },
            } },
            { INTERACTION_TYPE.ARGUE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.SOCIAL },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Annoyed" } },
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Annoyed" } },
            } },
            { INTERACTION_TYPE.CURSE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Cursed" } },
            } },
            { INTERACTION_TYPE.HUNT_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.FULLNESS_RECOVERY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.FULLNESS_RECOVERY } },
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.DEATH } },
            } },
            { INTERACTION_TYPE.BERSERK_ATTACK, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.OTHER },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                actorEffect = null,
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.ASK_FOR_HELP, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.OTHER },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.MOVE_TO_VISIT, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.OTHER },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.TRANSFER_HOME, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.OTHER },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.CHANGE_HOME } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.TORTURE_ACTION_NPC, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.OFFENSE },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Injured" } },
            } },
            { INTERACTION_TYPE.REST_AT_HOME_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.TIREDNESS_RECOVERY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TIREDNESS_RECOVERY } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.EAT_HOME_MEAL_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.FULLNESS_RECOVERY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.FULLNESS_RECOVERY } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.POISON_HOUSE_FOOD, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.DEATH } },
            } },
            { INTERACTION_TYPE.FEED_PRISONER_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.WORK },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{
                    new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, effectString = "Hungry" },
                    new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, effectString = "Starving" },
                },
            } },
            { INTERACTION_TYPE.BOOBY_TRAP_HOUSE, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.DEATH } },
            } },
            { INTERACTION_TYPE.STEAL_ACTION_NPC, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE },
                alignment = INTERACTION_ALIGNMENT.EVIL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.OBTAIN_ITEM } },
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.LOSE_ITEM } },
            } },
            { INTERACTION_TYPE.ASSAULT_ACTION_NPC, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.OFFENSE },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Injured" } },
            } },
            { INTERACTION_TYPE.CAMP_OUT_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.TIREDNESS_RECOVERY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TIREDNESS_RECOVERY } },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.HANG_OUT_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SOCIAL },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Happy" } },
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Happy" } },
            } },
            { INTERACTION_TYPE.MAKE_LOVE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.ROMANTIC },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Happy" } },
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Happy" } },
            } },
            { INTERACTION_TYPE.REMOVE_CURSE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SAVE },
                alignment = INTERACTION_ALIGNMENT.GOOD,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, effectString = "Cursed" } },
            } },
            { INTERACTION_TYPE.RESTRAIN_CRIMINAL_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.WORK },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Restrained" } },
            } },
            { INTERACTION_TYPE.MOVE_TO_RECRUIT_FRIEND_ACTION_FACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
                alignment = INTERACTION_ALIGNMENT.GOOD,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.CHANGE_FACTION, effectString = "Actor" } },
            } },
            { INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SAVE },
                alignment = INTERACTION_ALIGNMENT.GOOD,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, effectString = "Abducted" } },
            } },
            { INTERACTION_TYPE.FIRST_AID_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SAVE },
                alignment = INTERACTION_ALIGNMENT.GOOD,
                actorEffect = null,
                targetCharacterEffect = new InteractionCharacterEffect[]{
                    new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, effectString = "Unconscious" },
                    new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, effectString = "Sick" },
                },
            } },
            { INTERACTION_TYPE.PROTECT_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.OTHER },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, effectString = "Patrolling" } },
                targetCharacterEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, effectString = "Protected" } },
            } },
            { INTERACTION_TYPE.LOCATE_MISSING, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SOCIAL },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = null,
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.HUNT_SMALL_ANIMALS, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.FULLNESS_RECOVERY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.FULLNESS_RECOVERY} },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.FORAGE_ACTION, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.FULLNESS_RECOVERY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.FULLNESS_RECOVERY} },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.EAT_INN_MEAL, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.FULLNESS_RECOVERY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.FULLNESS_RECOVERY} },
                targetCharacterEffect = null,
            } },
            { INTERACTION_TYPE.REST_AT_INN, new InteractionAttributes(){
                categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.TIREDNESS_RECOVERY },
                alignment = INTERACTION_ALIGNMENT.NEUTRAL,
                actorEffect = new InteractionCharacterEffect[]{ new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TIREDNESS_RECOVERY} },
                targetCharacterEffect = null,
            } },
        };
    }
    public InteractionAttributes GetCategoryAndAlignment (INTERACTION_TYPE type, Character actor) {
        string typeString = type.ToString();
        if (typeString.Contains("MOVE_TO") && !interactionCategoryAndAlignment.ContainsKey(type)) {
            string actualInteractionString = typeString.Remove(0, 8);
            INTERACTION_TYPE actionInteractionType = INTERACTION_TYPE.NONE;
            if(System.Enum.TryParse(actualInteractionString, out actionInteractionType)) {
                if (interactionCategoryAndAlignment.ContainsKey(actionInteractionType)) {
                    return interactionCategoryAndAlignment[actionInteractionType];
                }
            }
        } else {
            if (interactionCategoryAndAlignment.ContainsKey(type)) {
                return interactionCategoryAndAlignment[type];
            } else {
                if((type == INTERACTION_TYPE.USE_ITEM_ON_CHARACTER || type == INTERACTION_TYPE.USE_ITEM_ON_SELF) && actor != null && actor.isHoldingItem) {
                    InteractionAttributes attributes = actor.tokenInInventory.interactionAttributes;
                    return attributes;
                }
            }
        }
        //Debug.LogWarning("No category and alignment for " + type.ToString());
        return null;
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
            case INTERACTION_TYPE.MOVE_TO_SCAVENGE_EVENT:
                createdInteraction = new MoveToScavenge(interactable);
                break;
            case INTERACTION_TYPE.SCAVENGE_EVENT:
                createdInteraction = new ScavengeEvent(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_RAID_EVENT:
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
            case INTERACTION_TYPE.MOVE_TO_EXPLORE_EVENT:
                createdInteraction = new MoveToExplore(interactable);
                break;
            case INTERACTION_TYPE.MINION_PEACE_NEGOTIATION:
                createdInteraction = new MinionPeaceNegotiation(interactable);
                break;
            case INTERACTION_TYPE.DEFENSE_UPGRADE:
                createdInteraction = new DefenseUpgrade(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_EXPANSION_EVENT:
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
            case INTERACTION_TYPE.MOVE_TO_RECRUIT_ACTION:
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
            case INTERACTION_TYPE.MOVE_TO_IMPROVE_RELATIONS_EVENT:
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
            case INTERACTION_TYPE.MOVE_TO_CHARM_ACTION:
                createdInteraction = new MoveToCharm(interactable);
                break;
            case INTERACTION_TYPE.CHARM_ACTION:
                createdInteraction = new CharmAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_STEAL_ACTION:
                createdInteraction = new MoveToSteal(interactable);
                break;
            case INTERACTION_TYPE.STEAL_ACTION:
                createdInteraction = new StealAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_ABDUCT_ACTION:
                createdInteraction = new MoveToAbduct(interactable);
                break;
            case INTERACTION_TYPE.ABDUCT_ACTION:
                createdInteraction = new AbductAction(interactable);
                break;
            //case INTERACTION_TYPE.MOVE_TO_HUNT_ACTION:
            //    createdInteraction = new MoveToHunt(interactable);
                //break;
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
            case INTERACTION_TYPE.MOVE_TO_REANIMATE_ACTION:
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
            case INTERACTION_TYPE.MOVE_TO_SAVE_ACTION:
                createdInteraction = new MoveToSave(interactable);
                break;
            //case INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION:
            //    createdInteraction = new ReleaseAbductedAction(interactable);
            //    break;
            case INTERACTION_TYPE.MOVE_TO_VISIT:
                createdInteraction = new MoveToVisit(interactable);
                break;
            case INTERACTION_TYPE.TRANSFER_HOME:
                createdInteraction = new TransferHome(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_STEAL_ACTION_FACTION:
                createdInteraction = new MoveToStealFaction(interactable);
                break;
            case INTERACTION_TYPE.STEAL_ACTION_FACTION:
                createdInteraction = new StealActionFaction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_RECRUIT_FRIEND_ACTION_FACTION:
                createdInteraction = new MoveToRecruitFriendFaction(interactable);
                break;
            case INTERACTION_TYPE.RECRUIT_FRIEND_ACTION_FACTION:
                createdInteraction = new RecruitFriendActionFaction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_ASSASSINATE_ACTION_FACTION:
                createdInteraction = new MoveToAssassinateFaction(interactable);
                break;
            case INTERACTION_TYPE.ASSASSINATE_ACTION_FACTION:
                createdInteraction = new AssassinateActionFaction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_LOOT_ACTION:
                createdInteraction = new MoveToLoot(interactable);
                break;
            case INTERACTION_TYPE.LOOT_ACTION:
                createdInteraction = new LootAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_TAME_BEAST_ACTION:
                createdInteraction = new MoveToTameBeast(interactable);
                break;
            case INTERACTION_TYPE.TAME_BEAST_ACTION:
                createdInteraction = new TameBeastAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_HANG_OUT_ACTION:
                createdInteraction = new MoveToHangOut(interactable);
                break;
            case INTERACTION_TYPE.HANG_OUT_ACTION:
                createdInteraction = new HangOutAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_CHARM_ACTION_FACTION:
                createdInteraction = new MoveToCharmFaction(interactable);
                break;
            case INTERACTION_TYPE.CHARM_ACTION_FACTION:
                createdInteraction = new CharmActionFaction(interactable);
                break;
            //case INTERACTION_TYPE.MOVE_TO_ARGUE_ACTION:
            //    createdInteraction = new MoveToArgue(interactable);
            //    break;
            case INTERACTION_TYPE.ARGUE_ACTION:
                createdInteraction = new ArgueAction(interactable);
                break;
            //case INTERACTION_TYPE.MOVE_TO_CURSE_ACTION:
            //    createdInteraction = new MoveToCurse(interactable);
            //    break;
            case INTERACTION_TYPE.CURSE_ACTION:
                createdInteraction = new CurseAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_SCAVENGE_EVENT_FACTION:
                createdInteraction = new MoveToScavengeFaction(interactable);
                break;
            case INTERACTION_TYPE.SCAVENGE_EVENT_FACTION:
                createdInteraction = new ScavengeEventFaction(interactable);
                break;
            case INTERACTION_TYPE.CRAFT_ITEM:
                createdInteraction = new CraftItem(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_RAID_EVENT_FACTION:
                createdInteraction = new MoveToRaidFaction(interactable);
                break;
            case INTERACTION_TYPE.RAID_EVENT_FACTION:
                createdInteraction = new RaidEventFaction(interactable);
                break;
            case INTERACTION_TYPE.BERSERK_ATTACK:
                createdInteraction = new BerserkAttack(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_OCCUPY_ACTION_FACTION:
                createdInteraction = new MoveToOccupyFaction(interactable);
                break;
            case INTERACTION_TYPE.OCCUPY_ACTION_FACTION:
                createdInteraction = new OccupyActionFaction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_MINE_ACTION:
                createdInteraction = new MoveToMine(interactable);
                break;
            case INTERACTION_TYPE.MINE_ACTION:
                createdInteraction = new MineAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_HARVEST_ACTION:
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
            case INTERACTION_TYPE.MOVE_TO_EXPLORE_EVENT_FACTION:
                createdInteraction = new MoveToExploreFaction(interactable);
                break;
            case INTERACTION_TYPE.EXPLORE_EVENT_FACTION:
                createdInteraction = new ExploreEventFaction(interactable);
                break;
            case INTERACTION_TYPE.ASK_FOR_HELP:
                createdInteraction = new AskForHelp(interactable);
                break;
            case INTERACTION_TYPE.TORTURE_ACTION_NPC:
                createdInteraction = new TortureActionNPC(interactable);
                break;
            case INTERACTION_TYPE.REST_AT_HOME_ACTION:
                createdInteraction = new RestAtHomeAction(interactable);
                break;
            case INTERACTION_TYPE.EAT_HOME_MEAL_ACTION:
                createdInteraction = new EatHomeMealAction(interactable);
                break;
            case INTERACTION_TYPE.POISON_HOUSE_FOOD:
                createdInteraction = new PoisonHouseFood(interactable);
                break;
            case INTERACTION_TYPE.FEED_PRISONER_ACTION:
                createdInteraction = new FeedPrisonerAction(interactable);
                break;
            case INTERACTION_TYPE.BOOBY_TRAP_HOUSE:
                createdInteraction = new BoobyTrapHouse(interactable);
                break;
            case INTERACTION_TYPE.STEAL_ACTION_NPC:
                createdInteraction = new StealActionNPC(interactable);
                break;
            case INTERACTION_TYPE.ASSAULT_ACTION_NPC:
                createdInteraction = new AssaultActionNPC(interactable);
                break;
            case INTERACTION_TYPE.CAMP_OUT_ACTION:
                createdInteraction = new CampOutAction(interactable);
                break;
            case INTERACTION_TYPE.FORAGE_ACTION:
                createdInteraction = new ForageAction(interactable);
                break;
            case INTERACTION_TYPE.MAKE_LOVE_ACTION:
                createdInteraction = new MakeLoveAction(interactable);
                break;
            case INTERACTION_TYPE.REMOVE_CURSE_ACTION:
                createdInteraction = new RemoveCurseAction(interactable);
                break;
            case INTERACTION_TYPE.RESTRAIN_CRIMINAL_ACTION:
                createdInteraction = new RestrainCriminalAction(interactable);
                break;
            case INTERACTION_TYPE.USE_ITEM_ON_STRUCTURE:
                createdInteraction = new UseItemOnStructure(interactable);
                break;
            case INTERACTION_TYPE.FIRST_AID_ACTION:
                createdInteraction = new FirstAidAction(interactable);
                break;
            case INTERACTION_TYPE.PROTECT_ACTION:
                createdInteraction = new ProtectAction(interactable);
                break;
            case INTERACTION_TYPE.LOCATE_MISSING:
                createdInteraction = new LocateMissing(interactable);
                break;
            case INTERACTION_TYPE.CONSUME_PRISONER_ACTION:
                createdInteraction = new ConsumePrisonerAction(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_COURTESY_CALL:
                createdInteraction = new MoveToCourtesyCall(interactable);
                break;
            case INTERACTION_TYPE.COURTESY_CALL:
                createdInteraction = new CourtesyCall(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_GIFT_ITEM:
                createdInteraction = new MoveToGiftItem(interactable);
                break;
            case INTERACTION_TYPE.GIFT_ITEM:
                createdInteraction = new GiftItem(interactable);
                break;
            case INTERACTION_TYPE.MOVE_TO_GIFT_BEAST:
                createdInteraction = new MoveToGiftBeast(interactable);
                break;
            case INTERACTION_TYPE.GIFT_BEAST:
                createdInteraction = new GiftBeast(interactable);
                break;
            case INTERACTION_TYPE.HUNT_SMALL_ANIMALS:
                createdInteraction = new HuntSmallAnimals(interactable);
                break;
            case INTERACTION_TYPE.EAT_INN_MEAL:
                createdInteraction = new EatInnMeal(interactable);
                break;
            case INTERACTION_TYPE.REST_AT_INN:
                createdInteraction = new RestAtInn(interactable);
                break;
        }
        return createdInteraction;
    }
    public T CreateNewGoapInteraction<T>(Character actor, IPointOfInterest target) { //TODO: Need to test performance of this
        ConstructorInfo ctor = typeof(T).GetConstructors().First();
        ObjectActivator<T> goapActionCreator = GetActivator<T>(ctor);
        T goapAction = goapActionCreator(actor, target);
        return goapAction;
    }
    public GoapAction CreateNewGoapInteraction(INTERACTION_TYPE type, Character actor, IPointOfInterest target, bool willInitialize = true) {
        GoapAction goapAction = null;
        switch (type) {
            case INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION:
                goapAction = new ReleaseCharacter(actor, target);
                break;
            case INTERACTION_TYPE.EAT_PLANT:
                goapAction = new EatPlant(actor, target);
                break;
            case INTERACTION_TYPE.EAT_SMALL_ANIMAL:
                goapAction = new EatAnimal(actor, target);
                break;
            case INTERACTION_TYPE.EAT_DWELLING_TABLE:
                goapAction = new EatAtTable(actor, target);
                break;
            case INTERACTION_TYPE.CRAFT_ITEM:
                goapAction = new CraftItemGoap(actor, target);
                break;
            case INTERACTION_TYPE.PICK_ITEM:
                goapAction = new PickItemGoap(actor, target);
                break;
            case INTERACTION_TYPE.MINE_ACTION:
                goapAction = new MineGoap(actor, target);
                break;
            case INTERACTION_TYPE.SLEEP:
                goapAction = new Sleep(actor, target);
                break;
            case INTERACTION_TYPE.ASSAULT_ACTION_NPC:
                goapAction = new AssaultCharacter(actor, target);
                break;
            case INTERACTION_TYPE.ABDUCT_ACTION:
                goapAction = new AbductCharacter(actor, target);
                break;
            case INTERACTION_TYPE.CARRY_CHARACTER:
                goapAction = new CarryCharacter(actor, target);
                break;
            case INTERACTION_TYPE.DROP_CHARACTER:
                goapAction = new DropCharacter(actor, target);
                break;
            case INTERACTION_TYPE.DAYDREAM:
                goapAction = new Daydream(actor, target);
                break;
            case INTERACTION_TYPE.PLAY_GUITAR:
                goapAction = new PlayGuitar(actor, target);
                break;
            case INTERACTION_TYPE.CHAT_CHARACTER:
                goapAction = new ChatCharacter(actor, target);
                break;
            case INTERACTION_TYPE.ARGUE_CHARACTER:
                goapAction = new ArgueCharacter(actor, target);
                break;
            case INTERACTION_TYPE.STROLL:
                goapAction = new Stroll(actor, target);
                break;
            case INTERACTION_TYPE.RETURN_HOME:
                goapAction = new ReturnHome(actor, target);
                break;
            case INTERACTION_TYPE.DRINK:
                goapAction = new Drink(actor, target);
                break;
            case INTERACTION_TYPE.SLEEP_OUTSIDE:
                goapAction = new SleepOutside(actor, target);
                break;
            case INTERACTION_TYPE.EXPLORE:
                goapAction = new Explore(actor, target);
                break;
            case INTERACTION_TYPE.TABLE_REMOVE_POISON:
                goapAction = new TableRemovePoison(actor, target);
                break;
            case INTERACTION_TYPE.TABLE_POISON:
                goapAction = new TablePoison(actor, target);
                break;
            case INTERACTION_TYPE.PRAY:
                goapAction = new Pray(actor, target);
                break;
            case INTERACTION_TYPE.CHOP_WOOD:
                goapAction = new ChopWood(actor, target);
                break;
            case INTERACTION_TYPE.MAGIC_CIRCLE_PERFORM_RITUAL:
                goapAction = new MagicCirclePerformRitual(actor, target);
                break;
            case INTERACTION_TYPE.PATROL:
                goapAction = new Patrol(actor, target);
                break;
            case INTERACTION_TYPE.STEAL:
                goapAction = new Steal(actor, target);
                break;
            case INTERACTION_TYPE.SCRAP:
                goapAction = new Scrap(actor, target);
                break;
            case INTERACTION_TYPE.GET_SUPPLY:
                goapAction = new GetSupply(actor, target);
                break;
            case INTERACTION_TYPE.DROP_SUPPLY:
                goapAction = new DropSupply(actor, target);
                break;
            case INTERACTION_TYPE.TILE_OBJECT_DESTROY:
                goapAction = new TileObjectDestroy(actor, target);
                break;
            case INTERACTION_TYPE.ITEM_DESTROY:
                goapAction = new ItemDestroy(actor, target);
                break;
            case INTERACTION_TYPE.TRAVEL:
                goapAction = new Travel(actor, target);
                break;
            case INTERACTION_TYPE.RETURN_HOME_LOCATION:
                goapAction = new ReturnHomeLocation(actor, target);
                break;
            case INTERACTION_TYPE.HUNT_ACTION:
                goapAction = new Hunt(actor, target);
                break;
            case INTERACTION_TYPE.PLAY:
                goapAction = new Play(actor, target);
                break;
            case INTERACTION_TYPE.PATROL_ROAM:
                goapAction = new PatrolRoam(actor, target);
                break;
        }
        if(goapAction != null && willInitialize) {
            goapAction.Initialize();
        }
        return goapAction;
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
                        if (resident.forcedInteraction == null && resident.doNotDisturb <= 0 && resident.IsInOwnParty() && !resident.isLeader && !resident.isDefender && !resident.currentParty.icon.isTravelling && !resident.characterClass.isNonCombatant && resident.specificLocation.id == location.id) {
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
    public bool CanCreateInteraction(INTERACTION_TYPE interactionType, Character character, Character targetCharacter = null) {
        Area area = null;
        FactionRelationship factionRel = null;
        WeightedDictionary<string> stringWeights = null;
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
            case INTERACTION_TYPE.MOVE_TO_SCAVENGE_EVENT:
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
            case INTERACTION_TYPE.MOVE_TO_RAID_EVENT:
                //There must be at least one other location that is occupied but not owned by the character's Faction and not owned by an Ally or a Friend faction
                //if (character.race == RACE.GOBLIN || character.race == RACE.SKELETON || character.race == RACE.HUMANS) {
                    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                        Area currArea = LandmarkManager.Instance.allAreas[i];
                        if (currArea.owner != null
                            && currArea.owner.id != character.faction.id
                            && currArea.id != PlayerManager.Instance.player.playerArea.id) {
                            factionRel = character.faction.GetRelationshipWith(currArea.owner);
                            if (factionRel.relationshipStatus != FACTION_RELATIONSHIP_STATUS.ALLY && factionRel.relationshipStatus != FACTION_RELATIONSHIP_STATUS.FRIEND) {
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
            case INTERACTION_TYPE.MOVE_TO_EXPANSION_EVENT:
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    area = LandmarkManager.Instance.allAreas[i];
                    if (area.id != character.specificLocation.id && area.owner == null && area.possibleOccupants.Contains(character.race)) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_IMPROVE_RELATIONS_EVENT:
                    //check if there are any areas owned by factions other than your own
                    //if(character.race == RACE.ELVES || character.race == RACE.HUMANS) {
                        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                            Faction faction = FactionManager.Instance.allFactions[i];
                            if (faction.ownedAreas.Count == 0) { //skip factions that don't have owned areas
                                continue; //skip
                            }
                            if (faction.id != PlayerManager.Instance.player.playerFaction.id && faction.id != character.faction.id && faction.isActive) {
                                factionRel = character.faction.GetRelationshipWith(faction);
                                if (factionRel.relationshipStatus != FACTION_RELATIONSHIP_STATUS.ALLY) {
                                    return true;

                                }
                            }
                        }
                    //}
                    return false;
            case INTERACTION_TYPE.MOVE_TO_RECRUIT_ACTION:
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
            case INTERACTION_TYPE.MOVE_TO_EXPLORE_EVENT:
                if(!character.isHoldingItem) {
                    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                        area = LandmarkManager.Instance.allAreas[i];
                        if (area.id != character.specificLocation.id && (area.owner == null || (area.owner != null && area.owner.id != character.faction.id && area.owner.id != PlayerManager.Instance.player.playerFaction.id))) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_CHARM_ACTION:
                //if (character.race == RACE.FAERY) {
                    if (!character.homeArea.IsResidentsFull()) {
                        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                            Character currCharacter = CharacterManager.Instance.allCharacters[i];
                            if(currCharacter.id != character.id && !currCharacter.isDead) {
                                if(currCharacter.isFactionless) {
                                    //Unaligned?
                                    return true;
                                } else if(currCharacter.faction.id != character.faction.id && currCharacter.faction.ownedAreas.Count > 0) {
                                        factionRel = currCharacter.faction.GetRelationshipWith(character.faction);
                                        if (factionRel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED 
                                            || factionRel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL
                                            || factionRel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                                            return true;
                                        }
                                }
                            }
                        }
                    }
                //}
                return false;
            //case INTERACTION_TYPE.MOVE_TO_ABDUCT_ACTION:
            //    //if (character.race == RACE.GOBLIN || character.race == RACE.SPIDER) {
            //        if (!character.homeArea.IsResidentsFull()) {
            //                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            //                    Area currArea = LandmarkManager.Instance.allAreas[i];
            //                    if (currArea.owner == null || currArea.owner.id != PlayerManager.Instance.player.playerFaction.id && currArea.owner.id != character.faction.id) {
            //                        for (int j = 0; j < currArea.charactersAtLocation.Count; j++) {
            //                            Character characterAtLocation = currArea.charactersAtLocation[j];
            //                            if (characterAtLocation.id != character.id && characterAtLocation.IsInOwnParty() && !characterAtLocation.currentParty.icon.isTravelling
            //                                && (characterAtLocation.isFactionless || characterAtLocation.faction.id != character.faction.id)) {
            //                                return true;
            //                            }
            //                        }
            //                    }
            //                }
            //                //for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            //                //    Character currCharacter = CharacterManager.Instance.allCharacters[i];
            //                //    if (currCharacter.id != character.id && !currCharacter.isDead && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty()) {
            //                //        if (currCharacter.isFactionless || currCharacter.faction.id != character.faction.id) {
            //                //            return true;
            //                //        }
            //                //    }
            //                //}
            //            }
            //    //}
            //    return false;
            case INTERACTION_TYPE.MOVE_TO_STEAL_ACTION:
                //if (character.race == RACE.GOBLIN || character.race == RACE.SKELETON || character.race == RACE.FAERY || character.race == RACE.DRAGON) {
                    if (!character.isHoldingItem) {
                        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                            Character currCharacter = CharacterManager.Instance.allCharacters[i];
                            if (currCharacter.id != character.id && !currCharacter.isDead && currCharacter.isHoldingItem) {
                                if (currCharacter.isFactionless || currCharacter.faction.id != character.faction.id) {
                                    return true;
                                }
                            }
                        }
                    }
                //}
                return false;
            case INTERACTION_TYPE.MOVE_TO_STEAL_ACTION_FACTION:
                //**Trigger Criteria 1**: This character must not have an item
                return !character.isHoldingItem;
            case INTERACTION_TYPE.HUNT_ACTION:
                if(character.role.roleType != CHARACTER_ROLE.BEAST) {
                    for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
                        Character currCharacter = character.specificLocation.charactersAtLocation[i];
                        if (currCharacter.id != character.id && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty() && !currCharacter.isLeader
                            && currCharacter.role.roleType == CHARACTER_ROLE.BEAST && currCharacter.isFactionless) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_SAVE_ACTION:
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
                bool isCannibalEatDefenseless = character.GetTrait("Cannibal") != null;
                for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
                    Character characterAtLocation = character.specificLocation.charactersAtLocation[i];
                    if (!isCannibalEatDefenseless && character.race == characterAtLocation.race) { continue; }
                    if (characterAtLocation.id != character.id && !characterAtLocation.currentParty.icon.isTravelling && characterAtLocation.IsInOwnParty() 
                    && characterAtLocation.currentStructure.isInside && characterAtLocation.level <= character.level
                    && characterAtLocation.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER)) {
                        if (characterAtLocation.faction == FactionManager.Instance.neutralFaction) {
                            return true;
                        } else {
                            if (characterAtLocation.faction != character.faction) {
                                return true;
                            } else {
                                if (character.HasRelationshipOfTypeWith(characterAtLocation, RELATIONSHIP_TRAIT.ENEMY)) {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.TORTURE_ACTION:
                //if (character.race == RACE.GOBLIN || character.race == RACE.HUMANS || character.race == RACE.SKELETON) {
                    for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
                        Character characterAtLocation = character.specificLocation.charactersAtLocation[i];
                        if (characterAtLocation.id != character.id && !characterAtLocation.currentParty.icon.isTravelling && characterAtLocation.IsInOwnParty() 
                        && characterAtLocation.currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA && characterAtLocation.GetTrait("Abducted") != null) {
                            return true;
                        }
                    }
                //}
                return false;
            case INTERACTION_TYPE.MOVE_TO_REANIMATE_ACTION:
                //Actor must be a Skeleton or must have Black Magic trait
                if (character.race == RACE.SKELETON || character.GetTrait("Black Magic") != null) {
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
                if (character.role.roleType != CHARACTER_ROLE.BEAST) { // && character.race != RACE.SKELETON
                    for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
                        Character currCharacter = character.specificLocation.charactersAtLocation[i];
                        if (currCharacter.id != character.id && currCharacter.role.roleType != CHARACTER_ROLE.BEAST && currCharacter.race != RACE.SKELETON) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.USE_ITEM_ON_CHARACTER:
            case INTERACTION_TYPE.USE_ITEM_ON_SELF:
                if (character.isHoldingItem) {
                    //return character.tokenInInventory.GetTargetCharacterFor(character) != null;
                    return character.tokenInInventory.CanBeUsedForTarget(character, targetCharacter);
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
                    return character.specificLocation.CanCharacterMigrateHere(character);
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_RECRUIT_FRIEND_ACTION_FACTION:
                //**Trigger Criteria 1**: Home location resident capacity is not yet full
                if (character.homeArea.IsResidentsFull()) { 
                    return false;
                }
                //**Trigger Criteria 2**: Actor has at least one friend from a different faction or unaligned
                if (!character.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.FRIEND, character.faction)) {
                    return false;
                }
                return true;
            case INTERACTION_TYPE.MOVE_TO_CHARM_ACTION_FACTION:
                if (character.homeArea.IsResidentsFull()) { //check if resident capacity is full
                    return false;
                }
                return true;
            case INTERACTION_TYPE.MOVE_TO_ASSASSINATE_ACTION_FACTION:
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
            case INTERACTION_TYPE.MOVE_TO_LOOT_ACTION:
                return !character.isHoldingItem;
            case INTERACTION_TYPE.MOVE_TO_TAME_BEAST_ACTION:
                if (!character.homeArea.IsResidentsFull()) {
                    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                        Character currCharacter = CharacterManager.Instance.allCharacters[i];
                        if (currCharacter.id != character.id && currCharacter.role.roleType == CHARACTER_ROLE.BEAST && currCharacter.faction == FactionManager.Instance.neutralFaction) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.HANG_OUT_ACTION:
                //**Trigger Criteria 1**: the target must also be in the character's current location and is in a structure Inside Settlement
                //**Trigger Criteria 2**: the target's **character missing** is False
                if (targetCharacter.specificLocation.id == character.specificLocation.id 
                    && targetCharacter.currentStructure.isInside
                    && !character.GetCharacterRelationshipData(targetCharacter).isCharacterMissing) {
                    stringWeights = new WeightedDictionary<string>();
                    int validWeightHangOut = 0;
                    int invalidWeightHangOut = 50;
                    if (character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.SERVANT, RELATIONSHIP_TRAIT.RELATIVE)) {
                        validWeightHangOut += 25;
                    }
                    if (character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.FRIEND)) {
                        validWeightHangOut += 50;
                    }
                    if (character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.PARAMOUR)) {
                        validWeightHangOut += 100;
                    }

                    bool isAnnoyedHangOut = false, isHungryHangOut = false, isTiredHangOut = false, isSickHangOut = false, isInjuredHangOut = false;
                    for (int i = 0; i < character.traits.Count; i++) {
                        if(character.traits[i].name == "Annoyed") { isAnnoyedHangOut = true; }
                        else if (character.traits[i].name == "Hungry") { isHungryHangOut = true; }
                        else if (character.traits[i].name == "Tired") { isTiredHangOut = true; } 
                        else if (character.traits[i].name == "Sick") { isSickHangOut = true; } 
                        else if (character.traits[i].name == "Injured") { isInjuredHangOut = true; }
                    }
                    if (isAnnoyedHangOut) {
                        invalidWeightHangOut = (int) (invalidWeightHangOut * 1.5f);
                    }
                    if (isHungryHangOut) {
                        invalidWeightHangOut = (int) (invalidWeightHangOut * 1.5f);
                    }
                    if (isTiredHangOut) {
                        invalidWeightHangOut = (int) (invalidWeightHangOut * 1.5f);
                    }
                    if (isSickHangOut) {
                        invalidWeightHangOut *= 2;
                    }
                    if (isInjuredHangOut) {
                        invalidWeightHangOut *= 2;
                    }
                    stringWeights.AddElement("Valid", validWeightHangOut);
                    stringWeights.AddElement("Invalid", invalidWeightHangOut);

                    string resultHangOut = stringWeights.PickRandomElementGivenWeights();
                    if (resultHangOut == "Valid") {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_SCAVENGE_EVENT_FACTION:
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
                return character.specificLocation.HasStructure(STRUCTURE_TYPE.WORK_AREA);
            case INTERACTION_TYPE.SCRAP_ITEM:
                return character.specificLocation.possibleSpecialTokenSpawns.Count > 0;
            case INTERACTION_TYPE.MOVE_TO_RAID_EVENT_FACTION:
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
            case INTERACTION_TYPE.MOVE_TO_MINE_ACTION:
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    Area currArea = LandmarkManager.Instance.allAreas[i];
                    if (currArea.id != PlayerManager.Instance.player.playerArea.id && currArea.coreTile.landmarkOnTile.specificLandmarkType.ToString().Contains("MINE")) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_HARVEST_ACTION:
                for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                    Area currArea = LandmarkManager.Instance.allAreas[i];
                    if (currArea.id != PlayerManager.Instance.player.playerArea.id && currArea.coreTile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.FARM) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_OCCUPY_ACTION_FACTION:
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
            //case INTERACTION_TYPE.MOVE_TO_CURSE_ACTION:
            //    return character.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.ENEMY);
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
            case INTERACTION_TYPE.PICK_ITEM:
                if (!character.isHoldingItem) {
                    for (int i = 0; i < character.specificLocation.possibleSpecialTokenSpawns.Count; i++) {
                        SpecialToken token = character.specificLocation.possibleSpecialTokenSpawns[i];
                        if (token.structureLocation.structureType == STRUCTURE_TYPE.WAREHOUSE ||
                            (character.isAtHomeStructure && token.structureLocation == character.homeStructure)) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_EXPLORE_EVENT_FACTION:
                //**Trigger Criteria 1**: The character must not be holding an item
                return !character.isHoldingItem;
            case INTERACTION_TYPE.TORTURE_ACTION_NPC:
                return targetCharacter.GetTraitOr("Abducted", "Restrained") != null && character.specificLocation.id == targetCharacter.specificLocation.id;
            case INTERACTION_TYPE.ARGUE_ACTION:
                if(character.specificLocation.id == targetCharacter.specificLocation.id && targetCharacter.currentStructure.isInside) {
                    CharacterRelationshipData characterRelationshipData = character.GetCharacterRelationshipData(targetCharacter);
                    if(characterRelationshipData != null && !characterRelationshipData.isCharacterMissing) {
                        stringWeights = new WeightedDictionary<string>();
                        int validWeightArgue = 0;
                        int invalidWeightArgue = 50;
                        if(character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.SERVANT, RELATIONSHIP_TRAIT.RELATIVE)) {
                            validWeightArgue += 20;
                        }
                        if (character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.FRIEND)) {
                            validWeightArgue += 10;
                        }
                        if (character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.ENEMY)) {
                            validWeightArgue += 30;
                        }
                        if (character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.PARAMOUR)) {
                            validWeightArgue += 10;
                        }

                        if(character.GetTrait("Happy") != null) {
                            invalidWeightArgue = (int) (invalidWeightArgue * 1.5f);
                        }
                        if (character.GetTrait("Hungry") != null) {
                            validWeightArgue *= 3;
                        }
                        if (character.GetTrait("Tired") != null) {
                            validWeightArgue *= 3;
                        }
                        stringWeights.AddElement("Valid", validWeightArgue);
                        stringWeights.AddElement("Invalid", invalidWeightArgue);

                        string resultArgue = stringWeights.PickRandomElementGivenWeights();
                        if(resultArgue == "Valid") {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.CURSE_ACTION:
            case INTERACTION_TYPE.ASK_FOR_HELP:
                return character.specificLocation.id == targetCharacter.specificLocation.id;
            case INTERACTION_TYPE.REST_AT_HOME_ACTION:
            case INTERACTION_TYPE.EAT_HOME_MEAL_ACTION:
                //**Trigger Criteria 1**: Character is in his Home location
                return character.specificLocation.id == character.homeArea.id && character.homeStructure != null;
            case INTERACTION_TYPE.POISON_HOUSE_FOOD:
            case INTERACTION_TYPE.BOOBY_TRAP_HOUSE:
                //**Trigger Criteria 1**: target's home Dwelling is in the current location
                return character.specificLocation.id == targetCharacter.homeArea.id && targetCharacter.homeStructure != null;
            case INTERACTION_TYPE.FEED_PRISONER_ACTION:
                if (character.isAtHomeArea) {
                    List<LocationStructure> structures = character.specificLocation.GetStructuresAtLocation(true);
                    for (int i = 0; i < structures.Count; i++) {
                        LocationStructure currStructure = structures[i];
                        for (int j = 0; j < currStructure.charactersHere.Count; j++) {
                            Character currCharacter = currStructure.charactersHere[j];
                            if(currCharacter.GetTraitOr("Abducted", "Restrained") != null && currCharacter.GetTraitOr("Hungry", "Starving") != null) {
                                return true;
                            }
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.STEAL_ACTION_NPC:
                return targetCharacter.specificLocation.id == character.specificLocation.id && targetCharacter.isHoldingItem && !character.isHoldingItem;
            case INTERACTION_TYPE.ASSAULT_ACTION_NPC:
                return targetCharacter.specificLocation.id == character.specificLocation.id && targetCharacter.IsInOwnParty();
            case INTERACTION_TYPE.CAMP_OUT_ACTION:
                //**Trigger Criteria 1**: character is not in his Home location or character is Unaligned
                return !character.isAtHomeArea || character.isFactionless;
            case INTERACTION_TYPE.MAKE_LOVE_ACTION:
                //**Trigger Criteria 1**: the target must be in the character's current location and must also be in the target's home Dwelling
                if (targetCharacter.specificLocation.id != character.specificLocation.id 
                    || targetCharacter.homeStructure == null || targetCharacter.currentStructure != targetCharacter.homeStructure) {
                    return false;
                }
                ////**Trigger Criteria 2**: the actor must not be https://trello.com/c/GzhGi1XZ/1135-starving nor https://trello.com/c/I3gnHfsZ/1185-exhausted
                //if (character.GetTrait("Starving") != null 
                //    || character.GetTrait("Exhausted") != null) {
                //    return false;
                //}
                //**Trigger Criteria 3**: the target's **character missing** is False
                if (character.GetCharacterRelationshipData(targetCharacter).isCharacterMissing) {
                    return false;
                }

                stringWeights = new WeightedDictionary<string>();
                int validWeightMakeLove = 0;
                int invalidWeightMakeLove = 50;
                if (character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.FRIEND)) {
                    validWeightMakeLove += 5;
                }
                if (character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.LOVER)) {
                    validWeightMakeLove += 25;
                }
                if (character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.PARAMOUR)) {
                    validWeightMakeLove += 50;
                }

                bool isAnnoyedMakeLove = false, isHungryMakeLove = false, isTiredMakeLove = false, isSickMakeLove = false, isInjuredMakeLove = false;
                for (int i = 0; i < character.traits.Count; i++) {
                    if (character.traits[i].name == "Annoyed") { isAnnoyedMakeLove = true; } else if (character.traits[i].name == "Hungry") { isHungryMakeLove = true; } else if (character.traits[i].name == "Tired") { isTiredMakeLove = true; } else if (character.traits[i].name == "Sick") { isSickMakeLove = true; } else if (character.traits[i].name == "Injured") { isInjuredMakeLove = true; }
                }
                if (isAnnoyedMakeLove) {
                    invalidWeightMakeLove = (int) (invalidWeightMakeLove * 1.5f);
                }
                if (isHungryMakeLove) {
                    invalidWeightMakeLove = (int) (invalidWeightMakeLove * 1.5f);
                }
                if (isTiredMakeLove) {
                    invalidWeightMakeLove = (int) (invalidWeightMakeLove * 1.5f);
                }
                if (isSickMakeLove) {
                    invalidWeightMakeLove *= 2;
                }
                if (isInjuredMakeLove) {
                    invalidWeightMakeLove *= 2;
                }
                stringWeights.AddElement("Valid", validWeightMakeLove);
                stringWeights.AddElement("Invalid", invalidWeightMakeLove);

                string resultMakeLove = stringWeights.PickRandomElementGivenWeights();
                if (resultMakeLove == "Valid") {
                    return true;
                }
                return false;
            case INTERACTION_TYPE.REMOVE_CURSE_ACTION:
                if(character.characterClass.attackType == ATTACK_TYPE.MAGICAL && targetCharacter.GetTrait("Cursed") != null) {
                    return true;
                }
                return false;
            case INTERACTION_TYPE.RESTRAIN_CRIMINAL_ACTION:
                if (character.isAtHomeArea) {
                    List<LocationStructure> insideSettlementsRestrain = character.specificLocation.GetStructuresAtLocation(true);
                    for (int i = 0; i < insideSettlementsRestrain.Count; i++) {
                        LocationStructure currStructure = insideSettlementsRestrain[i];
                        for (int j = 0; j < currStructure.charactersHere.Count; j++) {
                            Character currCharacter = currStructure.charactersHere[j];
                            if(currCharacter.id != character.id && currCharacter.faction.id == character.faction.id 
                                && currCharacter.GetTrait("Criminal") != null && currCharacter.GetTrait("Restrained") == null) {
                                return true;
                            }
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION:
                return targetCharacter.GetTrait("Abducted") != null 
                    && character.specificLocation.id == character.GetCharacterRelationshipData(targetCharacter).knownStructure.location.id;
            case INTERACTION_TYPE.FIRST_AID_ACTION:
                return targetCharacter.GetTraitOr("Unconscious", "Sick") != null;
            case INTERACTION_TYPE.PROTECT_ACTION:
                if(targetCharacter.currentStructure.isInside && targetCharacter.GetTrait("Protected") != null) {
                    if(character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR, RELATIONSHIP_TRAIT.MASTER)) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.LOCATE_MISSING:
                CharacterRelationshipData relationshipData = character.GetCharacterRelationshipData(targetCharacter);
                return relationshipData != null && relationshipData.isCharacterMissing && relationshipData.isCharacterLocated && !relationshipData.HasRelationshipTrait(RELATIONSHIP_TRAIT.ENEMY);
            case INTERACTION_TYPE.CONSUME_PRISONER_ACTION:
                List<LocationStructure> insideSettlementsConsumePrisoner = character.specificLocation.GetStructuresAtLocation(true);
                for (int i = 0; i < insideSettlementsConsumePrisoner.Count; i++) {
                    LocationStructure currStructure = insideSettlementsConsumePrisoner[i];
                    for (int j = 0; j < currStructure.charactersHere.Count; j++) {
                        Character currCharacter = currStructure.charactersHere[j];
                        if (currCharacter.id != character.id && currCharacter.GetTraitOr("Abducted", "Restrained") != null) {
                            return true;
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_COURTESY_CALL:
                return character.faction.id != FactionManager.Instance.neutralFaction.id;
            case INTERACTION_TYPE.MOVE_TO_GIFT_ITEM:
                if(character.faction.id != FactionManager.Instance.neutralFaction.id){
                    if (character.isHoldingItem) {
                        return true;
                    } else {
                        List<LocationStructure> allWarehouses = character.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.WAREHOUSE);
                        if (allWarehouses != null && allWarehouses.Count > 0) {
                            for (int i = 0; i < allWarehouses.Count; i++) {
                                if (allWarehouses[i].itemsInStructure.Count > 0) {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            case INTERACTION_TYPE.MOVE_TO_GIFT_BEAST:
                for (int i = 0; i < character.specificLocation.areaResidents.Count; i++) {
                    Character resident = character.specificLocation.areaResidents[i];
                    if(resident.id != character.id && resident.role.roleType == CHARACTER_ROLE.BEAST && resident.faction.id == character.faction.id && resident.isIdle) {
                        return true;
                    }
                }
                return false;
            case INTERACTION_TYPE.HUNT_SMALL_ANIMALS:
                return (!character.isAtHomeArea || character.homeStructure == null) && character.GetTrait("Carnivore") != null;
            case INTERACTION_TYPE.FORAGE_ACTION:
                return (!character.isAtHomeArea || character.homeStructure == null) && character.GetTrait("Herbivore") != null;
            case INTERACTION_TYPE.EAT_INN_MEAL:
            case INTERACTION_TYPE.REST_AT_INN:
                if(!character.isAtHomeArea && character.role.roleType != CHARACTER_ROLE.BEAST 
                    && character.specificLocation.owner != null && character.faction.id != FactionManager.Instance.neutralFaction.id
                    && character.specificLocation.owner.id != character.faction.id && character.specificLocation.HasStructure(STRUCTURE_TYPE.INN)) {
                    factionRel = character.faction.GetRelationshipWith(character.specificLocation.owner);
                    if(factionRel.relationshipStatus != FACTION_RELATIONSHIP_STATUS.AT_WAR) {
                        return true;
                    }
                }
                return false;
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
        }
        return targetArea;
    }
    public Reward GetReward(string rewardName) {
        if (rewardConfig.ContainsKey(rewardName)) {
            RewardConfig config = rewardConfig[rewardName];
            return new Reward { rewardType = config.rewardType, amount = UnityEngine.Random.Range(config.lowerRange, config.higherRange + 1) };
        }
        throw new System.Exception("There is no reward configuration with name " + rewardName);
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
                    Interaction chosen = interactionsInvolved[UnityEngine.Random.Range(0, interactionsInvolved.Count)];
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
            GameManager.Instance.pauseTickEnded2 = true;
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
        GameManager.Instance.pauseTickEnded2 = false;
        dailyInteractionSummary = GameManager.Instance.TodayLogString() + "Scheduling interactions";
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            ScheduleDefaultInteractionsInArea(currArea, ref dailyInteractionSummary);
            //StartCoroutine(DefaultInteractionsInAreaCoroutine(currArea, AddToDailySummary));
        }
        dailyInteractionSummary += "\n==========Done==========";
        //Debug.Log(dailyInteractionSummary);
    }
    public void ScheduleDefaultInteractionsInArea(Area area, ref string log) {
        if (area.currentInteractions.Count <= 0) {
            log += "\nNo interactions in " + area.name;
            return;
        }
        GameDate scheduledDate = GameManager.Instance.Today();
        scheduledDate.AddTicks(Character_Action_Delay);
        log += "\n==========Scheduling <b>" + area.name + "'s</b> interactions on " + scheduledDate.ConvertToContinuousDays()  + "==========";
        List<Interaction> interactionsInArea = new List<Interaction>();
        for (int j = 0; j < area.currentInteractions.Count; j++) {
            Interaction currInteraction = area.currentInteractions[j];
            Character character = currInteraction.characterInvolved;
            if (!currInteraction.hasActivatedTimeOut) {
                if (character == null || (!character.isDead && currInteraction.CanInteractionBeDoneBy(character))) {
                    currInteraction.PreLoad();
                    log += "\nScheduling interaction " + currInteraction.type.ToString();
                    if (character != null) {
                        log += " Involving <b><color=green>" + character.name + "</color></b>";
                        character.OnForcedInteractionSubmitted(currInteraction);
                        character.SetPlannedAction(currInteraction);
                    }
                    interactionsInArea.Add(currInteraction);
                    log += "\n";
                } else {
                    //area.RemoveInteraction(currInteraction);
                    currInteraction.EndInteraction(false);
                    log += "\n<color=red>" + character.name + " is unable to perform " + currInteraction.name + "!</color>";
                    //Unable to perform
                    UnableToPerform unable = CreateNewInteraction(INTERACTION_TYPE.UNABLE_TO_PERFORM, area) as UnableToPerform;
                    unable.SetActionNameThatCannotBePerformed(currInteraction.name);
                    unable.SetCharacterInvolved(character);
                    unable.PreLoad();
                    unable.TimedOutRunDefault(ref log);
                    log += "\n";
                }
            }
        }
        if(interactionsInArea.Count > 0) {
            SchedulingManager.Instance.AddEntry(scheduledDate, () => DefaultInteractionsInArea(interactionsInArea, area));
        }
        area.currentInteractions.Clear();
    }
    private void DefaultInteractionsInArea(List<Interaction> interactions, Area area) {
        string log = "\n==========" + GameManager.Instance.TodayLogString() + "Executing Scheduled <b>" + area.name + "'s</b> interactions==========";
        //if (area.stopDefaultAllExistingInteractions) {
        //    log += "\nCannot run areas default interactions because area interactions have been disabled";
        //    return; //skip
        //}
        for (int j = 0; j < interactions.Count; j++) {
            Interaction currInteraction = interactions[j];
            Character character = currInteraction.characterInvolved;
            if (character != null) {
                log += "\n<b><color=green>" + character.name + "</color></b> triggered his/her day tick to perform <b>" + currInteraction.name + "</b>";
            }
        }

        for (int j = 0; j < interactions.Count; j++) {
            Interaction currInteraction = interactions[j];
            Character character = currInteraction.characterInvolved;
            if (!currInteraction.hasActivatedTimeOut) {
                if (character == null || currInteraction.CanStillDoInteraction(character)) {
                    log += "\nRunning interaction default " + currInteraction.type.ToString();
                    if (character != null) {
                        log += " Involving <b><color=green>" + character.name + "</color></b>";
                    }
                    if(character.currentInteractionTick <= GameManager.Instance.tick) {
                        character.AdjustDailyInteractionGenerationTick();
                    }
                    currInteraction.TimedOutRunDefault(ref log);
                    log += "\n";
                } else {
                    //area.RemoveInteraction(currInteraction);
                    currInteraction.EndInteraction(false);
                    log += "\n<color=red>" + currInteraction.name + " can no longer be done by " + character.name + "!</color>";
                    log += "\n";
                }
            }
        }
        log += "\n==========Done==========";
        //Debug.Log(log);
    }
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

    #region Intel
    public Intel CreateNewIntel(IPointOfInterest poi) {
        switch (poi.poiType) {
            case POINT_OF_INTEREST_TYPE.ITEM:
            case POINT_OF_INTEREST_TYPE.TILE_OBJECT:
                return new TileObjectIntel(poi);
            default:
                return new Intel();
        }
    }
    public Intel CreateNewIntel(params object[] obj) {
        if (obj[0] is GoapPlan) {
            return new PlanIntel(obj[1] as Character, obj[0] as GoapPlan);
        } else if (obj[0] is GoapAction) {
            return new EventIntel(obj[1] as Character, obj[0] as GoapAction);
        }
        return null;
    }
    #endregion

    #region Goap Action Utilities
    public LocationGridTile GetTargetLocationTile(ACTION_LOCATION_TYPE locationType, Character actor, LocationGridTile knownPOITargetLocation, params object[] other) {
        List<LocationGridTile> choices;
        LocationStructure specifiedStructure;
        LocationGridTile chosenTile = null;
        //Action Location says where the character will go to when performing the action:
        switch (locationType) {
            case ACTION_LOCATION_TYPE.IN_PLACE:
                //**In Place**: where he currently is
                chosenTile = actor.gridTileLocation;
                break;
            case ACTION_LOCATION_TYPE.NEARBY:
                //**Nearby**: an unoccupied tile within a 3 tile radius around the character
                choices = actor.specificLocation.areaMap.GetTilesInRadius(actor.gridTileLocation, 3).Where(x => !x.isOccupied).ToList();
                if (choices.Count > 0) {
                    chosenTile = choices[Utilities.rng.Next(0, choices.Count)];
                }
                break;
            case ACTION_LOCATION_TYPE.RANDOM_LOCATION:
                //**Random Location**: chooses a random unoccupied tile in the specified structure
                specifiedStructure = other[0] as LocationStructure;
                choices = specifiedStructure.unoccupiedTiles;
                if (choices.Count > 0) {
                    chosenTile = choices[Utilities.rng.Next(0, choices.Count)];
                }
                break;
            case ACTION_LOCATION_TYPE.RANDOM_LOCATION_B:
                //**Random Location B**: chooses a random unoccupied tile in the specified structure that is also adjacent to one other unoccupied tile
                specifiedStructure = other[0] as LocationStructure;
                choices = specifiedStructure.unoccupiedTiles.Where(x => x.UnoccupiedNeighbours.Count > 0).ToList();
                if (choices.Count > 0) {
                    chosenTile = choices[Utilities.rng.Next(0, choices.Count)];
                }
                break;
            case ACTION_LOCATION_TYPE.NEAR_TARGET:
                //**Near Target**: adjacent unoccupied tile beside the target item, tile object, character
                choices = knownPOITargetLocation.UnoccupiedNeighbours;
                if (choices.Count > 0) {
                    chosenTile = choices[Utilities.rng.Next(0, choices.Count)];
                }
                break;
            case ACTION_LOCATION_TYPE.ON_TARGET:
                //**On Target**: in the same tile as the target item or tile object
                if(knownPOITargetLocation.occupant == null || knownPOITargetLocation.occupant == actor) {
                    chosenTile = knownPOITargetLocation;
                }
                break;
            default:
                break;
        }
        //if (chosenTile != null && chosenTile.occupant != null) {
        //    throw new Exception(actor.name + " is going to an occupied tile!");
        //}
        return chosenTile;
    }
    #endregion

    public ObjectActivator<T> GetActivator<T> (ConstructorInfo ctor) {
        Type type = ctor.DeclaringType;
        ParameterInfo[] paramsInfo = ctor.GetParameters();

        //create a single param of type object[]
        ParameterExpression param =
            Expression.Parameter(typeof(object[]), "args");

        Expression[] argsExp =
            new Expression[paramsInfo.Length];

        //pick each arg from the params array 
        //and create a typed expression of them
        for (int i = 0; i < paramsInfo.Length; i++) {
            Expression index = Expression.Constant(i);
            Type paramType = paramsInfo[i].ParameterType;

            Expression paramAccessorExp =
                Expression.ArrayIndex(param, index);

            Expression paramCastExp =
                Expression.Convert(paramAccessorExp, paramType);

            argsExp[i] = paramCastExp;
        }

        //make a NewExpression that calls the
        //ctor with the args we just created
        NewExpression newExp = Expression.New(ctor, argsExp);

        //create a lambda with the New
        //Expression as body and our param object[] as arg
        LambdaExpression lambda =
            Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

        //compile it
        ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();
        return compiled;
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

public class InteractionAttributes {
    public INTERACTION_CATEGORY[] categories;
    public INTERACTION_ALIGNMENT alignment;
    public Precondition[] preconditions;
    public InteractionCharacterEffect[] actorEffect;
    public InteractionCharacterEffect[] targetCharacterEffect;
    public int cost;
}
public struct InteractionCharacterEffect {
    public INTERACTION_CHARACTER_EFFECT effect;
    public string effectString;
}