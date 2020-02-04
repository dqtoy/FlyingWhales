using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class CombatLogItem : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI logLbl;
    [SerializeField] private Image logBGImg;
    [SerializeField] private EnvelopContentUnityUI envelopContent;

    public void SetLog(string text, SIDES side) {
        this.logLbl.text = text;
        if(side == SIDES.B) {
            Color temp = logBGImg.color;
            temp.a = 200f / 255f;
            logBGImg.color = temp;
        }
        envelopContent.Execute();
    }
}
