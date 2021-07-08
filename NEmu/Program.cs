namespace NEmu
{
	class Program
	{
		static void Main()
		{
			using (var game = new MainGame())
				game.Run();
		}
	}
}
