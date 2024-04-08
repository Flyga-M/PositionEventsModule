using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Flyga.PositionEventsModule.Debug.Entity.Primitive
{
    public class PlaneEntity : IEntity
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

        public Vector2 Dimensions { get; set; }

        public float DrawOrder => Vector3.Distance(Position, GameService.Gw2Mumble.PlayerCharacter.Position);

        public PlaneEntity(Texture2D texture, float opacity, Vector3 position, Vector3 orientation, Vector2 dimensions)
        {
            Texture = texture;
            Opacity = opacity;
            Position = position;
            Orientation = orientation;
            Dimensions = dimensions;

            Initialize();
        }

        private void BuildPlane()
        {
            Vector3 offsetCis = new Vector3(Dimensions.X / 2, Dimensions.Y / 2, 0);
            Vector3 offsetTrans = new Vector3(Dimensions.X / 2, -Dimensions.Y / 2, 0);

            var verts = new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture(-offsetCis, Color.White, new Vector2(0, 0)),
                new VertexPositionColorTexture(-offsetTrans, Color.White, new Vector2(0, 1)),
                new VertexPositionColorTexture(offsetTrans, Color.White, new Vector2(1, 0)),
                new VertexPositionColorTexture(offsetCis, Color.White, new Vector2(1, 1))
            };

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
            BuildPlane();
            _sharedEffect = new BasicEffect(GameService.Graphics.LendGraphicsDeviceContext().GraphicsDevice)
            {
                TextureEnabled = true
            };
        }

        public void Update(GameTime gameTime)
        {
            /** NOOP **/
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
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _vertexBuffer.VertexCount - 2);
            }
        }
    }
}
