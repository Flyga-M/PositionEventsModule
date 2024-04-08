using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Flyga.PositionEventsModule.Debug.Entity.Primitive
{
    public class SphereEntity : IEntity
    {
        private VertexPositionColorTexture[] _vertices;
        private VertexBuffer _vertexBuffer;
        private static BasicEffect _sharedEffect;

        public VertexPositionColorTexture[] Verticies { get { return _vertices; } }
        public VertexBuffer VertexBuffer { get { return _vertexBuffer; } }
        public BasicEffect SharedEffect { get { return _sharedEffect; } }

        public Texture2D Texture { get; set; }
        public float Opacity { get; set; } = 1f;
        public Vector3 Position { get; set; }
        public Vector3 Orientation { get; set; }
        public float Radius { get; set; }
        public int  Detail { get; set; }

        public float DrawOrder => Vector3.Distance(Position, GameService.Gw2Mumble.PlayerCharacter.Position);

        public SphereEntity(Texture2D texture, float opacity, Vector3 position, Vector3 orientation, float radius, int detail)
        {
            Texture = texture;
            Opacity = opacity;
            Position = position;
            Orientation = orientation;
            Radius = radius;
            Detail = detail;

            Initialize();
        }

        /// <summary>
        /// https://www.gamedev.net/forums/topic/454025-c-xna-rendering-a-textured-sphere/3997868/
        /// </summary>
        private void BuildSphere()
        {
            int horizontalSegments = Detail * 2;
            int verticalSegments = Detail;

            float horizontalRadius = Radius;
            float verticalRadius = Radius;

            Vector3 center = Vector3.Zero;

            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[verticalSegments * 2 * (horizontalSegments + 1)];

            int index = 0;
            for (int i = 0; i < verticalSegments; i++)
            {
                float theta1 = ((float)i / (float)verticalSegments) * MathHelper.ToRadians(360f);
                float theta2 = ((float)(i + 1) / (float)verticalSegments) * MathHelper.ToRadians(360f);

                for (int j = 0; j <= horizontalSegments; j++)
                {
                    float phi = ((float)j / (float)horizontalSegments) * MathHelper.ToRadians(180f);

                    float x1 = (float)(Math.Sin(phi) * Math.Cos(theta1)) * horizontalRadius;
                    float z1 = (float)(Math.Sin(phi) * Math.Sin(theta1)) * horizontalRadius;
                    float x2 = (float)(Math.Sin(phi) * Math.Cos(theta2)) * horizontalRadius;
                    float z2 = (float)(Math.Sin(phi) * Math.Sin(theta2)) * horizontalRadius;
                    float y = (float)Math.Cos(phi) * verticalRadius;

                    Vector3 position = center + new Vector3(x1, y, z1);
                    verts[index] = new VertexPositionColorTexture(position, Color.White, new Vector2((float)i / (float)verticalSegments, (float)j / (float)horizontalSegments));
                    index++;

                    position = center + new Vector3(x2, y, z2);
                    verts[index] = new VertexPositionColorTexture(position, Color.White, new Vector2((float)(i + 1) / (float)verticalSegments, (float)j / (float)horizontalSegments));
                    index++;
                }
            }

            _vertices = verts;
            UpdateVertexBuffer();
        }

        private void UpdateVertexBuffer()
        {
            var vertexBuffer = new VertexBuffer(GameService.Graphics.LendGraphicsDeviceContext().GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, _vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(_vertices);

            _vertexBuffer = vertexBuffer;
        }

        private void Initialize()
        {
            BuildSphere();
            _sharedEffect = new BasicEffect(GameService.Graphics.LendGraphicsDeviceContext().GraphicsDevice)
            {
                TextureEnabled = true
            };
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
                _sharedEffect.World = Matrix.CreateTranslation(Position);
            }
            else
            {
                _sharedEffect.World = Matrix.CreateBillboard(Vector3.Zero, Orientation, Vector3.UnitZ, null)
                                    * Matrix.CreateTranslation(Position);
            }

            _sharedEffect.Alpha = this.Opacity;
            _sharedEffect.Texture = this.Texture;

            foreach (var pass in _sharedEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.SetVertexBuffer(_vertexBuffer);
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _vertexBuffer.VertexCount - 2);
            }
        }

        public void Update(GameTime gameTime)
        {
            /** NOOP **/
        }
    }
}
