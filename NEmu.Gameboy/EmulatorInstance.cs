namespace NEmu.Gameboy
{
    public class EmulatorInstance
    {
	    public Memory Memory { get; set; }

	    public EmulatorInstance()
	    {
		    Memory = new Memory();
	    }
    }
}
