using UnityEngine;
using System.Collections;

[System.Serializable]
public class BodyAttribute {
    public IBodyPart.ATTRIBUTE attribute;
    private bool _isUsed;

    #region getters/setters
    internal bool isUsed {
        get { return _isUsed; }
    }
    #endregion

    internal void SetAttributeAsUsed(bool isUsed) {
        _isUsed = isUsed;
    }

}