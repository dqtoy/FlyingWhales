using UnityEngine;
using System.Collections;

[System.Serializable]
public class CityWar {

	public City city;
	public bool isActive;
	public WAR_TYPE warType;

	public CityWar(City city, bool isActive, WAR_TYPE warType){
		this.city = city;
		this.isActive = isActive;
		this.warType = warType;
	}
}
