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
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image mainImg;
    [SerializeField] private Image clickedImg;

    [Header("Male")]
    [SerializeField] private Sprite defaultMaleSprite;
    [SerializeField] private Sprite hoveredMaleSprite;
    [SerializeField] private Sprite clickedMaleSprite;

    [Header("Female")]
    [SerializeField] private Sprite defaultFemaleSprite;
    [SerializeField] private Sprite hoveredFemaleSprite;
    [SerializeField] private Sprite clickedFemaleSprite;

    private List<LocationGridTile> _currentPath;
    private Action _arrivalAction;

    private int _estimatedTravelTime;
    private int _currentTravelTime;

    private LocationGridTile _destinationTile;

    public void SetCharacter(Character character) {
        this.character = character;
        if (UIManager.Instance.characterInfoUI.isShowing) {
            toggle.isOn = UIManager.Instance.characterInfoUI.activeCharacter.id == character.id;
        }
        SpriteState ss = new SpriteState();
        switch (character.gender) {
            case GENDER.MALE:
                mainImg.sprite = defaultMaleSprite;
                clickedImg.sprite = clickedMaleSprite;
                ss.highlightedSprite = hoveredMaleSprite;
                ss.pressedSprite = clickedMaleSprite;
                break;
            case GENDER.FEMALE:
                mainImg.sprite = defaultFemaleSprite;
                clickedImg.sprite = clickedFemaleSprite;
                ss.highlightedSprite = hoveredFemaleSprite;
                ss.pressedSprite = clickedFemaleSprite;
                break;
            default:
                mainImg.sprite = defaultMaleSprite;
                clickedImg.sprite = clickedMaleSprite;
                ss.highlightedSprite = hoveredMaleSprite;
                ss.pressedSprite = clickedMaleSprite;
                break;
        }
        toggle.spriteState = ss;

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
    }
    public void HoverExitAction() {
        if (hoverExitAction != null) {
            hoverExitAction();
        }
    }

    public override void Reset() {
        base.Reset();
        character = null;
        location = null;
        hoverEnterAction = null;
        hoverExitAction = null;
        _destinationTile = null;
        StopMovement();
        Messenger.RemoveListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
    }

    public void OnPointerClick(bool state) {
        if (state) {
            UIManager.Instance.ShowCharacterInfo(character);
        }
    }

    private void OnMenuOpened(UIMenu menu) {
        if (menu is CharacterInfoUI) {
            if ((menu as CharacterInfoUI).activeCharacter.id == character.id) {
                toggle.isOn = true;
            } else {
                toggle.isOn = false;
            }
             
        }
    }
    private void OnMenuClosed(UIMenu menu) {
        if (menu is CharacterInfoUI) {
            toggle.isOn = false;
        }
    }

    #region Pathfinding Movement
    public void GoToTile(LocationGridTile destinationTile, Action arrivalAction = null) {
        _destinationTile = destinationTile;
        if (character.gridTileLocation.structure.location.areaMap.gameObject.activeSelf) {
            //If area map is showing, do pathfinding
            _currentPath = PathGenerator.Instance.GetPath(character.gridTileLocation, destinationTile, GRID_PATHFINDING_MODE.REALISTIC);
            if (_currentPath != null) {
                StartMovement();
            } else {
                Debug.LogError("Can't create path!");
            }
        } else {
            //If area map is not showing, do estimated travel
            _estimatedTravelTime = Mathf.RoundToInt(Vector2.Distance(character.gridTileLocation.localLocation, destinationTile.localLocation));
            if(_estimatedTravelTime > 0) {
                StartEstimatedMovement();
            } else {
                Debug.LogError("Estimated travel time is zero");
            }
        }
    }
    private void StartMovement() {
        character.currentParty.icon.SetIsTravelling(true);
        StartCoroutine(MoveToPosition(character.gridTileLocation.centeredWorldLocation, _currentPath[0].centeredWorldLocation));
        //Messenger.AddListener(Signals.TICK_STARTED, Move);
    }
    public void StopMovement() {
        if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
            Messenger.RemoveListener(Signals.TICK_STARTED, EstimatedMove);
        }
        character.currentParty.icon.SetIsTravelling(false);
        _arrivalAction = null;
        _currentPath = null;
    }
    private void Move() {
        if (character.isDead) {
            StopMovement();
            return;
        }
        if(_currentPath.Count > 0 && character.currentParty.icon.isTravelling) {
            LocationGridTile currentTile = _currentPath[0];
            if(currentTile.structure != character.currentStructure) {
                character.currentStructure.RemoveCharacterAtLocation(character);
                currentTile.structure.AddCharacterAtLocation(character, currentTile);
            } else {
                character.gridTileLocation.structure.location.areaMap.RemoveObject(character.gridTileLocation);
                currentTile.structure.location.areaMap.PlaceObject(character, currentTile);
            }
            _currentPath.RemoveAt(0);

            if(_currentPath.Count <= 0) {
                //Arrival
                character.currentParty.icon.SetIsTravelling(false);
                if (_arrivalAction != null) {
                    _arrivalAction();
                }
            } else {
                StartCoroutine(MoveToPosition(currentTile.centeredWorldLocation, _currentPath[0].centeredWorldLocation));
            }
        }
    }
    private IEnumerator MoveToPosition(Vector3 from, Vector3 to) {
        float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.rotation.x, gameObject.transform.rotation.y, angle);

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
            } else {
                StartMovement();
            }
        } else {
            Debug.LogError("Can't create path!");
        }
    }
    #endregion

    #region Estimated Movement
    private void StartEstimatedMovement() {
        character.currentParty.icon.SetIsTravelling(true);
        _currentTravelTime = 0;
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
}
