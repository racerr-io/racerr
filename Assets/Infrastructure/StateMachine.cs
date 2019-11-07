using Mirror;
using UnityEngine;

namespace Racerr.Infrastructure
{
    /// <summary>
    /// Used by State Machine to allow the caller to easily change the state of the
    /// state machine, based on any enumerated value passed in.
    /// </summary>
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

    /// <summary>
    /// Interface for State Machine, providing a common way of changing state.
    /// </summary>
    public interface IStateMachine
    {
        StateEnum StateType { get; }
        void ChangeState(StateEnum stateType, object optionalData = null);
    }

    /// <summary>
    /// A state machine can have many states, represented by this interface.
    /// State Machines call Enter() upon transitioning into a state, and Exit()
    /// when transitioning out of a state. These functions are useful for setup and cleanup respectively.
    /// Note that State Machines only enable/disable states and not destroy them for performance reasons.
    /// As a result, fields will remain the same unless they are overwritten, so be careful.
    /// This interface is intended to be implemented by a Unity script, and states can be used
    /// by placing it in any scene in the editor.
    /// </summary>
    public interface IState
    {
        void Enter(object optionalData = null);
        void Exit();
    }

    /// <summary>
    /// A state script which provides Networking capability. These scripts can be synchronised
    /// to all clients using Mirror's networking functionality such as SyncVars.
    /// Please note that even if a NetworkedState script is disabled, networking functionality is
    /// still active. 
    /// </summary>
    public abstract class NetworkedState : NetworkBehaviour, IState
    {
        public virtual void Enter(object optionalData = null) { }
        public virtual void Exit() { }
        protected virtual void FixedUpdate() { }
    }

    /// <summary>
    /// A state script for local scripts that don't need to be networked.
    /// </summary>
    public abstract class LocalState : MonoBehaviour, IState
    {
        public virtual void Enter(object optionalData = null) { }
        public virtual void Exit() { }
        protected virtual void FixedUpdate() { }
    }
}