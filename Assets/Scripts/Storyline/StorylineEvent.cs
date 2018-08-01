using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorylineEvent {
    protected int _id;
    protected string _name;
    protected string _description;
    protected string _buttonName;
    protected Storyline _storylineParent;
    protected int[] _choices;

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public int[] choices {
        get { return _choices; }
    }
    #endregion

    public StorylineEvent(Storyline parent) {
        _storylineParent = parent;
    }

    public StorylineEvent(int id) {
        _id = id;
    }

    #region Virtuals
    public virtual bool CanBeUnlocked() {
        return false;
    }
    public virtual void OnStartEvent() { }
    public virtual void OnEndEvent() { }
    #endregion
}
