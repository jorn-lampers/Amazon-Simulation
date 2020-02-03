var MODELS =
{
    Loader: new THREE.ObjectLoader(),
    Names: ['shelf', 'truck'],
    ModelsLoaded: (names) =>
    {
        for (let i = 0; i < names.length; i++)
            if (MODELS[names[i]] == null) return false;

        return true;
    },
    LoadModels: (names, callback) => 
    {
        for (let i = 0; i < names.length; i++) 
        {
            let name = names[i];
            let path = '/models/' + name + '/model.json';
            console.log("loading model " + path);

            $.getJSON(path, function (data)
            {
                MODELS[name] = MODELS.Loader.parse(data);
                if (MODELS.ModelsLoaded(names)) callback();
            });
        }
    },
    GetModelInstance: (name) => MODELS[name].clone(),
}
