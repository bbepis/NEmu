using System;
using System.IO;

namespace NEmu.Chip8
{
    public class EmulatorInstance
    {
	    public Memory Memory { get; set; }

	    public EmulatorInstance()
	    {
		    Memory = new Memory();
			Memory.Initialize();

			//Memory.LoadRom(File.ReadAllBytes(@"G:\Downloads\IBM Logo.ch8"));
			Memory.LoadRom(File.ReadAllBytes(@"B:\chip8-test-rom.ch8"));
	    }

	    public void RunInstructions(int count)
	    {
		    for (int i = 0; i < count; i++)
		    {
			    ushort rawInstruction = (ushort)((Memory.RAM[Memory.PC] << 8) | Memory.RAM[Memory.PC + 1]);

			    var instruction = DecodeInstruction(rawInstruction);

			    instruction.Execute(this, (ushort)(rawInstruction & ((1 << instruction.ArgumentSize) - 1)));
		    }
	    }

	    public InstructionDefinition DecodeInstruction(ushort rawInstruction)
	    {
		    switch (rawInstruction >> 12)
		    {
			    case 0x0:
			    {
				    if (rawInstruction == 0x00E0)
					    return InstructionDefinition.ClearScreen;

				    if (rawInstruction == 0x00EE)
					    return InstructionDefinition.Return;

				    throw new Exception($"Unknown instruction: {rawInstruction:X}");
			    }
			    case 0x1: return InstructionDefinition.Jump;
			    case 0x2: return InstructionDefinition.Call;
			    case 0x3: return InstructionDefinition.SkipIfEqual;
			    case 0x4: return InstructionDefinition.SkipIfNotEqual;
			    case 0x5: return InstructionDefinition.SkipIfRegistersEqual;
			    case 0x6: return InstructionDefinition.SetRegister;
			    case 0x7: return InstructionDefinition.AddRegister;
			    case 0x8:
			    {
				    switch (rawInstruction & 0xF)
				    {
						case 0x0: return InstructionDefinition.SetFromRegister;
						case 0x1: return InstructionDefinition.LogicalOr;
						case 0x2: return InstructionDefinition.LogicalAnd;
						case 0x3: return InstructionDefinition.LogicalXor;
						case 0x4: return InstructionDefinition.AddCarry;
						case 0x5: return InstructionDefinition.SubtractX;
						case 0x6: return InstructionDefinition.ShiftRight;
						case 0x7: return InstructionDefinition.SubtractY;
						case 0xE: return InstructionDefinition.ShiftLeft;
				    }
					goto default;
			    }
			    case 0x9: return InstructionDefinition.SkipIfRegistersNotEqual;
			    case 0xA: return InstructionDefinition.SetIndexRegister;
			    case 0xD: return InstructionDefinition.Draw;
			    case 0xF:
			    {
				    switch (rawInstruction & 0xFF)
				    {
					    case 0x29: return InstructionDefinition.FontCharacter;
					    case 0x33: return InstructionDefinition.ConvertBcd;
					    case 0x55: return InstructionDefinition.MemoryStore;
					    case 0x65: return InstructionDefinition.MemoryRead;
				    }
				    goto default;
			    }
				default:
				    throw new Exception($"Unknown instruction: {rawInstruction:X}");
		    }
		}
    }
}
