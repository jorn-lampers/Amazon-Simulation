var MODELS = 
{
    ModelsLoaded: () => MODELS.Shelf != null && MODELS.Truck != null,
    LoadModels: (callback) =>
    {
        $.getJSON("/models/shelf/model.json", function (data) {
            MODELS.Shelf = data;
            console.log("Loaded Shelf's model" + MODELS.ModelsLoaded());
            if(MODELS.ModelsLoaded()) callback();
        });
        $.getJSON("/models/truck/model.json", function (data) {
            MODELS.Truck = data;
            console.log("Loaded Truck's model" + MODELS.ModelsLoaded());
            if(MODELS.ModelsLoaded()) callback();
        });
    },
    Shelf: null,
    Truck: null
}
