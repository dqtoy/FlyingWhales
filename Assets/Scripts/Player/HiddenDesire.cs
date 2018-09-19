using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class HiddenDesire {
    protected string _name;
    protected string _description;
    protected HIDDEN_DESIRE _type;
    protected Character _host;
    protected bool _isAwakened;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public bool isAwakened {
        get { return _isAwakened; }
    }
    #endregion

    public HiddenDesire(HIDDEN_DESIRE type, Character host) {
        _host = host;
        _type = type;
        _name = Utilities.NormalizeStringUpperCaseFirstLetters(_type.ToString());
    }

    #region Virtuals
    public virtual void Awaken() {
        _isAwakened = true;
        Debug.Log(GameManager.Instance.TodayLogString() + "Awakened " + _host.name + "'s hidden desire: " + name);
    }
    #endregion
}
