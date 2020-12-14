using System;
using System.Collections.Generic;
using System.Timers;

namespace GDLibrary.Events
{
    /// <summary>
    /// Creates a list of events that can be fired after a user-defined delay in ms.
    ///
    /// Note:
    ///      - Events do NOT need to be added in CHRONOLOGICAL order
    ///      - No TWO events can occur at the EXACT SAME TIME
    ///
    /// Usage:
    ///       EventScheduler scheduler = new EventScheduler("scary sounds events");
    ///         scheduler.Add(
    ///            new EventData(EventCategoryType.Camera, EventActionType.OnPlay, null), 20000);
    ///        scheduler.Add(
    ///           new EventData(EventCategoryType.Camera, EventActionType.OnPause, null), 5000);
    ///        scheduler.Add(
    ///            new EventData(EventCategoryType.Camera, EventActionType.OnStop, null), 15000);
    ///        scheduler.Start();
    ///
    /// </summary>
    public class EventScheduler
    {
        #region Fields
        private string id;
        private Timer timer;
        private List<ScheduledEvent> scheduledEventList;
        #endregion Fields

        #region Properties
        public string ID { get => id; set => id = value.Trim(); }
        #endregion Properties

        #region Constructors & Core

        public EventScheduler(string id)
        {
            ID = id;
            scheduledEventList = new List<ScheduledEvent>();
        }

        /// <summary>
        /// Call this method to add an event to the scheduler
        /// </summary>
        /// <param name="eventData">EventData</param>
        /// <param name="delayInMs">Integer delay in ms</param>
        /// <returns></returns>
        public virtual bool Add(EventData eventData, int delayInMs)
        {
            ScheduledEvent delayedEvent = new ScheduledEvent(eventData, delayInMs);
            if (!scheduledEventList.Contains(delayedEvent))
            {
                scheduledEventList.Add(delayedEvent);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Call this method to Start the scheduler
        /// </summary>
        public virtual void Start()
        {
            if (scheduledEventList.Count > 1)
            {
                //sort the events so that delays are in ascending order
                scheduledEventList.Sort((a, b) => a.DelayInMs - b.DelayInMs);
            }

            if (scheduledEventList.Count > 0)
            {
                //calculate the delay for the next event to be published
                int delayInMs = scheduledEventList[0].DelayInMs;

                //start the first iteration of the timer
                StartTimer(delayInMs);

                //update all events AFTER the first event to substract the time spent to wait until the first
                UpdateFollowingEventDelays(delayInMs);
            }
        }

        /// <summary>
        /// Call this method to stop all outstanding timers and clear the list
        /// </summary>
        public virtual void Stop()
        {
            StopTimer();
            scheduledEventList.Clear();
        }

        /// <summary>
        /// Called when a timed interval between events occurs and starts the next timer in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            //publish the event
            EventDispatcher.Publish(scheduledEventList[0].EventData);

            //remove the event that was just published
            scheduledEventList.RemoveAt(0);

            if (scheduledEventList.Count != 0)
            {
                //calculate the delay for the next event to be published
                int delayInMs = scheduledEventList[0].DelayInMs;

                //update all events AFTER the first event to substract the time spent to wait until the first
                UpdateFollowingEventDelays(delayInMs);

                //start time timer with the delay until the next event
                if (scheduledEventList.Count != 0)
                {
                    StartTimer(delayInMs);
                }
            }
            //if no more events then release the resource from the last run timer
            else
            {
                StopTimer();
            }
        }

        private void UpdateFollowingEventDelays(int nextDelayInMs)
        {
            //subtract the time for the next event to be published from all other events
            for (int i = 1; i < scheduledEventList.Count; i++)
            {
                scheduledEventList[i].DelayInMs -= nextDelayInMs;
            }
        }

        private void StartTimer(int delayInMs)
        {
            StopTimer();
            timer = new Timer(delayInMs);
            timer.AutoReset = false;
            timer.Elapsed += HandleTimerElapsed;
            timer.Start();
        }

        private void StopTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }

        #endregion Constructors & Core
    }

    public sealed class ScheduledEvent
    {
        private EventData eventData;
        private int delayInMs;

        public EventData EventData { get => eventData; set => eventData = value; }
        public int DelayInMs { get => delayInMs; set => delayInMs = value; }

        public ScheduledEvent(EventData eventData, int delayInMs)
        {
            EventData = eventData;
            DelayInMs = delayInMs;
        }

        public override bool Equals(object obj)
        {
            return obj is ScheduledEvent @event &&
                   EqualityComparer<EventData>.Default.Equals(eventData, @event.eventData) &&
                   delayInMs == @event.delayInMs;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(eventData, delayInMs);
        }
    }
}