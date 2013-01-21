﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes.Genetics
{
    /// <summary>
    /// Base class for a solution candidate
    /// </summary>
    public abstract class Candidate : IComparable<Candidate>
    {



        protected Candidate()
        {

        }



        public abstract double GetAllelValue(Allel allel);

        public abstract void SetAllelValue(Allel allel, double value);
   

        public virtual void Prototype(Candidate prototype)
        {
            
        }


        /// <summary>
        /// Gets all allels (gen properties)
        /// </summary>
        public abstract Allel[] Allels { get; }


        /// <summary>
        /// Gets the Fitness of this candidate
        /// </summary>
        public double Fitness { get; internal set; }

        #region Implementation of IComparable<in Candidate>

        public int CompareTo(Candidate other)
        {
            return this.Fitness.CompareTo(other.Fitness);
        }

        #endregion

        public override string ToString()
        {
            return "Fitness: " + Fitness;
        }
    }
}
