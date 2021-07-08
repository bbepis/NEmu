using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NEmu
{
	class MainGame : Game
	{
		public static MainGame Instance { get; private set; }

		public GraphicsDeviceManager Graphics { get; set; }

		//public RenderTarget2D RenderTarget { get; set; }
		public SpriteBatch SpriteBatch { get; set; }

		public EmulatorHandler EmulatorHandler { get; set; }

		public MainGame()
		{
			Graphics = new GraphicsDeviceManager(this);

			Content.RootDirectory = "Content";

			Instance = this;

			Window.AllowUserResizing = true;
		}

		protected override void Initialize()
		{
			base.Initialize();

			IsMouseVisible = true;

			//RenderTarget = new RenderTarget2D(GraphicsDevice, 64, 32, false, SurfaceFormat.Color, DepthFormat.None);
			//GraphicsDevice.SetRenderTarget(RenderTarget);
			
			SpriteBatch = new SpriteBatch(GraphicsDevice);

			Window.AllowUserResizing = true;

			EmulatorHandler = new EmulatorHandler();
			EmulatorHandler.Initialize(this);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp,
				DepthStencilState.None, RasterizerState.CullNone);

			EmulatorHandler.Draw(SpriteBatch);

			SpriteBatch.End();

			base.Draw(gameTime);
		}

		protected override void Update(GameTime gameTime)
		{
			Input.Update();

			EmulatorHandler.Update(gameTime);

			base.Update(gameTime);
		}
	}
}