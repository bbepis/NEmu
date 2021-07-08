using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NEmu.Chip8
{
	public class RecompiledBlock
	{
		public ushort Position;
		public ushort Size;

		public DynamicMethod DynamicMethod;
		public Action<EmulatorInstance> CompiledMethod;

		public RecompiledBlock(ushort position, ushort size)
		{
			Position = position;
			Size = size;
			DynamicMethod = new DynamicMethod($"$CHIP8_{position}:{size}",
				typeof(void),
				new []{ typeof(EmulatorInstance) });
		}

		public void CompileBlock()
		{
			if (CompiledMethod != null)
				return;

			CompiledMethod = (Action<EmulatorInstance>)DynamicMethod.CreateDelegate(typeof(Action<EmulatorInstance>));
		}
	}

	public class Recompiler
	{
		public Dictionary<ushort, RecompiledBlock> RecompiledBlocks { get; } =
			new Dictionary<ushort, RecompiledBlock>();

		private static readonly Dictionary<InstructionDefinition, FieldInfo> FieldDictionary
			= new Dictionary<InstructionDefinition, FieldInfo>();

		static Recompiler()
		{
			foreach (var field in typeof(InstructionDefinition).GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				if (field.FieldType != typeof(InstructionDefinition))
					continue;

				FieldDictionary[(InstructionDefinition)field.GetValue(null)] = field;
			}
		}
		
		private static PropertyInfo instructionExecutePropertyInfo =
			typeof(InstructionDefinition).GetProperty(nameof(InstructionDefinition.Execute));

		private static MethodInfo invokeMethodInfo =
			typeof(Action<EmulatorInstance, ushort>).GetMethod(nameof(Action<EmulatorInstance, ushort>.Invoke));

		private static PropertyInfo memoryPropertyInfo =
			typeof(EmulatorInstance).GetProperty(nameof(EmulatorInstance.Memory));

		private static FieldInfo pcFieldInfo =
			typeof(Memory).GetField(nameof(Memory.PC));

		public RecompiledBlock GetOrCreateBlock(EmulatorInstance emulator, ushort pc, ushort size = 50)
		{
			if (RecompiledBlocks.TryGetValue(pc, out var block))
				return block;

			block = new RecompiledBlock(pc, size);

			var il = block.DynamicMethod.GetILGenerator();

			for (int i = 0; i < size; i += 2)
			{
				ushort rawInstruction;
				InstructionDefinition instruction;

				try
				{
					int localPc = emulator.Memory.PC + i;
					rawInstruction =
						(ushort)((emulator.Memory.RAM[localPc] << 8) | emulator.Memory.RAM[localPc + 1]);
					instruction = emulator.DecodeInstruction(rawInstruction);
				}
				catch
				{
					break;
				}

				ushort argument = (ushort)(rawInstruction & ((1 << instruction.ArgumentSize) - 1));

				il.Emit(OpCodes.Ldsfld, FieldDictionary[instruction]);
				il.Emit(OpCodes.Callvirt, instructionExecutePropertyInfo.GetMethod);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldc_I4, argument);
				il.Emit(OpCodes.Callvirt, invokeMethodInfo);

				if (instruction.AlternatePCHandling
				    && i != size - 2) // not the last instruction
				{
					// the instructions themselves could optimized into IL but it would be implementation specific
					
					var continueLabel = il.DefineLabel();

					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Callvirt, memoryPropertyInfo.GetMethod);
					il.Emit(OpCodes.Ldfld, pcFieldInfo);

					il.Emit(OpCodes.Ldc_I4, pc + i + 2);
					il.Emit(OpCodes.Beq_S, continueLabel);

					il.Emit(OpCodes.Ret);
					il.MarkLabel(continueLabel);
				}
				else
				{
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Callvirt, memoryPropertyInfo.GetMethod);
					il.Emit(OpCodes.Ldc_I4, pc + i + 2);
					il.Emit(OpCodes.Stfld, pcFieldInfo);
				}
			}

			il.Emit(OpCodes.Ret);

			block.CompileBlock();

			RecompiledBlocks[pc] = block;

			return block;
		}
	}
}
