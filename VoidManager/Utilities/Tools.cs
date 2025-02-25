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
        /// Checks if a request has been made to DelayDoUnique with uniqueObject that has not yet been run
        /// </summary>
        /// <param name="uniqueObject"></param>
        /// <returns></returns>
        public static bool IsDelayRunning(object uniqueObject)
        {
            return uniqueTasks.ContainsKey(uniqueObject);
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

        //Used to separate RepeatForUnique keys from DelayDoUnique keys
        private static readonly Dictionary<object, object> repeatMap = new();

        /// <summary>
        /// Perform an action every frame for the specified duration.<br/>
        /// Identical to "Events.Instance.LateUpdate += (_, _) => action();" for sufficiently large values of durationMs
        /// </summary>
        /// <param name="uniqueObject">Used in DelayDoUnique for the end time</param>
        /// <param name="action"></param>
        /// <param name="durationMs">The number of milliseconds before the task stops repeating</param>
        public static void RepeatForUnique(object uniqueObject, Action action, double durationMs)
        {
            CancelRepeatFor(uniqueObject);
            object unique = new();
            repeatMap.Add(uniqueObject, unique);
            void h(object o_, EventArgs e_) => action();
            Events.Instance.LateUpdate += h;
            DelayDoUnique(unique, () => Events.Instance.LateUpdate -= h, durationMs);
        }

        /// <summary>
        /// Perform an action every frame for the specified duration.<br/>
        /// Identical to "Events.Instance.LateUpdate += (_, _) => action();" for sufficiently large values of durationMs
        /// </summary>
        /// <param name="action"></param>
        /// <param name="durationMs">The number of milliseconds before the task stops repeating</param>
        public static void RepeatFor(Action action, double durationMs)
        {
            RepeatForUnique(new object(), action, durationMs);
        }

        /// <summary>
        /// Is the action still scheduled to run each frame
        /// </summary>
        /// <param name="uniqueObject">The unique object provided to RepeatForUnique</param>
        /// <returns></returns>
        public static bool IsRepeatRunning(object uniqueObject)
        {
            return repeatMap.ContainsKey(uniqueObject);
        }

        /// <summary>
        /// Stops the action from running on future frames
        /// </summary>
        /// <param name="uniqueObject">The unique object provided to RepeatForUnique</param>
        public static void CancelRepeatFor(object uniqueObject)
        {
            if (repeatMap.ContainsKey(uniqueObject))
            {
                object key = repeatMap[uniqueObject];
                repeatMap.Remove(uniqueObject);
                uniqueTasks[key].Item1.Invoke();
                uniqueTasks.Remove(key);
            }
        }
    }
}
