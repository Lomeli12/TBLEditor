# TBLLib & TBLEditor

**TBLLib** is a library for compiling and decompiling name tables (.tbl files) used by the 3DS game *Persona Q2: New Cinema Labyrinth*.

**TBLEditor** is a command line tool using **TBLLib** to compile editable tables (.ebl files) into name tables, and vice versa. An .ebl file is simple text file and can be opened with any text editor.

## Versions

There are two version of **TBLLib** and **TBLEditor**: *Core* and *Win*.

* **Core** is built on .NET Core, so it can run on any OS supported by .NET Core (Windows, macOS, and Linux).
* **Win** is built on the .NET Framework v4.6.1 and thus can only run on Windows with .NET Framework v4.6.1 installed.

### Why are there two versions?

Simply because I wanted the tools to be accessible to everyone, but also knew that most Windows users don't have .NET Core installed.

## How to use TBLEditor

### To decompile a TBL file into a EBL file:

**Core:**
```bash
dotnet tbleditor.dll yourtblfile.tbl
```
**Win**
```bat
tbleditor yourtblfile.tbl
```

Output: `yourtblfile.ebl`.

---

### To compile a EBL file into a TBL file:

**Core:**
```bash
dotnet tbleditor.dll youreblfile.ebl
```
**Win**
```bat
tbleditor youreblfile.ebl
```

Output: `youreblfile.tbl`

### Optional Arguments

| Argument | Description |
| --------------- | ----------- |
| -h, --help | Display help and options. |
| -d, --decompile | Decompile input into a EBL file. Not required if input has .tbl extension. |
| -c, --compile | Compile input into a TBL file. Not required if input has .ebl extension. |
| -o, --out | Set the output path. By default, the output will be the filename + .tbl if compiling, and .ebl when decompiling. |
| --debug | Display debug information in logs |


---

Special thanks to **Rea** and her program **[SkillEdit](https://github.com/RagnarHomsar/SkillEdit)**. I was completely lost on how the tbl header worked until I stumbled across it.