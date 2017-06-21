using UnityEngine;
using System.Collections;

public class CityItem : MonoBehaviour {

    private City _city;

    [SerializeField] private CharacterPortrait _governor;
    [SerializeField] private UILabel _hpLbl;
    [SerializeField] private UILabel _structuresLbl;
    [SerializeField] private UILabel _cityLbl;
    [SerializeField] private UIProgressBar _hpProgBar;
    [SerializeField] private GameObject _loyaltyGO;
    [SerializeField] private UILabel _loyaltyLbl;
    [SerializeField] private UIEventTrigger _loyaltyEventTrigger;
	[SerializeField] private GameObject _rebelIcon;

    #region getters/setters
    public City city {
        get { return this._city; }
    }
	public GameObject rebelIcon {
		get { return this._rebelIcon; }
	}
    #endregion

    public void SetCity(City _city, bool showLoyalty = false) {
        this._city = _city;
        this._governor.SetCitizen(city.governor);
        this._hpLbl.text = city.hp.ToString();
        this._structuresLbl.text = city.ownedTiles.Count.ToString();
        this._cityLbl.text = city.name;
        this._hpProgBar.value = (float)city.hp / (float)city.maxHP;

        if (showLoyalty) {
            this._loyaltyGO.SetActive(true);
            this._loyaltyLbl.text = ((Governor)this._governor.citizen.assignedRole).loyalty.ToString();
            EventDelegate.Set(_loyaltyEventTrigger.onHoverOver, delegate () { UIManager.Instance.ShowLoyaltySummary((Governor)this._governor.citizen.assignedRole); });
            EventDelegate.Set(_loyaltyEventTrigger.onHoverOut, delegate () { UIManager.Instance.HideLoyaltySummary(); });

        }
    }

    public void CenterOnCity() {
        CameraMove.Instance.CenterCameraOn(this._city.hexTile.gameObject);
    }

    public void SetKingdomAsSelected() {
        UIManager.Instance.SetKingdomAsSelected(this._city.kingdom);
    }
}
