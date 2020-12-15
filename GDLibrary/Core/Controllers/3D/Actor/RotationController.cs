using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Interfaces;
using Microsoft.Xna.Framework;

namespace GDLibrary.Controllers
{
    public class RotationController : Controller
    {
        private float rotationSpeed;
        private Vector3 rotationAxis;

        public RotationController(string id, ControllerType controllerType,
         float rotationSpeed, Vector3 rotationAxis) : base(id, controllerType)
        {
            this.rotationSpeed = rotationSpeed;
            this.rotationAxis = rotationAxis;
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            Actor3D parent = actor as Actor3D;

            if (parent != null)
            {
                parent.Transform3D.RotationInDegrees += rotationSpeed * rotationAxis;
            }

            base.Update(gameTime, actor);
        }
    }
}