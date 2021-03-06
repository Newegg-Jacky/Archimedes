﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Archimedes.Geometry.Primitives;

namespace Archimedes.Geometry
{
    /// <summary>
    /// The Base of every Geometry Object.
    /// </summary>
    public interface IGeometryBase : IDrawable, IDisposable
    {
        Vector2 Location { get; set; }
        Vector2 MiddlePoint { get; set; }
        

        IGeometryBase Clone();
        void Prototype(IGeometryBase prototype);

        void Move(Vector2 mov);
        void Scale(double fact);

        Vertices ToVertices();

        void AddToPath(GraphicsPath path);

        /// <summary>
        /// Returns a Rect which fully encloses the IGeometryBase object
        /// </summary>
        RectangleF BoundingBox { get; }

        Pen Pen { get; set; }

        /// <summary>
        /// Returns a Circle which fully encloses the IGeometryBase object
        /// </summary>
        Circle2 BoundingCircle { get; }

        /// <summary>
        /// Intersects the Gemoetry Element with a other Geometry Element
        /// </summary>
        /// <param name="geometryObject"></param>
        /// <returns>true if the objects collide</returns>
        bool IntersectsWith(IGeometryBase geometryObject);

        /// <summary>
        /// Checks Intersection and returns all Points which Intersects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        IEnumerable<Vector2> Intersect(IGeometryBase other);

        /// <summary>
        /// Checks if a Point is contained in the geometry
        /// </summary>
        /// <param name="Point"></param>
        /// <returns></returns>
        bool Contains(Vector2 pos);

    }
}
