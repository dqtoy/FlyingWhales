using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIRelationshipContainer : IRelationshipContainer {
    public Dictionary<Relatable, IRelationshipData> relationships { get; private set; }

    public POIRelationshipContainer() {
        relationships = new Dictionary<Relatable, IRelationshipData>();
    }

    public void AdjustRelationshipValue(Relatable relatable, int adjustment) {
        if (HasRelationshipWith(relatable) == false) {
            CreateNewRelationship(relatable);
        }
        relationships[relatable].AdjustRelationshipValue(adjustment);
    }

    #region Adding
    public void AddRelationship(Relatable relatable, RELATIONSHIP_TRAIT relType) {
        if (HasRelationshipWith(relatable) == false) {
            CreateNewRelationship(relatable);
        }
        relationships[relatable].AddRelationship(relType);
    }
    public void CreateNewRelationship(Relatable relatable) {
        relationships.Add(relatable, new POIRelationshipData());
    }
    #endregion

    #region Removing
    public void RemoveRelationship(Relatable relatable, RELATIONSHIP_TRAIT rel) {
        relationships[relatable].RemoveRelationship(rel);
    }
    #endregion

    #region Inquiry
    public bool HasRelationshipWith(Relatable relatable) {
        return relationships.ContainsKey(relatable);
    }
    public bool HasRelationshipWith(Relatable alterEgo, RELATIONSHIP_TRAIT relType) {
        if (HasRelationshipWith(alterEgo)) {
            IRelationshipData data = relationships[alterEgo];
            return data.relationships.Contains(relType);
        }
        return false;
    }
    #endregion

    #region Getting
    public Relatable GetFirstRelatableWithRelationship(params RELATIONSHIP_TRAIT[] type) {
        foreach (KeyValuePair<Relatable, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
                return kvp.Key;
            }
        }
        return null;
    }
    public List<Relatable> GetRelatablesWithRelationship(params RELATIONSHIP_TRAIT[] type) {
        List<Relatable> relatables = new List<Relatable>();
        foreach (KeyValuePair<Relatable, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
                relatables.Add(kvp.Key);
            }
        }
        return relatables;
    }
    public List<Relatable> GetRelatablesWithRelationship(RELATIONSHIP_EFFECT effect) {
        List<Relatable> relatables = new List<Relatable>();
        foreach (KeyValuePair<Relatable, IRelationshipData> kvp in relationships) {
            if (kvp.Value.relationshipStatus == effect) {
                relatables.Add(kvp.Key);
            }
        }
        return relatables;
    }
    public RELATIONSHIP_EFFECT GetRelationshipEffectWith(Relatable relatable) {
        if (HasRelationshipWith(relatable)) {
            return relationships[relatable].relationshipStatus;
        }
        return RELATIONSHIP_EFFECT.NONE;
    }
    public IRelationshipData GetRelationshipDataWith(Relatable relatable) {
        if (HasRelationshipWith(relatable)) {
            return relationships[relatable];
        }
        return null;
    }
    #endregion
}
