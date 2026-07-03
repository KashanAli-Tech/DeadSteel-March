using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.UI
{
    internal class HealthBar
    {
        private Texture2D white;
        public HealthBar(Texture2D white) { this.white = white; }

        public void Draw(SpriteBatch sb, Vector2 pos, int current, int max)
        {
            int w = 50, h = 6;
            sb.Draw(white, new Rectangle((int)pos.X - w / 2, (int)pos.Y - 20, w, h), Color.DarkGray);
            float pct = MathHelper.Clamp((float)current / max, 0f, 1f);
            sb.Draw(white, new Rectangle((int)pos.X - w / 2, (int)pos.Y - 20, (int)(w * pct), h), Color.Red);
        }
    }
}
