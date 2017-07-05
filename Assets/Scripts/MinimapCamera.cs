using UnityEngine;
using System.Collections;

public class MinimapCamera : MonoBehaviour {

    Vector2 newCameraPosition;
    Vector2 viewportPosition;
    float mainCameraHeight;

	void Update () {
        if (Input.GetMouseButton(0)) {
            viewportPosition = GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);
            Ray ray = GetComponent<Camera>().ViewportPointToRay(viewportPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                Debug.Log(hit.transform.name);
                newCameraPosition = hit.point;
                mainCameraHeight = Camera.main.transform.position.z;
                //CameraMove.Instance.CenterCameraOn(hit.transform.gameObject);
                Camera.main.transform.position = new Vector3(newCameraPosition.x, newCameraPosition.y, mainCameraHeight);
            }
        }
    }
}
