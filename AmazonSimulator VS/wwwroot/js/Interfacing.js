import * as THREE from '../lib/three.module.js';

export function Mouse( target )
{

    var position = new THREE.Vector2( 0, 0 );

    var updateSinceLastPoll = false;

    var target = target;

    var pollUpdate = function ( resetUpdate = true )
    {
        let update = updateSinceLastPoll;

        // Don't reset ___updateSinceLastPoll when caller explicitly requests this
        if ( !resetUpdate )
        {

            return update;

        }

        // By default, ___updateSinceLastPoll will be reverted to false after every call to this function
        else
        {

            updateSinceLastPoll = false;

        }

        return update;
    };

    var getPosition = function () {
        return position;
    };

    function onMouseMove( event )
    {

        updateSinceLastPoll = true;

        position.x = ( event.clientX / target.clientWidth ) * 2 - 1;
        position.y = - ( event.clientY / target.clientHeight ) * 2 + 1;

    }

    target.addEventListener( 'mousemove', event => onMouseMove( event ), false );

    return {
        pollUpdate: pollUpdate,
        getPosition: getPosition
    };

}

