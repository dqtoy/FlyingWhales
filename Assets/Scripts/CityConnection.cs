using UnityEngine;
using System.Collections;

public class CityConnection : MonoBehaviour {

	public UI2DSprite left;
	public UI2DSprite center;
	public UI2DSprite right;
	public UI2DSprite colorizer;

	public void SetLength(int length){
		left.height = length;
		center.height = length;
		right.height = length;
	}

	public void SetColor(Color color){
		this.colorizer.color = color;
	}
}
