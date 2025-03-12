using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace AstralAvarice.Utils.Layers
{
	/// <summary>
	/// A base class implementation of IComparable that uses a priority
	/// int for comparison.
	/// 
	/// Intended to be inherited from and then used in the LayerContainer.
	/// 
	/// Higher priority layers (layers with a more positive priority)
	/// are more likely to be at the top than lower priority layers.
	/// 
	/// Layers cannot mutate their priority to avoid issues with
	/// sorted sets becoming blindly unordered.
	/// </summary>
	public class Layer : IComparable<Layer>
	{
		public readonly int priority;

		public Layer(int priority)
		{
			this.priority = priority;
		}

		/// <summary>
		/// Compare two layers. If this priority is higher than the
		/// other, return a positive int. If this priority is lower than
		/// the other, return a negative int. If the priorities are the same,
		/// return 0.
		/// </summary>
		/// <param name="other">The layer to compare this layer to.</param>
		/// <returns>An int representing the difference in precedence between the two layers.</returns>
		public int CompareTo(Layer other)
		{
			return priority - other.priority;
		}
	}
}
