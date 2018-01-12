/*
 Add these to classes that can create quests
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface QuestCreator {

    List<Quest> activeQuests { get;}

    void AddNewQuest(Quest quest);
    void RemoveQuest(Quest quest);
}
