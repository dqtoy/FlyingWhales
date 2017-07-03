using UnityEngine;
using System.Collections;

[System.Serializable]
public class InitialKingdom {


    [SerializeField] private RACE _race;
    [SerializeField] private int _numOfCities;
	[SerializeField] private BIOMES _startingBiome;

    #region getters/setters
    public RACE race {
        get { return this._race; }
    }
    public int numOfCities {
        get { return this._numOfCities; }
    }
	public BIOMES startingBiome {
		get { return this._startingBiome; }
	}

    #endregion

	public InitialKingdom(RACE _race, int _numOfCities, BIOMES _startingBiome) {
        this._race = _race;
        this._numOfCities = _numOfCities;
		this._startingBiome = _startingBiome;
    }
}
