﻿using System;
using System.Collections.Generic;

namespace NEmu.Chip8
{
	public class Memory
	{
		public byte DelayTimer, SoundTimer;
		public byte[] Registers = new byte[16];
		public ushort I, PC;

		public Stack<ushort> CallStack = new Stack<ushort>(16);

		public byte[] RAM = new byte[4096];
		public byte[] VRAM = new byte[256];

		public void Initialize()
		{
			DelayTimer = SoundTimer = 0;
			I = 0;
			PC = 0x200;
			
			CallStack.Clear();

			Registers.ZeroArray();
			RAM.ZeroArray();
			VRAM.ZeroArray();

			byte[] fontData =
			{
				0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
				0x20, 0x60, 0x20, 0x20, 0x70, // 1
				0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
				0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
				0x90, 0x90, 0xF0, 0x10, 0x10, // 4
				0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
				0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
				0xF0, 0x10, 0x20, 0x40, 0x40, // 7
				0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
				0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
				0xF0, 0x90, 0xF0, 0x90, 0x90, // A
				0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
				0xF0, 0x80, 0x80, 0x80, 0xF0, // C
				0xE0, 0x90, 0x90, 0x90, 0xE0, // D
				0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
				0xF0, 0x80, 0xF0, 0x80, 0x80  // F
			};

			fontData.AsSpan().CopyTo(RAM.AsSpan(0x50));
		}

		public void LoadRom(byte[] rom)
		{
			rom.AsSpan().CopyTo(RAM.AsSpan(0x200));
		}
	}
}