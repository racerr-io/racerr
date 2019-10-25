﻿using Doozy.Engine.UI;
using Racerr.MultiplayerService;
using Racerr.StateMachine.Server;
using TMPro;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public class ClientRaceState : LocalState
    {
        [SerializeField] ServerRaceState serverRaceState;
        [SerializeField] UIView raceView;
        [SerializeField] TextMeshProUGUI raceTimerTMP;
        [SerializeField] TextMeshProUGUI speedTMP;
        [SerializeField] TextMeshProUGUI leaderboardTMP;

        public override void Enter(object optionalData = null)
        {
            raceView.Show();
        }

        public override void Exit()
        {
            raceView.Hide();
        }

        protected override void FixedUpdate()
        {   
            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
                TransitionToIntermission();
            }
            else if (Player.LocalPlayer.IsDead)
            {
                TransitionToSpectate();
            }
            else
            {
                raceTimerTMP.text = serverRaceState.CurrentRaceLength.ToRaceTimeFormat();
                speedTMP.text = Player.LocalPlayer.Car.Velocity.ToString() + " KPH";

                string leaderboardText = string.Empty;
                foreach (RaceSessionState.PlayerPositionDTO playerPositionDTO in serverRaceState.playerPositions)
                {
                    leaderboardText += $"{playerPositionDTO.position}. {playerPositionDTO.playerName}";

                    if (playerPositionDTO.timeString != null)
                    {
                        leaderboardText += $" ({playerPositionDTO.timeString})";
                    }

                    leaderboardText += "\n";
                }

                leaderboardTMP.text = leaderboardText;
            }
        }

        void TransitionToIntermission()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.Intermission);
        }

        void TransitionToSpectate()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.ClientSpectate);
        }
    }
}