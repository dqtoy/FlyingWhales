using UnityEngine;
using System.Collections;

public struct GameDate {
	public int month;
	public int day;
	public int year;

	public GameDate(bool isCurrent = true){
		this.month = GameManager.Instance.month;
		this.day = GameManager.Instance.days;
		this.year = GameManager.Instance.year;
	}

	public void AddDays(int amount){
		this.day += amount;
		while (this.day > GameManager.daysInMonth[this.month]) {
			this.day -= GameManager.daysInMonth [this.month];
			AddMonths (1);
		}
	} 

	public void AddMonths(int amount){
		this.month += amount;
		if (this.month > 12) {
			this.month -= 12;
			this.year += 1;
		}
	}

	public void AddYears(int amount){
		this.year += amount;
	}

	public void SetDate(int month, int day, int year){
		this.month = month;
		this.day = day;
		this.year = year;
	}
}
