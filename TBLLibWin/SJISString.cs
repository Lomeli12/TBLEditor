namespace TBLLib {
    public class SJISString {
        public static readonly int NAME_MAX = 60;
        private readonly byte[] characterBytes;
        private readonly string convertedString;

        public SJISString(ushort[] input) {
            var trueSize = 0;
            var tempBytes = new byte[NameTable.NAME_MAX * 2];
            for (int i = 0; i < NameTable.NAME_MAX; i++) {
                var upperByte = input[i] >> 8;
                var lowerByte = input[i] & 0xFF;
                tempBytes[i * 2] = (byte) upperByte;

                trueSize++;
                // when we hit a null byte, stop
                if (upperByte == 0x00)
                    break;

                trueSize++;
                tempBytes[(i * 2) + 1] = (byte) lowerByte;
            }
            characterBytes = new byte[trueSize];
            for (int i = 0; i < characterBytes.Length; i++)
                characterBytes[i] = tempBytes[i];
            convertedString = toShiftJISString();
        }

        public SJISString(string str) {
            convertedString = str;
            var tempArry = ShiftJISEncoding.getInstance().GetBytes(str);
            var padding = tempArry.Length % 2 != 0 ? 2 : 1; 
            characterBytes = new byte[tempArry.Length + padding];
            for (int i = 0; i < characterBytes.Length; i++) {
                if (i < tempArry.Length)
                    characterBytes[i] = tempArry[i];
                else {
                    if (padding == 2 && i == characterBytes.Length - 2)
                        characterBytes[i] = 32;
                    else characterBytes[i] = 0;
                }
            }
        }

        public string getString() => convertedString;

        public byte[] getCharBytes() => characterBytes;
        
        private string toShiftJISString() {
            var stringLength = (characterBytes.Length - 1) / 2;
            var stringChars = "";
            for (int i = 0; i < stringLength; i++) {
                byte b0 = 0;
                byte b1 = 0;
                var index = i * 2;
                if (index < characterBytes.Length)
                    b0 = characterBytes[index];
                index++;
                if (index < characterBytes.Length)
                    b1 = characterBytes[index];
                if (b0 == 0x0 || b1 == 0x0)
                    break;
                var str = ShiftJISEncoding.getInstance().GetString(new byte[]{b0, b1});

                stringChars += str;
            }

            return stringChars;
        }
    }
}