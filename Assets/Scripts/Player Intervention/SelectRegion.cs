using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectRegion : MonoBehaviour {

	public static SelectRegion Instance = null;

	[SerializeField] private RectTransform minimapTransform;
	[SerializeField] private RawImage minimapImage;
	[SerializeField] private Camera minimapCamera;
	[SerializeField] private Camera wholeMapCamera;


	private void Awake() {
		Instance = this;
	}

	public void OnClickRegion(){
//		Rect minimapRect = minimapTransform.rect;
//		Rect screenRect = new Rect (minimapRect.x, minimapRect.y, minimapRect.width, minimapRect.height);
//		Vector3 mousePos = Input.mousePosition;
//		float mPosY = mousePos.y - screenRect.y;
//		float mPosX = mousePos.x - screenRect.x;
//		float xPos = mPosX * (GridMap.Instance.mapWidth / screenRect.width);
//		float yPos = mPosY * (GridMap.Instance.mapHeight / screenRect.height);
//		Vector3 hitPos = new Vector3 (xPos, yPos, 0f);
		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapTransform, Input.mousePosition, minimapCamera, out localPoint);
		Vector3 viewPos = wholeMapCamera.GetComponent<Camera> ().WorldToViewportPoint (new Vector3 (localPoint.x, localPoint.y, 0f));
		Ray ray = wholeMapCamera.GetComponent<Camera>().ViewportPointToRay(viewPos);
//		Ray ray = new Ray (new Vector3 (localPoint.x, localPoint.y, 0f), Vector3.forward);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit)){
			Debug.Log (hit.transform.parent.name);
		}
		Debug.Log ("POSITION: " + localPoint.ToString ());
//		Debug.Log ("Screen POSITION: " + screenPoint.ToString ());

	}
	private Vector2 GetLocalCursorPoint(PointerEventData ped) {
		Vector2 localCursor;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapTransform, ped.position, ped.pressEventCamera, out localCursor))
			return Vector2.zero;
		return localCursor;
	}
}
