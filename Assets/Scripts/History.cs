using UnityEngine;
using System.Collections;

public class History {
	public int month;
	public int week;
	public int year;

	public string description;
	public HISTORY_IDENTIFIER identifier;
	public bool isPositive;

	public History(int month, int week, int year, string description, HISTORY_IDENTIFIER identifier, bool isPositive = true){ //= HISTORY_IDENTIFIER.NONE
		this.month = month;
		this.week = week;
		this.year = year;
		this.description = description;
		this.identifier = identifier;
		this.isPositive = isPositive;
	}
}
