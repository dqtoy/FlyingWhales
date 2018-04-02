using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Log {

    public int id;

	public MONTH month;
	public int day;
	public int year;

	public string category;
	public string file;
	public string key;

	public List<LogFiller> fillers;
	public object[] allInvolved;

    public string logCallStack;

	public Log(int month, int day, int year, string category, string file, string key){
        this.id = Utilities.SetID<Log>(this);
		this.month = (MONTH)month;
		this.day = day;
		this.year = year;
		this.category = category;
		this.file = file;
		this.key = key;
		this.fillers = new List<LogFiller>();
        logCallStack = StackTraceUtility.ExtractStackTrace();
	}
    public Log(GameDate date, string category, string file, string key) {
        this.id = Utilities.SetID<Log>(this);
        this.month = (MONTH)date.month;
        this.day = date.day;
        this.year = date.year;
        this.category = category;
        this.file = file;
        this.key = key;
        this.fillers = new List<LogFiller>();
        logCallStack = StackTraceUtility.ExtractStackTrace();
    }

    internal void AddToFillers(object obj, string value, LOG_IDENTIFIER identifier){
		this.fillers.Add (new LogFiller (obj, value, identifier));
	}

	internal void AddAllInvolvedObjects(object[] objects){
		this.allInvolved = objects;
	}
}
