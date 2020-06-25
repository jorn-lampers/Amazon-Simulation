var viewport;

var camera, scene, renderer;
var cameraControls;

var mouse; // INTERFACING.Mouse

var raycaster = new THREE.Raycaster();
var intersects = {};
var selected = [];

var gui;

window.onload = init;

function init()
{
    viewport = document.getElementById( 'viewport' );

    compassWidget = document.getElementById( 'compass' ).contentWindow;

    compassWidget.addEventListenerX( () => cameraControls.target = new THREE.Vector3( 1, 0, 0 ).add( camera.position ) );
    compassWidget.addEventListenerY( () => cameraControls.target = new THREE.Vector3( 0, 1, 0 ).add( camera.position ) );
    compassWidget.addEventListenerZ( () => cameraControls.target = new THREE.Vector3( 0, 0, 1 ).add( camera.position ) );

    initGFX();
    console.log( "Scene initialized! (" + UTILS.Timer() + " ms)" );

    requestAnimationFrame( render );
    console.log( "Animation loop started! (" + UTILS.Timer() + " ms)" )

    MODELS.LoadModels( ['truck', 'shelf'], () =>
    {
        console.log( "3D models loaded! (" + UTILS.Timer() + " ms)" );
        addModel( MODELS.GetModelInstance( 'truck' ), 'truck' );
    } );

    initUI();

}

function initUI()
{
    gui = new dat.gui.GUI();
    gui.remember(models);
}

function addModel(model, name = "NO_NAME")
{
    UTILS.WrapModel( model, name );

    models[name] = model;
    scene.add( model );

    return model;
}

function render()
{
    // Tick OrbitControls, this is required when you want camera smoothing
    if (cameraControls.update() || mouse.pollUpdate())
    {
        // Update raycaster with respect to updated camera pos
        raycaster.setFromCamera(mouse.getPosition(), camera);
    }

    // When objects move autonomously, they might intersect with an un-updated ray
    intersects = UTILS.FindIntersects( raycaster, Object.values( models ) );    

    // Render the scene
    renderer.render(scene, camera);

    // Render any potential box helper over the scene
    for ( const obj of intersects.objects )
    {
        renderer.render( obj.hitboxDisplay, camera );
    }

    for ( const sel of selected )
    {
        renderer.render( sel.hitboxDisplay, camera );
    }

    compassWidget.UpdateOrientation( camera );

    // Schedule the next call to render()
    requestAnimationFrame( render );

}

function initGFX()
{
    // Initialize default camera
    camera = new THREE.PerspectiveCamera( 70, viewport.clientWidth / viewport.clientHeight, 0.2, 1000 );
    camera.position.set( 0, 0, 10 );

    // Function tracking & converting mouse coordinates in window
    mouse = new INTERFACING.Mouse( viewport );

    // Orbitcontrols sourced from https://github.com/mrdoob/three.js
    cameraControls = new THREE.OrbitControls( camera, viewport );

    // Renderer setup
    renderer = new THREE.WebGLRenderer( { antialias: true } );
    // Disable auto-clear to enable subsequent drawing calls per frame
    renderer.autoClear = false;

    renderer.setPixelRatio( window.devicePixelRatio );
    renderer.setSize( viewport.clientWidth, viewport.clientHeight );

    // Add renderer to document
    viewport.appendChild( renderer.domElement );

    // Default window scaling & camera frustum adjusting
    INTERFACING.StartResizeHandler( viewport, camera, renderer );

    // Basic scene with an assisting grid
    scene = new THREE.Scene();
    scene.background = new THREE.Color( 0x000000 );

    let hxz = new THREE.GridHelper( 1000, 1000, 0xffffff, 0x0f0f0f);
    let hxy = new THREE.GridHelper( 1000, 1000, 0xffffff, 0x0f0f0f);
    let hyz = new THREE.GridHelper( 1000, 1000, 0xffffff, 0x0f0f0f);

    hxy.rotation.x = Math.PI * 0.5;
    hyz.rotation.z = Math.PI * 0.5;

    scene.add( hxz ); scene.add( hxy ); scene.add( hyz );

    // Add a dim ambientlight
    var light = new THREE.AmbientLight( 0xffffff, 0.3 );
    light.position.set( 100, 100, 100 );
    scene.add( light );

    viewport.addEventListener('click', function() {
        if ( intersects.objects.length > 0 ) selected = intersects.objects;
        else selected = [];
    });

    viewport.addEventListener( 'dblclick', function ()
    {
        if ( selected.length > 0 )
        {
            selected[0].updatePosition( new THREE.Vector3( 0, 1, 0 ).add( selected[0].position ) );
            cameraControls.target = selected[0].getCenter();

        }
    });
}