using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRelationshipProcessor : IRelationshipProcessor {

    public static CharacterRelationshipProcessor Instance = null;

    public CharacterRelationshipProcessor() {
        Instance = this;
    }

    public void OnRelationshipAdded(Relatable rel1, Relatable rel2, RELATIONSHIP_TRAIT relType) {
        Character character1 = (rel1 as AlterEgoData).owner;
        Character character2 = (rel2 as AlterEgoData).owner;

        switch (relType) {
            case RELATIONSHIP_TRAIT.ENEMY:
                CreateRelationshipLog("enemy", character1, character2);
                break;
            case RELATIONSHIP_TRAIT.FRIEND:
                CreateRelationshipLog("friend", character1, character2);
                break;
            case RELATIONSHIP_TRAIT.LOVER:
                if (character1.homeArea != null && character2.homeArea != null && character1.homeArea.id == character2.homeArea.id
                    && character1.homeStructure != character2.homeStructure) {
                    //Lover conquers all, even if one character is factionless they will be together, meaning the factionless character will still have home structure
                    character1.homeArea.AssignCharacterToDwellingInArea(character1, character2.homeStructure);
                }
                break;
            default:
                break;
        }
    }

    public void OnRelationshipRemoved(Relatable rel1, Relatable rel2, RELATIONSHIP_TRAIT relType) {
        Character character1 = (rel1 as AlterEgoData).owner;
        Character character2 = (rel2 as AlterEgoData).owner;

        switch (relType) {
            case RELATIONSHIP_TRAIT.ENEMY:
                CreateRelationshipLog("not_enemy", character1, character2);
                break;
            case RELATIONSHIP_TRAIT.FRIEND:
                CreateRelationshipLog("not_friend", character1, character2);
                break;
            default:
                break;
        }
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
