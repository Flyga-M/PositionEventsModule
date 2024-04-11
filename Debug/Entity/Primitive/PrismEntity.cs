using Blish_HUD;
using Blish_HUD.Entities;
using Blish_HUD.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Shapes;
using MonoGame.Extended.Triangulation;
using System.Linq;

namespace Flyga.PositionEventsModule.Debug.Entity.Primitive
{
    public class PrismEntity : IEntity
    {
        private VertexPositionColorTexture[] _vertices;
        private VertexBuffer _vertexBuffer;
        private static BasicEffect _sharedEffect;

        private Polygon _polygon;

        public VertexPositionColorTexture[] Verticies { get { return _vertices; } }
        public VertexBuffer VertexBuffer { get { return _vertexBuffer; } }
        public BasicEffect SharedEffect { get { return _sharedEffect; } }

        public Texture2D Texture { get; set; }
        public float Opacity { get; set; } = 1f;
        public Vector3 Position { get; set; }
        public Vector3 Orientation { get; set; }
        /// <summary>
        /// Should be centered around <see cref="Vector2.Zero"/>.
        /// </summary>
        public Polygon Polygon
        {
            get
            {
                return _polygon;
            }
            set
            {
                _polygon = PolygonUtil.CenterAround(value, Vector2.Zero);
            }
        }
        public float Length { get; set; }

        public float DrawOrder => Vector3.Distance(Position, GameService.Gw2Mumble.PlayerCharacter.Position);

        public PrismEntity(Texture2D texture, float opacity, Vector3 position, Vector3 orientation, Polygon polygon, float length)
        {
            Texture = texture;
            Opacity = opacity;
            Position = position;
            Orientation = orientation;
            Polygon = polygon;
            Length = length;
            Initialize();
        }

        private void BuildPrism()
        {
            VertexPositionColorTexture[] ring = PrimitivesUtil.TriangleListFromStrip(BuildRing());
            VertexPositionColorTexture[] lids = BuildLids();

            _vertices = ring.Concat(lids).ToArray();
            UpdateVertexBuffer();
        }

        /// <summary>
        /// Returns the ring (outer shell) of the prism as <see cref="PrimitiveType.TriangleStrip"/>.
        /// </summary>
        /// <returns>The vertices, that build the ring faces as a <see cref="PrimitiveType.TriangleStrip"/>.</returns>
        private VertexPositionColorTexture[] BuildRing()
        {
            float zTop = Length / 2;
            float zBottom = -Length / 2;

            // only use the top half of the texture for the ring
            Vector2 textureAreaMultiplier = new Vector2(1.0f, 0.5f);

            int baseVertAmount = Polygon.Vertices.Length;

            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[baseVertAmount * 2 + 2];

            for (int i = 0; i < baseVertAmount; i++)
            {
                Vector2 polygonVert = Polygon.Vertices[i];

                Vector3 positionTop = new Vector3(polygonVert.X, polygonVert.Y, zTop);
                Vector3 positionBottom = new Vector3(polygonVert.X, polygonVert.Y, zBottom);
                verts[i * 2] = new VertexPositionColorTexture(positionTop, Color.White, new Vector2((float)i / (float)baseVertAmount, 1.0f) * textureAreaMultiplier);
                verts[(i * 2) + 1] = new VertexPositionColorTexture(positionBottom, Color.White, new Vector2((float)i / (float)baseVertAmount, 0.0f) * textureAreaMultiplier);

                if (i == 0)
                {
                    verts[baseVertAmount * 2] = new VertexPositionColorTexture(positionTop, Color.White, new Vector2(1.0f, 1.0f) * textureAreaMultiplier);
                    verts[baseVertAmount * 2 + 1] = new VertexPositionColorTexture(positionBottom, Color.White, new Vector2(1.0f, 0.0f) * textureAreaMultiplier);
                }
            }

            return verts;
        }

        /// <summary>
        /// Returns the lids (top and bottom base) of the prism as <see cref="PrimitiveType.TriangleList"/>.
        /// </summary>
        /// <returns>The Vertices, that build the lid faces as a <see cref="PrimitiveType.TriangleList"/>.</returns>
        private VertexPositionColorTexture[] BuildLids()
        {
            Triangulator.Triangulate(Polygon.Vertices, Triangulator.DetermineWindingOrder(Polygon.Vertices), out Vector2[] outputVertices, out int[] indices);

            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[indices.Length * 2];

            // only use the bottom half of the texture for the ring
            Vector2 textureAreaMultiplier = new Vector2(1.0f, 0.5f);
            Vector2 textureAreaOffset = new Vector2(0.0f, 0.5f);

            float zTop = Length / 2;
            float zBottom = -Length / 2;

            for (int i = 0; i < indices.Length; i++)
            {
                Vector2 polygonVert = outputVertices[indices[i]];

                Vector3 positionTop = new Vector3(polygonVert.X, polygonVert.Y, zTop);
                Vector3 positionBottom = new Vector3(polygonVert.X, polygonVert.Y, zBottom);

                // move the polygon from center (0, 0) to leftTopCorner (0, 0)
                Vector2 polygonVertAboveZero = polygonVert - new Vector2(Polygon.Left, Polygon.Top);
                Vector2 relativePolygonVert = polygonVertAboveZero / new Vector2(Polygon.Right, Polygon.Bottom);

                verts[i] = new VertexPositionColorTexture(positionTop, Color.White, (relativePolygonVert / 2) * textureAreaMultiplier + textureAreaOffset);
                // reverse (winding) order for the bottom lid, because of backface culling
                // also flip texture coordinates for bottom lid
                verts[(indices.Length * 2) - 1 - i] = new VertexPositionColorTexture(positionBottom, Color.White, (new Vector2(0.5f, 0.0f) + ((Vector2.One - relativePolygonVert) / 2)) * textureAreaMultiplier + textureAreaOffset);
            }

            return verts;
        }

        private void UpdateVertexBuffer()
        {
            GraphicsDeviceContext ctx = GameService.Graphics.LendGraphicsDeviceContext();

            VertexBuffer vertexBuffer = new VertexBuffer(ctx.GraphicsDevice, typeof(VertexPositionColorTexture), _vertices.Length, BufferUsage.WriteOnly);

            ctx.Dispose();
            
            vertexBuffer.SetData(_vertices);

            _vertexBuffer = vertexBuffer;
        }

        private void Initialize()
        {
            BuildPrism();

            GraphicsDeviceContext ctx = GameService.Graphics.LendGraphicsDeviceContext();

            _sharedEffect = new BasicEffect(ctx.GraphicsDevice)
            {
                TextureEnabled = true
            };

            ctx.Dispose();
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
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertexBuffer.VertexCount / 3);
            }
        }

        public void Update(GameTime gameTime)
        {
            /** NOOP **/
        }
    }
}
