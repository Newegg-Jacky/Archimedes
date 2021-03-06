using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Drawing.Drawing2D;

namespace Archimedes.Geometry.Primitives
{
    /// <summary>
    /// Composition of several single IGeometryBase Elements to a common Interface
    /// </summary>
    public class ComplexGeometry : IGeometryBase
    {
        #region Fields

        readonly List<IGeometryBase> _geometries = new List<IGeometryBase>();
        Vertices _verticesCache = null;
        readonly object _verticesCacheLock = new object();

        RectangleF _boundingbox;
        Rectangle2 _boundingboxsmall;

        bool _boundingboxInvalidated = true;
        bool _boundingboxsmallInvalidated = true;

        #endregion

        #region Constructor

        public ComplexGeometry() { }

        #endregion

        #region Geometry Access Methods

        public void AddGeometry(IGeometryBase geometry) {
            _geometries.Add(geometry);
            Invalidate();
        }

        public void AddGeometries(IEnumerable<IGeometryBase> geometries) {
            _geometries.AddRange(geometries);
            Invalidate();
        }

        public void RemoveGeometry(IGeometryBase geometry) {
            _geometries.Remove(geometry);
            Invalidate();
        }

        public int Count() {
            return _geometries.Count();
        }

        public IGeometryBase this[int index]{
            get {
                return _geometries[index];
            }
            set {
                _geometries[index] = value;
                Invalidate();
            }
        }

        #endregion

        #region IDrawable

        public void Draw(Graphics g) {
            foreach (var geometry in _geometries)
                geometry.Draw(g);
        }

        public void AddToPath(GraphicsPath path) {
            foreach (var geometry in _geometries)
                geometry.AddToPath(path);
        }

        #endregion

        #region Public Properties

        public Vector2 FirstPoint {
            get { return ToPath().FirstPoint; }
        }

        public Vector2 LastPoint {
            get { return ToPath().LastPoint; }
        }

        public Pen Pen {
            get {
                if (_geometries.Any())
                    return _geometries[1].Pen;
                return null;
            }
            set {
                foreach (var g in _geometries)
                    g.Pen = value;
            }
        }

        private void Invalidate() {
            lock (_verticesCacheLock)
            {
                _verticesCache = null;
            }
            _boundingboxInvalidated = true;
            _boundingboxsmallInvalidated = true;
        }

        public Vector2 Location {
            get { return MiddlePoint; }
            set { MiddlePoint = value; }
        }

        public Vector2 MiddlePoint {
            get {

                double mpointX = 0;
                double mpointY = 0;

                var middlepoints = (from g in _geometries
                                   select g.MiddlePoint).ToList();

                foreach (var pnt in middlepoints){
                    mpointX += pnt.X;
                    mpointY += pnt.Y;
                }

               return new Vector2(
                   mpointX / middlepoints.Count,
                   mpointY / middlepoints.Count);
            }
            set {
                var mov = new Vector2(MiddlePoint, value);
                this.Move(mov);
            }
        }

        #endregion

        #region Public Methods

        public void Move(Vector2 mov) {
            foreach (var g in _geometries)
                g.Move(mov);
            Invalidate();
        }

        public void Scale(double fact) {
            foreach (var g in _geometries)
                g.Scale(fact);
            Invalidate();
        }


        /// <summary>
        /// Gets a deep copy of this Object
        /// </summary>
        /// <returns></returns>
        public IGeometryBase Clone() {
            var nuv = new ComplexGeometry();
            foreach (var g in _geometries)
                nuv.AddGeometry(g.Clone());
            return nuv;
        }

        public void Prototype(IGeometryBase iprototype) {

            var prototype = iprototype as ComplexGeometry;
            if (prototype == null)
                throw new NotSupportedException();

            _geometries.Clear();
            _geometries.AddRange(prototype.GetGeometries());
            this.Pen = prototype.Pen;
        }

        public bool Contains(Vector2 point) {
            foreach (var g in _geometries) {
                if (g.Contains(point)) {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(Vector2 point, ref IGeometryBase subGeometry) {
            subGeometry = null;
            foreach (var g in _geometries) {
                if (g.Contains(point)) {
                    subGeometry = g;
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Bounding Boxes

        public RectangleF BoundingBox {
            get {
                if (_boundingboxInvalidated) {
                    _boundingbox = ToVertices().BoundingBox;
                    _boundingboxInvalidated = false;
                }
                return _boundingbox;
            }
        }

        public Rectangle2 SmallestBoundingBox {
            get {
                if (_boundingboxsmallInvalidated) {
                    _boundingboxsmall = ToPolygon2().FindSmallestBoundingBox();
                    _boundingboxsmallInvalidated = false;
                }
                return _boundingboxsmall;
            }
        }

        public Rectangle2 SmallestWidthBoundingBox {
            get {
               return ToPolygon2().FindSmallestWidthBoundingBox();
            }
        }
        

        public Circle2 BoundingCircle {
            get { return this.ToPolygon2().BoundingCircle; }
        }

#endregion

        #region Intersection Methods

        public IEnumerable<Vector2> Intersect(IGeometryBase other){
           var intercepts = new List<Vector2>();
           foreach (var g in _geometries) {
                intercepts.AddRange(g.Intersect(other));
           }
           return intercepts;
        }

        public bool IntersectsWith(IGeometryBase geometry) {
            foreach (var g in _geometries) {
                if (g.IntersectsWith(geometry)) {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region To -> Transformer Methods

        /// <summary>
        /// Returns vertices of this geometry
        /// </summary>
        /// <returns></returns>
        public Vertices ToVertices() {
            lock (_verticesCacheLock) {
                if (_verticesCache == null)
                {
                    var vertices = ToPath().ToVertices().Distinct();
                    _verticesCache = new Vertices(vertices);
                }
                return new Vertices(_verticesCache);
            }
        }

        public IEnumerable<IGeometryBase> GetGeometries() {
            return new List<IGeometryBase>(_geometries);
        }

        public Polygon2 ToPolygon2() {
            return new Polygon2(ToVertices());
        }

        /// <summary>
        /// Creates a docked path from this geometries.
        /// </summary>
        /// <returns></returns>
        public Path2 ToPath() {
            var path = new Path2();
            foreach (var g in _geometries)
                path.DockGeometry(g);
            return path;
        }

        #endregion

        #region IDisposable

        public void Dispose() {
            foreach (var g in _geometries) {
                g.Dispose();
            }
            _geometries.Clear();
        }

        #endregion
    }
}
