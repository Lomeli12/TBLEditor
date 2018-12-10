using System;
using System.Collections.Generic;
using System.IO;
using TBLLib;

namespace TBLEditor {
    class Program {
        public static Logger log = new Logger("TBLEditor");

        private static string path;
        private static string outPath;
        private static TBLMode mode = TBLMode.NONE;

        static void Main(string[] args) {
            parseArgs(args);

            if (string.IsNullOrEmpty(path)) {
                displayHelp();
                log.close();
                return;
            }
            
            var fileInfo = new FileInfo(path);
            path = fileInfo.FullName;

            log.info("Starting TBLEditor");

            if (!File.Exists(path)) {
                log.error("Must be valid file!");
                log.close();
                return;
            }

            if (mode == TBLMode.NONE) {
                var ext = Path.GetExtension(path);
                if (ext.Equals(".ebl", StringComparison.CurrentCultureIgnoreCase)) mode = TBLMode.COMPILE;
                else if (ext.Equals(".tbl", StringComparison.CurrentCultureIgnoreCase)) mode = TBLMode.DECOMPILE;
            }

            if (string.IsNullOrEmpty(outPath)) {
                outPath = Path.GetFileNameWithoutExtension(path);
                if (mode == TBLMode.COMPILE) outPath += ".tbl";
                else outPath += ".ebl";
            }
            
            fileInfo = new FileInfo(outPath);
            outPath = fileInfo.FullName;
            
            switch (mode) {
                case TBLMode.COMPILE:
                    var lines = File.ReadLines(path);
                    log.info("Reading " + path + " to compile...");
                    var table = new NameTable(outPath, log);
                    int i = 0;
                    foreach (var line in lines) {
                        if (i == 0 && line.Equals("[use_special_header]", StringComparison.CurrentCultureIgnoreCase)) {
                            i++;
                            table.setSpecialOffset();
                            continue;
                        }

                        table.addString(line ?? "");
                        i++;
                    }

                    table.writeTable();
                    break;
                case TBLMode.DECOMPILE:
                    log.info("Reading " + path + " to decompile...");
                    var nameTable = NameTable.fromFile(path, log);
                    if (nameTable != null) {
                        var entryNames = nameTable.getEntryStrings();
                        if (entryNames != null && entryNames.Length > 0) {
                            var names = new List<string>();
                            if (nameTable.usesSpecialOffset())
                                names.Add(NameTable.SPECIAL_OFFSET_MARK);
                            foreach (var name in entryNames) {
                                if (name != null)
                                    names.Add(name.getString());
                            }

                            if (File.Exists(outPath))
                                File.Delete(outPath);
                            File.WriteAllLines(outPath, names);
                            log.info("Done!");
                        }
                    }

                    break;
                case TBLMode.NONE:
                    log.warn("Could not determine mode based on extension. Use either --decompile or --compile.");
                    break;
            }

            log.close();
        }

        private static void parseArgs(String[] args) {
            if (args == null || args.Length < 0)
                return;
            for (int i = 0; i < args.Length; i++) {
                var arg = args[i];
                if (arg.Equals("-h", StringComparison.CurrentCultureIgnoreCase) ||
                    arg.Equals("--help", StringComparison.CurrentCultureIgnoreCase)) {
                    displayHelp();
                    log.close();
                    Environment.Exit(0);
                } else if (arg.Equals("--debug", StringComparison.CurrentCultureIgnoreCase))
                    log.setDebugMode(true);
                else if (arg.Equals("-d", StringComparison.CurrentCultureIgnoreCase) ||
                         arg.Equals("--Decompile", StringComparison.CurrentCultureIgnoreCase)) {
                    mode = TBLMode.DECOMPILE;
                } else if (arg.Equals("-c", StringComparison.CurrentCultureIgnoreCase) ||
                           arg.Equals("--Compile", StringComparison.CurrentCultureIgnoreCase)) {
                    mode = TBLMode.COMPILE;
                } else if (arg.Equals("-o", StringComparison.CurrentCultureIgnoreCase) ||
                           arg.Equals("--out", StringComparison.CurrentCultureIgnoreCase)) {
                    if (i + 1 < args.Length) {
                        outPath = args[i + 1];
                        var valid = !string.IsNullOrEmpty(outPath) &&
                                    Path.GetFullPath(outPath).IndexOfAny(Path.GetInvalidPathChars()) < 0 &&
                                    Path.GetFileName(outPath).IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
                        if (!valid) {
                            log.error("Must give a valid file name for the output.");
                            log.close();
                            Environment.Exit(0);
                        }

                        i++;
                    } else {
                        log.error("Must give a file name for the output.");
                        log.close();
                        Environment.Exit(0);
                    }
                } else {
                    path = arg;
                }
            }
        }

        static readonly string USEAGE_FORMAT = "{0, 14}\n      {1}";
        static readonly string USEAGE_FORMAT_TWO = "{0, 5}, {1, 2}\n      {2}";

        private static void displayHelp() {
            Console.WriteLine("usage: dotnet tbleditor.dll <input file> [-h | --help] [--debug] " +
                              "[-o | --out <output file>] [-c | --compile] [-d | --decompile]\n");
            Console.WriteLine(USEAGE_FORMAT_TWO, "-h", "--help", "Display help and options");
            Console.WriteLine(USEAGE_FORMAT_TWO, "-d", "--decompile", "Decompile input into a EBL file.");
            Console.WriteLine(USEAGE_FORMAT_TWO, "-c", "--compile", "Compile input into a TBL file.");
            Console.WriteLine(USEAGE_FORMAT_TWO, "-o", "--out",
                "Set the output path. By default, the output will be the filename + .tbl if compiling, and .ebl when decompiling.");
            Console.WriteLine(USEAGE_FORMAT, "--debug", "Display debug information in logs.");
        }
    }
}