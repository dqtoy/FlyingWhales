using UnityEngine;
using System.Collections;

public struct CityWarPair {
	public City sourceCity;
	public City targetCity;

	public CityWarPair(City sourceCity, City targetCity){
		this.sourceCity = sourceCity;
		this.targetCity = targetCity;
	}
}
