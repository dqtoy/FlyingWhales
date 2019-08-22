using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;

public class TimerHubItem : PooledObject {

    public TextMeshProUGUI timeText;
    public TextMeshProUGUI descriptionText;

    public int durationInTicks { get; private set; }
    public System.Action onClickAction { get; private set; }

    public void Initialize(string description, int durationInTicks, System.Action onClickAction) {
        this.durationInTicks = durationInTicks;
        descriptionText.text = description;
        this.onClickAction = onClickAction;
        UpdateTime();
    }

    public void UpdateTime() {
        timeText.text = GameManager.Instance.GetHoursBasedOnTicks(durationInTicks).ToString();
    }

    //Returns true if duration is done, else, returns false
    public bool PerTick() {
        durationInTicks--;
        if(durationInTicks <= 0) {
            durationInTicks = 0;
            UpdateTime();
            return true;
        }
        UpdateTime();
        return false;
    }

    public void OnClickThis() {
        if(onClickAction != null) {
            onClickAction.Invoke();
        }
    }

    #region Overrides
    public override void Reset() {
        base.Reset();
        durationInTicks = 0;
    }
    #endregion
}
