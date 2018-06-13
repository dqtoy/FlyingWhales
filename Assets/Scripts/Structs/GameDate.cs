using UnityEngine;
using System.Collections;

public struct GameDate {
	public int month;
	public int day;
	public int year;
    public int hour;

	public GameDate(int month, int day, int year, int hour){
		this.month = month;
		this.day = day;
		this.year = year;
        this.hour = hour;
	}

    public void AddHours(int amount) {
        this.hour += amount;
        while (this.hour > 48) {
			this.hour -= 48;
            AddDays(1);
        }
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
		while (this.month > 12) {
			this.month -= 12;
            AddYears(1);
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
	public void SetDate(GameDate gameDate){
		this.month = gameDate.month;
		this.day = gameDate.day;
		this.year = gameDate.year;
	}
	public bool IsSameDate(int month, int day, int year){
		if(this.month == month && this.day == day && this.year == year){
			return true;
		}
		return false;
	}
	public bool IsSameDate(GameDate gameDate){
		if(this.month == gameDate.month && this.day == gameDate.day && this.year == gameDate.year){
			return true;
		}
		return false;
	}

	public void SetDay(int day){
		this.day = day;
	}

	public string ToStringDate(){
		return ((MONTH)this.month).ToString() + " " + this.day + ", " + this.year;
	}
}
