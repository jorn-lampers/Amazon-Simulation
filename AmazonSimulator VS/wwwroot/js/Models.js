var MODELS =
{
    Loader: new THREE.ObjectLoader(),
    ModelsLoaded: (names) =>
    {
        for (let i = 0; i < names.length; i++)
            if (MODELS[names[i]] == null) return false;

        return true;
    },
    LoadModels: (names, callback) => 
    {

        let loader = new THREE.GLTFLoader();

        let path = '/models/robot/model.glb.json';

        console.log( "Loading glb: " + path );

        loader.load( path, function ( gltf )
        {
            console.log( "glb " + path + " has been loaded!" );
            MODELS['robot'] = gltf.scene;

            if ( MODELS.ModelsLoaded( [ 'truck', 'robot', 'shelf' ] ) ) callback();

        }, undefined, function ( error )
            {

                console.error( error );

            }
        );

        path = '/models/shelf/model.glb.json';

        console.log( "Loading glb: " + path );

        loader.load( path, function ( gltf )
        {
            console.log( "glb " + path + " has been loaded!" );
            MODELS['shelf'] = gltf.scene;

            if ( MODELS.ModelsLoaded( ['truck', 'robot', 'shelf'] ) ) callback();

        }, undefined, function ( error )
            {

                console.error( error );

            }
        );

        path = '/models/truck/model.glb.json';

        console.log( "Loading glb: " + path );

        loader.load( path, function ( gltf )
        {
            console.log( "glb " + path + " has been loaded!" );
            MODELS['truck'] = gltf.scene;

            if ( MODELS.ModelsLoaded( ['truck', 'robot', 'shelf'] ) ) callback();

        }, undefined, function ( error )
            {

                console.error( error );

            }
        );
    },
    GetModelInstance: (name) => MODELS[name].clone(),
}
