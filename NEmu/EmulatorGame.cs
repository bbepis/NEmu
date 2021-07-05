using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NEmu.Chip8;

namespace NEmu
{
	class EmulatorGame : Game
	{
		public static EmulatorGame Instance { get; private set; }

		public GraphicsDeviceManager Graphics { get; set; }
		//public RenderTarget2D RenderTarget { get; set; }
		public EmulatorInstance Emulator { get; set; }
		public SpriteBatch SpriteBatch { get; set; }

		public EmulatorGame()
		{
			Graphics = new GraphicsDeviceManager(this);

			Content.RootDirectory = "Content";

			Instance = this;
		}

		protected override void Initialize()
		{
			base.Initialize();

			IsMouseVisible = true;

			//RenderTarget = new RenderTarget2D(GraphicsDevice, 64, 32, false, SurfaceFormat.Color, DepthFormat.None);
			//GraphicsDevice.SetRenderTarget(RenderTarget);
			textureBuffer = new Texture2D(GraphicsDevice, 64, 32, false, SurfaceFormat.Color);
			SpriteBatch = new SpriteBatch(GraphicsDevice);

			Emulator = new EmulatorInstance();
		}

		private Texture2D textureBuffer;
		private readonly uint[] PixelBuffer = new uint[64 * 32];

		protected override bool BeginDraw()
		{
			if (!Emulator.Memory.GraphicsDirtied)
				return false;

			return base.BeginDraw();
		}

		protected override void Draw(GameTime gameTime)
		{
			int bufferPtr = 0;

			foreach (var vbyte in Emulator.Memory.VRAM)
			{
				for (int bit = 0; bit < 8; bit++)
				{
					var vDataBit = (vbyte >> (7 - bit)) & 0x1;

					if (vDataBit == 0)
					{
						PixelBuffer[bufferPtr++] = 0xFF000000;
					}
					else
					{
						//PixelBuffer[bufferPtr++] = bit % 2 == 1 ? 0xFFCCCCCC : 0xFFFFFFFF;
						PixelBuffer[bufferPtr++] = 0xFFFFFFFF;
					}
				}
			}

			textureBuffer.SetData(PixelBuffer);
			

			GraphicsDevice.Clear(Color.Black);

			SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp,
				DepthStencilState.None, RasterizerState.CullNone);

			double ratio = Window.ClientBounds.Width / (double)Window.ClientBounds.Height;
			Rectangle destinationRectangle;

			if (Math.Abs(ratio - 2) < 0.001)
				destinationRectangle = new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
			else if (ratio > 2)
			{
				// black bars on top and bottom
				int finalHeight = (int)(Window.ClientBounds.Width / ratio);
				destinationRectangle = new Rectangle(0, (Window.ClientBounds.Height - finalHeight) / 2, Window.ClientBounds.Width, finalHeight);
			}
			else // (ratio < 2)
			{
				// black bars on left and right
				int finalWidth = (int)(Window.ClientBounds.Height * ratio);
				destinationRectangle = new Rectangle((Window.ClientBounds.Width - finalWidth) / 2, 0, finalWidth, Window.ClientBounds.Height);
			}

			SpriteBatch.Draw(textureBuffer, destinationRectangle, Color.White);

			SpriteBatch.End();

			base.Draw(gameTime);
		}

		protected override void Update(GameTime gameTime)
		{
			Input.Update();

			Emulator.RunInstructions(11);

			if (Input.KeyPressed(Keys.Space))
				using (var stream = File.OpenWrite("B:\\output.png"))
					textureBuffer.SaveAsPng(stream, 64, 32);

			base.Update(gameTime);
		}
	}
}
