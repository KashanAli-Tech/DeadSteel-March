using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects.Units
{
    using Coursework.GameObjects;

    internal class Tank : BaseUnit
    {
        public Tank(Texture2D texture, Vector2 position)
            : base(texture, 300, 1.2f, 40, position, 1.2f, 220f, 1.2f)
        {
        }

        public override bool FiresShells => true;

        public override void Update(Vector2 targetPosition, GameTime gameTime)
        {
            base.Update(targetPosition, gameTime);
        }
    }
}
