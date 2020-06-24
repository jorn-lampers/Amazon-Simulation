import * as THREE from '../lib/three.module.js';
import { GLTFLoader } from '../lib/GLTFLoader.js';

var Models = function ( )
{
    let _animations = { };
    let _models = { };
    let _envMap;

    let gltfLoader;

    export function overrideMaterialProperties( modelNames, properties ) {

        modelNames.forEach( ( modelName, i, a ) => {
            let model = _models[modelName];

            // Apply desired properties to objects imported from a GLB
            model.traverse(function (child) {

                let mat = child.material;

                // Apply supplied environmentMap to all materials that support envMapping
                if ( mat ) {

                    let matches = [];
                    Object.keys( properties ).forEach( ( p) =>
                    {
                        if( p in mat )
                            matches.push( p );
                    });

                    console.log('Setting ' + matches.length + ' properties in material with name "' + mat.name + '".');
                    matches.forEach(( p ) => mat[p] = properties[p]);

                }

            });

        });
    }

    export function loadModels( modelNames, gltfLoader ) {


        // Preload environment model before starting the simulation
        ["environment", "truck", "robot", "shelf"].forEach((name, i, a) => {
            gltfLoader
                .load('/models/' + name + '/model.glb', function ( gltf )
                {
                    let model = gltf.scene;

                    // Apply desired properties to objects imported from a GLB
                    model.traverse(function (child) {

                        // All models should cast shadows except materials named 'Material.Glass'
                        child.castShadow = (child.material && child.material.name != 'Material.Glass');

                        // All models should receive shadows.
                        child.receiveShadow = true;

                    });

                    models[name] = model;

                    domConsole.print('Preloaded model ' + name + '! (' + i + '/' + a.length + ')', 'green');

                });
        });

    }

};

export { Models };
