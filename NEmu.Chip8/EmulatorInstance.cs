using System;
using System.IO;

namespace NEmu.Chip8
{
    public class EmulatorInstance
    {
	    public Memory Memory { get; set; }

		public Recompiler Recompiler { get; set; }

		public Renderer Renderer { get; set; }

		public bool RecompilerEnabled { get; set; } = false;

	    public EmulatorInstance()
	    {
		    Memory = new Memory();
			Memory.Initialize();

			//Memory.LoadRom(File.ReadAllBytes(@"G:\Downloads\IBM Logo.ch8"));
			Memory.LoadRom(File.ReadAllBytes(@"B:\Trip8 Demo (2008) [Revival Studios].ch8"));
			//Memory.LoadRom(File.ReadAllBytes(@"B:\chip8-test-rom.ch8"));

			Recompiler = new Recompiler();
			Renderer = new Renderer();
	    }

	    public void Tick60Hz()
	    {
		    if (Memory.DelayTimer > 0)
			    Memory.DelayTimer--;
	    }

	    public void RunInstructions(int count)
	    {
		    for (int i = 0; i < count; i++)
		    {
			    RunSingleInstruction();
		    }
	    }

	    public void RunSingleInstruction()
	    {
		    ushort rawInstruction = (ushort)((Memory.RAM[Memory.PC] << 8) | Memory.RAM[Memory.PC + 1]);

		    var instruction = DecodeInstruction(rawInstruction);

		    instruction.Execute(this, (ushort)(rawInstruction & ((1 << instruction.ArgumentSize) - 1)));

			
		    if (!instruction.AlternatePCHandling)
		    {
			    Memory.PC += 2;
			    return;
		    }
			// removing this else statement increases performance by 40-50%
			// could possibly make this class virtual and have this override when recompilation is enabled
			else if (RecompilerEnabled)
		    {
			    HandleBranch(Memory.PC);
		    }

			//if (!instruction.AlternatePCHandling)
			//	Memory.PC += 2;
		}

	    public void HandleBranch(ushort pc)
	    {
		    if (RecompilerEnabled)
		    {
			    var block = Recompiler.GetOrCreateBlock(this, pc, 100);

			    block.CompiledMethod(this);
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

					return InstructionDefinition.SysNoOp;
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
			    case 0xC: return InstructionDefinition.Random;
			    case 0xD: return InstructionDefinition.Draw;
			    case 0xF:
			    {
				    switch (rawInstruction & 0xFF)
				    {
					    case 0x07: return InstructionDefinition.ReadDelayTimer;
					    case 0x15: return InstructionDefinition.LoadDelayTimer;
					    case 0x1E: return InstructionDefinition.AddIndexRegister;
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
