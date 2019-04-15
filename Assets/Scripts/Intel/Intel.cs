using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intel  {
    public Log intelLog;


    public void SetIntelLog(Log log) {
        intelLog = log;
    }

    /// <summary>
    /// Function called when an intel reaches it's expiry date and the player has not obtained it.
    /// </summary>
    public virtual void OnIntelExpire() { } 
}
