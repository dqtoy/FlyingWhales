using UnityEngine;
using System.Collections;

public class TradeDeal {

    private Kingdom _kingdom1;
    private Kingdom _kingdom2;

    #region getters/setters
    internal Kingdom kingdom1 {
        get { return _kingdom1; }
    }
    internal Kingdom kingdom2 {
        get { return _kingdom2; }
    }
    #endregion

    public TradeDeal(Kingdom kingdom1, Kingdom kingdom2) {
        _kingdom1 = kingdom1;
        _kingdom2 = kingdom2;
    }
}
