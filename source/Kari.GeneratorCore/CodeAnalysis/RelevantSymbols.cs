﻿// Autogenerated from RelevantSymbols.tt

namespace Kari.GeneratorCore.CodeAnalysis
{
	using System;
	using Microsoft.CodeAnalysis;
	using Kari;

	public class RelevantSymbols
	{
		public readonly ITypeSymbol Short;
		public readonly ITypeSymbol Int;
		public readonly ITypeSymbol Long;
		public readonly ITypeSymbol Ushort;
		public readonly ITypeSymbol Uint;
		public readonly ITypeSymbol Ulong;
		public readonly ITypeSymbol Float;
		public readonly ITypeSymbol Double;
		public readonly ITypeSymbol Bool;
		public readonly ITypeSymbol Byte;
		public readonly ITypeSymbol Sbyte;
		public readonly ITypeSymbol Decimal;
		public readonly ITypeSymbol Char;
		public readonly ITypeSymbol String;
		public readonly ITypeSymbol Object;
		public readonly ITypeSymbol Void;
		
		public RelevantSymbols(Compilation compilation)
		{
			Short 	= compilation.GetSpecialType(SpecialType.System_Int16);
			Int 	= compilation.GetSpecialType(SpecialType.System_Int32);
			Long 	= compilation.GetSpecialType(SpecialType.System_Int64);
			Ushort 	= compilation.GetSpecialType(SpecialType.System_UInt16);
			Uint 	= compilation.GetSpecialType(SpecialType.System_UInt32);
			Ulong 	= compilation.GetSpecialType(SpecialType.System_UInt64);
			Float 	= compilation.GetSpecialType(SpecialType.System_Single);
			Double	= compilation.GetSpecialType(SpecialType.System_Double);
			Bool 	= compilation.GetSpecialType(SpecialType.System_Boolean);
			Byte	= compilation.GetSpecialType(SpecialType.System_Byte);
			Sbyte 	= compilation.GetSpecialType(SpecialType.System_SByte);
			Decimal = compilation.GetSpecialType(SpecialType.System_Decimal);
			Char 	= compilation.GetSpecialType(SpecialType.System_Char);
			String 	= compilation.GetSpecialType(SpecialType.System_String);
			Object 	= compilation.GetSpecialType(SpecialType.System_Object);
			Void 	= compilation.GetSpecialType(SpecialType.System_Void);
		}
	}
}