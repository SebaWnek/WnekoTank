namespace CommonsLibrary
{
    /// <summary>
    /// Communication protocol codes
    /// </summary>
    public static class TankCommandList
    {
        /// <summary>
        /// empty
        /// </summary>
        public static string emergencyPrefix = "EMG";  
        /// <summary>
        /// int gear
        /// </summary>
        public static string setGear = "GEA"; 
        /// <summary>
        /// int speed
        /// </summary>
        public static string setLinearSpeed = "SPD";        
        /// <summary>
        /// int turn rate
        /// </summary>
        public static string setTurn = "TRN";    
        /// <summary>
        /// empty
        /// </summary>
        public static string stop = "STP";         
        /// <summary>
        /// 
        /// </summary>
        public static string wait = "WAI";   
        /// <summary>
        /// 
        /// </summary>
        public static string startInvoking = "QST";    
        /// <summary>
        /// 
        /// </summary>
        public static string stopInvoking = "QSP";     
        /// <summary>
        /// 
        /// </summary>
        public static string enumerateQueue = "QEN";    
        /// <summary>
        /// 
        /// </summary>
        public static string clearQueue = "QCL";       
        /// <summary>
        /// 
        /// </summary>
        public static string handshake = "HSK";     
        /// <summary>
        /// 
        /// </summary>
        public static string moveForwardBy = "MFW";
        /// <summary>
        /// int horizontal angle ; int vertical angle
        /// </summary>
        public static string moveByCamera = "MFC";
        /// <summary>
        /// 
        /// </summary>
        public static string softStop = "STS";    
        /// <summary>
        /// 
        /// </summary>
        public static string tempPres = "TPR";    
        /// <summary>
        /// 
        /// </summary>
        public static string position = "POS";      
        /// <summary>
        /// 
        /// </summary>
        public static string calibrate = "CAL";       
        /// <summary>
        /// 
        /// </summary>
        public static string checkCalibration = "CCL";  
        /// <summary>
        /// 
        /// </summary>
        public static string turnBy = "TBY";             
        /// <summary>
        /// 
        /// </summary>
        public static string turnToByCamera = "TTC";       
        /// <summary>
        /// 
        /// </summary>
        public static string turnTo = "TTO";           
        /// <summary>
        /// 
        /// </summary>
        public static string stabilize = "STB";      
        /// <summary>
        /// 
        /// </summary>
        public static string setProxSensors = "PXS";     
        /// <summary>
        /// 
        /// </summary>
        public static string setGimbalAngle = "GSA";        
        /// <summary>
        /// 
        /// </summary>
        public static string changeGimbalAngleBy = "GCB";   
        /// <summary>
        /// 
        /// </summary>
        public static string stabilizeGimbal = "GST";  
        /// <summary>
        /// 
        /// </summary>
        public static string diagnoze = "DGZ";     
        /// <summary>
        /// 
        /// </summary>
        public static string getElectricData = "ELD";    
        /// <summary>
        /// 
        /// </summary>
        public static string sendingElectricData = "ELS";   
        /// <summary>
        /// 
        /// </summary>
        public static string setElectricDataDelay = "ESD";  
        /// <summary>
        /// 
        /// </summary>
        public static string fanLedsState = "FLS";        
        /// <summary>
        /// 
        /// </summary>
        public static string fanInasState = "ILS";    
        /// <summary>
        /// 
        /// </summary>
        public static string fanMotorsState = "MLS";    
        /// <summary>
        /// int brightness
        /// </summary>
        public static string ledWidePower = "LWP";       
        /// <summary>
        /// int brightness
        /// </summary>
        public static string ledNarrowPower = "LNP";        
        //public static string cameraPower = "CAM";         

    }
}