using UnityEngine;
using System.Collections;

public class RoadManager : MonoBehaviour {
	public static RoadManager Instance = null;

	public Transform majorRoadParent;
	public Transform minorRoadParent;

	public GameObject majorRoadGO;
	public GameObject minorRoadGO;

	public int maxConnections;
	public int maxCityConnections;
	public int maxLandmarkConnections;

	// Use this for initialization
	void Awake () {
		Instance = this;
	}


	internal void DrawConnection(HexTile fromTile, HexTile toTile, ROAD_TYPE roadType){
		Debug.Log ("DRAW CONNECTION: " + fromTile.name + ", " + toTile.name);
		Vector3 fromPos = fromTile.gameObject.transform.position;
		Vector3 toPos = toTile.gameObject.transform.position;
		Vector3 targetDir = toPos - fromPos;

		//float angle = Vector3.Angle (targetDir, fromTile.transform.forward);
		float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
		Debug.Log ("ANGLE: " + angle);

		GameObject connectionGO = majorRoadGO;
		if(roadType == ROAD_TYPE.MINOR){
			connectionGO = minorRoadGO;
		}
		GameObject goConnection = (GameObject)GameObject.Instantiate (connectionGO);
		goConnection.transform.position = fromPos;
		goConnection.transform.Rotate(new Vector3(0f,0f,angle));
		if (roadType == ROAD_TYPE.MAJOR) {
			goConnection.transform.parent = majorRoadParent;
		}else if (roadType == ROAD_TYPE.MINOR) {
			goConnection.transform.parent = minorRoadParent;
		}

		RoadConnection roadConnection = goConnection.GetComponent<RoadConnection> ();
		roadConnection.SetConnection (fromTile, toTile, roadType);

		fromTile.connectedTiles.Add (toTile, roadConnection);
		toTile.connectedTiles.Add (fromTile, roadConnection);
		//		if(fromTile.city != null && toTile.city != null){
		//			if(fromTile.city.kingdom.id == toTile.city.kingdom.id){
		//				goConnection.GetComponent<CityConnection> ().SetColor (fromTile.city.kingdom.kingdomColor);
		//			}
		//		}
	}
	internal void DestroyConnection(HexTile fromTile, HexTile toTile){
		RoadConnection roadConnection = fromTile.connectedTiles [toTile];
		GameObject.Destroy (roadConnection.gameObject);
		fromTile.connectedTiles.Remove (toTile);
		toTile.connectedTiles.Remove (fromTile);
	}

	internal bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){

		Vector3 lineVec3 = linePoint2 - linePoint1;
		Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

		float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

		//is coplanar, and not parrallel
		if(Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
		{
			float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
			intersection = linePoint1 + (lineVec1 * s);
			return true;
		}
		else
		{
			intersection = Vector3.zero;
			return false;
		}
	}

	internal Vector3 GetIntersection(Vector3 firstPoint1, Vector3 firstPoint2, Vector3 secondPoint1, Vector3 secondPoint2){
		float A1 = firstPoint2.y - firstPoint1.y;
		float B1 = firstPoint1.x - firstPoint2.x;
		float C1 = A1 * firstPoint1.x + B1 * firstPoint1.y;

		float A2 = secondPoint2.y - secondPoint1.y;
		float B2 = secondPoint1.x - secondPoint2.x;
		float C2 = A2 * secondPoint1.x + B2 * secondPoint1.y;

		float det = A1 * B2 - A2 * B1;

		float x = (B2 * C1 - B1 * C2) / det;
		float y = (A1 * C2 - A2 * C1) / det;

		return new Vector3 (x, y, 0f);
	}

	internal bool IsIntersectingWith(HexTile fromTile, HexTile toTile, ROAD_TYPE roadType){
		Vector3 fromPos = fromTile.gameObject.transform.position;
		Vector3 toPos = toTile.gameObject.transform.position;
		Vector3 targetDir = toPos - fromPos;
//		Ray2D ray = new Ray2D (fromPos, targetDir);
		float distance = Vector3.Distance (fromTile.transform.position, toTile.transform.position);
		RaycastHit2D hit = Physics2D.Raycast (fromPos, targetDir, distance);
		if(hit){
//			Debug.LogError ("HIT: " + hit.collider.name);
			string tagName = "All";
			if(roadType == ROAD_TYPE.MAJOR){
				tagName = "MajorRoad";
			}else if(roadType == ROAD_TYPE.MINOR){
				tagName = "MinorRoad";
			}
//			if(hit.collider.tag == "Hextile" && hit.collider.name == toTile.name){
//				return false;
//			}
			if(tagName == "All"){
				if(hit.collider.tag == "MajorRoad" || hit.collider.tag == "MinorRoad"){
					RoadConnection roadConnection = hit.collider.transform.parent.gameObject.GetComponent<RoadConnection>();
//					Vector3 intersectionPoint = Vector3.zero;
					Vector3 intersectionPoint = RoadManager.Instance.GetIntersection (fromPos, toPos, roadConnection.fromTile.transform.position, roadConnection.toTile.transform.position);
					Debug.LogError (fromTile.name + " and " + toTile.name + " - " + roadConnection.fromTile.name + " and " + roadConnection.toTile.name + " HAS INTERSECTED AT POINT: " + intersectionPoint.ToString ());

					return true;
				}
			}else{
				if(hit.collider.tag == tagName){
					RoadConnection roadConnection = hit.collider.transform.parent.gameObject.GetComponent<RoadConnection>();
					Vector3 intersectionPoint = RoadManager.Instance.GetIntersection (fromPos, toPos, roadConnection.fromTile.transform.position, roadConnection.toTile.transform.position);
					Debug.LogError (fromTile.name + " and " + toTile.name + " - " + roadConnection.fromTile.name + " and " + roadConnection.toTile.name + " HAS INTERSECTED AT POINT: " + intersectionPoint.ToString ());
					return true;
				}
			}
		}
		return false;
	}
}
