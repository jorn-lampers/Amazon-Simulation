var MODELS = 
{
    Shelf: {},
    Truck: {}
}

LoadModels = function() {
    $.getJSON("/models/shelf/model.json", function (data) {
        MODELS.Shelf = data;
        console.log("Loaded Shelf's model");
    });
    $.getJSON("/models/truck/model.json", function (data) {
        MODELS.Truck = data;
        console.log("Loaded Truck's model");
    });
}