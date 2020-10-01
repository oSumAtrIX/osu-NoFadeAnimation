using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System.Collections.Generic;

using static System.Reflection.Assembly;
using static System.Security.Cryptography.MD5;
using static dnlib.DotNet.Emit.OpCodes;
using static System.IO.Path;
using static System.IO.File;
using static System.Console;



namespace osu_NoFadeAnimation
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 0x1)
            {
                WriteLine($"Drag & drop osu!.exe onto {GetExecutingAssembly().GetName().Name}.exe"); ReadLine();
                return;
            }

            string path = args[0x0];
            var module = ModuleDefMD.Load(path);

            //RIDs will change for builds other than build 20200918.3.
            IList<Instruction>
                entryMethodInstr = module.ResolveMethod(8910).Body.Instructions,
                hitArmMethodInstr = module.ResolveMethod(2729).Body.Instructions,
                hashCheckMethodInstr = module.ResolveMethod(8719).Body.Instructions;

            entryMethodInstr[7].OpCode = Brfalse_S;
            
            //Offsets might change for builds other than build 20200918.3.
            var offsets = new[] { 18, 39, 61 };
            for (int i = 0; i < offsets.Length; i++) hitArmMethodInstr[offsets[i]].OpCode = Nop;

            hashCheckMethodInstr[376].OpCode = Nop;
            hashCheckMethodInstr[377] = Ldstr.ToInstruction(string.Join("", Create().ComputeHash(ReadAllBytes(path)).Select(s => s.ToString("x2"))));

            module.Write(Combine(GetDirectoryName(path), $"{GetFileNameWithoutExtension(path)}-patched{GetExtension(path)}"), new ModuleWriterOptions(module) { Logger = new DummyLogger(null)});

        }
    }
}
