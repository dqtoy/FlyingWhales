using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRelationshipContainer {

    Dictionary<IRelatable, IRelationshipData> relationships { get; }

    void CreateNewRelationship(IRelatable relatable);
    void AdjustRelationshipValue(IRelatable relatable, int adjustment);
    void AddRelationship(IRelatable relatable, RELATIONSHIP_TRAIT rel);
    void RemoveRelationship(IRelatable relatable, RELATIONSHIP_TRAIT rel);
}
