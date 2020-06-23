using System;
using TinyZipper.Application.Core.Interfaces;

namespace TinyZipper.Application.Core.StatusUpdaters
{
    public class ConsoleStatusUpdateService : IStatusUpdateService
    {
        private readonly object _locker = new object();

        public void Info(string message)
        {
            lock (_locker)
            {
                WriteLineWithColor(message, ConsoleColor.Gray);
            }
        }

        public void Error(string message)
        {
            lock (_locker)
            {
                WriteLineWithColor(message, ConsoleColor.Red);
            }
        }

        private void WriteLineWithColor(string message, ConsoleColor color)
        {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = prevColor;
        }
    }
}