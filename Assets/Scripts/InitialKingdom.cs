using UnityEngine;
using System.Collections;

[System.Serializable]
public class InitialKingdom {


    [SerializeField] private RACE _race;
    [SerializeField] private int _numOfCities;

    #region getters/setters
    public RACE race {
        get { return this._race; }
    }
    public int numOfCities {
        get { return this._numOfCities; }
    }

    #endregion

    public InitialKingdom(RACE _race, int _numOfCities) {
        this._race = _race;
        this._numOfCities = _numOfCities;
    }
}
