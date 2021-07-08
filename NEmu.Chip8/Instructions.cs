using System;
using System.Diagnostics;

namespace NEmu.Chip8
{
	[DebuggerDisplay("{Name} ({Encoding})")]
	public class InstructionDefinition
	{
		public string Name { get; set; }
		public string Encoding { get; set; }

		public byte ArgumentSize { get; set; }
		public bool AlternatePCHandling { get; set; }

		public Action<EmulatorInstance, ushort> Execute { get; }

		public InstructionDefinition(string name, string encoding, byte argumentSize, bool alternatePCHandling, Action<EmulatorInstance, ushort> execute)
		{
			Name = name;
			Encoding = encoding;
			Execute = execute;
			ArgumentSize = argumentSize;
			AlternatePCHandling = alternatePCHandling;
		}
		
		public InstructionDefinition(string name, string encoding, byte argumentSize, Action<EmulatorInstance, ushort> execute) : this(name, encoding, argumentSize, false, execute) { }
		public InstructionDefinition(string name, string encoding, Action<EmulatorInstance, ushort> execute) : this(name, encoding, 0, false, execute) { }

		#region Control flow

		public static InstructionDefinition Jump = new InstructionDefinition("JP r12", "1NNN", 12, true, (instance, arg) =>
		{
			instance.Memory.PC = arg;
		});
		
		public static InstructionDefinition SysNoOp = new InstructionDefinition("SYS (NO-OP)", "0NNN", (instance, arg) => { });
		
		public static InstructionDefinition SkipIfEqual = new InstructionDefinition("SE Vx, r8", "3XNN", 12, true, (instance, arg) =>
		{
			int regX = arg >> 8;

			instance.Memory.PC += instance.Memory.Registers[regX] == (byte)(arg & 0xFF)
				? (ushort)4
				: (ushort)2;
		});

		public static InstructionDefinition SkipIfNotEqual = new InstructionDefinition("SNE Vx, r8", "4XNN", 12, true, (instance, arg) =>
		{
			int regX = arg >> 8;

			instance.Memory.PC += instance.Memory.Registers[regX] != (byte)(arg & 0xFF)
				? (ushort)4
				: (ushort)2;
		});

		public static InstructionDefinition SkipIfRegistersEqual = new InstructionDefinition("SE Vx, Vy", "5XY0", 12, true, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.PC += instance.Memory.Registers[regX] == instance.Memory.Registers[regY]
				? (ushort)4
				: (ushort)2;
		});

		public static InstructionDefinition SkipIfRegistersNotEqual = new InstructionDefinition("SNE Vx, Vy", "9XY0", 12, true, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.PC += instance.Memory.Registers[regX] != instance.Memory.Registers[regY]
				? (ushort)4
				: (ushort)2;
		});

		public static InstructionDefinition Call = new InstructionDefinition("CALL r12", "2NNN", 12, true, (instance, arg) =>
		{
			instance.Memory.CallStack.Push((ushort)(instance.Memory.PC + 2));

			instance.Memory.PC = arg;
		});

		public static InstructionDefinition Return = new InstructionDefinition("RET", "00EE", 0, true, (instance, arg) =>
		{
			instance.Memory.PC = instance.Memory.CallStack.Pop();
		});
		#endregion

		#region Memory / Registers

		public static InstructionDefinition SetRegister = new InstructionDefinition("LD Vx, r8", "6XNN", 12, false, (instance, arg) =>
		{
			int regX = arg >> 8;
			instance.Memory.Registers[regX] = (byte)(arg & 0xFF);
		});

		public static InstructionDefinition AddRegister = new InstructionDefinition("ADD Vx, r8", "7XNN", 12, false, (instance, arg) =>
		{
			int regX = arg >> 8;
			instance.Memory.Registers[regX] += (byte)(arg & 0xFF);
		});

		public static InstructionDefinition SetIndexRegister = new InstructionDefinition("LD I, r12", "ANNN", 12, (instance, arg) =>
		{
			instance.Memory.I = arg;
		});

		public static InstructionDefinition AddIndexRegister = new InstructionDefinition("ADD I, Vx", "FX1E", 12, (instance, arg) =>
		{
			int regX = arg >> 8;

			instance.Memory.I += instance.Memory.Registers[regX];
		});

		public static InstructionDefinition FontCharacter = new InstructionDefinition("LD I (FONT), Vx", "FX29", 12, (instance, arg) =>
		{
			int regX = arg >> 8;

			instance.Memory.I = (ushort)(0x50 + (instance.Memory.Registers[regX] & 0xF));
		});

		public static InstructionDefinition MemoryStore = new InstructionDefinition("LD [I], Vx", "FX55", 12, (instance, arg) =>
		{
			// TODO: Add configurable logic
			// https://tobiasvl.github.io/blog/write-a-chip-8-emulator/#fx55-and-fx65-store-and-load-memory

			int regX = arg >> 8;

			bool legacyIncrement = false;

			for (int i = 0; i < regX + 1; i++)
				instance.Memory.RAM[instance.Memory.I + i] = instance.Memory.Registers[i];

			if (legacyIncrement)
			{
				instance.Memory.I += (ushort)(regX + 1);
			}
		});

		public static InstructionDefinition MemoryRead = new InstructionDefinition("LD Vx, [I]", "FX65", 12, (instance, arg) =>
		{
			// TODO: Add configurable logic
			// https://tobiasvl.github.io/blog/write-a-chip-8-emulator/#fx55-and-fx65-store-and-load-memory

			int regX = arg >> 8;

			bool legacyIncrement = false;

			for (int i = 0; i < regX + 1; i++)
				instance.Memory.Registers[i] = instance.Memory.RAM[instance.Memory.I + i];

			if (legacyIncrement)
			{
				instance.Memory.I += (ushort)(regX + 1);
			}
		});

		#endregion

		#region Drawing

		public static InstructionDefinition Draw = new InstructionDefinition("DRAW Vx, Vy, r4", "DXYN", 12, (instance, arg) =>
		{
			byte x = (byte)(instance.Memory.Registers[arg >> 8] & 0x3F);
			byte y = (byte)(instance.Memory.Registers[(arg >> 4) & 0xF] & 0x1F);
			
			byte rows = (byte)(arg & 0xF);

			byte flag = 0;
			
			// TODO: optimize by switching loops out

			//fixed (byte* vramBlockPtr = instance.Memory.VRAM)
			//{
			for (int row = 0; row < rows; row++)
			{
				byte spriteData = instance.Memory.RAM[instance.Memory.I + row];

				int vramPtr = (x / 8) + (y * 8);

				void setVramData(byte data)
				{
					if ((instance.Memory.VRAM[vramPtr] & data) != 0)
						flag = 1;

					instance.Memory.VRAM[vramPtr] ^= data;
				}

				int offset = x & 0x07;

				if (offset == 0)
				{
					setVramData(spriteData);
				}
				else
				{
					// TODO: optimize with ushort pointers


					//if (x < 24)
					//{
					//	ushort* longPtr = (ushort*)(vramBlockPtr + vramPtr);
					//	ushort offsetData = (ushort)(spriteData << (8 - offset));

					//	if ((*longPtr & offsetData) != 0)
					//		flag = 1;

					//	*longPtr ^= offsetData;
					//}
					//else
					//{
					//	byte firstDataHalf = (byte)(spriteData >> offset);
					//	setVramData(firstDataHalf);
					//}


					byte firstDataHalf = (byte)(spriteData >> offset);
					setVramData(firstDataHalf);

					if (x < 56)
					{
						vramPtr++;
						byte secondDataHalf = (byte)((spriteData) << (8 - offset));
						setVramData(secondDataHalf);
					}
				}

				if (++y > 31)
					break;
			}
			//}

			instance.Renderer.GraphicsDirtied = true;
			instance.Memory.Registers[15] = flag;
		});

		public static InstructionDefinition ClearScreen = new InstructionDefinition("CLS", "00E0", (instance, arg) =>
		{
			for (int i = 0; i < 256; i++)
				instance.Memory.VRAM[i] = 0;

			instance.Renderer.GraphicsDirtied = true;
		});

		#endregion

		#region Arithmetic

		public static InstructionDefinition SetFromRegister = new InstructionDefinition("LD Vx, Vy", "8XY0", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.Registers[regX] = instance.Memory.Registers[regY];
		});

		public static InstructionDefinition LogicalOr = new InstructionDefinition("OR Vx, Vy", "8XY1", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.Registers[regX] |= instance.Memory.Registers[regY];
		});

		public static InstructionDefinition LogicalAnd = new InstructionDefinition("AND Vx, Vy", "8XY2", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.Registers[regX] &= instance.Memory.Registers[regY];
		});

		public static InstructionDefinition LogicalXor = new InstructionDefinition("XOR Vx, Vy", "8XY3", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.Registers[regX] ^= instance.Memory.Registers[regY];
		});

		public static InstructionDefinition AddCarry = new InstructionDefinition("ADD+C Vx, Vy", "8XY4", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			int result = instance.Memory.Registers[regX] + instance.Memory.Registers[regY];

			instance.Memory.Registers[15] = result > 255
				? (byte)1 : (byte)0;

			instance.Memory.Registers[regX] = (byte)result;
		});

		public static InstructionDefinition SubtractX = new InstructionDefinition("SUB Vx, Vy", "8XY5", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.Registers[15] = instance.Memory.Registers[regX] > instance.Memory.Registers[regY]
				? (byte)1 : (byte)0;

			instance.Memory.Registers[regX] -= instance.Memory.Registers[regY];
		});

		public static InstructionDefinition SubtractY = new InstructionDefinition("SUB Vy, Vx", "8XY7", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.Registers[15] = instance.Memory.Registers[regY] > instance.Memory.Registers[regX]
				? (byte)1 : (byte)0;

			instance.Memory.Registers[regY] -= instance.Memory.Registers[regX];
		});

		public static InstructionDefinition ShiftRight = new InstructionDefinition("SHR Vx", "8XY6", 12, (instance, arg) =>
		{
			// TODO: Add configurable logic
			// https://tobiasvl.github.io/blog/write-a-chip-8-emulator/#8xy6-and-8xye-shift

			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			bool legacyShift = false;

			if (legacyShift)
			{
				instance.Memory.Registers[regX] = instance.Memory.Registers[regY];
			}

			instance.Memory.Registers[15] = (byte)(instance.Memory.Registers[regX] & 0x1);

			instance.Memory.Registers[regX] >>= 1;
		});

		public static InstructionDefinition ShiftLeft = new InstructionDefinition("SHL Vx", "8XYE", 12, (instance, arg) =>
		{
			// TODO: Add configurable logic
			// https://tobiasvl.github.io/blog/write-a-chip-8-emulator/#8xy6-and-8xye-shift

			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			bool legacyShift = false;

			if (legacyShift)
			{
				instance.Memory.Registers[regX] = instance.Memory.Registers[regY];
			}

			instance.Memory.Registers[15] = (byte)(instance.Memory.Registers[regX] >> 7);

			instance.Memory.Registers[regX] <<= 1;
		});

		public static InstructionDefinition ConvertBcd = new InstructionDefinition("LD [I] (BCD), Vx", "FX33", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			byte value = instance.Memory.Registers[regX];

			instance.Memory.RAM[instance.Memory.I] = (byte)(value / 100);
			instance.Memory.RAM[instance.Memory.I + 1] = (byte)((value / 10) % 10);
			instance.Memory.RAM[instance.Memory.I + 2] = (byte)(value % 10);

			instance.Memory.PC += 2;
		});

		#endregion

		#region Timer

		public static InstructionDefinition LoadDelayTimer = new InstructionDefinition("LD DT, Vx", "FX15", 12, (instance, arg) =>
		{
			int regX = arg >> 8;

			instance.Memory.DelayTimer = instance.Memory.Registers[regX];
		});

		public static InstructionDefinition ReadDelayTimer = new InstructionDefinition("LD Vx, DT", "FX07", 12, (instance, arg) =>
		{
			int regX = arg >> 8;

			instance.Memory.Registers[regX] = instance.Memory.DelayTimer;
		});

		#endregion

		#region Misc

		private static Random _random = new Random();
		public static InstructionDefinition Random = new InstructionDefinition("RAND Vx, d8", "CXNN", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int n = arg & 0xFF;

			instance.Memory.Registers[regX] = (byte)(_random.Next(0, 256) & n);
		});

		#endregion
	}
}