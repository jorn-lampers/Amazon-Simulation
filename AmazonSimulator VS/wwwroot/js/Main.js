var viewport;

var socket;

var camera, scene, renderer;
var cameraControls;

var mouse; // INTERFACING.Mouse
var raycaster = new THREE.Raycaster();

var intersects = {};
var selected;

var worldObjects = {};

var warehouse;
var mixer;
var animations;

var clock;

var action;

function parseCommand( input = "" )
{
  return JSON.parse( input );
}

function sendCommand( type = "ViewCommand", args ) 
{
  var msg = JSON.stringify(
    {
      type: type,
      parameters: args
    }
  );

  socket.send(msg);
}

function runAnimation( index = 0, reversed = false ) 
{
  if ( !reversed )
  {
    action = this.mixer.clipAction( this.animations[index] );
    action.reset()
    action.timeScale = 1;
    action.clampWhenFinished = true;
  } else
  {
    action = this.mixer.clipAction( this.animations[index] );
    action.paused = false;
    action.timeScale = -1;
  }

  action.setLoop( THREE.LoopOnce );
  action.play();

}

window.onload = function () 
{

  function onKeyDown( e ) 
  {

      if ( e.keyCode == 192 || e.which == 192 )
      {

          e.preventDefault();
          $('#overlay').toggle( 250 );
          $( '#input' ).focus();

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

    compassWidget = document.getElementById('compass').contentWindow;

    compassWidget.addEventListenerX( () => cameraControls.target = new THREE.Vector3(1, 0, 0).add(camera.position) );
    compassWidget.addEventListenerY( () => cameraControls.target = new THREE.Vector3( 0, 1, 0 ).add( camera.position ) );
    compassWidget.addEventListenerZ( () => cameraControls.target = new THREE.Vector3( 0, 0, 1 ).add( camera.position ) );

    CONSOLE.Log( "Initializing viewport", "yellow" );

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

    // Lighting
    var lightIn = new THREE.PointLight(0x404040, 2, 100);
    var lightOut = new THREE.PointLight(0x404040, 3, 0);
    var lightAmbient = new THREE.AmbientLight(0x404040, 0.3);

    lightIn.position.set(0, 10, 0);
    lightOut.position.set(0, 100, 0);

    //light.intensity = 4;

    scene.add( lightIn );
    scene.add(lightOut);
    scene.add(lightAmbient);

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
    });

    viewport.appendChild(renderer.domElement);

    clock = new THREE.Clock( true );
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

              object.rotation.x = command.parameters.RotationX;
              object.rotation.y = command.parameters.RotationY;
              object.rotation.z = command.parameters.RotationZ;

          } else if (command.command == "DiscardModel3DCommand") {
              CONSOLE.Log("Discarding: " + JSON.stringify(command.parameters), 'white');
              scene.remove(worldObjects[command.parameters]);
              delete worldObjects[command.parameters];
          }
      }

      socket.onerror = function (event) 
      {

          CONSOLE.Log( "A websocket error occured!" + event, 'red' );

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

      compassWidget.UpdateOrientation(camera);

      // Tick animations
      var dt = clock.getDelta()
      mixer.update( dt );

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

  CONSOLE.Log( "Scene initialized! (" + timer() + " ms)", 'green' );

  MODELS.LoadModels(['truck', 'shelf', 'robot', 'warehouse', 'outside'], () => {

    CONSOLE.Log("3D models loaded! (" + timer() + " ms)", 'green');

    warehouse = MODELS.GetModelInstance('warehouse');
    mixer = new THREE.AnimationMixer(warehouse);

    animations = MODELS.GetAnimations('warehouse');

    warehouse.translateZ(8.5);
    warehouse.translateX(3.2);

    warehouse.rotateY(0.5 * Math.PI);

    scene.add(warehouse);

    var outside = MODELS.GetModelInstance('outside');
    outside.rotateY(-0.5 * Math.PI);
    outside.position.set(-13, -1.29, 25);

    scene.add(outside);

    initSocket( () => CONSOLE.Log( "Connected to back-end! (" + timer() + " ms)", 'green' ) );

    render();
    CONSOLE.Log("Animation loop started! (" + timer() + " ms)", 'green');

  });

}