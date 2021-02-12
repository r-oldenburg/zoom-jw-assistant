using System;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace ZoomJWAssistant.Core
{
    public class TextBoxOutputter : TextWriter
    {
        TextBox textBox = null;
        StringBuilder buffer = new StringBuilder();

        public TextBoxOutputter(TextBox output)
        {
            textBox = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            buffer.Append(value);

            if (value == '\n')
            {
                var newText = buffer.ToString();
                textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.AppendText(DateTime.Now.ToString("yyyy-dd-MM HH:mm:ss") + " - ");
                    textBox.AppendText(newText);
                    textBox.ScrollToEnd();
                }));
                buffer.Clear();
            }
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
