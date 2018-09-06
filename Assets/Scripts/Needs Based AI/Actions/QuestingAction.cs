using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestingAction : CharacterAction {
    public QuestingAction() : base(ACTION_TYPE.QUESTING) { }

    #region overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        if (party.mainCharacter is Character) {
            Character mainCharacter = party.mainCharacter as Character;
            if (mainCharacter.HasQuest()) {
                //the character already has a quest, do that.
                QuestAction questAction = mainCharacter.currentQuest.GetQuestAction(mainCharacter);
                party.actionData.ForceDoAction(questAction.action, questAction.targetObject);
            } else {
                //the character currently does not have a quest, get one from workplace
                if (mainCharacter.workplace.HasQuestBoard()) {
                    Quest createdQuest = mainCharacter.workplace.questBoard.GenerateQuestForCharacter(mainCharacter);
                    mainCharacter.SetQuest(createdQuest);
                } else {
                    throw new System.Exception(mainCharacter.name + "'s workplace has no quest board!");
                }
            }
            //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
            GiveAllReward(party);
        }
    }
    public override CharacterAction Clone() {
        QuestingAction action = new QuestingAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
