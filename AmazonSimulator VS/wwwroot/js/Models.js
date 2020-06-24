import * as THREE from '../lib/three.module.js';
import { GLTFLoader } from '../lib/GLTFLoader.js';

var ResourceManager = function ( )
{
    var _animations = { };
    var _models = { };
    var _envMap;
    function areModelsLoaded( names )
    {
        for (let i = 0; i < names.length; i++)
            if (_models[names[i]] == null) return false;

        return true;
    }

    this.getModelInstance = function( name )
    {
        return _models[name].clone();
    }

    this.setDefaultEnvMap = function( map )
    {
        _envMap = map;
        Object.keys(_models).forEach(function (item) {
            console.log('Applying new envMap to ' + item + '...'); // key
            _models[item].traverse( function ( child )
            {
                // Apply supplied environmentMap to all materials that support envMapping
                if( child.material && 'envMap' in child.material )
                    child.material.envMap = map;
            } );
        });
    }

    this.getAnimations = function(name)
    {
        return _animations[name];
    }

    this.createRobot = function ()
    {
        var robot = this.getModelInstance( 'robot' );

        robot.position.y = 0.25;

        var group = new THREE.Group();
        group.add(robot);
        group.name = "Robot";

        return group;
    }

    this.createTruck = function ()
    {
        var truck = this.getModelInstance('truck');

        // Encapsulate loaded model into a group
        var group = new THREE.Group();
        group.add(truck);
        group.name = "Truck";

        return group;
    }

    this.createShelf = function () {

        var group = new THREE.Group();

        group.add(this.getModelInstance('shelf'));
        group.name = "Shelf";

        return group;
    }

    this.createGraph = function (graph) {
        var group = new THREE.Group();
        graph.Edges.forEach(function (edge) {
            var lineGeometry = new THREE.Geometry();
            lineGeometry.vertices.push(new THREE.Vector3(edge.A.Position.X, edge.A.Position.Y, edge.A.Position.Z));
            lineGeometry.vertices.push(new THREE.Vector3(edge.B.Position.X, edge.B.Position.Y, edge.B.Position.Z));

            var mat = new THREE.LineBasicMaterial({ color: 0x0000ff });

            if (edge.Width > 0) mat = new THREE.LineBasicMaterial({ color: 0xff0000 });

            var mesh = new THREE.Line(
                lineGeometry,
                mat
            );
            group.add(mesh);
        });

        graph.Nodes.forEach(function (node) {
            var dotGeometry = new THREE.Geometry();
            dotGeometry.vertices.push(new THREE.Vector3(node.Position.X, node.Position.Y, node.Position.Z));
            var mesh = new THREE.Points(
                dotGeometry,
                new THREE.PointsMaterial({ size: 8, sizeAttenuation: false, color: 0xff0000 })
            );
            mesh.name = "Vertex";
            group.add(mesh);
        });
        group.name = "Graph";

        return group;
    }

    this.createStorage = function (length, width) {
        var bordersize = 0.15;
        var group = new THREE.Group();

        var a = new THREE.Mesh(
            new THREE.PlaneGeometry(width, bordersize),
            new THREE.MeshFaceMaterial([
                new THREE.MeshBasicMaterial({ color: "#ffff00", side: THREE.DoubleSide })
            ])
        );
        a.position.z = 0.5 * length - 0.5 * bordersize;
        a.position.y += 0.01;
        a.rotation.x = 0.5 * Math.PI;
        group.add(a);

        var b = new THREE.Mesh(
            new THREE.PlaneGeometry(width, bordersize),
            new THREE.MeshFaceMaterial([
                new THREE.MeshBasicMaterial({ color: "#ffff00", side: THREE.DoubleSide })
            ])
        );
        b.position.z = - ( 0.5 * length - 0.5 * bordersize );
        b.position.y += 0.01;
        b.rotation.x = 0.5 * Math.PI;
        group.add(b);

        var c = new THREE.Mesh(
            new THREE.PlaneGeometry(bordersize, length),
            new THREE.MeshFaceMaterial([
                new THREE.MeshBasicMaterial({ color: "#ffff00", side: THREE.DoubleSide })
            ])
        );
        c.position.x = 0.5 * width - 0.5 * bordersize;
        c.position.y += 0.01;
        c.rotation.x = 0.5 * Math.PI;
        group.add(c);

        var d = new THREE.Mesh(
            new THREE.PlaneGeometry(bordersize, length),
            new THREE.MeshFaceMaterial([
                new THREE.MeshBasicMaterial({ color: "#ffff00", side: THREE.DoubleSide })
            ])
        );
        d.position.x = -( 0.5 * width - 0.5 * bordersize ); 0
        d.position.y += 0.01;
        d.rotation.x = 0.5 * Math.PI;
        group.add(d);
        group.name = "Storage";

        return group;
    }

    this.loadModels = function( names, manager )
    {
      // Pass on LoadingManager supplied by invoker
      let loader = new GLTFLoader( manager );

      for ( let i = 0; i < names.length; i++ ) 
      {
          let path = '/models/' + names[i] + '/model.glb';

          loader.load( path, function ( gltf ) 
          {
            //CONSOLE.Log( 'glb ' + path + ' loaded successfully.');

            gltf.scene.userData = {}; // TODO:

            // Apply desired properties to objects imported from a GLB
            gltf.scene.traverse( function ( child )
            {
                // Apply supplied environmentMap to all materials that support envMapping
                if( _envMap && child.material && 'envMap' in child.material )
                {
                    console.log('Applying new envMap to ' + path + '...'); // key
                    child.material.envMap = _envMap;
                }

                // All models will cast shadows except materials named 'Material.Glass'
                child.castShadow = (child.material && child.material.name != 'Material.Glass');

                // All models should receive shadows.
                child.receiveShadow = true;
            } );

            // Cache the model in _models, module users can request a CLONE of the model with loadModelInstance()
            _models[names[i]] = gltf.scene;

            _animations[names[i]] = gltf.animations;

          }, undefined, function ( error )
          {

            console.error( error );

          });
      }

    };

};

export { ResourceManager };
