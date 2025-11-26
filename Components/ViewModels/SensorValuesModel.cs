using EasyBraking.Components.Models;
using EasyBraking.Components.Services;
using System.Numerics;

namespace EasyBraking.Components.ViewModels
{
    public class SensorValuesModel
    {
        private Quaternion absoluteAcceleration;
        private Location location = new Location();
        private float roll;
        private float pitch;
        private float yaw;
        private SensorManagerModel sensorManager;
        public Location Location => this.location;
        public int LocationPoints => sensorManager.LocationPoints;
        public Quaternion Orientation { get; set; }
        public Vector3 Acceleration { get; set; }
        public double Speed => this.location.Speed ?? 0D;
        public double AngleOfTravel { get; private set; }
        public double AccelFromLocations { get; private set; }
        public Quaternion AbsoluteAcceleration { get { return absoluteAcceleration; } }
        public double HorizAccel { get; private set; }
        public float Acceleration_X => (float)(Acceleration.X * SensorManagerModel.Grav2DistancePHPS());
        public float Acceleration_Y => (float)(Acceleration.Y * SensorManagerModel.Grav2DistancePHPS());
        public float Acceleration_Z => (float)(Acceleration.Z * SensorManagerModel.Grav2DistancePHPS());
        public int AccelerationPoints => sensorManager.AccelerationPoints;

        public float Orientation_Roll => (float)(roll * SensorManagerModel.Rads2degs);
        public float Orientation_Pitch => (float)(pitch * SensorManagerModel.Rads2degs);
        public float Orientation_Yaw => (float)(yaw * SensorManagerModel.Rads2degs);
        public float Orientation_X => (float)(Orientation.X);
        public float Orientation_Y => (float)(Orientation.Y);
        public float Orientation_Z => (float)(Orientation.Z);
        public float Orientation_W => (float)(Orientation.W);
        public int OrientationPoints => sensorManager.OrientationPoints;

        public float AbsAcceleration_X => (float)(AbsoluteAcceleration.X * SensorManagerModel.Grav2DistancePHPS());
        public float AbsAcceleration_Y => (float)(AbsoluteAcceleration.Y * SensorManagerModel.Grav2DistancePHPS());
        public float AbsAcceleration_Z => (float)(AbsoluteAcceleration.Z * SensorManagerModel.Grav2DistancePHPS());

        public float AccelBrakeLimit => (float)SensorManagerModel.Grav2DistancePHPS() / 3;

        public string AccelLabel => (SettingsMgr.Service.GetDistanceUnit() == DistanceUnits.Miles ? "mph /sec" : "kph /sec");
        public string SpeedLabel => (SettingsMgr.Service.GetDistanceUnit() == DistanceUnits.Miles ? "mph" : "kph");

        public SensorValuesModel(SensorManagerModel mgr)
        {
            sensorManager = mgr;
            GetSensorReadings();
        }

        public void GetSensorReadings()
        {
            this.location = sensorManager.Location;
            this.Orientation = sensorManager.Orientation;
            this.Acceleration = sensorManager.Acceleration;
            this.absoluteAcceleration = new Quaternion();
            this.AngleOfTravel = sensorManager.AngleOfTravel;
            this.AccelFromLocations = sensorManager.AccelFromLocations;
            CalculateMeasures();
        }

        public void CalculateMeasures()
        {
            if (this.Orientation.Length() == 0 ||
                this.Acceleration.Length() == 0)
            {
                return;
            }
            // The device's attitude is represented as a quaternion with the scalar last: [qx, qy, qz, qw].
            // The reported quaternion represents the 3D orientation of the device relative to the east-north-up
            // coordinate frame (aka world frame). The east-north-up coordinate system is defined as a direct
            // orthonormal basis where:
            //      X points east and is tangential to the ground.
            //      Y points north and is tangential to the ground. When magnetic declination is available Y points
            //        to true north, otherwise to magnetic north.
            //      Z points in the opposite direction as gravity and is perpendicular to the ground.

            // Get absolute acceleration relative to "East-North-Up" X-Y-Z frame

            // Refer below to "Quaternion Rotation" in https://danceswithcode.net/engineeringnotes/quaternions/quaternions.html

            // Determine yaw, pitch and roll

            var q0 = Orientation.W;
            var q1 = Orientation.X;
            var q2 = Orientation.Y;
            var q3 = Orientation.Z;

            var q0s = q0 * q0;
            var q1s = q1 * q1;
            var q2s = q2 * q2;
            var q3s = q3 * q3;

            roll = (float)Math.Atan(2 * (q0 * q1 + q2 * q3) / (q0s - q1s - q2s + q3s));
            pitch = (float)Math.Asin(2 * (q0 * q2 - q1 * q3));
            yaw = (float)Math.Atan(2 * (q0 * q3 + q1 * q2) / (q0s + q1s - q2s + q3s));

            // We will rotate Qaccel by the orientation q.

            // Treat acceleration vector as a point p.

            // The inverse of the orientation:
            Quaternion Orient_Inv = Quaternion.Inverse(Orientation);  // (eq. 13)

            // Convert the acceleration point into a quaternion
            Quaternion p = new Quaternion(Acceleration, 0);

            // Perform active rotation.
            // For active rotation:    p' = q−1 * p * q                   // (eq. 15a)
            // var Qa = Quaternion.Multiply(Orient_Inv, Qaccel);
            // absoluteAcceleration = Quaternion.Multiply(Qa, Orientation);


            // Perform passive rotation.
            // For passive rotation:    p' = q * p * q−1                  // (eq. 15b)
            //
            var Qa = Quaternion.Multiply(Orientation, p);
            absoluteAcceleration = Quaternion.Multiply(Qa, Orient_Inv);

            if (Acceleration.Length() != 0)
            {
                // To check, the Z value of this absolute accel. should be GRAVITY
                // (unless braking or accelerating up/down a hill):
                if (absoluteAcceleration.Z < 1.1F && absoluteAcceleration.Z > 0.9F)
                {
                    // about right
                }
                else
                {
                    // throw new Exception("Rotation wrong");
                }

                // Another good reference is at https://www.anyleaf.org/blog/quaternions:-a-practical-guide

                // The horizontal part of absolute accel. vector is the real accel
                // or braking without gravity.

                this.HorizAccel = Math.Sqrt(
                    (absoluteAcceleration.X * absoluteAcceleration.X) +
                    (absoluteAcceleration.Y * absoluteAcceleration.Y));

                // According to Pathagorean Theorem, horizontal acceleration
                // where the vertical height is gravity (1) and the hypotenuse is
                // total acceleration (Length() of the accel. vector)
                double horizontalAccel1 = Math.Sqrt(Acceleration.LengthSquared() - 1);

                if (HorizAccel < horizontalAccel1 + 0.15D &&
                    HorizAccel > horizontalAccel1 - 0.15D)
                {
                    // about right
                }
                else
                {
                    // throw new Exception("Something is wrong with this.");
                }

            }

            // heading versus direction of travel
            // Android uses East-North-Up (ENU) as the world coordinate frame.
            // heading is degrees from north. A rotation should compare direction of travel to degrees from north.
            // North would be negative longitude change with no latitude change;
            // west would be positive latitude change with no longitude change.
            // vectorOfTravel.Item1 is latitude
            // vectorOfTravel.Item2 is longitude
            // vectorOfTravel.Item1 = 0 and vectorOfTravel.Item2 = -1  ==> NORTH  (   0 degrees relative to north ) == atan (  0 /  1 )
            // vectorOfTravel.Item1 = -1 and vectorOfTravel.Item2 = 0  ==> EAST   (  90 degrees relative to north ) == atan (  1 /  0 )
            // vectorOfTravel.Item1 = 0 and vectorOfTravel.Item2 = 1   ==> SOUTH  ( 180 degrees relative to north ) == atan (  0 / -1 )
            // vectorOfTravel.Item1 = 1 and vectorOfTravel.Item2 = 0   ==> WEST   ( 270 degrees relative to north ) == atan ( -1 /  0 )
            // If both are 0, no measurement (NaN).

        }


    }
}
