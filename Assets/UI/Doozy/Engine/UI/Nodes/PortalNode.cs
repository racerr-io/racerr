// Copyright (c) 2015 - 2019 Doozy Entertainment / Marlink Trading SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Engine.Nody.Attributes;
using Doozy.Engine.Nody.Connections;
using Doozy.Engine.Nody.Models;
using Doozy.Engine.Utils;
using UnityEngine;

namespace Doozy.Engine.UI.Nodes
{
    /// <summary>
    ///    Global node that listens for a set game event (string value). When triggered it jumps instantly to the next node in the Graph.
    ///    A global node is active as long as its parent Graph is active.
    ///    This particular node allows for jumping from one part of the UI flow to another, without the need of a direct connection.
    ///    Due to the way it works, this node can also be considered as a 'virtual connection' between multiple active Graphs.
    /// </summary>
    [NodeMenu(MenuUtils.PortalNode_CreateNodeMenu_Name, MenuUtils.PortalNode_CreateNodeMenu_Order)]
    public class PortalNode : Node
    {
#if UNITY_EDITOR
        public override bool HasErrors { get { return base.HasErrors || ErrorNotListeningForAnyGameEvent; } }
        public bool ErrorNotListeningForAnyGameEvent;
#endif

        [SerializeField] private string m_gameEvent;
        public string GameEventToListenFor { get { return m_gameEvent; } }

        [NonSerialized] private Graph m_portalGraph;
        public Graph PortalGraph { get { return m_portalGraph; } set { m_portalGraph = value; } }

        public override void OnCreate()
        {
            base.OnCreate();
            CanBeDeleted = true;
            SetNodeType(NodeType.Global);
            SetName(UILabels.PortalNodeName);
        }

        public override void AddDefaultSockets()
        {
            base.AddDefaultSockets();
            AddOutputSocket(ConnectionMode.Override, typeof(PassthroughConnection), false, false);
        }

        private void AddListeners() { Message.AddListener<GameEventMessage>(OnGameEventMessage); }

        private void RemoveListeners() { Message.RemoveListener<GameEventMessage>(OnGameEventMessage); }

        public override void Activate(Graph portalGraph)
        {
            PortalGraph = portalGraph;
            AddListeners();
            base.Activate(portalGraph);
        }

        public override void Deactivate()
        {
            RemoveListeners();
            base.Deactivate();
        }

        private void OnGameEventMessage(GameEventMessage message)
        {
            if (message.EventName != GameEventToListenFor) return;
            PortalGraph.SetActiveNodeById(Id);
        }

        public override void CopyNode(Node original)
        {
            base.CopyNode(original);
            var portalNode = (PortalNode) original;
            m_gameEvent = portalNode.m_gameEvent;
        }

        public override void OnEnter(Node previousActiveNode, Connection connection)
        {
            base.OnEnter(previousActiveNode, connection);
            if (ActiveGraph == null) return;
            if (!FirstOutputSocket.IsConnected) return;
            ActiveGraph.SetActiveNodeByConnection(FirstOutputSocket.FirstConnection);
        }

        public override void CheckForErrors()
        {
            base.CheckForErrors();
#if UNITY_EDITOR
            ErrorNotListeningForAnyGameEvent = string.IsNullOrEmpty(m_gameEvent);
#endif
        }
    }
}