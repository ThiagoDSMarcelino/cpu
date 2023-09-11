namespace Assembler;

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

public static class Assembler16Bit
{
    private readonly static IDictionary<string, ushort> opcode = new Dictionary<string, ushort>()
    {
        {"nop",       0b0000_0000_0000_0000},

        // ULA opcodes
        {"and",       0b1111_0000_0000_0000},
        {"sub",       0b1111_0001_0000_0000},
        {"mult",       0b1111_0010_0000_0000},
        {"div",       0b1111_0011_0000_0000},
        {"nand",      0b1111_0100_0000_0000},
        {"rsh",       0b1111_0101_0000_0000},
        {"xnor",      0b1111_0110_0000_0000},
        {"inc",       0b1111_0111_0000_0000},
        {"dec",       0b1111_1000_0000_0000},
        {"xor",       0b1111_1001_0000_0000},
        {"not",       0b1111_1010_0000_0000},
        {"nor",       0b1111_1011_0000_0000},
        {"lsh",       0b1111_1100_0000_0000},
        {"add",       0b1111_1101_0000_0000},
        {"ivt",       0b1111_1110_0000_0000},
        {"or",        0b1111_1111_0000_0000},

        // Jumps opcodes
        {"jump",      0b0001_0000_0000_0000},
        {"je",        0b0010_0000_0000_0000},
        {"jne",       0b0011_0000_0000_0000},
        {"jg",        0b0100_0000_0000_0000},
        {"jge",       0b0101_0000_0000_0000},
        {"jz",        0b0110_0000_0000_0000},

        // Mov opcodes
        {"movconst",  0b1000_0000_0000_0000},
        {"load",      0b1001_0001_0000_0000},
        {"store",     0b1001_0010_0000_0000},
        {"mov",       0b1001_0011_0000_0000},
        {"push",      0b1001_0100_0000_0000},
        {"pop",       0b1001_0101_0000_0000},

        // Compare opcodes
        {"cmp",       0b1100_0000_0000_0000},
        {"cmpconst",  0b1110_0000_0000_0000},

        // Functions opcodes
        {"call",      0b1111_0000_0000_0000},
        {"ret",       0b1111_0000_0000_0000}
    };

    public static void Convert(string path, bool toBinary = false)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException();

        IDictionary<string, int> labels = new Dictionary<string, int>();
        StreamReader reader = new(path);
        StreamWriter writer = new("memory");

        writer.WriteLine("v2.0 raw");

        var doc = reader
            .ReadToEnd()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split(';')[0]);

        int index = 0;
        foreach (var line in doc)
        {
            if (line.Contains(':'))
                labels.Add(line.Replace(":", ""), index);
            else
                index++;
        }

        reader = new StreamReader(path);
        while (!reader.EndOfStream)
        {
            string line = ProcessLine(reader.ReadLine().Trim(), labels, toBinary);

            if (line.Length == 0)
                continue;

            writer.Write($"{line} ");
        }

        reader.Close();
        writer.Close();
    }

    private static string ProcessLine(string line, IDictionary<string, int> labels, bool toBinary)
    {
        if (line.Contains(';') || line.Contains(':') || line.Length == 0)
            return "";

        int code = 0;

        string[] script = Regex.Replace(line.Replace(',', ' '), " {2,}", " ").Split(' ');
        ushort a = 0;
        bool
            isB = false,
            hasB = script.Length > 2;

        foreach (var command in script)
        {
            if (opcode.ContainsKey(command))
                code = opcode[command];
            else if (command.Contains('$') && !isB)
            {
                a = ushort.Parse(command.Replace("$", ""));
                isB = true;

                if (!hasB)
                    code |= a << 4;
            }
            else if (command.Contains('$') && isB)
            {
                code |= a << 4;
                code |= ushort.Parse(command.Replace("$", ""));
            }
            else if (isB)
            {
                code |= a << 8;
                code |= ushort.Parse(command);
            }
            else
                code |= labels[command];
        }

        if (toBinary)
            return code.ToString();

        return code.ToString("X4");
    }
}