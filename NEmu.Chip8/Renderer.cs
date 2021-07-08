using Microsoft.Xna.Framework.Graphics;

namespace NEmu.Chip8
{
	public class Renderer
	{
		public bool GraphicsDirtied = false;

		private readonly uint[] PixelBuffer = new uint[64 * 32];

		public void RenderToTexture(EmulatorInstance emulator, Texture2D texture)
		{
			if (!GraphicsDirtied)
				return;

			int bufferPtr = 0;

			foreach (var vbyte in emulator.Memory.VRAM)
			{
				for (int bit = 0; bit < 8; bit++)
				{
					var vDataBit = (vbyte >> (7 - bit)) & 0x1;

					if (vDataBit == 0)
					{
						PixelBuffer[bufferPtr++] = 0x000000FF;
					}
					else
					{
						//PixelBuffer[bufferPtr++] = bit % 2 == 1 ? 0xFFCCCCCC : 0xFFFFFFFF;
						PixelBuffer[bufferPtr++] = 0xFFFFFFFF;
					}
				}
			}

			texture.SetData(PixelBuffer);

			GraphicsDirtied = false;
		}
	}
}
