using System;
using System.IO;
using System.Text;

namespace TBLLib {
    public class Logger {
        private static string LOG_FOLDER = Environment.CurrentDirectory + "/logs";
        private StreamWriter writer;
        private string name;
        private bool debugMode;

        public Logger(string name) {
            this.name = name;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            if (!Directory.Exists(LOG_FOLDER))
                Directory.CreateDirectory(LOG_FOLDER);
            writer = new StreamWriter(LOG_FOLDER + "/" + name + "_" + timestamp + ".log");
        }

        private void log(LogType type, object msg) {
            if (type == LogType.DEBUG && !debugMode) 
                return;
            var builder = new StringBuilder();
            builder.Append("[" + getTimestamp() + "]");
            builder.Append("[" + name + "]");
            builder.Append("[" + type + "]: ");
            builder.Append(msg ?? "Null");
            var logMsg = builder.ToString();
            Console.WriteLine(logMsg);
            writer.WriteLine(logMsg);
        }

        public void info(object msg) => log(LogType.INFO, msg);
        public void debug(object msg) => log(LogType.DEBUG, msg);
        public void warn(object msg) => log(LogType.WARN, msg);
        public void error(object msg) => log(LogType.ERROR, msg);

        public void error(object msg, Exception ex) {
            error(msg + "\n" + ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace);
        }

        public void close() {
            writer.Flush();
            writer.Dispose();
        }

        public void setDebugMode(bool enable) {
            debugMode = enable;
        }

        private static string getTimestamp() {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }
}