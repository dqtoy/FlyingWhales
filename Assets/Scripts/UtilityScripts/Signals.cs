using UnityEngine;
using System.Collections;

public static class Signals {

    public static string DAY_END = "OnDayEnd";
    public static string HOUR_STARTED = "OnHourStart";
    public static string HOUR_ENDED = "OnHourEnd";
    public static string DAY_START = "OnDayStart";
    public static string FOUND_ITEM = "OnItemFound"; //Parameters (Character characterThatFoundItem, Item foundItem)
    public static string FOUND_TRACE = "OnTraceFound"; //Parameters (Character characterThatFoundTrace, string traceFound)
    public static string ITEM_PLACED_LANDMARK = "OnItemPlacedAtLandmark"; //Parameters (Item item, BaseLandmark landmark)
    public static string ITEM_PLACED_INVENTORY = "OnItemPlacedAtInventory"; //Parameters (Item item, Character character)
    public static string CHARACTER_DEATH = "OnCharacterDied"; //Parameters (Character characterThatDied)
    public static string CHARACTER_KILLED = "OnCharacterKilled"; //Parameters (Character killer, Character characterThatDied)
    public static string MONSTER_DEATH = "OnMonsterDied"; //Parameters (Monster monsterThatDied)
    public static string COLLIDED_WITH_CHARACTER = "OnCollideWithCharacter"; //Parameters (Character character1, Character character2)
    public static string HISTORY_ADDED = "OnHistoryAdded"; //Parameters (object itemThatHadHistoryAdded) either a character or a landmark
    public static string CHARACTER_CREATED = "OnCharacterCreated"; //Parameters (Character createdCharacter)
    public static string ROLE_CHANGED = "OnCharacterRoleChanged"; //Parameters (Character characterThatChangedRole)
    public static string FACTION_SET = "OnFactionSet"; //Parameters (Character characterThatSetFaction)
    public static string PAUSED = "OnPauseChanged"; //Parameters (bool isGamePaused)
    public static string PROGRESSION_SPEED_CHANGED = "OnProgressionSpeedChanged"; //Parameters (PROGRESSION_SPEED progressionSpeed)
    public static string TILE_LEFT_CLICKED = "OnTileLeftClicked"; //Parameters (HexTile clickedTile)
    public static string TILE_RIGHT_CLICKED = "OnTileRightClicked"; //Parameters (HexTile clickedTile)
    public static string TILE_HOVERED_OVER = "OnTileHoveredOver"; //Parameters (HexTile hoveredTile)
    public static string TILE_HOVERED_OUT = "OnTileHoveredOut"; //Parameters (HexTile hoveredTile)
    public static string RELATIONSHIP_CREATED = "OnRelationshipCreated"; //Parameters (Relationship createdRelationship)
    public static string RELATIONSHIP_REMOVED = "OnRelationshipRemoved"; //Parameters (Relationship removedRelationship)
    public static string GENDER_CHANGED = "OnGenderChanged"; //Parameters (Character characterThatChangedGender, GENDER newGender)
    public static string CHARACTER_REMOVED = "OnCharacterRemoved"; //Parameters (Character removedCharacter)
    public static string AREA_CREATED = "OnAreaCreated"; //Parameters (Area newArea)
    public static string AREA_DELETED = "OnAreaDeleted"; //Parameters (Area deletedArea)
    public static string AREA_TILE_REMOVED = "OnAreaTileRemoved"; //Parameters (Area affectedArea)
    public static string AREA_TILE_ADDED = "OnAreaTileAdded"; //Parameters (Area affectedArea)
    public static string ITEM_EQUIPPED = "OnItemEquipped"; //Parameters (Item equippedItem, ECS.Character character)
    public static string ITEM_UNEQUIPPED = "OnItemUnequipped"; //Parameters (Item unequippedItem, ECS.Character character)
    public static string ITEM_OBTAINED = "OnObtainItem"; //Parameters (Item obtainedItem, Character characterThatObtainedItem)
    public static string ITEM_THROWN = "OnItemThrown"; //Parameters (Item unobtainedItem, ECS.Character character)
    public static string GAME_LOADED = "OnGameLoaded";
    public static string STRUCTURE_STATE_CHANGED = "OnStructureStateChanged"; //Parameters (StructureObj obj, ObjectState newState)
    public static string LANDMARK_ATTACK_TARGET_SELECTED = "OnLandmarkAttackTargetSelected"; //Parameters (BaseLandmark target)
    public static string PLAYER_LANDMARK_CREATED = "OnPlayerLandmarkCreated"; //Parameters (BaseLandmark createdLandmark)
    public static string PARTY_ENTERED_LANDMARK = "OnPartyEnteredLandmark"; //Parameters (IParty partyThatEntered)
    public static string LOOK_FOR_ACTION = "LookForAction"; //Parameters (ActionThread actionThread)
    public static string BUILD_STRUCTURE_LOOK_ACTION = "BuildStructureLookAction"; //Parameters (BuildStructureQuestData questData)


    #region UI
    public static string SHOW_POPUP_MESSAGE = "ShowPopupMessage"; //Parameters (string message, MESSAGE_BOX_MODE mode, bool expires)
    public static string HIDE_POPUP_MESSAGE = "HidePopupMessage";
    public static string UPDATE_UI = "UpdateUI";
    public static string SHOW_NOTIFICATION = "ShowNotification"; //Parameters (string text, UnityAction onClickAction)
    public static string SHOW_CHARACTER_DIALOG = "ShowCharacterDialog"; //Parameters(ECS.Character character, string text, List<CharacterDialogChoice> choices)
    #endregion

    #region Quest Signals
    public static string CHARACTER_SNATCHED = "OnCharacterSnatched"; //Parameters (Character snatchedCharacter)
    #endregion

    #region Party
    public static string MONSTER_PARTY_DIED = "OnMonsterPartyDied"; //Parameters (MonsterParty monsterParty)
    #endregion

    #region Factions
    public static string FACTION_CREATED = "OnFactionCreated"; //Parameters (Faction createdFaction)
    public static string FACTION_DELETED = "OnFactionDeleted"; //Parameters (Faction deletedFaction)
    #endregion

    #region Actions
    public static string ACTION_SUCCESS = "OnActionSuccess"; //Parameters (CharacterParty partyThatSucceeded, CharacterAction actionThatSucceeded)
    #endregion
}
