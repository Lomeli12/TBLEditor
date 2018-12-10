using System;
using System.Collections.Generic;
using System.IO;

namespace TBLLib {
    public class NameTable {
        public static readonly int NAME_MAX = 60;
        public static readonly string SPECIAL_OFFSET_MARK = "[use_special_header]";

        // For skillnametable, since that one likes to be special.
        private bool specialOffset;
        private readonly List<SJISString> nameEntries;
        private string path;
        private Logger log;

        public NameTable(string path, Logger log) {
            this.path = path;
            this.log = log;
            nameEntries = new List<SJISString>();
        }

        public bool writeTable() {
            log.debug("Writing to table to " + path);
            try {
                log.info("Table Size: " + nameEntries.Count);
                var fileBytes = new List<byte>();
                var sizeBytes = BitConverter.GetBytes(nameEntries.Count);
                fileBytes.Add(sizeBytes[0]);
                fileBytes.Add(sizeBytes[1]);
                log.debug($"Header: {sizeBytes[0]:X2} {sizeBytes[1]:X2}");

                var offset = 0;
                // For skillnametable, since that one likes to be special.
                if (specialOffset) {
                    log.debug("Using special header. Adding 0x00 to header...");
                    fileBytes.Add(0);
                    fileBytes.Add(0);
                }

                var entryBytes = new List<byte>();
                foreach (var entry in nameEntries) {
                    foreach (var b0 in entry.getCharBytes())
                        entryBytes.Add(b0);

                    offset += entry.getCharBytes().Length;
                    var size = BitConverter.GetBytes(offset);
                    fileBytes.Add(size[0]);
                    fileBytes.Add(size[1]);
                    log.debug($"Writing: Offset {offset:X2}, String: {entry.getString()}");
                }

                // For skillnametable, since that one likes to be special.
                if (specialOffset) {
                    log.debug("Special header in use. Removing last two offset bytes");
                    fileBytes.RemoveAt(fileBytes.Count - 1);
                    fileBytes.RemoveAt(fileBytes.Count - 1);
                }

                fileBytes.AddRange(entryBytes);
                File.WriteAllBytes(path, fileBytes.ToArray());
            } catch (Exception ex) {
                log.error("Error occured while saving name table: " + path, ex);
                return false;
            }
            log.info("Done!");
            return true;
        }

        private void readBytes(byte[] tableData) {
            var numOfEntries = (short) mergeBytesIntoShort(tableData, 0);
            log.info("Table Size: " + numOfEntries);
            log.debug($"Header: {tableData[0]:X2} {tableData[1]:X2}");
            // For some reason, skillnametable.tbl is slightly weird here and things fuck up
            // without this check for offsets.
            var arrOffset = 1;
            var prelimCheck = mergeBytesIntoShort(tableData, 2);
            if (prelimCheck == 0) {
                specialOffset = true;
                arrOffset = 0;
            }

            var offsets = new ushort[numOfEntries + arrOffset];
            offsets[0] = 0;            
            for (int i = 1; i < numOfEntries + arrOffset; i++) {
                if (arrOffset == 0) {
                    offsets[i] = mergeBytesIntoShort(tableData, (i + 1) * 2);
                } else
                    offsets[i] = mergeBytesIntoShort(tableData, i * 2);
            }

            var dataLoc = 2 * numOfEntries + 2;
            for (int i = 0; i < numOfEntries; i++) {
                var lastByte = 0x1;
                var stringPos = 0;
                var constructedData = new ushort[NAME_MAX];
                while (lastByte != 0x0) {
                    constructedData[stringPos] = mergeCharBytesIntoShort(tableData, dataLoc + offsets[i] + (stringPos * 2));
                    stringPos += 1;
                    var currentPos = dataLoc + offsets[i] + (stringPos * 2);

                    lastByte = currentPos < tableData.Length ? tableData[currentPos] : 0x0;
                }
                
                nameEntries.Add(new SJISString(constructedData));
                log.debug($"Reading: Offset {offsets[i]:X}; String: {nameEntries[nameEntries.Count - 1].getString()}");
            }
        }

        public void addString(string str) => nameEntries.Add(new SJISString(str));

        public bool remove(string str) => nameEntries.Remove(new SJISString(str));

        public void removeAt(int i) {
            if (i >= 0 && i < nameEntries.Count)
                nameEntries.RemoveAt(i);
        }

        public SJISString[] getEntryStrings() => nameEntries.ToArray();

        public void setSpecialOffset() {
            specialOffset = true;
        }

        public bool usesSpecialOffset() => specialOffset;
        
        public static NameTable fromFile(string path, Logger log) {
            if (!File.Exists(path))
                throw new FileNotFoundException();
            var tableData = File.ReadAllBytes(path);
            if (tableData != null && tableData.Length > 0) {
                var nameTable = new NameTable(path, log);
                nameTable.readBytes(tableData);
                return nameTable;
            }
            return null;
        }

        private static ushort mergeBytesIntoShort(byte[] table, int leftByteIndex) {
            return (ushort) ((table[leftByteIndex + 1] << 8) + table[leftByteIndex]);
        }

        private static ushort mergeCharBytesIntoShort(byte[] table, int leftByteIndex) {
            return (ushort) ((table[leftByteIndex] << 8) + table[leftByteIndex + 1]);
        }
    }
}