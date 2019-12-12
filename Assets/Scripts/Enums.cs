using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Inner_Maps;
using Traits;

public enum PROGRESSION_SPEED {
    X1,
    X2,
    X4
}
public enum BIOMES{
    GRASSLAND,
    SNOW,
	TUNDRA,
	DESERT,
	//WOODLAND,
	FOREST,
	BARE,
	NONE,
    ANCIENT_RUIN,
}
public enum EQUATOR_LINE{
	HORIZONTAL,
	VERTICAL,
	DIAGONAL_LEFT,
	DIAGONAL_RIGHT,
}
public enum ELEVATION{
    PLAIN,
    MOUNTAIN,
	WATER,
    TREES,
}
public enum RACE{
    NONE,
	HUMANS,
	ELVES,
	MINGONS,
	CROMADS,
	UNDEAD,
    GOBLIN,
	TROLL,
	DRAGON,
	DEHKBRUG,
    WOLF,
    SLIME,
    BEAST,
    SKELETON,
    DEMON,
    FAERY,
    INSECT,
    SPIDER,
    ABOMINATION,
}
public enum HEXTILE_DIRECTION {
    NORTH_WEST,
    NORTH_EAST,
    EAST,
    SOUTH_EAST,
    SOUTH_WEST,
    WEST,
    NONE
}
public enum PATHFINDING_MODE{
	NORMAL,
    UNRESTRICTED,
    PASSABLE,
}
public enum GRID_PATHFINDING_MODE {
    NORMAL,
}

public enum GENDER{
	MALE,
	FEMALE,
}

public enum PRONOUN_TYPE {
    SUBJECTIVE,
    OBJECTIVE,
    POSSESSIVE,
    REFLEXIVE,
}
public enum MONTH{
	NONE,
	JAN,
	FEB,
	MAR,
	APR,
	MAY,
	JUN,
	JUL,
	AUG,
	SEP,
	OCT,
	NOV,
	DEC,
}
public enum LANGUAGES{
	NONE,
	ENGLISH,
}
public enum DIRECTION{
	LEFT,
	RIGHT,
	UP,
	DOWN,
}
public enum LOG_IDENTIFIER{
	NONE,
	ACTIVE_CHARACTER,
	FACTION_1,
	FACTION_LEADER_1,
    //KING_1_SPOUSE,
    LANDMARK_1,
    PARTY_1,
    //RANDOM_CITY_1,
    //RANDOM_GOVERNOR_1,
    TARGET_CHARACTER,
	FACTION_2,
	FACTION_LEADER_2,
	//KING_2_SPOUSE,
	LANDMARK_2,
    PARTY_2,
    //RANDOM_CITY_2,
    //RANDOM_GOVERNOR_2,
    CHARACTER_3,
	FACTION_3,
	FACTION_LEADER_3,
	//KING_3_SPOUSE,
	LANDMARK_3,
    PARTY_3,
    //RANDOM_CITY_3,
    //RANDOM_GOVERNOR_3,
    ACTION_DESCRIPTION,
    QUEST_NAME,
	ACTIVE_CHARACTER_PRONOUN_S,
	ACTIVE_CHARACTER_PRONOUN_O,
	ACTIVE_CHARACTER_PRONOUN_P,
	ACTIVE_CHARACTER_PRONOUN_R,
	FACTION_LEADER_1_PRONOUN_S,
	FACTION_LEADER_1_PRONOUN_O,
	FACTION_LEADER_1_PRONOUN_P,
	FACTION_LEADER_1_PRONOUN_R,
	FACTION_LEADER_2_PRONOUN_S,
	FACTION_LEADER_2_PRONOUN_O,
	FACTION_LEADER_2_PRONOUN_P,
	FACTION_LEADER_2_PRONOUN_R,
	TARGET_CHARACTER_PRONOUN_S,
	TARGET_CHARACTER_PRONOUN_O,
	TARGET_CHARACTER_PRONOUN_P,
	TARGET_CHARACTER_PRONOUN_R,
    MINION_1_PRONOUN_S,
    MINION_1_PRONOUN_O,
    MINION_1_PRONOUN_P,
    MINION_1_PRONOUN_R,
    MINION_2_PRONOUN_S,
    MINION_2_PRONOUN_O,
    MINION_2_PRONOUN_P,
    MINION_2_PRONOUN_R,
    //SECESSION_CITIES,
    TASK,
	DATE,
	FACTION_LEADER_3_PRONOUN_S,
	FACTION_LEADER_3_PRONOUN_O,
	FACTION_LEADER_3_PRONOUN_P,
	FACTION_LEADER_3_PRONOUN_R,
    ITEM_1,
    ITEM_2,
    ITEM_3,
    COMBAT,
    //PARTY_NAME,
    OTHER,
    STRING_1,
    STRING_2,
    MINION_1,
    MINION_2,
    CHARACTER_LIST_1,
    CHARACTER_LIST_2,
    STRUCTURE_1,
    STRUCTURE_2,
    STRUCTURE_3,
    APPEND,
    OTHER_2,
}
public enum STRUCTURE_STATE {
    NORMAL,
    RUINED,
}
public enum WAR_SIDE{
	NONE,
	A,
	B,
}
public enum ROAD_TYPE{
    NONE,
    MAJOR,
	MINOR,
	ALL,
}
public enum LANDMARK_TAG {
    CAN_HUNT,
    CAN_SCAVENGE,
}
public enum LANDMARK_TYPE {
    NONE = 0,
    THE_PORTAL = 1,
    WORKSHOP = 4,
    MINES = 8,
    FARM = 17,
    PALACE = 20,
    BANDIT_CAMP = 24,
    MAGE_TOWER = 25,
    TEMPLE = 30,
    CAVE = 31,
    BARRACKS = 34,
    MONSTER_LAIR = 42,
    THE_SPIRE = 48,
    THE_CRYPT = 49,
    THE_KENNEL = 50,
    THE_ANVIL = 51,
    THE_FINGERS = 52,
    THE_EYE = 53,
    THE_PROFANE = 54,
    THE_NEEDLES = 55,
    THE_PIT = 56,
    LUMBERYARD = 57,
    QUARRY = 58,
}
public enum TECHNOLOGY {
    //Weapon Production
    BOW_MAKING,
    SWORD_MAKING,
    SPEAR_MAKING,
    DAGGER_MAKING,
    AXE_MAKING,
    STAFF_MAKING,

    //Armor Production
    CHEST_ARMOR_MAKING,
    LEGGINGS_MAKING,
    HELMET_MAKING,
    GLOVE_MAKING,
    BOOT_MAKING,

    //Construction
    BASIC_FARMING,
    ADVANCED_FARMING,
    BASIC_HUNTING,
    ADVANCED_HUNTING,
    BASIC_MINING,
    ADVANCED_MINING,
    BASIC_WOODCUTTING,
    ADVANCED_WOODCUTTING,
    BASIC_QUARRYING,
    ADVANCED_QUARRYING,

    //Training Tier 1
    ARCHER_CLASS,
    SWORDSMAN_CLASS,
    SPEARMAN_CLASS,
    WILDLING_CLASS,
    ROGUE_CLASS,
    MAGE_CLASS,

    //Training Tier 2
    RANGER_CLASS,
    BATTLEMAGE_CLASS,
    SCOUT_CLASS,
    BARBARIAN_CLASS,
    KNIGHT_CLASS,
    ARCANIST_CLASS,
    NIGHTBLADE_CLASS,

    //Unlock character roles
    ESPIONAGE,
    DIPLOMACY,
    NECROMANCY,
    DRAGON_TAMING,

    //Miscellaneous
    GOBLIN_LANGUAGE,
    ELVEN_LANGUAGE,
    HUMAN_LANGUAGE,
    TROLL_LANGUAGE,

	NONE,
}
public enum CHARACTER_ROLE {
    NONE,
    CIVILIAN,
    PLAYER,
    BANDIT,
    LEADER,
    BEAST,
    NOBLE,
    SOLDIER,
    ADVENTURER,
    MINION,
}
public enum CHARACTER_CLASS {
    WARRIOR,
    BARBARIAN,
    SHOPKEEPER,
    MINER,
    WOODCUTTER,
    FARMER,
    RETIRED_HERO
}
public enum QUEST_TYPE { 
    RELEASE_CHARACTER,
    BUILD_STRUCTURE,
    FETCH_ITEM,
    SURRENDER_ITEMS,
}
public enum FACTION_RELATIONSHIP_STATUS {
    FRIENDLY,
    HOSTILE,
    COLD_WAR,
}
//---------------------------------------- ENTITY COMPONENT SYSTEM ---------------------------------------//
//public enum BODY_PART{
//	HEAD,
//	TORSO,
//	TAIL,
//	ARM,
//	HAND,
//	LEG,
//	FEET,
//    HEART,
//    BRAIN,
//	EYE,
//	NOSE,
//	EAR,
//	ELBOW,
//	WRIST,
//	FINGER,
//	THIGH,
//	KNEE,
//	SHIN,
//	BREAST,
//	ABS,
//	RIB,
//	MOUTH,
//	WING,
//	HORN,
//	HIP,
//	CROTCH,
//	ASS,
//	PELVIS,
//}

public enum CHARACTER_CLASS_TYPE {
    GENERAL,
	NINJA
}

public enum SKILL_TYPE {
    ATTACK,
    HEAL,
    OBTAIN_ITEM,
    FLEE,
    MOVE
}
public enum SKILL_CATEGORY {
	GENERAL,
	CLASS,
}

public enum ATTACK_TYPE {
    PHYSICAL,
    MAGICAL,
}
public enum RANGE_TYPE {
    MELEE,
    RANGED,
}
public enum DEFEND_TYPE {
    DODGE,
    PARRY,
    BLOCK,
	NONE,
}
public enum DAMAGE_TYPE {
    NONE,
    SLASH,
    PIERCE,
    MAGIC,
    BASH,
}
public enum STATUS_EFFECT {
    NONE,
	INJURED,
	DECAPITATED,
    POISONED,
    STUNNED,
    BLEEDING,
    BURNING,
	CONFUSED,
}
public enum ITEM_TYPE{
	WEAPON,
	ARMOR,
	CONSUMABLE,
	KEY,
    JUNK,
    ACCESSORY,
}

public enum EQUIPMENT_TYPE {
    NONE,
    SWORD,
    DAGGER,
	SPEAR,
    BOW,
	STAFF,
	AXE,
    SHIRT,
    BRACER,
    HELMET,
	LEGGINGS,
	BOOT,
    GREAT_SWORD,
}

public enum WEAPON_TYPE{
	NONE = EQUIPMENT_TYPE.NONE,
	SWORD = EQUIPMENT_TYPE.SWORD,
	DAGGER = EQUIPMENT_TYPE.DAGGER,
	SPEAR = EQUIPMENT_TYPE.SPEAR,
	BOW = EQUIPMENT_TYPE.BOW,
    STAFF = EQUIPMENT_TYPE.STAFF,
	AXE = EQUIPMENT_TYPE.AXE,
    GREAT_SWORD = EQUIPMENT_TYPE.GREAT_SWORD,
}

public enum ARMOR_TYPE{
	NONE = EQUIPMENT_TYPE.NONE,
	SHIRT = EQUIPMENT_TYPE.SHIRT,
	BRACER = EQUIPMENT_TYPE.BRACER,
	HELMET = EQUIPMENT_TYPE.HELMET,
	LEGGINGS = EQUIPMENT_TYPE.LEGGINGS,
	BOOT = EQUIPMENT_TYPE.BOOT,
}
public enum QUALITY{
	NORMAL,
	CRUDE,
	EXCEPTIONAL,
}
public enum ATTRIBUTE{
    SAPIENT,

    HUNGRY,
    STARVING,

    FAMISHED,

    TIRED,
    EXHAUSTED,

    SAD,
    DEPRESSED,
    
    INSECURE,
    DRUNK,

    DISTURBED,
    CRAZED,

    ANXIOUS,
    DEMORALIZED,

    WOUNDED,
    WRECKED,

    IMPULSIVE,
    BETRAYED,
    HEARTBROKEN,

    GREGARIOUS,
    BOOKWORM,
    DEAFENED,
    MUTE,
    SINGER,
    ROYALTY,
    DAYDREAMER,
    MEDITATOR,
    LIBERATED,
    UNFAITHFUL,
    DIRTY,
    CLEANER,
    DO_NOT_DISTURB,
    INTROVERT,
    EXTROVERT,
    BELLIGERENT,
    HUMAN,
    STALKER,
    SPOOKED,
    MARKED,
}
public enum PRODUCTION_TYPE{
	WEAPON,
	ARMOR,
	CONSTRUCTION,
	TRAINING
}
public enum LOCATION_IDENTIFIER{
	LANDMARK,
	HEXTILE,
}
public enum MODE {
    DEFAULT,
    ALERT,
    STEALTH
}
public enum ACTION_TYPE {
    REST,
    MOVE_TO,
    HUNT,
    DESTROY,
    EAT,
    BUILD,
    REPAIR,
    DRINK,
    HARVEST,
    IDLE,
    POPULATE,
    TORTURE,
    PATROL,
    ABDUCT,
    DISPEL,
    PRAY,
    ATTACK,
    JOIN_BATTLE,
    GO_HOME,
    RELEASE,
    CLEANSE,
    ENROLL,
    TRAIN,
    DAYDREAM,
    BERSERK,
    PLAY,
    FLAIL,
    SELFMUTILATE,
    DEPOSIT,
    CHANGE_CLASS,
    CHAT,
    WAITING,
    DISBAND_PARTY,
    FORM_PARTY,
    IN_PARTY,
    JOIN_PARTY,
    GRIND,
    MINING,
    WOODCUTTING,
    WORKING,
    PARTY,
    READ,
    SING,
    PLAYING_INSTRUMENT,
    MEDITATE,
    FOOLING_AROUND,
    HOUSEKEEPING,
    ARGUE,
    STALK,
    QUESTING,
    FETCH,
    WAIT_FOR_PARTY,
    TURN_IN_QUEST,
    SUICIDE,
    DEFEND,
    RESEARCH,
    GIVE_ITEM,
    ATTACK_LANDMARK,
    HIBERNATE,
    DEFENDER,
    RAID_LANDMARK,
}
public enum ACTION_CATEGORY {
    DIRECT,
    INDIRECT,
    CONSUME,
}

public enum OBJECT_TYPE {
    CHARACTER,
    STRUCTURE,
    ITEM,
    NPC,
    LANDMARK,
    MONSTER,
}

public enum ACTION_RESULT {
    SUCCESS,
    FAIL,
}
public enum NEEDS {
    FULLNESS,
    ENERGY,
    FUN,
    PRESTIGE,
    SANITY,
    SAFETY,
}

public enum ACTION_FILTER_TYPE {
    ROLE,
    LOCATION,
    CLASS,
}
public enum ACTION_FILTER_CONDITION {
    IS,
    IS_NOT,
}

public enum ACTION_FILTER {
    HERO,
    VILLAIN,
    RUINED,
    CIVILIAN,
    FARMER,
    MINER,
    WOODCUTTER,
}

public enum PREREQUISITE {
    RESOURCE,
    ITEM,
    POWER,
}

public enum PASSABLE_TYPE {
    UNPASSABLE,
    MAJOR_DEADEND,
    MINOR_DEADEND,
    MAJOR_BOTTLENECK,
    MINOR_BOTTLENECK,
    CROSSROAD,
    WIDE_OPEN,
    OPEN
}
public enum LEVEL {
    HIGH,
    AVERAGE,
    LOW
}
public enum BASE_AREA_TYPE {
    SETTLEMENT,
    DUNGEON,
    PLAYER,
}
public enum LOCATION_TYPE {
    ELVEN_SETTLEMENT,
    HUMAN_SETTLEMENT,
    DEMONIC_INTRUSION,
    DUNGEON,
    EMPTY,
}
public enum ELEMENT {
    NONE,
    FIRE,
    WATER,
    EARTH,
    WIND,
}

public enum MONSTER_TYPE {
    SLIME,
    BEAST,
    FLORAL,
    INSECT,
    HUMANOID,
    DEMON,
}

public enum MONSTER_CATEGORY {
    NORMAL,
    BOSS,
}

public enum ATTACK_CATEGORY {
    PHYSICAL,
    MAGICAL,
}

public enum WEAPON_PREFIX {
    NONE,
}

public enum WEAPON_SUFFIX {
    NONE,
}

public enum ARMOR_PREFIX {
    NONE,
}

public enum ARMOR_SUFFIX {
    NONE,
}

public enum MESSAGE_BOX_MODE {
    MESSAGE_ONLY,
    YES_NO
}

public enum ICHARACTER_TYPE {
    CHARACTER,
    MONSTER,
}

public enum MOVEMENT_TYPE {
    NORMAL,
    AVOID
}

public enum TARGET_TYPE {
    SINGLE,
    PARTY,
}
public enum ATTRIBUTE_CATEGORY {
    CHARACTER,
    ITEM,
    STRUCTURE,
}
public enum ATTRIBUTE_BEHAVIOR {
    NONE,
    DEPLETE_FUN,
}
public enum SCHEDULE_PHASE_TYPE {
    WORK,
    MISC,
    SPECIAL,
}
public enum SCHEDULE_ACTION_CATEGORY {
    NONE,
    REST,
    WORK,
}
public enum ABUNDANCE {
    NONE,
    HIGH,
    MED,
    LOW,
}
public enum HIDDEN_DESIRE {
    NONE,
    SECRET_AFFAIR,
    RESEARCH_SCROLL,
}
public enum GAME_EVENT {
    SECRET_MEETING,
    MONSTER_ATTACK,
    TEST_EVENT,
    DRAGON_ATTACK,
    SUICIDE,
    RESEARCH_SCROLLS,
    PARTY_EVENT,
}
public enum EVENT_PHASE {
    PREPARATION,
    PROPER,
}
public enum ABILITY_TYPE {
    ALL,
    CHARACTER,
    STRUCTURE,
    MONSTER,
}
public enum QUEST_GIVER_TYPE {
    QUEST_BOARD,
    CHARACTER,
}
public enum COMBATANT_TYPE {
    CHARACTER,
    ARMY, //Party
}
public enum CURRENCY {
    SUPPLY,
    MANA,
    IMP,
}
public enum STAT {
    HP,
    ATTACK,
    SPEED,
    POWER,
    ALL,
}
public enum DAMAGE_IDENTIFIER {
    DEALT,
    RECEIVED,
}
public enum TRAIT_REQUIREMENT {
    RACE,
    TRAIT,
    ADJACENT_ALLIES,
    FRONTLINE,
    ONLY_1_TARGET,
    EVERY_MISSING_HP_25PCT,
    MELEE,
    RANGED,
    OPPOSITE_SEX,
    ONLY_DEMON,
    ROLE,
}
public enum MORALITY {
    GOOD,
    EVIL,
    NEUTRAL,
}
public enum FACTION_SIZE {
    MAJOR,
    MINOR
}
public enum FACTION_TYPE {
    HOSTILE,
    BALANCED,
    DEFENSIVE,
}
public enum MINIONS_SORT_TYPE {
    DEFAULT,
    LEVEL,
    TYPE,
}
public enum INTERACTION_TYPE {
    NONE,
    RETURN_HOME,
    DROP_ITEM,
    ABDUCT_CHARACTER,
    PICK_UP,
    RELEASE_CHARACTER,
    CRAFT_ITEM,
    MINE,
    ASK_FOR_HELP_SAVE_CHARACTER,
    ASSAULT,
    TRANSFORM_TO_WOLF_FORM,
    REVERT_TO_NORMAL_FORM,
    SLEEP,
    IMPRISON_CHARACTER,
    EAT,
    DAYDREAM,
    PLAY_GUITAR,
    CHAT_CHARACTER,
    DRINK,
    SLEEP_OUTSIDE,
    REMOVE_POISON,
    POISON,
    PRAY,
    CHOP_WOOD,
    STEAL,
    SCRAP,
    MAGIC_CIRCLE_PERFORM_RITUAL,
    GET_SUPPLY,
    DROP_RESOURCE,
    RETURN_HOME_LOCATION,
    PLAY,
    RESTRAIN_CHARACTER,
    FIRST_AID_CHARACTER,
    CURE_CHARACTER,
    CURSE_CHARACTER,
    DISPEL_MAGIC,
    JUDGE_CHARACTER,
    FEED,
    DROP_ITEM_WAREHOUSE,
    ASK_FOR_HELP_REMOVE_POISON_TABLE,
    SIT,
    STAND,
    NAP,
    BURY_CHARACTER,
    CARRY_CORPSE,
    REMEMBER_FALLEN,
    SPIT,
    REPORT_HOSTILE,
    MAKE_LOVE,
    INVITE,
    DRINK_BLOOD,
    REPLACE_TILE_OBJECT,
    CRAFT_FURNITURE,
    TANTRUM,
    BREAK_UP,
    SHARE_INFORMATION,
    WATCH,
    INSPECT,
    PUKE,
    SEPTIC_SHOCK,
    ZOMBIE_DEATH,
    CARRY,
    DROP,
    KNOCKOUT_CHARACTER,
    RITUAL_KILLING,
    RESOLVE_CONFLICT,
    GET_WATER,
    STUMBLE,
    ACCIDENT,
    OBTAIN_RESOURCE,
    DROP_FOOD,
    BUTCHER,
    ASK_TO_STOP_JOB,
    WELL_JUMP,
    STRANGLE,
    REPAIR,
    NARCOLEPTIC_NAP,
    SHOCK,
    CRY,
    CRAFT_TILE_OBJECT,
    PRAY_TILE_OBJECT,
    HAVE_AFFAIR,
    SLAY_CHARACTER,
    LAUGH_AT,
    FEELING_CONCERNED,
    TEASE,
    FEELING_SPOOKED,
    FEELING_BROKENHEARTED,
    GRIEVING,
    GO_TO,
    SING,
    DANCE,
    DESTROY_RESOURCE,
    SCREAM_FOR_HELP,
    REACT_TO_SCREAM,
    RESOLVE_COMBAT,
    CHANGE_CLASS,
    VISIT,
    PLACE_BLUEPRINT,
    BUILD_STRUCTURE,
    STEALTH_TRANSFORM,
    HARVEST_PLANT,
    REPAIR_STRUCTURE,
    HARVEST_FOOD_REGION,
    FORAGE_FOOD_REGION,
    CHOP_WOOD_REGION,
    MINE_METAL_REGION,
    MINE_STONE_REGION,
    CLAIM_REGION,
    CLEANSE_REGION,
    INVADE_REGION,
    CORRUPT_CULTIST,
    DEMONIC_INCANTATION,
    HOLY_INCANTATION,
    STUDY,
    OUTSIDE_SETTLEMENT_IDLE,
    ATTACK_REGION
}
public enum INTERACTION_ALIGNMENT {
    EVIL,
    NEUTRAL,
    GOOD,
}
public enum INTERACTION_CATEGORY {
    INVENTORY,
    RECRUITMENT,
    PERSONAL,
    OFFENSE,
    DEFENSE,
    SOCIAL,
    SUBTERFUGE,
    SUPPLY,
    DIPLOMACY,
    EXPANSION,
    FULLNESS_RECOVERY,
    TIREDNESS_RECOVERY,
    ROMANTIC,
    SAVE,
    SABOTAGE,
    ASSISTANCE,
    PERSONAL_EMPOWERMENT,
    WORK,
    OTHER,
}
public enum REWARD {
    SUPPLY,
    MANA,
    LEVEL,
}
public enum TRAIT_TYPE {
    ILLNESS,
    ATTACK,
    ABILITY,
    STATUS,
    COMBAT_POSITION,
    ENCHANTMENT,
    DISABLER,
    RACIAL,
    EMOTION,
    CRIMINAL,
    SPECIAL,
    RELATIONSHIP,
    BUFF,
    FLAW,
    PERSONALITY,
}
public enum TRAIT_EFFECT {
    NEUTRAL,
    POSITIVE,
    NEGATIVE,
}
public enum TRAIT_TRIGGER {
    OUTSIDE_COMBAT,
    START_OF_COMBAT,
    DURING_COMBAT,
}
public enum TRAIT_REQUIREMENT_SEPARATOR {
    OR,
    AND,
}
public enum TRAIT_REQUIREMENT_TARGET {
    SELF,
    ENEMY, //TARGET
    OTHER_PARTY_MEMBERS,
    ALL_PARTY_MEMBERS,
    ALL_IN_COMBAT,
    ALL_ENEMIES,
}
public enum TRAIT_REQUIREMENT_CHECKER {
    SELF,
    ENEMY, //TARGET
    OTHER_PARTY_MEMBERS,
    ALL_PARTY_MEMBERS,
    ALL_IN_COMBAT,
    ALL_ENEMIES,
}
public enum JOB {
    NONE,
    INSTIGATOR,
    EXPLORER,
    DIPLOMAT,
    SEDUCER,
    RAIDER,
    SPY,
    DEBILITATOR,
    LEADER,
    WORKER,
}
public enum RESULT {
    SUCCESS,
    FAIL,
    CRITICAL_FAIL
}
public enum RACE_SUB_TYPE {
    NORMAL,
    BANDIT,
}
public enum TOKEN_TYPE {
    CHARACTER,
    LOCATION,
    FACTION,
    SPECIAL,
}
public enum SPECIAL_TOKEN {
    BLIGHTED_POTION,
    BOOK_OF_THE_DEAD,
    CHARM_SPELL,
    FEAR_SPELL,
    MARK_OF_THE_WITCH,
    BRAND_OF_THE_BEASTMASTER,
    BOOK_OF_WIZARDRY,
    SECRET_SCROLL,
    MUTAGENIC_GOO,
    DISPEL_SCROLL,
    PANACEA,
    JUNK,
    HEALING_POTION,
    ENCHANTED_AMULET,
    GOLDEN_NECTAR,
    SCROLL_OF_POWER,
    ACID_FLASK,
    SCROLL_OF_FRENZY,
    TOOL,
    WATER_BUCKET,
}
public enum COMBAT_POSITION {
    FRONTLINE,
    BACKLINE,
}
public enum COMBAT_TARGET {
    SINGLE,
    ALL,
    FRONTROW,
    BACKROW, //N/A
    ROW,
    COLUMN,
    SINGLE_FRONTROW,
    SINGLE_BACKROW, //N/A
}
public enum COMBAT_OCCUPIED_TILE {
    SINGLE,
    COLUMN,
    ROW,
    ALL,
}
public enum JOB_ACTION_TARGET {
    NONE,
    CHARACTER,
    TILE_OBJECT,
    TILE,
}
public enum STRUCTURE_TYPE {
    EXIT,
    INN,
    WAREHOUSE,
    DWELLING,
    DUNGEON,
    WILDERNESS,
    WORK_AREA,
    EXPLORE_AREA,
    CEMETERY,
    PRISON,
    POND,
    CITY_CENTER,
    SMITHY,
    BARRACKS,
    APOTHECARY,
    GRANARY,
    MINER_CAMP,
    RAIDER_CAMP,
    ASSASSIN_GUILD,
    HUNTER_LODGE,
    MAGE_QUARTERS,
    NONE,
}
public enum RELATIONSHIP_TRAIT {
    NONE = 0,
    ENEMY = 1,
    FRIEND = 2,
    RELATIVE = 3,
    LOVER = 4,
    PARAMOUR = 5,
    MASTER = 6,
    SERVANT = 7,
    SAVER = 8,
    SAVE_TARGET = 9,
    EX_LOVER = 10,
}

public enum POINT_OF_INTEREST_TYPE {
    ITEM,
    CHARACTER,
    TILE_OBJECT,
}
public enum TILE_OBJECT_TYPE {
    WOOD_PILE = 0,
    SMALL_ANIMAL = 2,
    EDIBLE_PLANT = 3,
    GUITAR = 4,
    MAGIC_CIRCLE = 5,
    TABLE = 6,
    BED = 7,
    ORE = 8,
    TREE_OBJECT = 9,
    FOOD = 10,
    DESK = 11,
    TOMBSTONE = 12,
    NONE = 13,
    MUSHROOM = 14,
    NECRONOMICON = 15,
    CHAOS_ORB = 16,
    HERMES_STATUE = 17,
    ANKH_OF_ANUBIS = 18,
    MIASMA_EMITTER = 19,
    WATER_WELL = 20,
    GENERIC_TILE_OBJECT = 21,
    FOOD_PILE = 22,
    GODDESS_STATUE = 23,
    BUILD_SPOT_TILE_OBJECT = 24,
    STONE_PILE = 25,
    METAL_PILE = 26,
    TORNADO = 27,
    REGION_TILE_OBJECT = 28,
}
public enum POI_STATE {
    ACTIVE,
    INACTIVE,
}
public enum INTERACTION_CHARACTER_EFFECT {
    NONE,
    TRAIT_GAIN,
    TRAIT_REMOVE,
    OBTAIN_ITEM,
    OBTAIN_SUPPLY,
    FULLNESS_RECOVERY,
    DEATH,
    CHANGE_FACTION,
    LOSE_ITEM,
    CHANGE_HOME,
    TIREDNESS_RECOVERY,
}
public enum TARGET_POI { ACTOR, TARGET, }
public enum GridNeighbourDirection { North, South, West, East, North_West,  North_East, South_West, South_East }
public enum TIME_IN_WORDS { AFTER_MIDNIGHT, AFTER_MIDNIGHT_1, AFTER_MIDNIGHT_2, MORNING, MORNING_1, MORNING_2, AFTERNOON, AFTERNOON_1, AFTERNOON_2, EARLY_NIGHT, LATE_NIGHT, NIGHT_1, NIGHT_2, LUNCH_TIME, NONE }
//public enum CRIME_SEVERITY { NONE, INFRACTION, MISDEMEANOUR, SERIOUS_CRIME, }
public enum FOOD { BERRY, MUSHROOM, RABBIT, RAT }
public enum GOAP_EFFECT_CONDITION { NONE, REMOVE_TRAIT, HAS_TRAIT, HAS_WOOD, HAS_STONE, HAS_METAL, HAS_ITEM, FULLNESS_RECOVERY, TIREDNESS_RECOVERY, HAPPINESS_RECOVERY, CANNOT_MOVE, IN_PARTY, REMOVE_FROM_PARTY, DESTROY, DEATH, PATROL, EXPLORE, REMOVE_ITEM, HAS_TRAIT_EFFECT, HAS_PLAN, HAS_FOOD
        , TARGET_REMOVE_RELATIONSHIP, TARGET_STOP_ACTION_AND_JOB, RESTRAIN_CARRY, REMOVE_FROM_PARTY_NO_CONSENT, IN_VISION, REDUCE_HP, INVITED, MAKE_NOISE, STARTS_COMBAT, CHANGE_CLASS
        , PRODUCE_FOOD, PRODUCE_WOOD, PRODUCE_STONE, PRODUCE_METAL, DEPOSIT_RESOURCE, REMOVE_REGION_CORRUPTION, CLEAR_REGION_FACTION_OWNER, REGION_OWNED_BY_ACTOR_FACTION,
        FACTION_QUEST_DURATION_INCREASE,
        FACTION_QUEST_DURATION_DECREASE,
        DESTROY_REGION_LANDMARK,
        CHARACTER_TO_MINION
}
public enum GOAP_EFFECT_TARGET { ACTOR, TARGET, }
public enum GOAP_PLAN_STATE { IN_PROGRESS, SUCCESS, FAILED, CANCELLED, }
public enum GOAP_PLANNING_STATUS { NONE, RUNNING, PROCESSING_RESULT }
public enum GOAP_CATEGORY { NONE, IDLE, FULLNESS, TIREDNESS, HAPPINESS, WORK, REACTION,}
public enum JOB_TYPE { NONE, UNDERMINE_ENEMY, TIREDNESS_RECOVERY_EXHAUSTED, HUNGER_RECOVERY_STARVING, HAPPINESS_RECOVERY_FORLORN, TIREDNESS_RECOVERY, HUNGER_RECOVERY, HAPPINESS_RECOVERY, REMOVE_TRAIT, RESTRAIN
        , PRODUCE_WOOD, PRODUCE_FOOD, PRODUCE_STONE, PRODUCE_METAL
        , REMOVE_POISON, ASK_FOR_HELP_REMOVE_POISON_TABLE, FEED, KNOCKOUT, APPREHEND, ABDUCT, DESTROY_FRIENDSHIP
        , DESTROY_LOVE, BURY, DELIVER_TREASURE, CRAFT_TOOL, REPLACE_TILE_OBJECT, BREW_POTION
        , JUDGEMENT, BREAK_UP, SAVE_CHARACTER, ASK_FOR_HELP_SAVE_CHARACTER, TANTRUM, SHARE_INFORMATION, BUILD_FURNITURE, STEAL
        , BERSERK, PATROL, EXPLORE, OBTAIN_ITEM, WATCH, DROP, DEATH, HUNT_SERIAL_KILLER_VICTIM, INSPECT, RESOLVE_CONFLICT, REMOVE_FIRE, MISC, ATTEMPT_TO_STOP_JOB, MOVE_OUT, SUICIDE, SEDUCE, REPAIR
        , DESTROY, OBTAIN_FOOD_OUTSIDE, OBTAIN_SUPPLY_OUTSIDE, IMPROVE, COMBAT_WORLD_EVENT, BUILD_GODDESS_STATUE, DESTROY_PROFANE_LANDMARK, PERFORM_HOLY_INCANTATION, PRAY_GODDESS_STATUE, BUILD_TILE_OBJECT, CHEAT, HAVE_AFFAIR, TRIGGER_FLAW, RETURN_HOME
        , CORRUPT_CULTIST, DESTROY_SUPPLY, DESTROY_FOOD, SABOTAGE_FACTION, SEARCHING_WORLD_EVENT, REACT_TO_SCREAM, SCREAM, CHAT, IDLE, INTERRUPTION, CLEANSE_REGION, CLAIM_REGION, INVADE_REGION, ATTACK_NON_DEMONIC_REGION, ATTACK_DEMONIC_REGION
        , BUILD_BLUEPRINT, PLACE_BLUEPRINT, COMBAT, STROLL, HAUL }
public enum JOB_OWNER { CHARACTER, LOCATION, QUEST, }
public enum Cardinal_Direction { North, South, East, West };
public enum ACTION_LOCATION_TYPE {
    IN_PLACE,
    NEARBY,
    RANDOM_LOCATION,
    RANDOM_LOCATION_B,
    NEAR_TARGET,
    //ON_TARGET,
    TARGET_IN_VISION,
    OVERRIDE,
    NEAR_OTHER_TARGET,
}
public enum CHARACTER_STATE_CATEGORY { MAJOR, MINOR,}
//public enum MOVEMENT_MODE { NORMAL, FLEE, ENGAGE }
public enum CHARACTER_STATE { NONE, PATROL, HUNT, STROLL, BERSERKED, STROLL_OUTSIDE, COMBAT, DOUSE_FIRE, MOVE_OUT }
public enum CRIME_CATEGORY {
    NONE,
    INFRACTIONS,
    MISDEMEANOR,
    SERIOUS,
    HEINOUS,
}
public enum CRIME {
    NONE,
    [SubcategoryOf(CRIME_CATEGORY.MISDEMEANOR)]
    THEFT,
    [SubcategoryOf(CRIME_CATEGORY.MISDEMEANOR)]
    ASSAULT,
    [SubcategoryOf(CRIME_CATEGORY.MISDEMEANOR)]
    ATTEMPTED_MURDER,
    [SubcategoryOf(CRIME_CATEGORY.SERIOUS)]
    MURDER,
    [SubcategoryOf(CRIME_CATEGORY.HEINOUS)]
    ABERRATION,
    [SubcategoryOf(CRIME_CATEGORY.INFRACTIONS)]
    INFIDELITY,
    [SubcategoryOf(CRIME_CATEGORY.HEINOUS)]
    HERETIC,
    [SubcategoryOf(CRIME_CATEGORY.INFRACTIONS)]
    MINOR_ASSAULT,
    [SubcategoryOf(CRIME_CATEGORY.MISDEMEANOR)]
    MANSLAUGHTER,
    [SubcategoryOf(CRIME_CATEGORY.SERIOUS)]
    ARSON,
}
public enum CHARACTER_MOOD {
    DARK, BAD, GOOD, GREAT,
}
public enum SEXUALITY {
    STRAIGHT, BISEXUAL, GAY
}
public enum FACILITY_TYPE { NONE, HAPPINESS_RECOVERY, FULLNESS_RECOVERY, TIREDNESS_RECOVERY, SIT_DOWN_SPOT  }
public enum FURNITURE_TYPE { NONE, BED, TABLE, DESK, GUITAR, }
public enum RELATIONSHIP_EFFECT {
    NONE,
    NEUTRAL,
    POSITIVE,
    NEGATIVE,
}
public enum SHARE_INTEL_STATUS { NONE, WITNESSED, INFORMED,}
public enum INTERVENTION_ABILITY { NONE, ACCESS_MEMORIES, LYCANTHROPY, KLEPTOMANIA, VAMPIRISM, UNFAITHFULNESS, CANNIBALISM, ZAP, JOLT, SPOOK, ENRAGE, DISABLE
        , RILE_UP, ABDUCT, PROVOKE, DESTROY, RAISE_DEAD, CLOAK_OF_INVISIBILITY, EXPLOSION, IGNITE, LURE, CURSED_OBJECT, LULLABY, AGORAPHOBIA, SPOIL, ALCOHOLIC, PESTILENCE
        , PARALYSIS, RELEASE, ZOMBIE_VIRUS, PSYCHOPATHY, TORNADO }
public enum INTERVENTION_ABILITY_CATEGORY { NONE, SABOTAGE, MONSTER, DEVASTATION, HEX }
public enum COMBAT_ABILITY {
    SINGLE_HEAL, FLAMESTRIKE, FEAR_SPELL, SACRIFICE, TAUNT,
}

public enum TILE_TAG { CAVE, DUNGEON, FOREST, FLATLAND, MOUNTAIN, GRASSLAND, JUNGLE, TUNDRA, SNOW, DESERT, PROTECTIVE_BARRIER, HALLOWED_GROUNDS, }
public enum SUMMON_TYPE { None, Wolf, Skeleton, Golem, Succubus, Incubus, ThiefSummon, }
public enum ARTIFACT_TYPE { None, Necronomicon, Chaos_Orb, Hermes_Statue, Ankh_Of_Anubis, Miasma_Emitter, }
public enum ABILITY_TAG { NONE, MAGIC, SUPPORT, DEBUFF, CRIME, PHYSICAL, }
public enum LANDMARK_YIELD_TYPE { SUMMON, ARTIFACT, ABILITY, SKIRMISH, STORY_EVENT, }
public enum SERIAL_VICTIM_TYPE { GENDER, ROLE, TRAIT, STATUS }
public enum SPECIAL_OBJECT_TYPE { DEMON_STONE, SPELL_SCROLL, SKILL_SCROLL }
public enum WORLD_EVENT { NONE, HARVEST, SLAY_MINION, MINE_SUPPLY, STUDY, PRAY_AT_TEMPLE, DESTROY_DEMONIC_LANDMARK, HOLY_INCANTATION, CORRUPT_CULTIST, DEMONIC_INCANTATION, SEARCHING, CLAIM_REGION, CLEANSE_REGION, INVADE_REGION, ATTACK_DEMONIC_REGION, ATTACK_NON_DEMONIC_REGION }
public enum DEADLY_SIN_ACTION { SPELL_SOURCE, INSTIGATOR, BUILDER, SABOTEUR, INVADER, FIGHTER, RESEARCHER, }
public enum WORLD_EVENT_EFFECT { GET_FOOD, GET_SUPPLY, GAIN_POSITIVE_TRAIT, REMOVE_NEGATIVE_TRAIT, EXPLORE, COMBAT, DESTROY_LANDMARK, DIVINE_INTERVENTION_SPEED_UP, CORRUPT_CHARACTER, DIVINE_INTERVENTION_SLOW_DOWN, SEARCHING, CONQUER_REGION, REMOVE_CORRUPTION, INVADE_REGION, ATTACK_DEMONIC_REGION, ATTACK_NON_DEMONIC_REGION }
public enum WORLD_OBJECT_TYPE { NONE, ARTIFACT, SUMMON, SPECIAL_OBJECT, }
public enum REGION_FEATURE_TYPE { PASSIVE, ACTIVE }
public enum RESOURCE { FOOD, WOOD, STONE, METAL }
public enum MAP_OBJECT_STATE { BUILT, UNBUILT }
public enum FACTION_IDEOLOGY { INCLUSIVE = 0, EXCLUSIVE = 1, MILITARIST = 2, ECONOMIST = 3, DIVINE_WORSHIP = 4, NATURE_WORSHIP = 5, DEMON_WORSHIP = 6 }
public enum BEHAVIOUR_COMPONENT_ATTRIBUTE { INSIDE_SETTLEMENT_ONLY, OUTSIDE_SETTLEMENT_ONLY, ONCE_PER_DAY, DO_NOT_SKIP_PROCESSING, }
public enum EXCLUSIVE_IDEOLOGY_CATEGORIES { RACE, GENDER, TRAIT, }

/// <summary>
/// STARTED - actor is moving towards the target but is not yet performing action
/// PERFORMING - actor arrived at the target and is performing action
/// SUCCESS - only when action is finished; if action is successful
/// FAIL - only when action is finished; if action failed
/// </summary>
public enum ACTION_STATUS { NONE, STARTED, PERFORMING, SUCCESS, FAIL }

#region Crime Subcategories
[System.AttributeUsage(System.AttributeTargets.Field)]
public class SubcategoryOf : System.Attribute {
    public SubcategoryOf(CRIME_CATEGORY cat) {
        Category = cat;
    }
    public CRIME_CATEGORY Category { get; private set; }
}
#endregion
public static class Extensions {

    #region Crimes
    public static bool IsSubcategoryOf(this CRIME sub, CRIME_CATEGORY cat) {
        System.Type t = typeof(CRIME);
        MemberInfo mi = t.GetMember(sub.ToString()).FirstOrDefault(m => m.GetCustomAttribute(typeof(SubcategoryOf)) != null);
        if (mi == null) throw new System.ArgumentException("Subcategory " + sub + " has no category.");
        SubcategoryOf subAttr = (SubcategoryOf)mi.GetCustomAttribute(typeof(SubcategoryOf));
        return subAttr.Category == cat;
    }
    public static CRIME_CATEGORY GetCategory(this CRIME sub) {
        System.Type t = typeof(CRIME);
        MemberInfo mi = t.GetMember(sub.ToString()).FirstOrDefault(m => m.GetCustomAttribute(typeof(SubcategoryOf)) != null);
        if (mi == null) throw new System.ArgumentException("Subcategory " + sub + " has no category.");
        SubcategoryOf subAttr = (SubcategoryOf)mi.GetCustomAttribute(typeof(SubcategoryOf));
        return subAttr.Category;
    }
    public static bool IsLessThan(this CRIME_CATEGORY sub, CRIME_CATEGORY other) {
        return sub < other;
    }
    public static bool IsGreaterThanOrEqual(this CRIME_CATEGORY sub, CRIME_CATEGORY other) {
        return sub >= other;
    }
    #endregion

    #region Structures
    /// <summary>
    /// Is this stucture contained within walls?
    /// </summary>
    /// <param name="sub"></param>
    /// <returns>True or false</returns>
    public static bool IsOpenSpace(this STRUCTURE_TYPE sub) {
        switch (sub) {
            case STRUCTURE_TYPE.WILDERNESS:
            case STRUCTURE_TYPE.WORK_AREA:
            case STRUCTURE_TYPE.CEMETERY:
            case STRUCTURE_TYPE.POND:
            case STRUCTURE_TYPE.CITY_CENTER:
                return true;
            default:
                return false;
        }
    }
    public static bool ShouldBeGeneratedFromTemplate(this STRUCTURE_TYPE sub) {
        switch (sub) {
            case STRUCTURE_TYPE.INN:
            case STRUCTURE_TYPE.WAREHOUSE:
            case STRUCTURE_TYPE.DWELLING:
            case STRUCTURE_TYPE.CEMETERY:
            case STRUCTURE_TYPE.PRISON:
            case STRUCTURE_TYPE.POND:
            case STRUCTURE_TYPE.CITY_CENTER:
                return true;
            default:
                return false;
        }
    }
    /// <summary>
    /// Get the priority that each structure should be generated in.
    /// This determines what order the structures will be created during map generation. <see cref="AreaInnerTileMap.PlaceInitialStructures"/>
    /// </summary>
    /// <param name="sub">The type of structure</param>
    /// <returns>The priority of a given structure type. 0 being the highest priority.</returns>
    public static int StructureGenerationPriority(this STRUCTURE_TYPE sub) {
        switch (sub) {
            case STRUCTURE_TYPE.CITY_CENTER:
                return 0;
            case STRUCTURE_TYPE.INN:
                return 1;
            case STRUCTURE_TYPE.WAREHOUSE:
                return 2;
            case STRUCTURE_TYPE.DWELLING:
                return 3;
            case STRUCTURE_TYPE.CEMETERY:
                return 4;
            case STRUCTURE_TYPE.PRISON:
                return 5;
            default:
                return 99;
        }
    }
    #endregion

    #region Misc
    public static Cardinal_Direction OppositeDirection(this Cardinal_Direction dir) {
        switch (dir) {
            case Cardinal_Direction.North:
                return Cardinal_Direction.South;
            case Cardinal_Direction.South:
                return Cardinal_Direction.North;
            case Cardinal_Direction.East:
                return Cardinal_Direction.West;
            case Cardinal_Direction.West:
                return Cardinal_Direction.East;
        }
        throw new System.Exception("No opposite direction for " + dir.ToString());
    }
    public static bool IsCardinalDirection(this GridNeighbourDirection dir) {
        switch (dir) {
            case GridNeighbourDirection.North:
            case GridNeighbourDirection.South:
            case GridNeighbourDirection.West:
            case GridNeighbourDirection.East:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Actions
    /// <summary>
    /// Is this action type considered to be a hostile action.
    /// </summary>
    /// <returns>True or false.</returns>
    public static bool IsHostileAction(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.ASSAULT:
            case INTERACTION_TYPE.STEAL:
            case INTERACTION_TYPE.CURSE_CHARACTER:
            case INTERACTION_TYPE.RESTRAIN_CHARACTER:
                return true;
            default:
                return false;
        }
    }
    /// <summary>
    /// Will the given action type directly make it's actor enter combat state.
    /// </summary>
    /// <param name="type">The type of action.</param>
    /// <returns>True or false</returns>
    public static bool IsDirectCombatAction(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.ASSAULT:
                return true;
            default:
                return false;
        }
    }
    public static bool CanBeReplaced(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.EAT:
            case INTERACTION_TYPE.SIT:
                return true;
            default:
                return false;
        }
    }
    public static bool WillAvoidCharactersWhileMoving(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.RITUAL_KILLING:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region State
    public static bool IsCombatState(this CHARACTER_STATE type) {
        switch (type) {
            case CHARACTER_STATE.BERSERKED:
            case CHARACTER_STATE.COMBAT:
            case CHARACTER_STATE.HUNT:
                return true;
            default:
                return false;
        }
    }
    /// <summary>
    /// This is used to determine what class should be created when saving a CharacterState. <see cref="SaveUtilities.CreateCharacterStateSaveDataInstance(CharacterState)"/>
    /// </summary>
    /// <param name="type">The type of state</param>
    /// <returns>If the state type has a unique save data class or not.</returns>
    public static bool HasUniqueSaveData(this CHARACTER_STATE type) {
        switch (type) {
            case CHARACTER_STATE.DOUSE_FIRE:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Tokens
    public static bool CanBeCraftedBy(this SPECIAL_TOKEN type, Character character) {
        if (TokenManager.Instance.itemData.ContainsKey(type)) {
            ItemData data = TokenManager.Instance.itemData[type];
            if (data.canBeCraftedBy == null) {
                return true;
            }
            for (int i = 0; i < data.canBeCraftedBy.Length; i++) {
                if (character.traitContainer.GetNormalTrait<Trait>(data.canBeCraftedBy[i]) != null) {
                    return true;
                }
            }
            return false;
        }
        return true;
    }
    public static bool CreatesObjectWhenDropped(this SPECIAL_TOKEN type) {
        switch (type) {
            case SPECIAL_TOKEN.WATER_BUCKET:
                return false;
            default:
                return true;
        }
    }
    #endregion

    #region Furniture
    public static TILE_OBJECT_TYPE ConvertFurnitureToTileObject(this FURNITURE_TYPE type) {
        TILE_OBJECT_TYPE to;
        if (System.Enum.TryParse<TILE_OBJECT_TYPE>(type.ToString(), out to)) {
            return to;
        }
        return TILE_OBJECT_TYPE.NONE;
    }
    public static bool CanBeCraftedBy(this TILE_OBJECT_TYPE type, Character character) {
        if (type == TILE_OBJECT_TYPE.NONE) {
            return false;
        }
        TileObjectData data;
        if (TileObjectDB.TryGetTileObjectData(type, out data)) {
            if (string.IsNullOrEmpty(data.neededTraitType)) {
                return true;
            }
            return character.traitContainer.GetNormalTrait<Trait>(data.neededTraitType) != null;
        }
        return true;
    }
    #endregion

    #region Tile Objects
    public static FURNITURE_TYPE ConvertTileObjectToFurniture(this TILE_OBJECT_TYPE type) {
        FURNITURE_TYPE to;
        if (System.Enum.TryParse<FURNITURE_TYPE>(type.ToString(), out to)) {
            return to;
        }
        return FURNITURE_TYPE.NONE;
    }
    public static bool CanProvideFacility(this TILE_OBJECT_TYPE tileObj, FACILITY_TYPE facility) {
        TileObjectData data;
        if (TileObjectDB.TryGetTileObjectData(tileObj, out data)) {
            return data.CanProvideFacility(facility);
        }
        return false;
    }
    #endregion

    #region Jobs
    public static bool IsNeedsTypeJob(this JOB_TYPE type) {
        switch (type) {
            case JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED:
            case JOB_TYPE.HUNGER_RECOVERY_STARVING:
            case JOB_TYPE.HAPPINESS_RECOVERY_FORLORN:
            case JOB_TYPE.TIREDNESS_RECOVERY:
            case JOB_TYPE.HUNGER_RECOVERY:
            case JOB_TYPE.HAPPINESS_RECOVERY:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Summons
    public static string SummonName(this SUMMON_TYPE type) {
        switch (type) {
            case SUMMON_TYPE.ThiefSummon:
                return "Thief";
            default:
                return Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
        }
    }
    public static bool CanBeSummoned(this SUMMON_TYPE type) {
        switch (type) {
            case SUMMON_TYPE.None:
            case SUMMON_TYPE.ThiefSummon:
            case SUMMON_TYPE.Skeleton:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Artifacts
    public static bool CanBeSummoned(this ARTIFACT_TYPE type) {
        switch (type) {
            case ARTIFACT_TYPE.None:
            case ARTIFACT_TYPE.Hermes_Statue:
            case ARTIFACT_TYPE.Miasma_Emitter:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Intervention Abilities
    public static List<ABILITY_TAG> GetAbilityTags(this INTERVENTION_ABILITY type) {
        List<ABILITY_TAG> tags = new List<ABILITY_TAG>();
        switch (type) {
            case INTERVENTION_ABILITY.LYCANTHROPY:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            case INTERVENTION_ABILITY.KLEPTOMANIA:
                tags.Add(ABILITY_TAG.CRIME);
                break;
            case INTERVENTION_ABILITY.VAMPIRISM:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            case INTERVENTION_ABILITY.UNFAITHFULNESS:
                tags.Add(ABILITY_TAG.CRIME);
                break;
            case INTERVENTION_ABILITY.CANNIBALISM:
                tags.Add(ABILITY_TAG.MAGIC);
                tags.Add(ABILITY_TAG.CRIME);
                break;
            case INTERVENTION_ABILITY.ZAP:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            case INTERVENTION_ABILITY.JOLT:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            case INTERVENTION_ABILITY.ENRAGE:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            case INTERVENTION_ABILITY.PROVOKE:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            case INTERVENTION_ABILITY.RAISE_DEAD:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            case INTERVENTION_ABILITY.CLOAK_OF_INVISIBILITY:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
        }
        return tags;
    }
    #endregion

    #region Landmarks
    public static bool IsPlayerLandmark(this LANDMARK_TYPE type) {
        switch (type) {
            case LANDMARK_TYPE.THE_PORTAL:
            case LANDMARK_TYPE.THE_SPIRE:
            case LANDMARK_TYPE.THE_CRYPT:
            case LANDMARK_TYPE.THE_KENNEL:
            case LANDMARK_TYPE.THE_ANVIL:
            case LANDMARK_TYPE.THE_FINGERS:
            case LANDMARK_TYPE.THE_EYE:
            case LANDMARK_TYPE.THE_PROFANE:
            case LANDMARK_TYPE.THE_NEEDLES:
                return true;
            default:
                return false;
        }
    }
    public static string LandmarkToString(this LANDMARK_TYPE type) {
        switch (type) {
            case LANDMARK_TYPE.NONE:
                return "Empty";
            default:
                return Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
        }
    }
    #endregion

    #region Areas
    public static bool IsSettlementType(this LOCATION_TYPE type) {
        switch (type) {
            case LOCATION_TYPE.ELVEN_SETTLEMENT:
            case LOCATION_TYPE.HUMAN_SETTLEMENT:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Deadly Sins
    public static string Description(this DEADLY_SIN_ACTION sin) {
        switch (sin) {
            case DEADLY_SIN_ACTION.SPELL_SOURCE:
                return "Knows three Spells that can be extracted by the Ruinarch";
            case DEADLY_SIN_ACTION.INSTIGATOR:
                return "Can be assigned to spawn Chaos Events in The Fingers";
            case DEADLY_SIN_ACTION.BUILDER:
                return "Can construct demonic structures";
            case DEADLY_SIN_ACTION.SABOTEUR:
                return "Can interfere in Events spawned by non-combatant characters";
            case DEADLY_SIN_ACTION.INVADER:
                return "Can invade adjacent regions";
            case DEADLY_SIN_ACTION.FIGHTER:
                return "Can interfere in Events spawned by combat-ready characters";
            case DEADLY_SIN_ACTION.RESEARCHER:
                return "Can be assigned to research upgrades in The Anvil";
            default:
                return string.Empty;
        }
    }
    #endregion

    #region Combat Abilities
    public static string Description(this COMBAT_ABILITY ability) {
        switch (ability) {
            case COMBAT_ABILITY.SINGLE_HEAL:
                return "Heals a friendly unit by a percentage of its max HP.";
            case COMBAT_ABILITY.FLAMESTRIKE:
                return "Deal AOE damage in the surrounding area.";
            case COMBAT_ABILITY.FEAR_SPELL:
                return "Makes a character fear any other character.";
            case COMBAT_ABILITY.SACRIFICE:
                return "Sacrifice a friendly unit to deal AOE damage in the surrounding area.";
            case COMBAT_ABILITY.TAUNT:
                return "Taunts enemies into attacking this character.";
            default:
                return string.Empty;
        }

    }
    #endregion
}
