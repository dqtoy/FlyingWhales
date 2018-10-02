﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;

	public static int[] daysInMonth = {0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
    public static string[] daysInWords = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
	public int month;
	public int days;
	public int year;
    public int hour;
    public int continuousDays;
    public const int hoursPerDay = 144;

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

    private const float X1_SPEED = 0.75f;
    private const float X2_SPEED = 0.5f;
    private const float X4_SPEED = 0.25f;

    private float timeElapsed;
    private bool _gameHasStarted;

    [SerializeField] private Texture2D defaultCursorTexture;
    [SerializeField] private Texture2D targetCursorTexture;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

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
        if (_gameHasStarted && !isPaused) {
            if (this.timeElapsed == 0f) {
                this.HourStarted();
            }
            this.timeElapsed += Time.deltaTime * 1f;
            if (this.timeElapsed >= this.progressionSpeed) {
                this.timeElapsed = 0f;
                this.HourEnded();
            }
        }
    }
    private void Update() {
        if (!UIManager.Instance.IsConsoleShowing() && !PlayerManager.Instance.isChoosingStartingTile) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (isPaused) {
                    SetProgressionSpeed(currProgressionSpeed);
                    SetPausedState(false);
                } else {
                    //pause
                    SetPausedState(true);
                }
            }
        }
    }
    #endregion


    [ContextMenu("Start Progression")]
	public void StartProgression(){
        _gameHasStarted = true;
        //SetPausedState(false);
        UIManager.Instance.SetProgressionSpeed1X();
		SchedulingManager.Instance.StartScheduleCalls ();
	}

    public GameDate Today() {
        return new GameDate(this.month, this.days, this.year, this.hour);
    }
    public string TodayLogString() {
        return "[" + new GameDate(this.month, this.days, this.year, this.hour).GetDayAndTicksString() + "] ";
    }
    public GameDate EndOfTheMonth() {
        return new GameDate(this.month, daysInMonth[this.month], this.year, hoursPerDay);
    }
	public GameDate FirstDayOfTheMonth() {
		return new GameDate(this.month, 1, this.year, 1);
	}
    public void SetPausedState(bool isPaused){
        //Debug.Log("Set paused state to " + isPaused);
		this.isPaused = isPaused;
        Messenger.Broadcast(Signals.PAUSED, isPaused);
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
        ECS.CombatManager.Instance.updateIntervals = this.progressionSpeed / (float) ECS.CombatManager.Instance.numOfCombatActionPerDay;
        Messenger.Broadcast(Signals.PROGRESSION_SPEED_CHANGED, progSpeed);
	}
    public void HourStarted() {
        Messenger.Broadcast(Signals.HOUR_STARTED);
        Messenger.Broadcast(Signals.UPDATE_UI);
    }
    /*
     * Function that triggers daily actions
     * */
    public void HourEnded(){
        Messenger.Broadcast(Signals.HOUR_ENDED);
        Messenger.Broadcast(Signals.UPDATE_UI);

        this.hour += 1;
        if(this.hour > hoursPerDay) {
            this.hour = 1;
            this.days += 1;
            Messenger.Broadcast(Signals.DAY_START);
            this.continuousDays += 1;
            if (days > daysInMonth[this.month]) {
                this.days = 1;
                this.month += 1;
                if (this.month > 12) {
                    this.month = 1;
                    this.year += 1;
                }
            }
        }
	}

    #region Cursor
    public void SetCursorToDefault() {
        Cursor.SetCursor(defaultCursorTexture, hotSpot, cursorMode);
    }
    public void SetCursorToTarget() {
        Cursor.SetCursor(targetCursorTexture, hotSpot, cursorMode);
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
        CharacterManager.Instance.CategorizeLog(condition, stackTrace, type);
    }
    public void ToggleCharactersVisibility(bool state) {
        allCharactersAreVisible = state;
        Messenger.Broadcast(Signals.TOGGLE_CHARACTERS_VISIBILITY);
    }
    public void ToggleInspectAll(bool state) {
        inspectAll = state;
        Messenger.Broadcast(Signals.INSPECT_ALL);
    }
    #endregion
}
