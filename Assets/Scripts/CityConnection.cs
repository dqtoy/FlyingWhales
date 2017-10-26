using UnityEngine;
using System.Collections;

public class CityConnection : MonoBehaviour {

	public UI2DSprite left;
	public UI2DSprite center;
	public UI2DSprite right;
	public UI2DSprite colorizer;

	private HexTile _fromTile;
	private HexTile _toTile;

	public HexTile fromTile{
		get { return this._fromTile; }
	}
	public HexTile toTile{
		get { return this._toTile; }
	}

	public void SetConnection(HexTile fromTile, HexTile toTile){
		this._fromTile = fromTile;
		this._toTile = toTile;
		float distance = Vector3.Distance (fromTile.transform.position, toTile.transform.position);
		SetLength ((int)distance);
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
