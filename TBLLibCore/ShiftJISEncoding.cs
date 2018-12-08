using System.Text;

namespace TBLLib {
    public class ShiftJISEncoding {
        private static Encoding shiftJIS;

        private ShiftJISEncoding() {
        }

        public static Encoding getInstance() {
            if (shiftJIS == null) {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                shiftJIS = Encoding.GetEncoding(932);
            }
            return shiftJIS;
        }
    }
}