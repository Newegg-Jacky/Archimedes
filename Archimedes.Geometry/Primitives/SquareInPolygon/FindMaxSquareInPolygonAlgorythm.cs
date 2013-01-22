﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using Archimedes.Genetics;
using Archimedes.Genetics.Mutators;

namespace Archimedes.Geometry.Primitives.SquareInPolygon
{
    /// <summary>
    /// Genetic algorythm which searches the biggest square inside a polygon
    /// </summary>
    public class FindMaxSquareInPolygonAlgorythm : GeneticAlgorythm<SquareCandidate>
    {
        private Polygon2 _polygon;
        private Vector2 _center;
        private double _area;
        private double _areaSqrt;
        private RectangleF _boundingBox;

        

        [DebuggerStepThrough]
        public void SetPolygon(Polygon2 polygon)
        {
            if (polygon == null)
                throw new ArgumentNullException();

            Reset();

            _polygon = polygon;

            // calculate often required values

            _center = _polygon.MiddlePoint;
            _area = polygon.Area;
            _areaSqrt = Math.Sqrt(_area);
            _boundingBox = _polygon.BoundingBox;

            AdjustAllels();
        }


        /// <summary>
        /// Adjust the allels boundaries, so that we have usable limits
        /// </summary>
        private void AdjustAllels()
        {
            SquareCandidate.AllelY.MinValue = _boundingBox.Top;
            SquareCandidate.AllelY.MaxValue = _boundingBox.Bottom;

            SquareCandidate.AllelX.MinValue = _boundingBox.Right;
            SquareCandidate.AllelX.MaxValue = _boundingBox.Left;

            SquareCandidate.AllelSize.MinValue = 0;
            SquareCandidate.AllelSize.MaxValue = _boundingBox.Width;
        }


        /// <summary>
        /// Find the biggest Square inside a polygon
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public Rectangle2 FindMaxSquare(CancellationToken cancellationToken)
        {
            StartEvolution(GodLikePopulation(), cancellationToken);
            return BestCandidate != null ? BestCandidate.Geometry : null;
        }


        #region Overrides of GeneticAlgorythm<SquareCandidate>

        protected override IEnumerable<SquareCandidate> Selection(IEnumerable<SquareCandidate> currentPopulation)
        {
            //
            // evolution is hard and so is the population maximum  - only the best can survive :)
            //

            return (from c in currentPopulation
                    //where !c.IsOutside // we may remove all candidates which are no valid solutions
                    orderby c descending 
                    select c).Take(MaxPopulation);
        }

        /// <summary>
        /// Recombine two candidates into a new candidate
        /// </summary>
        /// <param name="dominator">the stronger parent</param>
        /// <param name="slave">the weaker parent</param>
        /// <returns></returns>
        public override SquareCandidate Recombine(SquareCandidate dominator, SquareCandidate slave)
        {
            var child = new SquareCandidate();

            // check if the dominators fitness is really better 
            // if not, swap them
            if (dominator.Fitness < slave.Fitness) 
            {   // adjust the slots
                SquareCandidate tmp = dominator;
                dominator = slave;
                slave = dominator;
            }

            // now apply each allel from either the dominator or the slave parent
            foreach (var allel in dominator.Allels)
            {
                // genes of the successor parent are dominating
                // we simulate this with a monte carlo selection
                // 75% chance that the dominators allel is used

                double value = (Random.NextDouble() < 0.75) ? dominator.GetAllelValue(allel) : slave.GetAllelValue(allel);
                child.SetAllelValue(allel, value); 
            }
            return child;
        }


        /// <summary>
        /// Mutate the given candidate randomly
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns>Returns a mutant of the given candidate</returns>
        public override SquareCandidate Mutate(SquareCandidate candidate)
        {
            var mutationTarget = GenericAlleleMutator.PickRandomAllel(candidate);
            var mutant =  GenericAlleleMutator.MutateRandom(candidate, mutationTarget);

            return mutant;
        }

        /// <summary>
        /// you guessed it from the name - this method creates a new candidate without any "parents"
        /// </summary>
        /// <returns></returns>
        private SquareCandidate GodLike()
        {
            // that we don't create something completly useless, we base our new candidate on the middlepoint of the polygon
            var newCandidate = new SquareCandidate(_center, _areaSqrt / 10, 0);
            return newCandidate;
        }

        /// <summary>
        /// Returns an initial population - fills the whole world
        /// </summary>
        /// <returns></returns>
        private  IEnumerable<SquareCandidate> GodLikePopulation()
        {
            var world = _boundingBox;
            double size = _areaSqrt / 10d;
            double sizeHalf = size / 2d;
            int columns = (int)world.Width / (int)size;
            int rows = (int)world.Height / (int)size;

            var population = new SquareCandidate[columns * rows];

            int z = 0;
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    population[z++] = new SquareCandidate(
                        new Vector2(
                            i * size + sizeHalf + world.X,
                            j * size + sizeHalf + world.Y),
                        size,
                        0);
                }
            }

            return population;
        }


        protected override void CalculateFitness(SquareCandidate candidate)
        {
            // was the fitness already calculated?
            if (candidate.Fitness.HasValue) return;

            double fitness = 0;

            var rectPoly = candidate.Geometry.ToPolygon2();

            fitness = candidate.Geometry.Width;

            if (!_polygon.Contains(rectPoly))
            {
                // the candidate is not fully inside the polygon
                // we have to reduce the fitness dramatically
                fitness = candidate.Geometry.Width * 0.001;
                candidate.IsOutside = true;
            }

            candidate.Fitness = fitness;
        }

        #endregion
    }
}