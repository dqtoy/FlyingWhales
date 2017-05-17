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

	public List<object> fillers;

	public Log(int month, int day, int year, string category, string file, string key, List<object> fillers){
		this.month = (MONTH)month;
		this.day = day;
		this.year = year;
		this.category = category;
		this.file = file;
		this.key = key;
		this.fillers = fillers;
	}
}
