using UnityEngine;
using System.Collections;

public class MinimapCamera : MonoBehaviour {

    Vector2 newCameraPosition;
    Vector2 viewportPosition;
    float mainCameraHeight;

    public bool isDragging;

    [SerializeField] private Camera minimapCamera;
    [SerializeField] private LayerMask raycastLayerMask;

    private void Update() {
        if (Input.GetMouseButton(0)) {
            Vector3 mouseToViewPort = minimapCamera.ScreenToViewportPoint(Input.mousePosition);
            Debug.Log("Mouse in viewport point: " + mouseToViewPort.ToString());
            //if (minimapCamera.rect.Contains(mouseToViewPort)) {
            //    Debug.Log("Click is inside minimap camera!");
            //}
        }
        //Vector3 mousePosInCamera = GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);
        //Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);
        //if (Input.GetMouseButton(0)) {
        //    viewportPosition = GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);
        //Debug.Log(viewportPosition.x + "," + viewportPosition.y);
        //    Ray ray = GetComponent<Camera>().ViewportPointToRay(viewportPosition);
        //    RaycastHit hit;
        //    if (Physics.Raycast(ray, out hit)) {
        //        isDragging = true;
        //        Debug.Log(hit.transform.name);
        //        newCameraPosition = hit.point;
        //        mainCameraHeight = Camera.main.transform.position.z;
        //        //CameraMove.Instance.CenterCameraOn(hit.transform.gameObject);
        //        Camera.main.transform.position = new Vector3(newCameraPosition.x, newCameraPosition.y, mainCameraHeight);
        //        CameraMove.Instance.ConstrainCameraBounds();
        //    }
        //} else if (Input.GetMouseButtonUp(0)) {
        //    isDragging = false;
        //}
        //}
    }

}
