﻿/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2021  Giorgio Bianchini
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Runtime.InteropServices;

namespace TreeViewer
{
    internal static class ConsoleWrapperUI
    {
        public static bool IsConsoleEnabled { get; private set; } = false;

        enum StandardHandle : uint
        {
            Input = unchecked((uint)-10),
            Output = unchecked((uint)-11),
            Error = unchecked((uint)-12)
        }

        enum FileType : uint
        {
            Unknown = 0x0000,
            Disk = 0x0001,
            Char = 0x0002,
            Pipe = 0x0003
        }


        public static void EnableConsole()
        {
            if (!IsConsoleEnabled)
            {
                // Apparently, only needed on Windows
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    [DllImport("kernel32.dll", SetLastError = true)]
                    static extern bool AttachConsole(int dwProcessId);
                    [DllImport("kernel32.dll", SetLastError = true)]
                    static extern IntPtr GetStdHandle(StandardHandle nStdHandle);
                    [DllImport("kernel32.dll", SetLastError = true)]
                    static extern bool SetStdHandle(StandardHandle nStdHandle, IntPtr handle);
                    [DllImport("kernel32.dll", SetLastError = true)]
                    static extern FileType GetFileType(IntPtr handle);

                    static bool IsRedirected(IntPtr handle)
                    {
                        FileType fileType = GetFileType(handle);

                        return (fileType == FileType.Disk) || (fileType == FileType.Pipe);
                    }

                    if (IsRedirected(GetStdHandle(StandardHandle.Output)))
                    {
                        var initialiseOut = Console.Out;
                    }

                    bool errorRedirected = IsRedirected(GetStdHandle(StandardHandle.Error));
                    if (errorRedirected)
                    {
                        var initialiseError = Console.Error;
                    }

                    AttachConsole(-1);

                    if (!errorRedirected)
                    {
                        SetStdHandle(StandardHandle.Error, GetStdHandle(StandardHandle.Output));
                    }
                }

                IsConsoleEnabled = true;
            }
        }

        public static void WriteLine()
        {
            EnableConsole();
            Console.WriteLine();
        }

        public static void WriteLine(string value)
        {
            EnableConsole();
            Console.WriteLine(value);
        }

        public static void WriteLine(string format, params object[] arg)
        {
            EnableConsole();
            Console.WriteLine(format, arg);
        }
    }
}
