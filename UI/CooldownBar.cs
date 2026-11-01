using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.UI
{
    // a small reusable drawing helper that takes a cooldown fraction without controlling the timer itself
    internal class CooldownBar
    {
        private Texture2D white;
        public CooldownBar(Texture2D white) { this.white = white; }

        // clamping the supplied fraction keeps the blue fill within the bar's width
        public void Draw(SpriteBatch sb, Vector2 pos, float pct)
        {
            int w = 50, h = 6;
            sb.Draw(white, new Rectangle((int)pos.X - w / 2, (int)pos.Y - 12, w, h), Color.DarkGray);
            sb.Draw(white, new Rectangle((int)pos.X - w / 2, (int)pos.Y - 12, (int)(w * MathHelper.Clamp(pct, 0f, 1f)), h), Color.CornflowerBlue);
        }
    }
}
