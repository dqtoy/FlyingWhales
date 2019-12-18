using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRelationshipValidator : IRelationshipValidator {

    public static CharacterRelationshipValidator Instance = null;

    public CharacterRelationshipValidator() {
        Instance = this;
    }

    public bool CanHaveRelationship(Relatable character, Relatable target, RELATIONSHIP_TYPE type) {
        //TODO:
        //if (target.characterClass.className == "Zombie" || character.characterClass.className == "Zombie") {
        //    return false; //Zombies cannot create relationships
        //}
        //NOTE: This is only one way checking. This character will only check itself, if he/she meets the requirements of a given relationship
        List<RELATIONSHIP_TYPE> relationshipsWithTarget = character.relationshipContainer.GetRelationshipDataWith(target)?.relationships ?? null;
        switch (type) {
            case RELATIONSHIP_TYPE.ENEMY:
                return relationshipsWithTarget == null || (!relationshipsWithTarget.Contains(RELATIONSHIP_TYPE.ENEMY) && !relationshipsWithTarget.Contains(RELATIONSHIP_TYPE.FRIEND) && !relationshipsWithTarget.Contains(RELATIONSHIP_TYPE.LOVER)); //check that the target character is not already this characters enemy and that this character is also not his friend or his lover
            case RELATIONSHIP_TYPE.FRIEND:
                return relationshipsWithTarget == null || (!relationshipsWithTarget.Contains(RELATIONSHIP_TYPE.FRIEND) && !relationshipsWithTarget.Contains(RELATIONSHIP_TYPE.ENEMY)); //check that the target character is not already this characters friend and that this character is also not his enemy
            case RELATIONSHIP_TYPE.LOVER:
                //- **Lover:** Positive, Permanent (Can only have 1)
                //check if this character already has a lover and that the target character is not his/her paramour
                if (character.relationshipContainer.GetRelatablesWithRelationship(type).Count > 0) {
                    return false;
                }
                if (relationshipsWithTarget != null &&
                    (relationshipsWithTarget.Contains(RELATIONSHIP_TYPE.PARAMOUR) || relationshipsWithTarget.Contains(RELATIONSHIP_TYPE.ENEMY))) {
                    return false;
                }
                return true;

            case RELATIONSHIP_TYPE.PARAMOUR:
                //- **Paramour:** Positive, Transient (Can only have 1)
                //check if this character already has a paramour and that the target character is not his/her lover
                //Comment Reason: Allowed multiple paramours
                //if (GetCharacterWithRelationship(type) != null) {
                //    return false;
                //}
                if (relationshipsWithTarget != null && relationshipsWithTarget.Contains(RELATIONSHIP_TYPE.LOVER)) {
                    return false;
                }
                //one of the characters must have a lover
                if (target.relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TYPE.LOVER).Count == 0  &&
                    character.relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TYPE.LOVER).Count == 0) {
                    return false;
                }

                return true;
        }
        return true;
    }
}
