using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Coursework.UI
{
    internal enum MenuAction { None, Retry, MainMenu, Exit, Resume, Settings, Instructions, Back }
    internal enum MenuPage { None, Pause, Instructions, Settings }

    // keeps the new menu drawing out of the gameplay update without changing how states work
    internal class MenuScreens : IDisposable
    {
        private readonly GraphicsDevice graphics;
        private readonly SpriteFont font;
        private readonly Texture2D button;
        private readonly Texture2D white;
        private float Scale => Math.Min(graphics.Viewport.Width / 1920f, graphics.Viewport.Height / 1080f);
        private Vector2 Offset => new Vector2((graphics.Viewport.Width - 1920 * Scale) / 2f, (graphics.Viewport.Height - 1080 * Scale) / 2f);

        public MenuScreens(GraphicsDevice graphics, ContentManager content, SpriteFont font)
        {
            this.graphics = graphics;
            this.font = font;
            button = content.Load<Texture2D>("Button");
            white = new Texture2D(graphics, 1, 1);
            white.SetData(new[] { Color.White });
        }

        // the same conversion is used for drawing and mouse checks, so smaller viewports keep matching hit areas
        public Rectangle Bounds(Rectangle logical) => new Rectangle((int)(Offset.X + logical.X * Scale),
            (int)(Offset.Y + logical.Y * Scale), (int)(logical.Width * Scale), (int)(logical.Height * Scale));

        public bool Clicked(Rectangle logical, MouseState mouse, MouseState previous) =>
            mouse.LeftButton == ButtonState.Pressed && previous.LeftButton == ButtonState.Released && Bounds(logical).Contains(mouse.Position);

        public void Panel(SpriteBatch batch, Rectangle logical, Color fill, Color border)
        {
            Rectangle bounds = Bounds(logical);
            batch.Draw(white, bounds, border);
            int inset = Math.Max(1, (int)(3 * Scale));
            batch.Draw(white, new Rectangle(bounds.X + inset, bounds.Y + inset, bounds.Width - inset * 2, bounds.Height - inset * 2), fill);
        }

        public void Dim(SpriteBatch batch, float opacity) => batch.Draw(white, graphics.Viewport.Bounds, Color.Black * opacity);

        // shrinking long labels only when needed keeps the existing button artwork and text readable
        public void Text(SpriteBatch batch, string text, float x, float y, float size, Color color, bool centred = false, float maxWidth = 1700)
        {
            Vector2 measured = font.MeasureString(text);
            float textScale = Math.Min(size, maxWidth / Math.Max(1, measured.X)) * Scale;
            Vector2 position = Offset + new Vector2(x, y) * Scale;
            if (centred) position.X -= measured.X * textScale / 2f;
            batch.DrawString(font, text, position + Vector2.One * Scale, Color.Black, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
            batch.DrawString(font, text, position, color, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
        }

        public void Button(SpriteBatch batch, Rectangle logical, string label, MouseState mouse)
        {
            Rectangle bounds = Bounds(logical);
            batch.Draw(button, bounds, bounds.Contains(mouse.Position) ? new Color(220, 235, 220) : Color.White);
            Text(batch, label, logical.Center.X, logical.Y + logical.Height / 2 - 17, .9f, Color.White, true, logical.Width - 60);
        }

        // only a fresh click runs an action, so holding a button cannot request several new matches
        public MenuAction UpdateResults(MouseState mouse, MouseState previous)
        {
            if (Clicked(new Rectangle(300, 760, 420, 110), mouse, previous)) return MenuAction.Retry;
            if (Clicked(new Rectangle(750, 760, 420, 110), mouse, previous)) return MenuAction.MainMenu;
            if (Clicked(new Rectangle(1200, 760, 420, 110), mouse, previous)) return MenuAction.Exit;
            return MenuAction.None;
        }

        // the final counters are only displayed here, so this screen cannot change match data
        public void DrawResults(SpriteBatch batch, Texture2D background, bool victory, double seconds, int eliminated, int deployed, int upgrades, MouseState mouse)
        {
            batch.Draw(background, graphics.Viewport.Bounds, Color.White);
            Dim(batch, .22f);
            Color accent = victory ? new Color(133, 190, 116) : new Color(224, 94, 89);
            Text(batch, victory ? "VICTORY" : "DEFEAT", 960, 120, 3.1f, accent, true);
            Text(batch, victory ? "MISSION COMPLETE" : "BASE DESTROYED", 960, 265, 1f, Color.White, true);
            Panel(batch, new Rectangle(540, 350, 840, 330), new Color(9, 14, 16, 225), accent);
            string[] labels = { "MATCH TIME", "ENEMIES ELIMINATED", "UNITS DEPLOYED", "UPGRADES PURCHASED" };
            long time = (long)Math.Max(0, seconds);
            string[] values = { $"{time / 60:00}:{time % 60:00}", eliminated.ToString(), deployed.ToString(), upgrades.ToString() };
            for (int i = 0; i < labels.Length; i++)
            {
                Text(batch, labels[i], 595, 390 + i * 68, .8f, Color.LightGray);
                Text(batch, values[i], 1290, 390 + i * 68, 1f, Color.White, true);
            }
            Button(batch, new Rectangle(300, 760, 420, 110), "RETRY MISSION", mouse);
            Button(batch, new Rectangle(750, 760, 420, 110), "MAIN MENU", mouse);
            Button(batch, new Rectangle(1200, 760, 420, 110), "EXIT GAME", mouse);
            Text(batch, "M: MAIN MENU", 960, 940, .7f, Color.LightGray, true);
        }

        public void Dispose() => white.Dispose();

        public MenuAction UpdatePause(MouseState mouse, MouseState previous)
        {
            MenuAction[] actions = { MenuAction.Resume, MenuAction.Settings, MenuAction.Instructions, MenuAction.MainMenu };
            for (int i = 0; i < actions.Length; i++)
                if (Clicked(new Rectangle(710, 340 + i * 130, 500, 105), mouse, previous)) return actions[i];
            return MenuAction.None;
        }

        public void DrawPause(SpriteBatch batch, MouseState mouse)
        {
            Dim(batch, .72f);
            Text(batch, "MISSION PAUSED", 960, 175, 1.9f, new Color(155, 185, 135), true);
            string[] labels = { "RESUME GAME", "SETTINGS", "HOW TO PLAY", "MAIN MENU" };
            for (int i = 0; i < labels.Length; i++) Button(batch, new Rectangle(710, 340 + i * 130, 500, 105), labels[i], mouse);
            Text(batch, "ESC: RESUME", 960, 925, .7f, Color.LightGray, true);
        }

        // the controls match the current input code, and costs are read from the active match
        public void DrawInstructions(SpriteBatch batch, MouseState mouse, int soldierCost, int gunnerCost, int tankCost)
        {
            Dim(batch, .84f);
            Color accent = new Color(155, 185, 135);
            Text(batch, "MISSION BRIEFING", 960, 100, 1.9f, accent, true);
            Panel(batch, new Rectangle(260, 220, 1400, 660), new Color(12, 18, 21, 235), accent);
            string[] lines =
            {
                "Destroy the enemy tower before your own tower is destroyed",
                $"1  SOLDIER (${soldierCost})      2  MACHINE GUNNER (${gunnerCost})      3  TANK (${tankCost})",
                "LEFT CLICK enemy units to damage them; heavier units need more clicks",
                "Finish off enemy units with clicks to earn money; spend it on units and upgrades",
                "The top-centre button shows one rotating upgrade offer and its current cost",
                "Press R to buy the displayed upgrade if you can afford it; offers alone give no bonus",
                "Bought bonuses stack; a shield protects your tower for its displayed duration",
                "ESC pauses or resumes the match; M returns to the main menu during gameplay",
                "Enemy tower at 0 HP: VICTORY     Your tower at 0 HP: DEFEAT"
            };
            for (int i = 0; i < lines.Length; i++) Text(batch, lines[i], 320, 265 + i * 64, .85f, Color.White, false, 1280);
            Button(batch, new Rectangle(760, 940, 400, 100), "BACK", mouse);
        }
    }
}
