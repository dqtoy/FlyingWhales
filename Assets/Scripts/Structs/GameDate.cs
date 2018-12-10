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
        this.hour = 0;
    }

   // public void AddHours(int amount) {
   //     this.hour += amount;
   //     while (this.hour > GameManager.hoursPerDay) {
			//this.hour -= GameManager.hoursPerDay;
   //         AddDays(1);
   //     }
   // }

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

    public void ReduceHours(int amount) {
        for (int i = 0; i < amount; i++) {
            this.hour -= 1;
            if (this.hour <= 0) {
                ReduceDays(1);
                this.hour = GameManager.hoursPerDay;
            }
        }
    }
    public void ReduceDays(int amount) {
        for (int i = 0; i < amount; i++) {
            this.day -= 1;
            if (this.day == 0) {
                ReduceMonth(1);
                this.day = GameManager.daysInMonth[this.month];
            }
        }
    }
    public void ReduceMonth(int amount) {
        for (int i = 0; i < amount; i++) {
            this.month -= 1;
            if (this.month == 0) {
                this.month = 12; //last month
                ReduceYear(1);
            }
        }
    }
    public void ReduceYear(int amount) {
        for (int i = 0; i < amount; i++) {
            this.year -= 1;
        }
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

    /*
     Is this date before other date
         */
    public bool IsBefore(GameDate otherDate) {
        if (this.year < otherDate.year) {
            return true;
        } else if (this.year == otherDate.year) {
            //the 2 dates are of the same year
            if (this.month < otherDate.month) {
                //this.month is less than the otherDate.month
                return true;
            } else if (this.month == otherDate.month) {
                //this.month is equal to otherDate.month
                if (this.day < otherDate.day) {
                    return true;
                } else if (this.day == otherDate.day) {
                    if (this.hour < otherDate.hour) {
                        return true;
                    } else if (this.hour == otherDate.hour) {
                        //the 2 dates are the exact same, return false
                        return false;
                    } else {
                        return false;
                    }
                } else {
                    //this.day is greater than otherDate.year
                    return false;
                }
            } else {
                //this.month is greater than otherDate.month)
                return false;
            }
        } else {
            //this.year is greater than otherDate.year
            return false;
        }
       
    }

    /*
     Is this date after other date
         */
    public bool IsAfter(GameDate otherDate) {
        if (this.year < otherDate.year) {
            return false;
        } else if (this.year == otherDate.year) {
            //the 2 dates are of the same year
            if (this.month < otherDate.month) {
                //this.month is less than the otherDate.month
                return false;
            } else if (this.month == otherDate.month) {
                //this.month is equal to otherDate.month
                if (this.day < otherDate.day) {
                    return false;
                } else if (this.day == otherDate.day) {
                    if (this.hour < otherDate.hour) {
                        return false;
                    } else if (this.hour == otherDate.hour) {
                        //the 2 dates are the exact same, return false
                        return false;
                    } else {
                        return true;
                    }
                } else {
                    //this.day is greater than otherDate.day
                    return true;
                }
            } else {
                //this.month is greater than otherDate.month)
                return false;
            }
        } else {
            //this.year is greater than otherDate.year
            return true;
        }

    }

    public string ToStringDate(){
		return ((MONTH)this.month).ToString() + " " + this.day + ", " + this.year + " H: " + this.hour;
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
        return ConvertToDays().ToString();// + "." + hour.ToString();
    }

    public override bool Equals(object obj) {
        //if (obj is GameDate) {
        //    return Equals((GameDate)obj);
        //}
        return base.Equals(obj);
    }

    public bool Equals(GameDate otherDate) {
        if (this.year == otherDate.year && this.month == otherDate.month && this.day == otherDate.day) {
            return true;
        }
        return false;
    }
}
