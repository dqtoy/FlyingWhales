using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRelationshipContainer {

    Dictionary<Relatable, IRelationshipData> relationships { get; }

    void AdjustRelationshipValue(Relatable relatable, int adjustment);

    #region Adding
    void AddRelationship(Relatable relatable, RELATIONSHIP_TRAIT rel);
    void CreateNewRelationship(Relatable relatable);
    #endregion

    #region Removing
    void RemoveRelationship(Relatable relatable, RELATIONSHIP_TRAIT rel);
    #endregion

    #region Inquiry
    bool HasRelationshipWith(Relatable relatable);
    bool HasRelationshipWith(Relatable alterEgo, RELATIONSHIP_TRAIT relType);
    #endregion

    #region Getting
    Relatable GetFirstRelatableWithRelationship(params RELATIONSHIP_TRAIT[] type);
    List<Relatable> GetRelatablesWithRelationship(params RELATIONSHIP_TRAIT[] type);
    List<Relatable> GetRelatablesWithRelationship(RELATIONSHIP_EFFECT effect);
    RELATIONSHIP_EFFECT GetRelationshipEffectWith(Relatable relatable);
    IRelationshipData GetRelationshipDataWith(Relatable relatable);
    #endregion
}
