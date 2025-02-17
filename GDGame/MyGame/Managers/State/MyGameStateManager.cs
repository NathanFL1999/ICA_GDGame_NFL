﻿using GDGame;
using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.GameComponents;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.Parameters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GDLibrary.Core.Managers.State
{
    /// <summary>
    /// Use this manager to listen for related events and perform actions in your game based on events received
    /// </summary>
    public class MyGameStateManager : PausableGameComponent, IEventHandler
    {

        private UITextObject deathCountTextObject;
        private int deathCount;
        private int level;

        public MyGameStateManager(Game game, StatusType statusType, UITextObject deathCountTextObject) : base(game, statusType)
        {
            this.deathCount = 0;
            this.level = 1;
            this.deathCountTextObject = deathCountTextObject;
        }


    public override void SubscribeToEvents()
        {
            EventDispatcher.Subscribe(EventCategoryType.UI, HandleEvent);
            EventDispatcher.Subscribe(EventCategoryType.Player, HandleEvent);
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

            else if (eventData.EventCategoryType == EventCategoryType.Player)
            {
                if (eventData.EventActionType == EventActionType.OnWin)
                {
                    level += (int)eventData.Parameters[0];                 
                    level++;
                }
                else if (eventData.EventActionType == EventActionType.OnGameOver)
                {
                    EndScreen();
                }
            }

        

            base.HandleEvent(eventData);
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            //add code here to check for the status of a particular set of related events e.g. collect all inventory items then...

            base.ApplyUpdate(gameTime);
        }

        private void EndScreen()
        {
            deathCountTextObject.Transform2D.Translation = new Vector2(720, 540);
        }
    }
}