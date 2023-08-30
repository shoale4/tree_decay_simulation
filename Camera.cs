using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Input;

namespace TreeGenerator
{
    public class Camera
    {
        private const float zoomUpperLimit = 15f;
        private const float zoomLowerLimit = 0.25f;
        private float zoom;
        private int ViewportWidth, ViewportHeight;

        #region Properties

        public Vector2 Position { get; set; }

        public float Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
                if (zoom < zoomLowerLimit)
                    zoom = zoomLowerLimit;
                if (zoom > zoomUpperLimit)
                    zoom = zoomUpperLimit;
            }
        }

        public void Move(Vector2 amount)
        {
            Position += amount;
        }
              
        public Matrix View
        {
            get
            {
                return Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *                        
                        Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                        Matrix.CreateTranslation(new Vector3(ViewportWidth * 0.5f, ViewportHeight * 0.5f, 0));
            }
        }

        #endregion

        public Camera(Viewport viewport, float initialZoom)
        {
            Zoom = initialZoom;
            Position = Vector2.Zero;
            ViewportWidth = viewport.Width;
            ViewportHeight = viewport.Height;
        }

    }
}