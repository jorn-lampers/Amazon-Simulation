using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public interface EntityEnvironmentInfoProvider
    {
        List<CollidablePathfindingEntity> GetCollisions(CollidablePathfindingEntity footprint);
        Node RobotQueueStart { get; }
        Node RobotTruckExit { get; }
    }
}
