public readonly struct TransportContactHistoryPlaneLayout
{
	public readonly int Width;
	public readonly int Height;
	public readonly int Capacity;

	public TransportContactHistoryPlaneLayout(int width, int height)
	{
		Width = width;
		Height = height;
		Capacity = width > 0 && height > 0 ? checked(width * height) : 0;
	}

	public bool IsFullFrameCapacity(int bufferLength) => bufferLength >= Capacity;

	public bool TryGetDestinationIndex(int sourceX, int sourceY, int stride, int destinationX, int destinationY, out int destinationIndex)
	{
		destinationIndex = -1;
		int safeStride = stride > 0 ? stride : 1;
		if (sourceX < 0 || sourceY < 0 || sourceX >= Width || sourceY >= Height ||
			destinationX < sourceX || destinationY < sourceY ||
			destinationX >= System.Math.Min(Width, sourceX + safeStride) ||
			destinationY >= System.Math.Min(Height, sourceY + safeStride))
			return false;
		destinationIndex = destinationY * Width + destinationX;
		return destinationIndex >= 0 && destinationIndex < Capacity;
	}
}
