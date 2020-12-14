﻿using GDLibrary.Actors;
using GDLibrary.Containers;
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
    public class LevelLoader
    {
        private static readonly Color ColorLevelLoaderIgnore = Color.White;

        private Dictionary<string, DrawnActor3D> objectArchetypeDictionary;
        private ContentDictionary<Texture2D> textureDictionary;

        public LevelLoader(Dictionary<string, DrawnActor3D> objectArchetypeDictionary,
            ContentDictionary<Texture2D> textureDictionary)
        {
            this.objectArchetypeDictionary = objectArchetypeDictionary;
            this.textureDictionary = textureDictionary;
        }

        public List<DrawnActor3D> Load(Texture2D texture,
            float scaleX, float scaleZ, float height, Vector3 offset)
        {
            List<DrawnActor3D> list = new List<DrawnActor3D>();
            Color[] colorData = new Color[texture.Height * texture.Width];
            texture.GetData<Color>(colorData);

            Color color;
            Vector3 position;
            DrawnActor3D actor;

            for (int y = 0; y < texture.Height; y++)
            {
                for (int x = 0; x < texture.Width; x++)
                {
                    color = colorData[x + y * texture.Width];

                    if (!color.Equals(ColorLevelLoaderIgnore))
                    {
                        //scale allows us to increase the separation between objects in the XZ plane
                        position = new Vector3(x * scaleX, height, y * scaleZ);

                        //the offset allows us to shift the whole set of objects in X, Y, and Z
                        position += offset;

                        actor = getObjectFromColor(color, position);

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

        private DrawnActor3D getObjectFromColor(Color color, Vector3 position)
        {
            //if the pixel is red then draw a tall (stretched collidable unlit cube)
            if (color.Equals(new Color(255, 0, 0)))
            {
                //wall/boundary
                return null;
            }
            else if (color.Equals(new Color(63, 72, 204)))
            {
                //enemy instance
                return null;
            }
            //add an else if for each type of object that you want to load...

            return null;
        }
    }
}