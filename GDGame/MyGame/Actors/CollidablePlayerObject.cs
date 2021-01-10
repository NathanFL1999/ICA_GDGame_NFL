using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.Parameters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary.MyGame
{
    /// <summary>
    /// Moveable, collidable player using keyboard and checks for collisions
    /// </summary>
    public class CollidablePlayerObject : CollidablePrimitiveObject
    {
        #region Fields
        private float moveSpeed, rotationSpeed;
        private KeyboardManager keyboardManager;
        private Game game;
        private int deathCount;
        private Keys[] moveKeys;
        #endregion Fields

        public CollidablePlayerObject(string id, ActorType actorType, StatusType statusType, Transform3D transform,
            EffectParameters effectParameters, IVertexData vertexData,
            ICollisionPrimitive collisionPrimitive, ObjectManager objectManager,
            Keys[] moveKeys, float moveSpeed, float rotationSpeed, KeyboardManager keyboardManager, Game game)
            : base(id, actorType, statusType, transform, effectParameters, vertexData, collisionPrimitive, objectManager)
        {
            this.moveKeys = moveKeys;
            this.moveSpeed = moveSpeed;
            this.rotationSpeed = rotationSpeed;

            //for movement
            this.keyboardManager = keyboardManager;
            this.game = game;

            this.deathCount = 0;
        }

        public override void Update(GameTime gameTime)
        {
            //read any input and store suggested increments
            HandleInput(gameTime);

            //have we collided with something?
            Collidee = CheckAllCollisions(gameTime);

            //how do we respond to this collidee e.g. pickup?
            HandleCollisionResponse(Collidee);

            //if no collision then move - see how we set this.Collidee to null in HandleCollisionResponse()
            //below when we hit against a zone
            if (Collidee == null)
            {
                ApplyInput(gameTime);
            }

            //reset translate and rotate and update primitive
            base.Update(gameTime);
        }

        protected override void HandleInput(GameTime gameTime)
        {
            if (keyboardManager.IsKeyDown(moveKeys[0])) //Forward
            {
                Transform3D.TranslateIncrement
                    = Transform3D.Look * gameTime.ElapsedGameTime.Milliseconds
                            * moveSpeed;
            }
            else if (keyboardManager.IsKeyDown(moveKeys[1])) //Backward
            {
                Transform3D.TranslateIncrement
                   = -Transform3D.Look * gameTime.ElapsedGameTime.Milliseconds
                           * moveSpeed;
            }

            if (keyboardManager.IsKeyDown(moveKeys[2])) //Left
            {
                Transform3D.TranslateIncrement += -Transform3D.Right * gameTime.ElapsedGameTime.Milliseconds * moveSpeed;
            }
            else if (keyboardManager.IsKeyDown(moveKeys[3])) //Right
            {
                Transform3D.TranslateIncrement += Transform3D.Right * gameTime.ElapsedGameTime.Milliseconds * moveSpeed;
            }

            if (keyboardManager.IsFirstKeyPress(moveKeys[4])) //Rotate Right
            {
                Transform3D.RotateAroundUpBy(-90);
            }
            else if (keyboardManager.IsFirstKeyPress(moveKeys[5])) //Rotate Left
            {
                Transform3D.RotateAroundUpBy(90);
            }

        }

        /********************************************************************************************/

        //this is where you write the application specific CDCR response for your game
        protected override void HandleCollisionResponse(Actor collidee)
        {
            if (collidee is CollidableZoneObject)
            {
                CollidableZoneObject simpleZoneObject = collidee as CollidableZoneObject;

                //do something based on the zone type - see Main::InitializeCollidableZones() for ID
                if (simpleZoneObject.ID.Equals("sound and camera trigger zone 1"))
                {
                    //publish an event e.g sound, health progress
                    object[] parameters = { "smokealarm" };
                    EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                        EventActionType.OnPlay2D, parameters));
                }

                //IMPORTANT - setting this to null means that the ApplyInput() method will get called and the player can move through the zone.
                Collidee = null;
            }
            else if (collidee is CollidablePrimitiveObject)
            {
                //the boxes on the left that we loaded from level loader
                if (collidee.ActorType == ActorType.CollidablePickup)
                {
                    //remove the object
                    object[] parameters = { collidee };
                    EventDispatcher.Publish(new EventData(EventCategoryType.Object, EventActionType.OnRemoveActor, parameters));
                }
                //the boxes on the right that move up and down
                else if (collidee.ActorType == ActorType.Enemy)
                {
                    (collidee as DrawnActor3D).EffectParameters.DiffuseColor = Color.Yellow;
                    Transform3D.Translation = new Vector3(150, 2.5f, 500);

                    deathCount++;
                }
                if (collidee.ActorType == ActorType.Obstacle)
                {
                    (collidee as DrawnActor3D).EffectParameters.DiffuseColor = Color.Purple;
                    Transform3D.Translation = new Vector3(150, 2.5f, 500);

                    deathCount++;
                }
            }
        }
    }
}