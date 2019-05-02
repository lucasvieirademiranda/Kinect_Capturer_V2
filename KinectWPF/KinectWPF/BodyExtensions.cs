using System;
using System.Linq;
using Microsoft.Kinect;
using LightBuzz.Vitruvius;

namespace KinectWPF
{
    public static class BodyExtensions
    {
        public static string ToUpperBodyCSV(this Body body)
        {
            var result = string.Empty;

            var upperJoints = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 16, 20, 21, 22, 23, 24 };

            if (body != null && body.IsTracked)
            {
                foreach (var upperJoint in upperJoints)
                {
                    var jointType = (JointType) upperJoint;

                    var joint = body.Joints[jointType];
                    var jointOrientation = body.JointOrientations[jointType];

                    var jointName = Enum.GetName(typeof(JointType), jointType);
                    var cameraPosition = joint.Position;
                    var colorPosition = joint.Position.ToPoint(Visualization.Color);
                    var depthPosition = joint.Position.ToPoint(Visualization.Depth);
                    var orientation = jointOrientation.Orientation;

                    result += string.Format(
                        "{0};{1};{2};{3};{4}\n", 
                        jointName, 
                        Math.Round(colorPosition.X), 
                        Math.Round(colorPosition.Y), 
                        0, 
                        0);

                    result += string.Format(
                        "{0};{1};{2};{3};{4}\n", 
                        jointName, 
                        Math.Round(depthPosition.X), 
                        Math.Round(depthPosition.Y), 
                        0, 
                        0);

                    result += string.Format(
                        "{0};{1};{2};{3};{4}\n", 
                        jointName, 
                        cameraPosition.X, 
                        cameraPosition.Y, 
                        cameraPosition.Z, 
                        0);

                    result += string.Format(
                        "{0};{1};{2};{3};{4}\n", 
                        jointName, 
                        orientation.X, 
                        orientation.Y, 
                        orientation.Z, 
                        orientation.W);
                }
            }
            else
            {
                foreach (var upperJoint in upperJoints)
                {
                    var jointType = (JointType) upperJoint;

                    var jointName = Enum.GetName(typeof(JointType), jointType);

                    result += string.Format("{0};{1};{2};{3};{4}\n", jointName, 0, 0, 0, 0);

                    result += string.Format("{0};{1};{2};{3};{4}\n", jointName, 0, 0, 0, 0);

                    result += string.Format("{0};{1};{2};{3};{4}\n", jointName, 0, 0, 0, 0);

                    result += string.Format("{0};{1};{2};{3};{4}\n", jointName, 0, 0, 0, 0);

                }
            }

            return result;
        }
    }
}
