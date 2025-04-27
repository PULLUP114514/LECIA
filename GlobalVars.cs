/// <summary>
/// 简单粗暴的全局变量
/// </summary>
public static class GlobalVars
{
    public static bool bThreadStarted;
    public static bool bKeepWorking = true;

    //结构体
    public struct stSettings
    {
        public bool bAutoStart;
        public int iDataTarget;    // 0 -> COM    1 -> TCP
        public string sComPort;
        public int iBaundRate;
        public string sNetIP;
        public int iNetPort;
        public string sMainDataFormat;

        public stSettings(bool autoStart, int dataTarget, string comPort, int BaundRate, string netIP, int netPort, string mainDataFormat)
        {
            bAutoStart = autoStart;
            iDataTarget = dataTarget;
            sComPort = comPort;
            sNetIP = netIP;
            iBaundRate = BaundRate;
            iNetPort = netPort;
            sMainDataFormat = mainDataFormat;
        }
    }

    public static stSettings sSettings= new stSettings(false, 0, "COM1", 115200, "", 0, "");
}
