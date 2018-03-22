using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class StorylineData {

    private STORYLINE _storyline;

    private Dictionary<Character, List<Log>> _relevantCharacters;
    private Dictionary<Item, List<Log>> _relevantItems;
    private Dictionary<Quest, List<Log>> _relevantQuests;
    private Dictionary<BaseLandmark, List<Log>> _relevantLandmarks;

    public StorylineData(STORYLINE storyline) {
        _storyline = storyline;
        _relevantCharacters = new Dictionary<Character, List<Log>>();
        _relevantItems = new Dictionary<Item, List<Log>>();
        _relevantQuests = new Dictionary<Quest, List<Log>>();
        _relevantLandmarks = new Dictionary<BaseLandmark, List<Log>>();
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

    #region Relevant Characters
    public void AddRelevantCharacter(Character character, Log initialDescription = null) {
        if (!_relevantCharacters.ContainsKey(character)) {
            _relevantCharacters.Add(character, new List<Log>());
        }
        if (initialDescription != null) {
            AddCharacterLog(character, initialDescription);
        }
    }
    public void AddCharacterLog(Character character, Log log) {
        if (_relevantCharacters.ContainsKey(character)) {
            _relevantCharacters[character].Add(log);
        }
    }
    #endregion

    #region Relevant Items
    public void AddRelevantItem(Item item, Log initialDescription = null) {
        if (!_relevantItems.ContainsKey(item)) {
            _relevantItems.Add(item, new List<Log>());
        }
        if (initialDescription != null) {
            AddItemLog(item, initialDescription);
        }
    }
    public void AddItemLog(Item item, Log log) {
        if (_relevantItems.ContainsKey(item)) {
            _relevantItems[item].Add(log);
        }
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
