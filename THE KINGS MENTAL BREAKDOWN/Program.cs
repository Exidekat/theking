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
    public enum FlagState
    {
        TURNING,
        IDLE
    }
    public enum DriveState
    {
        PRECISE, //slower, lower gain
        STRONGER //faster, higher gain 
    }
    public enum ElevatorState
    {
        IDLE,
        RISE,
        LOWER,
        ERROR
    }

    public class Program
    {
        static Stopwatch stopwatch = new Stopwatch();

        private static ElevatorState elevatorState = ElevatorState.ERROR;
        private static DriveState driveState = DriveState.PRECISE;  //creates a variable with a Drivestate type, makes the starting value PRECISE
        private static FlagState flagState = FlagState.IDLE; //creates a variable with a FlagState type, makes the starting value IDLE

        private static float turnAxis; //x-axis of stick 
        private static float forwardAxis; // y-axis of stick 
        private static float finalLeftTalonValue; //final motor value of left drive talon after conidering gain, turn, and forward  
        private static float finalRightTalonValue; //final motor value of right drive talon after conidering gain, turn, and forward 

        private static float forwardGain; // strength of forward
        private static float turnGain; //strength of turn

        public static void Main()
        {
            stopwatch.Start();
            var t1 = new Thread(Everything);
            t1.Start();
            var t2 = new Thread(PrintTime);
            t2.Start();
        }

        static void Everything()
        {
            float time = stopwatch.Duration; 

            /* Creating our gamepad */
            GameController gamepad1 = new GameController(new CTRE.Phoenix.UsbHostDevice(0));

            //creating TalonSRX objects(dirvetrain, climber or elevator, and flag raise)
            TalonSRX leftDriveTalon = new TalonSRX(0);
            TalonSRX rightDriveTalon = new TalonSRX(1);
            TalonSRX climbTalon = new TalonSRX(2);
            TalonSRX flagTalon = new TalonSRX(3);

            //loops forever 
            while (true)
            {
                rightDriveTalon.ConfigFactoryDefault();
                leftDriveTalon.ConfigFactoryDefault();
                climbTalon.ConfigFactoryDefault();
                flagTalon.ConfigFactoryDefault();

                /* this is checked periodically. Recommend every 20ms or faster */
                if (gamepad1.GetConnectionStatus() == CTRE.Phoenix.UsbDeviceConnection.Connected)
                {
                    /* print axis value */
                    Debug.Print("axis:" + gamepad1.GetAxis(1));

                    /* allow motor control */
                    CTRE.Phoenix.Watchdog.Feed();

                }

                // not finished autonomous period 
                if (time < 30)//time < 30
                {
                    Debug.Print("Auton is attempting to E");

                     leftDriveTalon.Set(ControlMode.PercentOutput, 0.5); //sets left drive talon to 50%
                     rightDriveTalon.Set(ControlMode.PercentOutput, 0.5); //sets right drive talon to 50%


                     /*  Amount of time to wait before repeating the loop */
                     Thread.Sleep(10);

                    Debug.Print("Auton ending, E complete.");
                    leftDriveTalon.Set(ControlMode.PercentOutput, 0.0); //sets left drive talon to 0%
                    rightDriveTalon.Set(ControlMode.PercentOutput, 0.0); //sets right drive talon to 0%
                }


                else // teleop
                {
                    /* NOTE TO HAYDON: ADD THE ACTUAL CONTROLLER AXISES IDS
                    stick 1 x axis: id 2?
                    stick 1 y axis: id 1?
                    stick 2 x axis: id
                    stick 2 y axis: id
                    */


                    //determine inputs
                    forwardAxis = gamepad1.GetAxis(1);
                    turnAxis = gamepad1.GetAxis(2);

                    //accounting for forward and turn b4 Eing into the talons
                    finalLeftTalonValue = (forwardAxis * forwardGain) + (turnAxis * turnGain);
                    finalRightTalonValue = (forwardAxis * forwardGain) - (turnAxis * turnGain);

                    /* pass motor value to talons */
                    leftDriveTalon.Set(ControlMode.PercentOutput, finalLeftTalonValue);
                    rightDriveTalon.Set(ControlMode.PercentOutput, finalRightTalonValue);

                    /* the climby controls NOTE TO HAYDON: ADD THE ACTUAL CONTROLLER BUTTON IDS*/
                    if (gamepad1.GetButton(6808099))
                    {
                        elevatorState = ElevatorState.RISE;
                    }
                    else if (gamepad1.GetButton(9897))
                    {
                        elevatorState = ElevatorState.LOWER;
                    }
                    else
                    {
                        elevatorState = ElevatorState.IDLE;
                    }

                    //if the button is pressed the drivestate will switch 
                    if (gamepad1.GetButton(1423) && driveState == DriveState.PRECISE)//stick button
                    {
                        driveState = DriveState.STRONGER;
                    }
                    else if (gamepad1.GetButton(1423) && driveState == DriveState.STRONGER)
                    {
                        driveState = DriveState.PRECISE;
                    }


                    if (gamepad1.GetButton(1232) && flagState == FlagState.IDLE)
                    {
                        flagState = FlagState.TURNING;
                    }
                    else if (gamepad1.GetButton(1232) && flagState == FlagState.TURNING)
                    {
                        flagState = FlagState.IDLE;
                    }

                }

                /* elevator state machine garbo */
                switch (elevatorState)
                {
                    case ElevatorState.IDLE:
                        Debug.Print("nothing");
                        climbTalon.Set(ControlMode.PercentOutput, 0.0); //sets the climber talon to 0%
                        break;
                    case ElevatorState.RISE:
                        Debug.Print("rising");
                        climbTalon.Set(ControlMode.PercentOutput, 0.5); //sets the climber talon to 50%
                        break;
                    case ElevatorState.LOWER:
                        Debug.Print("lowering");
                        climbTalon.Set(ControlMode.PercentOutput, -0.5); //sets the climber talon to -50%
                        break;
                    case ElevatorState.ERROR:
                        Debug.Print("ElevatorState error");
                        break;
                    default:
                        Debug.Print("very bad error");
                        break;
                }

                //drive gain state machine 
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

                //flag state machine 
                switch (flagState)
                {
                    case FlagState.TURNING:
                        flagTalon.Set(ControlMode.PercentOutput, 0.5);
                        Debug.Print("turning");
                        break;
                    case FlagState.IDLE:
                        flagTalon.Set(ControlMode.PercentOutput, 0.0);
                        Debug.Print("idle");
                        break;
                    default:
                        Debug.Print("FlagState error");
                        break;
                }


            }
        }

        static void PrintTime()
        {
            bool timer = true;
            while (timer)
            {
                Thread.Sleep(1000);
                float time = stopwatch.Duration;
                Debug.Print("time: " + time );
                if (time >= 10)
                {
                    timer = false;
                }
            }
        }
    }
}
