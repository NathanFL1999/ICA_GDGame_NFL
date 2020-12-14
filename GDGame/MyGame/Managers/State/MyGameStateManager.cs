using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.GameComponents;
using Microsoft.Xna.Framework;

namespace GDLibrary.Core.Managers.State
{
    /// <summary>
    /// Use this manager to listen for related events and perform actions in your game based on events received
    /// </summary>
    public class MyGameStateManager : PausableGameComponent
    {
        public MyGameStateManager(Game game, StatusType statusType) : base(game, statusType)
        {
        }

        protected override void SubscribeToEvents()
        {
            //add new events here...

            base.SubscribeToEvents();
        }

        protected override void HandleEvent(EventData eventData)
        {
            //add new if...else if statements to handle events here...

            base.HandleEvent(eventData);
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            //add code here to check for the status of a particular set of related events e.g. collect all inventory items then...

            base.ApplyUpdate(gameTime);
        }
    }
}