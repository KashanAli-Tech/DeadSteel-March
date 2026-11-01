using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.UI
{
    // a small reusable health bar helper, separate from the tower bars drawn by the ui manager
    internal class HealthBar
    {
        private Texture2D white;
        public HealthBar(Texture2D white) { this.white = white; }

        // clamping the health ratio keeps the fill within its background when drawn above a position
        public void Draw(SpriteBatch sb, Vector2 pos, int current, int max)
        {
            int w = 40, h = 6;
            sb.Draw(white, new Rectangle((int)pos.X - w / 2, (int)pos.Y - 20, w, h), Color.DarkGray);
            float pct = MathHelper.Clamp((float)current / max, 0f, 1f);
            sb.Draw(white, new Rectangle((int)pos.X - w / 2, (int)pos.Y - 20, (int)(w * pct), h), Color.Red);
        }
    }
}
