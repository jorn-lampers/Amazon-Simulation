using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Models
{
    public interface EntityEnvironmentInfoProvider
    {
        List<CollidablePathfindingEntity> GetCollisions(CollidablePathfindingEntity footprint);
        List<CollidablePathfindingEntity> GetCollisions(LineSegment2 segment);
        List<CollidablePathfindingEntity> GetCollisions(ICollection<LineSegment2> segment);



    }
}
