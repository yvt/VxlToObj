using System;
using System.Collections;
using System.Collections.Generic;

namespace VxlToObj.Core
{
	public class MeshSlice
	{
		public int Layer { get; set; }
		public Axis3 Axis { get; set; }
		public bool Face { get; set; }

		/// <summary>
		/// Gets or sets the array that contains the starting indices of every faces.
		/// </summary>
		/// <value>The array that contains indices of <code>Vertices</code>.</value>
		public int[] Faces { get; set; }
		public IntVector3[] Positions { get; set; }
		public Vector2[] UVs { get; set; } // unnormalized coordinate

		public MeshSliceFaces MeshSliceFaces =>
			new MeshSliceFaces(this);

		public MeshSlice()
		{
		}
	}

	public struct MeshSliceFace
	{
		public int StartIndex { get; set; }
		public int NumVertices { get; set; }
		public int EndIndex => StartIndex + NumVertices;
	}

	public struct MeshSliceFaces: IEnumerable<MeshSliceFace>
	{
		public MeshSlice MeshSlice { get; private set; }

		public MeshSliceFaces(MeshSlice s)
		{
			MeshSlice = s;
		}

		public IEnumerator<MeshSliceFace> GetEnumerator()
		{
			var faces = this.MeshSlice.Faces;
			for (var i = 0; i < faces.Length - 1; ++i)
			{
				yield return new MeshSliceFace()
				{
					StartIndex = faces[i],
					NumVertices = faces[i + 1] - faces[i]
				};
			}
			if (faces.Length > 0)
			{
				yield return new MeshSliceFace()
				{
					StartIndex = faces[faces.Length - 1],
					NumVertices = this.MeshSlice.Positions.Length - faces[faces.Length - 1]
				};
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}

	public class MeshSlices: IEnumerable<MeshSlice[]>
	{
		public MeshSlice[] PositiveXSlices;
		public MeshSlice[] PositiveYSlices;
		public MeshSlice[] PositiveZSlices;
		public MeshSlice[] NegativeXSlices;
		public MeshSlice[] NegativeYSlices;
		public MeshSlice[] NegativeZSlices;

		public MeshSlice[] this[Axis3 axis, bool face]
		{
			get
			{
				switch (axis)
				{
					case Axis3.X: return face ? PositiveXSlices : NegativeXSlices;
					case Axis3.Y: return face ? PositiveYSlices : NegativeYSlices;
					case Axis3.Z: return face ? PositiveZSlices : NegativeZSlices;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			set
			{
				switch (axis)
				{
					case Axis3.X:
						if (face)
						{
							PositiveXSlices = value;
						}
						else
						{
							NegativeXSlices = value;
						}
						break;
					case Axis3.Y:
						if (face)
						{
							PositiveYSlices = value;
						}
						else
						{
							NegativeYSlices = value;
						}
						break;
					case Axis3.Z:
						if (face)
						{
							PositiveZSlices = value;
						}
						else
						{
							NegativeZSlices = value;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public IEnumerator<MeshSlice[]> GetEnumerator()
		{
			yield return PositiveXSlices;
			yield return PositiveYSlices;
			yield return PositiveZSlices;
			yield return NegativeXSlices;
			yield return NegativeYSlices;
			yield return NegativeZSlices;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (var s in this)
			{
				yield return s;
			}
		}
	}
}

