using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concussion
{
    class Program
    {
        static byte[] memory;

        static int dp = 0;
        static int ip = 0;

        public static readonly List<Optimization> fucks = new List<Optimization>()
        {
            // [-] and [+] optimizations
            new Optimization(new[] { new Operation(0, false, EOpType.LoopStart), new Operation(0, false, EOpType.Decr), new Operation(0, false, EOpType.LoopEnd) }, new[] { new Operation(0, false, EOpType.ZeroMem) }),
            new Optimization(new[] { new Operation(0, false, EOpType.LoopStart), new Operation(0, false, EOpType.Incr), new Operation(0, false, EOpType.LoopEnd) }, new[] { new Operation(0, false, EOpType.ZeroMem) }),

            // Copy loop optimization
            new Optimization(new[] {
                new Operation(0, false, EOpType.LoopStart),
                new Operation(0, false, EOpType.Decr),
                new Operation(0, false, EOpType.MoveRight),
                new Operation(0, false, EOpType.Incr),
                new Operation(0, false, EOpType.MoveRight),
                new Operation(0, false, EOpType.Incr),
                new Operation(2, true, EOpType.MoveLeft), // operation count must match exactly
                new Operation(0, false, EOpType.LoopEnd) }, new[] {new Operation(1, false, EOpType.Copy), new Operation(2, false, EOpType.Copy), new Operation(0, false, EOpType.ZeroMem) }),
        };

        static void Main(string[] args)
        {
#if DEBUG
            args = new string[] { "test.b" };
#endif
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: concussion.exe [file.bf]");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                return;
            }

            byte[] program;

            try
            {
                program = System.IO.File.ReadAllBytes(args[0]);
            }
            catch (Exception owo)
            {
                Console.WriteLine("Fuck. " + owo.Message);
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                return;
            }

            Stopwatch sw = new Stopwatch();

            memory = new byte[32768];

            Console.WriteLine("Initial Size: " + program.Length);
            sw.Start();
            Operation[] prog_opt = GetFuckedProgram(program);
            sw.Stop();
            Console.WriteLine("Compressed: " + prog_opt.Length);
            Console.WriteLine("Compression time: " + sw.ElapsedMilliseconds.ToString("N0") + "ms");
            sw.Reset();

            sw.Start();
            CurbstompProgram(prog_opt);
            sw.Stop();
            Console.WriteLine("Interpret time: " + sw.ElapsedMilliseconds.ToString("N0") + "ms");

#if DEBUG
            Console.ReadLine();
#endif
        }

        private static Operation[] GetFuckedProgram(byte[] program)
        {
            List<Operation> ops = new List<Operation>();

            // RLE compression
            for(int i = 0; i < program.Length; i++)
            {
                Operation tmp = new Operation(0, false, 0);
                int skip = 0;

                switch(program[i])
                {
                    /* > */
                    case 0x3E:
                        tmp.opType = EOpType.MoveRight;
                        for (int j = i; j < program.Length; j++)
                        {
                            if (program[j] == 0x3E)
                            {
                                tmp.count++;
                                skip++;
                            }
                            else if (program[j] == 0x3C)
                            {
                                tmp.count--;
                                skip++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;

                    /* < */
                    case 0x3C:
                        tmp.opType = EOpType.MoveLeft;
                        for (int j = i; j < program.Length; j++)
                        {
                            if (program[j] == 0x3C)
                            {
                                tmp.count++;
                                skip++;
                            }
                            else if (program[j] == 0x3E)
                            {
                                tmp.count--;
                                skip++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;

                    /* + */
                    case 0x2B:
                        tmp.opType = EOpType.Incr;
                        for (int j = i; j < program.Length; j++)
                        {
                            if (program[j] == 0x2B)
                            {
                                tmp.count++;
                                skip++;
                            }
                            else if (program[j] == 0x2D)
                            {
                                tmp.count--;
                                skip++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;

                    /* - */
                    case 0x2D:
                        tmp.opType = EOpType.Decr;
                        for(int j = i; j < program.Length; j++)
                        {
                            if(program[j] == 0x2D)
                            {
                                tmp.count++;
                                skip++;
                            }
                            else if(program[j] == 0x2B)
                            {
                                tmp.count--;
                                skip++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;

                    /* . */
                    case 0x2E:
                        tmp.opType = EOpType.Print;
                        break;
                    
                    /* , */
                    case 0x2C:
                        tmp.opType = EOpType.Read;
                        break;

                    case 0x5B:
                        tmp.opType = EOpType.LoopStart;
                        break;

                    case 0x5D:
                        tmp.opType = EOpType.LoopEnd;
                        break;

                    default:
                        continue;
                }

                if(skip > 1)
                    i += (skip - 1);

                ops.Add(tmp);
            }

            // Other optimizations
            for(int i = 0; i < ops.Count; i++)
            {
                foreach(Optimization fuck in fucks)
                {
                    if (i + fuck.Length > ops.Count)
                        continue;

                    bool match = false;

                    for(int vag = 0; vag < fuck.pattern.Count; vag++)
                    {
                        if(fuck.pattern[vag].flag)
                        {
                            if (ops[i + vag].opType == fuck.pattern[vag].opType && ops[i + vag].count == fuck.pattern[vag].count)
                            {
                                match = true;
                            }
                            else
                            {
                                match = false;
                                break;
                            }
                        }
                        else
                        {
                            if (ops[i + vag].opType == fuck.pattern[vag].opType)
                            {
                                match = true;
                            }
                            else
                            {
                                match = false;
                                break;
                            }
                        }
                    }

                    if (!match)
                        continue;

                    for(int creampie = 0; creampie < fuck.KeepAmount; creampie++)
                    {
                        ops.RemoveAt(i);
                    }

                    for(int piss = 0; piss < fuck.replacement.Count; piss++)
                    {
                        ops[i + piss] = new Operation(fuck.replacement[piss].count, false, fuck.replacement[piss].opType);
                    }
                    i = 0;
                }

            }



            // Bracket matching (should be last optimization)

            bool matched = false;

            Operation[] opArray = ops.ToArray();

            do
            {
                matched = MatchNextBracketPair(ref opArray);
            } while (matched);

            return opArray;
        }

        private static bool MatchNextBracketPair(ref Operation[] ops)
        {
            int depth = 0;

            bool foundFirst = false;
            int firstIndex = 0;

            bool foundLast = false;
            int lastIndex = 0;

            for (int i = 0; i < ops.Length; i++)
            {
                switch(ops[i].opType)
                {
                    case EOpType.LoopStart:
                        if (ops[i].flag)
                            continue;

                        depth++;

                        if(!foundFirst)
                        {
                            foundFirst = true;
                            firstIndex = i;
                        }
                        break;

                    case EOpType.LoopEnd:
                        if (ops[i].flag)
                            continue;

                        depth--;

                        if(depth == 0)
                        {
                            foundLast = true;
                            lastIndex = i;
                        }
                        break;
                }

                if (foundFirst && foundLast)
                {
                    ops[firstIndex] = new Operation(lastIndex, true, EOpType.LoopStart);
                    ops[lastIndex] = new Operation(firstIndex, true, EOpType.LoopEnd);
                    break;
                }
            }

            return foundFirst && foundLast;
        }


        private static unsafe void CurbstompProgram(Operation[] prog)
        {
            int prog_length = prog.Length;
            long cycles = 0;
            while(ip < prog_length)
            {
                cycles++;
                Operation instr = prog[ip];

                switch(instr.opType)
                {
                    /* > */
                    case EOpType.MoveRight:
                        dp += instr.count;
                        break;

                    /* < */
                    case EOpType.MoveLeft:
                        dp -= instr.count;
                        break;

                    /* + */
                    case EOpType.Incr:
                        memory[dp] += (byte)instr.count;
                        break;

                    /* - */
                    case EOpType.Decr:
                        memory[dp] -= (byte)instr.count;
                        break;

                    /* . */
                    case EOpType.Print:
                        Console.Write((char)memory[dp]);
                        break;

                    case EOpType.Read:
                        memory[dp] = (byte)Console.Read();
                        break;

                    /* [ */
                    case EOpType.LoopStart:
                        if(memory[dp] == 0x00)
                        {
                            // jump to end bracket pos
                            ip = instr.count;

                            // break out so that the parent loop automatically increments past the end loop instruction
                            break;
                        }
                        else
                        {
                            // break out to continue code in the loop
                            break;
                        }

                    /* ] */
                    case EOpType.LoopEnd:
                        // jump to start of loop
                        if(memory[dp] != 0)
                            ip = instr.count;
                        break;

                    case EOpType.ZeroMem:
                        memory[dp] = 0;
                        break;

                    case EOpType.Copy:
                        memory[dp + instr.count] += memory[dp];
                        break;
                }

                ip++;
            }

#if DEBUG
            Console.WriteLine("Cycles: " + cycles.ToString("N0"));
#endif
        }
    }
}