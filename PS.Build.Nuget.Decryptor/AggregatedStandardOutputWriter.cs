using System;
using System.IO;
using System.Text;

namespace PS.Build.Nuget.Decryptor
{
    class AggregatedStandardOutputWriter : TextWriter
    {
        private readonly StreamWriter _standardOutput;
        private readonly StringWriter _stringWriter;

        #region Constructors

        public AggregatedStandardOutputWriter()
        {
            _standardOutput = new StreamWriter(Console.OpenStandardOutput());
            _standardOutput.AutoFlush = true;
            _stringWriter = new StringWriter();
        }

        #endregion

        #region Properties

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        #endregion

        #region Override members

        public override void Write(char value)
        {
            _standardOutput.Write(value);
            _stringWriter.Write(value);
        }

        #endregion

        #region Members

        public string GetText()
        {
            return _stringWriter.ToString();
        }

        #endregion
    }
}