﻿using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GDGame.MyGame.Managers
{
    public class MyMenuManager : MenuManager
    {
        private MouseManager mouseManager;
        private KeyboardManager keyboardManager;
        private List<DrawnActor2D> loadedTempTexture;

        public MyMenuManager(Game game, StatusType statusType, SpriteBatch spriteBatch,
            MouseManager mouseManager, KeyboardManager keyboardManager)
            : base(game, statusType, spriteBatch)
        {
            this.mouseManager = mouseManager;
            this.keyboardManager = keyboardManager;
            this.loadedTempTexture = new List<DrawnActor2D>();
            EventDispatcher.Subscribe(EventCategoryType.Player, HandleEvent);
        }

        public override void HandleEvent(EventData eventData)
        {
            if (eventData.EventCategoryType == EventCategoryType.Menu)
            {
                if (eventData.EventActionType == EventActionType.OnPause)
                    this.StatusType = StatusType.Drawn | StatusType.Update;
                else if (eventData.EventActionType == EventActionType.OnPlay)
                    this.StatusType = StatusType.Off;
            }

            else if (eventData.EventCategoryType == EventCategoryType.Player)
            {
                if (eventData.EventActionType == EventActionType.OnGameOver)
                {
                        StatusType = StatusType.Update | StatusType.Drawn;
                        
                        SetScene("end");
                    }
                }
            }
        

        protected override void HandleInput(GameTime gameTime)
        {
            //bug fix - 7.12.20 - Exit button was hidden but we were still testing for mouse click
            if ((this.StatusType & StatusType.Update) != 0)
            {
                HandleMouse(gameTime);
            }

            HandleKeyboard(gameTime);
            //base.HandleInput(gameTime); //nothing happening in the base method
        }

        protected override void HandleMouse(GameTime gameTime)
        {
            foreach (DrawnActor2D actor in this.ActiveList)
            {
                if (actor is UIButtonObject)
                {
                    if (actor.Transform2D.Bounds.Contains(this.mouseManager.Bounds))
                    {
                        if (this.mouseManager.IsLeftButtonClickedOnce())
                        {
                            HandleClickedButton(gameTime, actor as UIButtonObject);
                        }
                    }
                }
            }
            base.HandleMouse(gameTime);
        }

        private void HandleClickedButton(GameTime gameTime, UIButtonObject uIButtonObject)
        {
            //benefit of switch vs if...else is that code will jump to correct switch case directly
            switch (uIButtonObject.ID)
            {
                case "play":
                    object[] parameters = { "buttonClick" };
                    EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                        EventActionType.OnPlay2D, parameters));

                    EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPlay, null));

                    break;

                case "End_Button":

                    object[] parameters2 = { "buttonClick" };
                    EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                        EventActionType.OnPlay2D, parameters2));

                    this.Game.Exit();

                    break;

                case "controls":
                    this.SetScene("controls");
                    break;

                case "exit":
                    object[] parameters3 = { "buttonClick" };
                    EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                        EventActionType.OnPlay2D, parameters3));

                    this.Game.Exit();
                    break;

                default:
                    break;
            }
        }

        protected override void HandleKeyboard(GameTime gameTime)
        {
            if (this.keyboardManager.IsFirstKeyPress(Microsoft.Xna.Framework.Input.Keys.M))
            {
                if (this.StatusType == StatusType.Off)
                {
                    //show menu
                    EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPause, null));
                }
                else
                {
                    //show game
                    EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPlay, null));
                }
            }

            base.HandleKeyboard(gameTime);
        }
    }
}