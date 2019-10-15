using System;
using System.Runtime.InteropServices;

/// <summary>
/// EPX-18QC API library definitions
/// </summary>
public class EPX18QC
{

    //
    // Device status (Return codes)
    //
    public const int EPX18QC_OK = 0;
    public const int EPX18QC_INVALID_HANDLE = 1;
    public const int EPX18QC_DEVICE_NOT_FOUND = 2;
    public const int EPX18QC_DEVICE_NOT_OPENED = 3;
    public const int EPX18QC_OTHER_ERROR = 4;
    public const int EPX18QC_COMMUNICATION_ERROR = 5;
    public const int EPX18QC_INVALID_PARAMETER = 6;

    //
    // Counter CH
    //
    public const byte EPX18QC_CNT_CH0 = 0;
    public const byte EPX18QC_CNT_CH1 = 1;
    public const byte EPX18QC_CNT_CH2 = 2;
    public const byte EPX18QC_CNT_CH3 = 3;

    //
    // Counter Mode
    //
    public const byte EPX18QC_CNT_MODE_1PHASE = 0;
    public const byte EPX18QC_CNT_MODE_2PHASE = 1;

    //
    // Counter Direction
    //
    public const byte EPX18QC_CNT_DIR_UP = 0;
    public const byte EPX18QC_CNT_DIR_DOWN = 1;

    //
    // Counter Control
    //
    public const byte EPX18QC_CNT_DISABLE = 0;
    public const byte EPX18QC_CNT_ENABLE = 1;

    //
    // Counter Status
    //

    public const byte EPX18QC_CNT_OK = 0;
    public const byte EPX18QC_CNT_OVF = 1;
    public const byte EPX18QC_CNT_UDF = 2;

    //
    // Counter External Control
    //
    public const byte EPX18QC_CNT_EX_DISABLE = 0;
    public const byte EPX18QC_CNT_EX_ENABLE = 1;

    //
    // Counter Latch Flag
    //
    public const byte EPX18QC_CNT_LAT_FALSE = 0;
    public const byte EPX18QC_CNT_LAT_TRUE = 1;


    //
    // I/O Port
    //
    public const byte EPX18QC_PORT0 = 0;
    public const byte EPX18QC_PORT1 = 1;
    public const byte EPX18QC_PORT2 = 2;

    //
    // Port Direction
    //
    public const byte EPX18QC_PORT_DIR_INPUT = 0;
    public const byte EPX18QC_PORT_DIR_OUTPUT = 1;


    //
    // Device Functions
    //
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_GetNumberOfDevices(ref int Number);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_GetSerialNumber(int Index, ref int SerialNumber);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_Open(ref System.IntPtr Handle);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_OpenBySerialNumber(int SerialNumber, ref System.IntPtr Handle);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_Close(System.IntPtr Handle);

    //
    // Counter Functions
    //
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_SetCounterMode(System.IntPtr Handle, byte CH, byte Mode);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_GetCounterMode(System.IntPtr Handle, byte CH, ref byte Mode);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_SetCounterDirection(System.IntPtr Handle, byte CH, byte Direction);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_GetCounterDirection(System.IntPtr Handle, byte CH, ref byte Direction);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_SetCounterControl(System.IntPtr Handle, byte CH, byte Control);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_GetCounterControl(System.IntPtr Handle, byte CH, ref byte Control);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_ResetCounter(System.IntPtr Handle, byte CH);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_GetCounterValue(System.IntPtr Handle, byte CH, ref int Value);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_GetCounterStatus(System.IntPtr Handle, byte CH, ref byte Status);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_GetCounterExControl(System.IntPtr Handle, byte CH, ref byte Control);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_GetCounterLatchFlag(System.IntPtr Handle, byte CH, ref byte Flag);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_GetCounterLatchValue(System.IntPtr Handle, byte CH, ref int Value);

    //
    // I/O Port Functions
    //
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_SetPortDirection(System.IntPtr Handle, byte Port, byte Direction);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_GetPortDirection(System.IntPtr Handle, byte Port, ref byte Direction);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_OutputPort(System.IntPtr Handle, byte Port, byte Value);
    [DllImport("EPX18QC.dll")]
    public static extern int EPX18QC_InputPort(System.IntPtr Handle, byte Port, ref byte Value);

}
