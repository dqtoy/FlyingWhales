using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectRegion : MonoBehaviour {

	public static SelectRegion Instance;

	[SerializeField] private RectTransform minimapTransform;
	[SerializeField] private Camera minimapCamera;
	[SerializeField] private Camera wholeMapCamera;

	internal HexTile selectedTile = null;

	private void Awake() {
		Instance = this;
	}

	public void OnClickRegion(){
		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapTransform, Input.mousePosition, minimapCamera, out localPoint);
		Vector3 viewPos = wholeMapCamera.GetComponent<Camera> ().WorldToViewportPoint (new Vector3 (localPoint.x, localPoint.y, 0f));
		Ray ray = wholeMapCamera.GetComponent<Camera>().ViewportPointToRay(viewPos);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit)){
			if(hit.transform.parent.GetComponent<HexTile> () != null){
				selectedTile = hit.transform.parent.GetComponent<HexTile> ();
				PlayerInterventionUIManager.Instance.btnOkSelectRegion.isEnabled = true;
			}else{
				PlayerInterventionUIManager.Instance.btnOkSelectRegion.isEnabled = false;
				selectedTile = null;
			}
		}
	}
}
