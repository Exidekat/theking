using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;

namespace THE_KINGS_MENTAL_BREAKDOWN
{

    public enum ElevatorState
    {
        NOTHING,
        RISE,
        LOWER,
        FUCK
    }

    public class Program
    {
   
        private static bool autonActive;
        private static ElevatorState elevatorState = ElevatorState.FUCK;

        private static float forwardAxis;
        private static float turnAxis;
        private static float finalLeftTalonValue;
        private static float finalRightTalonValue;

        //these two multiplier decide the stronkgth of turning and forward drive.
        private const float forwardGain = 0.5f;
        private const float turnGain = 0.25f;

        public static void Main()
        {
            /* Do auton????? Maybe????? EE????? */
            autonActive = true;

            /* Creating our gamepad */
            GameController veryBadController = new GameController(new CTRE.Phoenix.UsbHostDevice(0));

            /* Talon defining. */
            TalonSRX leftDriveTalon = new TalonSRX(0);
            TalonSRX rightDriveTalon = new TalonSRX(1);
            TalonSRX climbyTalon = new TalonSRX(2);

            /* simple counter to print and watch using the debugger */
            int counter = 0;

            /* This loops everything inside forever. So, everything in the loop just... keeps going... assuming it works */
            while (true)
            {
                /* this is checked periodically. Recommend every 20ms or faster */
                if (veryBadController.GetConnectionStatus() == CTRE.Phoenix.UsbDeviceConnection.Connected)
                {
                    /* print axis value */
                    Debug.Print("axis:" + veryBadController.GetAxis(1));

                    /* allow motor control */
                    CTRE.Phoenix.Watchdog.Feed();

                }

                // Awfully amazing Auton
                if (autonActive == true)
                {
                    Debug.Print("Auton is attempting to E");

                    leftDriveTalon.Set(ControlMode.PercentOutput, 0.5); //sets left drive talon to 50%
                    rightDriveTalon.Set(ControlMode.PercentOutput, 0.5); //sets right drive talon to 50%


                    if (counter >= 600)
                    {
                        Debug.Print("Auton ending, E complete.");
                        leftDriveTalon.Set(ControlMode.PercentOutput, 0.0); //sets left drive talon to 0%
                        rightDriveTalon.Set(ControlMode.PercentOutput, 0.0); //sets right drive talon to 0%
                        autonActive = false;
                    };
                }
                else // Less awfully amazing teleop
                {
                    /* NOTE TO HAYDON: ADD THE ACTUAL CONTROLLER AXISES IDS
                    stick 1 x axis: id 2?
                    stick 1 y axis: id 1?
                    stick 2 x axis: id
                    stick 2 y axis: id
                    */


                    //determine inputs
                    forwardAxis = veryBadController.GetAxis(1);
                    turnAxis = veryBadController.GetAxis(2);

                    //accounting for forward and turn b4 Eing into the talons
                    finalLeftTalonValue = (forwardAxis * forwardGain) + (turnAxis * turnGain);
                    finalRightTalonValue = (forwardAxis * forwardGain) - (turnAxis * turnGain);

                    /* pass motor value to talons */
                    leftDriveTalon.Set(ControlMode.PercentOutput, finalLeftTalonValue);
                    rightDriveTalon.Set(ControlMode.PercentOutput, finalRightTalonValue);

                    /* the climby controls NOTE TO HAYDON: ADD THE ACTUAL CONTROLLER BUTTON IDS*/
                    if (veryBadController.GetButton(6808099))
                    {
                        elevatorState = ElevatorState.RISE;
                    } else if (veryBadController.GetButton(9897))
                    {
                        elevatorState = ElevatorState.LOWER;
                    } else
                    {
                        elevatorState = ElevatorState.NOTHING;
                    }

                }

                /* elevator state machine garbo */
                switch (elevatorState)
                {
                    case ElevatorState.NOTHING:
                        Debug.Print("the elevator aint doin shit");
                        climbyTalon.Set(ControlMode.PercentOutput, 0.0); //sets the climber talon to 0%
                        break;
                    case ElevatorState.RISE:
                        Debug.Print("OH LOARD HE RISEN");
                        climbyTalon.Set(ControlMode.PercentOutput, 0.5); //sets the climber talon to 50%
                        break;
                    case ElevatorState.LOWER:
                        Debug.Print("OH LOARD HE FALLEN");
                        climbyTalon.Set(ControlMode.PercentOutput, -0.5); //sets the climber talon to -50%
                        break;
                    case ElevatorState.FUCK:
                        Debug.Print("what fuck");
                        break;
                    default:
                        Debug.Print("WHAT FUCK??????///??");
                        break;
                }

                
                
                /* print the three analog inputs as three columns */
                Debug.Print("Counter Value: " + counter);

                /* increment counter */
                ++counter; /* try to land a breakpoint here and hover over 'counter' to see it's current value.  Or add it to the Watch Tab */

                /* Amount of time to wait before repeating the loop */
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
