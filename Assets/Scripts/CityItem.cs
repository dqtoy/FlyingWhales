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
    [SerializeField] private UIProgressBar _growthProgBar;

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
        _governor.SetCitizen(city.governor);
        _hpLbl.text = city.hp.ToString();
        _structuresLbl.text = city.ownedTiles.Count.ToString();
        _cityLbl.text = city.name;
		float hpValue = (float)city.hp / (float)city.maxHP;
		if(hpValue > 1f){
			hpValue = 1f;
		}
        _hpProgBar.value = hpValue;
        _growthProgBar.value = (float)city.currentGrowth / (float)city.maxGrowth;

        if (showLoyalty) {
            _loyaltyGO.SetActive(true);
            Governor thisGovernor = (Governor)_governor.citizen.assignedRole;
            _loyaltyLbl.text = thisGovernor.loyalty.ToString();
            EventDelegate.Set(_loyaltyEventTrigger.onHoverOver, delegate () {
                UIManager.Instance.ShowRelationshipSummary(thisGovernor.citizen, thisGovernor.loyaltySummary);
            });
            EventDelegate.Set(_loyaltyEventTrigger.onHoverOut, delegate () { UIManager.Instance.HideRelationshipSummary(); });

        }
    }

    public void CenterOnCity() {
        CameraMove.Instance.CenterCameraOn(_city.hexTile.gameObject);
    }

    public void SetKingdomAsSelected() {
        //UIManager.Instance.SetKingdomAsSelected(_city.kingdom);
    }
}
