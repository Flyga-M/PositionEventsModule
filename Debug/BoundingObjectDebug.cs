using Blish_HUD;
using Blish_HUD.Entities;
using Flyga.PositionEventsModule.Debug.Entity;
using Flyga.PositionEventsModule.Debug.Entity.Primitive;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PositionEvents.Area;
using PositionEvents.Implementation.OcTree.Bounds;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Flyga.PositionEventsModule.Debug
{
    public class BoundingObjectDebug : IDisposable
    {
        private static Logger Logger = Logger.GetLogger<BoundingObjectDebug>();

        private PositionEventsModule _module;

        private static readonly Dictionary<DebugColor, string> _debugColorTexturePath = new Dictionary<DebugColor, string>()
        {
            { DebugColor.Red, "Debug/red.png" },
            { DebugColor.Green, "Debug/green.png" },
            { DebugColor.Blue, "Debug/blue.png" }
        };

        private readonly Dictionary<DebugColor, Texture2D> _debugColorTexture;


        private readonly Dictionary<IBoundingObject, IEntity> _entities;

        public BoundingObjectDebug(PositionEventsModule module)
        {
            _module = module;

            _debugColorTexture = new Dictionary<DebugColor, Texture2D>();
            _entities = new Dictionary<IBoundingObject, IEntity>();
        }

        private Texture2D GetDebugTexture(DebugColor color)
        {
            string path = _debugColorTexturePath[color];

            if (!_debugColorTexture.ContainsKey(color))
            {
                _debugColorTexture[color] = _module.ContentsManager.GetTexture(path);
            }

            return _debugColorTexture[color];
        }

        /// <summary>
        /// Adds the <paramref name="boundingObject"/> as an <see cref="IEntity"/> to the world.
        /// </summary>
        /// <param name="boundingObject">The <see cref="IBoundingObject"/> to be displayed as an 
        /// <see cref="IEntity"/>.</param>
        /// <param name="color">The <see cref="DebugColor"/> with which the <paramref name="boundingObject"/> 
        /// should be rendered.</param>
        public void DisplayBoundingObject(IBoundingObject boundingObject, DebugColor color = DebugColor.Blue)
        {
            IEntity entity = GetEntity(boundingObject, GetDebugTexture(color), 0.2f);

            _entities[boundingObject] = entity;

            GameService.Graphics.World.AddEntity(entity);
        }

        /// <summary>
        /// Removes the <see cref="IEntity"/> that displays the <paramref name="boundingObject"/> from the world.
        /// </summary>
        /// <param name="boundingObject">The <see cref="IBoundingObject"/> whose <see cref="IEntity"/> 
        /// should be removed.</param>
        public void RemoveBoundingObject(IBoundingObject boundingObject)
        {
            if (!_entities.ContainsKey(boundingObject))
            {
                return;
            }

            GameService.Graphics.World.RemoveEntity(_entities[boundingObject]);

            _entities.Remove(boundingObject);
        }

        internal void RemoveAllBoundingObjects()
        {
            foreach (IBoundingObject boundingObject in _entities.Keys.ToArray())
            {
                RemoveBoundingObject(boundingObject);
            }
        }

        /// <summary>
        /// Changes the <see cref="DebugColor"/> of the <see cref="IEntity"/> associated with the given 
        /// <paramref name="boundingObject"/>. Will just display the <paramref name="boundingObject"/>, if it 
        /// has no <see cref="IEntity"/> assiciated with it.
        /// </summary>
        /// <param name="boundingObject">The <see cref="IBoundingObject"/> whose <see cref="IEntity"/> 
        /// should be changed.</param>
        /// <param name="color">The <see cref="DebugColor"/> with which the <paramref name="boundingObject"/> 
        /// should now be rendered.</param>
        public void ChangeBoundingObject(IBoundingObject boundingObject, DebugColor color)
        {
            if (!_entities.ContainsKey(boundingObject))
            {
                DisplayBoundingObject(boundingObject, color);
                return;
            }

            IEntity entity = _entities[boundingObject];

            Texture2D texture = GetDebugTexture(color);

            if (entity is CuboidEntity cuboidEntity)
            {
                cuboidEntity.Texture = texture;
                return;
            }

            if (entity is SphereEntity sphereEntity)
            {
                sphereEntity.Texture = texture;
                return;
            }

            if (entity is PrismEntity prismEntity)
            {
                prismEntity.Texture = texture;
                return;
            }

            if (entity is VoxelEntity voxelEntity)
            {
                voxelEntity.VoxelTexture = texture;
                return;
            }

            RemoveBoundingObject(boundingObject);
            DisplayBoundingObject(boundingObject, color);
        }

        /// <summary>
        /// Creates a <see cref="CuboidEntity"/>, that represents the <paramref name="boundingBox"/>.
        /// </summary>
        /// <param name="boundingBox">The <see cref="BoundingBox"/> that should be represented as a 
        /// <see cref="CuboidEntity"/>.</param>
        /// <param name="texture">The <see cref="Texture2D"/>, with which the <paramref name="boundingBox"/> should 
        /// be rendered.</param>
        /// <param name="opacity">The opacity of the resulting <see cref="CuboidEntity"/>.</param>
        /// <returns>The <see cref="CuboidEntity"/>, that represents the <paramref name="boundingBox"/>.</returns>
        public static CuboidEntity CuboidEntityFromBoundingBox(BoundingBox boundingBox, Texture2D texture, float opacity)
        {
            return new CuboidEntity(texture, opacity, boundingBox.GetCenter(), Vector3.UnitZ, boundingBox.Max - boundingBox.Min);
        }

        /// <summary>
        /// Creates a <see cref="SphereEntity"/>, that represents the <paramref name="boundingSphere"/>.
        /// </summary>
        /// <param name="boundingSphere">The <see cref="BoundingSphere"/> that should be represented as a 
        /// <see cref="SphereEntity"/>.</param>
        /// <param name="texture">The <see cref="Texture2D"/>, with which the <paramref name="boundingSphere"/> should 
        /// be rendered.</param>
        /// <param name="opacity">The opacity of the resulting <see cref="SphereEntity"/>.</param>
        /// <param name="detail">The amount of vertical segments for the sphere. If null, will be set to a 
        /// standard scaling value.</param>
        /// <returns>The <see cref="SphereEntity"/>, that represents the <paramref name="boundingSphere"/>.</returns>
        public static SphereEntity SphereEntityFromBoundingSphere(BoundingSphere boundingSphere, Texture2D texture, float opacity, int? detail = null)
        {
            if (!detail.HasValue)
            {
                detail = 8;
                if (boundingSphere.Radius > 2)
                {
                    detail += (int)(boundingSphere.Radius * 0.2f);
                }
            }

            return new SphereEntity(texture, opacity, boundingSphere.Center, Vector3.UnitZ, boundingSphere.Radius, detail.Value);
        }

        /// <summary>
        /// Creates a <see cref="PrismEntity"/>, that represents the <paramref name="prism"/>.
        /// </summary>
        /// <param name="prism">The <see cref="BoundingObjectPrism"/> that should be represented as a 
        /// <see cref="PrismEntity"/>.</param>
        /// <param name="texture">The <see cref="Texture2D"/>, with which the <paramref name="prism"/> should 
        /// be rendered.</param>
        /// <param name="opacity">The opacity of the resulting <see cref="PrismEntity"/>.</param>
        /// <returns>The <see cref="PrismEntity"/>, that represents the <paramref name="prism"/>.</returns>
        public static PrismEntity PrismEntityFromBoundingObjectPrism(BoundingObjectPrism prism, Texture2D texture, float opacity)
        {
            float length = prism.Top - prism.Bottom;

            Vector2 centerXY = PolygonUtil.GetCenter(prism.Polygon);
            float centerZ = prism.Bottom + (length / 2);

            Vector3 position = new Vector3(centerXY.X, centerXY.Y, centerZ);

            Vector3 orientation = Vector3.UnitZ;

            switch(prism.Alignment)
            {
                case Axis3.X:
                    {
                        orientation = Vector3.UnitX;
                        break;
                    }
                case Axis3.Y:
                    {
                        orientation = Vector3.UnitY;
                        break;
                    }
                case Axis3.Z:
                    {
                        orientation = Vector3.UnitZ;
                        break;
                    }
            }

            PrismEntity result = new PrismEntity(texture, opacity, position, orientation, prism.Polygon, length);

            return result;
        }

        /// <summary>
        /// Creates a <see cref="VoxelEntity"/>, that represents the <paramref name="boundingObject"/>.
        /// </summary>
        /// <param name="boundingObject">The <see cref="IBoundingObject"/> that should be represented as a 
        /// <see cref="VoxelEntity"/>.</param>
        /// <param name="texture">The <see cref="Texture2D"/>, with which the <paramref name="boundingObject"/> should 
        /// be rendered.</param>
        /// <param name="opacity">The opacity of the resulting <see cref="VoxelEntity"/>.</param>
        /// <param name="detail">The base length of the voxel cubes, that make up the <see cref="VoxelEntity"/>.</param>
        /// <returns>The <see cref="VoxelEntity"/>, that represents the <paramref name="boundingObject"/>.</returns>
        public static VoxelEntity VoxelEntityFromBoundingObject(IBoundingObject boundingObject, Texture2D texture, float opacity, float detail = 2.0f)
        {
            return new VoxelEntity(texture, opacity, Vector3.Zero, boundingObject.Contains, detail, boundingObject.Bounds);
        }

        /// <summary>
        /// Creates an <see cref="IEntity"/>, that represents the <paramref name="boundingObject"/>. 
        /// Primitives (Cuboid, Sphere and Prism) will be displayed as their respective <see cref="IEntity"/> 
        /// (<see cref="CuboidEntity"/>, <see cref="SphereEntity"/>, <see cref="PrismEntity"/>). All other 
        /// <see cref="IBoundingObject">IBoundingObjects</see> will be displayed as a <see cref="VoxelEntity"/>.
        /// </summary>
        /// <param name="boundingObject">The <see cref="IBoundingObject"/> that should be represented 
        /// as an <see cref="IEntity"/>.</param>
        /// <param name="texture">The <see cref="Texture2D"/>, with which the <paramref name="boundingObject"/> should 
        /// be rendered.</param>
        /// <param name="opacity">The opacity of the resulting <see cref="IEntity"/>.</param>
        /// <returns>The <see cref="IEntity"/> that represents the <paramref name="boundingObject"/>.</returns>
        public static IEntity GetEntity(IBoundingObject boundingObject, Texture2D texture, float opacity)
        {
            if (boundingObject is BoundingObjectSphere sphere)
            {
                return SphereEntityFromBoundingSphere(sphere.Sphere, texture, opacity);
            }

            if (boundingObject is BoundingObjectPrism prism)
            {
                return PrismEntityFromBoundingObjectPrism(prism, texture, opacity);
            }

            if (boundingObject is BoundingObjectBox box)
            {
                return CuboidEntityFromBoundingBox(box.Bounds, texture, opacity);
            }

            return VoxelEntityFromBoundingObject(boundingObject, texture, opacity);
        }

        public void Dispose()
        {
            RemoveAllBoundingObjects();

            foreach (Texture2D texture in _debugColorTexture.Values)
            {
                texture.Dispose();
            }

            _debugColorTexture.Clear();

            _module = null;
        }
    }
}
