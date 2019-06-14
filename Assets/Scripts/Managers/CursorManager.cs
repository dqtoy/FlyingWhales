using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public enum Cursor_Type {
        None, Default, Target, Drag_Hover, Drag_Clicked, Check, Cross
    }
    public Cursor_Type currentCursorType { get; private set; }


    #region Monobehaviours
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            SetCursorTo(Cursor_Type.Default);
        } else {
            Destroy(this.gameObject);
        }
    }
    private void Update() {
        if (PlayerManager.Instance != null && PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActivePlayerJobAction != null) {
            IPointOfInterest hoveredPOI = InteriorMapManager.Instance.currentlyHoveredPOI;
            if (hoveredPOI != null) {
                if (PlayerManager.Instance.player.currentActivePlayerJobAction.CanTarget(hoveredPOI)) {
                    SetCursorTo(Cursor_Type.Check);
                } else {
                    SetCursorTo(Cursor_Type.Cross);
                }
            } else {
                SetCursorTo(Cursor_Type.Cross);
            }
        }
        if (Input.GetMouseButtonDown(0)) {
            //left click
            ExecuteLeftClickActions();
            ClearLeftClickActions();
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
    private void ClearLeftClickActions() {
        leftClickActions.Clear();
    }
    #endregion

}
