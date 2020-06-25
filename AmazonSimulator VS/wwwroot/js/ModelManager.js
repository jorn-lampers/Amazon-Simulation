import DOMConsole from './DOMConsole.js';

var ModelManager = function ( gltfLoader )
{
    let _animations = { };
    let _models = { };

    let queued = [];

    let _gltfLoader = gltfLoader;

    function overrideMaterialProperties( modelNames, properties ) {

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

                    matches.forEach(( p ) => mat[p] = properties[p]);

                }

            });

        });
    }

    function overrideModelProperties( modelNames, properties ) {

        modelNames.forEach( ( modelName, i, a ) => {
            let model = _models[modelName];

            // Apply desired properties to objects imported from a GLB
            model.traverse(function (child) {

                let matches = [];

                Object.keys( properties ).forEach( ( p) =>
                {
                    if( p in child )
                        matches.push( p );
                });

                console.log('Setting ' + matches.length + ' properties in model with name "' + child.name + '".');
                matches.forEach(( p ) => child[p] = properties[p]);

            });

        });
    }

    function loadModels( modelNames, callback ) {

        // Preload environment model before starting the simulation
        modelNames.forEach((name, i, a) => {

            let url = '/models/' + name + '/model.glb';

            queued.push( url );

            _gltfLoader
                .load(url, function ( gltf )
                {
                    console.log('Preloaded model ' + name + '! (' + (a.length - i) + '/' + a.length + ')', 'green');

                    let model = gltf.scene;
                    let animations = gltf.animations;

                    // Apply desired properties to objects imported from a GLB
                    model.traverse(function (child) {

                        // All models should cast shadows except materials named 'Material.Glass'
                        child.castShadow = (child.material && child.material.name != 'Material.Glass');

                        // All models should receive shadows.
                        child.receiveShadow = true;

                    });

                    let index = queued.indexOf( url );
                    if ( index !== -1 ) queued.splice( index );

                    _models[name] = model;
                    _animations[name] = animations;

                    if( queued.length === 0 ) callback();

                });
        });

    }

    function getModelInstance( modelName ) {

        return _models[modelName].clone();

    }

    function getAnimations( modelName ) {

        return _animations[modelName];

    }

    return {
        loadModels: loadModels,
        getModelInstance: getModelInstance,
        getAnimations: getAnimations,
        overrideMaterialProperties: overrideMaterialProperties,
        overrideModelProperties: overrideModelProperties
    };

};

export { ModelManager };
