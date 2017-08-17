using UnityEngine;
using System.Collections;
using EZObjectPools;

public class NotificationItem : PooledObject {

    [SerializeField] private UILabel notifLabel;
    [SerializeField] private int baseNotificationExpiration;

    private int timeLeftBeforeExpiration;

    private Log _thisLog;

    #region getters/setters
    public Log thisLog {
        get { return this._thisLog; }
    }
    #endregion

    public void SetLog(Log log) {
        this._thisLog = log;
        if (_thisLog.fillers.Count > 0) {
            this.notifLabel.text = Utilities.LogReplacer(_thisLog);
        } else {
            this.notifLabel.text = LocalizationManager.Instance.GetLocalizedValue(_thisLog.category, _thisLog.file, _thisLog.key);
        }
        timeLeftBeforeExpiration = baseNotificationExpiration;
        InvokeRepeating("CheckForExpiry", 1f, 1f);
    }

    private void CheckForExpiry() {
        if (!GameManager.Instance.isPaused) {
            if(timeLeftBeforeExpiration <= 0) {
                ObjectPoolManager.Instance.DestroyObject(gameObject);
                UIManager.Instance.RepositionNotificationTable();
            }
            timeLeftBeforeExpiration -= 1;
        }
    }

    #region overrides
    public override void Reset() {
        base.Reset();
        CancelInvoke("CheckForExpiry");
        timeLeftBeforeExpiration = baseNotificationExpiration;
    }
    #endregion
}
