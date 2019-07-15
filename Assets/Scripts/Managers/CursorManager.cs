using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorManager : MonoBehaviour {

    public static CursorManager Instance = null;
    public bool isDraggingItem = false;

//#if UNITY_EDITOR
//    private CursorMode cursorMode = CursorMode.Auto;
//#else
    private CursorMode cursorMode = CursorMode.ForceSoftware;
//#endif

    private List<System.Action> leftClickActions = new List<System.Action>();

    [SerializeField] private CursorTextureDictionary cursors;

    [Header("Cursor Effects")]
    [SerializeField] private GameObject effectsParent;
    [SerializeField] private Camera effectsCamera;
    public GameObject electricEffect;
    [SerializeField] private GameObject tileObjectSparkle;

    public enum Cursor_Type {
        None, Default, Target, Drag_Hover, Drag_Clicked, Check, Cross
    }
    public Cursor_Type currentCursorType { get; private set; }

    public PointerEventData cursorPointerEventData { get; private set; }

    #region Monobehaviours
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            SetCursorTo(Cursor_Type.Default);
            Cursor.lockState = CursorLockMode.Confined;
        } else {
            Destroy(this.gameObject);
        }
    }
    //private void Start() {
    //    cursorPointerEventData = new PointerEventData(EventSystem.current);
    //}
    private void Update() {
        if (PlayerManager.Instance != null && PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActivePlayerJobAction != null) {
            IPointOfInterest hoveredPOI = InteriorMapManager.Instance.currentlyHoveredPOI;
            if (hoveredPOI != null) {
                if (PlayerManager.Instance.player.currentActivePlayerJobAction.CanTarget(hoveredPOI)) {
                    SetCursorTo(Cursor_Type.Check);
                } else {
                    SetCursorTo(Cursor_Type.Cross);
                }
            } 
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
            else {
                SetCursorTo(Cursor_Type.Cross);
            }
        }
        if (Input.GetMouseButtonDown(0)) {
            //left click
            ExecuteLeftClickActions();
            ClearLeftClickActions();
            Messenger.Broadcast(Signals.KEY_DOWN, KeyCode.Mouse0);
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
        Vector2 hotSpot = Vector2.zero;
        switch (type) {
            case Cursor_Type.Drag_Clicked:
                isDraggingItem = true;
                break;
            //case Cursor_Type.Target:
            //case Cursor_Type.Cross:
            //case Cursor_Type.Check:
            //    hotSpot = new Vector2(29f, 29f);
            //    break;
            default:
                isDraggingItem = false;
                break;
        }
        currentCursorType = type;
        Cursor.SetCursor(cursors[type], hotSpot, cursorMode);
    }

    #region Click Actions
    public void AddLeftClickAction(System.Action action) {
        leftClickActions.Add(action);
    }
    private void ExecuteLeftClickActions() {
        for (int i = 0; i < leftClickActions.Count; i++) {
            leftClickActions[i]();
        }
    }
    public void ClearLeftClickActions() {
        leftClickActions.Clear();
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
