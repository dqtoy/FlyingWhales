using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRelationshipProcessor : IRelationshipProcessor {

    public static CharacterRelationshipProcessor Instance = null;

    public CharacterRelationshipProcessor() {
        Instance = this;
    }

    public void OnRelationshipAdded(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE relType) {
        Character character1 = (rel1 as AlterEgoData).owner;
        Character character2 = (rel2 as AlterEgoData).owner;
        string relString = Utilities.NormalizeStringUpperCaseFirstLetters(relType.ToString());

        character1.opinionComponent.AdjustOpinion(character2, relString, 0);
        character2.opinionComponent.AdjustOpinion(character1, relString, 0);

        switch (relType) {
            case RELATIONSHIP_TYPE.LOVER:
                if (character1.homeRegion.area != null && character2.homeRegion.area != null && character1.homeRegion == character2.homeRegion
                    && character1.homeStructure != character2.homeStructure) {
                    if(character1.homeStructure == null && character2.homeStructure != null) {
                        character1.homeRegion.area.AssignCharacterToDwellingInArea(character1, character2.homeStructure);
                    } else if (character1.homeStructure != null && character2.homeStructure == null) {
                        character2.homeRegion.area.AssignCharacterToDwellingInArea(character2, character1.homeStructure);
                    } else {
                        //Lover conquers all, even if one character is factionless they will be together, meaning the factionless character will still have home structure
                        character1.homeRegion.area.AssignCharacterToDwellingInArea(character1, character2.homeStructure);
                    }
                }
                character1.opinionComponent.AdjustOpinion(character2, relString, 30);
                character2.opinionComponent.AdjustOpinion(character1, relString, 30);
                break;
            case RELATIONSHIP_TYPE.EX_LOVER:
                character1.opinionComponent.AdjustOpinion(character2, relString, -25);
                character2.opinionComponent.AdjustOpinion(character1, relString, - 25);
                break;
            case RELATIONSHIP_TYPE.RELATIVE:
            case RELATIONSHIP_TYPE.PARAMOUR:
                character1.opinionComponent.AdjustOpinion(character2, relString, 20);
                character2.opinionComponent.AdjustOpinion(character1, relString, 20);
                break;
            default:
                break;
        }
    }

    public void OnRelationshipRemoved(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE relType) {
        Character character1 = (rel1 as AlterEgoData).owner;
        Character character2 = (rel2 as AlterEgoData).owner;
        string relString = Utilities.NormalizeStringUpperCaseFirstLetters(relType.ToString());
        character1.opinionComponent.RemoveOpinion(character2, relString);
        character2.opinionComponent.RemoveOpinion(character1, relString);
        //switch (relType) {
        //    case RELATIONSHIP_TYPE.EX_LOVER:
        //        break;
        //    default:

        //        break;
        //}
    }

    private void CreateRelationshipLog(string key, Character character1, Character character2) {
        if (!GameManager.Instance.gameHasStarted || character1.isSwitchingAlterEgo) {
            return; //do not log initial relationships or when switching alter egos
        }
        Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", key);
        log.AddToFillers(character1, character1.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(character2, character2.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToInvolvedObjects();
    }
}
