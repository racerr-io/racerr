// Copyright (c) 2015 - 2019 Doozy Entertainment / Marlink Trading SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Engine.Nody.Attributes;
using Doozy.Engine.Nody.Connections;
using Doozy.Engine.Nody.Models;
using Doozy.Engine.SceneManagement;
using Doozy.Engine.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Doozy.Engine.UI.Nodes
{
    /// <summary>
    ///     It waits for a set Time duration or for a Game Event (string value) or for a Scene to load or for a Scene to unload or for the active scene to change.
    ///     Then it activates the next node in the Graph.
    ///     <para />
    ///     The next node in the Graph is the one connected to this node’s output socket.
    /// </summary>
    [NodeMenu(MenuUtils.WaitNode_CreateNodeMenu_Name, MenuUtils.WaitNode_CreateNodeMenu_Order)]
    public class WaitNode : Node
    {
#if UNITY_EDITOR
        public override bool HasErrors { get { return base.HasErrors || ErrorNoGameEvent || ErrorNoSceneName || ErrorBadBuildIndex; } }
        public bool ErrorNoGameEvent, ErrorNoSceneName, ErrorBadBuildIndex;
#endif

        private const WaitType DEFAULT_WAIT_TYPE = WaitType.Time;
        private const bool DEFAULT_ANY_VALUE = false;
        private const bool DEFAULT_IGNORE_UNITY_TIMESCALE = true;
        private const bool DEFAULT_RANDOM_DURATION = false;
        private const float DEFAULT_DURATION = 1f;
        private const float DEFAULT_DURATION_MAX = 1f;
        private const float DEFAULT_DURATION_MIN = 0f;
        private const string DEFAULT_GAME_EVENT = "";

        public enum WaitType
        {
            Time,
            GameEvent,
            SceneLoad,
            SceneUnload,
            ActiveSceneChange
        }

        public GetSceneBy GetSceneBy;
        public WaitType WaitFor = DEFAULT_WAIT_TYPE;
        public bool AnyValue = DEFAULT_ANY_VALUE;
        public bool IgnoreUnityTimescale = DEFAULT_IGNORE_UNITY_TIMESCALE;
        public bool RandomDuration = DEFAULT_RANDOM_DURATION;
        public float Duration = DEFAULT_DURATION;
        public float DurationMax = DEFAULT_DURATION_MAX;
        public float DurationMin = DEFAULT_DURATION_MIN;
        public int SceneBuildIndex;
        public string GameEvent = DEFAULT_GAME_EVENT;
        public string SceneName;

        [NonSerialized] public float CurrentDuration;
        [NonSerialized] private bool m_timerIsActive;
        [NonSerialized] private double m_timerStart;
        [NonSerialized] private float m_timeDelay;

        public float TimerProgress { get { return Mathf.Clamp01(m_timerIsActive ? (float) (Time.realtimeSinceStartup - m_timerStart) / m_timeDelay : 0f); } }

        public string WaitForInfoTitle
        {
            get
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (WaitFor)
                {
                    case WaitType.Time:
                        return (RandomDuration
                                    ? "[" + DurationMin + " - " + DurationMax + "]"
                                    : Duration + "")
                               + " " + UILabels.Seconds;
                    case WaitType.GameEvent:         return UILabels.GameEvent;
                    case WaitType.SceneLoad:         return UILabels.SceneLoad;
                    case WaitType.SceneUnload:       return UILabels.SceneUnload;
                    case WaitType.ActiveSceneChange: return UILabels.ActiveSceneChange;
                }

                return "---";
            }
        }

        public string WaitForInfoDescription
        {
            get
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (WaitFor)
                {
                    case WaitType.Time: return "";
                    case WaitType.GameEvent:
                        return AnyValue
                                   ? UILabels.AnyGameEvent
                                   : string.IsNullOrEmpty(GameEvent)
                                       ? "---"
                                       : GameEvent;
                    case WaitType.SceneLoad:
                    case WaitType.SceneUnload:
                    case WaitType.ActiveSceneChange:
                        if (AnyValue) return UILabels.AnyScene;
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (GetSceneBy)
                        {
                            case GetSceneBy.Name:
                                return UILabels.Scene + ": " + (string.IsNullOrEmpty(SceneName)
                                                                    ? "---"
                                                                    : SceneName);
                            case GetSceneBy.BuildIndex:
                                return UILabels.BuildIndex + ": " + SceneBuildIndex;
                        }

                        break;
                }

                return "---";
            }
        }

        public override void OnCreate()
        {
            base.OnCreate();
            CanBeDeleted = true;
            SetNodeType(NodeType.General);
            SetName(UILabels.WaitNodeName);
            SetAllowDuplicateNodeName(true);
        }

        public override void AddDefaultSockets()
        {
            base.AddDefaultSockets();
            AddInputSocket(ConnectionMode.Multiple, typeof(PassthroughConnection), false, false);
            AddOutputSocket(ConnectionMode.Override, typeof(PassthroughConnection), false, false);
        }

        public override void CopyNode(Node original)
        {
            base.CopyNode(original);
            var node = (WaitNode) original;
            GetSceneBy = node.GetSceneBy;
            WaitFor = node.WaitFor;
            AnyValue = node.AnyValue;
            IgnoreUnityTimescale = node.IgnoreUnityTimescale;
            RandomDuration = node.RandomDuration;
            Duration = node.Duration;
            DurationMax = node.DurationMax;
            DurationMin = node.DurationMin;
            SceneBuildIndex = node.SceneBuildIndex;
            GameEvent = node.GameEvent;
            SceneName = node.SceneName;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (WaitFor == WaitType.Time) UpdateCurrentDuration();
        }

        public override void OnEnter(Node previousActiveNode, Connection connection)
        {
            base.OnEnter(previousActiveNode, connection);
            if (ActiveGraph == null) return;
            StartWait();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!m_timerIsActive) return;
            if (TimerProgress < 1) return;
            m_timerIsActive = false;
            m_timerStart = Time.realtimeSinceStartup;
            ContinueToNextNode();
        }

        public override void OnExit(Node nextActiveNode, Connection connection)
        {
            base.OnExit(nextActiveNode, connection);
            EndWait();
        }

        private void UpdateCurrentDuration()
        {
            CurrentDuration = RandomDuration ? Random.Range(DurationMin, DurationMax) : Duration;
            CurrentDuration = (float) Math.Round(CurrentDuration, 2);
        }

        private void StartWait()
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (WaitFor)
            {
                case WaitType.Time:
                    ActivateTimer();
                    break;
                case WaitType.GameEvent:
                    Message.AddListener<GameEventMessage>(OnGameEventMessage);
                    break;
                case WaitType.SceneLoad:
                    SceneDirector.Instance.OnSceneLoaded.AddListener(SceneLoaded);
                    break;
                case WaitType.SceneUnload:
                    SceneDirector.Instance.OnSceneUnloaded.AddListener(SceneUnloaded);
                    break;
                case WaitType.ActiveSceneChange:
                    SceneDirector.Instance.OnSceneUnloaded.AddListener(SceneUnloaded);
                    break;
            }
        }

        private void EndWait()
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (WaitFor)
            {
                case WaitType.Time:
                    StopTimer();
                    UpdateCurrentDuration();
                    break;
                case WaitType.GameEvent:
                    Message.RemoveListener<GameEventMessage>(OnGameEventMessage);
                    break;
                case WaitType.SceneLoad:
                    SceneDirector.Instance.OnSceneLoaded.RemoveListener(SceneLoaded);
                    break;
                case WaitType.SceneUnload:
                    SceneDirector.Instance.OnSceneUnloaded.RemoveListener(SceneUnloaded);
                    break;
                case WaitType.ActiveSceneChange:
                    SceneDirector.Instance.OnActiveSceneChanged.RemoveListener(ActiveSceneChanged);
                    break;
            }
        }

        private void ActivateTimer()
        {
            m_timerIsActive = true;
            m_timerStart = Time.realtimeSinceStartup;
            m_timeDelay = CurrentDuration;
            UseUpdate = true;
        }

        private void StopTimer()
        {
            m_timerIsActive = false;
            UseUpdate = false;
        }

        private void OnGameEventMessage(GameEventMessage message)
        {
            if (DebugMode) DDebug.Log("GameEvent received: " + message.EventName + " / Listening for: " + GameEvent, this);
            if (AnyValue || GameEvent.Equals(message.EventName))
                ContinueToNextNode();
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (DebugMode) DDebug.Log("Scene Loaded - Scene: " + scene.name + " / LoadSceneMode: " + mode, this);
            if (AnyValue || IsTargetScene(scene))
                ContinueToNextNode();
        }

        private void SceneUnloaded(Scene unloadedScene)
        {
            if (DebugMode) DDebug.Log("Scene Unloaded - Scene: " + unloadedScene.name, this);
            if (AnyValue || IsTargetScene(unloadedScene))
                ContinueToNextNode();
        }

        private void ActiveSceneChanged(Scene current, Scene next)
        {
            if (DebugMode) DDebug.Log("Active Scene Changed - Replaced Scene: " + current.name + " / Next Scene: " + next.name, this);
            if (AnyValue || IsTargetScene(next))
                ContinueToNextNode();
        }

        private bool IsTargetScene(Scene scene)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (GetSceneBy)
            {
                case GetSceneBy.Name:
                    if (scene.name.Equals(SceneName))
                        return true;
                    break;
                case GetSceneBy.BuildIndex:
                    if (!scene.name.Equals(SceneManager.GetSceneByBuildIndex(SceneBuildIndex).name))
                        return true;
                    break;
            }

            return false;
        }

        private void ContinueToNextNode()
        {
            if (!FirstOutputSocket.IsConnected) return;
            ActiveGraph.SetActiveNodeByConnection(FirstOutputSocket.FirstConnection);
        }

        public override void CheckForErrors()
        {
            base.CheckForErrors();
#if UNITY_EDITOR
            ErrorNoSceneName = false;
            ErrorBadBuildIndex = false;
            ErrorNoGameEvent = false;

            if (AnyValue) return;

            switch (WaitFor)
            {
                case WaitType.GameEvent:
                    ErrorNoGameEvent = string.IsNullOrEmpty(GameEvent.Trim());
                    break;
                case WaitType.SceneLoad:
                case WaitType.SceneUnload:
                case WaitType.ActiveSceneChange:
                    ErrorNoSceneName = GetSceneBy == GetSceneBy.Name && string.IsNullOrEmpty(SceneName.Trim());
                    ErrorBadBuildIndex = GetSceneBy == GetSceneBy.BuildIndex && (SceneBuildIndex < 0 || SceneBuildIndex + 1 > SceneManager.sceneCountInBuildSettings);
                    break;
            }

#endif
        }
    }
}