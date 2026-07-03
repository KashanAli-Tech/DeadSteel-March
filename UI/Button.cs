using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Coursework.UI
{
    internal class Button
    {
        public Rectangle Rectangle;
        public string Text;

        private Texture2D texture;
        private SpriteFont font;

        private Color normalColor = Color.White;
        private Color hoverColor = Color.LightGray;
        private Color textColor = Color.Black;

        public bool IsHovered { get; private set; }
        public bool IsClicked { get; private set; }

        public Button(Texture2D texture, SpriteFont font, Rectangle rectangle, string text)
        {
            this.texture = texture;
            this.font = font;
            this.Rectangle = rectangle;
            this.Text = text;
        }

        public void Update(MouseState mouse)
        {
            IsHovered = Rectangle.Contains(mouse.Position);
            IsClicked = false;

            if (IsHovered && mouse.LeftButton == ButtonState.Pressed)
            {
                IsClicked = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw button texture
            spriteBatch.Draw(texture, Rectangle, IsHovered ? hoverColor : normalColor);

            // Center text
            Vector2 textSize = font.MeasureString(Text);
            Vector2 textPos = new Vector2(
                Rectangle.X + (Rectangle.Width - textSize.X) / 2,
                Rectangle.Y + (Rectangle.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(font, Text, textPos, textColor);
        }
    }
}
