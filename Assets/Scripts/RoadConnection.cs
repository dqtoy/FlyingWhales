using UnityEngine;
using System.Collections;

public class RoadConnection : MonoBehaviour {

	public UI2DSprite left;
	public UI2DSprite center;
	public UI2DSprite right;
	public UI2DSprite colorizer;

	private HexTile _fromTile;
	private HexTile _toTile;
	private ROAD_TYPE _roadType;

	#region Getters/Setters
	public HexTile fromTile{
		get { return this._fromTile; }
	}
	public HexTile toTile{
		get { return this._toTile; }
	}
	public ROAD_TYPE roadType{
		get { return this._roadType; }
	}
	#endregion
	public void SetConnection(HexTile fromTile, HexTile toTile, ROAD_TYPE roadType){
		this._fromTile = fromTile;
		this._toTile = toTile;
		this._roadType = roadType;
		float distance = Vector3.Distance (fromTile.transform.position, toTile.transform.position);
		int intDistance = Mathf.RoundToInt (distance);
		SetLength (intDistance);
	}
	public void SetLength(int length){
		left.height = length;
		center.height = length;
		right.height = length;
	}

	public void SetColor(Color color){
		this.colorizer.color = color;
	}
}
