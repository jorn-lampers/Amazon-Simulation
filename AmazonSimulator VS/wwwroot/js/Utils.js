var UTILS =
{
    WrapModel: function ( model, name = "NO_NAME" )
    {
        model.name = name;
        let box = new THREE.Box3().setFromObject( model );

        model.size = new THREE.Vector3();
        box.getSize( model.size );

        model.centerOffset = new THREE.Vector3();
        box.getCenter( model.centerOffset );
        model.centerOffset.add( model.position.clone().negate() );

        model.updatePosition = function ( pos )
        {
            model.position.set( pos.x, pos.y, pos.z );
            model.updateHitbox();
        }

        model.getCenter = function ()
        {
            let boxCenter = new THREE.Vector3( 0, 0, 0 );

            boxCenter.add( this.centerOffset );
            boxCenter.add( this.position );

            return boxCenter;
        }

        model.updateHitbox = function ()
        {
            let center = this.getCenter();

            this.hitbox = new THREE.Box3();
            this.hitbox.setFromCenterAndSize( center, this.size );

            //this.hitbox.applyMatrix4( model.matrixWorld );

            this.hitboxDisplay = new THREE.Box3Helper( this.hitbox, 0xff0000 );
            this.hitboxDisplay.updateMatrixWorld( true );
        }

        model.updateHitbox();
    },

    FindIntersects: function(raycaster, objects)
    {
        let intersects = [];
        let distances = [];

        let ray = raycaster.ray;

        for (let i = 0; i < objects.length; i++)
        {
            let obj = Object.values( objects )[i];
            let hb = obj.hitbox;

            let intersection = ray.intersectBox( hb );

            if ( intersection != null )
            {
                let distance = intersection.distanceTo( ray.origin );
                if(isNaN(distance)) continue;

                // If cursor is projected on object's hitbox add obj to current intersects
                intersects.push( obj );

                distances.push( distance );
            } 

        }

        return { 
            objects: intersects, 
            distances: distances
        };
    },

    NearestIndex: function( intersects )
    {
        let nearestIndex = 0;
        for ( let i = 1; i < intersects.distances.length; i++ )
        {
            if(intersects.distances[nearestIndex] > intersects.distances[i]) 
            {
                nearestIndex = i;
            }
        }

        return nearestIndex;
    },

    TS: performance.now(),
    Timer: function()
    {
        let diff = performance.now() - UTILS.TS;
        UTILS.TS = performance.now();
        return diff;
    }

}

