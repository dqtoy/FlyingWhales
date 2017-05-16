using UnityEngine;
using System.Collections;

public struct Log {

	public MONTH month;
	public int day;
	public int year;

	public string category;
	public string file;
	public string key;

	public Log(MONTH month, int day, int year, string category, string file, string key){
		this.month = month;
		this.day = day;
		this.year = year;
		this.category = category;
		this.file = file;
		this.key = key;
	}
}
