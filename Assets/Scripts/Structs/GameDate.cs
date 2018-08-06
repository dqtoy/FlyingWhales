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
        while (this.hour > GameManager.Instance.hoursPerDay) {
			this.hour -= GameManager.Instance.hoursPerDay;
            AddDays(1);
        }
    }

	public void AddDays(int amount){
		this.day += amount;
        int count = 0;
		while (this.day > GameManager.daysInMonth[this.month]) {
			this.day -= GameManager.daysInMonth [this.month];
            count++;
		}
        if(count > 0) {
            AddMonths(count);
        }
    } 

	public void AddMonths(int amount){
		this.month += amount;
		while (this.month > 12) {
			this.month -= 12;
            AddYears(1);
        }
        if(this.day > GameManager.daysInMonth[this.month]) {
            this.day = GameManager.daysInMonth[this.month];
        }
	}

	public void AddYears(int amount){
		this.year += amount;
	}

	public void SetDate(int month, int day, int year, int hour){
		this.month = month;
		this.day = day;
		this.year = year;
        this.hour = hour;
	}
    public void SetHours(int hour) {
        this.hour = hour;
    }
	public void SetDate(GameDate gameDate){
		this.month = gameDate.month;
		this.day = gameDate.day;
		this.year = gameDate.year;
        this.hour = gameDate.hour;
	}
	public bool IsSameDate(int month, int day, int year, int hour){
		if(this.month == month && this.day == day && this.year == year && this.hour == hour){
			return true;
		}
		return false;
	}
	public bool IsSameDate(GameDate gameDate){
		if(this.month == gameDate.month && this.day == gameDate.day && this.year == gameDate.year && this.hour == gameDate.hour) {
			return true;
		}
		return false;
	}

	public string ToStringDate(){
		return ((MONTH)this.month).ToString() + " " + this.day + ", " + this.year;
	}

    public int ConvertToDays() {
        int totalDays = 0;
        if (year > GameManager.Instance.startYear) {
            int difference = year - GameManager.Instance.startYear;
            totalDays += 365 * difference;
        }
        for (int i = 1; i < month; i++) {
            totalDays += GameManager.daysInMonth[i];
        }
        totalDays += day;

        return totalDays;
    }

    public string GetDayAndTicksString() {
        return ConvertToDays().ToString() + "." + hour.ToString();
    }
}
