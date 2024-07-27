using CG.Game;
using CG.Game.SpaceObjects.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoidManager.Utilities
{
    /// <summary>
    /// A collection of helpful functions to make modding easier
    /// </summary>
    public class Tools
    {
        /// <summary>
        /// Does a player ship exist
        /// </summary>
        /// <returns>True if a player ship exists, false otherwise</returns>
        public static bool PlayerShipExists { get => ClientGame.Current?.PlayerShip?.Platform != null; }

        /// <summary>
        /// Is the player ship in a void jump<br/>
        /// true for: VoidJumpTravellingStable, VoidJumpTravellingUnstable, VoidJumpInterdiction, VoidJumpApproachingDestination, VoidJumpSpinningDown<br/>
        /// false otherwise
        /// </summary>
        public static bool InVoid
        {
            get
            {
                VoidJumpSystem voidJumpSystem = ClientGame.Current?.PlayerShip?.GameObject?.GetComponent<VoidJumpSystem>();

                if (voidJumpSystem == null)
                    return false;

                if (voidJumpSystem.ActiveState is VoidJumpTravellingStable or VoidJumpTravellingUnstable or VoidJumpInterdiction or VoidJumpApproachingDestination or VoidJumpSpinningDown)
                    return true;

                return false;
            }
        }

        private static readonly Dictionary<object, (Action, DateTime)> uniqueTasks = new();

        /// <summary>
        /// Perform an action after a specified delay.<br/><br/>
        /// Intended for short durations only. Not recommended for anything over a minute.
        /// </summary>
        /// <param name="action">The action to perform after waiting</param>
        /// <param name="delayMs">The number of milliseconds to wait</param>
        public static void DelayDo(Action action, double delayMs)
        {
            DelayDoUnique(new object(), action, delayMs);
        }

        /// <summary>
        /// Perform an action after a specified delay.<br/>
        /// Multiple calls with the same uniqueObject will replace the action and restart the delay<br/><br/>
        /// Intended for short durations only. Not recommended for anything over a minute.
        /// </summary>
        /// <param name="uniqueObject">This is the only object checked for uniqueness<br/>action and delayMs are not checked</param>
        /// <param name="action">The action to perform after waiting</param>
        /// <param name="delayMs">The number of milliseconds to wait</param>
        public static void DelayDoUnique(object uniqueObject, Action action, double delayMs)
        {
            uniqueTasks.Remove(uniqueObject);
            uniqueTasks.Add(uniqueObject, (action, DateTime.Now.AddMilliseconds(delayMs)));

            if (uniqueTasks.Count == 1)
            {
                Events.Instance.LateUpdate += DoUniqueTasks;
            }
        }

        /// <summary>
        /// Removes an action without invoking it
        /// </summary>
        /// <param name="uniqueObject"></param>
        /// <returns>True if the object was found and the action removed, false otherwise</returns>
        public static bool CancelDelayDoUnique(object uniqueObject)
        {
            return uniqueTasks.Remove(uniqueObject);
        }

        private static void DoUniqueTasks(object sender, EventArgs e)
        {
            for (int i = uniqueTasks.Count - 1; i >= 0; i--)
            {
                KeyValuePair<object, (Action, DateTime)> pair = uniqueTasks.ElementAt(i);
                if (pair.Value.Item2 <= DateTime.Now)
                {
                    pair.Value.Item1.Invoke();
                    uniqueTasks.Remove(pair.Key);
                }
            }

            if (uniqueTasks.Count == 0)
            {
                Events.Instance.LateUpdate -= DoUniqueTasks;
            }
        }
    }
}
