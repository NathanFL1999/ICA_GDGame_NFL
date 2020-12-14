using GDLibrary.Enums;
using System;
using System.Collections.Generic;

namespace GDLibrary.Events
{
    /// <summary>
    /// Encapsulates the fields of an event within the game
    /// </summary>
    /// <see cref="GDLibrary.Events.EventDispatcher"/>
    public class EventData : ICloneable
    {
        #region Fields
        private EventCategoryType eventCategoryType;
        private EventActionType eventActionType;
        private object[] parameters;
        #endregion Fields

        #region Properties
        public EventCategoryType EventCategoryType { get => eventCategoryType; set => eventCategoryType = value; }
        public EventActionType EventActionType { get => eventActionType; set => eventActionType = value; }
        public object[] Parameters { get => parameters; set => parameters = value; }
        #endregion Properties

        #region Constructors & Core

        /// <summary>
        /// Used for events with no attached parameters (e.g. win/lose event for single player game)
        /// </summary>
        /// <param name="eventCategoryType"></param>
        /// <param name="eventActionType"></param>
        public EventData(EventCategoryType eventCategoryType,
           EventActionType eventActionType) : this(eventCategoryType, eventActionType, null)
        {
        }

        /// <summary>
        /// Used for events with associated parameters (e.g. sound events where params=["boing", 1, true]
        /// </summary>
        /// <param name="eventCategoryType"></param>
        /// <param name="eventActionType"></param>
        /// <param name="parameters"></param>
        public EventData(EventCategoryType eventCategoryType,
            EventActionType eventActionType, object[] parameters)
        {
            EventCategoryType = eventCategoryType;
            EventActionType = eventActionType;
            Parameters = parameters;
        }

        public override bool Equals(object obj)
        {
            //  if(parameters != null)
            return obj is EventData data &&
                   eventCategoryType == data.eventCategoryType &&
                   eventActionType == data.eventActionType &&
                   parameters != null
                   ? EqualityComparer<object[]>.Default.Equals(parameters, data.parameters) : false;
        }

        public override int GetHashCode()
        {
            int hashCode = HashCode.Combine(eventCategoryType, eventActionType);

            if (parameters != null)
                HashCode.Combine(hashCode, parameters);

            return hashCode;
        }

        public override string ToString()
        {
            if (parameters == null)
            {
                return eventCategoryType + "," + eventActionType + ", [no params]";
            }
            else
            {
                string parametersAsString = String.Join(",", Array.ConvertAll(parameters, item => item.ToString()));
                return eventCategoryType + "," + eventActionType + "," + parametersAsString;
            }
        }

        public object Clone()
        {
            return new EventData(eventCategoryType, eventActionType,
                parameters);
        }

        #endregion Constructors & Core
    }
}