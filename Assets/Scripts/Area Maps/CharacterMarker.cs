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

public class CharacterMarker : PooledObject {

    public delegate void HoverMarkerAction(Character character, LocationGridTile location);
    public HoverMarkerAction hoverEnterAction;

    public System.Action hoverExitAction;

    public Character character { get; private set; }

    public Transform visualsParent;
    [SerializeField] private SpriteRenderer mainImg;
    [SerializeField] private SpriteRenderer hoveredImg;
    [SerializeField] private SpriteRenderer clickedImg;
    [SerializeField] private TextMeshPro nameLbl;
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

    [Header("For Testing")]
    [SerializeField] private SpriteRenderer colorHighlight;

    //public MOVEMENT_MODE movementMode { get; private set; }

    //collision
    public List<IPointOfInterest> inVisionPOIs { get; private set; } //POI's in this characters vision collider
    public List<Character> hostilesInRange { get; private set; } //POI's in this characters hostility collider


    private LocationGridTile lastRemovedTileFromPath;
    private List<LocationGridTile> _currentPath;
    private Action _arrivalAction;

    private Action onArrivedAtTileAction;

    private IPointOfInterest _targetPOI;
    private bool shouldRecalculatePath = false;

    private Coroutine currentMoveCoroutine;

    public bool isStillMovingToAnotherTile { get; private set; }
    public InnerPathfindingThread pathfindingThread { get; private set; }
    public POICollisionTrigger collisionTrigger { get; private set; }
    public Vector2 anchoredPos { get; private set; }
    public LocationGridTile destinationTile { get; private set; }
    public bool cannotCombat { get; private set; }
    public float speedModifier { get; private set; }
    public int useWalkSpeed { get; private set; }
    public int targettedByRemoveNegativeTraitActionsCounter { get; private set; }

    private LocationGridTile _previousGridTile;
    private float progressionSpeedMultiplier;

    #region getters/setters
    public List<LocationGridTile> currentPath {
        get { return _currentPath; }
    }
    #endregion

    public void SetCharacter(Character character) {
        this.name = character.name + "'s Marker";
        nameLbl.SetText(character.name);
        this.character = character;
        _previousGridTile = character.gridTileLocation;
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

        PathfindingManager.Instance.AddAgent(pathfindingAI);
    }
    public void UpdateMarkerVisuals() {
        MarkerAsset assets = CharacterManager.Instance.GetMarkerAsset(character.race, character.gender);
        mainImg.sprite = assets.defaultSprite;
        animator.runtimeAnimatorController = assets.animator;
    }
    public void SetOnArriveAtTileAction(Action action) {
        onArrivedAtTileAction = action;
    }
    public void UpdatePosition() {
        //This is checked per update, stress test this for performance

        //I'm keeping a separate field called anchoredPos instead of using the rect transform anchoredPosition directly because the multithread cannot access transform components
        anchoredPos = transform.localPosition;

        //This is to check that the character has moved to another tile
        if (_previousGridTile != character.gridTileLocation) {
            //When a character moves to another tile, check previous tile if the character is the occupant there, if it is, remove it
            LocationGridTile previousGridTile = _previousGridTile;
            LocationGridTile currentGridTile = character.gridTileLocation;
            if (previousGridTile.occupant == character) {
                previousGridTile.RemoveOccupant();
            }

            //Now check if the current grid tile is of different structure than the previous one, if it is, remove character from the previous structure and add it to the current one
            if (previousGridTile.structure != currentGridTile.structure) {
                if (previousGridTile.structure != null) {
                    previousGridTile.structure.RemoveCharacterAtLocation(character);
                } else {
                    Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + character.name + " can't be removed from the structure of " + previousGridTile.ToString() + " because it is null!");
                }
                if (currentGridTile.structure != null) {
                    currentGridTile.structure.AddCharacterAtLocation(character, currentGridTile);
                } else {
                    Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + character.name + " can't be added to the structure of " + currentGridTile.ToString() + " because it is null!");
                }
            }

            //Lastly set the previous tile position to the current tile position so it will not trigger again
            _previousGridTile = character.gridTileLocation;
        }
    }
    public void PlaceMarkerAt(LocationGridTile tile) {
        if(tile != null) {
            transform.position = tile.centeredWorldLocation;
            tile.SetOccupant(character);
        }
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
            switch (goapAction.goapType) {
                case INTERACTION_TYPE.SLEEP_OUTSIDE:
                    if (GoapActionStateDB.GetStateResult(goapAction.goapType, goapState.name) == InteractionManager.Goap_State_Success) {
                        PlaySleepGround();
                    }
                    break;
                default:
                    break;
            }
        }
    }
    private void OnCharacterFinishedAction(Character character, GoapAction action, string result) {
        if (this.character == character) {
            //action icon
            UpdateActionIcon();

            //animation
            switch (action.goapType) {
                case INTERACTION_TYPE.SLEEP_OUTSIDE:
                    PlayIdle();
                    break;
                default:
                    break;
            }
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
                if (character.currentParty.icon.isTravelling && character.currentParty.icon.travelLine == null) {
                    StopMovementOnly();
                }
                pathfindingAI.AdjustDoNotMove(1);
            }
            if (trait.name == "Injured" && trait.responsibleCharacter != null && character.GetTrait("Unconscious") == null) {
                if (hostilesInRange.Contains(trait.responsibleCharacter)) {
                    Debug.Log(character.name + " gained an injured trait. Reacting...");
                    NormalReactToHostileCharacter(trait.responsibleCharacter, CHARACTER_STATE.FLEE);
                }
            }
            UpdateAnimationBasedOnGainedTrait(trait);
        }
    }
    public void OnCharacterLostTrait(Character character, Trait trait) {
        if (character == this.character) {
            if (trait.type == TRAIT_TYPE.DISABLER) { //if the character lost a disabler trait, adjust hinder movement value
                pathfindingAI.AdjustDoNotMove(-1);
            }
            //after this character loses combat recovery trait or unconscious trait, check if he or she can still react to another character, if yes, react.
            switch (trait.name) {
                case "Combat Recovery":
                case "Unconscious":
                    if (hostilesInRange.Count > 0) {
                        Character nearestHostile = GetNearestValidHostile();
                        if (nearestHostile != null) {
                            NormalReactToHostileCharacter(nearestHostile);
                        }
                    }
                    break;
                default:
                    break;
            }
            UpdateAnimationBasedOnLostTrait(trait);
        } else if (hostilesInRange.Contains(character)) {
            //if the character that lost a trait is not this character and that character is in this character's hostility range
            //and the trait that was lost is a negative disabler trait, react to them.
            if (trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
                NormalReactToHostileCharacter(character);
            }

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
        if (currentMoveCoroutine != null) {
            StopCoroutine(currentMoveCoroutine);
        }
        character = null;
        hoverEnterAction = null;
        hoverExitAction = null;
        destinationTile = null;
        PathfindingManager.Instance.RemoveAgent(pathfindingAI);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.RemoveListener<Character, GoapAction>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.RemoveListener<Character, GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
    }
    #endregion

    #region Pathfinding Movement
    public void GoToTile(LocationGridTile destinationTile, IPointOfInterest targetPOI, Action arrivalAction = null) {
        this.destinationTile = destinationTile;
        _arrivalAction = arrivalAction;
        _targetPOI = targetPOI;
        SetDestination(destinationTile.centeredWorldLocation);
        StartMovement();
    }
    public void ArrivedAtLocation() {
        if(character.currentParty.icon.isTravelling && character.gridTileLocation == destinationTile) { //&& destinationTile.occupant == null
            character.currentParty.icon.SetIsTravelling(false);
            //character.currentParty.icon.SetIsPlaceCharacterAsTileObject(true);
            //if (_destinationTile.structure != character.currentStructure) {
            //    character.currentStructure.RemoveCharacterAtLocation(character);
            //    _destinationTile.structure.AddCharacterAtLocation(character, _destinationTile);
            //} else {
            //    character.gridTileLocation.structure.location.areaMap.RemoveCharacter(character.gridTileLocation, character);
            //    _destinationTile.structure.location.areaMap.PlaceObject(character, _destinationTile);
            //}
            destinationTile.SetOccupant(character);
            PlayIdle();
            //if (Messenger.eventTable.ContainsKey(Signals.TILE_OCCUPIED)) {
            //    Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
            //}
            if (_arrivalAction != null) {
                _arrivalAction();
            }
        } 
        //else if (destinationTile != null) {
        //    if(character.currentParty.icon.isTravelling && destinationTile.occupant != null && destinationTile.occupant != character) {
        //        Debug.LogWarning(character.name + " cannot occupy " + destinationTile.ToString() + " because it is already occupied by " + destinationTile.occupant.name);
        //    }
        //}
    }
    private void StartMovement() {
        UpdateSpeed();
        pathfindingAI.SetIsStopMovement(false);
        character.currentParty.icon.SetIsTravelling(true);
        StartWalkingAnimation();
    }
    public void StopMovement(Action afterStoppingAction = null) {
        string log = character.name + " StopMovement function is called!";
        StopMovementOnly();
        log += "\n- Not moving to another tile, go to checker...";
        CheckIfCurrentTileIsOccupiedOnStopMovement(ref log, afterStoppingAction);
    }
    public void StopMovementOnly() {
        _arrivalAction = null;

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
        PlayIdle();
        //log += "\n- Not moving to another tile, go to checker...";
        //CheckIfCurrentTileIsOccupiedOnStopMovement(ref log, afterStoppingAction);
    }
    private void CheckIfCurrentTileIsOccupiedOnStopMovement(ref string log, Action afterStoppingAction = null) {
        if (character.gridTileLocation.tileState == LocationGridTile.Tile_State.Occupied || (character.gridTileLocation.occupant != null && character.gridTileLocation.occupant != character)) {
            log += "\n- Current tile " + character.gridTileLocation.ToString() + " is occupied, will check nearest tile to go to...";
            LocationGridTile newTargetTile = character.gridTileLocation.GetNearestUnoccupiedTileFromThis();
            if(newTargetTile != null) {
                log += "\n- New target tile found: " + newTargetTile.ToString() + ", will go to it...";
                character.marker.GoToTile(newTargetTile, character, afterStoppingAction);
            } else {
                log += "\n- Couldn't find nearby tile, will check nearby tile...";
                newTargetTile = InteractionManager.Instance.GetTargetLocationTile(ACTION_LOCATION_TYPE.NEARBY, character, character.gridTileLocation, character.gridTileLocation.structure);
                if (newTargetTile != null) {
                    log += "\n- New target tile found: " + newTargetTile.ToString() + ", will go to it...";
                    character.marker.GoToTile(newTargetTile, character, afterStoppingAction);
                } else {
                    throw new Exception(character.name + " is stuck and can't go anywhere because everything in the structure is occupied!");
                }
            }
        } else {
            log += "\n- Current tile " + character.gridTileLocation.ToString() + " is not occupied, will stay here and occupy this tile...";
            PlayIdle();
            if (character.currentParty.icon != null) {
                character.currentParty.icon.SetIsTravelling(false);
            }
            character.gridTileLocation.SetOccupant(character);
            if (afterStoppingAction != null) {
                afterStoppingAction();
            }
        }
        Debug.LogWarning(log);
    }
    private void Move() {
        if (character.isDead) {
            //StopMovement();
            Debug.LogWarning(character.name + " is dead! Stopped movement!");
            return;
        }

        //if the current path is not empty
        if (_currentPath != null && _currentPath.Count > 0) {
            LocationGridTile currentTile = _currentPath[0];
            //if(_currentPath.Count == 1) {
            //    //If the path only has 1 node left, this means that this is the destination tile, set the boolean to true so that when this character is placed
            //    //the algorithm will place the character as the object of the destination tile instead of being added in the moving characters list and the tile will be set as occupied
            //    character.currentParty.icon.SetIsPlaceCharacterAsTileObject(true);
            //}
            if (currentTile.structure != character.currentStructure) {
                character.currentStructure.RemoveCharacterAtLocation(character);
                currentTile.structure.AddCharacterAtLocation(character, currentTile);
            } else {
                character.gridTileLocation.structure.location.areaMap.RemoveCharacter(character.gridTileLocation, character);
                currentTile.structure.location.areaMap.PlaceObject(character, currentTile);
            }
            _currentPath.RemoveAt(0);
            lastRemovedTileFromPath = currentTile;
            //character.currentParty.icon.SetIsTravelling(currentIsTravelling);

            string recalculationSummary = string.Empty;
            //check if the marker should recalculate path
            if (shouldRecalculatePath) {
                bool result = RecalculatePath(ref recalculationSummary);
                if (result) return;
            }

            if (onArrivedAtTileAction != null) {
                //If this is not null, it means that this character will not finish the travel
                //Somehow, it is stopped and will this action instead of going to the destination tile
                PlayIdle();
                onArrivedAtTileAction();
                onArrivedAtTileAction = null;
                return;
            } else {
                if (_currentPath.Count <= 0) {
                    if (character.gridTileLocation.charactersHere.Remove(character)) {
                        character.ownParty.icon.SetIsPlaceCharacterAsTileObject(false);
                        character.gridTileLocation.SetOccupant(character);
                    }
                }
            }

            if (character.currentParty.icon.isTravelling) {
                if (_currentPath.Count <= 0) {
                    //Arrival
                    character.currentParty.icon.SetIsTravelling(false);
                    PlayIdle();
                    //if (Messenger.eventTable.ContainsKey(Signals.TILE_OCCUPIED)) {
                    //    Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
                    //}
                    if (_arrivalAction != null) {
                        _arrivalAction();
                    }
                } else {
                    if(_currentPath.Count == 1) {
                        Debug.LogWarning("Tile occupied signal fired for tile " + _currentPath[0].ToString() + " by " + character.name + " because that tile is the next tile and its destination");
                        Messenger.Broadcast(Signals.TILE_OCCUPIED, _currentPath[0], character as IPointOfInterest);
                    }
                    currentMoveCoroutine = StartCoroutine(MoveToPosition(transform.localPosition, _currentPath[0].centeredLocalLocation));
                }
            }
        }
    }
    private IEnumerator MoveToPosition(Vector3 from, Vector3 to) {
        RotateMarker(from, to);

        isStillMovingToAnotherTile = true;
        float t = 0f;
        while (t < 1) {
            if (!GameManager.Instance.isPaused) {
                t += Time.deltaTime / GameManager.Instance.progressionSpeed;
                transform.localPosition = Vector3.Lerp(from, to, t);
                anchoredPos = transform.localPosition;
            }
            yield return null;
        }
        isStillMovingToAnotherTile = false;
        Move();
    }
    public void RotateMarker(Vector3 from, Vector3 to) {
        //float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
        //visualsParent.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, angle);
        //Debug.Log(this.character.name + " is rotating " + angle);
    }
    public void LookAt(Vector3 target) {
        //only allow asset rotation if the character is in idle animation
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
            Vector3 diff = target - transform.position;
            diff.Normalize();
            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            visualsParent.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        }
    }
    public void ReceivePathFromPathfindingThread(InnerPathfindingThread innerPathfindingThread) {
        _currentPath = innerPathfindingThread.path;
        pathfindingThread = null;
        if (innerPathfindingThread.doNotMove) {
            return;
        }
        if (character.minion != null || !character.IsInOwnParty() || character.isDefender || character.doNotDisturb > 0 || character.job == null || character.isWaitingForInteraction > 0) {
            return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
        }
        if (_currentPath != null) {
            //Messenger.AddListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
            character.PrintLogIfActive("Created path for " + innerPathfindingThread.character.name + " from " + innerPathfindingThread.startingTile.ToString() + " to " + innerPathfindingThread.destinationTile.ToString());
            if(character.currentAction != null) {
                character.currentAction.UpdateTargetTile(innerPathfindingThread.destinationTile);
            }
            StartMovement();
        } else {
            Debug.LogError("Can't create path for " + innerPathfindingThread.character.name + " from " + innerPathfindingThread.startingTile.ToString() + " to " + innerPathfindingThread.destinationTile.ToString());
        }
    }
    /// <summary>
    /// Called when this marker needs to recalculate its path, usually because its current target tile is already occupied.
    /// </summary>
    /// <returns>Returns true if the character found another valid target tile.</returns>
    private bool RecalculatePath(ref string pathRecalSummary) {
        bool recalculationResult = false;
        pathRecalSummary = GameManager.Instance.TodayLogString() + this.character.name + "'s marker must recalculate path towards " + _targetPOI.name + "!";
        if (character.currentAction == null) {
            Debug.LogError(character.name + " can't recalculate path because there is no current action!");
            return false;
        }
        if (character.currentAction.poiTarget.gridTileLocation == null) {
            Debug.LogWarning(character.name + " can't recalculate path because the target is either dead or no longer there!");
            character.currentAction.FailAction();
            return true;
        }
        LocationGridTile nearestTileToTarget = character.currentAction.GetTargetLocationTile();
        //if (Messenger.eventTable.ContainsKey(Signals.TILE_OCCUPIED)) {
        //    Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
        //}

        if (nearestTileToTarget != null) {
            pathRecalSummary += "\nGot new target tile " + nearestTileToTarget.ToString() + ". Going there now.";
            //if (currentMoveCoroutine != null) {
            //    StopCoroutine(currentMoveCoroutine);
            //}
            shouldRecalculatePath = false;
            GoToTile(nearestTileToTarget, _targetPOI, _arrivalAction);
            recalculationResult = true;
        } else {
            //there is no longer any available tile for this character, continue towards last target tile.
            //if the next tile is already occupied, stay at the current tile and drop the plan
            pathRecalSummary += "\nCould not find new target tile. Continuing travel to original target tile.";
            if (_currentPath != null && _currentPath.Count > 0) {
                shouldRecalculatePath = false;
                LocationGridTile nextTile = _currentPath[0];
                if ((character.gridTileLocation.tileState == LocationGridTile.Tile_State.Occupied || (character.gridTileLocation.occupant != null && character.gridTileLocation.occupant != character)) || nextTile.isOccupied) {
                    pathRecalSummary += "\nTile " + character.gridTileLocation.ToString() + " or " + nextTile.ToString() + " is occupied. Stopping movement and action.";
                    character.currentAction.FailAction();
                    recalculationResult = true;
                }
            } else if (character.gridTileLocation.tileState == LocationGridTile.Tile_State.Occupied || (character.gridTileLocation.occupant != null && character.gridTileLocation.occupant != character)) {
                shouldRecalculatePath = false;
                pathRecalSummary += "\nCurrent Tile " + character.gridTileLocation.ToString() + " is occupied. Stopping movement and action.";
                character.currentAction.FailAction();
                recalculationResult = true;
            }
        }
        character.PrintLogIfActive(pathRecalSummary);
        return recalculationResult;
    }
    /// <summary>
    /// Listener for when a grid tile has been occupied.
    /// </summary>
    /// <param name="currTile">The tile that was occupied.</param>
    /// <param name="poi">The object that occupied the tile.</param>
    private void OnTileOccupied(LocationGridTile currTile, IPointOfInterest poi) {
        if (destinationTile != null && currTile == destinationTile && poi != this.character) {
            //shouldRecalculatePath = true;
            /*
             When location is **Nearby**, **Random Location**, **Random Location B** or **Near Target** and the character's target location becomes unavailable, 
             he should be informed so that he may attempt to choose another valid location and update his pathfinding. 
             If none is available, character will still attempt to go to last target tile.
             */
            if (this.character.currentAction != null) {
                switch (this.character.currentAction.actionLocationType) {
                    case ACTION_LOCATION_TYPE.NEARBY:
                    case ACTION_LOCATION_TYPE.RANDOM_LOCATION:
                    case ACTION_LOCATION_TYPE.RANDOM_LOCATION_B:
                    case ACTION_LOCATION_TYPE.NEAR_TARGET:
                    case ACTION_LOCATION_TYPE.ON_TARGET:
                        shouldRecalculatePath = true;
                        string recalculationSummary = string.Empty;
                        try {
                            RecalculatePath(ref recalculationSummary);
                        } catch (Exception e) {
                            throw new Exception(e.Message + "\nThere was a problem trying to recalculate path of " + this.character.name + "'s Marker. Recalculation Summary: \n" + recalculationSummary);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
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
    private void ShowPath() {
        if (character != null && _currentPath != null && character.specificLocation != null) {
            character.specificLocation.areaMap.ShowPath(_currentPath);
        }
    }
    private void HidePath() {
        if (character != null && character.specificLocation != null) {
            character.specificLocation.areaMap.HidePath();
        }
    }
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
    private void UpdateAnimationBasedOnGainedTrait(Trait trait) {
        switch (trait.name) {
            case "Unconscious":
                PlaySleepGround();
                break;
            default:
                break;
        }
    }
    private void UpdateAnimationBasedOnLostTrait(Trait trait) {
        switch (trait.name) {
            case "Unconscious":
                PlayIdle();
                break;
            default:
                break;
        }
    }
    private void StartWalkingAnimation() {
        if (!this.gameObject.activeInHierarchy) {
            return;
        }
        StartCoroutine(StartWalking());
    }
    IEnumerator StartWalking() {
        yield return null;
        animator.Play("Walk");
    }
    private void PlayIdle() {
        if (!this.gameObject.activeInHierarchy) {
            return;
        }
        animator.Play("Idle");
    }
    private void PlaySleepGround() {
        if (!this.gameObject.activeInHierarchy) {
            return;
        }
        animator.Play("Sleep Ground");
    }
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
    public bool AddHostileInRange(Character poi, CHARACTER_STATE forcedReaction = CHARACTER_STATE.NONE) {
        if (!hostilesInRange.Contains(poi)) {
            if (this.character.IsHostileWith(poi) 
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

        ////- All characters that see another hostile will drop a non-combat action, if doing any.
        //if (character.IsDoingCombatAction()) {
        //    summary += "\n" + character.name + " is already doing a combat action. Ignoring " + otherCharacter.name;
        //    Debug.Log(summary);
        //    //if currently doing a combat action, do not react to any characters
        //    return;
        //}

        if (forcedReaction != CHARACTER_STATE.NONE) {
            character.stateComponent.SwitchToState(forcedReaction, otherCharacter);
            summary += "\n" + character.name + " was forced to " + forcedReaction.ToString() + ".";
        } else {
            //- Determine whether to enter Flee mode or Engage mode:
            if (character.GetTrait("Injured") != null || character.role.roleType == CHARACTER_ROLE.CIVILIAN
                || character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.LEADER) {
                //- Injured characters, Civilians, Nobles and Faction Leaders always enter Flee mode
                character.stateComponent.SwitchToState(CHARACTER_STATE.FLEE, otherCharacter);
                summary += "\n" + character.name + " chose to flee.";
            } else if (character.doNotDisturb > 0 && character.GetTraitOf(TRAIT_TYPE.DISABLER) != null) {
                //- Disabled characters will not do anything
                summary += "\n" + character.name + " will not do anything.";
            } else if (character.role.roleType == CHARACTER_ROLE.BEAST || character.role.roleType == CHARACTER_ROLE.ADVENTURER
                || character.role.roleType == CHARACTER_ROLE.SOLDIER) {
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

                    //- Uninjured Beasts, Adventurers and Soldiers will enter Engage mode.
                    character.stateComponent.SwitchToState(CHARACTER_STATE.ENGAGE, otherCharacter);
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
        if (character.stateComponent.currentState == null || character.stateComponent.currentState.characterState != CHARACTER_STATE.FLEE) {
            return; //this is for cases that the character is no longer in a flee state, but the pathfinding thread returns a flee path
        }
        Debug.Log(character.name + " computed a flee path!");
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
        //pathfindingAI.canSearch = true;
        PlayIdle();
        //RedetermineFlee();
        (character.stateComponent.currentState as FleeState).CheckForEndState();
    }
    public void SetHasFleePath(bool state) {
        hasFleePath = state;
    }
    #endregion

    #region Engage
    public Character currentlyEngaging { get; private set; }
    public void OnStartEngage() {
        //determine nearest hostile in range
        Character nearestHostile = GetNearestValidHostile();
        //set them as a target
        SetTargetTransform(nearestHostile.marker.transform);
        SetCurrentlyEngaging(nearestHostile);
        StartMovement();
    }
    public void OnReachEngageTarget() {
        Debug.Log(character.name + " has reached engage target!");
        Character enemy = currentlyEngaging;
        //stop the enemy's movement
        enemy.marker.pathfindingAI.AdjustDoNotMove(1);

        //determine whether to start combat or not
        if (cannotCombat) {
            cannotCombat = false;
            (character.stateComponent.currentState as EngageState).CheckForEndState();
        } else {
            EngageState engageState = character.stateComponent.currentState as EngageState;
            Character thisCharacter = this.character;
            
            engageState.CombatOnEngage();

            this.OnFinishCombatWith(enemy);
            enemy.marker.OnFinishCombatWith(this.character);

            ////SetTargetTransform(null);
            ////SetCurrentlyEngaging(null);

            ////if this character died from combat
            ////remove him from the enemies hostiles in range
            //if (thisCharacter.isDead) {
            //    enemy.marker.RemoveHostileInRange(thisCharacter);
            //} 

            //if (enemy.isDead) {
            //    RemoveHostileInRange(enemy);
            //}
        }
        enemy.marker.pathfindingAI.AdjustDoNotMove(-1);
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
    /// <summary>
    /// This is called after this character finishes a combat encounter with another character
    /// regardless if he/she started it or not.
    /// </summary>
    /// <param name="otherCharacter">The character this character fought with</param>
    public void OnFinishCombatWith(Character otherCharacter) {
        if (currentlyEngaging != null && currentlyEngaging == otherCharacter) {
            SetCurrentlyEngaging(null);
            SetTargetTransform(null);
            if (otherCharacter.isDead) {
                RemoveHostileInRange(otherCharacter);
            }
            //exit current state, which should be engage state
            this.character.stateComponent.currentState.OnExitThisState();
        }
    }
    #endregion
}
