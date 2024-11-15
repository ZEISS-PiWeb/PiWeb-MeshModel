#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.PiWeb.MeshModel
{
	#region usings

	using Zeiss.PiWeb.ColorScale;

	#endregion

	/// <summary>
	/// Represents a triangulated mesh.
	/// </summary>
	public interface IMesh
	{
		#region properties

		/// <summary>
		/// Gets the name of the triangle mesh.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets a value indicating whether this instance is empty (Has no points).
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		/// Gets the index of the mesh in the <see cref="MeshModelPart"/>.
		/// </summary>
		int Index { get; }

		/// <summary>
		/// Gets the normal data array.
		/// </summary>
		Vector3F[] Normals { get; }

		/// <summary>
		///Gets the position data array.
		/// </summary>
		Vector3F[] Positions { get; }

		/// <summary>
		/// Gets the texture data array.
		/// </summary>
		Vector2F[] TextureCoordinates { get; }

		/// <summary>
		/// Gets the number of triangle indices
		/// </summary>
		int TriangleIndicesCount { get; }

		/// <summary>
		/// Gets the default color of the mesh.
		/// </summary>
		Color? Color { get; }

		/// <summary>
		/// Gets the color data array.
		/// </summary>
		Color[] Colors { get; }

		/// <summary>
		/// Gets the layers to which this mesh belongs
		/// </summary>
		string[] Layer { get; }

		/// <summary>
		/// Gets the bounding box of this triangle mesh.
		/// </summary>
		Rect3F Bounds { get; }

		#endregion

		#region methods

		/// <summary>
		/// Gibt die Dreiecksindizes des CAD-Modells zurück.
		/// </summary>
		int[] GetTriangleIndices();

		/// <summary>
		/// Gets the index of the triangle.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		int GetTriangleIndex( int index );

		/// <summary>
		/// Creates a mesh based on the current mesh, but with the specified default color.
		/// </summary>
		/// <param name="color">The default color.</param>
		/// <returns></returns>
		Mesh MeshWithColor( Color color );

		/// <summary>
		/// Creates a mesh based on the current mesh, but with a default color.
		/// </summary>
		/// <returns></returns>
		Mesh MeshWithoutColor();

		/// <summary>
		/// Creates a mesh based on the current mesh, but without color data.
		/// </summary>
		/// <returns></returns>
		Mesh MeshWithoutColors();

		#endregion
	}
}