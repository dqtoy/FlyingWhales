using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PanelDragBehaviour : MonoBehaviour, IDragHandler, IBeginDragHandler {
    private RectTransform rect;
    [SerializeField] private RectTransform canvasRectTransform;

    private bool allowDrag;

    public void Awake() {
        rect = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        allowDrag = false;
        if (raycastResults.Count > 0) {
            foreach (var go in raycastResults) {
                if (go.gameObject.GetComponent<PanelDragBehaviour>() != null) {
                    //Debug.Log(go.gameObject.name, go.gameObject);
                    allowDrag = true;
                    break;
                }

            }
        }
    }

    //public void OnDrag(UnityEngine.EventSystems.BaseEventData eventData) {
    //    var pointerData = eventData as UnityEngine.EventSystems.PointerEventData;
    //    //if (pointerData == null) { return; }

    //    //var currentPosition = rect.position;
    //    //currentPosition.x += pointerData.delta.x;
    //    //currentPosition.y += pointerData.delta.y;
    //    //rect.position = currentPosition;
    //    Debug.Log("Draggingggg");
    //    //Vector2 pos;
    //    //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, pointerData.position, Camera.main, out pos);

    //    transform.position = Input.mousePosition;
    //    //Debug.Log("Is Inside Canvas?: " + Utilities.IsUIElementInsideScreen(this.transform as RectTransform, worldcreator.WorldCreatorUI.Instance.canvas).ToString());
    //}

    public void OnDrag(PointerEventData eventData) {
        if (allowDrag) {
            transform.position = Input.mousePosition;
        }
    }
}