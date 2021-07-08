using System;

namespace NEmu.Chip8
{
	public class EmulatorException : Exception
	{
		public EmulatorException(string message) : base(message) { }
	}

	static class Utility
	{
		public static void ZeroArray(this byte[] array)
		{
			int l = array.Length;
			for (int i = 0; i < l; i++)
				array[i] = 0;
		}
	}
}