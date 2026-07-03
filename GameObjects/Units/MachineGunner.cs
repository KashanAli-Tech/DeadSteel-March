using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects.Units
{
    using Coursework.GameObjects;

    internal class MachineGunner : BaseUnit
    {
        public MachineGunner(Texture2D texture, Vector2 position)
            : base(texture, 120, 1.8f, 15, position, 1f)
        {
        }

        public void Update(Vector2 targetPosition, GameTime gameTime)
        {
            base.Update(targetPosition, gameTime);
        }
    }
}
