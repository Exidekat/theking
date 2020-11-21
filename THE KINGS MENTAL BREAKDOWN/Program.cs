using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix;

namespace THE_KINGS_MENTAL_BREAKDOWN
{
    public enum DriveState
    {
        PRECISE, //slower, lower gain
        STRONGER //faster, higher gain 
    }
    public enum ClimbState
    {
        IDLE,
        RISE,
        LOWER,
        ERROR
    }

    public class Program
    {
        private static Stopwatch stopwatch = new Stopwatch();

        private static ClimbState climbState = ClimbState.ERROR; //creates a variable with a ElevatorState type, makes the starting value ERROR 
        private static DriveState driveState = DriveState.PRECISE;  //creates a variable with a DriveState type, makes the starting value PRECISE

        //creating a GameController object
        private static GameController gamepad1 = new GameController(new CTRE.Phoenix.UsbHostDevice(0));

        //creating TalonSRX objects(drivetrain(right and left), climber or elevator, and flag raise)
        private static TalonSRX leftDriveTalon = new TalonSRX(0);
        private static TalonSRX rightDriveTalon = new TalonSRX(1);
        private static TalonSRX climbTalon = new TalonSRX(2);

        private static float turnAxis; //x-axis of stick 
        private static float forwardAxis; // y-axis of stick 
        private static float finalLeftTalonValue; //final motor value of left drive talon after conidering gain, turn, and forward  
        private static float finalRightTalonValue; //final motor value of right drive talon after conidering gain, turn, and forward 

        private static float forwardGain; // strength of forward
        private static float turnGain; //strength of turn
        private static float climbGain = 0.5f; //strength climber

        public static void Main()
        {
            //runs both PrintTime and Everything
            stopwatch.Start();
            var t1 = new Thread(Everything);
            t1.Start();
            var t2 = new Thread(PrintTime);
            t2.Start();
        }

        static void Everything()
        {
            // creates a variable for the time(seconds)
            float time = stopwatch.Duration; 

            // prevents unexpected behavior 
            rightDriveTalon.ConfigFactoryDefault();
            leftDriveTalon.ConfigFactoryDefault();
            climbTalon.ConfigFactoryDefault();

            // not finished autonomous period 
            if (time < 30)
            {
                Debug.Print("Auton is attempting to E");

                leftDriveTalon.Set(ControlMode.PercentOutput, 0.5); //sets left drive talon to 50%
                rightDriveTalon.Set(ControlMode.PercentOutput, 0.5); //sets right drive talon to 50%

                Thread.Sleep(10);

                Debug.Print("Auton ending, E complete.");
                leftDriveTalon.Set(ControlMode.PercentOutput, 0.0); //sets left drive talon to 0%
                rightDriveTalon.Set(ControlMode.PercentOutput, 0.0); //sets right drive talon to 0%
            }

            while (true)
            { 
                // print axis value ???
                Debug.Print("axis:" + gamepad1.GetAxis(1));

                // allows motor control 
                CTRE.Phoenix.Watchdog.Feed();

                // Teleoperated 
                // determine inputs
                forwardAxis = gamepad1.GetAxis(1);
                turnAxis = gamepad1.GetAxis(2);

                // accounting for forward and turn b4 Eing into the talons
                finalLeftTalonValue = (forwardAxis * forwardGain) + (turnAxis * turnGain);
                finalRightTalonValue = (forwardAxis * forwardGain) - (turnAxis * turnGain);

                // pass motor value to Talons 
                leftDriveTalon.Set(ControlMode.PercentOutput, finalLeftTalonValue);
                rightDriveTalon.Set(ControlMode.PercentOutput, finalRightTalonValue);

                 // Elevator controls 
                if (gamepad1.GetButton(6808099))
                {
                    climbState = ClimbState.RISE;
                }
                else if (gamepad1.GetButton(9897))
                {
                    climbState = ClimbState.LOWER;
                }
                else
                {
                    climbState = ClimbState.IDLE;
                }

                // if the button is pressed the drivestate will switch 
                if (gamepad1.GetButton(1423) && driveState == DriveState.PRECISE)// stick button
                {
                    driveState = DriveState.STRONGER;
                }
                else if (gamepad1.GetButton(1423) && driveState == DriveState.STRONGER)
                {
                    driveState = DriveState.PRECISE;
                }

                // elevator state machine
                switch (climbState)
                {
                    case ClimbState.IDLE:
                        Debug.Print("nothing");
                        climbTalon.Set(ControlMode.PercentOutput, 0.0); //sets the climber talon to 0%
                        break;
                    case ClimbState.RISE:
                        Debug.Print("rising");
                        climbTalon.Set(ControlMode.PercentOutput, 1 * climbGain); //sets the climber talon to 50%
                        break;
                    case ClimbState.LOWER:
                        Debug.Print("lowering");
                        climbTalon.Set(ControlMode.PercentOutput, -1 *  climbGain); //sets the climber talon to -50%
                        break;
                    case ClimbState.ERROR:
                        Debug.Print("ElevatorState error");
                        break;
                    default:
                        Debug.Print("very bad error");
                        break;
                }

                // drive gain state machine 
                switch (driveState)
                {
                    case DriveState.PRECISE:
                        forwardGain = 0.3f;
                        turnGain = 0.1f;
                        Debug.Print("precise");
                        break;
                    case DriveState.STRONGER:
                        forwardGain = 0.5f;
                        turnGain = 0.25f;
                        Debug.Print("stronger");
                        break;
                    default:
                        Debug.Print("DriveState error");
                        break;
                }
            }
        }

        //prints the time in seconds 
        static void PrintTime()
        {
            bool timer = true;
            while (timer)
            {
                Thread.Sleep(1000);
                float time = stopwatch.Duration;
                Debug.Print("time: " + time );
                if (time >= 200)
                {
                    timer = false;
                }
            }
        }
    }
}
