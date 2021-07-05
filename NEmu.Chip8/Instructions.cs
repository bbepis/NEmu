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

		public Action<EmulatorInstance, ushort> Execute { get; }

		public InstructionDefinition(string name, string encoding, byte argumentSize, Action<EmulatorInstance, ushort> execute)
		{
			Name = name;
			Encoding = encoding;
			Execute = execute;
			ArgumentSize = argumentSize;
		}
		
		public InstructionDefinition(string name, string encoding, Action<EmulatorInstance, ushort> execute) : this(name, encoding, 0, execute) { }

		public static InstructionDefinition ClearScreen = new InstructionDefinition("CLS", "00E0", (instance, arg) =>
		{
			for (int i = 0; i < 256; i++)
				instance.Memory.VRAM[i] = 0;

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition Jump = new InstructionDefinition("JMP", "1NNN", 12, (instance, arg) =>
		{
			instance.Memory.PC = arg;
		});

		public static InstructionDefinition SetRegister = new InstructionDefinition("SET REG", "6XNN", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			instance.Memory.Registers[regX] = (byte)(arg & 0xFF);

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition AddRegister = new InstructionDefinition("ADD REG", "7XNN", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			instance.Memory.Registers[regX] += (byte)(arg & 0xFF);

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition SetIndexRegister = new InstructionDefinition("SET INDEX REG", "ANNN", 12, (instance, arg) =>
		{
			instance.Memory.I = arg;

			instance.Memory.PC += 2;
		});

		public static unsafe InstructionDefinition Draw = new InstructionDefinition("DRAW", "DXYN", 12, (instance, arg) =>
		{
			byte x = (byte)(instance.Memory.Registers[arg >> 8] & 0x3F);
			byte y = (byte)(instance.Memory.Registers[(arg >> 4) & 0xF] & 0x1F);
			
			byte rows = (byte)(arg & 0xF);

			byte flag = 0;
			
			// TODO: optimize by switching loops out

			fixed (byte* vramBlockPtr = instance.Memory.VRAM)
			{
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
						// TODO: optimize with uint pointers


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
			}

			instance.Memory.GraphicsDirtied = true;
			instance.Memory.Registers[15] = flag;

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition SkipIfEqual = new InstructionDefinition("SEQ", "3XNN", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			
			instance.Memory.PC += instance.Memory.Registers[regX] == (byte)(arg & 0xFF)
				? (ushort)4
				: (ushort)2;
		});

		public static InstructionDefinition SkipIfNotEqual = new InstructionDefinition("SNEQ", "4XNN", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			
			instance.Memory.PC += instance.Memory.Registers[regX] != (byte)(arg & 0xFF)
				? (ushort)4
				: (ushort)2;
		});

		public static InstructionDefinition SkipIfRegistersEqual = new InstructionDefinition("SREQ", "5XY0", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;
			
			instance.Memory.PC += instance.Memory.Registers[regX] == instance.Memory.Registers[regY]
				? (ushort)4
				: (ushort)2;
		});

		public static InstructionDefinition SkipIfRegistersNotEqual = new InstructionDefinition("SRNEQ", "9XY0", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;
			
			instance.Memory.PC += instance.Memory.Registers[regX] != instance.Memory.Registers[regY]
				? (ushort)4
				: (ushort)2;
		});

		public static InstructionDefinition Call = new InstructionDefinition("CALL", "2NNN", 12, (instance, arg) =>
		{
			instance.Memory.CallStack.Push((ushort)(instance.Memory.PC + 2));

			instance.Memory.PC = arg;
		});

		public static InstructionDefinition Return = new InstructionDefinition("RET", "00EE", (instance, arg) =>
		{
			instance.Memory.PC = instance.Memory.CallStack.Pop();
		});

		public static InstructionDefinition SetFromRegister = new InstructionDefinition("SET REG R", "8XY0", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.Registers[regX] = instance.Memory.Registers[regY];

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition LogicalOr = new InstructionDefinition("OR", "8XY1", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.Registers[regX] |= instance.Memory.Registers[regY];

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition LogicalAnd = new InstructionDefinition("AND", "8XY2", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.Registers[regX] &= instance.Memory.Registers[regY];

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition LogicalXor = new InstructionDefinition("XOR", "8XY3", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.Registers[regX] ^= instance.Memory.Registers[regY];

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition AddCarry = new InstructionDefinition("ADD C", "8XY4", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			int result = instance.Memory.Registers[regX] + instance.Memory.Registers[regY];

			instance.Memory.Registers[15] = result > 255
				? (byte)1 : (byte)0;

			instance.Memory.Registers[regX] = (byte)result;

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition SubtractX = new InstructionDefinition("SUB X - Y", "8XY5", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.Registers[15] = instance.Memory.Registers[regX] > instance.Memory.Registers[regY]
				? (byte)1 : (byte)0;

			instance.Memory.Registers[regX] -= instance.Memory.Registers[regY];

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition SubtractY = new InstructionDefinition("SUB Y - X", "8XY7", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			int regY = (arg >> 4) & 0xF;

			instance.Memory.Registers[15] = instance.Memory.Registers[regY] > instance.Memory.Registers[regX]
				? (byte)1 : (byte)0;

			instance.Memory.Registers[regY] -= instance.Memory.Registers[regX];

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition ShiftRight = new InstructionDefinition("SHIFT R", "8XY6", 12, (instance, arg) =>
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

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition ShiftLeft = new InstructionDefinition("SHIFT L", "8XYE", 12, (instance, arg) =>
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

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition MemoryStore = new InstructionDefinition("STORE", "FX55", 12, (instance, arg) =>
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

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition MemoryRead = new InstructionDefinition("READ", "FX65", 12, (instance, arg) =>
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

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition ConvertBcd = new InstructionDefinition("CONVERT BCD", "FX33", 12, (instance, arg) =>
		{
			int regX = arg >> 8;
			byte value = instance.Memory.Registers[regX];

			instance.Memory.RAM[instance.Memory.I] = (byte)(value / 100);
			instance.Memory.RAM[instance.Memory.I + 1] = (byte)((value / 10) % 10);
			instance.Memory.RAM[instance.Memory.I + 2] = (byte)(value % 10);

			instance.Memory.PC += 2;
		});

		public static InstructionDefinition FontCharacter = new InstructionDefinition("FONT", "FX29", 12, (instance, arg) =>
		{
			int regX = arg >> 8;

			instance.Memory.I = (ushort)(0x50 + (instance.Memory.Registers[regX] & 0xF));

			instance.Memory.PC += 2;
		});
	}
}
