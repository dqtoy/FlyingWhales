using UnityEngine;
using System.Collections;

public class RoadManager : MonoBehaviour {
	public static RoadManager Instance = null;

	public Transform majorRoadParent;
	public Transform minorRoadParent;

	public GameObject majorRoadGO;
	public GameObject minorRoadGO;

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

	internal bool IsIntersectingWith(HexTile fromTile, HexTile toTile, ROAD_TYPE roadType){
		Vector3 fromPos = fromTile.gameObject.transform.position;
		Vector3 toPos = toTile.gameObject.transform.position;
		Vector3 targetDir = toPos - fromPos;
//		Ray2D ray = new Ray2D (fromPos, targetDir);
		RaycastHit2D hit = Physics2D.Raycast (fromPos, targetDir);
		if(hit){
			string tagName = "All";
			if(roadType == ROAD_TYPE.MAJOR){
				tagName = "MajorRoad";
			}else if(roadType == ROAD_TYPE.MINOR){
				tagName = "MinorRoad";
			}
			if(hit.collider.tag == "Hextile" && hit.collider.name == toTile.name){
				return false;
			}
			if(tagName == "All"){
				if(hit.collider.tag == "MajorRoad" || hit.collider.tag == "MinorRoad"){
					return true;
				}
			}else{
				if(hit.collider.tag == tagName){
					return true;
				}
			}
		}
		return false;
	}
}
