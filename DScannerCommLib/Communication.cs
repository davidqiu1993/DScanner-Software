using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace DScanner.Communication
{
    /// <summary>
    /// Controller for the stepper device communication.
    /// </summary>
    public class StepperController
    {
        #region Local Data Structures

        /// <summary>
        /// State of the stepper.
        /// </summary>
        private enum StepperState
        {
            Initialized = 0,
            Reset = 1,
            CommandSent = 2,
            CommandConfirmed = 3,
            CommandMismatched = 4,
            RotationStarted = 5,
            RotationFinished = 6
        }

        #endregion


        #region Local Variables

        SerialPort _StepperControllerPort; // Serial port of stepper controller

        #endregion


        #region Local Configurations

        int _DesireSteps; // Desired steps for the stepper motor to rotate
        StepperState _StepperState; // State of the stepper

        #endregion


        #region Local Functions

        /// <summary>
        /// The handler dealing with feedback messages from the 
        /// stepper controller port.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Arguments of the event.</param>
        private void _FeedbackHandler(object sender, SerialDataReceivedEventArgs args)
        {
            string line;
            string feedback;

            // Read the received feedback line
            line = _StepperControllerPort.ReadLine();
            
            // Check if the line matches format
            if (line.Length < 5)
            {
                OperationFailed();
                _StepperControllerPort.Write("X");
                return;
            }
            if (!(line[0] == '$' &&
                line[1] == '$' &&
                line[line.Length - 1] == '$' &&
                line[line.Length - 2] == '$'))
            {
                OperationFailed();
                _StepperControllerPort.Write("X");
                return;
            }

            // Retrive the feedback message
            feedback = line.Substring(2, line.Length - 4);

            // Check if desired steps confirm message received
            if(feedback[0] == ':')
            {
                // Obtain the confirm steps
                int confirmSteps;
                if (!int.TryParse(feedback.Substring(1), out confirmSteps))
                {
                    OperationFailed();
                    _StepperControllerPort.Write("X");
                    return;
                }

                // Check if confirm and desired steps match
                if(confirmSteps == _DesireSteps && _StepperState == StepperState.CommandSent)
                {
                    // Set the confirm flag
                    _StepperState = StepperState.CommandConfirmed;

                    // Confirm the rotation steps
                    _StepperControllerPort.Write("OK");
                }
                else
                {
                    // Reset the confirm flag
                    _StepperState = StepperState.CommandMismatched;

                    // Send operation failed event
                    OperationFailed();

                    // Trigger reset event
                    _StepperControllerPort.Write("X");
                }
            }
            else
            {
                switch(feedback)
                {
                    case "INIT":
                        {
                            // Set the stepper state as initialized
                            _DesireSteps = 0;
                            _StepperState = StepperState.Initialized;
                        }
                        break;

                    case "RESET":
                        {
                            // Set the stepper state as reset
                            _DesireSteps = 0;
                            _StepperState = StepperState.Reset;
                        }
                        break;

                    case "START":
                        {
                            // Set the stepper state as started
                            if (_StepperState == StepperState.CommandConfirmed) _StepperState = StepperState.RotationStarted;
                            else
                            {
                                OperationFailed();
                                _StepperControllerPort.Write("X");
                                return;
                            }

                        }
                        break;

                    case "FINISH":
                        {
                            // Set the stepper state as finished
                            _StepperState = StepperState.RotationFinished;
                            
                            // Send rotation finished event
                            RotationFinished();
                        }
                        break;

                    default:
                        {
                            OperationFailed();
                            _StepperControllerPort.Write("X");
                            return;
                        }
                }
            }
        }

        #endregion


        #region Delegates and Events

        /// <summary>
        /// Handler for rotation finished event.
        /// </summary>
        public delegate void RotationFinishedEventHandler();

        /// <summary>
        /// Rotation finished event.
        /// </summary>
        public event RotationFinishedEventHandler RotationFinished;

        /// <summary>
        /// Handler for operation failed event.
        /// </summary>
        public delegate void OperationFailedEventHandler();

        /// <summary>
        /// Operation failed event. It will be triggered when 
        /// error occurs after the command sent.
        /// </summary>
        public event OperationFailedEventHandler OperationFailed;

        #endregion


        #region Class Object Constructors and Destructor

        /// <summary>
        /// Construct a default StepperController class object.
        /// </summary>
        public StepperController()
        {
            // Initailze the stepper controller port
            _StepperControllerPort = new SerialPort();
            _StepperControllerPort.DataReceived += _FeedbackHandler;

            // Reset the stepper state
            _StepperState = StepperState.Initialized;
        }

        /// <summary>
        /// Construct a StepperController class object with given port name.
        /// </summary>
        /// <param name="portName">Port name of the stepper controller port.</param>
        public StepperController(string portName)
        {
            // Initailze the stepper controller port
            _StepperControllerPort = new SerialPort(portName);
            _StepperControllerPort.DataReceived += _FeedbackHandler;

            // Reset the stepper state
            _StepperState = StepperState.Initialized;
        }

        /// <summary>
        /// Construct a StepperController class object with full given parameters.
        /// </summary>
        /// <param name="portName">Name of the stepper controller port.</param>
        /// <param name="baudRate">Serial communication baud rate.</param>
        /// <param name="parity">Parity check protocal.</param>
        /// <param name="dataBits">Standard length of data bits per byte.</param>
        /// <param name="stopBits">Standard length of stop bits per byte.</param>
        public StepperController(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            // Initailze the stepper controller port
            _StepperControllerPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _StepperControllerPort.DataReceived += _FeedbackHandler;

            // Reset the stepper state
            _StepperState = StepperState.Initialized;
        }

        /// <summary>
        /// Destructor of the StepperController object.
        /// </summary>
        ~StepperController()
        {
            if (_StepperControllerPort.IsOpen) _StepperControllerPort.Close();
        }

        #endregion


        #region Class Attributes

        /// <summary>
        /// Get or set the port name.
        /// </summary>
        public string PortName
        {
            get { return _StepperControllerPort.PortName; }
            set { _StepperControllerPort.PortName = value; }
        }

        /// <summary>
        /// Get or set the serial baud rate.
        /// </summary>
        public int BaudRate
        {
            get { return _StepperControllerPort.BaudRate; }
            set { _StepperControllerPort.BaudRate = value; }
        }

        /// <summary>
        /// Get or set the parity check protocal.
        /// </summary>
        public Parity Parity
        {
            get { return _StepperControllerPort.Parity; }
            set { _StepperControllerPort.Parity = value; }
        }

        /// <summary>
        /// Get or set the standard length of data bits per byte.
        /// </summary>
        public int DataBits
        {
            get { return _StepperControllerPort.DataBits; }
            set { _StepperControllerPort.DataBits = value; }
        }

        /// <summary>
        /// Get or set the standard length of stop bits per byte.
        /// </summary>
        public StopBits StopBits
        {
            get { return _StepperControllerPort.StopBits; }
            set { _StepperControllerPort.StopBits = value; }
        }

        /// <summary>
        /// Get a bool indicating if the serial port is open.
        /// </summary>
        public bool IsOpen
        {
            get { return _StepperControllerPort.IsOpen; }
        }

        #endregion


        #region Channel Control Methods

        /// <summary>
        /// Open the stepper controller port.
        /// </summary>
        public void Open()
        {
            _StepperControllerPort.Open();
        }

        /// <summary>
        /// Close the stepper controller port.
        /// </summary>
        public void Close()
        {
            _StepperControllerPort.Close();
        }

        #endregion


        #region Operation Control Methods

        /// <summary>
        /// Rotate the stepper motor for specific number of steps, where 
        /// 512 steps is a full cycle of 360 degrees. The maximum number 
        /// of rotation will be 512 steps.
        /// </summary>
        /// <param name="steps">Number of steps to rotate.</param>
        /// <returns>
        /// A bool indicating if the command is sent successfully.
        /// </returns>
        public bool Rotate(int steps)
        {
            // Check the current stepper state
            if (!(_StepperState == StepperState.Initialized ||
                _StepperState == StepperState.Reset ||
                _StepperState == StepperState.RotationFinished))
            {
                return false;
            }
            
            // Check if the step number is valid
            if (steps <= 0 || steps > 512) return false;

            // Set the desired steps
            _DesireSteps = steps;

            // Set the stepper state as command sent
            _StepperState = StepperState.CommandSent;

            // Send the rotation command
            _StepperControllerPort.Write(steps.ToString());

            // Return success
            return true;
        }

        /// <summary>
        /// Reset the stepper.
        /// </summary>
        public void Reset()
        {
            _StepperControllerPort.Write("X");
        }

        #endregion
    }
}
