using UnityEngine;
using System.Collections;

public class MinimapCamera : MonoBehaviour {

    Vector2 newCameraPosition;
    Vector2 viewportPosition;
    float mainCameraHeight;

    public bool isDragging;

    [SerializeField] private Camera minimapTextureCamera;
    [SerializeField] private LayerMask raycastLayerMask;

    private void OnClick() {
        
        //Vector2 mousePosInMinimapCam = minimapTextureCamera.ScreenToViewportPoint(Input.mousePosition);
        
        //Vector2 convertedToMainCam = Camera.main.ViewportToWorldPoint(mousePosInMinimapCam);
        //Debug.Log(convertedToMainCam.x + "," + convertedToMainCam.y);
        //Camera.main.transform.position = mousePosInMinimapCam;

        //Rect tempRect = GetComponent<UITexture>().uvRect;
        //tempRect.y = Screen.height - tempRect.y;

        //Vector3 miniPos = Input.mousePosition;
        //Vector2 lerp = new Vector2((miniPos.x - tempRect.x) / tempRect.width, (miniPos.y - tempRect.y) / tempRect.height);

        //Vector4 camMinMax; //x = min x, y = z min, z = x max, w = z max

        //Vector2 camPos = new Vector2(Mathf.Lerp(CameraMove.Instance.MIN_X, CameraMove.Instance.MAX_X, lerp.x),
        //    Mathf.Lerp(CameraMove.Instance.MIN_Y, CameraMove.Instance.MAX_Y, lerp.y));
        //Camera.main.transform.position = camPos;
        //RaycastHit hit;
        //if (!Physics.Raycast(uiCamera.ScreenPointToRay(Input.mousePosition), out hit, raycastLayerMask)) {
        //    return;
        //}
        //Debug.Log("Hit!: " + hit.collider.gameObject.name);
        //Renderer rend = hit.transform.GetComponent<Renderer>();
        //MeshCollider meshCollider = hit.collider as MeshCollider;

        //Rect rect = hit.collider.gameObject.GetComponent<UITexture>().uvRect;
    }

    //void Update() {
        //viewportPosition = GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);

        //Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * -10, Color.red);
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
    //}

}
