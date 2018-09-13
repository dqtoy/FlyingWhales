using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DateRange {

    public GameDate startDate;
    public GameDate endDate;

    public DateRange(GameDate startDate, GameDate endDate) {
        this.startDate = startDate;
        this.endDate = endDate;
    }

    public bool IsInRange(GameDate date) {
        if (date.IsSameDate(startDate) || date.IsSameDate(endDate)) {
            return true; //date is the same as start or end date
        } else if (date.IsAfter(startDate) && date.IsBefore(endDate)) {
            return true; //date is in between start and end date
        } else {
            return false; //date is not in range
        }
    }
}
