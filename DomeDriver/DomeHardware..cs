﻿// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Dome hardware class for AMOS
//
// Description:	 <To be completed by driver developer>
//
// Implements:	ASCOM Dome interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//

using ASCOM;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Collections;
using System.Net.Configuration;
using System.Windows.Forms;

namespace ASCOM.AMOS.Dome
{
    //
    // TODO Replace the not implemented exceptions with code to implement the function or throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Dome hardware class for AMOS.
    /// </summary>
    [HardwareClass()] // Class attribute flag this as a device hardware class that needs to be disposed by the local server when it exits.
    internal static class DomeHardware
    {
        // Constants used for Profile persistence
        internal const string comPortProfileName = "COM Port";
        internal const string comPortDefault = "COM1";
        internal const string traceStateProfileName = "Trace Level";
        internal const string traceStateDefault = "true";

        private static string DriverProgId = ""; // ASCOM DeviceID (COM ProgID) for this driver, the value is set by the driver's class initialiser.
        private static string DriverDescription = ""; // The value is set by the driver's class initialiser.
        internal static string comPort; // COM port name (if required)
        private static bool connectedState; // Local server's connected state
        private static bool runOnce = false; // Flag to enable "one-off" activities only to run once.
        internal static Util utilities; // ASCOM Utilities object for use as required
        internal static AstroUtils astroUtilities; // ASCOM AstroUtilities object for use as required
        internal static TraceLogger tl; // Local server's trace logger object for diagnostic log with information that you specify
        internal static ASCOM.DriverAccess.Dome driver;

        /// <summary>
        /// Initializes a new instance of the device Hardware class.
        /// </summary>
        static DomeHardware()
        {
            try
            {
                // Create the hardware trace logger in the static initialiser.
                // All other initialisation should go in the InitialiseHardware method.
                tl = new TraceLogger("", "AMOS.Hardware");

                // DriverProgId has to be set here because it used by ReadProfile to get the TraceState flag.
                DriverProgId = Dome.DriverProgId; // Get this device's ProgID so that it can be used to read the Profile configuration values

                // ReadProfile has to go here before anything is written to the log because it loads the TraceLogger enable / disable state.
                ReadProfile(); // Read device configuration from the ASCOM Profile store, including the trace state

                LogMessage("DomeHardware", $"Static initialiser completed.");
            }
            catch (Exception ex)
            {
                try { LogMessage("DomeHardware", $"Initialisation exception: {ex}"); } catch { }
                MessageBox.Show($"{ex.Message}", "Exception creating ASCOM.AMOS.Dome", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// Place device initialisation code here
        /// </summary>
        /// <remarks>Called every time a new instance of the driver is created.</remarks>
        internal static void InitialiseHardware()
        {
            // This method will be called every time a new ASCOM client loads your driver
            LogMessage("InitialiseHardware", $"Start.");

            // Make sure that "one off" activities are only undertaken once
            if (runOnce == false)
            {
                LogMessage("InitialiseHardware", $"Starting one-off initialisation.");

                DriverDescription = Dome.DriverDescription; // Get this device's Chooser description

                LogMessage("InitialiseHardware", $"ProgID: {DriverProgId}, Description: {DriverDescription}");

                connectedState = false; // Initialise connected to false
                utilities = new Util(); //Initialise ASCOM Utilities object
                astroUtilities = new AstroUtils(); // Initialise ASCOM Astronomy Utilities object

                LogMessage("InitialiseHardware", "Completed basic initialisation");

                // Add your own "one off" device initialisation here e.g. validating existence of hardware and setting up communications

                LogMessage("InitialiseHardware", $"One-off initialisation complete.");
                runOnce = true; // Set the flag to ensure that this code is not run again
            }
        }

        // PUBLIC COM INTERFACE IDomeV2 IMPLEMENTATION

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialogue form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public static void SetupDialog()
        {
            // Don't permit the setup dialogue if already connected
            if (IsConnected)
                MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm(tl))
            {
                var result = F.ShowDialog();
                if (result == DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        /// <summary>Returns the list of custom action names supported by this driver.</summary>
        /// <value>An ArrayList of strings (SafeArray collection) containing the names of supported actions.</value>
        public static ArrayList SupportedActions
        {
            get
            {
                LogMessage("SupportedActions Get", "Returning empty ArrayList");
                return new ArrayList();
            }
        }

        /// <summary>Invokes the specified device-specific custom action.</summary>
        /// <param name="ActionName">A well known name agreed by interested parties that represents the action to be carried out.</param>
        /// <param name="ActionParameters">List of required parameters or an <see cref="String.Empty">Empty String</see> if none are required.</param>
        /// <returns>A string response. The meaning of returned strings is set by the driver author.
        /// <para>Suppose filter wheels start to appear with automatic wheel changers; new actions could be <c>QueryWheels</c> and <c>SelectWheel</c>. The former returning a formatted list
        /// of wheel names and the second taking a wheel name and making the change, returning appropriate values to indicate success or failure.</para>
        /// </returns>
        public static string Action(string actionName, string actionParameters)
        {
            LogMessage("Action", $"Action {actionName}, parameters {actionParameters} is not implemented");
            throw new ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and does not wait for a response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        public static void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // TODO The optional CommandBlind method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBlind must send the supplied command to the mount and return immediately without waiting for a response

            throw new MethodNotImplementedException($"CommandBlind - Command:{command}, Raw: {raw}.");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and waits for a boolean response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        /// <returns>
        /// Returns the interpreted boolean response received from the device.
        /// </returns>
        public static bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            // TODO The optional CommandBool method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBool must send the supplied command to the mount, wait for a response and parse this to return a True or False value

            throw new MethodNotImplementedException($"CommandBool - Command:{command}, Raw: {raw}.");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and waits for a string response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        /// <returns>
        /// Returns the string response received from the device.
        /// </returns>
        public static string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            // TODO The optional CommandString method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandString must send the supplied command to the mount and wait for a response before returning this to the client

            throw new MethodNotImplementedException($"CommandString - Command:{command}, Raw: {raw}.");
        }

        /// <summary>
        /// Deterministically release both managed and unmanaged resources that are used by this class.
        /// </summary>
        /// <remarks>
        /// TODO: Release any managed or unmanaged resources that are used in this class.
        /// 
        /// Do not call this method from the Dispose method in your driver class.
        ///
        /// This is because this hardware class is decorated with the <see cref="HardwareClassAttribute"/> attribute and this Dispose() method will be called 
        /// automatically by the  local server executable when it is irretrievably shutting down. This gives you the opportunity to release managed and unmanaged 
        /// resources in a timely fashion and avoid any time delay between local server close down and garbage collection by the .NET runtime.
        ///
        /// For the same reason, do not call the SharedResources.Dispose() method from this method. Any resources used in the static shared resources class
        /// itself should be released in the SharedResources.Dispose() method as usual. The SharedResources.Dispose() method will be called automatically 
        /// by the local server just before it shuts down.
        /// 
        /// </remarks>
        public static void Dispose()
        {
            try { LogMessage("Dispose", $"Disposing of assets and closing down."); } catch { }

            try
            {
                // Clean up the trace logger and utility objects
                tl.Enabled = false;
                tl.Dispose();
                tl = null;
            }
            catch { }

            try
            {
                utilities.Dispose();
                utilities = null;
            }
            catch { }

            try
            {
                astroUtilities.Dispose();
                astroUtilities = null;
            }
            catch { }
        }

        /// <summary>
        /// Set True to connect to the device hardware. Set False to disconnect from the device hardware.
        /// You can also read the property to check whether it is connected. This reports the current hardware state.
        /// </summary>
        /// <value><c>true</c> if connected to the hardware; otherwise, <c>false</c>.</value>
        public static bool Connected
        {
            get
            {
                LogMessage("Connected", $"Get {IsConnected}");
                return IsConnected;
            }
            set
            {
                LogMessage("Connected", $"Set {value}");
                if (value == IsConnected)
                    return;

                if (value)
                {
                    LogMessage("Connected Set", $"Connecting to port {comPort}");

                    driver = new ASCOM.DriverAccess.Dome("ASCOM.MaestroDome.Dome");
                    driver.Connected = true;
                    _shutterState = driver.ShutterStatus;


                    connectedState = true;
                }
                else
                {
                    LogMessage("Connected Set", $"Disconnecting from port {comPort}");

                    driver.Connected = false;
                    connectedState = false;
                }
            }
        }

        /// <summary>
        /// Returns a description of the device, such as manufacturer and model number. Any ASCII characters may be used.
        /// </summary>
        /// <value>The description.</value>
        public static string Description
        {
            // TODO customise this device description if required
            get
            {
                return driver.Description;
            }
        }

        /// <summary>
        /// Descriptive and version information about this ASCOM driver.
        /// </summary>
        public static string DriverInfo
        {
            get
            {
                return driver.DriverInfo; 
            }
        }

        /// <summary>
        /// A string containing only the major and minor version of the driver formatted as 'm.n'.
        /// </summary>
        public static string DriverVersion
        {
            get
            {
                return driver.DriverVersion;
            }
        }

        /// <summary>
        /// The interface version number that this device supports.
        /// </summary>
        public static short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                return driver.InterfaceVersion; 
            }
        }

        /// <summary>
        /// The short name of the driver, for display purposes
        /// </summary>
        public static string Name
        {
            // TODO customise this device name as required
            get
            {
                return driver.Name;
                
            }
        }

        #endregion

        #region IDome Implementation

        private static ShutterState _shutterState = ShutterState.shutterError; // Variable to hold the current state of the shutter

        /// <summary>
        /// Immediately stops any and all movement of the dome.
        /// </summary>
        internal static void AbortSlew()
        {
            driver.AbortSlew();
            
        }

        /// <summary>
        /// The altitude (degrees, horizon zero and increasing positive to 90 zenith) of the part of the sky that the observer wishes to observe.
        /// </summary>
        internal static double Altitude
        {
            get
            {
                return driver.Altitude;
                
            }
        }

        /// <summary>
        /// <para><see langword="true" /> when the dome is in the home position. Raises an error if not supported.</para>
        /// <para>
        /// This is normally used following a <see cref="FindHome" /> operation. The value is reset
        /// with any azimuth slew operation that moves the dome away from the home position.
        /// </para>
        /// <para>
        /// <see cref="AtHome" /> may optionally also become true during normal slew operations, if the
        /// dome passes through the home position and the dome controller hardware is capable of
        /// detecting that; or at the end of a slew operation if the dome comes to rest at the home
        /// position.
        /// </para>
        /// </summary>
        internal static bool AtHome
        {
            get
            {                
                return (driver.CanSetShutter);

            }
        }

        /// <summary>
        /// <see langword="true" /> if the dome is in the programmed park position.
        /// </summary>
        internal static bool AtPark
        {
            get
            {
                // if athome or atpark is true, then return true
                return (driver.CanSetShutter);
            }
        }

        /// <summary>
        /// The dome azimuth (degrees, North zero and increasing clockwise, i.e., 90 East, 180 South, 270 West). North is true north and not magnetic north.
        /// </summary>
        internal static double Azimuth
        {
            get
            {
                return driver.Azimuth;
                
            }
        }

        /// <summary>
        /// <see langword="true" /> if driver can perform a search for home position.
        /// </summary>
        internal static bool CanFindHome
        {
            get
            {
                return driver.CanFindHome;
                
            }
        }

        /// <summary>
        /// <see langword="true" /> if the driver is capable of parking the dome.
        /// </summary>
        internal static bool CanPark
        {
            get
            {
                return true;
                
            }
        }

        /// <summary>
        /// <see langword="true" /> if driver is capable of setting dome altitude.
        /// </summary>
        internal static bool CanSetAltitude
        {
            get
            {
                return driver.CanSetAltitude;
                
            }
        }

        /// <summary>
        /// <see langword="true" /> if driver is capable of rotating the dome. Muste be <see "langword="false" /> for a 
        /// roll-off roof or clamshell.
        /// </summary>
        internal static bool CanSetAzimuth
        {
            get
            {
                return driver.CanSetAzimuth;
                
            }
        }

        /// <summary>
        /// <see langword="true" /> if the driver can set the dome park position.
        /// </summary>
        internal static bool CanSetPark
        {
            get
            {
                return true;
                
            }
        }

        /// <summary>
        /// <see langword="true" /> if the driver is capable of opening and closing the shutter or roof
        /// mechanism.
        /// </summary>
        internal static bool CanSetShutter
        {
            get
            {
                return true;
                
            }
        }

        /// <summary>
        /// <see langword="true" /> if the dome hardware supports slaving to a telescope.
        /// </summary>
        internal static bool CanSlave
        {
            get
            {
                return driver.CanSlave;
                
            }
        }

        /// <summary>
        /// <see langword="true" /> if the driver is capable of synchronizing the dome azimuth position
        /// using the <see cref="SyncToAzimuth" /> method.
        /// </summary>
        internal static bool CanSyncAzimuth
        {
            get
            {
                return driver.CanSyncAzimuth;
                
            }
        }

        /// <summary>
        /// Close the shutter or otherwise shield the telescope from the sky.
        /// </summary>
        internal static void CloseShutter()
        {
            _shutterState = ShutterState.shutterClosing;
            driver.Park();
            //wait until parked
            while (driver.CanSetShutter == false)
            {
                System.Threading.Thread.Sleep(1000);
            }
            driver.CloseShutter();
            LogMessage("CloseShutter", "Shutter has been closed");
            //monitor shutter state
            while (driver.ShutterStatus != ShutterState.shutterClosed)
            {
                System.Threading.Thread.Sleep(1000);
            }
            _shutterState = ShutterState.shutterClosed;
        }

        /// <summary>
        /// Start operation to search for the dome home position.
        /// </summary>
        internal static void FindHome()
        {
            driver.FindHome();
            
        }

        /// <summary>
        /// Open shutter or otherwise expose telescope to the sky.
        /// </summary>
        internal static void OpenShutter()
        {
            _shutterState = ShutterState.shutterOpening;
            driver.Park();
            //wait until parked
                        
            while (driver.CanSetShutter == false)
            {
                System.Threading.Thread.Sleep(1000);
            }
            


            driver.OpenShutter();
            
            while (driver.ShutterStatus != ShutterState.shutterOpen)
            {
                System.Threading.Thread.Sleep(1000);
            }
            LogMessage("OpenShutter", "Shutter has been opened");
            _shutterState = ShutterState.shutterOpen;
        }

        /// <summary>
        /// Rotate dome in azimuth to park position.
        /// </summary>
        internal static void Park()
        {
            driver.Park();
            
        }

        /// <summary>
        /// Set the current azimuth position of dome to the park position.
        /// </summary>
        internal static void SetPark()
        {
            driver.SetPark();
            
        }

        /// <summary>
        /// Gets the status of the dome shutter or roof structure.
        /// </summary>
        internal static ShutterState ShutterStatus
        {
            get
            {  
                
                if (_shutterState == ShutterState.shutterError)
                {                    
                    return driver.ShutterStatus; 
                }
                else
                {
                    LogMessage("ShutterStatus Get", false.ToString());
                    return _shutterState;
                }                            
            }
        }

        /// <summary>
        /// <see langword="true"/> if the dome is slaved to the telescope in its hardware, else <see langword="false"/>.
        /// </summary>
        internal static bool Slaved
        {
            get
            {
                return driver.Slaved;
                
            }
            set
            {
                driver.Slaved = value;
                
            }
        }

        /// <summary>
        /// Ensure that the requested viewing altitude is available for observing.
        /// </summary>
        /// <param name="Altitude">
        /// The desired viewing altitude (degrees, horizon zero and increasing positive to 90 degrees at the zenith)
        /// </param>
        internal static void SlewToAltitude(double Altitude)
        {
            driver.SlewToAltitude(Altitude);
            
        }

        /// <summary>
        /// Ensure that the requested viewing azimuth is available for observing.
        /// The method should not block and the slew operation should complete asynchronously.
        /// </summary>
        /// <param name="Azimuth">
        /// Desired viewing azimuth (degrees, North zero and increasing clockwise. i.e., 90 East,
        /// 180 South, 270 West)
        /// </param>
        internal static void SlewToAzimuth(double Azimuth)
        {
            driver.SlewToAzimuth(Azimuth);
            
        }

        /// <summary>
        /// <see langword="true" /> if any part of the dome is currently moving or a move command has been issued, 
        /// but the dome has not yet started to move. <see langword="false" /> if all dome components are stationary
        /// and no move command has been issued. /> 
        /// </summary>
        internal static bool Slewing
        {
            get
            {
                return driver.Slewing;
                
            }
        }

        /// <summary>
        /// Synchronize the current position of the dome to the given azimuth.
        /// </summary>
        /// <param name="Azimuth">
        /// Target azimuth (degrees, North zero and increasing clockwise. i.e., 90 East,
        /// 180 South, 270 West)
        /// </param>
        internal static void SyncToAzimuth(double Azimuth)
        {
            driver.SyncToAzimuth(Azimuth);            
        }

        #endregion

        #region Private properties and methods
        // Useful methods that can be used as required to help with driver development

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private static bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private static void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal static void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Dome";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(DriverProgId, traceStateProfileName, string.Empty, traceStateDefault));
                comPort = driverProfile.GetValue(DriverProgId, comPortProfileName, string.Empty, comPortDefault);
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal static void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Dome";
                driverProfile.WriteValue(DriverProgId, traceStateProfileName, tl.Enabled.ToString());
                driverProfile.WriteValue(DriverProgId, comPortProfileName, comPort.ToString());
            }
        }

        /// <summary>
        /// Log helper function that takes identifier and message strings
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        internal static void LogMessage(string identifier, string message)
        {
            tl.LogMessageCrLf(identifier, message);
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal static void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            LogMessage(identifier, msg);
        }
        #endregion
    }
}

