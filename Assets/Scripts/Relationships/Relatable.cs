using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Relatable {

    public virtual string relatableName { get; }
    public IRelationshipContainer relationshipContainer { get { return _relationshipContainer; } }
    public IRelationshipValidator relationshipValidator { get { return _relationshipValidator; } }
    public IRelationshipProcessor relationshipProcessor { get { return _relationshipProcessor; } }

    private IRelationshipContainer _relationshipContainer;
    private IRelationshipValidator _relationshipValidator;
    private IRelationshipProcessor _relationshipProcessor;

    public Relatable() {
        _relationshipContainer = RelationshipManager.Instance.CreateRelationshipContainer(this);
        _relationshipValidator = RelationshipManager.Instance.GetValidator(this);
        _relationshipProcessor = RelationshipManager.Instance.GetProcessor(this);
    }

}
