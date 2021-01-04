using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Controllers;
using GDLibrary;

namespace GDGame.MyGame.Controllers
{
    public class PlayerController : Controller
    {
        #region Fields

        private KeyboardManager keyboardManager;
        private MouseManager mouseManager;
        private Camera3D camera3D;
        private float moveSpeed, rotationSpeed, originalMoveSpeed;
        private Keys[][] moveKeys;
        private bool cameraMoveConstraint;
        private bool locked;

        #endregion

        #region Constructors

        public PlayerController(string id, ControllerType controllerType,
            KeyboardManager keyboardManager,
            MouseManager mouseManager,
            Camera3D camera3D,
            float moveSpeed, float rotationSpeed,
            Keys[][] moveKeys)
            : base(id, controllerType)
        {
            this.keyboardManager = keyboardManager;
            this.mouseManager = mouseManager;
            this.camera3D = camera3D;
            this.moveSpeed = this.originalMoveSpeed = moveSpeed;
            this.rotationSpeed = rotationSpeed;
            this.moveKeys = moveKeys;
            this.cameraMoveConstraint = false;
            this.locked = false;
        }

    }
    #endregion
}
