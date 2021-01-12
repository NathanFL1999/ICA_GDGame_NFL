using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.GameComponents;
using GDLibrary.Interfaces;
using Microsoft.Xna.Framework;

namespace GDLibrary.Core.Managers.State
{
    /// <summary>
    /// Use this manager to listen for related events and perform actions in your game based on events received
    /// </summary>
    public class MyGameStateManager : PausableGameComponent, IEventHandler
    {

        private UITextObject deathCountTextObject;
        private int deathCount;
        public MyGameStateManager(Game game, StatusType statusType, UITextObject deathCountTextObject) : base(game, statusType)
        {
            this.deathCount = 0;
            this.deathCountTextObject = deathCountTextObject;
        }

        public override void SubscribeToEvents()
        {
            //add new events here...

            base.SubscribeToEvents();
        }

        public override void HandleEvent(EventData eventData)
        {
            if (eventData.EventCategoryType == EventCategoryType.UI)
            {
                if (eventData.EventActionType == EventActionType.OnDeathCountChange)
                {
                    deathCount += (int)eventData.Parameters[0];
                    deathCountTextObject.Text = "Death Count: " + deathCount;
                }
            }
            base.HandleEvent(eventData);
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            //add code here to check for the status of a particular set of related events e.g. collect all inventory items then...

            base.ApplyUpdate(gameTime);
        }
    }
}