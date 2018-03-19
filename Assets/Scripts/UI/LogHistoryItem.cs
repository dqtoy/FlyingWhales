using UnityEngine;
using System.Collections;

public class LogHistoryItem : MonoBehaviour {

    [SerializeField] private UILabel logLbl;
    [SerializeField] private UILabel dateLbl;
    [SerializeField] private UI2DSprite logBG;

    private Log _thisLog;

    #region getters/setters
    public Log thisLog {
        get { return _thisLog; }
    }
    #endregion

    public void SetLog(Log log) {
        this.name = log.id.ToString();
        _thisLog = log;
        dateLbl.text = Utilities.NormalizeString(log.month.ToString()) + " " + log.day + ", " + log.year;
        if (_thisLog.fillers.Count > 0) {
            this.logLbl.text = Utilities.LogReplacer(_thisLog);
        } else {
            this.logLbl.text = LocalizationManager.Instance.GetLocalizedValue(_thisLog.category, _thisLog.file, _thisLog.key);
        }
    }

    public void SetLogColor(Color color) {
        logBG.color = color;
    }
}
