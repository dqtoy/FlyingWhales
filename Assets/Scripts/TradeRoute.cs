using UnityEngine;
using System.Collections;

[System.Serializable]
public class TradeRoute {

    [SerializeField] private RESOURCE _resourceBeingTraded;
    [SerializeField] private Kingdom _sourceKingdom;
    [SerializeField] private Kingdom _targetKingdom;

    #region getters/setters
    public RESOURCE resourceBeingTraded {
        get { return this._resourceBeingTraded; }
    }
    public Kingdom targetKingdom {
        get { return this._targetKingdom;  }
    }
    public Kingdom sourceKingdom {
        get { return this._sourceKingdom; }
    }
    #endregion

    public TradeRoute(RESOURCE _resourceBeingTraded, Kingdom _sourceKingdom, Kingdom _targetKingdom) {
        this._resourceBeingTraded = _resourceBeingTraded;
        this._sourceKingdom = _sourceKingdom;
        this._targetKingdom = _targetKingdom;
    }
}
