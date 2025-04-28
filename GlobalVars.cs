/// <summary>
/// 简单粗暴的全局变量
/// </summary>
public static class GlobalVars
{
    public static bool bThreadStarted;
    public static bool bKeepWorking = true;

    public static string sConfigPath;

    //结构体
    public struct stSettings
    {
        public bool bAutoStart;
        public int iDataTarget;    // 0 -> COM    1 -> TCP
        public string sComPort;
        public int iBaundRate;
        public string sNetIP;
        public int iNetPort;
        public int iDelay;
        public string sMainDataFormat;

        public stSettings(bool autoStart, int dataTarget, string comPort, int BaundRate, string netIP, int netPort, string mainDataFormat,int delay)
        {
            bAutoStart = autoStart;
            iDataTarget = dataTarget;
            sComPort = comPort;
            sNetIP = netIP;
            iBaundRate = BaundRate;
            iNetPort = netPort;
            sMainDataFormat = mainDataFormat;
            iDelay = delay;
        }
    }

    public static stSettings sSettings = new stSettings(false, 0, "COM1", 115200, "", 0, "", 200);


    public static string sThreadMessage = "";
}
