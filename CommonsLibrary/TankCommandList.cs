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
        /// int linear speed ; int turn rate
        /// </summary>
        public static string setSpeedWithTurn = "SPT";

        /// <summary>
        /// int turn rate
        /// </summary>
        public static string setTurn = "TRN";    

        /// <summary>
        /// empty
        /// </summary>
        public static string stop = "STP";       
        
        /// <summary>
        /// int time in ms
        /// </summary>
        public static string wait = "WAI";   

        /// <summary>
        /// empty
        /// </summary>
        public static string startInvoking = "QST";    

        /// <summary>
        /// empty
        /// </summary>
        public static string stopInvoking = "QSP";    
        
        /// <summary>
        /// empty
        /// </summary>
        public static string enumerateQueue = "QEN";    

        /// <summary>
        /// empty
        /// </summary>
        public static string clearQueue = "QCL";       

        /// <summary>
        /// empty
        /// </summary>
        public static string handshake = "HSK";

        /// <summary>
        /// int speed ; float distance ; string 1 - should break, 0 - no break ; byte gear (optional) ; string 1 - soft stop
        /// </summary>
        public static string moveForwardBy = "MFW";

        /// <summary>
        /// int horizontal angle ; int vertical angle
        /// </summary>
        public static string moveByCamera = "MFC";

        /// <summary>
        /// empty
        /// </summary>
        public static string softStop = "STS";    

        /// <summary>
        /// empty
        /// </summary>
        public static string tempPres = "TPR";    

        /// <summary>
        /// empty
        /// </summary>
        public static string position = "POS";      

        /// <summary>
        /// empty
        /// </summary>
        public static string calibrate = "CAL";       

        /// <summary>
        /// empty
        /// </summary>
        public static string checkCalibration = "CCL";  

        /// <summary>
        /// int angle ; int turn rate ; byte gear
        /// </summary>
        public static string turnBy = "TBY";            
        
        /// <summary>
        /// int angle
        /// </summary>
        public static string turnToByCamera = "TTC";   
        
        /// <summary>
        /// int angle
        /// </summary>
        public static string turnTo = "TTO";           

        /// <summary>
        /// int stabilization state
        /// </summary>
        public static string stabilizeDirection = "STB";      

        /// <summary>
        /// int stop behavior reprezenting StopBehavior enum
        /// </summary>
        public static string setProxSensors = "PXS";

        /// <summary>
        /// int vertical change ; int horizontal change
        /// </summary>
        public static string setGimbalAngle = "GSA";        

        /// <summary>
        /// int vertical change ; int horizontal change
        /// </summary>
        public static string changeGimbalAngleBy = "GCB";   


        /// <summary>
        /// int stabilization state
        /// </summary>
        public static string stabilizeGimbal = "GST";  

        /// <summary>
        /// empty
        /// </summary>
        public static string diagnoze = "DGZ";     

        /// <summary>
        /// empty
        /// </summary>
        public static string getElectricData = "ELD";    

        /// <summary>
        /// int sending state
        /// </summary>
        public static string sendingElectricData = "ELS";   

        /// <summary>
        /// int time in ms
        /// </summary>
        public static string setElectricDataDelay = "ESD";

        /// <summary>
        /// int desired state
        /// </summary>
        public static string fanLedsState = "FLS";

        /// <summary>
        /// int desired state
        /// </summary>
        public static string fanInasState = "ILS";    

        /// <summary>
        /// int desired state
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