using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Log and Memory are the same now so assume that this class will have data that the Memory uses
public class Log {

    public int id;

	public MONTH month;
	public int day;
	public int year;
    public int tick;

    public string category;
	public string file;
	public string key;

    public string message;

	public List<LogFiller> fillers;

    private bool lockFillers;

    public string logCallStack;

    public GameDate date {
        get { return new GameDate((int)month, day, year, tick); }
    }

    //When this log is processed through the LogReplacer for the first time, the resulting text will be stored in this so that every time the text of this log is needed,
    //it will not go through the LogReplacer processing again, which saves cpu power
    public string logText { get; private set; }

    //Memory data
    private GoapAction _goapAction;
    public GoapAction goapAction {
        get { return GetGoapActionAssociatedWithThisLog(); }
    }

    public Log(GameDate date, string category, string file, string key, GoapAction goapAction = null) {
        this.id = Utilities.SetID<Log>(this);
        this.month = (MONTH)date.month;
        this.day = date.day;
        this.year = date.year;
        this.tick = date.tick;
        this.category = category;
        this.file = file;
        this.key = key;
        this._goapAction = goapAction;
        this.fillers = new List<LogFiller>();
        this.lockFillers = false;
        logText = string.Empty;
        //logCallStack = StackTraceUtility.ExtractStackTrace();
    }

    public Log(GameDate date, string message, GoapAction goapAction = null) {
        this.id = Utilities.SetID<Log>(this);
        this.month = (MONTH)date.month;
        this.day = date.day;
        this.year = date.year;
        this.tick = date.tick;
        this.message = message;
        this._goapAction = goapAction;
        this.fillers = new List<LogFiller>();
        this.lockFillers = false;
        logText = string.Empty;
        //logCallStack = StackTraceUtility.ExtractStackTrace();
    }

    internal void AddToFillers(object obj, string value, LOG_IDENTIFIER identifier, bool replaceExisting = true){
        if (lockFillers) {
            return;
        }
        if (replaceExisting && HasFillerForIdentifier(identifier)) {
            fillers.Remove(GetFillerForIdentifier(identifier));
        }
		this.fillers.Add (new LogFiller (obj, value, identifier));
	}
    internal void AddToFillers(LogFiller filler, bool replaceExisting = true) {
        AddToFillers(filler.obj, filler.value, filler.identifier, replaceExisting);
    }
    internal void AddToFillers(List<LogFiller> fillers, bool replaceExisting = true) {
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller filler = fillers[i];
            AddToFillers(filler);
        }
    }
    public void SetFillers(List<LogFiller> fillers) {
        if (lockFillers) {
            return;
        }
        this.fillers = fillers;
    }
    public void AddLogToInvolvedObjects() {
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller currFiller = fillers[i];
            object obj = currFiller.obj;
            if (obj != null) {
                if (obj is Character) {
                    (obj as Character).AddHistory(this);
                } 
                //else if (obj is Area) {
                //    (obj as Area).AddHistory(this);
                //} 
                else if (obj is Minion) {
                    (obj as Minion).character.AddHistory(this);
                } else if (obj is Faction) {
                    (obj as Faction).AddHistory(this);
                }
            }
        }
    }
    public void AddLogToSpecificObjects(params LOG_IDENTIFIER[] identifiers) {
        List<LOG_IDENTIFIER> identifiersList = identifiers.ToList();
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller currFiller = fillers[i];
            object obj = currFiller.obj;
            if (obj != null && identifiersList.Contains(currFiller.identifier)) {
                if (obj is Character) {
                    (obj as Character).AddHistory(this);
                } 
                //else if (obj is Area) {
                //    (obj as Area).AddHistory(this);
                //} 
                else if (obj is Minion) {
                    (obj as Minion).character.AddHistory(this);
                } else if (obj is Faction) {
                    (obj as Faction).AddHistory(this);
                }
            }
        }
    }
    public bool HasFillerForIdentifier(LOG_IDENTIFIER identifier) {
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller currFiller = fillers[i];
            if (currFiller.identifier == identifier) {
                return true;
            }
        }
        return false;
    }
    public LogFiller GetFillerForIdentifier(LOG_IDENTIFIER identifier) {
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller currFiller = fillers[i];
            if (currFiller.identifier == identifier) {
                return currFiller;
            }
        }
        return default(LogFiller);
    }
    public bool IsIncludedInFillers(object obj) {
        for (int i = 0; i < fillers.Count; i++) {
            if (fillers[i].obj == obj) {
                return true;
            }
        }
        return false;
    }
    private GoapAction GetGoapActionAssociatedWithThisLog() {
        if(_goapAction != null) {
            if(_goapAction.goapType == INTERACTION_TYPE.SHARE_INFORMATION) {
                return (_goapAction as ShareInformation).eventToBeShared;
            }else if (_goapAction.goapType == INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_FRIENDSHIP) {
                return (_goapAction as SpreadRumorRemoveFriendship).chosenMemory.goapAction;
            }else if (_goapAction.goapType == INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_LOVE) {
                return (_goapAction as SpreadRumorRemoveLove).chosenMemory.goapAction;
            }
        }
        return _goapAction;
    }

    #region Utilities
    public void SetFillerLockedState(bool state) {
        lockFillers = state;
    }
    public string GetLogDebugInfo() {
        //if (fromInteraction.intel != null) {
        //    return fromInteraction.intel.GetDebugInfo();
        //}
        return string.Empty;
    }
    public void SetDate(GameDate date) {
        this.month = (MONTH)date.month;
        this.day = date.day;
        this.year = date.year;
        this.tick = date.tick;
    }
    public void SetLogText(string text) {
        this.logText = text;
    }
    #endregion
}
