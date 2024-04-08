using Blish_HUD;
using Blish_HUD.Entities;
using Flyga.PositionEventsModule.Debug.Entity.Primitive;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using PositionEvents.Implementation.OcTree.Bounds;

namespace Flyga.PositionEventsModule.Debug.Entity
{
    public class VoxelEntity : IEntity
    {
        private readonly Func<Vector3, bool> _isInside;

        private CuboidEntity[,,] _voxels;

        private CuboidEntity[] _visibleVoxels;

        private float _detail;

        private BoundingBox _bounds;

        private Texture2D _texture;

        /// <summary>
        /// The <see cref="Texture2D"/>, that will be wrapped on every voxel making up this <see cref="VoxelEntity"/>.
        /// </summary>
        public Texture2D VoxelTexture
        {
            get
            {
                return _texture;
            }
            set
            {
                if (_visibleVoxels != null && _visibleVoxels.Length > 0)
                {
                    foreach(CuboidEntity voxel in _visibleVoxels)
                    {
                        voxel.Texture = value;
                    }
                }

                _texture = value;
            }
        }

        /// <summary>
        /// The opacity value for every voxel.
        /// </summary>
        public float VoxelOpacity { get; set; } = 1f;

        /// <summary>
        /// An <see cref="IEnumerable{CuboidEntity}"/> with every open (not encased) voxel making up this 
        /// <see cref="VoxelEntity"/>.
        /// </summary>
        public IEnumerable<CuboidEntity> Voxels => _visibleVoxels;

        /// <summary>
        /// The voxel base length.
        /// </summary>
        public float Detail
        {
            get
            {
                return _detail;
            }
            set
            {
                _detail = value;
                CalculateVoxels();
            }
        }

        /// <summary>
        /// The <see cref="BoundingBox"/> in which the voxels will be created.
        /// </summary>
        public BoundingBox Bounds => _bounds;

        /// <summary>
        /// An offset, to display the <see cref="VoxelEntity"/> at a different position than given by 
        /// the <see cref="Bounds"/>.
        /// </summary>
        public Vector3 Offset { get; set; }

        /// <summary>
        /// The position of the <see cref="VoxelEntity"/> with the <see cref="Offset"/> applied to it. To change the 
        /// position, change the <see cref="Offset"/> accordingly.
        /// </summary>
        public Vector3 Position => _bounds.GetCenter() + Offset;

        public float DrawOrder => Vector3.Distance(Position, GameService.Gw2Mumble.PlayerCharacter.Position);

        public VoxelEntity(Texture2D texture, float opacity, Vector3 offset, Func<Vector3, bool> isInside, float detail, BoundingBox bounds)
        {
            if (detail <= 0)
            {
                throw new ArgumentException("detail must be greater than zero.", nameof(detail));
            }

            VoxelTexture = texture;
            VoxelOpacity = opacity;
            Offset = offset;
            
            _isInside = isInside;
            _detail = detail;
            // make sure Min and Max are set correctly
            _bounds = BoundingBox.CreateFromPoints(new Vector3[] { bounds.Min, bounds.Max });

            CalculateVoxels();
        }

        private void CalculateVoxels()
        {
            Vector3 dimensions = Bounds.Max - Bounds.Min;

            CuboidEntity[,,] voxels = new CuboidEntity[(int)Math.Ceiling(dimensions.X/(float)Detail), (int)Math.Ceiling(dimensions.Y / (float)Detail), (int)Math.Ceiling(dimensions.Z / (float)Detail)];

            float x = Bounds.Min.X + (Detail / 2);

            for (int indexX = 0; indexX < voxels.GetLength(0); indexX++)
            {
                float y = Bounds.Min.Y + (Detail / 2);

                for (int indexY = 0; indexY < voxels.GetLength(1); indexY++)
                {

                    float z = Bounds.Min.Z + (Detail / 2);
                    for (int indexZ = 0; indexZ < voxels.GetLength(2); indexZ++)
                    {
                        Vector3 position = new Vector3(x, y, z);

                        if (!_isInside(position))
                        {
                            voxels[indexX, indexY, indexZ] = null;
                            z += Detail;
                            continue;
                        }

                        // center position around Vector3.Zero and add Position (incl. Offset later)
                        Vector3 relativePosition = position - Bounds.GetCenter();

                        voxels[indexX, indexY, indexZ] = new CuboidEntity(VoxelTexture, VoxelOpacity, relativePosition + Position, Vector3.UnitZ, new Vector3(Detail));

                        z += Detail;
                    }

                    y += Detail;
                }

                x += Detail;
            }

            _voxels = voxels;

            CalculateVisibleVoxels();
        }

        private void CalculateVisibleVoxels()
        {
            List<CuboidEntity> visibleVoxels = new List<CuboidEntity> ();

            int lengthX = _voxels.GetLength(0);
            int lengthY = _voxels.GetLength(1);
            int lengthZ = _voxels.GetLength(2);

            for (int indexX = 0; indexX < lengthX; indexX++)
            {
                for (int indexY = 0; indexY < lengthY; indexY++)
                {
                    for (int indexZ = 0; indexZ < lengthZ; indexZ++)
                    {
                        if (_voxels[indexX, indexY, indexZ] == null)
                        {
                            continue;
                        }

                        CuboidEntity voxel = _voxels[indexX, indexY, indexZ];
                        
                        if (indexX == 0 || indexY == 0 || indexZ == 0
                            || indexX == lengthX - 1 || indexY == lengthY - 1 || indexZ == lengthZ - 1)
                        {
                            visibleVoxels.Add(voxel);
                            continue;
                        }

                        if (   _voxels[indexX + 1, indexY, indexZ] != null
                            && _voxels[indexX - 1, indexY, indexZ] != null
                            && _voxels[indexX, indexY + 1, indexZ] != null
                            && _voxels[indexX, indexY - 1, indexZ] != null
                            && _voxels[indexX, indexY, indexZ + 1] != null
                            && _voxels[indexX, indexY, indexZ - 1] != null)
                        {
                            continue;
                        }

                        visibleVoxels.Add(voxel);
                    }
                }
            }

            _visibleVoxels = visibleVoxels.ToArray();
        }

        public void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera)
        {
            foreach (CuboidEntity voxel in _visibleVoxels)
            {
                voxel.Render(graphicsDevice, world, camera);
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach(CuboidEntity voxel in _visibleVoxels)
            {
                voxel.Update(gameTime);
            }
        }
    }
}
