
// <auto-generated>
// This file has been autogenerated by Kari.
// </auto-generated>

#pragma warning disable

namespace Kari.Generated
{
    public class HelloCommand : CommandBase
    {
        public HelloCommand() : base(_MinimumNumberOfArguments, _MaximumNumberOfArguments, "", _HelpMessage) {}
        public const string _HelpMessage = @"Usage: Hello i 
Argument/Option    Type    Description
--------------------------------------
i                  Int     
";        
        public const int _MinimumNumberOfArguments = 1;
        public const int _MaximumNumberOfArguments = 1;
        
        public override void Execute(CommandContext context)
        {
            // Take in all the positional arguments.
            var __i = context.ParseArgument(0, "i", Parsers.Int);
            context.EndParsing();
            // Make sure all required parameters have been given.
            if (context.HasErrors) return;
            // Call the function with correct arguments.
            Kari.Test.Program.Hello(i : __i);            
        }
        
    }

    public class FuncCommand : CommandBase
    {
        public override void Execute(CommandContext context) => Kari.Test.Program.Func(context);
        public FuncCommand() : base(0, -1, "") {}
    }

}
#pragma warning restore
