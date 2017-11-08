using UnityEngine;
using System.Collections;

public class KingdomSummaryEntry : MonoBehaviour {

    private Kingdom _kingdom;

    [SerializeField] private UI2DSprite bgSprite;

    [SerializeField] private UILabel kingdomNameLbl;
    [SerializeField] private UILabel populationLbl;
    [SerializeField] private UILabel citiesLbl;
    [SerializeField] private UILabel expansionRateLbl;
    [SerializeField] private UILabel weaponsLbl;
    [SerializeField] private UILabel armorLbl;

    [SerializeField] private UIEventTrigger weaponLblEventTrigger;
    [SerializeField] private UIEventTrigger armorLblEventTrigger;

    private UIEventTrigger nameLblEventTrigger;

    private void Awake() {
        nameLblEventTrigger = kingdomNameLbl.gameObject.GetComponent<UIEventTrigger>();
    }

    public void SetKingdom(Kingdom kingdom) {
        _kingdom = kingdom;
        bgSprite.color = kingdom.kingdomColor;
        kingdomNameLbl.text = kingdom.name;
        populationLbl.text = kingdom.population.ToString();
        citiesLbl.text = kingdom.cities.Count.ToString();
        expansionRateLbl.text = kingdom.currentExpansionRate.ToString();
        weaponsLbl.text = kingdom.effectiveAttack.ToString();
//        armorLbl.text = kingdom.effectiveDefense.ToString();

        
        EventDelegate.Set(nameLblEventTrigger.onClick, delegate () { SwitchKingdom(kingdom); });

        EventDelegate.Set(weaponLblEventTrigger.onHoverOver, delegate () { OnHoverWeapons(); });
        EventDelegate.Set(weaponLblEventTrigger.onHoverOut, delegate () { UIManager.Instance.HideSmallInfo(); });

//        EventDelegate.Set(armorLblEventTrigger.onHoverOver, delegate () { OnHoverArmor(); });
//        EventDelegate.Set(armorLblEventTrigger.onHoverOut, delegate () { UIManager.Instance.HideSmallInfo(); });
    }

    private void OnHoverWeapons() {
        string textToDisplay = "Total Weapons:[b] " + _kingdom.baseWeapons + "[/b]"
                                + "\nSoldiers:[b] " + _kingdom.soldiers.ToString() + "[/b]";
        UIManager.Instance.ShowSmallInfo(textToDisplay);
    }
//    private void OnHoverArmor() {
//		string textToDisplay = "Total Weapons:[b] " + _kingdom.baseWeapons + "[/b]" 
//                                + "\nSoldiers:[b] " + _kingdom.soldiers.ToString() + "[/b]";
//        UIManager.Instance.ShowSmallInfo(textToDisplay);
//    }

    private void SwitchKingdom(Kingdom kingdom) {
        UIManager.Instance.SetKingdomAsActive(_kingdom);
        CameraMove.Instance.CenterCameraOn(kingdom.capitalCity.hexTile.gameObject);
    }
}
