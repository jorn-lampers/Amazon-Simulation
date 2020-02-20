var INTERFACING =
{

    // A function returning.... another function :)
    StartResizeHandler: function ( target, camera, renderer )
    {
        new ResizeSensor(

            target,

            function ()
            {

                camera.aspect = target.clientWidth / target.clientHeight;
                camera.updateProjectionMatrix();

                renderer.setSize( target.clientWidth, target.clientHeight );

            }

        );
    },

    Mouse: function (target)
    {
        let position = new THREE.Vector2( 0, 0 );

        let updateSinceLastPoll = false;

        function onMouseMove( event )
        {

            updateSinceLastPoll = true;

            position.x = ( event.clientX / target.clientWidth ) * 2 - 1;
            position.y = - ( event.clientY / target.clientHeight ) * 2 + 1;

        }

        this.pollUpdate = function ( resetUpdate = true ) 
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

        this.getPosition = () => position;

        target.addEventListener( 'mousemove', event => onMouseMove( event ), false );

    }

}