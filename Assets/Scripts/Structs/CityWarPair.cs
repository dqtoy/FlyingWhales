using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct CityWarPair {
	public City kingdom1City;
	public City kingdom2City;
	public List<HexTile> path;

	public CityWarPair(City kingdom1City, City kingdom2City, List<HexTile> path){
		this.kingdom1City = kingdom1City;
		this.kingdom2City = kingdom2City;
		this.path = new List<HexTile>(path);
	}

	internal void DefaultValues(){
		this.kingdom1City = null;
		this.kingdom2City = null;
		this.path = null;
	}
}
