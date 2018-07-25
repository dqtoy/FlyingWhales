using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PanelDragBehaviour : MonoBehaviour {
    private RectTransform rect;
    [SerializeField] private RectTransform canvasRectTransform;

    public void Awake() {
        rect = GetComponent<RectTransform>();
    }

    public void OnDrag(UnityEngine.EventSystems.BaseEventData eventData) {
        var pointerData = eventData as UnityEngine.EventSystems.PointerEventData;
        //if (pointerData == null) { return; }

        //var currentPosition = rect.position;
        //currentPosition.x += pointerData.delta.x;
        //currentPosition.y += pointerData.delta.y;
        //rect.position = currentPosition;

        //Vector2 pos;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, pointerData.position, Camera.main, out pos);

        transform.position = Input.mousePosition;
        //Debug.Log("Is Inside Canvas?: " + Utilities.IsUIElementInsideScreen(this.transform as RectTransform, worldcreator.WorldCreatorUI.Instance.canvas).ToString());
    }
}