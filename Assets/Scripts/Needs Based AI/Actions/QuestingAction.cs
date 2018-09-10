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
            //check first if the character is part of a squad
            if (mainCharacter.squad != null) { //if the character is part of a squad
                //check if he/she is the squad leader
                if (mainCharacter.IsSquadLeader()) { //if the character is a squad leader
                    //check if he/she already has a quest
                    if (!mainCharacter.HasQuest()) { //if none, get one from workplace
                        if (mainCharacter.workplace.HasQuestBoard()) {
                            Quest createdQuest = mainCharacter.workplace.questBoard.GenerateQuestForCharacter(mainCharacter);
                            mainCharacter.SetQuest(createdQuest);
                        } else {
                            throw new System.Exception(mainCharacter.name + "'s workplace has no quest board!");
                        }
                    }
                    //After getting a quest, start waiting for party members
                    if (mainCharacter.squad.squadMembers.Count == mainCharacter.ownParty.icharacters.Count) {
                        //if the character's squad is already complete, do not wait
                        QuestAction questAction = mainCharacter.currentQuest.GetQuestAction(mainCharacter);
                        party.actionData.ForceDoAction(questAction.action, questAction.targetObject);
                    } else {
                        //else, wait for 1 hour (6 ticks)
                        party.actionData.ForceDoAction(party.characterObject.currentState.GetAction(ACTION_TYPE.WAIT_FOR_PARTY), party.characterObject);
                    }
                } else { //if character is a squad member
                    //check if squad leader is already waiting for party members
                    if (party.specificLocation.IsCharacterAtLocation(mainCharacter.squad.squadLeader)) {
                        //if yes, join the party leader's party, and set action to In Party indefinitely
                        party.actionData.ForceDoAction(mainCharacter.squad.squadLeader.ownParty.icharacterObject.currentState.GetAction(ACTION_TYPE.JOIN_PARTY), mainCharacter.squad.squadLeader.ownParty.icharacterObject);
                    } else {
                        //if no, set action to waiting for party, then wait for squad leader to arrive.
                        //Once squad leader arrives, join party immediately then set action to In Party indefinitely
                        party.actionData.ForceDoAction(party.characterObject.currentState.GetAction(ACTION_TYPE.WAIT_FOR_PARTY), party.characterObject);
                    }
                }
            } else { //if the character is NOT part of a squad
                //check if he/she has a quest
                if (!mainCharacter.HasQuest()) {//if not, get one from workplace
                    if (mainCharacter.workplace.HasQuestBoard()) {
                        Quest createdQuest = mainCharacter.workplace.questBoard.GenerateQuestForCharacter(mainCharacter);
                        mainCharacter.SetQuest(createdQuest);
                    } else {
                        throw new System.Exception(mainCharacter.name + "'s workplace has no quest board!");
                    }
                } else { //if yes, get action from quest
                    QuestAction questAction = mainCharacter.currentQuest.GetQuestAction(mainCharacter);
                    party.actionData.ForceDoAction(questAction.action, questAction.targetObject);
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
