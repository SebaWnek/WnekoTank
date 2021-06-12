namespace CommonsLibrary
{
    /// <summary>
    /// Communication protocol codes
    /// </summary>
    public static class TankCommandList
    {
        public static string emergencyPrefix = "EMG";
        public static string setGear = "GEA";
        public static string setLinearSpeed = "SPD";
        public static string setTurn = "TRN";
        public static string stop = "STP";
        public static string wait = "WAI";
        public static string startInvoking = "QST";
        public static string stopInvoking = "QSP";
        public static string enumerateQueue = "QEN";
        public static string clearQueue = "QCL";
        public static string handshake = "HSK";
        public static string moveForwardBy = "MFW";
        public static string softStop = "STS";
        public static string tempPres = "TPR";
        public static string position = "POS";
        public static string calibrate = "CAL";
        public static string checkCalibration = "CCL";
        public static string turnBy = "TBY";
        public static string stabilize = "STB";
        public static string setProxSensors = "PXS";
        public static string setGimbalAngle = "GSA";
        public static string changeGimbalAngleBy = "GCB";
        public static string stabilizeGimbal = "GST";
        public static string diagnoze = "DGZ";
        public static string getElectricData = "ELD";
        public static string sendingElectricData = "ELS";
        public static string fanLedsState = "FLS";
        public static string fanInasState = "ILS";
        public static string fanMotorsState = "MLS";
        public static string ledWidePower = "LWP";
        public static string ledNarrowPower = "LNP";
        //public static string cameraPower = "CAM";

    }
}