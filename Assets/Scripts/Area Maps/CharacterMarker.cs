using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;

public class CharacterMarker : PooledObject {

    public delegate void HoverMarkerAction(Character character, LocationGridTile location);
    public HoverMarkerAction hoverEnterAction;

    public System.Action hoverExitAction;

    public Character character { get; private set; }
    public LocationGridTile location { get; private set; }

    [SerializeField] private RectTransform visualsRT;
    [SerializeField] private Image mainImg;
    [SerializeField] private Image hoveredImg;
    [SerializeField] private Image clickedImg;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private Image actionIcon;

    [SerializeField] private StringSpriteDictionary actionIconDictionary;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private LocationGridTile lastRemovedTileFromPath;

    private List<LocationGridTile> _currentPath;
    private Action _arrivalAction;

    private int _estimatedTravelTime;
    private int _currentTravelTime;
    private bool _isMovementEstimated;
    private Action onArrivedAtTileAction;

    private LocationGridTile _destinationTile;
    private IPointOfInterest _targetPOI;
    private bool shouldRecalculatePath = false;

    private Coroutine currentMoveCoroutine;

    public bool isStillMovingToAnotherTile { get; private set; }

    #region getters/setters
    public List<LocationGridTile> currentPath {
        get { return _currentPath; }
    }
    #endregion

    public void SetCharacter(Character character) {
        this.name = character.name + "'s Marker";
        nameLbl.SetText(character.name);
        this.character = character;
        if (UIManager.Instance.characterInfoUI.isShowing) {
            clickedImg.gameObject.SetActive(UIManager.Instance.characterInfoUI.activeCharacter.id == character.id);
        }
        MarkerAsset assets = CharacterManager.Instance.GetMarkerAsset(character.race, character.gender);

        mainImg.sprite = assets.defaultSprite;
        animator.runtimeAnimatorController = assets.animator;
        //PlayIdle();

        Vector3 randomRotation = new Vector3(0f, 0f, 90f);
        randomRotation.z *= UnityEngine.Random.Range(1f, 4f);
        visualsRT.localRotation = Quaternion.Euler(randomRotation);
        UpdateActionIcon();

        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }
    public void SetLocation(LocationGridTile location) {
        this.location = location;
    }
    public void SetHoverAction(HoverMarkerAction hoverEnterAction, System.Action hoverExitAction) {
        this.hoverEnterAction = hoverEnterAction;
        this.hoverExitAction = hoverExitAction;
    }

    public void HoverAction() {
        if (hoverEnterAction != null) {
            hoverEnterAction.Invoke(character, location);
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

    public override void Reset() {
        base.Reset();
        StopMovement();
        character = null;
        location = null;
        hoverEnterAction = null;
        hoverExitAction = null;
        _destinationTile = null;
        Messenger.RemoveListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.RemoveListener<Character, GoapAction>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }

    public void OnPointerClick(BaseEventData bd) {
        UIManager.Instance.ShowCharacterInfo(character);
    }

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
        }
    }

    private void UpdateActionIcon() {
        if (character.currentAction != null && character.currentAction.actionIconString != GoapActionStateDB.No_Icon) {
            actionIcon.sprite = actionIconDictionary[character.currentAction.actionIconString];
            actionIcon.gameObject.SetActive(true);
        } else {
            actionIcon.gameObject.SetActive(false);
        }
    }

    private void OnCharacterDoingAction(Character character, GoapAction action) {
        if (this.character == character) {
            UpdateActionIcon();
        }
    }
    private void OnCharacterFinishedAction(Character character, GoapAction action, string result) {
        if (this.character == character) {
            UpdateActionIcon();
        }
    }

    #region Pathfinding Movement
    public void GoToTile(LocationGridTile destinationTile, IPointOfInterest targetPOI, Action arrivalAction = null) {
        if (isStillMovingToAnotherTile) {
            SetOnArriveAtTileAction(() => GoToTile(destinationTile, targetPOI, arrivalAction));
            return;
        }
        _destinationTile = destinationTile;
        _targetPOI = targetPOI;
        _arrivalAction = arrivalAction;
        lastRemovedTileFromPath = null;
        if (destinationTile.occupant != null) {
            //NOTE: Sometimes character's can still target tiles that are occupied for some reason, even though the logic for getting the target tile excludes occupied tiles. Need to investigate more, but for now this is the fix
            //throw new Exception(character.name + " is going to an occupied tile!");
            //destinationTile = character.currentAction.GetTargetLocationTile();
            shouldRecalculatePath = true;
        }
        if (character.gridTileLocation.structure.location.areaMap.gameObject.activeSelf) {
            //If area map is showing, do pathfinding
            _isMovementEstimated = false;
            _currentPath = PathGenerator.Instance.GetPath(character.gridTileLocation, destinationTile, GRID_PATHFINDING_MODE.REALISTIC);
            if (_currentPath != null) {
                Messenger.AddListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
                Debug.Log("Created path for " + character.name + " from " + character.gridTileLocation.ToString() + " to " + destinationTile.ToString());
                character.currentAction.UpdateTargetTile(destinationTile);
                StartMovement();
            } else {
                Debug.LogError("Can't create path for " + character.name + " from " + character.gridTileLocation.ToString() + " to " + destinationTile.ToString());
            }
        } else {
            //If area map is not showing, do estimated travel
            _estimatedTravelTime = Mathf.RoundToInt(character.gridTileLocation.GetDistanceTo(destinationTile));
            if(_estimatedTravelTime > 0) {
                StartEstimatedMovement();
            } else {
                Debug.LogError("Estimated travel time is zero");
            }
        }
    }
    private void StartMovement() {
        character.currentParty.icon.SetIsTravelling(true);
        StartWalkingAnimation();
        if (_currentPath.Count == 0) {
            //Arrival
            character.currentParty.icon.SetIsTravelling(false);
            PlayIdle();
            if (Messenger.eventTable.ContainsKey(Signals.TILE_OCCUPIED)) {
                Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
            }
            if (_arrivalAction != null) {
                _arrivalAction();
            }
            //throw new Exception(character.name + "'s marker path count is 0, but movement is starting! Destination Tile is: " + _destinationTile.ToString());
        } else {
            currentMoveCoroutine = StartCoroutine(MoveToPosition(transform.position, _currentPath[0].centeredWorldLocation));
        }
        //Messenger.AddListener(Signals.TICK_STARTED, Move);
    }
    public void StopMovement() {
        if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
            Messenger.RemoveListener(Signals.TICK_STARTED, EstimatedMove);
        }
        if (Messenger.eventTable.ContainsKey(Signals.TILE_OCCUPIED)) {
            Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
        }
        if (character.currentParty != null && character.currentParty.icon != null) {
            character.currentParty.icon.SetIsTravelling(false);
            character.currentParty.icon.SetIsPlaceCharacterAsTileObject(true);
        }
        _arrivalAction = null;
        //_currentPath = null;
        if (!isStillMovingToAnotherTile) {
            PlayIdle();
        }
    }
    private void Move() {
        if (character.isDead) {
            StopMovement();
            return;
        }
        string recalculationSummary = string.Empty;
        //check if the marker should recalculate path
        if (shouldRecalculatePath) {
            try {
                bool result = RecalculatePath(ref recalculationSummary);
                if (result) return;
            } catch (Exception e) {
                throw new Exception(e.Message + "\nThere was a problem trying to recalculate path of " + this.character.name + "'s Marker. Recalculation Summary: \n" + recalculationSummary);
            }
        }

        //if the current path is not empty
        if (_currentPath != null && _currentPath.Count > 0) {
            LocationGridTile currentTile = _currentPath[0];
            //bool currentIsTravelling = character.currentParty.icon.isTravelling;
            if(_currentPath.Count == 1) {
                //If the path only has 1 node left, this means that this is the destination tile, set the boolean to true so that when this character is placed
                //the algorithm will place the character as the object of the destination tile instead of being added in the moving characters list and the tile will be set as occupied
                character.currentParty.icon.SetIsPlaceCharacterAsTileObject(true);
            }
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

            if(onArrivedAtTileAction != null) {
                PlayIdle();
                onArrivedAtTileAction();
                onArrivedAtTileAction = null;
                return;
            }

            if (character.currentParty.icon.isTravelling) {
                if (_currentPath.Count <= 0) {
                    //Arrival
                    character.currentParty.icon.SetIsTravelling(false);
                    PlayIdle();
                    if (Messenger.eventTable.ContainsKey(Signals.TILE_OCCUPIED)) {
                        Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
                    }
                    if (_arrivalAction != null) {
                        _arrivalAction();
                    }
                } else {
                    if(_currentPath.Count == 1) {
                        Messenger.Broadcast(Signals.TILE_OCCUPIED, _currentPath[0], character as IPointOfInterest);
                    }
                    currentMoveCoroutine = StartCoroutine(MoveToPosition(transform.position, _currentPath[0].centeredWorldLocation));
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
                transform.position = Vector3.Lerp(from, to, t);
            }
            yield return null;
        }
        isStillMovingToAnotherTile = false;
        Move();
    }
    public void RotateMarker(Vector3 from, Vector3 to) {
        float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
        visualsRT.eulerAngles = new Vector3(visualsRT.rotation.x, visualsRT.rotation.y, angle);
    }
    public void SwitchToPathfinding() {
        if (!_isMovementEstimated) {
            return;
        }
        _isMovementEstimated = false;
        if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
            Messenger.RemoveListener(Signals.TICK_STARTED, EstimatedMove);
        }
        _currentPath = PathGenerator.Instance.GetPath(character.gridTileLocation, _destinationTile, GRID_PATHFINDING_MODE.REALISTIC);
        if (_currentPath != null) {
            int currentProgress = Mathf.RoundToInt((_currentTravelTime / (float) _estimatedTravelTime) * _currentPath.Count);
            if(currentProgress > 0) {
                _currentPath.RemoveRange(0, currentProgress);
                Move();
                if (_currentPath.Count > 1) {
                    StartWalkingAnimation();
                }
            } else {
                StartMovement();
            }
        } else {
            Debug.LogError("Can't create path for " + character.name + " from " + character.gridTileLocation.ToString() + " to " + _destinationTile.ToString());
        }
    }
    #endregion

    #region Estimated Movement
    public void SwitchToEstimatedMovement() {
        if (_isMovementEstimated) {
            return;
        }
        _isMovementEstimated = true;
        _estimatedTravelTime = _currentPath.Count;
        _currentTravelTime = 0;
        StartWalkingAnimation();
        Messenger.AddListener(Signals.TICK_STARTED, EstimatedMove);
        //if (_estimatedTravelTime > 0) {
            
        //} else {
        //    Debug.LogError(character.name + " can't switch to estimated movement because travel time is zero");
        //}
    }
    private void StartEstimatedMovement() {
        _isMovementEstimated = true;
        character.currentParty.icon.SetIsTravelling(true);
        _currentTravelTime = 0;
        StartWalkingAnimation();
        Messenger.AddListener(Signals.TICK_STARTED, EstimatedMove);
    }
    private void EstimatedMove() {
        if (character.isDead) {
            StopMovement();
            return;
        }
        if (_currentTravelTime >= _estimatedTravelTime) {
            //Arrival
            character.currentStructure.RemoveCharacterAtLocation(character);
            _destinationTile.structure.AddCharacterAtLocation(character, _destinationTile);
            character.currentParty.icon.SetIsTravelling(false);
            Action preservedArrivalAction = _arrivalAction;
            StopMovement();
            if (preservedArrivalAction != null) {
                preservedArrivalAction();
            }
        }
        _currentTravelTime++;
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
    #endregion

    #region Animation
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
    #endregion

    /// <summary>
    /// Called when this marker needs to recalculate its path, usually because its current target tile is already occupied.
    /// </summary>
    /// <returns>Returns true if the character found another valid target tile.</returns>
    private bool RecalculatePath(ref string pathRecalSummary) {
        bool recalculationResult = false;
        pathRecalSummary = GameManager.Instance.TodayLogString() + this.character.name + "'s marker must recalculate path towards " + _targetPOI.name + "!";
        LocationGridTile nearestTileToTarget = character.currentAction.GetTargetLocationTile();
        if (Messenger.eventTable.ContainsKey(Signals.TILE_OCCUPIED)) {
            Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
        }
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
            LocationGridTile nextTile = _currentPath[0];
            if (nextTile.isOccupied) {
                pathRecalSummary += "\nNext Tile " + nextTile.ToString() + " is occupied. Stopping movement and action.";
                character.currentAction.FailAction();
                if (currentMoveCoroutine != null) {
                    StopCoroutine(currentMoveCoroutine);
                }
                LocationGridTile tileToUse;
                if (lastRemovedTileFromPath != null) {
                    tileToUse = lastRemovedTileFromPath;
                } else {
                    tileToUse = nextTile;
                }
                if (tileToUse.structure != character.currentStructure) {
                    character.currentStructure.RemoveCharacterAtLocation(character);
                    tileToUse.structure.AddCharacterAtLocation(character, tileToUse);
                } else {
                    character.gridTileLocation.structure.location.areaMap.RemoveCharacter(character.gridTileLocation, character);
                    tileToUse.structure.location.areaMap.PlaceObject(character, tileToUse);
                }
                //_currentPath = null;
                recalculationResult = false;
            }
        }
        Debug.Log(pathRecalSummary);
        return recalculationResult;
    }

    /// <summary>
    /// Listener for when a grid tile has been occupied.
    /// </summary>
    /// <param name="currTile">The tile that was occupied.</param>
    /// <param name="poi">The object that occupied the tile.</param>
    private void OnTileOccupied(LocationGridTile currTile, IPointOfInterest poi) {
        if (_destinationTile != null && currTile == _destinationTile && poi != this.character) {
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
                        } catch (Exception e){
                            throw new Exception(e.Message + "\nThere was a problem trying to recalculate path of " + this.character.name + "'s Marker. Recalculation Summary: \n" + recalculationSummary);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
    public void SetOnArriveAtTileAction(Action action) {
        onArrivedAtTileAction = action;
    }
}
