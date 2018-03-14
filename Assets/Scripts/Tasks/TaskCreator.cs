/*
 Add these to classes that can create quests
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface TaskCreator {

    List<OldQuest.Quest> activeQuests { get;}

    void AddNewQuest(OldQuest.Quest quest);
    void RemoveQuest(OldQuest.Quest quest);
}
