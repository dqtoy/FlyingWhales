using UnityEngine;
using System.Collections;

public class CityItem : MonoBehaviour {

    private City _city;

    [SerializeField] private GameObject governorParentGO;
 //   [SerializeField] private GameObject powerGO;
	//[SerializeField] private GameObject defenseGO;
	//[SerializeField] private GameObject hpParentGO;
    [SerializeField] private GameObject cityNameParentGO;
    [SerializeField] private GameObject structuresParentGO;
    [SerializeField] private GameObject growthMeterParentGO;

    [SerializeField] private CharacterPortrait _governor;
 //   [SerializeField] private UILabel _hpLbl;
	//[SerializeField] private UILabel _powerLbl;
	//[SerializeField] private UILabel _defenseLbl;
    [SerializeField] private UILabel _structuresLbl;
    [SerializeField] private UILabel _cityLbl;
    [SerializeField] private UIProgressBar _hpProgBar;

    [SerializeField] private GameObject _loyaltyGO;
    [SerializeField] private UILabel _loyaltyLbl;
    [SerializeField] private UIEventTrigger _loyaltyEventTrigger;
	[SerializeField] private GameObject _rebelIcon;
    [SerializeField] private UIProgressBar _growthProgBar;

	[SerializeField] private GameObject _noFoodGO;
	[SerializeField] private GameObject _noMaterialGO;
	[SerializeField] private GameObject _noOreGO;


    [Header("For Testing")]
    [SerializeField] private GameObject forTestingGO;
    //[SerializeField] private UILabel newPowerLbl;
    //[SerializeField] private UILabel newDefLabel;
    [SerializeField] private UILabel loyaltyAdjustmentLbl;

    #region getters/setters
    public City city {
        get { return this._city; }
    }
	public GameObject rebelIcon {
		get { return this._rebelIcon; }
	}
    #endregion

    public void SetCity(City _city, bool showLoyalty = false, bool showNameOnly = false, bool showForTesting = false, bool forNamePlate = false) {
        this._city = _city;
		_structuresLbl.text = city.ownedTiles.Count.ToString();
		_cityLbl.text = city.name;

        if (!forNamePlate) {
            _governor.SetCitizen(city.governor);
		    //this._powerLbl.text = city.weapons.ToString();
		    //this._defenseLbl.text = city.armor.ToString();

    //        _hpLbl.text = city.hp.ToString();
            _structuresLbl.text = city.ownedTiles.Count.ToString();
            _cityLbl.text = city.name;
    //		float hpValue = (float)city.hp / (float)city.maxHP;
    //		if(hpValue > 1f){
    //			hpValue = 1f;
    //		}
    //        _hpProgBar.value = hpValue;
            _growthProgBar.value = (float)city.currentGrowth / (float)city.maxGrowth;

            if (showLoyalty) {
                _loyaltyGO.SetActive(true);
                _loyaltyLbl.text = _governor.citizen.loyaltyToKing.ToString();
                EventDelegate.Set(_loyaltyEventTrigger.onHoverOver, delegate () {
                    ShowLoyaltySummary();
                });
                EventDelegate.Set(_loyaltyEventTrigger.onHoverOut, delegate () { UIManager.Instance.HideRelationshipSummary(); });
            }

            if (showNameOnly) {
                governorParentGO.SetActive(false);
    //            hpParentGO.SetActive(false);
			    //powerGO.SetActive(false);
			    //defenseGO.SetActive(false);
                cityNameParentGO.SetActive(true);
                structuresParentGO.SetActive(false);
                growthMeterParentGO.SetActive(false);
            } else {
                governorParentGO.SetActive(true);
    //            hpParentGO.SetActive(true);
			    //powerGO.SetActive(true);
			    //defenseGO.SetActive(true);
                cityNameParentGO.SetActive(true);
                structuresParentGO.SetActive(true);
                growthMeterParentGO.SetActive(true);
            }

            if (showForTesting) {
                forTestingGO.SetActive(true);
                //newPowerLbl.text = _city.weapons.ToString();
                //newDefLabel.text = _city.armor.ToString();
                loyaltyAdjustmentLbl.text = ((Governor)_city.governor.assignedRole).forTestingLoyaltyModifier.ToString();
            } else {
                forTestingGO.SetActive(false);
            }
        
        }

    }
	public void UpdateFoodMaterialOreUI(){
		if(_city.foodCount <= 0){
			_noFoodGO.SetActive (true);
		}else{
			_noFoodGO.SetActive (false);
		}
		if(_city.materialCount <= 0){
			_noMaterialGO.SetActive (true);
		}else{
			_noMaterialGO.SetActive (false);
		}
		if(_city.oreCount <= 0){
			_noOreGO.SetActive (true);
		}else{
			_noOreGO.SetActive (false);
		}
	}
    public void CenterOnCity() {
        CameraMove.Instance.CenterCameraOn(_city.hexTile.gameObject);
    }
		

    public void SetKingdomAsSelected() {
        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id != _city.kingdom.id) {
            UIManager.Instance.SetKingdomAsActive(_city.kingdom);
        }
        //UIManager.Instance.SetKingdomAsSelected(_city.kingdom);
    }

    private void ShowLoyaltySummary() {
        Citizen thisCitizen = _governor.citizen;
        string loyaltySummary = string.Empty;
        if(thisCitizen.loyaltyDeductionFromWar != 0) {
            loyaltySummary += thisCitizen.loyaltyDeductionFromWar + "   Active Wars\n";
        }
        loyaltySummary += thisCitizen.loyaltySummary;

        int loyaltyFromStability = thisCitizen.GetLoyaltyFromStability();
        if(loyaltyFromStability != 0) {
            if (loyaltyFromStability > 0) {
                loyaltySummary += "+";
            }
            loyaltySummary += loyaltyFromStability + "   Stability\n";
        }

        if (thisCitizen.loyaltyModifierForTesting != 0) {
            if(thisCitizen.loyaltyModifierForTesting > 0) {
                loyaltySummary += "+";
            }
            loyaltySummary += thisCitizen.loyaltyModifierForTesting + "   Admin Modifier\n";
        }
        UIManager.Instance.ShowRelationshipSummary(thisCitizen, loyaltySummary);
    }

	public void ShowCityHistory(){
		UIManager.Instance.ShowCityHistory (this._city);
	}

    #region For Testing
    //public void SetPower() {
    //    _city.SetWeapons(System.Int32.Parse(newPowerLbl.text));
    //    _city.hexTile.UpdateCityNamePlate();
    //    this._powerLbl.text = city.weapons.ToString();
    //}
    //public void SetDefense() {
    //    _city.SetArmor(System.Int32.Parse(newDefLabel.text));
    //    _city.hexTile.UpdateCityNamePlate();
    //    this._defenseLbl.text = city.armor.ToString();
    //}
    public void SetGovernorLoyaltyAdjustment() {
        _city.governor.loyaltyModifierForTesting = System.Int32.Parse(loyaltyAdjustmentLbl.text);
        SetCity(_city, true, false, true);
    }
    #endregion

}
