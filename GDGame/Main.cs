﻿#define DEMO

using GDGame.Controllers;
using GDGame.MyGame.Actors;
using GDGame.MyGame.Controllers;
using GDGame.MyGame.Managers;
using GDLibrary.Actors;
using GDLibrary.Containers;
using GDLibrary.Controllers;
using GDLibrary.Core.Controllers;
using GDLibrary.Core.Managers.State;
using GDLibrary.Debug;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Factories;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.MyGame;
using GDLibrary.Parameters;
using GDLibrary.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace GDGame
{
    public class Main : Game
    {
        #region Fields

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private bool isPaused;
        private int level;
        private bool levelLoaded;

        private CameraManager<Camera3D> cameraManager;
        private ObjectManager objectManager;
        private KeyboardManager keyboardManager;
        private MouseManager mouseManager;
        private RenderManager renderManager;
        private UIManager uiManager;
        private MyMenuManager menuManager;
        private SoundManager soundManager;

        //used to process and deliver events received from publishers
        private EventDispatcher eventDispatcher;

        //store useful game resources (e.g. effects, models, rails and curves)
        private Dictionary<string, BasicEffect> effectDictionary;

        //use ContentDictionary to store assets (i.e. file content) that need the Content.Load() method to be called
        private ContentDictionary<Texture2D> textureDictionary;

        private ContentDictionary<SpriteFont> fontDictionary;
        private ContentDictionary<Model> modelDictionary;

        //use normal Dictionary to store objects that do NOT need the Content.Load() method to be called (i.e. the object is not based on an asset file)
        private Dictionary<string, Transform3DCurve> transform3DCurveDictionary;

        //stores the rails used by the camera
        private Dictionary<string, RailParameters> railDictionary;

        //stores the archetypal primitive objects (used in Main and LevelLoader)
        private Dictionary<string, PrimitiveObject> archetypeDictionary;

        //defines centre point for the mouse i.e. (w/2, h/2)
        private Vector2 screenCentre;

        private CollidablePlayerObject collidablePlayerObject;

        #endregion Fields

        #region Constructors

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        #endregion Constructors

        #region Debug
#if DEBUG

        private void InitDebug()
        {
            InitDebugInfo(true);
            bool bShowCDCRSurfaces = true;
            bool bShowZones = true;
            InitializeDebugCollisionSkinInfo(bShowCDCRSurfaces, bShowZones, Color.White);
        }

        private void InitializeDebugCollisionSkinInfo(bool bShowCDCRSurfaces, bool bShowZones, Color boundingBoxColor)
        {
            //draws CDCR surfaces for boxes and spheres
            PrimitiveDebugDrawer primitiveDebugDrawer =
                new PrimitiveDebugDrawer(this, StatusType.Drawn | StatusType.Update,
                objectManager, cameraManager,
                bShowCDCRSurfaces, bShowZones);

            primitiveDebugDrawer.DrawOrder = 5;
            BoundingBoxDrawer.BoundingBoxColor = boundingBoxColor;

            Components.Add(primitiveDebugDrawer);
        }

        private void InitDebugInfo(bool bEnable)
        {
            if (bEnable)
            {
                //create the debug drawer to draw debug info
                DebugDrawer debugInfoDrawer = new DebugDrawer(this, _spriteBatch,
                    Content.Load<SpriteFont>("Assets/Fonts/debug"),
                    cameraManager, objectManager);

                //set the debug drawer to be drawn AFTER the object manager to the screen
                debugInfoDrawer.DrawOrder = 2;

                //add the debug drawer to the component list so that it will have its Update/Draw methods called each cycle.
                Components.Add(debugInfoDrawer);
            }
        }

#endif
        #endregion Debug

        #region Load - Assets

        private void LoadSounds()
        {
            soundManager.Add(new GDLibrary.Managers.Cue("lose",
                Content.Load<SoundEffect>("Assets/Audio/Effects/lose"), SoundCategoryType.WinLose, new Vector3(0.4f, 0, 0), false));

            soundManager.Add(new GDLibrary.Managers.Cue("win",
                Content.Load<SoundEffect>("Assets/Audio/Effects/win"), SoundCategoryType.WinLose, new Vector3(0.4f, 0, 0), false));

            soundManager.Add(new GDLibrary.Managers.Cue("buttonClick",
                Content.Load<SoundEffect>("Assets/Audio/Effects/buttonClick"), SoundCategoryType.WinLose, new Vector3(0.5f, 0, 0), false));

            soundManager.Add(new GDLibrary.Managers.Cue("GameSong",
                Content.Load<SoundEffect>("Assets/Audio/SoundTrack/GameSong"), SoundCategoryType.BackgroundMusic, new Vector3(0.5f, 0, 0), true));

            //to do..add more sounds
        }

        private void LoadEffects()
        {
            //to do...
            BasicEffect effect = null;

            //used for unlit primitives with a texture (e.g. textured quad of skybox)
            effect = new BasicEffect(_graphics.GraphicsDevice);
            effect.VertexColorEnabled = true; //otherwise we wont see RGB
            effect.TextureEnabled = true;
            effectDictionary.Add(GameConstants.Effect_UnlitTextured, effect);

            //used for wireframe primitives with no lighting and no texture (e.g. origin helper)
            effect = new BasicEffect(_graphics.GraphicsDevice);
            effect.VertexColorEnabled = true;
            effectDictionary.Add(GameConstants.Effect_UnlitWireframe, effect);

            //to do...add a new effect to draw a lit textured surface (e.g. a lit pyramid)
            effect = new BasicEffect(_graphics.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.LightingEnabled = true; //redundant?
            effect.PreferPerPixelLighting = true; //cost GPU cycles
            effect.EnableDefaultLighting();
            //change lighting position, direction and color

            effectDictionary.Add(GameConstants.Effect_LitTextured, effect);
        }

        private void LoadTextures()
        {
            //level 1 where each image 1_1, 1_2 is a different Y-axis height specificied when we use the level loader
            textureDictionary.Load("Assets/Textures/Level/level1_1");
            textureDictionary.Load("Assets/Textures/Level/level1_2");
            textureDictionary.Load("Assets/Textures/Level/level2_1");
            textureDictionary.Load("Assets/Textures/Level/level2_2");
            //add more levels here...

            textureDictionary.Load("Assets/Textures/Level/walls");

            //sky
            textureDictionary.Load("Assets/Textures/Skybox/back");
            textureDictionary.Load("Assets/Textures/Skybox/left");
            textureDictionary.Load("Assets/Textures/Skybox/right");
            textureDictionary.Load("Assets/Textures/Skybox/front");
            textureDictionary.Load("Assets/Textures/Skybox/sky");
            textureDictionary.Load("Assets/Textures/Foliage/Ground/grass1");
            textureDictionary.Load("Assets/Textures/Foliage/Ground/cobblestone");
            textureDictionary.Load("Assets/Textures/Foliage/Ground/grey");

            //demo
            textureDictionary.Load("Assets/Demo/Textures/checkerboard");

            //ui
            textureDictionary.Load("Assets/Textures/UI/Controls/progress_white");

            //props
            textureDictionary.Load("Assets/Textures/Props/Crates/crate1");

            //cubes
            textureDictionary.Load("Assets/Textures/Cubes/purpleCube");
            textureDictionary.Load("Assets/Textures/Cubes/blueCube");
            textureDictionary.Load("Assets/Textures/Cubes/redCube");
            textureDictionary.Load("Assets/Textures/Cubes/Warning");

            //menu
            textureDictionary.Load("Assets/Textures/UI/Controls/genericbtn");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/mainmenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/audiomenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/controlsmenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/exitmenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/exitmenuwithtrans");
            textureDictionary.Load("Assets/Textures/UI/Buttons/PlayButton");
            textureDictionary.Load("Assets/Textures/UI/Buttons/PlaycolButton");
            textureDictionary.Load("Assets/Textures/UI/Buttons/ExitButton");
            textureDictionary.Load("Assets/Textures/UI/Buttons/ExitcolButton");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/controls");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/endmenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/metal");

            //ui
            textureDictionary.Load("Assets/Textures/UI/Controls/reticuleDefault");

            //add more...
        }

        private void LoadFonts()
        {
            fontDictionary.Load("Assets/Fonts/debug");
            fontDictionary.Load("Assets/Fonts/menu");
            fontDictionary.Load("Assets/Fonts/ui");
        }

        #endregion Load - Assets

        #region Initialization - Graphics, Managers, Dictionaries, Cameras, Menu, UI

        protected override void Initialize()
        {
            float worldScale = 2000;

            //set game title
            Window.Title = "Zero Deaths";

            isPaused = false;
            level = 1; //change to 2 for second level
            levelLoaded = false;

            //graphic settings - see https://en.wikipedia.org/wiki/Display_resolution#/media/File:Vector_Video_Standards8.svg
            InitGraphics(1440, 1080);

            //note that we moved this from LoadContent to allow InitDebug to be called in Initialize
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //create event dispatcher
            InitEventDispatcher();

            //managers
            InitManagers();

            //dictionaries
            InitDictionaries();

            //load from file or initialize assets, effects and vertices
            LoadEffects();
            LoadTextures();
            LoadFonts();
            LoadSounds();

            //ui
            InitUI();
            InitMenu();

            //add archetypes that can be cloned
            InitArchetypes();

            //drawn content (collidable and noncollidable together - its simpler)
            InitLevel(worldScale);

            //curves and rails used by cameras
            InitCurves();
            InitRails();

            //cameras - notice we moved the camera creation BELOW where we created the drawn content - see DriveController
            InitCameras3D();

            #region Debug
#if DEBUG
            //debug info
            //InitDebug();
#endif
            #endregion Debug


            object[] parameters = { "GameSong" };
            EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
              EventActionType.OnPlay2D, parameters));
            

            base.Initialize();
        }

        private void InitGraphics(int width, int height)
        {
            //set resolution
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;

            //dont forget to apply resolution changes otherwise we wont see the new WxH
            _graphics.ApplyChanges();

            //set screen centre based on resolution
            screenCentre = new Vector2(width / 2, height / 2);

            //set cull mode to show front and back faces - inefficient but we will change later
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            _graphics.GraphicsDevice.RasterizerState = rs;

            //we use a sampler state to set the texture address mode to solve the aliasing problem between skybox planes
            SamplerState samplerState = new SamplerState();
            samplerState.AddressU = TextureAddressMode.Clamp;
            samplerState.AddressV = TextureAddressMode.Clamp;
            _graphics.GraphicsDevice.SamplerStates[0] = samplerState;

            //set blending
            _graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //set screen centre for use when centering mouse
            screenCentre = new Vector2(width / 2, height / 2);
        }

        private void InitUI()
        {
            Transform2D transform2D = null;
            Texture2D texture = null;
            SpriteFont spriteFont = null;

            //end screen background
            texture = textureDictionary["metal"];

            transform2D = new Transform2D(screenCentre, 0,
                Vector2.One,
                new Vector2(texture.Width / 2, texture.Height / 2),
                new Integer2(texture.Width, texture.Height));

            UITextureObject endMenu = new UITextureObject("End", ActorType.UITextureObject,
                StatusType.Drawn, transform2D, Color.White, 1, SpriteEffects.None, texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));

            //Death Count UI
            #region Death Count
            spriteFont = Content.Load<SpriteFont>("Assets/Fonts/menu");

            string text = "Death Count : 0";
            Vector2 originalDimensions = spriteFont.MeasureString(text);

                transform2D = new Transform2D(
              new Vector2(27 + originalDimensions.X / 2, 20 + originalDimensions.Y / 2), 0,
              Vector2.One,
              new Vector2(originalDimensions.X / 2, originalDimensions.Y / 2),
              new Integer2(originalDimensions));

            UITextObject deathCount = new UITextObject("deathCount", ActorType.UIText,
                StatusType.Drawn, transform2D,
               Color.Black, 0,
               SpriteEffects.None,
               text, spriteFont);

            DeathCountController controller = new DeathCountController("deathCountController", ControllerType.Progress, deathCount);
            deathCount.ControllerList.Add(controller);

            uiManager.Add(deathCount);

            MyGameStateManager gameStateManager = new MyGameStateManager(this,
                StatusType.Off, deathCount);

            //end screen button
            texture = textureDictionary["genericbtn"];

            transform2D = new Transform2D(screenCentre + new Vector2(0, 160), 0,
                Vector2.One,
                new Vector2(texture.Width / 2, texture.Height / 2),
                new Integer2(texture.Width, texture.Height));

            UIButtonObject button = new UIButtonObject("End_Button", ActorType.UITextureObject,
                StatusType.Drawn, transform2D, Color.White, 3, SpriteEffects.None, texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height),
                "Exit",
                fontDictionary["menu"],
                Vector2.One,
                Color.Black,
                new Vector2(0, 0));

            menuManager.Add("end", button);

            menuManager.Add("end", endMenu);

            menuManager.Add("end", deathCount);

            uiManager.Add(deathCount);

            Components.Add(gameStateManager);

            #endregion Text Object
        }

        private void InitMenu()
        {
            Texture2D texture = null;
            Transform2D transform2D = null;
            DrawnActor2D uiObject = null;
            Vector2 fullScreenScaleFactor = Vector2.One;

            #region All Menu Background Images
            //background main
            texture = textureDictionary["mainmenu"];
            fullScreenScaleFactor = new Vector2((float)_graphics.PreferredBackBufferWidth / texture.Width, (float)_graphics.PreferredBackBufferHeight / texture.Height);

            transform2D = new Transform2D(fullScreenScaleFactor);
            uiObject = new UITextureObject("main_bckgnd", ActorType.UITextureObject, StatusType.Drawn,
                transform2D, Color.White, 1, SpriteEffects.None, texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));
            menuManager.Add("main", uiObject);

            //background audio
            texture = textureDictionary["audiomenu"];
            fullScreenScaleFactor = new Vector2((float)_graphics.PreferredBackBufferWidth / texture.Width, (float)_graphics.PreferredBackBufferHeight / texture.Height);
            transform2D = new Transform2D(fullScreenScaleFactor);
            uiObject = new UITextureObject("audio_bckgnd", ActorType.UITextureObject, StatusType.Drawn,
                transform2D, Color.White, 1, SpriteEffects.None, texture, new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));
            menuManager.Add("audio", uiObject);

            //background controls
            texture = textureDictionary["controlsmenu"];
            fullScreenScaleFactor = new Vector2((float)_graphics.PreferredBackBufferWidth / texture.Width, (float)_graphics.PreferredBackBufferHeight / texture.Height);
            transform2D = new Transform2D(fullScreenScaleFactor);
            uiObject = new UITextureObject("controls_bckgnd", ActorType.UITextureObject, StatusType.Drawn,
                transform2D, Color.White, 1, SpriteEffects.None, texture, new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));
            menuManager.Add("controls", uiObject);

            #endregion All Menu Background Images

            //main menu buttons
            texture = textureDictionary["genericbtn"];

            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            Integer2 imageDimensions = new Integer2(texture.Width, texture.Height);

            //play
            transform2D = new Transform2D(screenCentre - new Vector2(0, -150), 0, Vector2.One, origin, imageDimensions);
            uiObject = new UIButtonObject("play", ActorType.UITextureObject,
                     StatusType.Update | StatusType.Drawn,
                transform2D, Color.White, 1, SpriteEffects.None, texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height),
                "Play",
                fontDictionary["menu"],
                new Vector2(1, 1),
                Color.Blue,
                new Vector2(0, 0));

            uiObject.ControllerList.Add(new UIMouseOverController("moc1", ControllerType.MouseOver,
             mouseManager, Color.Purple, Color.White));

            uiObject.ControllerList.Add(new UIScaleLerpController("slc1", ControllerType.ScaleLerpOverTime,
              mouseManager, new TrigonometricParameters(0.02f, 1, 0)));

            menuManager.Add("main", uiObject);


            //exit
            transform2D = new Transform2D(screenCentre + new Vector2(0, 225), 0, Vector2.One, origin, imageDimensions);
            uiObject = new UIButtonObject("exit", ActorType.UITextureObject,
                StatusType.Update | StatusType.Drawn,
             transform2D, Color.White, 1, SpriteEffects.None, texture,
             new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height),
             "Exit",
             fontDictionary["menu"],
             new Vector2(1, 1),
             Color.Blue,
             new Vector2(0, 0));

            uiObject.ControllerList.Add(new UIMouseOverController("moc1", ControllerType.MouseOver,
                 mouseManager, Color.Purple, Color.White));

            uiObject.ControllerList.Add(new UIScaleLerpController("slc1", ControllerType.ScaleLerpOverTime,
              mouseManager, new TrigonometricParameters(0.02f, 1, 0)));

            menuManager.Add("main", uiObject);

            //finally dont forget to SetScene to say which menu should be drawn/updated!
            menuManager.SetScene("main");
        }

        private void InitEventDispatcher()
        {
            eventDispatcher = new EventDispatcher(this);
            Components.Add(eventDispatcher);

            EventDispatcher.Subscribe(EventCategoryType.Player, HandleEvent);
        }

        private void HandleEvent(EventData eventData)
        {
            //changes level if the level count increases by 1
            if (eventData.EventCategoryType == EventCategoryType.Player)
            {
                if (eventData.EventActionType == EventActionType.OnWin)
                {
                    level = (int)eventData.Parameters[0];
                    levelLoaded = false;
                    cameraManager.ActiveCameraIndex = 1;
                }
            }
            //handles the event to change the scene
            else if (eventData.EventCategoryType == EventCategoryType.End)
            {
                if (eventData.EventActionType == EventActionType.OnGameOver)
                {
                    menuManager.SetScene("end");
                }
            }
        }

        private void InitCurves()
        {
            //create the camera curve to be applied to the track controller
            Transform3DCurve curveA = new Transform3DCurve(CurveLoopType.Constant); //experiment with other CurveLoopTypes
            curveA.Add(new Vector3(100, 120, 800), new Vector3(5, -3, -2), Vector3.UnitY, 0); //start
            curveA.Add(new Vector3(150, 120, 400), new Vector3(5, -5, 0), Vector3.UnitY, 4000); //start position
            curveA.Add(new Vector3(200, 120, 300), new Vector3(5, -3, -2), Vector3.UnitY, 6000); //start position
            curveA.Add(new Vector3(200, 120, 200), new Vector3(5, -3, -2), Vector3.UnitY, 8000); //start position
            curveA.Add(new Vector3(200, 120, 100), new Vector3(5, -3, -2), Vector3.UnitY, 10000); //start position
            curveA.Add(new Vector3(350, 120, 100), new Vector3(5, -5, 0), Vector3.UnitY, 12000); //start position
            curveA.Add(new Vector3(350, 30, 80), new Vector3(5, -2, 0), Vector3.UnitY, 14000); //start position
            curveA.Add(new Vector3(400, 10, 80), new Vector3(5, -1, 0), Vector3.UnitY, 16000); //start position
            curveA.Add(new Vector3(400, 10, 80), new Vector3(5, -1, 0), Vector3.UnitY, 16000); //start position

            //add to the dictionary
            transform3DCurveDictionary.Add("intro", curveA);
        }

        private void InitRails()
        {
            //create the track to be applied to the non-collidable track camera 1
            railDictionary.Add("rail1", new RailParameters("rail1 - parallel to z-axis", new Vector3(20, 10, 50), new Vector3(20, 10, -50)));
        }

        private void InitCameras3D()
        {
            Transform3D transform3D = null;
            Camera3D camera3D = null;
            Viewport viewPort = new Viewport(0, 0, 1440, 1080);

            #region Noncollidable Camera - Curve3D

            //notice that it doesnt matter what translation, look, and up are since curve will set these
            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero, Vector3.Zero);

            camera3D = new Camera3D(GameConstants.Camera_NonCollidableCurveMainArena,
              ActorType.Camera3D, StatusType.Update, transform3D,
                        ProjectionParameters.StandardDeepSixteenTen, viewPort);

            camera3D.ControllerList.Add(
                new Curve3DController(GameConstants.Controllers_NonCollidableCurveMainArena,
                ControllerType.Curve,
                        transform3DCurveDictionary["intro"])); //use the curve dictionary to retrieve a transform3DCurve by id

            cameraManager.Add(camera3D);

            #endregion Noncollidable Camera - Curve3D

            #region Collidable Camera - 3rd Person

            transform3D = new Transform3D(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);

            camera3D = new Camera3D(GameConstants.Camera_CollidableThirdPerson,
                ActorType.Camera3D, StatusType.Update, transform3D,
                ProjectionParameters.StandardDeepSixteenTen,
                new Viewport(0, 0, 1440, 1080));

            //attach a controller
            camera3D.ControllerList.Add(new ThirdPersonController(
                GameConstants.Controllers_CollidableThirdPerson,
                ControllerType.ThirdPerson,
                collidablePlayerObject,
                170,
                25,
                1,
                mouseManager));
            cameraManager.Add(camera3D);

            #endregion Collidable Camera - 3rd Person

            cameraManager.ActiveCameraIndex = 0; //0, 1, 2, 3

            if (level == 2)
            {
                cameraManager.ActiveCameraIndex = 1;
            }

        }

        private void InitDictionaries()
        {
            //stores effects
            effectDictionary = new Dictionary<string, BasicEffect>();

            //stores textures, fonts & models
            modelDictionary = new ContentDictionary<Model>("models", Content);
            textureDictionary = new ContentDictionary<Texture2D>("textures", Content);
            fontDictionary = new ContentDictionary<SpriteFont>("fonts", Content);

            //curves - notice we use a basic Dictionary and not a ContentDictionary since curves and rails are NOT media content
            transform3DCurveDictionary = new Dictionary<string, Transform3DCurve>();

            //rails - store rails used by cameras
            railDictionary = new Dictionary<string, RailParameters>();

            //used to store archetypes for primitives in the game
            archetypeDictionary = new Dictionary<string, PrimitiveObject>();
        }

        private void InitManagers()
        {
            //physics and CD-CR (moved to top because MouseManager is dependent)
            //to do - replace with simplified CDCR

            //camera
            cameraManager = new CameraManager<Camera3D>(this, StatusType.Off);
            Components.Add(cameraManager);

            //keyboard
            keyboardManager = new KeyboardManager(this);
            Components.Add(keyboardManager);

            //mouse
            mouseManager = new MouseManager(this, true, screenCentre);
            Components.Add(mouseManager);

            //object
            objectManager = new ObjectManager(this, StatusType.Off, 6, 10);
            Components.Add(objectManager);

            //render
            renderManager = new RenderManager(this, StatusType.Drawn, ScreenLayoutType.Single,
                objectManager, cameraManager);
            Components.Add(renderManager);

            //add in-game ui
            uiManager = new UIManager(this, StatusType.Off, _spriteBatch, 10);
            uiManager.DrawOrder = 4;
            Components.Add(uiManager);

            //add menu
            menuManager = new MyMenuManager(this, StatusType.Update | StatusType.Drawn, _spriteBatch,
                mouseManager, keyboardManager);
            menuManager.DrawOrder = 5; //highest number of all drawable managers since we want it drawn on top!
            Components.Add(menuManager);

            //sound
            soundManager = new SoundManager(this, StatusType.Update);
            Components.Add(soundManager);
        }

        #endregion Initialization - Graphics, Managers, Dictionaries, Cameras, Menu, UI

        #region Initialization - Vertices, Archetypes, Helpers, Drawn Content(e.g. Skybox)

        /// <summary>
        /// Creates archetypes used in the game.
        ///
        /// What are the steps required to add a new primitive?
        ///    1. In the VertexFactory add a function to return Vertices[]
        ///    2. Add a new BasicEffect IFF this primitive cannot use existing effects(e.g.wireframe, unlit textured)
        ///    3. Add the new effect to effectDictionary
        ///    4. Create archetypal PrimitiveObject.
        ///    5. Add archetypal object to archetypeDictionary
        ///    6. Clone archetype, change its properties (transform, texture, color, alpha, ID) and add manually to the objectmanager or you can use LevelLoader.
        /// </summary>
        private void InitArchetypes() //formerly InitTexturedQuad
        {
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;
            IVertexData vertexData = null;
            PrimitiveType primitiveType; 
            int primitiveCount;

            /************************* Non-Collidable  *************************/

            #region Lit Textured Pyramid

            /*********** Transform, Vertices and VertexData ***********/
            //lit pyramid
            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero,
                 Vector3.One, Vector3.UnitZ, Vector3.UnitY);
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["checkerboard"], Color.White, 1);

            VertexPositionNormalTexture[] vertices
                = VertexFactory.GetVerticesPositionNormalTexturedPyramid(out primitiveType,
                out primitiveCount);

            //analog of the Model class in G-CA (i.e. it holdes vertices and type, count)
            vertexData = new VertexData<VertexPositionNormalTexture>(vertices,
                primitiveType, primitiveCount);

            /*********** PrimitiveObject ***********/
            //now we use the "FBX" file (our vertexdata) and make a PrimitiveObject
            PrimitiveObject primitiveObject = new PrimitiveObject(
                GameConstants.Primitive_LitTexturedPyramid,
                ActorType.Decorator, //we could specify any time e.g. Pickup
                StatusType.Drawn,
                transform3D, effectParameters,
                vertexData);

            /*********** Controllers (optional) ***********/
            //we could add controllers to the archetype and then all clones would have cloned controllers
            //  drawnActor3D.ControllerList.Add(
            //new RotationController("rot controller1", ControllerType.RotationOverTime,
            //1, new Vector3(0, 1, 0)));

            //to do...add demos of controllers on archetypes
            //ensure that the Clone() method of PrimitiveObject will Clone() all controllers

            archetypeDictionary.Add(primitiveObject.ID, primitiveObject);
            #endregion Lit Textured Pyramid

            #region Unlit Textured Quad
            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero,
                  Vector3.One, Vector3.UnitZ, Vector3.UnitY);

            effectParameters = new EffectParameters(
                effectDictionary[GameConstants.Effect_UnlitTextured],
                textureDictionary["grass1"], Color.White, 1);

            vertexData = new VertexData<VertexPositionColorTexture>(
                VertexFactory.GetTextureQuadVertices(out primitiveType, out primitiveCount),
                primitiveType, primitiveCount);

            archetypeDictionary.Add(GameConstants.Primitive_UnlitTexturedQuad,
                new PrimitiveObject(GameConstants.Primitive_UnlitTexturedQuad,
                ActorType.Decorator,
                StatusType.Update | StatusType.Drawn,
                transform3D, effectParameters, vertexData));
            #endregion Unlit Textured Quad

            #region Lit Textured Cube

            /*********** Transform, Vertices and VertexData ***********/
            //lit cube
            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero,
                 Vector3.One, Vector3.UnitZ, Vector3.UnitY);
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["checkerboard"], Color.White, 1);

                vertices
                = VertexFactory.GetVerticesPositionNormalTexturedCube(1, out primitiveType,
                out primitiveCount);

            //analog of the Model class in G-CA (i.e. it holdes vertices and type, count)
            vertexData = new VertexData<VertexPositionNormalTexture>(vertices,
                primitiveType, primitiveCount);

            /*********** PrimitiveObject ***********/
                primitiveObject = new PrimitiveObject(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.Decorator, //we could specify any time e.g. Pickup
                StatusType.Drawn,
                transform3D, effectParameters,
                vertexData);


            archetypeDictionary.Add(primitiveObject.ID, primitiveObject);
            #endregion Lit Textured Cube

            #region Unlit Origin Helper
            transform3D = new Transform3D(new Vector3(0, 20, 0),
                     Vector3.Zero, new Vector3(10, 10, 10),
                     Vector3.UnitZ, Vector3.UnitY);

            effectParameters = new EffectParameters(
                effectDictionary[GameConstants.Effect_UnlitWireframe],
                null, Color.White, 1);

            vertexData = new VertexData<VertexPositionColor>(VertexFactory.GetVerticesPositionColorOriginHelper(
                                    out primitiveType, out primitiveCount),
                                    primitiveType, primitiveCount);

            archetypeDictionary.Add(GameConstants.Primitive_WireframeOriginHelper,
                new PrimitiveObject(GameConstants.Primitive_WireframeOriginHelper,
                ActorType.Helper,
                StatusType.Update | StatusType.Drawn,
                transform3D, effectParameters, vertexData));

            #endregion Unlit Origin Helper

            //add more archetypes here...
        }

        private void InitLevel(float worldScale)//, List<string> levelNames)
        {
            //remove any old content (e.g. on restart or next level)
            
            if (level == 1)
            {
                objectManager.Clear();

                /************ Non-collidable ************/
                //adds origin helper etc
                InitHelpers();

                //add skybox
                InitSkybox(worldScale);

                //add grass plane
                InitGround(worldScale);

                //cube
                InitDecorators();

                /************ Collidable ************/

                InitCollidableEnemy1();

                InitCollidablePickups1();

                InitializeCollidablePlayer();

                /************ Level-loader (can be collidable or non-collidable) ************/

                LevelLoader<PrimitiveObject> levelLoader = new LevelLoader<PrimitiveObject>(
                    archetypeDictionary, textureDictionary, objectManager, effectDictionary);
                List<DrawnActor3D> actorList = null;

                //add level1_1 contents
                actorList = levelLoader.Load(
                    textureDictionary["level1_1"],
                                    10,     //number of in-world x-units represented by 1 pixel in image
                                    10,     //number of in-world z-units represented by 1 pixel in image
                                    15,     //y-axis height offset
                                    new Vector3(0, 0, 0) //offset to move all new objects by
                                    );
                objectManager.Add(actorList);

                //clear the list otherwise when we add level1_2 we would re-add level1_1 objects to object manager
                actorList.Clear();

                actorList = levelLoader.Load(
                   textureDictionary["level1_2"],
                                   10,     //number of in-world x-units represented by 1 pixel in image
                                   10,     //number of in-world z-units represented by 1 pixel in image
                                   0,     //y-axis height offset
                                   new Vector3(0, 0, 0) //offset to move all new objects by
                                   );
                objectManager.Add(actorList);
            }

            else if (level == 2)
            {
                objectManager.Clear();

                /************ Non-collidable ************/
                //adds origin helper etc
                InitHelpers();

                //add skybox
                InitSkybox(worldScale);

                //add grass plane
                InitGround(worldScale);

                /************ Collidable ************/

                InitCollidableEnemy2();

                InitCollidablePickups2();

                InitializeCollidablePlayer();

                /************ Level-loader (can be collidable or non-collidable) ************/

                LevelLoader<PrimitiveObject> levelLoader = new LevelLoader<PrimitiveObject>(
                    archetypeDictionary, textureDictionary, objectManager, effectDictionary);
                List<DrawnActor3D> actorList = null;

                //add level1_1 contents
                actorList = levelLoader.Load(
                    textureDictionary["level2_1"],
                                    10,     //number of in-world x-units represented by 1 pixel in image
                                    10,     //number of in-world z-units represented by 1 pixel in image
                                    15,     //y-axis height offset
                                    new Vector3(0, 0, 0) //offset to move all new objects by
                                    );
                objectManager.Add(actorList);

                //clear the list otherwise when we add level1_2 we would re-add level1_1 objects to object manager
                actorList.Clear();


                actorList = levelLoader.Load(
                   textureDictionary["level2_2"],
                                   10,     //number of in-world x-units represented by 1 pixel in image
                                   10,     //number of in-world z-units represented by 1 pixel in image
                                   0,     //y-axis height offset
                                   new Vector3(0, 0, 0) //offset to move all new objects by
                                   );
                objectManager.Add(actorList);
                
            }

            levelLoaded = true;

        }

        #region NEW - 26.12.20

        //content for level 1
        #region Level 1
        private void InitializeCollidablePlayer()
        {
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;
            IVertexData vertexData = null;
            ICollisionPrimitive collisionPrimitive = null;
            PrimitiveType primitiveType;
            int primitiveCount;

            //set the position
            transform3D = new Transform3D(new Vector3(170, 2.5f, 500), Vector3.Zero, new Vector3(5, 5, 5),
                -Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["purpleCube"], Color.White, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedCube(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make a CDCR surface - sphere or box, its up to you - you dont need to pass transform to either primitive anymore
            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //if we make this a field then we can pass to the 3rd person camera controller
            collidablePlayerObject
                = new CollidablePlayerObject("collidable player1",
                    //this is important as it will determine how we filter collisions in our collidable player CDCR code
                    ActorType.CollidablePlayer,
                    StatusType.Drawn | StatusType.Update,
                    transform3D,
                    effectParameters,
                    vertexData,
                    collisionPrimitive,
                    objectManager,
                    GameConstants.KeysOne,
                    GameConstants.playerMoveSpeed,
                    GameConstants.playerRotateSpeed,
                    keyboardManager,
                    this);

            objectManager.Add(collidablePlayerObject);
        }

        private void InitCollidableEnemy1()
        {
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;
            IVertexData vertexData = null;
            ICollisionPrimitive collisionPrimitive = null;
            CollidableEnemy collidablePrimitiveObject = null;
            PrimitiveType primitiveType;
            int primitiveCount;

            /************************* enemy 1 *************************/

            transform3D = new Transform3D(new Vector3(320, 3, 320), Vector3.Zero, new Vector3(5, 5, 5), Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["redCube"], Color.White, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedCube(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidableEnemy(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.Enemy,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager, 1, 1, collidablePlayerObject);
                

            //add to the archetype dictionary
            objectManager.Add(collidablePrimitiveObject);

            /************************* Enemy 2 *************************/

            transform3D = new Transform3D(new Vector3(400, 3, 480), Vector3.Zero, new Vector3(5, 5, 5), Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["redCube"], Color.White, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedCube(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidableEnemy(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.Enemy,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager, 1, 1, collidablePlayerObject);

            //add to the archetype dictionary
            objectManager.Add(collidablePrimitiveObject);

            /************************* Enemy 3 *************************/

            transform3D = new Transform3D(new Vector3(330, 3, 120), Vector3.Zero, new Vector3(5, 5, 5), Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["redCube"], Color.White, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedCube(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidableEnemy(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.Enemy,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager, 1, 1, collidablePlayerObject);

            //add to the archetype dictionary
            objectManager.Add(collidablePrimitiveObject);

            /************************* Enemy 4 *************************/

            transform3D = new Transform3D(new Vector3(330, 3, 40), Vector3.Zero, new Vector3(5, 5, 5), Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["redCube"], Color.White, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedCube(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidableEnemy(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.Enemy,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager, 1, 1, collidablePlayerObject);

            //add to the archetype dictionary
            objectManager.Add(collidablePrimitiveObject);
        }

        private void InitCollidablePickups1()
        {
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;
            IVertexData vertexData = null;
            ICollisionPrimitive collisionPrimitive = null;
            CollidablePrimitiveObject collidablePrimitiveObject = null;
            PrimitiveType primitiveType;
            int primitiveCount;

            /************************* Sphere Collision Primitive  *************************/

            transform3D = new Transform3D(new Vector3(450, 4, 80), Vector3.Zero, new Vector3(5, 5, 5), Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_UnlitTextured],
                textureDictionary["purpleCube"], Color.White, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionColorTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedOctahedron(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new SphereCollisionPrimitive(transform3D, 10);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidablePrimitiveObject(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.CollidablePickup,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager);

            //add to the archetype dictionary
            objectManager.Add(collidablePrimitiveObject);

            }

        #endregion

        //content for level 2
        #region Level 2

        private void InitCollidableEnemy2()
        {
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;
            IVertexData vertexData = null;
            ICollisionPrimitive collisionPrimitive = null;
            CollidableEnemy collidablePrimitiveObject = null;
            PrimitiveType primitiveType;
            int primitiveCount;

            /************************* enemy 1 *************************/

            transform3D = new Transform3D(new Vector3(440, 3, 80), Vector3.Zero, new Vector3(5, 5, 5), Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["redCube"], Color.White, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedCube(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidableEnemy(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.Enemy,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager, 1, 1, collidablePlayerObject);


            //add to the archetype dictionary
            objectManager.Add(collidablePrimitiveObject);

            /************************* Enemy 2 *************************/

            transform3D = new Transform3D(new Vector3(420, 3, 280), Vector3.Zero, new Vector3(5, 5, 5), Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["redCube"], Color.White, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedCube(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidableEnemy(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.Enemy,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager, 1, 1, collidablePlayerObject);

            //add to the archetype dictionary
          
            objectManager.Add(collidablePrimitiveObject);

            /************************* Enemy 3 *************************/

            transform3D = new Transform3D(new Vector3(490, 3, 550), Vector3.Zero, new Vector3(5, 5, 5), Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["redCube"], Color.White, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedCube(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidableEnemy(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.Enemy,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager, 1, 1, collidablePlayerObject);

            //add to the archetype dictionary

            objectManager.Add(collidablePrimitiveObject);

        }

        private void InitCollidablePickups2()
        {
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;
            IVertexData vertexData = null;
            ICollisionPrimitive collisionPrimitive = null;
            CollidablePrimitiveObject collidablePrimitiveObject = null;
            PrimitiveType primitiveType;
            int primitiveCount;

            /************************* Sphere Collision Primitive  *************************/

            transform3D = new Transform3D(new Vector3(570, 4, 600), Vector3.Zero, new Vector3(5, 5, 5), Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_UnlitTextured],
                textureDictionary["purpleCube"], Color.White, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionColorTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedOctahedron(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new SphereCollisionPrimitive(transform3D, 10);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidablePrimitiveObject(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.CollidablePickup,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager);

            //add to the archetype dictionary
            objectManager.Add(collidablePrimitiveObject);
        }

        #endregion

        #endregion NEW - 26.12.20

        /// <summary>
        /// Demos how we can clone an archetype and manually add to the object manager.
        /// </summary>
        private void InitDecorators()
        {
            //clone the archetypal Pyramid
            PrimitiveObject drawnActor3D
                = archetypeDictionary[GameConstants.Primitive_LitTexturedCube].Clone() as PrimitiveObject;

            //change it a bit
            drawnActor3D.ID = "Cube1";
            drawnActor3D.Transform3D.Scale = 10 * new Vector3(2, 2, 2);
            drawnActor3D.EffectParameters.Texture = textureDictionary["controls"];
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(90, 0, 0);
            drawnActor3D.Transform3D.Translation = new Vector3(135, 15, 436);
            drawnActor3D.EffectParameters.Alpha = 1;


            objectManager.Add(drawnActor3D);
        }

        private void InitHelpers()
        {
            //clone the archetype
            PrimitiveObject originHelper = archetypeDictionary[GameConstants.Primitive_WireframeOriginHelper].Clone() as PrimitiveObject;
            //add to the dictionary
            objectManager.Add(originHelper);
        }

        private void InitGround(float worldScale)
        {
            PrimitiveObject drawnActor3D = archetypeDictionary[GameConstants.Primitive_UnlitTexturedQuad].Clone() as PrimitiveObject;
            drawnActor3D.ActorType = ActorType.Ground;
            drawnActor3D.EffectParameters.Texture = textureDictionary["grey"];
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(-90, 0, 0);
            drawnActor3D.Transform3D.Scale = worldScale * Vector3.One;
            objectManager.Add(drawnActor3D);
        }

        private void InitSkybox(float worldScale)
        {
            PrimitiveObject drawnActor3D = null;

            //back
            drawnActor3D = archetypeDictionary[GameConstants.Primitive_UnlitTexturedQuad].Clone() as PrimitiveObject;
            drawnActor3D.ActorType = ActorType.Sky;

            //  primitiveObject.StatusType = StatusType.Off; //Experiment of the effect of StatusType
            drawnActor3D.ID = "sky back";
            drawnActor3D.EffectParameters.Texture = textureDictionary["back"]; ;
            drawnActor3D.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            drawnActor3D.Transform3D.Translation = new Vector3(0, 0, -worldScale / 2.0f);
            objectManager.Add(drawnActor3D);

            //left
            drawnActor3D = archetypeDictionary[GameConstants.Primitive_UnlitTexturedQuad].Clone() as PrimitiveObject;
            drawnActor3D.ActorType = ActorType.Sky;
            drawnActor3D.ID = "left back";
            drawnActor3D.EffectParameters.Texture = textureDictionary["left"]; ;
            drawnActor3D.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(0, 90, 0);
            drawnActor3D.Transform3D.Translation = new Vector3(-worldScale / 2.0f, 0, 0);
            objectManager.Add(drawnActor3D);

            //right
            drawnActor3D = archetypeDictionary[GameConstants.Primitive_UnlitTexturedQuad].Clone() as PrimitiveObject;
            drawnActor3D.ActorType = ActorType.Sky;
            drawnActor3D.ID = "sky right";
            drawnActor3D.EffectParameters.Texture = textureDictionary["right"];
            drawnActor3D.Transform3D.Scale = new Vector3(worldScale, worldScale, 20);
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(0, -90, 0);
            drawnActor3D.Transform3D.Translation = new Vector3(worldScale / 2.0f, 0, 0);
            objectManager.Add(drawnActor3D);

            //top
            drawnActor3D = archetypeDictionary[GameConstants.Primitive_UnlitTexturedQuad].Clone() as PrimitiveObject;
            drawnActor3D.ActorType = ActorType.Sky;
            drawnActor3D.ID = "sky top";
            drawnActor3D.EffectParameters.Texture = textureDictionary["sky"];
            drawnActor3D.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(90, -90, 0);
            drawnActor3D.Transform3D.Translation = new Vector3(0, worldScale / 2.0f, 0);
            objectManager.Add(drawnActor3D);

            //front
            drawnActor3D = archetypeDictionary[GameConstants.Primitive_UnlitTexturedQuad].Clone() as PrimitiveObject;
            drawnActor3D.ActorType = ActorType.Sky;
            drawnActor3D.ID = "sky front";
            drawnActor3D.EffectParameters.Texture = textureDictionary["front"];
            drawnActor3D.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(0, 180, 0);
            drawnActor3D.Transform3D.Translation = new Vector3(0, 0, worldScale / 2.0f);
            objectManager.Add(drawnActor3D);
        }

        #endregion Initialization - Vertices, Archetypes, Helpers, Drawn Content(e.g. Skybox)

        #region Load & Unload Game Assets

        protected override void LoadContent()
        {
        }

        protected override void UnloadContent()
        {
            //housekeeping - unload content
            textureDictionary.Dispose();
            modelDictionary.Dispose();
            fontDictionary.Dispose();
            modelDictionary.Dispose();
            soundManager.Dispose();

            base.UnloadContent();
        }

        #endregion Load & Unload Game Assets

        #region Update & Draw

        protected override void Update(GameTime gameTime)
        {
            if (keyboardManager.IsFirstKeyPress(Keys.Escape))
            {
                if (isPaused) //menu -> game
                {
                    EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPause,
                        new object[] { gameTime }));
                }
                else //game -> menu
                {
                    EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPlay,
                    new object[] { gameTime }));
                }
                isPaused = !isPaused;
            }

            #region Demo
#if DEMO

            #region Object Manager
            if (!isPaused)
            {
                EventDispatcher.Publish(new EventData(
                EventCategoryType.Object,
                EventActionType.OnApplyActionToFirstMatchActor,
                (actor) => actor.StatusType = StatusType.Drawn | StatusType.Update, //Action
                (actor) => actor.ActorType == ActorType.Decorator
                && actor.ID.Equals("cube1"), //Predicate
                null //parameters
                ));
            }
            #endregion Object Manager

            #region Sound Demos
          
            if (keyboardManager.IsFirstKeyPress(Keys.F4))
            {
                soundManager.SetMasterVolume(0);
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F5))
            {
                soundManager.SetMasterVolume(0.5f);
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F6))
            {
                AudioListener listener = new AudioListener();
                listener.Position = new Vector3(0, 5, 50);
                listener.Forward = -Vector3.UnitZ;
                listener.Up = Vector3.UnitY;

                AudioEmitter emitter = new AudioEmitter();
                emitter.DopplerScale = 1;
                emitter.Position = new Vector3(0, 5, 0);
                emitter.Forward = Vector3.UnitZ;
                emitter.Up = Vector3.UnitY;
            }
            #endregion Sound Demos

            #region Menu & UI Demos
            if (keyboardManager.IsFirstKeyPress(Keys.F9))
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPause, null));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F10))
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPlay, null));
            }

            if (keyboardManager.IsFirstKeyPress(Keys.Up))
            {
                object[] parameters = { 1 }; //will increase the progress by 1 to its max of 10 (see InitUI)
                EventDispatcher.Publish(new EventData(EventCategoryType.UI, EventActionType.OnHealthDelta, parameters));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.Down))
            {
                object[] parameters = { -1 }; //will decrease the progress by 1 to its min of 0 (see InitUI)
                EventDispatcher.Publish(new EventData(EventCategoryType.UI, EventActionType.OnHealthDelta, parameters));
            }

            if (keyboardManager.IsFirstKeyPress(Keys.F5)) //game -> menu
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPlay, null));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F6)) //menu -> game
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPause, null));
            }
            #endregion Menu & UI Demos

            #region Camera
            //camera cycles when c is pressed
            if (keyboardManager.IsFirstKeyPress(Keys.C))
            {
                cameraManager.CycleActiveCamera();
                EventDispatcher.Publish(new EventData(EventCategoryType.Camera,
                    EventActionType.OnCameraCycle, null));
            }

            //if (cameraManager.ActiveCameraIndex == 0)
            //{
            //    if ()
            //    {
            //        cameraManager.CycleActiveCamera();
            //        EventDispatcher.Publish(new EventData(EventCategoryType.Camera,
            //            EventActionType.OnCameraCycle, null));
            //    }
            //}

            #endregion Camera


#endif
            #endregion Demo

            //reloads the level if the level loader is true
            if (!levelLoaded)
            {
                InitLevel(2000);

            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }

        #endregion Update & Draw
    }
}
