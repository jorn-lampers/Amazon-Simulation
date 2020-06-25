import * as THREE from '../lib/three.module.js';

import DOMConsole from './DOMConsole.js';

import { GLTFLoader } from '../lib/GLTFLoader.js';
import { ModelManager } from './ModelManager.js';
import { EXRLoader } from '../lib/EXRLoader.js';

import { OrbitControls } from '../lib/OrbitControls.js';

import Stats from '../lib/stats.module.js';
import * as dat from '../lib/dat.gui.module.js';

import * as Utils from './Utils.js';


let viewport;
let compassWidget;
let domConsole;

let socket;
let stats;

let camera, scene, renderer;
let worldObjects = {};

// Lighting
let exrCubeRenderTarget;
let exrBackground;
let ambientLight, directionalLight;

// Animations
let animations;
let mixer;

// User-interfacing
let cameraControls;
let gui;

// Clock used to tick animations
let clock;

// Action-clip responsible for the opening/closing of warehouse-doors (A mesh encapsulated by Object3D 'environment')
let warehouseDoorActions = [];

let loadingManager;
let gltfLoader;
let exrLoader;

let modelManager;

init();

function init( )
{

  domConsole = new DOMConsole( getSocket );
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

  // Default window scaling & camera frustum adjusting
  new ResizeSensor(viewport, function ()
  {
    camera.aspect = viewport.clientWidth / viewport.clientHeight;
    camera.updateProjectionMatrix();

    renderer.setSize( viewport.clientWidth, viewport.clientHeight );
  });

  /////

  // Register eventhandlers for user input
  window.addEventListener( 'keydown', onKeyDown, false );

  stats = new Stats();
  viewport.appendChild( stats.dom );

  /////

  gui = new dat.gui.GUI();

  gui.add( directionalLight, 'intensity' );
  gui.add( directionalLight.shadow, 'bias' );
  gui.add( directionalLight.shadow, 'radius' );
  gui.add( directionalLight.position, 'y' );
  gui.add( ambientLight, 'intensity' );

  domConsole.print( "Scene initialized! (" + getMs() + " ms)", 'green' );

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

function initEnvironment( )
{

  let environment = modelManager.getModelInstance('environment');

  environment.translateZ(8.5);
  environment.translateX(3.2);

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

export function toggleDoor( id ){

  let action = warehouseDoorActions[id];

  domConsole.print('Running animation # ' + id + ' ' + getMs() );

  action.loop = THREE.LoopOnce;
  action.clampWhenFinished = true;

  action.timeScale = action.timeScale * -1;
  action.paused = false;
  action.enabled = true;

  // Only schedule action if it isn't already running
  if( !action.isRunning() ) action.play();

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
  let id = parameters.Guid;

  if( type === 'truck') {
    setDoor(3, true );
  }

  domConsole.print( 'Creating new model of type ' + type + ' with guid ' + id );

  let obj;

  if ( type == "storage" ) obj = Utils.createStorage( 2, 5 );
  else if ( type == "graphdisplay" ) obj = Utils.createGraphWrapper( parameters )
  else obj = modelManager.getModelInstance( type );

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

      }
      else if ( cmdName === "DiscardModel3DCommand" ) {

        domConsole.print( "Discarding: " + JSON.stringify( json.parameters ), 'white' );

        if( worldObjects[json.parameters].name === 'truck') {
          setDoor(3, false );
        }

        scene.remove( worldObjects[json.parameters] );
        delete worldObjects[json.parameters];

      } else {

        domConsole.onerror( "Received command of unknown type: " + cmdName );

      }

  }

  socket.onerror = ( event )  => domConsole.print( "A websocket error occured!" + event, 'red' );

  socket.onopen = () =>
  {

    domConsole.print( 'Connection with back-end established!' );

    const loadingScreen = document.getElementById( 'loading-screen' );
    loadingScreen.classList.add( 'fade-out' );

    // optional: remove loader from DOM via event listener
    loadingScreen.addEventListener( 'transitionend', ( event ) => event.target.remove() );


    domConsole.print( "Starting animation loop! (" + getMs() + " ms)", 'green' );
    render( );

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

export function getMs() { return clock.getElapsedTime() * 1000; }
export function getSocket() { return socket; }
