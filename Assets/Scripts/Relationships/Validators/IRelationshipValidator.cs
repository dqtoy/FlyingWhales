using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRelationshipValidator {
    bool CanHaveRelationship(Relatable rel1, Relatable rel2, RELATIONSHIP_TRAIT relType);

}
