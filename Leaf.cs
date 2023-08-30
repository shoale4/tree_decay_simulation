using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TreeGenerator
{
    public class Leaf
    {        
        public Vector2 Position { get; set; }
        public Rectangle Rectangle { get { return new Rectangle((int)Position.X, (int)Position.Y, 1, 1); } }
        public Branch ClosestBranch { get; set; }

        private Texture2D Texture;

        public Leaf(Vector2 position, Texture2D texture)
        {
            Position = position;
            Texture = texture;
        }

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Draw(Texture, Rectangle, Color.Green);
        }
    }
}
