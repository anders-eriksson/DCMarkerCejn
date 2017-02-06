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
                DeviceAddress = _profile.GetValue("Laser", nameof(DeviceAddress), "127.0.0.1");
                DeviceTimeout = _profile.GetValue("Laser", nameof(DeviceTimeout), 10);
                ImagePath = _profile.GetValue("Laser", nameof(ImagePath), @"C:\DCMarker\Images");
                LayoutPath = _profile.GetValue("Laser", nameof(LayoutPath), @"C:\DCMarker\Layouts");
                ExecuteTimeout = _profile.GetValue("Laser", nameof(ExecuteTimeout), 10000);
                IsIoEnabled = _profile.GetValue("Laser", nameof(IsIoEnabled), false);
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
                _profile.SetValue("Laser", nameof(DeviceAddress), DeviceAddress);
                _profile.GetValue("Laser", nameof(DeviceTimeout), DeviceTimeout);
                _profile.GetValue("Laser", nameof(ImagePath), ImagePath);
                _profile.GetValue("Laser", nameof(LayoutPath), LayoutPath);
                _profile.GetValue("Laser", nameof(ExecuteTimeout), ExecuteTimeout);
                _profile.GetValue("Laser", nameof(IsIoEnabled), IsIoEnabled);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error writing to dcmarker.xml config file");
                throw;
            }
        }

        #region Laser properties

        public string DeviceAddress { get; set; }
        public int DeviceTimeout { get; set; }
        public string ImagePath { get; set; }
        public string LayoutPath { get; set; }
        public int ExecuteTimeout { get; set; }
        public bool IsIoEnabled { get; set; }

        #endregion Laser properties
    }
}