using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRelationshipData {

    /// <summary>
    /// Value from -100 to 100
    /// </summary>
	int relationshipValue { get; }
    List<RELATIONSHIP_TRAIT> relationships { get; }
    RELATIONSHIP_EFFECT relationshipStatus { get; }

    void AdjustRelationshipValue(int amount);
    void AddRelationship(RELATIONSHIP_TRAIT relType);
    void RemoveRelationship(RELATIONSHIP_TRAIT relType);
    /// <summary>
    /// Check if this relationship has any of the relationship types provided.
    /// Will return true as long as 1 of the provided types were found.
    /// </summary>
    /// <param name="rels">The relationships to watch out for.</param>
    /// <returns>True or false</returns>
    bool HasRelationship(params RELATIONSHIP_TRAIT[] rels);
    List<RELATIONSHIP_TRAIT> GetAllRelationshipOfEffect(RELATIONSHIP_EFFECT effect);
}
