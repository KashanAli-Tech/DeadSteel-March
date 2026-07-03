using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Coursework.UI;

namespace Coursework.States
{
    internal class MenuState : BaseState
    {
        // Visuals
        private Texture2D menuBackground;
        private Texture2D buttonTexture;

        // Font
        private SpriteFont menuFont;

        // Music
        private Song menuMusic;

        // Buttons
        private Rectangle playButtonRect;
        private Rectangle quitButtonRect;

        public MenuState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
        }

        public override void LoadContent()
        {
            // load Visuals fror menu screen
            menuBackground = Content.Load<Texture2D>("MenuBackground"); // load the background texture from content folder
            buttonTexture = Content.Load<Texture2D>("Button"); // load the button texture from content folder

            // Font for the text on buttons and game name
            menuFont = Content.Load<SpriteFont>("DefaultFont"); // load the font file from content folder

            // Music that will play whne the user is in the menu screen
            menuMusic = Content.Load<Song>("MenuBackgroundMusic"); // load the background music from content folder
            MediaPlayer.IsRepeating = true; // the music should keep repeating unitl the player leaves the menu screen
            MediaPlayer.Play(menuMusic);   // using mediaplayer to play music

            // button setup
            playButtonRect = new Rectangle(700, 450, 500, 120); // centre of the screen so user can clearly see
            quitButtonRect = new Rectangle(700, 600, 500, 120);// 250 y coordinates below than play button
        }

        public override void UnloadContent()
        {
        }

        public override void Update(GameTime gameTime)
        {
            // get the current state of the keyboard
            var mouse = Mouse.GetState();


            // check whether Play button is clicked if mouse is left clicked and that click was in coordinates of the button rectangle
            if (mouse.LeftButton == ButtonState.Pressed &&
                playButtonRect.Contains(mouse.Position)) 
            {
                Game.ChangeState(new GameState(Game, GraphicsDevice, Content));
                // if clicked then, change the current state to gamestate
            }

            // check whether Quit button is clicked if mouse is left clicked and that click was in coordinates of the button rectangle
            if (mouse.LeftButton == ButtonState.Pressed &&
                quitButtonRect.Contains(mouse.Position))
            {
                Game.Exit(); // if clicked then, exit the game
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw the Background whcihc covers the whole screen
            spriteBatch.Draw(menuBackground, Vector2.Zero, Color.White);

            // Draw the textbox that says the Name of the Game
            spriteBatch.DrawString(menuFont, "DEADSTEEL MARCH",
                new Vector2(800, 200), Color.Red);

            // Draw the  Play Button
            spriteBatch.Draw(buttonTexture, playButtonRect, Color.White);
            spriteBatch.DrawString(menuFont, "PLAY",
                new Vector2(playButtonRect.X + 200, playButtonRect.Y + 35), 
                Color.Black);
            // Changed the X coordinate addition from 180 to 200


            // Draw the Quit Button
            spriteBatch.Draw(buttonTexture, quitButtonRect, Color.White);
            spriteBatch.DrawString(menuFont, "QUIT",
                new Vector2(quitButtonRect.X + 200, quitButtonRect.Y + 35),
                Color.Black);
            // Changed the X coordinate addition from 180 to 200

        }
    }
}
