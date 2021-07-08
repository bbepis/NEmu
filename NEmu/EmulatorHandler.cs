using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NEmu.Chip8;

namespace NEmu
{
	class EmulatorHandler
	{
		public EmulatorInstance Emulator { get; set; }
		public Thread EmulatorThread { get; set; }

		public bool Turbo { get; set; } = false;
		
		private Texture2D textureBuffer;
		private SpriteFont Arial;

		public void Initialize(MainGame game)
		{
			textureBuffer = new Texture2D(game.GraphicsDevice, 64, 32, false, SurfaceFormat.Color);

			Emulator = new EmulatorInstance();

			EmulatorThread = new Thread(EmulatorThreadTask);
			EmulatorThread.IsBackground = true;
			EmulatorThread.Start();

			Arial = game.Content.Load<SpriteFont>("Arial");
		}

		public void Update(GameTime gameTime)
		{
			Emulator.Tick60Hz();

			instructionsSinceLastSecond_shadow += instructionsSinceLastTick;
			instructionsSinceLastTick = 0;

			if ((gameTime.TotalGameTime - lastGameTime).TotalSeconds >= 1)
			{
				lastGameTime = gameTime.TotalGameTime;
				instructionsSinceLastSecond = instructionsSinceLastSecond_shadow;
				instructionsSinceLastSecond_shadow = 0;
			}

			if (Input.KeyPressed(Keys.Space))
				Turbo = !Turbo;

			if (Input.KeyPressed(Keys.F2))
				using (var stream = File.OpenWrite("B:\\output.png"))
					textureBuffer.SaveAsPng(stream, 64, 32);

			tickResetEvent.Set();
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			Emulator.Renderer.RenderToTexture(Emulator, textureBuffer);

			var windowSize = MainGame.Instance.Window.ClientBounds;

			double ratio = windowSize.Width / (double)windowSize.Height;
			Rectangle destinationRectangle;

			if (Math.Abs(ratio - 2) < 0.001)
				destinationRectangle = new Rectangle(0, 0, windowSize.Width, windowSize.Height);
			else if (ratio < 2)
			{
				// black bars on top and bottom
				int finalHeight = windowSize.Width / 2;
				destinationRectangle = new Rectangle(0, (windowSize.Height - finalHeight) / 2, windowSize.Width, finalHeight);
			}
			else // (ratio > 2)
			{
				// black bars on left and right
				int finalWidth = windowSize.Height * 2;
				destinationRectangle = new Rectangle((windowSize.Width - finalWidth) / 2, 0, finalWidth, windowSize.Height);
			}

			spriteBatch.Draw(textureBuffer, destinationRectangle, Color.White);

			spriteBatch.DrawString(Arial, $"{instructionsSinceLastSecond} ({millisecondsToComplete}ms)", Vector2.One, Color.White);
		}
		

		private TimeSpan lastGameTime = TimeSpan.Zero;
		private uint instructionsSinceLastTick = 0;
		private uint instructionsSinceLastSecond_shadow = 0;
		private uint instructionsSinceLastSecond = 0;
		private double millisecondsToComplete = 0;
		private readonly ManualResetEventSlim tickResetEvent = new ManualResetEventSlim(false);
		private void EmulatorThreadTask()
		{
			Stopwatch stopwatch = new Stopwatch();

			while (true)
			{
				if (Turbo)
				{
					while (Turbo)
					{
						const int instructionCount = 1000;

						Emulator.RunInstructions(instructionCount);

						instructionsSinceLastTick += instructionCount;
					}

					continue;
				}

				if (!tickResetEvent.Wait(100))
					continue;

				tickResetEvent.Reset();

				stopwatch.Start();

				Emulator.RunInstructions(11);
				instructionsSinceLastTick += 11;

				stopwatch.Stop();
				millisecondsToComplete = stopwatch.Elapsed.TotalMilliseconds;
				stopwatch.Reset();
			}
		}
	}
}
