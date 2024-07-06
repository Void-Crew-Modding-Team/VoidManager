using System;
using System.Collections.Generic;

namespace VoidManager.Utilities
{
    /// <summary>
    /// A collection of helpful functions to make modding easier
    /// </summary>
    public class Tools
    {
        private static List<Tuple<Action, DateTime>> tasks = new();

        /// <summary>
        /// Perform an action after a specified delay.<br/><br/>
        /// Intended for short durations only. Not recommended for anything over a minute.
        /// </summary>
        /// <param name="action">The action to perform after waiting</param>
        /// <param name="delayMs">The number of milliseconds to wait</param>
        public static void DelayDo(Action action, double delayMs)
        {
            DateTime time = DateTime.Now.AddMilliseconds(delayMs);
            tasks.Add(Tuple.Create(action, time));

            if (tasks.Count == 1)
            {
                Events.Instance.LateUpdate += DoTasks;
            }
        }

        private static void DoTasks(object sender, EventArgs e)
        {
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                if (tasks[i].Item2 <= DateTime.Now)
                {
                    tasks[i].Item1.Invoke();
                    tasks.RemoveAt(i);
                }
            }

            if (tasks.Count == 0)
            {
                Events.Instance.LateUpdate -= DoTasks;
            }
        }
    }
}
