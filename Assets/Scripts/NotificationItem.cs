using UnityEngine;
using System.Collections;
using EZObjectPools;

public class NotificationItem : PooledObject {

    [SerializeField] private UILabel notifLabel;
    [SerializeField] private UILabel notifDateLabel;
    [SerializeField] private int baseNotificationExpiration;

    [SerializeField] private int timeLeftBeforeExpiration;

    private Log _thisLog;

    #region getters/setters
    public Log thisLog {
        get { return this._thisLog; }
    }
    #endregion

    public void SetLog(Log log, bool shouldExpire = true) {
        GetComponent<UI2DSprite>().MarkAsChanged();
        this.name = log.id.ToString();
        this._thisLog = log;
        if (_thisLog.fillers.Count > 0) {
            this.notifLabel.text = Utilities.LogReplacer(_thisLog);
        } else {
            this.notifLabel.text = LocalizationManager.Instance.GetLocalizedValue(_thisLog.category, _thisLog.file, _thisLog.key);
        }
        notifDateLabel.text = log.month.ToString() + " " + log.day.ToString() + ", " + log.year.ToString();
        timeLeftBeforeExpiration = baseNotificationExpiration;
        if (shouldExpire) {
            InvokeRepeating("CheckForExpiry", 1f, 1f);
        }
    }

    private void CheckForExpiry() {
        if (!GameManager.Instance.isPaused) {
            if(timeLeftBeforeExpiration <= 0) {
                gameObject.SetActive(false);
                CancelInvoke("CheckForExpiry");
                timeLeftBeforeExpiration = baseNotificationExpiration;
                //ObjectPoolManager.Instance.DestroyObject(gameObject);
                UIManager.Instance.RepositionNotificationTable();
                UIManager.Instance.AddNotificationItemToReuseList(this);
                return;
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
