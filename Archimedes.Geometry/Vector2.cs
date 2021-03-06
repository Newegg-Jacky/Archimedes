﻿/*******************************************
 * 
 *  Written by P. Büttiker
 *  
 *  Last updated: December, 2012 
 *  
 * *****************************************
 * *****************************************/
using System;
using System.Drawing;
using Archimedes.Geometry.Extensions;

namespace Archimedes.Geometry
{
    /// <summary> 
    /// 2D Vector Type
    /// </summary>
    public struct Vector2 : IEquatable<Vector2>, IOrdered<Vector2>
    {
        #region Constructors

        /// <summary>
        /// Create a new 2D Vector with given x/y Parts
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Vector2(double x, double y)
            : this() {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Create a new 2D Vector from 2 given Points
        /// </summary>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        public Vector2(Vector2 P1, Vector2 P2)
            : this(P2.X - P1.X, P2.Y - P1.Y) { }

        #endregion

        #region Static Properties

        /// <summary>
        /// Returns a Null-Vector
        /// </summary>
        public static readonly Vector2 Zero = new Vector2(0, 0); 

        /// <summary>
        /// Returns a X-Axis aligned Vector
        /// </summary>
        public static readonly Vector2 UnitX = new Vector2(1, 0);

        /// <summary>
        /// Returns a Y-Axis aligned Vector
        /// </summary>
        public static readonly Vector2 UnitY = new Vector2(0, 1);

        #endregion

        #region Public Propertys

        /// <summary>
        /// Gets the X Component of the Vector
        /// </summary>
        public readonly double X;

        /// <summary>
        /// Gets the Y Component of the Vector
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// Gets or Sets the Lenght of this Vector
        /// </summary>
        public double Lenght
        {
            get { return (Math.Sqrt(Math.Pow(this.X, 2) + Math.Pow(this.Y, 2))); }
            set {
                if (this.Lenght != 0) {
                    //v * newsize / |v|
                    this = this * (value / this.Lenght);
                } else
                    this = Vector2.Zero;
            }
        }

        /// <summary>
        /// Returns the slope of this Vector
        /// </summary>
        public double Slope
        {
            get { return (this.X != 0) ? (this.Y / this.X) : 0; }
        }

        public bool IsEmpty {
            get {
                return this == Vector2.Zero;
            }
        }

        #endregion

        #region Operator Overloads

        public static Vector2 operator *(double f1, Vector2 v1)
        {
            return new Vector2(v1.X * f1, v1.Y * f1);
        }

        public static Vector2 operator *(Vector2 v1, double f1)
        {
            return new Vector2(v1.X * f1, v1.Y * f1);
        }

        public static Vector2 operator /(Vector2 v1, double f1)
        {
            return new Vector2(v1.X / f1, v1.Y / f1);
        }

        /// <summary>
        /// Calc Scalarproduct (Dot-Product) of two Vectors
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double operator *(Vector2 v1, Vector2 v2)
        {
            return v1.DotP(v2);
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2) {
            return new Vector2(v1.X + v2.X, v1.Y + v2.Y); ;
        }

        public static Vector2 operator -(Vector2 v1, Vector2 v2) {
            return new Vector2(v1.X - v2.X, v1.Y - v2.Y); ;
        }

        public static bool operator ==(Vector2 v1, Vector2 v2){
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        public static bool operator !=(Vector2 v1, Vector2 v2) {
            return !(v1 == v2);
        }

        /// <summary>
        /// Loosing precision in this conversion
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public static implicit operator PointF(Vector2 other) {
            return new PointF((float)other.X, (float)other.Y);
        }

        public static implicit operator Vector2(PointF other) {
            return new Vector2(other.X, other.Y);
        }

        /// <summary>
        /// Warning: You may loose all floating signs
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public static explicit operator Point(Vector2 other) {
            return new Point((int)other.X, (int)other.Y);
        }

        public static implicit operator Vector2(Point other) {
            return new Vector2(other.X, other.Y);
        }

        #endregion    

        #region Static Methods

        public static Vector2 VectorFromAngle(double angle, double length)
        {
            var gk = length * Math.Sin(MathHelper.ToRadians(angle));
            var ak = length * Math.Cos(MathHelper.ToRadians(angle));
            return new Vector2(ak, gk);
        }

        /// <summary>
        /// Multiplicate a Vector with a Scalar (Equal to v1[Vector] * Operator[Number])
        /// </summary>
        /// <param name="scalar"></param>
        public static Vector2 Multiply(Vector2 v, double scalar)
        {
            return new Vector2(v.X * scalar, v.Y * scalar);
        }

        /// <summary>
        /// Divides this Vector with a scalar
        /// </summary>
        /// <param name="scalar"></param>
        public Vector2 Divide(double scalar)
        {
            if (scalar != 0) {
                return new Vector2(this.X / scalar, this.Y / scalar);
            } else
                return Vector2.Zero;
        }

        /// <summary>
        ///  Calculates twice the signed area of the given triangle (p0, p1, p2)
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double Area2(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            return p0.X * (p1.Y - p2.Y) + p1.X * (p2.Y - p0.Y) + p2.X * (p0.Y - p1.Y);
        }


        #endregion

        #region Transformation Methods

        /// <summary> 
        /// Calculates the Dot-Product of two Vectors
        /// </summary>
        /// <param name="v2">Other Vector</param>
        /// <returns></returns>
        public double DotP(Vector2 v2)
        {
            return (this.X * v2.X) + (this.Y * v2.Y);
        }

        /// <summary>
        /// Gets an new Vector based on this rotated by the specified amount
        /// </summary>
        /// <param name="angle">Rotation Angle</param>
        public Vector2 GetRotated(double angle)
        {
            var v2 = this;
            v2.Rotate(angle);
            return v2;
        }

        /// <summary>
        /// Rotates this Vector by the given amount
        /// </summary>
        /// <param name="angle"></param>
        public void Rotate(double angle)
        {
            double r = this.Lenght;
            double thisAngle = MathHelper.ToRadians(angle + this.GetAngle2X());
            this = new Vector2(r * Math.Cos(thisAngle), r * Math.Sin(thisAngle));
        }

        /// <summary>
        /// Turns this Vector into an Unit-Vector - a Vector with the same orientation but with 1 unit lenght
        /// </summary>
        /// <param name="newLenght"></param>
        /// <returns></returns>
        public void Normalize() {

            var len = this.Lenght;
            // Check for divide by zero errors
            if (len == 0) {
                this = Vector2.Zero;
            } else {
                // find the inverse of the vectors magnitude
                double inverse = 1d / len;
                this = new Vector2
                    (
                        // multiply each component by the inverse of the magnitude
                        this.X * inverse,
                        this.Y * inverse
                    );
            }
        }

        #endregion

        #region Specail Factorys

        /// <summary>Calculate a Vector which stands orthogonal on this Vector.
        /// 
        /// </summary>
        /// <param name="LEFT">The desired Direction of the new orthogonal Vector</param>
        /// <param name="direction"> </param>
        /// <returns></returns>
        public Vector2 GetOrthogonalVector(Direction direction) {
            // Crossproduct as unary operator (getting orthogonal)
            double orthVectorX, orthVectorY;
            if (direction == Direction.LEFT) {
                orthVectorX = this.Y * (-1);
                orthVectorY = this.X;
            } else { // RIGHT
                orthVectorX = this.Y;
                orthVectorY = this.X * (-1);
            }
            return new Vector2(orthVectorX, orthVectorY);
        }



        #endregion

        #region Public Query Methods

        /// <summary>
        /// Returns a new Vector values rounded by the given amount
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Vector2 Rounded(int value) {
            return new Vector2(
                Math.Round(this.X, value), 
                Math.Round(this.Y, value));
           
        }

        /// <summary>
        /// Get a point starting from this, with the given distance and angle
        /// </summary>
        /// <param name="distance">distance to the new point</param>
        /// <param name="angle">angle to the new point</param>
        /// <returns></returns>
        public Vector2 GetPoint(double distance, double angle)
        {
            var newPnt = new Vector2(
                this.X + (distance * Math.Cos(MathHelper.ToRadians(angle))),
                this.Y + (distance * Math.Sin(MathHelper.ToRadians(angle))));
            return newPnt;
        }

        public bool IsParallel(Vector2 v2) {
            return (Math.Round((this.X / this.Y)) == Math.Round((v2.X / v2.Y)));
        }

        public bool IsVertical {
            get { return (this.X == 0); }
        }

        public bool IsHorizontal {
            get { return (this.Y == 0); }
        }

        public bool IsDirectionEqual(Vector2 v2) {
            bool bEqual = false;
            if (this.IsParallel(v2)) {
                bEqual = (this.X.IsSignEqual(v2.X) && this.Y.IsSignEqual(v2.Y));
            }
            return bEqual;
        }

        public double GetAngle2X()
        {

            double degree;
            var xVector = Vector2.UnitX;

            degree = MathHelper.ToDegree(Math.Acos(this.DotP(xVector) / this.Lenght));
            if (this.Y < 0) {
                degree = 360 - degree;
            }
            return Math.Round(degree, GeometrySettings.GEOMETRY_GLOBAL_ROUND);

        }

        /// <summary>
        /// Returns the Angle between two vectors 0° - 180° 
        /// (If you need 0° -360°, use GetAngleBetweenClockWise() instead.)
        /// </summary>
        /// <param name="vbase"></param>
        /// <returns></returns>
        public double GetAngle2V(Vector2 vbase)
        {
            double gamma;
            double tmp = this.DotP(vbase) / (this.Lenght * vbase.Lenght);
            gamma = MathHelper.ToDegree(Math.Acos(tmp));
            if (gamma > 180) {          //from mathematic definition, it's always the shorter angle to return.
                gamma = 360 - gamma;
            }
            return Math.Round(gamma, GeometrySettings.GEOMETRY_GLOBAL_ROUND);
        }

        /// <summary>
        /// Returns Angle between two vectors. 
        /// The Angle is calculated from this vector until to the Destination Vector.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="direction">RIGHT = Clockwise, LEFT = other direction</param>
        /// <returns>0° - 360° Angle in degree</returns>
        public double GetAngleBetweenClockWise(Vector2 b, Direction direction)
        {
            double theta;

            theta = GetAngle2V(b);

            if (((this.Y * b.X - this.X * b.Y) > 0) == (direction == Direction.RIGHT)) {
                return theta;
            } else {
                return 360 - theta;
            }
        }

        #endregion

        #region Common Methods

        public override string ToString() {
            string vecDump = "";
            vecDump += "X: " + this.X.ToString() + "\n";
            vecDump += "Y: " + this.Y.ToString() + "\n";
            vecDump += "norm: " + this.Lenght.ToString() + "\n";
            return vecDump;
        }

        #endregion

        #region IEquatable

        public bool Equals(Vector2 other) {
            return this.Y == other.Y && this.X == other.X;
        }

        public override bool Equals(object obj) {
            if (obj is Vector2) {
                return Equals((Vector2)obj);
            } else
                return false;
        }

        public override int GetHashCode() {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }

        #endregion

        #region IOrdered

        /// <summary>
        /// Lexically check if the given Vector is less than this one
        /// </summary>
        /// <param name="o2"></param>
        /// <returns></returns>
        public bool Less(IOrdered<Vector2> o2)
        {
            var p2 = (Vector2)o2;
            return X < p2.X || X == p2.X && Y < p2.Y;
        }

        #endregion
    }
}

