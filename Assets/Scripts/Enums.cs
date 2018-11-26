﻿public enum PROGRESSION_SPEED {
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
    ZOMBIE,
    INSECT,
    SPIDER,
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
	POINT_TO_POINT,
	NORMAL,
    ROAD_CREATION,
    NO_MAJOR_ROADS,
	MAJOR_ROADS,
	MINOR_ROADS,
	MAJOR_ROADS_ONLY_KINGDOM,
	MAJOR_ROADS_WITH_ALLIES,
	MINOR_ROADS_ONLY_KINGDOM,
    REGION_CONNECTION,
    LANDMARK_ROADS,
    USE_ROADS,
	USE_ROADS_WITH_ALLIES,
	USE_ROADS_TRADE,
    //USE_ROADS_FACTION_RELATIONSHIP,
    //NORMAL_FACTION_RELATIONSHIP,
    LANDMARK_CONNECTION,
    UNRESTRICTED,
    PASSABLE,
    PASSABLE_REGION_ONLY,
    REGION_ISLAND_CONNECTION,
    AREA_ONLY,
}

public enum GENDER{
	MALE,
	FEMALE,
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
public enum RESOURCE {
    NONE,
    //OAK,
    //ELF_CIVILIAN,
    //HUMAN_CIVILIAN,
    //IRON,
    WOOD,
    IRON,
    GOLD,
    FOOD,

}
//public enum RESOURCE_TYPE{
//	NONE,
//	FOOD,
//	MATERIAL,
//	ORE,
//}
public enum MATERIAL{
	NONE, //0
	CLAY, //11
	LIMESTONE, //12
	GRANITE, //14
	MARBLE,
	SILK,
	COTTON,
	PIGMEAT,
	COWMEAT,
	GOATHIDE,
	DEERHIDE,
	BEHEMOTHHIDE,
	OAK,
	YEW,
	EBONY,
	IRON,
	COBALT,
	MITHRIL,
    FLAX,
    CORN,
    RICE
}
public enum MATERIAL_CATEGORY{
	NONE,
	METAL,
	WOOD,
	STONE,
	CLOTH,
	PLANT,
	MEAT,
	LEATHER,
}
//public enum ROLE{
//	UNTRAINED,
//	FOODIE, //Farming or Hunting
//	GATHERER, //Lumberyard or Quarry
//	MINER, 
//	TRADER,
//	SPY,
//	GUARDIAN,
//	ENVOY,
//	GENERAL,
//	GOVERNOR,
//	KING,
//	EXPANDER,
//	RAIDER,
//	REINFORCER,
//	REBEL,
//    EXTERMINATOR,
//    SCOURGE,
//    HEALER,
//	PROVOKER,
//	MISSIONARY,
//	ABDUCTOR,
//    LYCANTHROPE,
//	INVESTIGATOR,
//	THIEF,
//    WITCH,
//    ADVENTURER,
//	RELIEVER,
//	INTERCEPTER,
//	RANGER,
//    MILITARY_ALLIANCE_OFFICER,
//	TREATYOFFICER,
//	TRIBUTER,
//	INSTIGATOR,
//    GRAND_CHANCELLOR,
//    GRAND_MARSHAL,
//    QUEEN,
//    CROWN_PRINCE,
//	CARAVAN,
//	REFUGEE,
//}
//public enum BASE_RESOURCE_TYPE{
//	FOOD,
//	WOOD,
//	STONE,
//	MANA_STONE,
//	MITHRIL,
//	COBALT,
//	GOLD,
//	NONE
//}
public enum STRUCTURE_TYPE{
	NONE,
	CITY,
	FARM,
	HUNTING_LODGE,
	QUARRY,
	LUMBERYARD,
	MINE,
	TRADING_POST,
    GENERIC
	//BARRACKS,
	//SPY_GUILD,
	//MINISTRY,
	//KEEP
}
public enum STRUCTURE_QUALITY{
	NONE,
	BASIC,
	ADVANCED,
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
    MINION_NAME,
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
//public enum BASE_LANDMARK_TYPE {
//    NONE,
//    SETTLEMENT,
//    RESOURCE,
//    DUNGEON,
//    LAIR
//}
public enum LANDMARK_TAG {
    CAN_HUNT,
    CAN_SCAVENGE,
}
public enum LANDMARK_TYPE {
    NONE = 0,
    DEMONIC_PORTAL = 1,
    //ELVEN_SETTLEMENT = 2,
    //HUMAN_SETTLEMENT = 3,
    GARRISON = 4,
    //OAK_FORTIFICATION = 5,
    //IRON_FORTIFICATION = 6,
    //OAK_LUMBERYARD = 7,
    IRON_MINES = 8,
    INN = 9,
    //PUB = 9,
    //TEMPLE = 10,
    HUNTING_GROUNDS = 11,
    HOUSES = 12,
    //HUMAN_HOUSES = 13,
    MONSTER_DEN = 14,
    SNATCHER_DEMONS_LAIR = 15,
    SHOP = 16,
    FARM = 17,
    GOLD_MINE = 18,
    LUMBERYARD = 19,
    PALACE = 20,
    CAMP = 21,
    LAIR = 22,
    ABANDONED_MINE = 23,
    BANDIT_CAMP = 24,
    HERMIT_HUT = 25,
    CATACOMB = 26,
    PYRAMID = 27,
    EXILE_CAMP = 28,
    GIANT_SKELETON = 29,
    ANCIENT_TEMPLE = 30,
    CAVE = 31,
    ICE_PIT = 32,
    MANA_EXTRACTOR = 33,
    BARRACKS = 34,
    MINIONS_HOLD = 35,
    DWELLINGS = 36,
    RAMPART = 37,
    CORRUPTION_NODE = 38,
    RITUAL_CIRCLE = 39,
    DRAGON_CAVE = 40,
    SKELETON_CEMETERY = 41,
    SPIDER_HIVE_LAIR = 42,
    ZOMBIE_PYRAMID = 43,
    IMP_KENNEL = 44,
    CEMETERY = 45,
    TRAINING_ARENA = 46,
    PENANCE_TEMPLE = 47,
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
    HERO,
	VILLAIN,
    CIVILIAN,
    KING,
    PLAYER,
    GUARDIAN,
    BANDIT,
    LEADER,
    BEAST,
    ARMY,
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
//public enum CHARACTER_JOB {
//    NONE,
//    SHOPKEEPER,
//    MINER,
//    WOODCUTTER,
//    FARMER,
//    RETIRED_HERO
//}
public enum QUEST_TYPE { 
    RELEASE_CHARACTER,
    BUILD_STRUCTURE,
    FETCH_ITEM,
    SURRENDER_ITEMS,
}
public enum FACTION_RELATIONSHIP_STATUS {
    NON_HOSTILE,
    HOSTILE,
    AT_WAR,
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
    CRUSH,
    PIERCE,
    SLASH,
    MAGIC,
	STATUS
}
public enum DEFEND_TYPE {
    DODGE,
    PARRY,
    BLOCK,
	NONE,
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
public enum STANCE {
    COMBAT,
    NEUTRAL,
    STEALTHY
}
public enum MODE {
    DEFAULT,
    ALERT,
    STEALTH
}
public enum STORYLINE {
    THE_DYING_KING,
}
public enum CHARACTER_RELATIONSHIP{
	FRIEND,
    MENTOR,
    STUDENT,
    FATHER,
    MOTHER,
    BROTHER,
    SISTER,
    SON,
    DAUGHTER,
    LOVER,
    HUSBAND,
    WIFE,
    ENEMY,
    RIVAL,
    STALKER,
    //RIVAL,
	//FRIEND,
	//ENEMY,
	//SIBLING,
	//PARENT,
	//CHILD,
	//LOVER,
	//EX_LOVER,
	//APPRENTICE,
	//MENTOR,
	//ACQUAINTANCE,
}

public enum CHARACTER_RELATIONSHIP_CATEGORY{
	NEGATIVE,
	POSITIVE,
	FAMILIAL,
	NEUTRAL,
}

public enum COMBAT_INTENT{
	KILL,
	IMPRISON,
	DEFEAT,
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
    WORK,
    MISC,
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

//public enum IMAGE_SIZE {
//    X64,
//    X256,
//    X72,
//    X36,
//}

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
public enum AREA_TYPE {
    ELVEN_SETTLEMENT,
    HUMAN_SETTLEMENT,
    WILDLANDS,
    DEMONIC_INTRUSION, //Player area
    ANCIENT_RUINS,
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
//public enum DEMON_TYPE {
//    NONE,
//    LUST,
//    GLUTTONY,
//    GREED,
//    SLOTH,
//    WRATH,
//    ENVY,
//    PRIDE,
//}
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
}
public enum DAMAGE_IDENTIFIER {
    DEALT,
    RECEIVED,
}
public enum TRAIT_REQUIREMENT {
    RACE,
    TRAIT,
}
public enum MORALITY {
    GOOD,
    EVIL
}
public enum MINIONS_SORT_TYPE {
    DEFAULT,
    LEVEL,
    TYPE,
}
public enum INTERACTION_TYPE {
    BANDIT_RAID,
    INVESTIGATE,
    ABANDONED_HOUSE,
    UNEXPLORED_CAVE,
    HUMAN_BANDIT_REINFORCEMENTS,
    HARVEST_SEASON,
    MYSTERY_HUM,
    SPIDER_QUEEN,
    ARMY_UNIT_TRAINING,
    ARMY_MOBILIZATION,
    UNFINISHED_CURSE,
    ARMY_ATTACKS,
    SUSPICIOUS_SOLDIER_MEETING,
    KILLER_ON_THE_LOOSE,
    MYSTERIOUS_SARCOPHAGUS,
    NOTHING_HAPPENED,
    GOBLIN_BANDIT_REINFORCEMENTS,
    CHARACTER_EXPLORES,
    CHARACTER_TRACKING,
    RETURN_HOME,
    THE_NECROMANCER,
    FACTION_ATTACKS,
}
public enum REWARD {
    SUPPLY,
    MANA,
    EXP,
}
public enum TRAIT_TYPE {
    NEUTRAL,
    POSITIVE,
    NEGATIVE,
}
public enum TRAIT_REQUIREMENT_SEPARATOR {
    OR,
    AND,
}
public enum TRAIT_REQUIREMENT_TARGET {
    SELF,
    ENEMY,
    OTHER_PARTY_MEMBERS,
    ALL_PARTY_MEMBERS,
    ALL_IN_COMBAT,
    ALL_ENEMIES,
    SELF_OTHER_PARTY_MEMBERS,
    SELF_ALL_PARTY_MEMBERS,
    SELF_ALL_IN_COMBAT,
    SELF_ALL_ENEMIES,
}
public enum JOB {
    NONE,
    INSTIGATOR,
    EXPLORER,
    DIPLOMAT,
    RECRUITER,
    RAIDER,
    SPY,
    DISCOURAGER,
}