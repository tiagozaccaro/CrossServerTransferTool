﻿using System;
using System.Drawing;

namespace CrossServerTransferTool.SharedKernel.Extensions
{
    public static class ConsoleHelper
    {
        public static void DrawTextProgress(string message, int count, ConsoleColor numberColor = ConsoleColor.Black)
        {
            Console.CursorVisible = false;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.CursorLeft = 0;
            Console.Write(message);
            Console.Write(": ");

            Console.BackgroundColor = numberColor;
            Console.Write("[");
            Console.Write(count.ToString());
            Console.Write("]");         
        }

        public static void DrawTextProgressBar(int progress, int total)
        {
            Console.CursorVisible = false;
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i <= onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }
    }
}
