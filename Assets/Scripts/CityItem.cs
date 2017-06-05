using UnityEngine;
using System.Collections;

public class CityItem : MonoBehaviour {

    private City _city;

    [SerializeField] private CharacterPortrait _governor;
    [SerializeField] private UILabel _hpLbl;
    [SerializeField] private UILabel _structuresLbl;
    [SerializeField] private UILabel _cityLbl;
    [SerializeField] private UIProgressBar _hpProgBar;

    #region getters/setters
    public City city {
        get { return this._city; }
    }
    #endregion

    public void SetCity(City _city) {
        this._city = _city;
        this._governor.SetCitizen(city.governor);
        this._hpLbl.text = city.hp.ToString();
        this._structuresLbl.text = (city.structures.Count + 1).ToString(); // +1 because the structures list does not contain the main tile
        this._cityLbl.text = city.name;
        this._hpProgBar.value = (float)city.hp / (float)city.maxHP;
    }

    public void CenterOnCity() {
        CameraMove.Instance.CenterCameraOn(this._city.hexTile.gameObject);
    }
}
