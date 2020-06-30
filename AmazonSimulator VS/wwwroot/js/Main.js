import * as THREE from '../lib/three.module.js';

import Stats from '../lib/stats.module.js';
import * as dat from '../lib/dat.gui.module.js';

import { GLTFLoader } from '../lib/GLTFLoader.js';
import { ModelManager } from './ModelManager.js';
import { EXRLoader } from '../lib/EXRLoader.js';

import { OrbitControls } from '../lib/OrbitControls.js';

import * as Utils from './Utils.js';
import DOMConsole from './DOMConsole.js';

let viewport;
let compassWidget;
let domConsole;

let socket;
let stats;

let camera, scene, renderer;
let worldObjects = {};

let pathfindingGraph;

// Lighting
let exrCubeRenderTarget;
let exrBackground;
let ambientLight, directionalLight;

// Animations
let mixer;

// Action-clip responsible for the opening/closing of warehouse-doors (A mesh encapsulated by Object3D 'environment')
let warehouseDoorActions = [];

// User-interfacing
let cameraControls;
let gui;

// Clock used to tick animations
let clock;

let loadingManager;
let gltfLoader;
let exrLoader;

let modelManager;

let eventRunning = false;
let eventTimer;

let controls = {
  shipmentSize: 10,

  autoRun: false,

  autoRunMinInterval: 2,
  autoRunMaxInterval: 10,

  autoRunMinCargo: 1,
  autoRunMaxCargo: 24
};

let settings = {
  shadowMapRes: 2048,
  displayPathfindingGraph: false,
  displayRobotFOV: false
};

function init( )
{

  domConsole = DOMConsole();
  viewport = document.getElementById( 'viewport' );

  initScene();

  clock = new THREE.Clock( true );
  mixer = new THREE.AnimationMixer( scene );

  // Instantiate renderer
  renderer = new THREE.WebGLRenderer( { antialias: true } );

  renderer.setPixelRatio( window.devicePixelRatio );
  renderer.setSize( viewport.clientWidth, viewport.clientHeight );

  renderer.shadowMap.enabled = true; //renderer.shadowMap.type = THREE.PCFSoftShadowMap;

  loadingManager = new THREE.LoadingManager(() => {

    domConsole.print("Finished loading of all assets! (" + getMs() + ")", 'green');

    initEnvironment();
    initSocket();

  } );

  // Invoke resource loaders
  gltfLoader = new GLTFLoader( loadingManager );
  modelManager = new ModelManager( gltfLoader );
  exrLoader = new EXRLoader( loadingManager );

  exrLoader.setDataType( THREE.UnsignedByteType );
  exrLoader.load('textures/kloppenheim_03_4k.exr',
      function ( texture )
  {

    let pmremGenerator = new THREE.PMREMGenerator( renderer );
    pmremGenerator.compileEquirectangularShader();

    exrCubeRenderTarget = pmremGenerator.fromEquirectangular(texture);
    exrBackground = exrCubeRenderTarget.texture;
    scene.background = exrBackground;

    texture.dispose();

    domConsole.print('EXR Background loaded! (' + getMs() + ')', 'green');

    let modelNames = ["environment", "truck", "robot", "shelf"];

    modelManager.loadModels(
        modelNames,
        () => {
          domConsole.print('Model loading finished! (' + getMs() + ')', 'green');
          modelManager.overrideMaterialProperties( modelNames, { envMap: exrBackground } );
        }
    );

  });

  // Append renderer to DOM
  viewport.appendChild(renderer.domElement);

  compassWidget = document.getElementById('compass').contentWindow;

  // Default window scaling & camera frustum adjusting
  new ResizeSensor(viewport, function ()
  {
    camera.aspect = viewport.clientWidth / viewport.clientHeight;
    camera.updateProjectionMatrix();

    renderer.setSize( viewport.clientWidth, viewport.clientHeight );
  });

  initGUI();

  // Orbitcontrols sourced from https://github.com/mrdoob/three.js
  // Modified to prevent camera from panning/zooming through the ground
  cameraControls = new OrbitControls( camera, viewport );

  cameraControls.minPan = new THREE.Vector3( -50, 0.75, -50 );
  cameraControls.maxPan = new THREE.Vector3( 50, 50, 50 );
  cameraControls.screenSpacePanning = true;
  cameraControls.panSpeed = 0.5;

  cameraControls.maxDistance = 60;

  cameraControls.enableDamping = true;
  cameraControls.dampingFactor = 0.1;

  cameraControls.rotateSpeed = 0.35;

  cameraControls.target.set( 0, 2, 0 );


  // Register eventhandlers for user input
  window.addEventListener( 'keydown', onKeyDown, false );

  stats = new Stats();
  viewport.appendChild( stats.dom );

  domConsole.print( "Scene initialized! (" + getMs() + " ms)", 'green' );

}

// Sends specified command to the back-end
export function sendCommand( type, args )
{
  let msg = JSON.stringify(
      {
        type: type,
        parameters: args
      }
  );

  socket.send(msg);
}

// Gets elapsed time in ms
export function getMs() {
  return clock.getElapsedTime() * 1000;
}

// Sets up lighting and other properties in the scene that don't require loading
function initScene( )
{
  // Initialize default camera
  camera = new THREE.PerspectiveCamera( 70, viewport.clientWidth / viewport.clientHeight, 1, 20000 );
  camera.position.set( 15, 5, 15 );

  // Scene
  scene = new THREE.Scene();

  // Lighting
  ambientLight = new THREE.AmbientLight( 0x808080, 1.0 ); // Ambient light

  directionalLight = new THREE.DirectionalLight( 0x808080, 2.8 ); // Directional light mimicking the sun

  directionalLight.castShadow = true;

  directionalLight.shadow.camera.right = 100;
  directionalLight.shadow.camera.left = -100;
  directionalLight.shadow.camera.top = 100;
  directionalLight.shadow.camera.bottom = -100;

  directionalLight.shadow.mapSize.width = 2048;
  directionalLight.shadow.mapSize.height = 2048;

  directionalLight.shadow.camera.near = 0.0;
  directionalLight.shadow.camera.far = 150;

  directionalLight.shadow.bias = -0.0015;
  directionalLight.shadow.radius = 1;

  directionalLight.position.set( 30, 60, 30 );

  scene.add( directionalLight );
  scene.add( ambientLight );
}

function runRandomEvent()
{

  if(eventTimer)
  {

    domConsole.print('An event has already been scheduled to run');

    return;

  }

  if(eventRunning)
  {

    domConsole.print('An event is already running, a new one will be scheduled as soon as it has finished...');

    return;

  }

  let dt = 1000 * Math.random() * controls.autoRunMaxInterval - controls.autoRunMinInterval;
  let t = 1000 * controls.autoRunMinInterval + dt;
  if(t < 0) t = 1;

  let a = Math.floor(Math.random() * (controls.autoRunMaxCargo - controls.autoRunMinCargo) + controls.autoRunMinCargo);
  if(a < 0) a = 0;

  let isReceive = Math.random() > 0.5;

  let cType = isReceive ? "ReceiveShipmentCommand" : "SendShipmentCommand";

  domConsole.print("Next event: '" + (isReceive ? 'receive' : 'send') + "' " + a + " starts in " + t + "ms!");

  eventTimer = setTimeout(function() {
    domConsole.print((isReceive ? 'Receiving ' : 'Sending ') + ' shipment of size: ' + a);
    sendCommand( cType, { amount: a }, t);
    eventTimer = null;
  }, t);

}

function initGUI()
{

  gui = new dat.gui.GUI();

  let uiSettings = gui.addFolder('Settings');

  uiSettings.add(settings, 'displayPathfindingGraph').onChange(function (enabled) {
    pathfindingGraph.visible = enabled;
  });

  uiSettings.add(settings, 'displayRobotFOV');

  let uiAutorun = gui.addFolder('Autorun');

  uiAutorun.add(controls, 'autoRunMinInterval', 0, 60);
  uiAutorun.add(controls, 'autoRunMaxInterval', 1, 120);

  uiAutorun.add(controls, 'autoRunMinCargo', 1, 24);
  uiAutorun.add(controls, 'autoRunMaxCargo', 1, 24);

  uiAutorun.add(controls, 'autoRun').onChange(function (enabled)
  {

    if (enabled) {

      domConsole.print("Random events are now enabled.");

      if(eventRunning) return;

      runRandomEvent();

    }
    else
    {

      domConsole.print("Random events have been disabled.");

      if(eventTimer)
      {

        clearTimeout(eventTimer);

        eventTimer = null;

      }

    }

  });

  let actions = {

    receiveShipment: function ()
    {

      let a = controls.shipmentSize;
      sendCommand( "ReceiveShipmentCommand", { amount: a } );

    },

    sendShipment: function()
    {

      let a = controls.shipmentSize;
      sendCommand( "SendShipmentCommand", { amount: a } );

    }

  }

  let uiControls = gui.addFolder('Controls');

  uiControls.add(controls, 'shipmentSize', 0, 32);

  uiControls.add(actions, 'receiveShipment');
  uiControls.add(actions, 'sendShipment');

  uiControls.open();

}

function initEnvironment( )
{

  let environment = modelManager.getModelInstance('environment');

  environment.translateZ(8.5);
  environment.translateX(1.2);

  environment.rotateY(0.5 * Math.PI);

  scene.add( environment );

  let clips = modelManager.getAnimations( 'environment' );

  for ( let i = 0; i < clips.length; i++ )
  {
    let action = mixer.clipAction( clips[i] );

    action.clampWhenFinished = true;

    action.timeScale = -1;

    warehouseDoorActions[i] = action;
  }

}

function setDoor( id, open )
{

  let action = warehouseDoorActions[id];

  action.loop = THREE.LoopOnce;
  action.clampWhenFinished = true;

  action.timeScale = open ? 1 : -1;

  action.paused = false;
  action.enabled = true;

  // Only schedule action if it isn't already running
  if( !action.isRunning() ) action.play();

}

function onKeyDown( e ) 
{
  if ( e.keyCode == 192 || e.which == 192 )
  {

    e.preventDefault();

    $('#overlay').toggle( 250 );

    $( '#input' ).focus();

  }

}

function create( parameters )
{

  let type = parameters.Type;

  // Play door opening animation
  if( type === 'truck') {

    eventRunning = true;

    setDoor(3, true );

  }

  let obj;

  // Objects of type 'storage' and 'graph' are 'procedurally' generated
  if ( type === "storage" ) obj = Utils.createStorage( 2, 5 );
  else if ( type === "graphdisplay" )
  {

    obj = Utils.createGraphWrapper( parameters );

    pathfindingGraph = obj;

  }
  else obj = modelManager.getModelInstance( type ); // Fetch preloaded model from ModelManager

  // Wrapper function that adds some extra functions to obj
  Utils.wrapModel( obj, type );

  worldObjects[parameters.Guid] = obj;

  scene.add( obj );

}

function initSocket( )
{

  socket = new WebSocket( "ws://" + window.location.hostname + ":" + window.location.port + "/connect_client" );
  domConsole.Socket = socket;

  // Define and assign function to parse and handle commands received through websocket
  socket.onmessage = function ( event ) 
  {

    let json = JSON.parse( event.data );
    let cmdName = json.command;
    let params = json.parameters;

      if ( cmdName === "UpdateModel3DCommand" ) {

        // If object's GUID was not found in worldObjects, we must first create a new representation of this object
        if ( Object.keys( worldObjects ).indexOf( params.Guid ) < 0 ) create( params );

        // Fetch object designated in command
        let object = worldObjects[params.Guid];

        object.updatePosition( params.X, params.Y, params.Z );
        object.updateRotation( params.RotationX, params.RotationY, params.RotationZ );

        if(object.name === "robot")
        {

          if(settings.displayRobotFOV)
            object.updateDebugUI( params, scene );

        }

      }
      else if ( cmdName === "DiscardModel3DCommand" ) {

        domConsole.print( "Discarding: " + JSON.stringify( json.parameters ), 'white' );

        if( worldObjects[json.parameters].name === 'truck') {
          setDoor(3, false );

          eventRunning = false;

          if(controls.autoRun) runRandomEvent();
        }

        scene.remove( worldObjects[json.parameters] );

        delete worldObjects[json.parameters];

      } else domConsole.onerror( "Received command of unknown type: " + cmdName );

  }

  socket.onerror = ( event )  => domConsole.print( "A websocket error occured!" + event, 'red' );

  socket.onopen = () =>
  {

    domConsole.print( 'Connection with back-end established!', 'green' );

    const loadingScreen = document.getElementById( 'loading-screen' );

    // At this point the scene will be done loading, fade the loading-screen overlay
    loadingScreen.classList.add( 'fade-out' );

    // Remove loading screen after it faded out so it doesn't interfere with mouse controls
    loadingScreen.addEventListener( 'transitionend', ( event ) => event.target.remove() );

    domConsole.print( "Starting animation loop! (" + getMs() + " ms)", 'green' );

    // Start render loop
    render();

  }

}

// Animation loop
function render()
{

  stats.begin();

  cameraControls.update();

  mixer.update( clock.getDelta() );

  renderer.render( scene, camera );

  compassWidget.UpdateOrientation(camera);

  stats.end();

  requestAnimationFrame( render );

}

// Starts setup
init();
