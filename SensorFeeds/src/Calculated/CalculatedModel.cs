using MauiSensorFeeds.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.Calculated
{
    public partial class CalculatedModel
    {
        public static CalculatedModel GetModel()
        {
            if (model == null)
                model = new CalculatedModel(SensorFeeds.GetSensorFeeds());
            return model;
        }

        #region sensors

#pragma warning disable CS8618 // Non-nullable field must contain a non-nullable value exiting ctor.
#pragma warning disable CS8603 // Possible null reference return.
        private static CalculatedModel model;
        protected SensorFeeds Feeds => SensorFeeds.GetSensorFeeds();
        protected IGeolocation GeolocationSource => Feeds.GeolocationSource;
        protected IOrientationSensor OrientationSensorSource => Feeds.OrientationSensorSource;
        protected IAccelerometer AccelerometerSource => Feeds.AccelerometerSource;
        protected ICompass CompassSource => Feeds.CompassSource;
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8618 // Non-nullable field must contain a non-nullable value exiting ctor.

        #endregion

        private CalculatedModel(SensorFeeds sensorFeeds)
        {
            CalculatedModel.model = this;
        }

        public virtual void Recalculate()
        {
            CalculateMeasures();
            CalculatedValuesChanged?.Invoke(null, new CalculatedModelChangedEventArgs(this));
        }

        #region Event handlers
        public event EventHandler<CalculatedModelChangedEventArgs> ? CalculatedValuesChanged;

        internal void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            if (e.Reading.Acceleration.Length() != 0)
                acceleration = e.Reading.Acceleration;
            Recalculate();
        }
        internal void GeolocationSource_LocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        {
            if (e.Location.Latitude + e.Location.Longitude != 0)
                location = e.Location;
            Recalculate();
        }
        internal void OrientationSensorSource_ReadingChanged(object? sender, OrientationSensorChangedEventArgs e)
        {
            if (e.Reading.Orientation.Length() != 0)
                orientation = e.Reading.Orientation;
            Recalculate();
        }
        internal void Compass_ReadingChanged(object? sender, CompassChangedEventArgs e)
        {
            if (e.Reading.HeadingMagneticNorth != double.NaN)
                headingFromNorth = (float) e.Reading.HeadingMagneticNorth;
            Recalculate();
        }
        #endregion

        #region constants and labels
        public static DistanceUnits UnitsOfDistance { get; set; }

        public static double Rads2degs => 180 / Math.PI;

        public static double Grav2DistancePHPS()
        {
            if (UnitsOfDistance == DistanceUnits.Miles)
                return 21.93759D;  // miles per hour per second, 9.807 meters/sec/sec *  0.00062137D miles / meter * 3600 seconds / hour
            else
                return 35.305D;    // kilometers per hour per second, 9.807 meters/sec/sec *  0.001D km / meter * 3600 seconds / hour
        }

        public static float AccelBrakeLimit => (float) Grav2DistancePHPS() / 3;
        #endregion

        #region measures

        private Quaternion orientation = new Quaternion();
        private Vector3 acceleration = new Vector3();
        private Location location = new Location();
        private float headingFromNorth = float.NaN;

        public Quaternion Orientation => orientation;

        public Vector3 Acceleration => acceleration;

        public Location Location => location;

        public float HeadingFromNorth => headingFromNorth;

        /// <summary>
        /// Gets the horizontal total acceleration (or decelleration) in G's by removing the gravity component.
        /// This is only meaningful at the Earth's surface where gravity is 1 G.
        /// </summary>
        public double HorizontalAccel
        {
            get
            {
                var lsq = Acceleration.LengthSquared();
                return lsq > 1 ? Math.Sqrt(lsq - 1) : 0;
            }
        }

        /*
        /// <summary>
        /// Estimate the acceleration rate from loc2 to loc3 versus from loc1 to loc2
        /// </summary>
        public double AccelRate
        {
            get
            {
                double lastAccelRate = 0;
                lock (samplesLock)
                {
                        Location lastLoc1;
                        Location lastLoc2;
                        Location lastLoc3;
                        long ticksInterval1;
                        long ticksInterval2;
                        double distance1;
                        double distance2;
                        lastLoc1 = sensorReadings.Values[BufferCount - 1];
                        lastLoc2 = sensorReadings.Values[BufferCount - 2];
                        lastLoc3 = sensorReadings.Values[BufferCount - 3];
                        ticksInterval1 = lastLoc1.Timestamp.Ticks - lastLoc2.Timestamp.Ticks;
                        ticksInterval2 = lastLoc2.Timestamp.Ticks - lastLoc3.Timestamp.Ticks;
                        distance1 = lastLoc1.CalculateDistance(lastLoc2, DistanceUnits.Miles);
                        distance2 = lastLoc2.CalculateDistance(lastLoc3, DistanceUnits.Miles);
                        var speed1 = distance1 / (ticksInterval1 * 10000 * 3600);  // mph
                        var speed2 = distance2 / (ticksInterval2 * 10000 * 3600); // mph
                        var avgTime = (lastLoc1.Timestamp.Ticks - lastLoc2.Timestamp.Ticks) * 10000 / 2; // secs
                        lastAccelRate = (speed2 - speed1) / avgTime; // mph per second
                }
                return lastAccelRate;
            }
        }
        */
        public double AngleOfTravel => Location.Course.GetValueOrDefault(double.NaN);
        public double Speed => Location.Speed ?? 0;

        private Quaternion absoluteAcceleration;
        private float roll;
        private float pitch;
        private float yaw;
        public double AccelFromLocations { get; private set; }
        public Quaternion AbsoluteAcceleration { get { return absoluteAcceleration; } }
        public double HorizAccel => Math.Sqrt(
                    (absoluteAcceleration.X * absoluteAcceleration.X) +
                    (absoluteAcceleration.Y * absoluteAcceleration.Y));
        public float Acceleration_X => (float)Math.Round((decimal)(Acceleration.X * Grav2DistancePHPS()), 3);
        public float Acceleration_Y => (float)Math.Round((decimal)(Acceleration.Y * Grav2DistancePHPS()), 3);
        public float Acceleration_Z => (float)Math.Round((decimal)(Acceleration.Z * Grav2DistancePHPS()), 3);

        public float Orientation_Roll => (float)Math.Round(roll * Rads2degs, 3);
        public float Orientation_Pitch => (float)Math.Round(pitch * Rads2degs, 3);
        public float Orientation_Yaw => (float)Math.Round(yaw * Rads2degs, 3);
        public float Orientation_X => (float)Math.Round(Orientation.X, 3);
        public float Orientation_Y => (float)Math.Round(Orientation.Y, 3);
        public float Orientation_Z => (float)Math.Round(Orientation.Z, 3);
        public float Orientation_W => (float)Math.Round(Orientation.W, 3);

        public float AbsAcceleration_X => (float)Math.Round(AbsoluteAcceleration.X * Grav2DistancePHPS(), 3);
        public float AbsAcceleration_Y => (float)Math.Round(AbsoluteAcceleration.Y * Grav2DistancePHPS(), 3);
        public float AbsAcceleration_Z => (float)Math.Round(AbsoluteAcceleration.Z * Grav2DistancePHPS(), 3);

        public string AccelLabel => (UnitsOfDistance == DistanceUnits.Miles ? "mph /sec" : "kph /sec");
        public string SpeedLabel => (UnitsOfDistance == DistanceUnits.Miles ? "mph" : "kph");

        #endregion

        protected virtual void CalculateYawPitchRoll()
        {
            if (this.Orientation.Length() == 0 ||
                this.Acceleration.Length() == 0)
            {
                return;
            }

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


        }

        public virtual void CalculateMeasures()
        {
            CalculateYawPitchRoll();
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


            // We will rotate Qaccel by the orientation q.

            // Treat acceleration vector as a point p.

            // The conjugate of the orientation:
            Quaternion Orient_Conj = Quaternion.Conjugate(Orientation);  // (eq. 13)

            // Convert the acceleration point into a quaternion
            Quaternion p = new Quaternion(Acceleration, 0);

            Quaternion p2 = Quaternion.CreateFromAxisAngle(Acceleration, 0);


            // Perform active rotation.
            // For active rotation:    p' = q−1 * p * q                   // (eq. 15a)
            // var Qa = Quaternion.Multiply(Orient_Inv, Qaccel);
            // absoluteAcceleration = Quaternion.Multiply(Qa, Orientation);

            // Perform passive rotation.
            // For passive rotation:    p' = q * p * q−1                  // (eq. 15b)
            var Qa = Quaternion.Multiply(Orientation, p);
            var QabsoluteAccel = Quaternion.Multiply(Qa, Orient_Conj);
            // convert quat. to vector
            absoluteAcceleration = QabsoluteAccel;
            var VabsoluteAccel = new Vector3(QabsoluteAccel.X, QabsoluteAccel.Y, QabsoluteAccel.Z);

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
