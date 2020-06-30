using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace AmazonSimulator_VS
{
    public static class Constants
    {
        public static readonly int SIM_TPS = 60;

        public static readonly float RobotAcceleration = 0.075f;
        public static readonly float TruckAcceleration = 0.05f;

        public static readonly float RobotSpeed = 10f;
        public static readonly float TruckSpeed = 15f;

        public static readonly Vector3[] RobotSpawns = {
            new Vector3(-31f, 0f, 18f),
            new Vector3(31f, 0f, 18f),

            new Vector3(-31f, 0f, 12f),
            new Vector3(31f, 0f, 12f),

            new Vector3(-31f, 0f, 6f),
            new Vector3(31f, 0f, 6f),

            new Vector3(-31f, 0f, 0f),
            new Vector3(31f, 0f, 0f)
        };

        public static readonly Vector3 TruckSpawn = new Vector3(-7.75f, -1.5f, 150f);
        public static readonly Vector3 TruckStop = new Vector3(-7.75f, -1.5f, 42.5f);
        public static readonly Vector3 TruckDespawn = new Vector3(-7.75f, -1.5f, 150f);

        public static readonly Vector3 RobotEnterTruck = new Vector3(-8.5f, 0f, 22f);
        public static readonly Vector3 RobotExitTruck = new Vector3(-7.0f, 0f, 22f);

        public static readonly float LaneWidth = 2.0f;

        public static readonly Vector3[] GraphNodePositions =
        {
            new Vector3(-30f, 0.0f, 18f),
            new Vector3(+0.0f, 0.0f, 18f),
            new Vector3(+30f, 0.0f, 18f),

            new Vector3(-30f, 0.0f, 12f),
            new Vector3(+0.0f, 0.0f, 12f),
            new Vector3(+30f, 0.0f, 12f),

            new Vector3(-30f, 0.0f, 6.0f),
            new Vector3(+0.0f, 0.0f, 6.0f),
            new Vector3(+30f, 0.0f, 6.0f),

            new Vector3(-30f, 0.0f, 0.0f),
            new Vector3(+0.0f, 0.0f, 0.0f),
            new Vector3(+30f, 0.0f, 0.0f)
        };

        public static readonly Edge[] GraphEdges =
        {
            new Edge(GraphNodePositions[0], GraphNodePositions[1], LaneWidth),
            new Edge(GraphNodePositions[1], GraphNodePositions[2], LaneWidth),

            new Edge(GraphNodePositions[1], GraphNodePositions[4], LaneWidth),
            new Edge(GraphNodePositions[3], GraphNodePositions[4], LaneWidth),

            new Edge(GraphNodePositions[4], GraphNodePositions[5], LaneWidth),
            new Edge(GraphNodePositions[4], GraphNodePositions[7], LaneWidth),

            new Edge(GraphNodePositions[6], GraphNodePositions[7], LaneWidth),
            new Edge(GraphNodePositions[7], GraphNodePositions[8], LaneWidth),

            new Edge(GraphNodePositions[7], GraphNodePositions[10], LaneWidth),
            new Edge(GraphNodePositions[9], GraphNodePositions[10], LaneWidth),
            new Edge(GraphNodePositions[10], GraphNodePositions[11], LaneWidth)
        };

        public static readonly int StoragePlotLength = 5;
        public static readonly int StoragePlotWidth = 2;

        public static readonly Vector3[] StoragePositions =
        {
            new Vector3(-4f, 0f, 3f),
            new Vector3(+4f, 0f, 3f),

            new Vector3(-4f, 0f, 9f),
            new Vector3(+4f, 0f, 9f),

            new Vector3(-4f, 0f, 15f),
            new Vector3(+4f, 0f, 15f),

            new Vector3(-12f, 0f, 3f),
            new Vector3(+12f, 0f, 3f),

            new Vector3(-12f, 0f, 9f),
            new Vector3(+12f, 0f, 9f),

            new Vector3(-12f, 0f, 15f),
            new Vector3(+12f, 0f, 15f),            
                
            new Vector3(-20f, 0f, 3f),
            new Vector3(+20f, 0f, 3f),

            new Vector3(-20f, 0f, 9f),
            new Vector3(+20f, 0f, 9f),

            new Vector3(-20f, 0f, 15f),
            new Vector3(+20f, 0f, 15f),

            new Vector3(-28f, 0f, 3f),
            new Vector3(+28f, 0f, 3f),

            new Vector3(-28f, 0f, 9f),
            new Vector3(+28f, 0f, 9f),

            new Vector3(-28f, 0f, 15f),
            new Vector3(+28f, 0f, 15f)
        };
    }

}
