using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace AC.Foundation
{
    public enum ActionTag
    {
        Gameplay,
        UI,
        VFX,
        Audio,
        Other
    }

    public interface IActionHandle
    {
        void Complete();
        bool IsCompleted { get; }
        int ActionId { get; }
        GameObject Owner { get; }
        ActionTag Category { get; }
    }

    public class ActionHandle : IActionHandle
    {
        public int ActionId { get; private set; }
        public GameObject Owner { get; private set; }
        public ActionTag Category { get; private set; }
        public bool IsCompleted { get; internal set; }

        internal void Initialize(int actionId, GameObject owner, ActionTag category)
        {
            ActionId = actionId;
            Owner = owner;
            Category = category;
            IsCompleted = false;
        }

        internal void Reset()
        {
            ActionId = 0;
            Owner = null;
            Category = ActionTag.Other;
            IsCompleted = false;
        }

        public void Complete()
        {
            ActionTracker.CompleteAction(ActionId);
        }
    }

    public static class ActionTracker
    {
        private class ActionData
        {
            public int ActionID { get; set; }
            public GameObject Owner { get; set; }
            public ActionTag Tag { get; set; }
            public float StartTime { get; set; }
            public float? Duration { get; set; }
            public bool IsCompleted { get; set; }

            public void Reset()
            {
                ActionID = 0;
                Owner = null;
                Tag = ActionTag.Other;
                StartTime = 0;
                Duration = null;
                IsCompleted = false;
            }
        }

        // Thread safety lock
        private static readonly object _lock = new();

        private static int _nextActionId = 1;
        private static readonly Dictionary<int, ActionData> activeActions = new();
        private static readonly Dictionary<GameObject, List<int>> objectActions = new();
        private static readonly Dictionary<int, ActionHandle> handles = new();

        // Signal-based waiting sources
        private static readonly Dictionary<int, UniTaskCompletionSource> completionSources = new();
        private static readonly Dictionary<ActionTag, UniTaskCompletionSource> categorySources = new();
        private static readonly Dictionary<GameObject, UniTaskCompletionSource> objectSources = new();
        private static UniTaskCompletionSource globalSource;

        // Category counters for efficient signal triggering
        private static readonly Dictionary<ActionTag, int> categoryCounters = new();

        // Object pooling
        private static readonly Stack<ActionHandle> handlePool = new();
        private static readonly Stack<ActionData> actionDataPool = new();
        private const int MaxPoolSize = 64;

        // Events
        public static event Action<IActionHandle> OnActionRegistered;
        public static event Action<IActionHandle> OnActionCompleted;

        #region Object Pooling

        private static ActionHandle RentHandle()
        {
            lock (_lock)
            {
                return handlePool.Count > 0 ? handlePool.Pop() : new ActionHandle();
            }
        }

        private static void ReturnHandle(ActionHandle handle)
        {
            if (handle == null) return;
            lock (_lock)
            {
                if (handlePool.Count < MaxPoolSize)
                {
                    handle.Reset();
                    handlePool.Push(handle);
                }
            }
        }

        private static ActionData RentActionData()
        {
            lock (_lock)
            {
                return actionDataPool.Count > 0 ? actionDataPool.Pop() : new ActionData();
            }
        }

        private static void ReturnActionData(ActionData data)
        {
            if (data == null) return;
            lock (_lock)
            {
                if (actionDataPool.Count < MaxPoolSize)
                {
                    data.Reset();
                    actionDataPool.Push(data);
                }
            }
        }

        #endregion

        public static IActionHandle RegisterAction(GameObject owner, ActionTag category, float? duration = null)
        {
            int actionId;
            ActionHandle handle;
            ActionData animData;

            lock (_lock)
            {
                actionId = _nextActionId++;

                animData = RentActionData();
                animData.ActionID = actionId;
                animData.Owner = owner;
                animData.Tag = category;
                animData.StartTime = Time.time;
                animData.Duration = duration;
                animData.IsCompleted = false;

                activeActions[actionId] = animData;

                // Update category counters and signals
                if (!categoryCounters.ContainsKey(category)) categoryCounters[category] = 0;
                categoryCounters[category]++;
                if (categoryCounters[category] == 1) categorySources[category] = new UniTaskCompletionSource();

                // Update global signal
                if (activeActions.Count == 1) globalSource = new UniTaskCompletionSource();

                if (owner != null)
                {
                    if (!objectActions.ContainsKey(owner))
                    {
                        objectActions[owner] = new List<int>();
                        objectSources[owner] = new UniTaskCompletionSource();
                    }
                    objectActions[owner].Add(actionId);
                }

                handle = RentHandle();
                handle.Initialize(actionId, owner, category);
                handles[actionId] = handle;

                // Single action signal
                completionSources[actionId] = new UniTaskCompletionSource();
            }

            OnActionRegistered?.Invoke(handle);

            if (duration.HasValue)
            {
                CompleteActionAfterDelay(actionId, duration.Value).Forget();
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            string ownerName = owner != null ? owner.name : "Null";
            Debug.Log($"[ActionTracker] Action registered: {actionId} ({ownerName}) - Category: {category}");
#endif
            return handle;
        }

        public static void CompleteAction(IActionHandle handle)
        {
            if (handle == null) return;
            CompleteAction(handle.ActionId);
        }

        public static void CompleteAction(int actionId)
        {
            ActionData animData;
            ActionTag category;
            GameObject owner;
            ActionHandle handle = null;
            UniTaskCompletionSource source = null;
            UniTaskCompletionSource catSource = null;
            UniTaskCompletionSource objSource = null;
            UniTaskCompletionSource globalSrc = null;
            bool shouldTriggerGlobal = false;

            lock (_lock)
            {
                if (!activeActions.TryGetValue(actionId, out animData)) return;

                animData.IsCompleted = true;
                category = animData.Tag;
                owner = animData.Owner;

                if (handles.TryGetValue(actionId, out handle))
                {
                    handle.IsCompleted = true;
                    handles.Remove(actionId);
                }

                // 1. Get single action signal
                if (completionSources.TryGetValue(actionId, out source))
                {
                    completionSources.Remove(actionId);
                }

                // 2. Get category signal if last one
                categoryCounters[category]--;
                if (categoryCounters[category] <= 0)
                {
                    if (categorySources.TryGetValue(category, out catSource))
                    {
                        categorySources.Remove(category);
                    }
                }

                // 3. Get object signal if last one
                if (owner != null && objectActions.TryGetValue(owner, out var list))
                {
                    list.Remove(actionId);
                    if (list.Count == 0)
                    {
                        if (objectSources.TryGetValue(owner, out objSource))
                        {
                            objectSources.Remove(owner);
                        }
                        objectActions.Remove(owner);
                    }
                }

                activeActions.Remove(actionId);

                // 4. Get global signal if last one
                if (activeActions.Count == 0)
                {
                    globalSrc = globalSource;
                    globalSource = null;
                    shouldTriggerGlobal = true;
                }

                // Return to pool
                ReturnActionData(animData);
            }

            // Trigger signals outside lock to avoid deadlock
            OnActionCompleted?.Invoke(handle);
            if (handle != null) ReturnHandle(handle);
            
            source?.TrySetResult();
            catSource?.TrySetResult();
            objSource?.TrySetResult();
            if (shouldTriggerGlobal) globalSrc?.TrySetResult();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[ActionTracker] Action completed: {actionId}");
#endif
        }

        public static void CompleteAnimation(int actionId) => CompleteAction(actionId);
        public static void CompleteAnimation(IActionHandle handle) => CompleteAction(handle);

        public static void CompleteAllAnimationsOfObject(GameObject owner)
        {
            if (owner == null) return;

            List<int> ids;
            lock (_lock)
            {
                if (!objectActions.TryGetValue(owner, out var actionIds)) return;
                ids = actionIds.ToList();
            }

            foreach (var id in ids)
            {
                CompleteAction(id);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[ActionTracker] All actions of {owner.name} completed");
#endif
        }

        public static void CompleteAllAnimationsInCategory(ActionTag category)
        {
            List<int> animsToComplete;
            lock (_lock)
            {
                animsToComplete = activeActions
                    .Values
                    .Where(a => a.Tag == category)
                    .Select(a => a.ActionID)
                    .ToList();
            }

            foreach (var id in animsToComplete)
            {
                CompleteAction(id);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[ActionTracker] All actions in category {category} completed");
#endif
        }

        public static UniTask WaitForAnimation(IActionHandle handle, CancellationToken ct = default)
        {
            if (handle == null) return UniTask.CompletedTask;
            return WaitForAction(handle.ActionId, ct);
        }

        public static UniTask WaitForAction(int actionId, CancellationToken ct = default)
        {
            UniTaskCompletionSource source;
            lock (_lock)
            {
                if (!completionSources.TryGetValue(actionId, out source))
                {
                    return UniTask.CompletedTask;
                }
            }

            if (ct == default)
            {
                return source.Task;
            }
            return source.Task.AttachExternalCancellation(ct);
        }

        public static UniTask WaitForAnimation(int actionId, CancellationToken ct = default) => WaitForAction(actionId, ct);

        public static UniTask WaitForObjectAnimations(GameObject owner, CancellationToken ct = default)
        {
            if (owner == null) return UniTask.CompletedTask;

            UniTaskCompletionSource source;
            lock (_lock)
            {
                if (!objectSources.TryGetValue(owner, out source))
                {
                    return UniTask.CompletedTask;
                }
            }

            if (ct == default)
            {
                return source.Task;
            }
            return source.Task.AttachExternalCancellation(ct);
        }

        public static UniTask WaitForAllAnimations(CancellationToken ct = default)
        {
            UniTaskCompletionSource src;
            lock (_lock)
            {
                src = globalSource;
            }

            if (src == null) return UniTask.CompletedTask;

            if (ct == default)
            {
                return src.Task;
            }
            return src.Task.AttachExternalCancellation(ct);
        }

        public static async UniTask WaitForCategory(ActionTag category, CancellationToken ct = default)
        {
            UniTaskCompletionSource source;
            lock (_lock)
            {
                if (!categorySources.TryGetValue(category, out source))
                {
                    return;
                }
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var startTime = DateTime.Now;
#endif
            if (ct == default)
            {
                await source.Task;
            }
            else
            {
                await source.Task.AttachExternalCancellation(ct);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var totalElapsed = (DateTime.Now - startTime).TotalSeconds;
            Debug.Log($"[ActionTracker] All actions in {category} completed (took {totalElapsed:F3}s)");
#endif
        }

        public static bool HasActiveAnimations(GameObject owner)
        {
            if (owner == null) return false;
            lock (_lock)
            {
                return objectActions.TryGetValue(owner, out var list) && list.Count > 0;
            }
        }

        public static bool HasAnyActiveAnimations()
        {
            lock (_lock)
            {
                return activeActions.Count > 0;
            }
        }

        public static int GetActiveAnimationCount(GameObject owner)
        {
            if (owner == null) return 0;
            lock (_lock)
            {
                return objectActions.TryGetValue(owner, out var list) ? list.Count : 0;
            }
        }

        public static List<IActionHandle> GetObjectAnimationHandles(GameObject owner)
        {
            var result = new List<IActionHandle>();
            if (owner == null) return result;

            lock (_lock)
            {
                if (objectActions.TryGetValue(owner, out var actionIds))
                {
                    foreach (var id in actionIds)
                    {
                        if (handles.TryGetValue(id, out var handle))
                        {
                            result.Add(handle);
                        }
                    }
                }
            }
            return result;
        }

        public static void LogActiveAnimations()
        {
            lock (_lock)
            {
                Debug.Log("=== Active Actions ===");
                foreach (var anim in activeActions.Values)
                {
                    string ownerName = anim.Owner != null ? anim.Owner.name : "[DESTROYED]";
                    Debug.Log($"ID: {anim.ActionID} | Owner: {ownerName} | Category: {anim.Tag}");
                }
                Debug.Log($"Total: {activeActions.Count}");
            }
        }

        /// <summary>
        /// Cleans up actions whose owner GameObjects have been destroyed.
        /// Call this periodically or when you suspect orphaned actions exist.
        /// </summary>
        public static void CleanupDestroyedOwners()
        {
            List<int> actionsToComplete = new();

            lock (_lock)
            {
                foreach (var kvp in activeActions)
                {
                    // Check if owner was destroyed (null check in Unity returns true for destroyed objects)
                    if (kvp.Value.Owner != null && kvp.Value.Owner == null)
                    {
                        actionsToComplete.Add(kvp.Key);
                    }
                }

                // Also check objectActions for destroyed keys
                var destroyedOwners = objectActions.Keys.Where(go => go == null).ToList();
                foreach (var owner in destroyedOwners)
                {
                    if (objectActions.TryGetValue(owner, out var ids))
                    {
                        actionsToComplete.AddRange(ids);
                    }
                }
            }

            foreach (var id in actionsToComplete.Distinct())
            {
                CompleteAction(id);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (actionsToComplete.Count > 0)
            {
                Debug.Log($"[ActionTracker] Cleaned up {actionsToComplete.Count} actions with destroyed owners");
            }
#endif
        }

        /// <summary>
        /// Starts automatic cleanup of destroyed owners at specified interval.
        /// </summary>
        public static void StartAutoCleanup(float intervalSeconds = 5f, CancellationToken ct = default)
        {
            AutoCleanupLoop(intervalSeconds, ct).Forget();
        }

        private static async UniTaskVoid AutoCleanupLoop(float intervalSeconds, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.WaitForSeconds(intervalSeconds, cancellationToken: ct);
                CleanupDestroyedOwners();
            }
        }

        private static async UniTaskVoid CompleteActionAfterDelay(int actionId, float delay)
        {
            await UniTask.WaitForSeconds(delay);
            CompleteAction(actionId);
        }

        public static void Clear()
        {
            lock (_lock)
            {
                // Return all handles and action data to pool before clearing
                foreach (var handle in handles.Values)
                {
                    ReturnHandle(handle);
                }
                foreach (var data in activeActions.Values)
                {
                    ReturnActionData(data);
                }

                activeActions.Clear();
                objectActions.Clear();
                handles.Clear();
                completionSources.Clear();
                categorySources.Clear();
                objectSources.Clear();
                categoryCounters.Clear();
                globalSource = null;
                _nextActionId = 1;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[ActionTracker] Cleared and cleaned up");
#endif
        }

        /// <summary>
        /// Clears the object pools. Call this if you want to free memory.
        /// </summary>
        public static void ClearPools()
        {
            lock (_lock)
            {
                handlePool.Clear();
                actionDataPool.Clear();
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[ActionTracker] Object pools cleared");
#endif
        }
    }
}
