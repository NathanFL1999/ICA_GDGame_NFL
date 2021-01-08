using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.Parameters;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GDGame.MyGame.Actors
{
    class CollidableEnemy : CollidablePrimitiveObject
    {

        private float speed;
        private float range;

        public CollidableEnemy(string id, ActorType actorType, StatusType statusType, Transform3D transform, EffectParameters effectParameters, IVertexData vertexData, ICollisionPrimitive collisionPrimitive, ObjectManager objectManager, float speed, float range)
            : base(id, actorType, statusType, transform, effectParameters, vertexData, collisionPrimitive, objectManager)
        {
            this.speed = speed;
            this.range = range;

        }

        public override void Update(GameTime gameTime)
        {
            //read any input and store suggested increments
            HandleMovement(gameTime);

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

        protected void HandleMovement(GameTime gameTime)
        {
            //if (keyboardManager.IsKeyDown(moveKeys[0])) //Forward
            //{
            //    Transform3D.TranslateIncrement
            //        = Transform3D.Look * gameTime.ElapsedGameTime.Milliseconds
            //                * moveSpeed;
            //}
            //else if (keyboardManager.IsKeyDown(moveKeys[1])) //Backward
            //{
            //    Transform3D.TranslateIncrement
            //       = -Transform3D.Look * gameTime.ElapsedGameTime.Milliseconds
            //               * moveSpeed;
            //}

            
        }

        /********************************************************************************************/

        //this is where you write the application specific CDCR response for your game
        protected override void HandleCollisionResponse(Actor collidee)
        {
            if (collidee is CollidableZoneObject)
            {
                CollidableZoneObject simpleZoneObject = collidee as CollidableZoneObject;
                

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
                else if (collidee.ActorType == ActorType.CollidableDecorator)
                {
                    (collidee as DrawnActor3D).EffectParameters.DiffuseColor = Color.Blue;
                }
            }
        }
    }
}
