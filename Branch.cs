using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace TreeGenerator
{
    public class Branch
    {
        public Branch Parent { get; private set; }
        public Vector2 GrowDirection { get; set; }
        public Vector2 OriginalGrowDirection { get; set; }
        public int GrowCount { get; set; }
        public Vector2 Position { get; private set; }
        public float Size { get; set; }

        private Texture2D Texture;

        public Branch(Branch parent, Vector2 position, Vector2 growDirection, Texture2D texture)
        {
            Parent = parent;
            Position = position;
            GrowDirection = growDirection;
            OriginalGrowDirection = growDirection;
            Texture = texture;
            Size = 0.002f;
        }

        public void Reset()
        {
            GrowCount = 0;
            GrowDirection = OriginalGrowDirection;
        }

        private void DrawLine(SpriteBatch spritebatch, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            length = length / 10;
            spritebatch.Draw(Texture, point1, null, color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
        }

        public void Draw(SpriteBatch spritebatch)
        {
            if (Parent != null)
                DrawLine(spritebatch, (float)Math.Sqrt(Size), Color.Brown, Position, Parent.Position);
        }
    }
}
