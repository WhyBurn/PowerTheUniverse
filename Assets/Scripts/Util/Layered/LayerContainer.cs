using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

namespace AstralAvarice.Utils.Layers
{
	/// <summary>
	/// A class that maintains a sorted list of "layer" objects.
	/// This is used by the inspector and tooltip to allow multiple
	/// sources to tell the inspector or tooltip what to display, but
	/// only choose to display the top element.
	/// 
	/// Essentially a wrapping around SortedSet with an OnTopLayerChanged event.
	/// </summary>
	/// <typeparam name="LayerType">The data that is stored in the layer. Must be comparable.</typeparam>
	public class LayerContainer<LayerType> where LayerType : IComparable<LayerType>
	{
		private SortedSet<LayerType> layers = new SortedSet<LayerType>();

		/// <summary>
		/// When the topmost layer is changed, this event fires with the
		/// old and new topmost layers respectively.
		/// </summary>
		public UnityEvent<LayerType, LayerType> OnTopLayerChanged = new UnityEvent<LayerType, LayerType>();

		/// <summary>
		/// Get whether two layers are equal.
		/// Necessary because LayerType == LayerType does not compile.
		/// Checks for null.
		/// </summary>
		/// <param name="first">A layer to compare equality with.</param>
		/// <param name="second">A layer to compare equality with.</param>
		/// <returns>Whether the two layers are equal.</returns>
		private static bool GetEquality(LayerType first, LayerType second)
		{
			if (first == null && second == null)
				return true;
			else if (first == null)
				return second.Equals(first);
			else
				return first.Equals(second);
		}
		
		/// <summary>
		/// Adds a layer to the sorted set.
		/// If the topmost layer is changed, OnTopLayerChanged is invoked.
		/// </summary>
		/// <param name="layer">The layer to add to the sorted set.</param>
		/// <returns>The return value of SortedSet.Add.</returns>
		private bool Add(LayerType layer)
		{
			LayerType oldTop = layers.Max;

			bool retVal = layers.Add(layer);

			LayerType newTop = layers.Max;

			if (! GetEquality(oldTop, newTop))
				OnTopLayerChanged?.Invoke(oldTop, newTop);

			return retVal;
		}

		/// <summary>
		/// Removes the layer from the sorted set.
		/// If the topmost layer is changed (i.e. removed), OnTopLayerchanged is invoked.
		/// </summary>
		/// <param name="layer">The layer to remove from the sorted set.</param>
		/// <returns>The return value of SortedSet.Remove.</returns>
		private bool Remove(LayerType layer)
		{
			LayerType oldTop = layers.Max;

			bool retVal = layers.Remove(layer);

			LayerType newTop = layers.Max;

			if (!GetEquality(oldTop, newTop))
				OnTopLayerChanged?.Invoke(oldTop, newTop);

			return retVal;
		}
	}
}
