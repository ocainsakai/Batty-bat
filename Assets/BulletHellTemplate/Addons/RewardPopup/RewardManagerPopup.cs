using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Central in-session tracker for "first completion" map rewards.
    /// 
    /// Responsibilities:
    /// - Track which maps were *completed this session* (pending reward).
    /// - Track which map rewards are *already claimed* (persistent via PlayerSave).
    /// - Merge server-authoritative claimed list on startup.
    /// - Provide pending list for UIRewardManagerPopup.
    /// - Mark maps claimed after successful server claim.
    /// 
    /// No per-map PlayerPrefs keys are used (legacy removed).
    /// Claimed persistence lives in PlayerSave.SaveClaimedMapRewards().
    /// Pending completions are session-only (lost on app restart if not claimed).
    /// </summary>
    public sealed class RewardManagerPopup : MonoBehaviour
    {
        public static RewardManagerPopup Singleton;

        // Maps completed this session but not yet claimed.
        private readonly HashSet<int> _completedSession = new();

        private List<int> _claimed = new();
        private bool _claimedLoaded = false;

        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Merge server claimed mapIds (from /auth/init) into local persistent list,
        /// then show pending rewards (if any).
        /// </summary>
        public void StartSyncFromServer(List<int> serverClaimedMapIds)
        {
            MergeServerClaimed(serverClaimedMapIds);
            StartCoroutine(WaitAndShowRewards());
        }

        /// <summary>
        /// Re-show pending rewards using current local state (no merge).
        /// </summary>
        public void StartSyncFromServer() => StartCoroutine(WaitAndShowRewards());

        /// <summary>
        /// Call when the player successfully completes a map.
        /// If that map is rewardable and not already claimed, it becomes pending for this session.
        /// </summary>
        public void RegisterMapCompleted(int mapId)
        {
            var mapData = GetMapDataById(mapId);
            if (mapData == null)
            {
                Debug.LogError($"[RewardManagerPopup] Map data not found (mapId:{mapId}).");
                return;
            }

            if (!ShouldReward(mapData))
                return; // not rewardable; ignore

            if (HasLocalMapClaimed(mapId))
                return; // already claimed; ignore

            if (_completedSession.Add(mapId))
            {
                Debug.Log($"[RewardManagerPopup] Map {mapId} marked completed (pending reward).");
            }
        }

        /// <summary>
        /// True if the specified map reward is already claimed (local persisted state).
        /// </summary>
        public bool HasLocalMapClaimed(int mapId)
        {
            EnsureClaimedLoaded();
            return _claimed.Contains(mapId);
        }

        /// <summary>
        /// Maps completed this session and not yet claimed.
        /// </summary>
        public List<int> GetLocalUnclaimedMaps()
        {
            EnsureClaimedLoaded();
            var list = new List<int>();
            foreach (var id in _completedSession)
            {
                if (!_claimed.Contains(id))
                    list.Add(id);
            }
            return list;
        }

        /// <summary>
        /// Mark a map's reward as successfully claimed.
        /// Removes from session pending and persists in PlayerSave.
        /// </summary>
        public void MarkRewardClaimed(int mapId)
        {
            EnsureClaimedLoaded();
            if (!_claimed.Contains(mapId))
            {
                _claimed.Add(mapId);
                PlayerSave.SaveClaimedMapRewards(_claimed);
            }
            _completedSession.Remove(mapId);
        }

        /// <summary>
        /// Merge server-authoritative claimed list into local persisted claimed list (union).
        /// </summary>
        private void MergeServerClaimed(List<int> serverClaimed)
        {
            EnsureClaimedLoaded();

            if (serverClaimed != null && serverClaimed.Count > 0)
            {
                bool changed = false;
                foreach (var id in serverClaimed)
                {
                    if (!_claimed.Contains(id))
                    {
                        _claimed.Add(id);
                        changed = true;
                    }
                    _completedSession.Remove(id);
                }
                if (changed)
                    PlayerSave.SaveClaimedMapRewards(_claimed);
            }
        }

        /// <summary>
        /// Ensure claimed cache is loaded from PlayerSave once.
        /// </summary>
        private void EnsureClaimedLoaded()
        {
            if (_claimedLoaded) return;
            _claimed = PlayerSave.LoadClaimedMapRewards() ?? new List<int>();
            _claimedLoaded = true;
        }

        /// <summary>
        /// Wait until UIRewardManagerPopup exists; then signal it to show pending rewards.
        /// </summary>
        private IEnumerator WaitAndShowRewards()
        {
            while (UIRewardManagerPopup.Singleton == null)
                yield return null;

            yield return null; // one frame for UI init
            UIRewardManagerPopup.Singleton.ShowPendingRewards();
        }

        /// <summary>
        /// Lookup map definition from GameInstance.
        /// </summary>
        private MapInfoData GetMapDataById(int mapId)
        {
            var maps = GameInstance.Singleton?.mapInfoData;
            if (maps == null) return null;
            return Array.Find(maps, m => m != null && m.mapId == mapId);
        }

        /// <summary>
        /// True if the map is configured to award a first-time completion reward and has reward content.
        /// </summary>
        private bool ShouldReward(MapInfoData mapData)
        {
            if (mapData == null) return false;
            bool hasList = mapData.WinMapRewards != null && mapData.WinMapRewards.Count > 0;
            bool hasSpecial = mapData.rewardType != MapRewardType.None;
            return mapData.isRewardOnCompleteFirstTime && (hasList || hasSpecial);
        }
    }
}
