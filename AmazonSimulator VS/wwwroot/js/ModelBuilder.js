CreateRobot = function ()
{
    var robot = MODELS.GetModelInstance( 'robot' );

    robot.position.y = 0.25;

    var group = new THREE.Group();
    group.add(robot);
    group.name = "Robot";

    return group;
}

CreateTruck = function ()
{
    var truck = MODELS.GetModelInstance('truck');

    // Encapsulate loaded model into a group
    var group = new THREE.Group();
    group.add(truck);
    group.name = "Truck";

    //group.rotation.y = -0.5 * Math.PI;

    return group;
}

CreateShelf = function () {

    var group = new THREE.Group();

    group.add(MODELS.GetModelInstance('shelf'));
    group.name = "Shelf";

    return group;
}

CreateGraph = function (graph) {
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

CreateStorage = function (length, width) {
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


