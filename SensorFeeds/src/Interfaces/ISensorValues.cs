using System;
using Microsoft.Maui.Devices.Sensors;
using System.Numerics;

namespace MauiSensorFeeds.Interfaces
{
    /// <summary>
    /// Full set of sensor readings with some calculated values.
    /// </summary>
    public interface ISensorValues
    {
        /// <summary>
        /// Kilometers or miles enum, defaults to kilometers if not set.
        /// Changing this should force recalculation.
        /// </summary>
        DistanceUnits UnitOfDistance { get; }

        /// <summary>
        /// Timestamp may be later than time of some measurements.
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// Location has its own timestamp
        /// </summary>
        Location Location { get; }
        // calculated from location. Location provides the speed and angle of travel.
        double Speed { get; }
        double AngleOfTravel { get; }
        double AccelByLocations { get; }

        Quaternion Orientation { get; }
        DateTime OrientationTimestamp { get; }
        // calculated from orientation:
        /// <summary>
        /// Roll, Pitch, Yaw are in degrees, from Orientation.
        /// </summary>
        float Roll { get; }
        float Pitch { get; }
        float Yaw { get; }

        /// <summary>
        /// Acceleration unit of meaasure is in Gravity, G (1G = 0.9887 meters/sec/sec)
        /// but relative to the device's actual position.
        /// </summary>
        Vector3 Acceleration { get; }
        DateTime AccelerationTimestamp { get; }

        /// <summary>
        /// Calculated from orientation plus acceleration, absolute acceleration
        /// is acceleration rotated by orientation, oriented to the absolute frame 
        /// of reference of the earth.
        /// Unit of meaasure is in Gravity, G (1G = 0.9887 meters/sec/sec)
        /// Z direction is usually gravitational force.
        /// </summary>
        Quaternion AbsoluteAcceleration { get; }
        /// <summary>
        /// Horizontal acceleration is Pathagorean of X and Y direction.
        /// Units are either kph/sec or mph/sec depending on UnitOfDistance setting.
        /// </summary>
        float HorizontalAcceleration { get; }
        float AbsoluteAcceleration_X { get; }
        float AbsoluteAcceleration_Y { get; }
        float AbsoluteAcceleration_Z { get; }
    }
}
