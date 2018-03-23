using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class StorylineData {

    private STORYLINE _storyline;
    protected Log _storylineDescription;

    protected Dictionary<object, List<Log>> _relevantItems;
    protected List<Quest> _relevantQuests;

    #region getters/setters
    public Log storylineDescription {
        get { return _storylineDescription; }
    }
    public Dictionary<object, List<Log>> relevantItems {
        get { return _relevantItems; }
    }
    public List<Quest> relevantQuests {
        get { return _relevantQuests; }
    }

    #endregion

    public StorylineData(STORYLINE storyline) {
        _storyline = storyline;
        _relevantItems = new Dictionary<object, List<Log>>();
        _relevantQuests = new List<Quest>();
    }

    #region virtuals
    /*
     Setup initial storyline elements. This is called upon world generation
         */
    public virtual void InitialStorylineSetup() { }
    /*
     Setup the storyline. Spawn characters, assign tags, create landmarks, etc. This is called when a storyline setup needs to be triggered in the middle of the game
         */
    public virtual void SetupStoryline() { }
    #endregion

    #region Relevant Items
    public void AddRelevantItem(object obj, Log initialDescription = null) {
        if (!_relevantItems.ContainsKey(obj)) {
            _relevantItems.Add(obj, new List<Log>());
        }
        if (initialDescription != null) {
            AddItemLog(obj, initialDescription);
        }
    }
    public void AddItemLog(object obj, Log log) {
        if (_relevantItems.ContainsKey(obj)) {
            _relevantItems[obj].Add(log);
        }
    }
    public void ReplaceItemLog(object obj, Log log, int indexToReplace) {
        if (_relevantItems.ContainsKey(obj)) {
            _relevantItems[obj].Insert(indexToReplace, log);
            if (_relevantItems[obj].Count < indexToReplace + 1) {
                _relevantItems[obj].RemoveAt(indexToReplace + 1);
            }
        }
    }
    #endregion

    #region Relevant Quests
    public void AddRelevantQuest(Quest quest) {
        if (!_relevantQuests.Contains(quest)) {
            _relevantQuests.Add(quest);
        }
    }
    public void RemoveRelevantQuest(Quest quest) {
        _relevantQuests.Remove(quest);
        
    }
    #endregion

    #region Logs
    /*
     This is a convenience function, so the developer doesn't have to input a log date, 
     category and file everytime they create a log.
         */
    protected virtual Log CreateLogForStoryline(string key) {
        return null;
    }
    #endregion

}
