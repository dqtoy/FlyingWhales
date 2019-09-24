using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorManager : MonoBehaviour {

    public static CursorManager Instance = null;
    public bool isDraggingItem = false;

    private CursorMode cursorMode = CursorMode.ForceSoftware;

    private List<System.Action> leftClickActions = new List<System.Action>();
    private List<System.Action> pendingLeftClickActions = new List<System.Action>();
    private List<System.Action> rightClickActions = new List<System.Action>();

    [SerializeField] private CursorTextureDictionary cursors;

    [Header("Cursor Effects")]
    [SerializeField] private GameObject effectsParent;
    [SerializeField] private Camera effectsCamera;
    public GameObject electricEffect;
    [SerializeField] private GameObject tileObjectSparkle;

    public enum Cursor_Type {
        None, Default, Target, Drag_Hover, Drag_Clicked, Check, Cross
    }
    public Cursor_Type currentCursorType;
    public Cursor_Type previousCursorType;

    public PointerEventData cursorPointerEventData { get; private set; }
    private LocationGridTile previousHoveredTile;

    #region Monobehaviours
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            SetCursorTo(Cursor_Type.Default);
            previousCursorType = Cursor_Type.Default;
            Cursor.lockState = CursorLockMode.Confined;
        } else {
            Destroy(this.gameObject);
        }
    }
    //private void Start() {
    //    cursorPointerEventData = new PointerEventData(EventSystem.current);
    //}
    private void Update() {
        if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
            if (PlayerManager.Instance.player.currentActivePlayerJobAction != null) {
                LocationGridTile hoveredTile = InteriorMapManager.Instance.GetTileFromMousePosition();
                if (previousHoveredTile != null && previousHoveredTile != hoveredTile) {
                    PlayerManager.Instance.player.currentActivePlayerJobAction.HideRange(previousHoveredTile);
                }
                bool canTarget = false;
                IPointOfInterest hoveredPOI = InteriorMapManager.Instance.currentlyHoveredPOI;
                string hoverText = string.Empty;
                for (int i = 0; i < PlayerManager.Instance.player.currentActivePlayerJobAction.targetTypes.Length; i++) {
                    switch (PlayerManager.Instance.player.currentActivePlayerJobAction.targetTypes[i]) {
                        case JOB_ACTION_TARGET.CHARACTER:
                        case JOB_ACTION_TARGET.TILE_OBJECT:
                            if (hoveredPOI != null) {
                                canTarget = PlayerManager.Instance.player.currentActivePlayerJobAction.CanTarget(hoveredPOI, ref hoverText);
                            }
                            break;
                        case JOB_ACTION_TARGET.TILE:
                            if (hoveredTile != null) {
                                canTarget = PlayerManager.Instance.player.currentActivePlayerJobAction.CanTarget(hoveredTile);
                            }
                            break;
                        default:
                            break;
                    }
                    if (canTarget) {
                        SetCursorTo(Cursor_Type.Check);
                        PlayerManager.Instance.player.currentActivePlayerJobAction.ShowRange(hoveredTile);
                        break;
                    } else {
                        SetCursorTo(Cursor_Type.Cross);
                        PlayerManager.Instance.player.currentActivePlayerJobAction.HideRange(hoveredTile);
                    }
                }
                previousHoveredTile = hoveredTile;
                if(hoveredPOI != null) {
                    if (hoverText != string.Empty) {
                        UIManager.Instance.ShowSmallInfo(hoverText);
                    }
                } else {
                    UIManager.Instance.HideSmallInfo();
                }
                //IPointOfInterest hoveredPOI = InteriorMapManager.Instance.currentlyHoveredPOI;
                //if (hoveredPOI != null) {
                //    if (PlayerManager.Instance.player.currentActivePlayerJobAction.CanTarget(hoveredPOI)) {
                //        SetCursorTo(Cursor_Type.Check);
                //    } else {
                //        SetCursorTo(Cursor_Type.Cross);
                //    }
                //}
                //else if (cursorPointerEventData.pointerEnter != null) {
                //    LandmarkCharacterItem charItem = cursorPointerEventData.pointerEnter.transform.parent.GetComponent<LandmarkCharacterItem>();
                //    if (charItem != null) {
                //        if (PlayerManager.Instance.player.currentActivePlayerJobAction.CanTarget(charItem.character)) {
                //            SetCursorTo(Cursor_Type.Check);
                //        } else {
                //            SetCursorTo(Cursor_Type.Cross);
                //        }
                //    } else {
                //        SetCursorTo(Cursor_Type.Cross);
                //    }
                //}
                //else {
                //    SetCursorTo(Cursor_Type.Cross);
                //}
            } else if (PlayerManager.Instance.player.currentActiveCombatAbility != null) {
                UIManager.Instance.HideSmallInfo();
                CombatAbility ability = PlayerManager.Instance.player.currentActiveCombatAbility;
                if (ability.abilityRadius == 0) {
                    IPointOfInterest hoveredPOI = InteriorMapManager.Instance.currentlyHoveredPOI;
                    if (hoveredPOI != null) {
                        if (ability.CanTarget(hoveredPOI)) {
                            SetCursorTo(Cursor_Type.Check);
                        } else {
                            SetCursorTo(Cursor_Type.Cross);
                        }
                    }
                } else {
                    LocationGridTile hoveredTile = InteriorMapManager.Instance.GetTileFromMousePosition();
                    if (hoveredTile != null) {
                        SetCursorTo(Cursor_Type.Check);
                        List<LocationGridTile> highlightTiles = hoveredTile.parentAreaMap.GetTilesInRadius(hoveredTile, ability.abilityRadius, includeCenterTile: true, includeTilesInDifferentStructure: true);
                        if (InteriorMapManager.Instance.currentlyHighlightedTiles != null) {
                            InteriorMapManager.Instance.UnhighlightTiles();
                            InteriorMapManager.Instance.HighlightTiles(highlightTiles);
                        } else {
                            InteriorMapManager.Instance.HighlightTiles(highlightTiles);
                        }
                    }
                }
            } else if (PlayerManager.Instance.player.currentActiveIntel != null) {
                IPointOfInterest hoveredPOI = InteriorMapManager.Instance.currentlyHoveredPOI;
                if (hoveredPOI != null) {
                    string hoverText = string.Empty;
                    if (PlayerManager.Instance.player.CanShareIntel(hoveredPOI, ref hoverText)) {
                        SetCursorTo(Cursor_Type.Check);
                    } else {
                        SetCursorTo(Cursor_Type.Cross);
                    }
                    if(hoverText != string.Empty) {
                        UIManager.Instance.ShowSmallInfo(hoverText);
                    }
                } else {
                    UIManager.Instance.HideSmallInfo();
                    SetCursorTo(Cursor_Type.Cross);
                }
            }
            //else {
            //    UIManager.Instance.HideSmallInfo();
            //}
        }
        if (Input.GetMouseButtonDown(0)) {
            //left click
            ExecuteLeftClickActions();
            ClearLeftClickActions();
            TransferPendingLeftClickActions();
            Messenger.Broadcast(Signals.KEY_DOWN, KeyCode.Mouse0);
        }else if (Input.GetMouseButtonDown(1)) {
            //right click
            ExecuteRightClickActions();
            ClearRightClickActions();
            Messenger.Broadcast(Signals.KEY_DOWN, KeyCode.Mouse1);
        }
        if (AreaMapCameraMove.Instance != null) {
            Vector3 pos = effectsCamera.ScreenToWorldPoint(Input.mousePosition);
            //pos.x += 0.05f;
            //pos.y -= 0.05f;
            pos.z = 0f;
            effectsParent.transform.position = pos;
        }
    }
    #endregion

    public void SetCursorTo(Cursor_Type type) {
        if (currentCursorType == type) {
            return; //ignore 
        }
        previousCursorType = currentCursorType;
        Vector2 hotSpot = Vector2.zero;
        switch (type) {
            case Cursor_Type.Drag_Clicked:
                isDraggingItem = true;
                break;
            //case Cursor_Type.Cross:
            //case Cursor_Type.Check:
            case Cursor_Type.Target:
                hotSpot = new Vector2(29f, 29f);
                break;
            default:
                isDraggingItem = false;
                break;
        }
        currentCursorType = type;
        //Debug.Log("Set cursor type to " + currentCursorType.ToString());
        Cursor.SetCursor(cursors[type], hotSpot, cursorMode);
    }
    public void RevertToPreviousCursor() {
        SetCursorTo(previousCursorType);
    }

    #region Click Actions
    private void TransferPendingLeftClickActions() {
        for (int i = 0; i < pendingLeftClickActions.Count; i++) {
            leftClickActions.Add(pendingLeftClickActions[i]);
        }
        pendingLeftClickActions.Clear();
    }
    public void AddLeftClickAction(System.Action action) {
        leftClickActions.Add(action);
    }
    public void AddPendingLeftClickAction(System.Action action) {
        pendingLeftClickActions.Add(action);
    }
    private void ExecuteLeftClickActions() {
        for (int i = 0; i < leftClickActions.Count; i++) {
            leftClickActions[i]();
        }
    }
    public void ClearLeftClickActions() {
        leftClickActions.Clear();
    }
    public void AddRightClickAction(System.Action action) {
        rightClickActions.Add(action);
    }
    private void ExecuteRightClickActions() {
        for (int i = 0; i < rightClickActions.Count; i++) {
            rightClickActions[i]();
        }
    }
    public void ClearRightClickActions() {
        rightClickActions.Clear();
    }
    #endregion

    #region Effects
    public void SetElectricEffectState(bool state) {
        electricEffect.gameObject.SetActive(state);
    }
    public void SetSparkleEffectState(bool state) {
        tileObjectSparkle.SetActive(state);
    }
    #endregion
}
