using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoLeetcode
{
    public class KeyboardSimulator
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct INPUT
        {
            [FieldOffset(0)]
            public uint type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }

        const uint KEYEVENTF_KEYDOWN = 0x0000;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const ushort VK_LCONTROL = 0xA2;
        const ushort VK_A = 0x41;
        const ushort VK_V = 0x56;
        const ushort VK_X = 0x58;
        const ushort VK_DELETE = 0x2E;

        public static void SendCtrlA()
        {
            INPUT[] inputs = new INPUT[3];

            inputs[0].type = 1; // INPUT_KEYBOARD
            inputs[0].ki.wVk = VK_LCONTROL;
            inputs[0].ki.dwFlags = KEYEVENTF_KEYDOWN;

            inputs[1].type = 1; // INPUT_KEYBOARD
            inputs[1].ki.wVk = VK_A;
            inputs[1].ki.dwFlags = KEYEVENTF_KEYDOWN;

            inputs[2].type = 1; // INPUT_KEYBOARD
            inputs[2].ki.wVk = VK_A;
            inputs[2].ki.dwFlags = KEYEVENTF_KEYUP;

            SendInput(3, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void SendCtrlX()
        {
            INPUT[] inputs = new INPUT[3];

            inputs[0].type = 1; // INPUT_KEYBOARD
            inputs[0].ki.wVk = VK_LCONTROL;
            inputs[0].ki.dwFlags = KEYEVENTF_KEYDOWN;

            inputs[1].type = 1; // INPUT_KEYBOARD
            inputs[1].ki.wVk = VK_X;
            inputs[1].ki.dwFlags = KEYEVENTF_KEYDOWN;

            inputs[2].type = 1; // INPUT_KEYBOARD
            inputs[2].ki.wVk = VK_X;
            inputs[2].ki.dwFlags = KEYEVENTF_KEYUP;

            SendInput(3, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void SendCtrlV()
        {
            INPUT[] inputs = new INPUT[3];

            inputs[0].type = 1; // INPUT_KEYBOARD
            inputs[0].ki.wVk = VK_LCONTROL;
            inputs[0].ki.dwFlags = KEYEVENTF_KEYDOWN;

            inputs[1].type = 1; // INPUT_KEYBOARD
            inputs[1].ki.wVk = VK_V;
            inputs[1].ki.dwFlags = KEYEVENTF_KEYDOWN;

            inputs[2].type = 1; // INPUT_KEYBOARD
            inputs[2].ki.wVk = VK_V;
            inputs[2].ki.dwFlags = KEYEVENTF_KEYUP;

            SendInput(3, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void ReleaseCtrl()
        {
            INPUT[] inputs = new INPUT[1];

            inputs[0].type = 1; // INPUT_KEYBOARD
            inputs[0].ki.wVk = VK_LCONTROL;
            inputs[0].ki.dwFlags = KEYEVENTF_KEYUP;

            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void SendDelete()
        {
            INPUT[] inputs = new INPUT[2];

            inputs[0].type = 1; // INPUT_KEYBOARD
            inputs[0].ki.wVk = VK_DELETE;
            inputs[0].ki.dwFlags = KEYEVENTF_KEYDOWN;

            inputs[1].type = 1; // INPUT_KEYBOARD
            inputs[1].ki.wVk = VK_DELETE;
            inputs[1].ki.dwFlags = KEYEVENTF_KEYUP;

            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }

}
