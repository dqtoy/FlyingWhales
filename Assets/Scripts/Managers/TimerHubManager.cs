using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerHubManager : MonoBehaviour {
    public static TimerHubManager Instance = null;

    #region Monobehaviour
    void Awake() {
        Instance = this;
    }
    #endregion


}
