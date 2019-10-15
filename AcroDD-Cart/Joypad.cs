using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcroDD_Cart
{
    class JoyPad
    {
        protected IntPtr ptr;
        public JOYINFOEX JoyInfoEx;

        public JoyPad()
        {
            JoyInfoEx = new JOYINFOEX();
            JoyInfoEx.dwSize = (uint)Marshal.SizeOf(typeof(JOYINFOEX));
            JoyInfoEx.dwFlags = JOY_RETURNALL;

            ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(JOYINFOEX)));

            Marshal.StructureToPtr(JoyInfoEx, ptr, false);
        }

        ~JoyPad()
        {
            Marshal.FreeCoTaskMem(ptr);
        }

        public JOYERR GetPosEx(uint JoyID)
        {
            JOYERR err = joyGetPosEx(JoyID, ptr);

            JoyInfoEx = (JOYINFOEX)Marshal.PtrToStructure(ptr, JoyInfoEx.GetType());

            return (err);
        }

        public uint[] GetJoypads()
        {
            List<uint> pads = new List<uint>();
            uint end = joyGetNumDevs();

            for (uint i = 0; i < end; i++)
            {
                if (GetPosEx(i) != JOYERR.NOERROR)
                { continue; }
                pads.Add(i);
            }

            return (pads.ToArray());
        }

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern uint joyGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern JOYERR joyGetPosEx(uint uJoyID, IntPtr pji);

        [StructLayout(LayoutKind.Sequential)]
        public struct JOYINFOEX
        {
            public UInt32 dwSize;                /* size of structure */
            public UInt32 dwFlags;               /* flags to indicate what to return */
            public UInt32 dwXpos;                /* x position */
            public UInt32 dwYpos;                /* y position */
            public UInt32 dwZpos;                /* z position */
            public UInt32 dwRpos;                /* rudder/4th axis position */
            public UInt32 dwUpos;                /* 5th axis position */
            public UInt32 dwVpos;                /* 6th axis position */
            public UInt32 dwButtons;             /* button states */
            public UInt32 dwButtonNumber;        /* current button number pressed */
            public UInt32 dwPOV;                 /* point of view state */
            public UInt32 dwReserved1;           /* reserved for communication between winmm & driver */
            public UInt32 dwReserved2;           /* reserved for future expansion */
        }

        /* joystick error return values */
        public enum JOYERR
        {
            NOERROR = (0),               /* no error */
            PARMS = (160 + 5),           /* bad parameters */
            NOCANDO = (160 + 6),         /* request not completed */
            UNPLUGGED = (160 + 7),       /* joystick is unplugged */
        }


        /* public constants used with JOYINFO and JOYINFOEX structures and MM_JOY* messages */
        public const UInt32 JOY_BUTTON1 = 0x0001;
        public const UInt32 JOY_BUTTON2 = 0x0002;
        public const UInt32 JOY_BUTTON3 = 0x0004;
        public const UInt32 JOY_BUTTON4 = 0x0008;
        public const UInt32 JOY_BUTTON1CHG = 0x0100;
        public const UInt32 JOY_BUTTON2CHG = 0x0200;
        public const UInt32 JOY_BUTTON3CHG = 0x0400;
        public const UInt32 JOY_BUTTON4CHG = 0x0800;

        /* public constants used with JOYINFOEX */
        public const UInt32 JOY_BUTTON5 = 0x00000010;
        public const UInt32 JOY_BUTTON6 = 0x00000020;
        public const UInt32 JOY_BUTTON7 = 0x00000040;
        public const UInt32 JOY_BUTTON8 = 0x00000080;
        public const UInt32 JOY_BUTTON9 = 0x00000100;
        public const UInt32 JOY_BUTTON10 = 0x00000200;
        public const UInt32 JOY_BUTTON11 = 0x00000400;
        public const UInt32 JOY_BUTTON12 = 0x00000800;
        public const UInt32 JOY_BUTTON13 = 0x00001000;
        public const UInt32 JOY_BUTTON14 = 0x00002000;
        public const UInt32 JOY_BUTTON15 = 0x00004000;
        public const UInt32 JOY_BUTTON16 = 0x00008000;
        public const UInt32 JOY_BUTTON17 = 0x00010000;
        public const UInt32 JOY_BUTTON18 = 0x00020000;
        public const UInt32 JOY_BUTTON19 = 0x00040000;
        public const UInt32 JOY_BUTTON20 = 0x00080000;
        public const UInt32 JOY_BUTTON21 = 0x00100000;
        public const UInt32 JOY_BUTTON22 = 0x00200000;
        public const UInt32 JOY_BUTTON23 = 0x00400000;
        public const UInt32 JOY_BUTTON24 = 0x00800000;
        public const UInt32 JOY_BUTTON25 = 0x01000000;
        public const UInt32 JOY_BUTTON26 = 0x02000000;
        public const UInt32 JOY_BUTTON27 = 0x04000000;
        public const UInt32 JOY_BUTTON28 = 0x08000000;
        public const UInt32 JOY_BUTTON29 = 0x10000000;
        public const UInt32 JOY_BUTTON30 = 0x20000000;
        public const UInt32 JOY_BUTTON31 = 0x40000000;
        public const UInt32 JOY_BUTTON32 = 0x80000000;

        /* public constants used with JOYINFOEX structure */
        public const UInt32 JOY_POVCENTERED = 65535;
        public const UInt32 JOY_POVFORWARD = 0;
        public const UInt32 JOY_POVRIGHT = 9000;
        public const UInt32 JOY_POVBACKWARD = 18000;
        public const UInt32 JOY_POVLEFT = 27000;

        public const UInt32 JOY_RETURNX = 0x00000001;
        public const UInt32 JOY_RETURNY = 0x00000002;
        public const UInt32 JOY_RETURNZ = 0x00000004;
        public const UInt32 JOY_RETURNR = 0x00000008;
        public const UInt32 JOY_RETURNU = 0x00000010;     /* axis 5 */
        public const UInt32 JOY_RETURNV = 0x00000020;     /* axis 6 */
        public const UInt32 JOY_RETURNPOV = 0x00000040;
        public const UInt32 JOY_RETURNBUTTONS = 0x00000080;
        public const UInt32 JOY_RETURNRAWDATA = 0x00000100;
        public const UInt32 JOY_RETURNPOVCTS = 0x00000200;
        public const UInt32 JOY_RETURNCENTERED = 0x00000400;
        public const UInt32 JOY_USEDEADZONE = 0x00000800;
        public const UInt32 JOY_RETURNALL =
            (JOY_RETURNX | JOY_RETURNY | JOY_RETURNZ |
            JOY_RETURNR | JOY_RETURNU | JOY_RETURNV |
            JOY_RETURNPOV | JOY_RETURNBUTTONS);
        public const UInt32 JOY_CAL_READALWAYS = 0x00010000;
        public const UInt32 JOY_CAL_READXYONLY = 0x00020000;
        public const UInt32 JOY_CAL_READ3 = 0x00040000;
        public const UInt32 JOY_CAL_READ4 = 0x00080000;
        public const UInt32 JOY_CAL_READXONLY = 0x00100000;
        public const UInt32 JOY_CAL_READYONLY = 0x00200000;
        public const UInt32 JOY_CAL_READ5 = 0x00400000;
        public const UInt32 JOY_CAL_READ6 = 0x00800000;
        public const UInt32 JOY_CAL_READZONLY = 0x01000000;
        public const UInt32 JOY_CAL_READRONLY = 0x02000000;
        public const UInt32 JOY_CAL_READUONLY = 0x04000000;
        public const UInt32 JOY_CAL_READVONLY = 0x08000000;

        /* joystick ID public constants */
        public const UInt32 JOYSTICKID1 = 0;
        public const UInt32 JOYSTICKID2 = 1;

        /* joystick driver capabilites */
        public const UInt32 JOYCAPS_HASZ = 0x0001;
        public const UInt32 JOYCAPS_HASR = 0x0002;
        public const UInt32 JOYCAPS_HASU = 0x0004;
        public const UInt32 JOYCAPS_HASV = 0x0008;
        public const UInt32 JOYCAPS_HASPOV = 0x0010;
        public const UInt32 JOYCAPS_POV4DIR = 0x0020;
        public const UInt32 JOYCAPS_POVCTS = 0x0040;
    }
}
