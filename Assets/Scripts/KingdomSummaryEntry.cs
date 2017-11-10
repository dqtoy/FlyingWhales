using UnityEngine;
using System.Collections;

public class KingdomSummaryEntry : MonoBehaviour {

    private Kingdom _kingdom;

    [SerializeField] private UI2DSprite bgSprite;

    [SerializeField] private UI2DSprite _emblemBG;
    [SerializeField] private UI2DSprite _emblemSprite;
    [SerializeField] private UI2DSprite _emblemOutline;

    [SerializeField] private UILabel kingdomNameLbl;
    [SerializeField] private UILabel populationLbl;
    [SerializeField] private UILabel citiesLbl;
    [SerializeField] private UILabel expansionRateLbl;
    [SerializeField] private UILabel weaponsLbl;
    [SerializeField] private UILabel armorLbl;

    [SerializeField] private UIEventTrigger weaponLblEventTrigger;
    [SerializeField] private UIEventTrigger armorLblEventTrigger;
    [SerializeField] private UIEventTrigger emblemEventTrigger;

    public void SetKingdom(Kingdom kingdom) {
        _kingdom = kingdom;
        ConstructEmblem();
        bgSprite.color = kingdom.kingdomColor;
        //kingdomNameLbl.text = kingdom.name;
        populationLbl.text = kingdom.population.ToString();
        citiesLbl.text = kingdom.cities.Count.ToString();
        expansionRateLbl.text = kingdom.currentExpansionRate.ToString();
        weaponsLbl.text = kingdom.effectiveAttack.ToString();
        //        armorLbl.text = kingdom.effectiveDefense.ToString();


        //EventDelegate.Set(nameLblEventTrigger.onClick, delegate () { SwitchKingdom(kingdom); });

        EventDelegate.Set(emblemEventTrigger.onClick, delegate () { SwitchKingdom(kingdom); });
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

    private void ConstructEmblem() {
        Color emblemShieldColor = _kingdom.kingdomColor;
        emblemShieldColor.a = 255f / 255f;
        _emblemBG.sprite2D = _kingdom.emblemBG;
        _emblemBG.color = emblemShieldColor;
        _emblemSprite.sprite2D = _kingdom.emblem;
        //_emblemSprite.MakePixelPerfect();
        _emblemOutline.sprite2D = _kingdom.emblemBG;
        _emblemOutline.width = _emblemBG.width;
        Color outlineColor;
        ColorUtility.TryParseHtmlString("#2d2e2e", out outlineColor);
        _emblemOutline.color = outlineColor;
        _emblemOutline.width += 8;

        _emblemSprite.width = 17;
        _emblemSprite.height = 19;
    }
}
