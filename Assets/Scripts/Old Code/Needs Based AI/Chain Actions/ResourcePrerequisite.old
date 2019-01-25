using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcePrerequisite : IPrerequisite {
    private RESOURCE _resourceType;
    public int amount;
    private CharacterAction _action;
    private IObject _iobject;

    #region getters/setters
    public PREREQUISITE prerequisiteType {
        get { return PREREQUISITE.RESOURCE; }
    }
    public CharacterAction action {
        get { return _action; }
    }
    public IObject iobject {
        get { return _iobject; }
    }
    public RESOURCE resourceType {
        get {
            if(_resourceType == RESOURCE.NONE) {
                return _iobject.madeOf;
            }
            return _resourceType;
        }
    }
    #endregion

    public ResourcePrerequisite(RESOURCE resource, int amount, CharacterAction characterAction, IObject iobject) {
        this._resourceType = resource;
        this.amount = amount;
        this._action = characterAction;
        this._iobject = iobject;
    }

    public void SetAction(CharacterAction action) {
        this._action = action;
    }
}
