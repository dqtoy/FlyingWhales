using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class HiddenDesire {
    protected string _name;
    protected string _description;
    protected HIDDEN_DESIRE _type;
    protected Character _host;

    public HiddenDesire(HIDDEN_DESIRE type, Character host) {
        _host = host;
        _type = type;
        _name = Utilities.NormalizeStringUpperCaseFirstLetters(_type.ToString());
    }

    #region Virtuals
    public virtual void Awaken() {

    }
    #endregion
}
