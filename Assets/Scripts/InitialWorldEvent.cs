using UnityEngine;
using System.Collections;

[System.Serializable]
public class InitialWorldEvent {
    [SerializeField] private EVENT_TYPES _eventType;
    [SerializeField] private int _quantity;

    #region getters/setters
    public EVENT_TYPES eventType {
        get { return _eventType; }
    }
    public int quantity {
        get { return _quantity; }
    }
    #endregion
}
