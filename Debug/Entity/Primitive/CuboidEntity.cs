﻿using Blish_HUD;
using Blish_HUD.Entities;
using Blish_HUD.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Flyga.PositionEventsModule.Debug.Entity.Primitive
{
    public class CuboidEntity : IEntity
    {
        private VertexPositionColorTexture[] _vertices;
        private VertexBuffer _vertexBuffer;
        private static BasicEffect _sharedEffect;

        public Texture2D Texture { get; set; }
        public float Opacity { get; set; } = 1f;
        public Vector3 Position { get; set; }
        public Vector3 Orientation { get; set; }
        public Vector3 Dimensions { get; set; }

        public float DrawOrder => Vector3.Distance(Position, GameService.Gw2Mumble.PlayerCharacter.Position);

        public CuboidEntity(Texture2D texture, float opacity, Vector3 position, Vector3 orientation, Vector3 dimensions)
        {
            Texture = texture;
            Opacity = opacity;
            Position = position;
            Orientation = orientation;
            Dimensions = dimensions;

            Initialize();
        }

        private void Initialize()
        {
            BuildCuboid();

            GraphicsDeviceContext ctx = GameService.Graphics.LendGraphicsDeviceContext();

            _sharedEffect = new BasicEffect(ctx.GraphicsDevice)
            {
                TextureEnabled = true
            };

            ctx.Dispose();
        }

        private void BuildCuboid()
        {
            Vector3[] vertices = PrimitivesUtil.GetCuboidVertices(Dimensions, Vector3.Up, Vector3.Forward, Vector3.Zero);

            VertexPositionColorTexture[] verticesAndTexture = PrimitivesUtil.GetCuboidFaces(vertices, Dimensions, new Vector2(Texture.Bounds.Width, Texture.Bounds.Height));

            _vertices = verticesAndTexture;
            UpdateVertexBuffer();
        }

        private void UpdateVertexBuffer()
        {
            GraphicsDeviceContext ctx = GameService.Graphics.LendGraphicsDeviceContext();

            VertexBuffer vertexBuffer = new VertexBuffer(ctx.GraphicsDevice, typeof(VertexPositionColorTexture), _vertices.Length, BufferUsage.WriteOnly);

            ctx.Dispose();

            vertexBuffer.SetData(_vertices);

            _vertexBuffer = vertexBuffer;
        }

        public void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera)
        {
            if (this._vertexBuffer.VertexCount == 0)
            {
                return;
            }

            _sharedEffect.View = GameService.Gw2Mumble.PlayerCamera.View;
            _sharedEffect.Projection = GameService.Gw2Mumble.PlayerCamera.Projection;
            if (Vector3.Cross(Orientation, Vector3.UnitZ) == Vector3.Zero)
            {
                _sharedEffect.World = Matrix.CreateTranslation(new Vector3(Dimensions.X * (-1 / 2), Dimensions.Y * (-1 / 2), 0))
                                    * Matrix.CreateTranslation(Position);
            }
            else
            {
                _sharedEffect.World = Matrix.CreateTranslation(new Vector3(Dimensions.X * (-1 / 2), Dimensions.Y * (-1 / 2), 0))
                                    * Matrix.CreateBillboard(Vector3.Zero, Orientation, Vector3.UnitZ, null)
                                    * Matrix.CreateTranslation(Position);
            }

            _sharedEffect.Alpha = this.Opacity;
            _sharedEffect.Texture = this.Texture;

            foreach (var pass in _sharedEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.SetVertexBuffer(_vertexBuffer);
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertexBuffer.VertexCount/3);
            }
        }

        public void Update(GameTime gameTime)
        {
            /** NOOP **/
        }
    }
}
