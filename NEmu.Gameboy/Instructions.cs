using System;
using System.Runtime.CompilerServices;

namespace NEmu.Gameboy
{
	public class Instruction
	{
		public string Name { get; set; }
		public byte CycleCount { get; set; }
		public byte BranchedCycleCount { get; set; }

		public byte ArgumentSize { get; set; }

		public Action<EmulatorInstance> Execute { get; }

		public Instruction(string name, byte cycleCount, byte branchedCycleCount, byte argumentSize, Action<EmulatorInstance> execute)
		{
			Name = name;
			CycleCount = cycleCount;
			BranchedCycleCount = branchedCycleCount;
			Execute = execute;
			ArgumentSize = argumentSize;
		}

		public Instruction(string name, byte cycleCount, byte argumentSize, Action<EmulatorInstance> execute) : this(name, cycleCount, cycleCount, argumentSize, execute) { }
		public Instruction(string name, byte cycleCount, Action<EmulatorInstance> execute) : this(name, cycleCount, cycleCount, 0, execute) { }



		public static Instruction[] CpuInstructions = new Instruction[512];

		static Instruction()
		{
			CpuInstructions[0x06] = new Instruction("LD B, r8", 8, 1, e => Load8Immediate(e, ref e.Memory.B));
			CpuInstructions[0x0E] = new Instruction("LD C, r8", 8, 1, e => Load8Immediate(e, ref e.Memory.C));
			CpuInstructions[0x16] = new Instruction("LD D, r8", 8, 1, e => Load8Immediate(e, ref e.Memory.D));
			CpuInstructions[0x1E] = new Instruction("LD E, r8", 8, 1, e => Load8Immediate(e, ref e.Memory.E));
			CpuInstructions[0x26] = new Instruction("LD H, r8", 8, 1, e => Load8Immediate(e, ref e.Memory.H));
			CpuInstructions[0x2E] = new Instruction("LD L, r8", 8, 1, e => Load8Immediate(e, ref e.Memory.L));
			CpuInstructions[0x3E] = new Instruction("LD A, r8", 8, 1, e => Load8Immediate(e, ref e.Memory.B));



			CpuInstructions[0x7F] = new Instruction("LD A, A", 4, e => Load8Register(e, ref e.Memory.A, e.Memory.A));
			CpuInstructions[0x78] = new Instruction("LD A, B", 4, e => Load8Register(e, ref e.Memory.A, e.Memory.B));
			CpuInstructions[0x79] = new Instruction("LD A, C", 4, e => Load8Register(e, ref e.Memory.A, e.Memory.C));
			CpuInstructions[0x7A] = new Instruction("LD A, D", 4, e => Load8Register(e, ref e.Memory.A, e.Memory.D));
			CpuInstructions[0x7B] = new Instruction("LD A, E", 4, e => Load8Register(e, ref e.Memory.A, e.Memory.E));
			CpuInstructions[0x7C] = new Instruction("LD A, H", 4, e => Load8Register(e, ref e.Memory.A, e.Memory.H));
			CpuInstructions[0x7D] = new Instruction("LD A, L", 4, e => Load8Register(e, ref e.Memory.A, e.Memory.L));
			CpuInstructions[0x7E] = new Instruction("LD A, (HL)", 8, e => Load8Register(e, ref e.Memory.A, e.Memory.HLAddress));
			
			CpuInstructions[0x40] = new Instruction("LD B, B", 4, e => Load8Register(e, ref e.Memory.B, e.Memory.B));
			CpuInstructions[0x41] = new Instruction("LD B, C", 4, e => Load8Register(e, ref e.Memory.B, e.Memory.C));
			CpuInstructions[0x42] = new Instruction("LD B, D", 4, e => Load8Register(e, ref e.Memory.B, e.Memory.D));
			CpuInstructions[0x43] = new Instruction("LD B, E", 4, e => Load8Register(e, ref e.Memory.B, e.Memory.E));
			CpuInstructions[0x44] = new Instruction("LD B, H", 4, e => Load8Register(e, ref e.Memory.B, e.Memory.H));
			CpuInstructions[0x45] = new Instruction("LD B, L", 4, e => Load8Register(e, ref e.Memory.B, e.Memory.L));
			CpuInstructions[0x46] = new Instruction("LD B, (HL)", 8, e => Load8Register(e, ref e.Memory.B, e.Memory.HLAddress));
			CpuInstructions[0x47] = new Instruction("LD B, A", 8, e => Load8Register(e, ref e.Memory.B, e.Memory.A));
			
			CpuInstructions[0x48] = new Instruction("LD C, B", 4, e => Load8Register(e, ref e.Memory.C, e.Memory.B));
			CpuInstructions[0x49] = new Instruction("LD C, C", 4, e => Load8Register(e, ref e.Memory.C, e.Memory.C));
			CpuInstructions[0x4A] = new Instruction("LD C, D", 4, e => Load8Register(e, ref e.Memory.C, e.Memory.D));
			CpuInstructions[0x4B] = new Instruction("LD C, E", 4, e => Load8Register(e, ref e.Memory.C, e.Memory.E));
			CpuInstructions[0x4C] = new Instruction("LD C, H", 4, e => Load8Register(e, ref e.Memory.C, e.Memory.H));
			CpuInstructions[0x4D] = new Instruction("LD C, L", 4, e => Load8Register(e, ref e.Memory.C, e.Memory.L));
			CpuInstructions[0x4E] = new Instruction("LD C, (HL)", 8, e => Load8Register(e, ref e.Memory.C, e.Memory.HLAddress));
			CpuInstructions[0x4F] = new Instruction("LD C, A", 8, e => Load8Register(e, ref e.Memory.C, e.Memory.A));

			CpuInstructions[0x50] = new Instruction("LD D, B", 4, e => Load8Register(e, ref e.Memory.D, e.Memory.B));
			CpuInstructions[0x51] = new Instruction("LD D, C", 4, e => Load8Register(e, ref e.Memory.D, e.Memory.C));
			CpuInstructions[0x52] = new Instruction("LD D, D", 4, e => Load8Register(e, ref e.Memory.D, e.Memory.D));
			CpuInstructions[0x53] = new Instruction("LD D, E", 4, e => Load8Register(e, ref e.Memory.D, e.Memory.E));
			CpuInstructions[0x54] = new Instruction("LD D, H", 4, e => Load8Register(e, ref e.Memory.D, e.Memory.H));
			CpuInstructions[0x55] = new Instruction("LD D, L", 4, e => Load8Register(e, ref e.Memory.D, e.Memory.L));
			CpuInstructions[0x56] = new Instruction("LD D, (HL)", 8, e => Load8Register(e, ref e.Memory.D, e.Memory.HLAddress));
			CpuInstructions[0x57] = new Instruction("LD D, A", 8, e => Load8Register(e, ref e.Memory.D, e.Memory.A));

			CpuInstructions[0x58] = new Instruction("LD E, B", 4, e => Load8Register(e, ref e.Memory.E, e.Memory.B));
			CpuInstructions[0x59] = new Instruction("LD E, C", 4, e => Load8Register(e, ref e.Memory.E, e.Memory.C));
			CpuInstructions[0x5A] = new Instruction("LD E, D", 4, e => Load8Register(e, ref e.Memory.E, e.Memory.D));
			CpuInstructions[0x5B] = new Instruction("LD E, E", 4, e => Load8Register(e, ref e.Memory.E, e.Memory.E));
			CpuInstructions[0x5C] = new Instruction("LD E, H", 4, e => Load8Register(e, ref e.Memory.E, e.Memory.H));
			CpuInstructions[0x5D] = new Instruction("LD E, L", 4, e => Load8Register(e, ref e.Memory.E, e.Memory.L));
			CpuInstructions[0x5E] = new Instruction("LD E, (HL)", 8, e => Load8Register(e, ref e.Memory.E, e.Memory.HLAddress));
			CpuInstructions[0x5F] = new Instruction("LD E, A", 8, e => Load8Register(e, ref e.Memory.E, e.Memory.A));

			CpuInstructions[0x60] = new Instruction("LD H, B", 4, e => Load8Register(e, ref e.Memory.H, e.Memory.B));
			CpuInstructions[0x61] = new Instruction("LD H, C", 4, e => Load8Register(e, ref e.Memory.H, e.Memory.C));
			CpuInstructions[0x62] = new Instruction("LD H, D", 4, e => Load8Register(e, ref e.Memory.H, e.Memory.D));
			CpuInstructions[0x63] = new Instruction("LD H, E", 4, e => Load8Register(e, ref e.Memory.H, e.Memory.E));
			CpuInstructions[0x64] = new Instruction("LD H, H", 4, e => Load8Register(e, ref e.Memory.H, e.Memory.H));
			CpuInstructions[0x65] = new Instruction("LD H, L", 4, e => Load8Register(e, ref e.Memory.H, e.Memory.L));
			CpuInstructions[0x66] = new Instruction("LD H, (HL)", 8, e => Load8Register(e, ref e.Memory.H, e.Memory.HLAddress));
			CpuInstructions[0x67] = new Instruction("LD H, A", 8, e => Load8Register(e, ref e.Memory.H, e.Memory.A));

			CpuInstructions[0x68] = new Instruction("LD L, B", 4, e => Load8Register(e, ref e.Memory.L, e.Memory.B));
			CpuInstructions[0x69] = new Instruction("LD L, C", 4, e => Load8Register(e, ref e.Memory.L, e.Memory.C));
			CpuInstructions[0x6A] = new Instruction("LD L, D", 4, e => Load8Register(e, ref e.Memory.L, e.Memory.D));
			CpuInstructions[0x6B] = new Instruction("LD L, E", 4, e => Load8Register(e, ref e.Memory.L, e.Memory.E));
			CpuInstructions[0x6C] = new Instruction("LD L, H", 4, e => Load8Register(e, ref e.Memory.L, e.Memory.H));
			CpuInstructions[0x6D] = new Instruction("LD L, L", 4, e => Load8Register(e, ref e.Memory.L, e.Memory.L));
			CpuInstructions[0x6E] = new Instruction("LD L, (HL)", 8, e => Load8Register(e, ref e.Memory.L, e.Memory.HLAddress));
			CpuInstructions[0x6F] = new Instruction("LD L, A", 8, e => Load8Register(e, ref e.Memory.L, e.Memory.A));

			CpuInstructions[0x70] = new Instruction("LD (HL), B", 8, e => Load8HLRef(e, e.Memory.B));
			CpuInstructions[0x71] = new Instruction("LD (HL), C", 8, e => Load8HLRef(e, e.Memory.C));
			CpuInstructions[0x72] = new Instruction("LD (HL), D", 8, e => Load8HLRef(e, e.Memory.D));
			CpuInstructions[0x73] = new Instruction("LD (HL), E", 8, e => Load8HLRef(e, e.Memory.E));
			CpuInstructions[0x74] = new Instruction("LD (HL), H", 8, e => Load8HLRef(e, e.Memory.H));
			CpuInstructions[0x75] = new Instruction("LD (HL), L", 8, e => Load8HLRef(e, e.Memory.L));
			CpuInstructions[0x77] = new Instruction("LD (HL), A", 8, e => Load8HLRef(e, e.Memory.A));
			CpuInstructions[0x36] = new Instruction("LD (HL), r8", 12, 1, Load8HLDirect);



			CpuInstructions[0x0A] = new Instruction("LD A, (BC)", 8, e => Read8FromAddress(e, ref e.Memory.A, e.Memory.BC));
			CpuInstructions[0x0A] = new Instruction("LD A, (DE)", 8, e => Read8FromAddress(e, ref e.Memory.A, e.Memory.DE));
			CpuInstructions[0x0A] = new Instruction("LD A, (nn)", 16, e => Read8FromDirectAddress(e, ref e.Memory.A));

			CpuInstructions[0x0A] = new Instruction("LD (BC), A", 8, e => Write8ToAddress(e, e.Memory.BC, e.Memory.A));
			CpuInstructions[0x0A] = new Instruction("LD (DE), A", 8, e => Write8ToAddress(e, e.Memory.DE, e.Memory.A));
			CpuInstructions[0x0A] = new Instruction("LD (nn), A", 16, e => Write8ToDirectAddress(e, e.Memory.A));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Load8Immediate(EmulatorInstance emulator, ref byte register)
		{
			register = emulator.Memory[emulator.Memory.PC + 1];

			emulator.Memory.PC += 2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Load8Register(EmulatorInstance emulator, ref byte register, byte source)
		{
			register = source;

			emulator.Memory.PC += 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Read8FromAddress(EmulatorInstance emulator, ref byte register, ushort address)
		{
			register = emulator.Memory[address];

			emulator.Memory.PC += 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Read8FromDirectAddress(EmulatorInstance emulator, ref byte register)
		{
			ushort address = (ushort)((emulator.Memory[emulator.Memory.PC] << 8) | emulator.Memory[emulator.Memory.PC]);

			register = emulator.Memory[address];

			emulator.Memory.PC += 3;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Write8ToAddress(EmulatorInstance emulator, ushort address, byte data)
		{
			emulator.Memory[address] = data;

			emulator.Memory.PC += 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Write8ToDirectAddress(EmulatorInstance emulator, byte value)
		{
			ushort address = (ushort)((emulator.Memory[emulator.Memory.PC] << 8) | emulator.Memory[emulator.Memory.PC]);

			emulator.Memory[address] = value;

			emulator.Memory.PC += 3;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Load8HLRef(EmulatorInstance emulator, byte source)
		{
			emulator.Memory.HLAddress = source;

			emulator.Memory.PC += 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Load8HLDirect(EmulatorInstance emulator)
		{
			emulator.Memory.HLAddress = emulator.Memory[emulator.Memory.PC + 1];

			emulator.Memory.PC += 2;
		}
	}
}
