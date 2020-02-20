var viewport;

var socket;

var camera, scene, renderer;
var cameraControls;

var mouse; // INTERFACING.Mouse
var raycaster = new THREE.Raycaster();

var intersects = {};
var selected;

var worldObjects = {};

function parseCommand(input = "") {
    return JSON.parse(input);
}

function sendCommand(type = "ViewCommand", args) {
    var msg = JSON.stringify({
        type: type,
        parameters: args
    });
    socket.send(msg);
}

window.onload = function () 
{

    function onKeyDown( e ) 
    {

        console.log(e.keyCode);

        if ( e.keyCode == 192 || e.which == 192 )
        {

            $('#overlay').toggle( 250 );

        }

    };

    function create( parameters )
    {
        let type = parameters.Type;

        let e;
        if ( type == "robot" )
        {
            e = CreateRobot();
        } else if ( type == "storage" )
        {
            e = CreateStorage( 2, 5 );
        } else if ( type == "shelf" )
        {
            e = MODELS.GetModelInstance( 'shelf' );
        } else if ( type == "graphdisplay" )
        {
            e = CreateGraph( parameters );
        } else if ( type == "target" )
        {
            e = CreateRobot();
            e.name = "target";
        } else if ( type == "truck" )
        {
            e = CreateTruck();
        }

        UTILS.WrapModel( e, type );
        return e;
    }

    function init() 
    {

        INTERFACING.Log( "Initializing viewport", "yellow" );

        viewport = document.getElementById( 'viewport' );

        // Renderer setup
        renderer = new THREE.WebGLRenderer( { antialias: true } );
        // Disable auto-clear to enable subsequent drawing calls per frame
        renderer.autoClear = false;

        renderer.setPixelRatio( window.devicePixelRatio );
        renderer.setSize( viewport.clientWidth, viewport.clientHeight );
        renderer.shadowMap.enabled = true;
        renderer.shadowMap.type = THREE.PCFSoftShadowMap;

        // Initialize default camera
        camera = new THREE.PerspectiveCamera( 70, viewport.clientWidth / viewport.clientHeight, 0.2, 1000 );
        camera.position.set( 15, 5, 15 );

        // Default window scaling & camera frustum adjusting
        INTERFACING.StartResizeHandler( viewport, camera, renderer );

        // Function tracking & converting mouse coordinates in window
        mouse = new INTERFACING.Mouse( viewport );

        // Orbitcontrols sourced from https://github.com/mrdoob/three.js
        cameraControls = new THREE.OrbitControls( camera, viewport );

        // Scene
        scene = new THREE.Scene();
        scene.background = new THREE.Color(0x33ccff);

        window.addEventListener('keydown', onKeyDown, false);

        // World base
        var geometry = new THREE.PlaneGeometry(18, 46, 32);
        var material = new THREE.MeshPhongMaterial({ color: 0xbbbbbb, side: THREE.DoubleSide });
        var plane = new THREE.Mesh(geometry, material);

        plane.rotation.x = Math.PI / 2.0;
        plane.position.set(0, 0, 0);

        plane.castShadow = true;
        plane.receiveShadow = true;

        scene.add(plane);

        // Lighting
        var light = new THREE.PointLight(0x404040, 3);
         
        light.position.set(0, 10, 0);
        //light.intensity = 4;

        scene.add( light );

        viewport.addEventListener( 'dblclick', function ()
        {
            if ( intersects.objects.length > 0 ) 
            {

                selected = intersects.objects[UTILS.NearestIndex( intersects )];

            }

            else 
            {

                selected = null;

            }
        } );

        viewport.appendChild( renderer.domElement );
    }

    function initSocket(onConnect) {
        socket = new WebSocket( "ws://" + window.location.hostname + ":" + window.location.port + "/connect_client" );

        // Define and assign function to parse and handle commands received through websocket
        socket.onmessage = function ( event ) 
        {
            var command = parseCommand(event.data);

            if (command.command == "UpdateModel3DCommand") {
                if (Object.keys(worldObjects).indexOf(command.parameters.Guid) < 0) {   // If object's GUID was not found in worldObjects, we must first create a new representation of this object
                    var obj = create( command.parameters );

                    worldObjects[command.parameters.Guid] = obj;
                    scene.add( obj );
                }

                // Fetch object designated in command
                var object = worldObjects[command.parameters.Guid];

                object.updatePosition( new THREE.Vector3( command.parameters.X, command.parameters.Y, command.parameters.Z ) );

                if ( command.parameters.Type == "truck" && command.parameters.Door != null )
                {
                    let door = scene.getObjectByName( "Trailer_Box_Door" );

                    if ( command.parameters.Door )
                        door.rotation.x = - Math.PI * 0.5;

                    else
                        door.rotation.x = 0;
                }

                object.rotation.x = command.parameters.RotationX;
                object.rotation.y = command.parameters.RotationY;
                object.rotation.z = command.parameters.RotationZ;

            } else if (command.command == "DiscardModel3DCommand") {
                INTERFACING.Log("Discarding: " + JSON.stringify(command.parameters), 'white');
                scene.remove(worldObjects[command.parameters]);
                delete worldObjects[command.parameters];
            }
        }

        socket.onerror = function (event) 
        {

            INTERFACING.Log( "A websocket error occured!" + event, 'red' );

        }

        socket.onopen = onConnect;
    }

    // Animation loop
    function render() 
    {
        // Tick OrbitControls, this is required when you want camera smoothing
        if ( cameraControls.update() || mouse.pollUpdate() )
        {

            // Update raycaster with respect to updated camera pos
            raycaster.setFromCamera( mouse.getPosition(), camera );

        }

        // Render the scene
        renderer.render( scene, camera );

        // When objects move autonomously, they might intersect with an un-updated ray
        intersects = UTILS.FindIntersects( raycaster, Object.values( worldObjects ) );

        // Render any potential box helper over the scene
        for ( const i of intersects.objects )
        {

            if ( i.hitbox == null ) continue;

            renderer.render( i.hitboxDisplay, camera );

        }

        // Render any potential box helper over the scene
        if ( selected != null )
        {
            if ( selected.hitboxDisplay != null ) 
            {

                renderer.render( selected.hitboxDisplay, camera );

            }
        }

        requestAnimationFrame( render );
    }

    var ts = performance.now();

    function timer()
    {
        let diff = performance.now() - ts;

        ts = performance.now();
        return diff;

    }


    init();
    INTERFACING.Log( "Scene initialized! (" + timer() + " ms)", 'green' );

    render();
    INTERFACING.Log( "Animation loop started! (" + timer() + " ms)", 'green' )

    MODELS.LoadModels(['truck', 'shelf'], () => {

        INTERFACING.Log( "3D models loaded! (" + timer() + " ms)" , 'green' );

        initSocket( () => INTERFACING.Log( "Connected to back-end! (" + timer() + " ms)", 'green' ) );

    });

}