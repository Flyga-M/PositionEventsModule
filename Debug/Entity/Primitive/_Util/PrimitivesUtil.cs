using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.PositionEventsModule.Debug.Entity.Primitive
{
    public static class PrimitivesUtil
    {
        /// <summary>
        /// Returns four vertices, that constitute a plane with the given <paramref name="dimensions"/> around 
        /// the <paramref name="center"/>. <paramref name="forward"/> is the relative Y dimension and 
        /// <paramref name="up"/> is the plane normal.
        /// </summary>
        /// <param name="dimensions"></param>
        /// <param name="up"></param>
        /// <param name="forward"></param>
        /// <param name="center"></param>
        /// <returns>The four plane vertices.</returns>
        public static Vector3[] GetPlaneVertices(Vector2 dimensions, Vector3 up, Vector3 forward, Vector3? center = null)
        {
            if (!center.HasValue)
            {
                center = Vector3.Zero;
            }

            Vector3 offsetCis = new Vector3(dimensions.X / 2, dimensions.Y / 2, 0);
            Vector3 offsetTrans = new Vector3(dimensions.X / 2, -dimensions.Y / 2, 0);

            var verts = new Vector3[]
            {
                -offsetCis,
                -offsetTrans,
                offsetTrans,
                offsetCis
            };

            Matrix rotationMatrix = Matrix.CreateWorld(center.Value, forward, up);

            return verts.Select(vertex =>
                Vector3.Transform(vertex, rotationMatrix)
            ).ToArray();
        }
        
        /// <summary>
        /// Returns eight vertices, that constitute a cuboid with the given <paramref name="dimensions"/> around 
        /// the <paramref name="center"/>. <paramref name="forward"/> is the relative Y dimension and 
        /// <paramref name="up"/> is the relative Z dimension.
        /// </summary>
        /// <param name="dimensions"></param>
        /// <param name="up"></param>
        /// <param name="forward"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        internal static Vector3[] GetCuboidVertices(Vector3 dimensions, Vector3 up, Vector3 forward, Vector3? center = null)
        {
            if (!center.HasValue)
            {
                center = Vector3.Zero;
            }

            Vector3 rightFrontTop = dimensions / 2;

            Vector3 rightBackBottom = new Vector3(rightFrontTop.X, -rightFrontTop.Y, -rightFrontTop.Z);
            Vector3 rightBackTop = new Vector3(rightFrontTop.X, -rightFrontTop.Y, rightFrontTop.Z);
            Vector3 rightFrontBottom = new Vector3(rightFrontTop.X, rightFrontTop.Y, -rightFrontTop.Z);

            Vector3 leftFrontTop = -rightBackBottom;
            Vector3 leftBackBottom = -rightFrontTop;
            Vector3 leftBackTop = -rightFrontBottom;
            Vector3 leftFrontBottom = -rightBackTop;

            var verts = new Vector3[]
            {
                leftBackTop,
                leftFrontTop,
                rightBackTop,
                rightFrontTop,
                
                rightFrontBottom,
                rightBackBottom,
                leftFrontBottom,
                leftBackBottom
            };

            Matrix rotationMatrix = Matrix.CreateWorld(center.Value, forward, up);

            return verts.Select(vertex =>
                Vector3.Transform(vertex, rotationMatrix)
            ).ToArray();
        }

        /// <summary>
        /// Assumes the order of <paramref name="vertices"/> to be like they are returned from 
        /// <see cref="GetCuboidVertices(Vector3, Vector3, Vector3, Vector3?)"/>. Generated for <see cref="PrimitiveType.TriangleList"/>.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        internal static VertexPositionColorTexture[] GetCuboidFaces(Vector3[] vertices, Vector3 dimensions, Vector2 textureDimensions)
        {
            Vector3 leftBackTop = vertices[0];
            Vector3 leftFrontTop = vertices[1];
            Vector3 rightBackTop = vertices[2];
            Vector3 rightFrontTop = vertices[3];

            Vector3 rightFrontBottom = vertices[4];
            Vector3 rightBackBottom = vertices[5];
            Vector3 leftFrontBottom = vertices[6];
            Vector3 leftBackBottom = vertices[7];

            // all clockwise
            Vector3[][] faces = new Vector3[][]
            {
                new Vector3[] { leftBackTop, leftFrontTop, rightBackTop }, // Top face 1
                new Vector3[] { rightBackTop, leftFrontTop, rightFrontTop }, // Top face 2
                
                new Vector3[] { rightBackTop, rightFrontTop, rightBackBottom }, // Right face 1
                new Vector3[] { rightBackBottom, rightFrontTop, rightFrontBottom }, // Right face 2

                new Vector3[] { leftBackBottom, leftFrontBottom, leftBackTop }, // Left face 1
                new Vector3[] { leftBackTop, leftFrontBottom, leftFrontTop }, // Left face 2
                
                new Vector3[] { leftFrontBottom, rightFrontBottom, leftFrontTop }, // Front face 1
                new Vector3[] { leftFrontTop, rightFrontBottom, rightFrontTop }, // Front face 2

                new Vector3[] { leftBackTop, rightBackTop, leftBackBottom }, // Back face 1
                new Vector3[] { leftBackBottom, rightBackTop, rightBackBottom }, // Back face 2

                new Vector3[] { rightBackBottom, rightFrontBottom, leftBackBottom }, // Bottom face 1
                new Vector3[] { leftBackBottom, rightFrontBottom, leftFrontBottom }, // Bottom face 2
            };

            Vector2[] textureCoordinates = GetCuboidFaceTextureCoordinates(dimensions, textureDimensions);

            List<VertexPositionColorTexture> result = new List<VertexPositionColorTexture>();

            for (int i = 0; i < faces.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    result.Add(new VertexPositionColorTexture(faces[i][j], Color.White, textureCoordinates[i+j]));
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Same order as <see cref="GetCuboidFaces(Vector3[])"/>.
        /// </summary>
        /// <param name="dimensions"></param>
        /// <param name="textureDimensions"></param>
        /// <returns></returns>
        private static Vector2[] GetCuboidFaceTextureCoordinates(Vector3 dimensions, Vector2 textureDimensions)
        {
            Dictionary<CuboidSide, Vector2[]> textureCoordinatesForPlanes = new Dictionary<CuboidSide, Vector2[]>();

            foreach (CuboidSide side in Enum.GetValues(typeof(CuboidSide)))
            {
                textureCoordinatesForPlanes[side] = GetSubTextureCoordinates(side, dimensions, textureDimensions);
            }
            
            Vector2[] textureCoordinates = new Vector2[]
            {
                textureCoordinatesForPlanes[CuboidSide.Top][2], // Top Face 1 Vertex 0
                textureCoordinatesForPlanes[CuboidSide.Top][0], // Top Face 1 Vertex 1
                textureCoordinatesForPlanes[CuboidSide.Top][3], // Top Face 1 Vertex 2

                textureCoordinatesForPlanes[CuboidSide.Top][3], // Top Face 2 Vertex 0
                textureCoordinatesForPlanes[CuboidSide.Top][0], // Top Face 2 Vertex 1
                textureCoordinatesForPlanes[CuboidSide.Top][1], // Top Face 2 Vertex 2
                
                textureCoordinatesForPlanes[CuboidSide.Right][0], // Right Face 1 Vertex 0
                textureCoordinatesForPlanes[CuboidSide.Right][1], // Right Face 1 Vertex 1
                textureCoordinatesForPlanes[CuboidSide.Right][2], // Right Face 1 Vertex 2

                textureCoordinatesForPlanes[CuboidSide.Right][2], // Right Face 2 Vertex 0
                textureCoordinatesForPlanes[CuboidSide.Right][1], // Right Face 2 Vertex 1
                textureCoordinatesForPlanes[CuboidSide.Right][3], // Right Face 2 Vertex 2

                textureCoordinatesForPlanes[CuboidSide.Left][3], // Left Face 1 Vertex 0
                textureCoordinatesForPlanes[CuboidSide.Left][2], // Left Face 1 Vertex 1
                textureCoordinatesForPlanes[CuboidSide.Left][1], // Left Face 1 Vertex 2

                textureCoordinatesForPlanes[CuboidSide.Left][1], // Left Face 2 Vertex 0
                textureCoordinatesForPlanes[CuboidSide.Left][2], // Left Face 2 Vertex 1
                textureCoordinatesForPlanes[CuboidSide.Left][0], // Left Face 2 Vertex 2

                textureCoordinatesForPlanes[CuboidSide.Front][0], // Front Face 1 Vertex 0
                textureCoordinatesForPlanes[CuboidSide.Front][1], // Front Face 1 Vertex 1
                textureCoordinatesForPlanes[CuboidSide.Front][2], // Front Face 1 Vertex 2

                textureCoordinatesForPlanes[CuboidSide.Front][2], // Front Face 2 Vertex 0
                textureCoordinatesForPlanes[CuboidSide.Front][1], // Front Face 2 Vertex 1
                textureCoordinatesForPlanes[CuboidSide.Front][3], // Front Face 2 Vertex 2

                textureCoordinatesForPlanes[CuboidSide.Back][1], // Back Face 1 Vertex 0
                textureCoordinatesForPlanes[CuboidSide.Back][3], // Back Face 1 Vertex 1
                textureCoordinatesForPlanes[CuboidSide.Back][0], // Back Face 1 Vertex 2

                textureCoordinatesForPlanes[CuboidSide.Back][0], // Back Face 2 Vertex 0
                textureCoordinatesForPlanes[CuboidSide.Back][3], // Back Face 2 Vertex 1
                textureCoordinatesForPlanes[CuboidSide.Back][2], // Back Face 2 Vertex 2

                textureCoordinatesForPlanes[CuboidSide.Bottom][0], // Bottom Face 1 Vertex 0
                textureCoordinatesForPlanes[CuboidSide.Bottom][1], // Bottom Face 1 Vertex 1
                textureCoordinatesForPlanes[CuboidSide.Bottom][2], // Bottom Face 1 Vertex 2

                textureCoordinatesForPlanes[CuboidSide.Bottom][2], // Bottom Face 2 Vertex 0
                textureCoordinatesForPlanes[CuboidSide.Bottom][1], // Bottom Face 2 Vertex 1
                textureCoordinatesForPlanes[CuboidSide.Bottom][3], // Bottom Face 2 Vertex 2
            };

            return textureCoordinates;
        }

        private static Vector2[] GetSubTextureCoordinates(CuboidSide side, Vector3 dimensions, Vector2 textureDimensions)
        {
            Rectangle bounds = GetSubTextureBounds(side, dimensions, textureDimensions);

            return new Vector2[]
            {
                new Vector2(bounds.Left, bounds.Top),
                new Vector2(bounds.Right, bounds.Top),
                new Vector2(bounds.Left, bounds.Bottom),
                new Vector2(bounds.Right, bounds.Bottom),
            };
        }

        private static Rectangle GetSubTextureBounds(CuboidSide side, Vector3 dimensions, Vector2 textureDimensions)
        {
            // left and right side depth + width
            float totalWidth = dimensions.Y * 2 + dimensions.X;
            // top and bottom depth + back and front height
            float totalHeight = dimensions.Y * 2 + dimensions.Z * 2;

            Vector2 relativePosition;
            Vector2 relativeDimensions;

            switch (side)
            {
                case CuboidSide.Top:
                    {
                        relativePosition = new Vector2(dimensions.Y / totalWidth, 0);
                        relativeDimensions = new Vector2(dimensions.X / totalWidth, dimensions.Y / totalHeight);

                        break;
                    }
                case CuboidSide.Left:
                    {
                        relativePosition = new Vector2(0, dimensions.Y / totalHeight);
                        relativeDimensions = new Vector2(dimensions.Y / totalWidth, dimensions.Z / totalHeight);

                        break;
                    }
                case CuboidSide.Right:
                    {
                        relativePosition = new Vector2((dimensions.Y + dimensions.X) / totalWidth, dimensions.Y / totalHeight);
                        relativeDimensions = new Vector2(dimensions.Y / totalWidth, dimensions.Z / totalHeight);

                        break;
                    }
                case CuboidSide.Front:
                    {
                        relativePosition = new Vector2(dimensions.Y / totalWidth, (2 * dimensions.Y + dimensions.Z) / totalHeight);
                        relativeDimensions = new Vector2(dimensions.X / totalWidth, dimensions.Z / totalHeight);

                        break;
                    }
                case CuboidSide.Back:
                    {
                        relativePosition = new Vector2(dimensions.Y / totalWidth, dimensions.Y / totalHeight);
                        relativeDimensions = new Vector2(dimensions.X / totalWidth, dimensions.Z / totalHeight);

                        break;
                    }
                case CuboidSide.Bottom:
                    {
                        relativePosition = new Vector2(dimensions.Y / totalWidth, (dimensions.Y + dimensions.Z) / totalHeight);
                        relativeDimensions = new Vector2(dimensions.X / totalWidth, dimensions.Y / totalHeight);

                        break;
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }

            Point position = GetPixelCoordinateFromRelative(relativePosition, textureDimensions);
            Point rectDimensions = GetPixelCoordinateFromRelative(relativeDimensions, textureDimensions);

            return new Rectangle(position, rectDimensions);
        }

        /// <summary>
        /// Returns the absolute pixel coordinate for the given <paramref name="relativePosition"/>.
        /// </summary>
        /// <param name="relativePosition"></param>
        /// <param name="dimensions">The dimensions of the texture.</param>
        /// <returns>The absolute pixel coordinate for the given <paramref name="relativePosition"/>.</returns>
        public static Point GetPixelCoordinateFromRelative(Vector2 relativePosition, Vector2 dimensions)
        {
            int x = (int)((float)dimensions.X * relativePosition.X);
            int y = (int)((float)dimensions.Y * relativePosition.Y);

            return new Point(x, y);
        }

        /// <summary>
        /// Reverses the face direction of the given <paramref name="vertices"/>. Assumes the vertices to 
        /// be in <see cref="PrimitiveType.TriangleList"/> configuration.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns>Returns the <paramref name="vertices"/>, with every face reversed.</returns>
        public static VertexPositionColorTexture[] ReverseFaceDirection(VertexPositionColorTexture[] vertices)
        {
            VertexPositionColorTexture[] result = new VertexPositionColorTexture[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                if (i % 3 == 1)
                {
                    continue;
                }

                if ( i % 3 == 0)
                {
                    result[i+2] = vertices[i];
                    continue;
                }

                if (i % 3 == 2)
                {
                    result[i - 2] = vertices[i];
                    continue;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a <see cref="PrimitiveType.TriangleList"/> from the given <paramref name="vertices"/>. The 
        /// <paramref name="vertices"/> are assumed to be in <see cref="PrimitiveType.TriangleStrip"/> configuration.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns>The triangle list.</returns>
        /// <exception cref="ArgumentException">When <paramref name="vertices"/>.Length is less than 3.</exception>
        public static VertexPositionColorTexture[] TriangleListFromStrip(VertexPositionColorTexture[] vertices)
        {
            if (vertices.Length < 3)
            {
                throw new ArgumentException("vertices must have at least 3 elements.", nameof(vertices));
            }
            
            VertexPositionColorTexture[] result = new VertexPositionColorTexture[(vertices.Length - 2) * 3];

            for (int i = 0; i < vertices.Length - 2; i++)
            {
                if (i % 2 == 0)
                {
                    result[i * 3] = vertices[i];
                    result[i * 3 + 1] = vertices[i + 1];
                    result[i * 3 + 2] = vertices[i + 2];
                }
                // reverse winding order of uneven faces
                else
                {
                    result[i * 3] = vertices[i + 2];
                    result[i * 3 + 1] = vertices[i + 1];
                    result[i * 3 + 2] = vertices[i];
                }
            }

            return result;
        }
    }
}
