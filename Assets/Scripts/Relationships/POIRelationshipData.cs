using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIRelationshipData : IRelationshipData {
    public int relationshipValue { get; private set; }
    public List<RELATIONSHIP_TRAIT> relationships { get; private set; }

    public POIRelationshipData() {
        relationships = new List<RELATIONSHIP_TRAIT>();
    }

    public void AdjustRelationshipValue(int amount) {
        relationshipValue += amount;
        relationshipValue = Mathf.Max(0, relationshipValue);
    }

    public void AddRelationship(RELATIONSHIP_TRAIT relType) {
        relationships.Add(relType);
    }
    public void RemoveRelationship(RELATIONSHIP_TRAIT relType) {
        relationships.Remove(relType);
    }
}
