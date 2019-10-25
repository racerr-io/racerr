using Mirror;
using UnityEngine;

namespace Racerr.StateMachine
{
    public enum StateEnum
    {
        #region Shared States
        Race,
        Intermission,
        #endregion

        #region Server Only States
        ServerIdle,
        #endregion

        #region Client Only States
        ClientStartMenu,
        ClientSpectate
        #endregion
    }

    public interface IStateMachine
    {
        StateEnum StateType { get; }
        void ChangeState(StateEnum stateType, object optionalData = null);
    }

    public interface IState
    {
        void Enter(object optionalData = null);
        void Exit();
    }

    public abstract class NetworkedState : NetworkBehaviour, IState
    {
        public virtual void Enter(object optionalData = null) { }
        public virtual void Exit() { }
        protected virtual void FixedUpdate() { }
    }

    public abstract class LocalState : MonoBehaviour, IState
    {
        public virtual void Enter(object optionalData = null) { }
        public virtual void Exit() { }
        protected virtual void FixedUpdate() { }
    }
}