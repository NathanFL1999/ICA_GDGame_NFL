using GDGame;
using GDGame.MyGame.Actors;
using GDLibrary.Actors;
using GDLibrary.Containers;
using GDLibrary.Enums;
using GDLibrary.Factories;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.Parameters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GDLibrary.Utilities
{
    /// <summary>
    /// Use the level loader to instanciate 3D drawn actors within your level from a PNG file.
    ///
    /// Usage:
    ///    LevelLoader levelLoader = new LevelLoader(this.objectArchetypeDictionary,
    ///    this.textureDictionary);
    ///     List<DrawnActor3D> actorList = levelLoader.Load(this.textureDictionary[fileName],
    ///           scaleX, scaleZ, height, offset);
    ///     this.object3DManager.Add(actorList);
    ///
    /// </summary>
    public class LevelLoader<T> where T : DrawnActor3D
    {
        private static readonly Color ColorLevelLoaderIgnore = Color.White;

        private Dictionary<string, T> archetypeDictionary;
        private ContentDictionary<Texture2D> textureDictionary;
        private ObjectManager objectManager;
        private Dictionary<string, BasicEffect> effectDictionary;


        public LevelLoader(Dictionary<string, T> archetypeDictionary,
            ContentDictionary<Texture2D> textureDictionary, ObjectManager objectManager, Dictionary<string, BasicEffect> effectDictionary)
        {
            this.archetypeDictionary = archetypeDictionary;
            this.textureDictionary = textureDictionary;
            this.objectManager = objectManager;
            this.effectDictionary = effectDictionary; 
        }

        public List<DrawnActor3D> Load(Texture2D texture,
            float scaleX, float scaleZ, float height, Vector3 offset)
        {
            List<DrawnActor3D> list = new List<DrawnActor3D>();
            Color[] colorData = new Color[texture.Height * texture.Width];
            texture.GetData<Color>(colorData);

            Color color;
            Vector3 translation;
            DrawnActor3D actor;

            for (int y = 0; y < texture.Height; y++)
            {
                for (int x = 0; x < texture.Width; x++)
                {
                    color = colorData[x + y * texture.Width];

                    if (!color.Equals(ColorLevelLoaderIgnore))
                    {
                        //scale allows us to increase the separation between objects in the XZ plane
                        translation = new Vector3(x * scaleX, height, y * scaleZ);

                        //the offset allows us to shift the whole set of objects in X, Y, and Z
                        translation += offset;

                        actor = getObjectFromColor(color, translation);

                        if (actor != null)
                        {
                            list.Add(actor);
                        }
                    }
                } //end for x
            } //end for y
            return list;
        }

        private Random rand = new Random();

        private int count = 1;

        private DrawnActor3D getObjectFromColor(Color color, Vector3 translation)
        {
            //if the pixel is red then draw a tall (stretched collidable unlit cube)
            if (color.Equals(new Color(255, 0, 0)))
            {
                PrimitiveObject archetype
                        = archetypeDictionary["lit textured cube"] as PrimitiveObject;

                PrimitiveObject drawnActor3D = archetype.Clone() as PrimitiveObject;

                //   PrimitiveObject drawnActor3D
                //       = archetypeDictionary["lit textured pyramid"].Clone() as PrimitiveObject;

                //change it a bit
                drawnActor3D.ID = "cube " + count++;
                drawnActor3D.Transform3D.Scale = 10 * new Vector3(2, 3, 1);
                drawnActor3D.EffectParameters.Texture = textureDictionary["walls"];
                drawnActor3D.EffectParameters.Alpha = 1;
                drawnActor3D.Transform3D.Translation = translation;
                drawnActor3D.Transform3D.RotationInDegrees = new Vector3(0, 0, 0);


                BoxCollisionPrimitive collisionPrimitive = new BoxCollisionPrimitive(drawnActor3D.Transform3D);

                //make a collidable object and pass in the primitive
                CollidablePrimitiveObject collidablePrimitiveObject = new CollidablePrimitiveObject(
                    drawnActor3D.ID,
                    ActorType.CollidableDecorator,
                    StatusType.Update | StatusType.Drawn,
                    drawnActor3D.Transform3D,
                    drawnActor3D.EffectParameters,
                    drawnActor3D.IVertexData,
                    collisionPrimitive, 
                    objectManager);

                return collidablePrimitiveObject;
            }

            else if (color.Equals(new Color(0, 0, 255)))
            {
                PrimitiveObject archetype
                       = archetypeDictionary["lit textured pyramid"] as PrimitiveObject;

                PrimitiveObject drawnActor3D = archetype.Clone() as PrimitiveObject;

                //change it a bit
                drawnActor3D.ID = "Pyramid " + count++;
                drawnActor3D.Transform3D.Scale = 10 * new Vector3(1, 1, 1);
                drawnActor3D.EffectParameters.Texture = textureDictionary["redCube"];
                drawnActor3D.EffectParameters.Alpha = 1;
                drawnActor3D.Transform3D.Translation = translation;
                drawnActor3D.Transform3D.RotationInDegrees = new Vector3(0, 0, 0);


                BoxCollisionPrimitive collisionPrimitive = new BoxCollisionPrimitive(drawnActor3D.Transform3D);

                //make a collidable object and pass in the primitive
                CollidablePrimitiveObject collidablePrimitiveObject = new CollidablePrimitiveObject(
                    drawnActor3D.ID,
                    ActorType.CollidableObstacle,
                    StatusType.Update | StatusType.Drawn,
                    drawnActor3D.Transform3D,
                    drawnActor3D.EffectParameters,
                    drawnActor3D.IVertexData,
                    collisionPrimitive,
                    objectManager);

                return collidablePrimitiveObject;
            }

            return null;
        }
    }
}