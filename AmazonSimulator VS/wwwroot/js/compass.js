var viewport;

var camera, scene, renderer;

var listenersX = [];
var listenersY = [];
var listenersZ = [];

window.onload = init;

var compass;

var UpdateOrientation = function ( camera )
{
    compass.quaternion.setFromRotationMatrix(
        new THREE.Matrix4().extractRotation( camera.matrixWorldInverse )
    );

}

var setClearColor = function ( color, alpha )
{
    renderer.setClearColor( color, alpha );
}

function init()
{
    viewport = document.getElementById( 'viewport' );

    viewport.addEventListener( 'click', onClick );

    initGFX();

    requestAnimationFrame( render );

}

function render()
{
    // Render the scene
    renderer.render( scene, camera );

    // Schedule the next call to render()
    requestAnimationFrame( render );

}

function createCompass()
{
    compass = new THREE.Group();

    let l = 0.25;
    let w = 0.02;
    let al = 0.075;

    let x = new THREE.ArrowHelper( new THREE.Vector3( 1, 0, 0 ), 0, l, 0xff0000, al, w );
    let y = new THREE.ArrowHelper( new THREE.Vector3( 0, 1, 0 ), 0, l, 0x00ff00, al, w );
    let z = new THREE.ArrowHelper( new THREE.Vector3( 0, 0, 1 ), 0, l, 0x0000ff, al, w );

    x.name = "ArrowX";
    y.name = "ArrowY";
    z.name = "ArrowZ";

    x.position.x = 0; x.position.y = 0; x.position.z = 0;
    y.position.x = 0; y.position.y = 0; y.position.z = 0;
    z.position.x = 0; z.position.y = 0; z.position.z = 0;

    compass.add( x );
    compass.add( y );
    compass.add( z );

    compass.name = "compass";
    scene.add( compass );

}

function initGFX()
{
    // Initialize default camera
    camera = new THREE.PerspectiveCamera( 30, viewport.clientWidth / viewport.clientHeight, 0.2, 10 );
    camera.position.set( 0, 0.0, 1 );

    // Renderer setup
    renderer = new THREE.WebGLRenderer( { alpha: true } );

    renderer.setPixelRatio( window.devicePixelRatio );
    renderer.setSize( viewport.clientWidth, viewport.clientHeight );

    // Add renderer to document
    viewport.appendChild( renderer.domElement );

    // Default window scaling & camera frustum adjusting
    new ResizeSensor(

        viewport,

        function ()
        {

            camera.aspect = viewport.clientWidth / viewport.clientHeight;
            camera.updateProjectionMatrix();

            renderer.setSize( viewport.clientWidth, viewport.clientHeight );

        }

    );

    // Basic scene with an assisting grid
    scene = new THREE.Scene();
    createCompass();
}

function onClick()
{
    let caster = new THREE.Raycaster();
    caster.setFromCamera( mouse.getPosition(), camera );

    let intersects = UTILS.FindIntersects( caster, compass.children );

    for ( const intersect of intersects )
    {
        console.log( "Intersects: " + intersect.name );

        switch ( intersect.name )
        {

            case "ArrowX":
                for ( const listener of listenersX ) listener();
                break;

            case "ArrowY":
                for ( const listener of listenersY ) listener();
                break;

            case "ArrowZ":
                for ( const listener of listenersZ ) listener();
                break;

        }

    }

}