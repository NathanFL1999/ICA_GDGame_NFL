using GDLibrary.Enums;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.Parameters;
using Microsoft.Xna.Framework;

namespace GDLibrary.Actors
{
    /// <summary>
    /// Parent class for all collidable primitive objects. Inherit from this
    /// class to add your own collidable objects (e.g. PlayerCollidablePrimitiveObject)
    /// </summary>
    public class CollidablePrimitiveObject : PrimitiveObject
    {
        private readonly EffectParameters effectParameters;
        #region Variables
        //the skin used to wrap the object
        private ICollisionPrimitive collisionPrimitive;

        //the object that im colliding with
        private Actor collidee;
        private ObjectManager objectManager;

        #endregion Variables

        #region Properties
        //returns a reference to whatever this object is colliding against
        public Actor Collidee
        {
            get
            {
                return collidee;
            }
            set
            {
                collidee = value;
            }
        }
        public ICollisionPrimitive CollisionPrimitive
        {
            get
            {
                return collisionPrimitive;
            }
            set
            {
                collisionPrimitive = value;
            }
        }
        public ObjectManager ObjectManager
        {
            get
            {
                return this.objectManager;
            }
        }

        #endregion Properties

        //used to draw collidable primitives that have a texture i.e. use VertexPositionColor vertex types only
        public CollidablePrimitiveObject(string id, ActorType actorType, StatusType statusType, Transform3D transform3D,
            EffectParameters effectParameters, IVertexData vertexData,
             ICollisionPrimitive collisionPrimitive, ObjectManager objectManager)
            : base(id, actorType, statusType, transform3D, effectParameters, vertexData)
        {
            this.effectParameters = effectParameters;
            this.collisionPrimitive = collisionPrimitive;

            //unusual to pass this in but we use it to test for collisions - see Update();
            this.objectManager = objectManager;
        }

        //read and store movement suggested by keyboard input
        protected virtual void HandleInput(GameTime gameTime)
        {
        }

        //define what happens when a collision occurs
        protected virtual void HandleCollisionResponse(Actor collidee)
        {
        }

        //test for collision against all opaque and transparent objects
        protected virtual Actor CheckCollisions(GameTime gameTime)
        {
            return null;
        }

        //test for collision against a specific object
        private Actor CheckCollisionWithActor(GameTime gameTime, Actor3D actor3D)
        {
            return null;
        }

        //to do...
    }
}