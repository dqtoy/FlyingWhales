using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;

    public static string[] daysInWords = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
    public static TIME_IN_WORDS[] timeInWords = new TIME_IN_WORDS[] {
        TIME_IN_WORDS.AFTER_MIDNIGHT
        , TIME_IN_WORDS.MORNING
        , TIME_IN_WORDS.AFTERNOON
        , TIME_IN_WORDS.EARLY_NIGHT
        , TIME_IN_WORDS.LATE_NIGHT };


    public int month;
    public static int days;
	public int year;
    public int tick;
    public int continuousDays;
    public const int daysPerMonth = 30;
    public const int ticksPerDay = 288;
    public const int ticksPerHour = 12;
    public const int ticksPerTimeInWords = 36;

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
    public static bool showAllTilesTooltip = false;
    
    public GameObject travelLineParentPrefab;
    public GameObject travelLinePrefab;
    public HexTile tile1;
    public HexTile tile2;

    [Header("Particle Effects")]
    [SerializeField] private GameObject electricEffectPrefab;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject fireEffectPrefab;
    [SerializeField] private GameObject aoeParticlesPrefab;
    [SerializeField] private GameObject aoeParticlesAutoDestroyPrefab;
    [SerializeField] private GameObject burningEffectPrefab;

    private const float X1_SPEED = 0.75f;
    private const float X2_SPEED = 0.5f;
    private const float X4_SPEED = 0.25f;

    private float timeElapsed;
    private bool _gameHasStarted;

    //[SerializeField] private Texture2D defaultCursorTexture;
    //[SerializeField] private Texture2D targetCursorTexture;
    //[SerializeField] private Texture2D dragWorldCursorTexture;
    //[SerializeField] private Texture2D dragItemHoverCursorTexture;
    //[SerializeField] private Texture2D dragItemClickedCursorTexture;
    //[SerializeField] private CursorMode cursorMode = CursorMode.Auto;
    //[SerializeField] private Vector2 hotSpot = Vector2.zero;

    public bool pauseTickEnded2 = false;
    //public int ticksToAddPerTick { get; private set; } //how many ticks to add per tick of progression? Set this to another value to advance ticks by more than 1. 
    //public bool isDraggingItem = false;

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
        //SetTicksToAddPerTick(1); //ticksPerHour //so that at start of the game time will advance by one hour per tick.
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
#if !WORLD_CREATION_TOOL
        //Application.logMessageReceived += LogCallback;
#endif
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            if (allowConsole) {
                UIManager.Instance.ToggleConsole();
            }
        } else if (Input.GetKeyDown(KeyCode.Space) && !UIManager.Instance.IsMouseOnInput()) {
            if (UIManager.Instance.pauseBtn.IsInteractable()) {
                if (isPaused) {
                    UIManager.Instance.Unpause();
                } else {
                    UIManager.Instance.Pause();
                }
            }
        } else if (Input.GetKeyDown(KeyCode.F1)) {
            if (UIManager.Instance.x1Btn.IsInteractable()) {
                UIManager.Instance.SetProgressionSpeed1X();
            }
        } else if (Input.GetKeyDown(KeyCode.F2)) {
            if (UIManager.Instance.x2Btn.IsInteractable()) {
                UIManager.Instance.SetProgressionSpeed2X();
            }
        } else if (Input.GetKeyDown(KeyCode.F3)) {
            if (UIManager.Instance.x4Btn.IsInteractable()) {
                UIManager.Instance.SetProgressionSpeed4X();
            }
        } else if (Input.GetKeyDown(KeyCode.Escape)) {
            Messenger.Broadcast(Signals.KEY_DOWN, KeyCode.Escape);
        }
    }
    private void FixedUpdate() {
        if (_gameHasStarted && !isPaused) {
            if (this.timeElapsed == 0f) {
                this.TickStarted();
            }
            this.timeElapsed += Time.deltaTime;
            if (this.timeElapsed >= this.progressionSpeed) {
                this.timeElapsed = 0f;
                TickEnded();
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
        days = 1;
        //SetPausedState(false);
        UIManager.Instance.SetProgressionSpeed1X();
        UIManager.Instance.Pause();
		SchedulingManager.Instance.StartScheduleCalls ();
        Messenger.Broadcast(Signals.MONTH_START); //for the first day
        TimerHubUI.Instance.AddItem("Until Divine Intervention", 4320, null);
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
        //go.transform.SetParent(tile1.UIParent);
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
        return new GameDate(this.month, days, this.year, this.tick);
    }
    public string TodayLogString() {
        return "[" + continuousDays + " - " + ConvertTickToTime(tick) + "] ";
    }
    public string ConvertDayToLogString(GameDate date) {
        return "[" + date.ConvertToContinuousDaysWithTime() + "] ";
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

        int daysDifference = (daysDiff + (yearDiff * 365)) * daysPerMonth;
        int tickDifference = fromDate.tick - toDate.tick;
        if (daysDifference > 0) {
            tickDifference = daysDifference * ticksPerDay;
            tickDifference = (tickDifference - toDate.tick) + fromDate.tick;
        }
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
    public void SetDelayedPausedState(bool state) {
        StartCoroutine(DelayedPausedState(state));
    }
    private IEnumerator DelayedPausedState(bool state) {
        yield return null;
        SetPausedState(state);
    }
    /*
     * Set day progression speed to 1x, 2x of 4x
     * */
	public void SetProgressionSpeed(PROGRESSION_SPEED progSpeed){
        if (!isPaused && currProgressionSpeed == progSpeed) {
            return; //ignore change
        }
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
    public void TickStarted() {
        if (tick % ticksPerHour == 0 && !IsStartOfGame()) {
            //hour reached
            Messenger.Broadcast(Signals.HOUR_STARTED);
        }
        Messenger.Broadcast(Signals.TICK_STARTED);
        Messenger.Broadcast(Signals.UPDATE_UI);
    }
    /*
     * Function that triggers daily actions
     * */
    public void TickEnded(){
        Messenger.Broadcast(Signals.TICK_ENDED);
        Messenger.Broadcast(Signals.UPDATE_UI);

        this.tick += 1;
        if (this.tick > ticksPerDay) {
            //int difference = this.tick - ticksPerDay; //Added this for cases when the ticks to be added per tick is greater than 1, so it is possible that the excess ticks over ticksPerDay can also be greater than 1
            this.tick = 1;
            DayStarted(false);
        }
    }
    public void SetTick(int amount) {
        this.tick = amount;
        Messenger.Broadcast(Signals.UPDATE_UI);
    }
    public void DayStarted(bool broadcastUI = true) {
        if (broadcastUI) {
            Messenger.Broadcast(Signals.UPDATE_UI);
        }
        days += 1;
        this.continuousDays += 1;
        Messenger.Broadcast(Signals.DAY_STARTED);
        if (days > daysPerMonth) {
            days = 1;
            this.month += 1;
            if (this.month > 12) {
                this.month = 1;
                this.year += 1;
            }
            Messenger.Broadcast(Signals.MONTH_START);
        }
    }
    public static string ConvertTickToTime(int tick) {
        float floatConversion = tick / (float) ticksPerHour;
        int hour = (int) floatConversion;
        int minutes = Mathf.RoundToInt(((floatConversion - hour) * 12) * 5);
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
        if (tick >= 1 && GameManager.Instance.tick <= 72) {
            return timeInWords[0];
        } else if (tick >= 73 && GameManager.Instance.tick <= 144) {
            return timeInWords[1];
        } else if (tick >= 145 && tick <= 204) {
            return timeInWords[2];
        } else if (tick >= 205 && tick <= 264) {
            return timeInWords[3];
        } else if (tick >= 265 && tick <= 288) {
            return timeInWords[4];
        }
        return TIME_IN_WORDS.NONE;
    }
    public static TIME_IN_WORDS GetCurrentTimeInWordsOfTick() {
        if(GameManager.Instance.tick >= 1 && GameManager.Instance.tick <= 72) {
            return timeInWords[0];
        } else if (GameManager.Instance.tick >= 73 && GameManager.Instance.tick <= 144) {
            return timeInWords[1];
        } else if (GameManager.Instance.tick >= 145 && GameManager.Instance.tick <= 204) {
            return timeInWords[2];
        } else if (GameManager.Instance.tick >= 205 && GameManager.Instance.tick <= 264) {
            return timeInWords[3];
        } else if (GameManager.Instance.tick >= 265 && GameManager.Instance.tick <= 288) {
            return timeInWords[4];
        }
        throw new System.Exception(GameManager.Instance.tick + " tick has no time in words!");
        //float time = GameManager.Instance.tick / (float) ticksPerTimeInWords;
        //int intTime = (int) time;
        //if (time == intTime && intTime > 0) {
        //    //This will make sure that the 12th tick is still part of the previous time in words
        //    //Example: In ticks 1 - 11, the intTime is 0 (AFTER_MIDNIGHT_1), however, in tick 12, intTime is already 1 (AFTER_MIDNIGHT_2), but we still want it to be part of AFTER_MIDNIGHT_1
        //    //Hence, this checker ensures that tick 12's intTime is 0
        //    intTime -= 1;
        //}
        //return timeInWords[intTime];
    }
    public static int GetRandomTickFromTimeInWords(TIME_IN_WORDS timeInWords) {
        if (timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            return UnityEngine.Random.Range(1, 73);
        } else if (timeInWords == TIME_IN_WORDS.MORNING) {
            return UnityEngine.Random.Range(73, 145);
        } else if (timeInWords == TIME_IN_WORDS.AFTERNOON) {
            return UnityEngine.Random.Range(145, 205);
        } else if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
            return UnityEngine.Random.Range(205, 265);
        } else if (timeInWords == TIME_IN_WORDS.LATE_NIGHT) {
            return UnityEngine.Random.Range(265, 289);
        }
        throw new System.Exception(timeInWords.ToString() + " time in words has no tick!");
    }
    public static int GetRandomTickFromTimeInWords(TIME_IN_WORDS timeInWords, int minimumThreshold) {
        if (timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            return UnityEngine.Random.Range(minimumThreshold, 73);
        } else if (timeInWords == TIME_IN_WORDS.MORNING) {
            return UnityEngine.Random.Range(minimumThreshold, 145);
        } else if (timeInWords == TIME_IN_WORDS.AFTERNOON) {
            return UnityEngine.Random.Range(minimumThreshold, 205);
        } else if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
            return UnityEngine.Random.Range(minimumThreshold, 265);
        } else if (timeInWords == TIME_IN_WORDS.LATE_NIGHT) {
            return UnityEngine.Random.Range(minimumThreshold, 289);
        }
        throw new System.Exception(timeInWords.ToString() + " time in words has no tick!");
    }
    public int GetTicksBasedOnHour(int hours) {
        return ticksPerHour * hours;
    }
    public int GetTicksBasedOnMinutes(int minutes) {
        float percent = (float)minutes/60f;
        return Mathf.FloorToInt(ticksPerHour * percent);
    }
    public int GetHoursBasedOnTicks(int ticks) {
        return ticks / ticksPerHour;
    }
    public int GetCeilingHoursBasedOnTicks(int ticks) {
        return Mathf.CeilToInt(ticks / (float) ticksPerHour);
    }
    public int GetCeilingDaysBasedOnTicks(int ticks) {
        return Mathf.CeilToInt(ticks / (float) ticksPerDay);
    }
    //public void SetTicksToAddPerTick(int amount) {
    //    ticksToAddPerTick = amount;
    //}

    #region Particle Effects
    public void CreateElectricEffectAt(Character character) {
        //StartCoroutine(ElectricEffect(character));
        if (character.marker == null) {
            return;
        }
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(electricEffectPrefab.name, Vector3.zero, Quaternion.identity, character.marker.transform);
        go.transform.localPosition = Vector3.zero;
        go.SetActive(true);
    }
    public void CreateHitEffectAt(Character character) {
        if (character.marker == null) {
            return;
        }
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(hitEffectPrefab.name, Vector3.zero, Quaternion.identity, character.marker.transform);
        go.transform.localPosition = Vector3.zero;
        go.SetActive(true);
    }
    public void CreateFireEffectAt(Character character) {
        if (character.marker == null) {
            return;
        }
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(fireEffectPrefab.name, Vector3.zero, Quaternion.identity, character.marker.transform);
        go.transform.localPosition = Vector3.zero;
        go.SetActive(true);
        //StartCoroutine(FireEffect(character));
    }
    public AOEParticle CreateAOEEffectAt(LocationGridTile tile, int range, bool autoDestroy = false) {
        GameObject go;
        if (autoDestroy) {
            go = ObjectPoolManager.Instance.InstantiateObjectFromPool(aoeParticlesAutoDestroyPrefab.name, Vector3.zero, Quaternion.identity, tile.parentAreaMap.objectsParent);
        } else {
            go = ObjectPoolManager.Instance.InstantiateObjectFromPool(aoeParticlesPrefab.name, Vector3.zero, Quaternion.identity, tile.parentAreaMap.objectsParent);
        }
        AOEParticle particle = go.GetComponent<AOEParticle>();
        particle.PlaceParticleEffect(tile, range, autoDestroy);
        return particle;
    }
    public GameObject CreateBurningEffectAt(LocationGridTile tile) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(burningEffectPrefab.name, Vector3.zero, Quaternion.identity, tile.parentAreaMap.objectsParent);
        go.transform.localPosition = tile.centeredLocalLocation;
        go.SetActive(true);
        return go;
    }
    public GameObject CreateBurningEffectAt(Character character) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(burningEffectPrefab.name, Vector3.zero, Quaternion.identity, character.marker.transform);
        go.transform.localPosition = Vector3.zero;
        go.SetActive(true);
        return go;
    }
    public GameObject CreateBurningEffectAt(TileObject obj) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(burningEffectPrefab.name, Vector3.zero, Quaternion.identity, obj.collisionTrigger.transform);
        go.transform.localPosition = Vector3.zero;
        go.SetActive(true);
        return go;
    }
    public GameObject CreateBurningEffectAt(SpecialToken obj) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(burningEffectPrefab.name, Vector3.zero, Quaternion.identity, obj.collisionTrigger.transform);
        go.transform.localPosition = Vector3.zero;
        go.SetActive(true);
        return go;
    }
    public void CreateExplodeEffectAt(Character character) {
        if (character.marker == null) {
            return;
        }
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(hitEffectPrefab.name, Vector3.zero, Quaternion.identity, character.marker.transform);
        go.transform.localPosition = Vector3.zero;
        go.SetActive(true);
    }
    public void CreateExplodeEffectAt(LocationGridTile tile) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(hitEffectPrefab.name, Vector3.zero, Quaternion.identity, tile.parentAreaMap.objectsParent);
        go.transform.localPosition = tile.centeredLocalLocation;
        go.SetActive(true);
    }
    #endregion

    //#region Cursor
    //public void SetCursorToDefault() {
    //    isDraggingItem = false;
    //    Cursor.SetCursor(defaultCursorTexture, hotSpot, cursorMode);
    //}
    //public void SetCursorToTarget() {
    //    Cursor.SetCursor(targetCursorTexture, hotSpot, cursorMode);
    //}
    //public void SetCursorToDrag() {
    //    Cursor.SetCursor(dragWorldCursorTexture, new Vector2(16f, 16f), cursorMode);
    //}
    //public void SetCursorToItemDragHover() {
    //    isDraggingItem = false;
    //    Cursor.SetCursor(dragItemHoverCursorTexture, hotSpot, cursorMode);
    //}
    //public void SetCursorToItemDragClicked() {
    //    isDraggingItem = true;
    //    Cursor.SetCursor(dragItemClickedCursorTexture, hotSpot, cursorMode);
    //}
    //#endregion

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
            Messenger.Broadcast<string, int, UnityAction>(Signals.SHOW_DEVELOPER_NOTIFICATION, notification, 100, null);
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
    private bool IsStartOfGame() {
        if (year == startYear && month == 1 && days == 1 && tick == 24) {
            return true;
        }
        return false;
    }
    public void SetStartDate(GameDate date) {
        month = date.month;
        days = date.day;
        tick = date.tick;
        year = date.year;
        continuousDays = date.ConvertToContinuousDays();
    }
    #endregion
}
