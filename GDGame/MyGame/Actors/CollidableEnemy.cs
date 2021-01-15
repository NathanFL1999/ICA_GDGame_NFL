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
    class CollidableEnemy : CollidablePrimitiveObject
    {

        private float speed;
        private float range;
        private CollidablePlayerObject player;

        public CollidableEnemy(string id, ActorType actorType, StatusType statusType, Transform3D transform, EffectParameters effectParameters, IVertexData vertexData, ICollisionPrimitive collisionPrimitive, ObjectManager objectManager, float speed, float range, CollidablePlayerObject player)
            : base(id, actorType, statusType, transform, effectParameters, vertexData, collisionPrimitive, objectManager)
        {
            this.speed = speed;
            this.range = range;
            this.player = player;

        }

        public override void Update(GameTime gameTime)
        {
            player = (CollidablePlayerObject)this.ObjectManager.GetActorByID("collidable player1");

            Vector3 movement = new Vector3(0, 1, 0);

            //enemy movement towards player, player has to be a certain distance away
            if ((this.Transform3D.Translation - player.Transform3D.Translation).Length() <= 60)
            {

                if (this.Transform3D.Translation.X <= player.Transform3D.Translation.X)
                    {
                        this.Transform3D.TranslateBy(new Vector3(0.2f, 0, 0));
                    }

                    else if (this.Transform3D.Translation.X >= player.Transform3D.Translation.X)
                    {
                        this.Transform3D.TranslateBy(new Vector3(-0.2f, 0, 0));
                    }

                    if (this.Transform3D.Translation.Z <= player.Transform3D.Translation.Z)
                    {
                        this.Transform3D.TranslateBy(new Vector3(0, 0, 0.2f));
                    }

                    else if (this.Transform3D.Translation.Z >= player.Transform3D.Translation.Z)
                    {
                        this.Transform3D.TranslateBy(new Vector3(0, 0, -0.2f));
                    }

            }

            //reset translate and rotate and update primitive
            base.Update(gameTime);

        }

    }
}
