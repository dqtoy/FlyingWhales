using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIRelationshipContainer : IRelationshipContainer {
    public Dictionary<IRelatable, IRelationshipData> relationships { get; private set; }

    public POIRelationshipContainer() {
        relationships = new Dictionary<IRelatable, IRelationshipData>();
    }

    #region Adding
    public void AddRelationship(IRelatable relatable, RELATIONSHIP_TRAIT relType) {
        if (HasRelationshipWith(relatable) == false) {
            CreateNewRelationship(relatable);
        }
        relationships[relatable].AddRelationship(relType);
    }
    public void AdjustRelationshipValue(IRelatable relatable, int adjustment) {
        if (HasRelationshipWith(relatable) == false) {
            CreateNewRelationship(relatable);
        }
        relationships[relatable].AdjustRelationshipValue(adjustment);
    }
    public void CreateNewRelationship(IRelatable relatable) {
        relationships.Add(relatable, new POIRelationshipData());
    }
    #endregion

    #region Removing
    public void RemoveRelationship(IRelatable relatable, RELATIONSHIP_TRAIT rel) {
        relationships[relatable].RemoveRelationship(rel);
    }
    #endregion

    #region Inquiry
    private bool HasRelationshipWith(IRelatable relatable) {
        return relationships.ContainsKey(relatable);
    }
    #endregion
}
