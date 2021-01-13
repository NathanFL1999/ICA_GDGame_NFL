using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.MyGame;
using GDLibrary.Parameters;
using Microsoft.Xna.Framework;
using System;

namespace GDGame.MyGame.Actors
{
    class CollidablePickups : CollidablePrimitiveObject
    {

        public CollidablePickups(string id, ActorType actorType, StatusType statusType, Transform3D transform, EffectParameters effectParameters, IVertexData vertexData, ICollisionPrimitive collisionPrimitive, ObjectManager objectManager)
            : base(id, actorType, statusType, transform, effectParameters, vertexData, collisionPrimitive, objectManager)
        {

        }

        public override void Update(GameTime gameTime)
        {

            
        }
    }
}
