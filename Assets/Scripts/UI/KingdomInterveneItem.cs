using UnityEngine;
using System.Collections;

public class KingdomInterveneItem : MonoBehaviour {

    [SerializeField] private UILabel kingdomNameLbl;
    [SerializeField] private UILabel kingdomInfoLbl;
    [SerializeField] private UI2DSprite kingdomFlagSprite;

    private Kingdom kingdom;

    public void SetKingdom(Kingdom kingdom) {
        this.kingdom = kingdom;
        kingdomNameLbl.text = kingdom.name;
        kingdomFlagSprite.color = kingdom.kingdomColor;

        string[] kingdomTypeWords = kingdom.kingdomType.ToString().Split('_');
        string kingdomType = string.Empty;
        for (int i = 0; i < kingdomTypeWords.Length; i++) {
            kingdomType += Utilities.FirstLetterToUpperCase(kingdomTypeWords[i].ToLower()) + " ";
        }
        kingdomInfoLbl.text = kingdomType + "\n";
        kingdomInfoLbl.text += kingdom.race.ToString() + "\n";
        if (kingdom.age > 1) {
            kingdomInfoLbl.text += kingdom.age.ToString() + " years old\n";
        } else if (kingdom.age == 1){
            kingdomInfoLbl.text += kingdom.age.ToString() + " year old\n";
        } else {
            kingdomInfoLbl.text += "Less than a year old\n";
        }

        if (kingdom.cities.Count > 1) {
            kingdomInfoLbl.text += kingdom.cities.Count + " cities";
        } else {
            kingdomInfoLbl.text += kingdom.cities.Count + " city";
        }
    }

    public void SwitchToKingdom() {
        UIManager.Instance.HideInterveneActionsMenu();
        UIManager.Instance.SetKingdomAsActive(kingdom);
        CameraMove.Instance.CenterCameraOn(kingdom.capitalCity.hexTile.gameObject);
    }
}
