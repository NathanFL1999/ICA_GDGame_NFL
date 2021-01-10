using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.MyGame;
using GDLibrary.Parameters;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GDGame.MyGame.Actors
{
    class CollidableObstacle : CollidablePrimitiveObject
    {

        private float speed;
        private int direction;
        private float range;

        public CollidableObstacle(string id, ActorType actorType, StatusType statusType, Transform3D transform, EffectParameters effectParameters, IVertexData vertexData, ICollisionPrimitive collisionPrimitive, ObjectManager objectManager, float speed, int direction, float range)
            : base(id, actorType, statusType, transform, effectParameters, vertexData, collisionPrimitive, objectManager)
        {
            this.speed = speed;
            this.direction = direction;
            this.range = range;

        }

        public override void Update(GameTime gameTime)
        {

            
            Vector3 movement = new Vector3(0, 0, 0);
            if (direction == 1)
            {
                this.Transform3D.TranslateBy(new Vector3(MathF.Sin((float)(1f * gameTime.TotalGameTime.TotalSeconds / 4) * 20f), 0, 0));
            }

            else if (direction == 2)
            {
                this.Transform3D.TranslateBy(new Vector3(0, 0, MathF.Sin((float)(1f * gameTime.TotalGameTime.TotalSeconds / 4) * 20f)));
            }

            //reset translate and rotate and update primitive
            base.Update(gameTime);
        }

    }
}