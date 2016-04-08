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
namespace ZombiePong
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D background, spritesheet;
        Sprite paddle1, paddle2, ball;
        Random rand = new Random();
        public int p1score, p2score;
        const int width = 1024;
        const int height = 768;
        List<Sprite> zombies = new List<Sprite>();
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();
            p1score = 0;
            p2score = 0;
        }
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content. Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            background = Content.Load<Texture2D>("background");
            spritesheet = Content.Load<Texture2D>("spritesheet");
            paddle1 = new Sprite(new Vector2(20, 20), spritesheet, new Rectangle(0, 516, 25, 150), Vector2.Zero);
            paddle2 = new Sprite(new Vector2(970, 20), spritesheet, new Rectangle(32, 516, 25, 150), Vector2.Zero);
            ball = new Sprite(new Vector2(700, 350), spritesheet, new Rectangle(76, 510, 40, 40), new Vector2(-500, 0));
            SpawnZombie(new Vector2(400, 400), new Vector2(-500, 0));

            Song melee = Content.Load<Song>(@"Sound\Melee - Battlefield");
            MediaPlayer.Play(melee);
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        public void SpawnZombie(Vector2 location, Vector2 velocity)
        {
            Sprite marth = new Sprite(location, spritesheet, new Rectangle(0, 25, 160, 150), velocity);
            for (int i = 1; i < 2; i++)
            {
                marth.AddFrame(new Rectangle(i * 0, 19, 187, 170));
                marth.AddFrame(new Rectangle(i * 218, 19, 187, 170));
                marth.AddFrame(new Rectangle(i * 456, 19, 200, 164));
                marth.AddFrame(new Rectangle(i * 696, 28, 195, 147));
                marth.AddFrame(new Rectangle(i * 935, 16, 195, 147));
            }
            zombies.Add(marth);
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
            MouseState ms = Mouse.GetState();
            paddle1.Location = new Vector2(paddle1.Location.X, ms.Y);

            // When ball goes past P1 paddle, reset ball location so it goes back towards you
            if (ball.Location.X < -20)
            {
                ball.Location = new Vector2(700, 350);
                ball.Velocity *= 0.75f;
                p2score++;
            }

            // When ball goes past P2 paddle, reset ball location so it goes back towards him
            if (ball.Location.X > 1044)
            {
                ball.Location = new Vector2(400, 350);
                ball.Velocity *= 0.75f;
                p1score++;
            }

            // Ball cannot go past the top/bottom boundaries
            if (ball.Location.Y >= height - 16)
                ball.Velocity = new Vector2(ball.Velocity.X, ball.Velocity.Y * -1);
            if (ball.Location.Y <= 0)
                ball.Velocity = new Vector2(ball.Velocity.X, ball.Velocity.Y * -1);

            // P1 can't go past the top/bottom boundaries
            if (paddle1.Location.Y <= 0)
                paddle1.Location = new Vector2(paddle1.Location.X, 0);
            if (paddle1.Location.Y >= height)
                paddle1.Location = new Vector2(paddle1.Location.X, height);

            paddle2.Location = new Vector2(paddle2.Location.X, rand.Next(0, 720));
            if (ball.IsBoxColliding(paddle1.BoundingBoxRect) && ball.Location.Y != paddle1.Center.Y)
            {
                ball.Velocity = new Vector2(ball.Velocity.X * -1.09f, (float)Math.Cos(ball.Location.Y - paddle1.Center.Y) * -100);
            }
            if (ball.IsBoxColliding(paddle2.BoundingBoxRect) && ball.Location.Y != paddle2.Center.Y)
            {
                ball.Velocity = new Vector2(ball.Velocity.X * -1.09f, (float)Math.Cos(ball.Location.Y - paddle2.Center.Y) * -100);
            }

            Window.Title = ("P1 Score: " + p1score + " | " + "P2 Score: " + p2score);

            ball.Update(gameTime);

            for (int i = 0; i < zombies.Count; i++)
            {
                zombies[i].Update(gameTime);
                // Zombie logic goes here..
                zombies[i].FlipHorizontal = false;
                zombies[i].FlipHorizontal = zombies[i].Velocity.X > 0;

                if (zombies[i].Location.X <= 0)
                {
                    zombies[i].Velocity = new Vector2(500, 0);
                }
                if (zombies[i].Location.X > 900)
                {
                    zombies[i].Velocity = new Vector2(-500, 0);
                }


                zombies[i].CollisionRadius = 40;
                if (zombies[i].IsCircleColliding(ball.Center, 10))
                {
                    zombies.RemoveAt(i);
                    ball.Velocity *= -1;

                    for (int marth = 0; marth < 2; marth++)
                    {
                        Vector2 location = new Vector2(rand.Next(500, 500));
                        Vector2 velocity = new Vector2(-500, 0);

                        if (zombies.Count < 10)
                            SpawnZombie(location, velocity);

                    }


                    continue;
                }

                if (zombies[i].Location.X < 0)
                {
                    zombies[i].FlipHorizontal = true;
                }

            }



        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(background, Vector2.Zero, Color.White);
            paddle1.Draw(spriteBatch);
            paddle2.Draw(spriteBatch);
            ball.Draw(spriteBatch);
            for (int i = 0; i < zombies.Count; i++)
            {
                zombies[i].Draw(spriteBatch);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}