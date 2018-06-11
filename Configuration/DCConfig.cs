using AMS.Profile;
using DCLog;
using System;

namespace Configuration
{
    public class DCConfig
    {
        private static readonly object mutex = new object();
        private static volatile DCConfig instance;

        /// <summary>
        /// Instantiate the one and only object!
        /// </summary>
        public static DCConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (mutex)
                    {
                        if (instance == null)
                        {
                            // Call constructor
                            instance = new DCConfig();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        ///     Logger instance
        /// </summary>

        private Xml _profile;

        public string ConfigName { get; set; }

        /// <summary>
        /// Create a new instance
        /// </summary>
        private DCConfig()
        {
            ConfigName = "dcmarker.xml";
            _profile = new Xml(ConfigName);
            ReadConfig();
#if false
            WriteConfig();
#endif
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="configName"></param>
        public DCConfig(string configName)
        {
            ConfigName = configName;
            _profile = new Xml(ConfigName);
        }

        public void ReadConfig()
        {
            try
            {
                /*  Machine Types
                 *
                 *  1   Automatic                   e.g. Kenny
                 *  2   Automatic with TO number    e.g. Filip?
                 *  3   Manual with TO number       e.g. Ultraflow
                 *  4   Automatic with ADAM         e.g. Nippel- & flödeskoppling
                 */

                // Machine
                TypeOfMachine = _profile.GetValue("Machine", nameof(TypeOfMachine), 1);
                GuiLanguage = _profile.GetValue("Machine", nameof(GuiLanguage), "sv-SE");
                Debug = _profile.GetValue("Machine", nameof(Debug), false);
                ClearClipboard = _profile.GetValue("Machine", nameof(ClearClipboard), true);
                CommunicationModule = _profile.GetValue("Machine", nameof(CommunicationModule), 1); // Default to ADAM
                // Laser
                DeviceAddress = _profile.GetValue("Laser", nameof(DeviceAddress), "127.0.0.1");
                DeviceTimeout = _profile.GetValue("Laser", nameof(DeviceTimeout), 10);
                ImagePath = _profile.GetValue("Laser", nameof(ImagePath), @"C:\DCMarker\Images");
                LayoutPath = _profile.GetValue("Laser", nameof(LayoutPath), @"C:\DCMarker\Layouts");
                ExecuteTimeout = _profile.GetValue("Laser", nameof(ExecuteTimeout), 10000);
                IsIoEnabled = _profile.GetValue("Laser", nameof(IsIoEnabled), true);
                ReadyToMark = _profile.GetValue("Laser", nameof(ReadyToMark), 0x01);                    // OUTPUT 0
                MarkingDone = _profile.GetValue("Laser", nameof(MarkingDone), 0x02);                    // OUTPUT 1
                ArticleReady = _profile.GetValue("Laser", nameof(ArticleReady), 0x10);                  // OUTPUT 4
                NextToLast = _profile.GetValue("Laser", nameof(NextToLast), 0x40);                      // OUTPUT 6
                Error = _profile.GetValue("Laser", nameof(Error), 0x80);                                // OUTPUT 7
                ItemInPlace = _profile.GetValue("Laser", nameof(ItemInPlace), 0x02);                    // INPUT 1
                EmergencyError = _profile.GetValue("Laser", nameof(EmergencyError), 0x10);              // INPUT 4
                ResetIo = _profile.GetValue("Laser", nameof(ResetIo), 0x8);                             // INPUT 3

                // TCP Server
                TcpPort = _profile.GetValue("TcpServer", nameof(TcpPort), 50000);
                BufferLength = _profile.GetValue("TcpServer", nameof(BufferLength), 12 + 7 + 2);
                ArticleNumberLength = _profile.GetValue("TcpServer", nameof(ArticleNumberLength), 12);
                ToNumberLength = _profile.GetValue("TcpServer", nameof(ToNumberLength), 7);

                // ADAM 6052
                AdamIpAddress = _profile.GetValue("Adam", nameof(AdamIpAddress), "10.0.0.100");
                AdamIpPort = _profile.GetValue("Adam", nameof(AdamIpPort), 502);
                AdamInvertSignal = _profile.GetValue("Adam", nameof(AdamInvertSignal), false);
                IsAdamErrorTimeoutActive = _profile.GetValue("Adam", nameof(IsAdamErrorTimeoutActive), true);
                AdamErrorTimeout = _profile.GetValue("Adam", nameof(AdamErrorTimeout), 1000);
                AdamAllowedTimeouts = _profile.GetValue("Adam", nameof(AdamAllowedTimeouts), 20);
                AdamWaitBeforeWrite = _profile.GetValue("Adam", nameof(AdamWaitBeforeWrite), 10);
                AdamWaitBeforeRead = _profile.GetValue("Adam", nameof(AdamWaitBeforeRead), 10);
                AdamMinRereads = _profile.GetValue("Adam", nameof(AdamMinRereads), 3);
                AdamPollInterval = _profile.GetValue("Adam", nameof(AdamPollInterval), 10);
                AdamLogTelegrams = _profile.GetValue("Adam", nameof(AdamLogTelegrams), false);
                // GUI
                ResetInputValues = _profile.GetValue("GUI", nameof(ResetInputValues), true);
                KeepQuantity = _profile.GetValue("GUI", nameof(KeepQuantity), false);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error reading dcmarker.xml config file");
                throw;
            }
        }

        public void WriteConfig()
        {
            try
            {
                // Machine
                _profile.SetValue("Machine", nameof(TypeOfMachine), TypeOfMachine);
                _profile.SetValue("Machine", nameof(GuiLanguage), GuiLanguage);
                _profile.SetValue("Machine", nameof(Debug), Debug);
                _profile.SetValue("Machine", nameof(ClearClipboard), ClearClipboard);
                _profile.SetValue("Machine", nameof(CommunicationModule), CommunicationModule);

                // Laser
                _profile.SetValue("Laser", nameof(DeviceAddress), DeviceAddress);
                _profile.SetValue("Laser", nameof(DeviceTimeout), DeviceTimeout);
                _profile.SetValue("Laser", nameof(ImagePath), ImagePath);
                _profile.SetValue("Laser", nameof(LayoutPath), LayoutPath);
                _profile.SetValue("Laser", nameof(ExecuteTimeout), ExecuteTimeout);
                _profile.SetValue("Laser", nameof(IsIoEnabled), IsIoEnabled);
                _profile.SetValue("Laser", nameof(ArticleReady), ArticleReady);
                _profile.SetValue("Laser", nameof(ReadyToMark), ReadyToMark);
                _profile.SetValue("Laser", nameof(MarkingDone), MarkingDone);
                _profile.SetValue("Laser", nameof(NextToLast), NextToLast);
                _profile.SetValue("Laser", nameof(Error), Error);
                _profile.SetValue("Laser", nameof(ItemInPlace), ItemInPlace);
                _profile.SetValue("Laser", nameof(EmergencyError), EmergencyError);
                _profile.SetValue("Laser", nameof(ResetIo), ResetIo);

                // TCP Server
                _profile.SetValue("TcpServer", nameof(TcpPort), TcpPort);
                _profile.SetValue("TcpServer", nameof(BufferLength), BufferLength);
                _profile.SetValue("TcpServer", nameof(ArticleNumberLength), ArticleNumberLength);
                _profile.SetValue("TcpServer", nameof(ToNumberLength), ToNumberLength);

                // ADAM 6052
                _profile.SetValue("Adam", nameof(AdamIpAddress), AdamIpAddress);
                _profile.SetValue("Adam", nameof(AdamIpPort), AdamIpPort);
                _profile.SetValue("Adam", nameof(AdamInvertSignal), AdamInvertSignal);
                _profile.SetValue("Adam", nameof(IsAdamErrorTimeoutActive), IsAdamErrorTimeoutActive);
                _profile.SetValue("Adam", nameof(AdamErrorTimeout), AdamErrorTimeout);
                _profile.SetValue("Adam", nameof(AdamAllowedTimeouts), AdamAllowedTimeouts);
                _profile.SetValue("Adam", nameof(AdamWaitBeforeWrite), AdamWaitBeforeWrite);
                _profile.SetValue("Adam", nameof(AdamWaitBeforeRead), AdamWaitBeforeRead);
                _profile.SetValue("Adam", nameof(AdamMinRereads), AdamMinRereads);
                _profile.SetValue("Adam", nameof(AdamPollInterval), AdamPollInterval);
                _profile.SetValue("Adam", nameof(AdamLogTelegrams), AdamLogTelegrams);
                // GUI
                _profile.SetValue("GUI", nameof(ResetInputValues), ResetInputValues);
                _profile.SetValue("GUI", nameof(KeepQuantity), KeepQuantity);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error writing to dcmarker.xml config file");
                throw;
            }
        }

        #region Machine properties

        public int TypeOfMachine { get; set; }
        public bool Debug { get; set; }
        public bool ClearClipboard { get; set; }

        public int CommunicationModule { get; set; }

        #endregion Machine properties

        #region Laser properties

        public string DeviceAddress { get; set; }
        public int DeviceTimeout { get; set; }
        public string ImagePath { get; set; }
        public string LayoutPath { get; set; }
        public int ExecuteTimeout { get; set; }
        public bool IsIoEnabled { get; set; }
        public int ArticleReady { get; set; }
        public int ReadyToMark { get; set; }
        public int MarkingDone { get; set; }
        public int NextToLast { get; set; }
        public int Error { get; set; }
        public int ItemInPlace { get; set; }
        public int EmergencyError { get; set; }
        public int ResetIo { get; set; }

        #endregion Laser properties

        #region TcpServer properties

        public int TcpPort { get; private set; }
        public int BufferLength { get; private set; }
        public int ArticleNumberLength { get; set; }
        public int ToNumberLength { get; set; }
        public string GuiLanguage { get; private set; }

        #endregion TcpServer properties

        #region ADAM

        /// <summary>
        /// ADAM module IP address
        /// </summary>
        public string AdamIpAddress { get; set; }

        /// <summary>
        /// ADAM module port
        /// </summary>
        public int AdamIpPort { get; set; }

        /// <summary>
        /// Should we invert the signal from the ADAM module
        /// </summary>
        public bool AdamInvertSignal { get; set; }

        public bool IsAdamErrorTimeoutActive { get; set; }

        /// <summary>
        /// Timeout in milliseconds before we treat the action as failed
        /// </summary>
        public int AdamErrorTimeout { get; set; }

        /// <summary>
        /// Number of timouts allowed before stopping communication!
        /// </summary>
        public int AdamAllowedTimeouts { get; set; }

        /// <summary>
        /// Number of milliseconds we wait before we send an STX.
        /// This so that the PLC have time to read the last ACK
        /// </summary>
        public int AdamWaitBeforeWrite { get; set; }

        public int AdamWaitBeforeRead { get; set; }

        /// <summary>
        /// Number of times we re-read a param to be sure that we have gotten the correct one!
        /// </summary>
        public int AdamMinRereads { get; set; }

        /// <summary>
        /// Interval in milliseconds between polling the ADAM module
        /// </summary>
        public int AdamPollInterval { get; set; }

        public bool AdamLogTelegrams { get; set; }

        #endregion ADAM

        #region GUI

        public bool ResetInputValues { get; set; }
        public bool KeepQuantity { get; set; }

        #endregion GUI
    }
}