using UnityEngine;
using System.Collections;

public static class Signals {

    public static string TICK_STARTED = "OnTickStart";
    public static string TICK_STARTED_2 = "OnTickStart2";
    public static string TICK_ENDED = "OnTickEnd";
    public static string TICK_ENDED_2 = "OnTickEnd2";
    public static string DAY_STARTED = "OnDayStart";
    public static string MONTH_START = "OnMonthStart";
    public static string MONTH_END = "OnMonthEnd";
    public static string FOUND_ITEM = "OnItemFound"; //Parameters (Character characterThatFoundItem, Item foundItem)
    public static string FOUND_TRACE = "OnTraceFound"; //Parameters (Character characterThatFoundTrace, string traceFound)
    public static string GAME_LOADED = "OnGameLoaded";
    public static string TOGGLE_CHARACTERS_VISIBILITY = "OnToggleCharactersVisibility";
    public static string INSPECT_ALL = "InspectAll";

    #region Tiles
    public static string TILE_LEFT_CLICKED = "OnTileLeftClicked"; //Parameters (HexTile clickedTile)
    public static string TILE_RIGHT_CLICKED = "OnTileRightClicked"; //Parameters (HexTile clickedTile)
    public static string TILE_HOVERED_OVER = "OnTileHoveredOver"; //Parameters (HexTile hoveredTile)
    public static string TILE_HOVERED_OUT = "OnTileHoveredOut"; //Parameters (HexTile hoveredTile)
    #endregion

    #region Areas
    public static string AREA_CREATED = "OnAreaCreated"; //Parameters (Area newArea)
    public static string AREA_DELETED = "OnAreaDeleted"; //Parameters (Area deletedArea)
    public static string AREA_TILE_REMOVED = "OnAreaTileRemoved"; //Parameters (Area affectedArea)
    public static string AREA_TILE_ADDED = "OnAreaTileAdded"; //Parameters (Area affectedArea)
    public static string AREA_SUPPLIES_CHANGED = "OnAreaSuppliesSet"; //Parameters (Area affectedArea)
    public static string AREA_OCCUPANY_CHANGED = "OnAreaOccupancyChanged"; //Parameters (Area affectedArea)
    public static string AREA_TOKEN_COLLECTION_CHANGED = "OnAreaTokenCollectionChanged"; //Parameters (Area affectedArea)
    public static string AREA_DEFENDERS_CHANGED = "OnAreaDefendersChanged"; //Parameters (Area affectedArea)
    public static string AREA_OWNER_CHANGED = "OnAreaOwnerChanged"; //Parameters (Area affectedArea)
    public static string AREA_RESIDENT_ADDED = "OnAreaResidentAdded"; //Parameters (Area affectedArea, Character newResident)
    public static string AREA_RESIDENT_REMOVED = "OnAreaResidentRemoved"; //Parameters (Area affectedArea, Character removedResident)
    public static string CHARACTER_ENTERED_AREA = "OnCharacterEnteredArea"; //Parameters (Area affectedArea, Character character)
    public static string CHARACTER_EXITED_AREA = "OnCharacterExitedArea"; //Parameters (Area affectedArea, Character character)
    public static string ITEM_REMOVED_FROM_AREA = "OnItemRemovedFromArea"; //Parameters (Area affectedArea, SpecialToken token)
    public static string ITEM_ADDED_TO_AREA = "OnItemAddedToArea"; //Parameters (Area affectedArea, SpecialToken token)
    #endregion

    #region Landmarks
    public static string ITEM_PLACED_AT_LANDMARK = "OnItemPlacedAtLandmark"; //Parameters (Item item, BaseLandmark landmark)
    public static string ITEM_REMOVED_FROM_LANDMARK = "OnItemRemovedFromLandmark"; //Parameters (Item item, BaseLandmark landmark)
    public static string STRUCTURE_STATE_CHANGED = "OnStructureStateChanged"; //Parameters (StructureObj obj, ObjectState newState)
    public static string LANDMARK_ATTACK_TARGET_SELECTED = "OnLandmarkAttackTargetSelected"; //Parameters (BaseLandmark target)
    public static string PLAYER_LANDMARK_CREATED = "OnPlayerLandmarkCreated"; //Parameters (BaseLandmark createdLandmark)
    public static string PARTY_ENTERED_LANDMARK = "OnPartyEnteredLandmark"; //Parameters (IParty partyThatEntered, BaseLandmark landmark)
    public static string PARTY_EXITED_LANDMARK = "OnPartyExitedLandmark"; //Parameters (IParty partyThatEntered, BaseLandmark landmark)
    public static string DESTROY_LANDMARK = "OnDestroyLandmark"; //Parameteres (BaseLandmark destroyedLandmark)
    public static string LANDMARK_UNDER_ATTACK = "OnLandmarkUnderAttack"; //Parameters (BaseLandmark underAttackedLandmark, GameEvent associatedEvent = null)
    public static string LANDMARK_INSPECTED = "OnLandmarkInspected"; //Parameters (BaseLandmark inspectedLandmark)
    public static string LANDMARK_RESIDENT_ADDED = "OnLandmarkResidentAdded"; //Parameters (BaseLandmark affectedLandmark, ICharacter character)
    public static string LANDMARK_RESIDENT_REMOVED = "OnLandmarkResidentRemoved"; //Parameters (BaseLandmark affectedLandmark, ICharacter character)
    public static string LANDMARK_INVESTIGATION_ACTIVATED = "OnLandmarkInvestigationActivated"; //Parameters (BaseLandmark investigatedLandmark)
    public static string UPDATE_RITUAL_CIRCLE_TRAIT = "OnUpdateRitualCircleTrait";
    #endregion

    #region Character
    public static string ATTRIBUTE_ADDED = "OnCharacterAttributeAdded"; //Parameters (Character affectedCharacter, CharacterAttribute addedTag)
    public static string ATTRIBUTE_REMOVED = "OnCharacterAttributeRemoved"; //Parameters (Character affectedCharacter, CharacterAttribute removedTag)
    public static string ITEM_PLACED_INVENTORY = "OnItemPlacedAtInventory"; //Parameters (Item item, Character character)
    public static string CHARACTER_DEATH = "OnCharacterDied"; //Parameters (Character characterThatDied)
    public static string CHARACTER_KILLED = "OnCharacterKilled"; //Parameters (Character killer, Character characterThatDied)
    public static string MONSTER_DEATH = "OnMonsterDied"; //Parameters (Monster monsterThatDied)
    public static string COLLIDED_WITH_CHARACTER = "OnCollideWithCharacter"; //Parameters (Character character1, Character character2)
    public static string CHARACTER_CREATED = "OnCharacterCreated"; //Parameters (Character createdCharacter)
    public static string ROLE_CHANGED = "OnCharacterRoleChanged"; //Parameters (Character characterThatChangedRole)
    //public static string RELATIONSHIP_CREATED = "OnRelationshipCreated"; //Parameters (Relationship createdRelationship)
    //public static string RELATIONSHIP_REMOVED = "OnRelationshipRemoved"; //Parameters (Relationship removedRelationship)
    public static string GENDER_CHANGED = "OnGenderChanged"; //Parameters (Character characterThatChangedGender, GENDER newGender)
    public static string CHARACTER_REMOVED = "OnCharacterRemoved"; //Parameters (Character removedCharacter)
    public static string ITEM_EQUIPPED = "OnItemEquipped"; //Parameters (Item equippedItem, Character character)
    public static string ITEM_UNEQUIPPED = "OnItemUnequipped"; //Parameters (Item unequippedItem, Character character)
    public static string ITEM_OBTAINED = "OnObtainItem"; //Parameters (Item obtainedItem, Character characterThatObtainedItem)
    public static string ITEM_THROWN = "OnItemThrown"; //Parameters (Item unobtainedItem, Character character)
    public static string CHARACTER_MARKED = "OnCharacterMarked";
    public static string CHARACTER_INSPECTED = "OnCharacterInspected"; //Parameters (Character inspectedCharacter)
    public static string CHARACTER_LEVEL_CHANGED = "OnCharacterLevelChange"; //Parameters (Character character)
    public static string TRAIT_ADDED = "OnTraitAdded";
    public static string TRAIT_REMOVED = "OnTraitRemoved"; //Parameters (Character character)
    public static string ADJUSTED_HP = "OnAdjustedHP";
    public static string PARTY_STARTED_TRAVELLING = "OnPartyStartedTravelling"; //Parameters (Party travellingParty)
    public static string PARTY_DONE_TRAVELLING = "OnPartyDoneTravelling"; //Parameters (Party travellingParty)
    public static string CHARACTER_MIGRATED_HOME = "OnCharacterChangedHome"; //Parameters (Character, Area previousHome, Area newHome); 
    public static string CHARACTER_CHANGED_RACE = "OnCharacterChangedRace"; //Parameters (Character); 
    public static string CHARACTER_ARRIVED_AT_STRUCTURE = "OnCharacterArrivedAtStructure"; //Parameters (Character, LocationStructure); 
    public static string RELATIONSHIP_ADDED = "OnCharacterGainedRelationship"; //Parameters (Character, RelationshipTrait)
    #endregion

    #region UI
    public static string SHOW_POPUP_MESSAGE = "ShowPopupMessage"; //Parameters (string message, MESSAGE_BOX_MODE mode, bool expires)
    public static string HIDE_POPUP_MESSAGE = "HidePopupMessage";
    public static string UPDATE_UI = "UpdateUI";
    public static string SHOW_NOTIFICATION = "ShowNotification"; //Parameters (string text, UnityAction onClickAction)
    public static string SHOW_CHARACTER_DIALOG = "ShowCharacterDialog"; //Parameters(Character character, string text, List<CharacterDialogChoice> choices)
    public static string HISTORY_ADDED = "OnHistoryAdded"; //Parameters (object itemThatHadHistoryAdded) either a character or a landmark
    public static string PAUSED = "OnPauseChanged"; //Parameters (bool isGamePaused)
    public static string PROGRESSION_SPEED_CHANGED = "OnProgressionSpeedChanged"; //Parameters (PROGRESSION_SPEED progressionSpeed)
    public static string MENU_OPENED = "OnMenuOpened"; //Parameters (UIMenu openedMenu)
    public static string MENU_CLOSED = "OnMenuClosed"; //Parameters (UIMenu closedMenu)
    public static string INTERACTION_MENU_OPENED = "OnInteractionMenuOpened"; //Parameters ()
    public static string INTERACTION_MENU_CLOSED = "OnInteractionMenuClosed"; //Parameters ()
    public static string CLICKED_INTERACTION_BUTTON = "OnClickedInteractionButton";
    public static string HIDE_MENUS = "HideMenus";
    public static string DRAG_OBJECT_CREATED = "OnDragObjectCreated"; //Parameters (DragObject obj)
    public static string DRAG_OBJECT_DESTROYED = "OnDragObjectDestroyed"; //Parameters (DragObject obj)
    #endregion

    #region Quest Signals
    public static string CHARACTER_SNATCHED = "OnCharacterSnatched"; //Parameters (Character snatchedCharacter)
    public static string CHARACTER_RELEASED = "OnCharacterReleased"; //Parameters (Character releasedCharacter)
    public static string QUEST_TURNED_IN = "OnQuestTurnedIn"; //Parameter (Quest turnedInQuest)
    #endregion

    #region Party
    public static string MONSTER_PARTY_DIED = "OnMonsterPartyDied"; //Parameters (MonsterParty monsterParty)
    public static string CHARACTER_JOINED_PARTY = "OnCharacterJoinedParty"; //Parameters (ICharacter characterThatJoined, NewParty affectedParty)
    public static string CHARACTER_LEFT_PARTY = "OnCharacterLeftParty"; //Parameters (ICharacter characterThatLeft, NewParty affectedParty)
    public static string PARTY_DIED = "OnPartyDied"; //Parameters (Party partyThatDied)
    #endregion

    #region Factions
    public static string FACTION_CREATED = "OnFactionCreated"; //Parameters (Faction createdFaction)
    public static string FACTION_DELETED = "OnFactionDeleted"; //Parameters (Faction deletedFaction)
    public static string CHARACTER_ADDED_TO_FACTION = "OnCharacterAddedToFaction"; //Parameters (Character addedCharacter, Faction affectedFaction)
    public static string CHARACTER_REMOVED_FROM_FACTION = "OnCharacterRemovedFromFaction"; //Parameters (Character addedCharacter, Faction affectedFaction)
    public static string FACTION_SET = "OnFactionSet"; //Parameters (Character characterThatSetFaction)
    public static string FACTION_LEADER_DIED = "OnFactionLeaderDied"; //Parameters (Faction affectedFaction)
    public static string FACTION_DIED = "OnFactionDied"; //Parameters (Faction affectedFaction)
    public static string FACTION_OWNED_AREA_ADDED = "OnFactionOwnedAreaAdded"; //Parameters (Faction affectedFaction, Area addedArea)
    public static string FACTION_OWNED_AREA_REMOVED = "OnFactionOwnedAreaRemoved"; //Parameters (Faction affectedFaction, Area removedArea)
    public static string FACTION_RELATIONSHIP_CHANGED = "OnFactionRelationshipChanged"; //Parameters (FactionRelationship rel)
    public static string FACTION_ACTIVE_CHANGED = "OnFactionActiveChanged"; //Parameters (Faction affectedFaction)
    #endregion

    #region Actions
    public static string ACTION_SUCCESS = "OnActionSuccess"; //Parameters (CharacterParty partyThatSucceeded, CharacterAction actionThatSucceeded)
    public static string ACTION_DAY_ADJUSTED = "OnActionDayAdjusted"; //Parameters (CharacterAction action, CharacterParty doer)
    public static string ACTION_TAKEN = "OnActionTaken"; //Parameters (CharacterAction action, CharacterParty doer)
    public static string ACTION_ADDED_TO_QUEUE = "OnActionAddedToQueue"; //Parameters (CharacterAction actionAdded, Character affectedCharacter)
    public static string ACTION_REMOVED_FROM_QUEUE = "OnActionRemovedFromQueue"; //Parameters (CharacterAction actionRemoved, Character affectedCharacter)
    public static string LOOK_FOR_ACTION = "LookForAction"; //Parameters (ActionThread actionThread)
    public static string BUILD_STRUCTURE_LOOK_ACTION = "BuildStructureLookAction"; //Parameters (BuildStructureQuestData questData)
    #endregion

    #region Squads
    public static string SQUAD_CREATED = "OnSquadCreated"; //Parameters (Squad createdSquad)
    public static string SQUAD_DELETED = "OnSquadDeleted"; //Parameters (Squad deletedSquad)
    public static string SQUAD_MEMBER_REMOVED = "OnSquadMemberRemoved"; //Parameters (ICharacter removedCharacter, Squad affectedSquad)
    public static string SQUAD_MEMBER_ADDED = "OnSquadMemberAdded"; //Parameters (ICharacter addedCharacter, Squad affectedSquad)
    public static string SQUAD_LEADER_SET = "OnSquadLeaderSet"; //Parameters (ICharacter leader, Squad affectedSquad)
    #endregion

    #region Combat
    public static string COMBAT_DONE = "OnCombatDone"; //Parameters (Combat doneCombat)
    public static string ADD_TO_COMBAT_LOGS = "OnAddToCombatLogs";
    public static string HIGHLIGHT_ATTACKER = "OnHighlightAttacker";
    public static string UNHIGHLIGHT_ATTACKER = "OnUnhighlightAttacker";
    public static string UPDATE_COMBAT_GRIDS = "OnUpdateCombatGrids";
    public static string NEWCOMBAT_DONE = "OnNewCombatDone";
    #endregion

    #region Player
    public static string TOKEN_ADDED = "OnIntelAdded"; //Parameters (Intel addedIntel)
    public static string CHARACTER_TOKEN_ADDED = "OnCharacterIntelAdded"; //Parameters (CharacterIntel characterIntel)
    public static string UPDATED_CURRENCIES = "OnUpdatesCurrencies";
    public static string CHARACTER_ASSIGNED_TO_JOB = "OnCharacterAssignedToJob"; //Parameters (JOB job, Character character);
    public static string CHARACTER_UNASSIGNED_FROM_JOB = "OnCharacterUnassignedFromJob"; //Parameters (JOB job, Character character);
    public static string JOB_ACTION_COOLDOWN_ACTIVATED = "OnJobActionCooldownActivated"; //Parameters (PlayerJobAction action);
    public static string JOB_ACTION_COOLDOWN_DONE = "OnJobActionCooldownDone"; //Parameters (PlayerJobAction action);
    public static string JOB_ACTION_SUB_TEXT_CHANGED = "OnJobActionSubTextChanged"; //Parameters (PlayerJobAction action);
    public static string JOB_SLOT_LOCK_CHANGED = "OnJobSlotLockChanged"; //Parameters (JOB job, bool lockedState);
    public static string PLAYER_OBTAINED_INTEL = "OnPlayerObtainedIntel"; //Parameters (InteractionIntel)
    public static string PLAYER_REMOVED_INTEL = "OnPlayerRemovedIntel"; //Parameters (InteractionIntel)
    #endregion

    #region Interaction
    public static string UPDATED_INTERACTION_STATE = "OnUpdatedInteractionState"; //Parameters (Interaction interaction)
    public static string CHANGED_ACTIVATED_STATE = "OnChangedInteractionState"; //Parameters (Interaction interaction)
    public static string ADDED_INTERACTION = "OnAddedInteraction"; //Parameters (Interaction interaction)
    public static string REMOVED_INTERACTION = "OnRemovedInteraction"; //Parameters (Interaction interaction)
    public static string INTERACTION_ENDED = "OnInteractionEnded"; //Parameters (Interaction interaction)
    public static string MINION_STARTS_INVESTIGATING_AREA = "OnMinionStartInvestigateArea"; //Parameters (Minion minion, Area area)
    public static string INTERACTION_INITIALIZED = "OnInteractionInitialized"; //Parameters (Interaction interaction)
    #endregion

    #region Tokens
    //public static string SPECIAL_TOKEN_RAN_OUT = "OnSpecialTokenRanOut"; //Parameters (SpecialToken specialToken)
    public static string SPECIAL_TOKEN_CREATED = "OnSpecialTokenCreated"; //Parameters (SpecialToken specialToken)
    public static string TOKEN_CONSUMED = "OnTokenConsumed"; //Parameters (SpecialToken specialToken)
    #endregion
}
