using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;
using Pathfinding;
using System.Linq;
using Pathfinding.RVO;

public class CharacterMarker : PooledObject {

    public delegate void HoverMarkerAction(Character character, LocationGridTile location);
    public HoverMarkerAction hoverEnterAction;
    public System.Action hoverExitAction;
    public Character character { get; private set; }

    public Transform visualsParent;
    public TextMeshPro nameLbl;
    [SerializeField] private SpriteRenderer mainImg;
    [SerializeField] private SpriteRenderer hoveredImg;
    [SerializeField] private SpriteRenderer clickedImg;
    [SerializeField] private SpriteRenderer actionIcon;

    [Header("Actions")]
    [SerializeField] private StringSpriteDictionary actionIconDictionary;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Pathfinding")]
    public CharacterAIPath pathfindingAI;    
    public AIDestinationSetter destinationSetter;
    [SerializeField] private Seeker seeker;
    [SerializeField] private Collider2D[] colliders;
    [SerializeField] private RVOController rvoController;
    [SerializeField] private FleeingRVOController fleeingRVOController;

    [Header("For Testing")]
    [SerializeField] private SpriteRenderer colorHighlight;

    //vision colliders
    public List<IPointOfInterest> inVisionPOIs { get; private set; } //POI's in this characters vision collider
    public List<Character> hostilesInRange { get; private set; } //POI's in this characters hostility collider

    //movement
    private Action _arrivalAction;
    public IPointOfInterest targetPOI { get; private set; }
    public InnerPathfindingThread pathfindingThread { get; private set; }
    public POICollisionTrigger collisionTrigger { get; private set; }
    public Vector2 anchoredPos { get; private set; }
    public LocationGridTile destinationTile { get; private set; }
    public bool cannotCombat { get; private set; }
    public float speedModifier { get; private set; }
    public int useWalkSpeed { get; private set; }
    public int targettedByRemoveNegativeTraitActionsCounter { get; private set; }
    public int isStoppedByOtherCharacter { get; private set; } //this is increased, when the action of another character stops this characters movement
    public List<Character> terrifyingCharacters { get; private set; } //list of characters that this character is terrified of and must avoid

    private bool forceFollowTarget; //If the character should follow the target no matter where they go, must only be used with characters
    private LocationGridTile _previousGridTile;
    private float progressionSpeedMultiplier;

    [ContextMenu("Visuals Forward")]
    public void PrintForwardPosition() {
        Debug.Log(visualsParent.up);
    }

    public void SetCharacter(Character character) {
        this.name = character.name + "'s Marker";
        nameLbl.SetText(character.name);
        this.character = character;
        //_previousGridTile = character.gridTileLocation;
        if (UIManager.Instance.characterInfoUI.isShowing) {
            clickedImg.gameObject.SetActive(UIManager.Instance.characterInfoUI.activeCharacter.id == character.id);
        }
        UpdateMarkerVisuals();
        //PlayIdle();

        Vector3 randomRotation = new Vector3(0f, 0f, 90f);
        randomRotation.z *= (float)UnityEngine.Random.Range(1, 4);
        //visualsRT.localRotation = Quaternion.Euler(randomRotation);
        UpdateActionIcon();

        inVisionPOIs = new List<IPointOfInterest>();
        hostilesInRange = new List<Character>();
        terrifyingCharacters = new List<Character>();

        GameObject collisionTriggerGO = GameObject.Instantiate(InteriorMapManager.Instance.characterCollisionTriggerPrefab, this.transform);
        collisionTriggerGO.transform.localPosition = Vector3.zero;
        collisionTrigger = collisionTriggerGO.GetComponent<POICollisionTrigger>();
        collisionTrigger.Initialize(character);

        //flee
        hasFleePath = false;

        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.AddListener<Character, GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);

        PathfindingManager.Instance.AddAgent(pathfindingAI);
        //InteriorMapManager.Instance.AddAgent(rvoController);
    }
    public void UpdateMarkerVisuals() {
        MarkerAsset assets = CharacterManager.Instance.GetMarkerAsset(character.race, character.gender);
        mainImg.sprite = assets.defaultSprite;
        animator.runtimeAnimatorController = assets.animator;
    }
    public void UpdatePosition() {
        //This is checked per update, stress test this for performance

        //I'm keeping a separate field called anchoredPos instead of using the rect transform anchoredPosition directly because the multithread cannot access transform components
        anchoredPos = transform.localPosition;
        //if (_previousGridTile == null) {
        //    _previousGridTile = character.gridTileLocation;
        //}
        if (_previousGridTile != character.gridTileLocation) {
            character.specificLocation.areaMap.OnCharacterMovedTo(character, character.gridTileLocation, _previousGridTile);
            _previousGridTile = character.gridTileLocation;
        }
        ////This is to check that the character has moved to another tile
        //if (_previousGridTile != character.gridTileLocation) {
        //    //When a character moves to another tile, check previous tile if the character is the occupant there, if it is, remove it
        //    LocationGridTile previousGridTile = _previousGridTile;
        //    LocationGridTile currentGridTile = character.gridTileLocation;
        //    if (previousGridTile.occupant == character) {
        //        previousGridTile.RemoveOccupant();
        //    }

        //    //Now check if the current grid tile is of different structure than the previous one, if it is, remove character from the previous structure and add it to the current one
        //    if (previousGridTile.structure != currentGridTile.structure) {
        //        if (previousGridTile.structure != null) {
        //            previousGridTile.structure.RemoveCharacterAtLocation(character);
        //        } else {
        //            Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + character.name + " can't be removed from the structure of " + previousGridTile.ToString() + " because it is null!");
        //        }
        //        if (currentGridTile.structure != null) {
        //            currentGridTile.structure.AddCharacterAtLocation(character, currentGridTile);
        //        } else {
        //            Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + character.name + " can't be added to the structure of " + currentGridTile.ToString() + " because it is null!");
        //        }
        //    }

        //    //Lastly set the previous tile position to the current tile position so it will not trigger again
        //    _previousGridTile = character.gridTileLocation;
        //}
    }
    public void PlaceMarkerAt(LocationGridTile tile) {
        //if(tile != null) {
        this.gameObject.transform.SetParent(tile.parentAreaMap.objectsParent);
        transform.position = tile.centeredWorldLocation;
        tile.structure.location.AddCharacterToLocation(character);
        SetActiveState(true);
        UpdatePosition();
        UpdateAnimation();
        UpdateActionIcon();
            //tile.SetOccupant(character);
        //}
    }

    #region Pointer Functions
    public void SetHoverAction(HoverMarkerAction hoverEnterAction, System.Action hoverExitAction) {
        this.hoverEnterAction = hoverEnterAction;
        this.hoverExitAction = hoverExitAction;
    }
    public void HoverAction() {
        if (hoverEnterAction != null) {
            hoverEnterAction.Invoke(character, character.gridTileLocation);
        }
        //show hovered image
        hoveredImg.gameObject.SetActive(true);
    }
    public void HoverExitAction() {
        if (hoverExitAction != null) {
            hoverExitAction();
        }
        //hide hovered image
        hoveredImg.gameObject.SetActive(false);
    }
    public void OnPointerClick(BaseEventData bd) {
        PointerEventData ped = bd as PointerEventData;
        //character.gridTileLocation.OnClickTileActions(ped.button);
        UIManager.Instance.ShowCharacterInfo(character);
    }
    #endregion

    #region Listeners
    private void OnActionStateSet(Character character, GoapAction goapAction, GoapActionState goapState) {
        if (this.character == character) {
            UpdateAnimation();
        }
    }
    private void OnCharacterFinishedAction(Character character, GoapAction action, string result) {
        if (this.character == character) {
            //action icon
            UpdateActionIcon();
            UpdateAnimation();
        } else {
            //crime system:
            //if the other character committed a crime,
            //check if that character is in this characters vision 
            //and that this character can react to a crime (not in flee or engage mode)
            if (action.IsConsideredACrimeBy(this.character)
                && inVisionPOIs.Contains(character)
                && this.character.CanReactToCrime()) {
                this.character.ReactToCrime(action);
            }
        }
    }
    public void OnCharacterGainedTrait(Character character, Trait trait) {
        //this will make this character flee when he/she gains an injured trait
        if (character == this.character) {
            if (trait.type == TRAIT_TYPE.DISABLER) { //if the character gained a disabler trait, hinder movement
                pathfindingAI.ClearPath();
                //if (character.currentParty.icon.isTravelling && character.currentParty.icon.travelLine == null) {
                //    StopMovementOnly();
                //}
                //rvoController.priority = 0;
                rvoController.enabled = false;
                pathfindingAI.AdjustDoNotMove(1);
            }
            if (trait.name == "Unconscious") {
                //if the character gained an unconscious trait, exit current state if it is flee
                if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.FLEE) {
                    character.stateComponent.currentState.OnExitThisState();
                }
            } else if (trait.name == "Injured" && trait.responsibleCharacter != null && character.GetTrait("Unconscious") == null) {
                if (hostilesInRange.Contains(trait.responsibleCharacter)) {
                    Debug.Log(character.name + " gained an injured trait. Reacting...");
                    NormalReactToHostileCharacter(trait.responsibleCharacter, CHARACTER_STATE.FLEE);
                }
            }
            UpdateAnimation();
            UpdateActionIcon();
        }
    }
    public void OnCharacterLostTrait(Character character, Trait trait) {
        if (character == this.character) {
            if (trait.type == TRAIT_TYPE.DISABLER) { //if the character lost a disabler trait, adjust hinder movement value
                pathfindingAI.AdjustDoNotMove(-1);
                if (pathfindingAI.doNotMove <= 0) {
                    //rvoController.priority = 0.75f;
                    rvoController.enabled = true;
                }
            }
            //after this character loses combat recovery trait or unconscious trait, check if he or she can still react to another character, if yes, react.
            switch (trait.name) {
                case "Combat Recovery":
                case "Unconscious":
                    if (character.GetTrait("Unconscious") == null && character.GetTrait("Combat Recovery") == null) {
                        if (hostilesInRange.Count > 0) {
                            Character nearestHostile = GetNearestValidHostile();
                            if (nearestHostile != null) {
                                NormalReactToHostileCharacter(nearestHostile);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            UpdateAnimation();
            UpdateActionIcon();
        } else if (hostilesInRange.Contains(character)) {
            //if the character that lost a trait is not this character and that character is in this character's hostility range
            //and the trait that was lost is a negative disabler trait, react to them.
            if (trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
                NormalReactToHostileCharacter(character);
            }

        }
    }
    /// <summary>
    /// Listener for when a party starts travelling towards another area.
    /// </summary>
    /// <param name="travellingParty">The travelling party.</param>
    private void OnCharacterAreaTravelling(Party travellingParty) {
        if (targetPOI is Character) {
            Character targetCharacter = targetPOI as Character;
            if (travellingParty.characters.Contains(targetCharacter)) {
                if (currentlyEngaging == targetCharacter) {
                    SetCurrentlyEngaging(null);
                }
                if (forceFollowTarget) {
                    //if this character must follow the target wherever, and the target started travelling to another area, make this character travel to that area too
                    character.currentParty.GoToLocation(travellingParty.icon.targetLocation, PATHFINDING_MODE.NORMAL, travellingParty.icon.targetStructure, _arrivalAction, null, targetPOI);
                } else {
                    //target character left the area
                    //go to the characters last tile
                    GoTo(targetCharacter.gridTileLocation, targetPOI, _arrivalAction);
                }
                if (Messenger.eventTable.ContainsKey(Signals.PARTY_STARTED_TRAVELLING)) {
                    Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
                }
            }
        }
        RemoveHostileInRange(travellingParty.owner);
        RemovePOIFromInVisionRange(travellingParty.owner);

    }
    private void OnCharacterDied(Character otherCharacter) {
        if (otherCharacter != character) {
            if (hostilesInRange.Contains(otherCharacter)) {
                //if this character is currently engaging(chasing) the character that died 
                //and they are not currently in combat, remove the character that died from this characters hostile range
                if (currentlyEngaging == otherCharacter) {
                    if (currentlyCombatting != otherCharacter) {
                        RemoveHostileInRange(otherCharacter);
                    }
                } else {
                    RemoveHostileInRange(otherCharacter);
                }
            }
            
        }
    }
    private void OnCharacterStartedState(Character character, CharacterState state) {
        if (character == this.character) {
            UpdateActionIcon();
        }
    }
    private void OnCharacterEndedState(Character character, CharacterState state) {
        if (character == this.character) {
            UpdateActionIcon();
        }
    }
    #endregion

    #region UI
    private void OnMenuOpened(UIMenu menu) {
        if (menu is CharacterInfoUI) {
            if ((menu as CharacterInfoUI).activeCharacter.id == character.id) {
                clickedImg.gameObject.SetActive(true);
            } else {
                clickedImg.gameObject.SetActive(false);
            }

        }
    }
    private void OnMenuClosed(UIMenu menu) {
        if (menu is CharacterInfoUI) {
            clickedImg.gameObject.SetActive(false);
            UnhighlightMarker();
        }
    }
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED progSpeed) {
        if (progSpeed == PROGRESSION_SPEED.X1) {
            progressionSpeedMultiplier = 1f;
        } else if (progSpeed == PROGRESSION_SPEED.X2) {
            progressionSpeedMultiplier = 1.5f;
        } else if (progSpeed == PROGRESSION_SPEED.X4) {
            progressionSpeedMultiplier = 2f;
        }
        UpdateSpeed();
    }
    #endregion

    #region Action Icon
    public void UpdateActionIcon() {
        if (character == null) {
            return;
        }
        if (character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            actionIcon.gameObject.SetActive(false);
            return;
        }
        if (character.isChatting) {
            actionIcon.sprite = actionIconDictionary[GoapActionStateDB.Social_Icon];
            actionIcon.gameObject.SetActive(true);
        } else {
            if (character.targettedByAction.Count > 0) {
                if (character.targettedByAction != null && character.targettedByAction[0].actionIconString != GoapActionStateDB.No_Icon) {
                    actionIcon.sprite = actionIconDictionary[character.targettedByAction[0].actionIconString];
                    actionIcon.gameObject.SetActive(true);
                } else {
                    actionIcon.gameObject.SetActive(false);
                }
            } else if (character.stateComponent.currentState != null) {
                if (character.stateComponent.currentState.actionIconString != GoapActionStateDB.No_Icon) {
                    actionIcon.sprite = actionIconDictionary[character.stateComponent.currentState.actionIconString];
                    actionIcon.gameObject.SetActive(true);
                } else {
                    actionIcon.gameObject.SetActive(false);
                }
            } else {
                if (character.currentAction != null && character.currentAction.actionIconString != GoapActionStateDB.No_Icon) {
                    actionIcon.sprite = actionIconDictionary[character.currentAction.actionIconString];
                    actionIcon.gameObject.SetActive(true);
                } else {
                    actionIcon.gameObject.SetActive(false);
                }
            }
        }
    }
    private void OnCharacterDoingAction(Character character, GoapAction action) {
        if (this.character == character) {
            UpdateActionIcon();
        }
    }
    public void OnCharacterTargettedByAction(GoapAction action) {
        UpdateActionIcon();
        for (int i = 0; i < action.expectedEffects.Count; i++) {
            if(action.expectedEffects[i].conditionType == GOAP_EFFECT_CONDITION.REMOVE_TRAIT) {
                if(action.expectedEffects[i].conditionKey is string) {
                    string key = (string) action.expectedEffects[i].conditionKey;
                    if(AttributeManager.Instance.allTraits.ContainsKey(key) && AttributeManager.Instance.allTraits[key].effect == TRAIT_EFFECT.NEGATIVE) {
                        AdjustTargettedByRemoveNegativeTraitActions(1);
                    } else if (key == "Negative") {
                        AdjustTargettedByRemoveNegativeTraitActions(1);
                    }
                }
            }
        }
    }
    public void OnCharacterRemovedTargettedByAction(GoapAction action) {
        UpdateActionIcon();
        for (int i = 0; i < action.expectedEffects.Count; i++) {
            if (action.expectedEffects[i].conditionType == GOAP_EFFECT_CONDITION.REMOVE_TRAIT) {
                if (action.expectedEffects[i].conditionKey is string) {
                    string key = (string) action.expectedEffects[i].conditionKey;
                    if (AttributeManager.Instance.allTraits.ContainsKey(key) && AttributeManager.Instance.allTraits[key].effect == TRAIT_EFFECT.NEGATIVE) {
                        AdjustTargettedByRemoveNegativeTraitActions(-1);
                    } else if (key == "Negative") {
                        AdjustTargettedByRemoveNegativeTraitActions(-1);
                    }
                }
            }
        }
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        //StopMovement();
        //if (currentMoveCoroutine != null) {
        //    StopCoroutine(currentMoveCoroutine);
        //}
        //character = null;
        hoverEnterAction = null;
        hoverExitAction = null;
        destinationTile = null;
        PathfindingManager.Instance.RemoveAgent(pathfindingAI);
        //InteriorMapManager.Instance.RemoveAgent(pathfindingAI);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.RemoveListener<Character, GoapAction>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.RemoveListener<Character, GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        if (Messenger.eventTable.ContainsKey(Signals.PARTY_STARTED_TRAVELLING)) {
            Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
        }
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    #endregion

    #region Pathfinding Movement
    public void GoTo(LocationGridTile destinationTile, IPointOfInterest targetPOI, Action arrivalAction = null) {
        this.destinationTile = destinationTile;
        //if (_arrivalAction != null) {
        //    throw new Exception(character.name + " already has an arrival action, but it is being overwritten!");
        //}
        _arrivalAction = arrivalAction;
        this.targetPOI = targetPOI;
        SetTargetTransform(null);
        if (destinationTile == character.gridTileLocation) {
            Debug.Log(character.name + " is already at " + destinationTile.ToString() + " executing action...");
            _arrivalAction?.Invoke();
            _arrivalAction = null;
        } else {
            SetDestination(destinationTile.centeredWorldLocation);
            StartMovement();
        }
        
    }
    public void GoTo(IPointOfInterest targetPOI, Action arrivalAction = null, bool forceFollow = false) {
        //if (_arrivalAction != null) {
        //    throw new Exception(character.name + " already has an arrival action, but it is being overwritten!");
        //}
        _arrivalAction = arrivalAction;
        this.targetPOI = targetPOI;
        switch (targetPOI.poiType) {
            case POINT_OF_INTEREST_TYPE.CHARACTER:
                Character targetCharacter = targetPOI as Character;
                SetTargetTransform(targetCharacter.marker.transform);
                forceFollowTarget = forceFollow;
                //if the target is a character, 
                //check first if he/she is still at the location, 
                if (targetCharacter.specificLocation != character.specificLocation) {
                    //if not, execute the arrival action
                    _arrivalAction?.Invoke();
                    _arrivalAction = null;
                } else if (targetCharacter.currentParty != null && targetCharacter.currentParty.icon != null && targetCharacter.currentParty.icon.isAreaTravelling) {
                    OnCharacterAreaTravelling(targetCharacter.currentParty);
                } else {
                    //else, Add listener for when a character starts to leave a location
                    Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
                }   
                break;
            default:
                SetDestination(targetPOI.gridTileLocation.centeredWorldLocation);
                break;
        }

        StartMovement();
    }
    public void ArrivedAtLocation() {
        StopMovementOnly();
        if (Messenger.eventTable.ContainsKey(Signals.PARTY_STARTED_TRAVELLING)) { Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling); }
        Action action = _arrivalAction;
        //set arrival action to null, because some arrival actions set 
        _arrivalAction = null;
        action?.Invoke();
    }
    private void StartMovement() {
        UpdateSpeed();
        pathfindingAI.SetIsStopMovement(false);
        character.currentParty.icon.SetIsTravelling(true);
        UpdateAnimation();
    }
    //private void StartMovement() {
    //    UpdateSpeed();
    //    pathfindingAI.SetIsStopMovement(false);
    //    if (_currentPath.Count == 0) {
    //        //Arrival
    //            _arrivalAction();
    //        }
    //        //throw new Exception(character.name + "'s marker path count is 0, but movement is starting! Destination Tile is: " + _destinationTile.ToString());
    //    } else {
    //        currentMoveCoroutine = StartCoroutine(MoveToPosition(transform.localPosition, _currentPath[0].centeredLocalLocation));
    //    }
    //    //Messenger.AddListener(Signals.TICK_STARTED, Move);
    //}
    public void StopMovement(Action afterStoppingAction = null) {
        string log = character.name + " StopMovement function is called!";
        StopMovementOnly();
        log += "\n- Not moving to another tile, go to checker...";
        Debug.LogWarning(log);
        //CheckIfCurrentTileIsOccupiedOnStopMovement(ref log, afterStoppingAction);
    }
    public void StopMovementOnly() {
        //_arrivalAction = null;

        //if (Messenger.eventTable.ContainsKey(Signals.TILE_OCCUPIED)) {
        //    Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
        //}
        //if (!isStillMovingToAnotherTile) {
        //    log += "\n- Not moving to another tile, go to checker...";
        //    CheckIfCurrentTileIsOccupiedOnStopMovement(ref log, afterStoppingAction);
        //} else {
        //    log += "\n- Still moving to another tile, wait until tile arrival...";
        //    SetOnArriveAtTileAction(() => CheckIfCurrentTileIsOccupiedOnStopMovement(ref log, afterStoppingAction));
        //}
        //destinationSetter.ClearPath();
        if (character.currentParty.icon != null) {
            character.currentParty.icon.SetIsTravelling(false);
        }
        hasFleePath = false;
        pathfindingAI.SetIsStopMovement(true);
        UpdateAnimation();
        //if (playIdle) {
        //    PlayIdle();
        //}
        //log += "\n- Not moving to another tile, go to checker...";
        //CheckIfCurrentTileIsOccupiedOnStopMovement(ref log, afterStoppingAction);
    }
    //private void CheckIfCurrentTileIsOccupiedOnStopMovement(ref string log, Action afterStoppingAction = null) {
    //    if (character.gridTileLocation.tileState == LocationGridTile.Tile_State.Occupied || (character.gridTileLocation.occupant != null && character.gridTileLocation.occupant != character)) {
    //        log += "\n- Current tile " + character.gridTileLocation.ToString() + " is occupied, will check nearest tile to go to...";
    //        LocationGridTile newTargetTile = character.gridTileLocation.GetNearestUnoccupiedTileFromThis();
    //        if(newTargetTile != null) {
    //            log += "\n- New target tile found: " + newTargetTile.ToString() + ", will go to it...";
    //            character.marker.GoToTile(newTargetTile, character, afterStoppingAction);
    //        } else {
    //            log += "\n- Couldn't find nearby tile, will check nearby tile...";
    //            newTargetTile = InteractionManager.Instance.GetTargetLocationTile(ACTION_LOCATION_TYPE.NEARBY, character, character.gridTileLocation, character.gridTileLocation.structure);
    //            if (newTargetTile != null) {
    //                log += "\n- New target tile found: " + newTargetTile.ToString() + ", will go to it...";
    //                character.marker.GoToTile(newTargetTile, character, afterStoppingAction);
    //            } else {
    //                throw new Exception(character.name + " is stuck and can't go anywhere because everything in the structure is occupied!");
    //            }
    //        }
    //    } else {
    //        log += "\n- Current tile " + character.gridTileLocation.ToString() + " is not occupied, will stay here and occupy this tile...";
    //        PlayIdle();
    //        if (character.currentParty.icon != null) {
    //            character.currentParty.icon.SetIsTravelling(false);
    //        }
    //        character.gridTileLocation.SetOccupant(character);
    //        if (afterStoppingAction != null) {
    //            afterStoppingAction();
    //        }
    //    }
    //    Debug.LogWarning(log);
    //}
    //private void Move() {
    //    if (character.isDead) {
    //        //StopMovement();
    //        Debug.LogWarning(character.name + " is dead! Stopped movement!");
    //        return;
    //    }

    //    //if the current path is not empty
    //    if (_currentPath != null && _currentPath.Count > 0) {
    //        LocationGridTile currentTile = _currentPath[0];
    //        //if(_currentPath.Count == 1) {
    //        //    //If the path only has 1 node left, this means that this is the destination tile, set the boolean to true so that when this character is placed
    //        //    //the algorithm will place the character as the object of the destination tile instead of being added in the moving characters list and the tile will be set as occupied
    //        //    character.currentParty.icon.SetIsPlaceCharacterAsTileObject(true);
    //        //}
    //        if (currentTile.structure != character.currentStructure) {
    //            character.currentStructure.RemoveCharacterAtLocation(character);
    //            currentTile.structure.AddCharacterAtLocation(character, currentTile);
    //        } else {
    //            character.gridTileLocation.structure.location.areaMap.RemoveCharacter(character.gridTileLocation, character);
    //            currentTile.structure.location.areaMap.PlaceObject(character, currentTile);
    //        }
    //        _currentPath.RemoveAt(0);
    //        lastRemovedTileFromPath = currentTile;
    //        //character.currentParty.icon.SetIsTravelling(currentIsTravelling);

    //        string recalculationSummary = string.Empty;
    //        //check if the marker should recalculate path
    //        if (shouldRecalculatePath) {
    //            bool result = RecalculatePath(ref recalculationSummary);
    //            if (result) return;
    //        }

    //        if (onArrivedAtTileAction != null) {
    //            //If this is not null, it means that this character will not finish the travel
    //            //Somehow, it is stopped and will this action instead of going to the destination tile
    //            PlayIdle();
    //            onArrivedAtTileAction();
    //            onArrivedAtTileAction = null;
    //            return;
    //        } else {
    //            if (_currentPath.Count <= 0) {
    //                if (character.gridTileLocation.charactersHere.Remove(character)) {
    //                    character.ownParty.icon.SetIsPlaceCharacterAsTileObject(false);
    //                    character.gridTileLocation.SetOccupant(character);
    //                }
    //            }
    //        }

    //        if (character.currentParty.icon.isTravelling) {
    //            if (_currentPath.Count <= 0) {
    //                //Arrival
    //                character.currentParty.icon.SetIsTravelling(false);
    //                PlayIdle();
    //                //if (Messenger.eventTable.ContainsKey(Signals.TILE_OCCUPIED)) {
    //                //    Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
    //                //}
    //                if (_arrivalAction != null) {
    //                    _arrivalAction();
    //                }
    //            } else {
    //                if(_currentPath.Count == 1) {
    //                    Debug.LogWarning("Tile occupied signal fired for tile " + _currentPath[0].ToString() + " by " + character.name + " because that tile is the next tile and its destination");
    //                    Messenger.Broadcast(Signals.TILE_OCCUPIED, _currentPath[0], character as IPointOfInterest);
    //                }
    //                currentMoveCoroutine = StartCoroutine(MoveToPosition(transform.localPosition, _currentPath[0].centeredLocalLocation));
    //            }
    //        }
    //    }
    //}
    //private IEnumerator MoveToPosition(Vector3 from, Vector3 to) {
    //    RotateMarker(from, to);

    //    isStillMovingToAnotherTile = true;
    //    float t = 0f;
    //    while (t < 1) {
    //        if (!GameManager.Instance.isPaused) {
    //            t += Time.deltaTime / GameManager.Instance.progressionSpeed;
    //            transform.localPosition = Vector3.Lerp(from, to, t);
    //            anchoredPos = transform.localPosition;
    //        }
    //        yield return null;
    //    }
    //    isStillMovingToAnotherTile = false;
    //    Move();
    //}
    //public void RotateMarker(Vector3 from, Vector3 to) {
    //    //float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
    //    //visualsParent.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, angle);
    //    //Debug.Log(this.character.name + " is rotating " + angle);
    //}
    public void LookAt(Vector3 target) {
        if (character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return;
        }
        Vector3 diff = target - transform.position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        visualsParent.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
    }
    //public void ReceivePathFromPathfindingThread(InnerPathfindingThread innerPathfindingThread) {
    //    _currentPath = innerPathfindingThread.path;
    //    pathfindingThread = null;
    //    if (innerPathfindingThread.doNotMove) {
    //        return;
    //    }
    //    if (character.minion != null || !character.IsInOwnParty() || character.isDefender || character.doNotDisturb > 0 || character.job == null || character.isWaitingForInteraction > 0) {
    //        return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
    //    }
    //    if (_currentPath != null) {
    //        //Messenger.AddListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
    //        character.PrintLogIfActive("Created path for " + innerPathfindingThread.character.name + " from " + innerPathfindingThread.startingTile.ToString() + " to " + innerPathfindingThread.destinationTile.ToString());
    //        if(character.currentAction != null) {
    //            character.currentAction.UpdateTargetTile(innerPathfindingThread.destinationTile);
    //        }
    //        StartMovement();
    //    } else {
    //        Debug.LogError("Can't create path for " + innerPathfindingThread.character.name + " from " + innerPathfindingThread.startingTile.ToString() + " to " + innerPathfindingThread.destinationTile.ToString());
    //    }
    //}
    /// <summary>
    /// Called when this marker needs to recalculate its path, usually because its current target tile is already occupied.
    /// </summary>
    /// <returns>Returns true if the character found another valid target tile.</returns>
    //private bool RecalculatePath(ref string pathRecalSummary) {
    //    bool recalculationResult = false;
    //    pathRecalSummary = GameManager.Instance.TodayLogString() + this.character.name + "'s marker must recalculate path towards " + _targetPOI.name + "!";
    //    if (character.currentAction == null) {
    //        Debug.LogError(character.name + " can't recalculate path because there is no current action!");
    //        return false;
    //    }
    //    if (character.currentAction.poiTarget.gridTileLocation == null) {
    //        Debug.LogWarning(character.name + " can't recalculate path because the target is either dead or no longer there!");
    //        character.currentAction.FailAction();
    //        return true;
    //    }
    //    LocationGridTile nearestTileToTarget = character.currentAction.GetTargetLocationTile();
    //    //if (Messenger.eventTable.ContainsKey(Signals.TILE_OCCUPIED)) {
    //    //    Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
    //    //}

    //    if (nearestTileToTarget != null) {
    //        pathRecalSummary += "\nGot new target tile " + nearestTileToTarget.ToString() + ". Going there now.";
    //        //if (currentMoveCoroutine != null) {
    //        //    StopCoroutine(currentMoveCoroutine);
    //        //}
    //        shouldRecalculatePath = false;
    //        GoToTile(nearestTileToTarget, _targetPOI, _arrivalAction);
    //        recalculationResult = true;
    //    } else {
    //        //there is no longer any available tile for this character, continue towards last target tile.
    //        //if the next tile is already occupied, stay at the current tile and drop the plan
    //        pathRecalSummary += "\nCould not find new target tile. Continuing travel to original target tile.";
    //        if (_currentPath != null && _currentPath.Count > 0) {
    //            shouldRecalculatePath = false;
    //            LocationGridTile nextTile = _currentPath[0];
    //            if ((character.gridTileLocation.tileState == LocationGridTile.Tile_State.Occupied || (character.gridTileLocation.occupant != null && character.gridTileLocation.occupant != character)) || nextTile.isOccupied) {
    //                pathRecalSummary += "\nTile " + character.gridTileLocation.ToString() + " or " + nextTile.ToString() + " is occupied. Stopping movement and action.";
    //                character.currentAction.FailAction();
    //                recalculationResult = true;
    //            }
    //        } else if (character.gridTileLocation.tileState == LocationGridTile.Tile_State.Occupied || (character.gridTileLocation.occupant != null && character.gridTileLocation.occupant != character)) {
    //            shouldRecalculatePath = false;
    //            pathRecalSummary += "\nCurrent Tile " + character.gridTileLocation.ToString() + " is occupied. Stopping movement and action.";
    //            character.currentAction.FailAction();
    //            recalculationResult = true;
    //        }
    //    }
    //    character.PrintLogIfActive(pathRecalSummary);
    //    return recalculationResult;
    //}
    /// <summary>
    /// Listener for when a grid tile has been occupied.
    /// </summary>
    /// <param name="currTile">The tile that was occupied.</param>
    /// <param name="poi">The object that occupied the tile.</param>
    //private void OnTileOccupied(LocationGridTile currTile, IPointOfInterest poi) {
    //    if (destinationTile != null && currTile == destinationTile && poi != this.character) {
    //        //shouldRecalculatePath = true;
    //        /*
    //         When location is **Nearby**, **Random Location**, **Random Location B** or **Near Target** and the character's target location becomes unavailable, 
    //         he should be informed so that he may attempt to choose another valid location and update his pathfinding. 
    //         If none is available, character will still attempt to go to last target tile.
    //         */
    //        if (this.character.currentAction != null) {
    //            switch (this.character.currentAction.actionLocationType) {
    //                case ACTION_LOCATION_TYPE.NEARBY:
    //                case ACTION_LOCATION_TYPE.RANDOM_LOCATION:
    //                case ACTION_LOCATION_TYPE.RANDOM_LOCATION_B:
    //                case ACTION_LOCATION_TYPE.NEAR_TARGET:
    //                case ACTION_LOCATION_TYPE.ON_TARGET:
    //                    shouldRecalculatePath = true;
    //                    string recalculationSummary = string.Empty;
    //                    try {
    //                        RecalculatePath(ref recalculationSummary);
    //                    } catch (Exception e) {
    //                        throw new Exception(e.Message + "\nThere was a problem trying to recalculate path of " + this.character.name + "'s Marker. Recalculation Summary: \n" + recalculationSummary);
    //                    }
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }
    //    }
    //}
    public void SetDestination(Vector3 destination) {
        pathfindingAI.destination = destination;
        pathfindingAI.canSearch = true;
    }
    public void SetTargetTransform(Transform target) {
        destinationSetter.target = target;
        pathfindingAI.canSearch = true;
    }
    #endregion

    #region For Testing
    public void HighlightMarker(Color color) {
        colorHighlight.gameObject.SetActive(true);
        colorHighlight.color = color;
    }
    public void UnhighlightMarker() {
        colorHighlight.gameObject.SetActive(false);
    }
    public void HighlightHostilesInRange() {
        for (int i = 0; i < hostilesInRange.Count; i++) {
            hostilesInRange.ElementAt(i).marker.HighlightMarker(Color.red);
        }
    }
    //public Vector3 lookAtTest;
    //[ContextMenu("Look At")]
    //public void LookAtTest() {
    //    LookAt(lookAtTest);
    //}
    #endregion

    #region Animation
    private void PlayWalkingAnimation() {
        if (!this.gameObject.activeInHierarchy) {
            return;
        }
        Play("Walk");
    }
    //IEnumerator StartWalking() {
    //    yield return null;
    //    Debug.Log(character.name + " played walk animation.");
    //    animator.Play("Walk");
    //}
    public void PlayIdle() {
        if (!this.gameObject.activeInHierarchy) {
            return;
        }
        Play("Idle");
    }
    private void PlaySleepGround() {
        if (!this.gameObject.activeInHierarchy) {
            return;
        }
        Play("Sleep Ground");
    }
    private void Play(string animation) {
        //Debug.Log(character.name + " played " + animation + " animation.");
        animator.Play(animation, 0, 0.5f);
        //StartCoroutine(PlayAnimation(animation));
    }
    private void UpdateAnimation() {
        if (character.isDead) {
            return;
        }
        if (character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            PlaySleepGround();
        } else if (isStoppedByOtherCharacter > 0) {
            PlayIdle();
        } else if (character.currentParty.icon != null && character.currentParty.icon.isTravelling) {
            //|| character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL
            PlayWalkingAnimation();
        } else if (character.currentAction != null && character.currentAction.currentState != null && !string.IsNullOrEmpty(character.currentAction.currentState.animationName)) {
            Play(character.currentAction.currentState.animationName);
        } else if (character.currentAction != null && !string.IsNullOrEmpty(character.currentAction.animationName)) {
            Play(character.currentAction.animationName);
        } else {
            PlayIdle();
        }
    }


    //private IEnumerator PlayAnimation(string animation) {
    //    yield return null;
    //    Debug.Log(character.name + " played " + animation + " animation.");
    //    animator.Play(animation);
    //}
    #endregion

    #region Utilities
    public void OnGameLoaded() {
        pathfindingAI.UpdateMe();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = true;
        }
        Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
    }
    private float GetSpeed() {
        float speed = character.raceSetting.runSpeed;
        if(character.stateComponent.currentState == null && targettedByRemoveNegativeTraitActionsCounter > 0) {
            speed = character.raceSetting.walkSpeed;
        } else {
            if (useWalkSpeed > 0) {
                speed = character.raceSetting.walkSpeed;
            } else {
                if (character.stateComponent.currentState != null) {
                    if (character.stateComponent.currentState.characterState == CHARACTER_STATE.EXPLORE 
                        || character.stateComponent.currentState.characterState == CHARACTER_STATE.PATROL
                        || character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL) {
                        //Walk
                        speed = character.raceSetting.walkSpeed;
                    }
                } 
                //else if (character.currentAction != null) {
                //    if (character.currentAction.goapType == INTERACTION_TYPE.STROLL) {
                //        //Walk
                //        speed = character.raceSetting.walkSpeed;
                //    }
                //}
            }
        }
        speed += (speed * speedModifier);
        if (speed <= 0f) {
            speed = 0.5f;
        }
        speed *= progressionSpeedMultiplier;
        return speed;
    }
    public void UpdateSpeed() {
        pathfindingAI.maxSpeed = GetSpeed();
    }
    public void AdjustSpeedModifier(float amount) {
        speedModifier += amount;
        UpdateSpeed();
    }
    public void AdjustUseWalkSpeed(int amount) {
        useWalkSpeed += amount;
        useWalkSpeed = Mathf.Max(0, useWalkSpeed);
    }
    public void AdjustTargettedByRemoveNegativeTraitActions(int amount) {
        targettedByRemoveNegativeTraitActionsCounter += amount;
        targettedByRemoveNegativeTraitActionsCounter = Mathf.Max(0, targettedByRemoveNegativeTraitActionsCounter);
    }
    public void AdjustIsStoppedByOtherCharacter(int amount) {
        isStoppedByOtherCharacter += amount;
        UpdateAnimation();
    }
    public void SetActiveState(bool state) {
        this.gameObject.SetActive(state);
    }
    #endregion

    #region Vision Collision
    public void AddPOIAsInVisionRange(IPointOfInterest poi) {
        if (!inVisionPOIs.Contains(poi)) {
            inVisionPOIs.Add(poi);
            character.AddAwareness(poi);
        }
    }
    public void RemovePOIFromInVisionRange(IPointOfInterest poi) {
        inVisionPOIs.Remove(poi);
    }
    public void LogPOIsInVisionRange() {
        string summary = character.name + "'s POIs in range: ";
        for (int i = 0; i < inVisionPOIs.Count; i++) {
            summary += "\n- " + inVisionPOIs.ElementAt(i).ToString();
        }
        Debug.Log(summary);
    }
    public void ClearPOIsInVisionRange() {
        inVisionPOIs.Clear();
    }
    public void RevalidatePOIsInVisionRange() {
        //check pois in vision to see if they are still valid
        List<IPointOfInterest> invalid = new List<IPointOfInterest>();
        for (int i = 0; i < inVisionPOIs.Count; i++) {
            IPointOfInterest poi = inVisionPOIs[i];
            if (poi.gridTileLocation == null || poi.gridTileLocation.structure != character.currentStructure) {
                invalid.Add(poi);
            }
        }
        for (int i = 0; i < invalid.Count; i++) {
            RemovePOIFromInVisionRange(invalid[i]);
        }
    }
    #endregion

    #region Hosility Collision
    public bool AddHostileInRange(Character poi, CHARACTER_STATE forcedReaction = CHARACTER_STATE.NONE, bool checkHostility = true) {
        if (!hostilesInRange.Contains(poi)) {
            if (!checkHostility || this.character.IsHostileWith(poi) 
                || forcedReaction != CHARACTER_STATE.NONE) { //if forced reaction is not equal to none, it means that this character must treat the other character as hostile, regardless of conditions
                hostilesInRange.Add(poi);
                NormalReactToHostileCharacter(poi, forcedReaction);
                return true;
            }
        }
        return false;
    }
    public void RemoveHostileInRange(Character poi) {
        if (hostilesInRange.Remove(poi)) {
            Debug.Log("Removed hostile in range " + poi.name + " from " + this.character.name);
            UnhighlightMarker(); //This is for testing only!
            OnHostileInRangeRemoved(poi);
        }
    }
    public void ClearHostilesInRange() {
        hostilesInRange.Clear();
    }
    private void OnHostileInRangeRemoved(Character removedCharacter) {
        if (character == null //character died
            || character.stateComponent.currentState == null) {
            return;
        }
        string removeHostileSummary = removedCharacter.name + " was removed from " + character.name + "'s hostile range.";
        if (character.stateComponent.currentState.characterState == CHARACTER_STATE.ENGAGE) {
            removeHostileSummary += "\n" + character.name + "'s current state is engage, checking for end state...";
            if (currentlyEngaging == removedCharacter) {
                (character.stateComponent.currentState as EngageState).CheckForEndState();
            }
        } else if (character.stateComponent.currentState.characterState == CHARACTER_STATE.FLEE) {
            removeHostileSummary += "\n" + character.name + "'s current state is flee, checking for end state...";
            (character.stateComponent.currentState as FleeState).CheckForEndState();
        }
        character.PrintLogIfActive(removeHostileSummary);
    }
    public void OnOtherCharacterDied(Character otherCharacter) {
        RemovePOIFromInVisionRange(otherCharacter);
        //RemoveHostileInRange(otherCharacter);
        if (this.hasFleePath) { //if this character is fleeing, remove the character that died from his/her hostile list
            //this is for cases when this character is fleeing from a character that died because another character assaulted them,
            //and so, the character that died was not removed from this character's hostile list
            hostilesInRange.Remove(otherCharacter);
        }
    }
    #endregion

    #region Reactions
    private void NormalReactToHostileCharacter(Character otherCharacter, CHARACTER_STATE forcedReaction = CHARACTER_STATE.NONE) {
        string summary = character.name + " will react to hostile " + otherCharacter.name;
        if (forcedReaction != CHARACTER_STATE.NONE) {
            character.stateComponent.SwitchToState(forcedReaction, otherCharacter);
            summary += "\n" + character.name + " was forced to " + forcedReaction.ToString() + ".";
        } else {
            //- Determine whether to enter Flee mode or Engage mode:
            //if the character will do a combat action towards the other character, do not flee.
            if (!this.character.IsDoingCombatActionTowards(otherCharacter) && (character.GetTrait("Injured") != null 
                || character.role.roleType == CHARACTER_ROLE.CIVILIAN
                || character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.LEADER)) {
                //- Injured characters, Civilians, Nobles and Faction Leaders always enter Flee mode
                //Check if that character is already in the list of terrifying characters, if it is, do not flee because it will avoid that character already, if not, enter flee mode
                if (!character.marker.terrifyingCharacters.Contains(otherCharacter)) {
                    character.stateComponent.SwitchToState(CHARACTER_STATE.FLEE, otherCharacter);
                }
                summary += "\n" + character.name + " chose to flee.";
            } else if (character.doNotDisturb > 0 && character.GetTraitOf(TRAIT_TYPE.DISABLER) != null) {
                //- Disabled characters will not do anything
                summary += "\n" + character.name + " will not do anything.";
            } else if (character.role.roleType == CHARACTER_ROLE.BEAST || character.role.roleType == CHARACTER_ROLE.ADVENTURER
                || character.role.roleType == CHARACTER_ROLE.SOLDIER) {
                //- Uninjured Beasts, Adventurers and Soldiers will enter Engage mode.
                if (otherCharacter.IsDoingCombatActionTowards(this.character) || this.character.IsDoingCombatActionTowards(otherCharacter)) {
                    //if the other character is already going to assault this character, and this character chose to engage, wait for the other characters assault instead
                    summary += "\n" + otherCharacter.name + " is already or will engage with this character (" + this.character.name + "), waiting for that, instead of starting new engage state.";
                } else {
                    //- A character that is in Flee mode will not trigger combat (but the other side still may)
                    if (hasFleePath) {
                        summary += "\n" + character.name + " is fleeing. Ignoring " + otherCharacter.name;
                        Debug.Log(summary);
                        return;
                    }

                    //- A character that is performing an Action will not trigger combat (but the other side still may)
                    if (character.currentAction != null && character.currentAction.isPerformingActualAction) {
                        summary += "\n" + character.name + " is currently performing" + character.currentAction.goapName + ". Ignoring " + otherCharacter.name;
                        Debug.Log(summary);
                        return;
                    }

                    //- A character in Combat Recovery will not trigger combat (but the other side still may)
                    //- A character that has a Disabler trait will not trigger combat (but the other side still may)
                    //since combat recovery is already a disabler trait, only use 1 case here
                    if (character.HasTraitOf(TRAIT_TYPE.DISABLER)) {
                        summary += "\n" + character.name + " has a disabler trait. Ignoring " + otherCharacter.name;
                        Debug.Log(summary);
                        return;
                    }

                    //- If the other character has a Negative Disabler trait, this character will not trigger combat
                    if (otherCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                        summary += "\n" + otherCharacter.name + " has a negative disabler trait. Ignoring " + otherCharacter.name;
                        Debug.Log(summary);
                        return;
                    }

                    if (this.character.stateComponent.currentState != null &&
                        this.character.stateComponent.currentState.characterState == CHARACTER_STATE.ENGAGE) {
                        //if the character is already in engage mode, check if the other character is nearer than the one that he/she is currently engaging
                        Character originalTarget = this.character.stateComponent.currentState.targetCharacter;
                        float distanceToOG = Vector2.Distance(this.transform.position, originalTarget.marker.transform.position);
                        float distanceToNewTarget = Vector2.Distance(this.transform.position, otherCharacter.marker.transform.position);
                        if (distanceToNewTarget < distanceToOG) {
                            //if yes, engage the other character instead, 
                            character.stateComponent.SwitchToState(CHARACTER_STATE.ENGAGE, otherCharacter);
                        }
                        //else, keep chasing the original target
                    } else {
                        //if the character is not yet in engage mode, engage the new target
                        character.stateComponent.SwitchToState(CHARACTER_STATE.ENGAGE, otherCharacter);
                    }
                    summary += "\n" + character.name + " chose to engage.";
                }
            }
        }
        Debug.Log(summary);
    }
    #endregion

    #region Flee
    public bool hasFleePath { get; private set; }
    public void OnStartFlee() {
        //if (hasFleePath) {
        //    return;
        //}
        if (hostilesInRange.Count == 0) {
            return;
        }
        //if (character.currentAction != null) {
        //    character.currentAction.StopAction();
        //}
        hasFleePath = true;
        pathfindingAI.canSearch = false; //set to false, because if this is true and a destination has been set in the ai path, the ai will still try and go to that point instead of the computed flee path
        FleeMultiplePath fleePath = FleeMultiplePath.Construct(this.transform.position, hostilesInRange.Select(x => x.marker.transform.position).ToArray(), 10000);
        fleePath.aimStrength = 1;
        fleePath.spread = 4000;
        seeker.StartPath(fleePath, OnFleePathComputed);
        //UIManager.Instance.Pause();
        //Debug.LogWarning(character.name + " is fleeing!");
    }
    private void OnFleePathComputed(Path path) {
        if (character == null || character.stateComponent.currentState == null || character.stateComponent.currentState.characterState != CHARACTER_STATE.FLEE 
            || character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return; //this is for cases that the character is no longer in a flee state, but the pathfinding thread returns a flee path
        }
        //Debug.Log(character.name + " computed a flee path!");
        StartMovement();
    }
    public void RedetermineFlee() {
        if (hostilesInRange.Count == 0) {
            return;
        }
        //if (character.currentAction != null) {
        //    character.currentAction.StopAction();
        //}
        hasFleePath = true;
        pathfindingAI.canSearch = false; //set to false, because if this is true and a destination has been set in the ai path, the ai will still try and go to that point instead of the computed flee path
        FleeMultiplePath fleePath = FleeMultiplePath.Construct(this.transform.position, hostilesInRange.Select(x => x.marker.transform.position).ToArray(), 10000);
        fleePath.aimStrength = 1;
        fleePath.spread = 4000;
        seeker.StartPath(fleePath, OnFleePathComputed);
    }
    public void OnFinishFleePath() {
        Debug.Log(name + " has finished traversing flee path.");
        hasFleePath = false;
        UpdateAnimation();
        (character.stateComponent.currentState as FleeState).CheckForEndState();
    }

    public void SetHasFleePath(bool state) {
        hasFleePath = state;
    }
    public void AddTerrifyingCharacter(Character character) {
        //terrifyingCharacters += amount;
        //terrifyingCharacters = Math.Max(0, terrifyingCharacters);
        if (!terrifyingCharacters.Contains(character)) {
            terrifyingCharacters.Add(character);
            UpdateFleeingRVOController();
        }
    }
    public void RemoveTerrifyingCharacter(Character character) {
        if (terrifyingCharacters.Remove(character)) {
            UpdateFleeingRVOController();
        }
    }
    public void ClearTerrifyingCharacters() {
        terrifyingCharacters.Clear();
        UpdateFleeingRVOController();
    }
    private void UpdateFleeingRVOController() {
        if (terrifyingCharacters.Count > 0) {
            rvoController.collidesWith = RVOLayer.DefaultAgent | RVOLayer.DefaultObstacle | RVOLayer.Layer2;
        } else {
            rvoController.collidesWith = RVOLayer.DefaultAgent | RVOLayer.DefaultObstacle;
        }
    }
    #endregion

    #region Engage
    public Character currentlyEngaging { get; private set; }
    public Character currentlyCombatting { get; private set; }
    private string engageSummary;

    public void OnStartEngage(Character target) {
        //determine nearest hostile in range
        //Character nearestHostile = GetNearestValidHostile();
        //set them as a target
        //SetTargetTransform(nearestHostile.marker.transform);
        if (currentlyEngaging != null) {
            engageSummary += this.character.name + " has decided to no longer pursue " + currentlyEngaging.name + "\n";
        }
        engageSummary += this.character.name + " will now engage " + target.name + "\n";

        GoTo(target, () => OnReachEngageTarget());
        SetCurrentlyEngaging(target);
        //StartMovement();
    }
    public void OnReachEngageTarget() {
        if (currentlyEngaging == null) {
            return;
        }

        engageSummary += this.character.name + " has reached engage target " + currentlyEngaging.name + "\n";

        Character enemy = currentlyEngaging;
        //stop the enemy's movement
        enemy.marker.pathfindingAI.AdjustDoNotMove(1);

        //determine whether to start combat or not
        if (cannotCombat) {
            cannotCombat = false;
            if (character.stateComponent.currentState is EngageState) {
                (character.stateComponent.currentState as EngageState).CheckForEndState();
            } else {
                throw new Exception(character.name + " reached engage target, but not in engage state! CurrentState is " + character.stateComponent.currentState?.stateName ?? "Null");
            }
        } else {
            EngageState engageState = character.stateComponent.currentState as EngageState;
            Character thisCharacter = this.character;

            //remove enemy's current action
            enemy.AdjustIsWaitingForInteraction(1);
            if (enemy.currentAction != null && !enemy.currentAction.isDone) {
                if (!enemy.currentAction.isPerformingActualAction) {
                    enemy.SetCurrentAction(null);
                } else {
                    enemy.currentAction.currentState.EndPerTickEffect();
                }
            }
            enemy.AdjustIsWaitingForInteraction(-1);

            engageState.CombatOnEngage();

            this.OnFinishCombatWith(enemy);
            enemy.marker.OnFinishCombatWith(this.character);
        }
        enemy.marker.pathfindingAI.AdjustDoNotMove(-1);
        Debug.Log(engageSummary);
        engageSummary = string.Empty;
    }
    public void SetCannotCombat(bool state) {
        cannotCombat = state;
    }
    public Character GetNearestValidHostile() {
        Character nearest = null;
        float nearestDist = 9999f;
        for (int i = 0; i < hostilesInRange.Count; i++) {
            Character currHostile = hostilesInRange.ElementAt(i);
            if (IsValidCombatTarget(currHostile)) {
                float dist = Vector2.Distance(this.transform.position, currHostile.marker.transform.position);
                if (nearest == null || dist < nearestDist) {
                    nearest = currHostile;
                    nearestDist = dist;
                }
            }
        }
        return nearest;
    }
    private bool IsValidCombatTarget(Character otherCharacter) {
        //- If the other character has a Negative Disabler trait, this character will not trigger combat
        return !otherCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
    }
    public void SetCurrentlyEngaging(Character character) {
        currentlyEngaging = character;
        //Debug.Log(GameManager.Instance.TodayLogString() + this.character.name + " set as engaging " + currentlyEngaging?.ToString() ?? "null");
    }
    public void SetCurrentlyCombatting(Character character) {
        currentlyCombatting = character;
        //Debug.Log(GameManager.Instance.TodayLogString() + this.character.name + " set as engaging " + currentlyEngaging?.ToString() ?? "null");
    }
    /// <summary>
    /// This is called after this character finishes a combat encounter with another character
    /// regardless if he/she started it or not.
    /// </summary>
    /// <param name="otherCharacter">The character this character fought with</param>
    private void OnFinishCombatWith(Character otherCharacter) {
        if (!this.character.isDead && currentlyCombatting != null && currentlyCombatting == otherCharacter) {
            SetCurrentlyEngaging(null);
            SetCurrentlyCombatting(null);
            SetTargetTransform(null);
            if (otherCharacter.isDead) { //remove hostile character in range, because the listener for character death is only for characters that did not enter combat with the other character
                RemoveHostileInRange(otherCharacter);
            }
        }
    }
    #endregion
}
