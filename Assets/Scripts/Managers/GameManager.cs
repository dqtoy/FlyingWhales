﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;

    public static string[] daysInWords = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
    public static TIME_IN_WORDS[] timeInWords = new TIME_IN_WORDS[] {
        TIME_IN_WORDS.AFTER_MIDNIGHT, TIME_IN_WORDS.AFTER_MIDNIGHT
        , TIME_IN_WORDS.MORNING, TIME_IN_WORDS.MORNING
        , TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.AFTERNOON
        , TIME_IN_WORDS.NIGHT, TIME_IN_WORDS.NIGHT };


    public int month;
	public int days;
	public int year;
    public int tick;
    public int continuousDays;
    public const int daysPerMonth = 30;
    public const int ticksPerDay = 96;
    public const int ticksPerTimeInWords = 12;

    public int startYear;

    public PROGRESSION_SPEED currProgressionSpeed;

	public float progressionSpeed;
	public bool isPaused = true;
    //public bool hideLandmarks = true;
    public bool initiallyHideRoads = false;
    public bool allowConsole = true;
    public bool displayFPS = true;
    public bool allCharactersAreVisible = true;
    public bool inspectAll = false;
    public bool ignoreEventTriggerWeights = false;
    public bool showFullDebug = false;

    public GameObject travelLineParentPrefab;
    public GameObject travelLinePrefab;
    public HexTile tile1;
    public HexTile tile2;

    private const float X1_SPEED = 0.75f;
    private const float X2_SPEED = 0.5f;
    private const float X4_SPEED = 0.25f;

    private float timeElapsed;
    private bool _gameHasStarted;

    [SerializeField] private Texture2D defaultCursorTexture;
    [SerializeField] private Texture2D targetCursorTexture;
    [SerializeField] private Texture2D dragWorldCursorTexture;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    public bool pauseDayEnded2 = false;

    #region getters/setters
    public bool gameHasStarted {
        get { return _gameHasStarted; }
    }
    #endregion

    #region Monobehaviours
    private void Awake() {
        Instance = this;
        this.timeElapsed = 0f;
        _gameHasStarted = false;
        SetCursorToDefault();
#if !WORLD_CREATION_TOOL
        Application.logMessageReceived += LogCallback;
#endif
    }
    private void FixedUpdate() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            if (allowConsole) {
                UIManager.Instance.ToggleConsole();
            }
        }
        if (_gameHasStarted && !isPaused) {
            if (this.timeElapsed == 0f) {
                this.DayStarted();
            }
            this.timeElapsed += Time.deltaTime;
            if (this.timeElapsed >= this.progressionSpeed) {
                this.timeElapsed = 0f;
                StartCoroutine(this.DayEnded());
            }
        }
    }
    //private void Update() {
    //    //Application.targetFrameRate = 60;
    //    //    if (!UIManager.Instance.IsConsoleShowing() && !UIManager.Instance.IsMouseOnInput() && !PlayerManager.Instance.isChoosingStartingTile) {
    //    //        if (Input.GetKeyDown(KeyCode.Space)) {
    //    //            if (isPaused) {
    //    //                //SetProgressionSpeed(currProgressionSpeed);
    //    //                //SetPausedState(false);
    //    //                if (currProgressionSpeed == PROGRESSION_SPEED.X1) {
    //    //                    UIManager.Instance.SetProgressionSpeed1X();
    //    //                } else if (currProgressionSpeed == PROGRESSION_SPEED.X2) {
    //    //                    UIManager.Instance.SetProgressionSpeed2X();
    //    //                } else if (currProgressionSpeed == PROGRESSION_SPEED.X4) {
    //    //                    UIManager.Instance.SetProgressionSpeed4X();
    //    //                }
    //    //            } else {
    //    //                //pause
    //    //                //SetPausedState(true);
    //    //                UIManager.Instance.Pause();
    //    //            }
    //    //        }
    //    //    }
    //}
    #endregion

    [ContextMenu("Start Progression")]
	public void StartProgression(){
        _gameHasStarted = true;
        //SetPausedState(false);
        UIManager.Instance.SetProgressionSpeed1X();
        UIManager.Instance.Pause();
		SchedulingManager.Instance.StartScheduleCalls ();
        Messenger.Broadcast(Signals.MONTH_START); //for the first day
    }

    [ContextMenu("Create Travel Line")]
    public void CreateTravelLine() {
        GameObject go = GameObject.Instantiate(travelLinePrefab);
        go.transform.position = tile1.transform.position;
        float angle = Mathf.Atan2(tile2.transform.position.y - tile1.transform.position.y, tile2.transform.position.x - tile1.transform.position.x) * Mathf.Rad2Deg;
        go.transform.eulerAngles = new Vector3(go.transform.rotation.x, go.transform.rotation.y, angle);
        float distance = Vector3.Distance(tile1.transform.position, tile2.transform.position);
        //float scale = (distance * 0.143f) * 6f;
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(distance, 0.2f);
        go.transform.SetParent(tile1.UIParent);
        //go.transform.localScale = new Vector3(scale, go.transform.localScale.y, go.transform.localScale.z);

        /*To get num of ticks to travel from one tile to another, get distance and divide it by 2.31588, round up the result then multiply by 6, such that,
         * numOfTicks = (Math.Ceil(distance / 2.315188)) * 6
         */
    }

    [ContextMenu("Get Distance")]
    public void GetDistance() {
        float distance = Vector3.Distance(tile1.transform.position, tile2.transform.position);
        int distanceAsTiles = Mathf.CeilToInt(distance / 2.315188f);
        Debug.LogWarning("Distance: " + distanceAsTiles);
    }

    private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2) {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }

    public GameDate Today() {
        return new GameDate(this.month, this.days, this.year, this.tick);
    }
    public string TodayLogString() {
        return "[" + continuousDays + " - " + ConvertTickToTime(tick) + "] ";
    }
    public GameDate EndOfTheMonth() {
        return new GameDate(this.month, daysPerMonth, this.year, ticksPerDay);
    }
    public int GetNextMonth() {
        int currMonth = this.month;
        currMonth++;
        if(currMonth > 12) {
            currMonth = 1;
        }
        return currMonth;
    }
    public int GetTicksDifferenceOfTwoDates(GameDate fromDate, GameDate toDate) {
        int date1DaysDiff = fromDate.day;
        for (int i = 1; i < fromDate.month; i++) {
            date1DaysDiff += daysPerMonth;
        }
        int date2DaysDiff = toDate.day;
        for (int i = 1; i < toDate.month; i++) {
            date2DaysDiff += daysPerMonth;
        }
        int daysDiff = date2DaysDiff - date1DaysDiff;
        int yearDiff = toDate.year - fromDate.year;
        if(fromDate.year > toDate.year) {
            yearDiff = fromDate.year - toDate.year;
        }

        int tickDifference = (daysDiff + (yearDiff * 365)) * daysPerMonth;
        return tickDifference;
    }
    public GameDate FirstDayOfTheMonth() {
		return new GameDate(this.month, 1, this.year, 1);
	}
    public void SetPausedState(bool isPaused){
        //Debug.Log("Set paused state to " + isPaused);
        if(this.isPaused != isPaused) {
            this.isPaused = isPaused;
            Messenger.Broadcast(Signals.PAUSED, isPaused);
        }
	}
    /*
     * Set day progression speed to 1x, 2x of 4x
     * */
	public void SetProgressionSpeed(PROGRESSION_SPEED progSpeed){
        currProgressionSpeed = progSpeed;
        //Debug.Log("Set progression speed to " + progSpeed.ToString());
        float speed = X1_SPEED;
        if (progSpeed == PROGRESSION_SPEED.X2) {
            speed = X2_SPEED;
        } else if(progSpeed == PROGRESSION_SPEED.X4){
            speed = X4_SPEED;
        }
		this.progressionSpeed = speed;
        //CombatManager.Instance.updateIntervals = this.progressionSpeed / (float) CombatManager.Instance.numOfCombatActionPerDay;
        Messenger.Broadcast(Signals.PROGRESSION_SPEED_CHANGED, progSpeed);
	}
    public void DayStarted() {
        Messenger.Broadcast(Signals.DAY_STARTED);
        Messenger.Broadcast(Signals.DAY_STARTED_2);
        Messenger.Broadcast(Signals.UPDATE_UI);
    }
    /*
     * Function that triggers daily actions
     * */
    public IEnumerator DayEnded(){
        Messenger.Broadcast(Signals.DAY_ENDED);
        Messenger.Broadcast(Signals.DAY_ENDED_2);
        while (pauseDayEnded2) {
            yield return null;
        }
        Messenger.Broadcast(Signals.UPDATE_UI);

        this.tick += 1;
        if (this.tick > ticksPerDay) {
            this.tick = 1;
            this.days += 1;
            this.continuousDays += 1;
            if (days > daysPerMonth) {
                this.days = 1;
                this.month += 1;
                if (this.month > 12) {
                    this.month = 1;
                    this.year += 1;
                }
                Messenger.Broadcast(Signals.MONTH_START);
            }
        }
    }
    public static string ConvertTickToTime(int tick) {
        float floatConversion = tick * 0.25f;
        int hour = (int) floatConversion;
        int minutes = (int)(((floatConversion - hour) * 4) * 15);
        string timeOfDay = "AM";
        if(hour >= 12) {
            if(hour < 24) {
                timeOfDay = "PM";
            }
            hour -= 12;
        }
        if(hour == 0) {
            hour = 12;
        }
        return hour + ":" + minutes.ToString("D2") + " " + timeOfDay;
    }
    public static TIME_IN_WORDS GetTimeInWordsOfTick(int tick) {
        float time = tick / (float)ticksPerTimeInWords;
        int intTime = (int) time;
        if(time == intTime && intTime > 0) {
            //This will make sure that the 12th tick is still part of the previous time in words
            //Example: In ticks 1 - 11, the intTime is 0 (AFTER_MIDNIGHT_1), however, in tick 12, intTime is already 1 (AFTER_MIDNIGHT_2), but we still want it to be part of AFTER_MIDNIGHT_1
            //Hence, this checker ensures that tick 12's intTime is 0
            intTime -= 1;
        }
        return timeInWords[intTime];
    }
    public static TIME_IN_WORDS GetCurrentTimeInWordsOfTick() {
        float time = GameManager.Instance.tick / (float) ticksPerTimeInWords;
        int intTime = (int) time;
        if (time == intTime && intTime > 0) {
            //This will make sure that the 12th tick is still part of the previous time in words
            //Example: In ticks 1 - 11, the intTime is 0 (AFTER_MIDNIGHT_1), however, in tick 12, intTime is already 1 (AFTER_MIDNIGHT_2), but we still want it to be part of AFTER_MIDNIGHT_1
            //Hence, this checker ensures that tick 12's intTime is 0
            intTime -= 1;
        }
        return timeInWords[intTime];
    }
    #region Cursor
    public void SetCursorToDefault() {
        Cursor.SetCursor(defaultCursorTexture, hotSpot, cursorMode);
    }
    public void SetCursorToTarget() {
        Cursor.SetCursor(targetCursorTexture, hotSpot, cursorMode);
    }
    public void SetCursorToDrag() {
        Cursor.SetCursor(dragWorldCursorTexture, new Vector2(16f, 16f), cursorMode);
    }
    #endregion

    #region For Testing
    [ContextMenu("Print Event Table")]
    public void PrintEventTable() {
        Messenger.PrintEventTable();
    }
    #endregion

    #region Utilities
    private void LogCallback(string condition, string stackTrace, LogType type) {
        //CharacterManager.Instance.CategorizeLog(condition, stackTrace, type);
        if (type == LogType.Error || type == LogType.Exception) {
            string notification = "<color=\"red\">" + TodayLogString() + "Error occurred! Check console for log message</color>";
            Messenger.Broadcast<string, int, UnityAction>(Signals.SHOW_NOTIFICATION, notification, 100, null);
            UIManager.Instance.Pause();
        }
    }
    public void ToggleCharactersVisibility(bool state) {
        allCharactersAreVisible = state;
        Messenger.Broadcast(Signals.TOGGLE_CHARACTERS_VISIBILITY);
    }
    public void ToggleInspectAll(bool state) {
        inspectAll = state;
        Messenger.Broadcast(Signals.INSPECT_ALL);
    }
    public void ToggleIgnoreEventTriggerWeights(bool state) {
        ignoreEventTriggerWeights = state;
    }
    #endregion
}
