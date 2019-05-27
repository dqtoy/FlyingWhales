using System.Linq;
using System.Reflection;

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
public enum GRID_PATHFINDING_MODE {
    NORMAL,
    ROADS_ONLY,
    REALISTIC,
    MAIN_ROAD_GEN,
    CAVE_ROAD_GEN,
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
    //SNATCHER_DEMONS_LAIR = 15,
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
    HIVE_LAIR = 42,
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
    AT_WAR,
    ENEMY,
    DISLIKED,
    NEUTRAL,
    FRIEND,
    ALLY,
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
    DUNGEON,
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
    USE_ITEM_ON_CHARACTER,
    DROP_ITEM,
    ABDUCT_ACTION,
    HUNT_ACTION,
    EAT_DEFENSELESS,
    PICK_ITEM,
    RELEASE_ABDUCTED_ACTION,
    MOVE_TO_VISIT,
    TRANSFER_HOME,
    HANG_OUT_ACTION,
    ARGUE_ACTION,
    CURSE_ACTION,
    CRAFT_ITEM,
    MINE_ACTION,
    ASK_FOR_HELP_SAVE_CHARACTER,
    ASSAULT_ACTION_NPC,
    TRANSFORM_TO_WOLF,
    REVERT_TO_NORMAL,
    EAT_PLANT,
    SLEEP,
    CARRY_CHARACTER,
    DROP_CHARACTER,
    EAT_SMALL_ANIMAL,
    EAT_DWELLING_TABLE,
    DAYDREAM,
    PLAY_GUITAR,
    CHAT_CHARACTER,
    ARGUE_CHARACTER,
    STROLL,
    DRINK,
    SLEEP_OUTSIDE,
    EXPLORE,
    PATROL,
    TABLE_REMOVE_POISON,
    TABLE_POISON,
    PRAY,
    CHOP_WOOD,
    STEAL,
    SCRAP,
    MAGIC_CIRCLE_PERFORM_RITUAL,
    GET_SUPPLY,
    DROP_SUPPLY,
    TILE_OBJECT_DESTROY,
    ITEM_DESTROY,
    TRAVEL,
    RETURN_HOME_LOCATION,
    PLAY,
    PATROL_ROAM,
    EAT_CORPSE,
    RESTRAIN_CHARACTER,
    FIRST_AID_CHARACTER,
    CURE_CHARACTER,
    CURSE_CHARACTER,
    DISPEL_MAGIC,
    JUDGE_CHARACTER,
    REPORT_CRIME,
    FEED,
    STEAL_CHARACTER,
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
    INVITE_TO_MAKE_LOVE,
    DRINK_BLOOD,
    REPLACE_TILE_OBJECT,
    CRAFT_FURNITURE,
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
    RECRUITER,
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
    AREA,
    FACTION,
    TILE_OBJECT,
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
}
public enum RELATIONSHIP_TRAIT {
    NONE,
    ENEMY,
    FRIEND,
    RELATIVE,
    LOVER,
    PARAMOUR,
    MASTER,
    SERVANT,
    SAVER,
    SAVE_TARGET,
}

public enum POINT_OF_INTEREST_TYPE {
    ITEM,
    LANDMARK,
    CHARACTER,
    TILE_OBJECT,
    STRUCTURE,
}
public enum TILE_OBJECT_TYPE {
    SUPPLY_PILE,
    CORPSE,
    SMALL_ANIMAL,
    EDIBLE_PLANT,
    GUITAR,
    MAGIC_CIRCLE,
    TABLE,
    BED,
    ORE,
    TREE,
    FOOD,
    DESK,
    TOMBSTONE,
    NONE,
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
public enum TileNeighbourDirection { North, South, West, East, North_West,  North_East, South_West, South_East }
public enum TIME_IN_WORDS { AFTER_MIDNIGHT, AFTER_MIDNIGHT_1, AFTER_MIDNIGHT_2, MORNING, MORNING_1, MORNING_2, AFTERNOON, AFTERNOON_1, AFTERNOON_2, EARLY_NIGHT, LATE_NIGHT, NIGHT_1, NIGHT_2 }
//public enum CRIME_SEVERITY { NONE, INFRACTION, MISDEMEANOUR, SERIOUS_CRIME, }
public enum FOOD { BERRY, MUSHROOM, RABBIT, RAT }
public enum GOAP_EFFECT_CONDITION { NONE, REMOVE_TRAIT, HAS_TRAIT, HAS_SUPPLY, HAS_ITEM, FULLNESS_RECOVERY, TIREDNESS_RECOVERY, HAPPINESS_RECOVERY, HAS_NON_POSITIVE_TRAIT, IN_PARTY, REMOVE_FROM_PARTY, DESTROY, DEATH, PATROL, EXPLORE, REMOVE_ITEM, HAS_TRAIT_EFFECT, HAS_PLAN}
public enum GOAP_PLAN_STATE { IN_PROGRESS, SUCCESS, FAILED, CANCELLED, }
public enum GOAP_CATEGORY { NONE, IDLE, FULLNESS, TIREDNESS, HAPPINESS, WORK, REACTION,}
public enum Cardinal_Direction { North, South, East, West };
public enum ACTION_LOCATION_TYPE {
    IN_PLACE,
    NEARBY,
    RANDOM_LOCATION,
    RANDOM_LOCATION_B,
    NEAR_TARGET,
    ON_TARGET,
}
public enum CHARACTER_STATE_CATEGORY { MAJOR, MINOR,}
//public enum MOVEMENT_MODE { NORMAL, FLEE, ENGAGE }
public enum CHARACTER_STATE { NONE, EXPLORE, PATROL, FLEE, ENGAGE, HUNT, STROLL, BERSERKED, STROLL_OUTSIDE, }
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
    [SubcategoryOf(CRIME_CATEGORY.SERIOUS)]
    ABERRATION,
    [SubcategoryOf(CRIME_CATEGORY.INFRACTIONS)]
    INFIDELITY,
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
    POSITIVE,
    NEGATIVE,
}

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
    /// Is this stucture contained within a rectangle?
    /// </summary>
    /// <param name="sub"></param>
    /// <returns>True or false</returns>
    public static bool IsOpenSpace(this STRUCTURE_TYPE sub) {
        switch (sub) {
            case STRUCTURE_TYPE.INN:
            case STRUCTURE_TYPE.WAREHOUSE:
            case STRUCTURE_TYPE.DWELLING:
            case STRUCTURE_TYPE.EXPLORE_AREA:
            case STRUCTURE_TYPE.CEMETERY:
                return false;
            default:
                return true;
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
    #endregion

    #region Actions
    public static bool IsCombatAction(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.ASSAULT_ACTION_NPC:
            case INTERACTION_TYPE.STEAL:
            case INTERACTION_TYPE.STEAL_CHARACTER:
            case INTERACTION_TYPE.TILE_OBJECT_DESTROY:
            case INTERACTION_TYPE.CURSE_CHARACTER:
            case INTERACTION_TYPE.RESTRAIN_CHARACTER:
                return true;
            default:
                return false;
        }
    }
    public static bool CanBeReplaced(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.EAT_DWELLING_TABLE:
            case INTERACTION_TYPE.SIT:
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
            case CHARACTER_STATE.ENGAGE:
            case CHARACTER_STATE.HUNT:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Tokens
    public static bool CanBeCraftedBy(this SPECIAL_TOKEN type, Character character) {
        if (ItemManager.Instance.itemData.ContainsKey(type)) {
            ItemData data = ItemManager.Instance.itemData[type];
            if (data.neededTraitType == null) {
                return true;
            }
            return character.HasTraitOf(data.neededTraitType);
        }
        return true;
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
            if (data.neededTraitType == null) {
                return true;
            }
            return character.HasTraitOf(data.neededTraitType);
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
}
