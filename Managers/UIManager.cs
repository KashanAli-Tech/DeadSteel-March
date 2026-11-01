using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Coursework.GameObjects;
using System;

namespace Coursework.Managers
{
    // keeps hud layout and health bar drawing separate from the gameplay values they display
    internal class UIManager : IDisposable
    {
        private SpriteFont font;
        private Texture2D white;
        private Texture2D buttonTexture;
        private Texture2D moneyIcon;
        private Texture2D clockIcon;

        // reusing content assets and one white pixel keeps the hud simple without extra drawing packages
        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphicsDevice)
        {
            font = content.Load<SpriteFont>("DefaultFont");
            buttonTexture = content.Load<Texture2D>("Button");
            moneyIcon = content.Load<Texture2D>("MoneyIcon");
            clockIcon = content.Load<Texture2D>("Clock");
            Dispose();
            white = new Texture2D(graphicsDevice, 1, 1);
            white.SetData(new[] { Color.White });
        }

        // only this pixel is owned here; fonts and content-loaded textures remain shared by the content manager
        public void Dispose()
        {
            white?.Dispose();
            white = null;
        }

        // shrinking the hud on smaller viewports prevents its panels overlapping, while larger viewports keep the normal size
        public static float GetHudScale(Viewport viewport)
        {
            return Math.Min(1f, Math.Min(viewport.Width / 1280f, viewport.Height / 720f));
        }

        // one shared button position gives the money, upgrade and timer panels the same top row
        public static Rectangle GetUpgradeButtonBounds(Viewport viewport)
        {
            float scale = GetHudScale(viewport);
            int width = (int)(520 * scale);
            return new Rectangle((viewport.Width - width) / 2, (int)(16 * scale), width, (int)(104 * scale));
        }

        // formatting the existing balance for display without creating or changing any money values
        public void DrawMoney(SpriteBatch spriteBatch, int money, Viewport viewport)
        {
            DrawHudItem(spriteBatch, moneyIcon, $"Money: {money:N0}", viewport, false, new Color(255, 224, 151));
        }

        // turning elapsed gameplay seconds into minutes and seconds, with leading zeroes to keep the clock easy to read
        public void DrawGameTimer(SpriteBatch spriteBatch, double gameplaySeconds, Viewport viewport)
        {
            long seconds = (long)Math.Max(0d, gameplaySeconds);
            DrawHudItem(spriteBatch, clockIcon, $"{seconds / 60:00}:{seconds % 60:00}", viewport, true, Color.White);
        }

        // sharing the side-panel layout keeps padding and icon sizes consistent for money and the clock
        private void DrawHudItem(SpriteBatch spriteBatch, Texture2D icon, string text, Viewport viewport, bool alignRight, Color color)
        {
            float scale = GetHudScale(viewport);
            Rectangle upgradeButton = GetUpgradeButtonBounds(viewport);
            int margin = (int)(20 * scale);
            int iconSize = (int)(48 * scale);
            float gap = 12 * scale;
            int buttonWidth = (int)(220 * scale);
            var button = new Rectangle(alignRight ? viewport.Width - margin - buttonWidth : margin,
                upgradeButton.Y, buttonWidth, upgradeButton.Height);
            spriteBatch.Draw(buttonTexture, button, Color.White);
            Vector2 textSize = font.MeasureString(text);
            float textSpace = Math.Max(1f, button.Width - 64 * scale);
            // shrinking long text only when needed keeps it inside the panel instead of over the frame
            float textScale = Math.Min(0.95f * scale, textSpace / Math.Max(1f, textSize.X));
            float textX = button.Center.X - textSize.X * textScale / 2f;
            int iconX = alignRight ? (int)(button.Left - gap - iconSize) : (int)(button.Right + gap);
            spriteBatch.Draw(icon, new Rectangle(iconX, button.Center.Y - iconSize / 2, iconSize, iconSize), Color.White);
            DrawShadowedText(spriteBatch, text, new Vector2(textX, button.Center.Y - textSize.Y * textScale / 2f), textScale, color);
        }

        // reading current and maximum health makes the label and bar reflect health upgrades automatically
        public void DrawTowerHealth(SpriteBatch spriteBatch, Tower tower, Rectangle rect, Color fillColor)
        {
            DrawHealthBar(spriteBatch, rect, tower.TowerHealth, tower.MaxTowerHealth, fillColor);
            string text = $"{tower.TowerHealth} / {tower.MaxTowerHealth}";
            Vector2 size = font.MeasureString(text);
            float scale = Math.Min(0.65f, (rect.Width - 16f) / Math.Max(1f, size.X));
            DrawShadowedText(spriteBatch, text, new Vector2(rect.Center.X - size.X * scale / 2f, rect.Center.Y - size.Y * scale / 2f), scale);
        }

        // scaling only the coloured fill by current health avoids reducing the bar width twice
        public void DrawHealthBar(SpriteBatch spriteBatch, Rectangle rect, int current, int max, Color fillColor)
        {
            float pct = max <= 0 ? 0f : MathHelper.Clamp((float)current / max, 0f, 1f);
            // layering a shadow, border and dark track gives the bar some depth using simple rectangles
            spriteBatch.Draw(white, new Rectangle(rect.X, rect.Y + 3, rect.Width, rect.Height), Color.Black * 0.35f);
            spriteBatch.Draw(white, rect, new Color(88, 103, 111));
            spriteBatch.Draw(white, new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2), new Color(24, 32, 38));
            spriteBatch.Draw(white, new Rectangle(rect.X + 1, rect.Y, rect.Width - 2, 1), new Color(133, 149, 157));

            var fill = new Rectangle(rect.X + 4, rect.Y + 4, (int)Math.Round((rect.Width - 8) * pct), rect.Height - 8);
            // skipping an empty fill avoids drawing a coloured strip when health has reached zero
            if (fill.Width > 0)
            {
                spriteBatch.Draw(white, fill, Color.Lerp(fillColor, Color.Black, 0.12f));
                spriteBatch.Draw(white, new Rectangle(fill.X, fill.Y, fill.Width, fill.Height / 2), Color.Lerp(fillColor, Color.White, 0.15f));
            }
        }

        // a small dark text shadow keeps numbers readable over the button texture and coloured health fill
        private void DrawShadowedText(SpriteBatch spriteBatch, string text, Vector2 position, float scale, Color? color = null)
        {
            spriteBatch.DrawString(font, text, position + new Vector2(1, 1), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, position, color ?? Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
