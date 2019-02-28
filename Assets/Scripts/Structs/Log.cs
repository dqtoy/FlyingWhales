using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Log {

    public int id;

	public MONTH month;
	public int day;
	public int year;
    public int tick;

	public string category;
	public string file;
	public string key;

	public List<LogFiller> fillers;

    private bool lockFillers;

    public string logCallStack;

    public Interaction fromInteraction { get; private set; }

    public GameDate date {
        get { return new GameDate((int)month, day, year, tick); }
    }

    public Log(int month, int day, int year, int tick, string category, string file, string key, Interaction fromInteraction = null){
        this.id = Utilities.SetID<Log>(this);
		this.month = (MONTH)month;
		this.day = day;
		this.year = year;
        this.tick = tick;
		this.category = category;
		this.file = file;
		this.key = key;
		this.fillers = new List<LogFiller>();
        this.lockFillers = false;
        this.fromInteraction = fromInteraction;
        logCallStack = StackTraceUtility.ExtractStackTrace();
	}
    public Log(GameDate date, string category, string file, string key, Interaction fromInteraction = null) {
        this.id = Utilities.SetID<Log>(this);
        this.month = (MONTH)date.month;
        this.day = date.day;
        this.year = date.year;
        this.tick = date.tick;
        this.category = category;
        this.file = file;
        this.key = key;
        this.fillers = new List<LogFiller>();
        this.lockFillers = false;
        this.fromInteraction = fromInteraction;
        logCallStack = StackTraceUtility.ExtractStackTrace();
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
                } else if (obj is BaseLandmark) {
                    (obj as BaseLandmark).AddHistory(this);
                } else if (obj is Area) {
                    (obj as Area).AddHistory(this);
                } else if (obj is Minion) {
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
    private LogFiller GetFillerForIdentifier(LOG_IDENTIFIER identifier) {
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
    #endregion

    #region Intel
    public InteractionIntel ConvertToIntel() {
        return new InteractionIntel(fromInteraction, this);
    }
    #endregion
}
