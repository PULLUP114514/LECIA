/// <summary>
/// 简单粗暴的全局变量
/// </summary>
public class GlobalVars
{
    public static bool bThreadStarted;
    public static bool bKeepWorking = true;

    public static string sConfigPath;

    //结构体
    public struct stSettings
    {
        public bool bAutoStart;
        public int iDataTarget;    // 0 -> COM    1 -> UDP
        public string sComPort;
        public int iBaundRate;
        public string sUDPNetIP;
        public int iUDPNetPort;
        public int iDelay;
        public string sMainDataFormat;

        public stSettings(bool autoStart, int dataTarget, string comPort, int BaundRate, string udpnetIP, int udpnetPort, string mainDataFormat,int delay)
        {
            bAutoStart = autoStart;
            iDataTarget = dataTarget;
            sComPort = comPort;
            sUDPNetIP = udpnetIP;
            iBaundRate = BaundRate;
            iUDPNetPort = udpnetPort;
            sMainDataFormat = mainDataFormat;
            iDelay = delay;
        }
    }

    public static stSettings sSettings = new stSettings(false, 0, "COM1", 115200, "", 0, "", 200);


    public static string sThreadMessage = "";
}
