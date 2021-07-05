using System.Runtime.CompilerServices;

namespace NEmu.Gameboy
{
	public enum CpuFlags : byte
	{
		None = 0x00,
		Carry = 0x10,
		HalfCarry = 0x20,
		Subtraction = 0x40,
		Zero = 0x80,
	}

	public class Memory
	{
		public byte A, B, C, D, E, H, L, F;
		public ushort PC, SP;

		public byte[] WRAM;

		public ushort BC
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (ushort)((B << 8) | C);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				B = (byte)(value >> 8);
				C = (byte)(value & 0xFF);
			}
		}

		public ushort DE
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (ushort)((D << 8) | E);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				D = (byte)(value >> 8);
				E = (byte)(value & 0xFF);
			}
		}

		public ushort HL
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (ushort)((H << 8) | L);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				H = (byte)(value >> 8);
				L = (byte)(value & 0xFF);
			}
		}

		public byte HLAddress
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this[HL];

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => this[HL] = value;
		}

		public void Initialize()
		{
			A = B = C = D = E = H = L = F = 0;
			PC = SP = 0;

			WRAM = new byte[8192];
		}

		public byte this[ushort address]
		{
			get
			{
				return 0;
			}
			set
			{

			}
		}

		public byte this[int address]
			=> this[(ushort)address];
	}
}
