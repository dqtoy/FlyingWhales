using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public static class Signals {

    public static string TICK_STARTED = "OnTickStart";
    //public static string TICK_STARTED_2 = "OnTickStart2";
    public static string TICK_ENDED = "OnTickEnd";
    //public static string TICK_ENDED_2 = "OnTickEnd2";
    public static string HOUR_STARTED = "OnHourStart";
    public static string DAY_STARTED = "OnDayStart";
    public static string MONTH_START = "OnMonthStart";
    public static string MONTH_END = "OnMonthEnd";
    public static string FOUND_ITEM = "OnItemFound"; //Parameters (Character characterThatFoundItem, Item foundItem)
    public static string FOUND_TRACE = "OnTraceFound"; //Parameters (Character characterThatFoundTrace, string traceFound)
    public static string GAME_LOADED = "OnGameLoaded";
    public static string TOGGLE_CHARACTERS_VISIBILITY = "OnToggleCharactersVisibility";
    public static string INSPECT_ALL = "InspectAll";
    /// <summary>
    /// Parameters: KeyCode (Pressed Key)
    /// </summary>
    public static string KEY_DOWN = "OnKeyDown";
    /// <summary>
    /// Parameters: GameObject (Destroyed Object)
    /// </summary>
    public static string POOLED_OBJECT_DESTROYED = "OnPooledObjectDestroyed";

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
    public static string AREA_FOOD_CHANGED = "OnAreaFoodSet"; //Parameters (Area affectedArea)
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
    public static string AREA_MAP_OPENED = "OnAreaMapOpened"; //parameters (Area area)
    public static string AREA_MAP_CLOSED = "OnAreaMapClosed"; //parameters (Area area)
    #endregion

    #region Landmarks
    public static string ITEM_PLACED_AT_LANDMARK = "OnItemPlacedAtLandmark"; //Parameters (Item item, BaseLandmark landmark)
    public static string ITEM_REMOVED_FROM_LANDMARK = "OnItemRemovedFromLandmark"; //Parameters (Item item, BaseLandmark landmark)
    public static string STRUCTURE_STATE_CHANGED = "OnStructureStateChanged"; //Parameters (StructureObj obj, ObjectState newState)
    public static string LANDMARK_ATTACK_TARGET_SELECTED = "OnLandmarkAttackTargetSelected"; //Parameters (BaseLandmark target)
    public static string CHARACTER_ENTERED_LANDMARK = "OnCharacterEnteredLandmark"; //Parameters (Character, BaseLandmark)
    public static string CHARACTER_EXITED_LANDMARK = "OnCharacterExitedLandmark"; //Parameters (Characte, BaseLandmark)
    public static string DESTROY_LANDMARK = "OnDestroyLandmark"; //Parameteres (BaseLandmark destroyedLandmark)
    public static string LANDMARK_UNDER_ATTACK = "OnLandmarkUnderAttack"; //Parameters (BaseLandmark underAttackedLandmark, GameEvent associatedEvent = null)
    public static string LANDMARK_INSPECTED = "OnLandmarkInspected"; //Parameters (BaseLandmark inspectedLandmark)
    public static string LANDMARK_RESIDENT_ADDED = "OnLandmarkResidentAdded"; //Parameters (BaseLandmark affectedLandmark, ICharacter character)
    public static string LANDMARK_RESIDENT_REMOVED = "OnLandmarkResidentRemoved"; //Parameters (BaseLandmark affectedLandmark, ICharacter character)
    public static string LANDMARK_INVESTIGATION_ACTIVATED = "OnLandmarkInvestigationActivated"; //Parameters (BaseLandmark investigatedLandmark)
    public static string UPDATE_RITUAL_CIRCLE_TRAIT = "OnUpdateRitualCircleTrait";
    /// <summary>
    /// Parameters: BaseLandmark, WorldEvent
    /// </summary>
    public static string WORLD_EVENT_SPAWNED = "OnWorldEventSpawnedAtLandmark";
    /// <summary>
    /// Parameters: BaseLandmark, WorldEvent
    /// </summary>
    public static string WORLD_EVENT_FINISHED_NORMAL = "OnWorldEventFinishedNormallyAtLandmark";
    /// <summary>
    /// Parameters: BaseLandmark, WorldEvent
    /// </summary>
    public static string WORLD_EVENT_DESPAWNED = "OnWorldEventDespawnedAtLandmark";
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
    public static string CHARACTER_OBTAINED_ITEM = "OnCharacterObtainItem"; //Parameters (SpecialToken obtainedItem, Character characterThatObtainedItem)
    public static string CHARACTER_LOST_ITEM = "OnCharacterLostItem"; //Parameters (SpecialToken unobtainedItem, Character character)
    public static string CHARACTER_MARKED = "OnCharacterMarked";
    public static string CHARACTER_INSPECTED = "OnCharacterInspected"; //Parameters (Character inspectedCharacter)
    public static string CHARACTER_LEVEL_CHANGED = "OnCharacterLevelChange"; //Parameters (Character character)
    public static string TRAIT_ADDED = "OnTraitAdded";
    public static string TRAIT_REMOVED = "OnTraitRemoved"; //Parameters (Character character, Trait)
    public static string ADJUSTED_HP = "OnAdjustedHP";
    public static string PARTY_STARTED_TRAVELLING = "OnPartyStartedTravelling"; //Parameters (Party travellingParty)
    public static string PARTY_DONE_TRAVELLING = "OnPartyDoneTravelling"; //Parameters (Party travellingParty)
    public static string CHARACTER_MIGRATED_HOME = "OnCharacterChangedHome"; //Parameters (Character, Area previousHome, Area newHome); 
    public static string CHARACTER_CHANGED_RACE = "OnCharacterChangedRace"; //Parameters (Character); 
    public static string CHARACTER_ARRIVED_AT_STRUCTURE = "OnCharacterArrivedAtStructure"; //Parameters (Character, LocationStructure); 
    public static string RELATIONSHIP_ADDED = "OnCharacterGainedRelationship"; //Parameters (Character, RelationshipTrait)
    public static string RELATIONSHIP_REMOVED = "OnCharacterRemovedRelationship"; //Parameters (Character, RELATIONSHIP_TRAIT, AlterEgoData)
    public static string ALL_RELATIONSHIP_REMOVED = "OnCharacterRemovedAllRelationship"; //Parameters (Character, Character)
    public static string CHARACTER_TRACKED = "OnCharacterTracked"; //Parameters (Character character)
    public static string CANCEL_CURRENT_ACTION = "OnCancelCurrentAction"; //Parameters (Character target, string cause)
    public static string CHARACTER_STARTED_STATE = "OnCharacterStartedState"; //Parameters (Character character, CharacterState state)
    public static string CHARACTER_ENDED_STATE = "OnCharacterEndedState"; //Parameters (Character character, CharacterState state)
    public static string CHARACTER_SWITCHED_ALTER_EGO = "OnCharacterSwitchedAlterEgo"; //Parameters (Character character)
    public static string DETERMINE_COMBAT_REACTION = "DetermineCombatReaction"; //Parameters (Character character)
    public static string TRANSFER_ENGAGE_TO_FLEE_LIST = "TransferEngageToFleeList"; //Parameters (Character character)W
    /// <summary>
    /// Parameters (Character characterWithVision, Character characterRemovedFromVision)
    /// </summary>
    public static string CHARACTER_REMOVED_FROM_VISION = "OnCharacterRemovedFromVision";
    /// <summary>
    /// Parameters (Character)
    /// </summary>
    public static string CHARACTER_STARTED_MOVING = "OnCharacterStartedMoving";
    /// <summary>
    /// Parameters (Character)
    /// </summary>
    public static string CHARACTER_STOPPED_MOVING = "OnCharacterStoppedMoving";
    /// <summary>
    /// Parameters (Character characterHit, Character chaarcterHitBy)
    /// </summary>
    public static string CHARACTER_WAS_HIT = "OnCharacterHit";
    /// <summary>
    /// Parameters (Character)
    /// </summary>
    public static string CHARACTER_RETURNED_TO_LIFE = "OnCharacterReturnedToLife";
    #endregion

    #region UI
    public static string SHOW_POPUP_MESSAGE = "ShowPopupMessage"; //Parameters (string message, MESSAGE_BOX_MODE mode, bool expires)
    public static string HIDE_POPUP_MESSAGE = "HidePopupMessage";
    public static string UPDATE_UI = "UpdateUI";
    /// <summary>
    /// Parameters (string text, int expiry, UnityAction onClickAction)
    /// </summary>
    public static string SHOW_DEVELOPER_NOTIFICATION = "ShowNotification";
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
    public static string SHOW_INTEL_NOTIFICATION = "ShowIntelNotification"; //Parameters (Intel)
    public static string SHOW_PLAYER_NOTIFICATION = "ShowPlayerNotification"; //Parameters (Log)
    public static string CAMERA_OUT_OF_FOCUS = "CameraOutOfFocus";
    public static string ON_OPEN_SHARE_INTEL = "OnOpenShareIntel";
    public static string ON_CLOSE_SHARE_INTEL = "OnCloseShareIntel";
    public static string SHOW_TIMER_HUB_ITEM = "ShowTimerHubItem";
    public static string AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT = "OnAreaInfoUIUpdateAppropriateContent";
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
    public static string OLD_NEWS_TRIGGER = "OnOldNewsTrigger"; //Parameters (IPointOfInterest poi)
    //public static string ON_TARGETTED_BY_ACTION = "OnCharacterTargettedByAction"; //Parameters (Character, GoapAction)
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
    public static string MINION_ASSIGNED_TO_JOB = "OnCharacterAssignedToJob"; //Parameters (JOB job, Character character);
    public static string MINION_UNASSIGNED_FROM_JOB = "OnCharacterUnassignedFromJob"; //Parameters (JOB job, Character character);
    public static string JOB_ACTION_COOLDOWN_ACTIVATED = "OnJobActionCooldownActivated"; //Parameters (PlayerJobAction action);
    public static string JOB_ACTION_COOLDOWN_DONE = "OnJobActionCooldownDone"; //Parameters (PlayerJobAction action);
    public static string JOB_ACTION_SUB_TEXT_CHANGED = "OnJobActionSubTextChanged"; //Parameters (PlayerJobAction action);
    public static string JOB_SLOT_LOCK_CHANGED = "OnJobSlotLockChanged"; //Parameters (JOB job, bool lockedState);
    public static string PLAYER_OBTAINED_INTEL = "OnPlayerObtainedIntel"; //Parameters (InteractionIntel)
    public static string PLAYER_REMOVED_INTEL = "OnPlayerRemovedIntel"; //Parameters (InteractionIntel)
    /// <summary>
    /// Parameters (Summon newSummon)
    /// </summary>
    public static string PLAYER_GAINED_SUMMON = "OnPlayerGainedSummon";
    public static string PLAYER_GAINED_SUMMON_LEVEL = "OnPlayerGainedSummonLevel";
    /// <summary>
    /// Parameters (Summon removedSummon)
    /// </summary>
    public static string PLAYER_REMOVED_SUMMON = "OnPlayerRemovedSummon";
    /// <summary>
    /// Parameters (Summon placedSummon)
    /// </summary>
    public static string PLAYER_PLACED_SUMMON = "OnPlayerPlacedSummon";
    /// <summary>
    /// Parameters (Artifact newArtifact)
    /// </summary>
    public static string PLAYER_GAINED_ARTIFACT = "OnPlayerGainedArtifact";
    public static string PLAYER_GAINED_ARTIFACT_LEVEL = "OnPlayerGainedArtifactLevel";
    /// <summary>
    /// Parameters (Artifact removedArtifact)
    /// </summary>
    public static string PLAYER_REMOVED_ARTIFACT = "OnPlayerRemovedArtifact";
    public static string PLAYER_USED_ARTIFACT = "OnPlayerUsedArtifact";
    /// <summary>
    /// Parameters (Area invadedArea)
    /// </summary>
    public static string SUCCESS_INVASION_AREA = "OnPlayerSuccessInvadeArea";
    /// <summary>
    /// parameters (Minion affectedMinion, PlayerJobAction)
    /// </summary>
    public static string PLAYER_LEARNED_INTERVENE_ABILITY = "OnMinionLearnedInterveneAbility";
    public static string PLAYER_CONSUMED_INTERVENE_ABILITY = "OnPlayerConsumedInterveneAbility";
    public static string PLAYER_GAINED_INTERVENE_LEVEL = "OnPlayerGainedInterveneLevel";
    /// <summary>
    /// parameters (Minion)
    /// </summary>
    public static string PLAYER_GAINED_MINION = "OnPlayerGainedMinion";
    /// <summary>
    /// parameters (Minion)
    /// </summary>
    public static string PLAYER_LOST_MINION = "OnPlayerLostMinion";
    /// <summary>
    /// parameters (Minion, BaseLandmark)
    /// </summary>
    public static string MINION_CHANGED_ASSIGNED_REGION = "OnMinionChangedInvadingLandmark";
    #endregion

    #region Interaction
    public static string UPDATED_INTERACTION_STATE = "OnUpdatedInteractionState"; //Parameters (Interaction interaction)
    public static string CHANGED_ACTIVATED_STATE = "OnChangedInteractionState"; //Parameters (Interaction interaction)
    public static string ADDED_INTERACTION = "OnAddedInteraction"; //Parameters (Interaction interaction)
    public static string REMOVED_INTERACTION = "OnRemovedInteraction"; //Parameters (Interaction interaction)
    public static string INTERACTION_ENDED = "OnInteractionEnded"; //Parameters (Interaction interaction)
    public static string MINION_STARTS_INVESTIGATING_AREA = "OnMinionStartInvestigateArea"; //Parameters (Minion minion, Area area)
    public static string INTERACTION_INITIALIZED = "OnInteractionInitialized"; //Parameters (Interaction interaction)
    public static string EVENT_POPPED_UP = "OnEventPopup"; //Parameters (EventPopup)
    #endregion

    #region Tokens
    //public static string SPECIAL_TOKEN_RAN_OUT = "OnSpecialTokenRanOut"; //Parameters (SpecialToken specialToken)
    public static string SPECIAL_TOKEN_CREATED = "OnSpecialTokenCreated"; //Parameters (SpecialToken specialToken)
    public static string TOKEN_CONSUMED = "OnTokenConsumed"; //Parameters (SpecialToken specialToken)
    #endregion

    #region GOAP
    public static string CHARACTER_WILL_DO_PLAN = "OnCharacterRecievedPlan"; //Parameters (Character, GoapPlan)
    public static string CHARACTER_DID_ACTION = "OnCharacterDidAction"; //Parameters (Character, GoapAction)
    public static string STOP_ACTION = "OnStopAction"; //Parameters (GoapAction)
    public static string CHARACTER_FINISHED_ACTION = "OnCharacterFinishedAction"; //Parameters (Character, GoapAction, String result)
    public static string CHARACTER_DOING_ACTION = "OnCharacterDoingAction"; //Parameters (Character, GoapAction)
    public static string ACTION_STATE_SET = "OnActionStateSet"; //Parameters (Character, GoapAction, GoapActionState)
    public static string CHARACTER_PERFORMING_ACTION = "OnCharacterPerformingAction"; //Parameters (Character, GoapAction)
    public static string ON_SET_JOB = "OnSetJob"; //Parameters (GoapPlanJob)
    #endregion

    #region Location Grid Tile
    public static string TILE_OCCUPIED = "OnTileOccupied"; //Parameters (LocationGridTile, IPointOfInterest)
    public static string CHECK_GHOST_COLLIDER_VALIDITY = "CheckGhostColliderValidity"; //Parameters (IPointOfInterest, List<LocationGridTile>)
    public static string OBJECT_PLACED_ON_TILE = "OnObjectPlacedOnTile"; //Parameters (LocationGridTile, IPointOfInterest)
    public static string TILE_OBJECT_REMOVED = "OnTileObjectDestroyed"; //Parameters (TileObject, Character removedBy)
    public static string TILE_OBJECT_PLACED = "OnTileObjectPlaced"; //Parameters (TileObject, LocationGridTile)
    public static string TILE_OBJECT_DISABLED = "OnTileObjectDisabled"; //Parameters (TileObject, Character removedBy)
    public static string ITEM_REMOVED_FROM_TILE = "OnItemRemovedFromTile"; //Parameters (SpecialToken, LocationGridTile)
    public static string ITEM_PLACED_ON_TILE = "OnItemPlacedOnTile"; //Parameters (SpecialToken, LocationGridTile)
    #endregion

    #region Combat Ability
    public static string COMBAT_ABILITY_UPDATE_BUTTON = "OnCombatAbilityStopCooldown";
    #endregion

    #region ITraitables
    /// <summary>
    /// Parameters (ITraitable, Trait)
    /// </summary>
    public static string TRAITABLE_GAINED_TRAIT = "OnTraitableGainedTrait";
    /// <summary>
    /// Parameters (ITraitable, Trait, Character removedBy)
    /// </summary>
    public static string TRAITABLE_LOST_TRAIT = "OnTraitableLostTrait";
    #endregion

    public static Dictionary<string, SignalMethod[]> orderedSignalExecution = new Dictionary<string, SignalMethod[]>() {
        { HOUR_STARTED, new SignalMethod[] {
            new SignalMethod() { methodName = "HourlyJobActions", objectType = typeof(Area) },
            new SignalMethod() { methodName = "DecreaseNeeds", objectType = typeof(Character) },
            new SignalMethod() { methodName = "PerHour", objectType = typeof(Zombie_Virus) },
        }},
        { TICK_STARTED, new SignalMethod[] {
            new SignalMethod() { methodName = "CheckSupply", objectType = typeof(SupplyPile) },
            new SignalMethod() { methodName = "CheckFood", objectType = typeof(FoodPile) },
            new SignalMethod() { methodName = "PerTick", objectType = typeof(TimerHubUI) },
            new SignalMethod() { methodName = string.Empty, objectType = typeof(Trait) },
            new SignalMethod() { methodName = "PerTickEffect", objectType = typeof(GoapActionState) },
            new SignalMethod() { methodName = "PerTickGoapPlanGeneration", objectType = typeof(Character) },
            new SignalMethod() { methodName = "PerTickInterventionAbility", objectType = typeof(Player) },
        }},
        { TICK_ENDED, new SignalMethod[] {
            new SignalMethod() { methodName = "CheckSchedule", objectType = typeof(SchedulingManager) },
            new SignalMethod() { methodName = string.Empty, objectType = typeof(Trait) },
            new SignalMethod() { methodName = string.Empty, objectType = typeof(Artifact) },
            new SignalMethod() { methodName = "PerTickMovement", objectType = typeof(CharacterMarker) },
            new SignalMethod() { methodName = "PerTickInState", objectType = typeof(CharacterState) },
            new SignalMethod() { methodName = "PerTickInvasion", objectType = typeof(Player) },
        }},
    };

    public static bool TryGetMatchingSignalMethod(string eventType, Callback method, out SignalMethod matching) {
        for (int i = 0; i < orderedSignalExecution[eventType].Length; i++) {
            SignalMethod sm = orderedSignalExecution[eventType][i];
            if (sm.Equals(method)) {
                matching = sm;
                return true;
            }
        }
        matching = default(SignalMethod);
        return false;
    }
}

public struct SignalMethod {
    public string methodName;
    public System.Type objectType;

    public bool Equals(Delegate d) {
        if (d.Method.Name.Contains(methodName) && (d.Target.GetType() == objectType || d.Target.GetType().BaseType == objectType)) {
            return true;
        }
        if (string.IsNullOrEmpty(methodName) && (d.Target.GetType() == objectType || d.Target.GetType().BaseType == objectType)) {
            //if the required method name is null, and the provided object is of the same type, consider it a match
            return true;
        }

        return false;
    }
}
