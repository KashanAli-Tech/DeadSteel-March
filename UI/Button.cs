using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Coursework.UI
{
    // groups the button's area, appearance and mouse checks so the same drawing code can be reused
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

        // this reports a click each update while the left button is held over the button
        public void Update(MouseState mouse)
        {
            IsHovered = Rectangle.Contains(mouse.Position);
            IsClicked = false;

            if (IsHovered && mouse.LeftButton == ButtonState.Pressed)
            {
                IsClicked = true;
            }
        }

        // changing the tint on hover gives feedback without needing a second button image
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Rectangle, IsHovered ? hoverColor : normalColor);

            // measuring the label lets it stay centred even when its text or the button size changes
            Vector2 textSize = font.MeasureString(Text);
            Vector2 textPos = new Vector2(
                Rectangle.X + (Rectangle.Width - textSize.X) / 2,
                Rectangle.Y + (Rectangle.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(font, Text, textPos, textColor);
        }
    }
}
