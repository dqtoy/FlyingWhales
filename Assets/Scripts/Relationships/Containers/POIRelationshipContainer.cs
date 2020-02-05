using System;
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
    public void AddRelationship(Relatable relatable, RELATIONSHIP_TYPE relType) {
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
    public void RemoveRelationship(Relatable relatable, RELATIONSHIP_TYPE rel) {
        relationships[relatable].RemoveRelationship(rel);
    }
    #endregion

    #region Inquiry
    public bool HasRelationshipWith(Relatable relatable) {
        return relationships.ContainsKey(relatable);
    }
    public bool HasRelationshipWith(Relatable alterEgo, RELATIONSHIP_TYPE relType) {
        if (HasRelationshipWith(alterEgo)) {
            IRelationshipData data = relationships[alterEgo];
            return data.relationships.Contains(relType);
        }
        return false;
    }
    public bool HasRelationshipWith(Relatable alterEgo, params RELATIONSHIP_TYPE[] relType) {
        if (HasRelationshipWith(alterEgo)) {
            IRelationshipData data = relationships[alterEgo];
            for (int i = 0; i < relType.Length; i++) {
                RELATIONSHIP_TYPE rel = relType[i];
                for (int j = 0; j < data.relationships.Count; j++) {
                    RELATIONSHIP_TYPE dataRel = data.relationships[j];
                    if(rel == dataRel) {
                        return true;
                    }
                }
            }
            return false;
        }
        return false;
    }
    public bool IsFamilyMember(Character target) {
        if (HasRelationshipWith(target)) {
            IRelationshipData data = GetRelationshipDataWith(target);
            return data.HasRelationship(RELATIONSHIP_TYPE.CHILD, RELATIONSHIP_TYPE.PARENT, RELATIONSHIP_TYPE.SIBLING);
        }
        return false;
    }
    #endregion

    #region Getting
    public Relatable GetFirstRelatableWithRelationship(params RELATIONSHIP_TYPE[] type) {
        foreach (KeyValuePair<Relatable, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
                return kvp.Key;
            }
        }
        return null;
    }
    public List<Relatable> GetRelatablesWithRelationship(params RELATIONSHIP_TYPE[] type) {
        List<Relatable> relatables = new List<Relatable>();
        foreach (KeyValuePair<Relatable, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
                relatables.Add(kvp.Key);
            }
        }
        return relatables;
    }
    public IRelationshipData GetRelationshipDataWith(Relatable relatable) {
        if (HasRelationshipWith(relatable)) {
            return relationships[relatable];
        }
        return null;
    }
    public RELATIONSHIP_TYPE GetRelationshipFromParametersWith(Relatable alterEgo, params RELATIONSHIP_TYPE[] relType) {
        if (HasRelationshipWith(alterEgo)) {
            IRelationshipData data = relationships[alterEgo];
            for (int i = 0; i < relType.Length; i++) {
                RELATIONSHIP_TYPE rel = relType[i];
                for (int j = 0; j < data.relationships.Count; j++) {
                    RELATIONSHIP_TYPE dataRel = data.relationships[j];
                    if (rel == dataRel) {
                        return rel;
                    }
                }
            }
            return RELATIONSHIP_TYPE.NONE;
        }
        return RELATIONSHIP_TYPE.NONE;
    }
    #endregion
}
