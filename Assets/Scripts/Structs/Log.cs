using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Log {

	public MONTH month;
	public int day;
	public int year;

	public string category;
	public string file;
	public string key;

	public List<LogFiller> fillers;

	public Log(int month, int day, int year, string category, string file, string key){
		this.month = (MONTH)month;
		this.day = day;
		this.year = year;
		this.category = category;
		this.file = file;
		this.key = key;
		this.fillers = new List<LogFiller>();
	}

	internal void AddToFillers(object obj, string value, LOG_IDENTIFIER identifier){
		this.fillers.Add (new LogFiller (obj, value, identifier));
	}
}
