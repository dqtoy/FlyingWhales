using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class CharacterMarker : PooledObject {

    public delegate void HoverMarkerAction(Character character, LocationGridTile location);
    public HoverMarkerAction hoverEnterAction;

    public System.Action hoverExitAction;

    public Character character { get; private set; }
    public LocationGridTile location { get; private set; }

    [SerializeField] private RectTransform rt;
    [SerializeField] private Image mainImg;
    [SerializeField] private Image hoveredImg;
    [SerializeField] private Image clickedImg;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private List<LocationGridTile> _currentPath;
    private Action _arrivalAction;

    private int _estimatedTravelTime;
    private int _currentTravelTime;

    private LocationGridTile _destinationTile;
    private IPointOfInterest _targetPOI;
    private bool shouldRecalculatePath = false;

    #region getters/setters
    public List<LocationGridTile> currentPath {
        get { return _currentPath; }
    }
    #endregion

    public void SetCharacter(Character character) {
        this.character = character;
        if (UIManager.Instance.characterInfoUI.isShowing) {
            clickedImg.gameObject.SetActive(UIManager.Instance.characterInfoUI.activeCharacter.id == character.id);
        }
        MarkerAsset assets = CharacterManager.Instance.GetMarkerAsset(character.race, character.gender);

        mainImg.sprite = assets.defaultSprite;
        //clickedImg.sprite = assets.clickedSprite;
        //hoveredImg.sprite = assets.hoverSprite;
        animator.runtimeAnimatorController = assets.animator;
        //PlayIdle();

        Vector3 randomRotation = new Vector3(0f, 0f, 90f);
        randomRotation.z *= UnityEngine.Random.Range(1f, 4f);
        rt.localRotation = Quaternion.Euler(randomRotation);

        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
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

    #region Pathfinding Movement
    public void GoToTile(LocationGridTile destinationTile, IPointOfInterest targetPOI, Action arrivalAction = null) {
        _destinationTile = destinationTile;
        _targetPOI = targetPOI;
        _arrivalAction = arrivalAction;
        Messenger.AddListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
        if (character.gridTileLocation.structure.location.areaMap.gameObject.activeSelf) {
            //If area map is showing, do pathfinding
            _currentPath = PathGenerator.Instance.GetPath(character.gridTileLocation, destinationTile, GRID_PATHFINDING_MODE.REALISTIC);
            if (_currentPath != null) {
                Debug.LogWarning("Created path for " + character.name + " from " + character.gridTileLocation.ToString() + " to " + destinationTile.ToString());
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
        StartCoroutine(MoveToPosition(character.gridTileLocation.centeredWorldLocation, _currentPath[0].centeredWorldLocation));
        //Messenger.AddListener(Signals.TICK_STARTED, Move);
    }
    public void StopMovement() {
        if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
            Messenger.RemoveListener(Signals.TICK_STARTED, EstimatedMove);
        }
        if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
            Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
        }
        if (character.currentParty != null && character.currentParty.icon != null) {
            character.currentParty.icon.SetIsTravelling(false);
        }
        _arrivalAction = null;
        //_currentPath = null;
        PlayIdle();
    }
    private void Move() {
        if (character.isDead) {
            StopMovement();
            return;
        }
        //check if the marker should recalculate path
        if (shouldRecalculatePath) {
            Debug.Log(GameManager.Instance.TodayLogString() + this.character.name + "'s marker must recalculate path towards " + _targetPOI.name + "!");
            LocationGridTile nearestTileToTarget = _targetPOI.GetNearestUnoccupiedTileFromThis();
            shouldRecalculatePath = false;
            character.gridTileLocation.structure.location.areaMap.RemoveCharacter(character.gridTileLocation, character);
            _currentPath[0].structure.AddCharacterAtLocation(character, _currentPath[0]);
            character.SetGridTileLocation(_currentPath[0]);
            Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
            if (nearestTileToTarget == null) {
                //Cancel current action and recalculate plan
                character.currentAction.StopAction();
                //character.RecalculatePlan(character.GetPlanWithAction(character.currentAction));
                //character.SetCurrentAction(null);
                //character.StartDailyGoapPlanGeneration();
                _currentPath = null;
                return;
            } else {
                if (nearestTileToTarget != character.gridTileLocation) {
                    GoToTile(nearestTileToTarget, _targetPOI, _arrivalAction);
                    return;
                }
            }
        }
        if (_currentPath.Count > 0) {
            LocationGridTile currentTile = _currentPath[0];
            bool currentIsTravelling = character.currentParty.icon.isTravelling;
            if(_currentPath.Count == 1) {
                character.currentParty.icon.SetIsTravelling(false); //Quick fix for movement issue
            }
            if (currentTile.structure != character.currentStructure) {
                character.currentStructure.RemoveCharacterAtLocation(character);
                currentTile.structure.AddCharacterAtLocation(character, currentTile);
            } else {
                character.gridTileLocation.structure.location.areaMap.RemoveCharacter(character.gridTileLocation, character);
                currentTile.structure.location.areaMap.PlaceObject(character, currentTile);
            }
            _currentPath.RemoveAt(0);
            character.currentParty.icon.SetIsTravelling(currentIsTravelling);

            if (character.currentParty.icon.isTravelling) {
                if (_currentPath.Count <= 0) {
                    //Arrival
                    character.currentParty.icon.SetIsTravelling(false);
                    if (_arrivalAction != null) {
                        _arrivalAction();
                    }
                    PlayIdle();
                    Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, OnTileOccupied);
                } else {
                    StartCoroutine(MoveToPosition(currentTile.centeredWorldLocation, _currentPath[0].centeredWorldLocation));
                }
            }
        }
    }
    private IEnumerator MoveToPosition(Vector3 from, Vector3 to) {
        RotateMarker(from, to);

        float t = 0f;
        while (t < 1) {
            if (!GameManager.Instance.isPaused) {
                t += Time.deltaTime / GameManager.Instance.progressionSpeed;
                transform.position = Vector3.Lerp(from, to, t);
            }
            yield return null;
        }
        Move();
    }
    public void RotateMarker(Vector3 from, Vector3 to) {
        float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.rotation.x, gameObject.transform.rotation.y, angle);
    }
    public void SwitchToPathfinding() {
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
    private void StartEstimatedMovement() {
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
            if (_arrivalAction != null) {
                _arrivalAction();
            }
            StopMovement();
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
        animator.Play("Walk");
    }
    private void PlayIdle() {
        if (!this.gameObject.activeInHierarchy) {
            return;
        }
        animator.Play("Idle");
    }
    #endregion

    private void OnTileOccupied(LocationGridTile currTile, IPointOfInterest poi) {
        if (_destinationTile != null && currTile == _destinationTile && poi != this.character) {
            shouldRecalculatePath = true;
        }
    }
}
