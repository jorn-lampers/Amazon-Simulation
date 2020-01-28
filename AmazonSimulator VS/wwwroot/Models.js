var MODELS = 
{
    Loader: new THREE.ObjectLoader(),
    LoadObject: (json) => MODELS.Loader.parse(json),
    ModelsLoaded: () => MODELS.Shelf != null && MODELS.Truck != null,

    LoadModels: (callback) =>
    {
        console.log("Loading 3d models...");
        $.getJSON("/models/shelf/model.json", function (data) {
            MODELS.Shelf = data;
            if(MODELS.ModelsLoaded()) callback();
        });
        $.getJSON("/models/truck/model.json", function (data) {
            MODELS.Truck = data;
            if (MODELS.ModelsLoaded()) {
                console.log("3d models loaded!");
                callback();
            }
        });
    },

    Shelf: null,

    Truck: null
}
