using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour {

    public static CursorManager Instance = null;
    public bool isDraggingItem = false;

    [SerializeField] private Texture2D defaultCursorTexture;
    [SerializeField] private Texture2D targetCursorTexture;
    [SerializeField] private Texture2D dragWorldCursorTexture;
    [SerializeField] private Texture2D dragItemHoverCursorTexture;
    [SerializeField] private Texture2D dragItemClickedCursorTexture;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    private List<System.Action> leftClickActions = new List<System.Action>();

    #region Monobehaviours
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            SetCursorToDefault();
        } else {
            Destroy(this.gameObject);
        }
    }
    private void LateUpdate() {
        if (Input.GetMouseButtonDown(0)) {
            //left click
            ExecuteLeftClickActions();
            ClearLeftClickActions();
        }
    }
    #endregion


    public void SetCursorToDefault() {
        isDraggingItem = false;
        Cursor.SetCursor(defaultCursorTexture, hotSpot, cursorMode);
    }
    public void SetCursorToTarget() {
        Cursor.SetCursor(targetCursorTexture, hotSpot, cursorMode);
    }
    public void SetCursorToDrag() {
        Cursor.SetCursor(dragWorldCursorTexture, new Vector2(16f, 16f), cursorMode);
    }
    public void SetCursorToItemDragHover() {
        isDraggingItem = false;
        Cursor.SetCursor(dragItemHoverCursorTexture, hotSpot, cursorMode);
    }
    public void SetCursorToItemDragClicked() {
        isDraggingItem = true;
        Cursor.SetCursor(dragItemClickedCursorTexture, hotSpot, cursorMode);
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
