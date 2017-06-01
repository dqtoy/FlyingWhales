using UnityEngine;
using System.Collections;

[System.Serializable]
public class TradeRoute {

    private RESOURCE _resourceGained;
    private Kingdom _kingdomInTradeWith;

    #region getters/setters
    public RESOURCE resourceGained {
        get { return this._resourceGained; }
    }
    public Kingdom kingdomInTradeWith {
        get { return this._kingdomInTradeWith;  }
    }
    #endregion

    public TradeRoute(RESOURCE _resourceGained, Kingdom _kingdomInTradeWith) {
        this._resourceGained = _resourceGained;
        this._kingdomInTradeWith = _kingdomInTradeWith;
    }
}
