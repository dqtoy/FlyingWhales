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

    //public Queue<Interaction> interactionUIQueue { get; private set; }

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
    //private void Start() {
    //    interactionUIQueue = new Queue<Interaction>();
    //    Messenger.AddListener<Interaction>(Signals.CLICKED_INTERACTION_BUTTON, OnClickInteraction);
    //}
    public void Initialize() {
        //Messenger.AddListener(Signals.TICK_ENDED_2, ExecuteInteractionsDefault); //TryExecuteInteractionsDefault
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
            { INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER, new InteractionAttributes(){
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
                //if((type == INTERACTION_TYPE.USE_ITEM_ON_CHARACTER || type == INTERACTION_TYPE.USE_ITEM_ON_SELF) && actor != null && actor.isHoldingItem) {
                //    InteractionAttributes attributes = actor.tokenInInventory.interactionAttributes;
                //    return attributes;
                //}
            }
        }
        //Debug.LogWarning("No category and alignment for " + type.ToString());
        return null;
    }
    #endregion

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
            case INTERACTION_TYPE.TRANSFORM_TO_WOLF:
                goapAction = new TransformToWolfForm(actor, target);
                break;
            case INTERACTION_TYPE.REVERT_TO_NORMAL:
                goapAction = new RevertToNormalForm(actor, target);
                break;
            case INTERACTION_TYPE.EAT_CORPSE:
                goapAction = new EatCorpse(actor, target);
                break;
            case INTERACTION_TYPE.RESTRAIN_CHARACTER:
                goapAction = new RestrainCharacter(actor, target);
                break;
            case INTERACTION_TYPE.FIRST_AID_CHARACTER:
                goapAction = new FirstAidCharacter(actor, target);
                break;
            case INTERACTION_TYPE.CURE_CHARACTER:
                goapAction = new CureCharacter(actor, target);
                break;
            case INTERACTION_TYPE.CURSE_CHARACTER:
                goapAction = new CurseCharacter(actor, target);
                break;
            case INTERACTION_TYPE.DISPEL_MAGIC:
                goapAction = new DispelMagic(actor, target);
                break;
            case INTERACTION_TYPE.JUDGE_CHARACTER:
                goapAction = new JudgeCharacter(actor, target);
                break;
            case INTERACTION_TYPE.REPORT_CRIME:
                goapAction = new ReportCrime(actor, target);
                break;
            case INTERACTION_TYPE.FEED:
                goapAction = new Feed(actor, target);
                break;
            case INTERACTION_TYPE.STEAL_CHARACTER:
                goapAction = new StealFromCharacter(actor, target);
                break;
            case INTERACTION_TYPE.DROP_ITEM:
                goapAction = new DropItemHome(actor, target);
                break;
            case INTERACTION_TYPE.DROP_ITEM_WAREHOUSE:
                goapAction = new DropItemWarehouse(actor, target);
                break;
            case INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER:
                goapAction = new AskForHelpSaveCharacter(actor, target);
                break;
            case INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE:
                goapAction = new AskForHelpRemovePoisonTable(actor, target);
                break;
            case INTERACTION_TYPE.STAND:
                goapAction = new Stand(actor, target);
                break;
            case INTERACTION_TYPE.SIT:
                goapAction = new Sit(actor, target);
                break;
            case INTERACTION_TYPE.NAP:
                goapAction = new Nap(actor, target);
                break;
            case INTERACTION_TYPE.BURY_CHARACTER:
                goapAction = new BuryCharacter(actor, target);
                break;
            case INTERACTION_TYPE.CARRY_CORPSE:
                goapAction = new CarryCorpse(actor, target);
                break;
            case INTERACTION_TYPE.REMEMBER_FALLEN:
                goapAction = new RememberFallen(actor, target);
                break;
            case INTERACTION_TYPE.SPIT:
                goapAction = new Spit(actor, target);
                break;
            case INTERACTION_TYPE.REPORT_HOSTILE:
                goapAction = new ReportHostile(actor, target);
                break;
            case INTERACTION_TYPE.INVITE_TO_MAKE_LOVE:
                goapAction = new InviteToMakeLove(actor, target);
                break;
            case INTERACTION_TYPE.MAKE_LOVE:
                goapAction = new MakeLove(actor, target);
                break;
            case INTERACTION_TYPE.DRINK_BLOOD:
                goapAction = new DrinkBlood(actor, target);
                break;
            case INTERACTION_TYPE.REPLACE_TILE_OBJECT:
                goapAction = new ReplaceTileObject(actor, target);
                break;
            case INTERACTION_TYPE.CRAFT_FURNITURE:
                goapAction = new CraftFurniture(actor, target);
                break;
        }
        if(goapAction != null && willInitialize) {
            goapAction.Initialize();
        }
        return goapAction;
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
    //public void OnClickInteraction(Interaction interaction) {
    //    if(interaction != null) {
    //        interaction.CancelSecondTimeOut();
    //        InteractionUI.Instance.OpenInteractionUI(interaction);
    //    }
    //}
    //public void AddToInteractionQueue(Interaction interaction) {
    //    interactionUIQueue.Enqueue(interaction);
    //}

    //private void TryExecuteInteractionsDefault() {
    //    List<Character> trackedCharacters = PlayerManager.Instance.player.GetTrackedCharacters();
    //    if (trackedCharacters.Count > 0) {
    //        for (int i = 0; i < trackedCharacters.Count; i++) {
    //            Character currCharacter = trackedCharacters[i];
    //            List<Interaction> interactionsInvolved = GetAllValidInteractionsInvolving(currCharacter);
    //            for (int j = 0; j < interactionsInvolved.Count; j++) {
    //                Interaction interaction = interactionsInvolved[j];
    //                if (!interactionUIQueue.Contains(interaction)) {
    //                    AddToInteractionQueue(interaction);
    //                    Debug.Log(GameManager.Instance.TodayLogString() + " Added " + interaction.name + " from " + currCharacter.name + " to execute interactions queue.");
    //                }
    //            }
    //        }
    //    }

    //    List<Area> trackedAreas = PlayerManager.Instance.player.GetTrackedAreas();
    //    if (trackedAreas.Count > 0) {
    //        for (int i = 0; i < trackedAreas.Count; i++) {
    //            Area currArea = trackedAreas[i];
    //            List<Interaction> interactionsInvolved = GetAllValidInteractionsInvolving(currArea).Where(x => !interactionUIQueue.Contains(x)).ToList();
    //            if (interactionsInvolved.Count > 0) {
    //                Interaction chosen = interactionsInvolved[UnityEngine.Random.Range(0, interactionsInvolved.Count)];
    //                AddToInteractionQueue(chosen);
    //                Debug.Log(GameManager.Instance.TodayLogString() + " Added " + chosen.name + " from " + currArea.name + " to execute interactions queue.");
    //            }
    //        }
    //    }

    //    if (interactionUIQueue.Count > 0) {
    //        //set the last interaction to execute all defaults after it
    //        Interaction lastInteraction = interactionUIQueue.Last();
    //        lastInteraction.AddEndInteractionAction(() => ExecuteInteractionsDefault());
    //        //then show the first interaction, that will then start the line of queues
    //        Interaction interactionToShow = interactionUIQueue.Dequeue();
    //        InteractionUI.Instance.OpenInteractionUI(interactionToShow);
    //        GameManager.Instance.pauseTickEnded2 = true;
    //    } else {
    //        ExecuteInteractionsDefault();
    //    }
    //}

    //private List<Interaction> GetAllValidInteractionsInvolving(Character character) {
    //    List<Interaction> interactions = new List<Interaction>();
    //    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
    //        Area currArea = LandmarkManager.Instance.allAreas[i];
    //        for (int j = 0; j < currArea.currentInteractions.Count; j++) {
    //            Interaction currInteraction = currArea.currentInteractions[j];
    //            if (currInteraction.type == INTERACTION_TYPE.MOVE_TO_RETURN_HOME
    //                || currInteraction.type == INTERACTION_TYPE.CHARACTER_FLEES 
    //                || !currInteraction.CanInteractionBeDoneBy(currInteraction.characterInvolved)) {
    //                continue;
    //            }
    //            if ((currInteraction.targetCharacter != null && currInteraction.targetCharacter.id == character.id)
    //                || (currInteraction.characterInvolved != null && currInteraction.characterInvolved.id == character.id)) {
    //                interactions.Add(currInteraction);
    //            }
    //        }
    //    }
    //    return interactions;
    //}
    //private List<Interaction> GetAllValidInteractionsInvolving(Area currArea) {
    //    List<Interaction> interactions = new List<Interaction>();
    //    for (int i = 0; i < currArea.currentInteractions.Count; i++) {
    //        Interaction currInteraction = currArea.currentInteractions[i];
    //        if (currInteraction.type == INTERACTION_TYPE.MOVE_TO_RETURN_HOME
    //            || currInteraction.type == INTERACTION_TYPE.CHARACTER_FLEES
    //            || !currInteraction.CanInteractionBeDoneBy(currInteraction.characterInvolved)) {
    //            continue;
    //        }
    //        interactions.Add(currInteraction);
    //    }
    //    return interactions;
    //}

    //private void ExecuteInteractionsDefault() {
    //    GameManager.Instance.pauseTickEnded2 = false;
    //    dailyInteractionSummary = GameManager.Instance.TodayLogString() + "Scheduling interactions";
    //    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
    //        Area currArea = LandmarkManager.Instance.allAreas[i];
    //        ScheduleDefaultInteractionsInArea(currArea, ref dailyInteractionSummary);
    //        //StartCoroutine(DefaultInteractionsInAreaCoroutine(currArea, AddToDailySummary));
    //    }
    //    dailyInteractionSummary += "\n==========Done==========";
    //    //Debug.Log(dailyInteractionSummary);
    //}
    //public void ScheduleDefaultInteractionsInArea(Area area, ref string log) {
    //    if (area.currentInteractions.Count <= 0) {
    //        log += "\nNo interactions in " + area.name;
    //        return;
    //    }
    //    GameDate scheduledDate = GameManager.Instance.Today();
    //    scheduledDate.AddTicks(Character_Action_Delay);
    //    log += "\n==========Scheduling <b>" + area.name + "'s</b> interactions on " + scheduledDate.ConvertToContinuousDays()  + "==========";
    //    List<Interaction> interactionsInArea = new List<Interaction>();
    //    for (int j = 0; j < area.currentInteractions.Count; j++) {
    //        Interaction currInteraction = area.currentInteractions[j];
    //        Character character = currInteraction.characterInvolved;
    //        if (!currInteraction.hasActivatedTimeOut) {
    //            if (character == null || (!character.isDead && currInteraction.CanInteractionBeDoneBy(character))) {
    //                currInteraction.PreLoad();
    //                log += "\nScheduling interaction " + currInteraction.type.ToString();
    //                if (character != null) {
    //                    log += " Involving <b><color=green>" + character.name + "</color></b>";
    //                    character.OnForcedInteractionSubmitted(currInteraction);
    //                    character.SetPlannedAction(currInteraction);
    //                }
    //                interactionsInArea.Add(currInteraction);
    //                log += "\n";
    //            } else {
    //                //area.RemoveInteraction(currInteraction);
    //                currInteraction.EndInteraction(false);
    //                log += "\n<color=red>" + character.name + " is unable to perform " + currInteraction.name + "!</color>";
    //                //Unable to perform
    //                UnableToPerform unable = CreateNewInteraction(INTERACTION_TYPE.UNABLE_TO_PERFORM, area) as UnableToPerform;
    //                unable.SetActionNameThatCannotBePerformed(currInteraction.name);
    //                unable.SetCharacterInvolved(character);
    //                unable.PreLoad();
    //                unable.TimedOutRunDefault(ref log);
    //                log += "\n";
    //            }
    //        }
    //    }
    //    if(interactionsInArea.Count > 0) {
    //        SchedulingManager.Instance.AddEntry(scheduledDate, () => DefaultInteractionsInArea(interactionsInArea, area));
    //    }
    //    area.currentInteractions.Clear();
    //}
    //private void DefaultInteractionsInArea(List<Interaction> interactions, Area area) {
    //    string log = "\n==========" + GameManager.Instance.TodayLogString() + "Executing Scheduled <b>" + area.name + "'s</b> interactions==========";
    //    //if (area.stopDefaultAllExistingInteractions) {
    //    //    log += "\nCannot run areas default interactions because area interactions have been disabled";
    //    //    return; //skip
    //    //}
    //    for (int j = 0; j < interactions.Count; j++) {
    //        Interaction currInteraction = interactions[j];
    //        Character character = currInteraction.characterInvolved;
    //        if (character != null) {
    //            log += "\n<b><color=green>" + character.name + "</color></b> triggered his/her day tick to perform <b>" + currInteraction.name + "</b>";
    //        }
    //    }

    //    for (int j = 0; j < interactions.Count; j++) {
    //        Interaction currInteraction = interactions[j];
    //        Character character = currInteraction.characterInvolved;
    //        if (!currInteraction.hasActivatedTimeOut) {
    //            if (character == null || currInteraction.CanStillDoInteraction(character)) {
    //                log += "\nRunning interaction default " + currInteraction.type.ToString();
    //                if (character != null) {
    //                    log += " Involving <b><color=green>" + character.name + "</color></b>";
    //                }
    //                if(character.currentInteractionTick <= GameManager.Instance.tick) {
    //                    character.AdjustDailyInteractionGenerationTick();
    //                }
    //                currInteraction.TimedOutRunDefault(ref log);
    //                log += "\n";
    //            } else {
    //                //area.RemoveInteraction(currInteraction);
    //                currInteraction.EndInteraction(false);
    //                log += "\n<color=red>" + currInteraction.name + " can no longer be done by " + character.name + "!</color>";
    //                log += "\n";
    //            }
    //        }
    //    }
    //    log += "\n==========Done==========";
    //    //Debug.Log(log);
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
            GoapAction action = obj[0] as GoapAction;
            switch (action.goapType) {
                case INTERACTION_TYPE.TABLE_POISON:
                    return new PoisonTableIntel(obj[1] as Character, obj[0] as GoapAction);
                default:
                    return new EventIntel(obj[1] as Character, obj[0] as GoapAction);
            }

            
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
                choices = actor.specificLocation.areaMap.GetTilesInRadius(actor.gridTileLocation, 3).Where(x => !x.isOccupied && x.structure != null).ToList();
                if (choices.Count > 0) {
                    chosenTile = choices[Utilities.rng.Next(0, choices.Count)];
                }
                break;
            case ACTION_LOCATION_TYPE.RANDOM_LOCATION:
                //**Random Location**: chooses a random unoccupied tile in the specified structure
                specifiedStructure = other[0] as LocationStructure;
                choices = specifiedStructure.unoccupiedTiles.Where(x => x.reservedObjectType == TILE_OBJECT_TYPE.NONE).ToList();
                if (choices.Count > 0) {
                    chosenTile = choices[Utilities.rng.Next(0, choices.Count)];
                }
                break;
            case ACTION_LOCATION_TYPE.RANDOM_LOCATION_B:
                //**Random Location B**: chooses a random unoccupied tile in the specified structure that is also adjacent to one other unoccupied tile
                specifiedStructure = other[0] as LocationStructure;
                choices = specifiedStructure.unoccupiedTiles.Where(x => x.UnoccupiedNeighbours.Count > 0 && x.reservedObjectType == TILE_OBJECT_TYPE.NONE).ToList();
                if (choices.Count > 0) {
                    chosenTile = choices[Utilities.rng.Next(0, choices.Count)];
                }
                break;
            case ACTION_LOCATION_TYPE.NEAR_TARGET:
                //**Near Target**: adjacent unoccupied tile beside the target item, tile object, character
                choices = knownPOITargetLocation.UnoccupiedNeighbours.Where(x => x.structure != null).OrderBy(x => Vector2.Distance(actor.gridTileLocation.localLocation, x.localLocation)).ToList();
                if (choices.Where(x => x.charactersHere.Contains(actor)).Count() > 0) {
                    //if the actors current location is already part of the choices, stay in place
                    chosenTile = actor.gridTileLocation;
                } else if (choices.Count > 0) {
                    //chosenTile = choices[Utilities.rng.Next(0, choices.Count)];
                    chosenTile = choices[0];
                }
                break;
            case ACTION_LOCATION_TYPE.ON_TARGET:
                //**On Target**: in the same tile as the target item or tile object
                //if(knownPOITargetLocation.occupant == null || knownPOITargetLocation.occupant == actor) {
                chosenTile = knownPOITargetLocation;
                //}
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