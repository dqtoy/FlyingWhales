using UnityEngine;
using System.Collections;

public class LogHistoryItem : MonoBehaviour {

    [SerializeField] private Color evenColor;
    [SerializeField] private Color oddColor;
    [SerializeField] private UI2DSprite bg;

    [SerializeField] private UILabel dateLbl;
    [SerializeField] private UILabel logLbl;

    public void SetLog(Log log, int index) {
        dateLbl.text = log.month.ToString() + " " + log.day.ToString() + ", " + log.year.ToString();
        if (log.fillers.Count > 0) {
            logLbl.text = Utilities.LogReplacer(log);
        } else {
            logLbl.text = LocalizationManager.Instance.GetLocalizedValue(log.category, log.file, log.key);
        }

        if(index % 2 == 0) {
            bg.color = evenColor;
        } else {
            bg.color = oddColor;
        }
    }
}
