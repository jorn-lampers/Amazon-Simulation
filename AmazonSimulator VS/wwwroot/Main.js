var socket;

var camera, scene, renderer;
var cameraControls;

var intersectedVertices = [];
var selectedVertex;
var raycaster;
var mouse;

var worldObjects = {};

function parseCommand(input = "") {
    return JSON.parse(input);
}

function sendCommand(type = "ViewCommand", args) {
    var msg = JSON.stringify({
        type: type,
        parameters: args
    });
    console.log("Sending: " + msg);
    socket.send(msg);
}

window.onload = function () {
    function onKeyDown(e) {
        switch (e.key) {
            case "m":
                var args = {};
                args.target = { x: selectedVertex.geometry.vertices[0].x, y: selectedVertex.geometry.vertices[0].y, z: selectedVertex.geometry.vertices[0].z };

                sendCommand("TestCommand", args);
                break;
            case "t":
                sendCommand("ReceiveShipmentCommand", {});
                break;
            case "y":
                sendCommand("SendShipmentCommand", {});
                break;

        }
    };

    function onMouseMove(event) {
        mouse.x = (event.clientX / window.innerWidth) * 2 - 1;
        mouse.y = - (event.clientY / window.innerHeight) * 2 + 1;
    }

    // Fits and scales renderer to viewport
    function onWindowResize() {
        camera.aspect = window.innerWidth / window.innerHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(window.innerWidth, window.innerHeight);
    }

    function init() {

        // Camera & controls
        camera = new THREE.PerspectiveCamera(70, window.innerWidth / window.innerHeight, 1, 1000);
        raycaster = new THREE.Raycaster();
        mouse = new THREE.Vector2();

        cameraControls = new THREE.OrbitControls(camera);

        camera.position.z = 15;
        camera.position.y = 5;
        camera.position.x = 15;

        cameraControls.update();

        // Scene
        scene = new THREE.Scene();
        scene.background = new THREE.Color(0x080808);

        // Renderer
        renderer = new THREE.WebGLRenderer({ antialias: true });

        renderer.setPixelRatio(window.devicePixelRatio);
        renderer.setSize(window.innerWidth, window.innerHeight + 5);
        renderer.shadowMap.enabled = true;
        renderer.shadowMap.type = THREE.PCFSoftShadowMap;

        document.body.appendChild(renderer.domElement);

        window.addEventListener('resize', onWindowResize, false);
        window.addEventListener('mousemove', onMouseMove, false);
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

        scene.add(light);
    }

    function initSocket() {
        socket = new WebSocket("ws://" + window.location.hostname + ":" + window.location.port + "/connect_client");
        // Define and assign function to parse and handle commands received through websocket
        socket.onmessage = function (event) {
            var command = parseCommand(event.data);

            if (command.command == "UpdateModel3DCommand") {
                if (Object.keys(worldObjects).indexOf(command.parameters.Guid) < 0) {   // If object's GUID was not found in worldObjects, we must first create a new representation of this object
                    var group = CreateEntity(command);
                    worldObjects[command.parameters.Guid] = group;
                    scene.add(group);
                }

                // Fetch object designated in command
                var object = worldObjects[command.parameters.Guid];


                // Update designated object with supplied parameters
                object.position.x = command.parameters.X;
                object.position.y = command.parameters.Y;
                object.position.z = command.parameters.Z;

                object.rotation.x = command.parameters.RotationX;
                object.rotation.y = command.parameters.RotationY;
                object.rotation.z = command.parameters.RotationZ;
            } else if (command.command == "DiscardModel3DCommand") {
                console.log(JSON.stringify(command.parameters));
                scene.remove(worldObjects[command.parameters]);
                delete worldObjects[command.parameters];
            }
        }

        socket.onerror = function (event) {
            console.log("A websocket error occured!" + event);
        }
    }

    // Animation loop
    function animate() {
        requestAnimationFrame(animate);

        cameraControls.update();
        renderer.render(scene, camera);
    }

    function start() {
        console.log("Loading finished, starting simulation!");
        initSocket();

        animate();
    }

    init();

    MODELS.LoadModels(start);
}