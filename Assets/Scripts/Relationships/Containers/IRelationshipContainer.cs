using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRelationshipContainer {

    Dictionary<Relatable, IRelationshipData> relationships { get; }

    void AdjustRelationshipValue(Relatable relatable, int adjustment);

    #region Adding
    void AddRelationship(Relatable relatable, RELATIONSHIP_TYPE rel);
    void CreateNewRelationship(Relatable relatable);
    #endregion

    #region Removing
    void RemoveRelationship(Relatable relatable, RELATIONSHIP_TYPE rel);
    #endregion

    #region Inquiry
    bool HasRelationshipWith(Relatable relatable);
    bool HasRelationshipWith(Relatable alterEgo, RELATIONSHIP_TYPE relType);
    bool HasRelationshipWith(Relatable alterEgo, params RELATIONSHIP_TYPE[] relType);
    #endregion

    #region Getting
    Relatable GetFirstRelatableWithRelationship(params RELATIONSHIP_TYPE[] type);
    List<Relatable> GetRelatablesWithRelationship(params RELATIONSHIP_TYPE[] type);
    List<Relatable> GetRelatablesWithRelationship(RELATIONSHIP_EFFECT effect);
    RELATIONSHIP_EFFECT GetRelationshipEffectWith(Relatable relatable);
    IRelationshipData GetRelationshipDataWith(Relatable relatable);
    //Returns the relationship where the choices are the relationships that are passed to the function
    //Example: If we want to know if the character is lover or paramour of another character we will use this function because this will return if their relationship is lover or paramour
    RELATIONSHIP_TYPE GetRelationshipFromParametersWith(Relatable alterEgo, params RELATIONSHIP_TYPE[] relType);
    #endregion
}
