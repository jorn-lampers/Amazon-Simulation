var MODELS =
{
    animations: {},
    Loader: new THREE.ObjectLoader(),
    ModelsLoaded: (names) =>
    {
        for (let i = 0; i < names.length; i++)
            if (MODELS[names[i]] == null) return false;

        return true;
    },
    LoadModels: (names, callback) => {

        let loader = new THREE.GLTFLoader();

        for (let i = 0; i < names.length; i++) {
            let path = '/models/' + names[i] + '/model.glb.json';

            //CONSOLE.Log("Loading glb: " + path);

            loader.load(path, function (gltf) {
                CONSOLE.Log("glb " + path + " has been loaded!");

                MODELS[names[i]] = gltf.scene;
                MODELS.animations[names[i]] = gltf.animations;

                if (MODELS.ModelsLoaded(names)) callback();

            }, undefined, function (error) {

                console.error(error);

            });
        }

    },
    GetModelInstance: (name) => MODELS[name].clone(),
    GetAnimations: (name) => MODELS.animations[name] // Do i really need to clone these ?
}
