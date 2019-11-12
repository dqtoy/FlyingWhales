using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Relatable {

    public virtual string relatableName { get; }
    public IRelationshipContainer relationshipContainer { get { return _relationshipContainer; } }
    public IRelationshipValidator relationshipValidator { get { return _relationshipValidator; } }

    private IRelationshipContainer _relationshipContainer;
    private IRelationshipValidator _relationshipValidator;

    public Relatable() {
        _relationshipContainer = RelationshipManager.Instance.CreateRelationshipContainer(this);
        _relationshipValidator = RelationshipManager.Instance.GetValidator(this);
    }

}
