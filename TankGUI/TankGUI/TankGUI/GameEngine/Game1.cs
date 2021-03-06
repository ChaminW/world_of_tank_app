using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TankGUI.socket;
using TankGUI.common;
using System.Threading;
using System.Text;
using TankGUI.decode;

namespace TankGUI.GameEngine
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    public struct PlayerData
    {
        public Vector2 Position;
        public bool IsAlive;
        public Color Color;
        public float Angle;
        public float Power;

        public String playerName;
        public int playerNum;
        public String Direction;
        public int IsShot;
        public int Health;
        public int Coin;
        public int Point;
        public String Detail;
    }

    public struct cellData
    {
        public Vector2 Position;
        public String type;  // W-wall   S-stone  L-water   c-coin
        public String Direction;

    }

    


    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GraphicsDevice device;

        Texture2D backgroundTexture;
        Texture2D foregroundTexture;
        Texture2D markTexture;

        Texture2D cannonTexture;
        Texture2D tankTexture;
        Texture2D shellTexture;
        Texture2D layoutTexture;

        bool shellFlying = false;
        Vector2 shellPosition;
        //Vector2 rocketDirection;
        //float rocketAngle;
        //float rocketScaling = 1f;

        String err = "";

        int screenWidth;
        int screenHeight;
        SpriteFont font;
        SpriteFont font1;

        float playerScaling;


        PlayerData[] players;
        cellData[][] cells;

        

        int numberOfPlayers = 6;

        int currentPlayer = 1;

       


        Decode TempDecode;
        client client1;
        server serverCon;
        Thread serverThread;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            
            client1 = new client();
            serverCon = new server();
            TempDecoder = serverCon.getDecode();
            serverThread = new Thread(new ThreadStart(() => serverCon.waitForConnection()));


            new socket.client().sendData(common.parameters.UP);


            client1.sendData(parameters.JOIN);
            serverThread.Start();

            

            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            graphics.PreferredBackBufferWidth = 1050;
            graphics.PreferredBackBufferHeight = 720;

            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            
            
           

            Window.Title = "World of Tank-SL";

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            device = graphics.GraphicsDevice;

            layoutTexture = Content.Load<Texture2D>("plane");
            backgroundTexture = Content.Load<Texture2D>("field2");
            foregroundTexture = Content.Load<Texture2D>("foreground");
            markTexture = Content.Load<Texture2D>("markBoard");


            cannonTexture = Content.Load<Texture2D>("cannon");
            tankTexture = Content.Load<Texture2D>("tank");
            shellTexture = Content.Load<Texture2D>("shell0");

            screenWidth = device.PresentationParameters.BackBufferWidth;
            screenHeight = device.PresentationParameters.BackBufferHeight;

            font = Content.Load<SpriteFont>("myFont");
            font1 = Content.Load<SpriteFont>("SpriteFont1");


            SetUpPlayers();
            SetupCells();

            playerScaling = 40.0f / (float)tankTexture.Width;

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            ProcessKeyboard();
            UpdateShell();
            err = "";
            UpdateCells();
            UpdateMark();

            base.Update(gameTime);
        }

       

        public void UpdateCells()
        {
            List<List<string>> currentGrid = TempDecoder.getGrid();




            for (int i = 0; i < 10; i++)
            {

                for (int j = 0; j < 10; j++)
                {

                    
                        cells[i][j].type = currentGrid[j][i];
                    

                }
            }
        }

        public void UpdateMark()
        {
            List<List<string>> MarkList = TempDecoder.getMarkList();

            int count;
            try { 

            count = MarkList.Count();

            }
            catch (Exception e)
            {

                count = 0;

            }

            if (count>1)

            {
                for (int i = 1; i < count; i++)
                {

                    players[i].playerName = MarkList[i][0];
                    players[i].playerNum = players[i].playerName[1];

                    //int x2 = (list2[2][1].Split(',').Select(int.Parse).ToList())[0];
                    //int y2 = (list2[2][1].Split(',').Select(int.Parse).ToList())[1];
                    players[i].Direction = MarkList[i][2];
                
                
                    switch (players[i].Direction)
                    {
                        case "0":
                            players[i].Angle=MathHelper.ToRadians(0);
                            break;
                        case "1":
                            players[i].Angle = MathHelper.ToRadians(90);
                            break;
                        case "2":
                            players[i].Angle = MathHelper.ToRadians(180);
                            break;
                        case "3":
                            players[i].Angle = MathHelper.ToRadians(270);
                            
                            break;
                        default:
                            //ErrorViwer.view("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                            break;
                   }

                    try
                    {
                        players[i].Detail = MarkList[i][0] + "       " + MarkList[i][4] + "       " + MarkList[i][5] + "       " + MarkList[i][6];
                        //players[i].IsShot = Int32.Parse(MarkList[i][3]);
                        //players[i].Health = Int32.Parse(MarkList[i][4]);
                        //players[i].Coin = Int32.Parse(MarkList[i][5]);
                        //players[i].Point = Int32.Parse(MarkList[i][6]);


                    }
                    catch (Exception e)
                    {
                        //ErrorViwer.view("xxxx "+MarkList[i][3]);
                        //ErrorViwer.view("xxxxx " + MarkList[i][4]);
                        //ErrorViwer.view("xxxxxx " + MarkList[i][5]);
                        //ErrorViwer.view("xxxxxx  " + MarkList[i][6]);
                    }

            
                }
            }
        }



        private void UpdateShell()
        {
            if (shellFlying)
            {
                //Vector2 gravity = new Vector2(0, 1);
                //rocketDirection += gravity / 10.0f;
                //rocketPosition += rocketDirection;
                //rocketAngle = (float)Math.Atan2(rocketDirection.X, -rocketDirection.Y);
                /*
                switch (players[currentPlayer].Direction)
                {
                    case "Left":
                        players[currentPlayer].Position.X += 1f;
                        break;
                    case "right":
                        //
                        break;
                    default:
                        //sdcss


                }
                */

            }
        }



        private void ProcessKeyboard()
        {
            KeyboardState keybState = Keyboard.GetState();
            if (keybState.IsKeyDown(Keys.Left))
                new socket.client().sendData(common.parameters.LEFT);
            if (keybState.IsKeyDown(Keys.Right))
                new socket.client().sendData(common.parameters.RIGHT);
          
            if (keybState.IsKeyDown(Keys.Down))
                new socket.client().sendData(common.parameters.DOWN);

            if (keybState.IsKeyDown(Keys.Up))
                new socket.client().sendData(common.parameters.UP);

            if (keybState.IsKeyDown(Keys.Enter) || keybState.IsKeyDown(Keys.Space))
            {
                new socket.client().sendData(common.parameters.SHOOT);
            }

        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            
            graphics.GraphicsDevice.Clear(Color.Gray);

            spriteBatch.Begin();
            DrawScenery();
            //DrawPlayers();
            DrawCells();
            
            //ConsolePrint();

            DrawShell();
            DrawText();

            spriteBatch.End();

            

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private void DrawScenery()
        {

            Rectangle layoutRectangle = new Rectangle(0, 0, 1050, 700);
            Rectangle screenRectangle = new Rectangle(100, 100, 500, 500);
            Rectangle markRectangle = new Rectangle(620, 50, 400, 600);

            spriteBatch.Draw(layoutTexture, layoutRectangle, Color.White);
            spriteBatch.Draw(backgroundTexture, screenRectangle, Color.White);
           

            spriteBatch.Draw(markTexture, markRectangle, Color.White);
        }

        private void DrawPlayers()
        {
            foreach (PlayerData player in players)
            {
                if (player.IsAlive)
                {

                    //int xPos = (int)player.Position.X;
                    //int yPos = (int)player.Position.Y;
                    //Vector2 cannonOrigin = new Vector2(11, 50);

                    //spriteBatch.Draw(tankTexture, new Vector2(xPos + 20, yPos - 10), null, player.Color, player.Angle, cannonOrigin, playerScaling, SpriteEffects.None, 1);
                    //spriteBatch.Draw(tankTexture, player.Position, null, player.Color, 0, new Vector2(0, tankTexture.Height), playerScaling, SpriteEffects.None, 0);

                    //spriteBatch.Draw(cannonTexture, new Vector2(xPos + 20, yPos - 10), null, player.Color, player.Angle, cannonOrigin, playerScaling, SpriteEffects.None, 1);
                    spriteBatch.Draw(tankTexture, player.Position, null, player.Color,player.Angle, new Vector2(0, tankTexture.Height), playerScaling, SpriteEffects.None,0);
                
                }
            }
        }

        private void DrawCells()
        {
            for (int i = 0; i < 10; i++ )
            {

                for (int j = 0; j < 10; j++ )
                {
                    

                    //cells[i][j].type = currentGrid[i][j];
                    String textuteType;
                    Color tempColor;
                    //cells[i][j].Direction;

                    PlayerData player ;
                    

                    switch (cells[i][j].type)
                    {
                        case "B ":
                            textuteType = "brick";
                            player = players[0];
                            break;
                        case "W ":
                            textuteType = "water";
                            player = players[0];
                            break;
                        case "S ":
                            textuteType = "stone";
                            player = players[0];
                            break;
                        case "- ":
                            textuteType = "blank";
                            player = players[0];
                            break;
                        case "$ ":
                            textuteType = "coin";
                            player = players[0];
                            break;
                        case "+ ":
                            textuteType = "life";
                            player = players[0];
                            break;
                        case "1 ":
                            textuteType = "tank";
                            player = players[1];
                            break;
                        case "2 ":
                            textuteType = "tank";
                            player = players[2];
                            break;
                        case "3 ":
                            textuteType = "tank";
                            player = players[3];
                            break;
                        case "4 ":
                            textuteType = "tank";
                            player = players[4];
                            break;
                        case "5 ":
                            textuteType = "tank";
                            player = players[5];
                            break;
                        default:
                            textuteType = "sand";
                            player = players[6];
                            break;


                    }

                    //try
                    //{
                        Texture2D tempTexture = Content.Load<Texture2D>(textuteType);
                        //spriteBatch.Draw(tempTexture, cells[i][j].Position, tempColor);

                        Rectangle tempRectangle = new Rectangle((int)(cells[i][j].Position.X), (int)cells[i][j].Position.Y, 50, 50);

                        //spriteBatch.Draw(tempTexture, tempRectangle, tempColor);
                        spriteBatch.Draw(tempTexture, cells[i][j].Position, null, Color.White, player.Angle, new Vector2(25,25), .8f, SpriteEffects.None, 0);
                
                   // }
                   // catch (Exception e)
                    //{

                        //
                    //}

                }


            }
        


        }


        private void DrawShell()
        {
            if (shellFlying)
                spriteBatch.Draw(shellTexture, shellPosition, null, players[currentPlayer].Color, 0, new Vector2(0, 0), 0, SpriteEffects.None,1);
        }


        private void DrawText()
        {
            
            //int currentAngle = (int)MathHelper.ToDegrees(player.Angle);

            Color tempC = Color.Red;

            spriteBatch.DrawString(font1, "World of Tank - SL", new Vector2(300, 20), tempC);

            spriteBatch.DrawString(font, "Score Board", new Vector2(750, 130), Color.SlateBlue);
            spriteBatch.DrawString(font, "Name  Health  Coins   Points", new Vector2(710, 160), tempC);
            spriteBatch.DrawString(font, "--------------------------------------", new Vector2(710, 170), tempC);
            String detail;
            try{

                detail=players[1].Detail;
                spriteBatch.DrawString(font, players[1].Detail, new Vector2(710, 190), tempC);
                spriteBatch.DrawString(font, players[2].Detail, new Vector2(710, 220), tempC);
                spriteBatch.DrawString(font, players[3].Detail, new Vector2(710, 250), tempC);
                spriteBatch.DrawString(font, players[4].Detail, new Vector2(710, 280), tempC);
                spriteBatch.DrawString(font, players[5].Detail, new Vector2(710, 310), tempC);
            }
            catch(Exception e){

                //
            }

            
            
        }

        private void ConsolePrint()
        {
            

            spriteBatch.DrawString(font, err, new Vector2(0, 650), Color.White);
            

        }



        private void SetUpPlayers()
        {
            Color[] playerColors = new Color[10];
            playerColors[0] = Color.White;
            playerColors[1] = Color.Green;
            playerColors[2] = Color.Blue;
            playerColors[3] = Color.Yellow;
            playerColors[4] = Color.Violet;
            playerColors[5] = Color.Red;

            players = new PlayerData[numberOfPlayers];
            for (int i = 1; i < numberOfPlayers; i++)
            {
                players[i].IsAlive = true;
                players[i].Color = playerColors[i];
                players[i].Angle = MathHelper.ToRadians(0);
                //players[i].Power = 100;
                players[i].Detail = "P0         0           0           0";
            }

            
            //players[0].Position = new Vector2(0, 0);
           
        }


        private void SetupCells()
        {
            cells = new cellData[10][];

            for (int i = 0; i < 10; i++)
            {
                cells[i] = new cellData[10];
            }


            for (int i = 0; i < 10; i++)
            {

                for (int j = 0; j < 10; j++)
                {

                    cells[i][j].Position = new Vector2((i * (50 ))+125, (j * (50))+125);
                    //cells[i][j].Position = new Vector2
                    //cells[i][j].type = "_ ";
                }
            }

        }

        public void playerMove()
        {




        }


        internal Decode TempDecoder { get; set; }
    }
}
