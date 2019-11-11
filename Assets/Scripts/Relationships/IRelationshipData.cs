using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRelationshipData {

	int relationshipValue { get; }
    List<RELATIONSHIP_TRAIT> relationships { get; }

    void AdjustRelationshipValue(int amount);
    void AddRelationship(RELATIONSHIP_TRAIT relType);
    void RemoveRelationship(RELATIONSHIP_TRAIT relType);
}
