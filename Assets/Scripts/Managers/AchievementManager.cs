using Steamworks;
using System.Collections;
using UnityEngine;

public class AchievementManager : MonoBehaviour {

    public static AchievementManager Instance = null;

    private enum Achievement : int {
        FIRST_SNATCH,
        FIRST_INTERACTION,
        SNATCH_5,
    };

    private SteamAchievement[] m_Achievements = new SteamAchievement[] {
        new SteamAchievement(Achievement.FIRST_SNATCH, "", ""),
        new SteamAchievement(Achievement.FIRST_INTERACTION, "", ""),
        new SteamAchievement(Achievement.SNATCH_5, "", ""),
    };

    private CGameID _gameID;

    protected Steamworks.Callback<UserStatsReceived_t> onUserStatsRecieved;
    protected Steamworks.Callback<UserStatsStored_t> onUserStatsStored;
    protected Steamworks.Callback<UserAchievementStored_t> onUserAchievementStored;

    // Did we get the stats from Steam?
    private bool statsRequested;
    private bool statsValid;

    // Should we store stats this frame?
    private bool shouldStoreStats;

    #region Monobehaviours
    private void OnEnable() {
        if (!SteamManager.Initialized)
            return;

        // Cache the GameID for use in the Callbacks
        _gameID = new CGameID(SteamUtils.GetAppID());

        onUserStatsRecieved = Steamworks.Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        onUserStatsStored = Steamworks.Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        onUserAchievementStored = Steamworks.Callback<UserAchievementStored_t>.Create(OnAchievementStored);

        // These need to be reset to get the stats upon an Assembly reload in the Editor.
        statsRequested = false;
        statsValid = false;
    }
    private void Awake() {
        Instance = this;
    }
    private void Update() {
        if (!SteamManager.Initialized)
            return;

        if (!statsRequested) {
            // Is Steam Loaded? if no, can't get stats, done
            if (!SteamManager.Initialized) {
                statsRequested = true;
                return;
            }

            // If yes, request our stats
            bool bSuccess = SteamUserStats.RequestCurrentStats();

            // This function should only return false if we weren't logged in, and we already checked that.
            // But handle it being false again anyway, just ask again later.
            statsRequested = bSuccess;
        }

        if (!statsValid)
            return;

        ////Store stats in the Steam database if necessary
        //if (shouldStoreStats) {
        //    // already set any achievements in UnlockAchievement

        //    // set stats
        //    SteamUserStats.SetStat("NumGames", m_nTotalGamesPlayed);
        //    SteamUserStats.SetStat("NumWins", m_nTotalNumWins);
        //    SteamUserStats.SetStat("NumLosses", m_nTotalNumLosses);
        //    SteamUserStats.SetStat("FeetTraveled", m_flTotalFeetTraveled);
        //    SteamUserStats.SetStat("MaxFeetTraveled", m_flMaxFeetTraveled);
        //    // Update average feet / second stat
        //    SteamUserStats.UpdateAvgRateStat("AverageSpeed", m_flGameFeetTraveled, m_flGameDurationSeconds);
        //    // The averaged result is calculated for us
        //    SteamUserStats.GetStat("AverageSpeed", out m_flAverageSpeed);

        //    bool bSuccess = SteamUserStats.StoreStats();
        //    // If this failed, we never sent anything to the server, try
        //    // again later.
        //    shouldStoreStats = !bSuccess;
        //}
    }
    #endregion

    private void Initialize() {

    }

    #region Callbacks
    private void OnUserStatsReceived(UserStatsReceived_t val) {
        if (!SteamManager.Initialized)
            return;

        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)_gameID == val.m_nGameID) {
            if (EResult.k_EResultOK == val.m_eResult) {
                Debug.Log("Received stats and achievements from Steam\n");

                statsValid = true;

                // load achievements
                foreach (SteamAchievement ach in m_Achievements) {
                    bool ret = SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
                    if (ret) {
                        ach.m_strName = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "name");
                        ach.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "desc");
                    } else {
                        Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.m_eAchievementID + "\nIs it registered in the Steam Partner site?");
                    }
                }

                //// load stats
                //SteamUserStats.GetStat("NumGames", out m_nTotalGamesPlayed);
                //SteamUserStats.GetStat("NumWins", out m_nTotalNumWins);
                //SteamUserStats.GetStat("NumLosses", out m_nTotalNumLosses);
                //SteamUserStats.GetStat("FeetTraveled", out m_flTotalFeetTraveled);
                //SteamUserStats.GetStat("MaxFeetTraveled", out m_flMaxFeetTraveled);
                //SteamUserStats.GetStat("AverageSpeed", out m_flAverageSpeed);
            } else {
                Debug.Log("RequestStats - failed, " + val.m_eResult);
            }
        }
    }
    private void OnUserStatsStored(UserStatsStored_t val) {

    }
    private void OnAchievementStored(UserAchievementStored_t val) {

    }
    #endregion

    private void UnlockAchievement(SteamAchievement achievement) {
        achievement.m_bAchieved = true;

        // the icon may change once it's unlocked
        //achievement.m_iIconImage = 0;

        // mark it down
        SteamUserStats.SetAchievement(achievement.m_eAchievementID.ToString());

        // Store stats end of frame
        shouldStoreStats = true;
    }

    #region Handlers

    #endregion

    private class SteamAchievement {
        public Achievement m_eAchievementID;
        public string m_strName;
        public string m_strDescription;
        public bool m_bAchieved;

        /// <summary>
        /// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid
        /// </summary>
        /// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
        /// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
        /// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
        public SteamAchievement(Achievement achievementID, string name, string desc) {
            m_eAchievementID = achievementID;
            m_strName = name;
            m_strDescription = desc;
            m_bAchieved = false;
        }
    }
}

